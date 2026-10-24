# Fase 2.4 — budget mensili per categoria, e i residui dati della 2.3

Scritto il **24 ottobre 2026**. Nasce dalla richiesta del 24 ottobre e dalla sequenza scritta in
`2026-09-10-modello-prodotto-design.md` §6, «Richiesta e piano del 24 ottobre». Le posizioni tecniche sono
di `tech-advisor` (24 ott, verificate sul codice); le scelte di prodotto sono dell'utente, nei popup del
24 ottobre, e sono trascritte in `handoff/PIANO.md`, `DECISIONI`. Si appoggia ai design delle ricorrenti
(`2026-09-03-spese-ricorrenti-design.md`) e della 2.3 (`2026-10-20-rate-entrate-design.md`): il percorso
unico di lettura, la materializzazione, la concorrenza ottimistica e il bilancio restano come sono.

La richiesta, verbatim: *«prevedo di spendere 50 euro per uscite, ma non sono soldi ancora usciti quindi
vorrei che siano previsti ma poi ogni volta che spendo vado lì e dico quanto ho speso, così mi dice il
rimanente del budget che ho previsto di spendere»*.

## 1. Cosa si fa, e cosa no

| Si fa | Non si fa (dove va) |
|---|---|
| A — un budget mensile per categoria di spesa, con il residuo del mese | il riporto del residuo al mese dopo (scelta dell'utente: si azzera) |
| B — «registra una spesa» dal budget, con la categoria già scelta | voci sotto la categoria, più budget sulla stessa categoria |
| C — una riga «Da spendere nei budget» sotto il bilancio del mese | il saldo previsto di fine mese (funzione nuova, da chiedere a parte) |
| D — i residui dati della 2.3 (§5) | la resa grafica curata del budget (2.1-bis); budget sulle entrate |

## 2. Il modello

**Un budget è una regola che vale ogni mese finché non la chiudi**, non una riga per mese: «Uscite, 50 €
al mese, da ottobre». Vale per **una categoria di spesa esistente** (l'elenco chiuso delle categorie di
spesa, quello già usato dal Registro), in **uno spazio**. Una categoria ha al più **un** budget attivo per
spazio.

**Il residuo del mese** = importo del budget − somma delle spese di quella categoria nel mese, calcolata
sulle stesse righe che danno ogni altro totale del mese (le spese registrate e le previste già scadute; le
«in arrivo» restano fuori, come oggi). Ne segue, senza casi speciali:

- una spesa della categoria registrata da qualunque schermata — Registro, tabella, budget — consuma il
  budget;
- una spesa ricorrente della stessa categoria lo consuma quando la sua occorrenza è nel totale del mese (chi
  mette un budget su «Casa» con l'affitto dentro lo vede sforato: la pagina lo dice, §3);
- una spesa spostata di mese, o cambiata di categoria, sposta il consumo con sé, perché il residuo è
  calcolato e non salvato;
- le entrate non toccano mai un budget.

**Fine mese.** Il residuo non si riporta: ogni mese riparte dall'importo pieno. Uno sforamento si mostra
come residuo negativo, «sforato di X», e non blocca nessun inserimento.

**Cambiare un budget.** Si può cambiare l'importo (vale dal mese corrente in poi, i mesi passati si
ricalcolano con l'importo nuovo: nessuno storico degli importi, è il prezzo della forma «una regola»),
oppure chiuderlo (non vale più dal mese successivo). Concorrenza ottimistica con `version`, come le
ricorrenti.

## 3. Come si mostra (resa minima)

La grafica curata è della 2.1-bis; qui si usano i componenti e le classi che esistono già.

- **Pagina dei budget** dello spazio, raggiungibile dalle pagine delle spese: un elenco, un budget per riga,
  con categoria, importo, speso nel mese, residuo («restano X» oppure «sforato di X») e una barra
  proporzionale. Il mese mostrato è quello corrente, con lo stesso selettore di mese delle altre pagine
  delle spese se c'è già.
- **Nuovo budget**: categoria (solo categorie di spesa senza un budget attivo) e importo mensile.
- **«Registra una spesa»** su ogni riga: apre il modulo di inserimento già esistente con la categoria
  precompilata e la data di oggi; al salvataggio si torna al budget con il residuo aggiornato. È una spesa
  come le altre.
- Una riga di testo nella pagina avverte che le spese ricorrenti della categoria contano nel budget.
- **Sotto il bilancio del mese**, dove il bilancio c'è già: «Da spendere nei budget: X», dove X è la somma
  dei residui positivi del mese; con zero budget la riga non compare. Il bilancio resta com'è: X non entra
  in nessun totale.

## 4. Lo schema: una migrazione sola

Una migrazione, scritta e provata sul Supabase locale, **applicata a mano dall'utente**:

- tabella dei budget: id, spazio, categoria, importo mensile (`> 0`, al centesimo), primo mese di validità
  (primo giorno del mese), ultimo mese facoltativo, `version`, date di creazione e modifica; un indice
  unico che impedisce due budget attivi sulla stessa categoria nello stesso spazio; RLS, GRANT e trigger
  ricopiati da quelli delle ricorrenti (membri dello spazio);
- **il tetto alle rate** dalla 2.3: un check che limita il numero di rate di un piano a 60, come già fa il
  client;
- **nessun vincolo categoria↔tipo nel database** (ratificato il 24 ott: il controllo sta nel client, §5);
- la query di controllo che l'utente lancia dopo, con l'esito atteso scritto accanto.

## 5. I residui dati della 2.3

Dal `FUORI SCOPE` del rapporto `storico/handoff/2026-10-20-CHIUSURA-rate-entrate.md`:

1. **`CambiaCategoriaAsync` controlla il tipo**: una categoria di spesa non si assegna a un'entrata, né il
   contrario (ratificato il 24 ott: nel client, non nel database).
2. **Test dedicati a `RecurringExpenseRepository`**, che oggi non ha un file di test suo.
3. **La chiave d'ordinamento «senza accenti e maiuscole» in un posto solo** (oggi in tre copie, rilievo BE1
   della 02).
4. **I claims JWT degli script `supabase/verifica-rls-*.sql` in un posto solo** (rilievo BE1 della 03), se
   lo strumento con cui gli script si lanciano lo consente; altrimenti si dichiara perché no.

Il tetto alle rate è nella migrazione del §4.

## 6. Le unità, in sequenza

La partizione definitiva la decide lo scout; questa è quella attesa.

| Unità | Cosa |
|---|---|
| 01-migrazione | §4, scritta e provata in locale, non applicata |
| GATE | l'utente applica la migrazione e incolla l'esito della query di controllo |
| 02-budget | §2 e §3: modello, lettura e scrittura, calcolo del residuo, pagina, riga nel bilancio |
| 03-residui | §5, punti 1-4 |

## 7. Rischi

- **Il residuo si ricalcola sui mesi passati** se cambi l'importo: chi guarda settembre dopo aver alzato il
  budget a ottobre vede il residuo nuovo. Accettato con la forma «una regola»; se diventa un problema, la
  tabella passa a una riga per mese (posizione di tech-advisor, «se mi sbaglio»).
- **Budget e ricorrenti nella stessa categoria**: lo sforamento immediato è corretto ma può sorprendere; la
  pagina lo dice.
- **Dispositivi con la PWA vecchia**: la tabella nuova non rompe niente su un client vecchio, che
  semplicemente non la legge. Il tetto alle rate rifiuta solo ciò che il client già rifiuta.
