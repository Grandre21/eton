# Spese in tabella (fase 2.2) — piano di implementazione

> **Per chi esegue:** questo piano si esegue con l'**architettura a sessioni-unità** del progetto
> (`~/.claude/architettura-sessioni.md`) e, dentro ogni unità, con il protocollo di implementazione
> (`~/.claude/protocollo.md`): ogni task diventa uno o più brief per un `implementer`, revisionati
> secondo i gate del §3. Gli step con `- [ ]` sono la traccia interna del mandato. La partizione in
> unità proposta è in fondo, alla sezione «Partizione proposta in sessioni-unità».

**Obiettivo:** una pagina `/expenses/table`, solo su schermo largo, in cui le spese di uno spazio si
trovano, si modificano e si esportano come in un foglio di calcolo, senza toccare il flusso del telefono.

**Architettura:** tre strati (spec §2). Calcoli puri e generici su `IReadOnlyList<T>` accanto a
`CalcoliSpese`; un componente generico `Griglia<T>` guidato da descrittori di colonna, con un piccolo
modulo JavaScript per il fuoco; la pagina `SpeseTabella.razor`, che fornisce colonne, salvataggio, azioni
di massa e lettura — **solo** dal percorso unico `ElencaConPrevisteAsync`. Il tutorial è un quarto pezzo
generico, riusabile dalla 2.1-bis.

**Tecnologie:** Blazor WebAssembly .NET 10 (`InvariantGlobalization`, `TrimMode=full`), Supabase /
PostgreSQL 17, `Supabase.Postgrest` 4.4.0, xUnit. **Nessuna dipendenza nuova.**

**Specifica:** `docs/superpowers/specs/2026-09-22-spese-tabella-design.md` — si legge **insieme** a
questo piano, non al posto suo. Le decisioni dell'utente che il piano rispetta stanno in
`handoff/PIANO.md`, sezione `DECISIONI` (22 settembre e 7 ottobre).

## ⚠️ Correzioni del 7 ottobre 2026

La spec è del 22 settembre; la 2.1 è stata implementata e pubblicata dopo (unità 01-03, chiusa il 7
ottobre). Il piano è scritto sul codice del 7 ottobre (`9709e9f`), e dove la spec presuppone un codice
che non c'è, o tace su un punto che il codice rende delicato, la correzione è **qui**, marcata nel testo
come `[corretto 7 ott]`. La spec non si modifica.

1. **Test**: oggi sono **335**. I gate sono 335 → 349 (task 1) → 377 (task 2) → 385 (task 4).
2. **Nessuna migrazione.** Tutto ciò che la 2.2 scrive ha già i privilegi: l'UPDATE su `category`,
   `amount`, `description`, `spent_on` (commento in `Models/Expense.cs:25-26`), le policy
   `expenses_update`/`expenses_delete` (`paid_by = auth.uid() or is_space_owner(space_id)`, citate in
   `Services/Permessi.cs:14-16`). Quindi **niente gate dell'utente** fra un task e l'altro, a differenza
   della 2.1. Se un'unità scopre di aver bisogno di schema, si ferma `BLOCKED` e applica la sequenza della
   2.1: migrazione additiva scritta e non applicata → l'utente la applica e lo dice in chat → solo dopo il
   codice che la usa.
3. **Gli id delle previste cambiano a ogni lettura.** `CalcoliRicorrenti.Occorrenza`
   (`Services/CalcoliRicorrenti.cs:136`) assegna `Id = Guid.NewGuid()` a ogni fusione. La spec §5.2 chiede
   di ricordare la cella attiva «per chiave di riga»: se la chiave fosse `Id`, ogni rilettura cambierebbe
   la chiave di ogni prevista e in arrivo, e il fuoco si perderebbe proprio nel caso che la spec dichiara
   rischioso. La chiave di riga è quindi `Id` per le registrate e `"<recurring_id>:<yyyy-MM>"` per previste
   e in arrivo (task 2, `RigaSpesa.Chiave`).
4. **`InvariantGlobalization` rende ordinale ogni confronto «culturale»** (`Eton.csproj:10`, e i test
   lo replicano in `Eton.Tests/Eton.Tests.csproj:11`). `string.Compare` con una cultura qualunque mette
   «Èrba» dopo «zucchero». La spec §7 chiede l'ordinamento del «testo con accenti»: si ottiene riducendo
   gli accenti con la tabella che il progetto ha già, `SchemaCampi.RiduciAccenti`
   (`Services/SchemaCampi.cs:177`), che da `private` diventa `internal`. Lo stesso per il filtro di testo.
5. **I tipi di colonna.** La spec §2.2 dice «quelli di `SchemaCampi.TipiAmmessi` più `money`» ma ne elenca
   cinque; `TipiAmmessi` (`Services/SchemaCampi.cs:26`) oggi ne ha sei (`text`, `number`, `select`,
   `date`, `bool`, `url`). La 2.2 dichiara solo i cinque della spec — `Testo`, `Numero`, `Data`,
   `Selezione`, `Denaro` —: `bool` e `url` li aggiunge la fase 3 insieme all'adattatore, quando esiste un
   chiamante che li usa.
6. **Il JavaScript non è uno solo.** La spec §5.2 dice che il modulo del fuoco è «l'unico JavaScript della
   pagina». Ne servono due, entrambi generici e caricati come modulo ES come `wwwroot/js/grafo-spazio.js`
   (`Shared/GrafoSpazio.razor:39`): `griglia.js` (fuoco, `preventDefault` sui tasti, scaricamento del CSV —
   Blazor non ha un modo nativo di far scaricare un file) e `tutorial.js` (porta in vista e marca un
   elemento fuori dal componente, che Blazor non raggiunge senza un `ElementReference`). Il commento di
   `Shared/TestataPagina.razor:8-10` («il progetto non ha un file .js proprio») è già falso oggi: fuori
   scope, segnalato.
