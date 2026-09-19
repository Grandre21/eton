using Supabase.Gotrue;

namespace Eton.Services;

/// <summary>
/// Nome e foto dall'identità Google, letti dai metadati di <see cref="User"/>: due decisioni pure,
/// isolate per essere verificabili senza un client Gotrue.
/// <para>
/// La precedenza delle chiavi — <c>full_name → name → email</c> per il nome, <c>avatar_url</c>
/// senza ripiego per la foto — è la stessa che la funzione di database
/// <c>public.handle_new_user()</c> applica al primo accesso, scrivendo
/// <c>profiles.display_name</c> con <c>coalesce(raw_user_meta_data ->> 'full_name', ->> 'name',
/// email)</c> e <c>profiles.avatar_url</c> con <c>raw_user_meta_data ->> 'avatar_url'</c>. È
/// questa coincidenza che rende confrontabile il valore vivo con quello scritto nel database: se
/// cambia la precedenza da una parte, va cambiata anche dall'altra.
/// Il C# è però più severo: scarta anche le stringhe vuote, quelle di soli spazi e i valori
/// non-stringa, dove <c>coalesce</c> in SQL salterebbe solo i <c>NULL</c>. Su quei casi il
/// confronto fra il valore vivo e quello in tabella trova una differenza e la corregge.
/// </para>
/// </summary>
public static class IdentitaGoogle
{
    /// <summary>Nome visualizzato: <c>full_name</c>, poi <c>name</c>, poi l'email.</summary>
    public static string? NomeDa(User? utente)
        => utente is null ? null : Metadato(utente, "full_name") ?? Metadato(utente, "name") ?? utente.Email;

    /// <summary>Foto profilo: <c>avatar_url</c>, senza ripiego.</summary>
    public static string? FotoDa(User? utente)
        => utente is null ? null : Metadato(utente, "avatar_url");

    // Sembra ridondante controllare UserMetadata is not null, ma i modelli arrivano da Newtonsoft:
    // un "user_metadata": null nel corpo della risposta può sovrascrivere l'inizializzatore di
    // campo. Senza questo controllo, un utente con metadati assenti manderebbe in
    // NullReferenceException il bootstrap.
    private static string? Metadato(User utente, string chiave)
        => utente.UserMetadata is not null
           && utente.UserMetadata.TryGetValue(chiave, out var valore)
           && valore is string s && !string.IsNullOrWhiteSpace(s)
            ? s : null;
}
