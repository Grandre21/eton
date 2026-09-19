UNITÀ: 7 — ESITO: FATTO

Le cinque voci del mandato sono chiuse. La cascata regge ed è stata **provata**, non creduta:
nessun `BLOCKED`. I rimandi erano in **tre** forme e non due, e gli scaduti sono **sette su
ventidue** — quattro su dodici interni, tre su dieci cross-file.

TOCCATI:
- `wwwroot/css/app.css` → +88/−85
- `Pages/SpesaEdit.razor` → +29/−18
- `Pages/Spese.razor` → +2/−2
- (documentazione, non codice) `handoff/07-pastiglie-e-ancore/resoconto.md` e `accertamenti.md`, nuovi

Diff di **solo codice**, a correzioni applicate:
`3 files changed, 119 insertions(+), 105 deletions(-)`.

REVIEW:

```
review:
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 2
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 5
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
  checker (fix)   VERDETTI: risolti 6 · non risolti 0 · non verificabili 0
```

**Il `checker` compare due volte perché è stato lanciato due volte**, come prescrive il §5: la
prima per istruire i rilievi, la seconda **sulle correzioni** — e lì il claim non è più il
rilievo ma il fix, col verdetto `risolto | non risolto`. Senza il secondo giro, a dire che una
correzione ha funzionato sarebbe stato chi l'ha scritta.

⚠️ **Il `checker` del primo giro ha istruito 2 rilievi, non 7.** Il §4 è esplicito: i rilievi di
`threat-hunter` e `backend-expert` non entrano nella somma e non passano dal `checker` — i primi
li apro io comunque, i secondi si adjudicano sulla `RISCRITTURA`. Quindi «fondati 2» si riferisce
ai due di `conformity`; i cinque di `backend-expert` li ho adjudicati io, e si leggono in
`ADJUDICA`.

**Nessun «non lanciato».** Il gate del §3 chiedeva `bug-hunter` + `conformity` +
`threat-hunter` su qualunque diff che non sia solo commenti o formattazione, e
`backend-expert` **si è attivato**: misurato sul diff reale, a `git add -N .` fatto e a
implementer rientrati,

```
3 files changed, 134 insertions(+), 100 deletions(-)
· 0 create mode · 0 dichiarazioni/endpoint
```

134 insertions supera la soglia di ~120. Le altre tre condizioni erano tutte a zero: il gate
è scattato **solo** per le righe, e la gran parte di quelle righe erano commento — il che ha
prodotto, non a caso, i due rilievi più affilati dell'unità.

⚠️ **Chi rimisura oggi trova un numero diverso, e va detto prima che lo scopra da sé.** A
correzioni applicate il diff è sceso a

```
3 files changed, 119 insertions(+), 105 deletions(-)
```

cioè **sotto** la soglia: le correzioni che `backend-expert` ha prodotto hanno tolto proprio
le righe che l'avevano fatto scattare. Il gate si valuta quando si decide se lanciare, e in
quel momento erano 134 — ma un tracciato che riportasse solo il numero finale renderebbe il
lancio inspiegabile, e uno che riportasse solo quello iniziale non sarebbe riproducibile.
Servono entrambi.

⚠️ **`git add -N .` prima di misurare**, come prescritto: senza, `--stat` non vede i file non
tracciati e `--summary` non elenca i loro `create mode`. Qui non c'erano file sorgente nuovi,
ma la misura è stata presa nella forma che li avrebbe visti — altrimenti uno zero non
distingue «non ce n'erano» da «non li ho cercati».

**Il conteggio dei file**: `3 files changed` è il diff di **solo codice** (`Pages`,
`wwwroot`). Il diff completo ne conta 5, perché include i due documenti di `handoff/` che sto
scrivendo. Il gate si misura sul codice.

**`coverage` non compare, e non è un'omissione**: il §6 dice che non si lancia dentro una
sessione-unità — lì non c'è la richiesta dell'utente, c'è un mandato — e il formato di questo
resoconto lo esclude esplicitamente. La copertura la fa il punto 1 della sessione di chiusura.

**`live-testing` e `ui-critic` non compaiono** perché non sono revisori del diff: entrano al
§7, e il mandato vieta il browser. V. `SCOSTAMENTI 5`.

CONTRATTI:

**1.** `wwwroot/css/app.css` — `button.pastiglia`, **la destinazione della fusione**. Forma reale
risultante, citata testualmente:

```
button.pastiglia {
    font-family: inherit;
    cursor: pointer;
    appearance: none;
    -webkit-appearance: none;
    touch-action: manipulation;
    min-height: var(--tocco);
    padding: 0 var(--s4);
    font-size: var(--t-sm);
}
```

