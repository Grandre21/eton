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
    public void FineDaMesi_conta_il_mese_di_inizio()
    {
        Assert.Equal(new DateTime(2026, 11, 1), CalcoliBudget.FineDaMesi(new(2026, 11, 1), 1));
        Assert.Equal(new DateTime(2027, 1, 1), CalcoliBudget.FineDaMesi(new(2026, 11, 1), 3));
        Assert.Equal(new DateTime(2028, 10, 1), CalcoliBudget.FineDaMesi(new(2026, 11, 25), 24));
        Assert.Throws<ArgumentOutOfRangeException>(() => CalcoliBudget.FineDaMesi(new(2026, 11, 1), 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => CalcoliBudget.FineDaMesi(new(2026, 11, 1), 25));
    }

    [Fact]
    public void Un_budget_a_periodo_si_chiude_prima_della_fine_ma_non_si_allunga()
    {
        var oggi = new DateTime(2026, 10, 25);
        Assert.True(CalcoliBudget.SiPuoChiudere(Budget(mese: 10, fine: 12), oggi));
        Assert.False(CalcoliBudget.SiPuoChiudere(Budget(mese: 10, fine: 10), oggi));
        Assert.False(CalcoliBudget.SiPuoChiudere(Budget(mese: 8, fine: 9), oggi));
        Assert.False(CalcoliBudget.SiPuoChiudere(Budget(mese: 11, fine: 12), oggi));
    }

    [Fact]
    public void TestoPeriodo_nei_quattro_casi()
    {
        Assert.Equal("da gennaio 2027, senza fine", CalcoliBudget.TestoPeriodo(new(2027, 1, 1), null));
        Assert.Equal("solo gennaio 2027", CalcoliBudget.TestoPeriodo(new(2027, 1, 1), new(2027, 1, 1)));
        Assert.Equal("da gennaio a marzo 2027", CalcoliBudget.TestoPeriodo(new(2027, 1, 1), new(2027, 3, 1)));
        Assert.Equal("da novembre 2026 a febbraio 2027", CalcoliBudget.TestoPeriodo(new(2026, 11, 1), new(2027, 2, 1)));
    }

    [Fact]
    public void Il_rifiuto_23P01_si_riconosce_solo_col_400_e_il_suo_codice()
    {
        const string corpo = "{\"code\":\"23P01\",\"details\":\"Key conflicts with existing key.\",\"hint\":null,\"message\":\"conflicting key value violates exclusion constraint \\\"category_budgets_no_overlap\\\"\"}";
        Assert.True(CalcoliBudget.RifiutoPerSovrapposizione(400, corpo));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(409, corpo));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(400, "{\"code\":\"23505\"}"));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(400, null));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(400, ""));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(400, "non json"));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(400, "[]"));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(400, "{\"code\":23}"));
    }

    [Fact]
    public void Il_rifiuto_23505_del_budget_aperto_si_riconosce_col_409()
    {
        const string corpo = "{\"code\":\"23505\",\"details\":\"Key (space_id, category)=(…) already exists.\",\"hint\":null,\"message\":\"duplicate key value violates unique constraint \\\"category_budgets_aperto_uq\\\"\"}";
        Assert.True(CalcoliBudget.RifiutoPerSovrapposizione(409, corpo));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(400, corpo));
        Assert.False(CalcoliBudget.RifiutoPerSovrapposizione(409, "{\"code\":\"23503\"}"));
    }

    [Fact]
    public void La_scorciatoia_apre_la_tabella_sul_mese_tipo_spesa_senza_categoria()
    {
        const string percorso = "expenses/table";
        var indirizzo = CalcoliBudget.IndirizzoSpeseDelMese(2026, 2);
        Assert.StartsWith(percorso + "?", indirizzo);
        var vista = VistaTabella.DaQuery(indirizzo[percorso.Length..]);
        Assert.Equal(TipoPeriodo.Libero, vista.Periodo);
        Assert.Equal(new DateTime(2026, 2, 1), vista.Da);
        Assert.Equal(new DateTime(2026, 2, 28), vista.A);
        Assert.Equal(TipoMovimento.Spesa, vista.Filtri.Tipo);
        Assert.Empty(vista.Filtri.Categorie);
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
