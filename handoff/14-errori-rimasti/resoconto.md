UNITÀ: 14 — ESITO: FATTO

TOCCATI:
- `Shared/PaginaRegistro.cs` → +40/−3
- `Pages/Spaces.razor` → +20/−3
- `Pages/Spese.razor` → +16/−5
- `Pages/Benvenuto.razor` → +15/−1
- `Pages/Home.razor` → +14/−3
- `Pages/CollectionEdit.razor` → +13/−4

`handoff/PIANO.md` risulta modificato nel working tree ma **non è mio**: nessuno dei miei brief lo
nominava. È la chiusura dell'unità 13, scritta dal capo prima di aprirmi.

Tutti i `file:line` di questo resoconto li ho riaperti io sul disco **dopo l'ultima correzione e
dopo i gate**. Nessuno viene da un revisore.

---

## IL CENSIMENTO DEL MANDATO ERA ESATTO, MA INCOMPLETO — E C'È UN UNDICESIMO PUNTO

Rifatto il `grep` come il mandato chiedeva. **Tutti e dieci i `file:line` combaciavano** col disco
prima che toccassi qualunque cosa.

Ma il `grep` del mandato cerca `errore = $"`, e **quel pattern ha un punto cieco**: intercetta il
campo che si chiama `errore`, non un campo che si chiama altrimenti. Allargandolo a `{ex.Message}`
su tutto il progetto è saltato fuori l'undicesimo:

| file:line | stringa | dove finisce |
|---|---|---|
| `Services/SupabaseService.cs:194` | `ErroreAccesso = $"Accesso non riuscito: {ex.Message}";` | `Benvenuto.razor:211` → `:28`, dentro lo stesso `<div class="errore">` che questa unità ha appena ripulito |

**È lo stesso schermo.** Un utente **non autenticato** che torna da Google con uno scambio di codice
fallito legge il corpo dell'eccezione nella stessa identica scatola rossa in cui, due righe di
codice più in là, adesso comparirebbe una frase in italiano.

**Non l'ho trattato, e il motivo è il perimetro**: `Services/**` è nel `NON TOCCARE` del mandato. La
frase pronta e il ragionamento sono in `FUORI SCOPE`, punto 1. `threat-hunter` l'ha trovato per
conto suo, partendo dai sei file e risalendo — le due scoperte sono indipendenti e coincidono.

---

## CONTRATTI

### Le tre regole che hanno deciso le frasi, e che il mandato non poteva sapere

Vengono tutte da qualcosa che ho aperto, non da un modello copiato a occhio.

**1. `AvviaAccessoGoogleAsync` non fa nessuna chiamata di rete.** `Services/SupabaseService.cs:156`
è `public Task`, non `async`: genera il verificatore PKCE, lo salva, costruisce l'URL e chiama
`NavigateTo(forceLoad: true)`. Le sole operazioni che possono lanciare sono la generazione del
verificatore e `_pkce.Salva()`, cioè un `localStorage.setItem` via JS interop **sincrono**
(`Services/PkceStore.cs:20-21`). Copiare «può essere la connessione» dalle altre undici frasi
avrebbe suggerito una causa **impossibile**.

**2. L'azione dipende dal pulsante che quella pagina ha davvero, e lo stesso guasto ne ha tre.** Il
`catch` di `AssicuraCaricatoAsync` esiste in tre punti con la stessa frase di partenza. Ma:
`Shared/ErroreRiprova.razor:6` ha un `<button>` letterale «Riprova», e `Home.razor:47` e i tre
registri lo rendono **solo quando `Spazi.Attivo` è nullo**; `Spaces.razor:22-24` non ha quel ramo
affatto. Tre azioni diverse per lo stesso guasto — e su questo punto avevo comunque sbagliato in
eccesso: v. `ADJUDICA`.

**3. Contare le chiamate di rete, la lezione dell'unità 10.** Riaperti e contati io:

| Metodo | Chiamate di rete | Conseguenza sul messaggio |
|---|---|---|
| `SpaceRepository.CreaAsync` (`:47-53`) | **1** (RPC `create_space`) | la promessa sul modulo è lecita |
| `SpaceRepository.EntraAsync` (`:58-62`) | **1** (RPC `join_space`) | idem |
| `ExpenseRepository.CreaAsync` (`:96-112`) | **1** (una `Insert`) | idem |
| `CollectionRepository.EliminaAsync` (`:125-153`) | **3** (`:146` lettura, `:149` DELETE, `:151` rilettura) | **nessuna promessa**: il mandato aveva ragione |

