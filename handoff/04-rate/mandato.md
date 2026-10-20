UNITÀ: 4/5 — Spese a rate

OBIETTIVO: nel modulo del Registro, con il tipo «Spesa», un interruttore «a rate» con il numero di rate (2-60)
crea, premendo «Segna», un piano a rate invece di una spesa singola. Il piano è una regola ricorrente mensile
che si chiude da sola dopo N rate. Ogni rata si chiama «<descrizione> · rata k/N», e l'ultima prende il resto
dell'arrotondamento. Nella pagina Ricorrenti i piani stanno nel gruppo «A rate», con rata k/N e il residuo da
pagare, e sono esclusi dai Mensili e dalla quota da accantonare. Nell'editor di un piano già partito si possono
cambiare solo descrizione e categoria, e si può usare Termina; prima della prima rata è tutto modificabile.
Spec: `docs/superpowers/specs/2026-10-20-rate-entrate-design.md` §4 e §5. Scelte dell'utente del 20 ott:
interruttore solo sulla spesa nuova, piano avviato solo chiudibile.

PERIMETRO: `Models/RecurringExpense.cs`, `Services/RecurringExpenseRepository.cs`, `Services/CalcoliRicorrenti.cs`,
`Pages/Spese.razor` (solo il modulo di inserimento), `Pages/RicorrenteEdit.razor`, `Pages/Ricorrenti.razor`
(solo il gruppo «A rate»), test sotto `Eton.Tests/`, `handoff/04-rate/` | NON TOCCARE: `supabase/`,
`Models/Expense.cs` (le rate non aggiungono colonne alle spese), `Pages/Home.razor`, `Pages/SpeseTabella.razor`,
`Services/TabellaSpese.cs`, `docs/`, `handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI: colonne della migrazione dell'unità 3, già applicata in produzione: `recurring_expenses.installments`
(smallint, null) e `recurring_expenses.plan_total` (numeric(12,2), null); `kind` esiste ma è dell'unità 5: qui
le regole create sono sempre `'expense'` per default. Firme da rispettare: le firme nuove di raggruppamento e
quota che l'unità 2 cita in `CONTRATTI` del suo resoconto (`handoff/02-gruppi-quota/resoconto.md`), a cui
aggiungi il gruppo «A rate». Dalla ricognizione: `Services/CalcoliRicorrenti.cs:134`
`public static Expense Occorrenza(RecurringExpense r, PeriodoDovuto d)` — è il punto unico dove si calcolano
suffisso e resto (spec §4.2); `Services/RecurringExpenseRepository.cs:11`
`public sealed record DatiRegola(decimal Importo, string Descrizione, string Categoria, int OgniMesi, int Giorno, DateTime Inizio)`.

STATO: unità 1, 2, 3 e passaggio dell'utente sulla migrazione — vedi `handoff/PIANO.md` e i loro resoconti.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.

RESA: minima, sui token esistenti, dichiarata provvisoria fino alla 2.1-bis.

PROVA NEL BROWSER: `live-testing` e `ui-critic` (§7). Server avviato e fermato da te dal tuo worktree, con porta
e PID in `handoff/04-rate/server.md`. **Produzione**: un piano di prova «PROVA COLLAUDO rate» parte il mese
prossimo, così non materializza nulla e resta eliminabile; va eliminato a fine prova, e il resoconto lo dice.
Per vedere una rata materializzata basta il test puro, non serve una spesa vera di oggi. Browser con
`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed;
`live-testing` verde | BUDGET: 8 M tok_eq

RESOCONTO IN: `handoff/04-rate/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
