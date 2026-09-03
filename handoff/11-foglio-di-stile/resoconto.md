UNITÀ: 11 — ESITO: PARZIALE

Sei voci su otto chiuse, una `BLOCKED` con la proposta pronta (voce 4), una lasciata per
mandato (voce 8, cosmesi che toccherebbe file non miei). Nessuna voce è stata dimenticata:
le otto righe d'esito sono più sotto.

**La domanda per il capo, in una riga**: posso aggiungere una classe a
`Shared/ConfermaAzione.razor` — **un file solo, non i quattro editor** — per dare al CSS un
gancio sull'azione distruttiva? Il dettaglio è nella riga della voce 4 e in `FUORI SCOPE`.

## TOCCATI

- `wwwroot/css/app.css` → +93/−9
- `wwwroot/index.html` → +11/−0
- `Pages/NoteEdit.razor` → +1/−1 (perimetro condizionale, motivato alla voce 5)

Totale `git diff --stat`: **105 inserzioni, 10 rimozioni su tre file** — e di quelle
inserzioni una settantina sono commenti, perché è il modo in cui questo foglio di stile è
scritto.

`git status --porcelain` mostra questi tre file e nessun altro. Nessun commit: il working
tree è come il mandato lo vuole.

## LE OTTO VOCI, UNA RIGA CIASCUNA

1. **`a.btn:not([href])` — FATTA.** `app.css:743-744`. Il selettore di `.btn:disabled` ora
   include i link senza `href`, cioè i «Chiudi» spenti dei quattro editor.
2. **«Salva» spento su nero — FATTA**, e il difetto era ancora vero: verificato prima di
   toccare, `.btn.primario` (`app.css:704-710`) dà `background: var(--accento)` e l'unica
   cosa che lo spegneva era `opacity: .5`. La voce 1 da sola **non bastava**: dimezzare
   l'opacità di un blu pieno su nero lascia comunque il colore più acceso della schermata.
   Serviva una regola sua, `app.css:761`.
3. **Pastiglie a 48px — FATTA.** `app.css:1982`. Una regola, tre pagine. L'inline
   `font-size: var(--t-lg)` di `CollectionEdit.razor:104` **resta**, e la decisione è
   motivata sotto in `SCOSTAMENTI`: non è la stessa pastiglia in tre misure, è testo in due
   pagine ed emoji nella terza.
4. **«Elimina» lontano da «Chiudi» — BLOCKED.** Nessun gancio nel markup, come il capo
   sospettava; ma il rimedio costa **meno** di quanto il mandato prevede. Dettaglio in
   `FUORI SCOPE`.
5. **Il salto di 358px dell'anteprima — FATTA.** `app.css:1466` più una parola tolta a
   `NoteEdit.razor:77`. Il CSS da solo non bastava: perché, sotto.
6. **Basi della barra laterale — FATTA**, e l'origine del disallineamento **si è potuta
   stabilire leggendo**, quindi non l'ho lasciata a `live-testing`. `app.css:2223`.
7. **Banner di aggiornamento — FATTA**, tutte e due le metà: «Più tardi»
   (`index.html:78` e `:107-115`) e il posizionamento, che copriva davvero l'azione
   principale — il calcolo è sotto.
8. **`.scelta-pastiglie` — NON FATTA, per mandato.** Rinominare tocca tre `.razor` che non
   sono miei; il mandato dice che in quel caso non si fa e si scrive qui. È in `FUORI SCOPE`.

## CONTRATTI

Ogni regola nuova o modificata, verbatim, col numero di riga riaperto dopo l'ultima
modifica.

**`app.css:743-744`** — selettore esteso, corpo invariato:

```css
.btn:disabled,
a.btn:not([href]) {
```

**`app.css:761-765`** — regola nuova:

```css
.btn.primario:disabled:not(.occupato) {
    background: var(--superficie-alta);
    border-color: var(--bordo-forte);
    color: var(--testo);
}
```

`:not(.occupato)` non c'era nella prima stesura: è la correzione del rilievo di
`bug-hunter`, adjudicato fondato. Vedi `ADJUDICA`.

**`app.css:1982-1986`** — regola nuova:

