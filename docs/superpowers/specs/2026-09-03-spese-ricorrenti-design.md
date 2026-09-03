# Spese ricorrenti — design

Scritto il **3 settembre 2026**, approvato dall'utente lo stesso giorno. È il primo dei tre
lavori in cui è stata decomposta la richiesta «gestione spese più completa»: ricorrenti,
poi vista tabellare, poi analisi. L'ordine è deciso e motivato al §2.

Presuppone il design delle spese del 24 agosto (`2026-08-24-spese-design.md`), di cui
rispetta tutte le scelte portanti: calcoli nel client, migrazioni applicate a mano in
produzione, nessun server applicativo, solo euro.

---

## 1. Che cosa si costruisce

Una **regola ricorrente** è la dichiarazione che una spesa si ripete: «120 € di palestra,
categoria Salute, il 5 di ogni mese, da settembre 2026». Da essa nascono **occorrenze**, che
sono righe `expenses` normali — modificabili, cancellabili e visibili come tutte le altre.

Il criterio che governa tutto il resto:

> **Il passato si materializza, il futuro si calcola.**
> Un mese trascorso è un fatto e sta su disco. Un mese a venire è una previsione e resta una
> regola.

Le due alternative sono state considerate e scartate. *Materializzare tutto* obbligherebbe a
riscrivere N righe future a ogni cambio d'importo, e a decidere cosa fare del passato quando
la regola cambia — ma il passato non si riscrive, quei soldi sono usciti davvero.
*Tenere solo la regola* renderebbe impossibile correggere una singola occorrenza (l'affitto
che un mese aumenta di 20 €, la palestra saltata a settembre) e costringerebbe ogni
aggregazione a fondere due sorgenti diverse.

## 2. Perché questo lavoro viene per primo

È l'unico dei tre che tocca lo schema, e in questo progetto lo schema si applica **a mano in
produzione prima del push**. Tutto il resto legge righe, e la forma della riga la decide
questo lavoro: `recurring_id`, e la distinzione fra «registrata» e «prevista». Costruire la
vista tabellare prima significherebbe rifarne le righe dopo.

L'analisi, che viene per terza, senza le ricorrenti darebbe un quadro falso: la spesa fissa è
proprio quella che pesa, e distinguerla dalla variabile è l'unica cosa che aggiunge
informazione rispetto a ciò che l'app già mostra.

## 3. Il modello dati

### 3.1 La tabella delle regole

Fotocopia strutturale di `expenses`, come `expenses` lo fu di `notes`. Stessi `check` di
lunghezza su `description` e `category`, stesso trigger di `version`/`updated_at`, stessa
forma di RLS e di grant per colonna.

```sql
create table if not exists public.recurring_expenses (
    id           uuid primary key default gen_random_uuid(),
    space_id     uuid not null references public.spaces (id) on delete cascade,
    paid_by      uuid not null default auth.uid() references auth.users (id) on delete cascade,
    amount       numeric(12,2) not null check (amount > 0),
    description  text not null check (length(btrim(description)) between 1 and 200),
    category     text not null check (length(btrim(category)) between 1 and 40),
    every_months smallint not null default 1 check (every_months between 1 and 12),
    day_of_month smallint not null check (day_of_month between 1 and 31),
    starts_on    date not null,
    ends_on      date null,
    materialized_through text null,   -- 'yyyy-MM' dell'ultimo periodo generato
    version      integer not null default 1,
    created_at   timestamptz not null default now(),
    updated_at   timestamptz not null default now()
);
```

**`every_months` è un intero, non un `interval` e non un enum.** Nessun tipo .NET
rappresenta un intervallo in mesi (`TimeSpan` non ha i mesi), e un `interval` attraverserebbe
Newtonsoft sotto `TrimMode=full` — la stessa classe di rischio per cui `Models/Expense.cs`
ha già scartato `DateOnly`. Un enum testuale sarebbe peggio di un intero: `'quarterly'` è un
dato che il generatore dovrebbe comunque ritradurre in `3`. Il database non fa aritmetica sui
periodi: la fa il client.

Con `every_months >= 1` c'è **al massimo un'occorrenza per mese**, e questo è ciò che
permette al periodo di restare `'yyyy-MM'` e alla chiave di idempotenza di non cambiare mai.
È anche il motivo per cui il settimanale non entra: si ancora al giorno della settimana, non
del mese, e romperebbe quella chiave.

