# Piano — spese ricorrenti (2.1) e vista tabellare (2.2)

Scritto il **22 settembre 2026**. Sostituisce il piano del 20 settembre, il cui goal è chiuso e
archiviato in `storico/handoff/2026-09-20-PIANO-tutto-cio-che-rimane.md`; il suo rapporto è ancora
in `handoff/CHIUSURA.md` e **non si archivia finché l'utente non l'ha letto**.
Autosufficiente: chi riprende non ha bisogno della conversazione da cui nasce.

## OBIETTIVO

> «vorrei fare sia la 2.1 che la 2.2 o almeno fare totalmente la loro progettazione»
> — utente, 22 settembre 2026

**La glossa.** Le fasi sono quelle di `docs/superpowers/specs/2026-09-10-modello-prodotto-design.md`
§4: **2.1** spese ricorrenti (design e piano da sei task già approvati il 3 settembre), **2.2** vista
tabellare delle spese (da progettare). Il minimo dichiarato dall'utente è la **progettazione totale di
entrambe**: per la 2.1 esiste già e va solo corretta; per la 2.2 vuol dire **spec e piano**. Il massimo
è entrambe implementate.

⚠️ **Tutto in una sessione non ci sta**, e lo si sa adesso: sei task della 2.1 più la 2.2, con in mezzo
un passo che solo l'utente può fare (la migrazione in produzione). Il gate della migrazione è il
confine naturale fra un capo e il successivo.

## DECISIONI

- **2026-09-22 utente (chat)** — alla domanda su come trattare la 2.2, con la posizione di
  `tech-advisor` nel popup, ha scelto: **«2.2 completa ora (consigliata)»**. Conseguenze:
  1. **La fase 2.1-bis (UI/UX, direzione SLY) si sposta DOPO la 2.2**, restando prima della fase 3.
     Revoca la collocazione del 19 settembre («fra 2.1 e 2.2»), che era a confidenza media e portava
     scritto l'obbligo di essere riconfermata. Motivo di `tech-advisor`: la tabellare è la schermata
     più densa dell'app, e un sistema visivo disegnato senza averla davanti si rifà quando arriva;
     restilizzarla dopo costa ore, aspettare costa settimane.
  2. La 2.2 si implementa con **resa visiva minima sui token esistenti, dichiarata provvisoria**.
  3. Ordine: **spec 2.2 → 2.1 implementata → piano e implementazione 2.2 → (goal futuro) 2.1-bis.**
     La spec 2.2 va **prima** del task 5 della 2.1, che altrimenti dovrebbe decidere alla cieca la
     sotto-navigazione «Registro · Tabella · Ricorrenti».

- **2026-09-22 utente (chat)** — cosa vuole dalla tabella, scelte tutte e quattro le voci proposte:
  trovare e ordinare (periodo libero, filtri, ordinamento per colonna), modificare nella riga, azioni
  su più righe, totali della selezione ed esportazione CSV. E in più, verbatim: *«vorrei anche avere
  una gestione piu tabellare in stile excel con delle funzioncine, magari solo lato desktop, mentre
  lato mobile segno solo le mie spese quando le faccio tieni sempre a mente l'obbiettivo del
  progetto»*.

- **2026-09-22 utente (chat)** — le «funzioncine»: scelte tutte e quattro — riepilogo sotto le
  colonne, raggruppamento con subtotali, incrocio categorie × mesi (è la 2.3), formule scritte
  dall'utente (è la fase 4). E un'idea nuova, verbatim: *«implementerei anche la possibilità di
  connettere una propria ai a eton in generale che possa leggere i propri dati soltanto, cosi da avere
  il tuo companion sempre pronto»*. **Non ancora collocata**: la scomposizione passa da `tech-advisor`.

