UNITÀ: 05/11 — L'editor delle collezioni smette di parlare come un database

## OBIETTIVO

Quattro risultati osservabili su `/collections/new` e `/collections/{id}/edit`.

1. **Il messaggio d'errore è in italiano, non in JSON.** Oggi, quando il salvataggio
   fallisce, il riquadro rosso mostra testualmente questo:

   ```
   Non è stato possibile salvare: {"code":"42501","details":null,"hint":"Grant the required
   privileges to the current role with: GRANT INSERT ON public.collections TO
   authenticated;","message":"permission denied for table collections"}
   ```

   Espone all'utente finale il codice SQLSTATE, il nome della tabella e **un'istruzione
   GRANT rivolta a un amministratore di database**. Non dice cosa è andato storto in termini
   comprensibili, non dice se riprovare, non dice se il lavoro è perso.

2. **Il pulsante «Salva» spento dice cosa manca.** Oggi è `disabled` ma reso con
   `opacity: 0.5` su fondo blu pieno: su nero resta saturo e legge come premibile. Nessun
   messaggio, `cursor: default`. Che manchi il nome si scopre per tentativi.

3. **L'icona si sceglie, invece di digitarla a memoria.** Oggi il campo icona è un
   `<input type="text">` con `maxlength: 16` in cui si è attesi digitare un'emoji a mano: da
   desktop bisogna conoscere `Win + .`, e ci si può scrivere qualunque cosa.

4. **L'esito del salvataggio si trova in un posto solo.** L'unità 04 ha spostato il blocco
   `errore`/`avviso` sopra i pulsanti, ma gli **errori di validazione** compaiono ancora
   dentro la scheda «Campi», in cima a essa. Dopo aver premuto lo stesso pulsante, l'utente
   deve cercare l'esito in due punti diversi — e il vincolo SQL ammette fino a 40 campi,
   quindi con molti campi l'errore di validazione torna fuori vista: cioè il difetto che
   l'unità 04 stava correggendo. Sollevato dall'unità 04 come `FUORI SCOPE` perché il suo
   mandato glielo vietava; è tuo.

## PERIMETRO — file di tua proprietà esclusiva

- `Pages/CollectionEdit.razor`
- `Services/CollectionRepository.cs`

## NON TOCCARE

- **`Shared/PaginaEditor.cs`**, il contratto degli editor. Se scopri che va cambiato è un
  `BLOCKED`: due unità dopo di te lo adotteranno.
- **Ciò che l'unità 04 ha appena fatto su questa pagina**: `@inherits PaginaEditor`, la riga
  di `<NavigationLock>` dentro il ramo del modulo, i due `Esci(...)`, la `<TestataPagina>`,
  il gate di «Chiudi» (`href="@(occupato ? null : "collections")"`), e la posizione del
  blocco `errore`/`avviso` sopra i pulsanti. È committato e verificato. Se un revisore
  propone di spostare `<NavigationLock>` fuori dai rami condizionali, **respingilo**: nel
  ramo in cui la collezione è sparita lo stato è ancora «sporco» e la guardia chiederebbe
  «hai modifiche non salvate» su una collezione che non esiste più.
- **`Pages/NoteEdit.razor`, `Pages/ItemEdit.razor`, `Pages/SpesaEdit.razor`**: di altre
  unità.
- **`Pages/SpaceDetail.razor` e `Shared/RecensioniElemento.razor`**, dove vivono le sei
  stringhe «Il database ha rifiutato…»: sono delle unità 08 e 10. Vedi `CONTRATTI`.
- **`wwwroot/css/app.css`.** Appartiene all'unità 11. Usa classi esistenti o stile inline;
  se non basta, torna `BLOCKED`.
- `Shared/TestataPagina.razor`, `Shared/Icona.razor`: li consumi, non li modifichi.

## CONTRATTI

**La traduzione dei messaggi d'errore è materia condivisa, e tu sei il primo a toccarla.**
Le stesse sei stringhe «Il database ha rifiutato…» vivono in `SpaceDetail.razor` e
`RecensioniElemento.razor`, che sono delle unità 08 e 10: quelle unità tradurranno **allo
stesso modo**, e leggeranno il tuo resoconto per sapere come.

**L'omologo già corretto da imitare è `Pages/NoteEdit.razor:278`.** Guardalo prima di
scrivere qualsiasi cosa: dice la causa invece del meccanismo, e dice cosa fare. È il metro.

Due strade, e la scelta è tua perché dipende da cosa trovi nel codice:

- se la traduzione risulta **inline nella pagina**, come in `NoteEdit`, fai uguale e non
  astrarre niente;
- se ti accorgi che serve un helper condiviso perché tre file lo vorrebbero, **non
  crearlo**: torna `BLOCKED` con la firma che proponi. Un helper in `Shared/` è un contratto
  fra unità, e crearlo dentro la tua senza dichiararlo lascerebbe due unità future a
  scoprirlo da sole.

