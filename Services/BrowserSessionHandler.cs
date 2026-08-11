using Microsoft.JSInterop;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace Eton.Services;

/// <summary>
/// Persistenza della sessione Gotrue su localStorage. L'interfaccia
/// <see cref="IGotrueSessionPersistence{T}"/> è SINCRONA, quindi serve
/// <see cref="IJSInProcessRuntime"/> (Invoke sincrono) e non <see cref="IJSRuntime"/>.
/// È anche il motivo per cui questo file non funzionerebbe in un'app MAUI Blazor Hybrid, dove
/// il JS interop è solo asincrono: v. §2.1 della spec.
/// La <see cref="Session"/> si serializza con Newtonsoft (i suoi attributi sono
/// <c>[JsonProperty]</c>), coerentemente con la libreria.
/// </summary>
public class BrowserSessionHandler : IGotrueSessionPersistence<Session>
{
    private const string StorageKey = "eton.session";
    private readonly IJSInProcessRuntime _js;

    public BrowserSessionHandler(IJSInProcessRuntime js) => _js = js;

    public void SaveSession(Session session)
        => _js.InvokeVoid("localStorage.setItem", StorageKey, JsonConvert.SerializeObject(session));

    public void DestroySession()
        => _js.InvokeVoid("localStorage.removeItem", StorageKey);

    /// <summary>
    /// Non lancia MAI. È un vincolo, non una cautela: <c>Client.LoadSession()</c> non ha alcun
    /// try/catch attorno a questa chiamata, e il bootstrap dell'app nemmeno — un'eccezione qui
    /// risalirebbe fino al riquadro d'errore di Blazor, e il pulsante "Ricarica" rileggerebbe lo
    /// stesso dato corrotto lanciando di nuovo. L'utente resterebbe chiuso fuori per sempre, senza
    /// altra via d'uscita che cancellare a mano i dati del sito.
    /// Un valore illeggibile viene quindi buttato: una sessione che non si sa interpretare non vale
    /// niente comunque, e un utente che rifà l'accesso è infinitamente meglio di un'app murata.
    /// </summary>
    public Session? LoadSession()
    {
        try
        {
            var json = _js.Invoke<string?>("localStorage.getItem", StorageKey);
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<Session>(json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] Sessione persistita illeggibile, la scarto: {ex.Message}");
            try { DestroySession(); } catch { /* se anche questo fallisce, localStorage è inservibile: si prosegue senza */ }
            return null;
        }
    }
}
