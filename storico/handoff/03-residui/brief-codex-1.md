# Brief codex-1 — residui dati della 2.3 (Eton, Blazor WASM, net10.0)

Lavori nel worktree `G:\Sviluppo\wt-Eton\03-residui`. Leggi tu i file citati con `file:line` prima di
scrivere: sono il modello da rispecchiare. Commenti e testi in italiano, nello stile dei file vicini
(commenti che dicono il perché, non il cosa; densità come `Services/RecurringExpenseRepository.cs`).
Quattro punti indipendenti. Nessuna firma pubblica cambia, nessun comportamento visibile cambia salvo
il punto 1.

## Punto 1 — `CambiaCategoriaAsync` non assegna una categoria dell'altro tipo

File: `Services/ExpenseRepository.cs`, solo il metodo `CambiaCategoriaAsync` (`:345-374`) e un helper
statico subito sopra di esso.

Oggi l'UPDATE di massa (`:361-368`) scrive `categoria.Trim()` su tutti gli id, senza guardare `kind`.
Il database accetta qualunque testo (decisione ratificata il 24 ott: il controllo sta nel client, il
database resta senza vincolo — non toccare `supabase/migrations/`).

- Aggiungi, subito sopra il metodo, `internal static IReadOnlyList<string> TipiAmmessi(string categoria)`:
  i valori di `TipoMovimento` (`Models/Expense.cs:66-72`, `Spesa` ed `Entrata`, in quest'ordine) per cui
  `CategorieSpesa.Conosciuta(categoria, tipo)` (`Services/CategorieSpesa.cs:31`) è vero. «Altro» sta in
  entrambi gli elenchi → due tipi; «Casa» → solo `expense`; «Stipendio» → solo `income`; un testo
  sconosciuto → nessuno. Commento di una riga: esiste come helper perché è la regola che i test provano
  senza database.
- Nel metodo: `var valore = categoria.Trim();` e `var tipi = TipiAmmessi(valore);`. Se `tipi` è vuoto non
  parte nessun UPDATE (zero blocchi). Altrimenti ogni UPDATE aggiunge, accanto al filtro su `id`,
  `.Filter("kind", Constants.Operator.In, tipi.Cast<object>().ToList())` e scrive `valore`.
- Il resto resta com'è: le non toccate si rileggono con `LeggiIdAsync` e `EsitoMassa.DaModifica`
  (`Services/TabellaSpese.cs:171`) le conta. Un movimento dell'altro tipo resta presente e non toccato,
  quindi cade fra le `Rifiutate`: è l'esito che la funzione già restituisce, e va bene così (non
  aggiungere campi a `EsitoMassa`, non cambiare `TestoEsitoMassa`).
- Aggiorna il `<summary>` del metodo con una frase: il filtro su `kind` impedisce di dare una categoria
  di spesa a un'entrata o il contrario (spec 2.4 §5.1); quelle righe tornano fra le rifiutate. La pagina
  (`Pages/SpeseTabella.razor:675`) già non lo chiede: è la difesa nel punto che scrive.

## Punto 2 — test dedicati a `RecurringExpenseRepository`

File: `Services/RecurringExpenseRepository.cs` (solo le due estrazioni qui sotto) e il file nuovo
`Eton.Tests/RecurringExpenseRepositoryTests.cs`, sul modello di `Eton.Tests/ExpenseRepositoryTests.cs`
(commento di testa che dice cosa si prova e perché senza database; `[Fact]` con nomi in italiano e
underscore). Il modello delle estrazioni è `ExpenseRepository.LeggiABlocchiAsync`
(`Services/ExpenseRepository.cs:32-50`): un `internal static` che riceve per delegate ciò che altrimenti
chiederebbe al database. `InternalsVisibleTo` verso `Eton.Tests` c'è già (`Eton.csproj:48`).

Estrazioni, senza cambiare comportamento:
- `internal static RecurringExpense NuovaRegola(Guid spazioId, Guid pagante, DatiRegola dati)`: il corpo
  dell'inizializzatore di `CreaAsync` (`:52-68`), `Id = Guid.NewGuid()` compreso. `CreaAsync` diventa
  `Insert(NuovaRegola(spazioId, pagante, dati))`.
- Il metodo privato `Esito` (`:124-137`) diventa `internal static async Task<RisultatoSalvataggio<RecurringExpense>> Esito(IReadOnlyList<RecurringExpense> toccate, int versioneLetta, Func<Task<RecurringExpense?>> rileggi)`,
  stessa logica (prima riga toccata → Salvata; altrimenti rilegge: null → Sparita; versione diversa →
  Conflitto; uguale → Rifiutata). `SalvaAsync` e `TerminaAsync` lo chiamano con
  `risposta.Models` e `() => LeggiAsync(regolaId)`. Mantieni il `<summary>` esistente, aggiungendo che il
  delegate serve ai test. Il `using Supabase.Postgrest.Responses;` si toglie se non serve più.

