using Eton.Models;
using Supabase.Postgrest;

namespace Eton.Services;

/// <summary>Come è finito un salvataggio. Non è un booleano perché i tre modi di fallire
/// vogliono tre rimedi diversi, e confonderli produrrebbe messaggi bugiardi.</summary>
public enum EsitoSalvataggio
{
    /// <summary>Scritta.</summary>
    Salvata,
    /// <summary>Qualcun altro ha salvato dopo che tu avevi aperto la nota. Il testo suo è in
    /// <c>Aggiornata</c>: si chiede a chi scrive se ricaricare o sovrascrivere.</summary>
    Conflitto,
    /// <summary>La RLS ha detto di no: non sei né l'autore né il proprietario dello spazio.
    /// Riprovare non serve a niente.</summary>
    Rifiutata,
    /// <summary>La nota non c'è più: cancellata da qualcun altro, o non sei più nello spazio.</summary>
    Sparita
}

/// <summary>L'esito, con la versione del server quando serve per decidere il da farsi.</summary>
public sealed record RisultatoSalvataggio(EsitoSalvataggio Esito, Note? Aggiornata);

/// <summary>
/// Accesso alle note. Come <see cref="SpaceRepository"/>: ogni metodo riparte da
/// <see cref="SupabaseService.GetClientAsync"/> e non tiene mai il client in un campo.
/// <para>
/// Nessun controllo di autorizzazione qui dentro. Le query chiedono "le note di questo spazio"
/// perché è il database a restituire già solo quelle che si possono vedere: filtrare lato client
/// sarebbe teatro, dato che chiunque può interrogare PostgREST con la chiave anon, che è pubblica.
/// </para>
/// </summary>
public class NoteRepository
{
    private readonly SupabaseService _supabase;

    public NoteRepository(SupabaseService supabase) => _supabase = supabase;

    /// <summary>Le note di uno spazio, dalla più recente. L'ordine combacia con l'indice
    /// (space_id, updated_at desc), quindi il database non deve ordinare a parte.
    /// <para>
    /// <paramref name="massimo"/> è opzionale e senza valore predefinito: l'elenco completo deve
    /// restare completo, perché troncarlo in silenzio è peggio del traffico che risparmia. Il
    /// limite lo chiede chi sa di volerne poche — la Home, che ne mostra tre.
    /// </para>
    /// </summary>
    public async Task<IReadOnlyList<Note>> ElencaAsync(Guid spazioId, int? massimo = null)
    {
        var client = await _supabase.GetClientAsync();

        var query = client.From<Note>()
            .Where(n => n.SpaceId == spazioId)
            .Order("updated_at", Constants.Ordering.Descending);

        // 'is > 0' e non 'is not null': uno zero o un negativo qui valgono "nessun limite", non
        // "nessuna nota". È la lettura giusta perché il parametro significa "quante me ne bastano",
        // e a nessuno ne bastano zero — chiedere zero note sarebbe un errore di chi chiama, e
        // restituire un elenco vuoto lo nasconderebbe invece di renderlo evidente.
        if (massimo is > 0) query = query.Limit(massimo.Value);

        var risposta = await query.Get();
        return risposta.Models;
    }

    public async Task<Note?> LeggiAsync(Guid notaId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Note>().Where(n => n.Id == notaId).Get();
        return risposta.Models.FirstOrDefault();
    }

    /// <summary>Crea una nota a proprio nome. L'autore va passato esplicitamente e non dedotto
    /// qui: la policy di INSERT pretende owner_id = auth.uid(), quindi sbagliarlo produce un
    /// rifiuto netto invece di una nota firmata male.</summary>
    public async Task<Note> CreaAsync(Guid spazioId, Guid autoreId, string titolo, string corpo)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Note>().Insert(new Note
        {
            SpaceId  = spazioId,
            OwnerId  = autoreId,
            Title    = titolo.Trim(),
            Body     = corpo
        });