```css
.scelta-categoria .pastiglia {
    min-height: var(--tocco);
    padding: 0 var(--s4);
    font-size: var(--t-sm);
}
```

Le tre proprietà sono quelle di `.barra-elenco .pastiglia` (`app.css:1544-1550`), che è la
pastiglia premibile già esistente del progetto. Le due che quella regola ha in più —
`flex: none` e la `transition` — sono omesse di proposito: `flex: none` là serve perché la
pastiglia divide una riga con un selettore elastico, qui no; `cursor` e `touch-action`
arrivano già da `button.pastiglia` (`app.css:1965-1971`), e ripeterli sarebbe duplicazione.

**`app.css:1466`** — una proprietà dentro `.markdown`:

```css
    min-height: 40vh;
```

Stesso valore, verbatim, di `.corpo-nota` (`app.css:1412`): non è un numero scelto adesso, è
quello che il progetto aveva già dichiarato per l'editor che questo blocco sostituisce.

**`app.css:2223`**, dentro `@media (min-width: 64rem)`, regola `.nav-piede`:

```css
        align-items: flex-end;
```

Era `center`. `flex-end` è ciò che `.barra-elenco` (`app.css:1527`) usa già per lo stesso
caso: un `<label class="selettore">` alto accanto a un controllo più basso.

**`app.css:457-467`** — il banner passa nel flusso:

```css
#aggiornamento-pwa {
    position: sticky;
    z-index: 30;
    margin: 0 var(--s3);
    bottom: calc(var(--s8) + env(safe-area-inset-bottom));
    display: flex;
    flex-wrap: wrap;
```

**`app.css:479`** — `min-width` sullo span, che è ciò che forza il capo:

```css
#aggiornamento-pwa span { flex: 1; min-width: 12rem; }
```

Il `12rem` è della stessa famiglia del `10rem` di `.barra-elenco .selettore`
(`app.css:1537`), ed è lì per la stessa ragione: `flex: 1` da solo lascia il figlio
elastico stringersi all'infinito senza mai mandare a capo i fratelli `flex: none`.

**`app.css:500-505`** — regola nuova:

```css
#aggiornamento-pwa button.dopo {
    background: transparent;
    color: var(--testo);
    border: 1px solid var(--bordo-forte);
    font-weight: 500;
}
```

`color: var(--testo)` e non `--testo-tenue`: è la correzione del rilievo di `conformity`,
adjudicato fondato. Vedi `ADJUDICA`.

**`app.css:375`** — guardia nuova:

```css
#app { min-height: 100vh; }
```

**`app.css:2279-2284`**, dentro `@media (min-width: 64rem)`:

```css
    #aggiornamento-pwa {
        margin: 0 var(--s5) 0 auto;
        bottom: var(--s5);
        max-width: 26rem;
    }
```

`margin-left: auto` sostituisce `left: auto; right: var(--s5)`: con `max-width` dichiarata è
ciò che accosta a destra un elemento che ora sta nel flusso invece di essere appeso ai bordi.

**`index.html:78`** — markup nuovo:

```html
        <button type="button" id="aggiorna-dopo" class="dopo">Più tardi</button>
```

Dopo «Aggiorna» e non prima: in ogni `<div class="azioni">` del progetto il primario viene
per primo (`Pages/NoteEdit.razor:106-114`).

**`index.html:113-115`** — l'unica logica nuova, dentro `proponi(worker)`:

```js
                document.getElementById('aggiorna-dopo').onclick = function () {
                    banner.hidden = true;
                };
```

`banner.hidden = true` e nient'altro, come deciso il 3 settembre: nessun `sessionStorage`,
nessun `localStorage`, nessun timer.

**`Pages/NoteEdit.razor:77`** — tolto l'attributo `rows="16"` dalla `<textarea>`. Niente
altro cambia sulla riga: `class`, `maxlength`, `@bind`, `@bind:event`, `disabled` e
`placeholder` sono identici.

## ADJUDICA

`bug-hunter` e `conformity` come da gate del mandato; `threat-hunter` in più perché il diff
tocca `index.html`, dove c'è JavaScript.

**istruttoria: 2 rilievi su 1 file → checker no.** Soglia: somma ≥ 4, oppure ≥ 3 file
distinti citati. Qui 2 rilievi, entrambi su `wwwroot/css/app.css`. Ho riaperto io le due
citazioni.

