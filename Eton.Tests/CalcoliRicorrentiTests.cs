using Eton.Services;

namespace Eton.Tests;

public class CalcoliRicorrentiTests
{
    [Fact]
    public void Mensile_produce_un_periodo_per_mese()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 9, 5), fine: null, ogniMesi: 1, giorno: 5,
            materializzatoFinoA: null,
            da: new DateTime(2026, 9, 1), a: new DateTime(2026, 11, 30));

        Assert.Equal(["2026-09", "2026-10", "2026-11"], dovuti.Select(d => d.Periodo));
        Assert.Equal(new DateTime(2026, 10, 5), dovuti[1].Data);
    }

    [Fact]
    public void Ogni_tre_mesi_salta_i_due_in_mezzo()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 1, 10), fine: null, ogniMesi: 3, giorno: 10,
            materializzatoFinoA: null,
            da: new DateTime(2026, 1, 1), a: new DateTime(2026, 12, 31));

        Assert.Equal(["2026-01", "2026-04", "2026-07", "2026-10"], dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void Il_giorno_trentuno_diventa_l_ultimo_giorno_del_mese()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 1, 31), fine: null, ogniMesi: 1, giorno: 31,
            materializzatoFinoA: null,
            da: new DateTime(2026, 2, 1), a: new DateTime(2026, 4, 30));

        Assert.Equal(new DateTime(2026, 2, 28), dovuti[0].Data);   // febbraio 2026: 28 giorni
        Assert.Equal(new DateTime(2026, 3, 31), dovuti[1].Data);
        Assert.Equal(new DateTime(2026, 4, 30), dovuti[2].Data);
    }

    [Fact]
    public void Il_buco_lasciato_dall_utente_non_si_riempie()
    {
        // L'utente ha cancellato l'occorrenza di settembre. Il watermark è avanti:
        // ottobre e novembre sono dovuti, settembre NO.
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 6, 5), fine: null, ogniMesi: 1, giorno: 5,
            materializzatoFinoA: "2026-09",
            da: new DateTime(2026, 6, 1), a: new DateTime(2026, 11, 30));

        Assert.Equal(["2026-10", "2026-11"], dovuti.Select(d => d.Periodo));
        Assert.DoesNotContain("2026-09", dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void Niente_prima_dell_inizio()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 10, 1), fine: null, ogniMesi: 1, giorno: 1,
            materializzatoFinoA: null,
            da: new DateTime(2026, 8, 1), a: new DateTime(2026, 10, 31));

        Assert.Equal(["2026-10"], dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void Niente_dopo_la_fine()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 1, 15), fine: new DateTime(2026, 3, 20),
            ogniMesi: 1, giorno: 15, materializzatoFinoA: null,
            da: new DateTime(2026, 1, 1), a: new DateTime(2026, 6, 30));

        Assert.Equal(["2026-01", "2026-02", "2026-03"], dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void La_fine_esclude_l_occorrenza_che_cade_dopo_di_essa_nello_stesso_mese()
    {
        // Termina il 3 marzo, l'occorrenza di marzo cadrebbe il 15: non è dovuta.
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 1, 15), fine: new DateTime(2026, 3, 3),
            ogniMesi: 1, giorno: 15, materializzatoFinoA: null,
            da: new DateTime(2026, 1, 1), a: new DateTime(2026, 6, 30));

        Assert.Equal(["2026-01", "2026-02"], dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void Periodo_formatta_a_due_cifre()
    {
        Assert.Equal("2026-03", CalcoliRicorrenti.Periodo(new DateTime(2026, 3, 7)));
    }

    [Fact]
    public void Prossima_e_null_per_una_regola_terminata()
    {
        var prossima = CalcoliRicorrenti.Prossima(
            inizio: new DateTime(2026, 1, 5), fine: new DateTime(2026, 6, 30),
            ogniMesi: 1, giorno: 5, oggi: new DateTime(2026, 9, 3));

        Assert.Null(prossima);
    }

    [Fact]
    public void Cadenza_mensile_dice_il_giorno()
    {
        Assert.Equal("ogni mese, il 5", CalcoliRicorrenti.Cadenza(1, 5, new DateTime(2026, 1, 5)));
    }

    [Fact]
    public void Cadenza_ogni_due_mesi_dice_il_giorno()
    {
        Assert.Equal("ogni 2 mesi, il 5", CalcoliRicorrenti.Cadenza(2, 5, new DateTime(2026, 1, 5)));
    }

    [Fact]
    public void Cadenza_mensile_col_trentuno_e_l_ultimo_giorno()
    {
        Assert.Equal("ogni mese, l'ultimo giorno", CalcoliRicorrenti.Cadenza(1, 31, new DateTime(2026, 1, 31)));
    }

    [Fact]
    public void Cadenza_elide_l_ottavo_e_ordina_il_primo()
    {
        Assert.Equal("ogni mese, il 1°", CalcoliRicorrenti.Cadenza(1, 1, new DateTime(2026, 1, 1)));
        Assert.Equal("ogni mese, l'8", CalcoliRicorrenti.Cadenza(1, 8, new DateTime(2026, 1, 8)));
    }

    [Fact]
    public void Cadenza_annuale_dice_giorno_e_mese()
    {
        Assert.Equal("ogni anno, il 5 marzo", CalcoliRicorrenti.Cadenza(12, 5, new DateTime(2026, 3, 5)));
    }

    [Fact]
    public void Cadenza_annuale_col_trenta_a_febbraio_e_l_ultimo_giorno_di_febbraio()
    {
        Assert.Equal("ogni anno, l'ultimo giorno di febbraio", CalcoliRicorrenti.Cadenza(12, 30, new DateTime(2026, 2, 1)));
    }

    [Fact]
    public void Cadenza_con_ogniMesi_zero_lancia()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CalcoliRicorrenti.Cadenza(0, 5, new DateTime(2026, 1, 5)));
    }
}
