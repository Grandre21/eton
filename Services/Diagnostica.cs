namespace Eton.Services;

/// <summary>
/// Registra in console un errore già gestito: la pagina ha già il suo messaggio per l'utente,
/// oppure il codice degrada da sé. Scrive su stdout e non su stderr di proposito: in Blazor
/// WebAssembly stderr accende la barra «Si è verificato un errore imprevisto» (#blazor-error-ui),
/// che deve restare alle eccezioni davvero non gestite.
/// </summary>
public static class Diagnostica
{
    public static void ErroreGestito(string messaggio) => Console.WriteLine(messaggio);

    /// <summary>
    /// Per il guasto che lo schermo non racconta in nessun altro modo: scrive su stderr, e quindi
    /// accende di proposito la barra «errore imprevisto», che qui è l'unico segnale per l'utente.
    /// Si usa solo dove la pagina non ha un messaggio suo; dove ce l'ha, vale <see cref="ErroreGestito"/>.
    /// </summary>
    public static void ErroreNonSegnalato(string messaggio) => Console.Error.WriteLine(messaggio);
}
