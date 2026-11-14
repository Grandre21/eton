# Brief codex-1 — budget mensile per categoria (Eton, Blazor WASM, net10.0)

Lavori nel worktree `G:\Sviluppo\wt-Eton\02-budget`. Leggi tu i file citati con `file:line` prima di
scrivere: sono il modello da rispecchiare. Commenti e testi in italiano, nello stile dei file vicini
(commenti che dicono il perché, non il cosa; densità come `Services/RecurringExpenseRepository.cs`).

## Cosa esiste già (schema in produzione, NON si tocca `supabase/`)

Tabella `public.category_budgets` (`supabase/migrations/20261024000000_budget.sql`):
`id uuid` (lo genera il client), `space_id uuid`, `created_by uuid` (default auth.uid(), solo INSERT),
`category text`, `amount numeric(12,2) > 0`, `starts_on date` (primo del mese), `ends_on date null`
(primo del mese, è l'ULTIMO mese in cui vale; `>= starts_on`), `version int`, `created_at`, `updated_at`.
GRANT: insert su `(id, space_id, created_by, category, amount, starts_on, ends_on)`; update SOLO su
`(amount, ends_on)`; select e delete. Indice unico: al più un budget con `ends_on is null` per
(space_id, category). Il trigger alza `version` e rimette category/starts_on/created_by/space_id.
RLS: SELECT ai membri; INSERT a un membro con created_by = auth.uid(); UPDATE/DELETE all'autore o al
proprietario dello spazio.

## File da creare

1. `Models/CategoryBudget.cs` — `[Table("category_budgets")] public class CategoryBudget : BaseModel`,
   forma identica a `Models/RecurringExpense.cs:11-47`: `[PrimaryKey("id", true)] Guid Id`;
   `SpaceId`, `CreatedBy` ("created_by"), `Category`, `StartsOn` con `ignoreOnUpdate: true`;
   `Amount` decimal; `EndsOn` DateTime?; `Version`, `CreatedAt`, `UpdatedAt` con
   `ignoreOnInsert: true, ignoreOnUpdate: true`. DateTime e non DateOnly (v. il commento in
   `Models/Expense.cs` su SpentOn).

2. `Services/CalcoliBudget.cs` — `public static class CalcoliBudget`, calcoli puri come
   `Services/CalcoliSpese.cs:30`. Contenuto esatto:
   - `public sealed record StatoBudget(CategoryBudget Budget, decimal Speso)` con
     `public decimal Residuo => Budget.Amount - Speso;` e `public int Quota` = percentuale intera di
     Speso su Amount, arrotondata AwayFromZero, limitata a 0..100 (la larghezza della barra).
   - `public static DateTime PrimoDelMese(DateTime data)`.
   - `public static bool ValeNelMese(CategoryBudget b, int anno, int mese)`: vero se
     `b.StartsOn.Date <= primo` e (`b.EndsOn is null` o `b.EndsOn.Value.Date >= primo`), con
     `primo = new DateTime(anno, mese, 1)`.
   - `public static IReadOnlyList<StatoBudget> DelMese(IEnumerable<CategoryBudget> budget, RiepilogoMese riepilogo, int anno, int mese)`:
     solo i budget che `ValeNelMese`; **lo speso si prende da `riepilogo.Categorie`** (il `Totale`
     della `VoceCategoria` con la stessa categoria, confronto `StringComparer.Ordinal`, 0 se manca).
     È il contratto: il residuo si calcola sulle stesse righe del totale del mese di
     `CalcoliSpese.PerMese` (`Services/CalcoliSpese.cs:66`), che esclude già le entrate — scrivilo nel
     commento. Ordine: quello di `CategorieSpesa.Elenco` (`Services/CategorieSpesa.cs:19`), poi le
     categorie fuori elenco in ordine ordinale.
   - `public static decimal DaSpendere(IEnumerable<StatoBudget> stati)`: somma dei soli residui > 0.
   - `public static IReadOnlyList<string> CategorieDisponibili(IEnumerable<CategoryBudget> budget)`:
     `CategorieSpesa.Elenco` meno le categorie che hanno un budget con `EndsOn is null`.
   - `public static DateTime InizioDiUnNuovo(IEnumerable<CategoryBudget> budget, string categoria, DateTime oggi)`:
     il massimo fra `PrimoDelMese(oggi)` e, per ogni budget chiuso di quella categoria,
     `EndsOn + 1 mese`. Così un budget nuovo su una categoria il cui ultimo budget vale ancora nel mese
     corrente parte dal mese dopo (nessuna sovrapposizione: il database non la impedisce).
   - `public static bool SiPuoChiudere(CategoryBudget b, DateTime oggi)`: `b.EndsOn is null &&
     b.StartsOn.Date <= PrimoDelMese(oggi)` (chiudere scrive ends_on = mese corrente, e il check
     `ends_on >= starts_on` rifiuterebbe un budget che parte il mese dopo: quello si elimina soltanto).

3. `Services/BudgetRepository.cs` — `public class BudgetRepository`, copia della forma di
   `Services/RecurringExpenseRepository.cs:18-159` (costruttore con `SupabaseService`, client
   ripreso a ogni metodo, nessun controllo d'autorizzazione, `Esito` privato come `:124-137`):
   - `Task<IReadOnlyList<CategoryBudget>> ElencaAsync(Guid spazioId)` (ordine `created_at` asc);
   - `Task<CategoryBudget?> LeggiAsync(Guid budgetId)`;
   - `Task<CategoryBudget> CreaAsync(Guid spazioId, Guid autore, string categoria, decimal importo, DateTime inizio)`:
     `Id = Guid.NewGuid()`, `StartsOn = ExpenseRepository.PerIlDatabase(CalcoliBudget.PrimoDelMese(inizio))`,
     `EndsOn = null`, come `CreaAsync` `:49-72`;
   - `Task<RisultatoSalvataggio<CategoryBudget>> SalvaImportoAsync(Guid budgetId, int versioneLetta, decimal importo)`:
     `Where(id && version == versioneLetta).Set(Amount).Update()`, come `SalvaAsync` `:81-102`;
   - `Task<RisultatoSalvataggio<CategoryBudget>> ChiudiAsync(Guid budgetId, int versioneLetta, DateTime ultimoMese)`:
     `.Set(b => b.EndsOn!, PerIlDatabase(PrimoDelMese(ultimoMese)))`, come `TerminaAsync` `:107-116`;
   - `Task<bool> EliminaAsync(Guid budgetId)`, come `:148-159` (qui nessuna FK lo impedisce).
   `version` non si scrive MAI: è solo filtro.

4. `Pages/Budget.razor` — `@page "/expenses/budgets"`, `@inherits PaginaRegistro`, sul modello di
   `Pages/Ricorrenti.razor` (struttura dei rami caricato/errore/senza spazio/vuoto `:24-69`) e della
   scheda del mese di `Pages/Spese.razor:180-213` (frecce ‹ ›, `rileggoMese`, `ScartaCambioSpazio`,
   `PuoAndareAvanti`, `MesePrecedente`/`MeseSuccessivo` `:577-605`: stesse regole e stessi commenti
   abbreviati con rimando a Spese.razor). Inietta `BudgetRepository` ed `ExpenseRepository`.
   - `Leggi(Space attivo)`: in un solo try (fallimento → `SegnalaNonLetti`): budget =
     `ElencaAsync`; `periodo = await Spese.ElencaConPrevisteAsync(attivo.Id, primo del mese, ultimo del mese)`
     (unico percorso di lettura, `Services/ExpenseRepository.cs:119`: non leggere `expenses` in altro modo);
     `riepilogo = CalcoliSpese.PerMese(periodo.Righe, anno, mese)`; `stati = CalcoliBudget.DelMese(...)`.
   - Testata «Budget» con azione `<a class="btn primario compatto" href="expenses/budgets/new">Nuovo budget</a>`
     (solo con uno spazio attivo); `<Aiuto>` con 3 frasi: il budget vale ogni mese e riparte pieno
     (il residuo non si riporta); ogni spesa della categoria lo consuma, da qualunque pagina la segni;
     le entrate non lo toccano.
   - `<NavigazioneSpese Attiva="expenses/budgets" />`, sottotitolo «In <strong>spazio</strong>».
   - Una `<p class="spiega">` fissa: «Le spese ricorrenti della categoria contano nel budget: un affitto
     in «Casa» lo consuma dal primo giorno.»
   - Nella `section class="scheda riepilogo-mese"` (testa col mese e le frecce): se nessun budget vale
     nel mese, `<p class="testo-tenue">` «Nessun budget in questo mese.»; altrimenti
     `<ul class="barre-categorie">`, un `<li>` per `StatoBudget`, figli in quest'ordine:
     `<span class="nome-categoria">Categoria</span>`,
     `<span class="totale-categoria dato">speso / importo €</span>` (`Denaro.Testo`, `&#160;€`),
     `<span class="barra"><span class="riempimento" style="width: @(s.Quota)%"></span></span>`,
     `<span class="quota dato">` «restano X €» se Residuo >= 0, altrimenti «sforato di X €» (X = valore assoluto),
     poi `<a class="btn compatto" href="@($"expenses?categoria={Uri.EscapeDataString(categoria)}")">Registra una spesa</a>`
     e `<a class="btn compatto" href="expenses/budgets/{id}">Modifica</a>`.
     Nessun CSS nuovo, nessuno `style` oltre alla larghezza della barra.
   - Stato vuoto (zero budget nello spazio): come `Pages/Ricorrenti.razor:61-69`, frase «Ancora nessun
     budget qui.», spiega «Decidi quanto spendere in un mese per una categoria, e qui vedi quanto ti
     resta.», pulsante «Nuovo budget».

5. `Pages/BudgetEdit.razor` — `@page "/expenses/budgets/new"` e `@page "/expenses/budgets/{Id:guid}"`,
   `@inherits PaginaEditor`, sul modello stretto di `Pages/RicorrenteEdit.razor` (generazione e
   caricamento `:403-467`, `Salva`/`SalvaCon`/`Ricarica`/`Sovrascrivi` `:469-619`, `Termina` → `Chiudi`
   `:621-659`, `Elimina` `:661-685`, `NavigationLock` `:48`, `SchedaConflitto` `:55`, rami
   sparita/errore `:32-45`, `ConfermaAzione` `:277` e `:300`). Campi: solo categoria e importo.
   - `PuoIntervenire => Nuova || (budget is not null && Permessi.PuoIntervenire(mioId, budget.CreatedBy, budget.SpaceId, Spazi.Elenco))`
     (`Services/Permessi.cs:63`). Chi non può: importo in sola lettura (`campo-lettura` come
     `RicorrenteEdit.razor:133-136`), nessun Salva/Chiudi/Elimina, e la frase
     «Questo budget l'ha creato qualcun altro: può cambiarlo, chiuderlo o eliminarlo solo chi l'ha creato, o chi possiede lo spazio.»
   - **Nuovo**: legge i budget dello spazio (`ElencaAsync`) in `OnParametersSetAsync`; pastiglie di
     `CalcoliBudget.CategorieDisponibili` (markup `RicorrenteEdit.razor:214-225`, senza la pastiglia
     extra); se è vuoto, `<p class="spiega">` «Ogni categoria ha già un budget.» e Salva disabilitato.
     Sotto: `<p class="spiega">` «Vale da <mese anno>, ogni mese.» con
     `CalcoliBudget.InizioDiUnNuovo(budget, categoria, DateTime.Today)` e `CalcoliSpese.NomeMese`; se
     quella data è dopo il mese corrente aggiunge «Il budget precedente di <categoria> vale ancora
     questo mese.» Crea con `Repository.CreaAsync(attivo.Id, autore, categoria, importo, inizio)`
     (autore da `AuthState.GetUserIdAsync()` come `RicorrenteEdit.razor:507-511`), poi
     `Esci("expenses/budgets", replace: true)`.
   - **Esistente**: categoria in `campo-lettura`; «Vale da <mese anno>» e, se chiuso, «Ultimo mese:
     <mese anno>»; importo modificabile, con `<p class="spiega">` «Cambiare l'importo vale anche per i
     mesi passati: lo storico degli importi non si tiene.» Salva → `SalvaImportoAsync` con i quattro
     esiti come `SalvaCon`. Sezione `scheda` (solo se PuoIntervenire): se `CalcoliBudget.SiPuoChiudere(budget, DateTime.Today)`
     testo «Chiudendolo vale fino alla fine di questo mese, poi non più.» e
     `ConfermaAzione Etichetta="Chiudi" EtichettaConferma="Sì, chiudi"` → `ChiudiAsync(Id, version, DateTime.Today)`;
     poi sempre «Eliminandolo sparisce anche dai mesi passati.» e `ConfermaAzione` (Elimina) →
     `EliminaAsync`, poi `Esci("expenses/budgets")`.
   - `Cambiata`: Nuova → importo non vuoto o categoria diversa dalla prima disponibile; esistente →
     `importoTesto != Denaro.TestoDigitabile(budget.Amount)`. Importo con `Denaro.Prova`,
     `Denaro.Verifica` e `Testi.MessaggioImporto` come `RicorrenteEdit.razor:127-141`.
   - Messaggi d'errore nella forma di `RicorrenteEdit.razor` (testo dell'eccezione solo in console,
     prefisso `[Budget]`). Una INSERT che fallisce per l'indice unico (un altro membro ha appena creato
     lo stesso budget) cade nel catch generico: va bene.
   - Pulsante «Chiudi» della barra azioni come `:260` ma verso `expenses/budgets`: chiamalo «Torna ai
     budget» per non confonderlo con la chiusura del budget.