**1. `bug-hunter`, `app.css:754` (numerazione di allora), severità media — FONDATO,
corretto.**

> «`.btn.primario:disabled` (0-3-0) batte `.btn.occupato` (0-2-0) e non viene mai
> neutralizzata per un pulsante insieme primario, occupato e disabled.»

Riaperto e confermato. La riga che regge il claim è `Pages/Spese.razor:103`:

```razor
<button class="btn primario @(occupato ? "occupato" : "")" @onclick="Segna" disabled="@(occupato || !ModuloValido || rileggoMese)">Segna</button>
```

Durante il salvataggio quel pulsante porta insieme le tre classi e l'attributo `disabled`.
La mia regola nuova gli avrebbe messo il grigio addosso proprio mentre `.btn.occupato` lo
fa pulsare per dire il contrario — e il commento di `.btn.occupato` (`app.css:769`) dichiara
per iscritto quel difetto: «un pulsante grigio non dice a chi preme se è morto o solo al
lavoro». **Verificato con `grep` su tutti i `.razor` che è l'unico punto del progetto dove
`.btn.primario` e `.occupato` coesistono**: un solo risultato, quella riga.

Corretto con `:not(.occupato)` sul selettore, non con la regola in più che il revisore
suggeriva: escludere costa una riga, ripetere i colori del primario in una seconda regola
sarebbe la duplicazione che `conformity` punisce nel rilievo qui sotto.

**2. `conformity`, `app.css:498` (numerazione di allora), severità media — FONDATO in
parte, corretto nella parte che regge.**

> «Il nuovo pulsante secondario non riusa il linguaggio del progetto per un'azione
> secondaria, e il commento afferma il contrario di quello che il codice fa.»

Riaperto. La prova citata è `app.css:670` e `679-680`, `.btn { background:
var(--superficie-alta); color: var(--testo); }`, riusato senza variazioni in ogni coppia
primaria/secondaria: `Shared/SchedaConflitto.razor:25`, `Shared/ConfermaAzione.razor:27`.
Nessuna di quelle usa `--testo-tenue`.

**Fondato sul colore del testo**: `--testo-tenue` era un terzo valore, e rendeva falso il
commento che dichiarava parità con `.btn`. Corretto in `var(--testo)`.

**Infondato sul fondo `transparent`**, e il revisore stesso lo anticipava nella sua nota di
adjudica: `.btn` usa `background: var(--superficie-alta)`, che è **lo stesso colore del
banner** (`app.css:471`) — un pulsante di quel colore dentro quel contenitore si vedrebbe
solo per il bordo. `transparent` non è un linguaggio nuovo, è l'unico modo di ottenere lì
l'effetto che `.btn` ottiene altrove. Ho scritto la ragione nel commento invece di lasciarlo
dire una cosa non vera.

**3. `threat-hunter` — 0 rilievi**, ed è un esito motivato, non un rapporto vuoto: ha
istruito le cinque domande del brief e ha risposto che il rimando non è persistente (il
banner torna da `registrazione.waiting` a `index.html:119`), che `onclick` è
un'assegnazione e non un `addEventListener` quindi non accumula handler, che lo sticky non
è clickjacking perché entrambe le azioni del banner sono locali e senza privilegi, che il
testo del banner è statico, e che la sanificazione del Markdown altrui
(`Services/MarkdownRenderer.cs`, `DisableHtml()` più whitelist di schemi) non è toccata dal
diff. Ha riaperto le righe prima di citarle, come richiesto.

**Campione sugli infondati**: il §5 chiede di riverificarne almeno uno per unità *quando ce
ne sono*. Qui non ci sono rilievi respinti per intero — i due arrivati sono entrambi fondati
e corretti, e l'unica parte respinta (il fondo `transparent`) l'ho riaperta e istruita qui
sopra citando `app.css:463`. Non c'è nient'altro da campionare, e lo dichiaro invece di
tacerlo.

**Nessun rilievo su `prefers-reduced-motion`**, che era il rischio annunciato dal mandato:
l'ho messo fuori mandato nei tre brief, citando la scelta dell'utente del 24 agosto 2026.

## FUORI SCOPE

