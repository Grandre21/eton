# Brief codex-2 — correzioni dopo la revisione (stesso worktree, stesso diff)

Correggi solo questi punti, nei soli file nominati. Nient'altro cambia.

1. `Pages/Budget.razor` — lo stato vuoto lampeggia cambiando mese (bug-hunter BH1).
   Oggi `Leggi` azzera `budget = []` prima dell'await e il ramo `else if (budget.Count == 0)` (:53) mostra
   «Ancora nessun budget qui» durante ogni rilettura. Correzione: campo `private bool senzaBudget;`
   assegnato **solo dopo** `ElencaAsync` riuscita (`senzaBudget = budget.Count == 0;`), e il ramo :53 usa
   `senzaBudget`. Dentro la scheda del mese, mentre `rileggoMese` è vero, al posto della lista mostra
   `<Scheletro Righe="2" />` come fa `Pages/Spese.razor:215-222` (testa e frecce restano a schermo).

2. `Pages/Budget.razor` — un budget che parte il mese dopo non si raggiunge (bug-hunter BH2).
   Sotto la `section` del mese, se esistono budget con `StartsOn.Date > CalcoliBudget.PrimoDelMese(DateTime.Today)`,
   una seconda `<section class="scheda">` con `<div class="testa-registro"><span>In arrivo</span></div>` e
   `<ul class="registro">`: per ciascuno `<li><a class="riga riga-spesa" href="expenses/budgets/{id}">`
   con `<span class="descrizione">Categoria</span>`, `<span class="meta">da <mese> <span class="dato">anno</span></span>`,
   `<span class="importo dato">importo&#160;€</span>` (forma delle righe di `Pages/Ricorrenti.razor:87-108`).
   Si mostra in qualunque mese si stia guardando. Nessun metodo nuovo in `CalcoliBudget`: è un `Where` nel
   markup o una proprietà calcolata della pagina.

3. `Pages/BudgetEdit.razor` — «Sovrascrivi» dopo un conflitto nato da «Chiudi» salva l'importo e non chiude
   (bug-hunter BH3). Campo `private bool conflittoDaChiusura;`: vero solo nel `case Conflitto` di `Chiudi`,
   falso in `Salva`, `Ricarica`, `Sovrascrivi` e `OnParametersSetAsync`. `SovrascriviAbilitato` di
   `SchedaConflitto` diventa `Valido && PuoIntervenire && !conflittoDaChiusura`; con il flag vero la premessa è
   «Mentre lo chiudevi, questo budget è cambiato: ricarica la versione più recente e poi chiudilo di nuovo.»
   Commento di una riga sul perché (Sovrascrivi riscrive l'importo, non ripete la chiusura).

4. `Pages/BudgetEdit.razor:176-179` — nessuno spazio attivo in creazione (backend-expert BE1). Togli il
   `throw new InvalidOperationException("Nessuno spazio attivo.")`: con `attivo is null` poni
   `aperturaFallita = true; errore = "Serve uno spazio attivo per creare un budget.";` senza passare dal catch,
   che resta per rete e RLS. `caricato = true` in coda vale per entrambi i rami.

5. `Pages/BudgetEdit.razor:44-47` — sopra la frase sui permessi un commento Razor come
   `Pages/RicorrenteEdit.razor:59`: stessa regola di `Services/Permessi.cs` (la RLS di category_budgets è
   autore o proprietario), senza un `Oggetto` dedicato lì; quando se ne aggiunge uno, la frase va spostata in
   `Permessi.Spiegazione` (conformity C2).

6. `Pages/Spese.razor:311-327` — (conformity C3, backend-expert BE4) riporta `private bool aRate;` e i campi
   del modulo che seguono a ridosso di `nuovoImporto`, senza il metodo in mezzo; raggruppa `Categoria`,
   `ultimaCategoria`, `daBudget`, `daSpendere` in un blocco di campi a sé con un commento di una riga
   («arrivo dal budget: /expenses?categoria=X»); sposta `OnParametersSet` subito dopo `OnInitializedAsync`
   (:384). Corpo identico.

NON TOCCARE: ogni altro file, `Shared/PaginaRegistro.cs` compreso. Nessun CSS. Nessun tipo nuovo.

Verifica: `dotnet build` e `dotnet test Eton.Tests/Eton.Tests.csproj` nel worktree. Non avviare Synapse.Server né l'app.

Ritorno: `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <motivo>`.

I file si creano e si modificano solo con le tue patch. Vietati gli interpreti con codice inline (`python -`, `python -c`, `node -e`, here-string passate a un interprete) e vietato scrivere file da PowerShell: niente comandi `powershell.exe -Command` né script `.ps1` usati per creare o riscrivere file, con `Set-Content`, `Out-File`, `Add-Content`, `[System.IO.File]::Write*` o sostituzioni a regex; leggere, compilare e lanciare i test da PowerShell resta permesso. Se una modifica non si può fare con una patch, fermati e scrivi `BLOCKED:` col motivo; se l'hai fatta in un altro modo, scrivi in `DEVIAZIONI` il comando esatto e i file che ha toccato.

Il tuo resoconto è il tuo messaggio finale: Codex lo salva da solo in `G:\Sviluppo\wt-Eton\02-budget\handoff\02-budget\codex-2.md` quando finisci (è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`). Non scrivere tu quel file, non creare altri file di resoconto e non scrivere in `handoff/02-budget/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
