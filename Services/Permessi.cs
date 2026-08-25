using Eton.Models;

namespace Eton.Services;

/// <summary>
/// La regola di modifica/eliminazione condivisa da quattro entità — note, collezioni, elementi di
/// una collezione, spese — scritta una volta sola invece che in quattro copie che nessun grep
/// collega alla migration da cui vengono. Le policy sono, testuali:
/// <c>owner_id = auth.uid() or is_space_owner(space_id)</c> per <c>notes</c>
/// (<c>supabase/migrations/20260812000000_note.sql</c>, <c>notes_update</c>/<c>notes_delete</c>)
/// e <c>collections</c> (<c>supabase/migrations/20260812120000_collections.sql</c>,
/// <c>collections_update</c>/<c>collections_delete</c>), <c>added_by = auth.uid() or
/// is_space_owner(space_id)</c> per <c>collection_items</c> (stesso file,
/// <c>collection_items_update</c>/<c>collection_items_delete</c>), e <c>paid_by = auth.uid() or
/// is_space_owner(space_id)</c> per <c>expenses</c>
/// (<c>supabase/migrations/20260824000000_spese.sql</c>, <c>expenses_update</c>/<c>expenses_delete</c>).
/// <para>
/// NON è un controllo di sicurezza: la sicurezza vera è la RLS nelle migration citate sopra, che
/// resta intatta e che chiunque può aggirare chiamando PostgREST a mano con la chiave anon —
/// pubblica per costruzione. Questo booleano è onestà dell'interfaccia: smette di offrire
/// un'azione che il database rifiuterebbe comunque, invece di offrirla e poi spiegare il rifiuto
/// con un messaggio che non aiuta nessuno. La divisione dei compiti è questa: la RLS decide,
/// l'interfaccia informa.
/// </para>
/// <para>
/// Si passa l'elenco degli spazi di chi guarda, non lo spazio attualmente selezionato
/// nell'interfaccia: la domanda è "possiedo lo spazio DI QUESTO OGGETTO?", un fatto che non
/// dipende da quale spazio è attivo adesso nel selettore, che è una preferenza dell'interfaccia —
/// cambia da un'altra pagina, o non è ancora caricata affatto quando questa è la prima pagina
/// aperta. Legarci la proprietà era il difetto di una versione precedente: chi apriva la pagina da
/// un collegamento diretto, prima che qualcos'altro caricasse lo spazio attivo, si vedeva negare
/// una modifica che invece gli spettava.
/// </para>
/// <para>
/// Fallisce chiuso: <paramref name="mioId"/> nullo restituisce sempre falso, e uno spazio assente
/// dall'elenco — non ancora caricato, o semplicemente non posseduto da chi guarda — non basta a
/// dare il permesso, restituisce falso allo stesso modo di uno spazio posseduto da qualcun altro.
/// Nel dubbio si mostra di meno, mai di più.
/// </para>
/// <para>
/// Il confine, importante quanto la regola: vale per queste quattro entità e basta. NON vale per
/// le recensioni — <c>supabase/migrations/20260812200000_recensioni.sql</c>
/// (<c>reviews_update</c>/<c>reviews_delete</c>) dice <c>user_id = auth.uid()</c> e basta, senza
/// la clausola sul proprietario dello spazio. Non è una dimenticanza: un voto è personale, e chi
/// possiede lo spazio non deve poter riscrivere il giudizio di qualcun altro — una nota condivisa
/// sì, il voto sulla birra no. Un metodo che si chiama "può intervenire" invita a essere applicato
/// ovunque: non va usato per le recensioni, o il proprietario di uno spazio otterrebbe il potere
/// di cambiare i voti altrui senza che nessuno se ne accorga finché non succede.
/// </para>
/// </summary>
public static class Permessi
{
    /// <summary>Le quattro entità che condividono la regola di <see cref="PuoIntervenire"/>: note,
    /// collezioni, elementi di una collezione, spese. V. <see cref="Spiegazione"/>.</summary>
    public enum Oggetto { Nota, Collezione, Elemento, Spesa }

    /// <summary>
    /// Vero quando <paramref name="mioId"/> è chi ha creato l'oggetto (<paramref name="autoreId"/>),
    /// oppure quando lo spazio dell'oggetto (<paramref name="spazioId"/>) compare in
    /// <paramref name="spazi"/> con <see cref="Space.OwnerId"/> uguale a <paramref name="mioId"/>.
    /// V. la classe per il perché di ogni scelta qui dentro.
    /// </summary>
    public static bool PuoIntervenire(Guid? mioId, Guid autoreId, Guid spazioId, IReadOnlyList<Space> spazi)
        => mioId is not null
            && (autoreId == mioId
                || spazi.FirstOrDefault(s => s.Id == spazioId)?.OwnerId == mioId);

    /// <summary>
    /// La stessa regola di <see cref="PuoIntervenire"/>, detta in prosa: la frase da mostrare quando
    /// il booleano è falso, perché dire "non puoi" senza dire perché è peggio che tacere.
    /// <para>
    /// Sta qui e non nelle quattro pagine che la mostrano: è la stessa policy RLS espressa a parole,
    /// e va tenuta a contatto con il booleano che la calcola, sulla riga sotto. Se vivesse nelle
    /// pagine, cambiare la regola in un posto e scordare le altre tre sarebbe l'esito normale — e la
    /// divergenza sarebbe invisibile, perché nessun grep collega una frase italiana a una policy SQL.
    /// </para>
    /// <para>
    /// Impersonale di proposito, non "puoi modificarla solo se l'hai pagata tu": il testo è una
    /// REGOLA, valida per chiunque apra una di queste quattro pagine, non un invito a chi la sta
    /// guardando adesso. La seconda persona (v. "i campi li decidi tu" in Collections.razor:60) è
    /// per gli INVITI — quello stesso testo, infatti, torna impersonale per l'azione di ogni altro
    /// membro.
    /// </para>
    /// </summary>
    public static string Spiegazione(Oggetto oggetto) => oggetto switch
    {
        // I due verbi sono diversi apposta: "segnata" è l'atto generico di registrare la spesa,
        // "pagata" è il campo paid_by che la regola guarda davvero. Nota, Collezione ed Elemento
        // non hanno questa fortuna — owner_id/added_by sono anche il modo naturale di descrivere
        // chi ha creato l'oggetto — quindi usano "è di qualcun altro" per la stessa ragione: evitare
        // di dire lo stesso verbo due volte nella stessa frase ("l'ha scritta... chi l'ha scritta").
        Oggetto.Spesa => "Questa spesa l'ha segnata qualcun altro: può modificarla o cancellarla solo chi l'ha pagata, o chi possiede lo spazio.",
        Oggetto.Nota => "Questa nota è di qualcun altro: può modificarla o cancellarla solo chi l'ha scritta, o chi possiede lo spazio.",
        Oggetto.Collezione => "Questa collezione è di qualcun altro: può modificarla o cancellarla solo chi l'ha creata, o chi possiede lo spazio.",
        Oggetto.Elemento => "Questo elemento è di qualcun altro: può modificarlo o cancellarlo solo chi l'ha aggiunto, o chi possiede lo spazio.",
        _ => throw new ArgumentOutOfRangeException(nameof(oggetto), oggetto, null),
    };
}
