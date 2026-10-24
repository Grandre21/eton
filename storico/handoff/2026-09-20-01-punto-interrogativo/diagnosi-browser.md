# Diagnosi nel browser del pulsante «?» — 19 settembre 2026, notte

Eseguita dal capo **prima** di aprire l'unità 01, sul commit `cdb0999`, server vivo sulla 5000.
Dodici prove. **Il motivo per cui esiste questo documento:** la voce 6 è osservabile solo nel
browser, e un'unità che correggesse sulla base della sola analisi statica rischierebbe di
sbagliare causa e di scoprirlo al collaudo finale — cioè dopo aver speso tutto.

`ESITO: difetti` — confermato, ma con una forma diversa da quella con cui la voce è nata.

## L'esperimento discriminante

**Domanda:** l'attesa dopo il montaggio della pagina cambia il comportamento del primo clic?

**Risposta: sì, ma la soglia non coincide con l'animazione.**

| Attesa dopo il montaggio | Esito del primo clic | Prove |
|---|---|---|
| nessuna attesa esplicita | **non apre** | 6 |
| 0,25 s e 0,30 s | **non apre** | 2 |
| 0,5 s | **apre** | 1 |
| 1,2 s | **apre** | 1 |

La soglia sta **fra 0,3 e 0,5 secondi**. L'animazione d'ingresso di pagina dura **200 ms**.

⚠️ **Conseguenza, ed è la ragione per cui l'esperimento valeva la pena:** se l'animazione fosse
l'unica causa, a 0,25 s il clic avrebbe già dovuto funzionare. **Non è sufficiente a spiegare la
soglia osservata.** L'ipotesi non è esclusa — un `transform` su un antenato resta un fatto — ma da
sola non regge il dato.

## Il sintomo vero non è quello con cui la voce è nata

La voce 6 dice: *«il primo clic mette solo il fuoco sul pulsante»*. **Misurato: falso.**

Quando il clic fallisce, `document.activeElement` resta **`BODY`** — il pulsante **non prende mai
il fuoco**. Il pannello è presente nel DOM, `:popover-open` è falso, le dimensioni sono nulle.
Quando riesce, fuoco e apertura compaiono **sempre insieme**; lo stato intermedio «fuoco sì,
pannello no» non è mai stato osservato.

**Perché la distinzione conta:** «il popover non si apre» e «il clic non raggiunge il pulsante»
sono due famiglie di cause diverse. La prima manda a cercare nel comportamento del popover, la
seconda nell'interattività dell'elemento in quel momento. La voce mandava nella direzione
sbagliata.

## Il difetto non è uniforme fra le rotte

| Rotta | Clic immediato | Prove |
|---|---|---|
| Home | **fallisce**, in modo affidabile | 6 |
| Note | apre | 1 |
| Collezioni (elenco) | apre | 1 |
| Spese | apre | 1 |

⚠️ **E qui c'è un buco che chi legge deve conoscere.** La ricognizione del 19 settembre aveva
riprodotto il difetto **tre volte su due rotte**, e quelle due rotte erano **l'editor di collezione
e l'editor di elemento** — che questa diagnosi **non ha testato**. Quindi «succede solo sulla Home»
**non è dimostrato**: è dimostrato che succede sulla Home e non su tre rotte di elenco.

La lettura che regge **entrambe** le osservazioni è che il difetto dipenda da quanto dura la
finestra in cui il sottoalbero di pagina viene ricostruito, e che sia più larga dove il caricamento
dei dati è più lungo o si ripete.

## Ciò che è stato escluso

**L'identificatore duplicato.** Il pannello porta un `id` statico e globale, e si poteva sospettare
che durante una transizione di rotta ne esistessero due. Misurato con un osservatore di mutazioni
armato **prima** del clic di navigazione: la sequenza è `1 → 1 → 0 → 1`. **Mai due nello stesso
istante.** L'ipotesi è chiusa e non va reinseguita.

## Il fatto nuovo che nessuno aveva censito

**Sulla Home ogni query verso il database parte due volte.** Spese, spazi, membri, profili, note,
collezioni: ognuna compare **due volte quasi in contemporanea** nel registro di rete. Non osservato
con la stessa intensità sulle altre rotte. Trentaquattro richieste in tutto, tutte `200`, nessuna
fallita.

**Il suo rapporto con la voce 6 non è stabilito.** È compatibile con un doppio ciclo di
caricamento e render che allarghi la finestra di irresponsività proprio dove il difetto si
riproduce — ma il nesso causale non è stato isolato, e l'agente l'ha dichiarato invece di
presentarlo come conclusione.

**È comunque un difetto suo**, indipendentemente dalla voce 6: raddoppia le chiamate al database
sulla schermata più visitata dell'applicazione.

## La voce 9, misurata

Con un titolo su sei righe (l'elemento dal nome lunghissimo della collezione di prova):

| Grandezza | Valore |
|---|---|
| blocco del titolo | da y=48 a y=384,9 — alto **336,9 px** |
| centro verticale del «?» | **y=216,45** |
| centro del blocco del titolo | **y=216,45** — coincide esattamente |
| centro della **prima riga** | **y=76,08** |
| **scarto** | **140 px** |

Il pulsante è allineato al centro dell'intero blocco di testo. Dove dovrebbe stare — alla prima
riga — è misurato e vale come criterio di accettazione.

## Dove il pulsante c'è e dove no

**Con il «?»:** Home, Note, Collezioni (elenco), Spese, Spazi, dettaglio di un elemento.
**Senza:** il **dettaglio di una collezione**, che non monta la testata condivisa — verificato
cercando nel DOM un pulsante con l'attributo di apertura del popover: nessuno.

**Non verificate:** nuova nota, nuovo elemento, profilo, dettaglio di una nota.

## Console e rete

Un'unica eccezione, e non è dell'applicazione: il messaggio di un listener che ha dichiarato una
risposta asincrona e ha chiuso il canale prima di riceverla — pattern tipico di un'estensione del
browser, non riprodotto nei clic successivi. Il resto sono messaggi informativi di Blazor.

## Limiti dichiarati

- I clic sono **sintetici**, generati dal protocollo di automazione, non input umano reale. Il
  pattern dipendente dalla soglia è però coerente in tutte e dodici le prove.
- Non è stato isolato quale fattore fra il containing block dell'animazione e il doppio ciclo di
  caricamento sia la causa reale: **dirimerlo richiede il codice, che era fuori dal mandato della
  diagnosi**. È il lavoro dell'unità 01.
