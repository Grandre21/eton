using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

/// <summary>
/// Una nota: Markdown dentro uno spazio. Non esiste la "nota personale" come caso a parte — è una
/// nota in uno spazio personale, che è a sua volta uno spazio con un membro solo.
/// </summary>
[Table("notes")]
public class Note : BaseModel
{
    [PrimaryKey("id", false)] public Guid Id { get; set; }

    // Scrivibili alla creazione, mai più dopo: i privilegi concedono l'UPDATE soltanto su title e
    // body. Una nota non cambia spazio e non cambia autore.
    [Column("space_id", ignoreOnUpdate: true)] public Guid SpaceId { get; set; }
    [Column("owner_id", ignoreOnUpdate: true)] public Guid OwnerId { get; set; }

    [Column("title")] public string Title { get; set; } = "";
    [Column("body")]  public string Body  { get; set; } = "";

    // Mai scritte dal client, in nessun momento: version e updated_at le calcola un trigger, e
    // created_at nasce da un default. Marcarle qui evita che una Update() le rispedisca indietro
    // facendo fallire l'intera riga per permessi.
    // Su version la marcatura è anche la ragione per cui la concorrenza ottimistica funziona: la
    // versione si USA come filtro, non si SCRIVE.
    [Column("version",    ignoreOnInsert: true, ignoreOnUpdate: true)] public int      Version   { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime CreatedAt { get; set; }
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }

    /// <summary>Il titolo da mostrare, con il ripiego per le note che non ne hanno uno.
    /// <para>
    /// È un metodo e non una proprietà di proposito: le proprietà di un BaseModel le guarda il
    /// serializzatore di Postgrest, i metodi no. Così non c'è modo di spedire per sbaglio al
    /// database una colonna che lì non esiste.
    /// </para>
    /// <para>
    /// La regola sta nel modello e non nelle pagine perché è una regola sola. Oggi è una costante,
    /// ma se un domani il titolo mancante lo si volesse ricavare dalla prima riga del corpo, il
    /// punto da cambiare deve essere uno.
    /// </para>
    /// </summary>
    public string TitoloVisibile() => string.IsNullOrWhiteSpace(Title) ? "Senza titolo" : Title;
}
