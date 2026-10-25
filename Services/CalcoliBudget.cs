using Eton.Models;

namespace Eton.Services;

public sealed record StatoBudget(CategoryBudget Budget, decimal Speso)
{
    public decimal Residuo => Budget.Amount - Speso;
    public int Quota => (int)Math.Clamp(Math.Round(Speso / Budget.Amount * 100, MidpointRounding.AwayFromZero), 0, 100);
}

/// <summary>Calcoli puri sui budget e sul riepilogo già letto del registro.</summary>
public static class CalcoliBudget
{
    public static DateTime PrimoDelMese(DateTime data) => new(data.Year, data.Month, 1);

    public static bool ValeNelMese(CategoryBudget b, int anno, int mese)
    {
        var primo = new DateTime(anno, mese, 1);
        return b.StartsOn.Date <= primo && (b.EndsOn is null || b.EndsOn.Value.Date >= primo);
    }

    public static IReadOnlyList<StatoBudget> DelMese(IEnumerable<CategoryBudget> budget, RiepilogoMese riepilogo, int anno, int mese)
    {
        // Il residuo usa le stesse righe del totale di CalcoliSpese.PerMese, che esclude già le
        // entrate: ricalcolare dalle spese farebbe divergere budget e registro.
        var speso = riepilogo.Categorie.ToDictionary(c => c.Categoria, c => c.Totale, StringComparer.Ordinal);
        var ordine = CategorieSpesa.Elenco.Select((c, i) => (c, i)).ToDictionary(v => v.c, v => v.i, StringComparer.Ordinal);
        return budget.Where(b => ValeNelMese(b, anno, mese))
            .OrderBy(b => ordine.GetValueOrDefault(b.Category, int.MaxValue))
            .ThenBy(b => b.Category, StringComparer.Ordinal)
            .Select(b => new StatoBudget(b, speso.GetValueOrDefault(b.Category)))
            .ToList();
    }

    public static decimal DaSpendere(IEnumerable<StatoBudget> stati) => stati.Where(s => s.Residuo > 0).Sum(s => s.Residuo);

    public static IReadOnlyList<string> CategorieDisponibili(IEnumerable<CategoryBudget> budget)
    {
        var aperte = budget.Where(b => b.EndsOn is null).Select(b => b.Category).ToHashSet(StringComparer.Ordinal);
        return CategorieSpesa.Elenco.Where(c => !aperte.Contains(c)).ToList();
    }

    public static DateTime InizioDiUnNuovo(IEnumerable<CategoryBudget> budget, string categoria, DateTime oggi)
    {
        // Il database vieta due aperti, ma non la sovrapposizione con l'ultimo mese di uno chiuso.
        var inizio = PrimoDelMese(oggi);
        foreach (var b in budget.Where(b => StringComparer.Ordinal.Equals(b.Category, categoria) && b.EndsOn is not null))
        {
            var dopo = b.EndsOn!.Value.AddMonths(1);
            if (dopo > inizio) inizio = dopo;
        }
        return inizio;
    }

    public static bool SiPuoChiudere(CategoryBudget b, DateTime oggi)
        => b.EndsOn is null && b.StartsOn.Date <= PrimoDelMese(oggi);
}
