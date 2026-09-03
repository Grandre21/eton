# Spese ricorrenti — piano di implementazione

> **Per chi esegue:** questo piano si esegue con l'**architettura a sessioni-unità** del
> progetto (`~/.claude/architettura-sessioni.md`), non con le skill di orchestrazione di
> superpowers, che il `CLAUDE.md` globale vieta. Ogni task qui sotto è un'unità: un mandato,
> una sessione, un resoconto. Gli step con `- [ ]` sono la traccia interna del mandato.

**Obiettivo:** dichiarare una spesa che si ripete, e vederla comparire da sola nel registro
del mese in cui è dovuta.

**Architettura:** il passato si materializza in righe `expenses` vere, il futuro resta una
regola. Il calcolo di quali occorrenze siano dovute è una classe C# statica e pura, testata
senza database; la scrittura la fa il client all'apertura, solo per le proprie regole, con un
watermark sulla regola a impedire di riempire i buchi che l'utente ha creato di proposito.

**Tecnologie:** Blazor WebAssembly .NET 10, Supabase/PostgreSQL 17, `Supabase.Postgrest`
4.4.0, xUnit. Nessuna dipendenza nuova.

**Specifica:** `docs/superpowers/specs/2026-09-03-spese-ricorrenti-design.md` — si legge
**insieme** a questo piano, non al posto suo.

## Vincoli globali

Valgono per **ogni** task, e non si ripetono dentro ciascuno.

- **Nessuna dipendenza nuova.** Il sito sta su GitHub Pages e deve funzionare offline come
  PWA.
- **Gate di ogni task:** `dotnet build -warnaserror` → 0 errori, 0 avvisi; `dotnet test` →
  tutti verdi. Erano **267** prima di questo lavoro.
- **Gli `implementer` non compilano mai**: `obj/` non ha lock fra processi. Compila
  l'orchestratore dell'unità, una volta, a fine giro.
- **Si testa la logica pura, non il repository né le pagine.** È la tradizione del progetto,
  dichiarata nel design del 24 agosto §9: repository e pagine richiederebbero un database, e
  il confine di sicurezza vero lo prova `verifica-rls-*.sql` da dentro Postgres.
- **`wwwroot/css/app.css` ha un solo proprietario alla volta.** Un'unità che ne ha bisogno lo
  dichiara e si ferma, non ci mette una toppa inline.
- **Le migrazioni non si eseguono**: si scrivono e si consegnano all'utente, che le applica a
  mano in produzione. Nessun agente si connette al database.
- **Testo accentato**: mai `printf`, `echo -e` o `git commit -m`. Heredoc quotato o
  `git commit -F` su file UTF-8; i file si scrivono con `Write`.

---

## Struttura dei file

| File | Responsabilità | Task |
|---|---|---|
| `Services/CalcoliRicorrenti.cs` | **creare** — calcolo puro: quali periodi sono dovuti, con che data e importo | 1 |
| `Eton.Tests/CalcoliRicorrentiTests.cs` | **creare** — i test del calcolo | 1 |
| `supabase/migrations/20260904000000_ricorrenti.sql` | **creare** — tabella, due colonne su `expenses`, vincolo, RLS, grant, trigger | 2 |
| `supabase/verifica-rls-ricorrenti.sql` | **creare** — collaudo RLS che inserisce **come inserisce il client** | 2 |
| `Models/RecurringExpense.cs` | **creare** — il modello, fotocopia di `Expense.cs` | 3 |
| `Services/RecurringExpenseRepository.cs` | **creare** — CRUD sulle regole, concorrenza ottimistica come `ExpenseRepository` | 3 |
| `Services/ExpenseRepository.cs` | **modificare** — materializzazione e **percorso di lettura unico** | 4 |
| `Pages/Spese.razor`, `Pages/Home.razor` | **modificare** — passano al percorso unico | 4 |
| `Pages/Ricorrenti.razor` | **creare** — elenco delle regole | 5 |
| `Pages/RicorrenteEdit.razor` | **creare** — editor di una regola | 6 |

