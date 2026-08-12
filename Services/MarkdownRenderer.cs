using System.Net;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Eton.Services;

/// <summary>
/// Rende in HTML il Markdown di una nota. Registrato come singleton: la pipeline si costruisce
/// una volta sola ed è immutabile.
/// <para>
/// È codice di sicurezza, non di presentazione. In uno spazio condiviso il testo lo scrive
/// qualcun altro, quindi ogni nota è potenzialmente ostile. Le difese sono due e servono
/// entrambe: <c>DisableHtml()</c> per l'HTML grezzo, e la bonifica delle URL qui sotto per i
/// link — che Markdig genera per conto suo e non controlla in alcun modo.
/// </para>
/// </summary>
public sealed class MarkdownRenderer
{
    // Le estensioni sono scelte a mano e NON con UseAdvancedExtensions(): quella scorciatoia
    // include UseMediaLinks(), che trasforma certi link in <iframe>. Su testo scritto da altri
    // significa regalare un iframe a chiunque sappia scrivere un URL di YouTube.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseTaskLists()
        .UseAutoLinks()
        .UsePipeTables()
        .UseEmphasisExtras()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    /// <summary>Il Markdown reso in HTML, già bonificato: il risultato si può inserire con
    /// <c>MarkupString</c> senza ulteriori passaggi.</summary>
    public string InHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return "";

        try
        {
            var documento = Markdown.Parse(markdown, Pipeline);
            Bonifica(documento);
            return documento.ToHtml(Pipeline);
        }
        catch (Exception ex)
        {
            // Markdig ha un tetto di annidamento e lancia quando viene superato: bastano ~130 '>'
            // consecutivi, cioè una catena di citazioni incollata da un'email. Senza questa rete
            // l'eccezione risalirebbe fino al rendering di Blazor, e siccome l'estratto si calcola
            // DENTRO il @foreach dell'elenco, una sola nota malformata romperebbe la pagina di
            // chiunque stia in quello spazio — non solo di chi l'ha scritta.
            Console.Error.WriteLine($"[Markdown] Testo non interpretabile, lo mostro grezzo: {ex.Message}");

            // Il ripiego è sicuro per costruzione: HtmlEncode non può produrre marcatura attiva.
            return $"<p>{WebUtility.HtmlEncode(markdown)}</p>";
        }
    }

    /// <summary>
    /// Neutralizza ogni URL che non si può mettere in un <c>href</c>.
    /// <para>
    /// Si interviene sull'albero e non sull'HTML già prodotto: una sostituzione a stringhe sul
    /// risultato finale è una gara a chi conosce più codifiche, e la si perde sempre.
    /// </para>
    /// </summary>
    private static void Bonifica(MarkdownDocument documento)
    {
        foreach (var link in documento.Descendants<LinkInline>())
        {
            if (!UrlAmmessa(link.Url))
            {
                // Il testo del link resta leggibile — chi legge deve poter capire che c'era un
                // collegamento — ma non porta più da nessuna parte.
                link.Url = "#";
            }
        }

        // AutolinkInline è un tipo di nodo DIVERSO da LinkInline: lo produce la sintassi
        // <schema:...> di CommonMark. Trattarlo a parte non è pignoleria — senza questo ciclo
        // <javascript:fetch(...)> arriva intatto nell'href, DisableHtml() non lo sfiora (l'ancora
        // la genera Markdig, non l'autore della nota) e il risultato finisce in un MarkupString.
        // Era una XSS vera: in uno spazio condiviso bastava una nota per leggere la sessione
        // Supabase di chiunque cliccasse.
        foreach (var autolink in documento.Descendants<AutolinkInline>())
        {
            if (!UrlAmmessa(autolink.Url))
            {
                // Qui il testo mostrato È l'URL, quindi azzerandolo si perde anche quello. È il
                // compromesso giusto: un <javascript:...> scritto in buona fede non esiste.
                autolink.Url = "#";
            }
        }
    }

    /// <summary>Il testo senza formattazione, per gli estratti nell'elenco delle note.</summary>
    public string InTestoSemplice(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return "";

        try
        {
            return Markdown.ToPlainText(markdown, Pipeline);
        }
        catch (Exception ex)
        {
            // Stesso tetto di annidamento di InHtml. Il ripiego qui è il testo grezzo così com'è:
            // finisce in un'interpolazione Razor normale, che l'escape lo fa da sé.
            Console.Error.WriteLine($"[Markdown] Estratto non calcolabile, uso il testo grezzo: {ex.Message}");
            return markdown;
        }
    }

    /// <summary>
    /// Vero se l'URL si può mettere in un <c>href</c> senza rischi.
    /// <para>
    /// Non basta guardare il prefisso: prima di interpretare lo schema i browser scartano spazi e
    /// caratteri di controllo, quindi <c>" java&#9;script:alert(1)"</c> viene eseguito benissimo
    /// pur non cominciando per "javascript". Per questo si giudica la stringa ripulita, non
    /// quella originale.
    /// </para>
    /// </summary>
    internal static bool UrlAmmessa(string? url)
    {
        if (url is null) return false;

        var pulita = new string(url.Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c)).ToArray());
        if (pulita.Length == 0) return false;

        var duePunti = pulita.IndexOf(':');
        if (duePunti < 0) return true;   // nessuno schema: è relativa, e una relativa non esegue nulla

        // Un ':' che arriva dopo il primo /, ? o # non introduce uno schema: sta dentro il
        // percorso o la query, e "pagina/a:b" è un indirizzo relativo del tutto legittimo.
        var separatore = pulita.IndexOfAny(['/', '?', '#']);
        if (separatore >= 0 && separatore < duePunti) return true;

        var schema = pulita[..duePunti].ToLowerInvariant();
        return schema is "http" or "https" or "mailto";
    }
}
