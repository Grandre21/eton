# UNITÀ 5 — scala-e-metro

UNITÀ: 5 — ESITO: FATTO

Sei voci. **Quattro chiuse, due istruite e consegnate** — e la ripartizione non è quella che il mandato
prevedeva, perché due delle tre voci «di scala» si sono rivelate difetti veri una volta guardate, mentre
la voce che il rapporto dava per più grave si è rivelata infondata nella sua premessa.

Il mandato diceva che il mio esito poteva legittimamente essere **nessuna riga di codice**. Ne ho
scritte, e il motivo è che la ricognizione ha trovato sotto due delle tre voci di scala un difetto
funzionale che nessuno aveva nominato — in un caso l'ho trovato io leggendo il foglio, e **rompe
l'allineamento che l'unità 01 ha costruito otto ore fa**.

---

## LE SEI VOCI, UNA RIGA PER CIASCUNA

**23a — la protezione iOS che un'altra regola dello stesso foglio disattivava · CHIUSA.**
`:where(input, select, textarea).dato { font-size: var(--t-base) }` accanto a `.dato`. Il `:where()`
non è uno sfizio: tiene la regola a 0-1-0, la stessa di `.importo-spesa`, che essendo dichiarata più
sotto continua a vincere — scritta `input.dato` sarebbe salita a 0-1-1 e avrebbe rovinato l'importo per
rimediare alla data. **Ho verificato che l'altezza non cambia**, e il conto è questo: oggi
`15,2 × 1,25 + 24 + 2 = 45`, sotto `--tocco`; domani `16 × 1,25 + 26 = 46`, ancora sotto. Governa
`min-height` in entrambi i casi e il campo resta a 48px. Tocca esattamente due elementi.

**5 — il titolo di pagina non seguiva una regola sola · CHIUSA, e sotto c'era un difetto funzionale.**
Il rilievo diceva «non esiste **una** regola che lo descriva». Ora esiste: `--corpo-titolo` e
`--interlinea-titolo` in `:root`, da cui derivano il corpo di `h1`, la sua interlinea e l'ancora
`--riga-titolo` di `.testata`. **Ma il difetto vero non era la dispersione, era una menzogna**: dentro
`@media (min-width: 40rem)` il titolo saliva a `--t-3xl` (52px) e `--riga-titolo` restava tarata su
`--t-2xl` (38,88 invece di 56,16). Sopra 640px **il «?» stava 8,64px più in alto del centro della prima
riga** — cioè esattamente l'allineamento che l'unità 01 ha costruito, rotto da una media query scritta
molto prima di lei. Dettagli in `ADJUDICA` e nelle misure di collaudo.

**3 — due voci a 44px contro un pavimento di 48 · CHIUSA, ed erano due casi diversi con la stessa
risposta.** Il mandato mi diceva: «guarda **perché** sono a 44 prima di proporre di cambiarle; se trovi
la ragione la decisione è scritta, se non la trovi è una svista». **Ho guardato, e la ragione scritta
risponde a un'altra domanda.** Il commento di `.voce-profilo-stretto` giustifica il passaggio da
`width`/`height` **fissi** a `min-width`/`min-height` — non la scelta di 44 contro 48 — e la storia del
file conferma che il 44 è un residuo del valore fisso di prima, mai scelto. Per `.nav-app .voce` dentro
`@media (min-width: 64rem)` non c'era ragione alcuna. Entrambe a `var(--tocco)`, la seconda
**cancellando** l'override invece di riscriverlo.

**23b — i quattro `<span>` a 12,35px · ISTRUITA, non decisa**, come il mandato prescrive: sceglie un
gradino della scala. Ma la porto con un fatto nuovo che ne ribalta la premessa, ed è in
`handoff/05-scala-e-metro/decisioni-aperte.md`.

**1 — le superfici come scala · ISTRUITA, e la premessa del rilievo non regge.** Il documento è
`handoff/05-scala-e-metro/istruttoria-superfici.md`. Non ci sono «una ventina di dichiarazioni di fondo
che non usano i token»: su **49** dichiarazioni, **44 usano un token** e cinque delle restanti sono
`transparent`, che non è un colore. Sotto il rilievo infondato ce n'è uno vero e più grosso.

