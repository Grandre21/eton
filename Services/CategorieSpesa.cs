namespace Eton.Services;

/// <summary>I due tipi di movimento, con i valori della colonna <c>kind</c> (check del database).</summary>
public static class TipoMovimento
{
    public const string Spesa = "expense", Entrata = "income";

    /// <summary>«Spesa» o «Entrata», per la colonna e il filtro della tabella e per gli editor.</summary>
    public static string Testo(string tipo) => tipo == Entrata ? "Entrata" : "Spesa";
}

/// <summary>
/// L'elenco chiuso delle categorie di spesa, offerto dall'interfaccia a pastiglie.
/// <para>
/// Il database (<c>expenses.category</c>) accetta qualunque testo: un <c>check</c> contro una
/// lista fisserebbe nello schema una scelta che qui costa una riga di C#. Aggiungere «Animali»
/// domani è una riga in questo file, non una migration da incollare a mano nel SQL Editor di
/// produzione. I dati restano puliti lo stesso perché a scrivere la categoria non è mai la
/// tastiera: la sceglie sempre un'interfaccia a pastiglie. E se un domani servissero categorie
/// definite dall'utente, sono una tabella in più, senza nessuna migrazione dei dati esistenti.
/// </para>
/// </summary>
public static class CategorieSpesa
{
    // Le categorie delle spese, offerte dall'interfaccia a pastiglie.
    public static readonly IReadOnlyList<string> Elenco =
    [
        "Spesa", "Casa", "Trasporti", "Ristoranti", "Salute", "Svago", "Abbigliamento",
        "Istruzione", "Regali", "Altro",
    ];

    public static readonly IReadOnlyList<string> Entrate = ["Stipendio", "Rimborso", "Regalo", "Vendita", "Altro"];
    public static readonly IReadOnlyList<string> Tutte = [.. Elenco, .. Entrate.Where(c => !Elenco.Contains(c))];

    public static IReadOnlyList<string> Di(string tipo) => tipo == TipoMovimento.Entrata ? Entrate : Elenco;

    /// <summary>Una categoria si accetta se appartiene all'elenco del proprio tipo, a parità esatta.</summary>
    public static bool Conosciuta(string? categoria, string tipo) => categoria is not null && Di(tipo).Contains(categoria, StringComparer.Ordinal);
}