### `Shared/PaginaRegistro.cs` — marcatore `[Registro]`, tre punti, tre pagine

```csharp
// :136 — in SegnalaNonLetti, chiamato dai catch di Notes, Collections e Spese.
// Da CollectionEdit:426 (fallimento in lettura), oggetto sostituito.
$"Lo spazio si è caricato, ma non è stato possibile leggerne {NomePlurale}: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova."

// :165 — catch di AssicuraCaricatoAsync in CaricaUnGiro. Da SpaceDetail:215, che ha la stessa
// forma per lo stesso guasto: un caricamento che comincia dall'autenticazione.
"Non è stato possibile caricare i tuoi spazi: può essere la connessione, oppure la sessione che è scaduta. Riprova fra un momento, e se non basta esci e rientra."

// :209 — catch di SuCambioSpazio. Nessun modello: questo guasto non esiste altrove.
$"Non è stato possibile aggiornare {NomePlurale} dopo il cambio di spazio: può essere la connessione, oppure il tuo accesso al nuovo spazio che è cambiato. Ricarica la pagina."
```
Diagnosi: `:135`, `:164`, `:208`.

**«Riprova» a `:136` è letterale, e a `:165` non lo è.** Sembra una contraddizione e non lo è: `:136`
alza `datiNonLetti`, e tutte e tre le pagine mostrano un `<button>Riprova</button>` dentro *quel*
ramo (`Notes.razor:55`, `Collections.razor:54`, `Spese.razor:111`). `:165` no — v. `ADJUDICA`.

**Il marcatore `[Registro]` è l'unico nuovo del diff.** Il precedente della casa è `[Elemento]`,
creato dall'unità 13 perché il dominio non ne aveva uno; qui il problema è l'opposto — il dominio ne
ha **due**, `[Note]` (`Notes.razor:141`) e `[Spese]` (`Spese.razor:423`), e una riga scritta nella
classe base esce da tre pagine. Prenderne uno sarebbe falso sulle altre due. `[Registro]` è il nome
che il progetto dà già a questa cosa: la classe si chiama `PaginaRegistro`, il markup `.registro`.
Motivato nel commento XML a `:119-132`.

### `Pages/Benvenuto.razor` — marcatore `[Auth]`, un punto

```csharp
// :241 — catch di Accedi(). NESSUN MODELLO: nessuna delle venticinque frasi precedenti si trasferisce.
"Non è stato possibile avviare l'accesso: può essere il browser che non lascia salvare i dati di questo sito — succede con la navigazione anonima, o con le impostazioni sulla riservatezza più severe. Prova di nuovo a entrare con Google, e se non basta apri Eton in una finestra normale."
```
Diagnosi: `:240`.

**Tre divergenze, tutte obbligate, tutte motivate nel commento a `:227-239`:**
- **niente «connessione»**: regola 1 sopra — non c'è rete in quel percorso;
- **niente «esci e rientra»**: chi legge **non è autenticato** e non ha nessuna sessione;
- **l'azione nomina «Entra con Google»**, che è il pulsante vero (`:31` e `:177`), e non un «Riprova»
  che a schermo non esiste. Il pulsante torna premibile perché il `catch` rimette `occupato = false`.

`[Auth]` non è inventato: `Services/SupabaseService.cs` lo usa già per le proprie sei diagnosi, ed è
il dominio esatto di questa pagina.

### `Pages/Spaces.razor` — marcatore `[Spazi]`, tre punti

```csharp
// :96 — catch di OnInitializedAsync. Stessa frase di PaginaRegistro:165, AZIONE DIVERSA.
"Non è stato possibile caricare i tuoi spazi: può essere la connessione, oppure la sessione che è scaduta. Ricarica la pagina, e se non basta esci e rientra."

// :135 — catch di Crea(). Da CollectionEdit:611, con la divergenza sull'oggetto che
// SpaceDetail:259 aveva già introdotto per la rinomina.
"Non è stato possibile creare lo spazio: il database ha rifiutato la creazione, oppure non è stato raggiunto. Il nome che hai scritto è ancora qui: riprova fra un momento."

// :178 — catch di Entra(). Stessa forma.
"Non è stato possibile entrare nello spazio: il database ha rifiutato la richiesta, oppure non è stato raggiunto. Il codice che hai scritto è ancora qui: riprova fra un momento."
```
Diagnosi: `:95`, `:134`, `:177`. Le due preesistenti (`:121`, `:168`) sono intatte.

