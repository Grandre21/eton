using Eton.Services;

namespace Eton.Tests;

public class DenaroTests
{
    // ---------- Prova ----------

    [Fact]
    public void La_virgola_italiana_viene_accettata()
    {
        Assert.True(Denaro.Prova("12,50", out var importo));
        Assert.Equal(12.50m, importo);
    }

    [Fact]
    public void Il_punto_del_tastierino_numerico_viene_accettato()
    {
        Assert.True(Denaro.Prova("12.50", out var importo));
        Assert.Equal(12.50m, importo);
    }

    [Fact]
    public void Un_piu_iniziale_viene_accettato()
    {
        // Una battuta a vuoto sulla tastiera, non un'ambiguità sul segno: il valore che ne esce è
        // lo stesso di "12,50".
        Assert.True(Denaro.Prova("+12,50", out var importo));
        Assert.Equal(12.50m, importo);
    }

    [Theory]
    [InlineData("0")]                // il vincolo del database è amount > 0
    // "-3" supera il parsing: AllowLeadingSign (v. la docstring di Prova) ammette anche il "-"
    // iniziale. A fermarlo non è il parsing ma il controllo su valore <= 0 più sotto nel metodo,
    // la stessa regola del vincolo amount > 0 del database — non un fallimento di lettura. Chi in
    // futuro togliesse AllowLeadingSign "per pulizia" starebbe spostando anche questo controllo.
    [InlineData("-3")]
    [InlineData("12,505")]            // tre decimali: la colonna è numeric(12,2)
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1.234,50")]          // punto delle migliaia più virgola: due separatori, non uno
    [InlineData("12,34,56")]          // due virgole: stesso motivo
    [InlineData("12 , 50")]           // gli spazi ai bordi si tolgono col Trim, quelli in mezzo no:
                                       // in mezzo a un numero uno spazio è un errore di battitura,
                                       // non una formattazione da tollerare
    public void Valori_non_ammessi_vengono_rifiutati(string testo)
    {
        Assert.False(Denaro.Prova(testo, out var importo));

        // Chi chiama può usare l'out senza guardare il valore di ritorno: un residuo diverso da
        // zero qui finirebbe scritto nel database come se fosse un importo valido.
        Assert.Equal(0m, importo);
    }

    [Fact]
    public void Il_valore_piu_alto_rappresentabile_da_numeric_12_2_viene_accettato()
    {
        // numeric(12,2): 12 cifre di precisione meno 2 di scala fanno 10 cifre intere. Questo è
        // l'ultimo valore che il campo può contenere.
        Assert.True(Denaro.Prova("9999999999,99", out var importo));
        Assert.Equal(9999999999.99m, importo);
    }

    [Fact]
    public void Il_primo_valore_che_supera_numeric_12_2_viene_rifiutato()
        // Un'unità in più rispetto al test precedente: la prima cifra che numeric(12,2) non può
        // più contenere.
        => Assert.False(Denaro.Prova("10000000000", out _));

    [Fact]
    public void Il_valore_nullo_viene_rifiutato()
        => Assert.False(Denaro.Prova(null, out _));

    [Fact]
    public void Gli_spazi_intorno_non_danno_fastidio()
    {
        Assert.True(Denaro.Prova(" 12,50 ", out var importo));
        Assert.Equal(12.50m, importo);
    }

    [Fact]
    public void Un_intero_senza_decimali_passa()
    {
        Assert.True(Denaro.Prova("7", out var importo));
        Assert.Equal(7m, importo);
    }

    // ---------- Testo ----------

    // Questo è il test che conta più di tutti. È precisamente il punto in cui una sostituzione a
    // due passaggi (virgola -> punto, poi punto -> virgola) ritroverebbe i punti che ha appena
    // scritto lei stessa — comprese le migliaia — e produrrebbe "1,284,50" invece di "1.284,50".
    // Sotto il migliaio il difetto non si vede, perché non c'è nessun separatore delle migliaia da
    // scambiare per sbaglio: ecco perché serve un valore sopra i mille per accorgersene.
    [Fact]
    public void Testo_sopra_il_migliaio_mette_il_punto_alle_migliaia_e_la_virgola_ai_decimali()
        => Assert.Equal("1.284,50", Denaro.Testo(1284.50m));

    [Fact]
    public void Testo_di_un_valore_sotto_l_unita_mostra_lo_zero_iniziale()
        => Assert.Equal("0,05", Denaro.Testo(0.05m));

    [Fact]
    public void Testo_con_piu_gruppi_di_migliaia_mette_un_punto_per_gruppo()
        => Assert.Equal("1.000.000,00", Denaro.Testo(1000000m));

    [Fact]
    public void Testo_di_un_intero_mostra_comunque_due_decimali()
        => Assert.Equal("7,00", Denaro.Testo(7m));

    // ---------- andata e ritorno ----------

    // Prova(Testo(x)) NON deve necessariamente ridare x: Testo introduce il separatore delle
    // migliaia ("1.284,50"), che Prova rifiuta di proposito perché più di un separatore vuol dire
    // "probabile errore di digitazione", non "migliaia". I due metodi servono due direzioni
    // diverse — uno legge ciò che una persona digita, l'altro mostra un valore già validato — e
    // questo test documenta l'asimmetria invece di nasconderla.
    [Fact]
    public void Testo_e_Prova_non_sono_l_uno_l_inverso_dell_altro_sopra_il_migliaio()
    {
        var testo = Denaro.Testo(1284.50m);

        Assert.Equal("1.284,50", testo);
        Assert.False(Denaro.Prova(testo, out _));
    }

    [Fact]
    public void Testo_e_Prova_sono_coerenti_sotto_il_migliaio()
    {
        var testo = Denaro.Testo(12.50m);

        Assert.True(Denaro.Prova(testo, out var importo));
        Assert.Equal(12.50m, importo);
    }
}
