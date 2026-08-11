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

    public AuthStateService(SupabaseService supabase, NavigationManager navigation)
    {
        _supabase = supabase;
        _navigation = navigation;
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
    /// </summary>
    public async Task LogoutAsync()
    {
        var uscito = await _supabase.SignOutAsync();
        _navigation.NavigateTo("login", forceLoad: !uscito, replace: true);
    }
}
