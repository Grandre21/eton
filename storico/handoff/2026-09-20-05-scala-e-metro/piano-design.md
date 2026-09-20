# PIANO-DESIGN — unità 05

> **Metro SCELTO, non misurato.** Nessun sito è stato aperto per produrre questi valori: il ramo
> del §0 che si applica qui è il secondo (nessun sito da imitare), quindi ciò che segue sono
> decisioni, non misure. Uno scostamento da questo piano è una cosa **da discutere**, non un
> errore da correggere — che è invece ciò che sarebbe sotto una `SPECIFICA-UI`.
>
> Prodotto invocando la skill `frontend-design`, prima passata più revisione, il 20 settembre 2026.

Il brief è atipico e vale dichiararlo: **non si progetta una schermata nuova**. Il sistema esiste, è
maturo, ed è dichiarato in prosa nell'intestazione di `app.css`. Il compito della skill qui non è
proporre una direzione — sarebbe scavalcare una decisione già presa dall'utente — ma dare un metro
con cui giudicare **tre questioni di sistema** che il foglio lascia irrisolte.

---

## COLORE — sei valori nominati, e la domanda è quanti livelli siano

    --sfondo           #000000   nero pieno: il fondo della pagina, e il fondo INCASSATO dei campi
    --sfondo-alt       #0a0a0a   oggi un quarto livello che la prosa del foglio non dichiara
    --superficie       #121212   livello 1: il piano rialzato con un filo di bordo
    --superficie-alta  #1b1b1b   livello 2: ciò che deve staccarsi davvero — più gli stati
    --accento          #4c8dff   blu: dove si PREME
    --secondario       #b6f36a   verde acido: dove si CONSTATA

