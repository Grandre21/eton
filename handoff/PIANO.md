# Piano — fase 2.4: budget mensili per categoria e residui dati della 2.3

## OBIETTIVO

2026-10-24 utente (chat), verbatim: «aggiungo una cosa, poi crei tu un piano per chiudere tutto cio che ce di
aperto : Sarebbe utile prevedere delle spese ricorrenti in un mese, tipo prevedo di spendere 50 euro per uscite,
ma non sono soldi ancora usciti quindi vorrei che siano previsti ma poi ogni volta che spendo vado lì e dico
quanto ho speso, così mi dice il rimanente del budget che ho previsto di spendere»

Questo goal è il primo dei due della sequenza scritta in
`docs/superpowers/specs/2026-09-10-modello-prodotto-design.md`, §6 «Richiesta e piano del 24 ottobre»: qui il
budget e i residui dati; la grafica è la 2.1-bis, goal successivo.

## DECISIONI

- 2026-10-24 utente (chat, popup, fine mese): «Si azzera (Consigliata)» — il residuo non si riporta; lo
  sforamento si mostra come «sforato di X», senza bloccare.
- 2026-10-24 utente (chat, popup, bilancio): «Riga a sé (Consigliata)» — il bilancio resta com'è, sotto una riga
  «Da spendere nei budget: X» con una barra per categoria; nessun saldo previsto di fine mese.
- 2026-10-24 utente (chat, popup, ordine): «Budget prima (Consigliata)» — 2.4 prima della 2.1-bis, resa minima.
- 2026-10-24 utente (chat, popup, archivio): «Sì, sposta tutto» — i resti dei cicli precedenti sono in
  `storico/handoff/` (commit di questa sessione).
- 2026-10-24 budget legato a una categoria esistente, in una tabella sua (non un tipo di regola ricorrente);
  «dico quanto ho speso» scrive una spesa vera con categoria precompilata; le ricorrenti della stessa categoria
  consumano il budget. Posizione di tech-advisor verificata sul codice — **ratificata 2026-10-24**.
- 2026-10-24 il vincolo categoria↔tipo resta nel client (`CambiaCategoriaAsync` controlla il tipo), il database
  resta senza check, come scelto in `Services/CategorieSpesa.cs` — **ratificata 2026-10-24**.
- 2026-10-24 utente (chat, popup, ratifiche): «Confermo (Consigliata)» a entrambe.
- 2026-10-24 utente (chat): «Non posso aprire una nuova sessione, fai partire qui il lavoro, sono dal cellulare
  quindi ti seguo a distanza» — questa sessione fa da coordinatore della 2.4.
- 2026-10-24 utente (chat, popup, spec): «Approvo, vai» — spec `docs/superpowers/specs/2026-10-24-budget-design.md`.
- 2026-11-12 utente (chat, bottone alla domanda «Partiamo con la prova nel browser del budget?», dopo il recap che
  chiedeva un sì nuovo per i dati di produzione): «Sì, partiamo» — seconda prova nel browser sui dati di
  produzione, da questa sessione interattiva. Esito della pulizia SQL del 25 ott non ricevuto: la prova verifica
  i resti della prima e li elimina dall'interfaccia se ci sono.