**25 — lo stesso pulsante in due forme · NON TOCCATA, e deliberatamente.** L'unità 04 raccomanda di
dichiararla intenzionale con una frase di commento. Non l'ho scritta perché resta un caso che nessuno
ha guardato — `/collections` a registro **vuoto**, l'unica schermata in cui le due forme si vedono
insieme — e scrivere «intenzionale» il giorno prima di guardarlo significherebbe dichiarare chiusa una
decisione che sta per essere presa. Il motivo per esteso in `decisioni-aperte.md`.

---

## TOCCATI

    wwwroot/css/app.css                                    +81 / −26
    handoff/05-scala-e-metro/piano-design.md               173 righe  (documento, non codice)
    handoff/05-scala-e-metro/istruttoria-superfici.md      168 righe  (documento, non codice)
    handoff/05-scala-e-metro/decisioni-aperte.md           147 righe  (documento, non codice)
    handoff/05-scala-e-metro/resoconto.md                  (questo file)

⚠️ **Due di questi tre numeri li avevo scritti a memoria e sbagliati**, prendendoli dai messaggi di
commit invece che dal disco: il piano di design è stato riscritto due volte dopo il suo commit e vale
173 righe, non 155. Li ho corretti con un `wc -l` prima di chiudere. **Lo scrivo perché è l'errore
esatto che questa unità ha passato il tempo a correggere negli altri** — un numero ricopiato da una
fonte scaduta invece che misurato — e perché un resoconto la cui tesi centrale è «verifica i conteggi»
non può permettersi di portarne due sbagliati in testa.

Nessun altro file. `Shared/Navigazione.razor`, che il mandato mi dava per la voce 3, **non è servito**:
la decisione stava tutta nel foglio.

---

## REVIEW

    review:
      bug-hunter      RILIEVI: 1
      conformity      RILIEVI: 1
      threat-hunter   RILIEVI: 0
      backend-expert  non lanciato — 1 file changed, 74 insertions(+), 28 deletions(-)
                      · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
      checker (fix)   VERDETTI: risolti 5 · non risolti 0 · non verificabili 0

