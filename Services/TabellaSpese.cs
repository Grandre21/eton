using System.Globalization;
using Eton.Models;

namespace Eton.Services;

/// <summary>Come una riga sta nel registro: una spesa vera, una occorrenza ricorrente già scaduta
/// ma non ancora materializzata, o una futura che nei totali non entra.</summary>
public enum StatoSpesa { Registrata, Prevista, InArrivo }

/// <summary>Una riga della tabella delle spese: la spesa e il suo stato.</summary>
public sealed record RigaSpesa(Expense Spesa, StatoSpesa Stato)
{
    /// <summary>L'identità della riga per la tabella: l'<c>Id</c> per le registrate,
    /// <c>"&lt;recurring_id&gt;:&lt;yyyy-MM&gt;"</c> per previste e in arrivo. Non l'<c>Id</c> per
    /// tutte perché <see cref="CalcoliRicorrenti.Occorrenza"/> dà un <c>Guid.NewGuid()</c> a ogni
    /// fusione: la cella attiva e la selezione si perderebbero a ogni rilettura.</summary>
    public string Chiave => Stato == StatoSpesa.Registrata
        ? Spesa.Id.ToString()
        : $"{Spesa.RecurringId}:{CalcoliRicorrenti.Periodo(Spesa.RecurringPeriod!.Value)}";

    /// <summary>Le in arrivo sono fuori da ogni totale: la loro data non è ancora passata.</summary>
    public bool ContaNeiTotali => Stato != StatoSpesa.InArrivo;
}

/// <summary>Il periodo della tabella. <c>UltimiTreMesi</c> è il mese in corso e i due precedenti.</summary>
public enum TipoPeriodo { QuestoMese, MeseScorso, UltimiTreMesi, QuestAnno, Libero }

/// <summary>I filtri della tabella, tutti in AND. Un insieme vuoto, un pagante nullo e un testo vuoto
/// non filtrano.</summary>
public sealed record FiltriSpese(IReadOnlySet<string> Categorie, Guid? Pagante, string Testo, IReadOnlySet<StatoSpesa> Stati)
{
    public static FiltriSpese Nessuno { get; } = new(new HashSet<string>(), null, "", new HashSet<StatoSpesa>());
}

