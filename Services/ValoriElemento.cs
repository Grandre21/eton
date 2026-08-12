using System.Globalization;

namespace Eton.Services;

/// <summary>
/// Conversione fra il valore digitato dall'utente, quello leggibile a schermo e quello salvato nel
/// jsonb di <c>collection_items.data</c>, per ciascun tipo di <see cref="SchemaCampi.TipiAmmessi"/>.
/// <para>
/// Ogni formattazione usa <see cref="CultureInfo.InvariantCulture"/> più un pattern esplicito, mai
/// <see cref="CultureInfo.CurrentCulture"/>: con <c>InvariantGlobalization</c> attivo esiste una
/// sola cultura, quindi impostarne un'altra lancerebbe. La virgola nei numeri è una scelta
/// d'interfaccia — l'app è in italiano — non l'effetto di una cultura del browser: l'uscita è la
/// stessa per chiunque apra l'app, ovunque si trovi.
/// </para>
/// </summary>
public static class ValoriElemento
{
    private static readonly string[] FormatiData = ["yyyy-MM-dd", "dd/MM/yyyy"];

    /// <summary>
    /// Il valore reso leggibile a schermo.
    /// <para>
    /// <c>number</c>: al massimo due decimali, senza zeri finali inutili, separatore decimale
    /// virgola. <c>date</c>: formato <c>dd/MM/yyyy</c>. <c>bool</c>: "Sì" o "No". <c>text</c>,
    /// <c>url</c> e <c>select</c>: il valore così com'è. <c>null</c> diventa sempre stringa vuota.
    /// </para>
    /// <para>
    /// Accetta <see cref="object"/> perché dal jsonb un numero torna deserializzato come
    /// <see cref="long"/> o <see cref="double"/> a seconda che porti decimali, e una data può
    /// tornare come <see cref="string"/> (se salvata senza orario) o come <see cref="DateTime"/>
    /// (se Newtonsoft la riconosce come ISO 8601 completa): entrambi i casi vanno gestiti.
    /// </para>
    /// </summary>
    public static string Testo(object? valore, string tipo)
    {
        if (valore is null) return "";

        try
        {
            return tipo switch
            {
                "number" => FormattaNumero(Convert.ToDouble(valore, CultureInfo.InvariantCulture)),
                "date" => FormattaData(valore),
                "bool" => (bool)valore ? "Sì" : "No",
                _ => valore.ToString() ?? "",
            };
        }
        catch (Exception ex)
        {
            // Stesso ragionamento di MarkdownRenderer.InTestoSemplice: questo testo finisce dentro
            // un @foreach di una scheda elemento, e un'eccezione qui romperebbe la pagina di
            // chiunque stia in quello spazio, non solo la scheda del valore malformato.
            Console.Error.WriteLine($"[ValoriElemento] Valore non convertibile, lo mostro grezzo: {ex.Message}");
            return valore.ToString() ?? "";
        }
    }

    private static string FormattaNumero(double numero)
        // Il separatore decimale è una scelta d'interfaccia — l'app è in italiano — non l'effetto
        // di una cultura: sotto InvariantGlobalization le culture non esistono, quindi il
        // separatore va deciso e scritto a mano, qui con una sostituzione dopo la formattazione
        // invariante.
        => numero.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');

    private static string FormattaData(object valore)
    {
        if (valore is DateTime data) return data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        var testo = valore.ToString() ?? "";
        return DateTime.TryParseExact(testo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsata)
            ? parsata.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            : testo;
    }

    /// <summary>
    /// Il valore da salvare nel jsonb, ricavato da quello che l'utente ha digitato. Null significa
    /// "non scrivere questa chiave".
    /// <para>
    /// <c>number</c>: accetta sia il punto sia la virgola come separatore decimale. <c>date</c>:
    /// accetta <c>yyyy-MM-dd</c> o <c>dd/MM/yyyy</c> in ingresso e restituisce sempre una
    /// <see cref="string"/> in formato <c>yyyy-MM-dd</c>, mai un <see cref="DateTime"/> — una
    /// stringa ISO senza orario resta stringa al ritorno dal database, mentre una con orario
    /// verrebbe riconvertita in <see cref="DateTime"/> da Newtonsoft: un tipo che cambia a seconda
    /// del contenuto è una fonte di bug permanente. <c>bool</c>: <c>"true"</c>/<c>"false"</c>, senza
    /// distinzione di maiuscole. <c>url</c>: la stringa, o null se lo schema non è ammesso — lo
    /// schema si giudica con <see cref="MarkdownRenderer.UrlAmmessa"/>, non con una riscrittura.
    /// </para>
    /// </summary>
    public static object? DaTesto(string? input, string tipo)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var pulito = input.Trim();