7. **Il conteggio dell'eliminazione multipla** è «presenti prima ∖ presenti dopo» (ratificato il 7 ottobre),
   non «richiesti ∖ presenti dopo»: se la RLS filtra la `select`, la sola rilettura finale direbbe «tutte
   eliminate» su righe ancora lì. Ne segue una correzione all'ultima frase della spec §5.4 («una riga
   cancellata da un altro membro nel frattempo conta fra le eliminate»): una riga già assente nella
   lettura **prima** non si conta fra le eliminate, si dice a parte («non c'era più»). È la stessa onestà
   di `ExpenseRepository.EliminaAsync` (`Services/ExpenseRepository.cs:289-300`), che fa lettura prima,
   DELETE e rilettura.
8. **Anche «Cambia categoria» distingue i due motivi.** La spec §5.4 scrive «12 su 15 modificate: 3 non
   erano tue», ma le righe non restituite dall'UPDATE possono anche essere sparite. Una rilettura delle
   sole non toccate (una query, e solo se ce ne sono) separa «non erano tue» da «non c'erano più».
9. **Un testo che comincia con `=`, `+`, `-` o `@` diventa una formula in Excel.** La spec §4.4 non ne
   parla; negli spazi condivisi la descrizione la scrive un altro membro, e l'esportazione la apre chi
   esporta. I campi di tipo `Testo` e `Selezione` che cominciano con uno di quei caratteri escono nel CSV
   preceduti da un apostrofo. Gli importi e le date li formatta il codice, non l'utente, e non si toccano.
   **Da ratificare.**
10. **La guardia sulla data esiste già in tre copie** con due forme diverse (`Pages/Spese.razor:265`,
    `Pages/SpesaEdit.razor:216-218`, `Pages/RicorrenteEdit.razor:246-248`). La tabella non ne aggiunge una
    quarta in pagina: la forma di `SpesaEdit` diventa `TabellaSpese.DataRagionevole`, pura e testata.
    Riunire le tre copie esistenti è fuori scope.
11. **`ConfermaAzione` si disarma solo quando cambia `Chiave`** (`Shared/ConfermaAzione.razor:47,53-60`).
    Per l'eliminazione multipla `Chiave` è l'impronta della selezione: senza, una conferma armata su 3 righe
    sopravvive a un cambio di selezione e cancella le 8 nuove senza che nessuno le abbia confermate.
12. **La sotto-navigazione esiste già** in `Shared/NavigazioneSpese.razor:24` con `Visibile: false`. Si
    accende nel **task 5**, ultimo step, dopo che `live-testing` della pagina è tornato verde.

## Vincoli globali

Valgono per **ogni** task e non si ripetono dentro ciascuno: chi scrive un brief li ricopia.

- **Nessuna dipendenza nuova.** Il sito sta su GitHub Pages.
- **`main` pubblica in produzione a ogni push, e l'app di sviluppo punta al database di produzione.**
  Ogni prova nel browser lavora **solo su righe create per la prova** (descrizione che comincia con
  `[collaudo]`) e le elimina a fine prova. Mai modificare o eliminare righe vere dell'utente.
- **Gate di ogni task:** `mcp__synapse__build` → 0 errori, 0 avvisi; `mcp__synapse__test` → tutti verdi
  (in un worktree con `root=<worktree>`). `dotnet build|test` in shell lo nega
  `hooks/build-via-synapse.ps1`; solo se il tool synapse fallisce si rilancia con `SYNAPSE_DOWN=1` davanti,
  detto nel resoconto. Erano **335** prima di questo lavoro.
- **Gli `implementer` non compilano e non eseguono interpreti né script**: `obj/` non ha lock fra processi.
  Compila l'esecutore dell'unità, una volta, a fine giro.
- **Si testa la logica pura, non il repository né le pagine** (design del 24 agosto §9). Tutto ciò che
  può essere puro lo è: ogni task con logica la mette in una classe statica e la testa.
- **La tabella legge solo da `ExpenseRepository.ElencaConPrevisteAsync`** (`Services/ExpenseRepository.cs:82`).
  `ElencaAsync` è già `private` (`:63`), quindi il compilatore impedisce la chiamata diretta: un brief non
  deve proporre di renderlo `internal` o `public` per nessun motivo.
- **Mai dialoghi nativi** (`confirm()`, `alert()`): le conferme passano da `ConfermaAzione`. Bloccano anche
  il plugin del browser con cui si collauda.
- **`wwwroot/css/app.css` ha un solo proprietario alla volta.** Lo tocca solo il task 6. Gli altri usano
  CSS isolato (`*.razor.css`, come `Shared/GrafoSpazio.razor.css` e `Pages/Benvenuto.razor.css`) e i
  token esistenti di `:root` (`--accento`, `--bordo`, `--superficie-alta`, `--tocco`, `--transizione`…).
- **Resa visiva minima sui token esistenti, dichiarata provvisoria** (decisione del 22 settembre, spec §1):
  la rifà la 2.1-bis. Il criterio del confine è della spec: ciò che un test o un lettore di schermo può
  osservare sta qui, ciò che cambia solo una variabile o una regola CSS è della 2.1-bis. Per il §0 del
  protocollo un brief che crea markup ha comunque il blocco `PIANO-DESIGN` della skill `frontend-design`,
  **ristretto ai valori già presenti in `app.css`**; i rilievi `TIPO: progetto` di `ui-critic` vanno alla
  2.1-bis, non si correggono qui.
- **Testo accentato**: mai `printf`, `echo -e` o `git commit -m`. Heredoc quotato o `git commit -F` su
  file UTF-8; i file si scrivono con `Write`.
- **`InvariantGlobalization`**: nessuna cultura esiste. Date e numeri si formattano con
  `CultureInfo.InvariantCulture` e le funzioni del progetto (`Denaro.Testo`, `Denaro.TestoDigitabile`,
  `Testi.DataSola`), mai con `ToString("C")` o `ToString("d")`.

---

## Struttura dei file

| File | Responsabilità | Task |
|---|---|---|
| `Services/ColonnaGriglia.cs` | **creare** — i descrittori generici: `ColonnaGriglia<T>`, `TipoColonna`, `Aggregazione`, `Gruppo<T>`, `Riepilogo` | 1 |
| `Services/CalcoliGriglia.cs` | **creare** — ordinare, raggruppare, aggregare, CSV, intervallo di selezione, resa di una cella | 1 |
| `Services/SchemaCampi.cs` | **modificare** — `RiduciAccenti` da `private` a `internal` `[corretto 7 ott]` | 1 |
| `Eton.Tests/CalcoliGrigliaTests.cs` | **creare** — 14 test | 1 |
| `Services/TabellaSpese.cs` | **creare** — la parte pura specifica delle spese: righe e stati, periodi, filtri, vista nell'URL, modifica di una cella, esito delle azioni di massa | 2 |
| `Eton.Tests/TabellaSpeseTests.cs` | **creare** — 28 test | 2 |
| `Services/ExpenseRepository.cs` | **modificare** — `CambiaCategoriaAsync`, `EliminaTutteAsync` | 3 |
| `Services/TastieraGriglia.cs` | **creare** — la tastiera come in Excel, pura | 4 |
| `Eton.Tests/TastieraGrigliaTests.cs` | **creare** — 8 test | 4 |
| `Shared/Griglia.razor` | **creare** — il componente generico `Griglia<T>` | 4 |
| `Shared/Griglia.razor.css` | **creare** — resa minima e provvisoria della griglia | 4 |
| `wwwroot/js/griglia.js` | **creare** — fuoco, `preventDefault` sui tasti, scaricamento | 4 |
| `Pages/SpeseTabella.razor` | **creare** — la pagina `/expenses/table` | 5 (6 aggiunge il tutorial) |
| `Pages/SpeseTabella.razor.css` | **creare** — l'avviso sotto i `40rem` e la barra degli strumenti | 5 |
| `Shared/NavigazioneSpese.razor` | **modificare** — la voce «Tabella» si accende | 5 |
| `Shared/Tutorial.razor` | **creare** — il componente generico del tutorial guidato | 6 |
| `wwwroot/js/tutorial.js` | **creare** — porta in vista e marca l'ancora | 6 |
| `wwwroot/css/app.css` | **modificare** — la sola classe globale `.tutorial-evidenziato` | 6 |

Il puro viene per primo di proposito, come nella 2.1: non dipende da nulla, si prova senza database e
fissa i nomi che tutto il resto consuma.

---

## Task 1 — I calcoli generici della griglia

**File:**
- Creare: `Services/ColonnaGriglia.cs`, `Services/CalcoliGriglia.cs`
- Modificare: `Services/SchemaCampi.cs:177` (solo la visibilità di `RiduciAccenti`)
- Test: `Eton.Tests/CalcoliGrigliaTests.cs`

**Interfacce:**
- *Consuma*: `Denaro.Testo` e `Denaro.TestoDigitabile` (`Services/Denaro.cs:123,151`),
  `Testi.DataSola` (`Services/Testi.cs:90`), `SchemaCampi.RiduciAccenti` (`Services/SchemaCampi.cs:177`).
- *Produce*, e i task 2, 4, 5 vi si appoggiano:
  ```csharp
  namespace Eton.Services;

  public enum TipoColonna { Testo, Numero, Data, Selezione, Denaro }

  [Flags]
  public enum Aggregazione { Nessuna = 0, Somma = 1, Media = 2, Min = 4, Max = 8, Conteggio = 16 }

  /// Chiave stabile (anche nell'URL), etichetta, tipo e la funzione che legge il valore dalla riga.
  /// Valore è una funzione e non un nome di proprietà: è il gancio della fase 4 (spec §2.2).
  public sealed record ColonnaGriglia<T>(string Chiave, string Etichetta, TipoColonna Tipo, Func<T, object?> Valore)
  {
      public Aggregazione Aggregazioni { get; init; }
      public bool Ordinabile { get; init; } = true;
      public Func<T, bool>? Modificabile { get; init; }        // null = colonna mai modificabile
      public IReadOnlyList<string> Opzioni { get; init; } = []; // per Selezione
  }

  public sealed record Gruppo<T>(string? Etichetta, IReadOnlyList<T> Righe);   // Etichetta null = nessun raggruppamento
  public sealed record Riepilogo(int Conteggio, decimal Somma, decimal? Media, decimal? Min, decimal? Max);

  public static class CalcoliGriglia
  {
      public static IReadOnlyList<T> Ordina<T>(IReadOnlyList<T> righe, ColonnaGriglia<T> colonna, bool decrescente);
      public static IReadOnlyList<Gruppo<T>> Raggruppa<T>(IReadOnlyList<T> righe, Func<T, string>? chiave);
      public static Riepilogo Aggrega<T>(IReadOnlyList<T> righe, Func<T, decimal> valore);
      public static string Csv<T>(IReadOnlyList<ColonnaGriglia<T>> colonne, IReadOnlyList<T> righe);
      public static IReadOnlyList<string> Intervallo(IReadOnlyList<string> chiaviInOrdine, string ancora, string fine);
      public static string TestoCella(TipoColonna tipo, object? valore);
      public static string TestoModificabile(TipoColonna tipo, object? valore);
  }
  ```
  **Lo stato di modifica delle celle non sta nei descrittori** (spec §2.2): sta nel componente del task 4.

- [ ] **Step 1: scrivere i test che falliscono**

```csharp
using Eton.Services;

namespace Eton.Tests;

public class CalcoliGrigliaTests
{
    private sealed record Riga(string Nome, decimal Importo, DateTime Quando, int Pos);

    private static readonly ColonnaGriglia<Riga> Nome = new("nome", "Nome", TipoColonna.Testo, r => r.Nome);
    private static readonly ColonnaGriglia<Riga> Importo = new("importo", "Importo", TipoColonna.Denaro, r => r.Importo)
    {
        Aggregazioni = Aggregazione.Somma | Aggregazione.Media | Aggregazione.Min | Aggregazione.Max | Aggregazione.Conteggio
    };
    private static readonly ColonnaGriglia<Riga> Quando = new("data", "Data", TipoColonna.Data, r => r.Quando);

    private static Riga R(string nome, decimal importo = 1m, int giorno = 1, int pos = 0)
        => new(nome, importo, new DateTime(2026, 9, giorno), pos);

    [Fact]
    public void Il_testo_si_ordina_ignorando_accenti_e_maiuscole()
    {
        // Sotto InvariantGlobalization un confronto "culturale" è ordinale: senza riduzione degli
        // accenti "Èrba" finirebbe dopo "zucchero".
        var righe = new[] { R("zucchero"), R("Èrba"), R("acqua"), R("erba") };

        var ordinate = CalcoliGriglia.Ordina(righe, Nome, decrescente: false);

        Assert.Equal(["acqua", "Èrba", "erba", "zucchero"], ordinate.Select(r => r.Nome));
    }

    [Fact]
    public void A_parita_resta_l_ordine_di_partenza_anche_in_decrescente()
    {
        var righe = new[] { R("a", 5m, pos: 1), R("b", 5m, pos: 2), R("c", 9m, pos: 3) };

        var ordinate = CalcoliGriglia.Ordina(righe, Importo, decrescente: true);

        Assert.Equal([3, 1, 2], ordinate.Select(r => r.Pos));
    }

    [Fact]
    public void Importi_e_date_si_ordinano_per_valore_non_per_testo()
    {
        var righe = new[] { R("a", 1284.50m, 10), R("b", 99m, 2), R("c", 1000m, 30) };

        Assert.Equal(["b", "c", "a"], CalcoliGriglia.Ordina(righe, Importo, false).Select(r => r.Nome));
        Assert.Equal(["c", "a", "b"], CalcoliGriglia.Ordina(righe, Quando, true).Select(r => r.Nome));
    }

    [Fact]
    public void I_gruppi_seguono_l_ordine_delle_righe()
    {
        var righe = new[] { R("b1"), R("a1"), R("b2") };

        var gruppi = CalcoliGriglia.Raggruppa(righe, r => r.Nome[..1]);

        Assert.Equal(["b", "a"], gruppi.Select(g => g.Etichetta));
        Assert.Equal(["b1", "b2"], gruppi[0].Righe.Select(r => r.Nome));
    }

    [Fact]
    public void Senza_chiave_c_e_un_solo_gruppo_senza_etichetta()
    {
        var gruppi = CalcoliGriglia.Raggruppa(new[] { R("a"), R("b") }, chiave: null);

        var unico = Assert.Single(gruppi);
        Assert.Null(unico.Etichetta);
        Assert.Equal(2, unico.Righe.Count);
    }

    [Fact]
    public void Nessuna_riga_nessun_gruppo()
    {
        Assert.Empty(CalcoliGriglia.Raggruppa(Array.Empty<Riga>(), r => r.Nome));
        Assert.Empty(CalcoliGriglia.Raggruppa(Array.Empty<Riga>(), chiave: null));
    }

    [Fact]
    public void Le_cinque_aggregazioni()
    {
        var riepilogo = CalcoliGriglia.Aggrega(new[] { R("a", 10m), R("b", 20m), R("c", 40m) }, r => r.Importo);

        // 70 / 3 = 23,333… arrotondato a due decimali, lontano dallo zero.
        Assert.Equal(new Riepilogo(3, 70m, 23.33m, 10m, 40m), riepilogo);
    }

    [Fact]
    public void Zero_righe_niente_media_niente_minimo_niente_massimo()
    {
        Assert.Equal(new Riepilogo(0, 0m, null, null, null),
            CalcoliGriglia.Aggrega(Array.Empty<Riga>(), r => r.Importo));
    }

    [Fact]
    public void Una_riga_e_media_minimo_e_massimo_di_se_stessa()
    {
        Assert.Equal(new Riepilogo(1, 7.5m, 7.5m, 7.5m, 7.5m),
            CalcoliGriglia.Aggrega(new[] { R("a", 7.5m) }, r => r.Importo));
    }

    [Fact]
    public void Il_csv_e_quello_che_l_excel_italiano_apre_direttamente()
    {
        var csv = CalcoliGriglia.Csv([Nome, Importo, Quando], new[] { R("Pane", 1284.5m, 3) });

        // BOM, punto e virgola, virgola decimale senza migliaia, data gg/mm/aaaa, a capo CRLF.
        Assert.Equal("﻿Nome;Importo;Data\r\nPane;1284,50;03/09/2026\r\n", csv);
    }

    [Fact]
    public void I_campi_con_separatore_virgolette_o_a_capo_si_racchiudono()
    {
        var csv = CalcoliGriglia.Csv([Nome], new[] { R("a;b"), R("di \"Mario\""), R("riga\nnuova") });

        Assert.Equal("﻿Nome\r\n\"a;b\"\r\n\"di \"\"Mario\"\"\"\r\n\"riga\nnuova\"\r\n", csv);
    }

    [Fact]
    public void Un_testo_che_sembra_una_formula_non_diventa_una_formula()
    {
        var csv = CalcoliGriglia.Csv([Nome], new[] { R("=SOMMA(A1)"), R("+39 333"), R("-sconto"), R("@capo") });

        Assert.Equal("﻿Nome\r\n'=SOMMA(A1)\r\n'+39 333\r\n'-sconto\r\n'@capo\r\n", csv);
    }

    [Fact]
    public void Shift_clic_seleziona_l_intervallo_in_entrambe_le_direzioni()
    {
        string[] chiavi = ["a", "b", "c", "d", "e"];

        Assert.Equal(["b", "c", "d"], CalcoliGriglia.Intervallo(chiavi, "b", "d"));
        Assert.Equal(["b", "c", "d"], CalcoliGriglia.Intervallo(chiavi, "d", "b"));
        // L'ancora non c'è più (filtrata via): si seleziona la sola riga cliccata.
        Assert.Equal(["e"], CalcoliGriglia.Intervallo(chiavi, "sparita", "e"));
    }

    [Fact]
    public void Resa_e_testo_modificabile_non_si_confondono()
    {
        // Denaro.Testo mette il punto delle migliaia, che Denaro.Verifica rifiuta in ingresso:
        // in un campo modificabile va TestoDigitabile (v. il commento in Services/Denaro.cs:113-120).
        Assert.Equal("1.284,50", CalcoliGriglia.TestoCella(TipoColonna.Denaro, 1284.5m));
        Assert.Equal("1284,50", CalcoliGriglia.TestoModificabile(TipoColonna.Denaro, 1284.5m));
        Assert.Equal("03/09/2026", CalcoliGriglia.TestoCella(TipoColonna.Data, new DateTime(2026, 9, 3)));
        Assert.Equal("2026-09-03", CalcoliGriglia.TestoModificabile(TipoColonna.Data, new DateTime(2026, 9, 3)));
        Assert.Equal("", CalcoliGriglia.TestoCella(TipoColonna.Testo, null));
    }
}
```

- [ ] **Step 2: verificare che falliscano**

Comando: `mcp__synapse__test` con filtro `CalcoliGrigliaTests` (`root=<worktree>`).
Atteso: **errore di compilazione** — `CalcoliGriglia` non esiste. È il fallimento giusto.

- [ ] **Step 3: scrivere l'implementazione minima**

Una classe statica sullo stile di `Services/CalcoliSpese.cs`: nessuna rete, nessuno stato. Regole, tutte
deducibili dai test:

- **`Ordina`**: `OrderBy`/`OrderByDescending` di LINQ, che sono **stabili** — è la ragione per cui non si
  scrive un ordinamento a mano. Il confronto dipende dal tipo: `Testo` e `Selezione` confrontano
  `SchemaCampi.RiduciAccenti(s).ToUpperInvariant()` con `StringComparer.Ordinal`; `Numero` e `Denaro`
  come `decimal`; `Data` come `DateTime`. Un valore `null` è il più piccolo. Un comparatore privato, non
  una classe pubblica.
- **`Raggruppa`**: chiave nulla → un solo gruppo con `Etichetta` null, purché ci sia almeno una riga.
  Altrimenti i gruppi nell'ordine della **prima comparsa** della chiave nelle righe già ordinate: chi
  raggruppa per mese con la data decrescente vede i mesi recenti per primi, chi ordina per categoria e
  raggruppa per categoria li vede in ordine alfabetico. Nessun gruppo vuoto, per costruzione.
- **`Aggrega`**: `Media` = `Math.Round(somma / n, 2, MidpointRounding.AwayFromZero)`, come
  `CalcoliSpese.PerMese` (`Services/CalcoliSpese.cs:58`). A zero righe somma 0 e il resto `null`: una
  media di niente non è zero.
- **`Csv`**: `﻿` in testa; intestazione con le `Etichetta`; `;` come separatore; `\r\n` a fine riga,
  anche l'ultima. Ogni campo passa per `TestoCsv`: `Denaro` → `Denaro.TestoDigitabile` (senza migliaia,
  così Excel lo legge come numero), `Data` → `dd/MM/yyyy`, `Numero` → invariante con la virgola. Poi
  `[corretto 7 ott]`: un campo `Testo`/`Selezione` che comincia con `=`, `+`, `-`, `@` prende un `'`
  davanti. Infine un campo che contiene `;`, `"`, `\r` o `\n` si racchiude fra virgolette, con le
  virgolette interne raddoppiate. L'apostrofo va **prima** delle virgolette.