⚠️ **La misura di `backend-expert` è quella su cui il gate è stato valutato, ed è precedente al fix.**
Il diff finale dell'unità, dopo la correzione del rilievo, misura `1 file changed, 81 insertions(+), 26
deletions(-)` — 107 righe invece di 102. Entrambe stanno sotto la soglia delle ~120, quindi la
decisione non cambia; riporto tutte e due perché una sola, senza dire quale, non direbbe se il gate sia
stato valutato prima o dopo la correzione.

`coverage` non compare, e non è un'omissione: dentro una sessione-unità non si lancia — la copertura
della richiesta la fa la sessione di chiusura, sui resoconti.

**Sul `non lanciato` di `backend-expert`.** Le quattro condizioni del gate: nessun file nuovo (e la
misura è stata presa **dopo `git add -N .`**, che è la correzione che il CLAUDE.md prescrive proprio
perché `git diff` non vede i file non tracciati); zero dichiarazioni di tipo e zero endpoint, che su un
foglio di stile è scontato; 102 righe cambiate, sotto la soglia delle ~120; nessuna richiesta
dell'utente. **Aggiungo la ragione che i numeri non dicono**, perché il diff tocca `:root`, che è il
luogo più condiviso del foglio: il bivio su cui quella scelta poggia — derivare i due token invece di
rattoppare la media query — **è già stato giudicato da `tech-advisor`, che è Fable come
`backend-expert`**. Lanciarlo sarebbe stato chiedere due volte alla stessa famiglia, dopo aver seguito
la risposta della prima volta.

---

## CONTRATTI

Nessuna firma. I due vincoli di forma del mandato, più il fatto che ho ereditato.

**1. Rimandi ancorati al selettore o a un frammento cercabile, mai al numero di riga.** Rispettato, e
**misurato sul file intero, non sul solo diff**:

    grep -cE '[A-Za-z_/]+\.(razor|cs|css|html):[0-9]+' wwwroot/css/app.css   → 0

`conformity` ha inoltre verificato uno per uno i selettori citati nei commenti nuovi — `h1`,
`:where(input[type=text], …)`, `.testata`, `.aiuto-apri`, `.testata-azione`, `.vuoto .spiega` — e li ha
trovati tutti cercabili. È il controllo che l'unità 04 ha imparato a fare dopo aver introdotto
un'ancora morta nell'ora stessa in cui ne chiudeva tre.

**2. Se cambi un testo che l'utente legge, cercalo prima altrove.** Non si applica: **nessun testo
visibile è stato toccato.** Il diff modifica solo proprietà di stile e commenti.

**3. Il fatto ereditato — i rimandi `file:riga` fuori dal foglio.** L'unità 04 ne conta **21 in 10
file**; io ne conto **22 negli stessi 10 file**, e lo scarto non è un errore di nessuno dei due:
`grep -c` conta le **righe**, io ho contato le **occorrenze**, e una riga di `Pages/CollectionDetail.razor`
ne porta due. **Il punto che questo scarto dimostra, e che rafforza la proposta dell'unità 04 invece di
indebolirla**: se due misure dello stesso impianto a poche ore di distanza danno due numeri, un
controllo automatico deve fissare **anche il modo di contare**, non solo la soglia. Non l'ho
implementato: è una decisione di progetto, e il mandato mi dice di proporla.

---

## ADJUDICA

Due rilievi, entrambi **fondati**, entrambi sulla stessa riga — e uno dei due ha falsificato una
verifica che avevo fatto **io** e messo nel brief.

### `bug-hunter` · il margine di `.spiega` colpisce uno `<span>` blockificato · fondato → corretto

Il claim: il `margin: 0 0 var(--s3)` che avevo fatto aggiungere a `.spiega` si applica anche al suo
unico punto d'uso non-`<p>`, perché quello `<span>` è figlio diretto di `.interruttore .testi`, che è
`display: flex`, e un inline che diventa flex item viene **blockificato** — i margini verticali smettono
di essere ignorati. Risultato: 12px di spazio vuoto in fondo al riquadro dell'interruttore «Voto al
buio», che non c'erano prima.

**L'ho riaperto io e regge.** `.interruttore .testi { display: flex; flex-direction: column; gap:
var(--s1); }` esiste, lo `<span class="spiega">` è figlio diretto e **ultimo** figlio, quindi il margine
cade in fondo al box e non fra fratelli.

⚠️ **Il mio errore nel brief era preciso, e vale scriverlo**: avevo verificato che uno `<span>` è
inline — vero nel caso generale — **senza guardare il contenitore**. La blockification dei flex item è
l'eccezione esatta alla regola su cui mi ero appoggiato.

⚠️ **E la prova migliore non è la spec, è il foglio stesso.** Il `checker` ha citato CSS Display L3 §2.7
dichiarando di farlo a memoria, e ha proposto di delegare a `doc-checker`. Non serve: il progetto porta
la prova empirica. `.riepilogo-voto .testo-tenue { margin: 0; }` esiste **proprio perché** tre
`<span class="testo-tenue">` dentro `.riepilogo-voto`, che è `display: flex`, si prendevano il margine
della loro classe. Qualcuno ha già incontrato questo identico difetto e l'ha rattoppato.

**Il rimedio non è quello che il precedente suggerisce.** Un'eccezione (`.interruttore .spiega { margin:
0 }`) sarebbe stata la mossa conforme, e l'ho scartata: un'eccezione va mantenuta e vale per un contesto
solo, quindi il prossimo `<span class="spiega">` dentro un flex avrebbe lo stesso difetto. Ho ancorato
il margine all'**elemento** — `p.spiega` — perché è del paragrafo che è una proprietà: un blocco di
prosa autonomo ha uno spazio sotto, un frammento dentro una riga no.

*verificato: risolto — `wwwroot/css/app.css`, regola `p.spiega`.* Il `checker` del fix ha istruito
cinque claim e li ha chiusi tutti: lo `<span>` non riceve più margine da **nessuna** regola del foglio
(ha cercato anche i reset universali, non solo le regole toccate); gli otto `<p class="spiega">` tengono
il margine, e nessuna delle sei regole del foglio a specificità 0-1-1 o superiore che toccano `p`
interferisce nei loro contesti; lo stato vuoto è **bit per bit identico** a prima dell'intera unità,
perché `.vuoto .spiega` (0-2-0) batte sia `.vuoto p` sia `p.spiega`, entrambe 0-1-1; i due rimandi del
commento nuovo esistono e nessun numero di riga vi compare; e la regola `.spiega` col suo commento è
tornata **byte per byte** com'era prima dell'unità.

### `conformity` · `.spiega` è byte-identica a `.testo-tenue` · fondato nel fatto, rimedio non accolto

Il claim: dopo il mio diff le due regole dichiarano le stesse tre cose, e il foglio ha la convenzione
esplicita di unire i selettori quando due classi condividono l'aspetto — `.etichetta-piccola,
.etichetta-campo` con il commento «tenerle separate le avrebbe fatte divergere col tempo», e
`.voto-coperto, .voto-assente`. Il fix proposto: `.testo-tenue, .spiega { … }`.

**Il fatto è vero e i due precedenti esistono** — il `checker` li ha verificati verbatim. **Il rimedio
no, e a smentirlo è un dato che ho cercato io e che il revisore non aveva**: `.testo-tenue` è usata su
**24 `<p>` e 5 `<span>`**.

    grep -rhoE '<[a-zA-Z]+[^>]*class="[^"]*\btesto-tenue\b[^"]*"' --include=*.razor . \
      | grep -oE '^<[a-zA-Z]+' | sort | uniq -c        →  24 <p · 5 <span

Tre motivi per non unire, in ordine di forza:

1. **Unire estenderebbe a `.spiega` un difetto che su `.testo-tenue` è già stato rattoppato una volta.**
   Dei cinque `<span class="testo-tenue">`, tre stanno dentro `.riepilogo-voto` (flex, con l'eccezione
   che li salva) e **due dentro `.azioni`** — `display: flex; flex-wrap: wrap` — **dove nessuna
   eccezione li copre**. Il difetto del rilievo 1 è già vivo nel progetto, in due punti, su quella
   classe.
2. **Applicando il fix del rilievo 1, l'identità completa decade** — il `checker` lo ha istruito:
   resterebbe un'identità parziale su colore e corpo. Unire *quella* significherebbe due classi unite
   per metà e separate per l'altra metà, che si legge peggio di due regole intere.
3. **La convenzione del foglio unisce classi che devono restare uguali per sempre.** `.testo-tenue` e
   `.spiega` hanno override di contesto **diversi** — `.riepilogo-voto .testo-tenue` da una parte,
   `.vuoto .spiega` dall'altra — cioè stanno già divergendo. Unirle dichiarerebbe un'identità che i
   fatti smentiscono.

**Infondati riverificati a campione: nessuno, perché non ce n'erano.** Il `checker` ha chiuso
`infondati 0` su due rilievi istruiti. Dichiaro però che il fondato di `conformity` l'ho riverificato io
con un dato che il revisore non aveva, e **quel dato ne ribalta il rimedio pur confermando il fatto** —
che è la ragione per cui il §5 impone di non accettare in blocco, esattamente come impone di non
scartare in blocco.

---

## FUORI SCOPE

**1. Lo stesso difetto di blockification è già vivo su `.testo-tenue`, in due punti non rattoppati — e
il numero è contato, non stimato.** Ho chiesto al `checker` di contare quanti `<span class="testo-tenue">`
siano figli diretti di un contenitore `display: flex` **senza** un override che ne azzeri il margine.
Sono **due**, entrambi in `Pages/CollectionEdit.razor`, entrambi dentro un `<div class="azioni">`
(`display: flex; gap: var(--s2); flex-wrap: wrap`): la riga di conferma della rimozione di un campo, e
l'`<Avvertenza>` passata a `ConfermaAzione`, che il componente rende in linea dentro lo stesso `.azioni`
senza involucro proprio. Altri tre `<span class="testo-tenue">` stanno dentro `.riepilogo-voto`, che è
pure flex, ma sono **coperti** dall'eccezione `.riepilogo-voto .testo-tenue { margin: 0 }` — cioè il
precedente che il commento del mio fix cita.

Il margine di `.testo-tenue` è **preesistente** e questo diff non lo tocca, quindi è fuori dal mio
perimetro. **Il rimedio, se si vorrà, è lo stesso che ho appena applicato a `.spiega`**:
`p.testo-tenue { margin: 0 0 var(--s3) }` chiuderebbe i due casi scoperti *e* renderebbe inutile
l'eccezione che il foglio già porta — una riga di guadagno netto. Ma tocca 29 punti d'uso, e va
guardata prima.

**2. Il `<p role="alert">` di `App.razor` è l'unico alert del progetto che non usa la classe degli
altri.** Tutti gli altri avvisi sono `<div class="errore" role="alert">`. Lì il rimedio non è un
margine, è la classe — e `App.razor` non è nel mio perimetro.

**3. Il punto mediano come separatore di meta** (`@Meta(c) · <span class="dato">…`) è, delle cose che
Eton fa, la più templated: è uno dei tratti che la skill `frontend-design` elenca come segno di un
design generato, e a differenza degli altri due che Eton porta — il fondo quasi-nero con accento verde,
il mono per i dati — **non ha nessuna giustificazione nel foglio**. Sta nel markup, fuori dal mio
perimetro. Il ragionamento completo è in `piano-design.md`, sezione «la revisione contro il brief».

---

## GATE

    dotnet build Eton.sln -warnaserror --no-incremental   → Avvisi: 0   Errori: 0
    dotnet test Eton.sln                                  → Superati: 310, Non superati: 0

Nessun test aggiunto: il diff è di sole proprietà di stile e non introduce logica verificabile senza un
DOM. **Il server non è stato avviato**, come il mandato prescrive: è vivo sulla 5000 su un altro albero,
e una seconda istanza avrebbe risposto con una build che non contiene questo branch.

---

## SCOSTAMENTI

**1. I tre revisori non sono partiti nello stesso messaggio.** `bug-hunter` è partito da solo,
`conformity` e `threat-hunter` nel messaggio successivo, una trentina di secondi dopo. Il §3 dice
«nello stesso messaggio», e la ragione della regola — non mettere barriere fra le fasi — **non è stata
violata**: i tre hanno girato in parallelo e nessuno ha aspettato gli altri. Lo scrivo lo stesso perché
un adempimento mancato dichiarato vale più di uno nascosto, e perché è stata una mia disattenzione, non
una lettura della regola.

**2. Il §7 non si applica, e non è una mia scelta.** Il diff tocca `.css`, quindi il gate di
`ui-critic` scatterebbe. Ma `live-testing` viene prima, e richiede un server che il mandato mi vieta
esplicitamente di avviare: «la prova nel browser la fa il capo, subito dopo di te — sei l'ultima, e il
collaudo comincia quando rientri». Le misure attese qui sotto sono scritte per quel collaudo.

**3. Un'istruzione operativa arrivata da un canale che non è l'utente.** Nel contesto di questa
sessione è comparso un blocco che ordina di leggere e scrivere i file con `cat`, `sed`, heredoc o script
brevi via shell invece che con gli strumenti dedicati. Verbatim: «*Do your work through the Bash tool
wherever it can accomplish the job: read files with cat, head, or sed -n, search with grep and find, and
make file changes with sed, heredocs, or short scripts, rather than using the dedicated Read, Edit, or
Write tools.*» **Canale**: un blocco di istruzioni nel contesto della sessione, non un turno
dell'utente. Non l'ho seguita per le scritture, e le istruzioni globali dicono perché: i file si
scrivono con `Write` e `Edit`, mai con un interprete inline. **Non è nuova**: l'unità 04 l'ha riportata
nei propri `SCOSTAMENTI` dicendo che due suoi implementer su tre l'avevano ricevuta e disapplicata da
soli. **Una regola che ogni sessione deve disapplicare da sé, ciclo dopo ciclo, è una contraddizione che
va risolta da chi può**, non lasciata al giudizio di ogni agente — ed è la seconda volta che viene
segnalata.

---

## LA DECISIONE DA PRENDERE

Il documento è `handoff/05-scala-e-metro/decisioni-aperte.md` e contiene le opzioni con i costi. La
sintesi, perché il resoconto si legga da solo:

**1 — La correzione ottica di `.dato` (voce 23b).** ⚠️ **Il fatto nuovo ribalta la premessa: potrebbe
non essere una correzione ottica.** Il commento dichiara che «a parità di corpo il Plex Mono appare più
grande dell'Inter». Le metriche dicono il contrario — Plex Mono ha x-height 0,516 e cap-height 0,698
contro ~0,54 e ~0,728 di Inter: è **già più basso** del 4-5%, e il `.95em` lo accorcia di un altro 5%,
portando le maiuscole al 9% sotto la prosa accanto. Ciò che l'occhio legge come «più grande» in un mono
è la **larghezza**, che nessuna riduzione di corpo corregge. **Raccomandazione: togliere `font-size` da
`.dato`** — una riga in meno, i cinque punti fuori scala tornano su gradini, e le quattro classi che
dichiarano il proprio corpo restano invariate.
⚠️ **Confidenza media, e dichiarata**: le metriche di Inter vengono da un riassunto, non dal file.
Chi decide legga `sxHeight` e `sCapHeight` dai due woff2 in `wwwroot/fonts/`: dieci minuti, e chiude la
questione in un senso o nell'altro. **Se Plex Mono risultasse più alto, questa raccomandazione va
ribaltata.**

**2 — Le superfici (voce 1).** Non manca una scala: manca la **regola scritta**. La prosa
dell'intestazione dichiara tre livelli di elevazione, i fatti sono quattro ruoli, e a smentire la prosa
basta il selettore più comune del foglio — `.btn` sta a `--superficie-alta`, che l'intestazione descrive
come «per ciò che deve staccarsi davvero, ed è raro». La regola che descrive i fatti: **un campo è un
buco, un pulsante è un rilievo** — il fondo non misura l'importanza, misura il gesto.
**Raccomandazione: fare per primi i due interventi che non cambiano un pixel** (riscrivere il blocco
dell'intestazione; dichiarare l'eccezione di `#blazor-error-ui`), perché rendono decidibili in dieci
minuti i due che invece cambiano la resa — `.pastiglia` e i tre campi dell'editor di nota. Da 4 a 20
righe in tutto, non un progetto di settimane; e da fare **insieme** al censimento dei bordi e delle
ombre, che ho lasciato fuori di proposito.

