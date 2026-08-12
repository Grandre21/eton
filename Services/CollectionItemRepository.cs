using Eton.Models;
using Supabase.Postgrest;

namespace Eton.Services;

/// <summary>
/// Accesso agli elementi di una collezione. Come <see cref="NoteRepository"/>: ogni metodo riparte
/// da <see cref="SupabaseService.GetClientAsync"/> e non tiene mai il client in un campo.
/// <para>
/// Nessun controllo di autorizzazione qui dentro. Le query chiedono "gli elementi di questa
/// collezione" perché è il database a restituire già solo quelli che si possono vedere: filtrare
/// lato client sarebbe teatro, dato che chiunque può interrogare PostgREST con la chiave anon,
/// che è pubblica.
/// </para>
/// </summary>
public class CollectionItemRepository
{
    private readonly SupabaseService _supabase;

    public CollectionItemRepository(SupabaseService supabase) => _supabase = supabase;

    /// <summary>Gli elementi di una collezione, in ordine alfabetico. L'ordine combacia con
    /// l'indice (collection_id, name), quindi il database non deve ordinare a parte.
    /// <para>
    /// Per nome e non per data, a differenza di ogni altra elencazione del progetto: un catalogo
    /// si consulta cercando un nome che si ha già in mente, non per vedere cosa è cambiato di
    /// recente.
    /// </para>
    /// </summary>
    public async Task<IReadOnlyList<CollectionItem>> ElencaAsync(Guid collezioneId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<CollectionItem>()
            .Where(i => i.CollectionId == collezioneId)
            .Order("name", Constants.Ordering.Ascending)
            .Get();
        return risposta.Models;
    }

    public async Task<CollectionItem?> LeggiAsync(Guid elementoId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<CollectionItem>().Where(i => i.Id == elementoId).Get();
        return risposta.Models.FirstOrDefault();
    }

    /// <summary>Crea un elemento a proprio nome. L'autore va passato esplicitamente e non dedotto
    /// qui: la policy di INSERT pretende added_by = auth.uid(), quindi sbagliarlo produce un
    /// rifiuto netto invece di un elemento firmato male.
    /// <para>
    /// <paramref name="spazioId"/> va passato esplicitamente anche se si potrebbe risalire dalla
    /// collezione: è la colonna denormalizzata su cui si aggancia la policy, e la chiave esterna
    /// composita (collection_id, space_id) rifiuta la riga se non combacia con lo spazio della
    /// collezione — quindi passarlo sbagliato produce un errore netto, non una riga storta.
    /// </para>
    /// </summary>
    public async Task<CollectionItem> CreaAsync(Guid collezioneId, Guid spazioId, Guid autoreId,
        string nome, string? immagine, IReadOnlyDictionary<string, object> dati)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<CollectionItem>().Insert(new CollectionItem
        {
            CollectionId = collezioneId,
            SpaceId      = spazioId,
            AddedBy      = autoreId,
            Name         = nome.Trim(),
            ImageUrl     = string.IsNullOrWhiteSpace(immagine) ? null : immagine,
            Data         = new Dictionary<string, object>(dati)
        });

