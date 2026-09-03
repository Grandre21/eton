using System.Reflection;
using System.Text.RegularExpressions;
using Eton.Models;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Eton.Tests;

// Il 12 agosto 2026 Insert<Collection> ha iniziato a fallire in produzione con 42501 permission
// denied for table collections: il modello inviava la colonna blind in ogni INSERT, ma
// 20260812230000_voto_al_buio.sql, che l'aveva aggiunta, l'aveva riconcessa solo in UPDATE. Il
// difetto è sopravvissuto due settimane perché senza un database a portata di dotnet test nessun
// test confrontava ciò che un modello invia con ciò che una migrazione concede. Questa classe lo
// fa: legge i grant/revoke delle migrazioni SQL e la reflection dei modelli, e verifica che ogni
// colonna inviata in INSERT sia sempre inclusa nell'insieme concesso.
public class PrivilegiInsertTests
{
    private const string Tutte = "*";

    private const string PatternTabella =
        @"^(?<verbo>grant|revoke)\s+(?<privs>.+?)\s+on\s+(?<objs>public\.\w+(?:\s*,\s*public\.\w+)*)\s+(?<dir>to|from)\s+(?<roles>[\w\s,]+)$";

    private const string PatternNonPertinente =
        @"^(?:grant|revoke)\s+.+?\s+on\s+(?:schema|function|all\s|sequence)";

    private static readonly Dictionary<string, HashSet<string>> Concesso = LeggiGrantInsert();

    public static IEnumerable<object[]> ModelliConGrantInsert() =>
        ModelliDaVerificare()
            .Where(t => Concesso.ContainsKey(NomeTabella(t)))
            .Select(t => new object[] { t });

    private static bool PuoInserire(HashSet<string> concesse, string colonna) =>
        concesse.Contains(Tutte) || concesse.Contains(colonna);

    private static IEnumerable<Type> ModelliDaVerificare() =>
        typeof(Collection).Assembly.GetTypes()
            .Where(t => !t.IsAbstract
                && typeof(BaseModel).IsAssignableFrom(t)
                && t.GetCustomAttribute<TableAttribute>() is not null);

    private static string NomeTabella(Type modello) =>
        modello.GetCustomAttribute<TableAttribute>()!.Name;

    private static Dictionary<string, HashSet<string>> LeggiGrantInsert()
    {
        var partenza = AppContext.BaseDirectory;
        string? cartella = null;
        for (var dir = new DirectoryInfo(partenza); dir is not null && cartella is null; dir = dir.Parent)
        {
            var candidata = Path.Combine(dir.FullName, "supabase", "migrations");
            if (Directory.Exists(candidata))
                cartella = candidata;
        }
        if (cartella is null)
            throw new InvalidOperationException($"Cartella supabase/migrations non trovata risalendo da {partenza}");

        var concesso = new Dictionary<string, HashSet<string>>();

        foreach (var file in Directory.GetFiles(cartella, "*.sql").OrderBy(Path.GetFileName, StringComparer.Ordinal))
        {
            var righeSenzaCommenti = File.ReadAllLines(file)
                .Select(riga => riga.Contains("--") ? riga[..riga.IndexOf("--", StringComparison.Ordinal)] : riga);
            var testoCollassato = Regex.Replace(string.Join(' ', righeSenzaCommenti), @"\s+", " ").Trim();

            foreach (var statement in testoCollassato.Split(';').Select(s => s.Trim()).Where(s => s.Length > 0))
            {
                if (!Regex.IsMatch(statement, @"\b(grant|revoke)\b", RegexOptions.IgnoreCase))
                    continue;

                ApplicaStatement(concesso, statement, file);
            }
        }

        return concesso;
    }

