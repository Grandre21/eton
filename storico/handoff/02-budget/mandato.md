UNITÀ: 2/3 — Il budget mensile per categoria nel client

OBIETTIVO: nello spazio corrente si crea un budget su una categoria di spesa («Svago, 50 € al mese»), lo si
vede in una pagina dei budget con speso del mese, residuo («restano X» / «sforato di X») e una barra; da ogni
riga «Registra una spesa» apre il Registro con quella categoria già scelta, e dopo il salvataggio il residuo è
aggiornato. Sotto il riepilogo del mese del Registro compare «Da spendere nei budget: X» (somma dei residui
positivi del mese; con zero budget la riga non c'è). Comportamento e casi limite: spec
`docs/superpowers/specs/2026-10-24-budget-design.md` §2 e §3, che è il riferimento; resa minima con classi e
componenti esistenti.

PERIMETRO: nuovi `Models/Budget.cs` (o il nome che il progetto userebbe), `Services/CalcoliBudget.cs`,
`Services/BudgetRepository.cs`, le pagine dei budget in `Pages/`, i test nuovi in `Eton.Tests/`; esistenti
`Pages/Spese.razor` (riga sotto il riepilogo, lettura della categoria dall'indirizzo), `Shared/NavigazioneSpese.razor`
(collegamento), la registrazione del repository nei servizi (`Program.cs` o dove stanno le altre),
`handoff/02-budget/` | NON TOCCARE: `supabase/` (la migrazione è applicata, non si cambia),
`Services/ExpenseRepository.cs` e `Services/CalcoliRicorrenti.cs` (si leggono e si chiamano, non si modificano:
li tocca la 3), `Pages/SpeseTabella.razor`, `Services/TabellaSpese.cs`, `Services/CalcoliGriglia.cs`,
`Services/SchemaCampi.cs`, `docs/`, `handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI:
  schema (dalla unità 1, applicato in produzione — i nomi esatti nel resoconto della 1, sezione `SQL PER
  L'UTENTE`): tabella `category_budgets` con `id`, `space_id`, `category`, `amount` numeric(12,2) > 0,
  `starts_on` (primo del mese), `ends_on` null o primo del mese, `version`; al più un budget con `ends_on`
  null per (`space_id`, `category`); `category` e `starts_on` non aggiornabili; autore `created_by` (default
  `auth.uid()`, solo INSERT). Modifica e chiusura le può fare l'autore o il proprietario dello spazio, come per
  le ricorrenti: la pagina non offre i comandi a chi il database li rifiuterebbe, se le ricorrenti fanno così.
  percorso di lettura delle spese, unico, da non duplicare (dallo scout, 24 ott):
    `Services/ExpenseRepository.cs:119` `public async Task<SpeseDelPeriodo> ElencaConPrevisteAsync(Guid spazioId, DateTime da, DateTime a)`
    `Services/CalcoliSpese.cs:66` `public static RiepilogoMese PerMese(IEnumerable<Expense> spese, int anno, int mese)`
  Il residuo si calcola sulle stesse righe che danno il totale del mese di `PerMese`: un budget e il riepilogo
  non possono mai contare una spesa in modo diverso. Le entrate non toccano i budget.
  categorie: `Services/CategorieSpesa.cs:19` `public static readonly IReadOnlyList<string> Elenco =`
  concorrenza: come `Services/RecurringExpenseRepository.cs:81`
    `public async Task<RisultatoSalvataggio<RecurringExpense>> SalvaAsync(Guid regolaId, int versioneLetta, DatiRegola dati)`
  indirizzo di «Registra una spesa»: il Registro (`/expenses`) con la categoria nell'indirizzo; nome del
  parametro a scelta dell'unità, una categoria sconosciuta o di entrata si ignora.
  Cambiare l'importo vale per tutti i mesi (nessuno storico, spec §2); chiudere imposta `ends_on` al mese
  corrente (è l'ultimo mese in cui vale, schema della 1); se le ricorrenti hanno «Elimina», il budget lo ha
  uguale. Nessuna sovrapposizione (rilievo BH1 della 1, il database non la impedisce): un budget nuovo su una
  categoria il cui ultimo budget vale ancora nel mese corrente parte dal mese dopo; la pagina lo dice.
  `version` non si scrive mai dal client: è solo filtro, la incrementa il trigger (schema della 1).

STATO: unità 1 (migrazione) — resoconto in `handoff/01-migrazione/resoconto.md`, applicata dall'utente
(GATE in `handoff/PIANO.md`). Scelte del capo da ratificare che ti riguardano: in `handoff/PIANO.md`, RAZIONALE.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.
⚠️ Lezioni della 2.3: subito dopo la clausola verbatim «il file `-o`» aggiungi: «(è il tuo messaggio
finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`)». Nel brief vieta a Codex
`Set-Content`, `Out-File` e `[System.IO.File]::Write*`: le modifiche ai file solo con apply_patch; cercale nel log.

RESA: minima, con classi e componenti già presenti; la grafica curata è della 2.1-bis. Mobile-first, bersagli
da 48 px come il resto dell'app.

PROVA NEL BROWSER: sì (§7). Dati in produzione **ammessi solo così**: un budget di prova su una categoria che
nello spazio non ha già un budget, e una spesa di prova con descrizione «PROVA AGENTE» registrata dal budget;
alla fine **entrambi eliminati** (o il budget chiuso, se l'eliminazione non esiste) e l'eliminazione verificata
ricaricando. Server avviato e fermato da te dal tuo worktree, porta e PID in `handoff/02-budget/server.md`;
riavvialo prima di ogni prova (memoria: il server di sviluppo si congela sui manifest). Browser con
`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`. Le conferme native di eliminazione bloccano il plugin: se
compaiono, la pulizia la fai fare a me (domanda al capo).

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed.
Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 10 M tok_eq

RESOCONTO IN: `handoff/02-budget/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