**`day_of_month` arriva a 31 e si tronca in lettura.** Il limite a 28 eviterebbe febbraio
vietandolo, ma «il 30» è il giorno più comune dopo il primo per affitti e stipendi: sarebbe
un rifiuto, non una semplificazione. Il troncamento è
`Math.Min(day_of_month, DateTime.DaysInMonth(anno, mese))`, e `DaysInMonth` è già la
convenzione della casa (`Pages/Spese.razor:379`). `31` diventa così «ultimo giorno del mese»
senza bisogno di un flag dedicato; nel modulo serve una riga d'aiuto che lo dica.

**`ends_on` invece del DELETE.** Terminare una regola scrive una data; non la cancella. Se si
cancellasse, i mesi passati perderebbero la provenienza delle loro righe, e con essa
l'analisi fisso/variabile. Vedi §6.3.

### 3.2 Le due colonne su `expenses`

```sql
alter table public.expenses
    add column if not exists recurring_id     uuid references public.recurring_expenses (id),  -- on delete no action, il default
    add column if not exists recurring_period date;

alter table public.expenses
    add constraint expenses_recurring_period_uq unique (recurring_id, recurring_period);
```

`recurring_id` è **provenienza, non vincolo**: l'occorrenza resta una spesa come le altre, e
il pagante la modifica in `SpesaEdit` senza saperne nulla.

**La FK è `on delete no action`** — il default, scritto qui per non lasciarlo implicito — e
il §6.3 spiega perché non è né `set null` né `restrict`.

Entrambe le colonne vanno nel `grant insert` e **nessuna delle due** nel `grant update`; il
trigger `handle_expense_update` le rimette a forza ai valori precedenti, esattamente come fa
già con `paid_by`, `space_id` e `created_at`.

Il vincolo `unique` accetta più righe con `recurring_id` nullo, perché in PostgreSQL i NULL
sono distinti fra loro: le spese inserite a mano non collidono.

## 4. Il generatore

### 4.1 Due domande diverse, due meccanismi diversi

È il punto su cui il design si regge o crolla, e la prima stesura lo aveva sbagliato.

| Domanda | Chi risponde |
|---|---|
| «Questa occorrenza **esiste** già?» | il vincolo `unique (recurring_id, recurring_period)` |
| «Questa occorrenza **deve** esistere?» | il watermark `materialized_through` |

Senza il watermark, cancellare un'occorrenza non funziona: l'utente cancella la palestra di
settembre, alla riapertura il generatore non la trova, il vincolo non ha niente da bloccare,
e la riscrive. L'app ignorerebbe un'azione esplicita dell'utente — e il difetto si
manifesterebbe *dopo*, alla prossima apertura, quindi quasi impossibile da attribuire.

Con il watermark il generatore produce **solo i periodi successivi all'ultimo materializzato**
e non riempie mai i buchi dietro. I buchi dietro sono decisioni dell'utente.

Il vincolo `unique` resta, ma con un mestiere diverso: arbitra le **corse** fra due schede o
due dispositivi dello stesso utente che aprono l'app insieme.

### 4.2 Il calcolo è puro

Una classe statica accanto a `CalcoliSpese`: nessuna rete, nessuno stato, testabile senza
database come i 144 test già esistenti sulle spese.

Un mese è dovuto se la distanza in mesi da `starts_on` è multipla di `every_months`, se il
periodo è successivo a `materialized_through`, e se `ends_on` è nullo o non ancora superato.
La data dell'occorrenza è il giorno troncato del §3.1.

### 4.3 La scrittura

Il client materializza **solo le regole di cui è il pagante**: lo impone la policy RLS di
`expenses`, che richiede `paid_by = auth.uid()` in inserimento. Non è una scelta di design, è
il vincolo di sicurezza esistente.

La scrittura usa `Upsert` con `DuplicateResolution.IgnoreDuplicates` — **mai
`MergeDuplicates`**, che è un UPDATE e sovrascriverebbe con l'importo della regola una spesa
che il pagante aveva già corretto a mano. Dopo la materializzazione, il client avanza
`materialized_through` sulla regola.

Non si usa `pg_cron`: non è abilitato nel progetto, `supabase db reset` non lo eserciterebbe,
e girerebbe come `postgres` bypassando la RLS per scrivere righe utente — un percorso di
fiducia nuovo. Non si usa una RPC `SECURITY DEFINER`: sposterebbe la matematica delle date in
SQL, dove `Eton.Tests` non arriva e dove si corregge solo con una migrazione incollata a mano
in produzione. Il caso che entrambe risolverebbero — il membro che vede la spesa di un altro
prima che quello apra l'app — è coperto dal §5.