**`:135` è la riga che il mandato chiamava «la peggiore del gruppo»**, e la promessa che fa è
verificata: `nuovoNome` si svuota a `:108`, cioè **solo dopo** che `CreaAsync` è tornata. Il commento
a `:128-133` dichiara anche il rovescio — la finestra della risposta persa dopo che l'INSERT è
passata, in cui lo spazio esiste eccome — e rimanda al commento già presente nel `try`, che spiega
perché `create_space` non deduplica sul nome.

**Su `:96` l'azione è «ricarica la pagina» e non «riprova», ed è l'unico dei tre punti in cui è
sempre vero**: su questa pagina `<ErroreRiprova>` non compare in nessun ramo. Nessun lavoro in
sospeso da perdere: siamo in `OnInitializedAsync`, i due campi sono ancora vuoti.

### `Pages/Spese.razor` — marcatore `[Spese]`, un punto

```csharp
// :336 — catch di Segna(). Da CollectionEdit:611, meno «e non chiudere la pagina».
"Non è stato possibile segnare la spesa: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Quello che hai scritto è ancora qui: riprova fra un momento."
```
Diagnosi: `:335`. La preesistente (`:423`) è intatta.

**«Segnare» e non «salvare»**: è il verbo del pulsante che porta lì (`:103`, «Segna»). È la regola che
l'unità 10 ha applicato per «Togli la mia recensione».

**La promessa è vera, e il mandato aveva ragione a farmela verificare**: questo è un modulo di
**creazione**, che dopo un successo si svuota. `nuovoImporto` e `nuovaDescrizione` si azzerano a
`:321-322`, cioè **dopo** che `CreaAsync` è tornata: ogni percorso che finisce nel `catch` li lascia
intatti. Data e categoria non si azzerano mai, di proposito.

**Caduto «e non chiudere la pagina»** del modello: là proteggeva il corpo di una nota in un editor a
pagina piena, qui si tratta di una riga in cima a un registro. È la stessa potatura che l'unità 13 ha
fatto su `SpaceDetail:259`.

### `Pages/Home.razor` — marcatore `[Home]`, due punti

```csharp
// :291 — catch di Carica(). Identica a PaginaRegistro:165: stesso guasto, stesso markup.
"Non è stato possibile caricare i tuoi spazi: può essere la connessione, oppure la sessione che è scaduta. Riprova fra un momento, e se non basta esci e rientra."

// :379 — catch di CaricaDettagli(). La premessa resta interpolata: i chiamanti ne passano due.
$"{premessa}, ma non è stato possibile leggerne i dettagli: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Ricarica la pagina per riprovare."
```
Diagnosi: `:290`, `:377`. La preesistente (`:401`) è intatta.

**Le due premesse sono letterali del codice**, e la frase regge con entrambe — riletta con ciascuna:
«**Spazio caricato**, ma non è stato possibile leggerne i dettagli: …» (`:300`) e «**Spazio
cambiato**, ma non è stato possibile leggerne i dettagli: …» (`:436`).

**La diagnosi di `:377` sta PRIMA della guardia `if (mia != generazione) return;`**, e diverge dalla
sua vicina preesistente (`:401`, dove sta dopo). Deliberato e commentato a `:373-376`: un fallimento
già sorpassato da una lettura più recente non deve andare **a schermo** — e infatti il `return`
resta — ma nemmeno sparire senza traccia, o quel ramo sarebbe un `catch` muto. È la regola che
l'unità 10 ha ricavato mettendo la diagnosi in cima al `catch`, prima del bivio. **Non ho spostato la
riga preesistente**: è fuori dai dodici punti e cambiare un percorso già collaudato per simmetria non
vale il rischio.

### `Pages/CollectionEdit.razor` — la promessa ritirata

```csharp
// :757 — catch di Elimina(). Era: «… La collezione è ancora al suo posto: riprova fra un momento.»
"Non è stato possibile eliminare: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere se la collezione c'è ancora."
```
**Contate io le tre chiamate di rete** (`Services/CollectionRepository.cs:146`, `:149`, `:151`), come
il mandato chiedeva invece di fidarsi del paragrafo. La frase nuova è la **quinta** occorrenza di una
formula che il progetto ha già quattro volte — `NoteEdit:391`, `ItemEdit:494`, `SpesaEdit:446`,
`RecensioniElemento:591` — verificate una per una da `conformity`. La divergenza dal modello è
motivata nel commento a `:751-756`.