- **`Intervallo`**: gli indici di `ancora` e `fine`; se `ancora` manca, `[fine]`; altrimenti le chiavi fra
  i due indici, estremi inclusi, nell'ordine dato.
- **`TestoCella`**: `Denaro` → `Denaro.Testo`; `Data` → `Testi.DataSola`; `Numero` → invariante con la
  virgola; il resto `ToString()`; `null` → `""`. **`TestoModificabile`**: `Denaro` →
  `Denaro.TestoDigitabile`; `Data` → `yyyy-MM-dd` (il formato del valore di un `<input type="date">`); il
  resto come `TestoCella`.
- In `Services/SchemaCampi.cs:177` `private static string RiduciAccenti` diventa `internal static`, e
  nient'altro di quel file cambia.

Budget: due file nuovi, nessun'altra astrazione. Il comparatore è un metodo privato, non un `IComparer`
pubblico.

- [ ] **Step 4: verificare che passino**

`mcp__synapse__test` con filtro `CalcoliGrigliaTests`. Atteso: 14 superati.

- [ ] **Step 5: il gate completo**

`mcp__synapse__build` poi `mcp__synapse__test`. Atteso: 0 avvisi; **349** superati (335 + 14).

- [ ] **Step 6: commit**

---

## Task 2 — La parte pura delle spese

**File:**
- Creare: `Services/TabellaSpese.cs`
- Test: `Eton.Tests/TabellaSpeseTests.cs`

**Interfacce:**
- *Consuma*: `SpeseDelPeriodo` (`Services/CalcoliRicorrenti.cs:15-16`), `Permessi.PuoIntervenire`
  (`Services/Permessi.cs:63`), `Denaro.Verifica` (`Services/Denaro.cs:66`), `Testi.MessaggioImporto`
  (`Services/Testi.cs:105`), `Testi.Conteggio` (`Services/Testi.cs:34`), `CategorieSpesa.Elenco` e
  `Conosciuta` (`Services/CategorieSpesa.cs:16,25`), `SchemaCampi.RiduciAccenti` (task 1),
  `CalcoliSpese.NomeMese` (`Services/CalcoliSpese.cs:37`).
- *Produce*, e i task 3, 5 e 6 vi si appoggiano:
  ```csharp
  namespace Eton.Services;

  public enum StatoSpesa { Registrata, Prevista, InArrivo }

  public sealed record RigaSpesa(Expense Spesa, StatoSpesa Stato)
  {
      // Id per le registrate; "<recurring_id>:<yyyy-MM>" per previste e in arrivo  [corretto 7 ott]
      public string Chiave { get; }
      public bool ContaNeiTotali => Stato != StatoSpesa.InArrivo;
  }

  public enum TipoPeriodo { QuestoMese, MeseScorso, UltimiTreMesi, QuestAnno, Libero }

  public sealed record FiltriSpese(IReadOnlySet<string> Categorie, Guid? Pagante, string Testo, IReadOnlySet<StatoSpesa> Stati)
  {
      public static FiltriSpese Nessuno { get; }
  }

  public sealed record VistaTabella(TipoPeriodo Periodo, DateTime? Da, DateTime? A, FiltriSpese Filtri,
      string OrdinaPer, bool Decrescente, string? RaggruppaPer)
  {
      public static VistaTabella Predefinita { get; }      // questo mese, nessun filtro, data decrescente
      public static VistaTabella DaQuery(string query);    // mai lancia: un parametro illeggibile torna al default
      public string InQuery();                             // "" per la predefinita, altrimenti "?…"
  }

  public sealed record EsitoModifica(Expense? Nuova, string? Errore);

  public enum AzioneMassa { CambiaCategoria, Elimina }

  public sealed record EsitoMassa(int Richieste, int Toccate, int Rifiutate, int Sparite)
  {
      public static EsitoMassa DaEliminazione(IReadOnlyCollection<Guid> richieste,
          IReadOnlySet<Guid> presentiPrima, IReadOnlySet<Guid> presentiDopo);
      public static EsitoMassa DaModifica(IReadOnlyCollection<Guid> richieste,
          IReadOnlySet<Guid> toccate, IReadOnlySet<Guid> nonToccateAncoraPresenti);
  }

  public static class TabellaSpese
  {
      public static class Colonne
      {
          public const string Data = "data", Descrizione = "descrizione", Categoria = "categoria",
              Importo = "importo", Pagante = "pagante", Stato = "stato";
      }
      public static class Gruppi { public const string Categoria = "categoria", Pagante = "pagante", Mese = "mese"; }

      public static IReadOnlyList<RigaSpesa> Righe(SpeseDelPeriodo periodo);
      public static int QuantePreviste(IEnumerable<RigaSpesa> righe);
      public static (DateTime Da, DateTime A) Intervallo(TipoPeriodo tipo, DateTime oggi, DateTime? da, DateTime? a);
      public static IReadOnlyList<RigaSpesa> Filtra(IReadOnlyList<RigaSpesa> righe, FiltriSpese filtri);
      public static bool Modificabile(RigaSpesa riga, Guid? mioId, IReadOnlyList<Space> spazi);
      public static bool DataRagionevole(DateTime? data, DateTime oggi);
      public static EsitoModifica Applica(Expense spesa, string colonna, string testo, DateTime oggi);
      public static string TestoStato(RigaSpesa riga);
      public static string TestoEsitoMassa(AzioneMassa azione, EsitoMassa esito);
  }
  ```

- [ ] **Step 1: scrivere i test che falliscono**