/// <summary>Tutto ciò che l'utente ha scelto della tabella — periodo, filtri, ordine, raggruppamento
/// — e che l'URL deve poter riportare: il link a una vista la riapre identica.</summary>
public sealed record VistaTabella(TipoPeriodo Periodo, DateTime? Da, DateTime? A, FiltriSpese Filtri,
    string OrdinaPer, bool Decrescente, string? RaggruppaPer)
{
    private static readonly (string Nome, TipoPeriodo Tipo)[] Periodi =
    [
        ("mese", TipoPeriodo.QuestoMese), ("scorso", TipoPeriodo.MeseScorso), ("3mesi", TipoPeriodo.UltimiTreMesi),
        ("anno", TipoPeriodo.QuestAnno), ("libero", TipoPeriodo.Libero)
    ];

    private static readonly (string Nome, StatoSpesa Stato)[] Stati =
    [
        ("registrata", StatoSpesa.Registrata), ("prevista", StatoSpesa.Prevista), ("in-arrivo", StatoSpesa.InArrivo)
    ];

    private static readonly string[] ColonneOrdinabili =
    [
        TabellaSpese.Colonne.Data, TabellaSpese.Colonne.Descrizione, TabellaSpese.Colonne.Categoria,
        TabellaSpese.Colonne.Importo, TabellaSpese.Colonne.Pagante, TabellaSpese.Colonne.Stato
    ];

    private static readonly string[] GruppiValidi =
        [TabellaSpese.Gruppi.Categoria, TabellaSpese.Gruppi.Pagante, TabellaSpese.Gruppi.Mese];

    /// <summary>Questo mese, nessun filtro, per data decrescente, senza raggruppamento.</summary>
    public static VistaTabella Predefinita { get; } =
        new(TipoPeriodo.QuestoMese, null, null, FiltriSpese.Nessuno, TabellaSpese.Colonne.Data, true, null);

    /// <summary>La vista scritta in un URL, con o senza il <c>?</c> iniziale. Non lancia mai: un
    /// parametro illeggibile torna al default di quel parametro e non tocca gli altri. Un periodo
    /// libero con date mancanti, impossibili o invertite torna a questo mese, e le date si perdono.</summary>
    public static VistaTabella DaQuery(string query)
    {
        var valori = new Dictionary<string, string>();
        foreach (var coppia in (query ?? "").TrimStart('?').Split('&'))
        {
            var uguale = coppia.IndexOf('=');
            if (uguale < 0) continue;
            try
            {
                valori[coppia[..uguale]] = Uri.UnescapeDataString(coppia[(uguale + 1)..]);
            }
            catch (UriFormatException)
            {
            }
        }

        string Valore(string chiave) => valori.GetValueOrDefault(chiave, "");

        DateTime? Data(string chiave) =>
            DateTime.TryParseExact(Valore(chiave), TabellaSpese.FormatoData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

        var periodo = Periodi.FirstOrDefault(p => p.Nome == Valore("periodo"), (Nome: "mese", Tipo: TipoPeriodo.QuestoMese)).Tipo;
        DateTime? da = null, a = null;
        if (periodo == TipoPeriodo.Libero)
        {
            da = Data("da");
            a = Data("a");
            if (da is null || a is null || da > a)
            {
                periodo = TipoPeriodo.QuestoMese;
                da = a = null;
            }
        }

        var categorie = Valore("cat").Split(',').Where(CategorieSpesa.Conosciuta).ToHashSet();
        var pagante = Guid.TryParse(Valore("pagante"), out var id) ? id : (Guid?)null;
        var nomiStati = Valore("stato").Split(',');
        var stati = Stati.Where(p => nomiStati.Contains(p.Nome)).Select(p => p.Stato).ToHashSet();
        var ordinaPer = ColonneOrdinabili.Contains(Valore("ord")) ? Valore("ord") : TabellaSpese.Colonne.Data;
        var raggruppaPer = GruppiValidi.Contains(Valore("grp")) ? Valore("grp") : null;

        return new VistaTabella(periodo, da, a, new FiltriSpese(categorie, pagante, Valore("testo"), stati),
            ordinaPer, Valore("dir") != "asc", raggruppaPer);
    }

    /// <summary>La vista come query string: solo i parametri diversi dal default, per cui la vista
    /// predefinita dà <c>""</c>. Ogni valore passa per <see cref="Uri.EscapeDataString"/>.</summary>
    public string InQuery()
    {
        var parametri = new List<string>();
        void Aggiungi(string chiave, string valore) => parametri.Add($"{chiave}={Uri.EscapeDataString(valore)}");

        if (Periodo != TipoPeriodo.QuestoMese) Aggiungi("periodo", Array.Find(Periodi, p => p.Tipo == Periodo).Nome);
        if (Periodo == TipoPeriodo.Libero)
        {
            if (Da is { } da) Aggiungi("da", da.ToString(TabellaSpese.FormatoData, CultureInfo.InvariantCulture));
            if (A is { } a) Aggiungi("a", a.ToString(TabellaSpese.FormatoData,CultureInfo.InvariantCulture));
        }

        var categorie = CategorieSpesa.Elenco.Where(Filtri.Categorie.Contains).ToList();
        if (categorie.Count > 0) Aggiungi("cat", string.Join(',', categorie));
        if (Filtri.Pagante is { } pagante) Aggiungi("pagante", pagante.ToString());
        if (Filtri.Testo.Length > 0) Aggiungi("testo", Filtri.Testo);

        var stati = Stati.Where(p => Filtri.Stati.Contains(p.Stato)).Select(p => p.Nome).ToList();
        if (stati.Count > 0) Aggiungi("stato", string.Join(',', stati));

        if (OrdinaPer != TabellaSpese.Colonne.Data) Aggiungi("ord", OrdinaPer);
        if (!Decrescente) Aggiungi("dir", "asc");
        if (RaggruppaPer is not null) Aggiungi("grp", RaggruppaPer);

        return parametri.Count == 0 ? "" : "?" + string.Join('&', parametri);
    }
}

/// <summary>Il risultato di una modifica di cella: la spesa nuova, oppure <c>Nuova = null</c> e il
/// motivo per cui non parte.</summary>
public sealed record EsitoModifica(Expense? Nuova, string? Errore);

public enum AzioneMassa { CambiaCategoria, Elimina }

/// <summary>Cosa è successo a una selezione di spese su cui si è agito in massa. <c>Rifiutate</c> sono
/// quelle ancora lì e non toccate (la RLS dice che non erano dell'utente), <c>Sparite</c> quelle che
/// non c'erano già più.</summary>
public sealed record EsitoMassa(int Richieste, int Toccate, int Rifiutate, int Sparite)
{
    /// <summary>L'esito di una cancellazione, dalle spese presenti prima e dopo. Si conta
    /// «presenti prima ∖ presenti dopo», mai «richieste ∖ presenti dopo»: se la RLS filtra la
    /// lettura, la seconda direbbe «tutte eliminate» su righe ancora lì. Una riga già assente
    /// prima va fra le <c>Sparite</c> (decisione del 7 ottobre).</summary>
    public static EsitoMassa DaEliminazione(IReadOnlyCollection<Guid> richieste,
        IReadOnlySet<Guid> presentiPrima, IReadOnlySet<Guid> presentiDopo) =>
        new(richieste.Count,
            presentiPrima.Count(id => !presentiDopo.Contains(id)),
            presentiPrima.Count(id => presentiDopo.Contains(id)),
            richieste.Count - presentiPrima.Count);

    /// <summary>L'esito di una modifica, dalle spese che la riga di ritorno dà per toccate e da
    /// quelle rimaste lì senza esserlo.</summary>
    public static EsitoMassa DaModifica(IReadOnlyCollection<Guid> richieste,
        IReadOnlySet<Guid> toccate, IReadOnlySet<Guid> nonToccateAncoraPresenti) =>
        new(richieste.Count, toccate.Count, nonToccateAncoraPresenti.Count,
            richieste.Count - toccate.Count - nonToccateAncoraPresenti.Count);
}

/// <summary>
/// Calcoli puri sulla tabella delle spese: righe e stati, periodi, filtri, modifica di una cella, esito
/// delle azioni di massa. Nessun accesso a rete o a stato; l'ordinamento lo fa la pagina.
/// </summary>
public static class TabellaSpese
{
    // È anche il formato del valore di un <input type="date">.
    internal const string FormatoData = "yyyy-MM-dd";

    // Il check sulla descrizione in supabase/migrations/20260824000000_spese.sql.
    private const int LunghezzaMassimaDescrizione = 200;

    public static class Colonne
    {
        public const string Data = "data", Descrizione = "descrizione", Categoria = "categoria",
            Importo = "importo", Pagante = "pagante", Stato = "stato";
    }

    public static class Gruppi
    {
        public const string Categoria = "categoria", Pagante = "pagante", Mese = "mese";
    }

    /// <summary>Le righe del periodo: prima le vere e le previste, poi le in arrivo. Senza ordinamento.</summary>
    public static IReadOnlyList<RigaSpesa> Righe(SpeseDelPeriodo periodo) =>
    [
        .. periodo.Righe.Select(s => new RigaSpesa(s, periodo.Previste.Contains(s.Id) ? StatoSpesa.Prevista : StatoSpesa.Registrata)),
        .. periodo.InArrivo.Select(s => new RigaSpesa(s, StatoSpesa.InArrivo))
    ];

    public static int QuantePreviste(IEnumerable<RigaSpesa> righe) => righe.Count(r => r.Stato == StatoSpesa.Prevista);

    /// <summary>Le date (a mezzanotte) del periodo, estremi compresi. <c>UltimiTreMesi</c> è il mese
    /// in corso e i due precedenti. <c>Libero</c> con una data nulla o con <c>da &gt; a</c> torna a
    /// questo mese; così un estremo fuori da <see cref="DataRagionevole"/>, perché un periodo di secoli
    /// farebbe generare a <c>CalcoliRicorrenti.Dovuti</c> un'occorrenza per mese.</summary>
    public static (DateTime Da, DateTime A) Intervallo(TipoPeriodo tipo, DateTime oggi, DateTime? da, DateTime? a)
    {
        var inizioMese = new DateTime(oggi.Year, oggi.Month, 1);
        var fineMese = inizioMese.AddMonths(1).AddDays(-1);

        return tipo switch
        {
            TipoPeriodo.MeseScorso => (inizioMese.AddMonths(-1), inizioMese.AddDays(-1)),
            TipoPeriodo.UltimiTreMesi => (inizioMese.AddMonths(-2), fineMese),
            TipoPeriodo.QuestAnno => (new DateTime(oggi.Year, 1, 1), new DateTime(oggi.Year, 12, 31)),
            TipoPeriodo.Libero when DataRagionevole(da, oggi) && DataRagionevole(a, oggi) && da!.Value.Date <= a!.Value.Date
                => (da.Value.Date, a.Value.Date),
            _ => (inizioMese, fineMese)
        };
    }

    public static IReadOnlyList<RigaSpesa> Filtra(IReadOnlyList<RigaSpesa> righe, FiltriSpese filtri)
    {
        var cercato = Normalizza(filtri.Testo);

        return righe
            .Where(r => filtri.Categorie.Count == 0 || filtri.Categorie.Contains(r.Spesa.Category))
            .Where(r => filtri.Pagante is null || r.Spesa.PaidBy == filtri.Pagante)
            .Where(r => filtri.Stati.Count == 0 || filtri.Stati.Contains(r.Stato))
            .Where(r => cercato.Length == 0 || Normalizza(r.Spesa.Description).Contains(cercato, StringComparison.Ordinal))
            .ToList();
    }

    private static string Normalizza(string testo) => SchemaCampi.RiduciAccenti(testo.Trim()).ToUpperInvariant();

    /// <summary>Solo una riga registrata, e solo se <see cref="Permessi.PuoIntervenire"/> lo consente:
    /// la regola sta in <c>Permessi</c> e non si riscrive.</summary>
    public static bool Modificabile(RigaSpesa riga, Guid? mioId, IReadOnlyList<Space> spazi) =>
        riga.Stato == StatoSpesa.Registrata
        && Permessi.PuoIntervenire(mioId, riga.Spesa.PaidBy, riga.Spesa.SpaceId, spazi);

    /// <summary>La stessa guardia di <c>Pages/SpesaEdit.razor</c> (<c>DataRagionevole</c>): intercetta
    /// una data digitata male, non valida il dominio.</summary>
    public static bool DataRagionevole(DateTime? data, DateTime oggi) =>
        data is not null && data.Value >= new DateTime(2000, 1, 1) && data.Value <= oggi.AddYears(1);

    /// <summary>La spesa con la sola cella <paramref name="colonna"/> cambiata, oppure l'errore che
    /// dice perché non parte. Non modifica mai <paramref name="spesa"/>: restituisce una copia, con la
    /// <c>Version</c> di prima, perché è quella a fare da filtro nell'aggiornamento. Una colonna non
    /// modificabile fallisce chiusa.</summary>
    public static EsitoModifica Applica(Expense spesa, string colonna, string testo, DateTime oggi)
    {
        switch (colonna)
        {
            case Colonne.Importo:
                var esito = Denaro.Verifica(testo, out var importo);
                if (esito != EsitoImporto.Valido)
                    return new EsitoModifica(null, Testi.MessaggioImporto(esito) ?? "Scrivi un importo.");
                var conImporto = Copia(spesa);
                conImporto.Amount = importo;
                return new EsitoModifica(conImporto, null);

            case Colonne.Descrizione:
                var descrizione = testo.Trim();
                if (descrizione.Length == 0)
                    return new EsitoModifica(null, "La descrizione non può restare vuota.");
                if (descrizione.Length > LunghezzaMassimaDescrizione)
                    return new EsitoModifica(null, "La descrizione può avere al massimo 200 caratteri.");
                var conDescrizione = Copia(spesa);
                conDescrizione.Description = descrizione;
                return new EsitoModifica(conDescrizione, null);

            case Colonne.Data:
                if (!DateTime.TryParseExact(testo, FormatoData,CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                    return new EsitoModifica(null, "Scrivi una data valida.");
                if (!DataRagionevole(data, oggi))
                    return new EsitoModifica(null, "L'anno non è verosimile: dev'essere fra il 2000 e l'anno prossimo.");
                var conData = Copia(spesa);
                conData.SpentOn = data;
                return new EsitoModifica(conData, null);

            case Colonne.Categoria:
                // Una categoria uscita dall'elenco dopo che la spesa è stata segnata: riconfermarla non è un errore.
                if (!CategorieSpesa.Conosciuta(testo) && testo != spesa.Category)
                    return new EsitoModifica(null, "Scegli una categoria dall'elenco.");
                var conCategoria = Copia(spesa);
                conCategoria.Category = testo;
                return new EsitoModifica(conCategoria, null);

            default:
                return new EsitoModifica(null, "Questa colonna non si modifica.");
        }
    }

    private static Expense Copia(Expense s) => new()
    {
        Id = s.Id, SpaceId = s.SpaceId, PaidBy = s.PaidBy, RecurringId = s.RecurringId,
        RecurringPeriod = s.RecurringPeriod, Amount = s.Amount, Description = s.Description,
        Category = s.Category, SpentOn = s.SpentOn, Version = s.Version, CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };

    /// <summary>«registrata», «prevista», «in arrivo»; una registrata nata da una regola ricorrente
    /// porta il segno «registrata · ricorrente».</summary>
    public static string TestoStato(RigaSpesa riga) => riga.Stato switch
    {
        StatoSpesa.Registrata => riga.Spesa.RecurringId is null ? "registrata" : "registrata · ricorrente",
        StatoSpesa.Prevista => "prevista",
        _ => "in arrivo"
    };

    /// <summary>La frase che racconta l'esito di un'azione di massa: se sono passate tutte, il
    /// conteggio; altrimenti quante su quante e i motivi di chi è rimasto fuori.</summary>
    public static string TestoEsitoMassa(AzioneMassa azione, EsitoMassa esito)
    {
        var (singolare, plurale) = azione == AzioneMassa.Elimina ? ("eliminata", "eliminate") : ("modificata", "modificate");

        if (esito.Toccate == esito.Richieste)
            return Testi.Conteggio(esito.Toccate, $"spesa {singolare}", $"spese {plurale}") + ".";

        var motivi = new List<string>();
        if (esito.Rifiutate > 0) motivi.Add(Testi.Conteggio(esito.Rifiutate, "non era tua", "non erano tue"));
        if (esito.Sparite > 0) motivi.Add(Testi.Conteggio(esito.Sparite, "non c'era più", "non c'erano più"));
        var elenco = string.Join(", ", motivi);

        return esito.Toccate == 0
            ? $"Nessuna {singolare}: {elenco}."
            : $"{esito.Toccate} su {esito.Richieste} {plurale}: {elenco}.";
    }
}
