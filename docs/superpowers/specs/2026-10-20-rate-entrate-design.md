# Fase 2.3 — doppioni, rate, ricorrenti divise, quota da accantonare, entrate e bilancio

Scritto il **20 ottobre 2026**. Nasce dalle richieste del 19 ottobre
(`2026-09-10-modello-prodotto-design.md` §6) e dal perimetro scelto dall'utente il 20 ottobre
(«Tutta la 2.3»). Le posizioni tecniche sono di `tech-advisor` (20 ott, verificate sul codice); le
quattro scelte di prodotto sono dell'utente, nel popup del 20 ottobre. Si appoggia al design delle
ricorrenti (`2026-09-03-spese-ricorrenti-design.md`): il percorso unico di lettura, la
materializzazione e la concorrenza ottimistica restano come sono.

## 1. Cosa si fa, e cosa no

| Si fa | Non si fa (dove va) |
|---|---|
| 0 — i doppioni della lettura a blocchi | l'incrocio categorie × mesi, il fisso/variabile (2.3 «Analisi», goal futuro) |
| 1 — spese a rate, create dal modulo del Registro | trasformare in rate una spesa già scritta (scelta dell'utente) |
| 2 — ricorrenti divise in gruppi, ciascuno con il suo totale | la resa grafica della tabella e delle pagine (2.1-bis) |
| 3 — quota mensile da accantonare, nella pagina Ricorrenti | la quota in Home (2.1-bis, che rifà la Home) |
| 4 — entrate, anche ricorrenti, e bilancio del mese | entrate a rate; modifiche di un piano a rate avviato oltre a Termina, descrizione e categoria |

## 2. I doppioni (punto 0)

La lettura a blocchi da 1000 righe usa un offset. Se un altro membro scrive fra due blocchi, la
stessa spesa può tornare due volte: i totali la contano doppia, e la griglia avrebbe due righe con la
stessa `@key`. **Rimedio**: in fondo alla lettura le righe si deduplicano per `Id` (la prima
occorrenza vince). Le spese previste hanno `Id` nuovi e una chiave `recurring_id:periodo` unica per
costruzione, quindi dopo la deduplicazione nessuna chiave si ripete. La riga *saltata*, cioè il caso
opposto, resta accettata com'è documentata dalla 2.2-E. Test: una lettura simulata con una riga
ripetuta fra due blocchi restituisce la riga una volta sola.

## 3. Lo schema: una migrazione sola

Una migrazione, `supabase/migrations/2026102X000000_rate_entrate.sql` (data del giorno in cui si
scrive). **La applica l'utente dall'SQL editor**, dopo l'unità che la scrive e **prima del push della
prima unità che la usa**.

- `expenses.kind` e `recurring_expenses.kind`: `text not null default 'expense'` con il vincolo
  `check (kind in ('expense','income'))`. Il dato si scrive solo alla creazione: è concesso in INSERT,
  non in UPDATE, e il trigger di aggiornamento delle spese lo rimette a `old.kind`, come già fa con
  `recurring_id`. Un movimento non cambia tipo: per cambiarlo lo cancelli e lo riscrivi.
- `recurring_expenses.installments smallint null` (numero di rate) e
  `recurring_expenses.plan_total numeric(12,2) null` (totale del piano). Vincoli: entrambi nulli o
  entrambi valorizzati; `installments >= 2`; `plan_total >= amount`; con `installments` valorizzato,
  `kind = 'expense'` ed `every_months = 1`. Concessi in INSERT e in UPDATE. L'UPDATE esiste per la
  modifica di un piano non ancora partito; il blocco a piano avviato lo fa il client (§4.3).
- Il vincolo `amount > 0` resta: le entrate sono importi positivi con un tipo, non importi negativi.

**Compatibilità.** Tutte le colonne nuove hanno un valore di default o possono restare vuote. Un
client già pubblicato, che non manda `kind`, scrive quindi una spesa: lo schema nuovo con il client
vecchio è sicuro. L'inverso no: il client nuovo con lo schema vecchio fallisce all'INSERT, ed è il
motivo del passaggio dell'utente. Resta un effetto transitorio accettato: una PWA non ancora
aggiornata somma le entrate come spese finché non si ricarica.

## 4. Le rate (punto 1)

### 4.1 Come si crea

Nel modulo del Registro, solo quando il tipo è «Spesa», c'è un interruttore **«a rate»**. Acceso,
mostra il campo **numero di rate** (da 2 a 60). Premendo «Segna» con l'interruttore acceso **non** si
crea una spesa: si crea una regola ricorrente con questi valori.

- `every_months = 1`, `day_of_month` = il giorno della data del modulo, `starts_on` = la data del
  modulo, `ends_on` = `starts_on` + (N−1) mesi;
- `amount` = totale / N, arrotondato a 2 decimali per difetto; `plan_total` = il totale;
  `installments` = N.

La data del modulo **è la data della prima rata**: oggi per Scalapay, il mese dopo per Cofidis, che
resta «in arrivo». Non serve un campo apposta. Dopo la creazione il mese si rilegge, e il percorso
unico materializza subito la prima rata se la sua data è passata o è oggi. Un totale che, diviso per N,
dà un importo sotto 0,01 € si rifiuta nel modulo, con il messaggio accanto al campo.

### 4.2 Come si mostra

L'occorrenza k di un piano si chiama «<descrizione> · rata k/N», e il suffisso si salva nella spesa
materializzata: il passato non si riscrive. L'ultima rata (k = N) vale
`plan_total − amount × (N−1)`, così i centesimi avanzati finiscono lì. Il calcolo sta in **un punto
solo**, la funzione che costruisce l'occorrenza da una regola: previsione e riga materializzata
coincidono senza un percorso nuovo.

### 4.3 Cosa si può cambiare

Si guarda se il piano è partito, cioè se almeno una rata è già stata materializzata.
- **Prima della prima rata**: si possono cambiare totale, numero di rate, data, descrizione e
  categoria; importo e data di fine si ricalcolano.
- **Dopo**: solo **Termina** (chiude il piano in anticipo, per un'estinzione), descrizione e categoria.
  Gli altri campi si vedono ma non si modificano, con una riga che spiega perché.
- Elimina: come per ogni ricorrente, la cancellazione è già limitata dal vincolo sulle spese collegate.

## 5. Le ricorrenti divise, e la quota da accantonare (punti 2 e 3)

La pagina Ricorrenti mostra le regole in gruppi, in quest'ordine, ciascuno con il proprio totale
mensile:

1. **Entrate** — regole `kind = 'income'`;
2. **Mensili** — spese con `every_months = 1`, piani a rate esclusi;
3. **A rate** — piani a rate, con «rata k/N» e il residuo da pagare;
4. **Annuali** — `every_months = 12`;
5. **Ogni N mesi** — le altre cadenze, con la cadenza scritta per ciascuna;
6. **Terminate** — le regole senza una prossima occorrenza, in fondo e fuori dai totali.

Dentro ogni gruppo l'ordine è per descrizione. Un gruppo vuoto non si mostra. Sostituisce l'ordine
attuale, che è per ultima modifica.

**La quota da accantonare** è la somma di `amount / every_months` di tutte le spese ricorrenti non
terminate, non mensili e non a rate. Comprende quelle che non sono ancora partite: si mette da parte
prima. Si mostra in testa alla pagina, con una riga sola: «Per annuali e periodiche, metti da parte X €
al mese». Il calcolo è puro, sta accanto agli altri calcoli delle ricorrenti e ha i suoi test.

## 6. Entrate e bilancio (punto 4)

### 6.1 Il tipo

`Expense` e `RecurringExpense` acquistano `Kind`, scritto solo in creazione. Le categorie delle
entrate sono un elenco proprio, scelto dall'utente: **Stipendio, Rimborso, Regalo, Vendita, Altro**.
Una categoria si accetta se appartiene all'elenco del proprio tipo.

### 6.2 Dove si inseriscono

- **Registro**: il modulo ha in testa una scelta «Spesa / Entrata», con «Spesa» preselezionata. La
  scelta cambia l'elenco delle categorie e nasconde «a rate» per le entrate.
- **Ricorrenti**: l'editor di una regola nuova ha la stessa scelta. Una regola esistente mostra il suo
  tipo senza poterlo cambiare. Lo stipendio è un'entrata ricorrente qualsiasi.
- **Editor di una spesa esistente**: mostra il tipo, senza poterlo cambiare.

### 6.3 Dove si contano

Oggi **niente distingue il segno**: i totali per mese e il riepilogo della griglia sommano ogni
importo, quindi un'entrata finirebbe nel totale delle spese. Vanno quindi toccati tutti i punti dove
si somma.

- **Il calcolo del mese** separa le due somme: *Spese* (solo `expense`), *Entrate* (solo `income`) e
  *Saldo* = Entrate − Spese. Le ripartizioni per categoria e i grafici restano **solo spese**.
- **Registro e Home** mostrano accanto al totale del mese le entrate e il saldo, con il segno. Nel
  registro una riga di entrata si distingue con un «+» davanti all'importo e un'etichetta «Entrata».
  Nessuno stile nuovo: la resa è provvisoria fino alla 2.1-bis.
- **Tabella**: colonna **Tipo**, filtro per tipo nell'URL come gli altri filtri, e il CSV la porta da
  sé. Il riepilogo sotto le colonne dà Spese, Entrate e Saldo delle righe filtrate o selezionate,
  invece di una somma unica.
- **Le previste** (le occorrenze future delle ricorrenti) seguono le stesse regole: un'entrata
  ricorrente in arrivo è un'entrata in arrivo.

## 7. Le unità, in sequenza

| Unità | Cosa | Schema |
|---|---|---|
| **U1** | punto 0, i doppioni | no |
| **U2** | punti 2 e 3, i gruppi e la quota, senza tipo né rate | no |
| **U3** | la migrazione del §3, scritta e **non applicata**; poi il passaggio dell'utente | sì |
| **U4** | punto 1, le rate: modello, calcolo dell'occorrenza, editor delle ricorrenti, modulo del Registro, gruppo «A rate» | usa U3 |
| **U5** | punto 4, entrate e bilancio, compresi il gruppo «Entrate» e la scelta del tipo negli editor | usa U3 |

U1 e U2 si pubblicano subito. U4 e U5 si pubblicano solo dopo che l'utente ha applicato la migrazione.
I file che più unità toccano — il modulo del Registro, il modello delle regole, l'editor delle
ricorrenti, la funzione dell'occorrenza — passano da un'unità alla successiva **in sequenza**, mai in
parallelo.

## 8. Rischi

- **Una somma dimenticata.** Se un punto che somma importi non viene toccato dalla U5, un'entrata si
  conta come spesa senza alcun errore visibile. Il resoconto della U5 deve elencare tutti i punti
  dove si sommano importi e dire, per ciascuno, come è stato trattato.
- **Una PWA non aggiornata** somma le entrate come spese finché non si ricarica: transitorio, accettato.
- **L'ultima rata** con N grande e un totale piccolo: il vincolo «importo per rata almeno 0,01 €»
  garantisce che l'ultima rata sia positiva, perché il resto è sempre ≥ 0.
- **Le prove nel browser** girano sulla produzione: entrate, piani a rate e regole di prova si chiamano
  «PROVA COLLAUDO …» e si eliminano a fine prova. Un piano a rate di prova parte il mese prossimo, così
  non materializza nulla e resta eliminabile.
