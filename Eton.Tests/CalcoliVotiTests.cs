using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class CalcoliVotiTests
{
    private static Review Recensione(Guid itemId, Guid userId, decimal? voto, string? commento = null)
        => new() { ItemId = itemId, UserId = userId, Rating = voto, Comment = commento };

    // ---------- PerElemento ----------

    [Fact]
    public void Nessuna_recensione_produce_un_riepilogo_vuoto()
    {
        var riepilogo = CalcoliVoti.PerElemento([], null);

        Assert.Null(riepilogo.Media);
        Assert.Equal(0, riepilogo.Voti);
        Assert.Null(riepilogo.Mio);
        Assert.False(riepilogo.HaiRecensito);
    }

    [Fact]
    public void Recensioni_di_solo_commento_non_contano_ne_in_media_ne_nel_conteggio()
    {
        // Una recensione senza voto non è un voto: includerla come zero abbasserebbe la media di
        // un elemento che nessuno ha giudicato male.
        var elemento = Guid.NewGuid();
        var recensioni = new[]
        {
            Recensione(elemento, Guid.NewGuid(), null, "Bello"),
            Recensione(elemento, Guid.NewGuid(), null, "Carino"),
        };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, null);

        Assert.Null(riepilogo.Media);
        Assert.Equal(0, riepilogo.Voti);
    }

    [Fact]
    public void Voti_e_commenti_misti_contano_solo_i_voti()
    {
        var elemento = Guid.NewGuid();
        var recensioni = new[]
        {
            Recensione(elemento, Guid.NewGuid(), 8m, "Ottimo"),
            Recensione(elemento, Guid.NewGuid(), null, "Solo un commento"),
        };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, null);

        Assert.Equal(8m, riepilogo.Media);
        Assert.Equal(1, riepilogo.Voti);
    }

    [Fact]
    public void La_media_di_due_voti_si_arrotonda_a_una_cifra_decimale()
    {
        var elemento = Guid.NewGuid();
        var recensioni = new[]
        {
            Recensione(elemento, Guid.NewGuid(), 7m),
            Recensione(elemento, Guid.NewGuid(), 8m),
        };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, null);

        Assert.Equal(7.5m, riepilogo.Media);
        Assert.Equal(2, riepilogo.Voti);
    }

    [Fact]
    public void La_media_esattamente_a_meta_si_arrotonda_per_eccesso()
    {
        // 7 + 7 + 7 + 8 = 29 / 4 = 7,25: esattamente a metà fra 7,2 e 7,3, che è l'unico caso in cui
        // la strategia di arrotondamento si vede. MidpointRounding.AwayFromZero dà 7,3; il default
        // di Math.Round è ToEven e darebbe 7,2, perché 2 è pari.
        //
        // Questa prova esiste perché il test qui sotto, da solo, non basta: 7,6666 non è un punto
        // medio ed è più vicino a 7,7 con qualunque strategia, quindi passerebbe identico anche il
        // giorno in cui qualcuno togliesse MidpointRounding.AwayFromZero credendolo superfluo.
        var elemento = Guid.NewGuid();
        var recensioni = new[]
        {
            Recensione(elemento, Guid.NewGuid(), 7m),
            Recensione(elemento, Guid.NewGuid(), 7m),
            Recensione(elemento, Guid.NewGuid(), 7m),
            Recensione(elemento, Guid.NewGuid(), 8m),
        };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, null);

        Assert.Equal(7.3m, riepilogo.Media);
        Assert.Equal(4, riepilogo.Voti);
    }

    [Fact]
    public void La_media_di_tre_voti_si_arrotonda_a_una_cifra()
    {
        // 7 + 8 + 8 = 23 / 3 = 7,6666... -> 7,7.
        var elemento = Guid.NewGuid();
        var recensioni = new[]
        {
            Recensione(elemento, Guid.NewGuid(), 7m),
            Recensione(elemento, Guid.NewGuid(), 8m),
            Recensione(elemento, Guid.NewGuid(), 8m),
        };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, null);

        Assert.Equal(7.7m, riepilogo.Media);
        Assert.Equal(3, riepilogo.Voti);
    }

    [Fact]
    public void Con_io_nullo_mio_e_sempre_nullo()
    {
        var elemento = Guid.NewGuid();
        var recensioni = new[] { Recensione(elemento, Guid.NewGuid(), 8m) };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, null);

        Assert.Null(riepilogo.Mio);
        Assert.False(riepilogo.HaiRecensito);
    }

    [Fact]
    public void Io_che_non_ha_recensito_ha_mio_nullo_e_hai_recensito_falso()
    {
        var elemento = Guid.NewGuid();
        var io = Guid.NewGuid();
        var recensioni = new[] { Recensione(elemento, Guid.NewGuid(), 8m) };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, io);

        Assert.Null(riepilogo.Mio);
        Assert.False(riepilogo.HaiRecensito);
    }

    [Fact]
    public void Io_che_ha_solo_commentato_ha_mio_nullo_ma_hai_recensito_vero()
    {
        // Mio e HaiRecensito sono due campi distinti apposta: chi ha commentato senza votare ha
        // comunque già provato l'elemento, e il filtro "da provare" deve escluderlo — un solo
        // campo "Mio" non basterebbe a distinguere "non ha votato" da "non ha mai recensito".
        var elemento = Guid.NewGuid();
        var io = Guid.NewGuid();
        var recensioni = new[] { Recensione(elemento, io, null, "L'ho provato ma non lo voto") };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, io);

        Assert.Null(riepilogo.Mio);
        Assert.True(riepilogo.HaiRecensito);
    }

    [Fact]
    public void Io_che_ha_votato_ha_il_proprio_voto_in_mio()
    {
        var elemento = Guid.NewGuid();
        var io = Guid.NewGuid();
        var recensioni = new[]
        {
            Recensione(elemento, io, 6m),
            Recensione(elemento, Guid.NewGuid(), 9m),
        };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, io);

        Assert.Equal(6m, riepilogo.Mio);
        Assert.True(riepilogo.HaiRecensito);
    }

    // ---------- Riepiloghi ----------

    [Fact]
    public void Riepiloghi_raggruppa_per_elemento()
    {
        var primo = Guid.NewGuid();
        var secondo = Guid.NewGuid();
        var io = Guid.NewGuid();
        var recensioni = new[]
        {
            Recensione(primo, io, 7m),
            Recensione(primo, Guid.NewGuid(), 9m),
            Recensione(secondo, Guid.NewGuid(), 5m),
        };

        var riepiloghi = CalcoliVoti.Riepiloghi(recensioni, io);

        Assert.Equal(2, riepiloghi.Count);
        Assert.Equal(8m, riepiloghi[primo].Media);
        Assert.Equal(7m, riepiloghi[primo].Mio);
        Assert.Equal(5m, riepiloghi[secondo].Media);
        Assert.Null(riepiloghi[secondo].Mio);
    }

    // ---------- Testo ----------

    [Fact]
    public void Un_voto_nullo_si_mostra_come_trattino()
        => Assert.Equal("—", CalcoliVoti.Testo(null));

    [Fact]
    public void Un_voto_intero_non_mostra_lo_zero_decimale()
        => Assert.Equal("8", CalcoliVoti.Testo(8.0m));

    [Fact]
    public void Un_voto_con_mezzo_punto_si_mostra_con_la_virgola()
        => Assert.Equal("7,5", CalcoliVoti.Testo(7.5m));

    // I due estremi ammessi dal vincolo check (rating > 0 and rating <= 10) della migrazione.
    // Il massimo è l'unico caso a due cifre intere: un troncamento sbagliato lo ridurrebbe a "1", e
    // uno zero decimale di troppo lo mostrerebbe come "10,0". Nessun altro test li tocca.
    [Fact]
    public void Il_voto_minimo_si_mostra_con_la_virgola()
        => Assert.Equal("0,5", CalcoliVoti.Testo(0.5m));

    [Fact]
    public void Il_voto_massimo_si_mostra_senza_decimali()
        => Assert.Equal("10", CalcoliVoti.Testo(10.0m));

    // ---------- TestoVoti ----------

    // Le tre forme compaiono nell'intestazione di un elemento e nell'elenco di una collezione: se
    // una di esse cambia senza volerlo, il singolare sbagliato compare in entrambi i punti insieme.
    [Fact]
    public void Nessun_votante_si_dice_a_parole_e_non_con_uno_zero()
        => Assert.Equal("nessun voto", CalcoliVoti.TestoVoti(0));

    [Fact]
    public void Un_solo_votante_va_al_singolare()
        => Assert.Equal("1 voto", CalcoliVoti.TestoVoti(1));

    [Fact]
    public void Piu_votanti_vanno_al_plurale()
        => Assert.Equal("4 voti", CalcoliVoti.TestoVoti(4));

    // ---------- TestoRecensioni ----------

    // Conta una cosa diversa da TestoVoti — le recensioni, comprese quelle di solo commento — e i
    // due non vanno confusi: dove si mostra un conteggio di righe la parola deve essere "recensioni",
    // altrimenti lo stesso elemento dichiara "2 voti" da coperto e "1 voto, 2 commenti" da scoperto.
    [Fact]
    public void Nessuna_recensione_si_dice_a_parole_e_non_con_uno_zero()
        => Assert.Equal("nessuna recensione", CalcoliVoti.TestoRecensioni(0));

    [Fact]
    public void Una_sola_recensione_va_al_singolare()
        => Assert.Equal("1 recensione", CalcoliVoti.TestoRecensioni(1));

    [Fact]
    public void Piu_recensioni_vanno_al_plurale()
        => Assert.Equal("4 recensioni", CalcoliVoti.TestoRecensioni(4));

    // ---------- comportamento su dati che il database non permette ----------

    [Fact]
    public void Due_recensioni_dello_stesso_utente_si_sommano_senza_lanciare()
    {
        // Il vincolo unique (item_id, user_id) rende questo caso irraggiungibile passando dai
        // repository: due recensioni della stessa persona sullo stesso elemento non possono esistere.
        // Ma CalcoliVoti è una funzione pura che accetta qualunque lista, e un domani potrebbe
        // riceverne una montata a mano o unita da due letture diverse.
        //
        // Questo test NON dice che sommarle sia il comportamento giusto — gonfia la media. Dice che
        // oggi è quello che succede, e che la funzione non lancia: se qualcuno introducesse una
        // deduplica, o al contrario un'eccezione, se ne accorgerebbe qui invece che in produzione.
        var elemento = Guid.NewGuid();
        var io = Guid.NewGuid();
        var recensioni = new[]
        {
            Recensione(elemento, io, 6m),
            Recensione(elemento, io, 9m),
        };

        var riepilogo = CalcoliVoti.PerElemento(recensioni, io);

        Assert.Equal(7.5m, riepilogo.Media);
        Assert.Equal(2, riepilogo.Voti);
        Assert.Equal(6m, riepilogo.Mio);        // il primo trovato, non l'ultimo
        Assert.True(riepilogo.HaiRecensito);
    }
}
