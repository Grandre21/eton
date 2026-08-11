using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Supabase.Gotrue;
using Postgrest = Supabase.Postgrest;

namespace Eton.Services;

/// <summary>
/// Provider di sessione e client Supabase: costruisce un <see cref="Client"/> Gotrue (auth) e un
/// <see cref="Postgrest.Client"/> (dati), e li espone dietro <see cref="SupabaseClient"/>.
///
/// Flusso di accesso: <b>PKCE</b>, non implicit. Google riporta un codice monouso in
/// <c>?code=</c>; lo si scambia con la sessione presentando il verificatore custodito da
/// <see cref="PkceStore"/>. Col flusso implicit l'access token arriverebbe nel fragment dell'URL,
/// cioè nella cronologia del browser e in ogni log che registri gli URL.
/// </summary>
public class SupabaseService
{
    private readonly Client _auth;
    private readonly Postgrest.Client _postgrest;
    private readonly SupabaseClient _facade;
    private readonly NavigationManager _navigation;
    private readonly BrowserSessionHandler _sessionHandler;
    private readonly PkceStore _pkce;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;
    private DateTime? _ultimoRefreshFallito;

    /// <summary>Messaggio dell'ultimo rifiuto del provider, letto da <c>Login.razor</c>.</summary>
    public string? ErroreAccesso { get; private set; }

    public SupabaseService(IConfiguration configuration, IJSRuntime js, NavigationManager navigation)
    {
        _navigation = navigation;

        var url = configuration["Supabase:Url"];
        var anonKey = configuration["Supabase:AnonKey"];

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(anonKey))
            throw new InvalidOperationException(
                "Supabase:Url e Supabase:AnonKey vanno valorizzati in wwwroot/appsettings.json.");

        _auth = new Client(new ClientOptions
        {
            Url = $"{url}/auth/v1",
            Headers = new Dictionary<string, string> { { "apikey", anonKey } },
        });

        _sessionHandler = new BrowserSessionHandler((IJSInProcessRuntime)js);
        _auth.SetPersistence(_sessionHandler);
        _pkce = new PkceStore((IJSInProcessRuntime)js);

        // Il token entra per-richiesta: così la RLS vede sempre quello valido. Si manda il Bearer
        // dell'utente SOLO se la sessione esiste e non è scaduta — un token scaduto verrebbe
        // rifiutato dal gateway (403 bad_jwt), mentre con l'anon key la richiesta arriva e viene
        // valutata dalle policy.
        _postgrest = new Postgrest.Client($"{url}/rest/v1", new Postgrest.ClientOptions())
        {
            GetHeaders = () =>
            {
                var session = _auth.CurrentSession;
                var scaduta = session is null
                    || DateTime.UtcNow >= SessionFreshness.ScadenzaUtc(session.CreatedAt, session.ExpiresIn);
                var bearer = scaduta ? anonKey : session!.AccessToken;
                return new Dictionary<string, string>
                {
                    { "apikey", anonKey },
                    { "Authorization", $"Bearer {bearer}" },
                };
            },
        };