### I cinque riferimenti incrociati, resi nominali e non rinumerati

Riaperto ciascun bersaglio prima di scrivere, come il mandato prescriveva.

| dove (oggi) | citava | contenuto verificato | forma nuova |
|---|---|---|---|
| `CollectionEdit.razor:254` | `NoteEdit.razor:91-94` | il commento su `conflitto is null` è a `NoteEdit:107-110` | «il commento gemello sopra il pulsante Salva di NoteEdit.razor» |
| `CollectionEdit.razor:425` | `CollectionDetail.razor:308,322` | le due diagnosi sono a `:319`, `:333` | «il marcatore `[Collezione]` che CollectionDetail.razor usa per le proprie diagnosi» |
| `CollectionEdit.razor:638` | `NoteEdit.razor:260-263` | il commento su `replace: true` è a `NoteEdit:288-291` | «il commento gemello dopo la creazione in NoteEdit.razor» |
| `Home.razor:322` | `NoteEdit.razor:119` | il campo `generazione` è a `NoteEdit:135` | «il campo 'generazione' di NoteEdit.razor e di SpaceDetail» |
| `Spese.razor:276` | `NoteEdit.razor:201-205` | **era sbagliato due volte**: il messaggio è a `NoteEdit:282`, non un commento | «il ramo in cui `Guid.TryParse` fallisce dentro `Crea`» |

**Più tre che ho sfasato io**, riparati nello stesso modo — v. `SCOSTAMENTI` 3.

### Nessun helper, e stavolta il divieto ha davvero tirato

Nessun tipo, metodo, campo, costante, `using` o helper nuovo. Ma **questa unità è la prima in cui la
tentazione era fondata**, e va detto: `PaginaRegistro.cs` **è già** il posto condiviso, e infatti le
tre frasi che ci ho scritto valgono per tre pagine. Non è un helper: è la classe base che quelle
pagine hanno in comune, e ci ho scritto frasi, non logica. La prova che la distinzione tiene è che
**non ci ho spostato niente degli altri cinque file**: le due frasi di `Home` e le tre di `Spaces`
nascono dallo stesso guasto della `:165` e restano cinque letterali distinti, perché il messaggio
giusto dipende da quale schermata stava aspettando — e su `Spaces` l'azione, infatti, è un'altra.

---

## ADJUDICA

    istruttoria: 1 rilievo su 1 file → checker no

Sotto entrambe le soglie del §4 (somma ≥ 4 fra `bug-hunter` e `conformity`, oppure ≥ 3 file
distinti): qui la somma è **1**, su **1** file. **Nessun `checker`.**

`bug-hunter` **1** · `conformity` **0** · `threat-hunter` **1**. Niente `backend-expert`: nessuna
superficie nuova — il budget la vietava — e il diff dei sei file è di 118 righe aggiunte, sotto la
soglia delle ~120 del §3. Il mandato lo diceva e l'ho verificato invece di assumerlo.

