BRIEF implementer 2 — due correzioni di leggibilità in Pages/BudgetEdit.razor (rilievi backend-expert BE1, BE3)

PROGETTO: nessun CLAUDE.md di progetto; convenzioni = file omologhi. Radice G:\Sviluppo\Eton.
FILE DA TOCCARE: solo Pages/BudgetEdit.razor. NON TOCCARE nient'altro.

1) (BE1) Inizio e ultimo mese di un budget nuovo oggi si calcolano due volte: nel markup (righe ~67-68,
   `var inizio = CalcoliBudget.InizioDiUnNuovo(...)` e il ternario su `mesi` dentro `TestoPeriodo`) e in `Salva`
   (righe ~267-268). Sostituiscile con due proprietà nel @code, accanto a `Valido`:
       private DateTime Inizio => CalcoliBudget.InizioDiUnNuovo(budgetDelloSpazio, categoria, DateTime.Today);
       private DateTime? UltimoMese => mesi > 0 ? CalcoliBudget.FineDaMesi(Inizio, mesi) : null;
   Nel markup: `Vale @CalcoliBudget.TestoPeriodo(Inizio, UltimoMese). …` e `@if (Inizio > CalcoliBudget.PrimoDelMese(DateTime.Today))`
   (togli la locale `var inizio`). In `Salva`: `await Repository.CreaAsync(attivo.Id, autore, categoria, importo, descrizione, Inizio, UltimoMese);`
   (togli le locali `inizio` e `ultimo`). Un commento di una riga sulle proprietà: ciò che si mostra è ciò che si salva.
2) (BE3) Nel `@for` delle opzioni della Durata togli la copia `var valore = n;` e usa `n` direttamente, come
   Pages/RicorrenteEdit.razor:182-185.

BUDGET DI COMPLESSITÀ: nient'altro; nessun cambiamento di comportamento.
verifica: `mcp__synapse__diagnostics` su Pages/BudgetEdit.razor, poi `mcp__synapse__build` e `mcp__synapse__test`
(root=G:\Sviluppo\Eton); `dotnet build|test` in shell lo nega `hooks/build-via-synapse.ps1`, e solo se il tool synapse
fallisce si rilancia con `SYNAPSE_DOWN=1` davanti, detto nel resoconto.
Formato di ritorno: FATTO / DEVIAZIONI / VERIFICA / NOTE, oppure BLOCKED: <domanda>.
