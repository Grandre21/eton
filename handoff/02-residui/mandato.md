UNITÀ: 2/3 — I residui del flusso budget lasciati dalla 2.4

OBIETTIVO: quattro difetti piccoli della 2.4, ciascuno con il suo risultato osservabile:
(a) nel Registro, arrivando da «Registra una spesa» di un budget, passare il tipo a Entrata e poi di nuovo a Spesa
riporta la categoria a quella del budget (oggi torna alla prima dell'elenco, e dopo «Segna» si torna lo stesso ai
budget, che non risultano consumati);
(b) nell'editor delle ricorrenti la guardia d'uscita non scatta più per la sola scelta di una categoria: conta come
modifica solo ciò che conta già nell'editor del budget dopo la correzione della 04 della 2.4 (stessa regola, stessa
forma);
(c) l'editor del budget dice, in una frase sotto la categoria, che le ricorrenti di quella categoria consumano il
budget dal giorno in cui scadono (l'elenco dei budget lo dice già; l'editor no);
(d) la decisione «dopo Segna si torna ai budget?» (oggi una condizione dentro la pagina del Registro) è estratta in
una funzione pura in `Services/`, con test xunit dei casi: spesa singola da budget → sì; entrata da budget → no;
piano a rate da budget → no (si resta nel Registro, scelta da ratificare in `PIANO.md`); spesa senza budget → no.
Nessuna resa grafica nuova: niente colori, spaziature o componenti nuovi.

PERIMETRO: `Pages/Spese.razor`, `Pages/RicorrenteEdit.razor`, `Pages/BudgetEdit.razor` (solo la frase di (c)), un
file nuovo in `Services/` per (d), un file di test nuovo in `Eton.Tests/`, `handoff/02-residui/` | NON TOCCARE:
`supabase/`, `Models/`, `Services/BudgetRepository.cs`, `Services/CalcoliBudget.cs`, `Pages/Budget.razor`,
`wwwroot/`, `docs/`, `handoff/PIANO.md`, `handoff/CODA.md`, `handoff/01-migrazione-periodo/`

CAPO: eton 1 (la sessione coordinatore che ti ha aperto con start_session; il nome è quello di ListAgents)

CONTRATTI: nessuna firma condivisa con altre unità. La 03 modificherà dopo di te `Pages/BudgetEdit.razor`: la tua
frase di (c) stia in un elemento a sé, senza spostare il resto del modulo.

STATO: la 01-migrazione-periodo ha scritto la migrazione della 2.5 (resoconto
`handoff/01-migrazione-periodo/resoconto.md`); non tocca i tuoi file. Il goal 2.4 ha costruito il flusso budget:
rapporto in `storico/handoff/2026-11-14-CHIUSURA-budget-2.4.md` (vedi FUORI SCOPE), resoconto della correzione della
guardia in `storico/handoff/04-correzioni-budget/resoconto.md`.

AUTORE DEL DIFF: `implementer`, come da `C:\Users\Andre!\.claude\protocollo.md`, che leggi per intero prima del
primo agente del ciclo.

PROVA NEL BROWSER: sì, per (a), (b) e (c), con l'app di sviluppo (`app_start project=<csproj di Eton>
profile=<profilo di sviluppo>`; memoria del progetto: il server di sviluppo va riavviato prima di ogni prova). Lo
sviluppo punta ai dati di produzione: **nessun dato si crea né si salva** — si prova fino al punto prima di
«Segna» / «Salva», e la guardia d'uscita si prova solo nella metà che non apre la finestra nativa (memoria: i
dialoghi nativi bloccano il plugin). La metà con la finestra nativa va in `SCOSTAMENTI` come prova per l'utente,
con i passi. Spegni l'app prima del resoconto.

GATE: `mcp__synapse__build root=G:\Sviluppo\Eton` → 0 errori; `mcp__synapse__test root=G:\Sviluppo\Eton` → 0
failed. Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 3 M tok_eq

COMMIT: a lavoro finito committi su `main` nella cartella del progetto **senza push** (il push lo fa il capo).

RESOCONTO IN: G:\Sviluppo\Eton\handoff\02-residui\resoconto.md

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