    private static void ApplicaStatement(
        Dictionary<string, HashSet<string>> concesso, string statement, string file)
    {
        var minuscolo = statement.ToLowerInvariant();
        var m = Regex.Match(minuscolo, PatternTabella);
        if (!m.Success)
        {
            if (!Regex.IsMatch(minuscolo, PatternNonPertinente))
                throw new InvalidOperationException(
                    $"Statement grant/revoke non riconosciuto in {Path.GetFileName(file)}: {statement}");
            return;
        }

        var verbo = m.Groups["verbo"].Value;
        var direzione = m.Groups["dir"].Value;
        if ((verbo == "grant" && direzione != "to") || (verbo == "revoke" && direzione != "from"))
            throw new InvalidOperationException(
                $"Statement grant/revoke con verbo e direzione incoerenti in {Path.GetFileName(file)}: {statement}");

        var revoca = verbo == "revoke";

        var ruoli = m.Groups["roles"].Value.Split(',').Select(r => r.Trim()).ToArray();
        foreach (var ruolo in ruoli)
            if (ruolo is not ("authenticated" or "anon" or "service_role" or "public"))
                throw new InvalidOperationException(
                    $"Ruolo sconosciuto \"{ruolo}\" in {Path.GetFileName(file)}: {statement}");

        if (!ruoli.Contains("authenticated") && !ruoli.Contains("public"))
            return;

        var privs = m.Groups["privs"].Value;
        var insertMatch = Regex.Match(privs, @"\binsert\b(?:\s*\(([^)]*)\))?");
        if (!insertMatch.Success && !Regex.IsMatch(privs, @"\ball\b"))
            return;

        var colonne = insertMatch.Groups[1].Success
            ? insertMatch.Groups[1].Value.Split(',').Select(c => c.Trim())
            : new[] { Tutte };

        foreach (var oggetto in m.Groups["objs"].Value.Split(',').Select(o => o.Trim()))
        {
            var tabella = oggetto[(oggetto.IndexOf('.') + 1)..];

            if (revoca)
            {
                concesso.Remove(tabella);
                continue;
            }

            if (!concesso.TryGetValue(tabella, out var set))
                concesso[tabella] = set = new HashSet<string>();
            set.UnionWith(colonne);
        }
    }

    // Il difetto delle collezioni, generalizzato: se un modello invia una colonna che nessuna
    // migrazione ha concesso in INSERT, il database risponde 42501 senza dire quale colonna manca.
    // Qui la si nomina prima ancora di toccare un database.
    [Theory]
    [MemberData(nameof(ModelliConGrantInsert))]
    public void Ogni_modello_invia_solo_colonne_concesse(Type modello)
    {
        var tabella = NomeTabella(modello);
        var concesse = Concesso[tabella];

        var inviate = new List<string>();
        foreach (var proprieta in modello.GetProperties())
        {
            var colonna = proprieta.GetCustomAttribute<ColumnAttribute>();
            if (colonna is not null && !colonna.IgnoreOnInsert)
                inviate.Add(colonna.ColumnName);

            var chiave = proprieta.GetCustomAttribute<PrimaryKeyAttribute>();
            if (chiave is not null && chiave.ShouldInsert)
                inviate.Add(chiave.ColumnName);
        }

        var mancanti = inviate.Where(c => !PuoInserire(concesse, c)).ToList();
        Assert.True(mancanti.Count == 0,
            $"{tabella}: colonne inviate in INSERT ma non concesse: {string.Join(", ", mancanti)}");
    }

    // Se una tabella perdesse il proprio grant insert per errore (come blind, per due settimane) e
    // nessun modello lo notasse, sparirebbe semplicemente dai casi del test sopra: questo test
    // fissa l'elenco atteso perché quel silenzio non passi inosservato.
    [Fact]
    public void Le_tabelle_senza_grant_insert_sono_esattamente_spaces_e_space_members()
    {
        var senzaGrant = ModelliDaVerificare()
            .Select(NomeTabella)
            .Where(t => !Concesso.ContainsKey(t))
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["space_members", "spaces"], senzaGrant);
    }

    // Il grant di expenses è scritto su due righe (20260824000000_spese.sql:144-145): un parser
    // che unisse per riga invece che per ';' spezzerebbe lo statement e perderebbe id e spent_on
    // senza che il modello smetta di inviarli, con lo stesso esito del difetto di collections.
    [Fact]
    public void Il_grant_insert_di_expenses_su_due_righe_e_riconosciuto()
    {
        var concesse = Concesso["expenses"];
        Assert.True(PuoInserire(concesse, "id"));
        Assert.True(PuoInserire(concesse, "spent_on"));
    }

    // grant select, insert on public.profiles non porta un elenco di colonne: significa TUTTE le
    // colonne. Un parser che pretendesse sempre le parentesi tratterebbe questo grant come vuoto e
    // ogni INSERT su profiles fallirebbe con 42501 al primo utente che si registra.
    [Fact]
    public void Un_grant_insert_senza_elenco_di_colonne_concede_tutte_le_colonne()
    {
        var concesse = Concesso["profiles"];
        Assert.True(PuoInserire(concesse, "display_name"));
        Assert.True(PuoInserire(concesse, "una_colonna_mai_dichiarata_altrove"));
    }
}
