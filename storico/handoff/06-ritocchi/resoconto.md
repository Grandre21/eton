UNITÀ: 6 — ESITO: FATTO

TOCCATI:
- Pages/RicorrenteEdit.razor → +9/−6
- Pages/Ricorrenti.razor → +2/−7
- Services/CalcoliRicorrenti.cs → +6/−2
- handoff/06-ritocchi/ (brief-codex-1.md, codex-1.md, brief-live-testing.md, server.md, questo resoconto) → nuovi

REVIEW:
review: 1 — testi secondo il tipo e overload Prossima
  autore          codex exec 0.161.0 — 3 files changed, 17 insertions(+), 15 deletions(-)
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 0
  checker         nessuna istruttoria
  live-testing    ESITO: verde
  ui-critic       non lanciato — tabella §7 riga 2 — 6 files changed, 143 insertions(+), 15 deletions(-)

(La riga `--stat` di ui-critic è quella del diff intero misurata dopo `git add -N .`, handoff compreso: 3 file
nuovi tutti in `handoff/`; sul codice 0 file nuovi, 0 dichiarazioni/endpoint dal secondo comando del §3. Il
mandato diceva comunque «non serve `ui-critic`». `backend-expert` lanciato pur senza obbligo, perché i
`create mode` del diff intero erano 3.)

CONTRATTI:
- Services/CalcoliRicorrenti.cs:90-91 `public static DateTime? Prossima(DateTime inizio, DateTime? fine, int ogniMesi, int giorno, DateTime oggi)` — invariata; resta per il test Eton.Tests/CalcoliRicorrentiTests.cs:100 e come destinazione dell'overload.
- Services/CalcoliRicorrenti.cs:106-107 `public static DateTime? Prossima(RecurringExpense r, DateTime oggi) => Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, oggi);` — nuova; chiamanti: `Stato` (:198), `Gruppo` (:249), Pages/RicorrenteEdit.razor:262, Pages/Ricorrenti.razor:86.
- Pages/RicorrenteEdit.razor:340 `private string NomeTipo => TipoMovimento.Testo(tipo).ToLowerInvariant();` — invariato.
- Pages/RicorrenteEdit.razor:343 `private string NomeTipoPlurale => tipo == TipoMovimento.Entrata ? "entrate" : "spese";` — nuovo, usato nei cinque testi (righe 200, 245, 267, 278, 297).
- `Dovuti`: non toccato.

ADJUDICA:
- nessun rilievo da nessun revisore. Infondati: nessuno, quindi nessun campione da riverificare.
- registro di Codex: nel log del lancio (`$CLAUDE_JOB_DIR\tmp\codex-1.log`) le forme vietate (`Set-Content`, `Out-File`, `[System.IO.File]::Write*`, interpreti inline) compaiono solo nell'eco del brief (righe 112 e 116), nessun comando eseguito. Controllo fatto da me con Grep, non da un checker (nessuna istruttoria era dovuta).

FUORI SCOPE:
- In Pages/RicorrenteEdit.razor restano testi che dicono «spesa ricorrente» anche con una regola di tipo Entrata:
  righe 14-15 (spiegazione in testa), 27 (regola sparita), i messaggi d'errore in `@code` (439, 479, 541, 618, 629,
  647, 655) e l'avviso dopo «Termina» alla riga 610 («Terminata: da domani non segna più spese.»). Non erano fra le
  cinque frasi ratificate; li segnalo al capo per un eventuale ritocco successivo.
- La riga 274 («Le spese già segnate restano.», piano a rate) resta «spese» di proposito: un piano a rate è sempre
  una spesa (vincolo del database, supabase/migrations/20261020000000_rate_entrate.sql:50).

GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\06-ritocchi → build: succeeded — 0 errors, 4 warnings — 3.6s
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\06-ritocchi → test: 0 failed, 417 passed, 0 skipped — 5.5s

PROVA NEL BROWSER: live-testing ESITO: verde, 4/4 criteri, nessun dato toccato (solo GET in rete). Provato il testo
della riga 200 su «Nuova ricorrente» con Spesa, Entrata e di nuovo Spesa: «le spese…» / «le entrate…», carattere per
carattere. Non provati nel browser, per la regola «niente dati in produzione», i quattro testi visibili solo su una
regola esistente (245, 267, 278, 297): coperti da build, bug-hunter e dalla stessa espressione già provata alla 200.
Server avviato e fermato da me (PID in server.md), porta 5000 libera a fine unità.

SCOSTAMENTI: nessuno

DECISIONI DELL'UTENTE QUI: nessuna

DA RATIFICARE:
- Il plurale sta in una proprietà locale della pagina (`NomeTipoPlurale`), non in `TipoMovimento` (Models/ era fuori
  perimetro). Raccomandazione: tenerla lì finché una seconda pagina non ne ha bisogno.

SEGNALAZIONI: nessuna

COMMIT: 7993d0c
