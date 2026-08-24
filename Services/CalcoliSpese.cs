using Eton.Models;

namespace Eton.Services;

/// <summary>Il totale di una categoria dentro un mese, con la sua quota percentuale sul totale del
/// mese — serve alla larghezza delle barre del registro.</summary>
public sealed record VoceCategoria(string Categoria, decimal Totale, int Quota);

/// <summary>Il riepilogo di un mese: quanto si è speso, in cosa, e il confronto col mese prima.
/// <c>VariazionePercentuale</c> è null quando il mese precedente è a zero — è il primo mese, non
/// una variazione dello 0%.</summary>
public sealed record RiepilogoMese(
    decimal Totale,
    IReadOnlyList<VoceCategoria> Categorie,
    decimal TotalePrecedente,
    int? VariazionePercentuale);

/// <summary>
/// Calcoli puri sulle spese: nessun accesso a rete o a stato, solo aggregazione di dati già in
/// memoria. Come <see cref="CalcoliVoti"/>.
/// </summary>
public static class CalcoliSpese
{
    // DateTime.ToString("MMMM") sotto InvariantGlobalization restituisce "August", non "agosto":
    // non esiste una cultura it-IT da poter impostare (v. CalcoliVoti.Testo per lo stesso problema
    // sui numeri). I nomi restano qui, e non in una pagina, perché la stessa tabella serve sia al
    // riepilogo del mese sia alle frecce ◀ ▶ per scorrerli.
    private static readonly string[] NomiMesi =
    [
        "gennaio", "febbraio", "marzo", "aprile", "maggio", "giugno",
        "luglio", "agosto", "settembre", "ottobre", "novembre", "dicembre"
    ];

    /// <summary>Il nome del mese in italiano, da 1 a 12. Un numero fuori intervallo non lancia:
    /// restituisce una stringa vuota, perché un mese sbagliato è un errore di chi chiama e non un
    /// motivo per far cadere una pagina che sta solo mostrando un'intestazione.</summary>
    public static string NomeMese(int mese) => mese is >= 1 and <= 12 ? NomiMesi[mese - 1] : "";

    /// <summary>Il riepilogo di un mese: totale, ripartizione per categoria e confronto col mese
    /// precedente. <paramref name="spese"/> deve contenere anche le spese del mese precedente,
    /// perché il confronto si calcola dalla stessa raccolta — è per questo che
    /// <see cref="ExpenseRepository.ElencaAsync"/> scarica due mesi in un colpo solo.</summary>
    public static RiepilogoMese PerMese(IEnumerable<Expense> spese, int anno, int mese)
    {
        var lista = spese as IReadOnlyCollection<Expense> ?? spese.ToList();

        var delMese = lista.Where(s => s.SpentOn.Year == anno && s.SpentOn.Month == mese).ToList();
        var totale = delMese.Sum(s => s.Amount);

        var mesePrecedente = mese == 1 ? 12 : mese - 1;
        var annoPrecedente = mese == 1 ? anno - 1 : anno;
        var totalePrecedente = lista
            .Where(s => s.SpentOn.Year == annoPrecedente && s.SpentOn.Month == mesePrecedente)
            .Sum(s => s.Amount);

        int? variazione = totalePrecedente == 0
            ? null
            : (int)Math.Round((totale - totalePrecedente) / totalePrecedente * 100, MidpointRounding.AwayFromZero);

        var categorie = delMese
            .GroupBy(s => s.Category)
            .Select(g => (Categoria: g.Key, Totale: g.Sum(s => s.Amount)))
            .OrderByDescending(c => c.Totale)
            .ToList();

        return new RiepilogoMese(totale, ConQuote(categorie, totale), totalePrecedente, variazione);
    }

    /// <summary>Assegna a ogni categoria una quota intera, con la garanzia che la somma faccia
    /// esattamente 100 quando c'è almeno una spesa. Arrotondare ogni quota per conto proprio dà 99
    /// o 101 — tre categorie da un terzo ciascuna arrotondano tutte a 33, e il totale non torna.
    /// <para>
    /// Metodo del resto maggiore: si arrotonda ogni quota per difetto, poi si distribuisce un punto
    /// alla volta alle categorie con il resto più grande, finché il totale non torna a 100.
    /// </para>
    /// </summary>
    private static List<VoceCategoria> ConQuote(List<(string Categoria, decimal Totale)> categorie, decimal totale)
    {
        if (categorie.Count == 0 || totale == 0)
            return categorie.Select(c => new VoceCategoria(c.Categoria, c.Totale, 0)).ToList();

        var quote = categorie
            .Select(c =>
            {
                var esatta = c.Totale / totale * 100;
                var perDifetto = (int)Math.Floor(esatta);
                return (c.Categoria, c.Totale, PerDifetto: perDifetto, Resto: esatta - perDifetto);
            })
            .ToList();

        var mancanti = 100 - quote.Sum(q => q.PerDifetto);

        var daAumentare = quote
            .Select((q, indice) => (q.Categoria, Indice: indice, q.Resto))
            .OrderByDescending(q => q.Resto)
            .Take(mancanti)
            .Select(q => q.Indice)
            .ToHashSet();

        return quote
            .Select((q, indice) => new VoceCategoria(q.Categoria, q.Totale, q.PerDifetto + (daAumentare.Contains(indice) ? 1 : 0)))
            .ToList();
    }
}