6. `Eton.Tests/CalcoliBudgetTests.cs` — xUnit, helper come `Eton.Tests/CalcoliSpeseTests.cs:8-19`.
   Casi, un `[Fact]` ciascuno, nomi in italiano con underscore:
   lo speso è il totale della categoria nel riepilogo; un'entrata in «Altro» non consuma il budget
   «Altro» (la categoria è in entrambi gli elenchi); una spesa di un altro mese non conta; un budget che
   parte dopo il mese o chiuso prima non compare; il mese di `ends_on` compare; residuo negativo quando
   sforato e Quota limitata a 100; `DaSpendere` somma solo i residui positivi e vale 0 senza budget;
   `CategorieDisponibili` esclude le categorie con un budget aperto e include quelle chiuse;
   `InizioDiUnNuovo` = mese corrente senza budget precedenti, mese dopo se il precedente è chiuso col
   mese corrente, mese corrente se è chiuso in un mese passato; `SiPuoChiudere` falso per un budget
   che parte il mese dopo e per uno già chiuso.

TEST NUOVO: Eton.Tests/CalcoliBudgetTests.cs — scrivilo PRIMA di `Services/CalcoliBudget.cs` e
`Models/CategoryBudget.cs`, lancia `dotnet test Eton.Tests/Eton.Tests.csproj` sul codice di prima, e in
`VERIFICA` ricopia una riga `test rosso  CalcoliBudgetTests: <prima riga d'errore del compilatore>`.

