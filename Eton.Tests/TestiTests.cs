using Eton.Services;

namespace Eton.Tests;

public class TestiTests
{
    // ---------- Conteggio ----------

    // Le tre forme compaiono nella testata di ogni registro dell'applicazione (collezioni, note,
    // spazi, elementi di una collezione). Prima erano quattro copie della stessa riga sparse nelle
    // pagine: se una divergeva, il singolare sbagliato compariva in una sola schermata e nessuno
    // capiva perché proprio lì.
    [Fact]
    public void Zero_va_al_plurale()
        => Assert.Equal("0 note", Testi.Conteggio(0, "nota", "note"));

    [Fact]
    public void Uno_va_al_singolare()
        => Assert.Equal("1 nota", Testi.Conteggio(1, "nota", "note"));

    [Fact]
    public void Piu_di_uno_va_al_plurale()
        => Assert.Equal("5 note", Testi.Conteggio(5, "nota", "note"));

    // Il singolare e il plurale sono parametri, non una regola dedotta dalla parola: in italiano
    // "spazio"/"spazi" e "collezione"/"collezioni" seguono schemi diversi, e indovinare la
    // desinenza funzionerebbe finché non arriva la parola che non ci sta.
    [Fact]
    public void Le_due_forme_arrivano_da_fuori_e_non_si_deducono()
        => Assert.Equal("1 spazio", Testi.Conteggio(1, "spazio", "spazi"));

    // ---------- Data ----------

    // DateTimeKind.Local e non Utc: ToLocalTime su un valore già locale è l'identità, quindi il
    // test dà lo stesso risultato su qualunque macchina. Con Utc dipenderebbe dal fuso di chi lo
    // esegue e fallirebbe in metà del mondo.
    [Fact]
    public void La_data_si_scrive_giorno_mese_anno()
    {
        var quando = new DateTime(2026, 8, 3, 14, 30, 0, DateTimeKind.Local);

        Assert.Equal("03/08/2026", Testi.Data(quando));
    }

    [Fact]
    public void Giorno_e_mese_hanno_sempre_due_cifre()
    {
        var quando = new DateTime(2026, 1, 9, 0, 5, 0, DateTimeKind.Local);

        Assert.Equal("09/01/2026", Testi.Data(quando));
    }

    // L'ora serve solo alle note, dove due modifiche dello stesso giorno sono la norma e la sola
    // data non basterebbe a dire quale è l'ultima.
    [Fact]
    public void La_data_con_ora_aggiunge_ore_e_minuti_a_ventiquattro_ore()
    {
        var quando = new DateTime(2026, 8, 3, 14, 5, 0, DateTimeKind.Local);

        Assert.Equal("03/08/2026 14:05", Testi.DataOra(quando));
    }

    // ---------- DataSola ----------

    [Fact]
    public void La_data_sola_si_scrive_giorno_mese_anno()
    {
        var quando = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Unspecified);

        Assert.Equal("05/01/2026", Testi.DataSola(quando));
    }

    // Il test che conta davvero. Una colonna "date" di Postgres arriva come DateTime a mezzanotte
    // con Kind = Unspecified, e .ToLocalTime() su Unspecified NON è l'identità: per specifica .NET
    // tratta il valore come se fosse UTC e applica l'offset locale. Su un fuso negativo
    // (America/New_York, UTC-5) questo sposta il 5 gennaio a mezzanotte al 4 gennaio delle 19 —
    // un giorno indietro, senza errore visibile. In Italia (fuso positivo) lo spostamento resta
    // nello stesso giorno e il difetto non si vede mai: ecco perché un test che passa solo in
    // Europa sarebbe peggio di nessun test.
    //
    // Non possiamo cambiare il fuso della macchina di collaudo, quindi non possiamo asserire un
    // valore fisso come "04/01/2026" atteso su UTC-5: quel valore dipenderebbe dal fuso di chi
    // esegue il test. Asseriamo invece la proprietà che non dipende dal fuso: DataSola restituisce
    // esattamente Year/Month/Day del DateTime ricevuto, qualunque sia il fuso della macchina.
    // Il 1° gennaio e il 31 dicembre a mezzanotte sono i due punti in cui uno spostamento di fuso
    // farebbe cambiare anche l'anno, non solo il giorno: sono i casi che smaschererebbero un
    // eventuale ToLocalTime() rimasto dentro DataSola.
    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2026, 12, 31)]
    [InlineData(2026, 1, 5)]
    [InlineData(2026, 6, 15)]
    public void La_data_sola_non_si_sposta_di_fuso_nemmeno_a_capodanno(int anno, int mese, int giorno)
    {
        var quando = new DateTime(anno, mese, giorno, 0, 0, 0, DateTimeKind.Unspecified);
        var atteso = $"{giorno:D2}/{mese:D2}/{anno:D4}";

        Assert.Equal(atteso, Testi.DataSola(quando));
    }

    // Stesso valore di calendario, Kind diversi: DataSola non guarda il Kind, quindi il risultato
    // non cambia. È la controprova di Data, che invece per costruzione lo fa cambiare tramite
    // ToLocalTime().
    [Fact]
    public void La_data_sola_ignora_il_kind()
    {
        var utc = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        var locale = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Local);
        var atteso = Testi.DataSola(new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Unspecified));

        Assert.Equal(atteso, Testi.DataSola(utc));
        Assert.Equal(atteso, Testi.DataSola(locale));
    }
}
