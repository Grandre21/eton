UNITÀ: 09/13 — Lo stato vuoto invita ad agire dove si sta guardando

## OBIETTIVO

**Un rilievo solo, il 13.** È l'unità più piccola del piano, e il mandato è corto perché il
lavoro lo è.

Su `/notes` e `/collections` vuote, il messaggio («Ancora nessuna nota qui», più una
spiegazione) sta al **centro**, e l'unico pulsante per agire è in **alto a destra**. Nella Home,
a parità di stato vuoto, il pulsante è **inline sotto il messaggio**.

Due trattamenti diversi per la stessa situazione, e quello meno comodo è nella pagina dedicata.

**La Home ha ragione, e non si tocca.** Il rimedio è portare il trattamento della Home sulle
altre due, non inventarne un terzo.

## PERIMETRO — file di tua proprietà esclusiva

- `Pages/Notes.razor`
- `Pages/Collections.razor`

**`Shared/ConfermaAzione.razor` era nel perimetro e ne è stato tolto.** Era lì per il rilievo 8
(«Elimina» a 55px da «Chiudi»), che il capo ha riassegnato all'unità 11: `.azioni` è governato
da una sola regola in `wwwroot/css/app.css` che vale per tutti e sei i blocchi delle quattro
pagine editor, quindi si separa lì, in un punto solo. **Non aprire `ConfermaAzione`**: la
ricognizione lo cita fra le cose che funzionano.

## NON TOCCARE

- **`Pages/Home.razor`**: è il **modello da copiare**, e l'ha appena chiusa l'unità 08. La
  **leggi** per vedere come fa lo stato vuoto, non la modifichi. Se ti sembra che la Home vada
  cambiata, è un `BLOCKED`: romperesti il modello prima che venga copiato.
- **`wwwroot/css/app.css`**: unità 11. Usa classi esistenti; se non basta, torna `BLOCKED` e la
  voce si accoda a quelle in attesa. L'unità 08 non ne ha avuto bisogno: guarda cosa ha usato.
- Tutti i `Pages/*Edit.razor` e `Shared/`: chiusi o di altre unità.

## COSA FARE

1. **Leggi come la Home rende lo stato vuoto.** Quale markup, quali classi, dove sta il pulsante
   rispetto al messaggio, che testo ha.
2. **Portalo su `Notes.razor` e `Collections.razor`**, adattando il **testo** al soggetto —
   nota, collezione — e **non la struttura**.
3. **Il pulsante in alto a destra**: decidi tu se resta o sparisce, e **dichiara la scelta con
   il motivo**. Non è ovvia. Tenerlo significa due strade per la stessa azione, il che è
   accettabile se la seconda è quella che si usa a registro pieno; toglierlo significa che a
   registro pieno non c'è più modo di creare. **Guarda cosa fa la Home** e allineati a quella:
   se la Home tiene entrambi, tienili; se no, no.
4. **Gli stati vuoti spiegano il concetto e vanno conservati.** La ricognizione li cita fra le
   cose meglio riuscite: «Una collezione è un elenco che si vota insieme: birre, film,
   ristoranti». **Non riscriverli, non accorciarli, non renderli generici.** Il rilievo riguarda
   *dove sta il pulsante*, non *cosa dice il messaggio*.

## BUDGET DI COMPLESSITÀ

Nessuna astrazione nuova, nessun tipo, nessun componente, nessun servizio, nessun file. Due
file, un rilievo, e il rimedio è spostare un pulsante copiando una pagina che esiste.

Se ti trovi a **estrarre un componente condiviso** per lo stato vuoto: **no**. Tre call-site
sarebbero il numero giusto in astratto, ma il budget di questa unità lo vieta e i tre testi sono
diversi. Se pensi valga la pena, scrivilo in `FUORI SCOPE` come proposta al capo, non farlo.

## STATO

Unità chiuse e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`), 05 (`e139ce8`),
06 (`f4f2dbd`), 07 (`4327598`), 12 (`8a4a89f`), 08 (`bdd858a`). Sei l'unica unità viva.

**Non committare.** Committa il capo, a resoconto letto: è lui che vede il quadro e scrive nel
registro del progetto. Lascia i file modificati nel working tree e dichiaralo nel resoconto,
come hanno fatto le unità 06, 07 e 12. (L'unità 08 ha committato da sé perché nessun mandato lo
diceva: ora lo dice.)

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: se ci trovi una riga che contraddice
questo mandato, vince la più recente. C'è una riga del 3 settembre sera che dice che **l'utente
non è raggiungibile**: qualunque domanda tu abbia, non aspettarla, portala nel resoconto.

**Due fatti operativi.**

- Le `file:line` di `threat-hunter` sono state **sfasate** sulle unità 04 e 05 ed **esatte**
  sulle 07 e 08. Riapri i numeri prima di riportarli, senza trattarlo come inaffidabile per
  principio.
- Se un tuo obiettivo e un tuo divieto si contraddicono, **obbedisci al più specifico e
  dichiaralo**. L'unità 08 l'ha fatto e ha scoperto così una contraddizione nel piano che
  nessuno aveva visto.

**Se i revisori tornano tutti a zero rilievi, non è finita.** Scrivi comunque la riga di
istruttoria, dichiara che non c'è nessun campione da riverificare, e verifica tu almeno la
domanda più rischiosa del tuo diff. Le unità 06, 07, 08 e 12 l'hanno fatto e ogni volta ne è
uscito qualcosa che i revisori non avevano isolato.

**Il gate della review, per un diff come il tuo.** Se il tuo diff è **solo markup e testo**,
sotto le ~30 righe e senza impatto su sicurezza, dati o concorrenza, il §3 chiede **il solo
`bug-hunter`**. Non lanciare quattro revisori su due pulsanti spostati: il gate si valuta, e la
valutazione va **scritta** nel resoconto qualunque sia l'esito. `conformity` però ha senso qui
più che altrove — il tuo lavoro *è* assomigliare a una pagina vicina — quindi lancialo se il
diff supera le 30 righe o se tocca il `@code`.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**.
- `dotnet test` → **273 superati**, com'erano quando parti. Il tuo diff non deve cambiarne
  nessuno.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai.

**Non avviare il server di sviluppo e non provare nel browser.**

BUDGET: 12 dollari

RESOCONTO IN: `handoff/09-registri-vuoti/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 09 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <la forma dello stato vuoto della Home che hai copiato, con file:line riaperti da te>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti, e a chi appartiene il rimedio>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

Aggiungi `DA PROVARE NEL BROWSER` con **due prove**, una per pagina, e in ciascuna **il testo
esatto** del messaggio e del pulsante, perché è quello che verrà cercato a schermo. Le prove
vanno fatte su uno spazio **davvero vuoto**: dillo esplicitamente, perché chi collauda dovrà
crearne uno.

Dichiara in `SCOSTAMENTI` la scelta sul pulsante in alto a destra, con il motivo: è l'unica
decisione che questa unità prende.