## 5. Il previsto, e l'unico divieto architetturale

**Deciso dall'utente il 3 settembre.** Le occorrenze **scadute e non ancora scritte** entrano
nel totale, marcate «previsto» nel registro e senza collegamento. Le occorrenze **future** si
mostrano come «in arrivo» e **non** entrano nel totale.

Il criterio: *il totale è ciò che esisterebbe se ogni pagante avesse aperto l'app*. È l'unica
delle due opzioni per cui due membri dello stesso spazio vedono lo stesso numero nello stesso
momento. Il prezzo, accettato consapevolmente: un totale può includere soldi non ancora
usciti davvero — se l'affitto è in ritardo, il totale dice che è stato pagato.

Le future restano fuori perché il totale della pagina è «quanto si è speso», non una
previsione — e sono virtuali per tutti, quindi non creano divergenza in nessun caso.

> **Divieto, da scrivere in ogni brief che tocchi la lettura delle spese.**
> Esiste **un solo percorso di lettura**, che fonde righe vere e occorrenze previste.
> Oggi `ExpenseRepository.ElencaAsync` ha due chiamanti — `Pages/Spese.razor` e
> `Pages/Home.razor` — e ne arriveranno altri due con la vista tabellare e con l'analisi. Se
> anche **uno solo** continua a chiamare `ElencaAsync` direttamente, la divergenza dei totali
> rientra dalla porta di servizio, e nessun test la intercetta.

## 6. Le schermate

### 6.1 Elenco delle regole

Una pagina-registro costruita su `Shared/PaginaRegistro.cs`, come Note e Collezioni. Ogni
riga mostra descrizione, importo, categoria, cadenza in parole («ogni 2 mesi, il 5») e la
**prossima occorrenza** («prossima: 1 ottobre»). Le regole terminate si distinguono da quelle
attive.

### 6.2 Editor di una regola

Costruito su `Shared/PaginaEditor.cs`, quindi con la guardia di uscita, la testata e il gate
di «Chiudi» che le unità 03-07 hanno standardizzato. Riusa `SchedaConflitto` e
`ConfermaAzione`. La cadenza è una `select` a cinque voci — mensile, ogni 2, ogni 3, ogni 6,
annuale — non un numero libero.

Due frasi che il modulo deve dire, perché il modello le implica e l'utente non può dedurle:

- «L'importo è quello atteso: se una bolletta arriva diversa, la correggi sulla spesa del
  mese.»
- «Le spese già segnate non cambiano.»

### 6.3 Terminare, non eliminare

L'azione in primo piano è **«Termina»**, che scrive `ends_on`. Il DELETE resta lecito solo per
una regola che non ha mai generato occorrenze, e a impedirlo negli altri casi è la chiave
esterna, non un controllo nel client.

**Tre comportamenti possibili della FK, e perché si sceglie il terzo.**

- `on delete set null` lascerebbe cancellare la regola: le occorrenze sopravvivrebbero come
  spese normali, ma perderebbero la provenienza — e con essa la distinzione fisso/variabile
  su tutti i mesi passati, che è il cuore del terzo lavoro. Il dato perso non è
  recuperabile.
- `on delete restrict` impedirebbe la cancellazione, ma controlla **subito**: e poiché
  `spaces → expenses` e `spaces → recurring_expenses` sono entrambe `cascade`, eliminare uno
  spazio fallirebbe, perché al momento del controllo le occorrenze esistono ancora.
- **`on delete no action`** — il default — impedisce la cancellazione controllando **a fine
  istruzione**, quando le righe figlie cascadate sono già sparite. Eliminare uno spazio
  funziona; eliminare una regola che ha generato occorrenze no, ed è ciò che si vuole.

Conseguenza da accettare consapevolmente: **una regola che ha generato anche una sola
occorrenza non si può più eliminare**, si può solo terminare. È il prezzo di non perdere la
storia, e l'interfaccia deve dirlo invece di lasciar sbattere l'utente contro un errore del
database.

### 6.4 Navigazione

Con questo lavoro e la vista tabellare le pagine delle spese diventano quattro, mentre la
barra di navigazione ha cinque posti già occupati. Serve una sotto-navigazione in testa alle
spese («Registro · Tabella · Ricorrenti»). La forma esatta è preferenza dell'utente e va
decisa quando arriva la tabellare, non ora.

