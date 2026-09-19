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

    /// <summary>Lancia di proposito, a differenza di <see cref="Leggi"/> e <see cref="Cancella"/>:
    /// senza verificatore salvato l'accesso non può riuscire, non esiste un ripiego onesto, e l'unica
    /// cosa giusta è non partire affatto verso Google. Il solo chiamante — <c>Benvenuto.Accedi</c> —
    /// ha già il try/catch con la frase sull'archiviazione bloccata; un secondo chiamante futuro non
    /// erediterebbe quella copertura e dovrebbe procurarsene una propria.</summary>
    public void Salva(string verificatore)
        => _js.InvokeVoid("localStorage.setItem", StorageKey, verificatore);

    /// <summary>Non lancia: un'eccezione qui risalirebbe dal bootstrap alla barra d'errore generica
    /// di Blazor, che è lo stesso vincolo dichiarato da <see cref="BrowserSessionHandler.LoadSession"/>.
    /// Il ripiego <c>null</c> è onesto, perché <c>SupabaseService.ScambiaCodiceAsync</c> lo tratta già
    /// come "verificatore assente" e ha una frase pronta per chi legge.</summary>
    public string? Leggi()
    {
        try
        {
            var valore = _js.Invoke<string?>("localStorage.getItem", StorageKey);
            return string.IsNullOrEmpty(valore) ? null : valore;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] Verificatore PKCE illeggibile, procedo senza: {ex.Message}");
            return null;
        }
    }

    /// <summary>Best-effort, come il try annidato di <see cref="BrowserSessionHandler.LoadSession"/>:
    /// il verificatore è monouso e il server lo consuma comunque, quindi un <c>removeItem</c> fallito
    /// non compromette nulla. Si registra e si prosegue.</summary>
    public void Cancella()
    {
        try
        {
            _js.InvokeVoid("localStorage.removeItem", StorageKey);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] Cancellazione del verificatore PKCE non riuscita: {ex.Message}");
        }
    }
}