        return tipo switch
        {
            "number" => DaTestoNumero(pulito),
            "date" => DaTestoData(pulito),
            "bool" => DaTestoBool(pulito),
            "url" => MarkdownRenderer.UrlAmmessa(pulito) ? pulito : null,
            _ => pulito,
        };
    }

    private static object? DaTestoNumero(string testo)
    {
        // Accettare la virgola non è una gentilezza: è quello che digita chi scrive in italiano.
        // Rifiutarla renderebbe inutilizzabile un campo come "prezzo".
        var normalizzato = testo.Replace(',', '.');
        if (!double.TryParse(normalizzato, NumberStyles.Float, CultureInfo.InvariantCulture, out var numero))
            return null;

        // NumberStyles.Float accetta "Infinity", "-Infinity" e "NaN", e un letterale come "1e400"
        // supera il range di double e diventa infinito: TryParse lo dichiara comunque riuscito.
        // Newtonsoft serializza un infinito come la stringa "Infinity", cambiando di nascosto il
        // tipo JSON da numero a stringa dentro collection_items.data.
        return double.IsFinite(numero) ? numero : null;
    }

    private static object? DaTestoData(string testo)
        // Si salva sempre una data secca come stringa, mai un DateTime: una stringa ISO senza
        // orario torna stringa dal database, mentre una con orario verrebbe riconvertita in
        // DateTime da Newtonsoft in lettura — un tipo che cambia a seconda del contenuto è una
        // fonte di bug permanente. È un comportamento misurato sul pacchetto reale.
        => DateTime.TryParseExact(testo, FormatiData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
            ? data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;

    private static object? DaTestoBool(string testo)
    {
        if (string.Equals(testo, "true", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(testo, "false", StringComparison.OrdinalIgnoreCase)) return false;
        return null;
    }

    /// <summary>Il valore così come va messo dentro un campo di input, cioè nella forma che
    /// <see cref="DaTesto"/> sa rileggere senza perdite.
    /// <para>
    /// Serve perché <see cref="Testo"/> e <see cref="DaTesto"/> non sono inverse per <c>bool</c>:
    /// <c>Testo(true, "bool")</c> restituisce "Sì", ma <c>DaTesto("Sì", "bool")</c> restituisce
    /// null, perché accetta solo "true"/"false" — il testo a schermo è in italiano, il valore
    /// salvato deve restare indipendente dalla lingua. Un editor che caricasse i valori con
    /// <see cref="Testo"/> e li risalvasse con <see cref="DaTesto"/> cancellerebbe ogni casella
    /// Sì/No al primo salvataggio, in silenzio.
    /// </para>
    /// </summary>
    public static string PerModifica(object? valore, string tipo)
    {
        if (valore is null) return "";

        return tipo switch
        {
            // "Sì"/"No" (Testo) non sono ciò che DaTesto sa rileggere: qui serve la forma
            // indipendente dalla lingua che DaTesto("true"/"false", ...) accetta.
            "bool" => valore is bool booleano ? (booleano ? "true" : "false") : "",

            // Il jsonb contiene già una stringa yyyy-MM-dd (la scrive DaTesto), ma un DateTime può
            // arrivare da dati scritti a mano: va normalizzato esplicitamente, altrimenti
            // ToString() userebbe un formato che DaTesto non riconosce.
            "date" => valore is DateTime data ? data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : valore.ToString() ?? "",

            // number: Testo produce la virgola decimale, e DaTesto la accetta. text, url, select:
            // passano invariati, ed è esattamente ciò che serve per rimetterli in un campo di input.
            _ => Testo(valore, tipo),
        };
    }
}