**1. Voce 4, «Elimina» adiacente a «Chiudi» — BLOCKED, e il rimedio costa meno del
previsto.**

Il mandato dice che aggiungere una classe «tocca i quattro editor». **Non è così**: i
quattro editor non scrivono «Elimina», lo delegano a `Shared/ConfermaAzione.razor`, che è
un file solo. I suoi call-site sono cinque — `NoteEdit.razor:121`, `SpesaEdit.razor:161`,
`ItemEdit.razor:142`, `CollectionEdit.razor:270`, `RecensioniElemento.razor:72` — e tutti
mettono il componente dentro un `<div class="azioni">` accanto a «Salva».

Il gancio non esiste davvero: `ConfermaAzione` rende `<button class="btn rosso">` nudo
(`:26` e `:31`), e `.btn.rosso` dentro `.azioni` **non è un selettore utilizzabile**, perché
lo stesso accoppiamento esiste in quattro posti dove spingere a destra sarebbe un danno:

- `Pages/Profile.razor:35` — «Esci» è l'unico pulsante della fila: finirebbe da solo a
  destra;
- `Pages/SpaceDetail.razor:128` — «Sì, elimina» è il **primo** dei due: `margin-left: auto`
  spingerebbe a destra anche «Annulla» che lo segue;
- `Shared/SchedaConflitto.razor:26` — «Sovrascrivi con la mia» si staccherebbe da «Ricarica
  la sua», che è la scelta gemella;
- `Pages/CollectionEdit.razor:187` — «Sì, togli» sta in mezzo a `↑ ↓ … Annulla`.

E nessun selettore posizionale regge, come il mandato aveva già escluso: `CollectionEdit`
ha tre blocchi `.azioni` e non tutti finiscono con l'azione distruttiva.

**La proposta, pronta da applicare se il capo la autorizza** (una riga di markup in un file,
più una regola in `app.css`, che è mio):

```razor
@* Shared/ConfermaAzione.razor:26 e :31 — la classe si aggiunge, .btn e .rosso restano *@
<button class="btn rosso azione-distruttiva" ...>
```

```css
/* app.css, nella sezione dei pulsanti, accanto a .azioni */
.azioni .azione-distruttiva { margin-left: auto; }
```

Effetto: nei cinque call-site del componente l'azione distruttiva va all'estremo opposto di
«Chiudi», e nel ramo armato «Sì, elimina» e «Annulla» si spostano **insieme** come gruppo,
il che è giusto — sono la stessa decisione. I quattro punti elencati sopra non sono toccati,
perché non passano dal componente. Non l'ho fatto: `Shared/ConfermaAzione.razor` non è nel
mio perimetro, e allargarmelo da solo sarebbe la cosa che il mandato vieta.

**2. Voce 8, `.scelta-categoria` → `.scelta-pastiglie` — non fatta, per mandato.** La classe
serve ora a due domini (categorie di spesa e icone di collezione) con un nome che parla solo
del primo. La rinomina costa tre sostituzioni in `Pages/Spese.razor:93`,
`Pages/SpesaEdit.razor:106`, `Pages/CollectionEdit.razor:96`, più due nel foglio di stile:
`.razor` che non sono miei, quindi il mandato dice di non farla e di scriverla qui. **Il mio
consiglio è di lasciarla**: `CollectionEdit.razor:78-82` porta già un commento che spiega
perché quel nome scomodo è stato riusato, quindi il difetto è documentato dove serve, ed è
nomenclatura.

**3. Il banner si sovrappone alla colonna di navigazione su schermo largo — trovato, non
corretto.** Con `margin: 0 var(--s5) 0 auto` e `max-width: 26rem` il banner sta a destra e
non tocca più la colonna: la sovrapposizione che c'era con `left: var(--s3)` **è sparita
come effetto collaterale** della modifica G4. Lo segnalo perché nessuno lo cerchi come
difetto residuo.

**4. `Pages/CollectionEdit.razor` non è stato toccato**, com'era previsto: l'inline resta,
e il perché è in `SCOSTAMENTI`.

## GATE

- `dotnet build -warnaserror` → **Compilazione completata. Avvisi: 0. Errori: 0.** Eseguita
  due volte: dopo le correzioni dei rilievi, e di nuovo alla fine dopo l'ultima rettifica di
  un numero di riga dentro un commento CSS. Entrambe 0 e 0.
