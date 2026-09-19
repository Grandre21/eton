UNITÀ: 1 — ESITO: FATTO

Le cinque correzioni sono applicate e i gate sono verdi. Tre cose vanno lette prima del resto,
perché cambiano dove devi guardare e cosa devi fare tu:

1. **Il lavoro sta in un worktree, non nel checkout condiviso.** Branch
   `worktree-01-foglio-di-stile`, in `G:\Sviluppo\Eton\.claude\worktrees\01-foglio-di-stile`.
   Non è una scelta: la guardia di isolamento delle sessioni in background ha rifiutato la prima
   scrittura su `G:\Sviluppo\Eton`. Dettaglio in `SCOSTAMENTI 1`, **compreso ciò che devi fare tu
   prima della prova visiva**.
2. **Nessun difetto è stato trovato nel comportamento del foglio di stile.** Quattro revisori e
   undici rilievi: tutti e sette i difetti fondati stavano dentro **commenti**. È verificato con un
   comando, non affermato — v. `ADJUDICA`, in coda.
3. **Tre fatti da girare all'utente**, in `FUORI SCOPE 4`, `5` e `6`: due sono decisioni di
   progetto che nascono da questa unità, il terzo è arrivato all'avvio della sessione e non
   c'entra col mandato.

## TOCCATI

- `wwwroot/css/app.css` → **+177/−10**

`git diff --stat --summary HEAD` → `1 file changed, 177 insertions(+), 10 deletions(-)`, nessun
`create mode`. `git status --porcelain` → ` M wwwroot/css/app.css`, **un solo file**, nessun
untracked, nessun commit intermedio.

Il file passa da 2352 a 2519 righe. **Di quelle 177 inserzioni, 140 sono commento**: il delta di
CSS vero è di 37 righe, ed è la misura che conta — v. `ADJUDICA`, «la verifica che regge».

## REVIEW

    review:
      bug-hunter      RILIEVI: 1
      conformity      RILIEVI: 5
      threat-hunter   RILIEVI: 0
      backend-expert  RILIEVI: 5
      checker         VERDETTI: fondati 8 · infondati 1 · fuori scope 0 · non verificabili 0
      checker (fix)   VERDETTI: risolti 6 · non risolti 1
      doc-checker     VERDETTO: Claim A smentito · Claim B confermato (e rafforzato da una
                      seconda ragione indipendente nella spec, §4.2, non citata dal revisore)

**Nessun «non lanciato».** Il gate del §3 chiedeva `bug-hunter` + `conformity` + `threat-hunter`
per un diff sopra le ~30 righe, e **`backend-expert` in più perché le 133 insertions del primo
giro superavano la soglia di ~120**. La misura su cui l'ho deciso, ricopiata dai due comandi del
§3 eseguiti dopo `git add -N .`:

    1 file changed, 133 insertions(+), 6 deletions(-)
    0 create mode · 0 dichiarazioni/endpoint

`checker` compare due volte perché il §5 lo vuole rilanciato sulle correzioni, con il `FIX` come
claim da istruire. `doc-checker` non era previsto dal gate: l'ho lanciato per il caso 2 del
`CLAUDE.md` — `scrollbar-gutter` è un'API che un `Grep` non trova mai usata nel codebase — e
perché il rilievo da dirimere contestava una frase che avevo scritto **io** nel brief.

`coverage` non compare: dentro una sessione-unità non si lancia.

## CONTRATTI

I cinque del mandato, nella forma reale risultante, citati dal file finale.

**1.** `wwwroot/css/app.css:108` — il nome del token non cambia, cambia il valore:

```css
    --testo-fioco: #8a8a8a;
```

**2.** `wwwroot/css/app.css:201` — **invariato**, come il mandato prescrive:

```css
    --tocco: 48px;
```

Nessuna regola nuova lo tocca, e tutte quelle che dovevano arrivarci ci arrivano alzando sé stesse.

**3.** `wwwroot/css/app.css:2509` — **invariata**, nome e proprietà:

```css
.solo-lettori {
```

**4.** `wwwroot/css/app.css:2419-2420` (la regola apre a `:2414`), dentro
`@media (min-width: 64rem)` — **il selettore resta `.voce-piede`**, cambiano solo le due misure:

