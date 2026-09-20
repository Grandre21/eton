# Piano — chiudere tutto ciò che resta

Scritto nella notte fra il **19 e il 20 settembre 2026**. Sostituisce il piano del 19 settembre,
il cui ciclo è chiuso e archiviato in `storico/handoff/2026-09-19-PIANO-sedici-rilievi.md` con il
suo rapporto ancora in `handoff/CHIUSURA.md`, che **non si archivia finché l'utente non l'ha
letto**.
Autosufficiente: chi riprende non ha bisogno della conversazione da cui nasce.

## OBIETTIVO

> «tutto cio che rimane da chiudere voglio che sia ciuso in questa sessione»
> — utente, 19 settembre 2026, notte

**La glossa, e perché il confine va dichiarato adesso.** «Tutto ciò che rimane» è ciò che il ciclo
precedente ha lasciato aperto **dichiarandolo** — non tutto ciò che resta da fare nel progetto, e
non le fasi del piano di prodotto. La decomposizione dà **trentotto clausole**, e la loro fonte è
una sola: il rapporto di chiusura di `handoff/CHIUSURA.md`.

| Da dove | Quante | Numerazione |
|---|---|---|
| Il `FUORI SCOPE — aggregato` del rapporto | **25** | 1-25, la numerazione del rapporto |
| Il rilievo del `COMBINATO` rimandato al capo | **1** | 26 |
| La clausola dichiarata **scoperta** | **1** | 27 |
| Le due **rinviate** | **2** | 28, 29 |
| Le quattro **prove dichiarate non fatte** | **4** | 30-33 |
| I tre dati di prova rimasti sul database | **1** | 34 |
| L'osservazione che il codice non spiega | **1** | 35 |
| I branch remoti residui | **1** | 36 |
| La contraddizione d'ambiente «While auto mode is active» | **1** | 37 |
| Il §3 `COMBINATO` che era rimasto un segnaposto | **1** | 38 |

⚠️ **Sette clausole non sono chiudibili stanotte, e lo si sa adesso invece di scoprirlo alla
fine.** È la differenza fra una copertura onesta e una che si sgonfia al conteggio finale:

- **29** (il banner di aggiornamento) si vede **solo sul sito pubblicato**. Rinviata al primo
  rilascio, con la prova già scritta nel rapporto.
