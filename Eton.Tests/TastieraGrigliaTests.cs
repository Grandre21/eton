using Eton.Services;

namespace Eton.Tests;

public class TastieraGrigliaTests
{
    private static AzioneTasto Fuori(string tasto, bool shift = false, bool ctrl = false, TipoColonna tipo = TipoColonna.Testo)
        => TastieraGriglia.Interpreta(tasto, shift, ctrl, inModifica: false, tipo);

    private static AzioneTasto Dentro(string tasto, bool shift = false)
        => TastieraGriglia.Interpreta(tasto, shift, false, inModifica: true, TipoColonna.Testo);

    [Fact]
    public void Fuori_modifica_le_frecce_spostano_la_cella()
    {
        Assert.Equal(AzioneTasto.Su, Fuori("ArrowUp"));
        Assert.Equal(AzioneTasto.Giu, Fuori("ArrowDown"));
        Assert.Equal(AzioneTasto.Sinistra, Fuori("ArrowLeft"));
        Assert.Equal(AzioneTasto.Destra, Fuori("ArrowRight"));
    }

    [Fact]
    public void Fuori_modifica_Invio_e_F2_entrano_in_modifica()
    {
        Assert.Equal(AzioneTasto.Modifica, Fuori("Enter"));
        Assert.Equal(AzioneTasto.Modifica, Fuori("F2"));
    }

    [Fact]
    public void Un_carattere_entra_sostituendo_ma_non_con_ctrl_e_non_su_data_o_menu()
    {
        Assert.Equal(AzioneTasto.ModificaSostituendo, Fuori("7", tipo: TipoColonna.Denaro));
        Assert.Equal(AzioneTasto.ModificaSostituendo, Fuori("è"));
        Assert.Equal(AzioneTasto.Nessuna, Fuori("c", ctrl: true));        // Ctrl+C resta del browser
        // Su una data o un menù "sostituire con un carattere" non ha senso: si entra e basta.
        Assert.Equal(AzioneTasto.Modifica, Fuori("2", tipo: TipoColonna.Data));
        Assert.Equal(AzioneTasto.Modifica, Fuori("C", tipo: TipoColonna.Selezione));
    }

    [Fact]
    public void Con_un_modificatore_frecce_Invio_e_F2_restano_del_browser()
    {
        Assert.Equal(AzioneTasto.Nessuna, Fuori("ArrowLeft", ctrl: true));   // Alt+Freccia: indietro
        Assert.Equal(AzioneTasto.Nessuna, Fuori("Enter", ctrl: true));
        Assert.Equal(AzioneTasto.Nessuna, Fuori("F2", ctrl: true));
        Assert.Equal(AzioneTasto.Giu, Fuori("ArrowDown", shift: true));       // Shift non è un modificatore qui
    }

    [Fact]
    public void Fuori_modifica_Spazio_seleziona_la_riga()
    {
        Assert.Equal(AzioneTasto.SelezionaRiga, Fuori(" "));
        Assert.Equal(AzioneTasto.Nessuna, Fuori("Escape"));
    }

    [Fact]
    public void In_modifica_Invio_Tab_ShiftTab_ed_Esc()
    {
        Assert.Equal(AzioneTasto.SalvaEGiu, Dentro("Enter"));
        Assert.Equal(AzioneTasto.SalvaEDestra, Dentro("Tab"));
        Assert.Equal(AzioneTasto.SalvaESinistra, Dentro("Tab", shift: true));
        Assert.Equal(AzioneTasto.Annulla, Dentro("Escape"));
    }

    [Fact]
    public void In_modifica_frecce_e_caratteri_restano_al_campo()
    {
        Assert.Equal(AzioneTasto.Nessuna, Dentro("ArrowLeft"));
        Assert.Equal(AzioneTasto.Nessuna, Dentro("a"));
        Assert.Equal(AzioneTasto.Nessuna, Dentro(" "));
    }

    [Fact]
    public void Lo_spostamento_si_ferma_ai_bordi()
    {
        Assert.Equal((0, 0), TastieraGriglia.Sposta((0, 0), AzioneTasto.Su, righe: 3, colonne: 4));
        Assert.Equal((0, 0), TastieraGriglia.Sposta((0, 0), AzioneTasto.Sinistra, 3, 4));
        Assert.Equal((2, 3), TastieraGriglia.Sposta((2, 3), AzioneTasto.Giu, 3, 4));
        Assert.Equal((2, 3), TastieraGriglia.Sposta((2, 3), AzioneTasto.SalvaEDestra, 3, 4));
        Assert.Equal((2, 1), TastieraGriglia.Sposta((1, 1), AzioneTasto.SalvaEGiu, 3, 4));
        Assert.Equal((1, 0), TastieraGriglia.Sposta((1, 1), AzioneTasto.SalvaESinistra, 3, 4));
    }

    [Fact]
    public void Dopo_un_ridisegno_la_cella_attiva_si_ritrova_per_chiave()
    {
        // ancora presente, anche se si è spostata: resta lei
        Assert.Equal("b", TastieraGriglia.Riallinea(["c", "a", "b"], "b", indicePrecedente: 1));
        // sparita (filtrata o eliminata): la riga che ora occupa lo stesso posto
        Assert.Equal("c", TastieraGriglia.Riallinea(["a", "c"], "b", indicePrecedente: 1));
        // sparita ed era l'ultima: la nuova ultima
        Assert.Equal("c", TastieraGriglia.Riallinea(["a", "c"], "z", indicePrecedente: 5));
        // tabella vuota: nessuna cella attiva
        Assert.Null(TastieraGriglia.Riallinea([], "b", indicePrecedente: 0));
    }
}
