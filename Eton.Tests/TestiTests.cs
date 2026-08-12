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
}
