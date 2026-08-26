# Piano — la vetrina che si compone, e un giro di ricognizione sull'app

Scritto a chiusura della sessione del **26 agosto 2026**. Autosufficiente: chi riprende non
ha bisogno di leggere la conversazione da cui nasce.

---

## Dove siamo

`main` è **pulito e allineato al remoto**, fino a `9e284cc`. Build 0 errori / 0 avvisi, 258
test su 258. Il server di sviluppo è **fermo** e la porta 5000 è libera.

Chiuso in questa sessione, già sul remoto:

- **Gli infobutton** — `Shared/TestataPagina.razor`, il «?» accanto al titolo su tutte e
  cinque le schermate. Provato nel browser: apre, chiude con Esc e con un clic fuori, ed è
  disegnato sopra la barra di navigazione. Cinque su cinque.
- **`Shared/PaginaRegistro.cs`** — la macchina di caricamento che viveva in tre copie
  identiche in Note, Collezioni e Spese.
- **Tre componenti condivisi** — `ErroreRiprova`, `SchedaConflitto`, `ConfermaAzione`.
- **Il collasso dei selettori di elenco** in `.registro` / `.riga`.
- Un difetto da perdita di dati: la conferma di eliminazione restava armata passando da
  un'entità all'altra. Corretto con il parametro `Chiave`, e **verificato contro la
  documentazione di .NET 10 e il sorgente del renderer** — vedi «Cosa non va ri-verificato».

---

## Lavoro 1 — La scena del grafo

**Deciso dall'utente**, con la posizione di `tech-advisor` già sul tavolo.

### Cosa si fa

La sezione **«Uno spazio, visto da dentro»** (`Pages/Benvenuto.razor:124-129`, che contiene
già `<GrafoSpazio Persone="@Membri" Elementi="@Votate" />`) diventa una **scena alta 2,5-3
schermate**. Il canvas resta `position: sticky` al suo interno, e mentre l'utente
attraversa la sezione il grafo **si compone**: prima le persone, poi gli archi che le
legano, poi i voti in transito.

L'ingresso è un **progresso di scorrimento smorzato**, non un valore crudo: un listener
`scroll` passivo scrive il bersaglio, e il ciclo `requestAnimationFrame` che già esiste lo
insegue. Il codice ha già l'idioma esatto — `el.caldo += (target - caldo) * .1` a
`wwwroot/js/grafo-spazio.js:217` — quindi è la stessa forma applicata a un ingresso nuovo.

### Perché così, e non pinnato sull'intera pagina

L'idea di partenza era un canvas `position: fixed` per tutta la vetrina, come fa il sito di
riferimento. **Fallisce in tre modi**, due dei quali fatali, e vale la pena che restino
scritti perché sono tutti verificabili nel codice:

1. **Il grafo è fatto di luce.** `grafo-spazio.js:230` disegna con
   `globalCompositeOperation = "lighter"`: i colori si sommano al fondo invece di coprirlo.
   Funziona su nero pieno; su fondo chiaro satura verso il bianco e l'oggetto sparisce. E
   la vetrina ha una `<section class="spazi chiara">` a fondo chiaro **per scelta
   documentata** (`Benvenuto.razor:92-95`). Il sito di riferimento può pinnare per 25.000px
   perché è cromaticamente omogeneo; questa vetrina non lo è.
2. **Il grafo è l'unica cosa toccabile della vetrina** («Passaci sopra il mouse, o
   toccalo»), e ha `pointermove` / `pointerdown`. Un canvas fisso a piena pagina o sta
   sotto il contenuto con `pointer-events: none` — e perde ciò che lo rende interessante —
   o sta sopra, e intercetta i click sul pulsante «Entra con Google». Non c'è una terza
   configurazione.
3. **Il costo per fotogramma.** Oggi il canvas è grande quanto la sua sezione e si spegne
   fuori vista. A piena pagina significa full-viewport a `devicePixelRatio` 2, con una
   `createRadialGradient` per ogni alone a ogni frame, su **CPU** — è Canvas 2D, non il
   WebGL2 del riferimento. Su un telefono di fascia media è il candidato ideale al jank.

