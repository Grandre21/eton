using Eton.Services;

namespace Eton.Tests;

public class CalcoliGrigliaTests
{
    private sealed record Riga(string Nome, decimal Importo, DateTime Quando, int Pos);

    private static readonly ColonnaGriglia<Riga> Nome = new("nome", "Nome", TipoColonna.Testo, r => r.Nome);
    private static readonly ColonnaGriglia<Riga> Importo = new("importo", "Importo", TipoColonna.Denaro, r => r.Importo)
    {
        Aggregazioni = Aggregazione.Somma | Aggregazione.Media | Aggregazione.Min | Aggregazione.Max | Aggregazione.Conteggio
    };
    private static readonly ColonnaGriglia<Riga> Quando = new("data", "Data", TipoColonna.Data, r => r.Quando);

    private static Riga R(string nome, decimal importo = 1m, int giorno = 1, int pos = 0)
        => new(nome, importo, new DateTime(2026, 9, giorno), pos);

    [Fact]
    public void Il_testo_si_ordina_ignorando_accenti_e_maiuscole()
    {
        // Sotto InvariantGlobalization un confronto "culturale" è ordinale: senza riduzione degli
        // accenti "Èrba" finirebbe dopo "zucchero".
        var righe = new[] { R("zucchero"), R("Èrba"), R("acqua"), R("erba") };

        var ordinate = CalcoliGriglia.Ordina(righe, Nome, decrescente: false);

        Assert.Equal(["acqua", "Èrba", "erba", "zucchero"], ordinate.Select(r => r.Nome));
    }

    [Fact]
    public void A_parita_resta_l_ordine_di_partenza_anche_in_decrescente()
    {
        var righe = new[] { R("a", 5m, pos: 1), R("b", 5m, pos: 2), R("c", 9m, pos: 3) };

        var ordinate = CalcoliGriglia.Ordina(righe, Importo, decrescente: true);

        Assert.Equal([3, 1, 2], ordinate.Select(r => r.Pos));
    }

    [Fact]
    public void Importi_e_date_si_ordinano_per_valore_non_per_testo()
    {
        var righe = new[] { R("a", 1284.50m, 10), R("b", 99m, 2), R("c", 1000m, 30) };

        Assert.Equal(["b", "c", "a"], CalcoliGriglia.Ordina(righe, Importo, false).Select(r => r.Nome));
        Assert.Equal(["c", "a", "b"], CalcoliGriglia.Ordina(righe, Quando, true).Select(r => r.Nome));
    }

    [Fact]
    public void I_gruppi_seguono_l_ordine_delle_righe()
    {
        var righe = new[] { R("b1"), R("a1"), R("b2") };

        var gruppi = CalcoliGriglia.Raggruppa(righe, r => r.Nome[..1]);

        Assert.Equal(["b", "a"], gruppi.Select(g => g.Etichetta));
        Assert.Equal(["b1", "b2"], gruppi[0].Righe.Select(r => r.Nome));
    }

    [Fact]
    public void Senza_chiave_c_e_un_solo_gruppo_senza_etichetta()
    {
        var gruppi = CalcoliGriglia.Raggruppa(new[] { R("a"), R("b") }, chiave: null);

        var unico = Assert.Single(gruppi);
        Assert.Null(unico.Etichetta);
        Assert.Equal(2, unico.Righe.Count);
    }

    [Fact]
    public void Nessuna_riga_nessun_gruppo()
    {
        Assert.Empty(CalcoliGriglia.Raggruppa(Array.Empty<Riga>(), r => r.Nome));
        Assert.Empty(CalcoliGriglia.Raggruppa(Array.Empty<Riga>(), chiave: null));
    }

    [Fact]
    public void Le_cinque_aggregazioni()
    {
        var riepilogo = CalcoliGriglia.Aggrega(new[] { R("a", 10m), R("b", 20m), R("c", 40m) }, r => r.Importo);

        // 70 / 3 = 23,333… arrotondato a due decimali, lontano dallo zero.
        Assert.Equal(new Riepilogo(3, 70m, 23.33m, 10m, 40m), riepilogo);
    }

    [Fact]
    public void Zero_righe_niente_media_niente_minimo_niente_massimo()
    {
        Assert.Equal(new Riepilogo(0, 0m, null, null, null),
            CalcoliGriglia.Aggrega(Array.Empty<Riga>(), r => r.Importo));
    }