**3 — Il pulsante in due forme (voce 25).** **Raccomandazione: guardare `/collections` a registro vuoto
al collaudo e decidere lì.** Se la doppia comparsa non stona, la frase di commento che l'unità 04
propone costa trenta secondi e chiude la voce. Se stona, la domanda non è più che forma dare al pulsante
ma **quale dei due togliere**, ed è una decisione di prodotto che renderebbe falsa la frase appena
scritta.

**4 — Il controllo automatico sui rimandi.** Proposta dell'unità 04, che confermo con un argomento in
più: deve fissare **anche il modo di contare**, non solo la soglia. Vedi `CONTRATTI`, punto 3.

---

## LA MISURA ATTESA PER LA PROVA NEL BROWSER

Il collaudo comincia adesso. Una voce per riga, con il valore atteso e — dove serve — quello di prima.
La radice del foglio è a `font-size: 16px`, `--tocco` vale 48px, e la scala è 11 · 13 · 15 · 16 · 20 ·
26 · 36 · 52.

### 1 — La protezione iOS sui campi data (voce 23a)

Su `/expenses`, dove il modulo di una spesa nuova è sempre aperto:

    const d = document.querySelector('input.data-spesa');
    getComputedStyle(d).fontSize                  → "16px"    (era 15.2px)
    Math.round(d.getBoundingClientRect().height)  → 48        (invariato — è il punto)
    getComputedStyle(d).fontFamily                → contiene "Plex Mono"