```csharp
using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class TabellaSpeseTests
{
    private static readonly Guid Io = Guid.NewGuid();
    private static readonly Guid Altro = Guid.NewGuid();
    private static readonly Guid SpazioId = Guid.NewGuid();
    private static readonly DateTime Oggi = new(2026, 10, 7);

    private static Expense Spesa(decimal importo = 10m, string descrizione = "Pane", string categoria = "Spesa",
        int giorno = 5, Guid? pagante = null, Guid? regola = null) => new()
    {
        Id = Guid.NewGuid(),
        SpaceId = SpazioId,
        PaidBy = pagante ?? Io,
        Amount = importo,
        Description = descrizione,
        Category = categoria,
        SpentOn = new DateTime(2026, 9, giorno),
        Version = 3,
        RecurringId = regola,
        RecurringPeriod = regola is null ? null : new DateTime(2026, 9, 1)
    };

    private static RigaSpesa Reg(Expense s) => new(s, StatoSpesa.Registrata);

    private static (IReadOnlyList<RigaSpesa> Righe, Expense Prevista) TreStati()
    {
        var vera = Spesa();
        var prevista = Spesa(regola: Guid.NewGuid());
        var futura = Spesa(giorno: 28, regola: Guid.NewGuid());
        var periodo = new SpeseDelPeriodo([vera, prevista], new HashSet<Guid> { prevista.Id }, [futura]);
        return (TabellaSpese.Righe(periodo), prevista);
    }

    // --- righe e stati ---

    [Fact]
    public void Ogni_riga_porta_il_suo_stato()
    {
        var (righe, _) = TreStati();

        Assert.Equal([StatoSpesa.Registrata, StatoSpesa.Prevista, StatoSpesa.InArrivo], righe.Select(r => r.Stato));
    }

    [Fact]
    public void Le_previste_contano_nei_totali_e_le_in_arrivo_no()
    {
        var (righe, _) = TreStati();

        Assert.Equal([true, true, false], righe.Select(r => r.ContaNeiTotali));
        Assert.Equal(1, TabellaSpese.QuantePreviste(righe));
    }

    [Fact]
    public void La_chiave_di_una_prevista_non_cambia_fra_due_letture()
    {
        // CalcoliRicorrenti.Occorrenza dà un Guid nuovo a ogni fusione: la chiave non può essere l'Id.
        var regola = Guid.NewGuid();
        var primaLettura = new RigaSpesa(Spesa(regola: regola), StatoSpesa.Prevista);
        var secondaLettura = new RigaSpesa(Spesa(regola: regola), StatoSpesa.Prevista);

        Assert.NotEqual(primaLettura.Spesa.Id, secondaLettura.Spesa.Id);
        Assert.Equal(primaLettura.Chiave, secondaLettura.Chiave);
    }

    [Fact]
    public void La_chiave_di_una_registrata_e_il_suo_id()
    {
        var vera = Spesa(regola: Guid.NewGuid());   // anche se nata da una ricorrente

        Assert.Equal(vera.Id.ToString(), Reg(vera).Chiave);
    }

    // --- periodi ---

    [Fact]
    public void I_periodi_visti_dal_sette_ottobre()
    {
        Assert.Equal((new DateTime(2026, 10, 1), new DateTime(2026, 10, 31)), TabellaSpese.Intervallo(TipoPeriodo.QuestoMese, Oggi, null, null));
        Assert.Equal((new DateTime(2026, 9, 1), new DateTime(2026, 9, 30)), TabellaSpese.Intervallo(TipoPeriodo.MeseScorso, Oggi, null, null));
        Assert.Equal((new DateTime(2026, 8, 1), new DateTime(2026, 10, 31)), TabellaSpese.Intervallo(TipoPeriodo.UltimiTreMesi, Oggi, null, null));
        Assert.Equal((new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)), TabellaSpese.Intervallo(TipoPeriodo.QuestAnno, Oggi, null, null));
        Assert.Equal((new DateTime(2026, 3, 2), new DateTime(2026, 4, 9)),
            TabellaSpese.Intervallo(TipoPeriodo.Libero, Oggi, new DateTime(2026, 3, 2), new DateTime(2026, 4, 9)));
    }

    [Fact]
    public void Il_mese_scorso_di_gennaio_e_dicembre_dell_anno_prima()
    {
        Assert.Equal((new DateTime(2025, 12, 1), new DateTime(2025, 12, 31)),
            TabellaSpese.Intervallo(TipoPeriodo.MeseScorso, new DateTime(2026, 1, 15), null, null));
    }

    [Fact]
    public void Un_periodo_libero_invertito_o_incompleto_torna_a_questo_mese()
    {
        var questoMese = (new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        Assert.Equal(questoMese, TabellaSpese.Intervallo(TipoPeriodo.Libero, Oggi, new DateTime(2026, 9, 30), new DateTime(2026, 9, 1)));
        Assert.Equal(questoMese, TabellaSpese.Intervallo(TipoPeriodo.Libero, Oggi, null, new DateTime(2026, 9, 1)));
    }

    // --- filtri ---

    private static IReadOnlyList<RigaSpesa> TreRighe() =>
    [
        Reg(Spesa(descrizione: "Pane", categoria: "Spesa")),
        Reg(Spesa(descrizione: "Cena da Mario", categoria: "Ristoranti", pagante: Altro)),
        new RigaSpesa(Spesa(descrizione: "Caffè al bar", categoria: "Svago", regola: Guid.NewGuid()), StatoSpesa.Prevista),
    ];

    [Fact]
    public void I_filtri_si_combinano()
    {
        var filtri = FiltriSpese.Nessuno with { Categorie = new HashSet<string> { "Spesa", "Ristoranti" }, Testo = "MARIO" };

        var filtrate = TabellaSpese.Filtra(TreRighe(), filtri);

        Assert.Equal(["Cena da Mario"], filtrate.Select(r => r.Spesa.Description));
    }

    [Fact]
    public void Il_testo_si_cerca_senza_accenti_e_senza_maiuscole()
    {
        var filtrate = TabellaSpese.Filtra(TreRighe(), FiltriSpese.Nessuno with { Testo = "  caffe " });

        Assert.Equal(["Caffè al bar"], filtrate.Select(r => r.Spesa.Description));
    }

    [Fact]
    public void Pagante_e_stato_filtrano_ciascuno_per_conto_proprio()
    {
        Assert.Equal(["Cena da Mario"],
            TabellaSpese.Filtra(TreRighe(), FiltriSpese.Nessuno with { Pagante = Altro }).Select(r => r.Spesa.Description));
        Assert.Equal(["Caffè al bar"],
            TabellaSpese.Filtra(TreRighe(), FiltriSpese.Nessuno with { Stati = new HashSet<StatoSpesa> { StatoSpesa.Prevista } })
                .Select(r => r.Spesa.Description));
    }

    [Fact]
    public void Senza_filtri_passano_tutte()
    {
        Assert.Equal(3, TabellaSpese.Filtra(TreRighe(), FiltriSpese.Nessuno).Count);
    }

    // --- chi modifica ---

    [Fact]
    public void Modificabile_per_riga_pagante_proprietario_altro_membro_prevista()
    {
        Space[] spazi = [new() { Id = SpazioId, OwnerId = Altro }];   // lo spazio lo possiede Altro

        Assert.True(TabellaSpese.Modificabile(Reg(Spesa(pagante: Io)), Io, spazi));        // l'ho pagata io
        Assert.True(TabellaSpese.Modificabile(Reg(Spesa(pagante: Io)), Altro, spazi));     // possiede lo spazio
        Assert.False(TabellaSpese.Modificabile(Reg(Spesa(pagante: Altro)), Io, spazi));    // un altro membro
        Assert.False(TabellaSpese.Modificabile(new RigaSpesa(Spesa(pagante: Io), StatoSpesa.Prevista), Io, spazi));
        Assert.False(TabellaSpese.Modificabile(Reg(Spesa(pagante: Io)), null, spazi));     // fallisce chiuso
    }

    // --- la modifica di una cella ---

    [Fact]
    public void Un_importo_valido_cambia_solo_l_importo_e_tiene_la_versione()
    {
        var spesa = Spesa(importo: 10m);

        var esito = TabellaSpese.Applica(spesa, TabellaSpese.Colonne.Importo, "1284,5", Oggi);

        Assert.Null(esito.Errore);
        Assert.Equal(1284.5m, esito.Nuova!.Amount);
        Assert.Equal(spesa.Version, esito.Nuova.Version);
        Assert.Equal(spesa.Id, esito.Nuova.Id);
        Assert.Equal(spesa.Description, esito.Nuova.Description);
        Assert.Equal(spesa.SpentOn, esito.Nuova.SpentOn);
    }

    [Fact]
    public void Un_importo_non_valido_non_parte_e_dice_perche()
    {
        Assert.Equal(Testi.MessaggioImporto(EsitoImporto.NonNumerico),
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Importo, "abc", Oggi).Errore);
        Assert.Equal(Testi.MessaggioImporto(EsitoImporto.TroppiDecimali),
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Importo, "1,234", Oggi).Errore);
        // MessaggioImporto(Vuoto) è null di proposito (Services/Testi.cs:93-97): in una cella
        // svuotata e confermata invece è un errore, e serve una frase sua.
        Assert.Equal("Scrivi un importo.", TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Importo, "", Oggi).Errore);
        Assert.Null(TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Importo, "abc", Oggi).Nuova);
    }

    [Fact]
    public void La_descrizione_si_ripulisce_e_non_resta_vuota()
    {
        Assert.Equal("Latte", TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Descrizione, "  Latte ", Oggi).Nuova!.Description);
        Assert.Equal("La descrizione non può restare vuota.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Descrizione, "   ", Oggi).Errore);
    }

    [Fact]
    public void La_data_ha_la_stessa_guardia_del_registro()
    {
        Assert.Equal(new DateTime(2026, 9, 10),
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Data, "2026-09-10", Oggi).Nuova!.SpentOn);
        Assert.Equal("L'anno non è verosimile: dev'essere fra il 2000 e l'anno prossimo.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Data, "1999-12-31", Oggi).Errore);
        Assert.Equal("Scrivi una data valida.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Data, "", Oggi).Errore);
    }

    [Fact]
    public void La_categoria_viene_dall_elenco_o_resta_quella_che_era()
    {
        Assert.Equal("Casa", TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Categoria, "Casa", Oggi).Nuova!.Category);
        Assert.Equal("Scegli una categoria dall'elenco.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Categoria, "Inventata", Oggi).Errore);
        // Una categoria uscita dall'elenco dopo che la spesa è stata segnata (v. Pages/SpesaEdit.razor:140):
        // riconfermarla non è un errore.
        Assert.Null(TabellaSpese.Applica(Spesa(categoria: "Vecchia"), TabellaSpese.Colonne.Categoria, "Vecchia", Oggi).Errore);
    }

    [Fact]
    public void La_modifica_non_tocca_la_riga_originale_e_rifiuta_le_colonne_di_sola_lettura()
    {
        var spesa = Spesa(importo: 10m);

        TabellaSpese.Applica(spesa, TabellaSpese.Colonne.Importo, "20", Oggi);

        Assert.Equal(10m, spesa.Amount);
        Assert.Equal("Questa colonna non si modifica.",
            TabellaSpese.Applica(spesa, TabellaSpese.Colonne.Pagante, "x", Oggi).Errore);
    }

    // --- le azioni di massa ---

    private static readonly Guid[] Cinque = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

    [Fact]
    public void Eliminazione_si_conta_su_presenti_prima_meno_presenti_dopo()
    {
        // la quinta non c'era già più; la quarta è sopravvissuta (RLS: non era tua)
        var prima = Cinque.Take(4).ToHashSet();
        var dopo = new HashSet<Guid> { Cinque[3] };

        Assert.Equal(new EsitoMassa(5, 3, 1, 1), EsitoMassa.DaEliminazione(Cinque, prima, dopo));
    }

    [Fact]
    public void Se_la_RLS_filtra_la_lettura_non_si_dice_tutte_eliminate()
    {
        // Prima e dopo vuote: con "richieste meno presenti dopo" sarebbero 5 eliminate, su righe ancora lì.
        var nessuna = new HashSet<Guid>();

        Assert.Equal(new EsitoMassa(5, 0, 0, 5), EsitoMassa.DaEliminazione(Cinque, nessuna, nessuna));
    }

    [Fact]
    public void Cambia_categoria_distingue_non_tue_da_sparite()
    {
        var toccate = Cinque.Take(3).ToHashSet();
        var ancoraPresenti = new HashSet<Guid> { Cinque[3] };

        Assert.Equal(new EsitoMassa(5, 3, 1, 1), EsitoMassa.DaModifica(Cinque, toccate, ancoraPresenti));
    }

    [Fact]
    public void Il_testo_dell_esito_quando_sono_passate_tutte()
    {
        Assert.Equal("15 spese modificate.", TabellaSpese.TestoEsitoMassa(AzioneMassa.CambiaCategoria, new EsitoMassa(15, 15, 0, 0)));
        Assert.Equal("1 spesa eliminata.", TabellaSpese.TestoEsitoMassa(AzioneMassa.Elimina, new EsitoMassa(1, 1, 0, 0)));
    }

    [Fact]
    public void Il_testo_dell_esito_quando_ne_passano_solo_alcune()
    {
        Assert.Equal("12 su 15 modificate: 3 non erano tue.",
            TabellaSpese.TestoEsitoMassa(AzioneMassa.CambiaCategoria, new EsitoMassa(15, 12, 3, 0)));
        Assert.Equal("3 su 5 eliminate: 1 non era tua, 1 non c'era più.",
            TabellaSpese.TestoEsitoMassa(AzioneMassa.Elimina, new EsitoMassa(5, 3, 1, 1)));
    }

    [Fact]
    public void Il_testo_dell_esito_quando_non_ne_passa_nessuna()
    {
        Assert.Equal("Nessuna eliminata: 2 non erano tue.",
            TabellaSpese.TestoEsitoMassa(AzioneMassa.Elimina, new EsitoMassa(2, 0, 2, 0)));
        Assert.Equal("Nessuna modificata: 5 non c'erano più.",
            TabellaSpese.TestoEsitoMassa(AzioneMassa.CambiaCategoria, new EsitoMassa(5, 0, 0, 5)));
    }

    // --- la vista nell'URL ---

    [Fact]
    public void La_vista_predefinita_non_scrive_niente_nell_url()
    {
        Assert.Equal("", VistaTabella.Predefinita.InQuery());

        var letta = VistaTabella.DaQuery("");
        Assert.Equal(TipoPeriodo.QuestoMese, letta.Periodo);
        Assert.Equal(TabellaSpese.Colonne.Data, letta.OrdinaPer);
        Assert.True(letta.Decrescente);
        Assert.Null(letta.RaggruppaPer);
    }

    [Fact]
    public void Una_vista_scritta_nell_url_si_rilegge_identica()
    {
        var vista = VistaTabella.Predefinita with
        {
            Periodo = TipoPeriodo.Libero, Da = new DateTime(2026, 3, 2), A = new DateTime(2026, 4, 9),
            Filtri = new FiltriSpese(new HashSet<string> { "Casa", "Svago" }, Altro, "bolletta",
                new HashSet<StatoSpesa> { StatoSpesa.Prevista, StatoSpesa.InArrivo }),
            OrdinaPer = TabellaSpese.Colonne.Importo, Decrescente = false, RaggruppaPer = TabellaSpese.Gruppi.Mese
        };

        var riletta = VistaTabella.DaQuery(vista.InQuery());

        Assert.Equal(vista.Periodo, riletta.Periodo);
        Assert.Equal(vista.Da, riletta.Da);
        Assert.Equal(vista.A, riletta.A);
        Assert.True(riletta.Filtri.Categorie.SetEquals(["Casa", "Svago"]));
        Assert.Equal(Altro, riletta.Filtri.Pagante);
        Assert.Equal("bolletta", riletta.Filtri.Testo);
        Assert.True(riletta.Filtri.Stati.SetEquals([StatoSpesa.Prevista, StatoSpesa.InArrivo]));
        Assert.Equal(vista.OrdinaPer, riletta.OrdinaPer);
        Assert.False(riletta.Decrescente);
        Assert.Equal(TabellaSpese.Gruppi.Mese, riletta.RaggruppaPer);
    }

    [Fact]
    public void Un_parametro_illeggibile_torna_al_default_senza_rompere_gli_altri()
    {
        var letta = VistaTabella.DaQuery("?periodo=boh&ord=password&dir=x&cat=Inesistente,Casa&pagante=xyz&grp=colore&stato=forse&testo=pane");

        Assert.Equal(TipoPeriodo.QuestoMese, letta.Periodo);
        Assert.Equal(TabellaSpese.Colonne.Data, letta.OrdinaPer);
        Assert.True(letta.Decrescente);
        Assert.True(letta.Filtri.Categorie.SetEquals(["Casa"]));
        Assert.Null(letta.Filtri.Pagante);
        Assert.Null(letta.RaggruppaPer);
        Assert.Empty(letta.Filtri.Stati);
        Assert.Equal("pane", letta.Filtri.Testo);

        // Un periodo libero con una data impossibile torna a questo mese, e le date si perdono.
        var libera = VistaTabella.DaQuery("?periodo=libero&da=2026-13-45&a=2026-09-30");
        Assert.Equal(TipoPeriodo.QuestoMese, libera.Periodo);
        Assert.Null(libera.Da);
        Assert.Null(libera.A);
    }

    [Fact]
    public void Il_testo_del_filtro_sopravvive_ai_caratteri_dell_url()
    {
        var vista = VistaTabella.Predefinita with { Filtri = FiltriSpese.Nessuno with { Testo = "caffè & co=1?" } };

        Assert.Equal("caffè & co=1?", VistaTabella.DaQuery(vista.InQuery()).Filtri.Testo);
    }
}
```

- [ ] **Step 2: verificare che falliscano**

`mcp__synapse__test` con filtro `TabellaSpeseTests`. Atteso: errore di compilazione.

- [ ] **Step 3: scrivere l'implementazione minima**

Un file solo, `Services/TabellaSpese.cs`, con i tipi dichiarati sopra. Regole che i test non dicono da soli:

- **`Righe`**: prima `periodo.Righe` (stato `Prevista` se l'`Id` è in `periodo.Previste`, altrimenti
  `Registrata`), poi `periodo.InArrivo` con stato `InArrivo`. L'ordinamento lo fa la pagina con
  `CalcoliGriglia.Ordina`, non questo metodo.
- **`RigaSpesa.Chiave`**: per `Registrata`, `Spesa.Id.ToString()`; altrimenti
  `$"{Spesa.RecurringId}:{CalcoliRicorrenti.Periodo(Spesa.RecurringPeriod!.Value)}"`. Calcolata nel
  costruttore o come proprietà con corpo: il record non deve diventare una classe.
