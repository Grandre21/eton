# Il prompt della sessione di chiusura

Scritto dal capo il **4 settembre 2026**, mentre l'unità 16 girava — cioè nel momento in cui
`~/.claude/architettura-sessioni.md` era ancora nel suo contesto. Scriverlo dopo il collaudo
avrebbe voluto dire riscriverlo a memoria.

**Non si apre questa sessione prima che il collaudo sia finito.** L'ordine è: unità 16 →
quattro giri di `live-testing` → chiusura. Una chiusura che gira prima del browser dichiara
«copertura completa» su un lavoro di cui nessuno ha ancora visto l'effetto.

**Prima di aprirla, il capo riempie il campo `ESITO DEL COLLAUDO` in fondo a questo file.**
È l'unica parte che non si può scrivere in anticipo, ed è quella che dice alla chiusura se
sta chiudendo un lavoro riuscito o uno con difetti noti.

---

## Il prompt da passare (`claude -p`, ASCII puro nell'argomento)

> Sei la **sessione di chiusura** del lavoro sui sedici rilievi della ricognizione UI di
> Eton. Non sei un quarto revisore e non sei un riassuntore.
>
> **Leggi per primo `~/.claude/architettura-sessioni.md`**, sezione «La sessione di
> chiusura»: contiene il tuo mandato completo e il formato del rapporto, che non conosci
> altrimenti. Poi leggi `handoff/CHIUSURA.md`, che è questo file, e `handoff/PIANO.md`.
>
> L'obiettivo dell'utente sta **verbatim** in `PIANO.md`, campo `OBIETTIVO`. Confrontalo con
> l'unione di ciò che i resoconti in `handoff/NN-*/resoconto.md` dichiarano, e **elenca ciò
> che nessuno ha fatto e nessuno ha detto di non aver fatto**.
>
> Il resoconto in `handoff/16-oauth-insieme-chiuso/resoconto.md` è l'ultimo del lavoro.
>
> **Non committare i tuoi commit di correzione senza dirlo nel rapporto.** L'utente non è
> raggiungibile: qualunque domanda va nel rapporto, non in chat.

## Le tre verifiche, e cosa sono in questo lavoro specifico

### 1. Copertura dell'obiettivo

L'obiettivo era «correggere tutto in sequenza». «Tutto» sono i sedici rilievi di
`handoff/01-ricognizione-ui/rilievi.md` (0-15) **più le tre pendenze minori in fondo a quel
file** — e sono le pendenze il punto cieco più probabile, perché nessuna tabella di questo
piano le ha mai avute come riga propria.

La `MAPPA RILIEVO → UNITÀ` di `PIANO.md` dice chi possiede cosa. **Confrontala con gli
`ESITO` reali dei resoconti**: una divergenza fra la colonna di stato e il disco è un difetto
rilevabile senza giudizio, e dice che il capo ha smesso di tenere il piano al passo.

Due cose sono chiuse **per decisione, non per esecuzione**, e non vanno segnalate come buchi:

- il **rilievo 15** (logout non riuscito) è stato **ritirato**: non era un difetto;
- l'**ottava voce dell'unità 11** (rinomina `.scelta-categoria` → `.scelta-pastiglie`) non si
  fa. Entrambe le motivazioni stanno in `DECISIONI`.

Il **rilievo 3** merita un occhio in più: è stato creduto chiuso **tre volte** prima di
esserlo davvero, e ogni volta la ragione era la stessa — una mappa *file → unità* dichiara un
rilievo chiuso quando è chiuso nel perimetro di chi l'ha toccato. L'unità 15 l'ha chiuso con
una prova (`grep` dei sink a zero righe, non dei sorgenti). **Rifai quel `grep`**: il modo di
verificarlo sta nel suo resoconto, e se qualche unità successiva ha reintrodotto un
`ex.Message` a schermo, questo è l'unico anello che può accorgersene.

### 2. Convergenza dei contratti

Due contratti hanno attraversato più unità, e vanno verificati **aprendo il codice**, non
leggendo i resoconti:

- **`Shared/PaginaEditor.cs`** — prodotto dall'unità 03, adottato da 04 (`CollectionEdit`),
  06 (`ItemEdit`), 07 (`SpesaEdit`) e dalla 03 stessa (`NoteEdit`). Quattro consumatori: la
  firma reale su cui sono atterrati deve essere **la stessa**. Il contratto testuale sta in
  `PIANO.md`, sezione `CONTRATTO — Shared/PaginaEditor.cs`.
