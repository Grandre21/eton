# `ui-critic` — 20 settembre 2026, dopo sei unità

```
RILIEVI: 5
```

*Il rapporto l'ha prodotto `ui-critic`; **il file l'ha scritto il capo**, perché quell'agente è di
sola lettura. Il testo è ricopiato; l'`ADJUDICA` in fondo è mia.*

Commit `d9cf096`. **Quindici rotte**, misurate a 1128px nella finestra reale e a **360, 600 e 800px
dentro un `<iframe>` same-origin con Blazor avviato dentro**.

⚠️ **Quell'iframe è una soluzione che nessuno aveva chiesto e che va conosciuta.** In questo
ambiente `resize_window` **non ha avuto effetto a nessun valore** — `innerWidth` è rimasto 1128 e
`outerWidth` ha risposto 0 — quindi non è stato raggiunto nemmeno il pavimento di 594px che il
collaudo di poche ore prima aveva toccato. Le media query e il layout rispondono alla larghezza
dell'iframe, quindi la misura è valida. **È la via da usare al prossimo collaudo**, invece di
dichiarare un limite.

---

## I CINQUE RILIEVI

### 1 · `app.css:2414-2416`, causa a `:937` · media · allineamento

**Il campo della data su `/expenses` è alto 71px con testo da 16px, da 640px in su.** Non è 48 (il
token) e non è la propria altezza di contenuto: **è quella del vicino «Importo»**.

⚠️ **E completa il difetto che il collaudo aveva trovato senza spiegarlo.** L'unità 04 aveva
dichiarato «data-spesa → 48»: **è vero, ma solo sotto i 640px**, dove i campi vanno in colonna.
Sopra, il contenitore è una griglia con stretch implicito e `.campo input { flex: 1 }` — scritto per
la variante *in riga* di `.campo` — dentro una `label.campo` in colonna fa crescere l'altezza.

    grid normal
    ["importo-spesa h71 fs36px", "descrizione-spesa h48 fs16px", "data-spesa h71 fs16px"]
    input.data-spesa → flex: 1 1 0%, height 71px, min-height 48px

**Tre campi, due altezze, e quella sbagliata è sul campo dal corpo normale.**

**Il fix è una decisione**, perché le due regole che si contendono la riga — *altezza dal token* e
*stessa altezza sulla stessa riga* — non si soddisfano insieme finché un campo da 36px sta accanto
a uno da 16:

- **(a)** l'importo da solo sulla prima riga a tutta larghezza, descrizione e data affiancate sulla
  seconda: le celle affiancate hanno lo stesso corpo e nessuna si stira;
- **(b)** tenere la coppia e fermare lo stiramento — ma allora la riga porta due altezze (71 e 48,
  Δ 23) e i fondi non coincidono, **cioè la forma del rilievo 4 del 19 settembre**.

**Misura attesa con (a):** `input.data-spesa` h=48 a 360, 600, 800 e 1128; ogni riga contiene input
di una sola altezza; l'importo resta 71.

### 2 · `app.css:1664` (markup `NoteEdit.razor:77`) · media · fedeltà · `METRO: PIANO-DESIGN`

**Il corpo della nota — la superficie di testo più grande dell'applicazione — è reso in una terza
famiglia tipografica che il metro non prevede:** il mono di sistema, né Inter né Plex Mono.

    getComputedStyle(textarea.corpo-nota).fontFamily
    → 'ui-monospace, "Cascadia Code", Consolas, monospace'
    famiglie distinte nel DOM visibile su /notes/{id} → 4, non 2

Il metro dice: *«Inter … la PROSA; IBM Plex Mono … i DATI»*, e il principio 2: *«Il mono su una
frase intera è sempre un errore»*.

**Il fix è una decisione, e va detto quale delle due voci è una nota mentre si scrive.** Se è prosa
— ed è ciò che l'anteprima poi rende — va al font della prosa; se si vuole un editor mono per il
Markdown, va al mono **con due righe che dichiarino l'eccezione al principio 2**.