        _facade = new SupabaseClient(_auth, _postgrest);
    }

    /// <summary>
    /// Bootstrap idempotente e serializzato: ripristino da localStorage, scambio del codice PKCE
    /// se siamo appena tornati da Google, refresh se la sessione sta per scadere.
    /// Ogni chiamata dati passa di qui, quindi è anche il punto in cui si garantisce un token vivo:
    /// <c>GetHeaders</c> è sincrono e non potrebbe rinfrescare nulla.
    /// </summary>
    public async Task<SupabaseClient> GetClientAsync()
    {
        if (_initialized)
        {
            var corrente = _auth.CurrentSession;
            if (corrente is not null && SessionFreshness.VaRinfrescata(
                    SessionFreshness.ScadenzaUtc(corrente.CreatedAt, corrente.ExpiresIn), DateTime.UtcNow))
                await RinnovaSessioneSeServeAsync();
            return _facade;
        }

        await _initLock.WaitAsync();
        try
        {
            if (!_initialized)
            {
                // 1) Sessione persistita (sincrono, nessuna rete) → si resta loggati al reload.
                _auth.LoadSession();

                // 2) Ritorno da Google?
                var esito = OAuthCallback.Analizza(_navigation.Uri);

                if (esito.Errore is not null)
                {
                    ErroreAccesso = esito.Errore;
                    _pkce.Cancella();
                }
                else if (esito.Codice is not null)
                {
                    await ScambiaCodiceAsync(esito.Codice);
                }
                else
                {
                    // Nessun ritorno OAuth: sessione ripristinata ma forse da rinfrescare.
                    // NIENTE in questo ramo può propagare un'eccezione, altrimenti l'app resta
                    // bloccata sul caricamento. Versione SENZA lock: siamo già dentro _initLock,
                    // che non è rientrante.
                    await RinnovaSessioneAsync();
                }

                _initialized = true;

                // 3) Ripulisce l'URL dai parametri OAuth, dopo aver marcato _initialized.
                if (esito.Codice is not null || esito.Errore is not null)
                    _navigation.NavigateTo(_navigation.BaseUri, forceLoad: false, replace: true);
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _facade;
    }

    /// <summary>Avvia l'accesso con Google: chiede l'URL del provider e ci porta il browser.</summary>
    public async Task AvviaAccessoGoogleAsync()
    {
        ErroreAccesso = null;

        var stato = await _auth.SignIn(Constants.Provider.Google, new SignInOptions
        {
            FlowType = Constants.OAuthFlowType.PKCE,
            RedirectTo = _navigation.BaseUri,
        });

        // Il verificatore deve sopravvivere al redirect: fra poco questa pagina non esisterà più.
        if (!string.IsNullOrEmpty(stato.PKCEVerifier))
            _pkce.Salva(stato.PKCEVerifier);

        // La libreria non redirige da sola in WebAssembly: forceLoad obbligatorio, altrimenti
        // il router di Blazor tratterebbe l'URL di Google come una rotta interna.
        _navigation.NavigateTo(stato.Uri.ToString(), forceLoad: true);
    }

    private async Task ScambiaCodiceAsync(string codice)
    {
        var verificatore = _pkce.Leggi();
        if (string.IsNullOrEmpty(verificatore))
        {
            ErroreAccesso = "Accesso non completato: riprova dall'inizio.";
            return;
        }

        try
        {
            var session = await _auth.ExchangeCodeForSession(verificatore, codice);
            if (session?.User is null)
                ErroreAccesso = "Accesso non completato: sessione senza utente.";
        }
        catch (Exception ex)
        {
            ErroreAccesso = $"Accesso non riuscito: {ex.Message}";
        }
        finally
        {
            // Monouso: si cancella comunque, riuscito o no.
            _pkce.Cancella();
        }
    }

    /// <remarks>
    /// Prende <see cref="_initLock"/>: da chiamare SOLO fuori dal lock.
    /// <see cref="SemaphoreSlim"/> non è rientrante — il bootstrap, che gira già dentro il lock,
    /// chiama <see cref="RinnovaSessioneAsync"/> direttamente.
    /// </remarks>
    private async Task RinnovaSessioneSeServeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            await RinnovaSessioneAsync();
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Rinnovo vero e proprio, senza prendere il lock. Ricontrolla da sé se serve (due chiamate
    /// concorrenti possono arrivare qui entrambe) e non propaga MAI eccezioni.
    /// Si usa l'overload a DUE argomenti di <c>RefreshToken</c>: legge i token da una
    /// <c>Session</c> già catturata in locale, quindi non può inciampare in un
    /// <c>CurrentSession</c> azzerato da un'altra chiamata concorrente nel frattempo.
    /// </summary>
    private async Task RinnovaSessioneAsync()
    {
        var session = _auth.CurrentSession;
        if (session is null
            || string.IsNullOrEmpty(session.AccessToken)
            || string.IsNullOrEmpty(session.RefreshToken)
            || !SessionFreshness.VaRinfrescata(
                   SessionFreshness.ScadenzaUtc(session.CreatedAt, session.ExpiresIn), DateTime.UtcNow))
            return;

        if (!SessionFreshness.SiPuoRitentare(_ultimoRefreshFallito, DateTime.UtcNow))
            return;

        try
        {
            // RefreshToken(a, r) assegna già CurrentSession ed emette TokenRefreshed, che il
            // PersistenceListener traduce in SaveSession: notificare SignedIn a mano non solo è
            // superfluo, è sbagliato — un rinnovo non è un accesso, e ogni ascoltatore dello stato
            // vedrebbe un "hai appena fatto login" a ogni ora.
            await _auth.RefreshToken(session.AccessToken, session.RefreshToken);
            _ultimoRefreshFallito = null;
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (
            ex.Reason == Supabase.Gotrue.Exceptions.FailureHint.Reason.ExpiredRefreshToken
            || ex.Reason == Supabase.Gotrue.Exceptions.FailureHint.Reason.InvalidRefreshToken)
        {
            Console.Error.WriteLine($"[Auth] Refresh token non valido, eseguo il logout: {ex.Message}");
            await SignOutAsync();
        }
        catch (Exception ex)
        {
            // Rete assente, 5xx, timeout: NON sloggare. Il refresh token può essere ancora buono.
            Console.Error.WriteLine($"[Auth] Refresh sessione fallito, riprovo più avanti: {ex.Message}");
            _ultimoRefreshFallito = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Porta l'app a uno stato di logout pulito senza MAI propagare eccezioni, e **dichiara se ci
    /// è riuscita**: restituisce <c>true</c> solo quando la sessione in memoria è davvero sparita.
    /// Un logout che fallisce in silenzio è peggio di uno che fallisce: chi ha premuto "Esci" su un
    /// dispositivo condiviso crede di essere uscito e non lo è.
    /// <para>
    /// Ogni passo ha il proprio <c>try</c>, e non è pedanteria: raggruppati, il fallimento del primo
    /// saltava i successivi — in particolare <c>LoadSession()</c>, che è l'unico che azzera davvero
    /// la sessione in memoria. <c>SignOut</c> da solo non basta, perché il suo
    /// <c>UpdateSession(null)</c> sta DOPO l'<c>await</c>: con la rete giù non ci arriva mai.
    /// </para>
    /// <c>DestroySession()</c>, <c>UpdateSession()</c> e il setter di <c>CurrentSession</c> sono
    /// tutti privati in Gotrue 6: l'unica porta pubblica è <c>LoadSession()</c>, che assegna alla
    /// sessione ciò che la persistenza restituisce. E la nostra persistenza, quando non riesce a
    /// leggere, restituisce <c>null</c> — quindi vale la pena chiamarla comunque.
    /// <c>SignOutScope.Local</c>: si esce da questo dispositivo senza revocare le sessioni altrove.
    /// </summary>
    public async Task<bool> SignOutAsync()
    {
        try
        {
            await _auth.SignOut(Constants.SignOutScope.Local);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] SignOut lato server fallito, procedo con la pulizia locale: {ex.Message}");
        }

        try
        {
            _sessionHandler.DestroySession();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] Cancellazione della sessione da localStorage fallita: {ex.Message}");
        }

        try
        {
            _pkce.Cancella();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] Cancellazione del verificatore PKCE fallita: {ex.Message}");
        }

        try
        {
            _auth.LoadSession();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] Azzeramento della sessione in memoria fallito: {ex.Message}");
        }

        var uscito = _auth.CurrentSession is null;
        if (!uscito)
            Console.Error.WriteLine("[Auth] Logout NON riuscito: la sessione è ancora attiva in memoria.");

        return uscito;
    }
}
