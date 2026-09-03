UNITÀ: 11/13 — Il foglio di stile paga i debiti di nove unità

## OBIETTIVO

Sei l'unica unità che possiede `wwwroot/css/app.css`, ed è per questo che nove unità prima di
te hanno rinunciato a toccarlo e hanno lasciato qui le loro voci. Più cinque rilievi che sono
CSS per natura.

**Sette cose, in ordine di importanza.** Le prime due chiudono difetti che oggi sono
**visibili e fuorvianti**; l'ultima è cosmesi e puoi lasciarla.

## PERIMETRO — file di tua proprietà esclusiva

- `wwwroot/css/app.css`
- `wwwroot/index.html`
- **Solo se il CSS non basta**, e dichiarandolo: `Shared/Navigazione.razor`,
  `Shared/SelettoreSpazio.razor` (voce 6), `Pages/NoteEdit.razor` (voce 5).

I tre file condizionali **non si aprono per abitudine**: si aprono quando hai **provato** che
la regola CSS da sola non risolve, e nel resoconto dici cosa hai provato.

## 1 — `a.btn:not([href])`, il debito di quattro unità

`app.css:704-709` ha `.btn:disabled`. Le unità 04, 05, 06 e 07 hanno reso «Chiudi»
funzionalmente inerte durante una scrittura togliendogli l'attributo `href` — un `<a>` senza
`href` non naviga e non prende focus — ma **resta acceso all'occhio**: sembra premibile e non
lo è.

Accoda `a.btn:not([href])` al selettore di `.btn:disabled`. Una riga, quattro pagine.

## 2 — La stessa regola chiude la metà visiva del rilievo 9

«Salva» spento è reso con `opacity: .5` su fondo blu pieno, e **su nero resta saturo**: non
sembra spento. L'unità 05 ha fatto la metà che si poteva fare senza CSS — dire *cosa manca* —
quindi oggi il pulsante spiega il perché ma non lo dimostra.

**Verifica che sia ancora vero** prima di intervenire, e se il rimedio della voce 1 non basta,
il pulsante spento deve leggersi come spento **su fondo nero**: è il fondo del progetto, non
un caso limite.

## 3 — Le pastiglie alte 21px, e il rilievo 5

`.scelta-categoria .pastiglia` è alta **~21px** contro i **48px** che il progetto stesso
dichiara in `--tocco` (`app.css:190`) e **già applica** a `.barra-elenco .pastiglia`. È il
rilievo 5, ed è la misura sotto la quale un bersaglio non si prende col pollice.

`min-height: var(--tocco)` su `.scelta-categoria .pastiglia`. **Vale per tre file** —
`Spese.razor`, `SpesaEdit.razor`, `CollectionEdit.razor` — e una riga li chiude tutti e tre.

**L'unità 05 ha messo un `font-size: var(--t-lg)` inline** su `CollectionEdit.razor` come
rimedio parziale **dichiarato**. Quando la regola vera arriva, **valuta se togliere quell'inline**:
se resta, due delle tre pastiglie avranno una misura del testo diversa dalla terza. Se decidi
di toglierlo tocchi `Pages/CollectionEdit.razor`, che **non è nel tuo perimetro**: in quel caso
torna `BLOCKED` con la proposta, non allargarti.

## 4 — Il rilievo 8: «Elimina» a 55px da «Chiudi»

La fila negli editor è `Salva` · `Chiudi` · `Elimina`, con l'azione distruttiva **adiacente** a
quella innocua. Il rosso aiuta, la distanza no — soprattutto col pollice. La conferma a valle
esiste (`ConfermaAzione`), quindi il danno è contenuto: resta un tocco sbagliato di troppo.

**Il capo ha assegnato questo rilievo a te e non agli editor**, perché `.azioni`
(`app.css:729`) è **una sola regola** che governa **sei blocchi** su quattro pagine: separare
lì separa ovunque, in un punto solo.

