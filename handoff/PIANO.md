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
  consumano il budget. Posizione di tech-advisor verificata sul codice — **da ratificare**. Raccomandazione:
  confermare, è l'unica forma senza doppia scrittura.
- 2026-10-24 il vincolo categoria↔tipo resta nel client (`CambiaCategoriaAsync` controlla il tipo), il database
  resta senza check, come scelto in `Services/CategorieSpesa.cs` — **da ratificare**. Raccomandazione:
  confermare; un check SQL rovescerebbe «una categoria nuova è una riga di C#».

## PARTIZIONE

| Unità | Cosa | Dove | Dipende da | Stato |
|---|---|---|---|---|
| **S — spec 2.4** | spec del budget (le decisioni qui sopra, la tabella, la resa minima, i casi limite: categoria cambiata, spesa spostata di mese, budget chiuso) e dei residui | `docs/superpowers/specs/2026-10-24-budget-design.md`, approvata in chat | — | PIANIFICATA |
| **scout** | ricognizione per la partizione delle unità, sul mandato dell'indice di `architettura-sessioni.md` | subagent `scout` | S | PIANIFICATA |
| **01-migrazione** | `category_budgets` con RLS + check `installments <= 60`, in una migrazione sola, scritta e provata sul Supabase locale, **non applicata** | sessione-unità | scout | PIANIFICATA |
| **GATE migrazione** | l'utente applica la migrazione dall'editor SQL e incolla l'esito della query di controllo | utente | 01 | PIANIFICATA |
| **02-budget** | modello, repository, calcolo del residuo, pagina minima dei budget, «registra una spesa» con categoria precompilata, riga nel bilancio | sessione-unità | GATE | PIANIFICATA |
| **03-residui** | `CambiaCategoriaAsync` col tipo, test di `RecurringExpenseRepository`, chiave d'ordinamento in un posto, claims JWT degli script RLS in un posto | sessione-unità | 02 (solo ordine, file probabilmente contesi) | PIANIFICATA |

La divisione 02/03 è provvisoria: la decide lo scout (regola 2 del capo).

## RAZIONALE

Una migrazione sola perché in produzione le applica a mano l'utente: il tetto alle rate viaggia con la tabella
nuova. I residui senza schema stanno in un'unità propria dopo il budget perché `ExpenseRepository` e i calcoli
delle spese sono probabilmente toccati da entrambe; lo conferma o lo smentisce lo scout.

## PROSSIMA AZIONE

Aprire una sessione nuova (`coordinatore` nella cartella del progetto): mostrare nel primo popup le due righe
**da ratificare** di `DECISIONI`, poi scrivere la spec S e farla approvare in chat.

## APERTO

- Granularità sotto la categoria («cene» dentro «Ristoranti»): con il budget per categoria si risolve
  aggiungendo una categoria. Se l'utente vuole più voci nella stessa categoria, o il riporto, la tabella va
  ripensata con una riga per mese (tech-advisor, «se mi sbaglio»).
- Chi mette un budget su una categoria che contiene una ricorrente (affitto in «Casa») lo vede sforato subito:
  è coerente, ma va detto nella pagina del budget.
- Vincolo operativo dalla 2.3: aggiornare la PWA su tutti i dispositivi prima di creare il primo piano a rate.
- Fuori da Eton, gesti dell'utente: `~/.claude` con 42 commit da pubblicare, 21 segnalazioni aperte, la
  clausola 37 mai applicata (`storico/handoff/2026-09-20-configurazione-da-applicare.md`).