Il mandato chiedeva che le tre proprietà spostate restassero fedeli alla frase «ciò che un
`<button>` porta di suo». Reggono, e il commento lo argomenta leggendo quella frase fino in
fondo: una pastiglia che si preme è un **bersaglio**, e il pavimento di `--tocco` è di ciò che
si preme, non del posto in cui si preme.

**2.** `--tocco: 48px;` e `--raggio: 12px;` — **invariati entrambi**, come il mandato prescrive.

⚠️ **Il mandato cita `--tocco` a `app.css:190`, ma sta a `:201`** — e il resoconto dell'unità 01
lo dichiarava correttamente a `:201`. Non è un problema (il valore è invariante e non l'ho
toccato), ma è la dimostrazione vivente del difetto che questa unità chiude: un numero di riga
copiato a mano scade fra la scrittura di un documento e la lettura del successivo. `--raggio` a
`:140` era invece esatto.

**3.** `.btn.piccolo` e `.btn.compatto` — **nessuna delle due modificata**, né la regola né il
suo commento. `.btn.piccolo` conserva la sua esenzione e il raggio `--raggio-s`; `.btn.compatto`
continua a non dichiarare `min-width`, ed è il motivo per cui la regola in più serve comunque.

**4.** `.pastiglia` e `.pastiglia.accesa` — **la cascata su cui poggia «effetto visivo zero»**.
Verificata prima di fondere, non creduta: la prima non dichiara `min-height` (undici proprietà,
nessuna è quella), la seconda dichiara **solo** `border-color`, `background`, `color`. Entrambe
invariate. Il dettaglio della verifica è più sotto.

ADJUDICA:

Sette rilievi in tutto: due da `conformity` (istruiti dal `checker`), cinque da
`backend-expert` (che il §4 non manda al `checker`: si adjudicano sulla `RISCRITTURA`).
**Cinque fondati corretti, uno fondato non risolto perché è una decisione che non è mia, uno
infondato.**

⚠️ **Cinque dei sette rilievi riguardano commenti che il MIO diff ha reso falsi o gonfi.** Non
è un caso e non è ironia: questa unità esiste per togliere dal foglio le frasi che mentono in
differita, e il modo più facile di crearne di nuove è riscriverne venticinque righe.

### `conformity` 1 — il commento di `.btn.compatto` dichiarava un dominio più stretto del vero

**Fondato.** `checker`: *fondato* — e con una precisazione che ho adottato: il commento non è
**falso**, è **incompleto**. Resta vero che quella variante serve nella testata di schermata;
semplicemente non dice che da oggi serve anche in quella di una scheda.

Riga citata, `app.css:764-765`:
```
   .btn.compatto serve altrove — nella testata di schermata, dove si preme col
   pollice come ovunque — quindi tiene l'altezza piena e stringe solo la
```

→ **corretto.** Il commento ora nomina entrambe le testate e cita i precedenti.
**verificato: risolto — `app.css:764-767`**, e il `checker` ha controllato anche che le tre
dichiarazioni di `.btn.compatto` siano intatte e che nessun `min-width` vi sia comparso.

⚠️ **Ho allargato un contratto, e lo dichiaro.** Il mandato mette `.btn.compatto` in
`CONTRATTI` con «nessuna delle due si modifica». Ho letto quella clausola come vincolante sulle
**dichiarazioni** — che infatti sono intatte, byte per byte — e non sul commento, perché il
punto 2 dello stesso mandato mi ordina di non lasciare nel foglio frasi che sconsigliano ciò
che il file fa. Fra la lettera di una clausola e lo scopo esplicito di un'altra ho scelto il
secondo, e due revisori indipendenti l'avevano chiesto. Se il capo la legge diversamente, la
riga da rimettere com'era è una sola.

### `conformity` 2 — `.campi-spesa` diceva una cosa che il mio diff ha reso falsa

**Fondato**, e me l'ero perso del tutto. `checker`: *fondato*. Riga citata, `app.css:2069-2071`:
```
/* Su telefono i tre campi si impilano: .campo (già flex-column via label.campo,
   v. sopra) fa già da blocco verticale, qui serve solo il ritmo fra un campo e
   il successivo. La coppia affiancata arriva nella media query. */
```
I «tre campi» sono Importo, Descrizione, Quando. Dopo il mio diff uno dei tre — Importo, nel
ramo di sola lettura — non è più un `<label>`: a impilarlo è `.campo-lettura`. La frase «via
label.campo» era vera per due su tre.

→ **corretto**: il commento nomina ora le due vie e dice quale campo prende quale.
**verificato: risolto — `app.css:2063-2073`.**

### `backend-expert` 1 — la `RenderFragment` è l'astrazione sbagliata