Il rimedio classico è spingere l'azione distruttiva dall'altra parte (`margin-left: auto` sul
pulsante di eliminazione). **Ma serve un modo di selezionarlo**, e un `grep` del capo non ha
trovato nessuna classe tipo `btn-pericolo`: guarda com'è reso «Elimina» in
`Pages/SpesaEdit.razor` e negli altri tre. Se non c'è un gancio nel markup, **torna `BLOCKED`
con la proposta di classe**: aggiungerla tocca i quattro editor, che non sono tuoi.

**Non risolvere con un selettore posizionale** tipo `:last-child`: `CollectionEdit` ha tre
blocchi `.azioni` diversi (`:61`, `:180`, `:253`) e non tutti finiscono con «Elimina».

## 5 — Il rilievo 6: «Anteprima» fa saltare il layout di 358 pixel

Su `NoteEdit`, passare all'anteprima Markdown sposta il contenuto di 358px. È un'**altezza non
riservata**: l'area cambia contenuto e nessuno le ha dato una misura minima.

Il rimedio è CSS — una `min-height` sull'area che ospita editor e anteprima, così i due stati
occupano lo stesso spazio. **`Pages/NoteEdit.razor` è nel tuo perimetro solo se il CSS non
basta**, e in quel caso dillo.

**Misura prima di scegliere il valore.** Un numero inventato o è troppo piccolo e non risolve,
o è troppo grande e lascia un buco su una nota di due righe.

## 6 — Il rilievo 14: selettore spazio e «Profilo»

Barra laterale in basso, a 1414px. Misurato dalla ricognizione: **non si sovrappongono** — il
select arriva a x=179, il link parte da x=187, 8px di stacco — ma **le basi sono disallineate**
(820 contro 838) e la prossimità li fa leggere come accavallati. È rifinitura.

Allinea le basi. `Shared/Navigazione.razor` e `Shared/SelettoreSpazio.razor` sono nel perimetro
**solo se il CSS non basta**.

**Questo è l'unico rilievo del tuo elenco che il codice non può confermare**: gli altri sei si
verificano leggendo, questo si vede solo a schermo, a quella larghezza. Se dopo aver guardato il
CSS non riesci a stabilire da dove venga il disallineamento, **dillo e lascialo**: `live-testing`
lo misurerà e tornerà con un numero. Meglio un rilievo aperto con una misura che una regola
inventata che sposta qualcos'altro.

## 7 — Il rilievo 4: il banner di aggiornamento

`wwwroot/index.html`. L'avviso di versione nuova **non si può rimandare e copre l'azione
principale**.

**La decisione è già presa e non si riapre** — 3 settembre, posizione di `tech-advisor` adottata
senza domanda perché smonta la premessa del rilievo invece di scegliere fra alternative:

> «Più tardi» = `banner.hidden = true` **e nient'altro**. Memoria **solo in RAM**. Nessun
> `sessionStorage`, nessun `localStorage`, nessun timer.

Il motivo: `index.html:108` ripropone già il banner **a ogni avvio** finché il worker è in
attesa, e `:116` all'arrivo di una versione più nuova. La differenza fra una «X» e un «Più
tardi» è **l'etichetta, non il meccanismo** — la persistenza che verrebbe naturale aggiungere
sarebbe codice nuovo che duplica un comportamento che il worker ha già.

Verifica anche che il banner **non copra** l'azione principale: se copre, è un fatto di
posizionamento, ed è tuo.

## 8 — Cosmesi, facoltativa: `.scelta-categoria` ha due domini e un nome

La classe la usano **le categorie di spesa e le icone di collezione**, ma il nome parla di
spese. `.scelta-pastiglie` costerebbe tre sostituzioni.

**Decidi tu, che possiedi il file.** Se la fai, tocchi tre `.razor` che non sono tuoi: allora
**non la fare** e scrivila in `FUORI SCOPE`. Se resta com'è, va bene: è nomenclatura, non un
difetto.

## NON TOCCARE

