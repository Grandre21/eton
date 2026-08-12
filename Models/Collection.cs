using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

/// <summary>
/// Una collezione: uno schema di campi scelto dall'utente, dentro uno spazio.
/// </summary>
[Table("collections")]
public class Collection : BaseModel
{
    [PrimaryKey("id", false)] public Guid Id { get; set; }

    // Scrivibili alla creazione, mai più dopo: una collezione non cambia spazio e non cambia
    // proprietario.
    [Column("space_id", ignoreOnUpdate: true)] public Guid SpaceId { get; set; }
    [Column("owner_id", ignoreOnUpdate: true)] public Guid OwnerId { get; set; }

    [Column("name")] public string Name { get; set; } = "";
    [Column("icon")] public string? Icon { get; set; }

    // Dichiarato a tipo forte e non come stringa: il client lo serializza come array JSON
    // annidato, che è ciò che jsonb si aspetta. Una stringa gestita a mano finirebbe in colonna
    // come testo JSON escapato.
    [Column("fields")] public List<CampoDefinizione> Fields { get; set; } = new();

    [Column("rating_max")] public short RatingMax { get; set; } = 10;

    // Mai scritte dal client, in nessun momento: version e updated_at le calcola un trigger, e
    // created_at nasce da un default. Marcarle qui evita che una Update() le rispedisca indietro
    // facendo fallire l'intera riga per permessi.
    [Column("version",    ignoreOnInsert: true, ignoreOnUpdate: true)] public int      Version   { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime CreatedAt { get; set; }
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }
}
