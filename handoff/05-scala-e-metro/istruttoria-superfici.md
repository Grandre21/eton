# Istruttoria della voce 1 — le superfici

> Documento di lavoro dell'unità 05. La voce 1 è l'unico rilievo di `ui-critic` che nomina una
> **regola assente** invece di un valore sbagliato, e il mandato la chiama «l'ingresso naturale della
> fase 2.1-bis». Qui ci sono i fatti censiti; la decisione non è di questa unità.
>
> **I rimandi sono ai selettori e mai ai numeri di riga**, come il foglio prescrive di sé: tre unità
> lo hanno riscritto in poche ore e qualunque riga citata oggi è scaduta domani.

---

## LA PREMESSA DEL RILIEVO NON REGGE, E VA DETTO PRIMA DI TUTTO

Il mandato riporta che la ricognizione ha misurato «**una ventina di dichiarazioni di fondo sparse**
che non usano i token o li usano in modo incoerente».

Contate ora, una per una, le dichiarazioni di fondo in `app.css` sono **49**. Di queste:

- **44 usano un token** — `var(--qualcosa)`, incluse le due composte: il `color-mix()` della barra di
  navigazione e il `linear-gradient()` del marchio, che partono entrambe da token;
- **5 non lo usano**, e di queste **cinque sono `transparent`**, che non è un colore ma un'assenza —
  serve a spegnere un fondo ereditato, e un token per «niente» non avrebbe senso.

**Le dichiarazioni davvero fuori dalla tavolozza sono due**, non venti:

| Dichiarazione | Dove | Giudizio |
|---|---|---|
| `background: #ffe08a` (e `color: #202020`) | `#blazor-error-ui` | **legittima, ma non dichiarata** |
| `background: rgba(0, 0, 0, .6)` | il velo del popover di aiuto | **legittima, ed è un velo** |

Il primo è il riquadro giallo che Blazor mostra quando l'applicazione è **morta**: è l'unico momento
in cui non si può contare su niente di ciò che il foglio costruisce, ed è esattamente il motivo per
cui un colore a mano lì è la scelta giusta. Ma l'intestazione del foglio dichiara, senza eccezioni,
che «un colore scritto a mano dentro una regola è un colore che non seguirà il giorno in cui la
tavolozza cambia» — **e questa eccezione non è scritta da nessuna parte**. Costo per chiuderla: una
riga di commento. Non è un difetto di colore, è un divieto assoluto con un'eccezione taciuta.

Il secondo è un velo di oscuramento, non una superficie: `rgba(0,0,0,.6)` sopra il contenuto mentre il
pannello è aperto. Nessun token della tavolozza è un nero trasparente, e inventarne uno per un solo
uso sarebbe peggio.

**Quindi il rilievo, come formulato, è infondato: i token esistono e sono usati.** Ma sotto di esso ce
n'è uno vero e più grosso, che il resto di questo documento istruisce.

---

## IL FATTO VERO — la prosa descrive una scala di elevazione, i fatti sono quattro ruoli

L'intestazione di `app.css` dichiara:

> «GERARCHIA DI SUPERFICIE, TRE LIVELLI. Il fondo non ha bordi e ospita quasi tutto. Il livello 1
> (`.scheda`, `.registro`) è un piano rialzato con un filo di bordo. Il livello 2 (`.superficie-alta`
> più ombra tinta) è per ciò che deve staccarsi davvero, ed è raro.»

Sono **ventitré** le dichiarazioni che usano uno dei quattro token di superficie. Ecco tutte, per
token e per selettore:

**`--sfondo` · #000000 · sei usi**
`html, body` · la regola base dei campi di testo · `.codice-grande` · `.pastiglia` ·
`.markdown code` · `.markdown pre`

**`--sfondo-alt` · #0a0a0a · quattro usi**
`.alla-cieca` · `.schede-testo` · `.modulo-spesa` · `.nav-app` (dentro `@media (min-width: 64rem)`)

**`--superficie` · #121212 · otto usi**
`.scheletro-riga` · `.scheda` · `.interruttore` · `.registro` · `.titolo-nota, .titolo-grande` ·
`.corpo-nota` · `.riga-campo` · `.markdown`

**`--superficie-alta` · #1b1b1b · cinque usi**
`#aggiornamento-pwa` · `.btn` · `.btn.primario:disabled:not(.occupato)` · `.aiuto-pannello` ·
`.riga:hover`

### Le due affermazioni della prosa che i fatti smentiscono

**1 — «il livello 2 è per ciò che deve staccarsi davvero, ed è raro».** `--superficie-alta` è il fondo
di **`.btn`**, cioè di ogni pulsante secondario dell'applicazione. Non è raro: è dappertutto. E gli
altri quattro usi sono uno stato di passaggio (`:hover`), uno stato disabilitato, un pannello che si
apre sopra la pagina e un banner sticky. **Non è un livello di elevazione: è il fondo dei controlli e
degli stati.**

**2 — i livelli sono quattro, non tre.** `--sfondo-alt` non compare nella prosa, e i suoi quattro usi
non sono casuali: `.alla-cieca`, `.schede-testo`, `.modulo-spesa` e la colonna di navigazione larga
sono tutti **contenitori di controlli**. Tre di loro portano per giunta lo stesso identico trio —
fondo, `1px solid var(--bordo)`, `var(--raggio)` — che è la definizione testuale del *livello 1*, ma
con un fondo diverso da quello che `.scheda` usa.

### La regola che descrive i fatti

Non è una scala di elevazione, sono **quattro ruoli**, e si riassumono in un principio fisico:

> **Un campo è un buco, un pulsante è un rilievo.**