- **2026-09-22 utente (chat)** — divisione: **«Sì, dividi così (consigliata)»**. Nella 2.2 anche il
  riepilogo sotto le colonne e il raggruppamento a una dimensione con subtotali; l'incrocio
  categorie × mesi resta alla **2.3**; le formule alla **fase 4** (la 2.2 prevede colonne calcolate
  come gancio); il **companion AI è una fase nuova dopo la fase 3**, con due condizioni: server OAuth
  2.1 di Supabase fuori beta (oggi la issue supabase/auth#2820 blocca i connettori MCP reali) e una
  regola sugli spazi condivisi (l'AI di un membro leggerebbe le spese degli altri).

- **2026-09-22 utente (chat)** — tastiera: **«Anche le frecce, come Excel»**, **contro** la
  raccomandazione di `tech-advisor` (Tab/Invio/Esc soltanto, frecce rinviate). Quindi nella 2.2:
  navigazione fra celle con le frecce, che richiede interop JS per il fuoco e una gestione per cella.
  Sotto i 640px la tabella non si rende: avviso con link al registro (proposta non contestata).

- **2026-09-22 utente (chat)** — approvate in chat le quattro sezioni del progetto 2.2 (struttura;
  cosa si vede; modifica; test e rischi), compresa l'esclusione della «riga nuova». Poi, verbatim:
  *«aggiungi anche i tasti per i tutorial guidati e gli helpbutton nella fase 4»*; chiarito col popup:
  **«Nella tabella (2.2)»** — la tabella ha il suo «?» e un pulsante «Tutorial» che la presenta passo
  passo. Il tutorial non esiste nell'app: si costruisce generico, riusabile dalla 2.1-bis.

- **2026-09-22 utente (chat)** — sulle tre decisioni tecniche del capo (tipo di sola scrittura
  `OccorrenzaRicorrente`, `ElencaAsync` privato, record `DatiRegola`): «dalle per buone». Tutte e tre
  **ratificate 2026-09-22**. Poi: «chiudiamo la sessione per oggi».
- **2026-09-22 — L'unità 02 ha trovato che il contratto di scrittura del piano non funzionava.** Un
  `Upsert<Expense>` in `Supabase.Postgrest` 4.4.0 scarta anche le colonne `ignoreOnUpdate`, quindi
  avrebbe omesso `space_id`, `paid_by`, `recurring_id` e `recurring_period` → 23502 in produzione al
  primo «apri Spese» di chi ha una regola, e `PrivilegiInsertTests` non l'avrebbe visto (controlla le
  colonne in più, non quelle mancanti). Rimedio: un tipo di sola scrittura `OccorrenzaRicorrente`
  senza flag `ignore*`, con un test che lo fissa. **Accettato dal capo, da ratificare**, come
  `ElencaAsync` reso `private` (il divieto del §5 ora lo impone il compilatore).
- **2026-09-22 — Deciso dal capo, da ratificare: il record `DatiRegola` entra nell'unità 03.**
  `CreaAsync`/`SalvaAsync` delle regole prendono otto argomenti posizionali, e `ogniMesi`/`giorno`
  scambiati compilano. L'unità 03 scrive il primo chiamante, quindi è l'ultimo momento in cui cambiare
  la firma costa poco. Si annulla con un revert locale.
- **2026-09-22 — Seconda violazione dichiarata**: un `implementer` dell'unità 02 ha compilato una volta,
  contro il divieto; nessun altro processo era attivo, la build ufficiale è stata rifatta. Stessa
  classe dello script Python dell'unità 01: il divieto nei brief non basta da solo.
- **2026-09-22 utente (chat)** — migrazione `20260922000000_ricorrenti.sql` applicata in produzione:
  «ok fatto». Vale anche come **ratifica** delle sei aggiunte allo schema qui sotto, che gli erano state
  elencate prima di applicarla con l'avvertenza che applicarla valeva come approvazione.
- **2026-09-22 — Sei aggiunte allo schema della 2.1, fatte dall'unità 01, ratificate** (le vede
  l'utente quando applica la migrazione): `recurring_period` è **il primo del mese** del periodo, con
  un check che lo impone; check che `recurring_id` e `recurring_period` siano entrambi nulli o entrambi
  pieni; check di formato su `materialized_through`; check d'intervallo sulle date (2000-2999, contro
  l'overflow nel client di un altro membro); **la policy `expenses_insert` ricreata più stretta** — una
  spesa si aggancia solo a una regola dello stesso spazio e dello stesso pagante (verificato dal capo:
  la vecchia era `is_space_member(space_id) and paid_by = auth.uid()`, la nuova è la stessa più una
  condizione, e nessun'altra migrazione la ridefinisce); `materialized_through` concessa anche in
  INSERT. Tutte restrittive o neutre.
- **2026-09-22 — Una violazione dichiarata dall'unità 01**: un `implementer` ha scritto ed eseguito uno
  script Python temporaneo nel worktree, non richiesto, poi rimosso. L'unità gliel'ha vietato nel giro
  successivo. Da riportare all'utente; nessun effetto sul codice consegnato.

- **2026-09-22 — Il confine fra modello e resa nella 2.2** (posizione di `tech-advisor`, adottata):
  ciò che un test xUnit o un lettore di schermo può osservare è modello/comportamento e va nella spec
  adesso; ciò che cambia solo una variabile o una regola in `app.css` è resa e va alla 2.1-bis. **Tre
  voci della zona grigia vanno nella spec, non rinviate**, perché decidono quale markup esiste:
  `<table>` semantica contro griglia di `<div>`; la strategia sotto ~600px (scorrimento orizzontale o
  collasso a schede); la densità delle righe, visto che oggi il pavimento di tocco è 48px e la riga
  di registro 66px (il pavimento vale solo su `pointer: coarse`?).

- **2026-09-22 — Il piano 2.1 del 3 settembre regge nell'impianto ma va corretto in sette punti**,
  verificati da `tech-advisor` sul codice di oggi (105 commit dopo). Vanno **dentro i mandati**:
  1. la premessa «deve funzionare offline come PWA» è caduta il 10 settembre — un brief che la ricopia
     vieta una cosa falsa;
  2. i test oggi sono **310**, non 267: il gate diventa 310 → 319;
  3. il piano rimanda a una sezione `CONTRATTO` di `handoff/PIANO.md` che non esiste più: il contratto
     vive in `Shared/PaginaEditor.cs` (`Cambiata`, `Esci`);
  4. conteggi di righe e un rimando a `Spese.razor` slittati di una decina di righe — cosmetici;
  5. **buco**: la tabella dei file non elenca `Models/Expense.cs`, ma il task 4 scrive `recurring_id`
     e `recurring_period` su `expenses`, quindi `Expense` acquista due colonne `ignoreOnUpdate` — ed è
     ciò che fa scattare `PrivilegiInsertTests`. Va nel perimetro del task 4;
  6. **buco**: il task 4 prescrive `Upsert` + `IgnoreDuplicates` e basta. Senza
     `OnConflict = "recurring_id,recurring_period"` PostgREST risolve sulla chiave primaria, e con
     `id` generato dal client la corsa fra due schede produce un **23505** invece di un duplicato
     ignorato. Verificato sull'xmldoc di `Supabase.Postgrest` 4.4.0;
  7. il nome della migrazione va datato al giorno in cui si scrive, non al 4 settembre.

- **2026-09-22 — Sequenza di pubblicazione** (`main` pubblica in produzione a ogni push, e nessun passo
  applica migrazioni). La migrazione è additiva e retrocompatibile col client già pubblicato, quindi
  «schema nuovo + codice vecchio» è sicuro per qualunque durata; l'inverso no. Ordine:
  task 1 → `main`; task 2 → `main` con il SQL integrale nel resoconto, e **si chiede subito all'utente
  di applicarlo**; **GATE: l'utente scrive in chat «applicata», e la conferma si trascrive qui con la
  data**; task 3; task 4 **solo dopo il gate** (senza schema ogni «Segna» fallisce); task 5-6.
  Un difetto dopo il task 4 si cura con `git revert` su `main`; lo schema non si toglie mai.

## PARTIZIONE

| Unità | Cosa | Dove | Dipende da | Stato |
|---|---|---|---|---|
| **S — spec 2.2** | spec della vista tabellare, modello e comportamento + le tre voci grigie | in chat con l'utente, `brainstorming`; scritta in `docs/superpowers/specs/2026-09-22-spese-tabella-design.md` | — | **FATTO** — approvata in chat il 22 set («ok procediamo a fare la fase 1»); verifica: `git -C /g/Sviluppo/Eton branch -r --contains 73e44b4` -> `origin/main` |
| **R — piano 2.1 corretto** | i sette punti applicati al piano del 3 settembre, più la sotto-navigazione decisa dalla S | documento | S | **FATTO** — più un **ottavo** punto: `SpeseDelPeriodo` restituisce anche `InArrivo` (le future, fuori dai totali), perché la 2.2 le mostra; verifica: `grep -c 'corretto 22 set' /g/Sviluppo/Eton/docs/superpowers/plans/2026-09-03-spese-ricorrenti.md` -> `15` |
| **01 calcolo-e-schema** | task 1 e 2: `Services/CalcoliRicorrenti.cs`, i suoi test, la migrazione e lo script RLS — **scritti, non applicati** | sessione-unità | R | **FATTO** — integrata con `8d0c1db`; verifica: `dotnet test Eton.sln --no-build` -> `Superati:   319` |
| **GATE migrazione** | l'utente applica `supabase/migrations/20260922000000_ricorrenti.sql` in produzione e scrive in chat «applicata»; si trascrive qui con la data | utente | 01 | **PARZIALE** — applicata secondo l'utente («ok fatto», chat, 22 set); **la query di controllo l'ha eseguita l'utente e ha incollato l'esito in chat: `recurring_id`, `recurring_period` — 2 righe su 2 attese.** Resta `PARZIALE` solo per la forma: la regola dei `FATTO` vuole un comando lanciato in sessione, e nessun agente interroga il database. **L'integrazione della 02 su `main` è sbloccata** |
| **02 regole-e-lettura** | task 3 e 4: modello e repository delle regole, `Models/Expense.cs`, materializzazione, percorso unico con `InArrivo`, `Pages/Spese.razor` e `Pages/Home.razor` passano al percorso unico | sessione-unità | GATE | **FATTO** — integrata su `main`; verifica: `dotnet test Eton.sln --no-build` -> `Superati:   328` |
| **03 pagine-ricorrenti** | task 5 e 6: elenco, editor, sotto-navigazione condivisa (entra anche in `Pages/Spese.razor`, dopo la 02), prova nel browser | sessione-unità | 02 | **IN CORSO** — senza prova nel browser: il collaudo lo fa il capo dopo l'integrazione |
| **P — piano 2.2** | piano da task, dopo che la 2.1 è rientrata | documento | S, 2.1 | PIANIFICATA |
| **2.2** | implementazione | sessioni-unità | P | PIANIFICATA |

La partizione delle unità di codice si decide **dopo** R, con un `Explore` di ricognizione: oggi i file
che la 2.2 tocca non sono noti, e la collisione probabile è `Pages/Spese.razor`, che la 2.1 e la 2.2
vogliono entrambe.

## RAZIONALE

La spec della 2.2 viene prima di tutto perché è l'unico passo che richiede l'utente in conversazione,
ed è anche quello che sblocca una decisione rimasta aperta nel piano della 2.1 (la sotto-navigazione).
Il resto è lavoro autonomo in sessioni-unità, separato in due metà dal gate della migrazione.

## PROSSIMA AZIONE

**Ripresa del 23 settembre in poi (sessione nuova).** L'utente ha chiuso la sessione del 22 mentre
l'unità **03** lavorava in background (sessione `321bb5c3`, lasciata finire: non pusha su `main`, quindi
non tocca la produzione). **Primo gesto del capo nuovo**: `claude agents --json` e `git worktree list`
per trovarla — il worktree **non** porta necessariamente il nome dell'unità (la 02 si chiamava
`unita-02-regole-e-lettura`). Se ha finito, il resoconto sta **dentro il worktree**, in
`handoff/03-pagine-ricorrenti/resoconto.md`; poi `claude stop` e `claude rm` sulla sessione dopo
l'integrazione. Se è `waiting`, leggere cosa aspetta con `claude logs 321bb5c3`.
⚠️ Una sessione `--bg` non notifica il capo quando finisce: la 02 è rimasta ferma e non vista per
un'ora. Dopo aver aperto un'unità, **armare subito un'attesa in background** su `claude agents --json`.
Al rientro della 03: audit, integrazione, poi il
**collaudo della 2.1 nel browser** lo fa il capo (server avviato da lui, PID in `handoff/server.md`).
⚠️ L'app di sviluppo punta a Supabase di **produzione**: il collaudo crea una regola che parte il mese
prossimo (nessuna occorrenza generata, quindi eliminabile) e la elimina a fine prova.
Dopo il collaudo: la 2.1 è chiusa, e si passa al **piano della 2.2** (unità P).
Se un utente segnala «colonna sconosciuta»: `NOTIFY pgrst, 'reload schema';` dall'SQL editor.