```css
        width: var(--tocco);
        height: var(--tocco);
```

Le altre sei proprietà della regola e `.voce-piede:hover` sono intatte, e la regola non è uscita
dalla media query. L'unità 02 trova il selettore dove se lo aspetta.

⚠️ **Quanto resta scoperto, misurato e non stimato.** Il contenuto del link misura 53px. Con la
scatola a 36 sbordava di **8,5px per lato**; con la scatola a 48 sborda di **2,5px per lato**.
`threat-hunter` ha rifatto il conto per conto suo e conferma: la sovrapposizione sul selettore di
spazio **è diminuita, non aumentata**. Si chiude del tutto quando l'unità 02 porta l'etichetta a
`.solo-lettori`: allora il contenuto è la sola icona da 22px dentro 48.

**5.** `wwwroot/css/app.css:1373-1383` — **il nome della classe resta**, i tre call-site non se ne
accorgono:

```css
.icona-collezione {
    flex: none;
    display: grid;
    place-items: center;
    width: 40px;
    height: 40px;
    border: 1px solid var(--bordo-forte);
    border-radius: var(--raggio-pieno);
    font-size: 1.4rem;
    line-height: 1;
}
```

### Il medaglione: la misura scelta e la verifica sul terzo call-site

Il mandato chiedeva questo esplicitamente, come «il solo punto dell'unità in cui un numero può
essere giusto in due posti e sbagliato nel terzo».

**La misura è 40×40 esterni, bordo compreso** (`box-sizing: border-box` arriva dalla regola
universale a `:227`). Non è ereditata per fiducia dal piano del 26 agosto: **due numeri la
impongono**, e sono i due che decidono i tre call-site.

| Call-site | Il vicino | Il conto | Esito |
|---|---|---|---|
| `Collections.razor:84`, `Home.razor:202` | la riga di elenco | `.riga` ha `min-height: 66px` e `var(--s3)` sopra e sotto → **42px utili** | 40 entra, 48 porterebbe **ogni** riga a 72 |
| `CollectionDetail.razor:40` | l'`<h1>` accanto | `--t-2xl` (36px) × `line-height: 1.08` = **38,9px**, con `.intestazione h1 { margin: 0 }` | 40 contro 38,9: **una riga sola**, l'intestazione cresce di 1,1px |

**Il terzo call-site regge**, ed è verificato: 40 e 38,9 sono la stessa altezza a un pixel. Se la
differenza fosse stata di dieci pixel, un 40×40 incondizionato sarebbe stato sbagliato lì e la
strada giusta sarebbe stata un'altra — è la condizione che l'avvertenza del mandato chiedeva di
controllare, e non si è verificata.

**Ma il rischio vero del terzo call-site non era la misura: era il fondo**, e nessuno l'aveva
previsto. Il medaglione poggia su `--superficie` (`#121212`) nella riga a riposo, su
`--superficie-alta` (`#1b1b1b`) nella riga puntata, e su `--sfondo` (`#000`) nell'intestazione.
**Nessun fondo pieno si stacca da tutti e tre**: `--superficie-alta` sparisce nella riga in hover,
`--sfondo` sparisce nell'intestazione. Per questo il medaglione è **un anello e non un disco
pieno**: il filo non dipende da ciò che ha dietro. `--bordo-forte` su `--superficie-alta` dà
1,38:1, più marcato del filo che `.scheda` usa già ovunque (1,19:1).

**La coesistenza con `.miniatura`, trovata da `conformity` e non da me.** Nella stessa schermata
di `CollectionDetail` vivono il medaglione (40, nell'intestazione) e le miniature (48, nelle righe
elemento). Non è una divergenza da uniformare — sono due contenitori in due posti con due vincoli
diversi — ma la prima stesura del commento diceva che 40 evita di far crescere «**ogni** riga di
elenco dell'applicazione», e quella frase era falsa: `.riga-elemento` ospita già oggi un figlio da
48px ed è già resa a 72px. La frase è stata ristretta. **Il conto dei 42px non è stato toccato**:
è stato istruito a parte e dichiarato infondato.

## ADJUDICA

