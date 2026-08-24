using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Models;

/// <summary>
/// Una spesa: importo, descrizione, categoria e data dentro uno spazio. Come le note, non esiste
/// un concetto di visibilità a parte — chi vede lo spazio vede la spesa.
/// </summary>
[Table("expenses")]
public class Expense : BaseModel
{
    // Su Note la chiave è [PrimaryKey("id", false)]: false è ShouldInsert, e significa "il
    // database genera questo valore, non spedirlo in inserimento" (verificato sulla xmldoc di
    // Supabase.Postgrest 4.4.0, PrimaryKeyAttribute.ShouldInsert: "Would be set to false in the
    // event that the database handles the generation of this property" — costruttore
    // PrimaryKeyAttribute(string columnName, bool shouldInsert = false)).
    // Qui è la deroga deliberata della tabella (v. design §3.2): l'id lo genera il CLIENT, con
    // Guid.NewGuid() in ExpenseRepository.CreaAsync, apposta perché un ritentativo dopo una rete
    // che cade porti lo stesso uuid invece di creare una spesa doppia. Serve quindi ShouldInsert
    // true, altrimenti Postgrest ometterebbe id dal corpo dell'INSERT e il default del database
    // (gen_random_uuid()) genererebbe un id diverso a ogni tentativo.
    [PrimaryKey("id", true)] public Guid Id { get; set; }

    // Scrivibili alla creazione, mai più dopo: i privilegi concedono l'UPDATE soltanto su amount,
    // description, category e spent_on. Una spesa non cambia spazio e non cambia pagante.
    [Column("space_id", ignoreOnUpdate: true)] public Guid SpaceId { get; set; }
    [Column("paid_by",  ignoreOnUpdate: true)] public Guid PaidBy  { get; set; }

    [Column("amount")]      public decimal Amount      { get; set; }
    [Column("description")] public string  Description { get; set; } = "";
    [Column("category")]    public string  Category    { get; set; } = "";

    // DateTime e non DateOnly, anche se DateOnly sarebbe il tipo giusto in astratto: DateOnly
    // attraversa Newtonsoft dentro Postgrest, e in Release il trimming è full (v.
    // TrimmerRootAssembly in Eton.csproj e il README). È la categoria di difetto che compila,
    // passa i test, e fallisce solo da pubblicata. DateTime è già dimostrato funzionare lungo
    // questa catena, come su Note e Collection.
    [Column("spent_on")] public DateTime SpentOn { get; set; }

    // Mai scritte dal client, in nessun momento: version e updated_at le calcola un trigger, e
    // created_at nasce da un default. Marcarle qui evita che una Update() le rispedisca indietro
    // facendo fallire l'intera riga per permessi.
    // Su version la marcatura è anche la ragione per cui la concorrenza ottimistica funziona: la
    // versione si USA come filtro, non si SCRIVE.
    [Column("version",    ignoreOnInsert: true, ignoreOnUpdate: true)] public int      Version   { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime CreatedAt { get; set; }
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }
}