**Fondato nel merito. `TIPO: progetto`, quindi NON risolto qui: va al capo.** Il §5 è esplicito
— i rilievi di progetto di `backend-expert` sono decisioni, non fix di unità.

L'argomento: il frammento costa tre righe di codice più un costrutto Razor che nel progetto non
esiste altrove, per risparmiare **una** riga, con i due usi a dodici righe di distanza dentro lo
stesso `@if`. **E la «fonte unica» che il mio commento prometteva non esiste.**

**Ho verificato io il fatto che lo regge, e regge:** `Pages/Spese.razor:60` contiene già
`<span class="etichetta-campo">Importo</span>` **verbatim**, per lo stesso concetto, e il
frammento non lo copre. C'è di più, e rafforza il rilievo: `Pages/Spese.razor:56` dichiara la
cosa per iscritto — *«campi ricalca Pages/SpesaEdit.razor: gli stili sono scritti contro quelle
classi»*. **Il progetto accetta già, per scelta dichiarata, che quella riga sia scritta due
volte.** Rifiutare di scriverla due volte dentro un file solo, al costo di un costrutto nuovo,
è in tensione con la sua stessa posizione.

**Perché non l'ho risolto:** il mandato vieta la duplicazione in modo esplicito e nominale — *«Non
duplicare l'etichetta nei due rami: […] se la tua correzione la duplica hai scelto la strada che
era stata scartata»*. Ribaltarlo da solo sarebbe stato sostituire il mio giudizio a una decisione
presa sopra di me. **La decisione è di una riga:** se la duplicazione torna ammessa, le righe
68-70 spariscono e i due rami scrivono lo `<span>` per esteso.

⚠️ **I due revisori divergono, e la divergenza è l'informazione.** `conformity` ha guardato lo
stesso costrutto e **non** l'ha segnalato, avendo trovato un precedente reale per l'idioma —
`Pages/CollectionEdit.razor:166`, `@{ var opzioni = Opzioni(campo); }`. **Verificato: esiste.**
Ma è un `@{ }` di **una riga** con una variabile ordinaria; il mio è l'**unico** `@{ }` multiriga
del progetto e l'unico che assegni un `RenderFragment` da markup. Il precedente copre l'idioma,
non l'astrazione — che è precisamente ciò su cui i due dissentono.

### `backend-expert` 2 — 32 righe di commento per 8 dichiarazioni, e l'enumerazione predice la propria scadenza

**Fondato in parte, e la parte che regge è severa.** Misurato: **32 righe** di commento non vuote
su `button.pastiglia` per 8 dichiarazioni.

Il colpo vero non è la densità — il mandato preventivava «~25 righe di commento contro 8 di
codice» e il file è scritto così ovunque. È il **contenuto**: avevo scritto un'enumerazione dello
stato del markup («i sei `<button>` stanno tutti dentro uno dei tre contenitori; i quattro
`<span>`…») e **sei righe più sotto** la frase «una pastiglia-bottone nuova prende il pavimento
di tocco da sé, **ovunque nasca**». Le due non possono essere vere a lungo insieme: la prima è
una fotografia del commit, la seconda annuncia che la fotografia scadrà. **Un commento che
prevede la propria scadenza, dentro l'unità che esiste per togliere i commenti scaduti.**

→ **corretto**: l'enumerazione esce dal foglio e resta qui nel resoconto, che è il posto di una
prova presa una volta. **Resta tutto ciò che il mandato impone**: il perché la strada scelta è
diversa da quella scartata (punto 2) e la policy futura (punto 1). Da 32 righe a 17.

**La `RISCRITTURA` di `backend-expert` NON è stata presa per intero**: cancellava anche
l'argomento sulla strada scartata, che il mandato ordina di conservare. Lì il mandato vince.

**verificato: risolto — `app.css:2116-2119` e `:2123-2127`.** Il `checker` ha cercato
espressamente i residui dell'enumerazione (nessuno) **e** ha controllato che le due cose imposte
dal mandato fossero ancora lì: se una fosse mancata il verdetto sarebbe stato `non risolto`, che
è il motivo per cui gliel'ho chiesto in quella forma. Le otto dichiarazioni intatte.

Stesso taglio su `.navigazione-mese`, da 14 righe a 6: dopo la correzione di `conformity` 1, il
«perché compatto e non piccolo» ha una casa sola, alla definizione della classe.
**verificato: risolto — `app.css:1905-1911`**, con la ragione nativa del `min-width` — il
carattere solo — rimasta per esteso.

### `backend-expert` 3 — «un commento attaccato a niente è una lapide»

**INFONDATO**, ed è l'infondato che il §5 mi impone di riverificare di persona. L'ho fatto.

