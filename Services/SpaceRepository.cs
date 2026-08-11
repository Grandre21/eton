using Eton.Models;
using Supabase.Postgrest;

namespace Eton.Services;

/// <summary>Un membro di uno spazio, già unito al suo profilo: è la forma che serve alle pagine.</summary>
public sealed record Membro(Guid UtenteId, string Nome, string? Avatar, bool EProprietario);

/// <summary>
/// Accesso agli spazi. Ogni metodo riparte da <see cref="SupabaseService.GetClientAsync"/> e non
/// tiene mai il client in un campo: è quella chiamata a garantire un token vivo prima di ogni
/// operazione (v. il commento di <see cref="SupabaseClient"/>).
/// <para>
/// Qui non c'è alcun controllo di autorizzazione, e non è una dimenticanza: filtrare per utente
/// lato client sarebbe teatro, perché chiunque può interrogare PostgREST direttamente con la
/// chiave anon, che è pubblica. A decidere chi vede cosa sono le policy RLS. Le query qui sotto
/// chiedono "tutti gli spazi" proprio perché il database restituisce già solo i propri.
/// </para>
/// </summary>
public class SpaceRepository
{
    private readonly SupabaseService _supabase;

    public SpaceRepository(SupabaseService supabase) => _supabase = supabase;

    /// <summary>Gli spazi di cui l'utente è membro, il personale per primo, poi per data.</summary>
    public async Task<IReadOnlyList<Space>> ElencaAsync()
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Space>()
            .Order("is_personal", Constants.Ordering.Descending)
            .Order("created_at", Constants.Ordering.Ascending)
            .Get();
        return risposta.Models;
    }

    public async Task<Space?> LeggiAsync(Guid spazioId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Space>().Where(s => s.Id == spazioId).Get();
        return risposta.Models.FirstOrDefault();
    }

    /// <summary>Crea uno spazio condiviso. Il creatore ne diventa proprietario e primo membro:
    /// lo fa la funzione, in una transazione sola, perché farlo in due chiamate lascerebbe uno
    /// spazio senza membri che la RLS renderebbe subito invisibile al suo stesso creatore.</summary>
    public async Task<Guid> CreaAsync(string nome)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.Rpc<string?>("create_space", new Dictionary<string, object> { ["p_name"] = nome.Trim() });
        return LeggiIdRpc(risposta)
               ?? throw new InvalidOperationException("Il database non ha restituito l'identificatore del nuovo spazio.");
    }

    /// <summary>Entra in uno spazio col codice invito; null se il codice non corrisponde a nulla.
    /// Il codice va passato così com'è stato digitato: cercare prima lo spazio per mostrarne il
    /// nome è impossibile, perché la RLS lo nasconde finché non si è membri.</summary>
    public async Task<Guid?> EntraAsync(string codice)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.Rpc<string?>("join_space", new Dictionary<string, object> { ["p_code"] = codice.Trim().ToUpperInvariant() });
        return LeggiIdRpc(risposta);
    }

    /// <summary>Rinomina. Restituisce false se il database non ha cambiato nulla — cioè se la RLS
    /// ha stabilito che non sei il proprietario. Non lancia: il rifiuto è una risposta, non un guasto.</summary>
    public async Task<bool> RinominaAsync(Guid spazioId, string nome)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<Space>()
            .Where(s => s.Id == spazioId)
            .Set(s => s.Name, nome.Trim())
            .Update();
        return risposta.Models.Count > 0;
    }

    /// <summary>Elimina lo spazio. False se la RLS ha rifiutato (non sei il proprietario, oppure
    /// è il tuo spazio personale, che non si cancella).</summary>
    public async Task<bool> EliminaAsync(Guid spazioId)
    {
        var client = await _supabase.GetClientAsync();
        var prima = await client.From<Space>().Where(s => s.Id == spazioId).Get();
        if (prima.Models.Count == 0) return false;

        await client.From<Space>().Where(s => s.Id == spazioId).Delete();

        var dopo = await client.From<Space>().Where(s => s.Id == spazioId).Get();
        return dopo.Models.Count == 0;
    }

    /// <summary>I membri dello spazio, col nome preso dal profilo.
    /// Due query invece di una join: la RLS su <c>profiles</c> lascia vedere solo chi condivide
    /// uno spazio con te, quindi la seconda restituisce già l'insieme giusto.</summary>
    public async Task<IReadOnlyList<Membro>> MembriAsync(Guid spazioId)
    {
        var client = await _supabase.GetClientAsync();

        var spazio = await LeggiAsync(spazioId);
        if (spazio is null) return [];

        var iscrizioni = await client.From<SpaceMember>()
            .Where(m => m.SpaceId == spazioId)
            .Order("joined_at", Constants.Ordering.Ascending)
            .Get();
        if (iscrizioni.Models.Count == 0) return [];

        var profili = await client.From<Profile>().Get();
        var perId = profili.Models.ToDictionary(p => p.Id);

        return iscrizioni.Models
            .Select(m => new Membro(
                m.UserId,
                perId.TryGetValue(m.UserId, out var p) && !string.IsNullOrWhiteSpace(p.DisplayName)
                    ? p.DisplayName!
                    : "utente",
                perId.TryGetValue(m.UserId, out var q) ? q.AvatarUrl : null,
                m.UserId == spazio.OwnerId))
            .ToList();
    }

    /// <summary>Esci da uno spazio, o espelline qualcun altro se ne sei il proprietario: è la
    /// stessa operazione, ed è la RLS a distinguere i due casi. False se ha rifiutato — succede
    /// sullo spazio personale e sulla riga del proprietario, che nessuno può rimuovere.</summary>
    public async Task<bool> EsciAsync(Guid spazioId, Guid utenteId)
    {
        var client = await _supabase.GetClientAsync();
        await client.From<SpaceMember>()
            .Where(m => m.SpaceId == spazioId && m.UserId == utenteId)
            .Delete();

        var rimaste = await client.From<SpaceMember>()
            .Where(m => m.SpaceId == spazioId && m.UserId == utenteId)
            .Get();
        return rimaste.Models.Count == 0;
    }

    /// <summary>
    /// Estrae l'uuid restituito da una funzione RPC. Il <c>Trim('"')</c> è ridondante — la
    /// libreria passa la risposta per <c>JsonConvert.DeserializeObject&lt;string&gt;</c>, che le
    /// virgolette le ha già tolte — ma costa nulla e copre il caso in cui una versione futura
    /// consegni il corpo grezzo. Un uuid valido non contiene virgolette, quindi non può guastarlo.
    /// </summary>
    private static Guid? LeggiIdRpc(string? risposta)
        => Guid.TryParse(risposta?.Trim('"'), out var id) ? id : null;
}