        return risposta.Models.FirstOrDefault()
               ?? throw new InvalidOperationException("Il database non ha restituito la nota appena creata.");
    }

    /// <summary>
    /// Salva, ma solo se nessuno ha scritto dopo di te: <paramref name="versioneLetta"/> entra
    /// come FILTRO, non come valore da scrivere — su version il client non ha nemmeno il
    /// privilegio di colonna. Zero righe toccate significa "non se ne fa niente", ma i motivi
    /// sono tre e vanno distinti, altrimenti si dice all'utente di riprovare quando riprovare
    /// non serve.
    /// </summary>
    public async Task<RisultatoSalvataggio> SalvaAsync(Guid notaId, int versioneLetta, string titolo, string corpo)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Note>()
            .Where(n => n.Id == notaId && n.Version == versioneLetta)
            .Set(n => n.Title, titolo.Trim())
            .Set(n => n.Body, corpo)
            .Update();

        if (risposta.Models.Count > 0)
            return new RisultatoSalvataggio(EsitoSalvataggio.Salvata, risposta.Models[0]);

        // Una seconda lettura per capire QUALE dei tre casi è: la prima query non lo dice, perché
        // "filtrata dalla RLS" e "versione non combacia" producono entrambe zero righe.
        var attuale = await LeggiAsync(notaId);

        if (attuale is null)
            return new RisultatoSalvataggio(EsitoSalvataggio.Sparita, null);

        // La distinzione è quasi sempre esatta, ma non del tutto, e conviene saperlo: se chi salva
        // non ha diritto di scrittura E nell'intervallo fra l'UPDATE e questa rilettura l'autore
        // vero salva davvero, la versione risulta cambiata e il caso viene letto come Conflitto
        // invece che come Rifiutata. All'utente compare la scheda "qualcun altro ha salvato prima
        // di te" al posto del messaggio sui permessi; premendo Sovrascrivi il tentativo fallisce di
        // nuovo e stavolta viene classificato bene. Nessuna scrittura indebita passa: il confine è
        // la RLS, non questa riclassificazione, che serve solo a scegliere il messaggio.
        // Risolverlo davvero richiederebbe una RPC che restituisca il motivo del rifiuto insieme
        // all'esito, e non vale il costo per una finestra di pochi millisecondi che si chiude da
        // sola al clic successivo.
        return attuale.Version != versioneLetta
            ? new RisultatoSalvataggio(EsitoSalvataggio.Conflitto, attuale)
            : new RisultatoSalvataggio(EsitoSalvataggio.Rifiutata, attuale);
    }

    /// <summary>Elimina. False se la RLS ha rifiutato — non sei né l'autore né il proprietario
    /// dello spazio. Non lancia: il rifiuto è una risposta, non un guasto.</summary>
    public async Task<bool> EliminaAsync(Guid notaId)
    {
        var client = await _supabase.GetClientAsync();

        // La lettura preventiva non è una cautela generica, è necessaria proprio QUI. Su 'notes'
        // la visibilità in SELECT non contiene il permesso di DELETE: notes_select chiede
        // is_space_member(space_id), notes_delete chiede owner_id = auth.uid() or
        // is_space_owner(space_id) — le due condizioni si intersecano invece di contenersi.
        // Chi non è né autore, né proprietario dello spazio, né più membro fallisce entrambe: la
        // DELETE tocca zero righe E la rilettura di conferma ne trova zero, ma perché non ha il
        // diritto di vederle, non perché siano sparite. Senza questa riga il metodo direbbe
        // "eliminata" su una nota che è ancora lì, visibile a tutti gli altri.
        // Su 'spaces' il problema non si pone (là SELECT è un soprainsieme di DELETE), ma
        // SpaceRepository.EliminaAsync fa comunque la stessa lettura: v. quel file.
        //
        // Il rovescio, dichiarato: siccome le due condizioni si intersecano, esiste anche il caso
        // opposto — l'autore che ha lasciato lo spazio conserva il diritto di DELETE ma perde
        // quello di SELECT, e questa riga glielo toglie. Non è raggiungibile dall'interfaccia
        // (per arrivare al pulsante serve aver aperto la nota, quindi aver potuto leggerla), salvo
        // uscire dallo spazio in un'altra scheda con la pagina già aperta: lì si legge "rifiutata"
        // su una cancellazione che sarebbe passata. È molto più raro del falso "eliminata" che
        // questa riga chiude, e sbagliare dicendo di non aver fatto qualcosa è meno grave che
        // sbagliare dicendo di averla fatta.
        var prima = await client.From<Note>().Where(n => n.Id == notaId).Get();
        if (prima.Models.Count == 0) return false;

        await client.From<Note>().Where(n => n.Id == notaId).Delete();

        var dopo = await client.From<Note>().Where(n => n.Id == notaId).Get();
        return dopo.Models.Count == 0;
    }
}
