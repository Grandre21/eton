UNITÀ: 5/5 — Entrate e bilancio del mese

OBIETTIVO: si possono registrare entrate, sia singole dal Registro sia ricorrenti (lo stipendio) dalla pagina
Ricorrenti, con le categorie Stipendio, Rimborso, Regalo, Vendita, Altro. Registro e Home mostrano entrate,
spese e saldo del mese. I grafici e le ripartizioni per categoria restano solo sulle spese. La tabella ha la
colonna e il filtro «Tipo», e il riepilogo dà Spese, Entrate e Saldo. Nella pagina Ricorrenti c'è il gruppo
«Entrate», escluso dai totali delle spese e dalla quota. Il tipo di un movimento non si cambia dopo la
creazione. Spec: `docs/superpowers/specs/2026-10-20-rate-entrate-design.md` §6 e §5.

**Il rischio principale è una somma dimenticata** (spec §8): se un punto che somma importi non distingue il
tipo, un'entrata si conta come spesa senza nessun errore visibile. Il resoconto porta una sezione
`PUNTI DI SOMMA` con **ogni** punto del codice che somma importi di spese (file:line), e per ciascuno come è
stato trattato. La trovi con una ricerca sul codice, non a memoria.

PERIMETRO: `Models/Expense.cs`, `Models/RecurringExpense.cs` (solo `Kind`), `Services/ExpenseRepository.cs`,
`Services/CalcoliSpese.cs`, `Services/CalcoliRicorrenti.cs`, `Services/CategorieSpesa.cs`,
`Services/TabellaSpese.cs`, `Services/RecurringExpenseRepository.cs`, `Pages/Spese.razor`, `Pages/Home.razor`,
`Pages/SpeseTabella.razor`, `Pages/SpesaEdit.razor`, `Pages/Ricorrenti.razor`, `Pages/RicorrenteEdit.razor`,
`Shared/Griglia.razor` (solo il riepilogo, se serve), test sotto `Eton.Tests/`, `handoff/05-entrate/` |
NON TOCCARE: `supabase/`, `docs/`, `handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI: colonna della migrazione dell'unità 3, già applicata: `expenses.kind` e `recurring_expenses.kind`
(`'expense'` | `'income'`, default `'expense'`, solo INSERT). Il modello acquista `Kind` scritto solo in
creazione: è una colonna «ignorata in aggiornamento», come `recurring_id`, e i test dei privilegi di INSERT lo
devono vedere. Le firme di raggruppamento e quota dell'unità 2, con il gruppo «A rate» dell'unità 4: citate nei
loro resoconti, `CONTRATTI` (`handoff/02-gruppi-quota/resoconto.md`, `handoff/04-rate/resoconto.md`). Dalla
ricognizione: `Services/ExpenseRepository.cs:214`
`public async Task<Expense> CreaAsync(Guid spazioId, Guid pagante, decimal importo, string descrizione, string categoria, DateTime data)`
— aggiunge il tipo; `Services/CalcoliRicorrenti.cs:15`
`public sealed record SpeseDelPeriodo(IReadOnlyList<Expense> Righe, IReadOnlySet<Guid> Previste, IReadOnlyList<Expense> InArrivo)`.

STATO: unità 1-4 e passaggio dell'utente sulla migrazione — vedi `handoff/PIANO.md` e i loro resoconti.
L'interruttore «a rate» dell'unità 4 sta nello stesso modulo del Registro: per le entrate va nascosto.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.
⚠️ Lezione dell'unità 01: la clausola verbatim «il file `-o`» fa creare a Codex un file chiamato `-o` nella radice del worktree, con il resoconto dentro (segnalazione aperta, la clausola resta verbatim). Dopo ogni lancio controlla se esiste, sposta il contenuto nel file `codex-<n>.md` e toglilo prima del commit. Se Codex scrive file con uno script PowerShell inline, scrivilo in `SCOSTAMENTI` e verifica encoding e accenti.
L'unità è grande: dividila in più brief in sequenza (per esempio modello e calcoli, poi pagine).

RESA: minima, sui token esistenti, dichiarata provvisoria fino alla 2.1-bis.

PROVA NEL BROWSER: `live-testing` e `ui-critic` (§7). Server avviato e fermato da te, con porta e PID in
`handoff/05-entrate/server.md`. **Produzione**: un'entrata di prova «PROVA COLLAUDO entrata» da 1,00 € e una
ricorrente di prova che parte il mese prossimo, entrambe eliminate a fine prova. Il resoconto lo dice, e dice
che i totali del mese sono tornati quelli di prima. Browser con `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed;
`live-testing` verde | BUDGET: 12 M tok_eq

RESOCONTO IN: `handoff/05-entrate/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
