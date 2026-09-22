using Eton.Models;
using Supabase.Postgrest;
using Supabase.Postgrest.Responses;

namespace Eton.Services;

/// <summary>
/// Accesso alle regole di spesa ricorrente. Come <see cref="ExpenseRepository"/>: ogni metodo
/// riparte da <see cref="SupabaseService.GetClientAsync"/> e non tiene mai il client in un campo.
/// Nessun controllo di autorizzazione qui dentro: lo fa la RLS.
/// </summary>
public class RecurringExpenseRepository
{
    private readonly SupabaseService _supabase;

    public RecurringExpenseRepository(SupabaseService supabase) => _supabase = supabase;

    /// <summary>Le regole di uno spazio, dalla modificata più di recente, come note e collezioni.
    /// A differenza di quelle, l'ordine NON è coperto da un indice: 'recurring_expenses_space_idx'
    /// è sul solo space_id, e il database ordina a parte — su poche decine di regole per spazio
    /// non conta.</summary>
    public async Task<IReadOnlyList<RecurringExpense>> ElencaAsync(Guid spazioId)
    {
        var client = await _supabase.GetClientAsync();

        var risposta = await client.From<RecurringExpense>()
            .Where(r => r.SpaceId == spazioId)
            .Order("updated_at", Constants.Ordering.Descending)
            .Get();

        return risposta.Models;
    }

    public async Task<RecurringExpense?> LeggiAsync(Guid regolaId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<RecurringExpense>().Where(r => r.Id == regolaId).Get();
        return risposta.Models.FirstOrDefault();
    }

    /// <summary>Crea una regola. L'id lo genera QUESTO metodo, con Guid.NewGuid(), come in
    /// <see cref="ExpenseRepository.CreaAsync"/> — stesso motivo: v. lì.</summary>
    public async Task<RecurringExpense> CreaAsync(Guid spazioId, Guid pagante, decimal importo, string descrizione, string categoria, int ogniMesi, int giorno, DateTime inizio)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<RecurringExpense>().Insert(new RecurringExpense
        {
            Id                   = Guid.NewGuid(),
            SpaceId              = spazioId,
            PaidBy               = pagante,
            Amount               = importo,
            Description          = descrizione.Trim(),
            Category             = categoria.Trim(),
            EveryMonths          = ogniMesi,
            DayOfMonth           = giorno,
            StartsOn             = ExpenseRepository.PerIlDatabase(inizio),
            EndsOn               = null,
            MaterializedThrough  = null
        });

        return risposta.Models.FirstOrDefault()
               ?? throw new InvalidOperationException("Il database non ha restituito la regola ricorrente appena creata.");
    }

    /// <summary>
    /// Salva, ma solo se nessuno ha scritto dopo di te: <paramref name="versioneLetta"/> entra
    /// come FILTRO, non come valore da scrivere — su version il client non ha nemmeno il
    /// privilegio di colonna. Zero righe toccate significa "non se ne fa niente", ma i motivi
    /// sono tre e vanno distinti, altrimenti si dice all'utente di riprovare quando riprovare
    /// non serve.
    /// </summary>
    public async Task<RisultatoSalvataggio<RecurringExpense>> SalvaAsync(Guid regolaId, int versioneLetta, decimal importo, string descrizione, string categoria, int ogniMesi, int giorno, DateTime inizio)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<RecurringExpense>()
            .Where(r => r.Id == regolaId && r.Version == versioneLetta)
            .Set(r => r.Amount, importo)
            .Set(r => r.Description, descrizione.Trim())
            .Set(r => r.Category, categoria.Trim())
            .Set(r => r.EveryMonths, ogniMesi)
            .Set(r => r.DayOfMonth, giorno)
            .Set(r => r.StartsOn, ExpenseRepository.PerIlDatabase(inizio))
            .Update();

        return await Esito(risposta, regolaId, versioneLetta);
    }

    /// <summary>Termina una regola fissandone la fine, senza toccare nient'altro: esiste come
    /// metodo a sé perché "Termina" è un'azione diversa e non deve poter cambiare altro per
    /// sbaglio.</summary>
    public async Task<RisultatoSalvataggio<RecurringExpense>> TerminaAsync(Guid regolaId, int versioneLetta, DateTime fine)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<RecurringExpense>()
            .Where(r => r.Id == regolaId && r.Version == versioneLetta)
            .Set(r => r.EndsOn!, ExpenseRepository.PerIlDatabase(fine))
            .Update();

        return await Esito(risposta, regolaId, versioneLetta);
    }

    /// <summary>
    /// La distinzione a zero righe fra Sparita, Conflitto e Rifiutata: identica in
    /// <see cref="SalvaAsync"/> e <see cref="TerminaAsync"/>, estratta qui per non copiarla due
    /// volte. Stessa finestra di imprecisione fra Conflitto e Rifiutata descritta in
    /// <see cref="ExpenseRepository.SalvaAsync"/>: v. lì.
    /// </summary>
    private async Task<RisultatoSalvataggio<RecurringExpense>> Esito(ModeledResponse<RecurringExpense> risposta, Guid regolaId, int versioneLetta)
    {
        if (risposta.Models.Count > 0)
            return new RisultatoSalvataggio<RecurringExpense>(EsitoSalvataggio.Salvata, risposta.Models[0]);

        var attuale = await LeggiAsync(regolaId);

        if (attuale is null)
            return new RisultatoSalvataggio<RecurringExpense>(EsitoSalvataggio.Sparita, null);

        return attuale.Version != versioneLetta
            ? new RisultatoSalvataggio<RecurringExpense>(EsitoSalvataggio.Conflitto, attuale)
            : new RisultatoSalvataggio<RecurringExpense>(EsitoSalvataggio.Rifiutata, attuale);
    }

    /// <summary>Elimina. False se la RLS ha rifiutato. Come <see cref="ExpenseRepository.EliminaAsync"/>,
    /// la lettura preventiva e quella di conferma servono a distinguere "non trovata" da "filtrata
    /// dalla RLS" — v. lì per il motivo esteso.
    /// <para>
    /// Una regola che ha già generato occorrenze non si elimina: la chiave esterna di
    /// expenses.recurring_id (no action) fa fallire la DELETE con 23503, e l'eccezione risale al
    /// chiamante. In quel caso si termina con <see cref="TerminaAsync"/>, non si elimina.
    /// </para>
    /// </summary>
    public async Task<bool> EliminaAsync(Guid regolaId)
    {
        var client = await _supabase.GetClientAsync();

        var prima = await client.From<RecurringExpense>().Where(r => r.Id == regolaId).Get();
        if (prima.Models.Count == 0) return false;

        await client.From<RecurringExpense>().Where(r => r.Id == regolaId).Delete();

        var dopo = await client.From<RecurringExpense>().Where(r => r.Id == regolaId).Get();
        return dopo.Models.Count == 0;
    }

    /// <summary>Avanza il watermark dopo che la materializzazione ha scritto le occorrenze del
    /// periodo. Nessun filtro di versione, di proposito: il valore lo calcolano allo stesso modo
    /// tutte le schede aperte su questa regola. Il trigger alza comunque 'version', quindi un
    /// editor aperto sulla stessa regola vedrà un Conflitto al salvataggio successivo —
    /// conseguenza accettata.</summary>
    public async Task AvanzaWatermarkAsync(Guid regolaId, string periodo)
    {
        var client = await _supabase.GetClientAsync();
        await client.From<RecurringExpense>()
            .Where(r => r.Id == regolaId)
            .Set(r => r.MaterializedThrough!, periodo)
            .Update();
    }
}
