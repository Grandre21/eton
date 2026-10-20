using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class CategorieSpesaTests
{
    [Fact]
    public void Una_categoria_vale_solo_per_il_suo_tipo()
    {
        Assert.True(CategorieSpesa.Conosciuta("Stipendio", TipoMovimento.Entrata));
        Assert.False(CategorieSpesa.Conosciuta("Stipendio", TipoMovimento.Spesa));
        Assert.True(CategorieSpesa.Conosciuta("Casa", TipoMovimento.Spesa));
        Assert.False(CategorieSpesa.Conosciuta("Casa", TipoMovimento.Entrata));
        Assert.True(CategorieSpesa.Conosciuta("Altro", TipoMovimento.Spesa));
        Assert.True(CategorieSpesa.Conosciuta("Altro", TipoMovimento.Entrata));
        Assert.Equal(["Stipendio", "Rimborso", "Regalo", "Vendita", "Altro"], CategorieSpesa.Entrate);
        Assert.Equal(CategorieSpesa.Tutte.Count, CategorieSpesa.Tutte.Distinct().Count());
    }

    [Fact]
    public void L_elenco_non_e_vuoto()
        => Assert.NotEmpty(CategorieSpesa.Elenco);

    [Fact]
    public void L_elenco_non_ha_duplicati()
        => Assert.Equal(CategorieSpesa.Elenco.Count, CategorieSpesa.Elenco.Distinct().Count());

    [Fact]
    public void L_elenco_non_ha_stringhe_vuote_o_di_soli_spazi()
        => Assert.All(CategorieSpesa.Elenco, categoria => Assert.False(string.IsNullOrWhiteSpace(categoria)));

    [Fact]
    public void Una_categoria_dell_elenco_e_conosciuta()
        => Assert.True(CategorieSpesa.Conosciuta("Casa", TipoMovimento.Spesa));

    [Fact]
    public void Una_categoria_fuori_dall_elenco_non_e_conosciuta()
        => Assert.False(CategorieSpesa.Conosciuta("Animali", TipoMovimento.Spesa));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("spesa")]  // minuscolo: il confronto è ordinale, non case-insensitive
    [InlineData("CASA")]   // tutto maiuscolo: stesso motivo
    public void Varianti_di_maiuscole_e_valori_vuoti_non_sono_conosciuti(string? categoria)
        // Il confronto è ordinale e sensibile alle maiuscole di proposito: le categorie non le
        // digita nessuno, le sceglie da un elenco chiuso mostrato dall'interfaccia a pastiglie.
        // Una variante di maiuscole quindi non è un utente distratto ma un dato arrivato da
        // un'altra strada, e vale la pena che si veda come "sconosciuta" invece di passare.
        => Assert.False(CategorieSpesa.Conosciuta(categoria, TipoMovimento.Spesa));
}