- **Il resto di `app.css`.** Il file è grande e vecchio: **non riordinarlo, non deduplicarlo,
  non rinominare variabili, non "mentre ci sei".** Ogni riga che tocchi oltre le sette voci
  qui sopra è un rischio che nessun test copre, su un progetto senza test di regressione
  visiva. Il tuo diff dovrebbe essere di poche decine di righe.
- **`prefers-reduced-motion`**: dal 24 agosto 2026 Eton **non lo rispetta più, per scelta
  dell'utente**. Non riaggiungere quel blocco, in nessuna forma, nemmeno se un revisore lo
  segnala come accessibilità mancante. Se lo segnala, adjudica **infondato** citando questa
  riga.
- Tutti i file `.razor` non elencati nel perimetro condizionale.

## BUDGET DI COMPLESSITÀ

Nessuna variabile CSS nuova se ne esiste una adatta — `--tocco` esiste, usala. Nessun file
`.razor.css` nuovo. Nessun framework, nessuna dipendenza, nessun `.js`. Nessun `!important`:
se ti serve, la specificità è sbagliata e la regola va scritta meglio.

## STATO

Unità chiuse e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`), 05 (`e139ce8`),
06 (`f4f2dbd`), 07 (`4327598`), 12 (`8a4a89f`), 08 (`bdd858a`), 09, 10, 13.
**Sei l'ultima unità del piano.** Dopo di te il capo avvia il server e lancia `live-testing` su
tutto.

**Non committare.** Committa il capo, a resoconto letto.

Il piano è in `handoff/PIANO.md`. La sezione **«DA PORTARE NEL MANDATO DELL'UNITÀ 11»** è la
fonte delle voci 1-3 e 8: **rileggila**, perché le unità 09, 10 e 13 potrebbero averne aggiunte
altre dopo che questo mandato è stato scritto. Se ce ne sono, sono tue.

**Due fatti operativi.**

- Le `file:line` di `threat-hunter` sono state **sfasate** sulle unità 04 e 05 ed **esatte**
  sulle 07 e 08. Riapri i numeri prima di riportarli.
- Se un tuo obiettivo e un tuo divieto si contraddicono, **obbedisci al più specifico e
  dichiaralo**.

## IL GATE DELLA REVIEW

Un diff di solo CSS ricade in «formattazione e config»? **No.** Il §3 esenta commenti, rinomine
meccaniche e lockfile; una regola che cambia l'altezza dei bersagli e la leggibilità di un
pulsante spento **cambia il comportamento** dell'interfaccia. Lancia **`bug-hunter` e
`conformity`**. `threat-hunter` **no**: nessuna superficie di fiducia — a meno che tu non tocchi
`index.html`, dove c'è JavaScript, e allora **sì**.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**.
- `dotnet test` → **273 superati**, com'erano quando parti. Il CSS non ha test: il gate serve a
  provare che non hai rotto la build con un `.razor` toccato per sbaglio.

Compili **tu**, una volta, a fine giro.

**Non avviare il server di sviluppo e non provare nel browser**, benché sia la cosa che
verrebbe più naturale su un'unità di CSS. Lo fa il capo subito dopo di te, con `live-testing`, e
la tua sezione qui sotto è ciò che quell'agente andrà a misurare.

BUDGET: 20 dollari

RESOCONTO IN: `handoff/11-foglio-di-stile/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 11 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <ogni regola nuova o modificata, con il selettore verbatim e il file:line>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <cosa non hai fatto e a chi appartiene il rimedio>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

**Per ciascuna delle otto voci scrivi una riga d'esito**: fatta, non fatta col motivo, oppure
`BLOCKED` col motivo. Otto righe, nessuna omessa — anche la cosmesi, anche quelle che decidi di
lasciare stare. È l'unico modo perché il capo sappia cosa resta aperto senza riaprire il file.

Aggiungi `DA PROVARE NEL BROWSER` con, per ogni voce, **la misura attesa e come misurarla**:
un'altezza in pixel, una larghezza di viewport, uno stato del pulsante. Il tuo lavoro è l'unico
del piano che **non si può verificare leggendo il codice**, quindi le prove sono l'unica prova
che esista.