Confinare la scena nella sezione toglie tutti e tre: resta nel proprio capitolo scuro,
mentre è pinnato è l'unica cosa a schermo, e il costo è limitato a quel tratto di scorrimento.

### Vincoli tecnici da rispettare

- **Nessuna dipendenza nuova.** `grafo-spazio.js:9-11` lo dichiara per iscritto: «Canvas 2D
  e non WebGL, e nessuna libreria: il sito sta su GitHub Pages e deve funzionare offline
  come PWA». Non è negoziabile ottimizzando gli asset — anche un modello 3D da 50 KB
  richiederebbe comunque la libreria.
- **Non aggiungere altro `animation-timeline`.** Quello che c'è, incapsulato in
  `@supports` con stato di partenza visibile, è il pattern giusto per decorazione non
  critica: si lascia com'è, e non ci si costruisce sopra un doppio binario per Firefox.
  Nota che la scena sticky, essendo guidata da JS, **funziona anche su Firefox** — dove le
  animazioni scroll-driven della vetrina oggi non partono.
- **`overflow: hidden` sul wrapper alto della scena romperebbe `sticky`.** `.hero` ce l'ha;
  la sezione nuova non deve averlo. Verificato che `Layout/VetrinaLayout.razor.css` non ha
  `transform`/`overflow`/`filter` che interferiscano.
- L'`IntersectionObserver` che oggi sospende il ciclo fuori vista **resta valido** e va
  tenuto: fuori dalla sezione la scena si spegne ancora.

### Dove può sbagliare

Due segnali, da `tech-advisor`, che vale la pena riconoscere presto:

- Se il collaudo mostra che ridisegnare il grafo a ogni frame **durante lo scorrimento**
  scatta su mobile, il costo stimato accettabile non lo è, e la scena degrada alla variante
  minima: **composizione a soglie** (stati commutati una volta) invece che agganciata di
  continuo allo scorrimento.
- Se l'utente, rivedendo il riferimento, dice che ciò che voleva è la **materia** dell'oggetto
  3D — la luce sul metallo — e non il pattern di scorrimento, allora la risposta onesta è
  «quel carattere non è replicabile dentro i vincoli di questo progetto», non questa scena.

L'altezza esatta della sezione e la coreografia della composizione **si tarano nel browser**,
non a tavolino: `tech-advisor` dà confidenza media su quel punto e alta su tutto il resto.

---

## Lavoro 2 — Ricognizione UI/UX dell'app

**Deciso dall'utente**: prima si guarda, poi si propone. Nessun miglioramento è stato
deciso, e indovinarli senza aver guardato sarebbe peggio che chiedere.

Un giro di `live-testing` in sola lettura che attraversi l'applicazione cercando **attriti
concreti**: passaggi che richiedono un tocco di troppo, stati che non dicono cosa sta
succedendo, cose che si scoprono solo per tentativi. Il prodotto è un **elenco ordinato per
gravità**, non un intervento.

**Vincolo sui dati**: lo spazio personale dell'utente ha 0 note, 0 collezioni e 1 sola
spesa. Tre verifiche sono già cadute per questo motivo in questa sessione (vedi sotto). Se
la ricognizione ha bisogno di elenchi popolati, **va chiesto all'utente**, non creato:
sono dati veri.

---

## Pendenze minori, tutte già individuate

| Cosa | Dove | Nota |
|---|---|---|
| Il segnaposto dell'editor Markdown mostra i codici `&#10;` invece di andare a capo | `Pages/NoteEdit.razor:77` | Trappola di Blazor, non un refuso: il compilatore Razor emette l'attributo come stringa letterale e il renderer lo passa a `setAttribute`, quindi **nessuno decodifica le entità HTML**. Si risolve dicendo l'a-capo in C#: `placeholder="@("… \n\n …")"`. Unico caso nel progetto. |
| Sei stringhe «Il database ha rifiutato…» | `Pages/SpaceDetail.razor:189, 217, 243, 271` · `Shared/RecensioniElemento.razor:434, 547` | Nominano il meccanismo invece della causa, e cinque su sei non dicono cosa fare. È **RLS che fa il proprio mestiere**, non un guasto: la difesa regge, è l'interfaccia che traduce male una risposta corretta. Omologo già corretto da imitare: `NoteEdit.razor:278`. |
| Il medaglione 📋 delle collezioni | `Pages/Collections.razor:73` · `Pages/Home.razor:193` | `.icona-collezione` da portare a un contenitore 40×40. Ereditato da un ciclo precedente. |
| La spesa di prova «PROVA AGENTE», 12,50 € del 20 agosto | spazio personale dell'utente | **La cancella l'utente.** Nessun agente deve toccarla. |

