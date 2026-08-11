using Eton.Services;

namespace Eton.Tests;

public class SessionFreshnessTests
{
    private static readonly DateTime Adesso = new(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Sessione_lontana_dalla_scadenza_non_va_rinfrescata()
        => Assert.False(SessionFreshness.VaRinfrescata(Adesso.AddMinutes(30), Adesso));

    [Fact]
    public void Sessione_dentro_il_margine_va_rinfrescata()
        => Assert.True(SessionFreshness.VaRinfrescata(Adesso.AddMinutes(4), Adesso));

    [Fact]
    public void Sessione_gia_scaduta_va_rinfrescata()
        => Assert.True(SessionFreshness.VaRinfrescata(Adesso.AddMinutes(-1), Adesso));

    [Fact]
    public void Senza_fallimenti_precedenti_si_puo_ritentare()
        => Assert.True(SessionFreshness.SiPuoRitentare(null, Adesso));

    [Fact]
    public void Subito_dopo_un_fallimento_non_si_ritenta()
        => Assert.False(SessionFreshness.SiPuoRitentare(Adesso.AddSeconds(-5), Adesso));

    [Fact]
    public void Passata_l_attesa_si_ritenta()
        => Assert.True(SessionFreshness.SiPuoRitentare(Adesso.AddSeconds(-31), Adesso));

    [Fact]
    public void Il_confine_esatto_del_margine_conta_come_da_rinfrescare()
        => Assert.True(SessionFreshness.VaRinfrescata(Adesso + SessionFreshness.Margine, Adesso));

    [Fact]
    public void Il_confine_esatto_dell_attesa_conta_come_ritentabile()
        => Assert.True(SessionFreshness.SiPuoRitentare(Adesso - SessionFreshness.AttesaDopoFallimento, Adesso));

    [Fact]
    public void La_scadenza_e_la_creazione_piu_la_durata()
        => Assert.Equal(Adesso.AddHours(1), SessionFreshness.ScadenzaUtc(Adesso, 3600));

    [Fact]
    public void La_scadenza_resta_in_utc()
        => Assert.Equal(DateTimeKind.Utc, SessionFreshness.ScadenzaUtc(Adesso, 3600).Kind);

    [Fact]
    public void Una_durata_assurda_non_fa_esplodere_il_calcolo()
    {
        var scadenza = SessionFreshness.ScadenzaUtc(Adesso, long.MaxValue);
        Assert.True(scadenza > Adesso);
        Assert.False(SessionFreshness.VaRinfrescata(scadenza, Adesso));
    }

    [Fact]
    public void Una_durata_nulla_significa_gia_scaduta()
        => Assert.True(SessionFreshness.VaRinfrescata(SessionFreshness.ScadenzaUtc(Adesso, 0), Adesso));
}
