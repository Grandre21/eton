using System.Globalization;

namespace Eton.Services;

/// <summary>Perché <see cref="Denaro.Verifica"/> ha rifiutato un importo — o se non l'ha rifiutato.
/// Non è un booleano perché ciascun motivo vuole un messaggio diverso: dire "non è un importo
/// valido" a chi ha scritto "0" è falso, il formato è corretto, è il valore a non andare bene.
/// </summary>
public enum EsitoImporto
{
    /// <summary>Si legge, rispetta il formato, è positivo e ci sta in <c>numeric(12,2)</c>:
    /// il parametro <c>out</c> del metodo che lo restituisce contiene il valore letto.</summary>
    Valido,
    /// <summary>Nullo, vuoto o soli spazi: un campo non ancora compilato, non un errore di
    /// battitura. Non si mostra nessun messaggio finché non c'è niente da leggere.</summary>
    Vuoto,
    /// <summary>Non si legge come numero, oppure ha più di un separatore ("1.234,50",
    /// "12,34,56"): più di un separatore è un probabile errore di battitura, non le migliaia.</summary>
    NonNumerico,
    /// <summary>Più di due cifre dopo il separatore: la colonna è <c>numeric(12,2)</c>, un terzo
    /// decimale verrebbe arrotondato in silenzio dal database.</summary>
    TroppiDecimali,
    /// <summary>Si legge come numero ma vale zero o meno: il vincolo del database è
    /// <c>amount &gt; 0</c>.</summary>
    NonPositivo,
    /// <summary>Supera ciò che <c>numeric(12,2)</c> può contenere.</summary>
    TroppoGrande
}

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
    /// controllo su <see cref="EsitoImporto.NonPositivo"/> qui sotto: così un importo negativo
    /// viene fermato dalla stessa regola del database invece che da un fallimento di parsing, e il
    /// motivo del rifiuto sta in un posto solo. Restituisce <see cref="EsitoImporto.NonPositivo"/>
    /// per zero e i negativi (il vincolo del database è <c>amount &gt; 0</c>: farli passare qui
    /// vorrebbe dire un errore di rete al posto di un messaggio comprensibile), <see
    /// cref="EsitoImporto.TroppiDecimali"/> per più di due decimali (la colonna è
    /// <c>numeric(12,2)</c>: un terzo decimale verrebbe arrotondato in silenzio dal database, e
    /// l'utente vedrebbe un numero diverso da quello che ha scritto), e <see
    /// cref="EsitoImporto.TroppoGrande"/> per ciò che eccede la capienza del campo.
    /// <para>
    /// In ogni caso diverso da <see cref="EsitoImporto.Valido"/>, <paramref name="importo"/> vale
    /// <c>0</c>: chi chiama può usare l'<c>out</c> senza guardare l'esito, e un residuo diverso da
    /// zero finirebbe scritto nel database come se fosse un importo valido.
    /// </para>
    /// </summary>
    public static EsitoImporto Verifica(string? testo, out decimal importo)
    {
        importo = 0m;

        if (string.IsNullOrWhiteSpace(testo)) return EsitoImporto.Vuoto;

        var pulito = testo.Trim();

        // Più di un separatore — virgola o punto, in qualunque combinazione — non è un separatore
        // delle migliaia da interpretare: nessuno lo digita in un campo importo. Non stiamo
        // scrivendo un parser delle migliaia in ingresso, e accettarne uno significherebbe
        // indovinare l'intenzione di chi scrive. "1.234,50" e "12,34,56" si fermano qui, come
        // NonNumerico: non sono un numero grande scritto con le migliaia, sono una stringa che il
        // parsing non legge affatto.
        if (pulito.Count(c => c is ',' or '.') > 1) return EsitoImporto.NonNumerico;

        var normalizzato = pulito.Replace(',', '.');

        var puntoIndex = normalizzato.IndexOf('.');
        if (puntoIndex >= 0 && normalizzato.Length - puntoIndex - 1 > 2) return EsitoImporto.TroppiDecimali;

        if (!decimal.TryParse(
                normalizzato,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var valore))
            return EsitoImporto.NonNumerico;

        if (valore <= 0) return EsitoImporto.NonPositivo;
        if (valore >= LimiteSuperiore) return EsitoImporto.TroppoGrande;

        importo = valore;
        return EsitoImporto.Valido;
    }

    /// <summary>Come <see cref="Verifica"/>, ma per chi ha bisogno solo di sapere se l'importo è
    /// utilizzabile e non del motivo del rifiuto.</summary>
    public static bool Prova(string? testo, out decimal importo)
        => Verifica(testo, out importo) == EsitoImporto.Valido;

    /// <summary>Da <c>1284.50m</c> a «1.284,50»: sempre due decimali, punto come separatore delle
    /// migliaia, virgola come separatore decimale.
    /// Serve solo per la visualizzazione; in un campo modificabile si usa <see
    /// cref="TestoDigitabile"/>, perché <see cref="Prova"/> rifiuta il punto delle migliaia.</summary>
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

    /// <summary>Da <c>1284.50m</c> a «1284,50»: sempre due decimali, virgola come separatore
    /// decimale, nessun separatore delle migliaia. Riempie un campo modificabile: ciò che ne esce
    /// è rileggibile da <see cref="Prova"/>, l'uscita di <see cref="Testo"/> sopra il migliaio no.</summary>
    public static string TestoDigitabile(decimal importo)
        // Si usa "F2" (fixed-point) e non "N2", che inserisce i gruppi delle migliaia — per questo
        // Testo non va bene in un campo modificabile. InvariantCulture più sostituzione esplicita,
        // come il resto della classe: con InvariantGlobalization attivo non c'è altra cultura da usare.
        => importo.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
}
