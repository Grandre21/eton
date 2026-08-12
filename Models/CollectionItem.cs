using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

/// <summary>
/// Un elemento dentro una collezione, coi valori dei campi definiti da <see cref="Collection.Fields"/>.
/// </summary>
[Table("collection_items")]
public class CollectionItem : BaseModel
{
    [PrimaryKey("id", false)] public Guid Id { get; set; }

    // space_id e collection_id fanno parte della chiave esterna composita: renderli scrivibili
    // permetterebbe di spostare un elemento in un'altra collezione, cioè di cambiare chi ha il
    // diritto di leggerlo.
    [Column("collection_id", ignoreOnUpdate: true)] public Guid CollectionId { get; set; }
    [Column("space_id",      ignoreOnUpdate: true)] public Guid SpaceId { get; set; }
    [Column("added_by",      ignoreOnUpdate: true)] public Guid AddedBy { get; set; }

    [Column("name")]      public string Name { get; set; } = "";
    [Column("image_url")] public string? ImageUrl { get; set; }

    [Column("data")] public Dictionary<string, object> Data { get; set; } = new();

    // Mai scritte dal client, in nessun momento: version e updated_at le calcola un trigger, e
    // created_at nasce da un default. Marcarle qui evita che una Update() le rispedisca indietro
    // facendo fallire l'intera riga per permessi.
    [Column("version",    ignoreOnInsert: true, ignoreOnUpdate: true)] public int      Version   { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime CreatedAt { get; set; }
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }
}
