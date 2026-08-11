using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

[Table("profiles")]
public class Profile : BaseModel
{
    [PrimaryKey("id", false)] public Guid Id { get; set; }

    [Column("display_name")] public string? DisplayName { get; set; }
    [Column("avatar_url")]   public string? AvatarUrl { get; set; }

    // Sola lettura dal client, come created_at su Space: i privilegi concedono l'UPDATE soltanto
    // su display_name e avatar_url. Senza questa marcatura una Update() rispedirebbe updated_at e
    // l'intera riga fallirebbe per permessi, non silenziosamente ma con un errore poco leggibile.
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }
}
