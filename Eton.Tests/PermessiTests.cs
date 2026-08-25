using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class PermessiTests
{
    private static Space Spazio(Guid id, Guid ownerId) => new() { Id = id, OwnerId = ownerId };

    // L'autore interviene sempre sulla propria roba, anche in uno spazio che non possiede: è la
    // metà della regola che non ha bisogno dell'elenco degli spazi per essere vera.
    [Fact]
    public void L_autore_puo_intervenire_anche_se_non_possiede_lo_spazio()
    {
        var mioId = Guid.NewGuid();
        var spazioId = Guid.NewGuid();
        var spazi = new[] { Spazio(spazioId, Guid.NewGuid()) };

        Assert.True(Permessi.PuoIntervenire(mioId, mioId, spazioId, spazi));
    }

    // Il proprietario dello spazio fa pulizia a casa propria, anche su un oggetto che ha creato
    // qualcun altro: è la seconda metà della regola, quella che ha bisogno dell'elenco.
    [Fact]
    public void Il_proprietario_dello_spazio_puo_intervenire_su_un_oggetto_altrui()
    {
        var mioId = Guid.NewGuid();
        var autoreId = Guid.NewGuid();
        var spazioId = Guid.NewGuid();
        var spazi = new[] { Spazio(spazioId, mioId) };

        Assert.True(Permessi.PuoIntervenire(mioId, autoreId, spazioId, spazi));
    }

    // Un terzo membro dello spazio non è né l'autore né il proprietario: nessuna delle due metà
    // della regola lo copre.
    [Fact]
    public void Un_terzo_membro_dello_spazio_non_puo_intervenire()
    {
        var mioId = Guid.NewGuid();
        var autoreId = Guid.NewGuid();
        var spazioId = Guid.NewGuid();
        var spazi = new[] { Spazio(spazioId, Guid.NewGuid()) };

        Assert.False(Permessi.PuoIntervenire(mioId, autoreId, spazioId, spazi));
    }

    // L'invariante "fallisce chiuso": mioId nullo non deve mai dare il permesso, nemmeno quando
    // autoreId coincide per caso con qualcosa — qui non può nemmeno succedere, perché null non è
    // mai uguale a un Guid non nullo, ma il test lo rende esplicito invece di lasciarlo implicito.
    [Fact]
    public void Mio_id_nullo_non_puo_mai_intervenire()
    {
        var autoreId = Guid.NewGuid();
        var spazioId = Guid.NewGuid();
        var spazi = new[] { Spazio(spazioId, Guid.NewGuid()) };

        Assert.False(Permessi.PuoIntervenire(null, autoreId, spazioId, spazi));
    }

    // Il caso che ha già prodotto un difetto: l'elenco degli spazi non ancora caricato non deve
    // dare il permesso a chi non è l'autore, anche se in astratto potrebbe possedere lo spazio.
    // Nel dubbio si mostra di meno, mai di più.
    [Fact]
    public void Elenco_spazi_vuoto_e_autore_diverso_non_puo_intervenire()
    {
        var mioId = Guid.NewGuid();
        var autoreId = Guid.NewGuid();
        var spazioId = Guid.NewGuid();

        Assert.False(Permessi.PuoIntervenire(mioId, autoreId, spazioId, Array.Empty<Space>()));
    }

    // Lo spazio dell'oggetto non compare nell'elenco di chi guarda: non ne fa parte, o l'elenco è
    // ancora incompleto. In entrambi i casi non c'è modo di affermare la proprietà, quindi niente
    // permesso.
    [Fact]
    public void Spazio_assente_dall_elenco_non_puo_intervenire()
    {
        var mioId = Guid.NewGuid();
        var autoreId = Guid.NewGuid();
        var spazioId = Guid.NewGuid();
        var spazi = new[] { Spazio(Guid.NewGuid(), mioId) };

        Assert.False(Permessi.PuoIntervenire(mioId, autoreId, spazioId, spazi));
    }

    // Autore e proprietario coincidono: le due metà della regola sono entrambe vere insieme, non
    // solo alternative.
    [Fact]
    public void Autore_e_proprietario_coincidenti_puo_intervenire()
    {
        var mioId = Guid.NewGuid();
        var spazioId = Guid.NewGuid();
        var spazi = new[] { Spazio(spazioId, mioId) };

        Assert.True(Permessi.PuoIntervenire(mioId, mioId, spazioId, spazi));
    }

    // ---------- Spiegazione ----------

    // Ogni caso dell'enum deve produrre una frase da mostrare: uno switch con un ramo dimenticato
    // restituirebbe una stringa vuota o null solo per quel caso, e lo si scoprirebbe in produzione,
    // sulla schermata di quell'unico oggetto.
    [Theory]
    [InlineData(Permessi.Oggetto.Nota)]
    [InlineData(Permessi.Oggetto.Collezione)]
    [InlineData(Permessi.Oggetto.Elemento)]
    [InlineData(Permessi.Oggetto.Spesa)]
    public void Ogni_oggetto_produce_una_frase_non_vuota(Permessi.Oggetto oggetto)
        => Assert.False(string.IsNullOrWhiteSpace(Permessi.Spiegazione(oggetto)));

    // Il difetto tipico di uno switch scritto male: due rami che restituiscono lo stesso testo per
    // errore, invisibile a occhio perché ogni schermata mostra solo la propria frase e nessuno le
    // confronta fianco a fianco.
    [Fact]
    public void Le_quattro_frasi_sono_tutte_diverse()
    {
        var frasi = new[]
        {
            Permessi.Spiegazione(Permessi.Oggetto.Nota),
            Permessi.Spiegazione(Permessi.Oggetto.Collezione),
            Permessi.Spiegazione(Permessi.Oggetto.Elemento),
            Permessi.Spiegazione(Permessi.Oggetto.Spesa),
        };

        Assert.Equal(frasi.Length, frasi.Distinct().Count());
    }

    // La frase della spesa era già in uso, testuale, in SpesaEdit.razor: questo test è il presidio
    // che impedisce a un futuro "miglioramento" di riformularla senza che nessuno se ne accorga.
    [Fact]
    public void La_frase_della_spesa_e_quella_storica()
        => Assert.Equal(
            "Questa spesa l'ha segnata qualcun altro: può modificarla o cancellarla solo chi l'ha pagata, o chi possiede lo spazio.",
            Permessi.Spiegazione(Permessi.Oggetto.Spesa));
}
