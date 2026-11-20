# Piano — fase 2.5: budget, seconda passata

## OBIETTIVO

2026-11-20 utente (chat), verbatim: «coordinatore, riprendi il lavoro che era rimasto da fare e lavora in autonomia
fino a completamento, se hai dubbi rivolgiti al techadvisor»

2026-11-12 utente (chat), verbatim, i due capoversi sui budget delle richieste del 12 novembre
(`docs/superpowers/specs/2026-09-10-modello-prodotto-design.md`, §6 «Richieste del 12 novembre 2026»):
«i budget non hanno una descrizione, ad esempio io imposto un budget per le uscite del mese di 200 euro, devo poter
segnare a cosa fa riferimento quel 200, inoltre sarebbe utile selezionare dalle voci di spesa già registrate quali
fanno parte di quel budget, visto che avevo già segnato quali erano le spese del mese prima che creassimo il budget.
il budget deve poter essere un budget che io mi fisso ogni mese come potenziale spesa, quindi ricorrente, ma
potrebbe essere un budget di spesa solo riferito ad un periodo di 1-x mesi.»

Il goal è il punto 3 della sequenza in vigore (spec di prodotto, §6 «Revisione del 12 novembre 2026»). Il piano e il
rapporto di chiusura della 2.4 sono in `storico/handoff/2026-10-24-PIANO-budget-2.4.md` e
`storico/handoff/2026-11-14-CHIUSURA-budget-2.4.md`.

## DECISIONI

- 2026-11-12 utente (chat, popup): «Resta la categoria (Consigliata)» — il budget resta legato alla categoria intera;
  si aggiungono la descrizione e una scorciatoia dal budget alla selezione delle spese in tabella.
- 2026-11-12 utente (chat, popup): «Stesso importo per N mesi (Consigliata)» — stesso importo ogni mese per N mesi,
  poi il budget finisce da solo; il residuo riparte pieno ogni mese.
- 2026-11-12 utente (chat, popup): «Ordine proposto (Consigliato)» — dopo le analisi viene la 2.5, poi «due persone
  vere», poi la 2.1-bis.
- 2026-11-20 «il lavoro rimasto» si legge come la 2.5 più i residui di budget della 2.4; «fino a completamento» vuol
  dire fino al goal chiuso della 2.5, non oltre («due persone vere» è un altro goal e tocca terzi). Posizione di
  tech-advisor — **da ratificare**. Raccomandazione: confermare.
- 2026-11-20 la descrizione è facoltativa, al più 200 caratteri; la durata si sceglie fra «ogni mese, finché non lo
  chiudo» e «per N mesi» con N da 1 a 24; il mese d'inizio resta come oggi — **da ratificare**. Raccomandazione:
  confermare.
- 2026-11-20 la migrazione porta anche il vincolo nel database contro due budget della stessa categoria sovrapposti
  nel tempo (estensione `btree_gist`): con la 2.5 i budget nascono già con una fine, e l'indice di oggi copre solo
  quelli aperti. È un'estensione nel database di produzione: il sì lo dà l'utente al GATE, e se dice no il blocco
  esce dalla migrazione senza toccare il resto — **da ratificare**. Raccomandazione: confermare.
- 2026-11-20 arrivando da «Registra una spesa» e scegliendo «a rate», dopo «Segna» si resta nel Registro (l'avviso
  del piano si legge solo lì): nessun codice — **da ratificare**. Raccomandazione: confermare.
- 2026-11-20 la guardia d'uscita dell'editor delle ricorrenti smette di contare la sola scelta della categoria come
  modifica, con la stessa regola già data al budget dalla 04 della 2.4 — **da ratificare**. Raccomandazione:
  confermare.
- 2026-11-20 nome del check della descrizione `category_budgets_description_max`, scelto dalla 01 sul modello di
  `recurring_expenses_installments_max` — **da ratificare**. Raccomandazione: confermare.
- 2026-11-20 dalla 02: in creazione di una ricorrente non conta come modifica nessuna scelta con un valore
  predefinito (categoria, cadenza, giorno, data, tipo), solo importo e descrizione scritti — la regola dell'editor del
  budget — **da ratificare**. Raccomandazione: confermare.
- 2026-11-20 le unità girano come side session nella cartella del progetto, una alla volta perché scrivono tutte;
  la 03 si pubblica solo a GATE superato, perché un push su `main` pubblica il sito e il client nuovo scrive la
  colonna nuova — scelta del coordinatore, **da ratificare**. Raccomandazione: confermare.

- 2026-11-20 utente (chat, popup, database): «Completa (Consigliata)» — migrazione con descrizione e vincolo sui
  periodi, estensione `btree_gist` compresa.
- 2026-11-20 utente (chat, popup, ratifiche): «Confermo tutte (Consigliata)» — lettura del «lavoro rimasto»,
  descrizione e durata, «a rate» resta nel Registro, guardia delle ricorrenti, unità in fila con la 03 pubblicata a
  GATE superato, archivio del rapporto della 2.4: **ratificate 2026-11-20** (le righe «da ratificare» qui sopra con
  data 2026-11-20, tranne il nome del check, che resta per il popup di fine goal).

## PARTIZIONE