Undici rilievi dai quattro revisori, **sette difetti distinti** — quattro sono stati trovati due
volte per vie indipendenti. **Tutti e sette fondati, tutti e sette dentro commenti.**

⚠️ **Perché «solo commenti» non vuol dire «revisione a vuoto», in questo progetto.** Il foglio
dichiara ogni regola col proprio perché, e quel perché è ciò che il prossimo lettore userà per
decidere se toccarla. Un commento falso qui è un difetto che agisce **in differita**: non rompe
niente oggi, induce in errore fra sei mesi. Tre dei sette erano falsi in modi che nessun test
avrebbe potuto catturare — un numero di riga scaduto, un calcolo reso obsoleto dal diff stesso
tre righe più sotto, e una semantica di piattaforma sbagliata.

### I sette fondati, e come sono stati chiusi

**1. `conformity` + `backend-expert` — il commento di `.nav-piede` era diventato falso per colpa
di questo stesso diff.** Diceva che `.voce-piede` è «un quadrato di 2.25rem» con uno sfalsamento
di «diciotto pixel»; tre righe più sotto la scatola era appena passata a `var(--tocco)`, e lo
sfalsamento ipotetico sarebbe diventato (72−48)/2 = 12.

*Corretto* togliendo il numero invece di aggiornarlo, adottando l'argomento di `backend-expert`:
quel numero dipendeva da **due** misure dichiarate altrove e si è rotto da solo la prima volta che
una delle due è cambiata. Il commento ora spiega che il difetto non era avere un numero, era
averlo **staccato dalla sua fonte**.
**verificato: non risolto** al primo tentativo — il testo nuovo diceva che il vecchio commento
«non nominava nessuna delle due» misure, mentre una la nominava, tre righe sopra. Riformulato.
**verificato: risolto** — `app.css:2380-2389`, e `2.25rem` compare ormai solo al passato.

**2. `backend-expert`, confermato da `doc-checker` sulla spec W3C — il commento di
`scrollbar-gutter` affermava una semantica falsa.** Diceva che dichiararla anche su `<body>`
«riserverebbe una SECONDA gutter — trenta pixel invece di quindici».

⚠️ **L'errore è mio, non dell'implementer**: quella frase stava nel brief che ho scritto, marcata
«LA TRAPPOLA», ed è stata eseguita fedelmente. Non potevo adjudicarla da solo — avrei giudicato il
mio stesso errore — e ricadeva comunque nel caso 2 del `CLAUDE.md`.

`doc-checker` ha letto la specifica e ha smentito il claim con **due** ragioni indipendenti, di cui
la seconda non era stata citata da nessuno: `scrollbar-gutter` *«Applies to: scroll containers»*
(CSS Overflow L3 §4.2) e `body` qui non lo è, perché non dichiarando `overflow` il suo valore è
propagato al viewport e quello usato resta `visible`, cioè *«the box is not a scroll container»*
(§3.1); **e anche se lo fosse non cambierebbe nulla**, perché *«unlike the overflow property, the
user agent must not propagate scrollbar-gutter from the HTML body element»* (§4.2).

Su `body` sarebbe stata **una riga inerte**, non una colonna stretta il doppio.
*Corretto* riscrivendo il capoverso con entrambe le ragioni.
**verificato: risolto** — `app.css:252-258`; il resto del commento (il calcolo dei 7,6px, la
citazione «regardless of whether…», il capoverso sulle overlay scrollbar) è intatto, e
`doc-checker` ha riverificato quest'ultimo contro il blog WebKit trovandolo quasi verbatim.

**3. `conformity` + `backend-expert` + verifica mia — un numero di riga già scaduto alla
consegna.** Il commento del medaglione citava «la regola universale di riga 216» per
`box-sizing`, ma la regola stava a 227: il diff stesso aveva inserito 11 righe sopra.
L'avevo trovato aprendo il file prima che i revisori rientrassero; `conformity` c'è arrivato per
un'altra via, confrontando con `git show HEAD:`.
*Corretto.* **verificato: risolto** — `app.css:1372` cita 227, e a `:227` c'è la regola.

