# Accertamenti dell'unità 07 — appunti di lavoro

Appunti presi durante l'esecuzione, per non perderli in una compaction. Il resoconto
definitivo è in `resoconto.md`; qui c'è la materia grezza su cui poggia.

## 1. La prova che la cascata regge (mandato, punto 1)

Il mandato dice di **verificarla invece di crederla**, e di fermarsi in `BLOCKED` se una
regola la contraddice. Nessuna la contraddice. La prova è per **enumerazione**, non per
deduzione: i due insiemi — selettori nel foglio, call-site nel markup — si incastrano
senza residui.

### 1.1 Tutti i selettori che possono agganciare una pastiglia

Nel foglio, prima della modifica, esistevano **sei** regole il cui selettore contiene
`.pastiglia`. Tutte le altre 13 occorrenze del grep `pastiglia` sono prosa dentro commenti.

| selettore | specificità | dichiara min-height / padding / font-size? |
|---|---|---|
| `.pastiglia` | 0-1-0 | no / sì (`--s1 --s3`) / sì (`--t-xs`) |
| `.pastiglia.accesa` | 0-2-0 | **no, solo tre colori** |
| `.barra-elenco .pastiglia` | 0-2-0 | sì / sì / sì — più flex, cursor, touch-action, transition |
| `.voto-input .pastiglia` | 0-2-0 | sì / sì / sì — e nient'altro |
| `.scelta-categoria .pastiglia` | 0-2-0 | sì / sì / sì — e nient'altro |
| `button.pastiglia` | 0-1-1 | no (prima) — font-family, cursor, appearance, touch-action |

I tre contenitori dichiaravano **le stesse identiche tre righe**, parola per parola:
`min-height: var(--tocco); padding: 0 var(--s4); font-size: var(--t-sm);`. È il fatto che
rende la fusione neutra: non ci sono tre valori da conciliare, ce n'è uno scritto tre volte.

### 1.2 Nessun'altra regola del foglio può agganciarle

Una regola aggancia una pastiglia solo se il suo **selettore chiave** è `*`, `button`,
`.pastiglia`, `.accesa`, o un attributo che quei `<button>` soddisfano. Cercati tutti:

- `* { box-sizing: border-box; }` — 0-0-0, non dichiara nessuna delle tre;
- `a, button, select, input, textarea, [role=button]` — 0-0-1, dichiara solo
  `-webkit-tap-highlight-color`;
- `#aggiornamento-pwa button` e `.schede-testo button` — nessuna pastiglia ci vive dentro
  (`.schede-testo` ha un solo call-site, `Pages/NoteEdit.razor`, senza pastiglie);
- **nessuna** pseudo-classe (`:hover`, `:active`, `:disabled`, `:focus-visible`) su
  `button` o su `.pastiglia`;
- **nessuna** regola su `.pastiglia` dentro le tre `@media` del foglio (le sei regole stanno
  tutte prima della prima `@media`, a `:2224`);
- `:root` è **unico** e nessuna `@media` ridefinisce `--tocco`, `--raggio`, `--t-sm`, `--s4`.

### 1.3 I dieci call-site, e perché nessuno cambia

| call-site | elemento | contenitore | riceveva le tre? | le riceve ora? |
|---|---|---|---|---|
| `CollectionDetail.razor:91` | `<button>` | `.barra-elenco` | sì | sì, da `button.pastiglia` |
| `CollectionEdit.razor:103` | `<button>` | `.scelta-categoria` | sì | sì, idem |
| `SpesaEdit.razor:126` | `<button>` | `.scelta-categoria` | sì | sì, idem |
| `SpesaEdit.razor:136` | `<button>` | `.scelta-categoria` | sì | sì, idem |
| `Spese.razor:97` | `<button>` | `.scelta-categoria` | sì | sì, idem |
| `VotoInput.razor:22` | `<button>` | `.voto-input` | sì | sì, idem |
| `SpaceDetail.razor:107` | `<span>` | riga di elenco | **no** | **no** |
| `Spaces.razor:38` | `<span>` | riga di elenco | **no** | **no** |
| `Spaces.razor:42` | `<span>` | riga di elenco | **no** | **no** |
| `Spese.razor:191` | `<span>` | riga di elenco (`.meta`) | **no** | **no** |

