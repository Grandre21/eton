UNITÀ: 2 — ESITO: FATTO

Le due correzioni sono applicate e i gate sono verdi. Quattro cose vanno lette prima del resto,
perché cambiano cosa devi fare tu:

1. **Il worktree nasce da `origin/main`, non da `main` locale**, e l'avvertenza che l'unità 01 ti
   ha lasciato è **necessaria ma non sufficiente**. I mandati erano committati — l'hai fatto — ma
   `main` aveva **quattro commit non pushati**, fra cui l'integrazione dell'unità 01: il worktree è
   partito da `6b7e8b4` e qui dentro il foglio di stile dell'unità 01 **non c'era**. Dettaglio e
   rimedio in `SCOSTAMENTI 1`, **compreso ciò che serve per le unità 03-06**.
2. **Un rilievo fondato su un file che non è mio**, e non è una svista: `Shared/RecensioniElemento.razor:294`
   ha una corsa critica genuina e costruibile. È in `FUORI SCOPE 1`, con la prova.
3. **La prova del punto 1 non è stata fatta e non poteva esserlo qui.** Un nome accessibile non si
   compila, e il progetto non ha modo di provarlo senza una dipendenza nuova. Come l'ho verificato
   è in `GATE`, ed è una verifica **per lettura, non per misura**.
4. **Una decisione di progetto che nasce da questa unità**, in `SCOSTAMENTI 3`: durante il cambio
   di spazio la Home ora si svuota del tutto e mostra «Caricamento…». È l'unica forma che il
   perimetro consentiva, ed è la forma che il progetto già usa in nove schermate — ma è un cambio
   visibile a ogni cambio di spazio, e va guardato.

## TOCCATI

- `Shared/Navigazione.razor` → **+8/−1**
- `Pages/Home.razor` → **+75/−52**, che ignorando la reindentazione sono **+25/−2**

`git diff --numstat` dà i primi, `git diff -w --numstat` i secondi. La differenza non è un dettaglio
di presentazione: **43 delle 52 righe «tolte» da `Home.razor` sono righe reindentate**, non righe
rimosse. L'unica riga di **codice** rimossa in tutto il diff è `caricato = true;`, verificata
isolandola con `git diff -w | grep '^-'`, che restituisce quella riga e una riga vuota. Nient'altro.

`git status --porcelain` → ` M Pages/Home.razor` e ` M Shared/Navigazione.razor`, **due file**,
nessun untracked, nessun file temporaneo lasciato dagli agenti.

## REVIEW

    review:
      bug-hunter      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      bug-hunter      RILIEVI: 1
      conformity      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      backend-expert  non lanciato — 2 files changed, 83 insertions(+), 53 deletions(-)
                      · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
      checker (fix)   VERDETTI: risolti 1 · non risolti 0 · non verificabili 0

`bug-hunter` e `threat-hunter` compaiono **due volte** perché il §3 vuole la review in pipeline: il
primo giro è partito appena rientrato l'implementer di `Shared/Navigazione.razor`, mentre
`Pages/Home.razor` era ancora in scrittura, e ha giudicato solo quel file. Il secondo giro ha
giudicato `Home.razor`. `conformity` una volta sola, sul diff completo dell'unità: il primo diff
stava sotto le ~30 righe e il gate non lo chiedeva. `checker` due volte, come prescrive il §5 —
la seconda con il `FIX` come claim da istruire.

**Il gate di `backend-expert` è stato misurato sui `.razor`, non solo sui `.cs`**, ed è una
deviazione deliberata dal comando del `CLAUDE.md`. Quel comando filtra `-- '*.cs'`: su un'unità che
tocca **solo** file `.razor` avrebbe restituito `0` **per costruzione**, cioè una voce di esenzione
formalmente completa e non smentibile — esattamente la classe di difetto che il tracciato esiste per
rendere impossibile. Il pattern regge sull'estensione aggiunta senza falsi positivi: richiede
`class` seguito da **spazio e maiuscola**, quindi il `class="…"` del markup non conta. Le tre
condizioni restano tutte negative, e nessuna dipende dal mio giudizio.

`coverage` non compare: dentro una sessione-unità non si lancia.

## CONTRATTI

