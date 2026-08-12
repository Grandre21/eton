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

    /// <summary>Voto al buio: finché non hai messo la tua recensione, su un elemento di questa
    /// collezione non vedi quelle degli altri — solo quante persone hanno recensito.
    /// <para>
    /// Modificabile dopo la creazione, a differenza di <c>SpaceId</c> e <c>OwnerId</c>: è una
    /// regola del gioco, e ci si può ripensare. A farla rispettare non è questa proprietà ma la
    /// policy di SELECT su <c>reviews</c> (v. supabase/migrations/20260812230000_voto_al_buio.sql):
    /// qui serve solo a sapere cosa scrivere a schermo, perché le recensioni nascoste non arrivano
    /// affatto al client e senza questo flag l'interfaccia non saprebbe distinguere "nessuno ha
    /// ancora votato" da "non puoi ancora vedere chi ha votato".
    /// </para>
    /// </summary>
    [Column("blind")] public bool Blind { get; set; }

    // Mai scritte dal client, in nessun momento: version e updated_at le calcola un trigger, e
    // created_at nasce da un default. Marcarle qui evita che una Update() le rispedisca indietro
    // facendo fallire l'intera riga per permessi.
    [Column("version",    ignoreOnInsert: true, ignoreOnUpdate: true)] public int      Version   { get; set; }
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime CreatedAt { get; set; }
    [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)] public DateTime UpdatedAt { get; set; }
}