Sei `<button>`, quattro `<span>`. **Tutti e sei i button** stanno dentro uno dei tre
contenitori; **nessuno dei quattro span** ci sta. Quindi nessun elemento perde le tre
proprietà e nessuno le guadagna. Nessuna generazione dinamica della classe: cercato
`pastiglia` in `.cs`, `.razor`, `.js`, `.html` fuori dagli attributi `class="…"` — solo
prosa di commenti.

`.barra-elenco` ha **un solo** call-site (`CollectionDetail.razor:80`) e contiene **un solo**
elemento con classe `pastiglia`, che è un `<button>`.

### 1.4 L'unico stile inline, e perché non è un'eccezione

`Pages/CollectionEdit.razor:103` porta `style="font-size: var(--t-lg)"`. Vinceva ieri su
`.scelta-categoria .pastiglia` (0-2-0) e vince oggi su `button.pastiglia` (0-1-1): lo stile
inline batte qualunque selettore senza `!important`. Invariato.

### 1.5 Perché `.pastiglia.accesa` non scavalca

`.pastiglia.accesa` è 0-2-0 e **batte** `button.pastiglia` (0-1-1). Ma dichiara solo
`border-color`, `background`, `color`: nessuna delle tre proprietà spostate. Nessun conflitto.

---

## 2. Il censimento dei rimandi (mandato, punto 4)

### 2.1 Quanti, e in quante forme

**Ventidue** rimandi in tutto: **dodici** interni al foglio, **dieci** verso altri file.

I dodici interni stanno in **tre** forme, non due. Il mandato ne prevedeva due e chiedeva di
dirlo se ne fosse comparsa una terza: è comparsa.

| forma | quante | esempio |
|---|---|---|
| A — `(v. riga N)` | 10 | `.app-layout (v. riga 409)` |
| B — `(v. il commento a riga N)` | 1 | `(v. il commento a riga 1039, sulla Home)` |
| **C — `di riga N`** | **1** | `la regola universale di riga 227` |

La forma C è quella che nessuno aveva cercato: non contiene né `v. riga` né `commento a
riga`. Il grep che le prende tutte e tre è `rig(a|he) [0-9]`, case-insensitive.

### 2.2 I quattro scaduti su dodici, verificati riga per riga

| il rimando sta a | dice | la riga N contiene davvero | esito | bersaglio reale |
|---|---|---|---|---|
| `:247` | riga 409 | `.app-layout {` | valido | — |
| `:1369` | riga 1461 | `.avatar.segnaposto {` | valido | — |
| `:1372` | riga 227 | `* { box-sizing: border-box; }` | valido | — |
| `:1799` | riga 2139 | `.scelta-categoria .pastiglia {` | valido | — |
| `:1805` | riga 1650 | `.barra-elenco .pastiglia {` | valido | — |
| **`:1886`** | riga 1039 | `display: inline-flex;` (dentro `.pastiglia`) | **SCADUTO** | il commento sulla Home, `:1403-1407` |
| `:1903` | riga 754 | il commento dell'esenzione di `.btn.piccolo` | valido | — |
| `:1909` | riga 762 | `.btn.compatto {` | valido | — |
| **`:1936`** | riga 306 | prosa dentro un commento | **SCADUTO** | `.testo-tenue`, `:340` |
| **`:2203`** | riga 330 | `}` | **SCADUTO** | `.etichetta-piccola, .etichetta-campo`, `:372` |
| **`:2209`** | riga 730 | commento su `.btn.primario` | **SCADUTO** | `.intestazione .selettore`, `:924` |
| `:2412` | riga 2509 | `.solo-lettori {` | valido | — |

