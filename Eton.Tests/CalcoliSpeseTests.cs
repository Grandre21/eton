using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class CalcoliSpeseTests
{
    private static Expense Spesa(decimal importo, string categoria, int anno, int mese, int giorno)
        => new()
        {
            Id = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            PaidBy = Guid.NewGuid(),
            Amount = importo,
            Description = "Spesa",
            Category = categoria,
            SpentOn = new DateTime(anno, mese, giorno)
        };

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