**`bug-hunter`, `PaginaRegistro.cs:156-160` (allora), media → FONDATO. Corretto.**
Claim: il commento affermava «questo ramo lascia `Spazi.Attivo` nullo», quindi «Riprova» è letterale.
**È falso in un caso raggiungibile.** L'ho riaperto io: `Services/SpaceStateService.cs:87-97` — il
`catch` di `CaricaAsync` fa `if (mia == _generazione) _caricato = false;` e poi `throw;`, e **non
tocca `Attivo`**, deliberatamente («l'elenco vecchio si tiene comunque»). Il servizio è un singleton
(`Program.cs`), quindi dopo un caricamento riuscito e una ricarica fallita `AssicuraCaricatoAsync`
può lanciare con `Attivo` ancora valorizzato: lì il markup sceglie il `<div class="errore">` nudo, e
la frase nomina un pulsante che non c'è. Corretto in «**Riprova fra un momento**» — la formula senza
pulsante che il progetto usa ovunque — su `PaginaRegistro:165` **e** sul gemello `Home.razor:291`, e
i due commenti ora dicono il vero. Il difetto era mio: il commento **preesistente** a `:152-154` era
scritto al condizionale giusto («*senza spazio attivo* l'errore si prende tutta la pagina»), e il
commento che ho fatto aggiungere ha trasformato quella condizione in un fatto.

**`threat-hunter`, `Services/SupabaseService.cs:194`, media → FONDATO, FUORI PERIMETRO.**
Stessa riga che avevo trovato col `grep` allargato, raggiunta per un'altra strada. Non l'ho toccata:
`Services/**` è nel `NON TOCCARE`. V. `FUORI SCOPE` 1.

**Il campione sugli infondati non c'è, perché non ci sono infondati.** Il §5 chiede di riverificarne
almeno uno per unità «quando ce ne sono»: entrambi i rilievi sono fondati e li ho aperti entrambi io,
senza passare da nessuno — il primo perché è mio da correggere, il secondo perché tocca esposizione
di dati, che il §5 mi obbliga ad aprire qualunque sia il verdetto.

**Una nota di `bug-hunter` fuori dai rilievi, che ho trattato come un rilievo**: il mio diff aveva
sfasato tre `file:line` che `Spese.razor` fa a `PaginaRegistro.cs`. È lo stesso danno che l'unità 13
ha prodotto e dichiarato; qui i file sono miei, quindi l'ho riparato. V. `SCOSTAMENTI` 3.

---

## LA DOMANDA PIÙ RISCHIOSA — e la risposta è stata NO

Il mandato ne suggeriva una: *la frase di `PaginaRegistro.cs` regge su tutte e tre le pagine?* Le tre
versioni, scritte per esteso come chiesto:

**`:136`** — «Lo spazio si è caricato, ma non è stato possibile leggerne **le note** / **le
collezioni** / **le spese**: può essere la connessione, oppure il tuo accesso a questo spazio che è
cambiato. Riprova.» → regge, e «Riprova» è letterale su tutte e tre.

**`:209`** — «Non è stato possibile aggiornare **le note** / **le collezioni** / **le spese** dopo il
cambio di spazio: può essere la connessione, oppure il tuo accesso al nuovo spazio che è cambiato.
Ricarica la pagina.» → regge.

**Ma la domanda giusta era un'altra, e me la sono fatta dopo la review: l'azione che suggerisco
funziona davvero?** Su `:209` avevo scritto «**Scegli di nuovo lo spazio**, e se non basta ricarica la
pagina», per non ordinare una ricarica che su Spese butta via il modulo. Aperto
`Services/SpaceStateService.cs:123-131`:

```csharp
    public void Imposta(Guid spazioId)
    {
        var scelto = _elenco.FirstOrDefault(s => s.Id == spazioId);
        if (scelto is null || scelto.Id == Attivo?.Id) return;
```

**Riselezionare lo spazio già attivo esce alla seconda riga e non emette nemmeno `Cambiato`.** E
anche passando da un terzo spazio, la guardia `if (Spazi.Attivo?.Id == spazioMostrato) return;` di
`PaginaRegistro` scarterebbe l'evento, perché `CaricaUnGiro` ha già aggiornato `spazioMostrato` al
nuovo spazio prima di fallire. **L'azione era inefficace per costruzione.**

Corretta in «**Ricarica la pagina.**», e il costo è dichiarato nel commento a `:198-207` invece che
nascosto: su Spese la ricarica butta via l'importo e la descrizione eventualmente digitati nel modulo
«segna una spesa». Si accetta perché **un'azione che non produce niente è peggio di una che costa
qualcosa**, e perché su Notes e Collections non c'è nulla da perdere.

Questo è l'unico difetto del diff che non ha trovato nessun revisore.

### Il conteggio: `catch` contro righe di diagnosi

Contati da me, sul disco a lavoro finito.

| file | `catch (Exception ex)` | `Console.Error.WriteLine` | `ex.Message` |
|---|---|---|---|
| `Benvenuto.razor` | 1 | 1 | 1 |
| `Home.razor` | 3 | 3 | 3 |
| `Spaces.razor` | 5 | 5 | 5 |
| `Spese.razor` | 3 | **2** | 2 |
| `CollectionEdit.razor` | 4 | 4 | 4 |
| `PaginaRegistro.cs` | 2 | **3** | 3 |

**I due numeri che non tornano tornano insieme, ed è il disegno.** Il terzo `catch` di `Spese` è
quello che chiama `SegnalaNonLetti(ex)`: la sua diagnosi è la terza di `PaginaRegistro`, che non è in
un `catch` ma serve i `catch` di tre pagine. **Nessun `catch` muto in nessuno dei sei file.**

E `ex.Message` compare **esattamente tante volte quante le righe di console** in tutti e sei: nessuna
via residua verso lo schermo.

**Due file fuori dal mio perimetro hanno smesso di avere `catch` muti senza che li toccassi**, ed è
il vantaggio della classe base: `Notes.razor` (2 `catch`, 1 diagnosi propria + 1 `SegnalaNonLetti`) e
soprattutto `Collections.razor`, che aveva **1 `catch` e zero diagnosi** — muto — e ora è coperto.

---

## FUORI SCOPE

### 1. L'undicesimo punto del rilievo 3 — `Services/SupabaseService.cs:194`

Fondato, trovato due volte in modo indipendente (dal mio `grep` allargato e da `threat-hunter`), e
**non toccato perché `Services/**` è nel `NON TOCCARE`**.

```csharp
            ErroreAccesso = $"Accesso non riuscito: {ex.Message}";
```
→ `Pages/Benvenuto.razor:211` (`errore = SupabaseService.ErroreAccesso;`) → `:28` (`@errore`).

**Perché è più grave degli altri dieci, e non meno.** Nasce in `ScambiaCodiceAsync`, cioè al ritorno
da Google: chi lo legge **non è autenticato**, ed è l'unico messaggio del progetto che un visitatore
anonimo può far comparire. `threat-hunter` osserva che il `.Message` di una `GotrueException` è tipicamente
il corpo della risposta d'errore dell'Auth server, che in configurazioni con un trigger
`on_auth_user_created` fallito può contenere il messaggio Postgres sottostante.

**Il rimedio è una riga, ed è già scritto**, nella forma che questa unità ha appena usato due righe
di codice più in là:
```csharp
            Console.Error.WriteLine($"[Auth] Scambio del codice non riuscito: {ex.Message}");
            ErroreAccesso = "Non è stato possibile completare l'accesso: può essere la connessione, oppure il collegamento che è scaduto — quelli di Google valgono pochi minuti. Torna alla pagina di ingresso e prova di nuovo a entrare con Google.";
```
Restano da trattare allo stesso modo, nello stesso metodo, anche `:182` e `:190`
(«Accesso non completato: riprova dall'inizio.» e «… sessione senza utente.»), che sono già in
italiano ma dicono il fatto e nient'altro — la forma che la regola 2 dell'unità 05 esclude.

**Serve una riga di mandato**, come l'unità 13 ha chiesto per `CollectionEdit:748` e l'ha ottenuta.

### 2. La lezione sul censimento, che è la stessa per la terza volta

Il rilievo 3 è stato dichiarato chiuso due volte a torto. La prima per una mappa *file → unità*, la
seconda idem. **La terza volta il rischio non era la mappa: era il `grep`.** `errore = $"` presuppone
che il campo si chiami `errore` — e in `SupabaseService` si chiama `ErroreAccesso`. Il pattern che
non ha punti ciechi è quello sul **sink** e non sulla sorgente: `= $"…{ex.` per le assegnazioni, e poi
si guarda dove ciascun campo viene reso.

---

## GATE

- `dotnet build -warnaserror --no-incremental` → **Avvisi: 0, Errori: 0**. Non incrementale come
  prescritto: il diff tocca markup Razor e una classe base.
- `dotnet test --no-build` → **Superato! Non superati: 0. Superati: 273. Ignorati: 0. Totale: 273.**
  Esattamente i 273 di partenza: nessun test copre le stringhe di una pagina, e non ne ho aggiunti.

Eseguiti **tre volte**: dopo il rientro dei sei `implementer`, dopo la correzione del rilievo, e dopo
la correzione dell'azione inefficace. Stesso esito.

Compilato **io**, e agli `implementer` l'ho vietato esplicitamente in ogni brief: `obj/` non ha lock
fra processi.

**Server di sviluppo non avviato, browser non usato**, come il mandato prescrive. Nessun processo
lasciato vivo, nessuna porta occupata. **Nessun commit**: i file sono nel working tree.

---

## SCOSTAMENTI

1. **La frase di `Benvenuto.razor` non discende da nessuno dei modelli**, ed è l'unica delle undici.
   Le tre divergenze — niente «connessione», niente «esci e rientra», azione che nomina «Entra con
   Google» — sono imposte dal codice, non da un gusto: prova in `CONTRATTI`, regola 1.

2. **Lo stesso guasto ha prodotto tre azioni diverse** (`Riprova fra un momento` in `PaginaRegistro` e
   `Home`, `Ricarica la pagina` in `Spaces`). Non è incoerenza: è quello che ciascuna pagina offre
   davvero. Verificato sul markup, file per file.

3. **Ho reso nominali tre `file:line` in più dei cinque del mandato** — `Spese.razor:354`, `:355`,
   `:370`, che citano `PaginaRegistro.cs`. I primi due li ha sfasati il mio stesso diff (`:155`,
   `:156` → `:180`, `:181`) e la riparazione è dovuta; **il terzo (`'partenza' a :98`) era ancora
   esatto**, e l'ho reso nominale lo stesso perché sta dentro lo stesso commento e lasciarcelo
   numerico avrebbe prodotto un commento incoerente, a scadenza. È l'unico allargamento del diff, ed
   è di soli commenti.

4. **La diagnosi di `Home.razor:377` sta prima della guardia di generazione**, dove la sua vicina
   preesistente sta dopo. Motivo in `CONTRATTI`; la riga preesistente non è stata spostata.

5. **`:209` non dice più «Scegli di nuovo lo spazio»**: l'azione che avevo scritto non funziona. È la
   mia verifica propria, e la prova è in `LA DOMANDA PIÙ RISCHIOSA`.

6. **Nessun `BLOCKED`, nessuna domanda in sospeso.** Il piano dice che l'utente non è raggiungibile:
   le due cose che gli appartengono sono in `FUORI SCOPE`, non in una domanda.

---

## IL RILIEVO 3 È CHIUSO?

**No. Resta un punto, e non è nel mio perimetro.**

Il `grep` rifatto adesso, dopo i gate, su tutto il progetto e non solo su tre cartelle:

```
grep -rn '(errore|avviso|ErroreAccesso|Messaggio)\s*=\s*[^;]*\bex\b' --include=*.razor --include=*.cs
```
→ **una sola riga in tutto il progetto**:

| file:line | perché non è legittima |
|---|---|
| `Services/SupabaseService.cs:194` | `ErroreAccesso = $"Accesso non riuscito: {ex.Message}";` — reso a `Benvenuto.razor:28` a un utente **non autenticato**. È l'undicesimo punto. Rimedio pronto in `FUORI SCOPE` 1. |

E il `grep` del mandato, quello originale su `Pages`, `Shared`, `Services`:

```
grep -rn 'errore = \$"' Pages Shared Services
```
→ **zero occorrenze**. Le dieci del censimento sono chiuse tutte e dieci.

**Perché ogni altra occorrenza di `{ex.Message}` nel progetto è legittima**: sono **tutte e sole**
righe `Console.Error.WriteLine`, cioè il posto dove il dettaglio tecnico deve stare secondo il
criterio dell'unità 05. Contate: nei miei sei file `ex.Message` compare esattamente tante volte
quante le righe di console (v. la tabella sopra), quindi non esiste nessuna via residua verso lo
schermo. Fuori dai miei sei file l'ho verificato col grep sui **sink** — `errore`, `avviso`,
`ErroreAccesso`, `Messaggio` — non sulla sorgente, che è il pattern che aveva il punto cieco.

**Le due righe di `SpaceDetail.razor` che il `grep` su `errore = $"` ancora intercetta** (`:277`,
`:299`) **sono legittime**: interpolano `{m.Nome}`, il nome del membro, non un'eccezione. Sono le
frasi che l'unità 13 ha scritto.

**Detto in una riga per il capo**: dieci punti su dieci chiusi, l'undicesimo trovato e documentato,
il rilievo 3 si chiude con una riga di mandato su `Services/SupabaseService.cs`.

---

## DA PROVARE NEL BROWSER

Nessuna di queste prove è stata fatta: il mandato vieta di avviare il server. Il testo è quello esatto
che deve comparire.

### Si provano staccando la rete

| # | Testo esatto atteso | Come provocarlo |
|---|---|---|
| 1 | «Non è stato possibile creare lo spazio: il database ha rifiutato la creazione, oppure non è stato raggiunto. Il nome che hai scritto è ancora qui: riprova fra un momento.» | **È la prova che il mandato indica come la più importante.** Su `/spaces`, scrivere un nome, poi DevTools → Rete → **Offline**, poi «Crea». **La cosa da guardare è che il nome digitato sia ancora nel campo**: è la promessa che il messaggio fa. In console `[Spazi] Creazione dello spazio non riuscita: …` |
| 2 | «Non è stato possibile entrare nello spazio: il database ha rifiutato la richiesta, oppure non è stato raggiunto. Il codice che hai scritto è ancora qui: riprova fra un momento.» | Stessa pagina: scrivere un codice qualsiasi di 6-8 caratteri, andare **Offline**, premere «Entra». Il codice deve restare nel campo. |
| 3 | «Non è stato possibile caricare i tuoi spazi: può essere la connessione, oppure la sessione che è scaduta. Ricarica la pagina, e se non basta esci e rientra.» | **Offline**, poi ricaricare `/spaces`. Compare in un riquadro rosso **senza** pulsante: è il motivo per cui qui l'azione è «ricarica». |
| 4 | «Non è stato possibile caricare i tuoi spazi: può essere la connessione, oppure la sessione che è scaduta. Riprova fra un momento, e se non basta esci e rientra.» | **Offline**, poi ricaricare la Home `/`. Qui il riquadro **ha** il pulsante «Riprova». Poi ripetere su `/notes`, `/collections`, `/expenses`: stesso testo, stesso pulsante. |
| 5 | «Spazio caricato, ma non è stato possibile leggerne i dettagli: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Ricarica la pagina per riprovare.» | Aprire la Home **online**, poi andare **Offline** e premere «Riprova». La striscia rossa compare **sopra** il contenuto, non al posto suo. La variante «Spazio cambiato, …» si ottiene cambiando spazio dal selettore mentre si è offline. |
| 6 | «Lo spazio si è caricato, ma non è stato possibile leggerne le note.» (e «le collezioni», «le spese») | La più difficile da innescare a mano: serve che gli **spazi** si leggano e i **dati** no. La via è aprire `/notes` online, andare Offline e premere «Riprova»: gli spazi sono già in memoria — `AssicuraCaricatoAsync` non tocca la rete quando `_caricato` è vero — e solo la lettura delle note fallisce. **Sotto il messaggio deve comparire il pulsante «Riprova»**: è quello che la frase promette. Ripetere su `/collections` e `/expenses` per verificare le tre versioni della stessa frase. |
| 7 | «Non è stato possibile segnare la spesa: … Quello che hai scritto è ancora qui: riprova fra un momento.» | Su `/expenses`, compilare importo e descrizione, andare **Offline**, premere «Segna». **Importo e descrizione devono restare nel modulo.** |
| 8 | «Non è stato possibile eliminare: … Ricarica la pagina per vedere se la collezione c'è ancora.» | Su `/collections/{id}/edit`, **Offline**, premere «Elimina» e confermare. Verificare che **non** dica «la collezione è ancora al suo posto»: è la promessa che questo diff ritira. |
| 9 | In console, un marcatore per ogni messaggio | Durante le prove 1-8 tenere aperta la console: ogni frase a schermo deve avere accanto la sua riga `[Spazi]`, `[Home]`, `[Registro]`, `[Spese]` o `[Collezione]` col JSON per esteso. **Se un messaggio compare senza la riga in console, è un difetto.** E durante l'uso normale non deve comparire nessuna di quelle righe. |

### Limiti dichiarati — non li so provocare a mano

- **`Benvenuto.razor:241`** («Non è stato possibile avviare l'accesso: …»). Servirebbe far fallire
  `localStorage.setItem`. Andare offline **non basta**: quel percorso non tocca la rete. La strada
  più promettente è un browser configurato per bloccare i dati dei siti, o la quota esaurita — non
  l'ho provata e non garantisco che produca un'eccezione anziché un fallimento silenzioso. **Da
  verificare prima di dichiararlo collaudato.**
- **`PaginaRegistro.cs:209`** («… dopo il cambio di spazio: … Ricarica la pagina.»). Il commento del
  codice dichiara che ci si arriva «solo per qualcosa che sfugge» al `try` di `Carica()`, che cattura
  già tutto: è un ramo di ultima istanza. **Non credo sia raggiungibile a mano**, e lo dichiaro
  invece di suggerire una procedura che non funziona.
- **Il caso in cui `PaginaRegistro:165` compare SENZA il pulsante** — cioè il rilievo di
  `bug-hunter`. Servirebbe: creare uno spazio con la rete che cade **solo** durante la rilettura
  dell'elenco, poi navigare su `/notes` con la rete ancora giù. Con l'interruttore Offline di
  DevTools la finestra è difficile da centrare. **La frase corretta è comunque giusta in entrambi gli
  stati**, che è il motivo per cui l'ho scelta: la prova serve a confermare, non a decidere.
- **Nessuna prova di aspetto**: il foglio di stile è dell'unità 11, che viene dopo. Se `.errore` o
  `.vuoto` cambiano lì, queste prove vanno rifatte dopo.