Il calcolo puro viene per primo di proposito: non dipende da nulla, si prova senza database,
e fissa i nomi che tutto il resto consuma.

---

## Task 1 — Il calcolo, e il buco che non si riempie

**File:**
- Creare: `Services/CalcoliRicorrenti.cs`
- Test: `Eton.Tests/CalcoliRicorrentiTests.cs`

**Interfacce:**
- *Consuma*: nulla. È il primo task e non dipende da niente.
- *Produce*, e i task 4, 5 e 6 vi si appoggiano:
  ```csharp
  public sealed record PeriodoDovuto(string Periodo, DateTime Data);
  public static IReadOnlyList<PeriodoDovuto> Dovuti(
      DateTime inizio, DateTime? fine, int ogniMesi, int giorno,
      string? materializzatoFinoA, DateTime da, DateTime a);
  public static string Periodo(DateTime data);              // "yyyy-MM"
  public static DateTime? Prossima(
      DateTime inizio, DateTime? fine, int ogniMesi, int giorno, DateTime oggi);
  ```
  `Dovuti` prende parametri primitivi e **non** il modello: così è testabile prima che il
  modello esista (task 3) e non trascina una dipendenza dal livello dati dentro una classe di
  calcolo. È la stessa forma di `CalcoliSpese.PerMese`, che prende `IEnumerable<Expense>` e
  due interi.

- [ ] **Step 1: scrivere i test che falliscono**

Nove test, e il quarto è quello che protegge il cuore del design.

```csharp
using Eton.Services;

public class CalcoliRicorrentiTests
{
    [Fact]
    public void Mensile_produce_un_periodo_per_mese()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 9, 5), fine: null, ogniMesi: 1, giorno: 5,
            materializzatoFinoA: null,
            da: new DateTime(2026, 9, 1), a: new DateTime(2026, 11, 30));

        Assert.Equal(["2026-09", "2026-10", "2026-11"], dovuti.Select(d => d.Periodo));
        Assert.Equal(new DateTime(2026, 10, 5), dovuti[1].Data);
    }

    [Fact]
    public void Ogni_tre_mesi_salta_i_due_in_mezzo()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 1, 10), fine: null, ogniMesi: 3, giorno: 10,
            materializzatoFinoA: null,
            da: new DateTime(2026, 1, 1), a: new DateTime(2026, 12, 31));

        Assert.Equal(["2026-01", "2026-04", "2026-07", "2026-10"], dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void Il_giorno_trentuno_diventa_l_ultimo_giorno_del_mese()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 1, 31), fine: null, ogniMesi: 1, giorno: 31,
            materializzatoFinoA: null,
            da: new DateTime(2026, 2, 1), a: new DateTime(2026, 4, 30));

        Assert.Equal(new DateTime(2026, 2, 28), dovuti[0].Data);   // febbraio 2026: 28 giorni
        Assert.Equal(new DateTime(2026, 3, 31), dovuti[1].Data);
        Assert.Equal(new DateTime(2026, 4, 30), dovuti[2].Data);
    }

    [Fact]
    public void Il_buco_lasciato_dall_utente_non_si_riempie()
    {
        // L'utente ha cancellato l'occorrenza di settembre. Il watermark è avanti:
        // ottobre e novembre sono dovuti, settembre NO.
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 6, 5), fine: null, ogniMesi: 1, giorno: 5,
            materializzatoFinoA: "2026-09",
            da: new DateTime(2026, 6, 1), a: new DateTime(2026, 11, 30));

        Assert.Equal(["2026-10", "2026-11"], dovuti.Select(d => d.Periodo));
        Assert.DoesNotContain("2026-09", dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void Niente_prima_dell_inizio()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 10, 1), fine: null, ogniMesi: 1, giorno: 1,
            materializzatoFinoA: null,
            da: new DateTime(2026, 8, 1), a: new DateTime(2026, 10, 31));

        Assert.Equal(["2026-10"], dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void Niente_dopo_la_fine()
    {
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 1, 15), fine: new DateTime(2026, 3, 20),
            ogniMesi: 1, giorno: 15, materializzatoFinoA: null,
            da: new DateTime(2026, 1, 1), a: new DateTime(2026, 6, 30));

        Assert.Equal(["2026-01", "2026-02", "2026-03"], dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void La_fine_esclude_l_occorrenza_che_cade_dopo_di_essa_nello_stesso_mese()
    {
        // Termina il 3 marzo, l'occorrenza di marzo cadrebbe il 15: non è dovuta.
        var dovuti = CalcoliRicorrenti.Dovuti(
            inizio: new DateTime(2026, 1, 15), fine: new DateTime(2026, 3, 3),
            ogniMesi: 1, giorno: 15, materializzatoFinoA: null,
            da: new DateTime(2026, 1, 1), a: new DateTime(2026, 6, 30));

        Assert.Equal(["2026-01", "2026-02"], dovuti.Select(d => d.Periodo));
    }

    [Fact]
    public void Periodo_formatta_a_due_cifre()
    {
        Assert.Equal("2026-03", CalcoliRicorrenti.Periodo(new DateTime(2026, 3, 7)));
    }

    [Fact]
    public void Prossima_e_null_per_una_regola_terminata()
    {
        var prossima = CalcoliRicorrenti.Prossima(
            inizio: new DateTime(2026, 1, 5), fine: new DateTime(2026, 6, 30),
            ogniMesi: 1, giorno: 5, oggi: new DateTime(2026, 9, 3));

        Assert.Null(prossima);
    }
}
```

