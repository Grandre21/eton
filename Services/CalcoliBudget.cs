using System.Text.Json;
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

    // La chiusura scrive come ultimo mese quello corrente: un budget che finisce già questo mese o prima non si chiude, o si allungherebbe.
    public static bool SiPuoChiudere(CategoryBudget b, DateTime oggi)
        => b.StartsOn.Date <= PrimoDelMese(oggi) && (b.EndsOn is null || b.EndsOn.Value.Date > PrimoDelMese(oggi));

    /// <summary>La durata massima di un budget a periodo, in mesi.</summary>
    public const int MesiMassimi = 24;

    /// <summary>La descrizione più lunga di un budget: 200 (check di category_budgets.description).</summary>
    public const int DescrizioneMassima = 200;

    /// <summary>L'ultimo mese coperto da un budget di <paramref name="mesi"/> mesi, mese di inizio compreso.
    /// Un valore fuori da 1..<see cref="MesiMassimi"/> lancia: degradare darebbe un risultato plausibile e sbagliato.</summary>
    public static DateTime FineDaMesi(DateTime inizio, int mesi)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(mesi, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(mesi, MesiMassimi);
        return PrimoDelMese(inizio).AddMonths(mesi - 1);
    }

    /// <summary>Il periodo in parole: «da gennaio 2027, senza fine», «solo gennaio 2027»,
    /// «da gennaio a marzo 2027», «da novembre 2026 a febbraio 2027».</summary>
    public static string TestoPeriodo(DateTime inizio, DateTime? fine)
    {
        var da = CalcoliSpese.NomeMese(inizio.Month);
        if (fine is not { } f) return $"da {da} {inizio.Year}, senza fine";
        var a = CalcoliSpese.NomeMese(f.Month);
        if (f.Year != inizio.Year) return $"da {da} {inizio.Year} a {a} {f.Year}";
        return f.Month == inizio.Month ? $"solo {da} {inizio.Year}" : $"da {da} a {a} {inizio.Year}";
    }

    // 23P01 = esclusione category_budgets_no_overlap (HTTP 400); 23505 = indice univoco category_budgets_aperto_uq dei budget aperti (HTTP 409),
    // l'unico conflitto di unicità possibile da un INSERT su category_budgets (l'id lo genera il client).
    public static bool RifiutoPerSovrapposizione(int stato, string? corpo)
    {
        if (stato is not (400 or 409) || string.IsNullOrWhiteSpace(corpo)) return false;
        try
        {
            using var documento = JsonDocument.Parse(corpo);
            return documento.RootElement.ValueKind == JsonValueKind.Object
                && documento.RootElement.TryGetProperty("code", out var codice)
                && codice.ValueKind == JsonValueKind.String
                && (stato == 400 ? codice.GetString() == "23P01" : codice.GetString() == "23505");
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>La tabella delle spese sul mese, solo spese e senza filtro di categoria: quelle da portare nel budget stanno in altre categorie.</summary>
    public static string IndirizzoSpeseDelMese(int anno, int mese)
        => "expenses/table" + new VistaTabella(TipoPeriodo.Libero, new DateTime(anno, mese, 1),
            new DateTime(anno, mese, DateTime.DaysInMonth(anno, mese)),
            FiltriSpese.Nessuno with { Tipo = TipoMovimento.Spesa }, TabellaSpese.Colonne.Data, true, null).InQuery();
}