## File esistenti da modificare

7. `Pages/Spese.razor`:
   - `@inject BudgetRepository Budget` e `@inject NavigationManager Navigation`.
   - `[SupplyParameterFromQuery(Name = "categoria")] public string? Categoria { get; set; }` (senza
     `[Parameter]`). In `OnParametersSet` (override nuovo): se `Categoria` è diverso dall'ultimo valore
     visto (campo privato), lo memorizzi e poni `daBudget = CategorieSpesa.Conosciuta(Categoria, TipoMovimento.Spesa)`;
     se vero: `nuovoTipo = TipoMovimento.Spesa`, `aRate = false`, `nuovaCategoria = Categoria!`,
     `nuovaData = DateTime.Today`. Una categoria sconosciuta o solo di entrata si ignora (daBudget falso,
     modulo invariato). Commento: il confronto con l'ultimo valore serve perché la pagina si riusa
     navigando da `/expenses?categoria=X` a `/expenses`.
   - In `Segna()`, ramo non a rate (`:428-432`), dopo `CreaAsync` riuscita: se `daBudget`,
     `Navigation.NavigateTo("expenses/budgets"); return;` (si torna al budget, che rilegge il residuo).
     Il `finally` resta com'è.
   - In `Leggi` (`:495-548`): campo `decimal? daSpendere`, azzerato a null in testa. Dopo il try delle
     spese, un try SEPARATO come quello dei membri (`:540-547`): `var budget = await Budget.ElencaAsync(attivo.Id);`
     `var stati = CalcoliBudget.DelMese(budget, riepilogo, anno, mese);`
     `daSpendere = stati.Count == 0 ? null : CalcoliBudget.DaSpendere(stati);` — un fallimento resta in
     console (`[Spese] Budget non disponibili: …`) e non nasconde le spese.
   - Markup: subito dopo la riga «Entrate · Saldo» (`:199`), solo se `daSpendere is { } x`:
     `<p class="testo-tenue">Da spendere nei budget <span class="dato">@Denaro.Testo(x)&#160;€</span> · <a href="expenses/budgets">Vedi i budget</a></p>`.
     Non entra in nessun totale.
   - Nell'`<Aiuto>` una frase in più: «Sotto il saldo, «Da spendere nei budget» somma quanto resta nei
     budget del mese; non entra nel saldo.»
