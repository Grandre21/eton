using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class SchemaCampiTests
{
    // ---------- ChiaveDa ----------

    [Theory]
    [InlineData("Marca", "marca")]
    [InlineData("Nicotina (mg)", "nicotina_mg")]   // i non alfanumerici diventano _, i _ ripetuti collassano, quelli ai lati si tolgono
    [InlineData("Città", "citta")]                  // gli accenti si riducono al carattere base
    [InlineData("  ", "campo")]                     // ripiego per un'etichetta che non produce niente
    [InlineData("2024", "campo_2024")]              // una chiave non comincia per cifra
    public void La_chiave_si_ricava_dall_etichetta(string etichetta, string attesa)
        => Assert.Equal(attesa, SchemaCampi.ChiaveDa(etichetta, []));

    [Fact]
    public void Una_etichetta_lunghissima_produce_una_chiave_troncata_a_40_caratteri()
    {
        var etichetta = new string('a', 100);
        Assert.Equal(40, SchemaCampi.ChiaveDa(etichetta, []).Length);
    }

    [Fact]
    public void Una_chiave_gia_usata_riceve_un_suffisso_numerico_progressivo()
    {
        Assert.Equal("marca_2", SchemaCampi.ChiaveDa("Marca", ["marca"]));
        Assert.Equal("marca_3", SchemaCampi.ChiaveDa("Marca", ["marca", "marca_2"]));
    }

    // ---------- Valida: casi validi ----------

    [Fact]
    public void Un_elenco_vuoto_e_valido()
        => Assert.True(SchemaCampi.Valida([]).Valido);

    [Fact]
    public void Un_campo_di_testo_ben_formato_e_valido()
    {
        var campi = new List<CampoDefinizione> { new() { Key = "marca", Label = "Marca", Type = "text", Order = 0 } };
        Assert.True(SchemaCampi.Valida(campi).Valido);
    }

    [Fact]
    public void Un_campo_select_con_due_opzioni_e_valido()
    {
        var campi = new List<CampoDefinizione>
        {
            new() { Key = "colore", Label = "Colore", Type = "select", Options = ["Rosso", "Blu"], Order = 0 },
        };
        Assert.True(SchemaCampi.Valida(campi).Valido);
    }

    // ---------- Valida: casi non validi ----------
    // Un caso per test: se ne cadono due insieme non si capisce quale regola sia stata violata.

    [Fact]
    public void Una_chiave_vuota_non_e_valida()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "", Label = "Marca", Type = "text" }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Una_chiave_con_maiuscole_o_spazi_non_e_valida()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "Marca Preferita", Label = "Marca", Type = "text" }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Due_campi_con_la_stessa_chiave_non_sono_validi()
    {
        var campi = new List<CampoDefinizione>
        {
            new() { Key = "marca", Label = "Marca", Type = "text", Order = 0 },
            new() { Key = "marca", Label = "Marca 2", Type = "text", Order = 1 },
        };
        var esito = SchemaCampi.Valida(campi);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Una_etichetta_vuota_non_e_valida()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "marca", Label = "", Type = "text" }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Un_tipo_non_ammesso_non_e_valido()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "marca", Label = "Marca", Type = "colore-esotico" }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Un_select_senza_opzioni_non_e_valido()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "colore", Label = "Colore", Type = "select", Options = null }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Un_select_con_una_sola_opzione_non_e_valido()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "colore", Label = "Colore", Type = "select", Options = ["Rosso"] }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Un_select_con_due_opzioni_identiche_non_e_valido()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "colore", Label = "Colore", Type = "select", Options = ["Rosso", "Rosso"] }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Un_campo_non_select_con_opzioni_non_e_valido()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "marca", Label = "Marca", Type = "text", Options = ["Rosso", "Blu"] }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Un_campo_con_etichetta_nulla_non_fa_esplodere_la_validazione()
    {
        // Newtonsoft sovrascrive con null l'inizializzatore "= """ del modello quando il jsonb
        // contiene "label": null, e il vincolo SQL collections_fields_shape controlla solo che
        // fields sia un array di al massimo 40 elementi, mai la forma dei singoli elementi: un
        // null qui dentro è quindi ammissibile per il database, ed è compito di Valida segnalarlo.
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "marca", Label = null!, Type = "text", Order = 1 }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Un_campo_con_chiave_nulla_non_fa_esplodere_la_validazione()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = null!, Label = "Marca", Type = "text", Order = 1 }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Un_select_con_una_opzione_nulla_non_e_valido()
    {
        var esito = SchemaCampi.Valida([new CampoDefinizione { Key = "pgvg", Label = "PG/VG", Type = "select", Options = ["50/50", null!] }]);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    [Fact]
    public void Piu_del_massimo_di_campi_non_e_valido()
    {
        var campi = Enumerable.Range(0, SchemaCampi.MassimoCampi + 1)
            .Select(i => new CampoDefinizione { Key = $"campo_{i}", Label = $"Campo {i}", Type = "text", Order = i })
            .ToList();

        var esito = SchemaCampi.Valida(campi);
        Assert.False(esito.Valido);
        Assert.NotEmpty(esito.Errori);
    }

    // ---------- Ordina ----------

    [Fact]
    public void Ordina_per_order_crescente()
    {
        var campi = new List<CampoDefinizione>
        {
            new() { Key = "c", Label = "C", Order = 2 },
            new() { Key = "a", Label = "A", Order = 0 },
            new() { Key = "b", Label = "B", Order = 1 },
        };

        var ordinati = SchemaCampi.Ordina(campi);
        Assert.Equal(["a", "b", "c"], ordinati.Select(c => c.Key).ToArray());
    }

    [Fact]
    public void A_parita_di_order_l_ordinamento_e_stabile()
    {
        // Due campi con lo stesso Order non devono scambiarsi a ogni ridisegno: un ordinamento
        // instabile farebbe "ballare" i campi nell'interfaccia senza che l'utente abbia fatto nulla.
        var campi = new List<CampoDefinizione>
        {
            new() { Key = "primo", Label = "Primo", Order = 0 },
            new() { Key = "secondo", Label = "Secondo", Order = 0 },
        };

        var ordinati = SchemaCampi.Ordina(campi);
        Assert.Equal(["primo", "secondo"], ordinati.Select(c => c.Key).ToArray());
    }

    // ---------- Modelli ----------

    [Fact]
    public void Esiste_un_modello_per_i_liquidi()
        => Assert.Contains(SchemaCampi.Modelli, m => m.Nome.Contains("Liquidi"));

    [Fact]
    public void Ogni_modello_e_valido()
    {
        // Un modello precompilato non valido sarebbe una trappola: l'utente lo sceglie e non
        // riesce a salvare.
        foreach (var modello in SchemaCampi.Modelli)
            Assert.True(SchemaCampi.Valida(modello.Campi).Valido);
    }

    [Fact]
    public void In_ogni_modello_le_chiavi_sono_uniche()
    {
        foreach (var modello in SchemaCampi.Modelli)
            Assert.Equal(modello.Campi.Count, modello.Campi.Select(c => c.Key).Distinct().Count());
    }

    [Fact]
    public void I_modelli_non_si_lasciano_modificare_da_chi_li_usa()
    {
        // Chi parte da un modello scrive modello.Campi.ToList(), che copia il contenitore e non
        // gli elementi. In WASM lo stato statico vive quanto la scheda del browser, quindi un
        // modello corrotto resta corrotto fino a un ricaricamento completo.
        var prima = SchemaCampi.Modelli.First(m => m.Nome == "Birre");
        var etichettaOriginale = prima.Campi[0].Label;
        prima.Campi[0].Label = "Manomesso";
        var dopo = SchemaCampi.Modelli.First(m => m.Nome == "Birre");
        Assert.Equal(etichettaOriginale, dopo.Campi[0].Label);
    }
}