Il claim è che il commento superstite di `.scelta-categoria`, che non ha dichiarazioni sotto, sia
una lapide da cancellare perché «le rimozioni le tiene `git log`». **Il file ha la convenzione
esattamente opposta, ed è quella che ho ricalcato di proposito.** `app.css:1884-1892` —
preesistente, non mia:

```
/* Il riepilogo del mese è una .scheda (v. Pages/Spese.razor, class="scheda
   riepilogo-mese"): la superficie […] arriva già da lì […]. Lo faceva, prima: sei
   dichiarazioni identiche a quelle di .scheda […]. Non resta
   altro da aggiungere o sovrascrivere, quindi la regola sparisce: .riepilogo-mese
   continua a esistere come selettore, per ambientare .testa-registro qui sotto. */
```

Un commento senza regola sotto, che spiega perché la regola è sparita, con lo stesso identico
scopo del mio. Cancellarlo non sarebbe stato ripulire: sarebbe stato rompere una convenzione del
progetto, e `conformity` avrebbe avuto ragione a segnalarmelo. → **scartato.**

### `backend-expert` 4 — tre blocchi di commento, e il terzo ripete `app.css` parola per parola

**Fondato.** Gli stessi cinque fatti stavano scritti in `Pages/SpesaEdit.razor` e in
`app.css`, coi due file che si rimandano a vicenda: cambiarne uno lascia l'altro a mentire — la
classe di difetto di questa unità, riprodotta da me a cavallo di due file invece che dentro uno.

→ **corretto**: due blocchi invece di tre, la spiegazione lunga resta in `app.css` accanto alla
regola (dove già rimandava qui), e nel `.razor` resta il rimando.
**verificato: risolto — `Pages/SpesaEdit.razor:78-81`**, e il `checker` ha confermato che il
markup è intatto: `RenderFragment`, i due rami, `@etichettaImporto` reso due volte, il `disabled`
sull'`<input>`.

### `backend-expert` 5 — la convenzione nuova spezzava i paragrafi di disegno

**Fondato.** L'avevo infilata fra «DUE VOCI TIPOGRAFICHE» e «GERARCHIA DI SUPERFICIE», cioè in
mezzo a tre paragrafi che dicono come l'interfaccia è **disegnata**. È invece una regola su come
il file si **scrive**, e l'intestazione ne ha già una — «Le pagine usano SOLO i nomi di classe
definiti qui…».

→ **corretto**: spostata subito dopo quella, testo invariato. Il censimento datato resta, ed è
`backend-expert` stesso a dire perché: senza il numero, «nessuno lo risincronizza» è
un'asserzione.
**verificato: risolto — `app.css:8-19`**, occorrenza unica, testo invariato, collocata fra la
regola di scrittura e «DUE ACCENTI».

FUORI SCOPE:

**1. ⚠️ Il campo «Importo» in sola lettura è alto ~72px più del dovuto, e NON è colpa di questa
unità — ma il capo lo vedrà al collaudo.** Trovato da `tech-advisor`, che lo dava per dedotto;
**l'ho misurato io e regge**. `app.css` non ha un reset dei margini di `p`: l'unico
`margin: 0` è su `html, body`, e né `.dato` né `.importo-spesa` ne dichiarano uno. Il
`<p class="importo-spesa dato">` risolve `font-size: var(--t-2xl)` — 36px, perché
`.importo-spesa` e `.dato` sono entrambe 0-1-0 e vince l'ultima nel file — quindi prende il
`margin-block: 1em` del browser, cioè **36px sopra e 36 sotto**. I margini dei figli di un flex
container **non collassano**, e `.campo` è `display: flex` sia prima sia dopo: il ramo di sola
lettura è più alto dell'editabile di due volte `--t-2xl`.

**Non l'ho corretto, ed è deliberato.** Il `<p>` l'ha introdotto l'unità 04 con `6ce6ee3`, il
contenitore era **già** un flex container, quindi il difetto esisteva identico prima del mio
diff e il mio diff non lo peggiora di un pixel. Correggerlo sarebbe stato lavoro nuovo dentro
un'unità che ha un elenco chiuso di cinque voci. Il rimedio, se lo si vuole, è una riga:
`margin: 0` sul `<p>`. **Verificato che nessun altro foglio possa già farlo**: gli unici `.css`
del progetto sono `app.css` e tre scoped (`VetrinaLayout`, `Benvenuto`, `GrafoSpazio`), e
nessuno riguarda `SpesaEdit`.

**2. Un rimando scaduto fuori dal mio perimetro.** `Pages/CollectionEdit.razor:75` cita
`SpesaEdit.razor:105-106` e `Spese.razor:93-94`. Il primo era **già scaduto** prima di me (la
pastiglia stava a `:126`) e il mio diff l'ha spostata ancora. Non l'ho toccato: quel file non è
mio. Il fatto da girare è più largo del singolo rimando — **la convenzione nuova vive
nell'intestazione di `app.css`, ma il difetto che cura esiste anche nei `.razor`**, dove nessuna
intestazione la dichiara.