8. `Shared/NavigazioneSpese.razor:26-32`: voce `new("Budget", "expenses/budgets", false)` dopo
   «Ricorrenti»; aggiorna il commento di `Attiva` (`:21`).
9. `Program.cs:21`: `builder.Services.AddSingleton<BudgetRepository>();` vicino alle altre.

## NON TOCCARE

`supabase/**`, `Services/ExpenseRepository.cs`, `Services/CalcoliRicorrenti.cs`, `Services/CalcoliSpese.cs`,
`Pages/SpeseTabella.razor`, `Services/TabellaSpese.cs`, `Services/CalcoliGriglia.cs`,
`Services/SchemaCampi.cs`, `wwwroot/**` (nessun CSS nuovo), `docs/**`, `handoff/**`, tutti gli altri file.

## Budget di complessità

Tipi nuovi: al massimo `CategoryBudget`, `CalcoliBudget`, `StatoBudget`, `BudgetRepository` e le due
pagine. Nessuna interfaccia, nessuna astrazione nuova, nessun componente Razor nuovo; un helper con un
solo call-site va inline. Niente copie dei testi di Permessi in un file nuovo.

## PIANO-DESIGN (resa minima, solo classi e token esistenti di `wwwroot/css/app.css`)

- Colori, dai token: `--sfondo #000000` fondo; `--superficie #121212` la scheda del mese;
  `--secondario #b6f36a` ciò che si constata (barra, speso, «restano»); `--accento #4c8dff` ciò che si
  preme («Registra una spesa», «Nuovo budget»); `--testo-tenue #a1a1a1` le frasi di servizio.
  «sforato di X» resta nella classe `quota` (nessun rosso nuovo: la resa curata è della 2.1-bis).
