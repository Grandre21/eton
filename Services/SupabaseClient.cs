using Supabase.Gotrue;
using Postgrest = Supabase.Postgrest;

namespace Eton.Services;

/// <summary>
/// Facade unica su autenticazione e dati, così i repository dipendono da un solo tipo e non
/// dai due client separati.
/// <para>
/// <b>Questa istanza non va conservata fra un'operazione e la successiva.</b> Si ottiene da
/// <see cref="SupabaseService.GetClientAsync"/>, ed è quella chiamata — non questa classe — a
/// garantire un token vivo: <c>GetHeaders</c> di Postgrest è sincrono e non può rinfrescare
/// nulla, quindi si limita a leggere la sessione com'è in quel momento. Un componente che
/// prendesse il client una volta in <c>OnInitializedAsync</c> e lo riusasse per ore vedrebbe il
/// rinnovo proattivo non scattare mai, e alla scadenza le richieste ripiegherebbero sull'anon key.
/// Non è un buco di sicurezza — la RLS le rifiuta comunque, e ad <c>anon</c> i privilegi sono
/// revocati — ma è un degrado silenzioso: i dati smettono di caricarsi senza che nulla dica il
/// perché. <b>Regola per i repository: <c>await GetClientAsync()</c> all'inizio di ogni operazione,
/// mai un campo di istanza.</b>
/// </para>
/// </summary>
public sealed class SupabaseClient
{
    private readonly Postgrest.Client _postgrest;

    public SupabaseClient(Client auth, Postgrest.Client postgrest)
    {
        Auth = auth;
        _postgrest = postgrest;
    }

    /// <summary>Client Gotrue: CurrentSession, SignIn, SignOut, ExchangeCodeForSession, …</summary>
    public Client Auth { get; }

    /// <summary>Tabella tipizzata. <c>Client.Table&lt;T&gt;()</c> dichiara il ritorno come
    /// <c>IPostgrestTable&lt;T&gt;</c>, ma l'istanza concreta è sempre <c>Table&lt;T&gt;</c>:
    /// il cast tiene la superficie pubblica comoda per i repository.</summary>
    public Postgrest.Table<T> From<T>() where T : Postgrest.Models.BaseModel, new()
        => (Postgrest.Table<T>)_postgrest.Table<T>();

    /// <summary>Chiamata a una funzione del database (create_space, join_space, …).</summary>
    public Task<T?> Rpc<T>(string nomeFunzione, Dictionary<string, object> parametri)
        => _postgrest.Rpc<T>(nomeFunzione, parametri);
}
