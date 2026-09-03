namespace Eton.Services;

/// <summary>Classificazione chiusa del rifiuto letto dal ritorno OAuth: mai testo libero del provider.</summary>
public enum OAuthRifiuto
{
    /// <summary>Nessun rifiuto: non c'è errore, oppure l'URL non è nemmeno un ritorno OAuth.</summary>
    Nessuno,
    /// <summary>Il permesso non è stato concesso sulla schermata di Google.</summary>
    Annullato,
    /// <summary>L'autorizzazione monouso non era più spendibile al ritorno.</summary>
    Scaduto,
    /// <summary>Rifiutato per un motivo che non sappiamo tradurre: ci cade ogni caso sconosciuto.</summary>
    Generico
}

/// <summary>Esito dell'analisi dell'URL di ritorno da Google.</summary>
/// <param name="Codice">Codice di autorizzazione monouso (flusso PKCE); null se assente o se c'è un errore.</param>
/// <param name="Errore">Classificazione del rifiuto del provider; <see cref="OAuthRifiuto.Nessuno"/> se l'accesso non è stato rifiutato.</param>
/// <param name="Diagnostica">Grezzo del provider (error, error_code, error_description), solo per la console: nessuna schermata la legge.</param>
public sealed record OAuthCallbackEsito(string? Codice, OAuthRifiuto Errore, string? Diagnostica);

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

        parametri.TryGetValue("error", out var errore);

        // L'errore ha la precedenza: se il provider ha rifiutato, un eventuale code è inutilizzabile.
        // In OAuth 2.0 "error" è OBBLIGATORIO e Gotrue lo mette sempre: un ?error_description= da
        // solo, senza "error", non è un rifiuto ma un URL costruito da un estraneo — e per questo
        // non apre nemmeno il riquadro generico. Altrimenti l'attaccante otterrebbe comunque un
        // allarme sul dominio vero, solo scritto con parole nostre invece che con le sue.
        if (!string.IsNullOrWhiteSpace(errore))
        {
            parametri.TryGetValue("error_code", out var codiceRifiuto);
            parametri.TryGetValue("error_description", out var descrizione);

            // Si guarda la COPPIA (error, error_code), mai la descrizione: "access_denied" SENZA
            // error_code è l'utente che ha annullato sulla schermata di Google; "access_denied" CON
            // error_code è una policy del server (iscrizioni chiuse, account sospeso). Guardando il
            // solo "error" si direbbe "hai annullato" anche a chi è stato rifiutato dal server.
            // Ogni valore mai visto cade su Generico per costruzione: un "riprova fra un momento" a
            // chi ha annullato è impreciso, un "hai annullato" a chi è stato rifiutato è falso.
            var rifiuto = (errore, codiceRifiuto) switch
            {
                (_, "bad_oauth_state" or "bad_oauth_callback" or "flow_state_already_used")
                    => OAuthRifiuto.Scaduto,   // codici Gotrue: state o verificatore PKCE consumato o scaduto
                ("access_denied", _) when string.IsNullOrWhiteSpace(codiceRifiuto)
                    => OAuthRifiuto.Annullato, // il no dell'utente sulla schermata di Google
                _ => OAuthRifiuto.Generico,    // access_denied CON error_code = policy del server; e tutto il resto
            };

            return new OAuthCallbackEsito(
                null,
                rifiuto,
                $"error={errore}; error_code={codiceRifiuto}; error_description={descrizione}");
        }

        if (parametri.TryGetValue("code", out var codice) && !string.IsNullOrWhiteSpace(codice))
            return new OAuthCallbackEsito(codice, OAuthRifiuto.Nessuno, null);

        return new OAuthCallbackEsito(null, OAuthRifiuto.Nessuno, null);
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
