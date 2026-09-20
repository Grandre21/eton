# Istruttoria delle due voci che non si correggono — 23 e 25

Documento di lavoro dell'unità 04. Il mandato dice: «Sono **non adjudicate** e toccano decisioni di
progetto. Il tuo lavoro è portarle a una decisione scritta, non prenderla.» Qui ci sono i numeri; la
sintesi sta nel `resoconto.md`.

---

## VOCE 23 — una classe, e sei corpi

`ui-critic` l'ha misurata così: «`.dato { font-size: .95em }` produce **12,35px e 15,2px** su quattro
rotte, mentre la stessa classe con corpo esplicito legge 13px: **tre corpi per una classe sola**».

### Primo scarto: i punti d'uso non sono quattordici

Il mandato ne dichiara quattordici. Contati ora sul worktree, escludendo le tre righe di **commento**
che contengono la stringa `class="dato"` (`Pages/Collections.razor:120`, `Pages/Notes.razor:163`,
`Pages/Spese.razor:504`), i punti d'uso reali nel markup sono **sedici**. La differenza non cambia
nessuna conclusione, ma il numero che va nel piano è 16.

### I sei corpi, e perché sono sei e non tre

`em` in `font-size` è relativo al corpo del **genitore**, quindi `.dato` non ha un corpo: ne ha uno
per contesto. E quando accanto a `.dato` c'è una classe che dichiara il proprio `font-size`, chi
vince dipende da specificità e ordine — `.dato` sta a riga 393, tutte le altre più sotto.

| Corpo | Punti d'uso | Da dove viene |
|---|---|---|
| **12,35px** | `Pages/CollectionDetail.razor:143`, `Pages/Collections.razor:91`, `Pages/Notes.razor:95`, `Pages/Spese.razor:192` | `<span class="dato">` **figlio** di un `.meta` da 13px: `.95 × 13` |
| **13px** | `Pages/Home.razor:182`, `Pages/Home.razor:221`, `Pages/Spese.razor:152`, `Pages/Spese.razor:153` | `.dato` sullo **stesso** elemento di `.riga-nota .meta` / `.riga-collezione .meta` / `.barre-categorie .quota` / `.barre-categorie .totale-categoria`, che valgono 0-2-0 e battono `.dato` (0-1-0) |
| **15,2px** | `Pages/Spese.razor:198`, `Pages/Spese.razor:80`, `Pages/SpesaEdit.razor:119` | genitore in prosa a 16px: `.95 × 16`. Il primo è `.riga-spesa .importo`, che non dichiara corpo; gli altri due sono `<input class="data-spesa dato">` |
| **26px** | `Pages/Home.razor:130` | `.striscia-spese .totale` (0-2-0), `--t-xl` |
| **36px** | `Pages/SpesaEdit.razor:91`, `Pages/SpesaEdit.razor:99`, `Pages/Spese.razor:61` | `.importo-spesa` (0-1-0, dichiarata **dopo** `.dato`), `--t-2xl` |
| **52px** | `Pages/Spese.razor:143` | `.totale-mese` (0-1-0, dichiarata dopo), `--t-3xl` |

### Quali sono intenzionali e quali no

**Intenzionali — quattro corpi, dodici punti d'uso su sedici.** 13, 26, 36 e 52 sono dichiarati da una
classe che sta lì apposta (`.quota`, `.totale`, `.importo-spesa`, `.totale-mese`) e sono tutti gradini
della scala. Lì `.dato` non porta un corpo: porta il **mono** e `tabular-nums`, che è il suo mestiere
secondo l'intestazione del foglio («Plex Mono scrive i DATI»). Toccarli sarebbe un danno.

**Non intenzionali — due corpi, quattro più tre punti d'uso.** 12,35 e 15,2 non stanno sulla scala
(11 · 13 · 15 · 16 · 20 · 26 · 36 · 52) e nessuno li ha scelti: sono il prodotto aritmetico di un
`.95em` in due contesti diversi.

Il `.95em` **ha una ragione**, e va detta prima di proporre di toglierlo: a parità di corpo il Plex
Mono appare più grande dell'Inter, e ridurlo del 5% serve a farlo sembrare della stessa misura del
testo accanto. È una correzione ottica legittima. Il difetto non è l'intenzione, è che il risultato
esce dalla scala — e, in tre punti, fa qualcosa di peggio.

### Il fatto che nessuno aveva censito: `.dato` annulla la protezione iOS

La regola base dei campi dichiara, con il suo commento:

    font-size: var(--t-base);   /* sotto i 16px iOS ingrandisce la pagina al fuoco */

Quella regola è avvolta in `:where(...)`, cioè vale **zero** di specificità. `.dato` vale 0-1-0 e
vince. Risultato: i due `<input class="data-spesa dato">` (`Pages/Spese.razor:80`,
`Pages/SpesaEdit.razor:119`) stanno a **15,2px**, e su iOS la pagina si ingrandisce quando ci si
entra dentro — esattamente ciò che quel commento dice di voler evitare.

Non è un'inconsistenza estetica: è una protezione dichiarata nel foglio e disattivata da un'altra
regola dello stesso foglio. Vale per i campi, non per gli altri punti d'uso.

### La proposta

**Non converto `.dato` a un valore assoluto**, che il mandato vieta e che sarebbe comunque una
decisione sulla scala, cioè dell'unità 05. Propongo di separare la voce in due decisioni, perché sono
di natura diversa:

**23a — i due campi di testo a 15,2px.** Indipendente dalla scala: non sceglie un gradino, ripristina
quello che la regola base già dichiara. Una riga, da mettere accanto a `.dato`:

    /* Su un campo di testo il .95em qui sopra vince sulla regola base — che è in :where() e
       vale zero — e porta il corpo a 15,2px: sotto i 16px iOS ingrandisce la pagina quando ci
       si entra dentro, che è precisamente ciò che la regola base dichiara di voler evitare.
       Il mono resta; il corpo torna quello dei campi. */
    input.dato, select.dato, textarea.dato { font-size: var(--t-base); }

