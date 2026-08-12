using System.Globalization;
using Eton.Models;

namespace Eton.Services;

/// <summary>Il riepilogo dei voti di un elemento. <c>Mio</c> è null se non hai ancora votato, e va
/// distinto da un voto assente: <c>Mio</c> risponde "che voto hai dato", <c>HaiRecensito</c>
/// risponde "l'hai già provato". Chi ha commentato senza votare ha <c>Mio</c> null ma
/// <c>HaiRecensito</c> vero — un solo campo non basterebbe a rappresentare entrambe le
/// domande.</summary>
public sealed record RiepilogoVoti(decimal? Media, int Voti, decimal? Mio, bool HaiRecensito);

/// <summary>
/// Calcoli puri sui voti delle recensioni: nessun accesso a rete o a stato, solo aggregazione di
/// dati già in memoria. Come <see cref="SchemaCampi"/> e <see cref="ValoriElemento"/>.
/// </summary>
public static class CalcoliVoti
{
    /// <summary>I riepiloghi di tutte le recensioni passate, raggruppati per elemento.</summary>
    public static IReadOnlyDictionary<Guid, RiepilogoVoti> Riepiloghi(IEnumerable<Review> recensioni, Guid? io)
        => recensioni
            .GroupBy(r => r.ItemId)
            .ToDictionary(gruppo => gruppo.Key, gruppo => PerElemento(gruppo, io));

    /// <summary>Il riepilogo di un solo elemento; mai null, anche senza recensioni.</summary>
    public static RiepilogoVoti PerElemento(IEnumerable<Review> recensioni, Guid? io)
    {
        var lista = recensioni as IReadOnlyCollection<Review> ?? recensioni.ToList();

        // Una recensione di solo commento non è un voto: non entra né nella media né nel
        // conteggio, altrimenti includerla come zero abbasserebbe la media di un elemento che
        // nessuno ha giudicato male.
        var voti = lista.Where(r => r.Rating is not null).Select(r => r.Rating!.Value).ToList();

        decimal? media = voti.Count == 0
            ? null
            : Math.Round(voti.Sum() / voti.Count, 1, MidpointRounding.AwayFromZero);

        var mia = io is null ? null : lista.FirstOrDefault(r => r.UserId == io);

        return new RiepilogoVoti(media, voti.Count, mia?.Rating, mia is not null);
    }

    /// <summary>Un voto reso leggibile: "7,5", "8", oppure "—" se non c'è.</summary>
    public static string Testo(decimal? voto)
        // Stesso approccio di ValoriElemento.FormattaNumero: pattern esplicito e
        // CultureInfo.InvariantCulture, poi sostituzione a mano del separatore — con
        // InvariantGlobalization attivo non esistono altre culture da poter impostare.
        => voto is null ? "—" : voto.Value.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',');
}
