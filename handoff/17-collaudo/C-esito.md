GIRO C - ESITO: PARZIALE

LE PROVE DEI RESOCONTI: 5/5 eseguite, tutte passate. Non ho potuto eseguirne altre (vedi sotto) per
mancanza di uno spazio condiviso e per restare nel minimo indispensabile di dati da creare, non sono
difetti, sono limiti dichiarati:
- Home, spazio Personale con la spesa di collaudo creata: "Per categoria: Spesa 100%" compare
  esatto, sulla stessa riga logica del totale mensile (unita 08, rilievo 11) - PASSA.
- /profile, pannello "?": tre paragrafi verbatim ("Nome ed email arrivano dall'account Google...",
  "Il nome e la foto compaiono agli altri membri...", "Esci chiude la sessione solo su questo
  dispositivo..."), nessuno ripete i dati gia a schermo, un solo h1 (unita 08) - PASSA.
- SpesaEdit, riapertura importo sopra il migliaio (unita 12, dettaglio sotto) - PASSA.
- Traduzione dell'errore su salvataggio SpesaEdit, creazione Spazi, apertura CollectionDetail
  (unita 12/13/14, dettaglio sotto) - PASSA su tutti e tre i file provati.
NON eseguite, dichiarate invece di saltate in silenzio: empty state di /notes e /collections (unita
09, richiederebbe un nuovo spazio vuoto), link "Gestisci questo spazio" e pannello aiuto a quattro
paragrafi di /spaces/{id} (unita 08, richiede uno spazio condiviso - ne esiste solo uno, Personale),
messaggi d'errore specifici di RecensioniElemento (unita 10, richiederebbe scrivere una recensione
vera sull'elemento di collaudo esistente), messaggio OAuth di Benvenuto.razor (unita 15, richiede la
disconnessione).

IMPORTO 1284,50 RIAPERTO: "1284,50" - verbatim, senza punto delle migliaia. Riaperta due volte (dopo
la creazione, e dopo un salvataggio successivo con descrizione cambiata e poi ripristinata), il
campo ha sempre mostrato "1284,50".

SALVA APPENA RIAPERTA: spento - con accanto "Non c'e niente da salvare: non hai ancora cambiato
niente." Riprovato il ciclo modifica/salva/ripristina/salva: Salva si accende con una modifica vera
e si spegne quando la si annulla, in entrambi i versi. "Chiudi" a modifiche nulle esce subito, senza
alcuna domanda di conferma.

LE TRE FRASI OAUTH: non eseguibili senza perdere l'accesso. Nessuna azione del plugin apre una
finestra di navigazione in incognito, e un logout e irreversibile per questa sessione (nessun modo
di rientrare con Google). Come indicato dal brief, ho saltato le tre prove invece di disconnettermi.

ERRORE VERO PROVOCATO: si, tre volte, su tre file diversi, mai un JSON, sempre una frase italiana.
Non avendo un modo di attivare la modalita aereo o l'offline delle DevTools dal plugin, ho
sovrascritto temporaneamente window.fetch per farlo fallire (la simulazione piu vicina a "togliere
la rete" che potevo fare senza toccare impostazioni di sistema; lo dichiaro perche cambia cosa ho
davvero provato) e l'ho ripristinato subito dopo ogni prova:
- SpesaEdit (salvataggio di una spesa esistente): "Non e stato possibile salvare: il database ha
  rifiutato la scrittura, oppure non e stato raggiunto. Quello che hai scritto e ancora qui: riprova
  fra un momento, e non chiudere la pagina." - verbatim come da unita 13. Console: "[Spese]
  Salvataggio non riuscito: TypeError: Failed to fetch (simulato dal collaudo)".
- Spazi (creazione nuovo spazio): "Non e stato possibile creare lo spazio: il database ha rifiutato
  la creazione, oppure non e stato raggiunto. Il nome che hai scritto e ancora qui: riprova fra un
  momento." - verbatim come da unita 14. Il nome digitato e rimasto nel campo; nessuno spazio e
  stato creato. Console: "[Spazi] Creazione dello spazio non riuscita: TypeError: Failed to fetch
  (simulato dal collaudo)".
