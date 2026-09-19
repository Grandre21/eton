using Eton.Services;
using Supabase.Gotrue;

namespace Eton.Tests;

public class IdentitaGoogleTests
{
    private static User Utente(string? email, params (string Chiave, object Valore)[] metadati)
    {
        var u = new User { Email = email };
        foreach (var (chiave, valore) in metadati) u.UserMetadata[chiave] = valore;
        return u;
    }

    [Fact]
    public void Full_name_vince_su_tutto()
        => Assert.Equal("Mario Rossi", IdentitaGoogle.NomeDa(Utente("mario@esempio.it", ("full_name", "Mario Rossi"), ("name", "Mario"))));

    [Fact]
    public void Senza_full_name_vince_name()
        => Assert.Equal("Mario", IdentitaGoogle.NomeDa(Utente("mario@esempio.it", ("name", "Mario"))));

    [Fact]
    public void Full_name_di_soli_spazi_passa_a_name()
        => Assert.Equal("Mario", IdentitaGoogle.NomeDa(Utente("mario@esempio.it", ("full_name", "   "), ("name", "Mario"))));

    [Fact]
    public void Senza_nessuna_chiave_si_ripiega_sull_email()
        => Assert.Equal("mario@esempio.it", IdentitaGoogle.NomeDa(Utente("mario@esempio.it")));

    [Fact]
    public void Un_full_name_non_stringa_si_ignora_e_si_ripiega()
        => Assert.Equal("mario@esempio.it", IdentitaGoogle.NomeDa(Utente("mario@esempio.it", ("full_name", 42))));

    [Fact]
    public void Utente_null_da_null_da_entrambi_i_metodi()
    {
        Assert.Null(IdentitaGoogle.NomeDa(null));
        Assert.Null(IdentitaGoogle.FotoDa(null));
    }

    [Fact]
    public void FotoDa_con_avatar_url_valorizzato_restituisce_quel_valore()
        => Assert.Equal("https://esempio.it/foto.png", IdentitaGoogle.FotoDa(Utente("mario@esempio.it", ("avatar_url", "https://esempio.it/foto.png"))));

    [Fact]
    public void FotoDa_senza_la_chiave_e_null_senza_ripiegare_sull_email()
        => Assert.Null(IdentitaGoogle.FotoDa(Utente("mario@esempio.it")));

    [Fact]
    public void FotoDa_con_avatar_url_di_soli_spazi_e_null()
        => Assert.Null(IdentitaGoogle.FotoDa(Utente("mario@esempio.it", ("avatar_url", "   "))));

    // Riproduce la deserializzazione di "user_metadata": null: senza il controllo su UserMetadata
    // in Metadato, nessuno si accorgerebbe se venisse tolto credendolo ridondante.
    [Fact]
    public void Metadati_null_dal_json_non_esplodono_e_si_ripiega_sull_email()
    {
        var utente = new User { Email = "mario@esempio.it", UserMetadata = null! };
        Assert.Equal("mario@esempio.it", IdentitaGoogle.NomeDa(utente));
        Assert.Null(IdentitaGoogle.FotoDa(utente));
    }
}