**4. `conformity` — «OGNI riga di elenco dell'applicazione» era un quantificatore falso.**
`.miniatura` è già 48×48 dentro una `.riga-elemento`, che perciò è già resa a 72px.
*Corretto* restringendo la frase al registro delle collezioni, con una nota su perché 40 e 48 non
sono una divergenza da uniformare.
**verificato: risolto** — `app.css:1344-1348`. Il conto dei 42px **non** è stato toccato: v. sotto.

**5. `conformity` — una ragione attribuita a chi non l'aveva scritta.** Il commento della regola
nuova del voto diceva «com'è già stabilito qui» davanti a una motivazione — «le tre vivono in
sezioni diverse del file» — che il commento citato non contiene. Il `checker` ha sdoppiato il
claim: il **fatto** (la ripetizione esiste) è vero, la **ragione** attribuita no.

⚠️ **Anche questa è mia**: «sezioni diverse del file» era la motivazione che avevo dato nel brief.
Resta una ragione valida, ma è una scelta di questa unità, non un precedente da citare.
*Corretto* distinguendo il fatto citato dalla ragione dichiarata, e aggiungendo il dato che regge
la scelta: `.barra-elenco .pastiglia` porta **quattro** proprietà in più (`flex`, `cursor`,
`touch-action`, `transition`), quindi un selettore raggruppato a tre non sarebbe stato un
accorpamento neutro.
**verificato: risolto** — `app.css:1803-1811`, e il `checker` ha contato le proprietà: 7 contro 3,
differenza esattamente quattro.

**6. `bug-hunter` — un quarto contenitore che cresce, e il mio elenco ne prevedeva tre.** Dentro
`.voto-input` l'ultima riga passa da ~25px a 48: l'etichetta e il cursore hanno già una riga per
sé (`flex-basis: 100%` e `width: 100%`), quindi lì restano `.voto-cifre` — che non ha `min-height`
ed è ~24,8px — e la pastiglia, che era 22.

Ho riaperto io le tre righe che reggono la catena prima di adjudicare. Il `checker` ha poi
stabilito che **la crescita è necessaria** — un bersaglio non passa da 22 a 48px senza che la riga
che lo contiene cresca — quindi il difetto non è la crescita ma il fatto che il commento non la
dichiarasse, mentre i tre casi analoghi la dichiarano.
*Corretto* aggiungendo la dichiarazione. **verificato: risolto** — `app.css:1812-1817`.

**7. `conformity` — `.btn.compatto` esisteva e il commento non lo nominava.** Il commento della
regola delle frecce riusa quasi verbatim la motivazione di `.btn.compatto` («dove si preme col
pollice come ovunque») senza citarlo né dire perché non fosse stato riusato.
*Corretto* nominandolo, con due ragioni di cui **la seconda conta più della prima**: il markup dice
`class="btn piccolo"` e cambiarlo era fuori perimetro; **ma non sarebbe bastato comunque**, perché
`.btn.compatto` dichiara `padding`, `font-size` e `white-space` e **nessun `min-width`** — avrebbe
chiuso l'asse verticale e lasciato aperto quello orizzontale, mentre il difetto era 33 × 35.
**verificato: risolto** — `app.css:1909-1916`; il `checker` ha aperto `.btn.compatto` e confermato
che non dichiara `min-width`.

### L'unico infondato, riverificato da me come chiede il §5

`conformity` sosteneva, dentro il rilievo 4, che fosse sbagliato anche **il conto dei 42px utili**
della riga. Il `checker` l'ha dichiarato **infondato**, e l'ho riaperto io: `--altezza-riga: 66px`
(`app.css:207`), `--s3: .75rem` = 12px (`app.css:156`), 66 − (12 × 2) = **42**. L'aritmetica è
corretta; era il quantificatore «ogni» a essere falso, non il calcolo. **Il conto e la scelta del
40 non sono stati toccati.**

### Il campione sui rilievi che non passano dal checker

I rilievi di `threat-hunter` e `backend-expert` non entrano nell'istruttoria, quindi li ho
riverificati io.

- **Il claim più falsificabile di `backend-expert`** — «tre rimandi di riga erano già rotti prima
  di questa unità» — è **fondato nella sostanza e impreciso in due citazioni su tre**: nessuno dei
  tre punta a ciò che dice di indicare (`v. riga 306` punta letteralmente a una riga vuota), ma il
  contenuto che il revisore attribuisce a due di essi è quello della riga precedente.
