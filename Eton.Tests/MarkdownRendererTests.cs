using Eton.Services;

namespace Eton.Tests;

public class MarkdownRendererTests
{
    private readonly MarkdownRenderer _rendi = new();

    // ---------- formattazione ----------

    [Fact]
    public void Il_grassetto_diventa_strong()
        => Assert.Contains("<strong>ciao</strong>", _rendi.InHtml("**ciao**"));

    [Fact]
    public void Le_checklist_diventano_caselle()
    {
        var html = _rendi.InHtml("- [x] fatto\n- [ ] da fare");
        Assert.Contains("type=\"checkbox\"", html);
        Assert.Contains("checked", html);
    }

    [Fact]
    public void Un_link_normale_resta_cliccabile()
        => Assert.Contains("href=\"https://esempio.it/\"", _rendi.InHtml("[qui](https://esempio.it/)"));

    [Fact]
    public void Il_testo_vuoto_non_produce_html()
    {
        Assert.Equal("", _rendi.InHtml(null));
        Assert.Equal("", _rendi.InHtml("   "));
    }

    [Fact]
    public void Il_testo_semplice_perde_la_formattazione()
        => Assert.DoesNotContain("**", _rendi.InTestoSemplice("un **titolo** e del testo"));

    // ---------- sicurezza: HTML grezzo ----------
    // Queste sono le prove che contano: in uno spazio condiviso la nota la scrive qualcun altro.

    [Fact]
    public void Lo_script_grezzo_non_sopravvive()
    {
        var html = _rendi.InHtml("<script>alert(1)</script>");
        Assert.DoesNotContain("<script>", html);
    }

    [Fact]
    public void Un_gestore_di_evento_in_html_grezzo_non_sopravvive()
    {
        var html = _rendi.InHtml("<img src=x onerror=alert(1)>");

        // DisableHtml non cancella il tag: lo riscrive come testo, con < e > sostituiti dalle
        // rispettive entità. La parola "onerror" resta quindi leggibile a schermo, ed è innocua
        // precisamente perché non esiste alcun elemento <img> a cui possa agganciarsi.
        // L'asserzione va fatta sul TAG, non sulla parola: cercare "onerror" verificherebbe una
        // proprietà che non serve a nessuno e che il rendering corretto non garantisce.
        Assert.DoesNotContain("<img", html);
        Assert.Contains("&lt;img", html);
    }

    // ---------- sicurezza: URL ----------

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("JaVaScRiPt:alert(1)")]
    [InlineData("  javascript:alert(1)")]
    [InlineData("java\tscript:alert(1)")]
    [InlineData("java\nscript:alert(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD4=")]
    [InlineData("vbscript:msgbox(1)")]
    public void Uno_schema_pericoloso_viene_respinto(string url)
        => Assert.False(MarkdownRenderer.UrlAmmessa(url));

    [Theory]
    [InlineData("https://esempio.it")]
    [InlineData("http://esempio.it/pagina?x=1")]
    [InlineData("mailto:tizio@esempio.it")]
    [InlineData("/pagina/interna")]
    [InlineData("pagina-relativa.html")]
    [InlineData("#ancora")]
    [InlineData("pagina/a:b")]        // i due punti dopo uno / non sono uno schema
    public void Un_indirizzo_legittimo_passa(string url)
        => Assert.True(MarkdownRenderer.UrlAmmessa(url));

    [Fact]
    public void Un_link_javascript_viene_neutralizzato_ma_il_testo_resta()
    {
        var html = _rendi.InHtml("[premi qui](javascript:alert(1))");
        Assert.DoesNotContain("javascript", html);
        Assert.Contains("premi qui", html);
    }

    [Fact]
    public void Anche_un_immagine_con_schema_pericoloso_viene_neutralizzata()
    {
        var html = _rendi.InHtml("![x](javascript:alert(1))");
        Assert.DoesNotContain("javascript", html);
    }

    [Fact]
    public void Il_link_automatico_di_un_indirizzo_scritto_nudo_resta_sicuro()
    {
        var html = _rendi.InHtml("scrivimi su https://esempio.it e poi vediamo");
        Assert.Contains("href=\"https://esempio.it\"", html);
    }

    // La sintassi <schema:...> di CommonMark produce un AutolinkInline, che è un tipo di nodo
    // DIVERSO da LinkInline. La prima versione della bonifica camminava solo sui LinkInline e
    // lasciava passare questo caso intatto: era una XSS vera, trovata in revisione. Il test resta
    // qui perché la differenza fra i due tipi di nodo non si vede leggendo il codice.
    [Fact]
    public void Un_autolink_fra_parentesi_angolari_con_schema_pericoloso_viene_neutralizzato()
    {
        var html = _rendi.InHtml("<javascript:alert(document.cookie)>");
        Assert.DoesNotContain("javascript", html);
    }

    [Fact]
    public void Un_autolink_fra_parentesi_angolari_legittimo_resta_cliccabile()
    {
        var html = _rendi.InHtml("<https://esempio.it/pagina>");
        Assert.Contains("href=\"https://esempio.it/pagina\"", html);
    }

    // ---------- robustezza ----------
    // Markdig lancia oltre un certo annidamento. L'estratto si calcola dentro il @foreach
    // dell'elenco note, quindi un'eccezione qui non romperebbe una nota: romperebbe la pagina di
    // chiunque stia in quello spazio.

    [Fact]
    public void Un_annidamento_assurdo_non_fa_esplodere_il_rendering()
    {
        var mostro = new string('>', 500) + " testo";

        var html = _rendi.InHtml(mostro);
        var semplice = _rendi.InTestoSemplice(mostro);

        Assert.NotNull(html);
        Assert.NotNull(semplice);
        // Qualunque cosa esca, non deve contenere marcatura attiva non prevista.
        Assert.DoesNotContain("<script", html);
    }

    [Fact]
    public void Anche_una_nota_enorme_non_fa_esplodere_il_rendering()
    {
        var enorme = string.Join("\n\n", Enumerable.Repeat("Paragrafo con **grassetto** e [link](https://esempio.it).", 2000));
        Assert.NotEmpty(_rendi.InHtml(enorme));
    }
}
