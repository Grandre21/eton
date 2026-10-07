namespace Eton.Services;

public enum TipoColonna { Testo, Numero, Data, Selezione, Denaro }

[Flags]
public enum Aggregazione { Nessuna = 0, Somma = 1, Media = 2, Min = 4, Max = 8, Conteggio = 16 }

/// <summary>Chiave stabile (anche nell'URL), etichetta, tipo e la funzione che legge il valore dalla
/// riga. <c>Valore</c> è una funzione e non un nome di proprietà: è il gancio della fase 4
/// (spec §2.2).</summary>
public sealed record ColonnaGriglia<T>(string Chiave, string Etichetta, TipoColonna Tipo, Func<T, object?> Valore)
{
    public Aggregazione Aggregazioni { get; init; }
    public bool Ordinabile { get; init; } = true;
    public Func<T, bool>? Modificabile { get; init; }        // null = colonna mai modificabile
    public IReadOnlyList<string> Opzioni { get; init; } = []; // per Selezione
}

/// <summary>Un blocco di righe con la sua etichetta; <c>Etichetta</c> null = nessun raggruppamento.</summary>
public sealed record Gruppo<T>(string? Etichetta, IReadOnlyList<T> Righe);

/// <summary>Le cinque aggregazioni di una colonna numerica. Media, Min e Max sono null a zero righe.</summary>
public sealed record Riepilogo(int Conteggio, decimal Somma, decimal? Media, decimal? Min, decimal? Max);