- **`Intervallo`**: `UltimiTreMesi` = dal primo del mese di due mesi fa alla fine di questo, cioè il mese
  corrente **e** i due precedenti — **da ratificare** (v. DOMANDE nel resoconto dell'unità P). `QuestAnno`
  arriva al 31 dicembre: le ricorrenti future compaiono come «in arrivo», ed è ciò che la spec §4.2 vuole.
  `Libero` con `da` o `a` nulli, o `da > a`, torna a `QuestoMese`.
- **`Filtra`**: un insieme vuoto (categorie, stati) e un `Pagante` nullo non filtrano. Il testo si confronta
  ridotto con `SchemaCampi.RiduciAccenti(...).ToUpperInvariant()` da entrambe le parti, dopo `Trim()`; un
  testo vuoto non filtra.
- **`Modificabile`**: `Stato == Registrata && Permessi.PuoIntervenire(mioId, Spesa.PaidBy, Spesa.SpaceId, spazi)`.
  Nient'altro: la regola sta in `Permessi` e non si riscrive.
- **`DataRagionevole`**: la forma di `Pages/SpesaEdit.razor:216-218`: `data >= 2000-01-01 && data <= oggi.AddYears(1)`.
- **`Applica`** restituisce una **copia** di `spesa` (tutte le proprietà, `Version` compresa) con il solo
  campo cambiato, oppure `Nuova = null` e un `Errore`. Importo: `Denaro.Verifica`, poi
  `Testi.MessaggioImporto(esito) ?? "Scrivi un importo."`. Descrizione: `Trim()`, vuota → errore. Data:
  `DateTime.TryParseExact(testo, "yyyy-MM-dd", CultureInfo.InvariantCulture, …)`, fallito → «Scrivi una
  data valida.», fuori guardia → la frase di `Pages/Spese.razor:92`. Categoria: `CategorieSpesa.Conosciuta(testo)`
  oppure uguale alla categoria attuale. Qualunque altra colonna → «Questa colonna non si modifica.»: fallisce
  chiuso.
- **`EsitoMassa.DaEliminazione`**: `Toccate = |prima ∖ dopo|`, `Rifiutate = |prima ∩ dopo|`,
  `Sparite = richieste − |prima|`. **`DaModifica`**: `Toccate = |toccate|`, `Rifiutate = |ancoraPresenti|`,
  `Sparite = richieste − Toccate − Rifiutate`.
- **`TestoEsitoMassa`**: tutte → `Testi.Conteggio(t, "spesa modificata", "spese modificate") + "."` (o
  `eliminata`/`eliminate`); alcune → `"{t} su {n} modificate: "` + i motivi; nessuna → `"Nessuna modificata: "`
  + i motivi. I motivi, separati da `", "`, nell'ordine rifiutate poi sparite, ciascuno solo se > 0, con
  `Testi.Conteggio(r, "non era tua", "non erano tue")` e `Testi.Conteggio(s, "non c'era più", "non c'erano più")`.
- **`TestoStato`**: «registrata», «prevista», «in arrivo»; una registrata con `RecurringId` diventa
  «registrata · ricorrente» — è il «segno» della spec §4.1.
- **`VistaTabella.InQuery`** scrive solo i parametri diversi dal default, in quest'ordine: `periodo`
  (`mese`, `scorso`, `3mesi`, `anno`, `libero`), `da`, `a` (`yyyy-MM-dd`, solo se `libero`), `cat`
  (separate da virgola, nell'ordine di `CategorieSpesa.Elenco`), `pagante`, `testo`, `stato`
  (`registrata`, `prevista`, `in-arrivo`), `ord`, `dir` (`asc`), `grp`. Ogni valore passa per
  `Uri.EscapeDataString`. **`DaQuery`** accetta la stringa con o senza `?`, divide su `&` e sul primo `=`,
  usa `Uri.UnescapeDataString`, ignora le chiavi sconosciute, e per ogni parametro illeggibile tiene il
  default **di quel parametro** senza toccare gli altri. Una categoria fuori elenco si scarta; `ord` deve
  essere una delle sei chiavi di `Colonne`; `grp` una delle tre di `Gruppi`. Non lancia mai.

Budget: un file nuovo; `RigaSpesa`, `FiltriSpese`, `VistaTabella`, `EsitoModifica`, `EsitoMassa` e i tre
enum sono i soli tipi nuovi. Nessun'interfaccia, nessun servizio registrato.

- [ ] **Step 4: verificare che passino** — `mcp__synapse__test` con filtro `TabellaSpeseTests`: 28 superati.

- [ ] **Step 5: il gate completo** — 0 avvisi; **377** superati (349 + 28).

- [ ] **Step 6: commit**

---

## Task 3 — Le due scritture di massa

**File:**
- Modificare: `Services/ExpenseRepository.cs` (due metodi pubblici nuovi e un helper privato, dopo
  `EliminaAsync`, `:289-300`)

**Interfacce:**
- *Consuma*: `EsitoMassa` (task 2).
- *Produce*, e il task 5 vi si appoggia:
  ```csharp
  public async Task<EsitoMassa> CambiaCategoriaAsync(IReadOnlyCollection<Guid> ids, string categoria);
  public async Task<EsitoMassa> EliminaTutteAsync(IReadOnlyCollection<Guid> ids);
  ```

**Verificato da `doc-checker` il 22 settembre** sul sorgente di `Supabase.Postgrest` 4.4.0 (spec §5.4, e
`handoff/PIANO.md`, APERTO): il filtro su un elenco di id è `Filter("id", Constants.Operator.In, lista)`
con una `List<object>`, quindi i Guid si passano con `ids.Cast<object>().ToList()`; produce `id=in.(…)`.
`Set(...).Update()` restituisce `ModeledResponse<T>` con le righe toccate. **`Filter(...).Delete()`
restituisce un `Task` senza righe.** `Operator.In` non è mai stato usato in Eton: è il caso 2 di
`doc-checker` («un simbolo esterno che un `Grep` non trova mai usato») — la verifica del 22 settembre vale
per la versione installata oggi, che è la stessa (`Eton.csproj:39`); se la versione cambia, si rifà.

- [ ] **Step 1: `CambiaCategoriaAsync`**

Un'istruzione sola: `client.From<Expense>().Filter("id", Constants.Operator.In, ids.Cast<object>().ToList())
.Set(e => e.Category, categoria.Trim()).Update()`. Le toccate sono gli `Id` di `risposta.Models`. Se ne
mancano, **una** rilettura delle sole non toccate (stesso filtro `In`, `.Get()`) dà quelle ancora presenti:
`EsitoMassa.DaModifica(ids, toccate, ancoraPresenti)` `[corretto 7 ott]`. Nessun filtro su `version`, e il
commento lo dice: vince l'ultima scrittura, accettato perché la categoria sceglie da un elenco chiuso; **se
un giorno si vorranno cambiare gli importi in massa, questa scelta va rivista** (spec §5.4, ricopiato).

- [ ] **Step 2: `EliminaTutteAsync`**

La stessa forma di `EliminaAsync` (`:289-300`) su un elenco: lettura **prima** degli id con `In`, DELETE
filtrato con `In` sui soli presenti prima, rilettura **dopo** degli stessi id,
`EsitoMassa.DaEliminazione(ids, prima, dopo)`. Con zero presenti prima non si manda il DELETE. Il commento
del metodo cita la decisione del 7 ottobre: **mai** contare dalla risposta del DELETE (è sempre vuota,
spec §8) e **mai** «richiesti meno presenti dopo» (direbbe «tutte eliminate» su righe che la RLS ha solo
reso invisibili).

L'helper privato `LeggiIdAsync(client, ids) → IReadOnlySet<Guid>` serve a entrambi i metodi: tre
call-site, quindi è un helper e non va inline.

- [ ] **Step 3: gate e commit** — nessun test nuovo (repository: tradizione del progetto, il calcolo
  dell'esito è già testato al task 2). **377** superati, 0 avvisi.

---

## Task 4 — La griglia generica e la tastiera come in Excel

È il task con il rischio dichiarato più alto della 2.2 (spec §8): **se il fuoco non sopravvive ai
ridisegni, la funzione che l'utente ha voluto contro il parere tecnico diventa inservibile al primo
salvataggio.** Il costo delle frecce è stato accettato il 22 settembre e va pagato qui per intero.

**File:**
- Creare: `Services/TastieraGriglia.cs`, `Shared/Griglia.razor`, `Shared/Griglia.razor.css`,
  `wwwroot/js/griglia.js`
- Test: `Eton.Tests/TastieraGrigliaTests.cs`

**Interfacce:**
- *Consuma*: tutto il task 1.
- *Produce*, e il task 5 vi si appoggia:
  ```csharp
  // Services/TastieraGriglia.cs
  public enum AzioneTasto { Nessuna, Su, Giu, Sinistra, Destra, Modifica, ModificaSostituendo,
      SelezionaRiga, SalvaEGiu, SalvaEDestra, SalvaESinistra, Annulla }

  public static class TastieraGriglia
  {
      public static AzioneTasto Interpreta(string tasto, bool shift, bool ctrlAltMeta, bool inModifica, TipoColonna tipo);
      public static (int Riga, int Colonna) Sposta((int Riga, int Colonna) da, AzioneTasto azione, int righe, int colonne);
      public static string? Riallinea(IReadOnlyList<string> chiavi, string? attiva, int indicePrecedente);
  }

  // Shared/Griglia.razor — @typeparam T
  public sealed record ModificaCella<T>(T Riga, string Colonna, string Testo);
  public enum EsitoCella { Salvata, NonValida, Chiusa }
  public sealed record RispostaCella(EsitoCella Esito, string? Messaggio);

  [Parameter, EditorRequired] IReadOnlyList<ColonnaGriglia<T>> Colonne
  [Parameter, EditorRequired] IReadOnlyList<Gruppo<T>> Gruppi
  [Parameter, EditorRequired] Func<T, string> ChiaveRiga
  [Parameter, EditorRequired] string Didascalia                       // <caption>, per il lettore di schermo
  [Parameter] Func<T, bool> ContaNeiTotali                            // default: tutte
  [Parameter] Func<T, bool> Selezionabile                             // default: nessuna
  [Parameter] IReadOnlySet<string> Selezione / EventCallback<IReadOnlySet<string>> SelezioneChanged   // @bind-Selezione
  [Parameter] string? OrdinataPer; [Parameter] bool Decrescente; [Parameter] EventCallback<string> OnOrdina
  [Parameter] Func<ModificaCella<T>, Task<RispostaCella>>? SalvaCella
  [Parameter] Func<T, string?>? MotivoSolaLettura                     // per le righe non modificabili
  [Parameter] Func<T, RenderFragment?>? SottoRiga                      // la scheda di conflitto, in una riga colspan
  [Parameter] string? NotaRiepilogo                                    // "di cui 3 previste"
  [Parameter] Func<T, string, string?>? AncoraCella                    // (riga, colonna) → valore di data-tutorial, o null
  public Task EsportaAsync(string nomeFile)                            // CSV di selezione o righe mostrate
  ```
  `EsitoCella.Chiusa` vuol dire «la modifica è finita, l'esito lo gestisce la pagina» (rifiutata, conflitto,
  sparita): la griglia esce dalla modifica e mostra `Messaggio` accanto alla riga. `NonValida` la tiene in
  modifica con il messaggio. `Salvata` esce dalla modifica: la riga riallineata arriva dalla pagina con il
  prossimo `Gruppi`.

- [ ] **Step 1: i test della tastiera, che falliscono**

```csharp
using Eton.Services;

namespace Eton.Tests;

public class TastieraGrigliaTests
{
    private static AzioneTasto Fuori(string tasto, bool shift = false, bool ctrl = false, TipoColonna tipo = TipoColonna.Testo)
        => TastieraGriglia.Interpreta(tasto, shift, ctrl, inModifica: false, tipo);

    private static AzioneTasto Dentro(string tasto, bool shift = false)
        => TastieraGriglia.Interpreta(tasto, shift, false, inModifica: true, TipoColonna.Testo);

    [Fact]
    public void Fuori_modifica_le_frecce_spostano_la_cella()
    {
        Assert.Equal(AzioneTasto.Su, Fuori("ArrowUp"));
        Assert.Equal(AzioneTasto.Giu, Fuori("ArrowDown"));
        Assert.Equal(AzioneTasto.Sinistra, Fuori("ArrowLeft"));
        Assert.Equal(AzioneTasto.Destra, Fuori("ArrowRight"));
    }

    [Fact]
    public void Fuori_modifica_Invio_e_F2_entrano_in_modifica()
    {
        Assert.Equal(AzioneTasto.Modifica, Fuori("Enter"));
        Assert.Equal(AzioneTasto.Modifica, Fuori("F2"));
    }

    [Fact]
    public void Un_carattere_entra_sostituendo_ma_non_con_ctrl_e_non_su_data_o_menu()
    {
        Assert.Equal(AzioneTasto.ModificaSostituendo, Fuori("7", tipo: TipoColonna.Denaro));
        Assert.Equal(AzioneTasto.ModificaSostituendo, Fuori("è"));
        Assert.Equal(AzioneTasto.Nessuna, Fuori("c", ctrl: true));        // Ctrl+C resta del browser
        // Su una data o un menù "sostituire con un carattere" non ha senso: si entra e basta.
        Assert.Equal(AzioneTasto.Modifica, Fuori("2", tipo: TipoColonna.Data));
        Assert.Equal(AzioneTasto.Modifica, Fuori("C", tipo: TipoColonna.Selezione));
    }

    [Fact]
    public void Fuori_modifica_Spazio_seleziona_la_riga()
    {
        Assert.Equal(AzioneTasto.SelezionaRiga, Fuori(" "));
        Assert.Equal(AzioneTasto.Nessuna, Fuori("Escape"));
    }

    [Fact]
    public void In_modifica_Invio_Tab_ShiftTab_ed_Esc()
    {
        Assert.Equal(AzioneTasto.SalvaEGiu, Dentro("Enter"));
        Assert.Equal(AzioneTasto.SalvaEDestra, Dentro("Tab"));
        Assert.Equal(AzioneTasto.SalvaESinistra, Dentro("Tab", shift: true));
        Assert.Equal(AzioneTasto.Annulla, Dentro("Escape"));
    }

    [Fact]
    public void In_modifica_frecce_e_caratteri_restano_al_campo()
    {
        Assert.Equal(AzioneTasto.Nessuna, Dentro("ArrowLeft"));
        Assert.Equal(AzioneTasto.Nessuna, Dentro("a"));
        Assert.Equal(AzioneTasto.Nessuna, Dentro(" "));
    }

    [Fact]
    public void Lo_spostamento_si_ferma_ai_bordi()
    {
        Assert.Equal((0, 0), TastieraGriglia.Sposta((0, 0), AzioneTasto.Su, righe: 3, colonne: 4));
        Assert.Equal((0, 0), TastieraGriglia.Sposta((0, 0), AzioneTasto.Sinistra, 3, 4));
        Assert.Equal((2, 3), TastieraGriglia.Sposta((2, 3), AzioneTasto.Giu, 3, 4));
        Assert.Equal((2, 3), TastieraGriglia.Sposta((2, 3), AzioneTasto.SalvaEDestra, 3, 4));
        Assert.Equal((2, 1), TastieraGriglia.Sposta((1, 1), AzioneTasto.SalvaEGiu, 3, 4));
        Assert.Equal((1, 0), TastieraGriglia.Sposta((1, 1), AzioneTasto.SalvaESinistra, 3, 4));
    }

    [Fact]
    public void Dopo_un_ridisegno_la_cella_attiva_si_ritrova_per_chiave()
    {
        // ancora presente, anche se si è spostata: resta lei
        Assert.Equal("b", TastieraGriglia.Riallinea(["c", "a", "b"], "b", indicePrecedente: 1));
        // sparita (filtrata o eliminata): la riga che ora occupa lo stesso posto
        Assert.Equal("c", TastieraGriglia.Riallinea(["a", "c"], "b", indicePrecedente: 1));
        // sparita ed era l'ultima: la nuova ultima
        Assert.Equal("c", TastieraGriglia.Riallinea(["a", "c"], "z", indicePrecedente: 5));
        // tabella vuota: nessuna cella attiva
        Assert.Null(TastieraGriglia.Riallinea([], "b", indicePrecedente: 0));
    }
}
```

Atteso allo step 2: errore di compilazione. Allo step 4: 8 superati.

- [ ] **Step 2: verificare che falliscano** — `mcp__synapse__test` con filtro `TastieraGrigliaTests`.

- [ ] **Step 3: `TastieraGriglia`, puro**

`Interpreta`: in modifica → `Enter` `SalvaEGiu`, `Tab` `SalvaEDestra` (con Shift `SalvaESinistra`),
`Escape` `Annulla`, tutto il resto `Nessuna` (lo gestisce il campo). Fuori modifica → frecce, `Enter` e `F2`
`Modifica`, `" "` `SelezionaRiga`; un `tasto` di lunghezza 1 senza `ctrlAltMeta` è `ModificaSostituendo` su
`Testo`, `Numero`, `Denaro`, e `Modifica` su `Data` e `Selezione` `[corretto 7 ott]` — la spec §5.2 non
distingue, ma «sostituire con un carattere» un menù o un campo data non ha un significato; **da ratificare**.
`Sposta` limita a `[0, righe-1] × [0, colonne-1]`, senza andare a capo. `Riallinea` come nei test.

- [ ] **Step 4: verificare che passino** — 8 superati.

- [ ] **Step 5: `wwwroot/js/griglia.js`**

Modulo ES, caricato con `JS.InvokeAsync<IJSObjectReference>("import", "./js/griglia.js")` in
`OnAfterRenderAsync(firstRender)`, come `Shared/GrafoSpazio.razor:31-52`, con lo stesso `try/catch` che
contiene il guasto, e smontato in `DisposeAsync` come `:54-75`. Tre esportazioni, nient'altro:

```js
// Una griglia senza questo modulo funziona ancora col mouse: il guasto si contiene qui.
export function avvia(tabella) {
  const suTasto = (e) => {
    const cella = e.target.closest('td[data-colonna]');
    if (!cella || !tabella.contains(cella)) return;
    const nelCampo = e.target.matches('input, select');
    const carattere = e.key.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey;
    if (!nelCampo && (carattere || ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Enter', 'F2'].includes(e.key)))
      e.preventDefault();          // niente scorrimento della pagina, niente carattere perso nel vuoto
    if (nelCampo && (e.key === 'Enter' || e.key === 'Tab'))
      e.preventDefault();          // Tab non deve far uscire il fuoco dalla tabella: lo sposta Blazor
  };
  tabella.addEventListener('keydown', suTasto);
  return { ferma: () => tabella.removeEventListener('keydown', suTasto) };
}

export function focalizza(tabella, riga, colonna) {
  const sel = `td[data-riga="${CSS.escape(riga)}"][data-colonna="${CSS.escape(colonna)}"]`;
  const cella = tabella.querySelector(sel);
  if (!cella) return;
  const campo = cella.querySelector('input, select');
  (campo ?? cella).focus();
  if (campo && campo.type === 'text') campo.setSelectionRange(campo.value.length, campo.value.length);
}

export function scarica(nomeFile, testo) {
  const url = URL.createObjectURL(new Blob([testo], { type: 'text/csv;charset=utf-8' }));
  const a = Object.assign(document.createElement('a'), { href: url, download: nomeFile });
  a.click();
  URL.revokeObjectURL(url);
}
```

Il `preventDefault` sta in JavaScript perché `@onkeydown:preventDefault` di Blazor è un booleano statico
per elemento, deciso **prima** che il gestore sappia quale tasto è: non può lasciar passare Ctrl+C e fermare
le frecce. La logica dei tasti resta tutta in C# (`TastieraGriglia`), e il JavaScript non decide niente.

- [ ] **Step 6: `Shared/Griglia.razor`**

Markup, nell'ordine (spec §3, `<table>` semantica):

- `<table @ref="tabella">` con `<caption>@Didascalia</caption>` (visibile solo al lettore di schermo con la
  classe che il progetto usa già per questo, se esiste; altrimenti visibile, resa provvisoria).
- `<thead>`: una prima colonna con la casella «tutte le righe» (`aria-label="Seleziona tutte le righe
  mostrate"`), presente solo se almeno una riga è `Selezionabile`; poi un `<th scope="col">` per colonna,
  con `aria-sort` (`ascending` / `descending` / assente) e, se `Ordinabile`, un `<button>` che invoca
  `OnOrdina(colonna.Chiave)`. La pagina decide il verso.
- Un `<tbody>` **per gruppo**. Se `Etichetta` non è nulla, la prima riga del `tbody` è
  `<tr><th scope="rowgroup" colspan="…">` con un pulsante `aria-expanded` che chiude e apre il gruppo,
  l'etichetta, il conteggio e il subtotale (somma delle sole righe `ContaNeiTotali`, per le colonne con
  `Aggregazione.Somma`). I gruppi chiusi stanno in un `HashSet<string>` del componente e **non** vanno
  nell'URL: sono un gesto, non una vista (spec §4.2).
- Ogni riga `<tr @key="ChiaveRiga(riga)">` — **il `@key` è ciò che tiene il nodo DOM della cella attiva
  attraverso un riordinamento**, e senza di lui il fuoco si perde a ogni filtro. Ogni cella dati è
  `<td data-riga="…" data-colonna="…" tabindex="@(attiva ? 0 : -1)" @onkeydown="…" @onclick="…">`: **roving
  tabindex**, una sola cella nel giro del Tab. Una riga non modificabile porta `MotivoSolaLettura(riga)` in
  un `title` e in un `aria-description`, e una classe che la rende visibilmente di sola lettura. La riga
  mostra «salvo…» (`role="status"`) mentre una sua cella è in volo.
- In modifica, al posto del testo: `Testo` → `<input type="text">`; `Denaro` → `<input type="text"
  inputmode="decimal">`; `Data` → `<input type="date" min="2000-01-01" max="…">` come
  `Pages/SpesaEdit.razor:119-121`; `Selezione` → `<select>` con `Opzioni` **più il valore attuale se non vi
  compare** (il caso di `Pages/SpesaEdit.razor:140-147`). Il valore iniziale è
  `CalcoliGriglia.TestoModificabile`, oppure il carattere premuto per `ModificaSostituendo`. Il messaggio di
  `NonValida` sta sotto il campo, `role="alert"`, legato con `aria-describedby`.
- Se `SottoRiga(riga)` non è nullo, subito sotto: `<tr><td colspan="…">@frammento</td></tr>`.
- `<tfoot>`: per ogni aggregazione presente in almeno una colonna, una riga con l'etichetta («Somma»,
  «Media», «Minimo», «Massimo», «Conteggio») e il valore sotto la colonna giusta, da
  `CalcoliGriglia.Aggrega` sulle righe che contano: **la selezione se non è vuota, altrimenti tutte le righe
  dei gruppi**, filtrate con `ContaNeiTotali`. Una didascalia dice quale dei due: «Sulle 42 righe mostrate» /
  «Sulla selezione: 3 righe». `NotaRiepilogo` accanto alla somma. Un valore nullo (media di zero righe) si
  scrive «—».

Stato interno: cella attiva come `(string Riga, string Colonna)` **per chiave, mai per indice** (spec §5.2);
`inModifica`, `testoInModifica`, `messaggio`; un contatore `modificaCorrente` che rende idempotente la
conferma — **Invio salva, poi il campo sparisce e il browser può emettere anche un `focusout`**: il secondo
arrivo trova il contatore già avanzato e non salva una seconda volta. In `OnParametersSet` la cella attiva
si riallinea con `TastieraGriglia.Riallinea` sulle chiavi in ordine di resa. In `OnAfterRenderAsync`, se un
gesto da tastiera o una modifica l'ha chiesto (un `bool fuocoDaRipristinare`, **non** a ogni render, per non
rubare il fuoco al campo di filtro che l'utente sta scrivendo), `focalizza` sulla cella attiva.

Gestore dei tasti: `TastieraGriglia.Interpreta`, poi `Sposta` per le frecce; `Modifica` /
`ModificaSostituendo` solo se `colonna.Modificabile?.Invoke(riga) == true`, altrimenti niente; `SelezionaRiga`
solo se `Selezionabile(riga)`; `Salva*` chiama `SalvaCella` e, a `Salvata` o `Chiusa`, sposta; `Annulla`
ripristina e esce. «Clic fuori: salva» è `@onfocusout` sul campo.

Selezione: casella per riga (`tabindex="-1"`: da tastiera la riga si seleziona con Spazio), Shift+clic per
l'intervallo con `CalcoliGriglia.Intervallo` sulle chiavi delle righe selezionabili in ordine di resa; la
casella in testa seleziona o deseleziona tutte le righe mostrate e selezionabili. Ogni cambio emette
`SelezioneChanged`.

`EsportaAsync(nomeFile)`: le righe della selezione, o tutte quelle dei gruppi, nell'ordine di resa,
`CalcoliGriglia.Csv(Colonne, righe)`, poi `scarica`.

Budget: un componente, un modulo JS, nessun sotto-componente per la cella (un `RenderFragment` locale se il
markup della cella si ripete). Nessun servizio registrato.

- [ ] **Step 7: `Shared/Griglia.razor.css`** — resa minima sui token esistenti: bordi `var(--bordo)`,
  testata appiccicata in alto (`position: sticky`), cella attiva con `outline` in `var(--accento)`, righe di
  sola lettura con il testo tenue che il progetto già usa, riga in salvataggio con un'opacità ridotta.
  Densità della spec §3: su `@media (pointer: fine)` riga a **38px**, su `pointer: coarse` resta il
  pavimento `var(--tocco)`; il valore esatto è della 2.1-bis e il commento lo dice.

- [ ] **Step 8: gate e commit** — 0 avvisi; **385** superati (377 + 8). La griglia non si prova nel browser
  in questo task: non ha ancora una pagina. Lo fa il task 5, ed è il motivo per cui il task 5 non si salta.

---

## Task 5 — La pagina `/expenses/table`

**File:**
- Creare: `Pages/SpeseTabella.razor`, `Pages/SpeseTabella.razor.css`
- Modificare: `Shared/NavigazioneSpese.razor:24` (ultimo step)

**Interfacce:**
- *Consuma*: `ExpenseRepository.ElencaConPrevisteAsync` (`Services/ExpenseRepository.cs:82`), `SalvaAsync`
  (`:231`), `CambiaCategoriaAsync` ed `EliminaTutteAsync` (task 3); `SpaceRepository.MembriAsync`
  (`Services/SpaceRepository.cs:103`); `PaginaRegistro` (`Shared/PaginaRegistro.cs:48,60,66,113`);
  `TestataPagina` (`Shared/TestataPagina.razor:50-59`); `NavigazioneSpese`; `SchedaConflitto`
  (`Shared/SchedaConflitto.razor:31-35`); `ConfermaAzione` (`Shared/ConfermaAzione.razor:43-48`);
  `Permessi.Spiegazione` (`Services/Permessi.cs:85`); tutto il task 2; `Griglia<T>` (task 4).
- *Produce*, e il task 6 vi si appoggia: le **ancore del tutorial**, come attributi `data-tutorial` nel
  markup di questa pagina: `periodo`, `filtri`, `intestazioni`, `cella-modificabile`, `riga-sola-lettura`,
  `selezione`, `azioni`, `riepilogo`, `esporta`. `cella-modificabile` e `riga-sola-lettura` li porta la
  griglia sulla **prima** cella modificabile e sulla **prima** cella di una riga di sola lettura, tramite il
  parametro `AncoraCella` del task 4: la pagina lo implementa ricordando, a ogni `Ricalcola()`, le chiavi
  della prima riga modificabile e della prima di sola lettura nell'ordine di resa.

- [ ] **Step 1: lo scheletro e la lettura**

`@page "/expenses/table"`, `@inherits PaginaRegistro`, `@inject ExpenseRepository Repository`,
`@inject SpaceRepository SpaziRepository`, `@inject AuthStateService AuthState`,
`@inject NavigationManager Navigation`. `<PageTitle>Spese in tabella — Eton</PageTitle>`,
`<TestataPagina Titolo="Spese in tabella">`, `<NavigazioneSpese Attiva="expenses/table" />`, e i rami di
caricamento, errore e spazio assente **come `Pages/Ricorrenti.razor:23-62`**.

`OnInitializedAsync`: `mioId` come `Pages/Spese.razor:283-287`, poi
`vista = VistaTabella.DaQuery(new Uri(Navigation.Uri).Query)`, poi `base.OnInitializedAsync()`.

`Leggi(attivo)`: azzera i campi; `(da, a) = TabellaSpese.Intervallo(vista.Periodo, DateTime.Today, vista.Da, vista.A)`;
`await Repository.ElencaConPrevisteAsync(attivo.Id, da, a)`; `righe = TabellaSpese.Righe(periodo)`; in caso di
eccezione `SegnalaNonLetti(ex)`. I membri in un `try` **separato**, come `Pages/Spese.razor:430-443` e per lo
stesso motivo. Poi `Ricalcola()`.

> **Divieto, dal §5 del design delle ricorrenti e dalla spec §2.3, da ricopiare nel brief.** La tabella è
> il terzo chiamante della lettura delle spese e legge **solo** da `ElencaConPrevisteAsync`. Una lettura
> diretta fa divergere i totali fra registro, Home e tabella, e nessun test lo intercetta.

- [ ] **Step 2: la vista — periodo, filtri, ordinamento, raggruppamento**

Una barra di strumenti sopra la griglia:
- **Periodo** (`data-tutorial="periodo"`): un `<select>` con le cinque voci, e due `<input type="date">`
  che compaiono solo per «dal … al …». Cambiare periodo **rilegge** (è l'unico filtro che va in rete).
- **Filtri** (`data-tutorial="filtri"`): categorie come pastiglie a scelta multipla (`aria-pressed`, come
  `Pages/Spese.razor:100-101`), pagante (solo negli spazi condivisi), testo, stato. **Non rileggono**: si
  ricalcola in memoria.
- **Raggruppa per**: nessuno · categoria · pagante · mese.
- L'ordinamento si cambia dalle intestazioni (`OnOrdina`): stessa colonna → inverte il verso; colonna nuova
  → crescente, tranne la data che parte decrescente come il default.

`Ricalcola()`: `TabellaSpese.Filtra(righe, vista.Filtri)` → `CalcoliGriglia.Ordina(…, colonna di
vista.OrdinaPer, vista.Decrescente)` → `CalcoliGriglia.Raggruppa(…, chiave del gruppo)`. Le chiavi di gruppo:
categoria → `Spesa.Category`; pagante → nome del membro; mese → `$"{CalcoliSpese.NomeMese(m)} {anno}"`. La
selezione si restringe alle chiavi ancora mostrate.

**Ogni cambio di vista si scrive nell'URL** con
`Navigation.NavigateTo("expenses/table" + vista.InQuery(), replace: true)`: ricaricare, salvare il link o
tornare indietro **da un'altra pagina** ritrova la stessa vista (spec §4.2). `replace: true` perché ogni
tasto nel filtro di testo non deve diventare un passo della cronologia — **da ratificare**. Il cambio di
query sulla stessa pagina **non** la ricrea: `Layout/MainLayout.razor:40,60-65` usa come `@key` il percorso
senza query, scritto apposta per questo. L'URL si legge **una volta**, all'apertura; dopo, lo stato vive
nella pagina e l'URL lo segue.

**La macchina a stati delle letture.** I chiamanti di `Carica()` sono il primo montaggio (ereditato), il
cambio di spazio (ereditato), «Riprova», il cambio di periodo e la fine di un'azione di massa. Valgono le
regole già scritte per `Pages/Spese.razor:359-388`, e il brief le ricopia: un flag `rileggo` alzato **come
ultima istruzione sincrona prima del primo `await`** di ogni percorso che porta a `Carica()`;
`ScartaCambioSpazio() => rileggo`; ogni controllo che avvia un percorso disabilitato mentre `rileggo` è
vero. Un `Riprova` proprio, come `Pages/Spese.razor:451`, perché qui rileggere non deve far sparire la barra
degli strumenti sotto uno scheletro. **Non** si tocca `Pages/Spese.razor`: la macchina si ricopia nella
pagina nuova, non si estrae.

- [ ] **Step 3: le colonne**

**Data · Descrizione · Categoria · Importo · Pagato da · Stato** (spec §4.1), con le chiavi di
`TabellaSpese.Colonne`. «Pagato da» solo se `!Spazi.Attivo.IsPersonal`. Importo con
`Aggregazioni = Somma | Media | Min | Max | Conteggio`. `Modificabile` per Data, Descrizione, Categoria e
Importo: `r => TabellaSpese.Modificabile(r, mioId, Spazi.Elenco)`. Categoria: `Opzioni =
CategorieSpesa.Elenco`. Stato: `Valore = TabellaSpese.TestoStato`, mai modificabile. `ChiaveRiga =
r => r.Chiave`; `ContaNeiTotali = r => r.ContaNeiTotali`; `Selezionabile = r => r.Stato ==
StatoSpesa.Registrata` (le previste e le in arrivo non si selezionano, spec §5.1); `MotivoSolaLettura`:
per una prevista «Prevista: diventa una spesa vera quando chi la paga apre Eton.», per una in arrivo
«In arrivo: non è ancora dovuta.», per una riga altrui `Permessi.Spiegazione(Permessi.Oggetto.Spesa)`.
`NotaRiepilogo`: «di cui N previste» con `TabellaSpese.QuantePreviste` sulle righe del riepilogo, solo se
N > 0.

- [ ] **Step 4: il salvataggio di una cella**

`SalvaCella(modifica)`: `TabellaSpese.Applica(riga.Spesa, modifica.Colonna, modifica.Testo, DateTime.Today)`;
errore → `RispostaCella(NonValida, errore)` e **non parte niente** (spec §5.3). Altrimenti
`Repository.SalvaAsync(nuova.Id, nuova.Version, nuova.Amount, nuova.Description, nuova.Category,
nuova.SpentOn)` — **la riga intera con il suo `version`**, così il filtro sulla versione protegge anche i tre
campi non toccati. Esiti, come `Pages/SpesaEdit.razor:369-396`:

| Esito | Cosa fa la pagina | Risposta alla griglia |
|---|---|---|
| `Salvata` | sostituisce la `RigaSpesa` con quella restituita, `Ricalcola()` | `Salvata` |
| `Conflitto` | ricorda `(chiave → versione del server)`; `SottoRiga` per quella chiave rende `<SchedaConflitto Premessa="Mentre modificavi, questa spesa è cambiata." …>` | `Chiusa` |
| `Rifiutata` | la riga resta com'era | `Chiusa` con la frase di `Pages/SpesaEdit.razor:390` |
| `Sparita` | toglie la riga, `Ricalcola()` | `Chiusa` con «Questa spesa non c'è più: l'ha eliminata qualcun altro.» |
| eccezione | la riga resta com'era | `Chiusa` con «Non è stato possibile salvare: riprova fra un momento.» |

La scheda di conflitto: «Ricarica la sua» sostituisce la riga con la versione del server e chiude la scheda;
«Sovrascrivi con la mia» risalva **la stessa modifica** con la versione del server, come
`Pages/SpesaEdit.razor:412-446`. `SovrascriviAbilitato` = la riga è ancora modificabile. Più conflitti
aperti insieme sono ammessi, uno per riga.

- [ ] **Step 5: le azioni su più righe**

Una barra (`data-tutorial="azioni"`) che compare solo con una selezione non vuota: «N selezionate», un
`<select>` di categoria con «Cambia categoria», e `<ConfermaAzione Chiave="@impronta" Etichetta="Elimina"
EtichettaConferma="Sì, elimina N spese" …>`, dove `impronta` è la stringa delle chiavi selezionate in
ordine (`[corretto 7 ott]`, correzione 11). Entrambe: `rileggo` alzato prima del primo `await`, la chiamata
al repository con gli `Id` delle righe selezionate, il testo `TabellaSpese.TestoEsitoMassa` in un `role="status"`,
la selezione svuotata, poi `Carica()` — le previste si ricalcolano e i totali tornano veri. Un'eccezione
dà una frase che **non** promette nulla sullo stato delle righe, come `Pages/SpesaEdit.razor:468-473`:
«Non è stato possibile completare l'operazione: ricarica per vedere com'è adesso.»

- [ ] **Step 6: esportazione, l'aiuto, e lo schermo stretto**

- **Esporta CSV** (`data-tutorial="esporta"`): `griglia.EsportaAsync(nome)`, con `nome =
  $"spese-{da:yyyy-MM-dd}-{a:yyyy-MM-dd}.csv"`. L'etichetta dice cosa esporta: «Esporta la selezione» o
  «Esporta le righe mostrate».
- **Il «?»** (`TestataPagina.Aiuto`, spec §6.1) dice almeno: i tre stati; perché le previste contano nel
  totale e le in arrivo no; perché alcune righe non si modificano; i tasti (la tabella della spec §5.2, in
  forma di elenco). Non ripete ciò che la pagina dice già da sé.
- **Sotto i `40rem`** (`Pages/SpeseTabella.razor.css`): la griglia e la barra degli strumenti non si
  mostrano, e al loro posto un paragrafo: «La tabella è pensata per uno schermo largo. Per segnare una spesa
  usa il registro.» con il link a `expenses`. Solo CSS, mobile-first come `wwwroot/css/app.css:2362`
  (`@media (min-width: 40rem)`): l'avviso visibile di base, la tabella visibile da `40rem` in su. Nessun
  `matchMedia`. La rotta esiste sempre.

- [ ] **Step 7: gate, commit, prova nel browser**

Gate: 0 avvisi; **385** superati (nessun test nuovo: la pagina non si testa, la sua logica sta nei task 1-2).

Poi il §7 del protocollo: l'applicazione la avvia l'esecutore (porta e PID su disco in
`handoff/<unità>/server.md`, riavviata prima della prova: il server di sviluppo si congela sui manifest),
e `live-testing` prova, **solo su righe `[collaudo]` create per la prova ed eliminate alla fine**:
1. i tasti della spec §5.2 uno per uno, e il fuoco che resta sulla cella dopo un salvataggio, dopo un
   cambio di filtro e dopo un riordinamento;
2. un conflitto provocato con due schede sulla stessa cella, con «Ricarica la sua» e con «Sovrascrivi»;
3. un'azione di massa su righe miste. Se nello spazio di prova non esistono righe di un altro membro, il
   caso «non erano tue» non si può provocare: resta coperto dal test puro del task 2, e lo si riporta come
   limite, non lo si aggira;
4. l'avviso sotto i `40rem`;
5. l'URL: ricaricare la pagina con filtri, ordinamento e raggruppamento attivi ritrova la stessa vista; un
   parametro scritto a mano illeggibile non rompe la pagina.

Poi `ui-critic`, perché il task crea un `.razor` con markup (§7, riga 3), con il `PIANO-DESIGN` del brief
come metro. I rilievi `TIPO: progetto` vanno alla 2.1-bis.

- [ ] **Step 8: la voce «Tabella» si accende**

Solo dopo `live-testing` verde: in `Shared/NavigazioneSpese.razor:24` `false` diventa `true`, e il commento
alle righe 22-23 si aggiorna. Il commento in testa al file (`:14`) che elenca le rotte accettate da
`Attiva` prende anche `"expenses/table"`. Commit.

**Aggiunta del 7 ottobre (decisione dell'utente, punto 6 del popup):** sotto i `40rem` la voce «Tabella»
**non si mostra**. La regola **non** va in `wwwroot/css/app.css` (riservato al task 6 dal vincolo globale):
va in un file nuovo `Shared/NavigazioneSpese.razor.css` (CSS isolato), con un marcatore sulla voce — un
flag «solo schermo largo» nel record della voce, che diventa una classe — nascosta di base e mostrata da
`40rem` in su, mobile-first. La rotta `/expenses/table` resta raggiungibile: un link salvato funziona e
mostra l'avviso con il link al registro.

---

## Task 6 — Il tutorial guidato, generico

**File:**
- Creare: `Shared/Tutorial.razor`, `wwwroot/js/tutorial.js`
- Modificare: `wwwroot/css/app.css` (una regola, in fondo al file), `Pages/SpeseTabella.razor` (il
  pulsante e i passi)

**Interfacce:**
- *Consuma*: le ancore `data-tutorial` del task 5; lo slot `Azione` di `TestataPagina`
  (`Shared/TestataPagina.razor:56`).
- *Produce*, per la 2.1-bis:
  ```csharp
  // Shared/Tutorial.razor
  public sealed record PassoTutorial(string Ancora, string Titolo, string Testo, string SeAssente);
  [Parameter, EditorRequired] IReadOnlyList<PassoTutorial> Passi
  [Parameter, EditorRequired] string Chiave        // "spese-tabella": nome del ricordo nel browser
  ```
  `Ancora` è il valore di un attributo `data-tutorial`; `SeAssente` è la frase che dice cosa si vedrebbe
  quando l'ancora non c'è.

- [ ] **Step 1: `wwwroot/js/tutorial.js`**

```js
export function evidenzia(ancora) {
  togli();
  const el = document.querySelector(`[data-tutorial="${CSS.escape(ancora)}"]`);
  // getClientRects vuoto: l'elemento c'è ma non si vede (gruppo chiuso, schermo stretto). Per il
  // tutorial è come se non ci fosse.
  if (!el || el.getClientRects().length === 0) return null;
  el.classList.add('tutorial-evidenziato');
  el.scrollIntoView({ block: 'center' });
  const r = el.getBoundingClientRect();
  return { sopra: r.top, sotto: r.bottom, sinistra: r.left, altezzaFinestra: window.innerHeight };
}

export function togli() {
  document.querySelectorAll('.tutorial-evidenziato').forEach(e => e.classList.remove('tutorial-evidenziato'));
}
```

- [ ] **Step 2: `Shared/Tutorial.razor`**

Rende il **pulsante «Tutorial»** e, se aperto, il **riquadro**. Spec §6.2, punto per punto:
- **Non parte da solo.** Si apre solo col pulsante.
- **Il ricordo del browser** (`localStorage`, chiave `eton.tutorial.<Chiave>`, scritto alla prima apertura)
  serve **solo** a non riproporre un invito: finché manca, il pulsante porta una pastiglia «nuovo». Letto e
  scritto con `JS.InvokeAsync("localStorage.getItem"/"setItem", …)` dentro un `try/catch` che in caso di
  guasto lascia il pulsante senza invito: se il ricordo manca, non si rompe niente. (Stessa API usata in
  `Services/SpaceStateService.cs:157,173`.)
- **Il riquadro** è `<div role="dialog" aria-modal="false" aria-labelledby="…" tabindex="-1">` con titolo,
  testo, «Passo 2 di 9», e i comandi «Indietro · Avanti · Chiudi» («Avanti» diventa «Fine» all'ultimo).
  A ogni passo: `evidenzia(passo.Ancora)`; se torna `null`, il riquadro si mostra **centrato** con
  `SeAssente` sotto il testo; altrimenti `position: fixed` sotto l'elemento, o sopra se sotto non c'è posto.
  Il fuoco va nel riquadro (`ElementReference.FocusAsync()`, senza JavaScript); **Esc chiude**
  (`@onkeydown` sul riquadro); alla chiusura `togli()` e il fuoco **torna al pulsante**. **Non oscura la
  pagina**: nessun velo.
- `IAsyncDisposable`: `togli()` e lo smontaggio del modulo, con il `try/catch` di
  `Shared/GrafoSpazio.razor:54-75`.

- [ ] **Step 3: la classe globale in `app.css`**

`.tutorial-evidenziato { outline: 3px solid var(--accento); outline-offset: 4px; }` con un commento che
dice perché sta in `app.css` e non nel CSS isolato del componente: l'elemento marcato sta **fuori** dal
componente, e il CSS isolato di Blazor non lo raggiunge. Resa provvisoria, della 2.1-bis.

- [ ] **Step 4: i passi della tabella**

Nello slot `Azione` di `TestataPagina` in `Pages/SpeseTabella.razor`: `<Tutorial Chiave="spese-tabella"
Passi="…" />`. I passi, nell'ordine della spec §6.2: il periodo; i filtri; l'ordinamento cliccando le
intestazioni (`intestazioni`); una cella modificabile e i tasti; una riga in sola lettura e perché; la
selezione e le azioni (`selezione`, poi `azioni`); il riepilogo in fondo e cosa conta — le previste sì, le
in arrivo no (`riepilogo`); l'esportazione. Ogni `SeAssente` dice cosa si vedrebbe: per
`riga-sola-lettura`, «In questo periodo non ci sono righe in sola lettura: lo sono le previste, le in
arrivo e le spese segnate da altri.»; per `azioni`, «Le azioni compaiono quando selezioni almeno una riga.».

- [ ] **Step 5: gate, commit, prova nel browser** — 0 avvisi, **385** superati. `live-testing`: il
  tutorial fino in fondo; un passo con l'ancora assente (un periodo senza righe di sola lettura, o un
  gruppo chiuso); Esc a metà e il fuoco che torna al pulsante; il ricordo nel browser cancellato a mano e la
  pagina che funziona lo stesso. Poi `ui-critic` (file `.razor` nuovo con markup).

---

## Autoverifica del piano

**Copertura della specifica.** §1 (cosa entra) → task 1-6; ciò che va altrove non ha task, ed è corretto.
§2.1 → task 1. §2.2 → task 1 (descrittori) e 4 (componente; stato di modifica nel componente). §2.3 e il
divieto → task 5 step 1 e vincoli globali. §2.4 → task 5 step 8. §3 → task 4 step 6-7 (`<table>`, densità)
e task 5 step 6 (`40rem`). §4.1 → task 2 (`TestoStato`) e task 5 step 3. §4.2 → task 2 (`Intervallo`,
`Filtra`, `VistaTabella`), task 1 (`Ordina`, `Raggruppa`), task 5 step 2. §4.3 → task 1 (`Aggrega`), task 2
(`ContaNeiTotali`, `QuantePreviste`), task 4 step 6 (`tfoot`, selezione). §4.4 → task 1 (`Csv`), task 4
(`EsportaAsync`, `scarica`), task 5 step 6. §4.5 → nessun task: è una soglia scritta, non un limite. §5.1 →
task 2 (`Modificabile`), task 5 step 3. §5.2 → task 4 per intero. §5.3 → task 2 (`Applica`), task 5 step 4.
§5.4 → task 2 (`EsitoMassa`, testo), task 3, task 5 step 5. §5.5 → nessun task, ed è la decisione. §6.1 →
task 5 step 6. §6.2 → task 6. §7 → i test dei task 1, 2, 4 (ogni voce della lista ha almeno un test: ordinamento
per tipo e stabile; filtri combinati e testo; raggruppamento e gruppi vuoti; cinque aggregazioni con zero e
una riga; riepilogo da filtrate a selezione — **questo è nel componente, quindi si prova nel browser e non
in xUnit**; previste e in arrivo; CSV; `Modificabile`; esito di massa; URL) e le prove nel browser dei task 5
e 6. §8 → divieti dentro i task 3, 4, 5. §9 → nessun task.

**Un buco dichiarato**: il passaggio del riepilogo dalle righe filtrate alla selezione (spec §7, quinta
voce) vive nel componente e non in una funzione pura; si verifica con `live-testing`, non con xUnit. Estrarlo
in puro varrebbe un metodo di due righe con un solo chiamante, contro il budget del task 4.

**Coerenza dei nomi.** `ColonnaGriglia<T>`, `TipoColonna`, `Aggregazione`, `Gruppo<T>`, `Riepilogo`,
`CalcoliGriglia.*` definiti al task 1, usati con la stessa firma ai task 4 e 5. `RigaSpesa.Chiave`,
`ContaNeiTotali`, `TabellaSpese.Colonne.*`, `VistaTabella.DaQuery`/`InQuery`, `EsitoMassa.DaEliminazione`/
`DaModifica` definiti al task 2, consumati ai task 3 e 5. `ModificaCella<T>`, `RispostaCella`, `EsitoCella`,
`EsportaAsync` definiti al task 4, consumati al task 5. `PassoTutorial` al task 6.

**Conteggi dei test.** 14 + 28 + 8 = 50 nuovi: 335 → 349 → 377 → 385. Nessun `[Theory]`, così il numero
di casi coincide col numero di metodi.

**Ordine e dipendenze.** 1 → nessuna. 2 → 1 (`RiduciAccenti` interno). 3 → 2 (`EsitoMassa`). 4 → 1.
5 → 2, 3, 4. 6 → 5 (le ancore).

---

## Partizione proposta in sessioni-unità

| Unità | Task | File (proprietà esclusiva) | Dipende da | Prova nel browser |
|---|---|---|---|---|
| **A — calcoli** | 1 e 2 | `Services/ColonnaGriglia.cs`, `Services/CalcoliGriglia.cs`, `Services/TabellaSpese.cs`, `Services/SchemaCampi.cs` (una parola), `Eton.Tests/CalcoliGrigliaTests.cs`, `Eton.Tests/TabellaSpeseTests.cs` | — | no |
| **B — scrittura e griglia** | 3 e 4 | `Services/ExpenseRepository.cs`, `Services/TastieraGriglia.cs`, `Shared/Griglia.razor`, `Shared/Griglia.razor.css`, `wwwroot/js/griglia.js`, `Eton.Tests/TastieraGrigliaTests.cs` | A | no (non c'è ancora una pagina) |
| **C — pagina** | 5 | `Pages/SpeseTabella.razor`, `Pages/SpeseTabella.razor.css`, `Shared/NavigazioneSpese.razor` | B | sì: `live-testing` + `ui-critic` |
| **D — tutorial** | 6 | `Shared/Tutorial.razor`, `wwwroot/js/tutorial.js`, `wwwroot/css/app.css`, `Pages/SpeseTabella.razor` (dopo C) | C | sì: `live-testing` + `ui-critic` |

**Perché quattro e non sei, né tre.** I task 1 e 2 sono puri, piccoli, e il 2 dipende dal 1 per una sola
visibilità: insieme fissano tutti i nomi. Il task 3 da solo non produce niente di osservabile e sta bene
accanto al 4, che è il più rischioso ma non tocca file di nessun altro. Il task 5 è il più lungo e ha la
macchina a stati delicata: merita una sessione sola, con il collaudo vero dentro. Il 6 è generico e può
fallire senza togliere niente alla tabella.

**File contesi.** `Pages/SpeseTabella.razor` (C crea, D aggiunge il pulsante e i passi): in sequenza, mai
in parallelo, e D parte dal commit di C su `origin/main`. `Shared/Griglia.razor`: solo B — se C trova un
difetto della griglia lo corregge lì, in sequenza dopo B, e lo dichiara in `SCOSTAMENTI`.
`wwwroot/css/app.css`: solo D. `Services/ExpenseRepository.cs`: solo B.
`Pages/Spese.razor`: **nessuno** — la collisione che il piano della 2.1 temeva non c'è, perché la 2.2 non
tocca il registro.

**Pubblicazione.** Nessuna migrazione, quindi nessun gate dell'utente fra le unità. Ogni unità può andare su
`main` appena integrata: A e B aggiungono codice che nessuna pagina usa ancora, C rende la pagina
raggiungibile (prima solo per URL, poi dalla sotto-navigazione allo step 8), D aggiunge il tutorial. Un
difetto dopo C si cura con `git revert` su `main`.

**Firme fra le unità** (il contratto che ogni mandato ricopia):

| Firma | Prodotta da | Consumata da |
|---|---|---|
| `ColonnaGriglia<T>`, `TipoColonna`, `Aggregazione`, `Gruppo<T>`, `Riepilogo` | A (task 1) | `Shared/Griglia.razor` (B, task 4 step 6); `Pages/SpeseTabella.razor` (C, task 5 step 3) |
| `CalcoliGriglia.Ordina/Raggruppa/Aggrega/Csv/Intervallo/TestoCella/TestoModificabile` | A (task 1) | `Shared/Griglia.razor` (B); `Pages/SpeseTabella.razor` `Ricalcola()` (C, step 2) |
| `RigaSpesa` (`Chiave`, `ContaNeiTotali`), `StatoSpesa`, `TabellaSpese.*`, `VistaTabella`, `FiltriSpese` | A (task 2) | `Pages/SpeseTabella.razor` (C, step 1-5) |
| `EsitoMassa.DaEliminazione/DaModifica` | A (task 2) | `Services/ExpenseRepository.cs`, dopo `EliminaAsync` (`:289-300`) (B, task 3) |
| `CambiaCategoriaAsync(IReadOnlyCollection<Guid>, string) → EsitoMassa`, `EliminaTutteAsync(IReadOnlyCollection<Guid>) → EsitoMassa` | B (task 3) | `Pages/SpeseTabella.razor` (C, step 5) |
| `Griglia<T>` (parametri del task 4), `ModificaCella<T>`, `RispostaCella`, `EsitoCella`, `EsportaAsync` | B (task 4) | `Pages/SpeseTabella.razor` (C, step 3-6) |
| ancore `data-tutorial` (nove nomi, task 5) | C | `Pages/SpeseTabella.razor` passi del tutorial (D, task 6 step 4) |
| `PassoTutorial`, `Tutorial` (`Passi`, `Chiave`) | D (task 6) | `Pages/SpeseTabella.razor` (D stesso); la 2.1-bis |
| esistenti: `ElencaConPrevisteAsync` (`Services/ExpenseRepository.cs:82`), `SalvaAsync` (`:231`), `Permessi.PuoIntervenire` (`Services/Permessi.cs:63`), `SchedaConflitto` (`Shared/SchedaConflitto.razor:31-35`), `ConfermaAzione` (`Shared/ConfermaAzione.razor:43-48`), `TestataPagina.Azione` (`Shared/TestataPagina.razor:56`), `NavigazioneSpese` (`Shared/NavigazioneSpese.razor:24`) | 2.1 e prima | C e D: **non cambiano firma**; un brief che proponesse di cambiarne una è fuori mandato |