| Fondo | Ruolo | Il gesto |
|---|---|---|
| **#000** `--sfondo` | ciò in cui si **entra**, e i contorni | ci si scrive dentro |
| **#0a0a0a** `--sfondo-alt` | ciò che **contiene** controlli | sta fra il buco e la pagina |
| **#121212** `--superficie` | ciò che si **legge** | ci si posa sopra l'occhio |
| **#1b1b1b** `--superficie-alta` | ciò che si **preme**, e gli **stati** | ci si preme sopra |

Il pregio di questa formulazione rispetto a «tre livelli» è che **si verifica guardando una
schermata**: si prende un elemento, si chiede che gesto inviti, e si controlla il fondo. «Tre livelli»
non si verifica, perché non dice cosa vada su quale — ed è la ragione per cui il foglio ha potuto
derivare per quattro token senza che nessuna regola scattasse.

---

## LE DUE ECCEZIONI CHE LA REGOLA NON SPIEGA

Vanno dichiarate, non appianate: chi deciderà la fase 2.1-bis deve sapere che esistono.

**1 — I tre campi dell'editor di nota stanno sul fondo sbagliato per la regola.**
`.titolo-nota, .titolo-grande` e `.corpo-nota` sono campi di testo, e stanno a `--superficie`
(#121212) mentre tutti gli altri campi del progetto stanno a `--sfondo` (#000) per via della regola
base. Sono tre dei quattro campi più grandi dell'applicazione — il titolo di una collezione, il titolo
di un elemento, il titolo e il corpo di una nota.
*Cosa potrebbe essere:* una scelta deliberata (l'editor è un foglio su cui si scrive, non un modulo da
compilare), oppure una deriva nata quando quei campi sono stati costruiti prima della regola base. **Il
foglio non lo dice**, e la differenza cambia il rimedio: nel primo caso va scritta la ragione, nel
secondo vanno portati a #000.

**2 — Due cose premibili sui due fondi opposti.** `.pastiglia` sta a `--sfondo` (#000, il più scuro) e
`.btn` a `--superficie-alta` (#1b1b1b, il più chiaro). Una pastiglia **a volte si preme** — l'unità 04
ha verificato che l'unica `.pastiglia` dentro una `.barra-elenco` è un `<button>` — e a volte è
un'etichetta che si legge.
*La lettura che la salva:* una pastiglia è un **contorno**, non un piano — bordo tondo su fondo, senza
elevazione — e quindi appartiene alla famiglia del #000 per forma, non per gesto. *La lettura che la
condanna:* se si preme, deve dirlo come lo dice un pulsante. **È una decisione, non un errore.**

---

## COSA COSTA ADOTTARE LA REGOLA, RIGA PER RIGA

Il mandato chiede «l'elenco di cosa andrebbe cambiato per adottarla e quanto costa». Sono quattro
interventi, e vale la pena notare che **il più caro è di prosa e il più economico è di codice** —
l'inverso di quello che ci si aspetta da un lavoro su un sistema di design.

| # | Intervento | Tocca | Costo | Rischio |
|---|---|---|---|---|
| 1 | Riscrivere il blocco «gerarchia di superficie» dell'intestazione: quattro ruoli invece di tre livelli, con la tabella del gesto | l'intestazione di `app.css` | ~15 righe di prosa, **zero** di CSS | nessuno: nessun byte reso cambia |
| 2 | Dichiarare l'eccezione di `#blazor-error-ui` accanto alla regola | un commento | 2 righe | nessuno |
| 3 | Decidere i tre campi dell'editor di nota: ragione scritta **oppure** fondo a `--sfondo` | `.titolo-nota, .titolo-grande`, `.corpo-nota` | 3 righe, **o** 2 di commento | **medio**: cambia la resa di tre schermate, va guardato |
| 4 | Decidere `.pastiglia`: contorno per forma **oppure** premibile come un pulsante | `.pastiglia`, e l'eventuale `button.pastiglia` | 2 righe, **o** 2 di commento | **medio**: le pastiglie sono in sei schermate |

**Il totale onesto: da 4 a 20 righe, e due decisioni di prodotto.** Non è un progetto di settimane —
quello che *è* un progetto di settimane è la scala **tipografica** e quella dello **spazio**, che
questa istruttoria non tocca. Le superfici sono in ordine molto più di quanto il rilievo lasciasse
credere: mancava la regola scritta, non i token.

**L'ordine consigliato è 1 → 2 → 4 → 3**, e il motivo è che l'intervento 1 non cambia nessun pixel e
rende gli altri tre decidibili in dieci minuti ciascuno: finché la regola non è scritta, ogni
discussione sui campi dell'editor riparte dal principio.

---

## COSA NON HO GUARDATO, E ANDREBBE GUARDATO INSIEME

Il censimento sopra riguarda **i fondi**. Un sistema di superfici ha altre due dimensioni che ho
lasciato fuori di proposito, per non sfondare il perimetro che il mandato mi dà:

- **I bordi.** `--bordo` (#242424) e `--bordo-forte` (#333333), più quattro usi di `--bordo` come
  **fondo** di una barra o di un filo (`.barra`, i separatori). Un token usato sia come bordo sia come
  superficie è la stessa classe di ambiguità che questo documento descrive per i fondi.
- **Le ombre.** `--ombra` e `--ombra-accento`, e il fatto che l'intestazione leghi il livello 2 a
  «`--superficie-alta` **più ombra tinta**» — ma `.btn`, che usa `--superficie-alta`, non porta
  nessuna ombra. È un terzo punto in cui la prosa e i fatti divergono, e si legge dalla stessa tabella.

Chi apre la fase 2.1-bis faccia i due censimenti insieme a questo: sono lo stesso lavoro, e farli in
tre momenti diversi produrrebbe tre regole che non si parlano.