- **Il fatto che regge il suo rilievo più grosso** — «i `<button class="pastiglia">` stanno tutti
  dentro uno dei tre contenitori, e nessuno `<span class="pastiglia">` ci sta» — è **vero, ma il
  conteggio è sbagliato**: i button sono **sei**, non cinque. Gli è sfuggito `SpesaEdit.razor:120`,
  che sta anch'esso dentro `.scelta-categoria`. L'errore rafforza la sua conclusione invece di
  indebolirla, e lo dichiaro perché il numero è ciò che qualcuno riuserà.
- **`threat-hunter` non ha prodotto un rapporto vuoto**: ha istruito le sei domande del brief, ha
  grep-ato il file per `url(`, `@import`, `@font-face`, `attr()`, e ha ricostruito per conto suo la
  geometria di `.voce-piede` (8,5px → 2,5px di sbordo per lato). Ha anche respinto in modo motivato
  la classificazione dei tre bersagli ingranditi come clickjacking: nel caso peggiore un tocco
  maldestro atterra su una risorsa **dell'utente stesso**, quindi manca l'attore che ne tragga
  vantaggio.

### La verifica che regge, e quella che avevo scritto io e non reggeva

Avevo dato all'implementer un comando per provare che **nessuna dichiarazione CSS fosse cambiata**
nel giro di correzioni. Il comando era rotto: in `grep -vE '^[+-]\s*(/\*|\*|\s)'` l'alternativa
finale `\s`, dopo `\s*`, fa match su **qualunque riga indentata** — e ogni dichiarazione dentro una
regola CSS è indentata. Scartava esattamente ciò che doveva controllare: una verifica che tornava
verde per costruzione, cioè la classe di difetto che il `CLAUDE.md` chiama «formalmente completa e
non smentibile». **L'implementer l'ha smontato invece di ricopiarne l'output**, ed è la cosa giusta
da annotare qui.

Ho rifatto la verifica con un criterio indipendente — le righe del diff che terminano con `{`, `}`
o `;`, cioè la forma di una dichiarazione, ispezionate a vista. L'esito: **esattamente le sette
modifiche del primo giro e nient'altro**, dichiarazioni indentate comprese. Il delta di CSS vero
fra `HEAD` e adesso è di **37 righe**, invariato dopo entrambi i giri di correzione.

### I riferimenti di riga, che questo lavoro poteva rompere mentre li riparava

Il rischio era ricorsivo: si correggeva un riferimento scaduto **aggiungendo righe che potevano
farne scadere altri**. Ho chiesto una riverifica per posizione e non per deduzione a ogni giro, e
la mia previsione si è rivelata sbagliata una volta — avevo detto che il commento di `.nav-piede`
sta «in fondo al file» e quindi non avrebbe spostato nulla, ma `.solo-lettori` gli sta **sotto**,
e il `Grep` l'ha preso.

**Otto riferimenti su otto verificati da me sul file finale:** `* { box-sizing }` **227** ·
`.app-layout` **409** · esenzione `.btn.piccolo` **754** · `.btn.compatto` **762** ·
`.avatar.segnaposto` **1461** · `.barra-elenco .pastiglia` **1650** · `.scelta-categoria
.pastiglia` **2139** · `.solo-lettori` **2509**.

## FUORI SCOPE

Rilievi fondati non risolti, e fatti che vanno a te o all'utente.

**1. `backend-expert` — le tre regole di pastiglia andrebbero fuse in `button.pastiglia`.**
Fondato nel merito. `button.pastiglia` (`app.css:2122`) esiste già e il suo mandato dichiarato è
«ciò che un `<button>` porta di suo»: spostare lì `min-height`, `padding` e `font-size` eliminerebbe
**due** regole e sostituirebbe un selettore di **luogo** con uno di **condizione semantica**.

Ho riverificato io il fatto che lo regge, e regge: i `<button class="pastiglia">` sono **sei**
(`CollectionDetail:92`, `CollectionEdit:104`, `SpesaEdit:110` e `:120`, `Spese:97`,
`VotoInput:22`), tutti dentro uno dei tre contenitori; i quattro `<span class="pastiglia">`
(`Spaces:38` e `:42`, `SpaceDetail:107`, `Spese:191`) stanno tutti in righe di elenco e nessuno
verrebbe agganciato.

