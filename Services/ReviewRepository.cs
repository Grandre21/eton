using Eton.Models;
using Supabase.Postgrest;

namespace Eton.Services;

/// <summary>
/// Accesso alle recensioni. Come <see cref="CollectionItemRepository"/>: ogni metodo riparte da
/// <see cref="SupabaseService.GetClientAsync"/> e non tiene mai il client in un campo.
/// <para>
/// Nessun controllo di autorizzazione qui dentro. Le query chiedono "le recensioni di questo
/// elemento/spazio" perché è il database a restituire già solo quelle che si possono vedere:
/// filtrare lato client sarebbe teatro, dato che chiunque può interrogare PostgREST con la chiave
/// anon, che è pubblica.
/// </para>
/// </summary>
public class ReviewRepository
{
    private readonly SupabaseService _supabase;

    public ReviewRepository(SupabaseService supabase) => _supabase = supabase;

    /// <summary>Le recensioni di un elemento, dalla prima all'ultima.
    /// <para>
    /// Ascendente, a differenza di ogni altra elencazione del progetto: un elenco di opinioni si
    /// legge nell'ordine in cui sono state espresse — è la lettura di una conversazione — mentre
    /// note e collezioni si elencano dalla più recente, perché lì conta cosa è cambiato di
    /// recente, non l'ordine con cui è stato scritto.
    /// </para>
    /// </summary>
    public async Task<IReadOnlyList<Review>> ElencaPerElementoAsync(Guid elementoId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Review>()
            .Where(r => r.ItemId == elementoId)
            .Order("created_at", Constants.Ordering.Ascending)
            .Get();
        return risposta.Models;
    }

    /// <summary>Le recensioni di uno spazio, in nessun ordine particolare: alimentano
    /// <see cref="CalcoliVoti.Riepiloghi"/>, che le raggruppa per elemento da sé.
    /// <para>
    /// Si filtra per <c>space_id</c> e non per un elenco di <c>item_id</c> degli elementi della
    /// collezione: con qualche centinaio di elementi un filtro <c>item_id=in.(...)</c> produrrebbe
    /// un URL da decine di kilobyte. <c>space_id</c> è denormalizzato su <c>reviews</c> anche per
    /// questo, oltre che per la RLS (v. Models/Review.cs).
    /// </para>
    /// </summary>
    public async Task<IReadOnlyList<Review>> ElencaPerSpazioAsync(Guid spazioId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Review>().Where(r => r.SpaceId == spazioId).Get();
        return risposta.Models;
    }

    public async Task<Review?> LeggiAsync(Guid recensioneId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Review>().Where(r => r.Id == recensioneId).Get();
        return risposta.Models.FirstOrDefault();
    }

    /// <summary>Crea una recensione a proprio nome. L'autore va passato esplicitamente e non
    /// dedotto qui: la policy di INSERT pretende user_id = auth.uid(), quindi sbagliarlo produce un
    /// rifiuto netto invece di una recensione firmata male.
    /// <para>
    /// <paramref name="spazioId"/> va passato esplicitamente anche se si potrebbe risalire
    /// dall'elemento: è la colonna denormalizzata su cui si aggancia la policy, e la chiave esterna
    /// composita (item_id, space_id) rifiuta la riga se non combacia con lo spazio dell'elemento —
    /// quindi passarlo sbagliato produce un errore netto, non una riga storta.
    /// </para>
    /// </summary>
    public async Task<Review> CreaAsync(Guid elementoId, Guid spazioId, Guid utenteId, decimal? voto, string? commento)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Review>().Insert(new Review
        {
            ItemId  = elementoId,
            SpaceId = spazioId,
            UserId  = utenteId,
            Rating  = voto,
            Comment = string.IsNullOrWhiteSpace(commento) ? null : commento
        });