Effetto misurato sui punti d'uso: i due `.data-spesa` passano da 15,2 a 16px. I tre
`<input class="importo-spesa dato">` non sono toccati, perché `.importo-spesa` (0-1-0, più sotto nel
foglio) continua a vincere su `input.dato` (0-1-1)? **No**: `input.dato` vale 0-1-1 e batte
`.importo-spesa` 0-1-0. Se si adotta questa riga va quindi scritta come `:where(input, select,
textarea).dato { font-size: var(--t-base) }` — 0-1-0, stessa specificità di `.importo-spesa`, che
essendo dichiarata dopo continua a vincere e i campi dell'importo restano a 36px. **È la forma da
adottare**; l'altra rovinerebbe l'importo per rimediare alla data.

**23b — i quattro `<span>` a 12,35px.** Qui si sceglie un gradino della scala, quindi **è dell'unità
05**. Il fatto da consegnarle: quattro punti d'uso, tutti figli di un `.meta` da 13px, tutti in
elenchi (`.riga-collezione`, `.riga-nota`, `.riga-spesa`, `.riga-elemento`). Se la 05 decide che
`.dato` non deve avere corpo proprio, quei quattro passano a 13px e il Plex Mono lì appare un filo più
grande dell'Inter accanto: è il prezzo della correzione ottica che si perde, e va guardato sulla resa
prima di accettarlo.

---

## VOCE 25 — lo stesso pulsante in due forme, e il suo gemello

`ui-critic`: «"Nuova collezione" è `.btn.primario` 16px/181px in Home e `.btn.primario.compatto`
13px/133px su `/collections`: stesso testo, due forme.» Il mandato aggiunge il gemello: «Nuova nota»
ha la stessa doppia forma.

### La mappa completa: non due punti, sei

| Etichetta | Punto | Forma | Posizione |
|---|---|---|---|
| Nuova collezione | `Pages/Collections.razor:14` | `btn primario compatto` | testata di schermata (`<Azione>`) |
| Nuova collezione | `Pages/Collections.razor:71` | `btn primario` | dentro `.vuoto`, a registro vuoto |
| Nuova collezione | `Pages/Home.razor:230` | `btn primario` | `.azioni` in fondo alla sezione |
| Nuova nota | `Pages/Notes.razor:16` | `btn primario compatto` | testata di schermata (`<Azione>`) |
| Nuova nota | `Pages/Notes.razor:72` | `btn primario` | dentro `.vuoto`, a registro vuoto |
| Nuova nota | `Pages/Home.razor:190` | `btn primario` | `.azioni` in fondo alla sezione |

Le due coppie sono **identiche fra loro in tutti e tre i punti**. Il gemello non aggiunge
un'incoerenza: la stessa regola è applicata due volte.

### La regola esiste già, scritta in due posti

Il commento di `.btn.compatto` in `app.css` la dichiara: la variante serve «in una TESTATA: di
schermata … oppure di scheda». E il commento di `Pages/Collections.razor:60-66` dichiara l'altra metà,
con la ragione:

> «A registro vuoto l'occhio sta su questo blocco centrato — è lì che va messa l'azione — mentre
> quello in testata è compatto e in un angolo. Non è ridondante nemmeno per gli stati: a registro
> pieno questo blocco non esiste, e resta solo la testata.»

Quindi la forma dipende dalla **posizione**, non dall'etichetta: in testata compatto, nel flusso
pieno. I due pulsanti che `ui-critic` ha confrontato stanno in due posizioni diverse, e nessuno dei
due viola la regola.

### Il fatto che la voce 25 non dice

Le due forme **non convivono mai a schermo**: a registro pieno esiste solo la testata, a registro
vuoto esistono testata e blocco centrato — ed è l'unico caso in cui si vedono insieme, che è anche
l'unico in cui il rapporto non ha guardato. Su `/collections` con zero collezioni, «Nuova collezione»
compare **due volte nella stessa schermata**, a 133px in alto e a 181px al centro. È il caso su cui
vale la pena decidere; il confronto Home ↔ `/collections` no, perché sono due schermate diverse con
due posizioni diverse.

### La proposta, per entrambe le coppie

**A — dichiarare intenzionale, e chiudere la voce senza codice.** La regola c'è, è scritta, è
applicata in modo identico sulle due coppie. Costo: zero righe. Si aggiunge una frase al commento di
`.btn.compatto` che nomina il terzo luogo (il blocco `.azioni` in fondo a un registro della Home), così
il commento copre tutti e tre i casi invece di due.

**B — uniformare al pieno**: togliere `compatto` dalle due testate. Costo: il pulsante passa da 133 a
181px accanto a un `h1` da 26px su schermo stretto; va verificato a 360px che il titolo non vada a
capo. Contro: perde la ragione per cui `.btn.compatto` esiste, e la regola del foglio andrebbe
riscritta.

**C — uniformare al compatto**: rimpicciolire il pulsante della Home e dello stato vuoto. Contro: lo
stato vuoto è il momento in cui l'unica cosa da fare è quella, e il principio del progetto è che
l'azione lì sta al centro dell'occhio.

**Raccomandazione: A**, più una verifica di collaudo sul caso «registro vuoto», che è l'unico in cui
le due forme si vedono insieme e che nessuno ha ancora guardato. Se lì la doppia comparsa stona, la
decisione da prendere non è sulla forma del pulsante ma su **quale dei due togliere**, e quella è una
decisione di prodotto.