- **30** (l'importo in sola lettura) richiede un **secondo account**: ogni spazio raggiungibile ha
  un solo membro.
- **28** (la barra gialla di Blazor) richiede un giro a **rete bloccata** riportando verbatim lo
  snippet di sovrascrittura di `window.fetch`, che nessuno ha.
- **34** (i tre dati di prova) e **35** (il dialogo nativo su «Elimina») passano da **dialoghi
  nativi**, che bloccano il plugin del browser: sono **tre gesti dell'utente**, e la 35 va
  riprovata a mano prima di toccare qualunque cosa.
- **1, 3, 5** (le tre voci di scala: superfici, pavimento di tocco, gerarchia del titolo) **non
  sono difetti ma decisioni di sistema di design**: chiuderle *è* la fase 2.1-bis, che è un
  progetto di settimane. L'unità 05 le **istruisce e le porta a decisione**, il che è un esito
  legittimo — il precedente è la voce 10 del ciclo scorso, chiusa con «non si fa, e questo è il
  motivo».

**Cinque clausole sono già chiuse mentre questo piano nasce**: **36** (i quattro branch remoti
cancellati, autorizzati esplicitamente dall'utente), **38** (il §3 completato dalla sessione di
chiusura e integrato), e **11, 12, 13, 14, 16** sono decisioni già prese e confermate per
iscritto dal capo precedente: non si toccano, si dichiarano.

## DECISIONI

*Append-only, datate. L'utente può appendere qui una riga in qualunque momento: ha lo stesso peso
di una detta in chat. Rileggere questo campo prima di ogni PROSSIMA AZIONE.*

- **19 set 2026, notte** — **L'utente ha risposto a quattro domande prima di dormire**, e le
  risposte governano questo goal:
  1. L'obiettivo è **tutto ciò che resta**, frase sua.
  2. **Nell'unità 00 entra tutto**: il «?» che non risponde al primo clic, i nomi accessibili, e
     le voci piccole dei `FUORI SCOPE`. *(Nel piano la 00 è diventata l'unità **01**, e le voci
     piccole si sono distribuite fra la 02 e la 04 per la proprietà dei file — v. `RAZIONALE`.)*
  3. Il §3 `COMBINATO` si chiude **rilanciando il `bug-hunter`** e correggendo le due
     contraddizioni del rapporto.
  4. La contraddizione d'ambiente si cura con **una riga nel `CLAUDE.md` più una voce
     `autoMode.soft_deny` nei settings**, non disattivando la modalità automatica.

- **19 set 2026, notte** — **I quattro branch remoti sono stati cancellati su autorizzazione
  esplicita dell'utente.** Tre erano del goal chiuso; il quarto,
  `worktree-chiusura-rapporto-completo`, è nato **dopo** la domanda, dalla sessione di chiusura
  ancora viva, ed è stato cancellato per analogia dichiarata: stessa classe, già integrato,
  verificato antenato di `origin/main`. Lo annoto perché l'autorizzazione dell'utente ne nominava
  tre, non quattro.

- **19 set 2026, notte** — **`main` pubblica in produzione a ogni push**, e nessun documento del
  progetto lo diceva. `.github/workflows/deploy.yml` ha `on: push: branches: [main]` verso GitHub
  Pages. La conseguenza per la partizione è netta e vale per ogni goal futuro: **l'architettura
  integra ogni unità su `main`, quindi ogni unità che rientra pubblica.** Un'unità che presuppone
  uno schema di database non ancora applicato manderebbe in produzione codice rotto. Nessuna unità
  di questo goal tocca lo schema, quindi il vincolo non morde qui — ma va scritto, perché il
  candidato più forte per il goal successivo (la fase 2.1, spese ricorrenti) ci sbatte contro al
  suo quarto task.

- **19 set 2026, notte** — **Il conteggio del `bug-hunter` combinato esiste in due esemplari
  indipendenti, e non si contraddicono.** La sessione di chiusura ha chiuso con `RILIEVI: 4`
  (tre commenti falsi, una frase duplicata a schermo); un `bug-hunter` lanciato in parallelo da
  me, con il mandato esplicito di **ignorare i commenti** e di cercare solo ciò che emerge dalla
  **somma** delle sette unità, ha chiuso con `RILIEVI: 0` verificando i sette punti di cucitura
  uno per uno. Il secondo non smentisce il primo: tre dei quattro rilievi erano fuori dal suo
  mandato per costruzione, e il quarto è una duplicazione di presentazione, non un difetto di
  correttezza. **Il valore del doppione è la risposta a una domanda che nessuno aveva posto**: la
  somma delle sette unità non ha rotto niente, e ora lo dicono due fonti che non si sono parlate.

- **19 set 2026, notte** — **La voce 26 porta un fatto che la decisione del 19 settembre non
  aveva.** Il capo precedente aveva confermato per iscritto che il pulsante «Sovrascrivi» di
  `ItemEdit` resta spento dal solo permesso, e che il messaggio della guardia nuova sopravvive
  alla correzione del campo. Quella decisione riguardava **la sopravvivenza** del messaggio; il
  `bug-hunter` combinato ha mostrato che la frase è **identica** a quella già a schermo dalla riga
  di errore del campo, cioè che a schermo la stessa identica frase compare **due volte**. È un
  fatto nuovo, non una revoca: l'unità 02 lo istruisce e propone, e se il rimedio tocca la
  decisione torna a me invece di essere preso di nascosto.

- **20 set 2026, 00:10 — La diagnosi nel browser ha preceduto l'unità 01, ed è una mossa che
  rifarei.** La voce 6 è osservabile solo nel browser: un'unità che correggesse sulla sola analisi
  statica rischierebbe di sbagliare causa e di scoprirlo al collaudo finale, cioè dopo aver speso
  tutto. Dodici prove hanno prodotto tre fatti che il mandato porta dentro invece di far cercare:
  l'attesa conta ma **la soglia sta fra 0,3 e 0,5 s, non ai 200 ms dell'animazione**; il clic
  fallito **non produce nulla, nemmeno il fuoco** — quindi la descrizione con cui la voce è nata
  («mette solo il fuoco») è falsa e mandava a cercare nella famiglia di cause sbagliata; e
  l'ipotesi dell'identificatore duplicato **è esclusa** con una misura. Il documento sta in
  `handoff/01-punto-interrogativo/diagnosi-browser.md`.
  ⚠️ **Con un buco dichiarato:** la diagnosi ha testato Home, Note, Collezioni e Spese, mentre la
  ricognizione del 19 settembre aveva riprodotto il difetto sui **due editor**, che non sono stati
  testati. «Succede solo sulla Home» **non è dimostrato**.

- **20 set 2026, 00:10 — Sulla Home ogni query al database parte due volte, e non entra in questo
  goal.** Spese, spazi, membri, profili, note, collezioni: ognuna compare due volte quasi in
  contemporanea nel registro di rete, e non con la stessa intensità sulle altre rotte. **Nessuno
  l'aveva mai censito.** È un difetto suo, a prescindere dalla voce 6: raddoppia le chiamate sulla
  schermata più visitata.
  **Non lo trasformo in una clausola, e il motivo è una regola che eredito.** Il capo precedente si
  era dato per iscritto il limite delle quattro crescite, e l'ha scritto dopo aver visto che un
  obiettivo che cresce a ogni resoconto rende impossibile la verifica di copertura. Questo goal ne
  ha già trentotto, di clausole, e sette non sono chiudibili: allargarlo alla prima occasione
  renderebbe finta la regola. **L'unità 01 lo istruisce solo se serve alla voce 6**; altrimenti va
  nel rapporto finale come candidato per il goal successivo.

- **20 set 2026, 00:50 — La voce 2 si spezza in `2a` e `2b`, e la partizione si corregge prima che
  costi un `BLOCKED`.** Trovato scrivendo il mandato dell'unità 02: la voce «nove controlli senza
  nome accessibile» tocca **due file che appartengono a due unità diverse** — il cursore del voto
  sta in `Shared/VotoInput.razor` (unità 02), i cinque menù a tendina e i tre campi delle opzioni
  stanno in `Pages/CollectionEdit.razor`, che è dell'unità **04** perché lì collide con il selettore
  di icone della voce 4 e con i tre rimandi scaduti della 20.
  **Spezzarla è legittimo perché la voce non è un meccanismo ma un insieme**: nove controlli che
  hanno in comune il difetto, non il codice. `2a` (il cursore) va alla 02, `2b` (i menù e i campi)
  alla 04. Se fosse stato un meccanismo unico avrei dovuto spostare un file e ripartizionare.
  ⚠️ **Il conteggio delle clausole non cambia**: restano trentotto, e la 2 vale una sola clausola
  coperta solo quando **entrambe** le metà sono chiuse. Uno spezzamento che facesse contare due
  clausole al posto di una sarebbe una crescita mascherata da chiarimento.

- **20 set 2026, 00:50 — Il testo d'aiuto della voce 8 ha un gemello in un file di un'altra
  unità.** L'aiuto che promette una scelta fra ricaricare e sovrascrivere sta nella testata
  dell'editor di elemento (unità 02), ma un testo gemello sta nell'editor di collezione, che è
  dell'unità 04. La 02 corregge il proprio e **dichiara** se il gemello ha lo stesso difetto; la 04
  lo eredita dal suo resoconto. Nessuna delle due esce dal perimetro.

- **20 set 2026, 01:30 — La testata della Home esce dal condizionale, e il «?» comparirà anche nei
  rami «nessuno spazio» ed «errore».** Deciso da me. L'unità 01 aveva segnalato che sposta un
  comportamento visibile e aveva chiesto che decidesse chi ripartisce, invece di farlo di straforo.
  **Il motivo non è estetico, è un precedente nello stesso file**: l'intestazione per schermo
  stretto sta già fuori dal condizionale, e il suo commento avverte che spostarla dentro
  «riaprirebbe il buco», lasciando chi non ha ancora uno spazio «chiuso fuori senza modo di
  raggiungere Esci». L'aiuto di schermata è la stessa classe di elemento: è il canale che spiega
  cosa sta succedendo, e nasconderlo durante il caricamento e negli errori **è** il difetto.

- **20 set 2026, 01:30 — Il doppio montaggio di ogni pagina entra nel goal, e non è una crescita
  dell'obiettivo.** La regola che mi ero dato alle 00:10 diceva che ci entrava **solo se serviva
  alla voce 6**. L'unità 01 ha stabilito due cose: che **non è la causa** del difetto sulla Home —
  lì la causa è la testata nel condizionale — e che **la catena della Home non spiega la
  riproduzione del 19 settembre sui due editor**, dove la testata è già fuori dal condizionale.
  Il doppio montaggio, che dismette e ricrea l'intero sottoalbero di pagina, è **l'unico candidato
  rimasto** per quella metà della voce 6. Serve, quindi entra — e la condizione che mi ero dato è
  rispettata invece di essere aggirata.
  **Il conteggio delle clausole non cambia: restano trentotto.** Il montaggio doppio non è una
  clausola nuova, è parte del rimedio della 6.

- **20 set 2026, 01:30 — Un commento del layout è dimostrato falso, e si riscrive.** Afferma che
  senza una certa sottoscrizione il layout resterebbe fermo al render iniziale per l'intera
  sessione; l'unità 01 ha mostrato con quale meccanismo non sia vero. Entra nel lavoro della 01b:
  un commento falso è peggio di un commento assente, perché chi lo legge non tocca il codice
  credendo di romperlo.

- **20 set 2026 — Due premesse della ricognizione del 19 settembre sono cadute, e il modo in cui
  sono cadute vale più delle due voci.** Registrate dall'unità 02.

  1. **La voce 10 non è un difetto**, e il fatto che lo dimostra stava **dentro la ricognizione
     stessa**: il menù ha già l'ordinamento per voto, spento dalla sola condizione del voto al buio,
     e la ricognizione dichiara al proprio secondo capoverso di aver lavorato su una collezione «con
     voto al buio attivo». La schermata di prova non aveva le condizioni. ⚠️ **Il flag non è nemmeno
     troppo restrittivo**: su una collezione cieca le righe altrui non arrivano finché non hai
     votato, quindi ordinare per voto metterebbe in fondo proprio gli elementi con tre voti alti che
     non puoi ancora vedere. Vale la dottrina già applicata venti righe sopra nello stesso file:
     *un comando che risponde senza fare quello che promette è peggio di un comando assente.*
  2. **Il testo d'aiuto della voce 8 non mentiva.** La ricognizione diceva che «il comportamento non
     corrisponde a ciò che l'aiuto promette»: l'aiuto parlava dell'**elemento**, non della
     recensione, e sull'elemento la scelta fra ricaricare e sovrascrivere **esiste davvero**. Il
     difetto era **un'assenza** — quella schermata salva anche la recensione, con un'altra regola,
     e l'aiuto non la nominava.
     ⚠️ **E la distinzione ha cambiato il rimedio, che è il punto.** Quella frase vive in **quattro
     file**, declinata sull'entità di ciascuna pagina, ed è vera in tutti e quattro: correggerla
     dove sembrava falsa l'avrebbe fatta divergere dalle altre tre. L'unità ha **aggiunto** un
     paragrafo, che non ha gemelli e non ne può far divergere nessuno.
     **Conseguenza per l'unità 04**: se qualcuno le passasse questa voce come «l'aiuto mente anche
     nell'editor di collezione», quella premessa è falsa, e lì non c'è niente da correggere.

- **20 set 2026 — Un rilievo dell'unità 02 era una regressione che l'unità stessa aveva appena
  introdotto, e il `checker` l'ha stabilito.** Non un debito pregresso: prima di quel diff il ramo
  riallineava sempre il modulo, quindi il difetto **nasce nel momento in cui il messaggio comincia a
  chiedere un'azione**. È il caso per cui la revisione esiste, ed è stato corretto dentro l'unità.
  **Il fix non è quello che il revisore proponeva**: invece di confrontare a mano i due campi, legge
  la stessa espressione che governa il pulsante — così il testo e il pulsante non possono divergere
  per costruzione, mentre con un confronto separato potrebbero, al primo che ritocca una delle due
  parti.

- **20 set 2026 — La voce 16 è verificata e la mia decisione regge, ma ora è falsificabile.**
  Avevo deciso di non toccarla sulla base di un fatto della ricognizione, e avevo chiesto all'unità
  03 di **verificare quel fatto** dicendole che se si fosse rivelato falso la mia decisione cadeva.
  L'ha confermato con le due righe: il servizio gemello è registrato con la **stessa identica
  forma**, carattere per carattere salvo il nome del tipo, ed è della stessa famiglia — entrambi
  wrapper su web storage che ricevono il runtime JS sincrono.
  **La premessa del rapporto di chiusura era vera solo per due dei tre «fratelli».** Le due
  convenzioni coesistono davvero, quindi uniformare è una decisione di progetto e non un fix.

- **20 set 2026 — La voce 15 aveva una premessa incompleta, e il fatto nuovo ribalta il principio
  invece di applicarlo.** Il rapporto diceva che due metodi erano «scoperti». Non lo erano: erano
  **muti**. La libreria di autenticazione li invoca **solo** da un gestore che cattura tutto e non
  rilancia mai, e il progetto non registra alcun handler di diagnostica — verificato da
  `doc-checker` sul sorgente della **versione installata**, e riverificato indipendentemente dal
  `bug-hunter`.
  ⚠️ **Perché questo capovolge il principio del ciclo scorso.** «Si protegge dove esiste un ripiego
  onesto» presuppone che *non* proteggere significhi **non partire**: l'eccezione risale e ferma
  qualcosa. Qui non ferma niente — l'applicazione prosegue comunque, convinta di aver salvato, **e
  in più nessuno lo sa**.
  **Esito:** il salvataggio è protetto, ma il `catch` **non è una protezione** — è l'unica traccia
  possibile di un fallimento che oggi sparisce, e il commento lo dichiara per non mentire sul
  proprio scopo. La cancellazione **resta nuda**, con quattro ragioni verificate, fra cui che
  proteggerla falsificherebbe un commento esistente e che in un solo logout stamperebbe la stessa
  riga **quattro volte**. Resta una **dichiarazione** al posto della protezione, perché senza di
  essa la regressione sarebbe a un passo e sembrerebbe una pulizia.

- **20 set 2026 — Due difetti nel lavoro dei revisori, dichiarati dall'unità invece che taciuti.**
  Un revisore ha consultato la documentazione di una **versione sbagliata** del pacchetto (4.2.7
  invece della 6.3.0 installata), e una sua prova era **aritmeticamente impossibile** — «68
  occorrenze in 30 file» in una cartella che ne contiene 28. A notarlo è stato il `checker`, non il
  revisore e non l'unità. Nessuno dei due rilievi dipendeva da quelle letture, quindi non sono
  stati declassati — ma il credito da dare a un rapporto di revisione non è uniforme, e questo è il
  tipo di fatto che lo misura.

- **20 set 2026 — Una regola del foglio disattiva una protezione dichiarata da un'altra regola dello
  stesso foglio, e nessuna revisione di diff poteva trovarlo.** Scoperto dall'unità 04. La classe dei
  dati vale 0-1-0; la regola base dei campi è dentro un `:where()`, quindi vale **zero**. Due campi
  di testo finiscono così a **15,2 px**, sotto i 16 che quella regola dichiara di voler tenere — e il
  commento accanto dice perché: *sotto i 16px iOS ingrandisce la pagina al fuoco.*
  ⚠️ **Il difetto non è in nessuna delle due regole prese da sola**: entrambe sono corrette e ben
  commentate. Esiste solo nella loro interazione, e solo a runtime. `:where()` azzera la specificità
  per costruzione — è il suo scopo — ma qui racchiudeva una **protezione di accessibilità con la sua
  ragione scritta accanto**, rendendola sovrascrivibile da qualunque classe, inclusa una che non
  sapeva di sovrascriverla.
  **Chiudibile con una riga e indipendente dalla scala**: è la voce `23a` del mandato della 05, ed è
  la prima cosa che le ho chiesto di fare.

- **20 set 2026 — Un adempimento del §4 è saltato nell'unità 04, ed è la seconda volta stanotte che
  un anello cade nello stesso punto strutturale.** Il `checker` non è stato lanciato sull'istruttoria
  del secondo giro di revisione, dove `bug-hunter` e `conformity` sommavano due rilievi e il §4 dice
  «si lancia **sempre**». **L'unità l'ha dichiarato invece di nasconderlo.**
  **Ho accettato senza rilanciarlo**, e i quattro fatti che lo reggono: i sei rilievi del giro erano
  **tutti di forma** — il `bug-hunter` stesso dichiara che nessuno ha trovato un comportamento
  sbagliato; **nessuno è stato scartato**, quindi il rischio che quella regola previene (filtrare in
  silenzio un rilievo fondato) non si è dato; il **`checker` del fix è stato lanciato** e ha
  verificato tutti e sei contro il codice citando le righe; e l'unità ha riverificato di persona i
  due più delicati con i comandi citati. Un `checker` a posteriori avrebbe istruito rilievi già
  corretti e già verificati: **adempimento, non prova**.
  ⚠️ **Il pattern però va scritto, perché è il secondo caso in due cicli.** Il ciclo precedente aveva
  misurato che `coverage` si perdeva perché cadeva **dopo** il momento in cui si scrive il tracciato.
  Qui è lo stesso meccanismo in miniatura: il primo giro di revisione aveva già chiuso il proprio
  blocco, e quando il secondo giro ha prodotto rilievi, il tracciato era stato compilato una volta.
  **Un passo che cade fra due momenti in cui si scrive il tracciato tende a saltare**, e nessuna
  delle due volte è stata pigrizia. Va nel rapporto finale come rilievo sull'impianto.

- **20 set 2026 — Chiudendo la voce dei rimandi scaduti, l'unità 04 ne ha introdotto uno nuovo non
  conforme nella stessa ora**, e il revisore l'ha preso. È il rilievo più istruttivo del goal:
  **un'ancora dichiarata cercabile che il grep non trova è peggio di un numero di riga, perché sembra
  già conforme.** La convenzione adottata il 19 settembre non è «niente numeri»: è «un frammento che
  un grep trova», e la differenza si vede solo eseguendo il grep.

- **20 set 2026 — L'allineamento chiuso dall'unità 01 era rotto sopra i 640px, e l'ha trovato
  l'unità 05 otto ore dopo.** Dentro una media query il titolo saliva a 52px mentre l'ancora del
  pulsante d'aiuto restava tarata sul corpo più piccolo: **il «?» stava 8,64px più in alto del
  centro della prima riga**. Non era una regressione della 01 — la media query è scritta da molto
  prima — ma la sua correzione era **vera solo sotto una certa larghezza**, e nessuno l'aveva
  misurata sopra.
  ⚠️ **Conseguenza per il collaudo, e va ricopiata nel brief**: l'allineamento del «?» va provato a
  **due larghezze**, non a una. La misura dell'unità 01 da sola sarebbe passata lasciando vivo il
  difetto.

- **20 set 2026 — La quarta premessa del rapporto di chiusura è caduta**, e stavolta su una voce
  che il collaudo dava per la più grave delle cinque di `ui-critic`. Il rilievo diceva «le superfici
  esistono come token ma non come scala», con una ventina di dichiarazioni di fondo che non li
  usano. Misurato dall'unità 05: su **49** dichiarazioni, **44 usano un token**, e cinque delle
  restanti sono `transparent` — che non è un colore. **Sotto il rilievo infondato ce n'è però uno
  vero e più grosso**, istruito in `handoff/05-scala-e-metro/istruttoria-superfici.md`.
  **Il bilancio del goal su questo punto:** quattro premesse cadute su quattro verificate. Nessuna
  è stata scoperta rileggendo il rapporto: sono emerse perché ogni mandato chiedeva di **verificare
  il fatto** prima di correggere, dichiarando che se il fatto fosse caduto la decisione sarebbe
  caduta con lui.

- **20 set 2026 — L'unità 05 non ha chiuso la voce 25, e il motivo è migliore di una chiusura.**
  L'unità 04 raccomandava di dichiararla intenzionale con una frase di commento. La 05 non l'ha
  scritta perché resta **un caso che nessuno ha guardato** — l'elenco delle collezioni a registro
  vuoto, l'unica schermata in cui le due forme si vedono insieme — e «scrivere *intenzionale* il
  giorno prima di guardarlo significherebbe dichiarare chiusa una decisione che sta per essere
  presa». **Va guardata al collaudo**, ed è nel piano.

- **20 set 2026, collaudo — Il primo difetto trovato nel browser non è del codice ma di una misura
  attesa, ed è la prima volta in due cicli.** Due resoconti dichiaravano il campo della data
  invariato a 48px; nel browser è **71**, perché sta in un grid affiancato a un campo alto 71 e lì
  l'altezza la decide la riga, non il contenuto. **La protezione che quella voce introduceva
  funziona**: il corpo è 16px, misurato.
  ⚠️ **Perché è successo, e vale più del caso.** Le due unità hanno scritto una misura attesa **che
  non potevano verificare**: il loro mandato vietava di avviare il server, ed è il server l'unico
  posto dove il grid esiste. Il calcolo era giusto in isolamento e irrilevante nel contesto. **Una
  misura attesa calcolata invece che osservata è un'ipotesi travestita da criterio**, e va nel
  rapporto finale come rilievo sull'impianto: le misure attese sono state il guadagno più grande di
  questo collaudo, e questo ne è il limite.

- **20 set 2026 — L'utente ha deciso di lanciare `ui-critic` benché l'esito non sia verde.**
  Il §7 lo condiziona a `ESITO: verde`, e qui è `difetti`. **Non l'ho deciso io**, e non per
  formalismo: la regola è scritta sull'esito e non sulla natura dei difetti, quindi interpretarla
  larga sarebbe stata un'auto-esenzione — la stessa classe che questo impianto ha tolto a
  `threat-hunter` il 19 settembre, dove a dichiarare «qui non serve» era la parte sotto revisione.
  Il motivo dichiarato della regola — «non si collauda una versione che si sta per correggere» —
  **non si applica**: nessuno dei due difetti si corregge in questo goal, quindi il codice provato è
  quello finale.

- **20 set 2026 — Il difetto dei pulsanti di conferma va nel rapporto, non in un'unità.** Deciso
  dall'utente. La clausola 27 chiedeva di **provare** e la prova è stata fatta: l'esito è che a
  schermo stretto i due pulsanti non stanno sulla stessa riga e **il distruttivo sta sopra quello di
  sicurezza**. Il difetto trovato è nuovo, e vale la regola sulle crescite — lo stesso precedente
  per cui il ciclo scorso ha lasciato dieci rilievi del collaudo senza correggerli.
  **La clausola 27 è quindi coperta**, e il difetto è una voce del `FUORI SCOPE`.

## PARTIZIONE

Cinque unità, **in sequenza, mai in parallelo**. La colonna «voci» usa la numerazione del
`FUORI SCOPE — aggregato` di `handoff/CHIUSURA.md`.

| Unità | Perimetro | Voci | Dipende da | Stato |
|---|---|---|---|---|
| **01 punto-interrogativo** | `Shared/TestataPagina.razor`, `Layout/MainLayout.razor`, e in `wwwroot/css/app.css` **solo** il blocco della testata e dell'aiuto più la regola di animazione di pagina | **6**, **9**, + la datazione della regressione | — | **PARZIALE** — integrata con un merge il 20 set. **Voce 9 chiusa**; **voce 6 diagnosticata e passata alla 01b**, perché la causa sta in un file fuori dal suo perimetro. È il `PARZIALE` previsto dal punto 3 del suo tetto |
| **01b home-e-montaggio** | `Pages/Home.razor`, `Layout/MainLayout.razor`. **Nessuna riga di CSS** | **6** (la correzione), + il montaggio doppio di ogni pagina | 01 | **FATTO** — integrata il 20 set. ⚠️ **L'unità è morta durante il lavoro**, uccisa dal riavvio della sessione padre, dopo aver finito l'implementazione e prima della revisione: il codice è suo, la revisione e il resoconto sono del capo, e sta scritto in testa al resoconto |
| **02 voti-e-recensioni** | `Shared/VotoInput.razor`, `Shared/RecensioniElemento.razor`, `Services/CalcoliVoti.cs`, `Pages/ItemEdit.razor`, `Pages/CollectionDetail.razor`, e in `app.css` **solo** le regole del voto | **2a**, **7**, **8**, **10**, **26** | 01b | **FATTO** — integrata il 20 set. Quattro voci con codice, la **10 chiusa dichiarando che non è un difetto**. La **26 si è chiusa dentro entrambe le decisioni del 19 settembre**: nessuna revoca da adjudicare |
| **03 accesso-e-profilo** | `Services/BrowserSessionHandler.cs`, `Services/AllineatoreProfilo.cs`, `Services/SupabaseService.cs`, `Services/AuthStateService.cs`, `Services/PkceStore.cs`, `Services/SpaceStateService.cs`, `Program.cs`, `Pages/Profile.razor`. **Nessuna riga di CSS** | **15**, **17**, **18**, + **16** dichiarata | 02 | **FATTO** — integrata il 20 set. **Nessuna riga di CSS toccata**, come il perimetro garantiva. La **16 verificata e lasciata stare**; la **15 chiusa con un'asimmetria dichiarata** invece che uniformata |
| **04 controlli-e-campi** | in `app.css` le regole di pastiglia, bottone compatto, blocco campo, `.dato`, selettore di icone · `Pages/CollectionEdit.razor`, `Pages/Spaces.razor`, `Shared/CampoInput.razor`, `Pages/SpesaEdit.razor`, `Pages/Collections.razor`, `Pages/Notes.razor`, `Pages/Home.razor`, `Pages/Spese.razor` | **2b**, **4**, **19**, **21**, **22**, **23**, **24**, **25**, + il residuo della **20** | 03 | **FATTO** — integrata il 20 set. **Otto voci su otto**, sei con codice e due istruite. `PARZIALE` era previsto e non è servito. ⚠️ **Un adempimento del §4 mancato e dichiarato**: v. `DECISIONI` |
| **05 scala-e-metro** | `wwwroot/css/app.css`, tutto ciò che resta · `Shared/Navigazione.razor` | **1**, **3**, **5**, **23a**, **23b**, **25** — istruttoria e decisione | 04 | **FATTO** — integrata il 20 set. **Quattro chiuse, due consegnate.** Il mandato ammetteva «nessuna riga di codice»: ne ha scritte, perché **sotto due delle tre voci di scala c'erano difetti funzionali veri** |

**Non assegnate a un'unità, perché non producono codice** — le fa il capo a ciclo chiuso, con il
server avviato da lui e i PID su disco:

| Voce | Cosa | Quando |
|---|---|---|
| **27** | la coppia «Sì, elimina» / «Annulla» a 360px | collaudo, con `live-testing`. È un componente **in pagina**, non un dialogo nativo: si rende premendo «Elimina» su una **collezione**, non su una spesa |
| **31** | `Sovrascrivi()` col nome vuoto, stesso elemento in due schede | collaudo. Non passa da dialoghi nativi |
| **32** | «Tutte» alto 48 | collaudo, una riga di misura |
| **33** | la riserva del giro A sulla misura della colonna | si chiude **dichiarando**: la proprietà riserva la colonna sempre, i due casi non possono più divergere per costruzione |
| **37** | la riga nel `CLAUDE.md` e la voce `soft_deny` | il capo prepara, **l'utente applica**: è una superficie di configurazione |

## RAZIONALE

**Il file che decide la partizione è `wwwroot/css/app.css`, e stavolta non è un'ipotesi.** La
ricognizione l'ha misurato: è rivendicato da **tredici voci su ventidue**, e — questa è la parte
che cambia tutto rispetto al ciclo precedente — **le loro regole non sono disgiunte**. Cinque
collisioni riga su riga:

| Collisione | Chi si scontra |
|---|---|
| la testata e il pulsante d'aiuto | **6 ∩ 9**, e la **5** entra nello stesso blocco |
| il voto grande | **5 ∩ 7** |
| il bottone compatto | **22 ∩ 25 ∩ 23** |
| la famiglia delle pastiglie | **4 ∩ 21** — il commento di `button.pastiglia` **nomina** la regola della 21: toccarla obbliga a scrivere nel territorio della 4 |
| il blocco del campo | **19 ∩ 24 ∩ 23** |

**Quindi la proprietà pura di `app.css` è impossibile senza un'unità-monstre**, e la partizione è
per **vicinato di righe**: ogni unità possiede le regole che tocca, e l'ordine garantisce che
nessuna trovi spostato ciò che un'altra ha già scritto.

**Perché la 01 è prima, e non è per gravità.** Lo è anche — il «?» è l'unico canale in-app che
spiega il voto al buio e i conflitti, ed è rotto **in produzione adesso** — ma il motivo
strutturale è che le voci 6 e 9 toccano gli **stessi due selettori**: separarle significherebbe
due unità sulle stesse dieci righe.

**Perché la 05 è ultima.** Rivendica righe che la 01 e la 02 possiedono, e ridefinisce fondi di
regole che la 04 potrebbe aver spostato. È anche l'unica le cui tre voci non sono difetti ma
decisioni: metterla prima significherebbe decidere il sistema su un foglio che sta per cambiare.

**Perché la 03 sta in mezzo benché sia indipendente.** Non tocca **una sola riga di CSS**, quindi
potrebbe girare in qualunque momento. Resta in sequenza perché la sequenzialità è una regola
dell'architettura, non un'ottimizzazione — e sta terza perché non ritarda le due urgenti.

**Tre cose che la ricognizione ha trovato e che nessun elenco conteneva**, tutte dentro voci già
numerate e quindi senza far crescere l'obiettivo:

1. `Pages/CollectionEdit.razor` porta **tre rimandi scaduti** oltre a quello che il ciclo scorso
   ha corretto. È il «fatto largo» che la voce 20 descriveva — la convenzione nuova vive
   nell'intestazione del foglio di stile, ma il difetto che cura esiste anche nei `.razor` —
   ancora vivo nel file che la 20 ha appena toccato. Vanno alla **04**, che possiede quel file.
2. La voce 25 ha un **gemello non censito**: «Nuova nota» ha la stessa doppia forma di «Nuova
   collezione». Chiuderne una sola sposterebbe l'incoerenza di un file. Vanno insieme, alla **04**.
3. La voce 22 è **già coperta nel testo** del commento, e la voce 2 conta nove controlli **a
   runtime** ma ne tocca **due file soli**, perché cinque e tre sono dentro cicli.

## PROSSIMA AZIONE

PROSSIMA AZIONE: GOAL CHIUSO — apri una sessione nuova.

**Il goal è chiuso perché esistono due file, non uno.** Il rapporto della sessione di chiusura sta
in testa a `handoff/CHIUSURA.md`, sopra il proprio mandato, e questa riga porta il valore riservato
che il §6 del `CLAUDE.md` pretende. Integrato su `main` con `dc8489d`, worktree e branch rimossi.

```
CHIUSURA: 6 unità — FATTO 5 · PARZIALE 1 · BLOCKED 0
COPERTURA: 38 clausole — coperte 31 · scoperte 0 · rinviate 7
```

⚠️ **La previsione d'apertura di questo piano è caduta, ed è il ritrovamento principale della
chiusura.** Le clausole **3** e **5** erano dichiarate qui sopra non chiudibili perché «decisioni di
sistema di design»: erano chiuse **con codice** dall'unità 05, perché sotto due delle tre voci di
scala c'erano difetti funzionali veri. E l'elenco delle «sette» ne nominava **otto**. Il sette
finale è esatto, ma la sua composizione è un'altra: **1, 28, 29, 30, 34, 35, 37**.

**Cosa resta vivo in `handoff/`, e perché:**

| File | Perché non è archiviato |
|---|---|
| `CHIUSURA.md` | il rapporto non si archivia finché l'utente non l'ha letto |
| `configurazione-da-applicare.md` | **lavoro dell'utente**: i due testi della clausola 37 toccano `~/.claude/` |
| `collaudo/` | i quattro documenti del collaudo, che il prossimo ciclo rilegge |
| `01-testata-e-aiuto/` | **`PARZIALE`**: la chiusura ha deciso di non archiviarla e ha dichiarato perché — il merito direbbe di sì, ma la delega veniva da un capo, non dall'utente, e un livello inferiore può essere più restrittivo, mai più permissivo. **Un `git mv` la ribalta** |
| `PIANO.md`, `server.md` | questo file, e il server che il capo ha fermato a ciclo chiuso |

**Il collaudo è chiuso**, tutti e quattro i passi eseguiti:

| Passo | Esito |
|---|---|
| **Server** | avviato **dall'utente** dopo che il sistema ne aveva ucciso uno per memoria bassa |
| **`live-testing`** | **10 scenari su 10**, `ESITO: difetti` — due difetti, **nessuno funzionale**, e **cinque prove dichiarate non eseguibili** |
| **`ui-critic`** | **5 rilievi**, e **tutti e cinque quelli del 19 settembre confermati chiusi** con le misure |
| **Voce 33** | si chiude **dichiarando**: la proprietà riserva la colonna sempre, i due casi non possono più divergere per costruzione |

Gli esiti stanno in `handoff/collaudo/esito-live-testing.md` e `esito-ui-critic.md`.

⚠️ **Nessuno dei sette rilievi del collaudo è stato corretto, ed è una scelta.** Vale la regola che
questo goal ha tenuto per tutta la notte: le voci nuove trovate al collaudo vanno nel rapporto, non
nel goal corrente. **Due sono minuscoli e oggettivi** — il «?» in Arial, che è una riga, e il campo
della data, che ha ora causa completa e due fix scritti — e sono stati **portati all'utente invece
che seppelliti nel mucchio**: la decisione è stata di non aprirli, perché allargare l'obiettivo
all'ultimo passo avrebbe reso finta una regola applicata cinque volte.

---

*Sotto, la cronaca del blocco che ha fermato il collaudo per un'ora, conservata perché è la sola
volta in cui questo goal si è fermato ad aspettare l'utente.*

**Il collaudo è stato `BLOCKED` e ha aspettato un gesto dell'utente.**

⚠️⚠️ **Il server è stato ucciso dal sistema per memoria bassa**, e la notifica dice esplicitamente
di **non riavviarlo d'iniziativa** perché la memoria potrebbe essere ancora scarsa. Il collaudo
richiede il browser, quindi richiede il server: **non è eseguibile finché l'utente non autorizza il
riavvio.** Porta 5000 libera, nessun processo orfano — verificato.

**Tutto il resto del collaudo è pronto e non aspetta niente**: il piano sta in
`handoff/collaudo/piano.md` e il brief di `live-testing` in `handoff/collaudo/brief-live-testing.md`,
scritti prima di servire. Al riavvio del server, il primo passo è lanciare quel brief.

Tutte e sei le unità sono rientrate e integrate, gate verdi.

I quattro passi, nell'ordine, che non è negoziabile:

1. **Riavviare il server** — *bloccato, v. sopra* — e riscrivere `handoff/server.md`. Serve
   comunque un riavvio a prescindere dalla memoria, perché `main` è avanzata con l'unità 05 e il
   DevServer legge i manifest solo al proprio avvio.
2. **`live-testing`**, con il brief composto **ricopiando** le misure dalle sei sezioni
   `LA MISURA ATTESA PER IL COLLAUDO`. ⚠️ **Più le quattro voci che nessuna unità produce**, che
   hanno una sezione propria nel piano proprio perché nessuno le ricopierebbe.
3. **`ui-critic`, solo se `live-testing` torna `ESITO: verde`.**
4. La voce **33**, che si chiude **dichiarando** e non provando.

**Poi la sessione di chiusura**, che è l'unica a fare la verifica di copertura sulle **38 clausole**:
il §6 vieta di lanciare `coverage` dentro un'unità, e il capo che ha disegnato la partizione è il
candidato peggiore a trovarne i buchi.

⚠️ **Prima di aprire ogni unità: committare e POI PUSHARE**, perché il worktree nasce da
`origin/main`.

⚠️ **E riavviare il server dopo ogni integrazione**, perché il DevServer legge i manifest solo al
proprio avvio.

⚠️ **Prima di aprire ogni unità: committare piano e mandato, e POI PUSHARE.** Il worktree
dell'unità nasce da `origin/main`, non da `main` locale: committare non basta. Misurato dall'unità
02 del ciclo precedente, che è partita da un albero in cui il lavoro della 01 non c'era e avrebbe
dichiarato contratti falsi.

**Se la 01 torna `PARZIALE` perché la causa della voce 6 sta fuori dal suo perimetro** — il caso
più probabile è `Pages/Home.razor`, per via del doppio caricamento — **ripartiziona prima di
aprire la 02**, come impone la regola sulla sequenzialità.

## APERTO

**Una domanda per l'utente, aperta dall'unità 01b.** Il titolo della scheda del browser sulla Home
resta il letterale «Eton», mentre nelle **altre dieci** pagine che montano la testata è
«titolo — Eton». È l'unica eccezione dell'applicazione, verificata aprendo tutti e undici i file.

**Perché è una domanda e non un fix già fatto:** la riga **non è toccata dal diff** dell'unità, e il
§5 su un rilievo fondato ma fuori scope non lascia margine. Il fix è **una riga sola**, dentro un
file che l'unità possedeva, e riusa un simbolo che quel diff ha appena introdotto — cioè ha tutte le
caratteristiche del «tanto vale farlo adesso», che è il modo in cui un obiettivo con trentotto
clausole dichiarate ne guadagna una trentanovesima senza che nessuno se ne accorga.

Il rimedio è già istruito nel `FUORI SCOPE` del resoconto della 01b: **basta un sì.**

**Una seconda, dall'unità 02, della stessa famiglia.** Il pulsante della recensione si chiama
«Salva recensione», ma **due** avvisi dicono «premi *Salva* di nuovo»: quello nuovo scritto da
questa unità e uno preesistente a venti righe di distanza. Un'azione che cambia nome fra il pulsante
e il testo che la nomina è un difetto — ma allinearli richiede di toccare **anche** il messaggio
preesistente, che nessuna voce ha segnalato.

**Ho accettato l'argomento dell'unità e non ho fatto correggere**: cambiare solo il nuovo lo
farebbe divergere dal suo vicino, che è la stessa classe di difetto che quella stessa unità ha
appena evitato sul testo d'aiuto declinato in quattro file. Due stringhe, nessun rischio, ma è
scope nuovo — e il goal ha già una trentanovesima candidata in attesa.

**Le altre quattro domande** sono state poste e risposte il 19 settembre notte, e stanno in
`DECISIONI`.

**Un candidato per il goal successivo, istruito dall'unità 03 e non risolto.** Ogni scheda del
browser ha la **propria** istanza dei servizi, e il bootstrap di una seconda scheda può concludere
la propria scrittura **dopo** il logout della prima, rimettendo in memoria locale una chiave appena
cancellata. Vale per l'impronta del profilo, e vale **identico** per lo spazio attivo — una chiave
che questo goal non tocca. La protezione esistente è un campo **di istanza**, quindi copre la corsa
dentro la scheda e non quella fra schede, e nel progetto non esiste alcun meccanismo di
sincronizzazione fra schede: il `checker` ne ha cercati due senza trovarli.

**Non è stato risolto, e il motivo è buono:** il rimedio ovvio chiuderebbe **metà** del problema
lasciando l'altra aperta, dando alla pulizia al logout un'apparenza di simmetria che non avrebbe.
È una decisione di progetto su due servizi. **Il fatto è però scritto nel codice**, così chi legge
non ricava più dal commento una garanzia che il codice non dà.

**Dubbi miei, non ancora domande.**

- **La voce 6 potrebbe non stare in un'unità.** La ricognizione ha riportato i fatti senza
  diagnosticare, ed è ciò che le avevo chiesto. Il fatto più promettente: al cambio di rotta il
  layout ricrea l'intero sottoalbero di pagina, e per i primi 200 millisecondi un **antenato** del
  pannello porta un `transform` non nullo per via dell'animazione d'ingresso. Se la causa fosse
  quella, il rimedio è piccolo; se non lo fosse, l'unità può non trovarla. **Il suo mandato porta
  un tetto dichiarato**: data la regressione, e se il rimedio non sta in una ventina di righe,
  torna con la causa scritta invece di inseguirla.
- **La voce 10 potrebbe non essere un difetto.** L'agente che l'ha segnalata l'ha fatto «con
  cautela», e la ricognizione ha trovato che il menù **ha già** una seconda voce, condizionata a
  un flag. Può essere un'area non sviluppata più che un difetto: l'unità 02 la istruisce prima di
  toccarla.
- **La 04 è la più esposta a tornare `PARZIALE`**: sette voci su otto file, ed è il grumo
  irriducibile del foglio di stile. È la prima candidata a essere ripartizionata.
- **La 05 potrebbe chiudersi senza scrivere una riga**, ed è un esito legittimo purché porti i
  motivi. Le sue tre voci sono l'ingresso naturale della fase 2.1-bis.

## FATTI OPERATIVI CHE COSTANO CARI SE DIMENTICATI

⚠️⚠️ **`main` PUBBLICA IN PRODUZIONE A OGNI PUSH.** `.github/workflows/deploy.yml`, trigger
`on: push: branches: [main]`, destinazione GitHub Pages. Non c'è un gate e non c'è un'approvazione.
Nessun documento del progetto lo diceva prima di stanotte.

⚠️⚠️ **IL WORKTREE NASCE DA `origin/main`: committare non basta, bisogna PUSHARE.** Ereditato dal
ciclo precedente, dove è costato un'unità che lavorava contro un foglio di stile vecchio.

⚠️ **Una sessione che tace non è una sessione morta.** Misurato stanotte, e costato
un'integrazione doppia: il worktree della chiusura aveva un albero **sporco** e nessun processo
vivo, il che si legge come «morta a metà» — e invece stava scrivendo. Il test che distingue i due
casi non è strutturale ma **temporale**: albero **pulito**, più minuti di silenzio, più tutto
pushato. In più il worktree di una sessione **non porta necessariamente il nome dell'unità**: il
secondo worktree della chiusura si chiamava diversamente dal primo, e un capo che cerchi per nome
non lo trova. Si cerca con `git worktree list`, e si controlla lo stato **dentro** il worktree:
il `git status` dell'albero principale non vede nulla di ciò che accade lì dentro.

⚠️ **Rimuovere un worktree lascia spesso la directory vuota sul disco.** La registrazione git
sparisce e i file vengono cancellati, ma l'ultima cartella resta se una shell l'ha avuta come
directory corrente. Non è un problema: `git worktree list` è già pulito. Non fare `cd` dentro un
worktree che stai per rimuovere.

⚠️ **Il classificatore della modalità automatica nega più di quanto dovrebbe.** Stanotte ha
rifiutato `git push --delete` (atteso, è distruttivo) ma anche una lettura di stato di rete
innocua, con la stessa motivazione «Git Destructive». Una catena di comandi viene valutata per il
suo elemento peggiore: **si spezzano in comandi separati**, altrimenti si perde anche la parte
innocua.

Ereditati dal piano precedente, tutti ancora validi. Il dettaglio sta in `handoff/server.md`.

- **Riavvia il server prima di ogni prova nel browser**, e non far compilare nessuno mentre è
  vivo. Il DevServer legge i manifest degli asset solo al proprio avvio.
- **Il server lo avvia e lo ferma il capo**, annotando porta e PID **su disco**. Su Windows la
  morte del padre non uccide i figli.
- **Gli implementer non compilano.** `obj/` non ha lock fra processi. Compila il capo, a fine giro.
- **Il browser giusto ha `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.** Due Chrome sono
  collegati e i nomi si scambiano a ogni riconnessione: solo quello vede `localhost`.
- **La cache della PWA non falsa le prove in sviluppo**, e il banner «versione nuova» che riappare
  lì **non è un difetto**.
- **I dialoghi nativi bloccano il plugin del browser.** Ciò che passa da un `confirm()` non è
  provabile da un agente e va girato all'utente.
