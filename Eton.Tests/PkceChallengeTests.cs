using Eton.Services;

namespace Eton.Tests;

public class PkceChallengeTests
{
    // Vettore di prova della RFC 7636, appendice B. Se questo test passa, l'impronta è
    // esattamente quella che il resto del mondo si aspetta — non solo internamente coerente.
    [Fact]
    public void L_impronta_corrisponde_al_vettore_di_prova_della_rfc7636()
        => Assert.Equal(
            "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM",
            PkceChallenge.Impronta("dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"));

    [Fact]
    public void Il_verificatore_ha_la_lunghezza_minima_ammessa()
        => Assert.Equal(43, PkceChallenge.GeneraVerificatore().Length);

    [Fact]
    public void Due_verificatori_non_coincidono_mai()
        => Assert.NotEqual(PkceChallenge.GeneraVerificatore(), PkceChallenge.GeneraVerificatore());

    [Fact]
    public void Il_verificatore_usa_solo_caratteri_sicuri_per_un_url()
        => Assert.Matches("^[A-Za-z0-9_-]+$", PkceChallenge.GeneraVerificatore());

    [Fact]
    public void L_impronta_usa_solo_caratteri_sicuri_per_un_url()
        => Assert.Matches("^[A-Za-z0-9_-]+$", PkceChallenge.Impronta(PkceChallenge.GeneraVerificatore()));

    [Fact]
    public void La_stessa_impronta_per_lo_stesso_verificatore()
    {
        var v = PkceChallenge.GeneraVerificatore();
        Assert.Equal(PkceChallenge.Impronta(v), PkceChallenge.Impronta(v));
    }
}
