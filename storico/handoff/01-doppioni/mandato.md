UNITÀ: 1/5 — I doppioni della lettura a blocchi

OBIETTIVO: la lettura delle spese di un periodo non restituisce mai due volte la stessa spesa, anche quando
un altro membro scrive fra due blocchi da 1000 righe. Spec: `docs/superpowers/specs/2026-10-20-rate-entrate-design.md`
§2 (rimedio, test atteso, e il caso «riga saltata» che resta accettato e documentato). Un test che fallisce
prima della correzione e passa dopo. Nessun cambiamento visibile all'utente sotto le 1000 spese.

PERIMETRO: `Services/ExpenseRepository.cs`, i test sotto `Eton.Tests/` che servono a fissare la correzione
(file esistente o nuovo), `handoff/01-doppioni/` | NON TOCCARE: `supabase/`, `Pages/`, `Shared/`, `Models/`,
`Services/CalcoliRicorrenti.cs`, `docs/`, `handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI: nessuna firma pubblica cambia. Da rispettare, dalla ricognizione dello scout del 20 ott:
`Services/ExpenseRepository.cs:113` `public async Task<SpeseDelPeriodo> ElencaConPrevisteAsync(Guid spazioId, DateTime da, DateTime a)`
— consumata da Home, Spese e SpeseTabella, resta identica.

STATO: prima unità del goal 2.3 (piano in `handoff/PIANO.md`). La lettura a blocchi l'ha introdotta la 2.2-E
(resoconto archiviato in `storico/handoff/08-correttiva-lettura/resoconto.md`); il rilievo da correggere è R1
della chiusura del 12 ottobre (`handoff/CHIUSURA.md`), ratificato dall'utente il 19 ottobre. Posizione di
`tech-advisor` (20 ott): deduplicare per `Id` al ritorno della lettura basta, e `doc-checker` su `@key` duplicata
non serve, perché dopo la deduplicazione nessuna chiave della griglia si ripete.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato» (lanci, clausole verbatim, ripiego su `implementer` solo se Codex non parte). Leggila prima del primo
`codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.

PROVA NEL BROWSER: non serve (nessuna interfaccia toccata); dichiaralo nel resoconto.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed, con
il numero dei superati ≥ quello di `main` più i test nuovi | BUDGET: 3 M tok_eq

RESOCONTO IN: `handoff/01-doppioni/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
