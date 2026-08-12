using System.Text;
using Eton.Models;

namespace Eton.Services;

/// <summary>Esito della validazione di un elenco di campi.</summary>
/// <param name="Valido">True se l'elenco può essere salvato così com'è.</param>
/// <param name="Errori">Un messaggio per ciascuna regola violata; vuoto se <paramref name="Valido"/> è true.</param>
public sealed record EsitoValidazione(bool Valido, IReadOnlyList<string> Errori);

/// <summary>Un modello precompilato di collezione, proposto all'utente come punto di partenza.</summary>
/// <param name="Nome">Il nome suggerito per la collezione.</param>
/// <param name="Icona">L'icona suggerita.</param>
/// <param name="Campi">I campi già pronti, nell'ordine in cui vanno mostrati.</param>
public sealed record ModelloCollezione(string Nome, string Icona, IReadOnlyList<CampoDefinizione> Campi);

/// <summary>
/// Le regole sullo schema di una collezione: quali campi sono ammessi, come si ricava una chiave
/// dall'etichetta che l'utente digita, in che ordine i campi vanno mostrati, e i modelli
/// precompilati fra cui scegliere. È lo stesso schema che finisce dentro <c>collections.fields</c>
/// tramite <see cref="Eton.Models.Collection.Fields"/>.
/// </summary>
public static class SchemaCampi
{
    /// <summary>I soli tipi di campo che una <see cref="CampoDefinizione"/> può dichiarare.</summary>
    public static readonly IReadOnlyList<string> TipiAmmessi = ["text", "number", "select", "date", "bool", "url"];

    /// <summary>Numero massimo di campi che una collezione può avere.</summary>
    public const int MassimoCampi = 40;

    private const int LunghezzaMassimaChiave = 40;

    // Tabella esplicita e non Normalize(...): con InvariantGlobalization attivo ICU non è
    // disponibile, quindi qualunque cosa altrove dipenderebbe dalla cultura va scritta a mano qui
    // pure. Copre solo i caratteri accentati che possono ragionevolmente comparire in un'etichetta
    // scritta in italiano (o nelle lingue da cui l'italiano prende in prestito parole).
    private static readonly IReadOnlyDictionary<char, char> RiduzioneAccenti = new Dictionary<char, char>
    {
        ['à'] = 'a', ['á'] = 'a', ['â'] = 'a', ['ã'] = 'a', ['ä'] = 'a', ['å'] = 'a',
        ['è'] = 'e', ['é'] = 'e', ['ê'] = 'e', ['ë'] = 'e',
        ['ì'] = 'i', ['í'] = 'i', ['î'] = 'i', ['ï'] = 'i',
        ['ò'] = 'o', ['ó'] = 'o', ['ô'] = 'o', ['õ'] = 'o', ['ö'] = 'o',
        ['ù'] = 'u', ['ú'] = 'u', ['û'] = 'u', ['ü'] = 'u',
        ['ç'] = 'c',
        ['ñ'] = 'n',
        ['ý'] = 'y',
        ['À'] = 'A', ['Á'] = 'A', ['Â'] = 'A', ['Ã'] = 'A', ['Ä'] = 'A', ['Å'] = 'A',
        ['È'] = 'E', ['É'] = 'E', ['Ê'] = 'E', ['Ë'] = 'E',
        ['Ì'] = 'I', ['Í'] = 'I', ['Î'] = 'I', ['Ï'] = 'I',
        ['Ò'] = 'O', ['Ó'] = 'O', ['Ô'] = 'O', ['Õ'] = 'O', ['Ö'] = 'O',
        ['Ù'] = 'U', ['Ú'] = 'U', ['Û'] = 'U', ['Ü'] = 'U',
        ['Ç'] = 'C',
        ['Ñ'] = 'N',
        ['Ý'] = 'Y',
    };

