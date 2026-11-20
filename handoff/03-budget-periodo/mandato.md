UNITÀ: 3/3 — Descrizione e durata dei budget, e la scorciatoia alle spese del mese

OBIETTIVO: nella pagina dei budget e nel suo editor:
(a) ogni budget ha una **descrizione** facoltativa (al più 200 caratteri, il limite lo mostra l'editor), che si
scrive in creazione e in modifica e si legge nell'elenco accanto alla categoria;
(b) in creazione si sceglie la **durata**: «ogni mese, finché non lo chiudo» (come oggi, senza fine) oppure «per N
mesi», con N da 1 a 24; con N mesi il budget finisce da solo all'ultimo mese, lo stesso importo vale ogni mese e il
residuo riparte pieno ogni mese (decisione dell'utente del 12 nov). L'elenco dice per ogni budget il periodo
(«da gennaio 2027, senza fine» / «da gennaio a marzo 2027»);
(c) un budget a periodo si può **chiudere prima** della sua fine (oggi la chiusura è ammessa solo per i budget senza
fine), e la chiusura non può allungarlo;
(d) se il database rifiuta un budget perché si sovrappone a un altro della stessa categoria, l'utente legge un
messaggio proprio («c'è già un budget per questa categoria in quei mesi»), non il messaggio generico: il rifiuto
arriva da PostgREST come HTTP 400 con `code` 23P01 nel corpo (contratto verificato dalla 01);
(e) da ogni budget una **scorciatoia** apre la tabella delle spese sul mese mostrato, tipo «spesa», **senza** filtro
di categoria (le spese da portare dentro il budget stanno in altre categorie: si spostano con «Cambia categoria» in
massa, che la tabella ha già);
(f) test xunit per la parte pura: calcolo della fine da N mesi, chiusura ammessa o no, riconoscimento del 23P01,
indirizzo della scorciatoia.
Nessuna resa grafica nuova oltre ai campi e alle righe necessari, nelle classi CSS che esistono.

PERIMETRO: `Models/CategoryBudget.cs`, `Services/BudgetRepository.cs`, `Services/CalcoliBudget.cs`,
`Pages/Budget.razor`, `Pages/BudgetEdit.razor`, file nuovi in `Services/` se servono, test in `Eton.Tests/`,
`handoff/03-budget-periodo/` | NON TOCCARE: `supabase/`, `Pages/Spese.razor`, `Pages/RicorrenteEdit.razor`,
`Pages/SpeseTabella.razor` e `Services/TabellaSpese.cs` (la scorciatoia usa l'indirizzo che la tabella legge già;
se non basta è una domanda al capo), `wwwroot/` salvo una regola CSS indispensabile dichiarata in `SCOSTAMENTI`,
`docs/`, `handoff/PIANO.md`, `handoff/CODA.md`, le cartelle delle altre unità

CAPO: la sessione coordinatore che ti ha aperto con start_session (domande e riga di fine le mandi a lei)

CONTRATTI: dalla 01, già su `main` in `supabase/migrations/20261120000000_budget_periodo.sql` e applicata in
produzione dall'utente:
  colonna `category_budgets.description` text not null default '', check `category_budgets_description_max`
  (`char_length(description) <= 200`), scrivibile in INSERT e UPDATE;
  vincolo `category_budgets_no_overlap` exclude using gist (`space_id` =, `category` =,
  `daterange(starts_on, (ends_on + interval '1 month')::date, '[)')` &&): `ends_on` è il primo giorno dell'ultimo
  mese coperto, null = senza fine;
  una violazione torna da PostgREST: HTTP 400, corpo
  `{"code":"23P01","details":"Key conflicts with existing key.","hint":null,"message":"conflicting key value violates exclusion constraint \"category_budgets_no_overlap\""}`.
  `ends_on` è già nel grant di INSERT e di UPDATE dalla 2.4; `starts_on` e `category` restano solo in INSERT.

STATO: 01 FATTO (resoconto `handoff/01-migrazione-periodo/resoconto.md`); 02 FATTO (resoconto
`handoff/02-residui/resoconto.md`: ha aggiunto una frase sulle ricorrenti in `Pages/BudgetEdit.razor`, in un
elemento a sé dopo la categoria e prima dell'importo, da non spostare). Per la prova nel browser: il pannello Browser
dell'app ha già l'accesso a Eton fatto dall'utente il 20 nov; se torna la schermata «Entra con Google», il login lo
fa l'utente. La 2.4 ha costruito il flusso budget: rapporto in `storico/handoff/2026-11-14-CHIUSURA-budget-2.4.md`,
resoconti in `storico/handoff/02-budget/` e `storico/handoff/04-correzioni-budget/`. Lo sviluppo punta ai dati di
produzione e un push su `main` pubblica il sito.

AUTORE DEL DIFF: `implementer`, come da `C:\Users\Andre!\.claude\protocollo.md`, che leggi per intero prima del
primo agente del ciclo. `threat-hunter` guarda che la descrizione sia trattata come testo dell'utente ovunque si
mostra.

PROVA NEL BROWSER: sì, con l'app di sviluppo (`app_start project=<csproj di Eton> profile=<profilo di sviluppo>`;
memoria del progetto: il server di sviluppo va riavviato prima di ogni prova). Sui dati di produzione **nessun dato
si salva** in questa unità: si prova fino al punto prima di «Salva» (campi, limite dei 200 caratteri, scelta della
durata, periodo mostrato, scorciatoia che apre la tabella giusta). La prova con salvataggio — creare un budget per N
mesi, chiuderlo prima, provocare il rifiuto per sovrapposizione, poi cancellare i dati di prova — la fa il capo dopo
di te con un sì nuovo dell'utente: scrivi in `SCOSTAMENTI` i passi esatti, uno per riga, con l'atteso. Spegni l'app
prima del resoconto.

GATE: `mcp__synapse__build root=G:\Sviluppo\Eton` → 0 errori; `mcp__synapse__test root=G:\Sviluppo\Eton` → 0
failed. Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 5 M tok_eq

COMMIT: a lavoro finito committi su `main` nella cartella del progetto **senza push** (il push lo fa il capo).

RESOCONTO IN: G:\Sviluppo\Eton\handoff\03-budget-periodo\resoconto.md

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