- Typeface: Inter (`--prosa`) per categorie e frasi; Plex Mono (`--mono`, classe `dato`) per ogni cifra.
- Layout: colonna unica allineata a sinistra, mobile-first. Testata → sotto-navigazione → «In spazio»
  → frase sulle ricorrenti → scheda del mese con la testa «ottobre 2026 ‹ ›» → una voce
  `barre-categorie` per budget (nome | speso/importo; barra | restano X; sotto, i due pulsanti, che la
  griglia mette da sé sulla terza riga). Ogni pulsante è `.btn`, quindi alto 48 px (`--tocco`).
- Principi: la cifra che conta è il residuo; la pagina ricalca il riepilogo del Registro, così il
  budget si legge come la stessa contabilità vista per categoria; verde = constatato, blu = premibile.

## Verifica

`dotnet build` e `dotnet test Eton.Tests/Eton.Tests.csproj` nel worktree (il GATE lo rilancia la
sessione con synapse). Non avviare Synapse.Server. Non avviare l'app.

## Ritorno

Formato: `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <motivo>`.

I file si creano e si modificano solo con le tue patch. Vietati gli interpreti con codice inline (`python -`, `python -c`, `node -e`, here-string passate a un interprete) e vietato scrivere file da PowerShell: niente comandi `powershell.exe -Command` né script `.ps1` usati per creare o riscrivere file, con `Set-Content`, `Out-File`, `Add-Content`, `[System.IO.File]::Write*` o sostituzioni a regex; leggere, compilare e lanciare i test da PowerShell resta permesso. Se una modifica non si può fare con una patch, fermati e scrivi `BLOCKED:` col motivo; se l'hai fatta in un altro modo, scrivi in `DEVIAZIONI` il comando esatto e i file che ha toccato.

Il tuo resoconto è il tuo messaggio finale: Codex lo salva da solo in `G:\Sviluppo\wt-Eton\02-budget\handoff\02-budget\codex-1.md` quando finisci (è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`). Non scrivere tu quel file, non creare altri file di resoconto e non scrivere in `handoff/02-budget/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
