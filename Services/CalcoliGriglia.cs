using System.Globalization;
using System.Text;

namespace Eton.Services;

/// <summary>
/// Calcoli puri della griglia: ordinamento, raggruppamento, aggregazioni, CSV e selezione per
/// intervallo. Nessun accesso a rete o a stato. Come <see cref="CalcoliSpese"/>.
/// </summary>
public static class CalcoliGriglia
{
    /// <summary>Ordina per il valore della colonna. LINQ <c>OrderBy</c> è stabile: a parità resta
    /// l'ordine di partenza, anche in decrescente. Il testo si confronta senza accenti né maiuscole:
    /// sotto InvariantGlobalization un confronto "culturale" è ordinale, e senza la riduzione
    /// "Èrba" finirebbe dopo "zucchero". Un valore null è il più piccolo.</summary>
    public static IReadOnlyList<T> Ordina<T>(IReadOnlyList<T> righe, ColonnaGriglia<T> colonna, bool decrescente)
    {
        var confronto = Comparer<object?>.Create((a, b) => Confronta(colonna.Tipo, a, b));

        return (decrescente
            ? righe.OrderByDescending(colonna.Valore, confronto)
            : righe.OrderBy(colonna.Valore, confronto)).ToList();
    }

    private static int Confronta(TipoColonna tipo, object? a, object? b)
    {
        if (a is null) return b is null ? 0 : -1;
        if (b is null) return 1;

        return tipo switch
        {
            TipoColonna.Testo or TipoColonna.Selezione => StringComparer.Ordinal.Compare(
                SchemaCampi.RiduciAccenti(a.ToString()!).ToUpperInvariant(),
                SchemaCampi.RiduciAccenti(b.ToString()!).ToUpperInvariant()),
            TipoColonna.Numero or TipoColonna.Denaro => Convert.ToDecimal(a, CultureInfo.InvariantCulture)
                .CompareTo(Convert.ToDecimal(b, CultureInfo.InvariantCulture)),
            _ => ((DateTime)a).CompareTo((DateTime)b)
        };
    }

    /// <summary>Raggruppa nell'ordine della prima comparsa della chiave nelle righe già ordinate.
    /// Chiave nulla: un solo gruppo senza etichetta, purché ci sia almeno una riga. Nessun gruppo
    /// vuoto, per costruzione.</summary>
    public static IReadOnlyList<Gruppo<T>> Raggruppa<T>(IReadOnlyList<T> righe, Func<T, string>? chiave)
    {
        if (chiave is null)
            return righe.Count > 0 ? [new Gruppo<T>(null, righe)] : [];

        return righe.GroupBy(chiave).Select(g => new Gruppo<T>(g.Key, g.ToList())).ToList();
    }

    /// <summary>Conteggio, somma, media, minimo e massimo. A zero righe la somma è 0 e il resto è
    /// null: una media di niente non è zero. Media arrotondata come <see cref="CalcoliSpese.PerMese"/>.</summary>
    public static Riepilogo Aggrega<T>(IReadOnlyList<T> righe, Func<T, decimal> valore)
    {
        if (righe.Count == 0)
            return new Riepilogo(0, 0m, null, null, null);

        var valori = righe.Select(valore).ToList();
        var somma = valori.Sum();
        var media = Math.Round(somma / valori.Count, 2, MidpointRounding.AwayFromZero);

        return new Riepilogo(valori.Count, somma, media, valori.Min(), valori.Max());
    }

    /// <summary>Il CSV che l'Excel italiano apre direttamente: BOM, punto e virgola, virgola decimale
    /// senza migliaia, data gg/mm/aaaa, CRLF a fine riga (anche l'ultima).</summary>
    public static string Csv<T>(IReadOnlyList<ColonnaGriglia<T>> colonne, IReadOnlyList<T> righe)
    {
        var csv = new StringBuilder("\uFEFF");

        csv.Append(string.Join(';', colonne.Select(c => Racchiudi(c.Etichetta)))).Append("\r\n");
        foreach (var riga in righe)
            csv.Append(string.Join(';', colonne.Select(c => TestoCsv(c.Tipo, c.Valore(riga))))).Append("\r\n");

        return csv.ToString();
    }