- [ ] **Step 2: verificare che falliscano**

Comando: `dotnet test Eton.Tests/Eton.Tests.csproj --filter CalcoliRicorrentiTests`
Atteso: **errore di compilazione** — `CalcoliRicorrenti` non esiste. È il fallimento giusto.

- [ ] **Step 3: scrivere l'implementazione minima**

Una classe statica in `Services/CalcoliRicorrenti.cs`, namespace `Eton.Services`, sullo stile
di `CalcoliSpese.cs`: nessuna rete, nessuno stato, nessuna dipendenza da `Models`.

Regole, tutte deducibili dai test:
- un mese `m` è dovuto se `(m - mese di inizio)` in mesi è multiplo di `ogniMesi`;
- la data è `Math.Min(giorno, DateTime.DaysInMonth(anno, mese))`;
- si scarta il periodo se la data è prima di `inizio`, o dopo `fine`;
- si scarta il periodo se `materializzatoFinoA` non è nullo e il periodo è
  `string.CompareOrdinal(periodo, materializzatoFinoA) <= 0` — il formato `yyyy-MM` si ordina
  correttamente come stringa, ed è la ragione per cui è quel formato e non un intero;
- `Prossima` è il primo dovuto con data `> oggi`, ignorando il watermark.

- [ ] **Step 4: verificare che passino**

Comando: `dotnet test Eton.Tests/Eton.Tests.csproj --filter CalcoliRicorrentiTests`
Atteso: 9 superati.

- [ ] **Step 5: il gate completo**

Comando: `dotnet build -warnaserror` poi `dotnet test`
Atteso: 0 avvisi; **276** superati (267 + 9).

- [ ] **Step 6: commit**

---

## Task 2 — Lo schema, e lo script che collauda ciò che l'app fa davvero

**File:**
- Creare: `supabase/migrations/20260904000000_ricorrenti.sql`
- Creare: `supabase/verifica-rls-ricorrenti.sql`

**Interfacce:**
- *Consuma*: nulla dal task 1.
- *Produce*: i nomi di colonna che il task 3 mappa nel modello. Il resoconto deve citarli
  testualmente.

