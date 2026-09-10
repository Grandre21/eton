UNITÀ: 09 — ESITO: FATTO

TOCCATI:
- `Pages/Notes.razor` → +9/−3 (commento sostituito, una riga di markup aggiunta)
- `Pages/Collections.razor` → +9/−3 (idem, speculare)
- Totale: 18 aggiunte, 6 rimozioni, di cui **due sole righe di codice** — i due `<a>`. Il resto è
  commento. Nessun blocco `@code` toccato, nessun file nuovo, nessuna astrazione.
- **Non committato**, come da mandato: i due file sono nel working tree.

CONTRATTI:

La forma dello stato vuoto della Home, riaperta da me riga per riga:

- `Pages/Home.razor:154-157` — il messaggio: `<p class="testo-tenue">Ancora nessuna nota in questo
  spazio.</p>`. Una riga sola, senza icona e senza spiegazione.
- `Pages/Home.razor:173-175` — il pulsante: `<div class="azioni"><a class="btn primario"
  href="notes/new">Nuova nota</a></div>`, **fuori** dal ramo condizionale, quindi presente sia a
  elenco vuoto sia a elenco pieno.
- `Pages/Home.razor:213-215` — lo stesso, speculare, per le collezioni.
- `Pages/Home.razor:35-41` — la Home **non ha `TestataPagina`** e quindi non ha nessun pulsante in
  alto a destra: la sua sola testata è `<header class="intestazione solo-stretto">` con selettore
  spazio e profilo. È il fatto che decide il punto 3 del mandato, ed è scritto in SCOSTAMENTI.

Ciò che ho copiato è il **principio** — l'azione sta dove sta l'occhio — con la struttura del
vicino più stretto invece di quella della Home. Il motivo è in SCOSTAMENTI, punto 2.

La forma effettivamente adottata, identica nei due file:

```razor
<div class="vuoto">
    <span class="icona-vuoto"><Icona Nome="note" /></span>
    <p>Ancora nessuna nota qui.</p>
    <p class="spiega">…</p>
    <a class="btn primario" href="notes/new">Nuova nota</a>
</div>
```

Fatti verificati da me sul disco, tutti riaperti adesso e non riferiti:

- `wwwroot/css/app.css:592` — `.vuoto .btn { margin-top: var(--s4); }`. **Il caso era già previsto
  dal foglio di stile**: nessuna riga di CSS serve, e l'unità 11 non riceve nulla da questa unità.
- `wwwroot/css/app.css:639` — `.btn { display: inline-flex; }`, quindi dentro `.vuoto`
  (`app.css:581`, `text-align: center`) il pulsante si centra da sé.
- `wwwroot/css/app.css:643` — `min-height: var(--tocco)`, cioè i 48px pieni: il pulsante nuovo non
  riapre il rilievo 5 sui bersagli piccoli.
- `Pages/NoteEdit.razor:1` (`@page "/notes/new"`) e `Pages/CollectionEdit.razor:1`
  (`@page "/collections/new"`) — le due rotte esistono.
- `wwwroot/index.html:24` — `<base href="/" />`: gli href relativi risolvono alla radice
  indipendentemente dal path corrente, come già fa il pulsante della testata.

ADJUDICA:

**istruttoria: 0 rilievi su 0 file → checker no**

`bug-hunter` e `conformity` sono rientrati entrambi con `RILIEVI: 0`. Nessun rilievo da adjudicare,
e **nessun campione da riverificare**, perché non ci sono infondati.

Le tre verifiche che ho fatto io, come impone il mandato quando i revisori tornano a zero:

1. **La domanda più rischiosa del diff era la resa, non la logica**, perché il collaudo nel browser
   mi è vietato e nessuno dei due revisori guarda come un elemento si disegna. Un `.btn` che fosse
   stato block-level si sarebbe steso per tutta la larghezza dello stato vuoto, largo quanto la
   pagina. **Non è così**: `app.css:639` lo dichiara `inline-flex`, quindi il contenitore centrato
   lo centra. Verificato aprendo il foglio di stile, non dedotto dal fatto che gli altri call-site
   funzionano.
2. **Il claim che ho fatto scrivere nel commento** — «`Spazi.Attivo` è non nullo per costruzione» —
   è vero, e regge per una ragione più forte di quella che avevo in mente. Ho aperto
   `Shared/PaginaRegistro.cs:153-176`: `SuCambioSpazio` forza `caricato = false` prima di
   ricaricare, il che farebbe ricadere sul ramo `Scheletro`. Ma quella è una garanzia di
   temporizzazione, e non serve: la catena `@if` di `Notes.razor:25-40` intercetta
   `Spazi.Attivo is null` **nello stesso render** in cui valuta `note.Count == 0`. Il ramo non può
   disegnarsi senza spazio attivo qualunque cosa faccia il cambio di spazio. Il commento nel codice
   dice la ragione giusta.
