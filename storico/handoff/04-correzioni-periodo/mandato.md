UNITÀ: 4/4 — I due difetti della prova con salvataggio della 03

OBIETTIVO: correggere i due difetti trovati dalla prova del capo del 2026-11-20 (esito completo in
`handoff/03-budget-periodo/prova-capo.md`, schermate in `handoff/03-budget-periodo/prova-capo/`):
(a) **eccezione dopo il rifiuto per sovrapposizione.** Nell'editor «Nuovo budget», quando il database rifiuta il
salvataggio con 23P01 il messaggio proprio compare correttamente e il modulo resta com'è, ma subito dopo compare la
barra gialla di Blazor «Si è verificato un errore imprevisto. Ricarica» (`#blazor-error-ui` visibile): c'è
un'eccezione non gestita su quel percorso. Riprodotto due volte. Dopo la correzione il messaggio resta, la barra non
compare, e un secondo «Salva» si comporta allo stesso modo senza barra. Trova la causa con un test rosso se la parte
è pura, altrimenti dichiaralo;
(b) **descrizione lunga senza spazi che esce dalla pagina.** Nell'elenco dei budget una descrizione di 200 caratteri
senza spazi allarga la pagina (a 1280 px `scrollWidth` 1808 contro 1265; a 375 px 1466 contro 375): lo
`span.spiega` dentro `.azioni` non va a capo. Dopo la correzione, con la stessa descrizione, `scrollWidth` è uguale
a `clientWidth` a 1280 e a 375 px, la descrizione va a capo dentro la card e i pulsanti restano dove sono. Una
regola CSS minima, nel posto dove stanno già le regole di `.barre-categorie` e `.azioni`, senza cambiare la resa con
una descrizione normale.
Nessun'altra modifica.

PERIMETRO: `Pages/BudgetEdit.razor`, `Pages/Budget.razor`, `Services/BudgetRepository.cs`, `Services/CalcoliBudget.cs`,
`wwwroot/css/app.css` (solo la regola di (b)), test in `Eton.Tests/`, `handoff/04-correzioni-periodo/` |
NON TOCCARE: `supabase/`, `Models/`, `Pages/Spese.razor`, `Pages/RicorrenteEdit.razor`, `Pages/SpeseTabella.razor`,
`docs/`, `handoff/PIANO.md`, `handoff/CODA.md`, le cartelle delle altre unità

CAPO: la sessione coordinatore che ti ha aperto con start_session (domande e riga di fine le mandi a lei)

CONTRATTI: invariati quelli della 03 (resoconto `handoff/03-budget-periodo/resoconto.md`, CONTRATTI). Il messaggio
della sovrapposizione resta quello di oggi, parola per parola.

STATO: 01, 02, 03 FATTO e pubblicate; migrazione applicata in produzione. Prova del capo: 8 passi su 10 OK, i due KO
sono questa unità.

AUTORE DEL DIFF: `implementer`, come da `C:\Users\Andre!\.claude\protocollo.md`, che leggi per intero prima del
primo agente del ciclo.

PROVA NEL BROWSER: sì. Per (b) basta l'elenco: crea un solo budget «Casa» nello spazio «Personale» con la
descrizione di 200 caratteri senza spazi («PROVA2.5» seguito da «x»), misura `scrollWidth` contro `clientWidth` a
1280 e a 375 px, poi **eliminalo** (l'utente ha autorizzato in chat il 2026-11-20 la prova con salvataggio a patto di
cancellare i dati di prova; solo budget «Casa» con descrizione che inizia con «PROVA»). Per (a): stessa procedura
della prova del capo, due schede, un budget «per 1 mese» salvato nella prima e il rifiuto nella seconda; controlla
che `#blazor-error-ui` resti nascosto; poi elimina tutti i budget di prova (quello di dicembre si raggiunge da «In
arrivo»). L'elenco di «Personale» deve tornare «Ancora nessun budget qui.». Il pannello Browser ha già l'accesso
dell'utente; se torna «Entra con Google», il login lo fa l'utente. Spegni l'app prima del resoconto.

GATE: `mcp__synapse__build root=G:\Sviluppo\Eton` → 0 errori; `mcp__synapse__test root=G:\Sviluppo\Eton` → 0
failed. Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 2 M tok_eq

COMMIT: a lavoro finito committi su `main` nella cartella del progetto **e pubblichi con push** (l'utente: «voglio
che si pushi sempre prima di chiudere ove possibile», 2026-11-20).

RESOCONTO IN: G:\Sviluppo\Eton\handoff\04-correzioni-periodo\resoconto.md

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
