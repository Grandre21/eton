UNITÀ: 2/5 — Ricorrenti divise in gruppi, e quota mensile da accantonare

OBIETTIVO: la pagina Ricorrenti mostra le regole divise nei gruppi Mensili, Annuali, Ogni N mesi e Terminate,
ciascuno col suo totale mensile, ordinate per descrizione dentro il gruppo; in testa una riga «Per annuali e
periodiche, metti da parte X € al mese». Spec: `docs/superpowers/specs/2026-10-20-rate-entrate-design.md` §5.
**Questa unità non tocca lo schema**: i gruppi «Entrate» e «A rate» del §5 non esistono ancora (arrivano con
le unità 4 e 5, dopo la migrazione). Il raggruppamento va costruito in modo che quei due gruppi si aggiungano
senza riscriverlo. Il calcolo della quota e il raggruppamento sono puri e hanno i loro test.

PERIMETRO: `Services/CalcoliRicorrenti.cs`, `Pages/Ricorrenti.razor` (e il suo `.razor.css` se esiste o
serve), `Services/RecurringExpenseRepository.cs` (solo per l'ordine della lettura, se serve), test sotto
`Eton.Tests/`, `handoff/02-gruppi-quota/` | NON TOCCARE: `supabase/`, `Models/`, `Services/ExpenseRepository.cs`,
`Pages/Spese.razor`, `Pages/Home.razor`, `Pages/SpeseTabella.razor`, `Pages/RicorrenteEdit.razor`, `docs/`,
`handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI: dalla ricognizione dello scout (20 ott), da non rompere:
`Services/CalcoliRicorrenti.cs:96` `public static string Cadenza(int ogniMesi, int giorno, DateTime inizio)`;
`Services/CalcoliRicorrenti.cs:152` `public static SpeseDelPeriodo Fondi(IReadOnlyList<Expense> vere, IReadOnlyList<RecurringExpense> regole, DateTime da, DateTime a, DateTime oggi)`;
`Services/RecurringExpenseRepository.cs:28` `public async Task<IReadOnlyList<RecurringExpense>> ElencaAsync(Guid spazioId)`.
Le firme nuove (raggruppamento e quota) le decidi tu e le citi in `CONTRATTI` del resoconto: le unità 4 e 5 ci
aggiungeranno i loro gruppi.

STATO: unità 1 (doppioni) — vedi `handoff/PIANO.md` per l'esito. File disgiunti da questa.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.

RESA: minima, sui token e sui componenti esistenti, dichiarata provvisoria: la grafica si rifà nella fase
2.1-bis (decisione del 22 settembre, la stessa della tabella). Nessuno stile nuovo oltre a ciò che serve per
separare i gruppi in modo leggibile.

PROVA NEL BROWSER: `live-testing` e `ui-critic` (§7). Il server lo avvii e lo fermi tu, dal tuo worktree, con
porta e PID in `handoff/02-gruppi-quota/server.md`. L'app di sviluppo punta a Supabase di **produzione**: le
regole di prova si chiamano «PROVA COLLAUDO …», partono il mese prossimo (così non materializzano niente) e si
eliminano a fine prova; il resoconto dice che non ne resta nessuna. Il browser giusto ha
`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed;
`live-testing` verde | BUDGET: 6 M tok_eq

RESOCONTO IN: `handoff/02-gruppi-quota/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
