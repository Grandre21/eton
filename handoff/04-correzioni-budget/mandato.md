UNITÀ: 4/4 — I difetti della prova nel browser del budget

OBIETTIVO: correggere i tre difetti trovati dalla prova nel browser della unità 2 (dettaglio e misure in
`handoff/02-budget/prova-capo.md`, sezione «Esito di live-testing»), osservabili così:
(1) in `/expenses/budgets` ogni riga di budget mostra la barra con una larghezza proporzionale allo speso (piena
quando sforato): oggi `ul.barre-categorie` è una griglia a 4 colonne (143,6 / 436,4 / 0 / 130,4 px misurati a
1258 px) e `span.barra` cade nella colonna da 0 px; la ripartizione per categoria del Registro, che usa la
stessa classe, resta identica a oggi; la riga regge a 390 px senza scorrimento orizzontale e con bersagli da
48 px;
(2) su «Nuovo budget», scegliere soltanto la categoria senza scrivere un importo e poi uscire con «Torna ai
budget» non apre la finestra nativa «uscire senza salvare?» (guardia d'uscita, `NavigationLock`): la guardia
scatta solo quando c'è qualcosa che andrebbe perso, con la stessa regola che usano gli altri editor del
progetto (es. l'editor delle ricorrenti) — se la loro regola è la stessa che ha causato il blocco, dillo in
SCOSTAMENTI e allinea il budget al criterio «importo scritto o modificato»;
(3) con zero budget la pagina mostra comunque il mese corrente con ‹ › sopra «Ancora nessun budget qui».
Facoltativo, solo se nello stesso punto e senza codice nuovo: la sezione «In arrivo» che scompare e ricompare
durante il cambio di mese (nota del checker della unità 2).

PERIMETRO: `Pages/Budget.razor`, `Pages/BudgetEdit.razor`, i loro eventuali `.razor.css`, il foglio di stile
globale dove vive `barre-categorie` (solo regole nuove o con un selettore limitato alle righe dei budget),
`Eton.Tests/` (test nuovi se la correzione ha logica testabile), `handoff/04-correzioni-budget/` | NON
TOCCARE: `supabase/`, `Services/`, `Models/`, `Pages/Spese.razor`, `Pages/SpeseTabella.razor`, `Shared/`,
`docs/`, `handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI: nessuna firma cambia. La resa resta minima (classi e componenti esistenti; la grafica curata è della
2.1-bis): «sforato» con colore d'allarme NON si fa qui.

STATO: unità 1 (migrazione, applicata in produzione), 2 (budget, PARZIALE per la prova), 3 (residui) fuse su
`main`. Resoconti in `handoff/0N-*/resoconto.md`.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.
⚠️ Lezioni della 2.3: subito dopo la clausola verbatim «il file `-o`» aggiungi: «(è il tuo messaggio
finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`)». Nel brief vieta a Codex
`Set-Content`, `Out-File` e `[System.IO.File]::Write*`: le modifiche ai file solo con apply_patch; cercale nel log.

RESA: minima; mobile-first, bersagli da 48 px.

PROVA NEL BROWSER: **non la fai tu, e non lanci live-testing**: la pagina dei budget si prova solo con dati in
produzione, che una sessione in background non può scrivere (unità 2, FUORI SCOPE). La rifà il capo dopo il
merge. Tu misuri ciò che si misura senza dati: nessuna. Scrivi nel resoconto, in una sezione `PASSI PER IL
CAPO`, cosa guardare per ciascuno dei tre punti (selettore, misura attesa). ui-critic non si lancia qui.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed.
Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 4 M tok_eq

RESOCONTO IN: `handoff/04-correzioni-budget/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