**Perché tre unità e non sei**: i task 1 e 2 sono piccoli e stanno entrambi prima del gate; il 3 da solo
non produce niente di osservabile ed è la base del 4; il 5 e il 6 sono due pagine della stessa area. Il
task 4 è il più delicato, e resta nell'unità 02 con il solo task 3 accanto.

## APERTO

- **Il rapporto `handoff/CHIUSURA.md` del 20 settembre non è ancora stato letto dall'utente**, e fino
  ad allora non si archivia. ⚠️ Il path è fisso: la chiusura di **questo** goal lo sovrascriverebbe, quindi
  prima di scrivere il mandato di chiusura va archiviato con prefisso di data.
- Restano vivi dal goal precedente e **non sono di questo goal**: `handoff/configurazione-da-applicare.md`
  (gesto dell'utente, clausola 37), `handoff/collaudo/`, `handoff/01-punto-interrogativo/` (`PARZIALE`),
  `handoff/server.md`.
- **Dubbio mio**: la migrazione della 2.1 è il primo SQL di produzione di questo goal, e per la memoria
  del progetto l'SQL di produzione si ferma sempre all'utente. Il gate lo rispetta per costruzione; resta
  da scrivere nel mandato del task 2 che l'unità **non** lo applica.
- **Verificato da `doc-checker` il 22 set** (spec 2.2 §5.4): `Operator.In` in `Supabase.Postgrest`
  4.4.0 filtra UPDATE e DELETE su un elenco di id in una sola istruzione (`PATCH …?id=in.(…)`); il
  criterio documentato è `List<object>` (i Guid si passano con `.Cast<object>().ToList()`, il
  `ToString()` lo fa la libreria). Mai usato finora nel codice di Eton. **Le righe restituite**: UPDATE sì
  (`ModeledResponse<T>`), **DELETE filtrato no** (restituisce `Task`) — la spec 2.2 §5.4 è corretta di
  conseguenza (DELETE + rilettura degli stessi id). Decisione tecnica mia, **da ratificare**: non cambia
  niente di visibile all'utente.
- **Verificato da `doc-checker` il 22 set — la cache dello schema**: Supabase installa un event trigger
  (`pgrst_ddl_watch`) che ricarica la cache su `CREATE TABLE`, `ALTER TABLE`, `CREATE TRIGGER`,
  `CREATE FUNCTION` e simili, quindi la migrazione della 2.1 la ricarica da sola. `GRANT` e
  `CREATE POLICY` non la fanno scattare, ma non serve: la cache non contiene privilegi, che PostgreSQL
  controlla a ogni richiesta. Se dopo la migrazione un inserimento dà ancora «colonna sconosciuta»
  (PGRST204), il rimedio documentato da Supabase è `NOTIFY pgrst, 'reload schema';` dall'SQL editor.
  **Va nei passi per l'utente al gate, come ripiego.** Il punto qui sotto è quindi chiuso.
- `tech-advisor` ha dato **a memoria** (non verificato) che su Supabase hosted la cache dello schema si
  ricarica da sola dopo un DDL, e che altrimenti serve `notify pgrst, 'reload schema';`. Da far
  verificare a `doc-checker` prima del gate.

## FATTI OPERATIVI CHE COSTANO CARI SE DIMENTICATI

⚠️⚠️ **`main` PUBBLICA IN PRODUZIONE A OGNI PUSH** (`.github/workflows/deploy.yml`, GitHub Pages, nessun
gate).

⚠️⚠️ **IL WORKTREE DI UN'UNITÀ NASCE DA `origin/main`**: prima di aprirla si committa **e si pusha**.

- Il server lo avvia e lo ferma il capo, annotando porta e PID in `handoff/server.md`. Su Windows la
  morte del padre non uccide i figli. Riavviarlo prima di ogni prova nel browser.
- Gli implementer non compilano: `obj/` non ha lock fra processi. Compila il capo, a fine giro.
- Il browser giusto ha `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`: solo quello vede `localhost`.
- I dialoghi nativi bloccano il plugin del browser: ciò che passa da un `confirm()` va girato all'utente.
- Una catena di comandi viene valutata dal classificatore per il suo elemento peggiore: spezzarla.