        return risposta.Models.FirstOrDefault()
               ?? throw new InvalidOperationException("Il database non ha restituito l'elemento appena creato.");
    }

    /// <summary>
    /// Salva, ma solo se nessuno ha scritto dopo di te: <paramref name="versioneLetta"/> entra
    /// come FILTRO, non come valore da scrivere — su version il client non ha nemmeno il
    /// privilegio di colonna. Zero righe toccate significa "non se ne fa niente", ma i motivi
    /// sono tre e vanno distinti, altrimenti si dice all'utente di riprovare quando riprovare
    /// non serve.
    /// </summary>
    public async Task<RisultatoSalvataggio<CollectionItem>> SalvaAsync(Guid elementoId, int versioneLetta,
        string nome, string? immagine, IReadOnlyDictionary<string, object> dati)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<CollectionItem>()
            .Where(i => i.Id == elementoId && i.Version == versioneLetta)
            .Set(i => i.Name, nome.Trim())
            .Set(i => i.ImageUrl!, string.IsNullOrWhiteSpace(immagine) ? null : immagine)
            .Set(i => i.Data, new Dictionary<string, object>(dati))
            .Update();

        if (risposta.Models.Count > 0)
            return new RisultatoSalvataggio<CollectionItem>(EsitoSalvataggio.Salvata, risposta.Models[0]);

        // Una seconda lettura per capire QUALE dei tre casi è: la prima query non lo dice, perché
        // "filtrata dalla RLS" e "versione non combacia" producono entrambe zero righe.
        var attuale = await LeggiAsync(elementoId);

        if (attuale is null)
            return new RisultatoSalvataggio<CollectionItem>(EsitoSalvataggio.Sparita, null);

        // La distinzione è quasi sempre esatta, ma non del tutto, e conviene saperlo: se chi salva
        // non ha diritto di scrittura E nell'intervallo fra l'UPDATE e questa rilettura chi ha
        // aggiunto l'elemento salva davvero, la versione risulta cambiata e il caso viene letto
        // come Conflitto invece che come Rifiutata. All'utente compare la scheda "qualcun altro ha
        // salvato prima di te" al posto del messaggio sui permessi; premendo Sovrascrivi il
        // tentativo fallisce di nuovo e stavolta viene classificato bene. Nessuna scrittura
        // indebita passa: il confine è la RLS, non questa riclassificazione, che serve solo a
        // scegliere il messaggio.
        // Risolverlo davvero richiederebbe una RPC che restituisca il motivo del rifiuto insieme
        // all'esito, e non vale il costo per una finestra di pochi millisecondi che si chiude da
        // sola al clic successivo.
        return attuale.Version != versioneLetta
            ? new RisultatoSalvataggio<CollectionItem>(EsitoSalvataggio.Conflitto, attuale)
            : new RisultatoSalvataggio<CollectionItem>(EsitoSalvataggio.Rifiutata, attuale);
    }

    /// <summary>Elimina. False se la RLS ha rifiutato — non sei né chi ha aggiunto l'elemento né
    /// il proprietario dello spazio. Non lancia: il rifiuto è una risposta, non un guasto.</summary>
    public async Task<bool> EliminaAsync(Guid elementoId)
    {
        var client = await _supabase.GetClientAsync();

        // La lettura preventiva non è una cautela generica, è necessaria proprio QUI. Su
        // 'collection_items' la visibilità in SELECT non contiene il permesso di DELETE:
        // collection_items_select chiede is_space_member(space_id), collection_items_delete
        // chiede added_by = auth.uid() or is_space_owner(space_id) — le due condizioni si
        // intersecano invece di contenersi. Chi non è né chi ha aggiunto l'elemento, né
        // proprietario dello spazio, né più membro fallisce entrambe: la DELETE tocca zero righe E
        // la rilettura di conferma ne trova zero, ma perché non ha il diritto di vederle, non
        // perché siano sparite. Senza questa riga il metodo direbbe "eliminato" un elemento che è
        // ancora lì, visibile a tutti gli altri.
        //
        // Il rovescio, dichiarato: siccome le due condizioni si intersecano, esiste anche il caso
        // opposto — chi ha aggiunto l'elemento e ha lasciato lo spazio conserva il diritto di
        // DELETE ma perde quello di SELECT, e questa riga glielo toglie. Non è raggiungibile
        // dall'interfaccia (per arrivare al pulsante serve aver aperto l'elemento, quindi aver
        // potuto leggerlo), salvo uscire dallo spazio in un'altra scheda con la pagina già aperta:
        // lì si legge "rifiutata" su una cancellazione che sarebbe passata. È molto più raro del
        // falso "eliminato" che questa riga chiude, e sbagliare dicendo di non aver fatto qualcosa
        // è meno grave che sbagliare dicendo di averla fatta.
        var prima = await client.From<CollectionItem>().Where(i => i.Id == elementoId).Get();
        if (prima.Models.Count == 0) return false;

        await client.From<CollectionItem>().Where(i => i.Id == elementoId).Delete();

        var dopo = await client.From<CollectionItem>().Where(i => i.Id == elementoId).Get();
        return dopo.Models.Count == 0;
    }
}
