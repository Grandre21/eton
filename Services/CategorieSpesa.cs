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
    public static readonly IReadOnlyList<string> Elenco =
    [
        "Spesa", "Casa", "Trasporti", "Ristoranti", "Salute", "Svago", "Abbigliamento",
        "Istruzione", "Regali", "Altro",
    ];

    /// <summary>True se <paramref name="categoria"/> è una di quelle di <see cref="Elenco"/>, a
    /// parità esatta: l'interfaccia mostra sempre uno di questi valori, mai una variante di
    /// maiuscole o spazi. Serve a non mostrare come «sconosciuta» una categoria che invece c'è.</summary>
    public static bool Conosciuta(string? categoria) => categoria is not null && Elenco.Contains(categoria, StringComparer.Ordinal);
}
