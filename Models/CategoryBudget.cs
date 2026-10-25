using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

[Table("category_budgets")]
public class CategoryBudget : BaseModel
{
    // L'id nasce sul client, come in Expense.cs.
    [PrimaryKey("id", true)] public Guid Id { get; set; }

    // Il grant permette di deciderli solo in creazione: un budget non cambia identità o periodo iniziale.
    [Column("space_id", ignoreOnUpdate: true)] public Guid SpaceId { get; set; }
    [Column("created_by", ignoreOnUpdate: true)] public Guid CreatedBy { get; set; }
    [Column("category", ignoreOnUpdate: true)] public string Category { get; set; } = "";
    // DateTime e non DateOnly: v. Expense.SpentOn per la catena di serializzazione e trimming.
    [Column("starts_on", ignoreOnUpdate: true)] public DateTime StartsOn { get; set; }
    [Column("amount")] public decimal Amount { get; set; }
    [Column("ends_on")] public DateTime? EndsOn { get; set; }

    // La versione si usa come filtro, mai come valore: la scrive il trigger, come in Expense.cs.
    [Column("version", ignoreOnInsert: true, ignoreOnUpdate: true)] public int Version { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime CreatedAt { get; set; }
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }
}
