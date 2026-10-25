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

## PARTIZIONE

| Unità | Cosa | Dove | Dipende da | Stato |
|---|---|---|---|---|
| **S — spec 2.4** | spec del budget (le decisioni qui sopra, la tabella, la resa minima, i casi limite: categoria cambiata, spesa spostata di mese, budget chiuso) e dei residui | `docs/superpowers/specs/2026-10-24-budget-design.md`, approvata in chat | — | **PARZIALE** — approvata dall'utente in chat il 24 ott («Approvo, vai»); è un documento, nessun comando lo verifica |
| **scout** | ricognizione per la partizione delle unità, sul mandato dell'indice di `architettura-sessioni.md` | subagent `scout` | S | **PARZIALE** — mappa ricevuta il 24 ott (tre gruppi, file contesi `ExpenseRepository.cs` e `CalcoliRicorrenti.cs`); è una ricognizione, nessun comando la verifica |
| **01-migrazione** | `category_budgets` con RLS + check `installments <= 60`, in una migrazione sola, scritta e provata sul Supabase locale, **non applicata** | sessione-unità, worktree `G:\Sviluppo\wt-Eton\01-migrazione`, mandato in `handoff/01-migrazione/` | scout | **FATTO** — fusa su `main` il 24 ott (merge 7876feb), provata sul Supabase locale (14 errori attesi su 14), BH1 (sovrapposizione di periodi) passato come regola al mandato della 02, tre scelte da ratificare nel resoconto; verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 417 passed, 0 skipped` |
| **GATE migrazione** | l'utente applica la migrazione dall'editor SQL e incolla l'esito della query di controllo | utente | 01 | **SUPERATO** — 2026-10-25 l'utente ha incollato in chat l'esito della query di controllo: `tabella_esiste=true, rls_attiva=true, policy=4, indice_aperto=true, trigger_versione=true, tetto_rate=true, max_rate=0` (atteso) |
| **02-budget** | modello, repository, calcolo del residuo, pagina minima dei budget, «registra una spesa» con categoria precompilata, riga nel bilancio | sessione-unità, worktree `G:\Sviluppo\wt-Eton\02-budget`, ramo `unita-02-budget` | GATE | **PARZIALE** — fusa su `main` il 25 ott; GATE dell'unità verdi (build 0 errori, test 0 failed / 432 passed); 6 rilievi fondati tutti corretti; **manca la prova nel browser (§7) e ui-critic**: live-testing sui dati di produzione rifiutato dal classificatore della modalità auto in `--bg`; passi della prova in `handoff/02-budget/resoconto.md`, FUORI SCOPE; cinque scelte da ratificare nel resoconto |
| **03-residui** | `CambiaCategoriaAsync` col tipo, test di `RecurringExpenseRepository`, chiave d'ordinamento in un posto, claims JWT degli script RLS in un posto | sessione-unità, worktree `G:\Sviluppo\wt-Eton\03-residui`, ramo `unita-03-residui` | 02 (solo ordine, file probabilmente contesi) | **FATTO** — fusa su `main` il 25 ott; script RLS provati sul Supabase locale prima/dopo con output identico; 5 rilievi fondati corretti, 2 test rossi confermati; tre scelte da ratificare nel resoconto, BE1 (un `Esito<T>` unico per sette repository) di progetto per l'utente; verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 442 passed, 0 skipped` |
| **04-correzioni-budget** | i difetti della prova nel browser della 02 (`handoff/02-budget/prova-capo.md`): barra invisibile, guardia d'uscita su «Nuovo budget» senza importo, mese e frecce con zero budget; poi i passi 9-10 della prova, rifatti da questa sessione | sessione-unità, worktree `G:\Sviluppo\wt-Eton\04-correzioni-budget`, ramo `unita-04-correzioni-budget`, mandato in `handoff/04-correzioni-budget/` | 03 (file della 02 su `main`) | **FATTO** (codice) — fusa su `main` il 25 ott; GATE dell'unità build 0 errori, test 442 passed; 1 rilievo fondato corretto; «In arrivo» facoltativo non fatto (codice nuovo); stesso difetto di guardia in `Pages/RicorrenteEdit.razor:388-389` fuori perimetro; la prova nel browser resta del capo (PROSSIMA AZIONE); verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 442 passed, 0 skipped` |

## RAZIONALE

Una migrazione sola perché in produzione le applica a mano l'utente: il tetto alle rate viaggia con la tabella
nuova. Lo scout (24 ott) conferma tre gruppi disgiunti salvo due file: `Services/ExpenseRepository.cs` (la 02
lo legge e chiama `CreaAsync`, la 03 cambia `CambiaCategoriaAsync`) e `Services/CalcoliRicorrenti.cs` (la 02
ne legge le occorrenze del mese, la 03 vi sposta la chiave d'ordinamento): contesa di file, non di firma, quindi
02 e 03 in sequenza e la 03 da `main` dopo la 02. Nessuna interfaccia nuova fra le due.

Scelte del capo per il mandato della 02 (annullabili con un revert, **da ratificare** a fine goal):
- 2026-10-24 la riga «Da spendere nei budget» va solo sotto il riepilogo del mese del Registro, non nel bilancio
  della tabella, che vale per un periodo filtrato e non per un mese — **da ratificare**. Raccomandazione:
  confermare.
- 2026-10-24 «Registra una spesa» apre il Registro con la categoria passata nell'indirizzo (oggi il modulo non
  riceve parametri) — **da ratificare**. Raccomandazione: confermare, è il modo già usato dalla tabella.
- 2026-10-24 risposta alla domanda della 01: autore del budget `created_by` (non `paid_by`); modifica ed
  eliminazione all'autore o al proprietario dello spazio, come le ricorrenti — **da ratificare**.
  Raccomandazione: confermare, è la copia delle regole già in produzione.
- 2026-10-25 risposta alla domanda della 03: il perimetro si allarga a `Services/RecurringExpenseRepository.cs`,
  solo per estrarre due punti statici `internal` (costruzione della regola nuova, decisione dell'esito con la
  rilettura come delegate) che rendono testabili senza database scritture e concorrenza; nessuna firma pubblica
  cambia — **da ratificare**. Raccomandazione: confermare, senza non si prova la concorrenza.
- 2026-10-24 la 03 porta in un posto solo anche la copia di ricerca della chiave (in `TabellaSpese`), se la sua
  semantica è identica — **da ratificare**. Raccomandazione: confermare.

## PROSSIMA AZIONE

04 fusa. Rifare da questa sessione la
prova nel browser del budget (passi della 02 più i `PASSI PER IL CAPO` della 04) sui dati di produzione, con
un sì nuovo dell'utente (quello del 25 ott valeva per la prima prova), poi ui-critic. Pulizia della prima
prova: SQL dato all'utente in chat il 25 ott, esito da ricevere. Le righe DA RATIFICARE della 01, della 02 e del
capo vanno nel popup di fine goal. Rilievi fuori scope della 02 per la 2.1-bis: commento di
`Shared/PaginaRegistro.cs` (C1), ripetizioni BE2/BE3, «In arrivo» che lampeggia al cambio mese.

controllo contesto 2026-10-24 (avvio): nulla

## APERTO

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