3. **Il pulsante nuovo non ha la guardia `@if (Spazi.Attivo is not null)`** che protegge quello
   della testata (`Notes.razor:14-17`). È corretto per il punto 2: là la guardia serve perché la
   testata sta **fuori** dalla catena e si disegna anche senza spazio; qui sarebbe codice morto.

FUORI SCOPE:

1. **`Pages/CollectionDetail.razor:54-58` ha lo stesso identico stato vuoto** — icona, `<p>`,
   `<p class="spiega">` — e resta **senza pulsante**: «Ancora nessun elemento in questa collezione.»
   Dopo questa unità è l'ultima schermata con la forma vecchia. **Non appartiene al perimetro di
   nessuna unità della tabella PARTIZIONE**: l'ho cercato e non c'è (l'unità 10 possiede
   `Shared/RecensioniElemento.razor`, non questa pagina). È un file orfano come i quattro rilievi
   che l'unità 08 aveva trovato senza proprietario. Il rimedio è di una riga e appartiene al capo,
   che decida se assegnarlo.
2. **`Pages/Spese.razor:169-171` resta fuori, e ho verificato che è giusto così.** Il suo stato
   vuoto è il solo `<p>Ancora nessuna spesa qui.</p>`, senza icona né `spiega`, e soprattutto la
   pagina **non ha uno slot `<Azione>` in testata** (`Spese.razor:10-15`): la creazione avviene in
   un modulo sempre visibile in cima alla pagina (`:57-105`). Non ha il difetto che il rilievo 13
   descrive, quindi non è un'incoerenza lasciarla com'è. Il merito è di `conformity`, che l'ha
   argomentato invece di limitarsi a segnalare l'asimmetria.
