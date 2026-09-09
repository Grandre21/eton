# Il debito visivo di Eton, misurato

Prodotto da `ui-critic` il **10 settembre 2026**, subito dopo che il giro D del collaudo è
tornato `ESITO: verde` con 7 misure su 7 e zero difetti. Adjudicato dall'esecutore lo stesso
giorno.

**Perché esiste questo file.** Il rapporto di un subagent muore col subagent. Questi cinque
rilievi hanno un numero letto dal DOM e una misura attesa dopo il fix: sono l'unico materiale
misurato che esista sull'interfaccia di Eton, e sono l'ingresso naturale di qualunque lavoro
futuro sulla UI. In `handoff/` sarebbero archiviati a ciclo chiuso.

**Cosa NON sono.** Non sono difetti introdotti dal lavoro dei sedici rilievi. Sono tutti
**preesistenti e fuori dal perimetro** di quel lavoro: nessuno dei sedici li nominava, e il
collaudo non poteva trovarli perché stava verificando altro. Il §5 del protocollo dice che un
rilievo fondato ma fuori scope si riporta all'utente e non si risolve di nascosto: è ciò che
questo file fa.

**Il metro.** Nessuna `SPECIFICA-UI` e nessun `PIANO-DESIGN` esistevano per il lavoro dei
sedici rilievi, quindi a `ui-critic` non è stato dato un metro numerico e **non ha emesso
rilievi di `TIPO: fedeltà`**. La direzione approvata — SLY, luce additiva su nero pieno, non
neon — gli è stata passata come contesto. Il fatto che Eton non rispetti `prefers-reduced-motion`
è una scelta dichiarata e non è stato segnalato.

Schermate coperte: `/`, `/notes`, `/notes/new`, `/notes/{id}`, `/collections`,
`/collections/{id}`, `/collections/{id}/edit`, `/collections/{id}/items/{id}`, `/expenses`,
`/expenses/{id}`, `/spaces`, `/spaces/{id}`, `/profile` — a 1280px di finestra e a 357px in un
iframe same-origin.

---

## 1 — Le micro-etichette non raggiungono il contrasto minimo · alta · `TIPO: progetto`

`wwwroot/css/app.css:97` — `--testo-fioco: #6e6e6e`

Tutte le micro-etichette dell'applicazione — i nomi dei campi di ogni modulo («IMPORTO»,
«QUANDO», «VOTO»), i contatori di lista («1 NOTA»), le testate delle schede («CAMPI»,
«MEMBRI (1)»), «SPAZIO ATTIVO» nella barra — sono a **11px** in `#6e6e6e`, e non raggiungono
4.5:1 su **nessuno** dei quattro fondi su cui compaiono. Nell'editor di elemento sono 9 su 9.

| Fondo | Contrasto | Soglia |
|---|---|---|
| `#000000` | **4.12:1** | 4.5:1 |
| `#0a0a0a` (barra) | **3.88:1** | 4.5:1 |
| `#121212` (scheda) | **3.67:1** | 4.5:1 |
| `#1b1b1b` (superficie alta) | **3.38:1** | 4.5:1 |

A 11px non è testo grande, quindi la soglia è 4.5:1 e non 3:1. Elementi in difetto per rotta:
`/` 4 · `/collections/{id}` 4 · `/collections/{id}/edit` 4 · editor di elemento 9 · `/expenses` 7
· `/spaces` 4 · tutte le altre ≥ 1.

**Fix proposto:** `--testo-fioco: #8a8a8a` → 6.08 / 5.73 / 5.43 / 4.99, passa su tutti e quattro.
L'alternativa più conservativa `#808080` dà 5.32 su nero e 4.74 su scheda, ma **4.36 su `#1b1b1b`**:
va bene solo se nessuna etichetta poggia su `--superficie-alta`, condizione da verificare e non
da assumere.

**Verificato dall'esecutore:** il token è a `app.css:97`, con quel valore. La scelta fra i due
grigi è una decisione di progetto, non un fix: cambia l'aspetto di ogni schermata.

---

## 2 — «Profilo» si sovrappone al selettore di spazio · alta · `TIPO: allineamento`

`Shared/Navigazione.razor:42-45` e `wwwroot/css/app.css:2247-2258`, dentro
`@media (min-width: 64rem)`

Su **ogni schermata larga**, l'icona del collegamento «Profilo» in fondo alla barra laterale è
disegnata a cavallo del bordo destro del selettore di spazio.

| Misura | Valore |
|---|---|
| `a.voce-piede` box | 187 → 223 (36×36) |
| `scrollWidth` vs `clientWidth` | **53 contro 36** |
| `svg` interno | 170 → 192 — *comincia 17px prima del box* |
| `select` finisce a | 179 |
| **Sovrapposizione** | **9px** |
| `.etichetta` | `display: block`, non nascosta |

