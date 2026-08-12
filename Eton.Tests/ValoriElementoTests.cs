using Eton.Services;

namespace Eton.Tests;

public class ValoriElementoTests
{
    // ---------- DaTesto ----------

    [Fact]
    public void Il_testo_libero_passa_cosi_com_e()
        => Assert.Equal("Vaporart", ValoriElemento.DaTesto("Vaporart", "text"));

    [Fact]
    public void Il_testo_libero_viene_ripulito_dagli_spazi_ai_lati()
        => Assert.Equal("Vaporart", ValoriElemento.DaTesto("  Vaporart  ", "text"));

    [Fact]
    public void Un_testo_vuoto_non_produce_un_valore()
        => Assert.Null(ValoriElemento.DaTesto("", "text"));

    [Fact]
    public void Un_testo_nullo_non_produce_un_valore()
        => Assert.Null(ValoriElemento.DaTesto(null, "text"));

    [Theory]
    [InlineData("12.90")]
    [InlineData("12,90")]   // la virgola decimale non è un dettaglio: è quello che digita chi scrive in italiano
    public void Un_numero_si_riconosce_col_punto_o_con_la_virgola(string input)
        => Assert.Equal(12.90d, ValoriElemento.DaTesto(input, "number"));

    [Fact]
    public void Un_numero_non_riconoscibile_non_produce_un_valore()
        => Assert.Null(ValoriElemento.DaTesto("abc", "number"));

    [Theory]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    [InlineData("NaN")]
    [InlineData("1e400")]
    public void Un_numero_non_finito_non_e_un_numero(string input)
        // double.TryParse con NumberStyles.Float accetta questi valori, e Newtonsoft serializza
        // un infinito come la stringa "Infinity", cambiando di nascosto il tipo JSON da numero
        // a stringa dentro collection_items.data, e mostrando all'utente una parola inglese in
        // un campo che ovunque altrove è curato per restare in italiano.
        => Assert.Null(ValoriElemento.DaTesto(input, "number"));

    [Fact]
    public void Una_data_gia_in_formato_iso_resta_tale()
        => Assert.Equal("2027-05-01", ValoriElemento.DaTesto("2027-05-01", "date"));

    [Fact]
    public void Una_data_in_formato_italiano_viene_normalizzata_in_iso()
        => Assert.Equal("2027-05-01", ValoriElemento.DaTesto("01/05/2027", "date"));

    [Fact]
    public void Una_data_non_riconoscibile_non_produce_un_valore()
        => Assert.Null(ValoriElemento.DaTesto("non una data", "date"));

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Un_booleano_si_riconosce_dal_testo(string input, bool atteso)
        => Assert.Equal(atteso, ValoriElemento.DaTesto(input, "bool"));

    [Fact]
    public void Un_url_legittimo_passa_cosi_com_e()
        => Assert.Equal("https://esempio.it", ValoriElemento.DaTesto("https://esempio.it", "url"));

    [Fact]
    public void Un_url_con_schema_pericoloso_non_produce_un_valore()
        => Assert.Null(ValoriElemento.DaTesto("javascript:alert(1)", "url"));

    // ---------- Testo ----------

    [Fact]
    public void Un_valore_nullo_diventa_stringa_vuota()
        => Assert.Equal("", ValoriElemento.Testo(null, "text"));

    [Fact]
    public void Un_intero_dal_jsonb_arriva_come_int64()
        // Dal jsonb un numero senza decimali torna deserializzato come Int64, non come int: se
        // Testo() lo gestisse solo per int, un prezzo intero romperebbe la scheda.
        => Assert.Equal("6", ValoriElemento.Testo(6L, "number"));

    [Theory]
    [InlineData(12.9d, "12,9")]
    [InlineData(12.5d, "12,5")]
    [InlineData(6.0d, "6")]   // zeri finali inutili non si mostrano, nemmeno il separatore decimale
    public void Un_numero_si_mostra_con_la_virgola_decimale(double valore, string atteso)
        => Assert.Equal(atteso, ValoriElemento.Testo(valore, "number"));