- `dotnet test` → **Superato! Non superati: 0. Superati: 273. Ignorati: 0. Totale: 273.**
  Esattamente i 273 di partenza. Eseguito dopo l'ultima modifica di codice; ciò che è
  cambiato dopo è una cifra dentro un commento di `app.css`, che non entra nella
  compilazione dei test.
- `git status --porcelain` → i tre file previsti, nessun altro, nessun commit.
- Il server di sviluppo **non è stato avviato** e nessuna prova è stata fatta nel browser,
  come il mandato prescrive. Compilato una volta sola, a fine giro, con nessun agente attivo.

## SCOSTAMENTI

**1. `Pages/NoteEdit.razor` aperto, e perché il CSS non bastava.** Il mandato lo consente
«solo se il CSS non basta, e dichiarandolo». Non bastava, ed è dimostrabile leggendo:
`.corpo-nota` dichiara `min-height: 40vh` (`app.css:1412`), ma la `<textarea>` portava
`rows="16"`, che impone un'altezza intrinseca di sedici righe di mono — su una finestra
normale **più alta di 40vh**, quindi vinceva lei. Quel pavimento cambia col viewport: a
900px di altezza sono ~384px contro 360, a 1200px sono 384 contro 480. Nessun valore scritto
su `.markdown` avrebbe potuto pareggiare un bersaglio che si muove. Tolto `rows`, comanda
`min-height: 40vh` in tutti e due gli stati e i due box sono alti uguali **per costruzione**,
su ogni schermo. Il numero non è inventato: è quello che il progetto aveva già scelto.

La conferma che la diagnosi è giusta sta nel numero del rilievo: sedici righe di mono a
`--t-md` con `line-height: 1.6` fanno ~358px, cioè **esattamente** il salto misurato dalla
ricognizione.

**2. L'inline `font-size: var(--t-lg)` di `CollectionEdit.razor:105` resta, e non è un
rimedio parziale rimasto lì per dimenticanza.** Il mandato chiedeva di valutare se toglierlo
«perché due delle tre pastiglie avrebbero una misura del testo diversa dalla terza». Riaperto
il markup, la premessa non regge: non sono tre pastiglie uguali in tre pagine, sono
**pastiglie di testo** in `Spese.razor:97` e `SpesaEdit.razor:110` e **pastiglie di emoji**
in `CollectionEdit.razor:104`. Un'emoji a `--t-sm` non si riconosce, e il commento a
`CollectionEdit.razor:91-95` dichiara che quel corpo è lo stesso `--t-lg` della `.icona-input`
che le sta **accanto nello stesso fieldset** (`app.css:1426`): toglierlo farebbe stonare le
due cose vicine per far combaciare due cose lontane. La mia regola non tocca l'inline — che
vince su di essa per costruzione — e il commento a `app.css:1973-1981` scrive questa ragione,
così nessuno la riapre fra sei mesi. **Nessun `BLOCKED` serviva**: non ho dovuto toccare
`CollectionEdit.razor`.

**3. Voce 6, non lasciata a `live-testing`.** Il mandato ammetteva di lasciarla aperta se
l'origine non fosse stata determinabile leggendo. Lo era: `.nav-piede` centrava due figli di
altezza diversa — il `<label class="selettore">` (etichetta più campo da 48px, ~72px in
tutto) e `.voce-piede` (`2.25rem`, 36px). Centrare due cose alte 72 e 36 lascia le basi
sfalsate di `(72−36)/2 = 18px`, che è **esattamente** lo scarto misurato dalla ricognizione:
820 contro 838. Un numero che torna al pixel non è una coincidenza, quindi ho corretto invece
di rimandare.

**4. Voce 7, il banner: la seconda metà del rilievo era vera, e il rimedio è più grosso di
«Più tardi».** Il calcolo, tutto da lettura: il banner sta a `bottom: var(--s8)` = 72px dal
fondo ed è alto ~74px (48 di pulsante più 24 di padding più 2 di bordo), quindi occupava la
fascia **72-146px**; `.app-layout` riserva `padding-bottom: calc(var(--s8) + var(--s5))` =
**96px** sul telefono (`app.css:387`) e `var(--s7)` = **48px** su schermo largo
(`app.css:2133`). A pagina scorsa in fondo il banner si posava dunque sull'ultima fascia di
contenuto — negli editor proprio sopra la fila `.azioni` con «Salva» — per ~50px sul telefono
e ~98px su schermo largo.