**Il modello da fotocopiare è `supabase/migrations/20260824000000_spese.sql`.** Aprilo e
seguilo: stessa struttura di commenti, stesso ordine delle sezioni, stessa forma di trigger,
RLS e grant. Non inventare uno stile.

- [ ] **Step 1: la tabella e le due colonne**

Lo schema esatto è al §3.1 e §3.2 della specifica. Copiarlo da lì, non riscriverlo a memoria.

Tre cose che si sbagliano facilmente e che la specifica motiva:
- la FK `expenses.recurring_id` è **`on delete no action`**, cioè il default: non
  `set null`, non `restrict`. Il §6.3 spiega perché ognuna delle altre due rompe qualcosa.
- il vincolo `unique (recurring_id, recurring_period)`.
- `recurring_id` e `recurring_period` vanno nel `grant insert` di `expenses` e **non** nel
  `grant update`; il trigger `handle_expense_update` va esteso perché le rimetta a forza ai
  valori precedenti, come fa già con `paid_by`.

- [ ] **Step 2: RLS e grant, fotocopia di `expenses`**

`select` per i membri; `insert` per i membri con `paid_by = auth.uid()`; `update`/`delete`
per il pagante o per chi possiede lo spazio. `revoke all` seguito da grant espliciti, e
**mai** `version`, `created_at`, `updated_at` fra le colonne concesse.

`materialized_through` **sì** in `grant update`: lo scrive il client dopo aver materializzato.

Il grant si scrive nella **forma minima per colonna**, come
`20260903000000_grant_insert_blind.sql`: è la convenzione adottata il 3 settembre dopo che
la forma a elenco completo aveva prodotto un difetto rimasto invisibile due settimane.

- [ ] **Step 3: lo script di verifica RLS**

Modello: `supabase/verifica-rls-spese.sql`. Due utenti finti impersonati con
`set local request.jwt.claims`, e in testa la dichiarazione di quanti errori lo script **deve**
produrre.

**Il vincolo che questo script esiste per rispettare:** ogni `insert into public.expenses`
deve elencare **tutte** le colonne che il client invia, `recurring_id` e `recurring_period`
comprese. Uno script che inserisce un sottoinsieme collauda un percorso che l'applicazione non
usa — è esattamente il difetto che ha tenuto le collezioni rotte per due settimane senza che
nulla diventasse rosso.

Casi da coprire almeno: un membro crea una regola nel proprio spazio; un estraneo non la
vede; il pagante materializza un'occorrenza; **un secondo inserimento con lo stesso
`(recurring_id, recurring_period)` fallisce** con 23505; eliminare una regola con occorrenze
fallisce con 23503; eliminare lo **spazio** funziona lo stesso (è il caso che distingue
`no action` da `restrict`).

- [ ] **Step 4: gate**

`dotnet build -warnaserror` e `dotnet test`. Il test statico `PrivilegiInsertTests` copre
**automaticamente** la tabella nuova: se il grant dimenticasse una colonna che il modello
invierà, fallirebbe. Al task 2 il modello non esiste ancora, quindi non fallirà: **rileggere
questo punto al task 3**.

- [ ] **Step 5: commit, e consegna all'utente**

Il resoconto deve contenere, sotto `DA CONSEGNARE ALL'UTENTE`, il **testo integrale della
migrazione**. La applica lui in produzione: nessun agente si connette al database.

---

## Task 3 — Il modello e il repository delle regole

**File:**
- Creare: `Models/RecurringExpense.cs`
- Creare: `Services/RecurringExpenseRepository.cs`
- Modificare: `Program.cs` (registrazione del servizio, come per `ExpenseRepository`)

