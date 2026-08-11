using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

/// <summary>
/// Uno spazio. Quello personale è uno spazio come gli altri con un solo membro e
/// <see cref="IsPersonal"/> a true: non esiste un caso particolare da nessuna parte, solo un
/// indice unico parziale sul database che ne impedisce un secondo per persona.
/// </summary>
[Table("spaces")]
public class Space : BaseModel
{
    [PrimaryKey("id", false)] public Guid Id { get; set; }

    [Column("name")] public string Name { get; set; } = "";

    // Sola lettura dal client: i privilegi concedono l'UPDATE solo su "name". Marcarle qui evita
    // che una Update() le rispedisca indietro facendo fallire l'intera riga per permessi.
    [Column("owner_id", ignoreOnInsert: true, ignoreOnUpdate: true)]    public Guid OwnerId { get; set; }
    [Column("invite_code", ignoreOnInsert: true, ignoreOnUpdate: true)] public string? InviteCode { get; set; }
    [Column("is_personal", ignoreOnInsert: true, ignoreOnUpdate: true)] public bool IsPersonal { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]  public DateTime CreatedAt { get; set; }
}