- **`Shared/PaginaRegistro.cs`** — preesistente, ma l'unità 14 l'ha toccato mentre 09 e 13
  toccavano i suoi consumatori.

Il terzo è nuovo e ha un solo consumatore, quindi è meno esposto ma più delicato:
l'**`enum`** di `Services/OAuthCallback.cs` prodotto dall'unità 16 e consumato da
`Services/SupabaseService.cs`. Qui la verifica non è «convergono», è **«il tipo tiene»**:
controlla che nessun percorso riporti la stringa `error_description` verso una schermata.

### 3. `bug-hunter` sul diff combinato

Il diff da dargli è `git diff dc4ca55..HEAD` — `dc4ca55` è l'ultimo commit prima che questo
lavoro cominciasse. È grande: **passaglielo per file, non in blocco**, e dagli in più i
**call-site di cucitura**, cioè i punti in cui il lavoro di due unità diverse si tocca:

- le quattro pagine editor che consumano `PaginaEditor`;
- `Pages/Notes.razor` e `Pages/Collections.razor`, che consumano `PaginaRegistro` toccato
  dalla 14;
- `Shared/ConfermaAzione.razor` e i suoi quattro call-site, dove l'unità 11 ha aggiunto una
  classe che il foglio di stile usa;
- `Services/SupabaseService.cs`, toccato dalle unità 15 e 16 a poche righe di distanza.

Poi rieseguì i gate del progetto sullo stato finale: `dotnet build -warnaserror` (0 avvisi) e
`dotnet test`. **Erano 273 test prima dell'unità 16**, che ne aggiunge e ne riscrive quattro:
il numero atteso lo dichiara il suo resoconto.

## Il confine: cosa correggi e cosa no

**Correggi**, dispacciando `implementer` col protocollo normale, tutto ciò che è interno a una
sola unità. Lasciare fino al mattino un difetto già noto butta via le ore che questa
architettura serviva a guadagnare.

**Non correggi, e torna al capo**, tutto ciò che tocca un contratto fra unità: se
`PaginaEditor` è divergente su una delle quattro pagine, la decisione è di chi ha scritto il
contratto, non tua.

## L'archiviazione

Solo le unità con esito **`FATTO`**, secondo l'eccezione dichiarata nel `CLAUDE.md` globale:
si spostano in `storico/handoff/NN-slug/` con `git mv`, **senza chiedere conferma**, perché il
giudizio «superato» l'utente l'ha già dato in anticipo per i documenti di `handoff/`.

Restano vivi in `handoff/`:

- le unità `PARZIALE` o `BLOCKED`, se ce ne sono;
- **`PIANO.md`**, che non è un'unità;
- **`01-ricognizione-ui/`**, che è la fonte dei rilievi e non un'unità di lavoro;
- **il tuo rapporto**, che non si archivia finché l'utente non l'ha letto.

## Cosa va nel rapporto finale, oltre al formato

Il formato sta in `~/.claude/architettura-sessioni.md`. In più, **tre cose che il capo deve
consegnare all'utente e che nessun resoconto contiene per intero** — raccoglile dai resoconti
e mettile in fondo, sotto un titolo `DA PORTARE ALL'UTENTE`:

1. **La migrazione SQL delle spese ricorrenti** (task 2 del piano in
   `docs/superpowers/plans/2026-09-03-spese-ricorrenti.md`): **nessun agente la esegue**, la
   esegue l'utente in produzione. Non è ancora scritta — è lavoro del filone successivo — ma
   il vincolo va ricordato ora perché è l'unica cosa in tutto il progetto che si ferma su una
   persona.
2. **`profiles.display_name` e `avatar_url` sono congelati al primo accesso**: chi cambia nome
   o foto su Google non lo vede riflesso. Emerso durante il lavoro, fuori dal perimetro di
   ogni unità.
3. **La proposta di ricognire `/collections/{id}` e tutta l'area voto/recensioni**: la
   ricognizione del 27 agosto non le ha mai viste, quindi i sedici rilievi non le coprono.
   L'unità 09 ha trovato lì un orfano (`CollectionDetail.razor`) senza cercarlo.

---

## ESITO DEL COLLAUDO

*Lo riempie il capo prima di aprire questa sessione, con una riga per giro. Finché c'è questa
frase, il collaudo non è stato fatto e la sessione di chiusura non va aperta.*

- **Giro A — il bloccante** (creare una collezione): *da riempire*
- **Giro B — il contratto degli editor**: *da riempire*
- **Giro C — testo e messaggi**: *da riempire*
- **Giro D — le misure**: *da riempire*