    /// <summary>
    /// Verifica che l'elenco di campi sia salvabile: chiavi in formato valido e uniche, etichette
    /// non vuote, tipo fra quelli ammessi, opzioni coerenti col tipo (<c>select</c> ne richiede
    /// almeno due, distinte; gli altri tipi non ne portano), e non più di <see cref="MassimoCampi"/>
    /// campi in totale.
    /// </summary>
    public static EsitoValidazione Valida(IReadOnlyList<CampoDefinizione> campi)
    {
        var errori = new List<string>();
        var chiaviViste = new HashSet<string>(StringComparer.Ordinal);

        foreach (var campo in campi)
        {
            // Gli inizializzatori "= """ del modello (CampoDefinizione.cs) non proteggono da un
            // null qui: Newtonsoft li sovrascrive quando il jsonb contiene esplicitamente
            // "label": null o "key": null, e il vincolo SQL collections_fields_shape controlla
            // solo che fields sia un array di al massimo 40 elementi, mai la forma dei singoli
            // elementi. Un campo così arriva quindi a Valida, che deve segnalarlo senza esplodere.
            var chiave = campo.Key ?? "";
            var label = campo.Label ?? "";
            var tipo = campo.Type ?? "";
            var etichetta = string.IsNullOrEmpty(label) ? chiave : label;

            if (!ChiaveValida(chiave))
                errori.Add($"La chiave \"{chiave}\" del campo \"{etichetta}\" non è valida: deve iniziare con una lettera minuscola ed essere composta solo da lettere minuscole, cifre e underscore, entro 40 caratteri.");
            else if (!chiaviViste.Add(chiave))
                errori.Add($"La chiave \"{chiave}\" è usata da più campi: ogni campo deve avere una chiave unica.");

            var labelPulita = label.Trim();
            if (labelPulita.Length == 0)
                errori.Add($"Il campo \"{chiave}\" non ha un'etichetta.");
            else if (labelPulita.Length > 60)
                errori.Add($"L'etichetta \"{label}\" del campo \"{chiave}\" è troppo lunga: al massimo 60 caratteri.");

            if (!TipiAmmessi.Contains(tipo))
            {
                errori.Add($"Il campo \"{etichetta}\" ha un tipo non ammesso: \"{tipo}\".");
            }
            else if (tipo == "select")
            {
                var opzioni = (campo.Options ?? []).Select(o => o ?? "").ToList();
                if (opzioni.Count < 2)
                    errori.Add($"Il campo \"{etichetta}\" è di tipo select ma ha meno di due opzioni.");
                if (opzioni.Any(o => o.Trim().Length == 0))
                    errori.Add($"Il campo \"{etichetta}\" ha un'opzione vuota.");
                if (opzioni.Count > 40)
                    errori.Add($"Il campo \"{etichetta}\" ha più di 40 opzioni.");
                if (opzioni.Distinct(StringComparer.Ordinal).Count() != opzioni.Count)
                    errori.Add($"Il campo \"{etichetta}\" ha opzioni duplicate.");
            }
            else if (campo.Options is { Count: > 0 })
            {
                errori.Add($"Il campo \"{etichetta}\" non è di tipo select ma ha delle opzioni.");
            }
        }

        if (campi.Count > MassimoCampi)
            errori.Add($"Ci sono {campi.Count} campi: il massimo consentito è {MassimoCampi}.");

        return new EsitoValidazione(errori.Count == 0, errori);
    }

    private static bool ChiaveValida(string chiave)
    {
        if (chiave.Length == 0 || chiave.Length > LunghezzaMassimaChiave) return false;
        if (chiave[0] is < 'a' or > 'z') return false;

        foreach (var c in chiave)
        {
            var ammesso = c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_';
            if (!ammesso) return false;
        }

        return true;
    }

    /// <summary>
    /// Ricava una chiave da un'etichetta digitata dall'utente: minuscolo, senza accenti, con ogni
    /// sequenza di caratteri non alfanumerici collassata in un singolo <c>_</c>, senza <c>_</c>
    /// iniziali o finali, senza cifra iniziale, troncata a 40 caratteri. Se il risultato collide con
    /// una chiave già in uso in <paramref name="giaUsate"/>, vi si aggiunge un suffisso numerico
    /// progressivo (<c>_2</c>, <c>_3</c>, …) finché non è libera.
    /// </summary>
    public static string ChiaveDa(string etichetta, IReadOnlyCollection<string> giaUsate)
    {
        var senzaAccenti = RiduciAccenti(etichetta);
        var minuscola = senzaAccenti.ToLowerInvariant();

        var grezza = new StringBuilder(minuscola.Length);
        foreach (var c in minuscola)
            grezza.Append(c is (>= 'a' and <= 'z') or (>= '0' and <= '9') ? c : '_');

        var chiave = Collassa(grezza.ToString()).Trim('_');

        if (chiave.Length == 0) chiave = "campo";
        else if (char.IsDigit(chiave[0])) chiave = "campo_" + chiave;

        if (chiave.Length > LunghezzaMassimaChiave) chiave = chiave[..LunghezzaMassimaChiave];
        chiave = chiave.TrimEnd('_');

        if (giaUsate.Contains(chiave, StringComparer.Ordinal))
        {
            var contatore = 2;
            string candidata;
            do
            {
                var suffisso = "_" + contatore;
                var basePerSuffisso = chiave.Length + suffisso.Length > LunghezzaMassimaChiave
                    ? chiave[..(LunghezzaMassimaChiave - suffisso.Length)]
                    : chiave;
                candidata = basePerSuffisso + suffisso;
                contatore++;
            } while (giaUsate.Contains(candidata, StringComparer.Ordinal));

            chiave = candidata;
        }

        return chiave;
    }