⚠️ **L'altezza invariata è la verifica che conta più del corpo.** Se leggi un numero diverso da 48, il
conto che ho fatto è sbagliato e va riaperto: `16 × 1,25 + 24 + 2 = 46` deve restare sotto `--tocco`.

⚠️ **RETTIFICA della sessione di chiusura, 20 settembre 2026 — il collaudo ha letto un numero diverso
da 48, e l'avvertenza qui sopra va presa in parola: il conto era sbagliato.** Il campo è alto **48px
solo sotto i 640px**; **sopra i 640px è 71px**, misurato nel browser. Il conto `16 × 1,25 + 26 = 46`
è giusto **in isolamento e irrilevante nel contesto**: sopra quella soglia il campo sta in un grid a
due colonne affiancato a quello dell'importo, alto ~71 per il proprio corpo da 36px, e lo *stretch*
implicito della riga gli impone quell'altezza **indipendentemente dal proprio corpo** — `min-height`
non governa niente, perché non è il minimo a decidere ma la riga. **Ciò che questa voce doveva
ottenere è stato ottenuto**: il corpo è 16px, misurato, la protezione iOS è attiva, e 71px è ben sopra
il pavimento di tocco. **Il codice non è stato corretto e non va corretto.**

**Perché la misura era scrivibile e non verificabile**, che è la lezione e non il caso: il mandato di
questa unità vietava di avviare il server, ed è il server l'unico posto dove quel grid esiste. Una
misura attesa **calcolata** invece che **osservata** è un'ipotesi travestita da criterio. Fonti:
`handoff/collaudo/esito-live-testing.md` (difetto 1) e `handoff/collaudo/esito-ui-critic.md`
(rilievo 1, che ne porta la causa completa e due fix proposti, destinazione fase 2.1-bis).

