using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

/// <summary>
/// Una regola di spesa ricorrente: da questa riga la materializzazione genera le occorrenze in
/// 'expenses'. Stessa forma di <see cref="Expense"/>, a cui si rimanda per i motivi non ripetuti
/// qui.
/// </summary>
[Table("recurring_expenses")]
public class RecurringExpense : BaseModel
{
    // ShouldInsert true: l'id lo genera il client, come su Expense.cs — v. lì il perché.
    [PrimaryKey("id", true)] public Guid Id { get; set; }

    // Scrivibili alla creazione, mai più dopo: nemmeno qui il grant UPDATE li comprende. V. Expense.SpaceId.
    [Column("space_id", ignoreOnUpdate: true)] public Guid SpaceId { get; set; }
    [Column("paid_by",  ignoreOnUpdate: true)] public Guid PaidBy  { get; set; }

    [Column("amount")]      public decimal Amount      { get; set; }
    [Column("description")] public string  Description { get; set; } = "";
    [Column("category")]    public string  Category    { get; set; } = "";

    [Column("every_months")] public int EveryMonths { get; set; }
    [Column("day_of_month")] public int DayOfMonth  { get; set; }

    // DateTime e non DateOnly per lo stesso motivo: v. Expense.SpentOn.
    [Column("starts_on")] public DateTime  StartsOn { get; set; }
    [Column("ends_on")]   public DateTime? EndsOn   { get; set; }

    // Il watermark 'yyyy-MM' dell'ultimo periodo materializzato; si confronta come stringa
    // (v. CalcoliRicorrenti.Dovuti).
    [Column("materialized_through")] public string? MaterializedThrough { get; set; }

    // Mai scritte dal client: v. Expense.Version.
    [Column("version",    ignoreOnInsert: true, ignoreOnUpdate: true)] public int      Version   { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime CreatedAt { get; set; }
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }
}