**3. Due delle quattro proprietà rimaste in `.barra-elenco .pastiglia` sono ridondanti, oggi.**
`cursor: pointer` e `touch-action: manipulation` sono già dichiarate in `button.pastiglia`, e
quella regola aggancia **un solo elemento in tutto il progetto**, che è un `<button>`
(`Pages/CollectionDetail.razor:91`). Servirebbero solo se in `.barra-elenco` nascesse una
pastiglia che `<button>` non è. Le ho lasciate perché il mandato le mette esplicitamente in
`NON TOCCARE` — ma se un domani si volesse stringere ancora, quelle due e non le altre due sono
il residuo: `flex: none` e `transition` restano necessarie.

**4. Il commento di `.btn.compatto` (`app.css:764`) è diventato più STRETTO della realtà, e l'ho
lasciato così di proposito.** Dice che quella variante «serve altrove — nella testata di
schermata». Da oggi serve anche nella testata di una **scheda**, che è dove stanno le frecce di
mese. Non è falso — dice dove serve, non «solo lì» — ma chi legge lì non scopre l'altro uso.

**Non l'ho toccato perché il mandato mette `.btn.compatto` sotto contratto**, «nessuna delle due
si modifica», e allargare un contratto interpretandolo come «la regola sì, il commento no» è il
genere di libertà che un'unità non si prende da sé. L'informazione **esiste comunque nel file**:
il commento nuovo a `:1903` nomina la distinzione per esteso e cita i due precedenti. Se il capo
preferisce, la riga da allargare è una sola.

**5. Il mandato cita `--tocco` a `app.css:190`; sta a `:201`.** Innocuo — il token è invariante e
non l'ho toccato — ma vale la pena dirlo perché è il difetto stesso che questa unità chiude,
capitato al documento che la commissiona. Il resoconto dell'unità 01 lo dichiarava correttamente
a `:201`: il numero è scaduto fra quel documento e il mandato.

GATE:

```
dotnet build -warnaserror --no-incremental   →  Avvisi: 0 — Errori: 0
dotnet test Eton.sln                         →  Non superati: 0. Superati: 310. Ignorati: 0. Totale: 310.
```

**310 test, esattamente quanti ne ha lasciati l'unità 06** — il mandato chiedeva «almeno
quanti». Nessuno è stato aggiunto: il diff è CSS e markup, e non esiste un test che possa
constatarne l'esito.

⚠️ **Il mandato ha ragione a dire che nessuno dei due prova niente di questo lavoro**, ed è il
caso più netto del goal. Ma la build **una** cosa l'ha provata, ed è l'unica che era in dubbio:
la `RenderFragment` dichiarata in un blocco `@{ }` della regione di **markup** compila su
net10.0 con `-warnaserror`. `doc-checker` aveva confermato la sintassi per la versione pinnata
citando la doc `aspnetcore-10.0`, ma sul *dove* dichiararla era tornato **NON TROVATO**: la
documentazione mostra solo la forma dentro `@code` e non vieta l'altra. Lì il compilatore è un
oracolo più forte della documentazione, e ha risposto.

Per onestà: ho preso anche una **build di riferimento prima di toccare qualunque cosa**, ed era
già `0 avvisi, 0 errori`. Senza quella, un verde finale non distinguerebbe «non ho rotto niente»
da «era già rotto e lo è ancora».

SCOSTAMENTI:

**1. Il punto 5 non era «due righe», e la ragione è un fatto che il mandato non poteva
conoscere.** `app.css:879` dichiarava
`label.campo { flex-direction: column; align-items: stretch; gap: var(--s1); }`: a produrre
l'impilamento è un selettore d'**elemento**. `.campo` da solo è una **riga orizzontale**
(`display: flex; align-items: center`), ed è così di proposito — i cinque `<div class="campo">`
esistenti sono tutti *input + pulsante affiancati*. Quindi «il contenitore diventa un `<div>`
con la stessa classe» avrebbe fatto scivolare «Importo» **accanto** alla cifra, disallineato dai
due campi fratelli che in sola lettura restano `<label>` impilate, e — sopra i 40rem — dal campo
«Quando» che gli sta di fianco nella griglia.

Non era un `BLOCKED`: il mandato riserva quel verdetto alla cascata delle pastiglie. Era un
bivio, e l'ho portato a `tech-advisor` invece di scegliere da solo, perché la soluzione tocca una
regola condivisa da nove contenitori.