    // Un testo scritto da un altro membro che comincia con = + - @ è una formula, per Excel: l'apostrofo
    // la rende testo. Denaro, numero e data non si toccano, li formatta il codice. L'apostrofo va prima
    // delle virgolette, sta dentro. Denaro e numero escono come nel campo modificabile (valore esatto,
    // senza migliaia né arrotondamenti); la data come in cella.
    private static string TestoCsv(TipoColonna tipo, object? valore)
    {
        var testo = tipo is TipoColonna.Denaro or TipoColonna.Numero
            ? TestoModificabile(tipo, valore)
            : TestoCella(tipo, valore);

        if ((tipo is TipoColonna.Testo or TipoColonna.Selezione) && testo.Length > 0 && testo[0] is '=' or '+' or '-' or '@')
            testo = "'" + testo;

        return Racchiudi(testo);
    }

    private static string Racchiudi(string testo)
        => testo.IndexOfAny([';', '"', '\r', '\n']) >= 0 ? "\"" + testo.Replace("\"", "\"\"") + "\"" : testo;

    /// <summary>Le chiavi fra <paramref name="ancora"/> e <paramref name="fine"/>, estremi inclusi,
    /// nell'ordine dato (vale anche se l'ancora viene dopo). Se l'ancora non c'è più (filtrata via)
    /// resta la sola riga cliccata.</summary>
    public static IReadOnlyList<string> Intervallo(IReadOnlyList<string> chiaviInOrdine, string ancora, string fine)
    {
        var chiavi = chiaviInOrdine.ToList();
        var indiceFine = chiavi.IndexOf(fine);
        if (indiceFine < 0)
            return [];

        var indiceAncora = chiavi.IndexOf(ancora);
        if (indiceAncora < 0)
            return [fine];

        var da = Math.Min(indiceAncora, indiceFine);
        return chiavi.GetRange(da, Math.Abs(indiceAncora - indiceFine) + 1);
    }

    /// <summary>Il testo mostrato in cella. Il denaro ha il punto delle migliaia; il numero ha la resa
    /// delle collezioni (<c>ValoriElemento.Testo</c>, al più due decimali).</summary>
    public static string TestoCella(TipoColonna tipo, object? valore)
    {
        if (valore is null)
            return "";

        return tipo switch
        {
            TipoColonna.Denaro => Denaro.Testo(Convert.ToDecimal(valore, CultureInfo.InvariantCulture)),
            TipoColonna.Data => Testi.DataSola((DateTime)valore),
            TipoColonna.Numero => ValoriElemento.Testo(valore, "number"),
            _ => valore.ToString() ?? ""
        };
    }

    /// <summary>Il testo da mettere in un campo modificabile. <c>Denaro.Testo</c> mette il punto delle
    /// migliaia, che <c>Denaro.Verifica</c> rifiuta in ingresso: qui va <c>TestoDigitabile</c> (v. il
    /// commento in Services/Denaro.cs). La data è nel formato di un <c>&lt;input type="date"&gt;</c>.
    /// Il numero è esatto, senza arrotondare: ciò che il campo contiene si rilegge e si salva.</summary>
    public static string TestoModificabile(TipoColonna tipo, object? valore)
    {
        if (valore is null)
            return "";

        return tipo switch
        {
            TipoColonna.Denaro => Denaro.TestoDigitabile(Convert.ToDecimal(valore, CultureInfo.InvariantCulture)),
            TipoColonna.Data => ((DateTime)valore).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TipoColonna.Numero => Convert.ToString(valore, CultureInfo.InvariantCulture)!.Replace('.', ','),
            _ => TestoCella(tipo, valore)
        };
    }
}