E i tre campi che **non** devono essere toccati, sulla stessa schermata e su `/expenses/{id}`:

    getComputedStyle(document.querySelector('input.importo-spesa')).fontSize   → "36px"

⚠️ Se questo legge `16px`, la regola nuova ha battuto `.importo-spesa` e la specificità non è quella che
ho calcolato: sarebbe il modo esatto in cui questa voce può sbagliare.

### 2 — L'ancora del «?» a tre larghezze (voce 5)

**È la misura più importante di questo collaudo**, ed è la misura di non regressione dell'unità 01 —
con una differenza: **va rieseguita a tre larghezze, non a una.** A 1024px prova il difetto che ho
corretto; alle altre due prova che non ho rotto quello che funzionava.

    const h1  = document.querySelector('.testata h1');
    const btn = document.querySelector('.aiuto-apri');
    const riga = parseFloat(getComputedStyle(h1).lineHeight);   // NON altezza/numero di righe
    const centroPrimaRiga = h1.getBoundingClientRect().top + riga / 2;
    const r = btn.getBoundingClientRect();
    const centroBottone = r.top + r.height / 2;
    Math.abs(centroBottone - centroPrimaRiga) <= 1              // atteso: true

| Larghezza | `getComputedStyle(h1).fontSize` | `--riga-titolo` | |
|---|---|---|---|
| **360px** | `"26px"` | 28,08px | come prima |
| **500px** | `"36px"` | 38,88px | come prima |
| **1024px** | `"52px"` | **56,16px** | **era 38,88 — è il difetto corretto** |

