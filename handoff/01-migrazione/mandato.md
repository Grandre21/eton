UNITÀ: 1/3 — La migrazione dei budget e del tetto alle rate, scritta e non applicata

OBIETTIVO: esiste `supabase/migrations/20261024000000_budget.sql` con esattamente lo schema della spec
`docs/superpowers/specs/2026-10-24-budget-design.md` §4: la tabella `category_budgets` con RLS, GRANT e trigger
ricopiati per forma da quelli di `recurring_expenses` (membri dello spazio), e il check che limita
`recurring_expenses.installments` a 60. La migrazione è additiva e sicura con il client già pubblicato. **Non la
applichi**: nessun agente tocca il database di produzione. La applica l'utente dall'SQL editor. Il resoconto
porta, in una sezione `SQL PER L'UTENTE`: (1) il testo integrale della migrazione; (2) una query di controllo
che l'utente lancia dopo, che restituisce una riga sola di valori, con il risultato atteso scritto accanto
(comprende: la tabella esiste con RLS attiva, il numero di policy, il check sulle rate presente, il massimo di
`installments` già in tabella ≤ 60); (3) il ripiego `NOTIFY pgrst, 'reload schema';` se compare «tabella
sconosciuta». **Prima del check sulle rate** la migrazione deve reggere su dati esistenti: se un piano con più
di 60 rate esistesse, il check fallirebbe; la query di controllo lo dice prima, e il resoconto spiega cosa fare
in quel caso. I modelli C# **non** cambiano in questa unità (li scrive la 2).

PERIMETRO: `supabase/migrations/` (solo il file nuovo), uno script nuovo `supabase/verifica-rls-budget.sql` sul
modello degli script `supabase/verifica-rls-*.sql` esistenti, `handoff/01-migrazione/` | NON TOCCARE: le
migrazioni esistenti, gli script `verifica-rls-*.sql` esistenti (li riordina la 3), `Models/`, `Services/`,
`Pages/`, `Shared/`, `Eton.Tests/`, `docs/`, `handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI: nomi decisi dal capo, li usa la 2. Non rinominarli.
  tabella `category_budgets`: `id` uuid pk; `space_id` uuid (FK sugli spazi come `recurring_expenses`);
  `category` text non vuoto; `amount` numeric(12,2) `> 0`; `starts_on` date, primo giorno del mese (check);
  `ends_on` date null, primo giorno del mese e `>= starts_on` (check); `version` int; colonne di creazione,
  autore e modifica con gli stessi nomi e default di `recurring_expenses`. Indice unico parziale su
  (`space_id`, `category`) dove `ends_on is null`: al più un budget aperto per categoria e spazio.
  Privilegi per colonna come `recurring_expenses`: `space_id` e l'autore solo in INSERT; `amount`, `ends_on`,
  `version` aggiornabili; `category` e `starts_on` solo in INSERT. Concorrenza ottimistica su `version` come
  le ricorrenti.
  check su `recurring_expenses`: `installments is null or installments <= 60`, con un nome esplicito.
  Nessun vincolo fra categoria e tipo del movimento (ratificato il 24 ott, spec §4).

STATO: prima unità del goal, nessuna unità precedente. Partizione e decisioni in `handoff/PIANO.md`.
Riferimento per lo stile: `supabase/migrations/20260922000000_ricorrenti.sql` (trigger e privilegi per colonna)
e `supabase/migrations/20261020000000_rate_entrate.sql` (l'ultima, con il suo script di collaudo
`supabase/verifica-rls-rate-entrate.sql`).

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.
⚠️ Lezioni della 2.3: subito dopo la clausola verbatim «il file `-o`» aggiungi: «(è il tuo messaggio
finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`)». Nel brief vieta a Codex
`Set-Content`, `Out-File` e `[System.IO.File]::Write*`: le modifiche ai file solo con apply_patch; cercale nel log.
`threat-hunter` guarda in particolare RLS e privilegi per colonna: un utente non deve leggere né scrivere i
budget di uno spazio di cui non è membro, né spostare un budget in un altro spazio.

PROVA NEL BROWSER: non serve. Prova la migrazione **sul Supabase locale** (memoria del progetto: porte
riservate da Windows; Docker lo avvii tu e lo spegni appena finito, con `docker desktop stop`), insieme allo
script di collaudo RLS, e scrivi nel resoconto gli errori attesi e ottenuti. Se non puoi, dichiaralo.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed.
Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 4 M tok_eq

RESOCONTO IN: `handoff/01-migrazione/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
