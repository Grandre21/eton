using Eton.Models;
using Supabase.Postgrest;

namespace Eton.Services;

/// <summary>
/// Accesso alle collezioni. Come <see cref="NoteRepository"/>: ogni metodo riparte da
/// <see cref="SupabaseService.GetClientAsync"/> e non tiene mai il client in un campo.
/// <para>
/// Nessun controllo di autorizzazione qui dentro. Le query chiedono "le collezioni di questo
/// spazio" perché è il database a restituire già solo quelle che si possono vedere: filtrare
/// lato client sarebbe teatro, dato che chiunque può interrogare PostgREST con la chiave anon,
/// che è pubblica.
/// </para>
/// </summary>
public class CollectionRepository
{
    private readonly SupabaseService _supabase;

    public CollectionRepository(SupabaseService supabase) => _supabase = supabase;

    /// <summary>Le collezioni di uno spazio, dalla più recente. L'ordine combacia con l'indice
    /// (space_id, updated_at desc), quindi il database non deve ordinare a parte.
    /// <para>
    /// <paramref name="massimo"/> è opzionale e senza valore predefinito: l'elenco completo deve
    /// restare completo, perché troncarlo in silenzio è peggio del traffico che risparmia. Il
    /// limite lo chiede chi sa di volerne poche.
    /// </para>
    /// </summary>
    public async Task<IReadOnlyList<Collection>> ElencaAsync(Guid spazioId, int? massimo = null)
    {
        var client = await _supabase.GetClientAsync();

        var query = client.From<Collection>()
            .Where(c => c.SpaceId == spazioId)
            .Order("updated_at", Constants.Ordering.Descending);

        // 'is > 0' e non 'is not null': uno zero o un negativo qui valgono "nessun limite", non
        // "nessuna collezione". È la lettura giusta perché il parametro significa "quante me ne
        // bastano", e a nessuno ne bastano zero — chiedere zero collezioni sarebbe un errore di
        // chi chiama, e restituire un elenco vuoto lo nasconderebbe invece di renderlo evidente.
        if (massimo is > 0) query = query.Limit(massimo.Value);

        var risposta = await query.Get();
        return risposta.Models;
    }

    public async Task<Collection?> LeggiAsync(Guid collezioneId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Collection>().Where(c => c.Id == collezioneId).Get();
        return risposta.Models.FirstOrDefault();
    }

    /// <summary>Crea una collezione a proprio nome. L'autore va passato esplicitamente e non
    /// dedotto qui: la policy di INSERT pretende owner_id = auth.uid(), quindi sbagliarlo produce
    /// un rifiuto netto invece di una collezione firmata male.</summary>
    public async Task<Collection> CreaAsync(Guid spazioId, Guid autoreId, string nome, string? icona,
        IReadOnlyList<CampoDefinizione> campi, short votoMassimo)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Collection>().Insert(new Collection
        {
            SpaceId   = spazioId,
            OwnerId   = autoreId,
            Name      = nome.Trim(),
            Icon      = string.IsNullOrWhiteSpace(icona) ? null : icona,
            Fields    = campi.ToList(),
            RatingMax = votoMassimo
        });