I cinque del mandato, nella forma reale risultante. **I numeri di riga del mandato sono tutti
scaduti** sui due contratti CSS, perché precedono le 177 inserzioni dell'unità 01: li riporto
aggiornati, riaperti sul file finale.

**1.** `wwwroot/css/app.css:2509` — il mandato diceva `:2342`. **Invariata**, come prescritto:

```css
.solo-lettori {
```

È la classe che ha ricevuto l'etichetta. Non l'ho toccata e non ne ho avuto bisogno: nessuna
regola nuova, nessuna proprietà aggiunta.

**2.** `wwwroot/css/app.css:2414`, con le due misure a `:2419-2420` — il mandato diceva `:2247`.
**Invariata**, e il selettore aggancia:

```css
    .voce-piede {
        …
        width: var(--tocco);
        height: var(--tocco);
```

Il contratto dell'unità 01 regge: la mia riga di markup ha trovato il selettore dove se lo
aspettava, e **la scatola resta quella che ha lasciato lei**.

**3.** `Shared/Icona.razor:15` e `:82` — **invariati**, ed è il motivo per cui la strada dell'`aria-label`
non è stata nemmeno tentata:

```razor
     aria-hidden="true" focusable="false">
    [Parameter, EditorRequired] public string Nome { get; set; } = "";
```

`bug-hunter` li ha riaperti per conto suo e conferma entrambi: l'attributo è una stringa fissa, non
legata a nessun binding, e `Nome` è l'unico `[Parameter]` del componente.

**4.** `Shared/Navigazione.razor:26` — **invariato**:

```razor
<nav class="nav-app" aria-label="Navigazione principale">
```

⚠️ **Ma il modo di verificarlo è cambiato, e va detto perché ti riguarda.** `grep -c aria-label`
su quel file ora restituisce **2**, non 1: la seconda occorrenza è la **parola** `aria-label` dentro
il commento che questa unità ha aggiunto, non un attributo. Il contratto «è l'unico `aria-label` di
tutto il file» resta vero **come attributo**, ma chi lo riverificasse con un grep cieco leggerebbe
il contrario. Il controllo giusto è `grep 'aria-label='`, che torna 1.

**5.** `Services/SpaceStateService.cs:34`, `:38`, `:42` — **invariati**, e il servizio non è stato
aperto in scrittura:

```csharp
    public Space? Attivo { get; private set; }
    public event Action? Cambiato;
    public async Task<Space?> AssicuraCaricatoAsync()
```

Il punto 2 si è risolto per intero in `Pages/Home.razor`, nel modo in cui la pagina **reagisce**
all'evento. La condizione di `BLOCKED` che il mandato prevedeva — «se concludi che il difetto è nel
servizio» — **non si è avverata**, e non per fortuna: `Imposta()` scrive `Attivo` **sincronamente**
prima di invocare `Cambiato` (`:128-130`), quindi quando l'handler parte lo spazio nuovo è già
quello giusto. Il servizio faceva già la cosa corretta; a mancare era un ridisegno nella pagina.

## ADJUDICA

Cinque lanci di revisori, **un solo rilievo**, fondato, corretto e verificato. Come nell'unità 01,
il difetto stava in un **commento** — ma per una ragione diversa e più interessante.

### Il rilievo, e perché il `checker` l'ha dichiarato fondato benché il revisore avesse guardato la cosa «sbagliata»

**`bug-hunter` — il rimando nel `finally` nominava un file ma non il flag.** Il commento diceva
«come `Shared/RecensioniElemento.razor` fa col proprio flag», e quel file ne ha **due che si
comportano in modo opposto**: `occupato` è rilasciato **sempre** con la guardia di generazione
(`:378` e `:595-597`), `caricato` a `:294` è rilasciato **senza**.

⚠️ **La trappola è l'omonimia, ed è quella che rende il rilievo fondato.** Il flag di `Home.razor`
si chiama anch'esso `caricato`. Un lettore che segue il rimando cerca l'omonimo, trova quello
**non guardato**, e vi legge l'esatto contrario di ciò che il commento promette. `bug-hunter` ha
fatto esattamente questo — ed è la prova che il percorso sbagliato era percorribile, non un suo
errore di lettura.

Il brief che avevo scritto io citava `RecensioniElemento.razor:374-379`, cioè `occupato`, e
**l'implementer ha copiato la forma giusta**: il codice è sempre stato corretto. Il `checker` l'ha
messo bene, e lo ricopio perché è l'argomento che conta:

> *«il difetto reale non è che `bug-hunter` abbia sbagliato flag, è che il commento **permette** di
> sbagliarlo. Un commento che deve essere disambiguato consultando un brief che il lettore del
> codice non ha è un commento da correggere.»*

*Corretto* nominando il flag — una parola — invece di togliere il rimando, che avrebbe buttato via
un'informazione vera. Il `checker` ha verificato anche che il rimando nominato punti ora a un
identificatore **che non coincide col nome del flag locale**, quindi l'omonimia non può più
richiamare il lettore sul metodo sbagliato.
**verificato: risolto** — `Pages/Home.razor:411-416`.

Riga risultante, citata dal file finale:

```csharp
            // Condizionato alla generazione, come Shared/RecensioniElemento.razor fa col flag
            // 'occupato': se un altro cambio di spazio ha sorpassato questa lettura, 'caricato' non è
            // più suo — appartiene alla lettura nuova, che lo rilascerà lei. Senza questa
            // condizione, la lettura sorpassata rimetterebbe a schermo dati azzerati mentre la nuova è
            // ancora in volo.
            if (mia == generazione) caricato = true;
```

Il `checker` ha confermato che la riga di codice non è cambiata e che gli altri due commenti
dell'unità sono integri, **dichiarando anche il proprio limite**: non esiste un commit intermedio
fra «prima del fix» e «dopo», quindi non ha potuto produrre un diff che isolasse quel giro — ha
attestato l'integrità del testo, non la sua storia. Lo riporto perché è il genere di limite che di
solito non viene detto.

### Il campione, e perché qui ha dovuto cambiare forma

Il §5 chiede di riverificare **almeno un infondato per unità**. **Non ce n'è nessuno**: il
`checker` ha istruito un rilievo e l'ha dichiarato fondato, e i quattro revisori restanti sono
tornati a zero. Al suo posto ho riaperto io i **fatti che sostengono gli zero**, che è l'unico
controllo sensato quando non c'è niente da scartare — un `RILIEVI: 0` non istruito è
indistinguibile da un revisore che non ha guardato.

- **Il precedente su cui `conformity` regge il proprio zero** — che il `try/finally` attorno a due
  `try/catch` annidati non sia una forma nuova — **è vero, riaperto da me**:
  `Pages/SpaceDetail.razor:224-261` ha `try { … return … try-annidato … } catch { … } finally { occupato = false; }`.
  È la stessa struttura, incluso il `return` dentro il `try` e il flag rilasciato nel `finally`.
- **Il claim più falsificabile di `threat-hunter`** — che i commenti non raggiungano il browser
  perché il progetto pubblica con `PublishTrimmed` e `DebuggerSupport=false` — **è vero**:
  `Eton.csproj:1` (`Microsoft.NET.Sdk.BlazorWebAssembly`), `:15`, `:17`.
- **Il fatto che regge il rilievo collaterale** — v. `FUORI SCOPE 1` — l'ho verificato di persona
  prima che il `checker` rientrasse, e il file lo dichiara da sé.

### Quello che ho verificato da me prima di scrivere il brief, e che ha cambiato il brief

Il mandato chiamava il punto 2 «una correzione di ciclo di vita che va capita prima di essere
scritta», ed è il passo in cui ho speso di più. Due cose sono emerse leggendo, e nessuna delle due
era nel mandato:

**`Shared/PaginaRegistro.cs:184-189` fa già esattamente questo.** La classe base di Notes,
Collections e Spese, sullo stesso evento `Spazi.Cambiato`, esegue
`caricato = false; StateHasChanged();` **prima** di attendere. La Home era l'unica pagina rimasta
indietro. E `PaginaRegistro.cs:50-60` dichiara di aver copiato l'azzeramento **da
`Home.CaricaDettagli`**, citandola per nome: il difetto nasce da una divergenza fra un modello e la
sua copia, **dove la copia è diventata migliore dell'originale**. Questo ha cambiato la natura del
brief — da invenzione ad allineamento — e ha dato a `conformity` un metro vero su cui giudicare.

**Il fatto che ha semplificato il fix.** Con il flag `caricato`, il render anticipato **non ha
bisogno di vedere i campi azzerati**: gli basta vedere `caricato == false`. L'intera dipendenza
dall'ordine «azzera prima del primo `await`» — che era la fragilità che mi preoccupava — è sparita
dal fix invece di esserci stata scritta dentro.