Test, un `[Fact]` ciascuno:
- `NuovaRegola`: descrizione e categoria arrivano senza spazi ai bordi; `StartsOn` è la data di inizio a
  mezzanotte con `DateTimeKind.Utc` (come `ExpenseRepository.PerIlDatabase`), anche partendo da un
  `DateTimeKind.Unspecified`; senza `Rate` `EndsOn`, `Installments` e `PlanTotal` sono null; con `Rate = 3`
  e inizio 2026-01-31 `EndsOn` è `CalcoliRicorrenti.FinePiano(inizio, 3)` in UTC e `Installments`/`PlanTotal`
  sono quelli dati; `Kind` è quello di `dati` (prova `TipoMovimento.Entrata`); `MaterializedThrough` è
  null; due chiamate danno due `Id` diversi e non vuoti; `SpaceId`, `PaidBy`, `Amount`, `EveryMonths`,
  `DayOfMonth` sono quelli passati.
- `Esito`: con una riga toccata → `Salvata` con quella riga, e il delegate di rilettura NON è chiamato
  (contalo); zero righe e rilettura null → `Sparita` con `Aggiornata` null; zero righe e rilettura con
  `Version` diversa → `Conflitto` con la riga riletta; zero righe e stessa `Version` → `Rifiutata` con la
  riga riletta.
- Colonne (le scritture che il modello permette, come fa `Eton.Tests/PrivilegiInsertTests.cs` con la
  reflection su `ColumnAttribute`): su `RecurringExpense` `space_id`, `paid_by` e `kind` hanno
  `IgnoreOnUpdate`; `version`, `created_at`, `updated_at` hanno `IgnoreOnInsert` e `IgnoreOnUpdate`.

TEST NUOVO: Eton.Tests/RecurringExpenseRepositoryTests.cs — scrivilo PRIMA delle estrazioni, lancia
`dotnet test Eton.Tests/Eton.Tests.csproj` sul codice di prima, e in `VERIFICA` ricopia una riga
`test rosso  RecurringExpenseRepositoryTests: <prima riga d'errore del compilatore>`.

E per il punto 1 aggiungi in `Eton.Tests/ExpenseRepositoryTests.cs`, in fondo, i casi di `TipiAmmessi`
(«Casa» → solo spesa; «Stipendio» → solo entrata; «Altro» → entrambi, nell'ordine spesa, entrata;
«Inventata» → nessuno; il confronto è esatto: «casa» → nessuno).

TEST NUOVO: ExpenseRepositoryTests.TipiAmmessi — scrivili PRIMA dell'helper, lancia i test sul codice di
prima, e in `VERIFICA` ricopia `test rosso  ExpenseRepositoryTests.TipiAmmessi: <prima riga d'errore>`.
(Puoi scrivere i due file di test insieme e fare un solo lancio rosso: ricopia le due righe dallo stesso
output.)

## Punto 3 — la chiave «senza accenti e maiuscole» in un posto solo

- In `Services/SchemaCampi.cs`, subito sotto `RiduciAccenti` (`:177`), aggiungi
  `internal static string ChiaveConfronto(string testo) => RiduciAccenti(testo).ToUpperInvariant();` con
  un commento di una riga: la chiave con cui ordinamenti e ricerca confrontano i testi, sempre con
  `StringComparer.Ordinal`/`StringComparison.Ordinal`.