- CollectionDetail (apertura collezione): "Non e stato possibile aprire la collezione: puo essere
  la connessione, oppure il tuo accesso a questo spazio che e cambiato. Riprova fra un momento." -
  verbatim come da unita 13, con il pulsante "Riprova" presente. Console: "[Collezione] Apertura del
  dettaglio non riuscita: TypeError: Failed to fetch (simulato dal collaudo)".

JSON GREZZO VISTO DA QUALCHE PARTE: no. In nessuna delle tre prove sopra, ne altrove nel giro, e
comparso un messaggio grezzo di PostgreSQL/PostgREST. Ogni frase osservata era italiano leggibile, e
ogni frase aveva la sua riga di diagnosi in console con ex.Message per esteso, mai a schermo.

CONSOLE: pulita rispetto a JSON grezzo o errori inattesi. Le uniche righe ERROR viste sono le tre
diagnosi attese sopra, con l'eccezione simulata da me. Le righe INFO "Debugging hotkey:
Shift+Alt+D" sono di Blazor stesso, non un difetto. Non ho incontrato la riga dell'estensione "A
listener indicated an asynchronous response..." citata nel contesto, ma e comunque classificata come
rumore da ignorare.

RESTA NEL DATABASE: la spesa "COLLAUDO 4 SET", 1.284,50 EUR, categoria Spesa, 04/09/2026, spazio
Personale, creata per la prova obbligatoria sull'importo, come richiesto dal brief. Nessun'altra
cosa creata: il tentativo di creare uno spazio "COLLAUDO 4 SET" e fallito per costruzione (fetch
bloccato apposta) e non ha lasciato nulla nel database; nessuna nuova nota, collezione, elemento o
recensione.

ALTRO CHE HAI VISTO: in tutte e tre le prove con fetch bloccato, insieme al messaggio italiano
corretto e comparsa ANCHE la barra gialla generica di Blazor "Si e verificato un errore imprevisto.
Ricarica X", in fondo alla pagina. Il testo e fisso e non espone dettagli tecnici (coerente con
quanto gia verificato nel resoconto dell'unita 15), ma resta un elemento tecnico non tradotto che
compare ogni volta che la rete cade durante un'azione, accanto al messaggio ben scritto, nessuna
riga di console aggiuntiva ne accompagnava la comparsa, quindi non sono risalito a quale chiamata
non protetta la scateni. Potrebbe essere legato ai due JS interop "fuori dalla rete di sicurezza"
gia segnalati come FUORI SCOPE nel resoconto dell'unita 15, o a un'altra chiamata di sfondo. Lo
segnalo perche osservato in modo identico su tre file diversi, non come caso isolato, non e JSON
grezzo e quindi non riapre il difetto centrale, ma e un residuo tecnico visibile all'utente ogni
volta che la rete manca durante un salvataggio.

NON PROVATO:
- Le tre frasi OAuth (URL con error_description, error=access_denied, error=access_denied e
  error_code=signup_disabled) - richiedono di essere disconnessi; nessun modo di farlo con il plugin
  senza perdere l'accesso (niente incognito disponibile), saltate come indicato dal brief.
- Empty state di /notes e /collections (unita 09) - lo spazio Personale ha gia una nota e una
  collezione (create dai giri precedenti); provarlo avrebbe richiesto un nuovo spazio vuoto, che
  eccede il minimo indispensabile per questo giro e non e fra le prove esplicitamente richieste dal
  brief C.
- Link "Gestisci questo spazio" (unita 08, rilievo 7) e pannello aiuto a quattro paragrafi di
  /spaces/{id} - richiedono uno spazio condiviso, che non esiste (Personale e l'unico spazio). Non
  creato uno spazio apposta, per la stessa ragione di minimalita.
- Messaggi d'errore specifici di Shared/RecensioniElemento.razor (lettura/salvataggio/eliminazione
  recensione, unita 10) - non provocati: avrebbero richiesto scrivere una recensione vera
  sull'elemento di collaudo esistente e poi farla fallire offline. Ho invece verificato lo stesso
  meccanismo di traduzione degli errori su tre altri file (Spese, Spazi, Collezione), con esito
  positivo su tutti e tre.
- Messaggio OAuth di Benvenuto.razor (unita 15, scambio codice PKCE) - richiede la disconnessione e
  la manipolazione del flusso Google/PKCE a mano; stesso limite del punto OAuth sopra.