    // Il formato non dipende dalla cultura del browser. Sotto InvariantGlobalization esiste una
    // sola cultura, quindi impostarne una lancerebbe: l'unico modo di avere un'uscita in italiano
    // è scriverne il pattern a mano. Questa prova esiste perché il giorno in cui qualcuno
    // "semplificasse" con un ToString() senza argomenti, il numero uscirebbe col punto e nessun
    // altro test se ne accorgerebbe.
    [Fact]
    public void Il_numero_usa_sempre_la_virgola_indipendentemente_dalla_cultura_del_sistema()
    {
        var testo = ValoriElemento.Testo(12.9d, "number");
        Assert.Contains(",", testo);
        Assert.DoesNotContain(".", testo);
    }

    [Theory]
    [InlineData(true, "Sì")]
    [InlineData(false, "No")]
    public void Un_booleano_si_mostra_in_italiano(bool valore, string atteso)
        => Assert.Equal(atteso, ValoriElemento.Testo(valore, "bool"));

    [Fact]
    public void Una_data_iso_si_mostra_in_formato_italiano()
        => Assert.Equal("01/05/2027", ValoriElemento.Testo("2027-05-01", "date"));

    [Fact]
    public void Una_data_arrivata_come_datetime_si_mostra_in_formato_italiano()
        // Serve davvero: se qualcuno salvasse una stringa ISO con orario, dal database tornerebbe
        // un DateTime (Newtonsoft la riconvertirebbe in fase di lettura), e questa prova documenta
        // che il caso è gestito invece di far esplodere la scheda.
        => Assert.Equal("01/05/2027", ValoriElemento.Testo(new DateTime(2027, 5, 1), "date"));

    // ---------- PerModifica ----------

    [Fact]
    public void Un_valore_nullo_per_la_modifica_diventa_stringa_vuota()
        => Assert.Equal("", ValoriElemento.PerModifica(null, "text"));

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Un_booleano_per_la_modifica_usa_true_false_non_si_no(bool valore, string atteso)
        // A differenza di Testo (che scrive "Sì"/"No" per lo schermo), PerModifica deve produrre
        // ciò che DaTesto sa rileggere: un editor che ricaricasse con Testo e risalvasse con
        // DaTesto azzererebbe ogni casella Sì/No al primo salvataggio.
        => Assert.Equal(atteso, ValoriElemento.PerModifica(valore, "bool"));

    [Fact]
    public void Un_valore_non_booleano_per_un_campo_bool_diventa_stringa_vuota_per_la_modifica()
        => Assert.Equal("", ValoriElemento.PerModifica("qualcosa", "bool"));

    [Fact]
    public void Un_valore_nullo_per_un_campo_bool_diventa_stringa_vuota_per_la_modifica()
        => Assert.Equal("", ValoriElemento.PerModifica(null, "bool"));

    [Fact]
    public void Una_data_iso_per_la_modifica_resta_iso()
        => Assert.Equal("2027-05-01", ValoriElemento.PerModifica("2027-05-01", "date"));

    [Fact]
    public void Una_data_arrivata_come_datetime_per_la_modifica_si_normalizza_in_iso()
        // A differenza di Testo (che la mostrerebbe in italiano), PerModifica deve restituire
        // sempre yyyy-MM-dd: è il formato che il campo HTML type="date" si aspetta e che DaTesto
        // sa rileggere.
        => Assert.Equal("2027-05-01", ValoriElemento.PerModifica(new DateTime(2027, 5, 1), "date"));

    [Theory]
    [InlineData("text", "Vaporart")]
    [InlineData("url", "https://esempio.it")]
    [InlineData("select", "60/40")]
    public void Il_giro_completo_PerModifica_DaTesto_ripropone_lo_stesso_valore_per_i_tipi_testuali(string tipo, string valore)
        => Assert.Equal(valore, ValoriElemento.DaTesto(ValoriElemento.PerModifica(valore, tipo), tipo));

    [Fact]
    public void Il_giro_completo_PerModifica_DaTesto_ripropone_lo_stesso_numero()
        => Assert.Equal(12.9d, ValoriElemento.DaTesto(ValoriElemento.PerModifica(12.9d, "number"), "number"));

    [Fact]
    public void Il_giro_completo_PerModifica_DaTesto_ripropone_la_stessa_data()
        => Assert.Equal("2027-05-01", ValoriElemento.DaTesto(ValoriElemento.PerModifica(new DateTime(2027, 5, 1), "date"), "date"));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Il_giro_completo_PerModifica_DaTesto_ripropone_lo_stesso_booleano(bool valore)
        => Assert.Equal(valore, ValoriElemento.DaTesto(ValoriElemento.PerModifica(valore, "bool"), "bool"));
}