        return risposta.Models.FirstOrDefault()
               ?? throw new InvalidOperationException("Il database non ha restituito la recensione appena creata.");
    }

    /// <summary>
    /// Salva, ma solo se nessuno ha scritto dopo di te: <paramref name="versioneLetta"/> entra
    /// come FILTRO, non come valore da scrivere — su version il client non ha nemmeno il
    /// privilegio di colonna. Zero righe toccate significa "non se ne fa niente", ma i motivi
    /// sono tre e vanno distinti, altrimenti si dice all'utente di riprovare quando riprovare
    /// non serve.
    /// </summary>
    public async Task<RisultatoSalvataggio<Review>> SalvaAsync(Guid recensioneId, int versioneLetta, decimal? voto, string? commento)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Review>()
            .Where(r => r.Id == recensioneId && r.Version == versioneLetta)
            .Set(r => r.Rating!, voto)
            .Set(r => r.Comment!, string.IsNullOrWhiteSpace(commento) ? null : commento)
            .Update();

        if (risposta.Models.Count > 0)
            return new RisultatoSalvataggio<Review>(EsitoSalvataggio.Salvata, risposta.Models[0]);

        // Una seconda lettura per capire QUALE dei tre casi è: la prima query non lo dice, perché
        // "filtrata dalla RLS" e "versione non combacia" producono entrambe zero righe.
        var attuale = await LeggiAsync(recensioneId);

        if (attuale is null)
            return new RisultatoSalvataggio<Review>(EsitoSalvataggio.Sparita, null);

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
            ? new RisultatoSalvataggio<Review>(EsitoSalvataggio.Conflitto, attuale)
            : new RisultatoSalvataggio<Review>(EsitoSalvataggio.Rifiutata, attuale);
    }

    /// <summary>Elimina. False se la RLS ha rifiutato — non sei l'autore della recensione: qui, a
    /// differenza di note, collezioni ed elementi, il proprietario dello spazio non ha un diritto
    /// di cancellazione aggiuntivo. Non lancia: il rifiuto è una risposta, non un guasto.</summary>
    public async Task<bool> EliminaAsync(Guid recensioneId)
    {
        var client = await _supabase.GetClientAsync();

        // La lettura preventiva non è una cautela generica, è necessaria proprio QUI. Su 'reviews'
        // la policy di DELETE è user_id = auth.uid() e basta — niente "o proprietario dello
        // spazio" come per note, collezioni ed elementi: un voto è un'opinione personale, e
        // permettere al proprietario di cancellare quello altrui sarebbe falsificarlo, non
        // moderarlo. reviews_select chiede is_space_member(space_id), reviews_delete chiede
        // user_id = auth.uid(): le due condizioni si intersecano invece di contenersi. Chi non è
        // né l'autore né più membro dello spazio fallisce entrambe: la DELETE tocca zero righe E
        // la rilettura di conferma ne trova zero, ma perché non ha il diritto di vederle, non
        // perché siano sparite. Senza questa riga il metodo direbbe "eliminata" una recensione che
        // è ancora lì, visibile a tutti gli altri membri.
        //
        // Il rovescio, dichiarato: chi ha recensito e poi è uscito dallo spazio conserva il
        // diritto di DELETE (che qui non dipende affatto dalla membership) ma perde quello di
        // SELECT (che la richiede), e questa riga glielo toglie — legge "rifiutata" su una
        // cancellazione che sarebbe passata. È il caso opposto a quello sopra, e su 'reviews' è
        // più raggiungibile che altrove: uscire dallo spazio è l'unico modo di perdere SELECT sul
        // proprio voto, dato che qui non esiste una moderazione del proprietario che lo tolga per
        // altre vie.
        var prima = await client.From<Review>().Where(r => r.Id == recensioneId).Get();
        if (prima.Models.Count == 0) return false;

        await client.From<Review>().Where(r => r.Id == recensioneId).Delete();

        var dopo = await client.From<Review>().Where(r => r.Id == recensioneId).Get();
        return dopo.Models.Count == 0;
    }
}
