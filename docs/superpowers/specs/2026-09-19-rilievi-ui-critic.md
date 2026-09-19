# Il debito visivo di Eton, misurato la seconda volta

Prodotto da `ui-critic` il **19 settembre 2026**, sul commit `bb77cf7`, dopo che tutte e sette le
unità del goal «chiudere i punti rimanenti» erano integrate e **dopo** due giri di collaudo
funzionale tornati verdi. Sostituisce come fotografia corrente
`2026-09-10-rilievi-ui-critic.md`, i cui cinque rilievi sono stati **tutti chiusi** da questo goal.

**`METRO: PIANO-DESIGN`**, e la distinzione decide come vanno letti: i valori del metro sono
**scelti** dal progetto, non **misurati** su un sito di riferimento. Uno scostamento è quindi una
**decisione da discutere**, non un errore da correggere.

## Il pavimento, passato su tutte e dieci le rotte

Prima dei rilievi va detto cosa **non** è emerso, perché una critica che elenca solo difetti dà
un'immagine storta:

- **contrasto** — zero elementi di testo sotto soglia; gli unici sotto 4,5:1 sono i pulsanti
  disabilitati a `opacity: .5`, che non contano;
- **360px** — `scrollWidth == clientWidth` **ovunque**, nessun elemento fuori dal viewport;
- **fuoco da tastiera** — dopo otto `Tab` il fuoco è visibile con `outline rgb(76,141,255)`, e i
  campi hanno il proprio anello al fuoco.

⚠️ *Nota di metodo dell'agente: Chrome ha rifiutato di scendere sotto 1280px, quindi la prova a
360px è stata fatta in un `<iframe>` same-origin di 360×744 nella stessa scheda. Le media query
rispondono alla larghezza dell'iframe, e i valori letti lo confermano — ma non è un viewport mobile
vero. È lo stesso limite dichiarato dalla critica del 10 settembre.*

---

## 1 — Le tre superfici esistono come token ma non come scala · alta · `TIPO: progetto`

**È il rilievo che conta più degli altri quattro**, perché è l'unico che nomina una regola assente
invece di un valore sbagliato.

Il progetto dichiara tre superfici sopra il nero: `#0a0a0a` per le barre, `#121212` per le schede,
`#1b1b1b` per la superficie alta. Ma vengono assegnate **per componente**, non **per profondità**:

| Dove | Cosa si legge | Il problema |
|---|---|---|
| `/expenses` | `section.modulo-spesa` `bg=#0a0a0a r=12px shadow=none`, e subito sotto `section.scheda.riepilogo-mese` `bg=#121212 r=16px shadow=yes` | due schede impilate, **tre** proprietà divergenti |
| `/collections/{id}/edit` | `li.riga-campo bg=#121212` dentro `div.scheda bg=#121212` | **zero gradini** di differenza: la si vede solo per il bordo |
| `/collections/{id}/items/{id}` | `div.alla-cieca bg=#0a0a0a` su fondo pagina | il grigio **delle barre** a livello pagina |

**La regola che manca:** in una scala di tre superfici, ogni livello di annidamento sale di **un
gradino esatto** — scheda su pagina = `--superficie`, scheda dentro scheda = `--superficie-alta`,
`--sfondo-alt` riservato alle barre.

*Misura attesa dopo il fix:* ogni contenitore a livello pagina legge `bg=#121212 parentBg=#000000`,
ogni contenitore annidato `bg=#1b1b1b parentBg=#121212`, e `#0a0a0a` compare **solo** su
`nav.nav-app`.

---

## 2 — Nove controlli senza nome accessibile · alta · `TIPO: coerenza`

⚠️ **Questo non è una decisione di progetto: è un difetto oggettivo**, ed è l'unico dei cinque che
si può chiudere senza discuterne.

| Dove | Quanti | Cosa |
|---|---|---|
| `/collections/{id}/items/{id}` | 1 | il **cursore del voto** (`input[type=range]`): nome vuoto. L'etichetta «Voto» esiste a schermo ma è uno `<span>` **fratello**, non legato al controllo |
| `/collections/{id}/edit` | 5 | i `<select>` del tipo di campo: nome vuoto, nemmeno un placeholder |
| `/collections/{id}/edit` | 3 | i campi delle opzioni: nome vuoto, `placeholder=""` |

**È la stessa famiglia del rilievo 2 del 10 settembre** — l'etichetta di «Profilo» — che questo
goal ha appena chiuso: il nome visivo e il nome programmatico non coincidono.

*Il fix è noto e piccolo:* `aria-labelledby` sul cursore verso lo `<span>` che già esiste,
`aria-label` sui `<select>` e sui campi delle opzioni.
*Misura attesa:* lo snippet dei controlli senza nome restituisce `[]` su entrambe le rotte, e il
nome calcolato del cursore è «Voto».

