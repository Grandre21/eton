using System.Globalization;

namespace Eton.Services;

/// <summary>
/// Conversione fra un importo digitato da una persona e il <c>decimal</c> che finisce nella colonna
/// <c>numeric(12,2)</c> di <c>expenses.amount</c>, e viceversa. Stesso approccio di
/// <see cref="ValoriElemento"/>: con <c>InvariantGlobalization</c> attivo <c>new CultureInfo("it-IT")</c>
/// lancia a runtime, non esiste nessuna cultura da impostare, quindi la virgola decimale si legge e
/// si scrive a mano con <see cref="CultureInfo.InvariantCulture"/> più sostituzioni esplicite.
/// </summary>
public static class Denaro
{
    // numeric(12,2): 12 cifre di precisione totale, 2 di scala, quindi al massimo 10 cifre intere
    // (l'ultimo valore rappresentabile è 9.999.999.999,99). 10 elevato alla 10 è la prima soglia
    // che il campo non può più contenere.
    private const decimal LimiteSuperiore = 10_000_000_000m;

    /// <summary>
    /// Legge un importo digitato a mano in un campo di testo. Accetta sia «12,50» sia «12.50»: chi
    /// digita su un tastierino numerico ottiene il punto, chi digita su una tastiera italiana la
    /// virgola, e rifiutare uno dei due significherebbe rifiutare metà degli inserimenti. Accetta
    /// anche un «+» iniziale («+12,50»): è una battuta a vuoto sulla tastiera, non un'ambiguità sul
    /// segno, e il valore che ne esce è comunque 12,50. Lo stesso <see
    /// cref="NumberStyles.AllowLeadingSign"/> lascia passare anche il «-» iniziale, ma solo fino al
    /// controllo sui negativi qui sotto: così un importo negativo viene fermato dalla stessa regola
    /// del database invece che da un fallimento di parsing, e il motivo del rifiuto sta in un posto
    /// solo. Rifiuta zero e i negativi (il vincolo del database è <c>amount &gt; 0</c>: farli
    /// passare qui vorrebbe dire un errore di rete al posto di un messaggio comprensibile), più di
    /// due decimali (la colonna è <c>numeric(12,2)</c>: un terzo decimale verrebbe arrotondato in
    /// silenzio dal database, e l'utente vedrebbe un numero diverso da quello che ha scritto), e
    /// ciò che eccede la capienza del campo.
    /// </summary>
    public static bool Prova(string? testo, out decimal importo)
    {
        importo = 0m;

        if (string.IsNullOrWhiteSpace(testo)) return false;

        var pulito = testo.Trim();

        // Più di un separatore — virgola o punto, in qualunque combinazione — non è un separatore
        // delle migliaia da interpretare: nessuno lo digita in un campo importo. Non stiamo
        // scrivendo un parser delle migliaia in ingresso, e accettarne uno significherebbe
        // indovinare l'intenzione di chi scrive. "1.234,50" e "12,34,56" si fermano qui.
        if (pulito.Count(c => c is ',' or '.') > 1) return false;

        var normalizzato = pulito.Replace(',', '.');

        var puntoIndex = normalizzato.IndexOf('.');
        if (puntoIndex >= 0 && normalizzato.Length - puntoIndex - 1 > 2) return false;

        if (!decimal.TryParse(
                normalizzato,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var valore))
            return false;

        if (valore <= 0) return false;
        if (valore >= LimiteSuperiore) return false;

        importo = valore;
        return true;
    }

    /// <summary>Da <c>1284.50m</c> a «1.284,50»: sempre due decimali, punto come separatore delle
    /// migliaia, virgola come separatore decimale.</summary>
    public static string Testo(decimal importo)
    {
        // Formattando con InvariantCulture si ottiene "1,284.50" (virgola alle migliaia, punto
        // decimale — l'inverso di quello che serve). Una sostituzione diretta ',' -> '.' seguita da
        // '.' -> ',' NON funziona: dopo il primo passaggio la stringa è "1.284.50", e il secondo
        // passaggio la trasforma in "1,284,50" invece di "1.284,50", perché ritrova esattamente i
        // punti che ha appena scritto lui stesso, comprese le migliaia. Serve un carattere
        // segnaposto che non compaia mai nell'output intermedio, cosicché i due scambi non si
        // pestino i piedi a vicenda.
        var invariante = importo.ToString("N2", CultureInfo.InvariantCulture);
        return invariante.Replace(',', '\0').Replace('.', ',').Replace('\0', '.');
    }
}