    private static string RiduciAccenti(string testo)
    {
        var risultato = new char[testo.Length];
        for (var i = 0; i < testo.Length; i++)
            risultato[i] = RiduzioneAccenti.TryGetValue(testo[i], out var senzaAccento) ? senzaAccento : testo[i];

        return new string(risultato);
    }

    private static string Collassa(string testo)
    {
        var risultato = new StringBuilder(testo.Length);
        var precedenteEraUnderscore = false;

        foreach (var c in testo)
        {
            if (c == '_')
            {
                if (!precedenteEraUnderscore) risultato.Append('_');
                precedenteEraUnderscore = true;
            }
            else
            {
                risultato.Append(c);
                precedenteEraUnderscore = false;
            }
        }

        return risultato.ToString();
    }

    /// <summary>Restituisce i campi ordinati per <see cref="CampoDefinizione.Order"/> crescente, in
    /// modo stabile: a parità di <c>Order</c> l'ordine relativo di partenza non cambia.</summary>
    public static IReadOnlyList<CampoDefinizione> Ordina(IReadOnlyList<CampoDefinizione> campi)
        // OrderBy di LINQ è stabile per contratto documentato: a parità di chiave di ordinamento
        // mantiene l'ordine relativo di partenza. È esattamente ciò che serve qui — due campi con
        // lo stesso Order non devono scambiarsi di posto a ogni ridisegno, o l'editor sembrerebbe
        // impazzito senza che l'utente abbia toccato nulla.
        => campi.OrderBy(c => c.Order).ToList();

    /// <summary>I modelli di collezione proposti in fase di creazione. Ognuno passa <see cref="Valida"/>:
    /// un modello precompilato non valido sarebbe una trappola, perché l'utente lo sceglierebbe senza
    /// poi riuscire a salvare.
    /// <para>
    /// Ogni accesso costruisce nuovi oggetti: non è uno spreco, è voluto. Chi parte da un modello
    /// scrive <c>modello.Campi.ToList()</c>, che copia solo il contenitore e lascia condivisi gli
    /// elementi — modificare la copia corromperebbe anche il modello originale, e in WASM lo stato
    /// statico vive quanto la scheda del browser, quindi resterebbe corrotto fino a un ricaricamento
    /// completo. Chi ha bisogno dei modelli più volte se li catturi in una variabile: ogni accesso
    /// alloca.
    /// </para>
    /// </summary>
    public static IReadOnlyList<ModelloCollezione> Modelli =>
    [
        new ModelloCollezione("Liquidi svapo", "🧪",
        [
            new CampoDefinizione { Key = "marca", Label = "Marca", Type = "text", Order = 1 },
            new CampoDefinizione { Key = "aroma", Label = "Aroma", Type = "text", Order = 2 },
            new CampoDefinizione { Key = "nicotina_mg", Label = "Nicotina (mg)", Type = "number", Order = 3 },
            new CampoDefinizione { Key = "pgvg", Label = "PG/VG", Type = "select", Options = ["50/50", "60/40", "70/30"], Order = 4 },
            new CampoDefinizione { Key = "prezzo", Label = "Prezzo (€)", Type = "number", Order = 5 },
        ]),
        new ModelloCollezione("Birre", "🍺",
        [
            new CampoDefinizione { Key = "birrificio", Label = "Birrificio", Type = "text", Order = 1 },
            new CampoDefinizione { Key = "stile", Label = "Stile", Type = "text", Order = 2 },
            new CampoDefinizione { Key = "gradazione", Label = "Gradazione (%)", Type = "number", Order = 3 },
            new CampoDefinizione { Key = "formato", Label = "Formato", Type = "select", Options = ["33 cl", "50 cl", "75 cl"], Order = 4 },
            new CampoDefinizione { Key = "prezzo", Label = "Prezzo (€)", Type = "number", Order = 5 },
        ]),
        new ModelloCollezione("Film", "🎬",
        [
            new CampoDefinizione { Key = "regista", Label = "Regista", Type = "text", Order = 1 },
            new CampoDefinizione { Key = "anno", Label = "Anno", Type = "number", Order = 2 },
            new CampoDefinizione { Key = "genere", Label = "Genere", Type = "select", Options = ["Azione", "Commedia", "Drammatico", "Fantascienza", "Documentario"], Order = 3 },
            new CampoDefinizione { Key = "visto_il", Label = "Visto il", Type = "date", Order = 4 },
        ]),
    ];
}
