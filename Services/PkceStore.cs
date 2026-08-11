using Microsoft.JSInterop;

namespace Eton.Services;

/// <summary>
/// Custodisce il verificatore PKCE fra la partenza verso Google e il ritorno.
/// Serve perché nel mezzo il browser lascia l'applicazione: quando torna, la pagina è stata
/// ricaricata da zero e nulla che stesse in memoria è sopravvissuto. Il verificatore è l'unica
/// prova che chi presenta il codice è lo stesso che l'ha richiesto: senza, il codice intercettato
/// da qualcun altro sarebbe spendibile.
/// Si cancella subito dopo l'uso — è monouso per definizione.
/// </summary>
public class PkceStore
{
    private const string StorageKey = "eton.pkce";
    private readonly IJSInProcessRuntime _js;

    public PkceStore(IJSInProcessRuntime js) => _js = js;

    public void Salva(string verificatore)
        => _js.InvokeVoid("localStorage.setItem", StorageKey, verificatore);

    public string? Leggi()
    {
        var valore = _js.Invoke<string?>("localStorage.getItem", StorageKey);
        return string.IsNullOrEmpty(valore) ? null : valore;
    }

    public void Cancella() => _js.InvokeVoid("localStorage.removeItem", StorageKey);
}