- Sostituisci le copie, senza altro cambio:
  - `Services/CalcoliGriglia.cs:33-34` → `SchemaCampi.ChiaveConfronto(a.ToString()!)` e idem per `b`;
  - `Services/CalcoliRicorrenti.cs:269` → `SchemaCampi.ChiaveConfronto(r.Description)`;
  - `Services/TabellaSpese.cs:242` → `Normalizza(string testo) => SchemaCampi.ChiaveConfronto(testo.Trim());`
    (il `Trim` resta qui, fuori dalla chiave: è della ricerca, non dell'ordinamento).
- NON toccare `Services/SchemaCampi.cs:142-143` (`ChiaveDa`: minuscole per le chiavi di campo, è un'altra
  cosa).
- I comportamenti sono già coperti: `Eton.Tests/CalcoliGrigliaTests.cs:20`,
  `Eton.Tests/GruppiRicorrentiTests.cs:49-51`, `Eton.Tests/TabellaSpeseTests.cs:168-172`. Non toccarli:
  devono passare invariati. Aggiungi in `Eton.Tests/SchemaCampiTests.cs` un `[Fact]` su `ChiaveConfronto`
  («Èrba» e «erba» danno la stessa chiave; «Città» → «CITTA»; gli spazi ai bordi restano).

TEST DI CARATTERIZZAZIONE: SchemaCampiTests.ChiaveConfronto (nessuna riga rossa: fissa il comportamento
delle tre copie).

## Punto 4 — i claims JWT degli script RLS in un posto solo per script

File: i nove `supabase/verifica-rls*.sql`. Si lanciano così (testa di ciascuno, es.
`supabase/verifica-rls-budget.sql:7-9`): `docker exec -i supabase_db_Eton psql ... -f - < script`, cioè da
standard input dentro il container: un `\i` verso un file comune non funzionerebbe (il file non è nel
container). Le variabili psql invece sì, e gli script le usano già (`:'condiviso'` dopo `\gset`).

- In ogni script, subito dopo le righe `\set ON_ERROR_STOP off` / `\pset pager off` (o in testa alle
  direttive psql, se mancano), un blocco con un commento di una riga e una `\set` per ogni utente
  impersonato in quello script:
  `\set claims_<nome> '{"sub":"<uuid>","role":"authenticated"}'`
  dove `<nome>` è il nome dell'utente in minuscolo come appare nell'email o nel nome dello stesso script
  (alice, bruno, carla…); se lo script non ne dà uno, le prime otto cifre dell'uuid
  (`claims_11111111`).
- Ogni riga `set local request.jwt.claims = '{"sub":"<uuid>","role":"authenticated"}';` diventa
  `set local request.jwt.claims = :'claims_<nome>';`. Nessun'altra riga cambia: né `set local role`, né
  i commenti, né i conteggi «DEVE PRODURRE ESATTAMENTE N ERRORI».
- Gli script restano senza accenti (lo dicono le loro teste).
- Alla fine, nessuna riga `request.jwt.claims = '{` deve restare: verificalo con una ricerca e ricopia il
  conteggio (0) in `VERIFICA`. Non lanciare gli script: li prova la sessione sul database locale.

## NON TOCCARE

`supabase/migrations/**`, `supabase/config.toml`, `Pages/**`, `Shared/**`, `docs/**`, `handoff/**`,
`Models/**`, `Services/CategorieSpesa.cs`, i file del budget (`Models/CategoryBudget.cs`,
`Services/BudgetRepository.cs`, `Services/CalcoliBudget.cs`, `Pages/Budget*.razor`,
`Eton.Tests/CalcoliBudgetTests.cs`), in `Services/ExpenseRepository.cs` tutto tranne
`CambiaCategoriaAsync` e il suo helper, tutti gli altri file.

## Budget di complessità

Nessun tipo nuovo, nessuna interfaccia, nessuna astrazione oltre ai quattro helper nominati
(`TipiAmmessi`, `NuovaRegola`, `Esito` reso statico, `ChiaveConfronto`). Un file nuovo soltanto:
`Eton.Tests/RecurringExpenseRepositoryTests.cs`.

## Verifica

`dotnet build` e `dotnet test Eton.Tests/Eton.Tests.csproj` nel worktree (il GATE lo rilancia la
sessione con synapse). Non avviare Synapse.Server. Non avviare l'app. Non avviare Docker né Supabase.

## Ritorno

Formato: `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <motivo>`.

I file si creano e si modificano solo con le tue patch. Vietati gli interpreti con codice inline (`python -`, `python -c`, `node -e`, here-string passate a un interprete) e vietato scrivere file da PowerShell: niente comandi `powershell.exe -Command` né script `.ps1` usati per creare o riscrivere file, con `Set-Content`, `Out-File`, `Add-Content`, `[System.IO.File]::Write*` o sostituzioni a regex; leggere, compilare e lanciare i test da PowerShell resta permesso. Se una modifica non si può fare con una patch, fermati e scrivi `BLOCKED:` col motivo; se l'hai fatta in un altro modo, scrivi in `DEVIAZIONI` il comando esatto e i file che ha toccato.

Il tuo resoconto è il tuo messaggio finale: Codex lo salva da solo in `G:\Sviluppo\wt-Eton\03-residui\handoff\03-residui\codex-1.md` quando finisci (è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`). Non scrivere tu quel file, non creare altri file di resoconto e non scrivere in `handoff/03-residui/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
