UNITÀ: 2 — ESITO: FATTO — verifica: `dotnet test Eton.sln --no-build` → Superati: 328, Non superati: 0

Branch: `worktree-unita-02-regole-e-lettura` (worktree `.claude/worktrees/unita-02-regole-e-lettura`). Non pushato su `main`: integra il capo.

TOCCATI:
- `Models/RecurringExpense.cs` → +40/−0 (nuovo)
- `Services/RecurringExpenseRepository.cs` → +158/−0 (nuovo)
- `Program.cs` → +1/−0 (registrazione di `RecurringExpenseRepository`)
- `Models/Expense.cs` → +10/−0 (le due colonne `recurring_id`, `recurring_period`)
- `Services/ExpenseRepository.cs` → +111/−1 (percorso unico, materializzazione, `ElencaAsync` privato, tipo di sola scrittura `OccorrenzaRicorrente`)
- `Services/CalcoliRicorrenti.cs` → +62/−0 (solo aggiunte: `SpeseDelPeriodo`, `Occorrenza`, `Fondi`; firme esistenti intatte)
- `Pages/Spese.razor` → +20/−3 (lettura dentro `Leggi`, marcatura delle previste, una riga d'aiuto)
- `Pages/Home.razor` → +2/−1 (lettura)
- `Eton.Tests/FusioneRicorrentiTests.cs` → +154/−0 (nuovo, 7 test)
- `handoff/02-regole-e-lettura/resoconto.md` → questo file

REVIEW:

review: A — modello e repository delle regole
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 2
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 4
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 3 · non risolti 0 · non verificabili 0

review: B — materializzazione e percorso unico di lettura
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 2
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 3
  checker         VERDETTI: fondati 0 · infondati 0 · fuori scope 2 · non verificabili 0
  checker         VERDETTI: risolti 4 · non risolti 0 · non verificabili 0

Misure del gate, a implementer rientrati e dopo `git add -N .`:
- A: `3 files changed, 194 insertions(+)` · 2 create mode · 2 dichiarazioni/endpoint → backend-expert dovuto.
- B: `6 files changed, 348 insertions(+), 6 deletions(-)` · 1 create mode · 3 dichiarazioni/endpoint → backend-expert dovuto.
`coverage` non lanciato: dentro un'unità non si lancia (§6).
`live-testing` non lanciato: v. SCOSTAMENTI.

Consultati fuori dal ciclo di review:
- `doc-checker` (caso 2: `Upsert`/`QueryOptions` mai usati nel codebase), su Supabase.Postgrest 4.4.0: firme e nomi confermati; **claim 3 parziale** — v. SCOSTAMENTI, è il fatto che ha cambiato il disegno della scrittura.
- `tech-advisor`, sul modo di scrivere le occorrenze dopo quel fatto: «(a), senza riserve» — un tipo di sola scrittura `internal sealed` nel file del repository, più un test che gli vieti i flag `ignore*`.

CONTRATTI (firme come sono risultate):
- `Services/CalcoliRicorrenti.cs:15-16` `public sealed record SpeseDelPeriodo(IReadOnlyList<Expense> Righe, IReadOnlySet<Guid> Previste,` / `    IReadOnlyList<Expense> InArrivo);`
- `Services/ExpenseRepository.cs:82` `public async Task<SpeseDelPeriodo> ElencaConPrevisteAsync(Guid spazioId, DateTime da, DateTime a)`
  - Semantica: legge le regole dello spazio; materializza le occorrenze con data ≤ oggi delle regole con `PaidBy` = utente corrente (`Upsert` con `OnConflict = "recurring_id,recurring_period"` e `IgnoreDuplicates`, `:134-135`; poi il watermark); legge le righe vere in `[da, a]`; fonde. `Righe` = vere + previste scadute, ordinate per `SpentOn` decrescente: sono ciò che entra nei totali. `Previste` = Id sintetici (non esistono nel database: nessun collegamento). `InArrivo` = occorrenze con data > oggi dentro `[da, a]`, fuori da `Righe`. Se la lettura delle regole fallisce, il metodo lancia: senza regole il totale sarebbe sbagliato senza dirlo. Se fallisce la scrittura di una regola, si registra in console e la previsione copre comunque il totale.
- `Services/CalcoliRicorrenti.cs:109` `public static SpeseDelPeriodo Fondi(IReadOnlyList<Expense> vere, IReadOnlyList<RecurringExpense> regole,` (+ `DateTime da, DateTime a, DateTime oggi)`) — la fusione pura, testata.
- `Services/CalcoliRicorrenti.cs:91` `public static Expense Occorrenza(RecurringExpense r, PeriodoDovuto d) => new()` — l'unica sede della forma di un'occorrenza (prevista o scritta); `RecurringPeriod` = primo del mese.
- `Services/ExpenseRepository.cs:63` `private async Task<IReadOnlyList<Expense>> ElencaAsync(Guid spazioId, DateTime da, DateTime a)` — **privato**.
- `Models/Expense.cs:37-38` `[Column("recurring_id",     ignoreOnUpdate: true)] public Guid?     RecurringId     { get; set; }` · `[Column("recurring_period", ignoreOnUpdate: true)] public DateTime? RecurringPeriod { get; set; }`
- `Models/RecurringExpense.cs:12` `public class RecurringExpense : BaseModel` su `[Table("recurring_expenses")]`; `ignoreOnInsert` solo su Version/CreatedAt/UpdatedAt.
- `Services/RecurringExpenseRepository.cs` (firme del task 3 del piano, identiche):
  - `:22` `public async Task<IReadOnlyList<RecurringExpense>> ElencaAsync(Guid spazioId)` (dalla modificata più di recente)
  - `:34` `public async Task<RecurringExpense?> LeggiAsync(Guid regolaId)`
  - `:43` `public async Task<RecurringExpense> CreaAsync(Guid spazioId, Guid pagante, decimal importo, string descrizione, string categoria, int ogniMesi, int giorno, DateTime inizio)`
  - `:72` `public async Task<RisultatoSalvataggio<RecurringExpense>> SalvaAsync(Guid regolaId, int versioneLetta, decimal importo, string descrizione, string categoria, int ogniMesi, int giorno, DateTime inizio)`
  - `:91` `public async Task<RisultatoSalvataggio<RecurringExpense>> TerminaAsync(Guid regolaId, int versioneLetta, DateTime fine)`
  - `:132` `public async Task<bool> EliminaAsync(Guid regolaId)` — **per l'unità 3**: su una regola che ha già generato occorrenze la DELETE fallisce con 23503 (FK no action) e l'eccezione risale al chiamante; non restituisce `false`.
  - `:150` `public async Task AvanzaWatermarkAsync(Guid regolaId, string periodo)` — senza filtro di versione. **Per l'unità 3**: il trigger alza comunque `version`, quindi se Spese o Home materializzano mentre l'editor di una regola è aperto, il salvataggio dell'editor riceve `Conflitto`.
- Divieto §5, verificato col grep `\.ElencaAsync\(` su `*.cs`/`*.razor`: nessun chiamante di `ExpenseRepository.ElencaAsync` fuori dalla classe (i risultati sono `NoteRepository`, `CollectionRepository`, `CollectionItemRepository`, `SpaceRepository` e `_ricorrenti.ElencaAsync` a `ExpenseRepository.cs:85`). L'unico chiamante è `ElencaConPrevisteAsync` (`:87`). Essendo `private`, un chiamante nuovo **non compila**. Spese (`Pages/Spese.razor`, in `Leggi`) e Home (`Pages/Home.razor:418-419`) leggono solo da `ElencaConPrevisteAsync`.

ADJUDICA:
- A · conformity 1 · SpaceId/PaidBy senza commento di rimando → **fondato → corretto**.
  `// Scrivibili alla creazione, mai più dopo: nemmeno qui il grant UPDATE li comprende. V. Expense.SpaceId.`
  verificato: risolto — Models/RecurringExpense.cs:17-19
- A · conformity 2 · `ElencaAsync` senza summary sull'ordinamento, che l'indice (solo space_id) non copre → **fondato → corretto**.
  `/// A differenza di quelle, l'ordine NON è coperto da un indice: 'recurring_expenses_space_idx'`
  verificato: risolto — Services/RecurringExpenseRepository.cs:18-22
- A · backend-expert 4 · rimandi per numero di riga a `Expense.cs`, che il brief B stava modificando → **fondato → corretto**, con rimandi al membro.
  `// DateTime e non DateOnly per lo stesso motivo: v. Expense.SpentOn.`
  verificato: risolto — Models/RecurringExpense.cs:17,28,36
- A · backend-expert 1 (`TIPO: progetto`) · la classificazione a zero righe Salvata/Sparita/Conflitto/Rifiutata è ora la sesta copia nel progetto → **all'utente**, v. FUORI SCOPE.
- A · backend-expert 2 · `CreaAsync`/`SalvaAsync` con 8 parametri posizionali e due `int` adiacenti (`ogniMesi`, `giorno`) → **fondato, fuori scope**: le firme sono il contratto del task 3, consumato dall'unità 3. V. FUORI SCOPE.
- A · backend-expert 3 · `PerIlDatabase` vive in `ExpenseRepository` ma ha due consumatori → **fondato, fuori scope**: spostarlo tocca `Eton.Tests/ExpenseRepositoryTests.cs`, che è fuori perimetro. Il piano ammetteva «richiamarlo». V. FUORI SCOPE.
- B · backend-expert 1 · la forma dell'occorrenza era scritta due volte (Fondi e MaterializzaAsync) → **fondato → corretto** con `CalcoliRicorrenti.Occorrenza`.
  `public static Expense Occorrenza(RecurringExpense r, PeriodoDovuto d) => new()`
  verificato: risolto — Services/CalcoliRicorrenti.cs:91-102, Services/ExpenseRepository.cs:111-118
- B · backend-expert 2 · `previste.Contains(s.Id)` valutato due volte per riga → **fondato → corretto**.
  `var prevista = previste.Contains(s.Id);`
  verificato: risolto — Pages/Spese.razor:183, 194, 199
- B · backend-expert 3 · tre parametri dell'helper di test mai passati → **fondato → corretto**.
  verificato: risolto — Eton.Tests/FusioneRicorrentiTests.cs:10-26
- B · conformity 1 · `ExpenseRepository` iniettava `AuthStateService` solo per l'id utente; nessun repository lo fa → checker: «fuori scope» (era prescritto dal mio brief). **Lo accolgo: l'errore era nel brief.** Corretto: l'id si legge dal client già in mano, come `AllineatoreProfilo`.
  `if (!Guid.TryParse(client.Auth.CurrentSession?.User?.Id, out var io)) return;`
  verificato: risolto — Services/ExpenseRepository.cs:23-27, 98-100
- B · conformity 2 · primo try/catch con log dentro un repository → checker: «fuori scope». **Scartato come fix, e il codice resta com'è.** L'isolamento per regola è voluto dal design (§4.3/§5: la previsione copre il totale anche senza materializzazione). Il fix proposto, un conteggio dentro `SpeseDelPeriodo`, cambierebbe un contratto fissato dal mandato. È l'unico rilievo scartato dell'unità, e l'ho riverificato io sul codice:
  `Console.Error.WriteLine($"[Ricorrenti] Occorrenze non scritte per una regola: {ex.Message}");`
- Aperti da me senza intermediari, perché toccano dati e concorrenza: `ExpenseRepository.ElencaConPrevisteAsync`/`MaterializzaAsync` e `CalcoliRicorrenti.Fondi`, letti per intero dopo il brief B. Ne è venuto un caso limite, riportato in FUORI SCOPE punto 4.

FUORI SCOPE (per l'utente):
1. (backend-expert A1, `TIPO: progetto`) La classificazione a zero righe Salvata/Sparita/Conflitto/Rifiutata esiste in sei copie identiche: Note, Collection, CollectionItem, Review, Expense, RecurringExpense. La proposta è un helper generico unico accanto a `RisultatoSalvataggio<T>`, a cui i sei repository delegano.
2. (backend-expert A2) `RecurringExpenseRepository.CreaAsync`/`SalvaAsync`: otto argomenti posizionali, e `ogniMesi`/`giorno` scambiati compilano. La proposta è un record `DatiRegola`. Conviene decidere **prima** che l'unità 3 scriva l'editor che li chiama.
3. (backend-expert A3) `PerIlDatabase` spostato in una classe statica propria (`DatePostgrest`), dato che non riguarda le spese ma il percorso `.Set()` di Postgrest. Toccherebbe `ExpenseRepositoryTests.cs`.
4. (mio, lettura diretta) Un caso limite della fusione: una regola con il watermark rimasto indietro (materializzazione riuscita, ma avanzamento del watermark fallito) la cui occorrenza materializzata è stata poi spostata a mano **fuori** dal mese. Consultando quel mese, la riga vera non è fra le `vere` lette, quindi la previsione ricompare. Richiede due guasti in sequenza, e al giro successivo il watermark si riallinea. Non l'ho corretto: il rimedio (leggere le righe per `recurring_period` oltre che per `spent_on`) cambierebbe la query di lettura.

GATE:
dotnet build Eton.sln -warnaserror → Avvisi: 0, Errori: 0
dotnet test Eton.sln --no-build → Superato! Non superati: 0. Superati: 328. Totale: 328 (319 + 7 di `FusioneRicorrentiTests` + 2 casi nuovi della Theory di `PrivilegiInsertTests`, per `RecurringExpense` e `OccorrenzaRicorrente`)
dotnet test Eton.sln --no-build --filter PrivilegiInsert → Superati: 11, Non superati: 0

SCOSTAMENTI:
- **Il contratto di scrittura del mandato, preso alla lettera, non funzionava, e la forma è cambiata (da ratificare).** Il mandato prescrive `Upsert` con `OnConflict` e `IgnoreDuplicates`. `doc-checker` ha verificato sul sorgente del tag v4.4.0 (`PostgrestContractResolver.CreateProperty`) che un `Upsert` serializza con `IsInsert` e `IsUpsert` **insieme**, e con `IsUpsert` scarta anche le colonne `IgnoreOnUpdate`:
  `if ((IsUpsert && columnAttribute.IgnoreOnUpdate) || (IsUpsert && columnAttribute.IgnoreOnInsert)) prop.Ignored = true;`
  Un `Upsert<Expense>` avrebbe quindi omesso `space_id`, `paid_by`, `recurring_id` e `recurring_period` → 23502 in produzione, al primo «apri Spese» di chi ha una regola. `PrivilegiInsertTests` non l'avrebbe visto: controlla le colonne **in più** rispetto ai grant, non quelle mancanti. La correzione, con la posizione di `tech-advisor`: `Upsert`, `OnConflict` e `IgnoreDuplicates` restano come da contratto, ma su un tipo di sola scrittura `internal sealed class OccorrenzaRicorrente` (`Services/ExpenseRepository.cs:311`) con le nove colonne del grant INSERT e nessun flag `ignore*`. Il test `Il_modello_di_scrittura_non_omette_nessuna_colonna` lo fissa. `Expense` resta con `ignoreOnUpdate` sulle colonne nuove, come chiedeva il mandato. Si annulla con un revert locale.
- **`ElencaAsync` è diventato `private` (da ratificare).** Il mandato chiedeva un grep. Oltre al grep, il divieto ora è imposto dal compilatore. Si annulla togliendo una parola.
- **`live-testing` (§7) non lanciato.** Il diff tocca il markup di Spese (marcatura «prevista»). Però l'app di sviluppo punta a Supabase di produzione (`wwwroot/appsettings.json:3`, nessun ambiente di collaudo), e in produzione nessuna regola esiste ancora: l'interfaccia per crearle è dell'unità 3. Una prova oggi vedrebbe solo che le pagine caricano ancora; vedere una «prevista» richiederebbe scrivere una regola in produzione, che è un gesto dell'utente. Il piano fissa la prova nel browser al task 6. `ui-critic`: non dovuto, perché è markup modificato in un `.razor` esistente, senza file nuovi e senza metro §0.
- **Un implementer ha violato il divieto di compilare.** L'implementer del brief B ha lanciato `dotnet build Eton.Tests/Eton.Tests.csproj` una volta, e se n'è accorto a build finita. In quel momento nessun altro implementer era attivo (il brief A era già rientrato), quindi `obj/` non è stato conteso; la build ufficiale l'ho rifatta io da capo. Lo riporto perché è un comando non richiesto, dello stesso genere dello script Python dell'unità 1.
- Aggiunta non chiesta ma nel perimetro: un terzo `<p>` nell'aiuto di Spese, che spiega all'utente la pastiglia «prevista».
- Lavoro nuovo arrivato durante l'unità: nessuno.