**La causa è una contraddizione fra un commento e il markup**, e sta scritta in chiaro. Il CSS
dichiara l'intenzione a `app.css:2244-2246`:

> *«Una destinazione secondaria accanto al selettore: somiglia a `.voce` (stesso raggio, stesso
> schema hover) ma pesa meno — **nessun'etichetta di testo**, un'icona sola in un riquadro più
> piccolo.»*

E poi fissa `width: 2.25rem; height: 2.25rem` con `justify-content: center`. Ma il markup ci
mette anche l'etichetta:

```razor
<a href="profile" class="voce-piede">
    <span class="icona"><Icona Nome="persona" /></span>
    <span class="etichetta">Profilo</span>
</a>
```

Contenuto da 53px in una scatola da 36px, centrato: esce da **entrambi** i lati.

### ⚠️ Il fix (a) proposto da `ui-critic` è sbagliato, e va scartato

`ui-critic` propone due strade, «una delle due, non entrambe»: **(a)** `.voce-piede .etichetta
{ display: none }`, per onorare il commento; **(b)** far crescere la scatola, per onorare il
markup.

**La (a) come è scritta introduce un difetto di accessibilità.** Quel link non ha `aria-label`
— verificato, `Navigazione.razor` ha un solo `aria-label` e sta sul `<nav>` alla riga 26 — quindi
il suo **nome accessibile è esattamente il testo «Profilo»**. `display: none` lo rimuove
dall'albero di accessibilità, e il collegamento diventa un'icona senza nome per un lettore di
schermo. Si scambierebbe un difetto visibile con uno invisibile, che è il peggiore dei due.

### Il fix che il progetto già possiede