Ho consultato **`tech-advisor`** su come rimediare senza poter misurare nel browser, perché
il rimedio ovvio (riservare spazio con `body:has(#aggiornamento-pwa:not([hidden]))`) richiede
di **indovinare** l'altezza del banner, che cresce quando il testo va a capo e ora ha due
pulsanti. La sua posizione, adottata: `position: sticky` invece di `fixed`. Un elemento
`fixed` è fuori dal flusso e il documento non sa che esiste; uno `sticky` sta nel flusso e si
riserva la propria altezza **qualunque essa sia**, e finché c'è pagina sotto resta incollato
a `--s8` dal fondo esattamente come prima. Il conto non dipende più dall'altezza: restano i
`--s5` di stacco fra fondo del contenuto e banner, su ogni schermo. Ho verificato io le due
condizioni che lo reggono: `html, body` non dichiarano né `overflow` né `height`
(`app.css:218-233`), quindi lo scroll container è la viewport; e il banner è l'ultimo box in
flusso di `<body>`, perché `#blazor-error-ui` che lo precede è `display: none` finché Blazor
non lo mostra, e allora è `position: fixed`.

La guardia `#app { min-height: 100vh; }` (`app.css:375`) è la contropartita: il worker può
proporre il banner entro millisecondi dall'avvio (`index.html:119`) mentre il boot WebAssembly
dura secondi, e in quella finestra `#app` contiene il solo «Caricamento…» — senza pagina sopra
di sé, uno sticky non ha a cosa incollarsi e il banner comparirebbe a mezza schermata per poi
saltare in fondo. A regime la riga non fa nulla: `.area-contenuto` (`app.css:377`) è già alta
almeno così.

**5. `backend-expert` non è stato chiamato.** Il gate lo vuole per superficie nuova,
astrazioni o diff oltre ~120 righe: qui il diff è di 105 righe e non introduce né tipi né
servizi né astrazioni — sono nove regole CSS e un pulsante. Il mandato nominava
`bug-hunter` e `conformity`, più `threat-hunter` in quanto ho toccato `index.html`: è
esattamente ciò che ho lanciato.

**6. Non ho aggiunto `a.btn.primario:not([href])` accanto alla regola della voce 2**, benché
la simmetria la suggerisse: oggi nessun `<a class="btn primario">` del progetto perde
l'`href` — quelli che esistono (`Home.razor:174`, `Home.razor:214`,
`CollectionDetail.razor:46`) hanno un indirizzo costante. Sarebbe stata una riga per un caso
inesistente. Lo dichiaro perché la scelta sia visibile: se un domani un editor spegnesse un
link primario, quella riga andrebbe aggiunta.

## DA PROVARE NEL BROWSER

Il mio lavoro è l'unico del piano che non si verifica leggendo il codice. Per ogni voce: la
misura attesa e come prenderla. Tutte le altezze si misurano con
`getBoundingClientRect().height` sull'elemento, i colori con `getComputedStyle`.

**Prova 1 — «Chiudi» spento si vede spento (voce 1).** Aprire `/notes/{id}` di una nota
esistente, modificare il titolo, premere «Salva» e guardare durante il salvataggio.
*Atteso*: l'`<a class="btn">Chiudi</a>` ha `opacity: 0.5`, `cursor: default`, nessun
`box-shadow`, e l'hover non lo schiarisce. *Prima*: identico a un pulsante attivo. Vale
uguale su `/expenses/{id}`, `/collections/{id}/edit`, `/collections/{id}/items/{id}`.

**Prova 2 — «Salva» spento non è più blu, ma «Segna» al lavoro sì (voce 2, e il rilievo
adjudicato).** Due misure distinte, e **la seconda è quella che verifica la correzione**:
- Aprire `/notes/new` senza scrivere niente: «Salva» deve avere
  `background-color` = `--superficie-alta` (**non** il blu `--accento`) e `opacity: 0.5`.