    [Fact]
    public void Una_riga_e_media_minimo_e_massimo_di_se_stessa()
    {
        Assert.Equal(new Riepilogo(1, 7.5m, 7.5m, 7.5m, 7.5m),
            CalcoliGriglia.Aggrega(new[] { R("a", 7.5m) }, r => r.Importo));
    }

    [Fact]
    public void Il_csv_e_quello_che_l_excel_italiano_apre_direttamente()
    {
        var csv = CalcoliGriglia.Csv([Nome, Importo, Quando], new[] { R("Pane", 1284.5m, 3) });

        // BOM, punto e virgola, virgola decimale senza migliaia, data gg/mm/aaaa, a capo CRLF.
        Assert.Equal("\uFEFFNome;Importo;Data\r\nPane;1284,50;03/09/2026\r\n", csv);
    }

    [Fact]
    public void I_campi_con_separatore_virgolette_o_a_capo_si_racchiudono()
    {
        var csv = CalcoliGriglia.Csv([Nome], new[] { R("a;b"), R("di \"Mario\""), R("riga\nnuova") });

        Assert.Equal("\uFEFFNome\r\n\"a;b\"\r\n\"di \"\"Mario\"\"\"\r\n\"riga\nnuova\"\r\n", csv);
    }

    [Fact]
    public void Un_testo_che_sembra_una_formula_non_diventa_una_formula()
    {
        var csv = CalcoliGriglia.Csv([Nome], new[] { R("=SOMMA(A1)"), R("+39 333"), R("-sconto"), R("@capo") });

        Assert.Equal("\uFEFFNome\r\n'=SOMMA(A1)\r\n'+39 333\r\n'-sconto\r\n'@capo\r\n", csv);
    }

    [Fact]
    public void Shift_clic_seleziona_l_intervallo_in_entrambe_le_direzioni()
    {
        string[] chiavi = ["a", "b", "c", "d", "e"];

        Assert.Equal(["b", "c", "d"], CalcoliGriglia.Intervallo(chiavi, "b", "d"));
        Assert.Equal(["b", "c", "d"], CalcoliGriglia.Intervallo(chiavi, "d", "b"));
        // L'ancora non c'è più (filtrata via): si seleziona la sola riga cliccata.
        Assert.Equal(["e"], CalcoliGriglia.Intervallo(chiavi, "sparita", "e"));
    }

    [Fact]
    public void Resa_e_testo_modificabile_non_si_confondono()
    {
        // Denaro.Testo mette il punto delle migliaia, che Denaro.Verifica rifiuta in ingresso:
        // in un campo modificabile va TestoDigitabile (v. il commento in Services/Denaro.cs:113-120).
        Assert.Equal("1.284,50", CalcoliGriglia.TestoCella(TipoColonna.Denaro, 1284.5m));
        Assert.Equal("1284,50", CalcoliGriglia.TestoModificabile(TipoColonna.Denaro, 1284.5m));
        Assert.Equal("03/09/2026", CalcoliGriglia.TestoCella(TipoColonna.Data, new DateTime(2026, 9, 3)));
        Assert.Equal("2026-09-03", CalcoliGriglia.TestoModificabile(TipoColonna.Data, new DateTime(2026, 9, 3)));
        Assert.Equal("", CalcoliGriglia.TestoCella(TipoColonna.Testo, null));
    }

    [Fact]
    public void Il_numero_in_cella_si_legge_come_nelle_collezioni_ma_si_modifica_ed_esporta_esatto()
    {
        // La cella usa la resa di ValoriElemento (al più due decimali); il campo modificabile e il
        // CSV no: ciò che contengono si rilegge, e arrotondare riscriverebbe il valore.
        Assert.Equal("1,24", CalcoliGriglia.TestoCella(TipoColonna.Numero, 1.239m));
        Assert.Equal("3", CalcoliGriglia.TestoCella(TipoColonna.Numero, 3m));
        Assert.Equal("1,239", CalcoliGriglia.TestoModificabile(TipoColonna.Numero, 1.239m));

        var pezzi = new ColonnaGriglia<Riga>("pezzi", "Pezzi", TipoColonna.Numero, r => r.Importo);
        Assert.Equal("\uFEFFPezzi\r\n1,239\r\n", CalcoliGriglia.Csv([pezzi], new[] { R("a", 1.239m) }));
    }
}
