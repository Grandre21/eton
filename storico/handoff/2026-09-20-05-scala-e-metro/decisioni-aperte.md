# LA DECISIONE DA PRENDERE — le tre voci che restano aperte

> Documento di lavoro dell'unità 05. Il mandato chiede che le decisioni che questa unità **non**
> prende restino «prendibili in un'ora invece che da rifare da capo». Qui ci sono le opzioni, il costo
> di ognuna e la raccomandazione. Il resoconto rimanda qui.
>
> Nessuna di queste tre è un difetto: sono scelte di sistema di design, e prenderle è la fase 2.1-bis.

---

## 1 — La correzione ottica di `.dato` (voce 23b)

**Dov'è.** `.dato { font-size: .95em }`. Su sedici punti d'uso produce sei corpi: 12,35 · 13 · 15,2 ·
26 · 36 · 52. Quattro sono intenzionali — li dichiara una classe accanto, e lì `.dato` porta solo il
mono. I due prodotti dal `.95em` sono **12,35px** (quattro `<span class="dato">` figli di un `.meta`
da 13px, negli elenchi di collezioni, note, spese ed elementi) e **15,2px** (`.riga-spesa .importo`).
La scala dichiarata è 11 · 13 · 15 · 16 · 20 · 26 · 36 · 52: nessuno dei due ci sta.

**La metà urgente è già chiusa**: i due campi data che finivano a 15,2px e disattivavano la protezione
iOS sono rientrati a 16px con l'intervento A, che non tocca `.dato` e non presuppone questa decisione.

**Il fatto nuovo, e ribalta la premessa.** Il `.95em` è dichiarato come correzione ottica: «a parità di
corpo il Plex Mono appare più grande dell'Inter». **Le metriche dicono il contrario.** IBM Plex Mono ha
x-height 0,516 e cap-height 0,698 su UPM 1000; Inter ha x-height ~0,535-0,546 e cap-height ~0,728. A
parità di corpo Plex Mono è **già più basso** di Inter del 4-5% su entrambi gli assi, e ridurlo di un
altro 5% porta le sue maiuscole a ~0,66em contro 0,73em della prosa accanto — **il 9% più corte**.

Ciò che l'occhio legge come «più grande» in un mono non è l'altezza: è la **larghezza**, l'avanzamento
fisso a 0,6em che fa occupare a una cifra più spazio di quanto ne occuperebbe in proporzionale. Nessuna
riduzione di corpo la corregge — abbassa le cifre senza stringerle.

⚠️ **Confidenza: media, e il modo di alzarla è noto.** Le metriche di Plex Mono vengono dalla pagina
delle vmet di IBM Plex; quelle di Inter da un riassunto, non dal file `wwwroot/fonts/inter.woff2`. Chi
prende questa decisione può leggere `sxHeight` e `sCapHeight` dai due woff2 e chiudere la questione in
dieci minuti. **Se Plex Mono risultasse più alto di Inter, la correzione avrebbe un fondamento e questa
raccomandazione va ribaltata.**

**E `font-size-adjust` non è il rimedio**, benché sia la forma corretta di una correzione ottica in
generale: la formula è `u = (m / m′) · s`, e con `ex-height 0.54` di Inter applicato a Plex Mono dà
0,54 / 0,516 = **+4,7%**. Ingrandirebbe. L'unica variante che punta alla larghezza è `ch-width`, che
produrrebbe comunque un corpo *reso* fuori scala nascondendolo dal *computed* — peggio, perché una
scala è un vincolo su ciò che si vede.

**Il foglio ha già preso questa decisione altrove, ed è l'argomento più forte.** Il commento della scala
tipografica dice che sotto `--t-base` restano due soli gradini perché i dati «si distinguono già per
famiglia prima che per dimensione». Il `.95em` contraddice la ragione dichiarata della scala.

### Le opzioni

| | Cosa | Costo | Conseguenza |
|---|---|---|---|
| **A** | Togliere `font-size` da `.dato` | **−1 riga** | I quattro `<span>` vanno a 13px e `.riga-spesa .importo` a 16px: entrambi gradini. Le quattro classi che dichiarano il proprio corpo sono invariate. L'intervento A diventa ridondante e si può togliere con lui |
| **B** | Tenerlo e neutralizzarlo dove cumula (`.meta .dato { font-size: inherit }`) | +2 righe | I corpi tornano sulla scala nei punti che contano, ma la classe resta a corpo variabile: il prossimo punto d'uso in un contesto nuovo produrrà un settimo corpo |
| **C** | Non toccare, e dichiarare che la correzione vale il costo | +3 righe di commento | Onesto solo se le metriche smentiscono quanto sopra |

**Raccomandazione: A**, subordinata alla verifica delle metriche sui due woff2. Se il progetto vuole
che il mono pesi meno accanto a Inter, la leva è il **peso** (400 invece di 600) o il **colore**, non
il corpo — e sono due leve che il foglio già usa altrove.

---

## 2 — Le superfici (voce 1)

L'istruttoria completa sta in `handoff/05-scala-e-metro/istruttoria-superfici.md`. Qui il minimo per
decidere.

**La premessa del rilievo è infondata.** Non ci sono «una ventina di dichiarazioni di fondo che non
usano i token»: su 49 dichiarazioni, **44 usano un token**, cinque delle restanti sono `transparent` —
che non è un colore — e le due davvero fuori tavolozza (`#ffe08a` del riquadro di crash di Blazor,
`rgba(0,0,0,.6)` del velo del popover) sono entrambe legittime.

