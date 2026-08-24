using System.Globalization;
using Eton.Models;
using Supabase.Postgrest;

namespace Eton.Services;

/// <summary>
/// Accesso alle spese. Come <see cref="NoteRepository"/>: ogni metodo riparte da
/// <see cref="SupabaseService.GetClientAsync"/> e non tiene mai il client in un campo.
/// <para>
/// Nessun controllo di autorizzazione qui dentro. Le query chiedono "le spese di questo spazio"
/// perché è il database a restituire già solo quelle che si possono vedere: filtrare lato client
/// sarebbe teatro, dato che chiunque può interrogare PostgREST con la chiave anon, che è pubblica.
/// </para>
/// </summary>
public class ExpenseRepository
{
    private readonly SupabaseService _supabase;

    public ExpenseRepository(SupabaseService supabase) => _supabase = supabase;

    /// <summary>Le spese di uno spazio con spent_on nell'intervallo [<paramref name="da"/>,
    /// <paramref name="a"/>], estremi inclusi, dalla più recente. L'ordine combacia con l'indice
    /// (space_id, spent_on desc), quindi il database non deve ordinare a parte.
    /// <para>
    /// L'intervallo e non un singolo mese perché la pagina ha bisogno del mese corrente E di
    /// quello precedente per il confronto: due viaggi di rete dove ne basta uno sono due viaggi
    /// di rete.
    /// </para>
    /// <para>
    /// spent_on è un 'date', e il filtro va scritto come stringa 'yyyy-MM-dd' con
    /// CultureInfo.InvariantCulture: InvariantGlobalization è attivo e non esistono culture da
    /// impostare.
    /// </para>
    /// <para>
    /// Le due date usano .Filter e non .Where, a differenza della riga sopra e di ogni altro
    /// repository del progetto: è una deroga deliberata, come l'id generato dal client in
    /// <see cref="CreaAsync"/> e come SpentOn su DateTime invece che DateOnly (v. Models/Expense.cs).
    /// spent_on è un 'date', mentre <paramref name="da"/> e <paramref name="a"/> sono DateTime e si
    /// portano dietro un'ora e un Kind. Con .Where(e => e.SpentOn >= da) la stringa spedita la
    /// costruisce il convertitore della libreria, e quale sia esattamente — con o senza orario, con
    /// o senza offset — è un dettaglio interno che non si vede leggendo questa riga. Con .Filter e
    /// una stringa 'yyyy-MM-dd' scritta a mano, ciò che arriva al database è visibile qui, e il
    /// confine del giorno non dipende da niente che stia fuori da questo file. Detto con onestà: il
    /// convertitore della libreria è stato provato, e scrive l'orario a orologio senza convertirlo
    /// in UTC — anche .Where avrebbe funzionato. La ragione di preferire .Filter non è che l'altro
    /// rompe, è che qui la correttezza si legge sulla riga invece di dipendere da un comportamento
    /// verificato altrove.
    /// </para>
    /// </summary>
    public async Task<IReadOnlyList<Expense>> ElencaAsync(Guid spazioId, DateTime da, DateTime a)
    {
        var client = await _supabase.GetClientAsync();

        var risposta = await client.From<Expense>()
            .Where(e => e.SpaceId == spazioId)
            .Filter("spent_on", Constants.Operator.GreaterThanOrEqual, da.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Filter("spent_on", Constants.Operator.LessThanOrEqual, a.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Order("spent_on", Constants.Ordering.Descending)
            .Get();

        return risposta.Models;
    }

    public async Task<Expense?> LeggiAsync(Guid speseId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Expense>().Where(e => e.Id == speseId).Get();
        return risposta.Models.FirstOrDefault();
    }

    /// <summary>Crea una spesa. L'id lo genera QUESTO metodo, con Guid.NewGuid(), e non il
    /// database: una spesa si segna al bar con la rete che va e viene, e se l'inserimento fallisce
    /// a metà non si sa se è passato. Con l'id generato dal client il ritentativo porta lo stesso
    /// uuid, quindi o passa (idempotente) o viene rifiutato dalla chiave primaria — mai una spesa
    /// doppia. Con l'id generato dal database ne nascerebbero due, identiche e indistinguibili.
    /// <para>
    /// Il pagante va passato esplicitamente e non dedotto qui, per la stessa ragione per cui
    /// <see cref="NoteRepository.CreaAsync"/> prende l'autore: la policy pretende
    /// paid_by = auth.uid(), quindi sbagliarlo produce un rifiuto netto invece di una riga firmata
    /// male.
    /// </para>
    /// </summary>
    public async Task<Expense> CreaAsync(Guid spazioId, Guid pagante, decimal importo, string descrizione, string categoria, DateTime data)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Expense>().Insert(new Expense
        {
            Id          = Guid.NewGuid(),
            SpaceId     = spazioId,
            PaidBy      = pagante,
            Amount      = importo,
            Description = descrizione.Trim(),
            Category    = categoria.Trim(),
            SpentOn     = data
        });

        return risposta.Models.FirstOrDefault()
               ?? throw new InvalidOperationException("Il database non ha restituito la spesa appena creata.");
    }

    /// <summary>
    /// Salva, ma solo se nessuno ha scritto dopo di te: <paramref name="versioneLetta"/> entra
    /// come FILTRO, non come valore da scrivere — su version il client non ha nemmeno il
    /// privilegio di colonna. Zero righe toccate significa "non se ne fa niente", ma i motivi
    /// sono tre e vanno distinti, altrimenti si dice all'utente di riprovare quando riprovare
    /// non serve.
    /// </summary>
    public async Task<RisultatoSalvataggio<Expense>> SalvaAsync(Guid speseId, int versioneLetta, decimal importo, string descrizione, string categoria, DateTime data)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Expense>()
            .Where(e => e.Id == speseId && e.Version == versioneLetta)
            .Set(e => e.Amount, importo)
            .Set(e => e.Description, descrizione.Trim())
            .Set(e => e.Category, categoria.Trim())
            .Set(e => e.SpentOn, data)
            .Update();

        if (risposta.Models.Count > 0)
            return new RisultatoSalvataggio<Expense>(EsitoSalvataggio.Salvata, risposta.Models[0]);

        // Una seconda lettura per capire QUALE dei tre casi è: la prima query non lo dice, perché
        // "filtrata dalla RLS" e "versione non combacia" producono entrambe zero righe.
        var attuale = await LeggiAsync(speseId);

        if (attuale is null)
            return new RisultatoSalvataggio<Expense>(EsitoSalvataggio.Sparita, null);

        // La distinzione è quasi sempre esatta, ma non del tutto, e conviene saperlo: se chi salva
        // non ha diritto di scrittura E nell'intervallo fra l'UPDATE e questa rilettura chi ha
        // diritto salva davvero, la versione risulta cambiata e il caso viene letto come Conflitto
        // invece che come Rifiutata. Stessa finestra di NoteRepository.SalvaAsync, stesso motivo:
        // v. quel metodo.
        return attuale.Version != versioneLetta
            ? new RisultatoSalvataggio<Expense>(EsitoSalvataggio.Conflitto, attuale)
            : new RisultatoSalvataggio<Expense>(EsitoSalvataggio.Rifiutata, attuale);
    }