## 7. Sicurezza

RLS e grant sono la fotocopia di `expenses`, che è il pattern già collaudato da
`verifica-rls-spese.sql`:

- `select` per i membri dello spazio (`is_space_member`);
- `insert` per i membri, con `paid_by = auth.uid()` come rete di sicurezza ridondante
  rispetto al `default`;
- `update` e `delete` per il pagante o per chi possiede lo spazio;
- `revoke all` seguito da grant espliciti; **mai** `version`, `created_at`, `updated_at` fra
  le colonne concesse.

`materialized_through` è concesso in update: lo scrive il client dopo aver materializzato.
Non è un dato sensibile — al peggio, un valore sbagliato produce occorrenze mancanti o
ripetute per la **propria** regola, e le ripetute le ferma comunque il vincolo `unique`.

Serve un `supabase/verifica-rls-ricorrenti.sql` sul modello degli altri cinque, che
**inserisca come inserisce il client** — con tutte le colonne che il modello invia. È la
lezione del difetto trovato il 3 settembre sulle collezioni: uno script che collauda un
percorso diverso da quello dell'applicazione può restare verde mentre la funzione è rotta.

Il test statico `Eton.Tests/PrivilegiInsertTests.cs` copre già automaticamente la nuova
tabella: confronta per riflessione le colonne che ogni modello invia in INSERT con i grant
estratti dalle migrazioni. Se il grant dimenticasse una colonna, fallirebbe subito.

## 8. Test

Nella tradizione del progetto: si testa la **logica pura**, non il repository né le pagine.

- il generatore: mese dovuto e non dovuto per ogni valore di `every_months`; troncamento del
  giorno su febbraio, su un mese da 30 e su uno da 31; rispetto di `starts_on` e di
  `ends_on`; **il buco non si riempie** (occorrenza cancellata, watermark oltre, nessuna
  rigenerazione) — è il test che protegge il §4.1;
- la fusione previsto/reale: che un'occorrenza materializzata **sostituisca** la sua
  previsione e non si sommi ad essa; che le future non entrino nel totale;
- il calcolo della prossima occorrenza mostrata nell'elenco.

## 9. Come questo design può sbagliare

- **Se qualcuno legge le spese senza passare dal percorso unico** (§5), i totali divergono e
  nessun test lo nota. È il rischio più alto perché è per omissione.
- **Se `Upsert` viene usato con `MergeDuplicates`**, le correzioni manuali alle occorrenze
  vengono sovrascritte alla riapertura successiva. Silenzioso e distruttivo.
- **Se il watermark non viene avanzato** dopo la materializzazione, ogni apertura ritenta gli
  stessi inserimenti: il vincolo li ferma, quindi nessun danno ai dati, ma il costo di rete
  cresce a ogni mese che passa.
- **Se l'utente vuole vedere solo ciò che è stato pagato davvero**, la scelta del §5 cade e i
  totali divergeranno per costruzione. La risposta onesta a quel punto non è una RPC, è
  mostrare «di cui 3 previste» accanto al totale.

## 10. Fuori scope, e cosa costerebbe dopo

| Cosa | Perché non ora | Prezzo del rinvio |
|---|---|---|
| **Importi variabili** | già coperto: l'occorrenza è una spesa normale e il pagante la corregge | piccolo — un booleano additivo «importo da confermare» |
| **Notifiche e promemoria** | senza server servono Web Push, chiavi VAPID, `pg_cron` e una tabella di sottoscrizioni: è un sottosistema, e su iOS in PWA è limitato | grande, ma ortogonale: niente da preparare oggi. Il promemoria della v1 è la riga «in arrivo» |
| **Sospensione temporanea** | «Termina» più una regola nuova la copre | ~20 righe: una colonna `paused_until`, una condizione nel generatore, tre test |
| **Periodicità settimanale** | si ancora al giorno della settimana e romperebbe la chiave `'yyyy-MM'` | una migrazione più 60-80 righe di C# |
| **Previsione oltre la prossima occorrenza** | la v1 mostra «prossima: 1 ottobre» e basta | zero schema: la stessa funzione pura su un intervallo più largo |
| **Propagazione delle modifiche alle occorrenze passate** | il passato non si riscrive, ed è il criterio del §1 | zero schema: un'azione di interfaccia su righe già modificabili |
| **Divisione fra membri, valute** | già fuori dal design del 24 agosto | invariato rispetto a quanto lì dichiarato |