**Interfacce:**
- *Consuma*: i nomi di colonna del task 2.
- *Produce*, e i task 4, 5 e 6 vi si appoggiano:
  ```csharp
  public async Task<IReadOnlyList<RecurringExpense>> ElencaAsync(Guid spazioId);
  public async Task<RecurringExpense?> LeggiAsync(Guid regolaId);
  public async Task<RecurringExpense> CreaAsync(Guid spazioId, Guid pagante, decimal importo,
      string descrizione, string categoria, int ogniMesi, int giorno, DateTime inizio);
  public async Task<RisultatoSalvataggio<RecurringExpense>> SalvaAsync(Guid regolaId,
      int versioneLetta, decimal importo, string descrizione, string categoria,
      int ogniMesi, int giorno, DateTime inizio);
  public async Task<RisultatoSalvataggio<RecurringExpense>> TerminaAsync(Guid regolaId,
      int versioneLetta, DateTime fine);
  public async Task<bool> EliminaAsync(Guid regolaId);
  public async Task AvanzaWatermarkAsync(Guid regolaId, string periodo);
  ```

- [ ] **Step 1: il modello, fotocopia di `Models/Expense.cs`**

Aprire `Expense.cs` e seguirlo. Tre cose che si sbagliano:
- `[PrimaryKey("id", true)]` con `ShouldInsert = true`, come `Expense` e **non** come `Note`:
  l'id lo genera il client per l'idempotenza sui ritentativi di rete;
- `ignoreOnInsert: true` su `Version`, `CreatedAt`, `UpdatedAt`, e **su nient'altro**. È la
  colonna dimenticata di questo attributo che ha tenuto le collezioni rotte due settimane;
- `DateTime` e non `DateOnly` per `starts_on`/`ends_on`: il motivo è scritto in
  `Models/Expense.cs:34-38` e riguarda il trimming in Release.

- [ ] **Step 2: verificare che `PrivilegiInsertTests` sia verde**

Comando: `dotnet test Eton.Tests/Eton.Tests.csproj --filter PrivilegiInsert`
Atteso: verde. Se è **rosso**, il grant del task 2 e il modello non concordano: è il test che
esiste apposta per questo, e va corretta la parte sbagliata, non il test.

- [ ] **Step 3: il repository, fotocopia di `ExpenseRepository.cs`**

Concorrenza ottimistica identica: `versioneLetta` è **filtro di query**, non valore scritto; a
zero righe modificate si rilegge per distinguere `Sparita` / `Conflitto` / `Rifiutata`.
Le date passano da `PerIlDatabase`, che è `internal static` in `ExpenseRepository`: **non
duplicarlo**, renderlo condiviso o richiamarlo.

`TerminaAsync` è un `SalvaAsync` che scrive solo `ends_on`; esiste come metodo a sé perché è
un'azione diversa nell'interfaccia e non deve poter cambiare altro per sbaglio.

- [ ] **Step 4: gate e commit**

---

## Task 4 — La materializzazione, e il percorso di lettura unico

È il task più delicato del piano: tocca codice che già funziona ed è letto da due pagine.

**File:**
- Modificare: `Services/ExpenseRepository.cs`
- Modificare: `Pages/Spese.razor`, `Pages/Home.razor`

**Interfacce:**
- *Consuma*: `CalcoliRicorrenti.Dovuti` (task 1), `RecurringExpenseRepository` (task 3).
- *Produce*:
  ```csharp
  public sealed record SpeseDelPeriodo(IReadOnlyList<Expense> Righe, IReadOnlySet<Guid> Previste);
  public async Task<SpeseDelPeriodo> ElencaConPrevisteAsync(Guid spazioId, DateTime da, DateTime a);
  ```
  `Previste` contiene gli `Id` sintetici delle righe non ancora scritte: le pagine le marcano
  e non ci mettono un collegamento.

- [ ] **Step 1: la materializzazione**

All'apertura, per ogni regola **di cui l'utente è il pagante**: calcolare i periodi dovuti con
data `<= oggi`, scriverli con `Upsert` e `DuplicateResolution.IgnoreDuplicates`, poi avanzare
il watermark.

