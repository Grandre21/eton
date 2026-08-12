using Newtonsoft.Json;

namespace Eton.Models;

/// <summary>
/// La definizione di un campo dentro <c>collections.fields</c>. Non è un BaseModel e non usa
/// <c>[Column]</c>: vive dentro un jsonb, non in colonne sue.
/// <para>
/// Gli attributi <c>[JsonProperty]</c> NON sono decorativi. Il resolver di Supabase.Postgrest è un
/// DefaultContractResolver senza naming strategy: gestisce il nome della colonna per le proprietà
/// del BaseModel, ma dentro un tipo annidato non interviene affatto. Senza questi attributi le
/// chiavi finirebbero nel jsonb in PascalCase — "Key" invece di "key" — e lo schema concordato col
/// database sarebbe sbagliato senza che niente lo segnali.
/// </para>
/// </summary>
public class CampoDefinizione
{
    [JsonProperty("key")]   public string Key { get; set; } = "";
    [JsonProperty("label")] public string Label { get; set; } = "";
    [JsonProperty("type")]  public string Type { get; set; } = "text";

    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Options { get; set; }

    [JsonProperty("order")] public int Order { get; set; }
}
