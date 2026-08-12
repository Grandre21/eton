using Microsoft.JSInterop;

namespace Eton.Services;

/// <summary>
/// Custodisce la rotta privata richiesta da chi arriva senza sessione, fra il rimbalzo verso la
/// vetrina e il ritorno dopo l'accesso.
/// Si usa <c>sessionStorage</c> e non <c>localStorage</c>: una rotta in attesa vale per il giro di
/// accesso in corso, non per sempre — con <c>localStorage</c> una destinazione salvata e mai
/// raggiunta (l'utente chiude senza accedere) riemergerebbe settimane dopo, dirottando un accesso
/// che non c'entra niente. <c>sessionStorage</c> sopravvive al salto verso Google e al ritorno
/// perché resta la stessa scheda, e muore quando la scheda si chiude.
/// </summary>
public class RottaRichiesta
{
    private const string StorageKey = "eton.rotta-richiesta";
    private readonly IJSInProcessRuntime _js;

    public RottaRichiesta(IJSInProcessRuntime js) => _js = js;

    public void Salva(string rotta)
        => _js.InvokeVoid("sessionStorage.setItem", StorageKey, rotta);

    public string? Consuma()
    {
        var valore = _js.Invoke<string?>("sessionStorage.getItem", StorageKey);
        _js.InvokeVoid("sessionStorage.removeItem", StorageKey);
        return EInterna(valore) ? valore : null;
    }

    /// <summary>
    /// Solo rotte interne, e non è una formalità: questo valore finisce dritto in
    /// <c>NavigateTo</c>, che con un indirizzo assoluto porta fuori dal sito. Chi riuscisse a
    /// scrivere in <c>sessionStorage</c> otterrebbe così un rimbalzo verso un dominio qualunque
    /// subito dopo l'accesso — una pagina di accesso finta ha buon gioco proprio lì, dove l'utente
    /// si aspetta di essere rimandato da qualche parte.
    /// <para>
    /// Chi può scrivere in <c>sessionStorage</c> sta già eseguendo codice su questo dominio, quindi
    /// non è la difesa che regge l'applicazione: è la stessa cautela di
    /// <see cref="MarkdownRenderer"/> sugli URL scritti da altri, e costa una riga.
    /// </para>
    /// <c>ToBaseRelativePath</c> restituisce rotte senza barra iniziale, quindi tutto ciò che
    /// comincia per <c>/</c> o contiene <c>:</c> non è mai stato prodotto da noi.
    /// </summary>
    private static bool EInterna(string? rotta)
        => !string.IsNullOrEmpty(rotta)
           && !rotta.StartsWith('/')
           && !rotta.Contains(':', StringComparison.Ordinal);
}
