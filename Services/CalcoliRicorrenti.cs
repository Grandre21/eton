using System.Globalization;

namespace Eton.Services;

/// <summary>Un'occorrenza dovuta di una regola ricorrente. <see cref="Periodo"/> è <c>yyyy-MM</c> ed
/// è la chiave di idempotenza: la stessa occorrenza non si materializza due volte.</summary>
public sealed record PeriodoDovuto(string Periodo, DateTime Data);

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
}