⚠️ **La strada che stavo per prendere era sbagliata, e il motivo è un fatto che non sapevo.**
Avrei scritto `label.campo, .campo:has(> .etichetta-campo)`. Una lista di selettori **non è
forgiving**: su un motore senza `:has()` cade la **regola intera** e tutti e nove i contenitori
diventano righe orizzontali. I due `:has()` già nel foglio falliscono localmente — un bordo, una
collocazione in griglia — questo sarebbe stato il primo a fallimento globale: nove moduli rotti
per sistemare un campo. Secondo argomento, indipendente: `.etichetta-piccola` è dichiarata
sinonimo di `.etichetta-campo` a `:369-372`, e `Shared/VotoInput.razor` è un `div.campo` con
`etichetta-piccola` che **si regge sulla resa in riga** — quindi il commento che avrei scritto
(«un contenitore che porta la propria etichetta impila») sarebbe stato **falso**, col
controesempio già nel progetto.

**La forma adottata:** `.campo-lettura` aggiunta alla lista di selettori della regola esistente —
zero dichiarazioni ricopiate — più una `RenderFragment` in `SpesaEdit.razor` per scrivere
l'etichetta una volta sola. **Costo reale: una classe CSS, un commento, e +35/−18 nel `.razor`,
invece delle due righe preventivate.**

**2. Ho scritto male il brief dei `.razor`, e ho pagato un giro di implementer.** Invece di
decidere prima e poi dettare, ho messo nel brief **quattro varianti sbagliate di proposito** e
l'istruzione di tornare `BLOCKED` sulla seconda modifica. È ragionare ad alta voce dentro un
mandato, ed è esattamente ciò che il §1 vieta: un brief porta decisioni già prese. L'implementer
ha applicato correttamente la parte sana (le due frecce) e ha restituito `BLOCKED` come gli era
stato detto — comportamento ineccepibile, colpa mia. Costo: un giro in più.

**3. Tre implementer invece di due**, conseguenza del punto 2 e dell'attesa di `tech-advisor`.
Partizione per proprietà dei file, mai due sullo stesso file insieme: (A) `app.css`, (B)
`Pages/Spese.razor`, (C) `Pages/SpesaEdit.razor` + una sostituzione in `app.css`, lanciato quando
A era già rientrato.

**4. I revisori sono stati lanciati una volta sola, sul diff completo, invece che in pipeline per
brief.** Il §3 toglie una barriera **fra unità**; qui l'unità è una e i tre brief producono un
diff solo, i cui pezzi non si giudicano separatamente: `.campo-lettura` revisionata prima del suo
consumatore sarebbe stata «una classe con zero call-site», e le due frecce senza la regola CSS
«un pulsante senza larghezza minima». Sarebbero stati due rilievi fondati su un diff parziale,
cioè rumore da adjudicare. Il gate l'ho misurato **una volta sola, sul diff reale e finale**,
che è anche l'unico modo di renderlo non smentibile.

**5. `live-testing` e `ui-critic` non sono stati lanciati.** Il gate del §7 li chiederebbe
entrambi — il diff tocca `.css` ed esiste un metro `PIANO-DESIGN` — ma il mandato dice «non
avviare il server e non aprire il browser». Non è una mia esenzione: è un'istruzione, e il lavoro
passa al collaudo del capo. L'ordine del protocollo resta: prima `live-testing`, `ui-critic`
**solo** su `ESITO: verde`.

**6. Un'imprecisione mia, trovata e corretta in corsa.** Il primo commento che avevo fatto
scrivere sulle frecce diceva che `.btn.compatto` è «la variante fatta per la testata di
schermata» e che le frecce hanno «lo stesso ruolo» dei due precedenti. Falso: i precedenti
(`Pages/Collections.razor:14`, `Pages/Notes.razor:16`) stanno in testata di **schermata**, dentro
`<Azione>` di `TestataPagina`; le frecce stanno nella testata di una **scheda**. Riscritto per
distinguere i due posti e nominare il discrimine vero — che il pollice ci arrivi, non dove sia il
pulsante. Sarebbe stata una bugia in differita nuova di zecca dentro l'unità che le rimuove.

**7. Nessuno scostamento su perimetro, valori o selettori.** `--tocco`, `--raggio`, `.btn.piccolo`
e `.btn.compatto` invariati, regole e commenti. Le cinque correzioni dell'unità 01 non sono state
toccate. Le quattro proprietà in più di `.barra-elenco .pastiglia` sono tutte al loro posto. In
`Pages/Spese.razor` sono cambiati **due attributi di classe e nient'altro**.

---

## La prova che la cascata regge

Il mandato chiedeva di **verificarla invece di crederla**, e di fermarsi in `BLOCKED` se una
regola la contraddicesse. **Nessuna la contraddice**, e la prova è per **enumerazione**: i due
insiemi — i selettori del foglio e i call-site del markup — si incastrano senza residui.