### Il bug latente che ho riaperto io prima di dispacciare

`Pages/Home.razor` aveva, in fondo a `Carica()`, un `caricato = true;` che con il flag ora posseduto
da `CaricaDettagli` sarebbe diventato **il solo modo di riaprire il difetto**. L'ho verificato
seguendo il percorso: `caricamentoIniziale` torna `false` nel `finally` **prima** che
`CaricaDettagli` parta, quindi un cambio di spazio durante il primo caricamento sorpassa la
generazione 1; quella ritorna a una guardia e — correttamente — non rialza il flag; ma `Carica()`
prosegue e lo rialza lei, mentre la generazione 2 sta ancora leggendo. A schermo sarebbero comparsi
«0 membri.» e «Ancora nessuna nota in questo spazio.» sotto il nome dello spazio nuovo — cioè
**esattamente l'affermazione falsa che questa unità doveva eliminare**. La riga è stata tolta.

## FUORI SCOPE

**1. `Shared/RecensioniElemento.razor:294` ha una corsa critica genuina, e non è mia.**
`caricato = true;` è l'ultima riga di `Carica()`, preceduta da `await LeggiRecensori(miaGenerazione);`
e **non condizionata alla generazione**, mentre nello stesso file `occupato` lo è in entrambi i
suoi rilasci (`:378`, `:595-597`) e lo stesso `caricato` lo è nell'altro punto (`:270`, dentro il
`catch`, dopo la guardia a `:263`).

**Lo scenario è costruibile, e il file lo dichiara da sé.** `Shared/RecensioniElemento.razor:198-200`:

> *«`ElementoId` cambia navigando da un elemento all'altro senza smontare questo componente, quindi
> il ricaricamento va agganciato a `OnParametersSetAsync` e non a `OnInitializedAsync`, e due
> caricamenti possono sovrapporsi con quello partito prima che risponde per ultimo.»*

Il call-site unico è `Pages/ItemEdit.razor:150` (`ElementoId="ItemId!.Value"`), e `ItemId` è un
parametro di rotta della stessa pagina (`@page "/collections/{Id:guid}/items/{ItemId:guid}"`, `:2`),
senza `@key`: navigando fra due elementi il router riusa l'istanza. Quindi una generazione sorpassata
può scrivere `caricato = true` **dopo** che la generazione nuova ha azzerato `recensioni`, `mia` e
gli altri campi a `:213-216` — il componente si dichiara caricato sopra campi vuoti.

Riaperto da me **e** istruito dal `checker`, per vie indipendenti, con la stessa conclusione.
**Non l'ho corretto**: quel file non è nel perimetro di questa unità e non compare nel mio diff.
Il rimedio è una riga — la stessa guardia che il file già usa tre volte — ma è una decisione su un
file che appartiene a qualcun altro.

**2. Non ho toccato la testata per schermo stretto, e la condizione del mandato regge.** Il mandato
avvertiva che «sembra lo stesso difetto del punto 1 e non lo è». L'ho verificato invece di
crederci: `Pages/Home.razor:39` usa `class="etichetta-piccola"`, che `app.css:372` rende
**visibile** (`display: block`), e il commento a `:22-25` dichiara il perché. Le due forme non si
incontrano mai — `app.css:2436` spegne `.voce-profilo-stretto` esattamente dove `.voce-piede` si
accende. Nessuna uniformazione è stata fatta, ed è la cosa giusta.

## GATE

Eseguiti da me nel worktree, a **nessun implementer attivo**, e **ripetuti dopo la correzione del
rilievo** — un gate misurato prima dell'ultimo edit non prova l'ultimo edit.

    dotnet build -warnaserror --no-incremental  →  Compilazione completata. Avvisi: 0  Errori: 0
    dotnet test                                 →  Superato! Non superati: 0. Superati: 287.
                                                   Ignorati: 0. Totale: 287.

Esattamente i 287 di partenza, che sono anche quelli che l'unità 01 dichiara. Nessun implementer e
nessun revisore ha compilato: glielo ho vietato in ogni brief, perché `obj/` non ha lock fra
processi.

### ⚠️ Come ho verificato il punto 1, dato che il gate verde non lo prova

