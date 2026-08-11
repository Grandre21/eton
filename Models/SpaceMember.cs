using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

/// <summary>
/// L'iscrizione di un utente a uno spazio.
/// <para>
/// Non viene MAI scritto tramite Postgrest, e per questo nessuna colonna porta
/// <c>ignoreOnInsert</c>/<c>ignoreOnUpdate</c>: non c'è nulla da ignorare, perché non esiste
/// alcun GRANT di INSERT o UPDATE su questa tabella. Una membership si crea solo con
/// <c>create_space()</c> o <c>join_space()</c>, e si toglie con una DELETE.
/// </para>
/// </summary>
[Table("space_members")]
public class SpaceMember : BaseModel
{
    [PrimaryKey("id", false)] public Guid Id { get; set; }

    [Column("space_id")]  public Guid SpaceId { get; set; }
    [Column("user_id")]   public Guid UserId { get; set; }
    [Column("joined_at")] public DateTime JoinedAt { get; set; }
}