- 2026-11-12 utente (chat, popup delle ratifiche di fine goal, con la posizione di tech-advisor dentro ogni
  domanda): «Confermo tutte (Consigliata)» al database (01 ×3 e autore `created_by`), «Confermo tutte
  (Consigliata)» alle pagine (02 ×5, riga solo sotto il Registro, categoria nell'indirizzo), «Confermo tutte
  (Consigliata)» al codice interno e alle scelte del 12 nov (03 ×3, 04 ×1, capo ×2, regola dei 16 px, salto
  di «Salva») — tutte le DA RATIFICARE della 2.4 sono **ratificate 2026-11-12**. «Correggi ora
  (Consigliata)» a `aria-pressed`: unità piccola prima della chiusura del goal.

## PARTIZIONE

| Unità | Cosa | Dove | Dipende da | Stato |
|---|---|---|---|---|
| **S — spec 2.4** | spec del budget (le decisioni qui sopra, la tabella, la resa minima, i casi limite: categoria cambiata, spesa spostata di mese, budget chiuso) e dei residui | `docs/superpowers/specs/2026-10-24-budget-design.md`, approvata in chat | — | **PARZIALE** — approvata dall'utente in chat il 24 ott («Approvo, vai»); è un documento, nessun comando lo verifica |
| **scout** | ricognizione per la partizione delle unità, sul mandato dell'indice di `architettura-sessioni.md` | subagent `scout` | S | **PARZIALE** — mappa ricevuta il 24 ott (tre gruppi, file contesi `ExpenseRepository.cs` e `CalcoliRicorrenti.cs`); è una ricognizione, nessun comando la verifica |
| **01-migrazione** | `category_budgets` con RLS + check `installments <= 60`, in una migrazione sola, scritta e provata sul Supabase locale, **non applicata** | sessione-unità, worktree `G:\Sviluppo\wt-Eton\01-migrazione`, mandato in `handoff/01-migrazione/` | scout | **FATTO** — fusa su `main` il 24 ott (merge 7876feb), provata sul Supabase locale (14 errori attesi su 14), BH1 (sovrapposizione di periodi) passato come regola al mandato della 02, tre scelte da ratificare nel resoconto; verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 417 passed, 0 skipped` |
| **GATE migrazione** | l'utente applica la migrazione dall'editor SQL e incolla l'esito della query di controllo | utente | 01 | **SUPERATO** — 2026-10-25 l'utente ha incollato in chat l'esito della query di controllo: `tabella_esiste=true, rls_attiva=true, policy=4, indice_aperto=true, trigger_versione=true, tetto_rate=true, max_rate=0` (atteso) |
| **02-budget** | modello, repository, calcolo del residuo, pagina minima dei budget, «registra una spesa» con categoria precompilata, riga nel bilancio | sessione-unità, worktree `G:\Sviluppo\wt-Eton\02-budget`, ramo `unita-02-budget` | GATE | **FATTO** — fusa su `main` il 25 ott; prova nel browser (§7) rifatta il 12 nov da questa sessione dopo la 04: tutti i passi funzionali ok, dati di prova e resti del 25 ott eliminati; ui-critic 5 rilievi, 4 corretti in `wwwroot/css/app.css` più 2 di bug-hunter sulle correzioni (checker: risolti 6), `aria-pressed` rinviato all'utente (TIPO progetto); esito e tracciato in `handoff/02-budget/prova-capo.md`; seconda metà della guardia d'uscita provata a mano dall'utente il 12 nov, rifatta dopo un clic sbagliato («Chiede conferma»); verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 442 passed, 0 skipped` |
| **03-residui** | `CambiaCategoriaAsync` col tipo, test di `RecurringExpenseRepository`, chiave d'ordinamento in un posto, claims JWT degli script RLS in un posto | sessione-unità, worktree `G:\Sviluppo\wt-Eton\03-residui`, ramo `unita-03-residui` | 02 (solo ordine, file probabilmente contesi) | **FATTO** — fusa su `main` il 25 ott; script RLS provati sul Supabase locale prima/dopo con output identico; 5 rilievi fondati corretti, 2 test rossi confermati; tre scelte da ratificare nel resoconto, BE1 (un `Esito<T>` unico per sette repository) di progetto per l'utente; verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 442 passed, 0 skipped` |
| **04-correzioni-budget** | i difetti della prova nel browser della 02 (`handoff/02-budget/prova-capo.md`): barra invisibile, guardia d'uscita su «Nuovo budget» senza importo, mese e frecce con zero budget; poi i passi 9-10 della prova, rifatti da questa sessione | sessione-unità, worktree `G:\Sviluppo\wt-Eton\04-correzioni-budget`, ramo `unita-04-correzioni-budget`, mandato in `handoff/04-correzioni-budget/` | 03 (file della 02 su `main`) | **FATTO** (codice) — fusa su `main` il 25 ott; GATE dell'unità build 0 errori, test 442 passed; 1 rilievo fondato corretto; «In arrivo» facoltativo non fatto (codice nuovo); stesso difetto di guardia in `Pages/RicorrenteEdit.razor:388-389` fuori perimetro; la prova nel browser resta del capo (PROSSIMA AZIONE); verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 442 passed, 0 skipped` |

## RAZIONALE

Una migrazione sola perché in produzione le applica a mano l'utente: il tetto alle rate viaggia con la tabella
nuova. Lo scout (24 ott) conferma tre gruppi disgiunti salvo due file: `Services/ExpenseRepository.cs` (la 02
lo legge e chiama `CreaAsync`, la 03 cambia `CambiaCategoriaAsync`) e `Services/CalcoliRicorrenti.cs` (la 02
ne legge le occorrenze del mese, la 03 vi sposta la chiave d'ordinamento): contesa di file, non di firma, quindi
02 e 03 in sequenza e la 03 da `main` dopo la 02. Nessuna interfaccia nuova fra le due.

Scelte del capo per il mandato della 02 (annullabili con un revert, **ratificata 2026-11-12** a fine goal):
- 2026-10-24 la riga «Da spendere nei budget» va solo sotto il riepilogo del mese del Registro, non nel bilancio
  della tabella, che vale per un periodo filtrato e non per un mese — **ratificata 2026-11-12**. Raccomandazione:
  confermare.
- 2026-10-24 «Registra una spesa» apre il Registro con la categoria passata nell'indirizzo (oggi il modulo non
  riceve parametri) — **ratificata 2026-11-12**. Raccomandazione: confermare, è il modo già usato dalla tabella.
- 2026-10-24 risposta alla domanda della 01: autore del budget `created_by` (non `paid_by`); modifica ed
  eliminazione all'autore o al proprietario dello spazio, come le ricorrenti — **ratificata 2026-11-12**.
  Raccomandazione: confermare, è la copia delle regole già in produzione.
- 2026-10-25 risposta alla domanda della 03: il perimetro si allarga a `Services/RecurringExpenseRepository.cs`,
  solo per estrarre due punti statici `internal` (costruzione della regola nuova, decisione dell'esito con la
  rilettura come delegate) che rendono testabili senza database scritture e concorrenza; nessuna firma pubblica
  cambia — **ratificata 2026-11-12**. Raccomandazione: confermare, senza non si prova la concorrenza.
- 2026-10-24 la 03 porta in un posto solo anche la copia di ricerca della chiave (in `TabellaSpese`), se la sua
  semantica è identica — **ratificata 2026-11-12**. Raccomandazione: confermare.
- 2026-11-12 la regola `label.campo + .azioni, label.campo + .testo-tenue` (16 px fra un campo e le azioni o
  l'avviso) vale per ogni editor in flusso normale, non solo per il budget: cambia di 16 px anche la scheda
  delle recensioni (`Shared/RecensioniElemento.razor`) e i campi di un elemento (`Pages/ItemEdit.razor`, da 12
  a 16) — **ratificata 2026-11-12**. Raccomandazione: confermare, un campo attaccato al suo pulsante è lo stesso
  difetto ovunque.
- 2026-11-12 il salto di «Salva» quando compare l'avviso in «Nuovo budget» resta (ui-critic chiedeva la y
  fissa): è il comportamento degli altri editor — **ratificata 2026-11-12**. Raccomandazione: confermare.

## PROSSIMA AZIONE

2026-10-25 utente (chat): «sto spegnendo il pc, le prove le facciamo domani» — sessione del capo finita qui;
nessuna unità, rete o server acceso. Domani una sessione `coordinatore` nuova riparte da questo paragrafo.
2026-10-25 utente (chat, popup): «Sì, quando sono al PC (Consigliata)» — la prova parte quando l'utente scrive
di aver chiuso la finestra nativa bloccata su Chrome e lanciato le due query di pulizia. 04 fusa. Rifare da questa sessione la
prova nel browser del budget (passi della 02 più i `PASSI PER IL CAPO` della 04) sui dati di produzione, con
un sì nuovo dell'utente (quello del 25 ott valeva per la prima prova), poi ui-critic. Pulizia della prima
prova: SQL dato all'utente in chat il 25 ott, esito da ricevere. Le righe DA RATIFICARE della 01, della 02 e del
capo vanno nel popup di fine goal. Rilievi fuori scope della 02 per la 2.1-bis: commento di
`Shared/PaginaRegistro.cs` (C1), ripetizioni BE2/BE3, «In arrivo» che lampeggia al cambio mese,
«sforato di X» con un colore d'allarme (oggi lo stesso verde di «restano»), la y fissa di «Salva» in tutti gli
editor insieme, e le tre scelte del PIANO-DESIGN segnalate da ui-critic il 12 nov (cifra e link di «Da spendere
nei budget» in grigio, residuo non distinto dal totale, binario della barra a contrasto 1,21:1).

2026-11-12 seconda prova: build verde, server acceso (PID 24440, `handoff/02-budget/prova-capo.md`); il lancio
di live-testing è stato rifiutato dal classificatore della modalità auto anche nella sessione interattiva
(«judged this action dangerous», senza motivo). Nessun dato creato. Si aspetta la scelta dell'utente.
2026-11-12 utente (chat): «Cambio modalità, riprova» — prova eseguita, ui-critic eseguito, correzioni CSS
fatte e verificate, server fermato. Resta: la seconda metà della guardia d'uscita a mano dall'utente
(«Nuovo budget» con importo scritto → «Torna ai budget» deve chiedere conferma); la decisione su
`aria-pressed` (APERTO); le ratifiche di fine goal in un popup; poi la chiusura del goal 2.4.
2026-11-12 `aria-pressed` corretto e ratifiche date. Guardia: un primo esito cliccato per errore («ho
sbagliato a cliccare, devo rifare le prove»), poi prova rifatta: «Chiede conferma». Tutte le unità sono FATTO,
server fermato. Prossimo passo: la sessione di chiusura del goal 2.4 (copertura sull'OBIETTIVO, archivio delle unità FATTO in `storico/handoff/`,
sistemazione delle due righe storiche della 01 e della 03 che dicono ancora «da ratificare»), poi la 2.1-bis.

controllo contesto 2026-10-24 (avvio): nulla

## APERTO

- `aria-pressed` delle pastiglie: **risolto 2026-11-12** su decisione dell'utente («Correggi ora»), 9 punti
  in 6 file con la stringa `"true"/"false"`. review: bug-hunter `RILIEVI: 0`, threat-hunter `RILIEVI: 0`,
  checker `nessuna istruttoria`, coverage `COPERTURA: 7 clausole — coperte 6 · scoperte 0 · rinviate 1` (la
  rinviata è il commit e push, fatto subito dopo); verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` →
  `test: 0 failed, 442 passed, 0 skipped`.

- La guardia d'uscita dell'editor delle ricorrenti (`Pages/RicorrenteEdit.razor:388-389`) conta come modifica
  anche la sola scelta di una categoria: stesso difetto corretto nel budget dalla 04. Per la 2.1-bis o per
  un'unità piccola.

- Granularità sotto la categoria («cene» dentro «Ristoranti»): con il budget per categoria si risolve
  aggiungendo una categoria. Se l'utente vuole più voci nella stessa categoria, o il riporto, la tabella va
  ripensata con una riga per mese (tech-advisor, «se mi sbaglio»).
- Chi mette un budget su una categoria che contiene una ricorrente (affitto in «Casa») lo vede sforato subito:
  è coerente, ma va detto nella pagina del budget.
- Vincolo operativo dalla 2.3: aggiornare la PWA su tutti i dispositivi prima di creare il primo piano a rate.
- Fuori da Eton, gesti dell'utente: `~/.claude` con 42 commit da pubblicare, 21 segnalazioni aperte, la
  clausola 37 mai applicata (`storico/handoff/2026-09-20-configurazione-da-applicare.md`).
