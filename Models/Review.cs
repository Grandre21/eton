using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

/// <summary>
/// La recensione di un utente su un elemento: un voto, un commento, o entrambi.
/// </summary>
[Table("reviews")]
public class Review : BaseModel
{
    [PrimaryKey("id", false)] public Guid Id { get; set; }

    // item_id, space_id e user_id sono ignoreOnUpdate per lo stesso motivo delle chiavi esterne di
    // CollectionItem: spostare una recensione su un altro elemento cambierebbe chi ha il diritto di
    // leggerla, e riassegnarne l'autore significherebbe attribuire a qualcuno un'opinione che non
    // ha espresso.
    [Column("item_id",  ignoreOnUpdate: true)] public Guid ItemId { get; set; }
    [Column("space_id", ignoreOnUpdate: true)] public Guid SpaceId { get; set; }
    [Column("user_id",  ignoreOnUpdate: true)] public Guid UserId { get; set; }

    // decimal? e non double?: verificato sul sorgente di Newtonsoft che, per una proprietà
    // tipizzata decimal, il parser legge direttamente dal buffer di caratteri del JSON senza
    // passare da double, e che JsonConvert.ToString(decimal) cabla CultureInfo.InvariantCulture —
    // quindi nessuna perdita di precisione in lettura e nessun rischio di virgola in scrittura. Il
    // comportamento "un decimale diventa Double" vale solo dentro un Dictionary<string, object>,
    // che qui non c'entra: Review ha una proprietà tipizzata, non un campo dentro un jsonb generico.
    [Column("rating")]  public decimal? Rating { get; set; }
    [Column("comment")] public string? Comment { get; set; }

    // Mai scritte dal client, in nessun momento: version e updated_at le calcola un trigger, e
    // created_at nasce da un default. Marcarle qui evita che una Update() le rispedisca indietro
    // facendo fallire l'intera riga per permessi.
    [Column("version",    ignoreOnInsert: true, ignoreOnUpdate: true)] public int      Version   { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime CreatedAt { get; set; }
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }
}