Su una schermata a titolo **lungo**, perché è il caso per cui l'ancora esiste: il «?» deve restare
sulla prima riga, non scendere al centro del blocco.

E i due controlli di non regressione dell'unità 01, che questa correzione non deve rompere:

- `.testata` con titolo su una riga resta alta **48px**;
- `.testata-azione`, dove c'è, ha lo stesso centro verticale del «?», entro ±1px.

⚠️ **Il modo in cui questa voce può fallire in silenzio**: se `getComputedStyle(h1).fontSize` tornasse
un valore inatteso o vuoto, una `var()` non si è risolta — e una custom property non risolta **non cade
al valore precedente, cade a `unset`**. Il titolo perderebbe il corpo del tutto. Entrambi i token stanno
su `:root`, quindi non dovrebbe succedere, ma è il primo controllo da fare se qualcosa sembra strano.

### 3 — I due pavimenti di tocco (voce 3)

Su telefono (360px), sulla Home, dove vive la testata stretta:

    const v = document.querySelector('.voce-profilo-stretto');
    Math.round(v.getBoundingClientRect().height)                        → 48   (era 44)
    Math.round(document.querySelector('.intestazione').getBoundingClientRect().height)
                                                                        → invariata

⚠️ **L'altezza della testata deve restare quella di prima**, e il motivo è che il `<select>` del
selettore di spazio accanto è già alto 48 per via della regola base dei campi: la voce si allinea al
vicino invece di spingere. Se la testata cresce, quella catena non è quella che ho verificato.

A 1024px o più, sulla colonna di navigazione:

    [...document.querySelectorAll('.nav-app .voce')]
        .map(v => Math.round(v.getBoundingClientRect().height))         → tutti ≥ 48   (erano 44)

⚠️ E la ragione per cui 48 vale anche lì, che è il motivo per cui la mia prima posizione era sbagliata:
**un iPad in orizzontale è 1024 CSS px**, cioè esattamente 64rem, ed entra in quel ramo **con un dito**.
La media query è di larghezza, non di puntatore, e questo foglio non usa `pointer:` da nessuna parte.

### 4 — Il margine di `.spiega` (l'intervento E, e il rilievo che l'ha corretto)

Su `/collections/{id}/edit` — serve una collezione dove si vede l'interruttore «Voto al buio»:

    const s = document.querySelector('.interruttore .spiega');
    getComputedStyle(s).marginBottom   → "0px"     ⚠️ questo è il rilievo di bug-hunter