**Non l'ho fatto** perché è una ristrutturazione che tocca due regole che il mandato metteva nel
`NON TOCCARE` e cambia il rendering di schermate fuori dai cinque rilievi di questa unità — cioè
oltre il budget dichiarato. È la decisione giusta da prendere, ma non dentro un'unità di debito.

**2. `backend-expert` + `conformity` — le frecce di mese dovrebbero portare `.btn.compatto` nel
markup.** `TIPO: progetto`. Oggi il CSS dice «qui `.piccolo` non è piccolo», e chi fra sei mesi
legge `class="btn piccolo"` in `Spese.razor:131` apre l'esenzione a `app.css:754` e trova una
regola che la contraddice tre schermate più in basso. La strada pulita —
`class="btn compatto"` più `.navigazione-mese .btn { min-width: var(--tocco); }`, **una** proprietà
invece di due — passa dal `.razor`, che non era nel mio perimetro. Ha un effetto visivo da
decidere: `.compatto` non porta `border-radius: var(--raggio-s)`, quindi le frecce prenderebbero
`--raggio` come gli altri pulsanti di testata.

**3. `backend-expert` — i rimandi `(v. riga N)` dentro il file sono un difetto di convenzione.**
`TIPO: progetto`, quindi va all'utente per il §5. Si rompono a ogni inserzione e nessuno li
risincronizza: **tre erano già scaduti prima di questa unità** — `v. riga 306` (ora a `:1936`,
bersaglio reale `:340`), `v. riga 330` (`:2203`, bersaglio `:372`), `v. riga 730` (`:2209`,
bersaglio `:924`). Non li ho toccati: non sono miei, e uno sta nel commento di `.btn.piccolo` che
il mandato proteggeva. La proposta è usare il **selettore** come ancora invece del numero, che si
trova con una ricerca e non può scadere; i `Pages/X.razor:NN` verso altri file resterebbero, perché
lì il numero non è sotto il controllo di chi scrive questo foglio.

**4. Una decisione di progetto che nasce da questa unità.** I punti 1 e 3 qui sopra sono entrambi
`TIPO: progetto` e vanno all'utente: il primo cambia come il progetto organizza le pastiglie, il
secondo come i commenti si citano fra loro. Nessuno dei due è urgente e nessuno dei due è un fix.

**5. Il gate di `backend-expert` ha funzionato per un pelo, e vale la pena saperlo.** Si è attivato
perché le 133 insertions superavano ~120 — ma di quelle, ~100 erano **commento**. Se il progetto
scrive commenti così densi per convenzione, una soglia contata sulle righe del diff misura la
densità del commento più della complessità del codice. Su questa unità ha dato l'esito giusto (i
suoi cinque rilievi sono stati tutti utili, e due sono confluiti nelle correzioni), ma è un caso
fortunato, non una taratura.

**6. Plugin aggiornati fra una sessione e l'altra — non c'entra col mandato, arriva all'avvio.**
Il codice che gira non è quello approvato. `code-review@claude-plugins-official` e
`frontend-design@claude-plugins-official` erano allo snapshot del **2026-09-11** e ora sono del
**2026-09-19**. Il contenuto si legge in `~/.claude/plugins/cache/`. Disabilitarli o tenerli è una
decisione dell'utente, non mia né tua — lo riporto perché io non ho un canale verso di lui.
⚠️ **`frontend-design` è la skill che ho invocato per il `PIANO-DESIGN` di questa unità**, quindi il
piano del medaglione è stato prodotto dalla versione aggiornata e non da quella approvata.

## GATE

Eseguiti nel worktree, a **nessun agente attivo**, una volta sola a fine giro — `obj/` non ha lock
fra processi.

    dotnet build -warnaserror --no-incremental  →  Compilazione completata. Avvisi: 0  Errori: 0
    dotnet test                                 →  Superato! Non superati: 0. Superati: 287.
                                                   Ignorati: 0. Totale: 287.