`app.css:2342` definisce `.solo-lettori`, l'idioma del progetto per il testo che deve restare
nell'albero di accessibilità senza occupare spazio (`position: absolute`, 1px, `clip-path:
inset(50%)`, col commento che spiega perché non è il vecchio `clip: rect()`).

Quindi: **`class="etichetta"` → `class="solo-lettori"`** su quello `span`, e il quadrato a
`var(--tocco)` (48px) invece di 2.25rem — che chiude anche il rilievo 5 per questo elemento.
Oppure la (b), che non ha il problema del nome accessibile. **Non** la (a).

*Misura attesa:* `scrollWidth === clientWidth`; `svg.left − select.right ≥ 8`; `a.voce-piede`
alto 48; il nome accessibile del link resta «Profilo».

**Verificato dall'esecutore:** letto il markup, letta la regola CSS con il suo commento, letta
l'assenza di qualunque regola che nasconda `.etichetta` dentro `.voce-piede`, letta l'assenza di
`aria-label` sul link, letta l'esistenza di `.solo-lettori`. Il claim regge e la citazione lo
sostiene.

### Non contraddice il giro D

Il giro D ha misurato la prova «selettore spazio e Profilo non più accavallati» e l'ha passata:
**`bottom` identico, scarto 0** (erano 820 e 838). Quella era la sovrapposizione **verticale**,
ed è il rilievo che il lavoro dei sedici doveva chiudere. Questa è **orizzontale**, e nessuno la
stava cercando. Due assi diversi sullo stesso elemento: entrambe le misure sono corrette.

---

## 3 — La colonna del contenuto non sta ferma fra una schermata e l'altra · media · `TIPO: progetto`

`wwwroot/css/app.css:379-382` — `.app-layout { max-width: 720px; margin: 0 auto; }`

`.app-layout` si centra nello spazio residuo. Quando una pagina scorre, la barra di scorrimento
toglie 15px al viewport e la colonna si sposta a sinistra di **7,6px**. Navigando da un elenco
al suo editor e tornando, `h1` salta.

| Rotta | `h1.left` | scorre |
|---|---|---|
| `/notes`, `/collections`, `/profile`, `/notes/{id}`, `/expenses/{id}` | **289,2** | no |
| `/collections/{id}/edit`, editor di elemento, `/expenses` | **281,6** | sì |

`main.app-layout` ha margine laterale 9,2px contro 1,6px; `clientWidth` 1126 contro 1111.

**Fix proposto:** `html { scrollbar-gutter: stable; }` — riserva la colonna della barra anche
quando non serve. Oppure, se non si vogliono cedere 15px alle pagine corte, ancorare la colonna
nel blocco schermo largo con `margin: 0` e `padding-left` fisso. **È una decisione**, non un
fix: le due strade danno layout diversi.

*Misura attesa:* `h1.left` identico a ±0,5px fra `/notes` e `/collections/{id}/edit`.

---

## 4 — Due azioni primarie nella stessa vista, e nell'editor di elemento hanno la stessa parola · media · `TIPO: gerarchia`

`Pages/ItemEdit.razor:132` + `Shared/RecensioniElemento.razor:66` · `Pages/Home.razor:174` e `:214`

Nell'editor di elemento ci sono **due `btn primario`**, entrambi con il testo **«Salva»** — uno
per l'elemento a y=747, uno per la recensione a y=1252 — con lo stesso fondo, lo stesso
`font-weight` 600, lo stesso raggio da 12px, e spenti allo stesso modo. La pagina non dice quale
dei due sia *la* cosa da fare. In Home, «Nuova nota» e «Nuova collezione» sono due primari
affiancati per due scorciatoie di sezione.

Tutte le altre rotte hanno ≤ 1 primario: l'anomalia è circoscritta a due schermate.

**Fix proposto:** in `RecensioniElemento.razor:66` togliere `primario` (resta `btn`) e portare il
testo a «Salva recensione»; in `Home.razor:174,214` togliere `primario` alle due scorciatoie.

*Misura attesa:* `document.querySelectorAll('#app .btn.primario').length` = 1 sull'editor di
elemento e = 0 sulla Home; nessuna coppia di pulsanti con lo stesso `textContent` nella stessa
vista.

**Nota dell'esecutore:** questo è il rilievo più discutibile dei cinque, e non perché la misura
sia debole — è che «una sola azione primaria per vista» è una regola di progetto che Eton non ha
mai dichiarato. Va deciso, non applicato.

---

## 5 — Quattro controlli sotto il pavimento di tocco che il progetto si è dato · bassa · `TIPO: progetto`

`Shared/VotoInput.razor:22-24` · `Pages/Spese.razor:131` · `Pages/Home.razor:144,181` · e il
`.voce-piede` del rilievo 2

Il progetto dichiara `--tocco: 48px` a `app.css:190` ed esenta esplicitamente solo `.btn.piccolo`
dentro le righe di elenco (`app.css:724-731`). Quattro controlli stanno sotto senza rientrare in
quell'esenzione:

| Controllo | Dove | Misura |
|---|---|---|
| «Nessun voto» | editor di elemento | 95 × **22** — sotto anche il minimo WCAG 2.5.8 di 24px |
| «Mese precedente» / «Mese successivo» | `/expenses`, **testata della scheda**, non una riga | 33 × 35 |
| «Tutte» (×2) | Home | 32 × **20** |
| «Profilo» | tutte le rotte | 36 × 36 |

**Fix proposto:** `.voto-input .pastiglia { min-height: var(--tocco); padding: 0 var(--s4); }`; per
le frecce di mese in `Spese.razor:131` usare `btn` senza `piccolo`, o dare al contenitore
`min-height: var(--tocco)`; per «Tutte» in `Home.razor:144,181` `padding: var(--s3)` con
`margin: calc(var(--s3) * -1)`, così l'area cresce senza spostare il testo.

*Misura attesa:* nessun elemento interattivo con altezza < 24px, e sotto 44px solo `.btn.piccolo`
dentro le righe di elenco.

---

## Le tre cose che `ui-critic` ha dichiarato non misurabili

Le riporta perché le ha viste, non perché sappia dire se sono difetti. Nessuna ha una soglia.

1. **«Salva» spento e «Chiudi» hanno lo stesso fondo `#1b1b1b`, lo stesso bordo `#333`, lo stesso
   raggio: l'unica differenza è `opacity: .5`.** Il giro D l'ha misurato e passato. Il residuo è
   che la differenza vive su **un solo canale**, e su un monitor scadente potrebbe non bastare.
2. **Negli editor di nota, elemento e collezione il titolo compare due volte in verticale:** `h1`
   a 52px/800 e, subito sotto, il campo di testo con lo stesso valore a 20px.
3. **La prova a 360px è stata fatta in un iframe** da 357px, perché la finestra di Chrome non
   scende sotto 526px. Nessuna rotta produce scorrimento orizzontale, ma non è un viewport mobile
   vero — niente `dvh`, niente barra del browser.

## Cosa è stato misurato e passato senza rilievo

Fuoco da tastiera visibile (`:focus-visible`, outline `rgb(76,141,255)`, verificato con Tab su
tre elementi diversi) · nessuna freccia `→` nei pulsanti · nessun gradiente decorativo · icone
tutte 22×22 · un solo near-black e una gerarchia di tre grigi tokenizzata.

Nessuna scrittura sul database, nessun dato creato, iframe di prova rimosso.