**`IgnoreDuplicates` e mai `MergeDuplicates`**: il secondo è un UPDATE e sovrascriverebbe con
l'importo della regola una spesa che il pagante aveva già corretto a mano. È il difetto
silenzioso e distruttivo del §9 della specifica.

- [ ] **Step 2: la fusione**

`ElencaConPrevisteAsync` legge le righe vere con `ElencaAsync`, calcola le occorrenze
**previste** (periodi dovuti oltre il watermark, di regole di **chiunque**), e le fonde:

- una previsione il cui `(recurring_id, periodo)` esiste già fra le righe vere **non** si
  aggiunge — la riga vera vince sempre;
- le previste con data `<= oggi` entrano nell'elenco e nei totali;
- le previste con data `> oggi` **non** entrano: sono «in arrivo» e le mostrerà il task 5.

- [ ] **Step 3: i due chiamanti passano al percorso unico**

`Pages/Spese.razor` e `Pages/Home.razor` chiamano oggi `ElencaAsync` direttamente. Devono
passare a `ElencaConPrevisteAsync`.

> **Divieto, dal §5 della specifica.** Dopo questo task **nessun chiamante nuovo** può usare
> `ElencaAsync` direttamente. Se anche uno solo lo fa, i totali di due membri divergono e
> nessun test se ne accorge. Vale per la vista tabellare e per l'analisi, che arriveranno
> dopo.

Attenzione a `Pages/Spese.razor`: 502 righe con una macchina a stati documentata come
delicata ai suoi commenti interni, dove le guardie vanno alzate **come ultima istruzione
sincrona prima del primo `await`**. Questo task **non** deve aggiungere un chiamante nuovo di
`Carica()`: sostituisce la chiamata dentro quello che c'è già.

- [ ] **Step 4: gate, e la prova che la fusione non somma due volte**

Il comportamento «la riga vera vince sulla previsione» è logica pura e **va testato**:
estrarre la fusione in un metodo statico puro accanto a `CalcoliRicorrenti` e scrivere il
test. Se resta dentro il repository non è testabile, e questa è la regressione più probabile
dell'intero lavoro.

- [ ] **Step 5: commit**

---

## Task 5 — L'elenco delle regole

**File:**
- Creare: `Pages/Ricorrenti.razor` (`@page "/expenses/recurring"`)

**Interfacce:**
- *Consuma*: `RecurringExpenseRepository.ElencaAsync` (task 3), `CalcoliRicorrenti.Prossima`
  (task 1).
- *Produce*: nulla che altri task consumino.

- [ ] **Step 1: la pagina**

Eredita `Shared/PaginaRegistro.cs`, come Note e Collezioni. **Non** usare i suoi due punti di
estensione (`ScartaCambioSpazio`, `PrimaDiRicaricare`): oggi li usa solo `Spese.razor`, e
aggiungere un secondo utente di quella meccanica delicata non serve a questa pagina.

Ogni riga: descrizione, importo, categoria, cadenza in parole («ogni 2 mesi, il 5») e
**prossima occorrenza** da `CalcoliRicorrenti.Prossima`. Le regole terminate si distinguono da
quelle attive. Stato vuoto che spiega il concetto, come fanno gli altri registri.

Testata con `<TestataPagina>` e infobutton, che deve dire ciò che la pagina **non** dice già
da sé.

- [ ] **Step 2: la sotto-navigazione**

Con questa pagina le spese diventano tre schermate e la barra di navigazione ha cinque posti
già occupati. Serve una riga di collegamenti in testa («Registro · Ricorrenti»).
**Chiedere all'utente la forma** prima di inventarla: la specifica §6.4 la dichiara sua
preferenza.

- [ ] **Step 3: gate e commit**

---

## Task 6 — L'editor di una regola