In ogni caso il resoconto deve contenere, in `CONTRATTI`, **la forma esatta del messaggio
tradotto** — testo verbatim e criterio con cui hai deciso cosa dire — perché le unità 08 e
10 lo copino invece di inventarne un terzo.

**Una difficoltà nota, dichiarata in anticipo.** Il 42501 su cui il rilievo è stato
osservato **non è più riproducibile**: la migrazione dell'unità 02 è stata eseguita in
produzione, e creare una collezione ora funziona. Per provare la traduzione devi innescare
un'altra eccezione Postgrest — un vincolo di lunghezza, una forma di `fields` non valida, o
la rete disattivata. Non è un ostacolo al lavoro, è un ostacolo alla *prova*: dichiara nel
resoconto come l'hai innescata, o che non ci sei riuscito.

## LE DECISIONI GIÀ PRESE DALL'UTENTE — non riaprirle

**Il selettore di icona** (obiettivo 3). Deciso: **tavolozza fissa di 16-24 emoji accanto al
campo di testo, che resta** come via d'uscita per chi ne vuole una fuori elenco.

- Le **prime tre coincidono** con le emoji dei tre modelli predefiniti — 🧪 🍺 🎬, che stanno
  in `Services/SchemaCampi.cs` — così tavolozza e modelli non si contraddicono.
- Un `static readonly string[]` nel blocco `@code` della pagina. **Niente componente nuovo**:
  un solo call-site.
- I pulsanti seguono il pattern delle pastiglie già usate altrove nel progetto, con
  `aria-pressed` su quella scelta. Cerca l'omologo invece di inventare il markup.
- **Scartate** le icone SVG di `Shared/Icona.razor`: sono otto icone di interfaccia (casa,
  note, persona, spese…), nessuna è un soggetto da collezione, e adottarle significherebbe
  cambiare il modello dati, i tre modelli predefiniti e ogni punto di rendering. Se un
  revisore lo propone, respingilo citando questo paragrafo.
- Vincolo del progetto, non negoziabile: **nessuna dipendenza nuova**. Il sito sta su GitHub
  Pages e deve funzionare offline come PWA.

## BUDGET DI COMPLESSITÀ

Nessun componente nuovo. Nessun servizio nuovo. Nessun pacchetto. Al massimo **un** tipo
nuovo, e solo se la traduzione degli errori lo richiede davvero — in tal caso è un
`BLOCKED`, non una decisione tua. Un helper con un solo call-site va inline.

Il pulsante spento (obiettivo 2) è il punto in cui è più facile esagerare: la risposta
minima è dire cosa manca, non costruire un sistema di validazione. Guarda se esiste già una
classe per gli errori di campo prima di aggiungerne una.

## STATO

Unità precedenti, tutte `FATTO` e committate:

- **02** — `handoff/02-collezioni-insert/resoconto.md`. La migrazione **è stata eseguita in
  produzione**: le collezioni si creano di nuovo.
- **03** — `handoff/03-contratto-editor/resoconto.md`. Il contratto degli editor.
- **04** — `handoff/04-collezione-contratto/resoconto.md`. **Leggilo**: ha lavorato sul tuo
  stesso file poche ore fa, e la sua sezione `FUORI SCOPE` è il tuo obiettivo 4. Contiene
  anche l'elenco delle prove da fare nel browser, a cui aggiungerai le tue.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: se ci trovi una riga che contraddice
questo mandato, vince la più recente.

**Un fatto operativo che ti risparmia un errore**: le `file:line` di `threat-hunter` sono
risultate sfasate sul diff dell'unità 04 (dava riga 68 per qualcosa che stava a riga 201).
I suoi verdetti reggevano; i suoi numeri no. Accogli i verdetti se reggono per contenuto,
ma non riportare un suo numero di riga senza averlo riaperto.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**. Usa `-warnaserror` come ha fatto
  l'unità 04: è più severo del minimo e intercetta ciò che una build permissiva declassa.
- `dotnet test` → tutti verdi. Erano **267** all'ultimo giro.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai.

**Non avviare il server di sviluppo e non provare nel browser.** Lo fa il capo con
`live-testing` quando gli editor sono tutti rientrati.

BUDGET: 22 dollari

RESOCONTO IN: `handoff/05-collezione-rilievi/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 05 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <la forma esatta del messaggio d'errore tradotto, verbatim, e il criterio con
            cui hai deciso cosa dire. Le unità 08 e 10 lo copieranno: se qui è vago,
            inventeranno un terzo modo di dire la stessa cosa>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

Aggiungi `DA PROVARE NEL BROWSER` con i criteri di accettazione, e dichiara **come hai
innescato l'errore Postgrest** per provare la traduzione, o che non ci sei riuscito. Un
limite dichiarato vale più di una prova data per fatta.