**Quattro su dodici**, esattamente come il mandato prevedeva — e il quarto è proprio quello
in forma B, che l'unità 01 aveva mancato cercando solo la forma A.

### 2.3 I dieci cross-file: tre erano scaduti, e nessuno lo sapeva

Il mandato li chiamava «dieci opzionali»; `DECISIONI` li rende obbligatori («cross-file
compresi»). La misura conferma il rovesciamento di `tech-advisor`:

| sta a | dice | reale | esito |
|---|---|---|---|
| `:769` | `NoteEdit.razor:114` | 114 | valido |
| `:789` | `Spese.razor:103` | 103 | valido |
| **`:1348`** | `CollectionDetail.razor:129` | **128** | **SCADUTO** |
| **`:1353`** | `CollectionDetail.razor:40` | **39** (`.icona-collezione`) | **SCADUTO** |
| `:1424` | `Home.razor:144` e `:181` | 144, 181 | validi |
| `:1796` | `VotoInput.razor:22` | 22 | valido |
| `:1816` | `RecensioniElemento.razor:54` | 54 | valido |
| `:1899` | `Spese.razor:131` | 131 | valido |
| `:1912` | `Spese.razor:131` | 131 | valido |
| **`:2136`** | `CollectionEdit.razor:104` | **103** | **SCADUTO** |

**Tre su dieci, cioè il 30%**, contro il 33% degli interni. I numeri verso altri file non
scadono meno di quelli interni: scadono **uguale**. La premessa che li voleva esentati era
falsa, ed è misurata qui.

**Totale: sette rimandi scaduti su ventidue.** È il numero scritto nella nuova voce
dell'intestazione di `app.css`.

### 2.4 Un rimando scaduto FUORI dal mio perimetro

`Pages/CollectionEdit.razor:75` cita `SpesaEdit.razor:105-106` e `Spese.razor:93-94`. Il
primo è **scaduto**: la pastiglia di `SpesaEdit` sta a `:126`. Non l'ho toccato — quel file
non è mio. Va segnalato: la convenzione nuova vive in `app.css`, ma il difetto esiste anche
nei `.razor`.

---

## 3. Il precedente di `.btn.compatto`, e perché il primo grep l'aveva mancato

`grep 'btn compatto'` non trova **niente**: la classe è sempre scritta
`class="btn primario compatto"`, quindi le due parole non sono mai contigue. I due call-site
vivi esistono e sono:

- `Pages/Collections.razor:14` — `<a class="btn primario compatto" href="collections/new">`
- `Pages/Notes.razor:16` — `<a class="btn primario compatto" href="notes/new">`

Entrambi dentro `<Azione>` di `TestataPagina`, cioè in **testata di schermata**. Le frecce di
mese stanno invece nella testata di una **scheda** (`.testa-registro` dentro
`.scheda riepilogo-mese`): il ruolo è lo stesso — si preme col pollice, non è una riga di
elenco — ma i due posti non sono lo stesso posto, e il commento deve dirlo senza equipararli.

`.btn.primario` non dichiara `border-radius`, quindi i due precedenti sono già a `--raggio`
(12px): le frecce non introducono un raggio nuovo, lo raggiungono.

---

## 4. Il delta visivo reale, voce per voce

| voce | cosa cambia a schermo |
|---|---|
| fusione delle pastiglie | **niente** (§1) |
| frecce `.piccolo` → `.compatto` | **solo il raggio**, da `--raggio-s` 8px a `--raggio` 12px. Altezza 48 → 48, larghezza 48 → 48, padding `0 var(--s3)` → identico, corpo `--t-sm` → identico. `white-space: nowrap` in più, ineffettivo su un glifo solo |
| rimandi e commenti | niente |
| `<label>` → `<div class="campo campo-lettura">` | niente, **grazie** alla regola nuova: senza, l'etichetta scivolerebbe accanto alla cifra |