---

## 3 — Due voci di navigazione a 44px invece di 48 · media · `TIPO: fedeltà`

Il metro prescrive `--tocco: 48px` con **una sola** esenzione dichiarata. Queste due non ci
rientrano e sono scritte **a mano**:

- a 1280px, su tutte e dieci le rotte: `a.voce 223×44` ×5 — la regola base dice
  `min-height: var(--tocco)`, quella desktop la sovrascrive con `44px` **letterale**;
- a 360px: `a.voce-profilo-stretto 109×44`, con `min-width: 44px; min-height: 44px`.

Il token esiste ed è a tre righe di distanza, ma lì non viene usato.

**È una decisione, non un errore:** se 44px sulla barra laterale desktop è voluto — puntatore, non
dito — va scritto nel piano come **seconda esenzione col suo perché**; altrimenti si usa il token.
Quel che non va è che sia scritto a mano senza dire quale delle due cose sia.

---

## 4 — Controlli sulla stessa riga con altezze diverse · media · `TIPO: allineamento`

| Dove | Misura | Δ |
|---|---|---|
| selettore di icone, `/collections/{id}/edit` | fila 1 **56,6px**, file 2 e 3 **48px** | **8,6px** |
| `/spaces` | `input h=50,4` accanto a `button h=48` | **2,4px** |
| scheda elemento | `select` 48px fra `input` da 50,4px | **2,4px** |

**Le cause sono due, entrambe misurate:** il campo dell'icona ha `padding: 12px 8px` e corpo 20px,
e il `flex-wrap` allinea in `stretch`, quindi **le dieci pastiglie della sua fila si stirano con
lui**; e i campi di testo ereditano `line-height: 1.55`, che con padding e bordo fa 50,4 > 48.

*Osservato:* le pastiglie della prima fila sono quasi circolari, quelle delle due file sotto sono
ovali più schiacciati — la stessa componente in due forme a tre centimetri di distanza. Il raggio a
pillola presuppone un'altezza costante.

---

## 5 — Il titolo di pagina non segue una regola sola · media · `TIPO: gerarchia`

- su `/expenses` a 1280px: **due** elementi in cima alla scala — `h1 "Spese" 52px/800` e
  `p.totale-mese "2.569,00 €" 52px/600`;
- su `/profile`: l'etichetta di rotta è **2,6×** il contenuto (52px contro 20px);
- a 360px: `h1` = **26px su nove rotte** e **36px su `/collections/{id}`**, perché lì il titolo sta
  in `.intestazione` e la riduzione è scritta solo su `.testata h1`.

⚠️ **E un commento del foglio è diventato falso senza che nessuno lo toccasse:** dichiara che
`--t-3xl` è usato «solo» dal voto e dal totale, ma da ≥40rem lo usa **anche ogni `h1`**. La
coincidenza non è stata decisa: è accaduta.

*Osservato:* «Spese» e «2.569,00 €» hanno lo stesso corpo; il verde vince per colore, non per
gerarchia, e la parola pesa quanto il numero che la pagina esiste per mostrare.

---

## Oltre il tetto dei cinque — misurati, non adjudicati

L'agente li ha misurati e dichiarati fuori dal proprio tetto. Non sono istruiti:

- `.dato { font-size: .95em }` produce **12,35px e 15,2px** — fuori scala — su quattro rotte,
  mentre la stessa classe con corpo esplicito legge 13px: **tre corpi per una classe sola**;
- nella scheda elemento il primo campo è a **0px** dal secondo e tutti gli altri a 12px, perché il
  margine sta su una classe che il primo non ha;
- «Nuova collezione» è `.btn.primario` 16px/181px in Home e `.btn.primario.compatto` 13px/133px su
  `/collections`: stesso testo, due forme.

## Dove finiscono questi cinque

**Nel rapporto del goal, come candidati per l'obiettivo successivo — non in questo goal.**

Il motivo è una regola che il capo si è dato per iscritto il 19 settembre, dopo la quarta crescita
dell'obiettivo: *dalla quinta in poi le voci nuove vanno nel rapporto finale*. Violarla alla prima
occasione la renderebbe finta.

E hanno una destinazione naturale: sono l'ingresso della **fase 2.1-bis**, il mandato UI/UX
collocato lo stesso giorno fra le spese ricorrenti e la vista tabellare. Il rilievo 1 in
particolare — la scala di superfici che esiste come token e non come regola — è esattamente il
genere di decisione che quella fase deve prendere, e che un'unità di debito non può prendere al
posto suo.

⚠️ **L'eccezione possibile è il rilievo 2**, che non è una decisione di progetto ma un difetto di
accessibilità con un fix noto e piccolo. Se l'utente vuole chiuderlo subito, è l'unico dei cinque
che si può chiudere senza discuterne.
