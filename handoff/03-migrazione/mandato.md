UNITÀ: 3/5 — La migrazione di rate ed entrate, scritta e non applicata

OBIETTIVO: esiste `supabase/migrations/<data di oggi>000000_rate_entrate.sql` con esattamente lo schema della
spec `docs/superpowers/specs/2026-10-20-rate-entrate-design.md` §3: `kind` su `expenses` e
`recurring_expenses` (default `'expense'`, check sui due valori, solo INSERT, rimesso a `old.kind` dal trigger
di aggiornamento delle spese), `installments` e `plan_total` su `recurring_expenses` con i loro vincoli e
privilegi. La migrazione è additiva e sicura con il client già pubblicato. **Non la applichi**: nessun agente
tocca il database di produzione. La applica l'utente dall'SQL editor. Il resoconto porta, in una sezione
`SQL PER L'UTENTE`: (1) il testo integrale della migrazione; (2) una query di controllo che l'utente lancia
dopo, con il risultato atteso scritto accanto; (3) il ripiego `NOTIFY pgrst, 'reload schema';` se compare
«colonna sconosciuta». I modelli C# **non** cambiano in questa unità; i test che confrontano schema e modelli
(per esempio quelli dei privilegi di INSERT) li adegua la 4 o la 5, a meno che non rompano qui: in quel caso lo
scrivi in `SCOSTAMENTI`.

PERIMETRO: `supabase/migrations/` (solo il file nuovo), gli eventuali script di prova RLS sotto `supabase/` se
il progetto ne ha per le migrazioni precedenti, `handoff/03-migrazione/` | NON TOCCARE: le migrazioni esistenti,
`Models/`, `Services/`, `Pages/`, `Shared/`, `docs/`, `handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI: i nomi delle colonne e dei valori sono decisi dal capo e li useranno le unità 4 e 5: `kind`
(`'expense'` | `'income'`), `installments` (smallint, null), `plan_total` (numeric(12,2), null). Non
rinominarli.

STATO: unità 1 (doppioni) e 2 (gruppi e quota) — vedi `handoff/PIANO.md`. Nessuna delle due tocca lo schema.
Riferimento per lo stile: le migrazioni `20260824000000_spese.sql` e `20260922000000_ricorrenti.sql`, con la
seconda che ha già un trigger e i privilegi per colonna.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.
`threat-hunter` guarda in particolare i privilegi per colonna e il trigger: un membro non deve poter cambiare
il tipo di un movimento di un altro, né il proprio dopo la creazione.

PROVA NEL BROWSER: non serve. Se hai Docker e il Supabase locale (memoria del progetto: porte riservate da
Windows, e Docker si spegne appena finito), puoi applicarla **in locale** per provarla; se non puoi, dichiaralo.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed |
BUDGET: 4 M tok_eq

RESOCONTO IN: `handoff/03-migrazione/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
