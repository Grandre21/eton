using Eton.Services;

namespace Eton.Tests;

public class AllineatoreProfiloTests
{
    private const string UtenteA = "11111111-1111-1111-1111-111111111111";
    private const string UtenteB = "22222222-2222-2222-2222-222222222222";

    [Fact]
    public void L_impronta_e_stabile_a_parita_di_argomenti()
        => Assert.Equal(
            AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1"),
            AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1"));

    [Fact]
    public void L_impronta_cambia_se_cambia_il_nome()
        => Assert.NotEqual(
            AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1"),
            AllineatoreProfilo.Impronta(UtenteA, "Mario Bianchi", "https://foto/1"));

    [Fact]
    public void L_impronta_cambia_se_cambia_la_foto()
        => Assert.NotEqual(
            AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1"),
            AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/2"));

    [Fact]
    public void L_impronta_cambia_se_cambia_l_utente_a_parita_di_nome_e_foto()
        => Assert.NotEqual(
            AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1"),
            AllineatoreProfilo.Impronta(UtenteB, "Mario Rossi", "https://foto/1"));

    [Fact]
    public void Nome_e_foto_nulli_non_esplodono_e_producono_un_impronta_valida()
        => Assert.False(string.IsNullOrEmpty(AllineatoreProfilo.Impronta(UtenteA, null, null)));

    [Fact]
    public void Con_impronta_identica_e_senza_accesso_appena_fatto_non_si_scrive()
    {
        var viva = AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1");
        Assert.False(AllineatoreProfilo.VaScritto(viva, viva, dopoAccesso: false));
    }

    [Fact]
    public void Con_impronte_diverse_si_scrive()
    {
        var salvata = AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1");
        var viva = AllineatoreProfilo.Impronta(UtenteA, "Mario Bianchi", "https://foto/1");
        Assert.True(AllineatoreProfilo.VaScritto(salvata, viva, dopoAccesso: false));
    }

    [Fact]
    public void Con_impronta_salvata_assente_si_scrive()
    {
        var viva = AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1");
        Assert.True(AllineatoreProfilo.VaScritto(null, viva, dopoAccesso: false));
    }

    [Fact]
    public void Dopo_un_accesso_si_scrive_anche_se_le_impronte_coincidono()
    {
        var viva = AllineatoreProfilo.Impronta(UtenteA, "Mario Rossi", "https://foto/1");
        Assert.True(AllineatoreProfilo.VaScritto(viva, viva, dopoAccesso: true));
    }

    // Il separatore non deve essere un carattere che può comparire nei valori: con "\n" queste due impronte erano identiche.
    [Fact]
    public void Un_a_capo_nei_valori_non_fa_collidere_due_impronte_diverse()
        => Assert.NotEqual(
            AllineatoreProfilo.Impronta("u", "A", "B\nC"),
            AllineatoreProfilo.Impronta("u", "A\nB", "C"));
}
