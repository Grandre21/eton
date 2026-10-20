UNITÀ: 7/7 — Le scritte «spesa» rimaste sull'editor di un'entrata ricorrente

OBIETTIVO: in `Pages/RicorrenteEdit.razor`, con una regola di tipo Entrata, nessuna scritta deve parlare di
«spesa/spese». Decisione dell'utente del 20 ott: «A ora, B alla 2.1-bis (Consigliata)» — solo testi, zero CSS.
Punti noti (checker della 06 e `tech-advisor`): spiegazione in testa (righe 14-15, compreso «quando chi la paga
apre Eton»), 27 (regola sparita), 136 («sulla spesa del mese»), 610 («Terminata: da domani non segna più
spese.»), i messaggi d'errore in `@code` (439/442, 479/482, 541, 618, 629, 647, 655). Rifai tu il `grep` di
«spes» sull'intero file: l'elenco è un punto di partenza, non il perimetro.
Regole: dove il tipo è noto usa `NomeTipo` (:340) e `NomeTipoPlurale` (:343). **Dove la regola non è ancora
caricata** (la 27 e l'errore in apertura, 442: `tipo` è ancora al default `Spesa`, :313) serve una parola neutra,
«questa ricorrente» o simile, non il tipo. La 274 («Le spese già segnate restano.», piano a rate) resta «spese»:
un piano a rate è sempre una spesa. Con tipo Spesa ogni testo resta identico a oggi, salvo le frasi neutre.

PERIMETRO: `Pages/RicorrenteEdit.razor`, test sotto `Eton.Tests/` se servono, `handoff/07-testi/` | NON TOCCARE:
ogni CSS, `supabase/`, `docs/`, `handoff/PIANO.md`, `Models/`, `Services/`

CAPO: coordinatore-Eton

CONTRATTI: `Pages/RicorrenteEdit.razor:340` `private string NomeTipo => TipoMovimento.Testo(tipo).ToLowerInvariant();`;
`:343` `private string NomeTipoPlurale => tipo == TipoMovimento.Entrata ? "entrate" : "spese";`.

STATO: unità 1-6 fuse e pubblicate su `main` il 20 ott — vedi `handoff/PIANO.md` e i resoconti.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.
⚠️ Lezioni delle unità 01-06: subito dopo la clausola verbatim «il file `-o`» aggiungi: «(è il tuo messaggio
finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`)». Nel brief vieta a Codex
`Set-Content`, `Out-File` e `[System.IO.File]::Write*`: le modifiche ai file solo con apply_patch; cercale nel log.

RESA: nessuna modifica visiva oltre ai testi.

PROVA NEL BROWSER: `live-testing` solo se lo richiede la tabella del §7; se sì, **niente dati in produzione**: si
guarda «Nuova ricorrente» con Spesa ed Entrata senza salvare. Server avviato e fermato da te dal tuo worktree,
porta e PID in `handoff/07-testi/server.md`. Browser con `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed.
Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 3 M tok_eq

RESOCONTO IN: `handoff/07-testi/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
