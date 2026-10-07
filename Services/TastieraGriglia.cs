namespace Eton.Services;

/// <summary>Che cosa vuole fare l'utente premendo un tasto su una cella della griglia.</summary>
public enum AzioneTasto
{
    Nessuna, Su, Giu, Sinistra, Destra, Modifica, ModificaSostituendo,
    SelezionaRiga, SalvaEGiu, SalvaEDestra, SalvaESinistra, Annulla
}

// Il contratto di modifica di una cella di Shared/Griglia.razor. Stanno qui e non nel .razor perché un
// .razor non può dichiarare tipi fuori dalla sua classe, e annidati in Griglia<T> si chiamerebbero
// Griglia<RigaSpesa>.ModificaCella dalla pagina.

/// <summary>Ciò che la griglia consegna alla pagina quando una cella esce dalla modifica: la riga,
/// la colonna e il testo digitato.</summary>
public sealed record ModificaCella<T>(T Riga, string Colonna, string Testo);

/// <summary>Come la pagina ha trattato una modifica. <c>Salvata</c> esce dalla modifica, e la riga
/// riallineata arriva dalla pagina col prossimo <c>Gruppi</c>. <c>NonValida</c> tiene la cella in
/// modifica col messaggio. <c>Chiusa</c>: la modifica è finita e l'esito lo gestisce la pagina
/// (rifiutata, conflitto, sparita): la griglia esce dalla modifica e mostra il messaggio accanto alla riga.</summary>
public enum EsitoCella { Salvata, NonValida, Chiusa }

/// <summary>La risposta della pagina a una <see cref="ModificaCella{T}"/>: l'esito e, se serve, il
/// messaggio da mostrare.</summary>
public sealed record RispostaCella(EsitoCella Esito, string? Messaggio);

/// <summary>
/// La tastiera della griglia come in Excel, pura: traduce un tasto in un'azione, sposta la cella
/// attiva e la ritrova dopo un ridisegno. Nessun accesso a stato o a rete.
/// </summary>
public static class TastieraGriglia
{
    /// <summary>Traduce <c>KeyboardEvent.key</c> in un'azione, a seconda che la cella sia in modifica.
    /// Fuori modifica un carattere singolo entra sostituendo, ma non su data e menù, dove "sostituire
    /// con un carattere" non ha significato (decisione dell'utente del 7 ottobre). <c>ctrlAltMeta</c> lo
    /// calcola il chiamante come «Meta, oppure Ctrl o Alt da soli»: con un modificatore frecce, Invio e F2
    /// restano al browser (Alt+Freccia e Cmd+Freccia sono indietro e avanti). AltGr, che i browser su
    /// Windows riportano come Ctrl+Alt insieme, non è un modificatore: produce caratteri (@, #, €, [ ]
    /// sulla tastiera italiana).</summary>
    public static AzioneTasto Interpreta(string tasto, bool shift, bool ctrlAltMeta, bool inModifica, TipoColonna tipo)
    {
        if (inModifica)
            return tasto switch
            {
                "Enter" => AzioneTasto.SalvaEGiu,
                "Tab" => shift ? AzioneTasto.SalvaESinistra : AzioneTasto.SalvaEDestra,
                "Escape" => AzioneTasto.Annulla,
                _ => AzioneTasto.Nessuna
            };

        return tasto switch
        {
            "ArrowUp" when !ctrlAltMeta => AzioneTasto.Su,
            "ArrowDown" when !ctrlAltMeta => AzioneTasto.Giu,
            "ArrowLeft" when !ctrlAltMeta => AzioneTasto.Sinistra,
            "ArrowRight" when !ctrlAltMeta => AzioneTasto.Destra,
            "Enter" or "F2" when !ctrlAltMeta => AzioneTasto.Modifica,
            " " => AzioneTasto.SelezionaRiga,
            { Length: 1 } when !ctrlAltMeta => tipo is TipoColonna.Data or TipoColonna.Selezione
                ? AzioneTasto.Modifica
                : AzioneTasto.ModificaSostituendo,
            _ => AzioneTasto.Nessuna
        };
    }

    /// <summary>La cella raggiunta con l'azione, ferma ai bordi della griglia (non va a capo). Le azioni
    /// che non spostano, e una griglia senza righe o colonne, lasciano la cella dov'è.</summary>
    public static (int Riga, int Colonna) Sposta((int Riga, int Colonna) da, AzioneTasto azione, int righe, int colonne)
    {
        if (righe == 0 || colonne == 0) return da;

        var (riga, colonna) = azione switch
        {
            AzioneTasto.Su => (da.Riga - 1, da.Colonna),
            AzioneTasto.Giu or AzioneTasto.SalvaEGiu => (da.Riga + 1, da.Colonna),
            AzioneTasto.Sinistra or AzioneTasto.SalvaESinistra => (da.Riga, da.Colonna - 1),
            AzioneTasto.Destra or AzioneTasto.SalvaEDestra => (da.Riga, da.Colonna + 1),
            _ => da
        };

        return (Math.Clamp(riga, 0, righe - 1), Math.Clamp(colonna, 0, colonne - 1));
    }

    /// <summary>Dopo un ridisegno ritrova la riga attiva per chiave. Se è sparita (filtrata o eliminata)
    /// è la chiave che ora occupa lo stesso posto, o la nuova ultima. Senza righe, nessuna.</summary>
    public static string? Riallinea(IReadOnlyList<string> chiavi, string? attiva, int indicePrecedente)
    {
        if (chiavi.Count == 0) return null;
        if (attiva is not null && chiavi.Contains(attiva)) return attiva;

        return chiavi[Math.Clamp(indicePrecedente, 0, chiavi.Count - 1)];
    }
}