        return risposta.Models.FirstOrDefault()
               ?? throw new InvalidOperationException("Il database non ha restituito la collezione appena creata.");
    }

    /// <summary>
    /// Salva, ma solo se nessuno ha scritto dopo di te: <paramref name="versioneLetta"/> entra
    /// come FILTRO, non come valore da scrivere — su version il client non ha nemmeno il
    /// privilegio di colonna. Zero righe toccate significa "non se ne fa niente", ma i motivi
    /// sono tre e vanno distinti, altrimenti si dice all'utente di riprovare quando riprovare
    /// non serve.
    /// </summary>
    public async Task<RisultatoSalvataggio<Collection>> SalvaAsync(Guid collezioneId, int versioneLetta,
        string nome, string? icona, IReadOnlyList<CampoDefinizione> campi, short votoMassimo)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Collection>()
            .Where(c => c.Id == collezioneId && c.Version == versioneLetta)
            .Set(c => c.Name, nome.Trim())
            .Set(c => c.Icon!, string.IsNullOrWhiteSpace(icona) ? null : icona)
            .Set(c => c.Fields, campi.ToList())
            .Set(c => c.RatingMax, votoMassimo)
            .Update();

        if (risposta.Models.Count > 0)
            return new RisultatoSalvataggio<Collection>(EsitoSalvataggio.Salvata, risposta.Models[0]);

        // Una seconda lettura per capire QUALE dei tre casi è: la prima query non lo dice, perché
        // "filtrata dalla RLS" e "versione non combacia" producono entrambe zero righe.
        var attuale = await LeggiAsync(collezioneId);

        if (attuale is null)
            return new RisultatoSalvataggio<Collection>(EsitoSalvataggio.Sparita, null);

        // La distinzione è quasi sempre esatta, ma non del tutto, e conviene saperlo: se chi salva
        // non ha diritto di scrittura E nell'intervallo fra l'UPDATE e questa rilettura il
        // proprietario vero salva davvero, la versione risulta cambiata e il caso viene letto come
        // Conflitto invece che come Rifiutata. All'utente compare la scheda "qualcun altro ha
        // salvato prima di te" al posto del messaggio sui permessi; premendo Sovrascrivi il
        // tentativo fallisce di nuovo e stavolta viene classificato bene. Nessuna scrittura
        // indebita passa: il confine è la RLS, non questa riclassificazione, che serve solo a
        // scegliere il messaggio.
        // Risolverlo davvero richiederebbe una RPC che restituisca il motivo del rifiuto insieme
        // all'esito, e non vale il costo per una finestra di pochi millisecondi che si chiude da
        // sola al clic successivo.
        return attuale.Version != versioneLetta
            ? new RisultatoSalvataggio<Collection>(EsitoSalvataggio.Conflitto, attuale)
            : new RisultatoSalvataggio<Collection>(EsitoSalvataggio.Rifiutata, attuale);
    }

    /// <summary>Elimina. False se la RLS ha rifiutato — non sei né l'autore né il proprietario
    /// dello spazio. Non lancia: il rifiuto è una risposta, non un guasto.</summary>
    public async Task<bool> EliminaAsync(Guid collezioneId)
    {
        var client = await _supabase.GetClientAsync();

        // La lettura preventiva non è una cautela generica, è necessaria proprio QUI. Su
        // 'collections' la visibilità in SELECT non contiene il permesso di DELETE:
        // collections_select chiede is_space_member(space_id), collections_delete chiede
        // owner_id = auth.uid() or is_space_owner(space_id) — le due condizioni si intersecano
        // invece di contenersi. Chi non è né autore, né proprietario dello spazio, né più membro
        // fallisce entrambe: la DELETE tocca zero righe E la rilettura di conferma ne trova zero,
        // ma perché non ha il diritto di vederle, non perché siano sparite. Senza questa riga il
        // metodo direbbe "eliminata" su una collezione che è ancora lì, visibile a tutti gli altri.
        //
        // Il rovescio, dichiarato: siccome le due condizioni si intersecano, esiste anche il caso
        // opposto — l'autore che ha lasciato lo spazio conserva il diritto di DELETE ma perde
        // quello di SELECT, e questa riga glielo toglie. Non è raggiungibile dall'interfaccia (per
        // arrivare al pulsante serve aver aperto la collezione, quindi aver potuto leggerla), salvo
        // uscire dallo spazio in un'altra scheda con la pagina già aperta: lì si legge "rifiutata"
        // su una cancellazione che sarebbe passata. È molto più raro del falso "eliminata" che
        // questa riga chiude, e sbagliare dicendo di non aver fatto qualcosa è meno grave che
        // sbagliare dicendo di averla fatta.
        var prima = await client.From<Collection>().Where(c => c.Id == collezioneId).Get();
        if (prima.Models.Count == 0) return false;

        await client.From<Collection>().Where(c => c.Id == collezioneId).Delete();

        var dopo = await client.From<Collection>().Where(c => c.Id == collezioneId).Get();
        return dopo.Models.Count == 0;
    }
}