---

## Cosa NON va ri-verificato

Costa caro rifarlo, ed è già fatto.

- **La misura del sito di riferimento** (`thewatch.60fps.fr`) è in memoria, con tutti i
  numeri. In sintesi: nessuna libreria di animazione, **scroll nativo non addolcito**,
  Three.js + WebGL2 su canvas `fixed`, **11,77 MB** di cui 8,59 per il solo modello, e
  **zero** `animation-timeline` nei loro fogli di stile. Le sue transizioni CSS sono più
  semplici di quelle che Eton ha già: copiarle sarebbe un regresso.
- **La testata a 360px**: misurata nel browser, **+8,5px di margine** sui 328 di area utile
  per il caso peggiore («Collezioni» + «Nuova collezione»). Il commento sopra la regola a
  `wwwroot/css/app.css` porta il numero e spiega perché una misura di `scrollWidth` lì dà
  un falso «tutto a posto».
- **Il disarmo della conferma di eliminazione**: verificato contro la documentazione di
  .NET 10 e il sorgente del renderer. Il framework **non** invoca `OnParametersSet` a ogni
  ri-render — lo salta del tutto quando ogni parametro è di tipo noto immutabile e nessuno
  è cambiato. Qui scatta perché `Chiave` è precisamente il parametro che cambia.

**Rimasto non provato nel browser**, e va detto: la transizione A→B della conferma di
eliminazione (manca un secondo oggetto), gli elenchi di Note e Collezioni (vuoti), la
scheda di conflitto (richiede due salvataggi in corsa).

---

## Fatti operativi che costano cari se dimenticati

- **Riavvia il server prima di ogni prova nel browser**, e non far compilare nessuno mentre
  è vivo. Il DevServer legge i manifest degli asset solo al proprio avvio: dopo qualche
  build annuncia nomi con impronta che non esistono più, e l'app non parte. È successo
  stasera. Il rimedio, se accade: `rm -rf obj bin`, ricompila, riavvia.
- **Il server lo avvia e lo ferma l'orchestratore**, annotando porta e PID. Su Windows la
  morte del padre non uccide i figli: `dotnet run` lascia un processo DevServer separato,
  e vanno fermati entrambi.
- **Gli implementer non compilano.** `obj/` non ha lock fra processi: due build concorrenti
  sullo stesso `.csproj` si corrompono a vicenda. Compila l'orchestratore, una volta, a
  fine giro.
- **Il testo accentato non passa per gli argomenti della shell.** `printf`, `echo -e` e
  `git commit -m` mangiano gli accenti su questo setup: «è» → «e», «più» → «piu». Usa un
  heredoc quotato, o un file UTF-8 con `git commit -F`. Il corpo del commit `33f5c08` ne
  porta la cicatrice.
- **Il Chrome giusto** ha `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`. Identifica per
  deviceId, **mai** per nome visualizzato: i nomi si scambiano a ogni riconnessione, e
  l'altro non raggiunge `localhost`.
- **Il login su `localhost` non arriva all'agente da solo.** `launchBrowser: true` in
  `Properties/launchSettings.json` fa aprire a `dotnet run` il browser **predefinito di
  sistema**, che è un profilo diverso da quello dell'estensione. Apri **tu** la scheda con
  `navigate`, poi chiedi all'utente di accedere **in quella scheda**.
- **`resize_window` non scende sotto ~526px** su questo PC. Per misurare un layout stretto,
  restringi il contenitore via JS **replicando a mano le media query attive a quella
  larghezza** — altrimenti misuri con i valori sbagliati.