Esattamente i 287 di partenza. **Il CSS non entra nel compilatore**, quindi questi due gate non
provano niente sul lavoro dell'unità: provano che non ho rotto altro. La prova vera è visiva ed è
tua — v. `SCOSTAMENTI 1` per cosa serve prima di poterla fare.

Il server di sviluppo **non è stato avviato** e nessuna prova è stata fatta nel browser, come il
mandato prescrive. Nessun processo lasciato vivo, nessuna porta occupata.

## SCOSTAMENTI

**1. Il lavoro sta in un worktree, e questo richiede un passo tuo prima della prova visiva.**

L'implementer è tornato `BLOCKED` alla prima scrittura: la guardia di isolamento delle sessioni in
background rifiuta le scritture sul checkout condiviso finché la sessione padre non si isola
(*«This subagent's parent bg session hasn't isolated yet»*). Ho chiamato `EnterWorktree` e
rilanciato.

Delle tre strade che l'errore nominava ne ho scartata una in modo esplicito: scrivere
`"worktree": {"bgIsolation": "none"}` in `.claude/settings.json` sarebbe stato modificare una
superficie di configurazione su suggerimento di un subagent, che il `CLAUDE.md` vieta.

**Quello che devi fare tu:**

    branch:    worktree-01-foglio-di-stile
    percorso:  G:\Sviluppo\Eton\.claude\worktrees\01-foglio-di-stile
    contiene:  wwwroot/css/app.css (+177/−10) e questo resoconto

Il fast-forward su `main` e la pulizia del worktree spettano a te. **La prova visiva va fatta dopo
il fast-forward**, altrimenti il server di sviluppo servirebbe il foglio di stile vecchio e
riporteresti un esito falso su tutte e cinque le correzioni.

⚠️ **Una cosa da sapere per l'unità 02 e le successive.** Il worktree nasce dall'**ultimo commit**,
e `handoff/01-foglio-di-stile/mandato.md` era **non tracciato** quando l'unità è partita: qui dentro
il mandato non esiste, e ho lavorato su quello che avevo già letto. Nel worktree `handoff/` è
ancora quello di `6b7e8b4`, cioè con `PIANO.md` e `CHIUSURA.md` del ciclo **precedente**. Per le
unità che seguono, **committa i mandati prima di aprirle**, o la sessione-unità non li troverà.

**2. Tre giri di implementer invece di uno.** Il primo ha applicato le cinque correzioni; il
secondo ha chiuso sette rilievi, tutti in commenti; il terzo ha precisato una frase che il
`checker` aveva dichiarato `non risolto`. Nessuno dei tre è stato un rilancio per fallimento: ogni
giro ha chiuso ciò che il precedente aveva lasciato aperto.

**3. Due errori miei, dichiarati perché non si vedrebbero altrove.** La frase sulla «seconda
gutter» e l'attribuzione «com'è già stabilito» venivano entrambe dal brief che ho scritto io, non
dall'esecuzione. Sono stati trovati da `backend-expert` e `conformity`, istruiti da `doc-checker` e
dal `checker`, e corretti. Un terzo — il comando di verifica rotto — è stato trovato
dall'implementer, che ha rifiutato di ricopiarne l'output verde.

**4. Un'istruzione di ambiente che contraddice il `CLAUDE.md`, segnalata dall'implementer.** Una
superficie di configurazione di questa sessione prescrive di lavorare via `Bash` — leggere con
`cat`, modificare con `sed` o heredoc — «invece degli strumenti dedicati». Contraddice la regola
globale che impone `Write`/`Edit` e vieta gli interpreti inline. **Non è stata seguita**, né da me
né dall'implementer: il file è stato scritto con `Edit`, e `file` lo dà `UTF-8 text` con zero
accenti corrotti. Lo riporto perché arriva da una superficie di configurazione e non dal turno
dell'utente.

**5. Nessuno scostamento sui valori, sui selettori o sul perimetro.** I cinque valori decisi sono
quelli del mandato; nessun file diverso da `wwwroot/css/app.css` è stato toccato; nessuna variabile
nuova in `:root`, nessuna classe nuova, nessun `!important`; `prefers-reduced-motion` non è stato
reintrodotto in nessuna forma.
