using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class CalcoliBudgetTests
{
    private static CategoryBudget Budget(string categoria = "Casa", decimal importo = 100, int mese = 10, int? fine = null)
        => new() { Id = Guid.NewGuid(), Category = categoria, Amount = importo,
            StartsOn = new DateTime(2026, mese, 1), EndsOn = fine is int f ? new DateTime(2026, f, 1) : null };

    private static Expense Spesa(decimal importo, string categoria = "Casa", int mese = 10, string tipo = TipoMovimento.Spesa)
        => new() { Id = Guid.NewGuid(), Amount = importo, Category = categoria, Kind = tipo, SpentOn = new DateTime(2026, mese, 2) };

    [Fact]
    public void Lo_speso_e_il_totale_della_categoria_nel_riepilogo()
    {
        var r = new RiepilogoMese(37, [new("Casa", 37, 100)], 0, null);
        Assert.Equal(37, Assert.Single(CalcoliBudget.DelMese([Budget()], r, 2026, 10)).Speso);
    }

    [Fact]
    public void Un_entrata_in_Altro_non_consuma_il_budget()
    {
        var r = CalcoliSpese.PerMese([Spesa(20, "Altro"), Spesa(1000, "Altro", tipo: TipoMovimento.Entrata)], 2026, 10);
        Assert.Equal(20, Assert.Single(CalcoliBudget.DelMese([Budget("Altro")], r, 2026, 10)).Speso);
    }

    [Fact]
    public void Una_spesa_di_un_altro_mese_non_conta()
    {
        var r = CalcoliSpese.PerMese([Spesa(20), Spesa(99, mese: 9)], 2026, 10);
        Assert.Equal(20, Assert.Single(CalcoliBudget.DelMese([Budget()], r, 2026, 10)).Speso);
    }

    [Fact]
    public void Un_budget_futuro_o_chiuso_prima_non_compare()
        => Assert.Empty(CalcoliBudget.DelMese([Budget(mese: 11), Budget(mese: 8, fine: 9)], CalcoliSpese.PerMese([], 2026, 10), 2026, 10));

    [Fact]
    public void Il_mese_di_fine_compare()
        => Assert.True(CalcoliBudget.ValeNelMese(Budget(fine: 10), 2026, 10));

    [Fact]
    public void Sforare_da_residuo_negativo_e_quota_cento()
    {
        var s = new StatoBudget(Budget(), 150);
        Assert.Equal(-50, s.Residuo);
        Assert.Equal(100, s.Quota);
    }

    [Fact]
    public void DaSpendere_somma_solo_residui_positivi_e_senza_budget_e_zero()
    {
        Assert.Equal(60, CalcoliBudget.DaSpendere([new(Budget(), 40), new(Budget(), 150)]));
        Assert.Equal(0, CalcoliBudget.DaSpendere([]));
    }

    [Fact]
    public void Le_categorie_disponibili_escludono_solo_budget_aperti()
    {
        var categorie = CalcoliBudget.CategorieDisponibili([Budget("Casa"), Budget("Spesa", fine: 10)]);
        Assert.DoesNotContain("Casa", categorie);
        Assert.Contains("Spesa", categorie);
    }

    [Fact]
    public void Un_nuovo_budget_senza_precedenti_parte_nel_mese_corrente()
        => Assert.Equal(new DateTime(2026, 10, 1), CalcoliBudget.InizioDiUnNuovo([], "Casa", new(2026, 10, 25)));

    [Fact]
    public void Un_precedente_chiuso_questo_mese_sposta_al_successivo()
        => Assert.Equal(new DateTime(2026, 11, 1), CalcoliBudget.InizioDiUnNuovo([Budget(fine: 10)], "Casa", new(2026, 10, 25)));

    [Fact]
    public void Un_precedente_chiuso_nel_passato_lascia_il_mese_corrente()
        => Assert.Equal(new DateTime(2026, 10, 1), CalcoliBudget.InizioDiUnNuovo([Budget(mese: 8, fine: 9)], "Casa", new(2026, 10, 25)));

    [Fact]
    public void Non_si_chiude_un_budget_futuro_o_gia_chiuso()
    {
        Assert.False(CalcoliBudget.SiPuoChiudere(Budget(mese: 11), new(2026, 10, 25)));
        Assert.False(CalcoliBudget.SiPuoChiudere(Budget(fine: 10), new(2026, 10, 25)));
        Assert.True(CalcoliBudget.SiPuoChiudere(Budget(), new(2026, 10, 25)));
    }

    [Fact]
    public void Ordine_elenco_poi_ordinale_e_confronto_esatto()
    {
        var r = new RiepilogoMese(10, [new("casa", 10, 100)], 0, null);
        var stati = CalcoliBudget.DelMese([Budget("Zeta"), Budget("Casa"), Budget("Alfa"), Budget("Spesa")], r, 2026, 10);
        Assert.Equal(["Spesa", "Casa", "Alfa", "Zeta"], stati.Select(s => s.Budget.Category));
        Assert.All(stati, s => Assert.Equal(0, s.Speso));
    }

    [Fact]
    public void Quota_arrotonda_lontano_da_zero_e_non_scende_sotto_zero()
    {
        Assert.Equal(13, new StatoBudget(Budget(importo: 200), 25).Quota);
        Assert.Equal(0, new StatoBudget(Budget(), -1).Quota);
        Assert.Equal(new DateTime(2026, 10, 1), CalcoliBudget.PrimoDelMese(new(2026, 10, 25, 12, 30, 0)));
    }
}