**File:**
- Creare: `Pages/RicorrenteEdit.razor` (`@page "/expenses/recurring/{Id:guid}"` e la variante
  di creazione)

**Interfacce:**
- *Consuma*: `RecurringExpenseRepository` (task 3), `Shared/PaginaEditor.cs`.
- *Produce*: nulla.

- [ ] **Step 1: adottare il contratto degli editor**

`@inherits PaginaEditor`, `Cambiata` come `protected override`, `<NavigationLock>` **dentro il
ramo del modulo**, i `NavigateTo` post-creazione ed eliminazione sostituiti da `Esci(...)`,
`@inject NavigationManager` **non** dichiarato. La firma reale è in `handoff/PIANO.md`,
sezione `CONTRATTO`.

Il gate di «Chiudi»: `href="@(occupato ? null : "…")"` con `null` **letterale** — mai `?? ""`,
che produrrebbe un link valido verso la radice dell'applicazione.

- [ ] **Step 2: il modulo**

Cadenza come `select` a cinque voci (mensile, ogni 2, ogni 3, ogni 6, annuale), non un numero
libero. Giorno del mese 1-31 con una riga d'aiuto: «31 = ultimo giorno del mese».

Due frasi che il modulo **deve** dire, perché il modello le implica e l'utente non può
dedurle:
- «L'importo è quello atteso: se una bolletta arriva diversa, la correggi sulla spesa del
  mese.»
- «Le spese già segnate non cambiano.»

- [ ] **Step 3: «Termina», non «Elimina»**

L'azione in primo piano scrive `ends_on` con `TerminaAsync`. «Elimina» esiste ma è secondaria,
e **fallisce sul database** con 23503 se la regola ha generato occorrenze: l'interfaccia deve
dirlo prima, non lasciar sbattere l'utente contro l'errore. Il messaggio segue la forma delle
sei frasi scritte dall'unità 05 in `Pages/CollectionEdit.razor`: fatto, causa, azione.

- [ ] **Step 4: gate, commit, e prova nel browser**

Dopo questo task il lavoro è funzionalmente completo: è il momento di `live-testing`, non
prima. L'applicazione la avvia l'orchestratore, annotando porta e PID **su disco**, e la ferma
a ciclo chiuso — entrambi i processi, perché su Windows la morte del padre non uccide il
figlio.

---

## Autoverifica del piano

**Copertura della specifica.** §1 → task 1 e 4. §3.1 e §3.2 → task 2. §4.1 (watermark) →
task 1 step 1 test 4, e task 4 step 1. §4.2 → task 1. §4.3 (`IgnoreDuplicates`) → task 4
step 1. §5 (previsto e percorso unico) → task 4 step 2 e 3. §6.1 → task 5. §6.2 → task 6
step 1 e 2. §6.3 → task 2 step 1 e task 6 step 3. §6.4 → task 5 step 2. §7 → task 2 step 2 e
3. §8 → task 1 e task 4 step 4. §10 (fuori scope) → nessun task, ed è corretto.

**Nessun buco trovato.** L'unico punto della specifica senza un task dedicato è il §9, che
elenca i modi in cui il design può sbagliare: è materia da mandato, e infatti i tre modi
compaiono come divieti espliciti nei task 4 e 6.

**Coerenza dei nomi.** `CalcoliRicorrenti.Dovuti` / `Periodo` / `Prossima` sono usati con la
stessa firma nei task 1, 4 e 5. `ElencaConPrevisteAsync` compare nei task 4 e — come divieto —
nei lavori futuri. `TerminaAsync` è definito nel task 3 e consumato nel task 6.

**Ordine e dipendenze.** 1 → nessuna. 2 → nessuna. 3 → 2. 4 → 1, 3. 5 → 1, 3. 6 → 3.
Il task 1 può girare per primo e in isolamento, ed è quello che dà più valore per unità di
rischio: fissa i nomi e prova il cuore del design senza toccare niente di esistente.
