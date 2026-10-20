using Eton.Models;

namespace Eton.Services;

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
    public static readonly IReadOnlyList<string> Tutte = [.. Elenco.Union(Entrate)];

    public static IReadOnlyList<string> Di(string tipo) => tipo == TipoMovimento.Entrata ? Entrate : Elenco;

    /// <summary>Una categoria si accetta se appartiene all'elenco del proprio tipo, a parità esatta.</summary>
    public static bool Conosciuta(string? categoria, string tipo) => categoria is not null && Di(tipo).Contains(categoria, StringComparer.Ordinal);
}