3. **Proposta al capo, non fatta perché il budget la vieta.** La forma `icona + <p> + <p class=
   "spiega"> + pulsante` vive ora in tre punti (`Notes.razor:68-73`, `Collections.razor:67-72`,
   `CollectionDetail.razor:54-58`, quest'ultimo senza pulsante). Tre call-site sarebbero il numero
   giusto per un componente `<StatoVuoto>` in astratto, e `conformity` conferma che oggi in
   `Shared/` non esiste nulla del genere da riusare. **Non l'ho fatto**: il budget lo vieta
   esplicitamente, i tre testi sono tutti diversi, e il parametro `Spiega` sarebbe un `RenderFragment`
   per una sola riga di prosa. Lo scrivo perché il mandato chiede di scriverlo, non perché lo
   consigli: il guadagno sarebbe la coerenza del terzo call-site, che si ottiene anche con una riga.

GATE:

- `dotnet build -warnaserror` → **Avvisi: 0, Errori: 0**, exit 0.
- `dotnet test` → **273 superati**, 0 non superati, 0 ignorati. Esattamente i 273 di partenza: il
  diff non ne ha cambiato nessuno, come previsto — non tocca una riga di C#.
- **Valutazione del gate della review, scritta com'è richiesto anche se l'esito è banale.** Diff di
  24 righe modificate, di cui 2 di codice; solo markup e commenti; nessun `@code`; nessun impatto su
  sicurezza, dati o concorrenza. La tabella del §3 e il mandato chiedevano qui il **solo
  `bug-hunter`**: la soglia per `conformity` (oltre 30 righe, oppure `@code` toccato) **non era
  raggiunta**. L'ho lanciato lo stesso — v. SCOSTAMENTI, punto 3. `threat-hunter` e `backend-expert`
  non lanciati: nessuna superficie di fiducia toccata (un `<a href>` verso una rotta interna già
  raggiungibile dalla stessa pagina), nessuna superficie nuova, nessuna astrazione.
- Server di sviluppo **non avviato** e browser non usato, come imposto dal mandato.

SCOSTAMENTI:

1. **La decisione sul pulsante in alto a destra: RESTA.** È l'unica scelta che questa unità prende,
   e la dichiaro col motivo.

   La domanda del mandato era «guarda cosa fa la Home e allineati». Aprendola, la domanda si scioglie
   in un fatto: **la Home non ha una `TestataPagina`** (`Home.razor:35-41`), quindi non tiene né
   scarta un pulsante in alto a destra — non ha proprio l'alto a destra. Non c'è un «entrambi» da
   copiare, e il confronto letterale non decide nulla.

   Quindi ho deciso sul merito. **Toglierlo era l'opzione rotta**: a registro pieno il blocco
   `.vuoto` non esiste, e con la testata svuotata non resterebbe **nessun** modo di creare una nota
   dalla pagina delle note. **Tenerlo** costa due controlli con la stessa etichetta e la stessa
   destinazione, ma solo nello stato vuoto, e in quello stato è la convenzione: la testata è
   `compatto` e in un angolo, il blocco vuoto è centrato con un'icona che cattura lo sguardo. In più
   `TestataPagina` con slot `Azione` è la forma di tutte le pagine registro del progetto: svuotarla
   su due su tre le renderebbe l'eccezione.

   Il commento preesistente (`Notes.razor:60-62`, `Collections.razor:59-61`) diceva l'opposto —
   «Duplicarlo qui darebbe due pulsanti uguali sullo stesso schermo, e chi guarda si chiederebbe se
   fanno cose diverse». Era una scelta deliberata di chi ha scritto la pagina, e **l'ho contraddetta
   consapevolmente**, sostituendola invece di lasciarla lì a smentire il codice: il timore vale
   quando etichetta o destinazione differiscono, e qui sono identiche apposta. Il nuovo commento
   spiega il perché, così la scelta non viene riaperta fra sei mesi.

   `tech-advisor`: raccomanda «resta» (opzione A) con confidenza alta, senza dissenso da me; aggiunge
   che il rilievo ha ragione contro il commento esistente, che l'etichetta inline **non** va
   differenziata («Scrivi la prima nota» ricreerebbe proprio l'ambiguità temuta), e che va usato
   `btn primario` e non `compatto`. Ha inoltre segnalato per primo il punto 2 del FUORI SCOPE.

2. **Il pulsante sta dentro `.vuoto`, non in un `<div class="azioni">` fuori dal ramo come nella
   Home.** È uno scostamento dalla lettera del mandato («portalo… adattando il testo e non la
   struttura») e lo dichiaro.

   Copiare la struttura della Home alla lettera significava un blocco `.azioni` **fuori** dal ramo
   condizionale, cioè presente **anche a registro pieno**: avrebbe cambiato una schermata che il
   rilievo 13 non tocca, e avrebbe messo due pulsanti identici sullo stesso schermo in **ogni**
   stato — proprio il difetto che il vecchio commento temeva, e che il punto 1 accetta solo perché
   circoscritto allo stato vuoto. La forma adottata ha invece un precedente **nella stessa pagina**:
   `Notes.razor:35-38` e `Collections.razor:33-36` mettono già `<a class="btn primario">` dentro un
   `<div class="vuoto">` nel ramo «Serve uno spazio», e `app.css:592` esiste per quello. Il vicino
   più stretto batte il modello più lontano, e il risultato a schermo — l'azione sotto il messaggio,
   al centro — è quello che il mandato chiedeva.

3. **Ho lanciato `conformity` oltre il minimo richiesto dal gate.** La soglia del mandato non era
   raggiunta (v. GATE). L'ho lanciato perché il compito di questa unità *è* assomigliare a una
   pagina sorella, e un revisore che non apre i vicini non poteva dirmi se ci fossi riuscito. È
   costato poco ed è servito: il punto 2 del FUORI SCOPE viene da lui.

DA PROVARE NEL BROWSER:

**Prerequisito comune, e va detto perché costa un passo in più:** entrambe le prove richiedono uno
spazio **davvero vuoto**. Lo spazio personale esistente ha già dati, quindi chi collauda deve
**creare uno spazio nuovo** (da `/spaces`, «Nuovo spazio»), renderlo attivo dal selettore, e provare
lì. Su uno spazio con anche una sola nota il blocco che stiamo collaudando **non si disegna
affatto**: al suo posto compare l'elenco, e la prova risulterebbe verde senza aver visto niente.

**Prova 1 — `/notes` su spazio vuoto.**
A schermo, dall'alto: la testata «Note» con a destra il pulsante compatto **«Nuova nota»**; poi il
sottotitolo «In <nome dello spazio>»; poi il blocco centrato con l'icona fioca, la frase
**«Ancora nessuna nota qui.»**, la riga esplicativa **«Una nota si scrive in Markdown e resta nello
spazio: appunti, liste di cose da fare, quello che non vuoi dimenticare.»** e, **sotto di essa e
centrato**, un secondo pulsante blu pieno con l'etichetta **«Nuova nota»**.
È verde se: il pulsante inline c'è, è centrato e **non** largo quanto la pagina; porta a
`/notes/new`; la sua etichetta è identica a quella in testata; il pulsante in testata è **ancora
lì**; le due frasi sono quelle scritte sopra, parola per parola.

**Prova 2 — `/collections` sullo stesso spazio vuoto.**
Identica per forma: testata «Collezioni» col pulsante compatto **«Nuova collezione»** a destra; nel
blocco centrato la frase **«Ancora nessuna collezione qui.»**, la riga **«Una collezione è un elenco
che si vota insieme: birre, film, ristoranti. I campi li decidi tu, e ogni membro dello spazio
lascia il suo voto.»** e sotto il pulsante inline **«Nuova collezione»**, che porta a
`/collections/new`.
È verde alle stesse condizioni della prova 1.

**Controprova, una sola, e vale per entrambe:** creata la prima nota, tornando su `/notes` il
pulsante inline **deve sparire** insieme al blocco vuoto, e in testata deve restare l'unico
«Nuova nota». Se restassero due pulsanti a registro pieno, la scelta dichiarata al punto 1 degli
SCOSTAMENTI sarebbe stata implementata male.