**La decisione di colore non è quale nero**, che è già scelto e giustificato (nero pieno perché su
OLED un #0d0d0d è un grigio che si vede). È **quanti gradini servano**, e la risposta di questo piano
è **quattro, non tre — ma perché non sono una scala di elevazione, sono quattro ruoli.**

La prosa del foglio dice «gerarchia di superficie, tre livelli», cioè descrive una scala in cui il
fondo sale con l'importanza. Il censimento dei 49 fondi reali dice che non è così, e il caso che lo
rompe è il più comune di tutti: **`.btn` sta a `--superficie-alta`, il fondo più chiaro dei quattro**,
e un pulsante secondario non è «ciò che deve staccarsi davvero, ed è raro» — è dappertutto.

I ruoli veri, letti dai fatti:

| Fondo | Ruolo | Dove |
|---|---|---|
| **#000** `--sfondo` | ciò in cui si **entra**, e i contorni | la pagina, i campi di testo, `.pastiglia`, `code`, `pre` |
| **#0a0a0a** `--sfondo-alt` | ciò che **contiene controlli** | `.modulo-spesa`, `.schede-testo`, `.alla-cieca`, la colonna larga |
| **#121212** `--superficie` | ciò che si **legge** | `.scheda`, `.registro`, `.markdown`, `.riga-campo` |
| **#1b1b1b** `--superficie-alta` | ciò che si **preme**, e gli **stati** | `.btn`, `.riga:hover`, `:disabled`, `.aiuto-pannello`, il banner PWA |

**Il principio che ne esce, e che il foglio non ha mai scritto: un campo è un buco, un pulsante è un
rilievo.** Ci si scrive *dentro* un campo, e il suo fondo è il più scuro dei quattro; ci si preme
*sopra* un pulsante, e il suo fondo è il più chiaro. Il contenitore dei campi si mette in mezzo, appena
sopra il buco. È una regola fisica, e per questo si verifica guardando una schermata invece di
contare i token — che è ciò che «tre livelli» non permette di fare, perché non dice cosa vada su quale.

**Due eccezioni che questo principio non spiega, e che vanno dichiarate invece che appianate:**
`.titolo-nota`, `.titolo-grande` e `.corpo-nota` sono campi di testo a **#121212** invece che a #000
come tutti gli altri campi; e `.pastiglia` sta a #000 mentre `.btn` sta a #1b1b1b, cioè due cose
premibili sui due fondi opposti. Possono essere due scelte o due derive: il foglio non lo dice, e
**questo — non il numero dei token — è il lavoro della fase 2.1-bis.**

---

## TIPO — due voci, un discrimine, e una scala che va difesa

    Inter, variabile 100-900     la PROSA: titoli, frasi, etichette di campo
    IBM Plex Mono, 400 e 600     i DATI: voti, medie, conteggi, date, codici

    scala   11 · 13 · 15 · 16 · 20 · 26 · 36 · 52

Le due voci e il loro discrimine sono la cosa meno templated di Eton e non si toccano. Quello che
questo piano aggiunge riguarda la scala:

**Una scala esiste solo se nessun corpo vive fuori di lei.** Un gradino è una decisione; un valore
prodotto da un'aritmetica è un residuo. `12,35px` e `15,2px` non sono gradini scelti male: sono il
risultato di `.95 × qualcosa`, e nessuno li ha scritti. La differenza conta perché una scala serve a
**riconoscere** — chi guarda deve poter dire «questo è il gradino dei dati secondari» — e un corpo che
sta fra due gradini non si riconosce, si subisce.

**La correzione ottica è legittima, il modo in cui è espressa no.** Che il mono appaia più grande
dell'Inter a parità di corpo è vero, e pareggiarlo è il mestiere di un tipografo. Ma espressa in `em`
la correzione **si moltiplica per il contesto** invece di applicarsi al carattere: dentro un `.meta`
da 13px dà 12,35, dentro la prosa da 16 dà 15,2, e dentro una classe che dichiara il proprio corpo
non dà niente perché perde la cascata. Una correzione che agisce in tre modi diversi a seconda di dove
cade non è una correzione: è un effetto collaterale con una buona intenzione.

**Principio: la correzione ottica appartiene al FONT, non alla regola.** Se si vuole tenerla, la sua
forma è `font-size-adjust`, che pareggia le altezze-x lasciando intatto il corpo dichiarato; se non la
si vuole, si toglie e i corpi tornano sui gradini. Quello che non regge è la terza via attuale, in cui
la correzione c'è in quattro punti su sedici e altrove no.

---

## LAYOUT — il metro del bersaglio, e perché non è uno solo

Il concetto di layout di Eton è già fissato e non è in discussione: colonna unica su telefono con la
navigazione in fondo, colonna laterale fissa da 248px sopra 64rem, contenuto **allineato a sinistra**
salvo gli stati vuoti, che sono centrati perché lì l'unica cosa da fare è una sola.

    telefono (base)                 schermo largo (≥64rem)
    ┌───────────────────────┐       ┌────────┬────────────────────────┐
    │ [selettore]  [Profilo]│       │ marchio│  h1                    │
    │ h1                  ? │       │ ─────  │  ────────────────────  │
    │                       │       │ voci   │  contenuto             │
    │  contenuto            │       │  ·     │                        │
    │                       │       │  ·     │                        │
    ├───────────────────────┤       │ ─────  │                        │
    │  ·  ·  ·  ·  ·        │       │ spazio │                        │
    └───────────────────────┘       └────────┴────────────────────────┘
      barra in fondo: 48px            colonna: voci a 44px oggi

**La decisione di layout di questo piano è una sola, e riguarda il pavimento del bersaglio.** Un
pavimento di tocco è una misura del **dito**, non della vista: 48px è il metro di Material e delle
linee guida, 44px è il minimo sotto cui il colpo sbaglia, e 24px è ciò che WCAG 2.5.8 chiede in AA.
Tre numeri diversi perché rispondono a tre domande diverse.

**Il principio: un pavimento vale dove vale il suo dominio, e il dominio va scritto accanto al
numero.** Sulla barra in fondo al telefono il dito c'è e il metro è 48. Nella colonna laterale sopra
64rem il puntatore è un mouse e 44 è una scelta difendibile — ma oggi quel 44 non porta nessuna
ragione scritta, e un numero senza ragione è indistinguibile da una svista. Il difetto non è il
valore: è che **il foglio non dice mai a quale puntatore si stia rivolgendo**.

---

## PRINCIPI — le quattro righe che rendono questo foglio diverso da un foglio qualunque

1. **Il colore porta informazione, non decorazione.** Blu dove si preme, verde dove si constata, e il
   discrimine è «premibile contro constatato», non «numero contro testo». Un ✓ accanto a «non
   vendiamo i tuoi dati» è verde perché è un fatto.
2. **Il carattere porta la stessa informazione della cornice.** Inter si legge, Plex Mono si
   consulta. Il mono su una frase intera è sempre un errore.
3. **Un campo è un buco, un pulsante è un rilievo.** Il fondo non misura l'importanza, misura il
   gesto: ci si scrive dentro, ci si preme sopra. Quando ogni cosa è un riquadro, nessuna lo è.
4. **Ogni numero del foglio dichiara il proprio dominio.** Un 44 senza «perché qui il puntatore è un
   mouse» accanto è un numero che il prossimo lettore correggerà per sbaglio, o peggio, lascerà per
   paura.

---

## LA REVISIONE CONTRO IL BRIEF — dove Eton somiglia a un default, e dove no

La skill chiede una seconda passata che cerchi nel piano ciò che assomiglia al default prodotto per
qualunque brief simile. **Eton porta tre dei tratti che la skill elenca come segni di un design
generato**, e vale dirlo all'utente perché è un'informazione che non ha:

- **fondo quasi-nero con un solo accento verde acido** (tratto 2 dell'elenco);
- **un carattere monospace per le etichette di dato** (tratto 5, «template chrome»);
- **stringhe di meta unite da punti mediani** — `@Meta(c) · <span class="dato">` in
  `Pages/Collections.razor`, e lo stesso schema altrove.

**Ma due dei tre non sono default, e la differenza è verificabile invece che opinabile.** Un tratto è
un default quando compare *a prescindere dal soggetto*. Qui il verde non è un accento: è **metà di un
sistema semantico** in cui l'altra metà è il blu, e la regola che li separa è scritta, si applica a
ogni schermata e si può falsificare guardandone una. Lo stesso per il mono: non veste «le etichette
piccole» perché stanno bene in mono, veste **i dati**, e il foglio dichiara che il mono su una frase è
un errore. Un default non si lascia falsificare, perché non fa nessuna affermazione.

Il terzo — il punto mediano — **è un default**, e non ha nessuna giustificazione nel foglio. Non lo
porto come rilievo di questa unità perché sta nel markup e non nel mio perimetro, ma è la cosa più
templated che Eton abbia, ed è anche la più economica da cambiare.

**Cosa ho cambiato, e sono due revisioni non una.**

*Prima revisione.* La stesura iniziale raccomandava di **ridurre le superfici da quattro a tre**, per
far combaciare il foglio con la propria prosa. Scartata: era la risposta ovvia alla domanda posta («la
prosa ne dichiara tre, i fatti ne usano quattro»), e arrivava a una scala più povera senza aver
guardato *cosa* distingue il quarto.

*Seconda revisione, e correggeva la prima.* Il principio che avevo messo al suo posto — «il fondo
scende quando il contenuto sale» — è durato finché non ho guardato il selettore a riga 718: **`.btn`
sta al fondo più chiaro dei quattro**, e un pulsante è esattamente un controllo, cioè la cosa che
secondo quel principio doveva stare in alto sul fondo basso. La regola era elegante e falsa, e a
smentirla è bastato il selettore più comune del foglio. Quella che c'è adesso — *un campo è un buco,
un pulsante è un rilievo* — regge su tutti e quattro i fondi e lascia fuori due eccezioni, che sono
dichiarate sopra invece di essere appianate.

**La lezione, che vale oltre questo piano:** una regola di design si scrive dopo aver censito i casi,
non prima. Le due formulazioni scartate erano entrambe più ordinate di quella finale — ed è il motivo
per cui erano sbagliate: **descrivevano la scala che avrei voluto trovare, non quella che c'è.**