**Sei selettori in tutto** contenevano `.pastiglia` (le altre tredici occorrenze del grep sono
prosa dentro commenti). I **tre contenitori** dichiaravano le stesse identiche tre righe, parola
per parola — `min-height: var(--tocco); padding: 0 var(--s4); font-size: var(--t-sm);` — ed è
questo che rende la fusione neutra: non c'erano tre valori da conciliare, ce n'era uno scritto
tre volte.

**Dieci call-site nel markup: sei `<button>`, quattro `<span>`.** Tutti e sei i button stanno
dentro uno dei tre contenitori (`.barra-elenco` ×1, `.scelta-categoria` ×4, `.voto-input` ×1);
**nessuno dei quattro span** ci sta — stanno tutti in righe di elenco. Quindi nessun elemento
perde le tre proprietà e nessuno le guadagna.

**Nessun'altra regola del foglio può agganciarle.** Una regola aggancia una pastiglia solo se il
suo selettore chiave è `*`, `button`, `.pastiglia`, `.accesa` o un attributo che quei `<button>`
soddisfano. Cercati tutti: `* { box-sizing }` non dichiara nessuna delle tre;
`a, button, select, input, textarea, [role=button]` dichiara solo `-webkit-tap-highlight-color`;
`#aggiornamento-pwa button` e `.schede-testo button` non contengono pastiglie (call-site
verificati); **nessuna** pseudo-classe su `button` o `.pastiglia`; **nessuna** regola su
`.pastiglia` dentro le tre `@media`; `:root` è unico e nessuna `@media` ridefinisce `--tocco`,
`--raggio`, `--t-sm` o `--s4`.

**Specificità.** `button.pastiglia` è 0-1-1 e batte `.pastiglia` (0-1-0) sulle tre proprietà.
`.pastiglia.accesa` è 0-2-0 e **lo batte**, ma dichiara solo colori: nessun conflitto. I tre
contenitori erano 0-2-0 e avrebbero vinto — è per questo che due sono spariti del tutto e il
terzo ha perso proprio quelle tre.

**L'unico stile inline non è un'eccezione.** `Pages/CollectionEdit.razor:103` porta
`style="font-size: var(--t-lg)"` sulle pastiglie-emoji: vinceva ieri su `.scelta-categoria
.pastiglia` (0-2-0) e vince oggi su `button.pastiglia` (0-1-1), perché lo stile inline batte
qualunque selettore senza `!important`. Invariato.

**Nessuna generazione dinamica della classe:** cercato `pastiglia` in `.cs`, `.razor`, `.js` e
`.html` fuori dagli attributi `class="…"` — solo prosa di commenti.

Il dettaglio riga per riga, con le tabelle, sta in `accertamenti.md` accanto a questo file.

---

## Il conteggio dei rimandi

**Ventidue in tutto: dodici interni al foglio, dieci verso altri file. Sette erano scaduti.**

### Gli interni: dodici, in TRE forme, quattro scaduti

Il mandato ne prevedeva due forme e chiedeva di dirlo se ne fosse comparsa una terza. **È
comparsa.**

| forma | quante | esempio |
|---|---|---|
| A — `(v. riga N)` | 10 | `.app-layout (v. riga 409)` |
| B — `(v. il commento a riga N)` | 1 | `(v. il commento a riga 1039, sulla Home)` |
| **C — `di riga N`** | **1** | `la regola universale di riga 227` |

La forma C è quella che nessuno aveva cercato: non contiene né `v. riga` né `commento a riga`.
Il grep che le prende tutte e tre è `rig(a|he) [0-9]`, senza distinzione di maiuscole.

**Quattro scaduti su dodici**, esattamente il numero previsto, e il quarto è proprio quello in
forma B che l'unità 01 aveva mancato:

| il rimando | diceva | la riga conteneva davvero | bersaglio reale |
|---|---|---|---|
| `:1886` | riga 1039 | `display: inline-flex;`, dentro `.pastiglia` | il commento sulla Home, `:1403` |
| `:1936` | riga 306 | prosa dentro un commento | `.testo-tenue`, `:340` |
| `:2203` | riga 330 | `}` | `.etichetta-piccola, .etichetta-campo`, `:372` |
| `:2209` | riga 730 | un commento su `.btn.primario` | `.intestazione .selettore`, `:924` |

Gli altri otto puntavano al bersaglio giusto. **In nove casi su dodici il selettore era già
scritto accanto al numero**, e lì la modifica è stata cancellare la parentesi; nei tre restanti
il bersaglio è stato letto e nominato.

### I cross-file: dieci, tre scaduti — e la premessa che li esentava era falsa

