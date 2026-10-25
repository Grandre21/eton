using Eton.Models;
using Supabase.Postgrest;

namespace Eton.Services;

/// <summary>I dati di una regola ricorrente per <see cref="RecurringExpenseRepository.CreaAsync"/> e
/// <see cref="RecurringExpenseRepository.SalvaAsync"/>: due interi adiacenti come
/// <c>ogniMesi</c> e <c>giorno</c> scambiati compilano lo stesso se sono argomenti posizionali,
/// con i nomi no. Kind vale solo in creazione. Rate e TotalePiano valorizzati = piano a rate (v. <see cref="CalcoliRicorrenti.DatiPiano"/>).</summary>
public sealed record DatiRegola(decimal Importo, string Descrizione, string Categoria, int OgniMesi, int Giorno, DateTime Inizio, int? Rate = null, decimal? TotalePiano = null, string Kind = TipoMovimento.Spesa);

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
    public async Task<RecurringExpense> CreaAsync(Guid spazioId, Guid pagante, DatiRegola dati)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<RecurringExpense>().Insert(NuovaRegola(spazioId, pagante, dati));

        return risposta.Models.FirstOrDefault()
               ?? throw new InvalidOperationException("Il database non ha restituito la regola ricorrente appena creata.");
    }

    // Separata da CreaAsync per provare senza database come i dati diventano riga: date a mezzanotte UTC, fine del piano, testi senza spazi.
    internal static RecurringExpense NuovaRegola(Guid spazioId, Guid pagante, DatiRegola dati) =>
        new RecurringExpense
        {
            Id                   = Guid.NewGuid(),
            SpaceId              = spazioId,
            PaidBy               = pagante,
            Amount               = dati.Importo,
            Kind                 = dati.Kind,
            Description          = dati.Descrizione.Trim(),
            Category             = dati.Categoria.Trim(),
            EveryMonths          = dati.OgniMesi,
            DayOfMonth           = dati.Giorno,
            StartsOn             = ExpenseRepository.PerIlDatabase(dati.Inizio),
            EndsOn               = dati.Rate is int n ? ExpenseRepository.PerIlDatabase(CalcoliRicorrenti.FinePiano(dati.Inizio, n)) : null,
            Installments         = dati.Rate,
            PlanTotal            = dati.TotalePiano,
            MaterializedThrough  = null
        };

    /// <summary>
    /// Salva, ma solo se nessuno ha scritto dopo di te: <paramref name="versioneLetta"/> entra
    /// come FILTRO, non come valore da scrivere — su version il client non ha nemmeno il
    /// privilegio di colonna. Zero righe toccate significa "non se ne fa niente", ma i motivi
    /// sono tre e vanno distinti, altrimenti si dice all'utente di riprovare quando riprovare
    /// non serve.
    /// </summary>
    public async Task<RisultatoSalvataggio<RecurringExpense>> SalvaAsync(Guid regolaId, int versioneLetta, DatiRegola dati)
    {
        var client = await _supabase.GetClientAsync();
        var query = client.From<RecurringExpense>()
            .Where(r => r.Id == regolaId && r.Version == versioneLetta)
            .Set(r => r.Amount, dati.Importo)
            .Set(r => r.Description, dati.Descrizione.Trim())
            .Set(r => r.Category, dati.Categoria.Trim())
            .Set(r => r.EveryMonths, dati.OgniMesi)
            .Set(r => r.DayOfMonth, dati.Giorno)
            .Set(r => r.StartsOn, ExpenseRepository.PerIlDatabase(dati.Inizio));

        // Senza Rate non si tocca ends_on, altrimenti salvare una regola terminata la riaprirebbe.
        if (dati.Rate is int n)
            query = query.Set(r => r.Installments!, n)
                .Set(r => r.PlanTotal!, dati.TotalePiano!.Value)
                .Set(r => r.EndsOn!, ExpenseRepository.PerIlDatabase(CalcoliRicorrenti.FinePiano(dati.Inizio, n)));

        var risposta = await query.Update();

        return await Esito(risposta.Models, versioneLetta, () => LeggiAsync(regolaId));
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

        return await Esito(risposta.Models, versioneLetta, () => LeggiAsync(regolaId));
    }

    /// <summary>
    /// La distinzione a zero righe fra Sparita, Conflitto e Rifiutata: identica in
    /// <see cref="SalvaAsync"/> e <see cref="TerminaAsync"/>, estratta qui per non copiarla due
    /// volte. Stessa finestra di imprecisione fra Conflitto e Rifiutata descritta in
    /// <see cref="ExpenseRepository.SalvaAsync"/>: v. lì. Il delegate serve ai test senza database.
    /// </summary>
    internal static async Task<RisultatoSalvataggio<RecurringExpense>> Esito(IReadOnlyList<RecurringExpense> toccate, int versioneLetta, Func<Task<RecurringExpense?>> rileggi)
    {
        if (toccate.Count > 0)
            return new RisultatoSalvataggio<RecurringExpense>(EsitoSalvataggio.Salvata, toccate[0]);

        var attuale = await rileggi();

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