**Sotto c'è un rilievo vero.** L'intestazione dichiara «gerarchia di superficie, tre livelli», cioè una
scala di elevazione. I fatti sono **quattro ruoli**, e a smentire la prosa basta il selettore più comune
del foglio: `.btn` sta a `--superficie-alta`, che l'intestazione descrive come «per ciò che deve
staccarsi davvero, ed è raro».

La regola che descrive i fatti: **un campo è un buco, un pulsante è un rilievo.** Il fondo non misura
l'importanza, misura il gesto — `--sfondo` ciò in cui si entra, `--sfondo-alt` ciò che contiene
controlli, `--superficie` ciò che si legge, `--superficie-alta` ciò che si preme e gli stati.

**Costo per adottarla: da 4 a 20 righe e due decisioni di prodotto**, non un progetto di settimane. I
quattro interventi, nell'ordine consigliato:

1. riscrivere il blocco «gerarchia di superficie» dell'intestazione — ~15 righe di **prosa**, zero di
   CSS, **nessun pixel cambia**;
2. dichiarare l'eccezione di `#blazor-error-ui` accanto al divieto che l'intestazione pone — 2 righe;
3. decidere `.pastiglia`: contorno per forma, oppure premibile come un pulsante — 2 righe;
4. decidere i tre campi dell'editor di nota (`.titolo-nota`, `.titolo-grande`, `.corpo-nota`), che
   stanno a `--superficie` mentre tutti gli altri campi stanno a `--sfondo` — 3 righe, **o** 2 di
   commento se è una scelta.

**Raccomandazione: fare 1 e 2 subito**, perché non cambiano un pixel e rendono 3 e 4 decidibili in
dieci minuti ciascuno. Finché la regola non è scritta, ogni discussione sui campi dell'editor riparte
dal principio.

**E farle insieme ai bordi e alle ombre**, che questa istruttoria ha lasciato fuori di proposito: un
token usato sia come bordo sia come superficie (`--bordo` fa entrambe le cose) è la stessa ambiguità, e
l'intestazione lega il livello 2 a «`--superficie-alta` più ombra tinta» mentre `.btn`, che usa quel
fondo, non porta nessuna ombra. Tre censimenti in tre momenti diversi producono tre regole che non si
parlano.

---

## 3 — Il pulsante in due forme (voce 25)

**Non l'ho toccata, ed è deliberato.** L'unità 04 ha ribaltato la premessa del rapporto — non sono due
punti d'uso ma **sei**, e le due coppie («Nuova collezione», «Nuova nota») sono identiche fra loro,
quindi il «gemello» non aggiunge incoerenza: applica due volte la stessa regola, che è già scritta nel
commento di `.btn.compatto` e in quello di `Pages/Collections.razor`. La forma dipende dalla
**posizione** — compatto in testata, pieno nel flusso — non dall'etichetta.

La sua raccomandazione è **dichiarare la cosa intenzionale**, aggiungendo al commento di `.btn.compatto`
una frase che nomini il terzo luogo (il blocco `.azioni` in fondo a un registro della Home).

**Perché non l'ho scritta.** Resta un caso che nessuno ha ancora guardato, ed è l'unico in cui le due
forme si vedono **insieme**: `/collections` a registro **vuoto**, dove «Nuova collezione» compare due
volte nella stessa schermata, a 133px in testata e a 181px al centro. Scrivere «intenzionale» nel
foglio il giorno prima di guardare quella schermata significherebbe dichiarare chiusa una decisione che
sta per essere presa. **Se al collaudo la doppia comparsa stona, la domanda non è più che forma dare al
pulsante ma quale dei due togliere** — e quella è una decisione di prodotto, che cambierebbe la frase
appena scritta.

**Raccomandazione: guardare `/collections` a registro vuoto al collaudo, e decidere lì.** Se non stona,
la frase di commento costa trenta secondi e chiude la voce; se stona, si è evitato di scrivere una
motivazione per una cosa che cambia.

---

## 4 — Una proposta che non è una voce, ma che scade se nessuno la prende

L'unità 04 ha misurato che restano **rimandi per numero di riga** nei `.razor` e nei `.cs`, fuori dal
foglio di stile, dove la convenzione non è dichiarata. La sua proposta: non basta una riga di prosa in
un `CLAUDE.md` di progetto, perché **una convenzione che nessuno misura scade di nuovo in un ciclo**.

**Ho rimisurato, e il numero è diverso dal suo — in un modo che rafforza la proposta invece di
indebolirla.** L'unità 04 conta **21 in 10 file**; io ne conto **22 negli stessi 10 file**. Non è un
errore di nessuno dei due: `grep -c` conta le **righe**, io ho contato le **occorrenze**, e la riga 72
di `Pages/CollectionDetail.razor` ne porta due sulla stessa riga.

    # righe che contengono almeno un rimando  → 21
    # occorrenze di rimando                   → 22

**Il punto: se due misure fatte a poche ore di distanza dallo stesso impianto danno due numeri, allora
un controllo automatico deve fissare anche il modo di contare**, non solo la soglia. Un controllo che
conta righe e una soglia tarata su occorrenze si sbloccano a vicenda senza che nessuno se ne accorga.

Non l'ho implementato: il mandato dice di proporlo, ed è una decisione di progetto.