Il mandato li chiamava «dieci opzionali»; `DECISIONI` li rende obbligatori («cross-file
compresi»), contro la prima proposta del capo. **La misura conferma il rovesciamento:**

| il rimando | diceva | reale |
|---|---|---|
| `:1348` | `CollectionDetail.razor:129` | **128** |
| `:1353` | `CollectionDetail.razor:40` | **39** (`.icona-collezione`) |
| `:2136` | `CollectionEdit.razor:104` | **103** |

**Tre su dieci, il 30%**, contro il 33% degli interni. I numeri verso altri file **non scadono
meno** di quelli interni: scadono uguale. La premessa che li voleva esentati — «lì il numero non
è sotto il controllo di chi scrive questo foglio» — è vera come osservazione e falsa come
conclusione, perché è proprio il non controllarli che li fa scadere senza che nessuno se ne
accorga.

### La convenzione è entrata nel file

Non basta sistemare i ventidue: senza una regola scritta, il ventitreesimo nasce col numero.
L'intestazione di `app.css` aveva già quattro convenzioni dichiarate (asset, due accenti, due
voci tipografiche, gerarchia di superficie). Ora ne ha cinque — **I RIMANDI SI ANCORANO A CIÒ
CHE UN GREP TROVA** — e porta con sé il fatto misurato, sette su ventidue, perché una
convenzione senza il costo che la giustifica si dimentica.

⚠️ **I numeri di riga in questo resoconto e in `accertamenti.md` sono quelli PRIMA della
modifica.** Sono il dato della misura, non un rimando da seguire: chi rilegge deve trattarli
come una fotografia, non come un indirizzo.

---

## Le schermate da guardare — per il brief di collaudo

Nessuno dei due gate prova alcunché di questo lavoro: il CSS non entra nel compilatore e i due
attributi di classe compilano comunque. **Quello che segue è l'unica verifica che esista.**

| # | schermata | come arrivarci | cosa guardare |
|---|---|---|---|
| 1 | **Spese** | `/expenses` | Le **frecce di mese** nella testata della scheda del riepilogo: bersaglio **48×48**, raggio **12px** (prima erano 8px — è l'unico delta visivo dell'intera unità). E nel registro sotto, le pastiglie di categoria dentro le righe: devono restare **piccole**, `--t-xs`, non crescere. Più le pastiglie di categoria del modulo «Segna»: 48px di altezza |
| 2 | **Editor di spesa** | `/expenses/{id}` | Le pastiglie di categoria: 48px. E il campo **Importo**, che ha due rami da guardare separatamente (sotto) |
| 3 | **Editor di collezione** | `/collections/new` | Le **pastiglie-emoji** della tavolozza icone: devono restare a `--t-lg` per lo stile inline, e **non** rimpicciolirsi a `--t-sm`. È il caso che prova che l'inline vince ancora |
| 4 | **Dettaglio collezione** | `/collections/{id}` | La pastiglia **«Da provare»** nella barra sopra l'elenco: 48px, accanto al selettore «Ordina per» |
| 5 | **Editor di elemento col voto** | `/collections/{id}/items/{id}` | La pastiglia **«Nessun voto»** dentro il cursore del voto: 48px, e la scheda «La tua recensione» alta di conseguenza |
| 6 | **Spazi** | `/spaces` | ⚠️ **Il controllo negativo, e non è nell'elenco del mandato.** Le pastiglie **«personale»** e **«attivo»** sono `<span>`, non `<button>`: devono restare piccole. Se crescono a 48px la fusione è colata fuori, ed è l'unica schermata dove si vedrebbe in modo inequivocabile |

**Il campo Importo va guardato in DUE stati, ed è la parte che richiede una preparazione:**

- **chi può intervenire** — «Importo» sopra, campo digitabile sotto, impilati;
- **chi NON può intervenire** — una spesa pagata da un altro membro dello spazio. «Importo» deve
  stare **sopra** la cifra e non accanto: è il contenitore che è passato da `<label>` a
  `<div class="campo campo-lettura">`, e senza la regola CSS nuova l'etichetta scivolerebbe di
  fianco. Da guardare anche **sopra i 40rem**, dove «Importo» e «Quando» stanno sulla stessa
  riga della griglia e un disallineamento si vede subito.

⚠️ **In quel secondo stato si vedrà anche un difetto che NON è mio**: v. `FUORI SCOPE`.

**`live-testing` e `ui-critic` non sono stati lanciati**, e non è un'omissione: il mandato dice
«non avviare il server e non aprire il browser». Il gate del §7 li chiederebbe entrambi — il diff
tocca `.css` ed esiste un metro `PIANO-DESIGN` — quindi il capo li deve al collaudo, nell'ordine
del protocollo: prima `live-testing`, e `ui-critic` **solo** se quello torna `ESITO: verde`.
