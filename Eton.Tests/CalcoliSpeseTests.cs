using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class CalcoliSpeseTests
{
    private static Expense Spesa(decimal importo, string categoria, int anno, int mese, int giorno, string tipo = TipoMovimento.Spesa)
        => new()
        {
            Id = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            PaidBy = Guid.NewGuid(),
            Amount = importo,
            Kind = tipo,
            Description = "Spesa",
            Category = categoria,
            SpentOn = new DateTime(anno, mese, giorno)
        };

    [Fact]
    public void Le_entrate_non_entrano_nel_totale_ne_nelle_categorie()
    {
        var r = CalcoliSpese.PerMese([Spesa(60, "Casa", 2026, 8, 1), Spesa(40, "Svago", 2026, 8, 2),
            Spesa(1500, "Stipendio", 2026, 8, 3, TipoMovimento.Entrata)], 2026, 8);
        Assert.Equal(100m, r.Totale);
        Assert.Equal(1500m, r.Entrate);
        Assert.Equal(1400m, r.Saldo);
        Assert.Equal(["Casa", "Svago"], r.Categorie.Select(c => c.Categoria));
        Assert.Equal(100, r.Categorie.Sum(c => c.Quota));
    }

    [Fact]
    public void Il_confronto_col_mese_prima_guarda_solo_le_spese()
    {
        var r = CalcoliSpese.PerMese([Spesa(150, "Casa", 2026, 8, 1), Spesa(100, "Casa", 2026, 7, 1),
            Spesa(1500, "Stipendio", 2026, 7, 1, TipoMovimento.Entrata)], 2026, 8);
        Assert.Equal(100m, r.TotalePrecedente);
        Assert.Equal(50, r.VariazionePercentuale);
    }

    [Fact]
    public void BilancioDi_separa_spese_ed_entrate()
    {
        var b = CalcoliSpese.BilancioDi([Spesa(20, "Casa", 2026, 8, 1),
            Spesa(100, "Stipendio", 2026, 8, 1, TipoMovimento.Entrata), Spesa(5, "Altro", 2026, 8, 1, "boh")]);
        Assert.Equal(25m, b.Spese);
        Assert.Equal(100m, b.Entrate);
        Assert.Equal(75m, b.Saldo);
        var vuoto = CalcoliSpese.BilancioDi([]);
        Assert.Equal(0m, vuoto.Spese);
        Assert.Equal(0m, vuoto.Entrate);
        Assert.Equal(0m, vuoto.Saldo);
    }

    [Fact]
    public void ConSegno_mette_piu_meno_e_niente_sullo_zero()
    {
        Assert.Equal("+1.500,00", CalcoliSpese.ConSegno(1500));
        Assert.Equal("−20,50", CalcoliSpese.ConSegno(-20.50m));
        Assert.Equal("0,00", CalcoliSpese.ConSegno(0));
    }

    // ---------- PerMese ----------

    [Fact]
    public void Un_mese_senza_spese_da_totale_zero_e_nessuna_categoria()
    {
        var riepilogo = CalcoliSpese.PerMese([], 2026, 8);

        Assert.Equal(0m, riepilogo.Totale);
        Assert.Empty(riepilogo.Categorie);
    }

    [Fact]
    public void Le_spese_di_altri_mesi_non_entrano_nel_totale_del_mese_chiesto()
    {
        var spese = new[]
        {
            Spesa(10m, "Spesa", 2026, 8, 1),
            Spesa(999m, "Spesa", 2026, 7, 15),
            Spesa(999m, "Spesa", 2025, 8, 15),
        };

        var riepilogo = CalcoliSpese.PerMese(spese, 2026, 8);

        Assert.Equal(10m, riepilogo.Totale);
        Assert.Single(riepilogo.Categorie);
    }

    [Fact]
    public void Le_categorie_sono_ordinate_per_totale_decrescente()
    {
        var spese = new[]
        {
            Spesa(10m, "Trasporti", 2026, 8, 1),
            Spesa(50m, "Casa", 2026, 8, 2),
            Spesa(30m, "Svago", 2026, 8, 3),
        };

        var riepilogo = CalcoliSpese.PerMese(spese, 2026, 8);

        Assert.Equal(["Casa", "Svago", "Trasporti"], riepilogo.Categorie.Select(c => c.Categoria));
    }

    [Fact]
    public void Le_quote_sommano_a_cento_anche_quando_la_divisione_non_e_esatta()
    {
        // Tre categorie da un terzo ciascuna: arrotondate per difetto darebbero 33+33+33=99, non
        // 100. È esattamente il caso che il metodo del resto maggiore deve correggere.
        var spese = new[]
        {
            Spesa(10m, "Casa", 2026, 8, 1),
            Spesa(10m, "Svago", 2026, 8, 2),
            Spesa(10m, "Trasporti", 2026, 8, 3),
        };

        var riepilogo = CalcoliSpese.PerMese(spese, 2026, 8);

        Assert.Equal(100, riepilogo.Categorie.Sum(c => c.Quota));
    }

    [Fact]
    public void Variazione_percentuale_e_nulla_quando_il_mese_precedente_e_vuoto()
    {
        var spese = new[] { Spesa(50m, "Casa", 2026, 8, 1) };

        var riepilogo = CalcoliSpese.PerMese(spese, 2026, 8);

        Assert.Null(riepilogo.VariazionePercentuale);
    }

    [Fact]
    public void Variazione_percentuale_e_positiva_quando_si_spende_di_piu()
    {
        var spese = new[]
        {
            Spesa(150m, "Casa", 2026, 8, 1),
            Spesa(100m, "Casa", 2026, 7, 1),
        };

        var riepilogo = CalcoliSpese.PerMese(spese, 2026, 8);

        Assert.Equal(50, riepilogo.VariazionePercentuale);
    }

    [Fact]
    public void Variazione_percentuale_e_negativa_quando_si_spende_di_meno()
    {
        var spese = new[]
        {
            Spesa(50m, "Casa", 2026, 8, 1),
            Spesa(100m, "Casa", 2026, 7, 1),
        };

        var riepilogo = CalcoliSpese.PerMese(spese, 2026, 8);

        Assert.Equal(-50, riepilogo.VariazionePercentuale);
    }

    [Fact]
    public void Il_confronto_con_gennaio_guarda_dicembre_dell_anno_prima()
    {
        var spese = new[]
        {
            Spesa(100m, "Casa", 2026, 1, 1),
            Spesa(100m, "Casa", 2025, 12, 1),
        };

        var riepilogo = CalcoliSpese.PerMese(spese, 2026, 1);

        Assert.Equal(100m, riepilogo.TotalePrecedente);
        Assert.Equal(0, riepilogo.VariazionePercentuale);
    }

    // ---------- NomeMese ----------

    [Fact]
    public void Il_nome_del_mese_di_agosto_e_agosto()
        => Assert.Equal("agosto", CalcoliSpese.NomeMese(8));

    [Fact]
    public void Un_numero_di_mese_fuori_intervallo_non_manda_in_crash()
    {
        Assert.Equal("", CalcoliSpese.NomeMese(0));
        Assert.Equal("", CalcoliSpese.NomeMese(13));
    }
}
