using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Eton.Services;

/// <summary>
/// La coppia PKCE (RFC 7636): un <b>verificatore</b> casuale che non lascia mai questo dispositivo,
/// e la sua <b>impronta</b> SHA-256, che è l'unica cosa che viaggia verso il provider.
/// Al ritorno si presenta il verificatore: solo chi ha avviato l'accesso ce l'ha, quindi un codice
/// di autorizzazione intercettato da qualcun altro non è spendibile.
/// <para>
/// Esiste perché <c>Supabase.Gotrue</c> 6.3.0 non è utilizzabile per questo passo: il suo
/// <c>SignIn(Provider, …)</c> accoda all'URL di <c>/authorize</c> un parametro <c>state</c> di
/// propria invenzione, che il server inoltra a Google al posto del proprio e poi non riconosce più
/// al ritorno (<c>OAuth state parameter is invalid</c>). Non è disattivabile da nessuna opzione.
/// </para>
/// </summary>
public static class PkceChallenge
{
    /// <summary>Verificatore casuale: 32 byte da CSPRNG, cioè 43 caratteri in base64url — il
    /// minimo che la RFC 7636 ammette, e più che sufficiente.</summary>
    public static string GeneraVerificatore()
        => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));

    /// <summary>Impronta del verificatore: <c>BASE64URL(SHA256(ASCII(verificatore)))</c>.
    /// Senza riempimento e con l'alfabeto sicuro per gli URL, come impone la RFC: il valore finisce
    /// in una query string, dove <c>+</c> <c>/</c> <c>=</c> verrebbero riscritti.</summary>
    public static string Impronta(string verificatore)
        => Base64Url.EncodeToString(SHA256.HashData(Encoding.ASCII.GetBytes(verificatore)));
}
