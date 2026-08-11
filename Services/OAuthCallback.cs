namespace Eton.Services;

/// <summary>Esito dell'analisi dell'URL di ritorno da Google.</summary>
/// <param name="Codice">Codice di autorizzazione monouso (flusso PKCE); null se assente o se c'è un errore.</param>
/// <param name="Errore">Messaggio di rifiuto del provider; null se l'accesso non è stato rifiutato.</param>
public sealed record OAuthCallbackEsito(string? Codice, string? Errore);

/// <summary>
/// Analisi pura dell'URL su cui il provider ci riporta dopo l'accesso. Isolata da
/// <see cref="SupabaseService"/> perché è l'unico pezzo del flusso OAuth verificabile senza un
/// browser: qui si sbaglia in silenzio (un parametro letto male = login che non si completa mai),
/// e un test costa niente.
/// Si legge SOLO la query, non il fragment: col flusso PKCE il provider restituisce un codice
/// monouso in <c>?code=</c>. Il fragment conterrebbe un access token — è il motivo per cui il
/// flusso implicit è stato abbandonato.
/// </summary>
public static class OAuthCallback
{
    public static OAuthCallbackEsito Analizza(string uri)
    {
        var parametri = LeggiQuery(uri);

        // L'errore ha la precedenza: se il provider ha rifiutato, un eventuale code è inutilizzabile.
        if (parametri.TryGetValue("error_description", out var descrizione) && !string.IsNullOrWhiteSpace(descrizione))
            return new OAuthCallbackEsito(null, descrizione);

        if (parametri.TryGetValue("error", out var errore) && !string.IsNullOrWhiteSpace(errore))
            return new OAuthCallbackEsito(null, errore);

        if (parametri.TryGetValue("code", out var codice) && !string.IsNullOrWhiteSpace(codice))
            return new OAuthCallbackEsito(codice, null);

        return new OAuthCallbackEsito(null, null);
    }

    private static Dictionary<string, string> LeggiQuery(string uri)
    {
        var risultato = new Dictionary<string, string>(StringComparer.Ordinal);

        // Il fragment si taglia PRIMA di cercare la query, non dopo: in "…/#frag?code=x" il '?' sta
        // dentro il fragment, e cercarlo per primo lo scambierebbe per l'inizio della query —
        // esattamente il contenuto che questa classe esiste per NON leggere.
        var inizioFragment = uri.IndexOf('#');
        var senzaFragment = inizioFragment >= 0 ? uri[..inizioFragment] : uri;

        var inizio = senzaFragment.IndexOf('?');
        if (inizio < 0) return risultato;

        var query = senzaFragment[(inizio + 1)..];

        foreach (var coppia in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatore = coppia.IndexOf('=');
            if (separatore <= 0) continue;

            var chiave = Uri.UnescapeDataString(coppia[..separatore]);
            var valore = Uri.UnescapeDataString(coppia[(separatore + 1)..].Replace('+', ' '));
            risultato[chiave] = valore;
        }

        return risultato;
    }
}