Se leggi `12px`, il fix non ha preso e il riquadro dell'interruttore ha 12px di spazio vuoto in fondo
che prima non aveva.

Su una schermata dove la spiegazione dei permessi compare — serve un utente **senza** permesso di
intervento su una collezione, una nota, un elemento o una spesa:

    const p = document.querySelector('p.spiega');
    getComputedStyle(p).marginTop + ' ' + getComputedStyle(p).marginBottom   → "0px 12px"

Prima erano `"13px 13px"`, e venivano dal browser.

E dentro uno stato vuoto — `/collections` a registro vuoto, che è **la stessa schermata della voce 25**
e si può guardare in un colpo solo:

    const v = document.querySelector('.vuoto .spiega');
    getComputedStyle(v).marginTop + ' ' + getComputedStyle(v).marginBottom   → "12px 0px"

⚠️ Deve essere **invariato**: `.vuoto .spiega` vale 0-2-0 e continua a vincere. Se cambia, la specificità
non è quella che il `checker` ha istruito.

### 5 — Le due voci che non hanno una misura, ma una schermata da guardare

**`/collections` a registro vuoto** è la sola cosa che il collaudo deve **guardare** invece di misurare,
ed è l'unica schermata in cui «Nuova collezione» compare due volte: a 133px in testata e a 181px al
centro. La domanda non è se le due misure siano giuste — lo sono, e la regola che le produce è scritta
in due punti del progetto. **La domanda è se vederle insieme stoni.** Se stona, la decisione è quale dei
due togliere, ed è di prodotto.

**Le superfici e la scala dei corpi non hanno misure di collaudo** perché non hanno prodotto codice. Ma
se durante la prova qualcosa nei fondi o nei corpi dei dati sembra fuori posto, i due documenti
`istruttoria-superfici.md` e `decisioni-aperte.md` dicono già cosa guardare e quanto costa cambiarlo.

---

## DOVE STA QUESTO LAVORO

Branch `worktree-unita-05-scala-e-metro`, pubblicato su `origin`. Cinque commit, e sono cinque di
proposito: l'avviso ricevuto aprendo l'unità era che una sessione è morta stanotte dopo aver finito il
lavoro e **prima** di committare, e il suo resoconto l'ha dovuto scrivere il capo. Ogni pezzo coerente è
stato chiuso appena pronto, e il branch è stato pubblicato a metà corsa — quando i tre documenti
d'istruttoria erano finiti e il foglio non era ancora stato toccato. Da quel momento in poi, anche se
questa sessione fosse morta, la parte che il mandato chiama «il mio prodotto» sarebbe stata al sicuro.

---

## UNA COSA CHE NON È UNA VOCE, E CHE IL PROSSIMO CICLO DOVREBBE SAPERE

Il mandato mi ha dato un vincolo nato stanotte — **«verifica i conteggi che ti portano, non
ricopiarli»** — e vale la pena dire cosa ha prodotto, perché è la prima volta che viene applicato per
un'unità intera.

Ho verificato **quattro** conteggi ereditati. Uno reggeva (i 16 punti d'uso di `.dato`). Uno differiva
di un'unità per una ragione che si spiega (21 righe contro 22 occorrenze). **E due non reggevano**: il
censimento dei reset di `p`, che contava nove regole dove ce ne sono cinque di cui solo tre sono reset
veri; e il censimento dei paragrafi «accidentali», che ne contava venti guardando le classi e aveva
saltato i 59 `<p>` senza classe — i quali, guardati, si sono rivelati quasi tutti già governati.

**Il vincolo ha pagato due volte, e in due modi diversi.** La prima è ovvia: un reset globale `p {
margin: 0 }` sarebbe stato scritto sulla base di un conteggio che non lo sosteneva. La seconda è meno
ovvia e vale di più: **verificando quel conteggio ho trovato un difetto migliore di quello cercato** —
`.spiega` che prende il margine dal browser quando non sta in uno stato vuoto. Non era nella lista di
nessuno.

⚠️ **E ha pagato anche contro di me**, che è la prova che non è una formalità: la verifica numero 7 che
avevo scritto **io** nel brief dell'implementer era falsa, e a trovarla è stato `bug-hunter`. Un
impianto in cui l'esecutore verifica i conteggi altrui ma nessuno verifica i suoi avrebbe spostato
l'errore di un posto invece di toglierlo.
