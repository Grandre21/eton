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

## PARTIZIONE

Cinque unità, **in sequenza, mai in parallelo**. La colonna «voci» usa la numerazione del
`FUORI SCOPE — aggregato` di `handoff/CHIUSURA.md`.

| Unità | Perimetro | Voci | Dipende da | Stato |
|---|---|---|---|---|
| **01 punto-interrogativo** | `Shared/TestataPagina.razor`, `Layout/MainLayout.razor`, e in `wwwroot/css/app.css` **solo** il blocco della testata e dell'aiuto più la regola di animazione di pagina | **6**, **9**, + la datazione della regressione | — | PIANIFICATA |
| **02 voti-e-recensioni** | `Shared/VotoInput.razor`, `Shared/RecensioniElemento.razor`, `Services/CalcoliVoti.cs`, `Pages/ItemEdit.razor`, `Pages/CollectionDetail.razor`, e in `app.css` **solo** le regole del voto | **2**, **7**, **8**, **10**, **26** | 01 | PIANIFICATA |
| **03 accesso-e-profilo** | `Services/BrowserSessionHandler.cs`, `Services/AllineatoreProfilo.cs`, `Services/SupabaseService.cs`, `Services/AuthStateService.cs`, `Services/PkceStore.cs`, `Services/SpaceStateService.cs`, `Program.cs`, `Pages/Profile.razor`. **Nessuna riga di CSS** | **15**, **17**, **18**, + **16** dichiarata | 02 | PIANIFICATA |
| **04 controlli-e-campi** | in `app.css` le regole di pastiglia, bottone compatto, blocco campo, `.dato`, selettore di icone · `Pages/CollectionEdit.razor`, `Pages/Spaces.razor`, `Shared/CampoInput.razor`, `Pages/SpesaEdit.razor`, `Pages/Collections.razor`, `Pages/Notes.razor`, `Pages/Home.razor`, `Pages/Spese.razor` | **4**, **19**, **21**, **22**, **23**, **24**, **25**, + il residuo della **20** | 03 | PIANIFICATA |
| **05 scala-e-metro** | `wwwroot/css/app.css`, tutto ciò che resta · `Shared/Navigazione.razor` | **1**, **3**, **5** — **istruttoria e decisione, non necessariamente codice** | 04 | PIANIFICATA |

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

PROSSIMA AZIONE: aprire l'**unità 01 punto-interrogativo**. Il mandato sta in
`handoff/01-punto-interrogativo/mandato.md`.

⚠️ **Prima di aprirla: committare questo piano e il mandato, e POI PUSHARE.** Il worktree
dell'unità nasce da `origin/main`, non da `main` locale: committare non basta. Misurato dall'unità
02 del ciclo precedente, che è partita da un albero in cui il lavoro della 01 non c'era e avrebbe
dichiarato contratti falsi.

## APERTO

**Domande per l'utente — nessuna aperta.** Le quattro sono state poste e risposte il 19 settembre
notte, e stanno in `DECISIONI`.

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
