UNITÀ: 6/6 — Ritocchi dopo la ratifica di 04 e 05

OBIETTIVO: due ritocchi piccoli, ratificati dall'utente il 20 ott («Tieni, correggo le 5 frasi (Consigliata)»).
(a) Nell'editor di un'entrata ricorrente (`Pages/RicorrenteEdit.razor`) i testi secondari che parlano ancora di
«spese» già segnate devono seguire il tipo, con il `NomeTipo` che l'unità 05 ha già introdotto
(`Pages/RicorrenteEdit.razor:340`). Punti dati dal checker della 05: righe 200 («le spese dei mesi trascorsi si
segnano da sole»), 245, 267, 278, 297. Con tipo Spesa ogni testo resta identico a oggi. (b) BE4 dell'unità 04:
un overload `CalcoliRicorrenti.Prossima(RecurringExpense r, DateTime oggi)` al posto dei cinque argomenti presi
dalla stessa regola (`Services/CalcoliRicorrenti.cs:194`, `:245`, `Pages/RicorrenteEdit.razor:262`,
`Pages/Ricorrenti.razor:158`, che così perde il proprio wrapper privato). Refactor puro: nessun comportamento
cambia. La forma a cinque argomenti resta se ha altri chiamanti o test. `Dovuti` non si tocca.

PERIMETRO: `Services/CalcoliRicorrenti.cs`, `Pages/RicorrenteEdit.razor`, `Pages/Ricorrenti.razor`, test sotto
`Eton.Tests/`, `handoff/06-ritocchi/` | NON TOCCARE: `supabase/`, `docs/`, `handoff/PIANO.md`, `Models/`,
`Services/ExpenseRepository.cs`, ogni CSS

CAPO: coordinatore-Eton

CONTRATTI: `Services/CalcoliRicorrenti.cs` — la firma esistente di `Prossima(DateTime inizio, DateTime? fine,
int ogniMesi, int giorno, DateTime oggi)` (verificala con synapse); `Pages/RicorrenteEdit.razor:340`
`private string NomeTipo => TipoMovimento.Testo(tipo).ToLowerInvariant();`.

STATO: unità 1-5 fuse e pubblicate su `main` il 20 ott — vedi `handoff/PIANO.md` e i resoconti.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.
⚠️ Lezioni delle unità 01-05: la clausola verbatim «il file `-o`» fa o creare a Codex un file chiamato `-o`, o
fermarsi `BLOCKED` a chiedere quale sia. Subito dopo la clausola verbatim aggiungi: «(è il tuo messaggio finale:
la riga di comando lo salva da sé; non creare un file chiamato `-o`)» — con questa riga la 05 non ha più avuto
il problema. Nel brief vieta a Codex `Set-Content`, `Out-File` e `[System.IO.File]::Write*`: le modifiche ai file
solo con apply_patch; il `checker` le cerca nel log.

RESA: nessuna modifica visiva oltre ai testi.

PROVA NEL BROWSER: non serve `ui-critic`. `live-testing` solo se lo richiede la tabella del §7; se sì, **niente
dati in produzione**: i testi dell'editor si vedono aprendo «Nuova entrata ricorrente» senza salvare. Server
avviato e fermato da te dal tuo worktree, porta e PID in `handoff/06-ritocchi/server.md`. Browser con
`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed.
Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 3 M tok_eq

RESOCONTO IN: `handoff/06-ritocchi/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