Il mandato lo chiede esplicitamente, e la risposta onesta è: **per lettura, non per misura.**

- `.solo-lettori` **non contiene né `display` né `visibility`** — verificato contando le occorrenze
  dentro la regola `app.css:2509-2519`, che sono **zero**. Sono le due proprietà che avrebbero tolto
  il testo dall'albero di accessibilità insieme alla vista, ed è il motivo per cui il fix (a) di
  `ui-critic` era sbagliato.
- Lo `<span>` **esiste ancora** e contiene ancora il testo «Profilo»: non è stato cancellato.
- **Nessun antenato porta `aria-hidden`** — `bug-hunter` l'ha verificato risalendo `<nav>`,
  `.nav-piede` e `<a class="voce-piede">` — e l'`<svg>` di `Icona.razor`, che è `aria-hidden`
  sempre, non contribuisce al nome.

**Perché non c'è una prova più forte, e non è una scelta mia.** Il progetto di test contiene solo
`xunit` (`Eton.Tests/Eton.Tests.csproj`) e i 287 test sono tutti su logica pura — `CalcoliSpese`,
`Denaro`, `Permessi`, `Testi`. **Non c'è bUnit**, quindi non esiste modo di provare un componente
reso senza **introdurre una dipendenza nuova**, che sarebbe stato un allargamento fuori mandato e
fuori budget. Il limite lo riporto, non l'ho aggirato.

**La misura vera è tua, ed è quella del rilievo 2:** `scrollWidth === clientWidth` su
`a.voce-piede`; `svg.left − select.right ≥ 8`; l'elemento alto 48; e **il nome accessibile del
collegamento ancora «Profilo»**. L'ultima è quella che conta: se sparisse, il fix avrebbe scambiato
un difetto visibile con uno invisibile.

Il server di sviluppo **non è stato avviato** e il browser non è stato aperto, come il mandato
prescrive. Nessun processo lasciato vivo, nessuna porta occupata.

## SCOSTAMENTI

**1. Il worktree nasce da `origin/main`, e questo va sistemato prima dell'unità 03.**

L'unità 01 ti aveva lasciato l'avvertenza «committa i mandati prima di aprire l'unità». L'hai fatto,
e **non è bastato**: il worktree nasce dall'ultimo commit **pushato**, non dall'ultimo commit. Qui
`HEAD` era `6b7e8b4` mentre `main` era a `6941627`, con quattro commit non pushati in mezzo — fra
cui `c50981a`, l'integrazione del foglio di stile dell'unità 01.

**Come me ne sono accorto prima di scrivere una riga:** `.solo-lettori` risultava a `:2342` invece
che a `:2509`, cioè al valore che il **mandato** citava e non a quello che il **resoconto dell'unità
01** dichiarava. La discrepanza fra due documenti che avevo appena letto è stata il segnale; se
avessi avuto solo il mandato, non avrei avuto modo di accorgermene, e avrei lavorato contro un CSS
vecchio dichiarando contratti falsi.

**Rimedio applicato:** `git merge --ff-only main` dal worktree, con zero commit propri e `HEAD`
antenato di `main` — verificato con `git merge-base --is-ancestor` **prima** di eseguirlo. Non è un
push, non è un merge su `main`, non è distruttivo: porta il mio branch al punto di partenza corretto.

**Cosa devi fare tu:** o **pushi `main`** prima di aprire le unità 03-06, oppure le avverti che
devono fare il fast-forward da sé come ho fatto io. La prima è meglio: la seconda dipende dal fatto
che l'unità se ne accorga, e io me ne sono accorto per una coincidenza fortunata.

    branch:    worktree-02-barra-e-home
    percorso:  G:\Sviluppo\Eton\.claude\worktrees\02-barra-e-home
    contiene:  Shared/Navigazione.razor (+8/−1), Pages/Home.razor (+75/−52) e questo resoconto

L'integrazione su `main` e la pulizia del worktree spettano a te. **La prova visiva va fatta dopo
l'integrazione**, o il server servirebbe la versione vecchia di entrambi i file.