| Unità | Cosa | Dove | Dipende da | Stato |
|---|---|---|---|---|
| **01-migrazione-periodo** | una migrazione additiva: colonna `description` (facoltativa, ≤ 200) con i privilegi di colonna, e in un blocco a sé `btree_gist` + vincolo di esclusione sui periodi, preceduto da un controllo che fallisce se in tabella ci sono già sovrapposizioni; script di collaudo RLS; provata sul Supabase locale, **non applicata** | side session `local_1ae7e8fa-1314-461c-80b5-916b69c89cbf`, avviata dall'utente il 2026-11-20, mandato in `handoff/01-migrazione-periodo/` | — | **FATTO** — commit 6b0f857 (resoconto 4c08daa); provata sul Supabase locale: 4 errori attesi su 4 nello script nuovo, 14 su 14 nella regressione della 2.4, il controllo preliminare ferma la migrazione su dati sovrapposti; la violazione torna da PostgREST come HTTP 400 con `code` 23P01 (contratto per la 03); BE2/BE3 di backend-expert fuori scope; verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 442 passed, 0 skipped` |
| **02-residui** | ScegliTipo che dopo Entrata→Spesa perde la categoria del budget; guardia d'uscita dell'editor delle ricorrenti; frase sulle ricorrenti nell'editor del budget; la decisione del ritorno ai budget dopo «Segna» estratta in una funzione pura e coperta da test | side session `local_7e70954d-f830-4431-a42a-5742d837c0be`, avviata dall'utente il 2026-11-20, mandato in `handoff/02-residui/` | 01 (solo ordine: chi scrive va in fila) | **FATTO** — commit 7d5ad61 (resoconto 532b827); 4 rilievi fondati corretti e verificati dal checker; prova nel browser 3/3 dopo il login dell'utente nel pannello, nessun dato salvato; resta per l'utente la metà della guardia delle ricorrenti con la finestra nativa (passi nel resoconto); verifica: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 446 passed, 0 skipped` |
| **GATE migrazione** | l'utente applica la migrazione in DBeaver (con il sì all'estensione) e incolla l'esito della query di controllo | utente | 01 | **SUPERATO** — 2026-11-20 l'utente ha incollato in chat l'esito della query di controllo: `colonna_presente=true, check_presente=true, grant_insert=true, grant_update=true, schema_estensione=extensions, vincolo_presente=true` (tutti uguali all'atteso) |
| **03-budget-periodo** | modello e repository con `description` ed `EndsOn`; editor con descrizione e durata; chiusura anticipata di un budget a periodo; messaggio proprio per il rifiuto del vincolo di sovrapposizione; elenco con descrizione e periodo; scorciatoia dal budget alla tabella del mese; test | side session, mandato in `handoff/03-budget-periodo/` | GATE, 02 | PIANIFICATA |
| **prova del capo** | prova nel browser sui dati di produzione con un sì nuovo dell'utente, poi ui-critic | coordinatore | 03 | PIANIFICATA |

## RAZIONALE

Una migrazione sola perché in produzione la applica a mano l'utente; il vincolo di sovrapposizione sta in un blocco
a sé, così un no all'estensione lo toglie senza rifare la migrazione. Il periodo di N mesi non vuole schema: le
colonne `starts_on`/`ends_on` esistono già. La 02 tocca pagine diverse da quelle della 03 tranne la frase
nell'editor del budget: per questo la 02 viene prima e la 03 parte da `main` dopo di lei. La 03 aspetta il GATE
perché il push su `main` pubblica, e il client nuovo scriverebbe una colonna che in produzione non c'è ancora.

## PROSSIMA AZIONE

2026-11-20 GATE SUPERATO e 02 recepita (perimetro verificato, riga `consentito` nel log). Prossimo: aprire la 03
(scheda proposta: il turno nasce dal messaggio della side). L'archivio della side 02 è stato rifiutato dall'app
(sessione aperta dall'utente): da archiviare a fine goal. Il campo `CAPO: eton 1` non risolve per le side: il
mandato della 03 lo dice come «la sessione che ti ha aperto».
(storico della riga precedente) 2026-11-20 01 FATTO e recepita (perimetro verificato, riga `consentito` nel log delle verifiche), side archiviata.
Prossimo: aprire la 02 (scheda proposta: il turno nasce dal messaggio della side) e, in parallelo, il popup del GATE
all'utente con lo SQL per DBeaver dal resoconto della 01, il sì all'estensione e le righe da ratificare.

controllo contesto 2026-11-20 (avvio): nulla

## APERTO

- Fuori da questo goal, per la 2.1-bis o per un'unità a sé: BE1/BE2/BE3 e C1 della 2.4, «In arrivo» che sparisce al
  cambio mese, tutta la resa («sforato» senza colore d'allarme, la y fissa di «Salva», «Da spendere nei budget» in
  grigio, residuo non distinto dal totale, binario della barra a 1,21:1), i calcoli del cruscotto.
- Dalla 01, fuori scope: script di collaudo che diano l'esito da soli invece del confronto a occhio (BE3, tutti gli
  `supabase/verifica-*.sql`, decisione dell'utente); nota di threat-hunter non provata e preesistente: la policy di
  modifica dei budget non chiede di essere ancora membro dello spazio, quindi un ex membro autore di un budget
  potrebbe forse ancora modificarlo. Da collocare con S1/S3 della sicurezza.
- Granularità sotto la categoria: con la categoria intera resta aggiungere una categoria.
- Vincolo operativo dalla 2.3: aggiornare la PWA su tutti i dispositivi prima di creare il primo piano a rate.
- I rilievi S1/S3 dell'analisi di sicurezza vanno collocati prima di «due persone vere» (goal loro).
- Il rapporto di chiusura della 2.4 (`storico/handoff/2026-11-14-CHIUSURA-budget-2.4.md`) è stato archiviato con il
  piano: se l'utente non l'aveva ancora letto, va segnalato nel primo popup.
- Fuori da Eton, gesti dell'utente: `~/.claude` con commit da pubblicare, segnalazioni aperte, la clausola 37 mai
  applicata (`storico/handoff/2026-09-20-configurazione-da-applicare.md`).