- Su `/expenses`, compilare il modulo e premere «Segna»: durante il salvataggio il pulsante
  deve restare **blu** e pulsare (`.btn.occupato`). Se qui diventa grigio, la correzione
  `:not(.occupato)` non ha preso ed è una regressione da riportare.

**Prova 3 — pastiglie prendibili col pollice (voce 3).** Su `/expenses`, `/expenses/{id}` e
`/collections/{id}/edit`, misurare l'altezza di **ogni** `.pastiglia` dentro
`.scelta-categoria`. *Atteso*: ≥ **48px** ciascuna (erano ~21px). Su
`/collections/{id}/edit` verificare in più che le pastiglie-emoji restino a corpo grande
(`font-size` ≈ `--t-lg`, l'inline deve vincere) e che siano alte quanto la `.icona-input`
accanto, anch'essa 48px.

**Prova 4 — l'anteprima non sposta più niente (voce 5).** Su `/notes/{id}` con una nota
**corta** (due righe): annotare `getBoundingClientRect().top` del `<div class="azioni">` in
modalità «Scrivi», premere «Anteprima», rimisurarlo. *Atteso*: **differenza 0** (era 358px).
Ripetere con una nota **lunga**: lì l'anteprima può essere più alta dell'editor e il
contenuto scendere — è corretto, cresce solo verso il basso. Verificare anche che la
textarea non si sia rimpicciolita: `.corpo-nota` deve essere alta **40vh**.

**Prova 5 — le basi della barra laterale (voce 6).** Portare la finestra a **1414px** di
larghezza, la stessa della ricognizione, e leggere `getBoundingClientRect().bottom` del
`<select>` dentro `.nav-piede .selettore` e quello di `.voce-piede`. *Atteso*: **lo stesso
valore**, scarto 0 (erano 820 e 838). Verificare che il selettore non si sia stirato: deve
occupare la larghezza restante con `.voce-piede` incollato al bordo destro.

**Prova 6 — il banner (voce 7).** In sviluppo il banner non compare da sé: mostrarlo con
`document.getElementById('aggiornamento-pwa').hidden = false` dalla console, che è
esattamente ciò che fa `proponi()`. Cinque misure:
- **a)** È visibile senza scorrere, ancorato a **72px** (`--s8`) dal fondo della finestra.
- **b)** *La prova che chiude il rilievo*: aprire `/notes/{id}`, scorrere fino in fondo con
  il banner visibile. La fila `.azioni` con «Salva» deve essere **interamente visibile**, e
  fra il suo bordo inferiore e il bordo superiore del banner devono esserci ~**24px**
  (`--s5`). *Prima*: il banner la copriva per ~50px.
- **c)** «Più tardi» nasconde il banner e non fa altro: nessuna richiesta di rete, nessuna
  chiave in `localStorage` o `sessionStorage` (controllare la scheda Application), e
  ricaricando la pagina in produzione il banner **torna** finché il worker resta in attesa.
- **d)** A **360px** di larghezza: testo e due pulsanti non si schiacciano — i pulsanti vanno
  a capo sotto il testo, ciascuno alto **48px**, e nessuna parola si spezza in colonna.
- **e)** Sopra **1024px**: il banner si accosta in basso a destra, largo al massimo **26rem**,
  e **non** si sovrappone alla colonna di navigazione a sinistra.

**Due effetti attesi dello sticky, da non scambiare per difetti.** Primo: quando il banner è
visibile, una pagina che stava esattamente in una schermata diventa scorribile di ~74px — è
lo spazio che il banner si riserva, ed è il punto della modifica. Secondo: sul telefono, a
scorrimento massimo, sotto il banner restano ~74px occupati dalla barra di navigazione, che
è dove il banner stava già prima.

**Cosa non ho potuto provare, e va detto.** Il banner **in condizioni vere** — cioè con un
service worker in attesa — si vede solo sul sito pubblicato: in sviluppo il worker è un
no-op, e mostrarlo dalla console prova il layout ma non la sequenza
`waiting` → `proponi()` → «Più tardi» → riapparizione al riavvio. Quella sequenza resta da
verificare al primo rilascio dopo questo lavoro.
