UNITÀ: 7 — ESITO: PARZIALE

Il lavoro sul codice è finito e verificato; manca solo la prova nel browser, rinviata al capo (v. SCOSTAMENTI).

TOCCATI:
- Pages/RicorrenteEdit.razor → +33/−12
- handoff/07-testi/brief-codex-1.md → +62/−0
- handoff/07-testi/brief-codex-2.md → +44/−0
- handoff/07-testi/codex-1.md → +7/−0
- handoff/07-testi/codex-2.md → +7/−0
- handoff/07-testi/server.md → +17/−0
- handoff/07-testi/resoconto.md → questo file

REVIEW:
  review:
    autore          codex exec 0.161.0 — 4 files changed, 88 insertions(+), 12 deletions(-)
    bug-hunter      RILIEVI: 2
    conformity      RILIEVI: 2
    threat-hunter   RILIEVI: 0
    backend-expert  RILIEVI: 1
    checker         VERDETTI: fondati 3 · infondati 0 · fuori scope 1 · non verificabili 0
    checker         VERDETTI: risolti 3 · non risolti 0 · non verificabili 0
    live-testing    ESITO: BLOCKED
    ui-critic       non lanciato — tabella §7 riga 2 — 4 files changed, 88 insertions(+), 12 deletions(-)

CONTRATTI:
- Pages/RicorrenteEdit.razor:349 `private string NomeTipo => TipoMovimento.Testo(tipo).ToLowerInvariant();` (invariato, riga slittata da :340)
- Pages/RicorrenteEdit.razor:351 `private string NomeTipoPlurale => tipo == TipoMovimento.Entrata ? "entrate" : "spese";` (invariato, riga slittata da :343)
- nuove, private: `UnNomeTipo` («una spesa»/«un'entrata»), `LaNomeTipo` («la spesa»/«l'entrata»), `Titolare` («chi la paga»/«chi la riceve»), `TipoNoto => Nuova || regola is not null`

ADJUDICA:
- bug-hunter BH1: fondato → corretto — con Entrata «La entrata», «una entrata», «la entrata» (difetto nato dal mio brief) — `errore = $"Non è stato possibile terminare la {NomeTipo} ricorrente: …"`
  verificato: risolto — Pages/RicorrenteEdit.razor:16 (LaNomeTipo :357, UnNomeTipo :354, usi :503, :653, :671, :679)
- bug-hunter BH2: fondato → corretto — a regola non caricata `tipo` resta Spesa (:313), Aiuto e titolo dicevano «spesa»/«chi la paga» — `private string tipo = TipoMovimento.Spesa;`
  verificato: risolto — Pages/RicorrenteEdit.razor:363 `private bool TipoNoto => Nuova || regola is not null;` (Aiuto :14-23, titolo :369)
- conformity CF1: fondato → corretto — duplicato di BH1 — `<p>La @NomeTipo ricorrente segna da sola …`
  verificato: risolto — Pages/RicorrenteEdit.razor:16
- conformity CF2: fondato → corretto (il checker l'ha dato fuori scope perché il nome veniva dal mio brief: il brief è mio, quindi l'ho corretto) — `private string ChiLaPaga => tipo == TipoMovimento.Entrata ? "chi la riceve" : "chi la paga";`
  verificato: risolto — Pages/RicorrenteEdit.razor:360 `private string Titolare => …`
- backend-expert BE1: fondato → corretto — stesso rilievo di CF2, RISCRITTURA con un altro nome; scelto `Titolare` — vedi CF2
  verificato: risolto — Pages/RicorrenteEdit.razor:360
- threat-hunter: 0 rilievi; aperto da me il diff: solo letterali fissi interpolati, resi come testo da Razor.
- Riverifica a campione degli infondati: nessun infondato.
- Con tipo Spesa ogni testo coincide carattere per carattere con HEAD, salvo i testi neutri a regola non caricata (checker, confronto con `git show HEAD:`).
- Log di Codex (codex-1.log, codex-2.log): nessuna scrittura con Set-Content/Out-File/File::Write né interpreti inline; le uniche righe che li nominano sono il testo del brief.

FUORI SCOPE: nessuno.

GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\07-testi → build: succeeded — 0 errors, 4 warnings — 3.4s
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\07-testi → test: 0 failed, 417 passed, 0 skipped — 5.6s

SCOSTAMENTI:
- Prova nel browser: rinviata al capo, browser del PC non collegato. Il Chrome d3148d48… del mandato non era collegato; l'unico collegato (9c100b99…, isLocal true) su localhost:5000 dava ERR_CONNECTION_REFUSED, mentre da questo PC il server rispondeva 200. ui-critic non serviva comunque (tabella §7 riga 2).
- Domanda al capo (circa 19:20, ora non annotata al momento): «aspetto che l'utente ricolleghi il Chrome oppure chiudo PARZIALE con la prova rinviata?». Risposta di coordinatore-Eton (circa 19:25): «vai con (b). Ferma il server […], chiudi il resoconto PARZIALE con la prova nel browser in SCOSTAMENTI come "rinviata al capo, browser del PC non collegato", e committa. La prova nel browser la faccio io dalla sessione interattiva dopo la fusione.»
- Oltre l'elenco del mandato, sempre nel perimetro: pannello d'aiuto e titolo neutri a regola non caricata (BH2), «chi la riceve» al posto di «chi la paga» con Entrata anche alla riga del permesso (:62), articoli con elisione.
- Server di sviluppo avviato e fermato da me (PID 30012/11968, porta 5000 libera alle 19:26): handoff/07-testi/server.md.
- Nessun file fuori perimetro.

DECISIONI DELL'UTENTE QUI: nessuna

DA RATIFICARE:
- Per le entrate «chi la riceve» al posto di «chi la paga» (aiuto, avviso sul permesso, errore di eliminazione). Raccomandazione: tenerlo, «chi la paga» un'entrata è sbagliato.
- Testi neutri a regola non caricata: aiuto «Una ricorrente si segna da sola nel mese in cui è dovuta, quando chi la paga o la riceve apre Eton.» e titolo «Ricorrente». Raccomandazione: tenerli, applicano la regola del mandato sulle frasi neutre.

SEGNALAZIONI: nessuna

COMMIT: 97d2006 (lavoro), b215f60 (server fermato)