⚠️ **È lo stesso selettore che porta anche l'altra eccezione non scritta**, quella sul fondo
(#121212 invece di #000) che `istruttoria-superfici.md` già segnala: **due decisioni non scritte su
un elemento solo**.

### 3 · `app.css:1639` (markup `NoteEdit.razor:61-64`) · media · fedeltà · `METRO: PIANO-DESIGN`

**I due segmenti «Scrivi / Anteprima» sono bersagli alti 40px, sotto il pavimento di 48 — e il
commento che li precede dichiara l'opposto**, per esteso:

> *«il bersaglio tattile non dipende da cosa il pulsante significa — dipende dal dito»*

    button 391×40 ×2 a 1128 · 150×40 ×2 a 360 · min-height: calc(48px − 8px)
    elementFromPoint(centro, top+3).className → "schede-testo"   (il contenitore, non il pulsante)

**Il padding di 4px del contenitore non fa parte del bersaglio**: un tocco lì cade sul `<div>`.

L'unica esenzione scritta del foglio è per i bottoni piccoli; **questa non lo è**. Il fix: portarlo
al token, **oppure** scrivere l'esenzione nella forma di quella esistente e riscrivere il commento.

### 4 · `app.css:1094-1107` · bassa · coerenza

**Il «?» dell'aiuto è l'unico glifo dell'applicazione reso in Arial**, su **14 rotte su 15**:
`.aiuto-apri` è un `<button>` senza `font: inherit`, e lo pseudo-elemento che disegna il segno
eredita il font dello user agent.

    getComputedStyle('.aiuto-apri','::before').fontFamily → "Arial"
    getComputedStyle('.aiuto-apri').fontSize             → "13.3333px"  (UA)
    elementi in Arial dentro #app                        → ["aiuto-apri"]

**Il fix è una riga**: `font: inherit`, come già fa la classe dei bottoni.

⚠️ **È il pulsante che questo goal ha passato la notte a sistemare** — esistenza, allineamento,
ancora — e nessuno aveva guardato di che font fosse il segno.

### 5 · `Shared/TestataPagina.razor:34-37` · bassa · progetto

**Il «?» è ancorato alla larghezza del titolo, e in sei schermate su undici il titolo al primo
render è una parola di ripiego: quando il dato arriva, il pulsante si sposta.**

    «Spazio» 452,6 → «Personale» 523,2                    Δ 70,6   (Home)
    «Elemento» 514,3 → «Chianti Classico 2021» 784,8      Δ 270,5  (ItemEdit)
    «Nota» 406,4 → «COLLAUDO 4 SET» 708,8                 Δ 302,4  (NoteEdit)

⚠️ **È un effetto del lavoro dell'unità 01b**, che ha portato la testata fuori dal condizionale di
caricamento: prima il pulsante non esisteva affatto in quella finestra, quindi non poteva spostarsi.
**Il rimedio ha reso visibile un difetto che prima era nascosto da un difetto peggiore.**

E: **lo slot sotto la testata parte 8px più in basso durante il caricamento** che in ogni altro
stato — `p.avvio` porta il margine dello user agent, unico dei quattro stati.

**Il fix è una decisione di progetto.** Il metro disegna il «?» **al bordo destro** della riga del
titolo; la resa lo mette subito dopo l'ultima lettera. Seguendo il disegno, lo scarto va a **zero su
tutte e sei le schermate**. Altrimenti va scritto che si sposta. In entrambi i casi il margine di
`.avvio` va azzerato.

**Sul resto degli stati nuovi, il giudizio è positivo e sui numeri**: titolo di ripiego a sinistra,
blocco di stato centrato come il metro prescrive per gli stati vuoti, pulsante primario 150×48,
padding coerente in tutti e tre. **Nessun valore fuori scala tranne quel margine.**

---

## LA VERIFICA DEI CINQUE RILIEVI DEL 19 SETTEMBRE

**Tutti e cinque confermati chiusi**, con le misure:

| # | Esito |
|---|---|
| **1** superfici | **d'accordo con l'istruttoria dell'unità 05**, coi numeri: ogni fondo segue il ruolo che il piano descrive. Non ripetuto |
| **2** nomi accessibili | **chiuso**. Controlli senza nome: `[]` su **tutte le 15 rotte** (75 controlli sull'editor di collezione). Controlli con **due** nomi: `[]` ovunque |
| **3** 44→48 | **chiuso**. `a.voce` 223×48 ×5; il profilo stretto 48×109,5 a 360 |
| **4** altezze | **chiuso dove misurato**: selettore di icone 48, 24 pastiglie tutte 48, sette campi tutti 48, gap [12×6]. ⚠️ **Ma la stessa classe di difetto si ripresenta nel modulo spese sopra i 640px — il rilievo 1 — dove il collaudo non era arrivato** |
| **5** titolo | **chiuso come regola**: `h1` 52/36/26 a 1128/600/360; centro del «?» sulla prima riga con Δ **0** e **0,01**, titolo lungo compreso |

**Due residui sulla voce 5**, che l'unità 05 non ha toccato: il dettaglio di una collezione tiene il
titolo fuori dalla testata condivisa — **è l'unica schermata senza «?»** — e su `/expenses` il
titolo e il totale restano entrambi in cima alla scala sopra i 640px, con un commento del foglio che
è ancora falso.

---

## OLTRE IL TETTO — misurati, non adjudicati

- **Il «?» e l'azione di testata si stringono a 360px con una barra di scorrimento classica**: il
  pulsante scende a 44,7px e «Collezioni» si spezza in due righe. Senza barra — cioè su un telefono
  vero — sta su una riga con **3,2px** di margine, non gli 8,5 che il commento dichiara.
- I corpi fuori scala già noti (decisione aperta 23b), non ripetuti.
- Un'emoji-icona a 22,4px, unico corpo fuori scala che non sia un dato.
- Una casella di spunta col margine dello user agent, unico controllo non azzerato.
- Nell'editor di elemento **il titolo e il campo del nome mostrano lo stesso testo** a 52px e a
  20px, a 8px di distanza. *«La regola che lo vieterebbe non ha una soglia nominabile.»*

## IL PAVIMENTO

| | |
|---|---|
| **Contrasto** | **zero** elementi sotto soglia su 15 rotte a 1128 e 11 a 360 — composizione dei fondi risalendo gli antenati. Il segnaposto grigio su nero: **6,08:1** |
| **360px** | `scrollWidth ≤ clientWidth` su **tutte** le 11 rotte, nessun elemento oltre il viewport |
| **Bersagli sotto 44px** | solo il bottone piccolo (esenzione scritta), i due segmenti del rilievo 3, e una casella dentro la propria etichetta |

## NON MISURABILE, DICHIARATO

- **Il fuoco da tastiera**: i `Tab` inviati dal plugin non hanno spostato il fuoco dall'`h1` che
  Blazor mette a fuoco alla navigazione. La regola esiste nel foglio e l'istanza del 19 settembre
  l'aveva vista; questa non ha potuto riprovarla.
- **La resa visiva**: la cattura a scala piena va in timeout, a scala ridotta restituisce un ritaglio
  dell'angolo. **Un solo `OSSERVATO` è visivo** — il crop a 800px del modulo spese — e per il resto
  il giudizio è sui numeri.
- **Gli stati «nessuno spazio» ed «errore»**: non producibili coi dati dell'utente, misurati
  iniettando il markup reale sotto la testata reale.

---

## ADJUDICA DEL CAPO

**Tutti e cinque vanno in `FUORI SCOPE`, destinazione fase 2.1-bis.** È la regola che questo goal si
è dato e che ha rispettato per tutta la notte: le voci nuove trovate al collaudo vanno nel rapporto,
non nel goal corrente. Il ciclo precedente ne lasciò dieci allo stesso modo.

⚠️ **Due però sono minuscoli e oggettivi, e li porto all'utente invece di seppellirli nel mucchio:**

- **Il rilievo 4** è **una riga** (`font: inherit`), non è una decisione di design, e riguarda
  proprio il pulsante che questo goal ha sistemato sotto ogni altro aspetto.
- **Il rilievo 1** ha ora **la causa completa e due fix scritti**, e chiude il difetto che il
  collaudo aveva trovato senza spiegare. È anche l'unico dei cinque che **smentisce una misura
  attesa scritta in due resoconti**.

Gli altri tre sono decisioni di sistema di design: appartengono alla 2.1-bis per costruzione.
