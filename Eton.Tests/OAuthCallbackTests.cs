using Eton.Services;

namespace Eton.Tests;

public class OAuthCallbackTests
{
    [Fact]
    public void Url_normale_non_e_un_ritorno_oauth()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/eton/");
        Assert.Null(esito.Codice);
        Assert.Null(esito.Errore);
    }

    [Fact]
    public void Estrae_il_codice_dalla_query()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/eton/?code=abc123");
        Assert.Equal("abc123", esito.Codice);
        Assert.Null(esito.Errore);
    }

    [Fact]
    public void Estrae_il_codice_anche_con_altri_parametri()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?stato=x&code=abc123&altro=y");
        Assert.Equal("abc123", esito.Codice);
    }

    [Fact]
    public void Decodifica_i_valori_percent_encoded()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?error_description=Accesso%20negato");
        Assert.Equal("Accesso negato", esito.Errore);
        Assert.Null(esito.Codice);
    }

    [Fact]
    public void Il_parametro_error_vale_come_errore_anche_senza_descrizione()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?error=access_denied");
        Assert.Equal("access_denied", esito.Errore);
    }

    [Fact]
    public void Un_errore_ha_la_precedenza_sul_codice()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?code=abc&error=access_denied");
        Assert.Null(esito.Codice);
        Assert.Equal("access_denied", esito.Errore);
    }

    [Fact]
    public void Non_confonde_un_parametro_che_finisce_per_code()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?postcode=20100");
        Assert.Null(esito.Codice);
    }

    [Fact]
    public void Non_legge_un_codice_che_sta_nel_fragment()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/#frammento?code=nascosto");
        Assert.Null(esito.Codice);
        Assert.Null(esito.Errore);
    }

    [Fact]
    public void Non_legge_un_errore_che_sta_nel_fragment()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/#error=access_denied");
        Assert.Null(esito.Errore);
    }

    [Fact]
    public void Ignora_il_fragment_che_segue_una_query_valida()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?code=abc#altro?code=nascosto");
        Assert.Equal("abc", esito.Codice);
    }

    [Fact]
    public void Un_valore_vuoto_non_vale_come_codice()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?code=");
        Assert.Null(esito.Codice);
    }

    [Fact]
    public void Una_sequenza_percent_encoded_rotta_non_lancia()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?code=ab%zz");
        Assert.Equal("ab%zz", esito.Codice);
    }
}