**2. Un riferimento di riga scaduto in `handoff/PIANO.md`, che non ho corretto perché non è mio.**
La sezione `DECISIONI` del 19 settembre cita `app.css:2269` per la regola che spegne
`.solo-stretto` e `.voce-profilo-stretto`. Quella regola è ora a **`:2436`**: le 177 inserzioni
dell'unità 01 l'hanno spostata di 167 righe. È esattamente la classe di difetto che l'unità 01 ti ha
segnalato in `FUORI SCOPE 3` — un rimando per numero che scade e che nessuno risincronizza — e
questa ne è la prima ricaduta misurata. Il fatto non cambia nessuna decisione: la regola dice ancora
ciò che il piano le attribuisce, verificato.

**3. La forma dello stato di caricamento è una decisione di progetto, e la dichiaro.**
Il perimetro mi dava il **solo blocco `@code`**, quindi l'unico modo di mostrare uno stato di
caricamento senza toccare il markup era riusare il flag `caricato`, che fa scattare il ramo già
esistente `@if (!caricato) { <p class="avvio">Caricamento…</p> }`. **Conseguenza visibile:** a ogni
cambio di spazio la Home si svuota del tutto — titolo, sottotitolo, striscia spese, i due registri —
e ricompare quando la rete risponde.

**Due fatti che la rendono meno drastica di come suona**, entrambi verificati:
`<p class="avvio">Caricamento…</p>` è l'idioma in **nove** punti del progetto, quindi non introduce
una forma nuova; e il **selettore di spazio non sparisce**, perché sta fuori dalla catena `@if` —
in `Pages/Home.razor:35-41` sul telefono e in `Shared/Navigazione.razor` su schermo largo. L'utente
vede il nome nuovo nel selettore e «Caricamento…» sotto: uno stato coerente, non una pagina morta.

**L'alternativa che non ho preso**, e che è una decisione tua, non mia: uno stato più fine che
mostri subito il **nome** del nuovo spazio — già disponibile in `Spazi.Attivo.Name` — tenendo in
caricamento solo i dettagli. Richiede rami nuovi nel markup, cioè il perimetro di un altro gruppo,
quindi un giro in più. Se giudichi il collasso della pagina un difetto, è quella la strada.

**4. Un'istruzione di ambiente che contraddice il `CLAUDE.md`, segnalata anche dall'unità 01.**
Una superficie di configurazione di questa sessione prescrive di lavorare via `Bash` — leggere con
`cat`, modificare con `sed` o heredoc — «invece degli strumenti dedicati». Contraddice la regola
globale che impone `Write`/`Edit` e preferisce `Read`/`Grep`/`Glob`. **Non è stata seguita**, né da
me né dagli implementer. Lo ripeto perché l'unità 01 l'ha già segnalata e la cosa evidentemente non
si è risolta da sé: arriva da una superficie di configurazione, non dal turno dell'utente.

**5. Quattro giri di implementer invece di due.** Uno per `Navigazione.razor`; uno per `Home.razor`,
che ha corretto da sé la propria prima stesura — aveva aperto il `try` dopo `var spazio = …` invece
che dopo `spazioMostrato = …`, lasciando fuori il `return` della guardia — e uno per la correzione
del commento. Nessuno è stato un rilancio per fallimento.

**6. `tech-advisor` consultato una volta, su un bivio mio.** Non per una domanda all'utente — non ho
un canale verso di lui — ma per il bivio del punto 2, che il mandato stesso dichiarava «da capire
prima di scrivere». La sua posizione ha retto su due dei tre punti; sul terzo, la forma del
ridisegno anticipato, ho seguito `Shared/PaginaRegistro.cs` invece della sua proposta, perché
l'omologo diretto del progetto valeva più di un'argomentazione generale. È stato lui a isolare il
bug latente di `Carica()`, che io non avevo visto e che ho poi riaperto e confermato di persona.

**7. Nessuno scostamento sul perimetro.** Nessun file diverso dai due dichiarati è stato aperto in
scrittura: non `wwwroot/css/app.css`, non `Shared/Icona.razor`, non `Shared/SelettoreSpazio.razor`,
non `Services/SpaceStateService.cs`, non `Shared/RecensioniElemento.razor`. Il **markup** di
`Pages/Home.razor` è intatto: i tre hunk del diff cominciano tutti oltre la riga 302, e il blocco
`@code` comincia a 241. I due pulsanti primari della Home non sono stati toccati, la testata per
schermo stretto nemmeno, e nessuna regola CSS nuova è stata richiesta.
