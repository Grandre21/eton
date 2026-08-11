using Microsoft.AspNetCore.Components;

namespace Eton.Services;

/// <summary>
/// Identità dell'utente autenticato, letta dalla sessione Gotrue. Ogni metodo passa da
/// <see cref="SupabaseService.GetClientAsync"/>, che garantisce il bootstrap già avvenuto: le
/// pagine non devono sapere nulla dell'ordine di inizializzazione.
/// </summary>
public class AuthStateService
{
    private readonly SupabaseService _supabase;
    private readonly NavigationManager _navigation;
    private readonly SpaceStateService _spazi;

    public AuthStateService(SupabaseService supabase, NavigationManager navigation, SpaceStateService spazi)
    {
        _supabase = supabase;
        _navigation = navigation;
        _spazi = spazi;
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var client = await _supabase.GetClientAsync();
        return client.Auth.CurrentSession?.User is not null;
    }

    /// <summary>Id Gotrue dell'utente (<c>auth.users.id</c>): è l'<c>owner_id</c> di ogni risorsa.</summary>
    public async Task<string?> GetUserIdAsync()
    {
        var client = await _supabase.GetClientAsync();
        return client.Auth.CurrentSession?.User?.Id;
    }

    public async Task<string?> GetEmailAsync()
    {
        var client = await _supabase.GetClientAsync();
        return client.Auth.CurrentSession?.User?.Email;
    }

    /// <summary>Nome visualizzato: nome completo Google, con ripiego sull'email.</summary>
    public async Task<string?> GetDisplayNameAsync()
    {
        var client = await _supabase.GetClientAsync();
        var user = client.Auth.CurrentSession?.User;
        if (user is null) return null;

        if (user.UserMetadata is not null)
        {
            foreach (var chiave in new[] { "full_name", "name" })
            {
                if (user.UserMetadata.TryGetValue(chiave, out var valore)
                    && valore is string s && !string.IsNullOrWhiteSpace(s))
                {
                    return s;
                }
            }
        }

        return user.Email;
    }

    /// <summary>
    /// <c>replace: true</c> come ogni altra navigazione del flusso d'accesso: senza, il tasto
    /// Indietro riporterebbe sulla Home, che rimonta, mostra "Caricamento…" e viene rimbalzata di
    /// nuovo al login — uno sfarfallio a ogni pressione.
    /// <c>forceLoad</c> solo quando l'uscita non è riuscita: ricaricare la pagina butta via tutto
    /// lo stato in memoria, ed è l'ultima carta quando la sessione non si è lasciata azzerare.
    /// <para>
    /// <c>Dimentica()</c> va DOPO <c>SignOutAsync()</c>, e l'ordine non è indifferente: quell'await
    /// è una chiamata di rete vera, e per tutta la sua durata la sessione è ancora valida. Se lo
    /// spazio venisse dimenticato prima, una navigazione dell'utente in quella finestra — l'app
    /// resta interattiva — ricaricherebbe l'elenco con una sessione che funziona ancora e
    /// riscriverebbe in localStorage lo spazio di chi sta uscendo, che è esattamente ciò che
    /// <c>Dimentica()</c> serve a impedire. Dopo, invece, non c'è più niente da cui ripopolarsi.
    /// <c>SignOutAsync()</c> non propaga eccezioni (ogni passo ha il proprio try e restituisce un
    /// bool), quindi metterlo prima non rischia di far saltare la pulizia.
    /// </para>
    /// </summary>
    public async Task LogoutAsync()
    {
        var uscito = await _supabase.SignOutAsync();
        _spazi.Dimentica();
        _navigation.NavigateTo("login", forceLoad: !uscito, replace: true);
    }
}
