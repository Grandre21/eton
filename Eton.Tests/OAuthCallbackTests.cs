using Eton.Services;

namespace Eton.Tests;

public class OAuthCallbackTests
{
    [Fact]
    public void Url_normale_non_e_un_ritorno_oauth()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/eton/");
        Assert.Null(esito.Codice);
        Assert.Equal(OAuthRifiuto.Nessuno, esito.Errore);
    }

    [Fact]
    public void Estrae_il_codice_dalla_query()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/eton/?code=abc123");
        Assert.Equal("abc123", esito.Codice);
        Assert.Equal(OAuthRifiuto.Nessuno, esito.Errore);
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
        var esito = OAuthCallback.Analizza("https://esempio.it/?code=ab%20c");
        Assert.Equal("ab c", esito.Codice);
        Assert.Equal(OAuthRifiuto.Nessuno, esito.Errore);
    }

    [Fact]
    public void Access_denied_senza_codice_e_un_annullamento()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?error=access_denied");
        Assert.Equal(OAuthRifiuto.Annullato, esito.Errore);
    }

    [Fact]
    public void Un_errore_ha_la_precedenza_sul_codice()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?code=abc&error=access_denied");
        Assert.Null(esito.Codice);
        Assert.Equal(OAuthRifiuto.Annullato, esito.Errore);
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
        Assert.Equal(OAuthRifiuto.Nessuno, esito.Errore);
    }

    [Fact]
    public void Non_legge_un_errore_che_sta_nel_fragment()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/#error=access_denied");
        Assert.Equal(OAuthRifiuto.Nessuno, esito.Errore);
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

    [Fact]
    public void Una_descrizione_senza_error_non_e_un_rifiuto()
    {
        // È l'URL dell'attacco: solo error_description, senza error.
        var esito = OAuthCallback.Analizza("https://esempio.it/?error_description=Account+bloccato,+chiama+il+numero");
        Assert.Equal(OAuthRifiuto.Nessuno, esito.Errore);
        Assert.Null(esito.Codice);
        Assert.Null(esito.Diagnostica);
    }

    [Fact]
    public void Access_denied_con_un_codice_di_errore_non_e_un_annullamento()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?error=access_denied&error_code=signup_disabled");
        Assert.Equal(OAuthRifiuto.Generico, esito.Errore);
    }

    [Fact]
    public void Un_codice_di_stato_non_valido_e_una_scadenza()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?error=invalid_request&error_code=bad_oauth_state");
        Assert.Equal(OAuthRifiuto.Scaduto, esito.Errore);
    }

    [Theory]
    [InlineData("bad_oauth_state")]
    [InlineData("bad_oauth_callback")]
    [InlineData("flow_state_already_used")]
    public void Ogni_codice_di_stato_scaduto_e_una_scadenza(string codiceErrore)
    {
        var esito = OAuthCallback.Analizza($"https://esempio.it/?error=invalid_request&error_code={codiceErrore}");
        Assert.Equal(OAuthRifiuto.Scaduto, esito.Errore);
    }

    [Fact]
    public void Un_errore_mai_visto_cade_sul_generico()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?error=temporarily_unavailable");
        Assert.Equal(OAuthRifiuto.Generico, esito.Errore);
    }

    [Fact]
    public void Una_descrizione_accanto_a_un_codice_valido_non_lo_invalida()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?code=abc&error_description=x");
        Assert.Equal("abc", esito.Codice);
        Assert.Equal(OAuthRifiuto.Nessuno, esito.Errore);
    }

    [Fact]
    public void La_diagnostica_raccoglie_tutti_e_tre_i_parametri_grezzi()
    {
        var esito = OAuthCallback.Analizza(
            "https://esempio.it/?error=access_denied&error_code=signup_disabled&error_description=Le+iscrizioni+sono+chiuse");

        Assert.NotNull(esito.Diagnostica);
        Assert.Contains("access_denied", esito.Diagnostica);
        Assert.Contains("signup_disabled", esito.Diagnostica);
        Assert.Contains("Le iscrizioni sono chiuse", esito.Diagnostica);
    }

    [Theory]
    [InlineData("https://esempio.it/?error_description=ESCA")]
    [InlineData("https://esempio.it/?error=access_denied&error_description=ESCA")]
    [InlineData("https://esempio.it/?error=invalid_request&error_code=bad_oauth_state&error_description=ESCA")]
    [InlineData("https://esempio.it/?error=chissa&error_code=chissa&error_description=ESCA")]
    [InlineData("https://esempio.it/?code=abc&error_description=ESCA")]
    public void La_descrizione_del_provider_non_esce_mai_dal_canale_diagnostico(string uri)
    {
        var esito = OAuthCallback.Analizza(uri);

        // Ogni proprietà pubblica dell'esito che non sia Diagnostica è un canale che una schermata
        // può leggere: nessuna di esse deve poter contenere testo scelto da un estraneo. Si guarda
        // per reflection, così sono coperte anche le proprietà che ancora non esistono — ed è
        // ToString() e non "as string" perché con "as string" la sequenza sarebbe VUOTA nei casi
        // d'attacco (Codice è null, Errore è un enum), e Assert.All su una sequenza vuota passa
        // sempre: il test non asserirebbe niente proprio dove serve. Così invece Errore ci rientra,
        // e ci rientrerebbe una proprietà futura di qualunque tipo il cui ToString() porti il testo.
        var visibili = typeof(OAuthCallbackEsito)
            .GetProperties()
            .Where(p => p.Name != nameof(OAuthCallbackEsito.Diagnostica))
            .Select(p => p.GetValue(esito)?.ToString())
            .Where(v => v is not null);

        Assert.All(visibili, v => Assert.DoesNotContain("ESCA", v!, StringComparison.Ordinal));
    }

    // Il test da solo non basterebbe: una proprietà stringa esistente potrebbe tornare a ospitare
    // testo del provider con una semplice riassegnazione, e la reflection se ne accorgerebbe solo
    // a runtime, con un test che già esiste. A chiudere la porta per sempre è il tipo: Errore è un
    // enum, quindi assegnargli di nuovo una stringa libera è un errore di compilazione (CS0029),
    // non un'eccezione a runtime. Il test presidia le proprietà nuove, il tipo presidia quella vecchia.
}