    /// <summary>Elimina. False se la RLS ha rifiutato — non sei né chi ha pagato né il proprietario
    /// dello spazio. Non lancia: il rifiuto è una risposta, non un guasto.
    /// <para>
    /// La lettura preventiva non è una cautela generica, è necessaria proprio QUI, come in
    /// <see cref="NoteRepository.EliminaAsync"/>: su 'expenses' expenses_select chiede
    /// is_space_member(space_id), mentre expenses_delete chiede paid_by = auth.uid() or
    /// is_space_owner(space_id). Sono le stesse condizioni di 'notes' (notes_select con
    /// is_space_member, notes_delete con owner_id = auth.uid() or is_space_owner), che si
    /// intersecano invece di contenersi: chi è membro dello spazio ma non ha pagato e non è
    /// proprietario vede la spesa ma non può cancellarla. La DELETE tocca zero righe E la rilettura
    /// di conferma ne trova zero, ma perché non ha il diritto di vederle, non perché siano sparite.
    /// Senza questa lettura il metodo direbbe "eliminata" su una spesa ancora lì e visibile a tutti
    /// gli altri membri — lo stesso rischio descritto nel commento lungo di
    /// NoteRepository.EliminaAsync, e qui vale identico: serve la stessa lettura prima e dopo.
    /// </para>
    /// <para>
    /// Il rovescio, dichiarato: siccome le due condizioni si intersecano, esiste anche il caso
    /// opposto — chi ha pagato una spesa ma poi è uscito dallo spazio soddisfa la condizione di
    /// DELETE (paid_by = auth.uid() non dipende dall'appartenenza) ma non quella di SELECT
    /// (is_space_member fallisce): la lettura preventiva non trova niente, il metodo restituisce
    /// false prima ancora di provare a cancellare, e chi chiama legge "non trovata" su una spesa
    /// che invece c'è, e che sarebbe stato in diritto di rimuovere. È un vicolo cieco raro, e si
    /// accetta perché l'alternativa — cancellare alla cieca senza lettura preventiva — direbbe
    /// "eliminata" su una spesa ancora lì e visibile a tutti gli altri membri, che è la bugia
    /// peggiore delle due.
    /// </para>
    /// </summary>
    public async Task<bool> EliminaAsync(Guid speseId)
    {
        var client = await _supabase.GetClientAsync();

        var prima = await client.From<Expense>().Where(e => e.Id == speseId).Get();
        if (prima.Models.Count == 0) return false;

        await client.From<Expense>().Where(e => e.Id == speseId).Delete();

        var dopo = await client.From<Expense>().Where(e => e.Id == speseId).Get();
        return dopo.Models.Count == 0;
    }
}
