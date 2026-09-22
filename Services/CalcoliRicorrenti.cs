using System.Globalization;
using Eton.Models;

namespace Eton.Services;

/// <summary>Un'occorrenza dovuta di una regola ricorrente. <see cref="Periodo"/> è <c>yyyy-MM</c> ed
/// è la chiave di idempotenza: la stessa occorrenza non si materializza due volte.</summary>
public sealed record PeriodoDovuto(string Periodo, DateTime Data);

/// <summary>Il risultato della fusione fra le spese vere e le occorrenze delle regole ricorrenti
/// per un intervallo. <c>Righe</c> = vere + previste scadute, sono ciò che entra nei totali;
/// <c>Previste</c> = gli Id sintetici delle previste dentro <c>Righe</c>, le pagine le marcano e
/// non ci mettono un collegamento; <c>InArrivo</c> = occorrenze con data futura rispetto a oggi,
/// fuori da <c>Righe</c> e da ogni totale, per la vista tabellare.</summary>
public sealed record SpeseDelPeriodo(IReadOnlyList<Expense> Righe, IReadOnlySet<Guid> Previste,
    IReadOnlyList<Expense> InArrivo);

/// <summary>
/// Calcoli puri sulle regole ricorrenti: quali occorrenze sono dovute in una finestra di date e
/// quando cade la prossima. Nessun accesso a rete o a stato, solo aritmetica su date già in memoria.
/// </summary>
public static class CalcoliRicorrenti
{
    /// <summary>Le occorrenze dovute in <c>[da.Date, a.Date]</c>, in ordine cronologico crescente,
    /// escludendo quelle già coperte dal watermark <paramref name="materializzatoFinoA"/>.
    /// A differenza di <see cref="CalcoliSpese.NomeMese"/>, qui un valore fuori intervallo lancia,
    /// perché degradare darebbe un risultato plausibile e sbagliato (un <paramref name="giorno"/> 99
    /// troncato all'ultimo del mese, un <paramref name="ogniMesi"/> negativo che si comporta come
    /// positivo); gli intervalli sono gli stessi dei check del database (<c>every_months</c> 1..12,
    /// <c>day_of_month</c> 1..31).</summary>
    public static IReadOnlyList<PeriodoDovuto> Dovuti(
        DateTime inizio, DateTime? fine, int ogniMesi, int giorno,
        string? materializzatoFinoA, DateTime da, DateTime a)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ogniMesi, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ogniMesi, 12);
        ArgumentOutOfRangeException.ThrowIfLessThan(giorno, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(giorno, 31);

        var risultato = new List<PeriodoDovuto>();

        var inizioIterazione = da.Date > inizio.Date ? da.Date : inizio.Date;
        var fineIterazione = fine.HasValue && fine.Value.Date < a.Date ? fine.Value.Date : a.Date;
        if (fineIterazione < inizioIterazione)
            return risultato;

        var meseBase = inizio.Year * 12 + inizio.Month - 1;
        var ultimo = fineIterazione.Year * 12 + fineIterazione.Month - 1;

        for (var indice = inizioIterazione.Year * 12 + inizioIterazione.Month - 1; indice <= ultimo; indice++)
        {
            if ((indice - meseBase) % ogniMesi != 0) continue;

            var anno = indice / 12;
            var mese = indice % 12 + 1;
            var data = new DateTime(anno, mese, Math.Min(giorno, DateTime.DaysInMonth(anno, mese)));
            if (data < inizioIterazione || data > fineIterazione) continue;

            var periodo = Periodo(data);
            // "yyyy-MM" si ordina come stringa esattamente come le date che rappresenta.
            if (materializzatoFinoA is null || string.CompareOrdinal(periodo, materializzatoFinoA) > 0)
                risultato.Add(new PeriodoDovuto(periodo, data));
        }

        return risultato;
    }

    /// <summary>Il periodo <c>yyyy-MM</c> di una data.</summary>
    public static string Periodo(DateTime data) => data.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    /// <summary>La data della prima occorrenza dopo <paramref name="oggi"/>, o <c>null</c> se la
    /// regola è terminata. Ignora il watermark: serve a sapere cosa arriverà, non cosa manca.</summary>
    public static DateTime? Prossima(
        DateTime inizio, DateTime? fine, int ogniMesi, int giorno, DateTime oggi)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ogniMesi, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ogniMesi, 12);

        var da = oggi.Date.AddDays(1) > inizio.Date ? oggi.Date.AddDays(1) : inizio.Date;
        // Una regola la scrive il pagante e la leggono tutti i membri: una data d'inizio estrema non
        // deve far cadere la pagina di un altro. Oltre il limite di DateTime la finestra si ferma lì.
        var a = da <= DateTime.MaxValue.AddMonths(-(ogniMesi + 1)) ? da.AddMonths(ogniMesi + 1) : DateTime.MaxValue.Date;

        var dovuti = Dovuti(inizio, fine, ogniMesi, giorno, materializzatoFinoA: null, da, a);
        return dovuti.Count > 0 ? dovuti[0].Data : null;
    }

    /// <summary>L'occorrenza di una regola in un periodo, così come esiste — prevista o scritta. Una sola
    /// sede, perché una riga prevista e la stessa riga materializzata non devono poter divergere; il
    /// periodo è il primo giorno del mese (check expenses_recurring_period_primo_del_mese).</summary>
    public static Expense Occorrenza(RecurringExpense r, PeriodoDovuto d) => new()
    {
        Id              = Guid.NewGuid(),
        SpaceId         = r.SpaceId,
        PaidBy          = r.PaidBy,
        Amount          = r.Amount,
        Description     = r.Description,
        Category        = r.Category,
        SpentOn         = d.Data,
        RecurringId     = r.Id,
        RecurringPeriod = new DateTime(d.Data.Year, d.Data.Month, 1)
    };

    /// <summary>Fonde le spese vere con le occorrenze dovute delle regole ricorrenti nell'intervallo
    /// <c>[da, a]</c>: il criterio è il §5 del design delle ricorrenti — il totale è ciò che
    /// esisterebbe se ogni pagante avesse aperto l'app, e le occorrenze future non entrano.
    /// Rispetta il watermark di ciascuna regola: un buco lasciato dall'utente (v.
    /// <see cref="Dovuti"/>) resta un buco anche nella previsione.</summary>
    public static SpeseDelPeriodo Fondi(IReadOnlyList<Expense> vere, IReadOnlyList<RecurringExpense> regole,
        DateTime da, DateTime a, DateTime oggi)
    {
        var presenti = vere
            .Where(e => e.RecurringId is not null && e.RecurringPeriod is not null)
            .Select(e => (RecurringId: e.RecurringId!.Value, Periodo: Periodo(e.RecurringPeriod!.Value)))
            .ToHashSet();

        var previsteRighe = new List<Expense>();
        var inArrivo = new List<Expense>();

        foreach (var r in regole)
        {
            var dovuti = Dovuti(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, r.MaterializedThrough, da, a);
            foreach (var d in dovuti)
            {
                if (presenti.Contains((r.Id, d.Periodo))) continue;

                var occorrenza = Occorrenza(r, d);

                if (d.Data <= oggi.Date) previsteRighe.Add(occorrenza);
                else inArrivo.Add(occorrenza);
            }
        }

        var righe = vere.Concat(previsteRighe).OrderByDescending(e => e.SpentOn).ToList();
        var inArrivoOrdinato = inArrivo.OrderByDescending(e => e.SpentOn).ToList();
        var previste = previsteRighe.Select(e => e.Id).ToHashSet();

        return new SpeseDelPeriodo(righe, previste, inArrivoOrdinato);
    }
}
