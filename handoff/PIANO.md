# Piano — correggere in sequenza i quindici rilievi della ricognizione

Riscritto il **3 settembre 2026** all'apertura del lavoro. Sostituisce il piano del
26-27 agosto, i cui due lavori sono chiusi e sul remoto (`3cb5924`, `dc4ca55`); ciò che di
quel piano resta vincolante è conservato in fondo a questo file.
Autosufficiente: chi riprende non ha bisogno della conversazione da cui nasce.

## OBIETTIVO

> «io vorrei correggere tutto in sequenza nel prossimo lavoro.»
> — utente, 3 settembre 2026

«Tutto» = i sedici rilievi di `handoff/01-ricognizione-ui/rilievi.md` (0-15) più le tre
pendenze minori elencate in fondo a quel file. Nessuna esclusione dichiarata.

## STATO DI PARTENZA

`main` pulito, **0 avanti / 0 indietro** rispetto a `origin/main`, fino a `dc4ca55`.
Porta 5000 libera, nessun server di sviluppo vivo. I PID annotati in
`handoff/01-ricognizione-ui/server.md` appartengono a una sessione chiusa il 27 agosto:
sono storia, non processi da fermare.

## DECISIONI

*Append-only, datate. L'utente può appendere qui una riga in qualunque momento: ha lo
stesso peso di una detta in chat. Rileggere questo campo prima di ogni PROSSIMA AZIONE.*

- **3 set 2026** — Si corregge tutto, in sequenza. Deciso dall'utente.
- **3 set 2026** — Rilievo 0, posizione di `tech-advisor` (confidenza alta): **solo la
  migrazione SQL**. La toppa C# (`ignoreOnInsert` su `Blind`) è peggio del problema —
  sposterebbe il difetto da «non si crea» a «si crea ignorando in silenzio l'interruttore
  Voto al buio, che il modulo di creazione mostra attivo» — e A+B è contraddittoria.
  `Models/Collection.cs` **non si tocca**: `ignoreOnInsert` e il grant sono complementari,
  non ridondanti.
- **3 set 2026** — Rilievo 0, **confermato dall'utente**: migrazione SQL **più** il test
  statico in `Eton.Tests`. **La migrazione in produzione la esegue l'utente**: nessun agente
  tocca il database vero.
- **3 set 2026** — Rilievo 10, **deciso dall'utente**: tavolozza fissa di 16-24 emoji
  accanto al campo di testo, che **resta** come via d'uscita. Le prime tre coincidono con
  le emoji dei modelli predefiniti (🧪 🍺 🎬, `SchemaCampi.cs:231,239,247`). Niente
  componente nuovo: un `static readonly string[]` in `@code` di `CollectionEdit.razor`,
  un solo call-site. Scartate le SVG di `Shared/Icona.razor`: sono otto icone di
  interfaccia, nessuna è un soggetto da collezione.
- **3 set 2026** — Rilievo 7, **deciso dall'utente**: si **nasconde il link** «Gestisci
  questo spazio» in Home quando lo spazio è personale. Nessuna funzione nuova. La rinomina
  dello spazio personale il database la permetterebbe, ma è esclusa dall'interfaccia di
  proposito e resta esclusa.
- **3 set 2026** — Rilievo 15, **deciso dall'utente**: **non è un difetto, non si corregge.**
  Si aggiorna il testo del rilievo in `rilievi.md` spiegando perché il caso è chiuso, così
  nessuno lo riapre fra sei mesi credendolo in sospeso. L'obiettivo scende a **15 rilievi
  su 16**, dichiarato qui. Motivo: con `forceLoad: true` gli esiti sono due, entrambi
  onesti — o l'utente è davvero uscito, o `Benvenuto.razor:213` lo rimanda alla Home. Lo
  schermo che il rilievo descrive non può prodursi.
- **3 set 2026** — Rilievo 4, adottata la posizione di `tech-advisor` senza domanda perché
  smonta la premessa del rilievo invece di scegliere fra alternative: «Più tardi» =
  `banner.hidden = true` e nient'altro, memoria **solo in RAM**. Nessun `sessionStorage`,
  nessun `localStorage`, nessun timer. `index.html:108` ripropone già il banner a ogni
  avvio finché il worker è in attesa, e `:116` all'arrivo di una versione più nuova: la
  differenza fra una X e un «Più tardi» è l'etichetta, non il meccanismo.
- **3 set 2026** — **La migrazione è stata eseguita in produzione dall'utente.** Il vincolo
  di collaudo cade: `/collections/{id}`, `/collections/{id}/edit` e
  `/collections/{id}/items/{id}` tornano raggiungibili, e le unità 04, 05, 09 e 10 sono di
  nuovo collaudabili nel browser. **Non ancora verificato dal vivo**: la prima cosa che
  `live-testing` dovrà fare, al primo giro utile, è creare una collezione e vedere che il
  42501 non compare più. Finché non è visto, resta un fatto riferito, non misurato.
- **3 set 2026** — Rilievi 1, 2, 12: la **guardia di uscita** diventa una classe base
  `Shared/PaginaEditor.cs : ComponentBase`, la stessa forma di `PaginaRegistro.cs`.
  Testata (12) ed esito (2) **non** entrano nell'astrazione: sono quattro applicazioni di
  `TestataPagina` già esistente e uno spostamento di markup. Premessa corretta da
  `tech-advisor` contro la doc .NET 10 e il sorgente di `NavigationLock`:
  `OnBeforeInternalNavigation` **copre anche il tasto Indietro** del browser;
  `ConfirmExternalNavigation` serve solo per chiusura scheda, ricarica e link esterni.

## IL QUARTO ANELLO DEL RILIEVO 0

Trovato da `tech-advisor` il 3 settembre, non era nella ricognizione, e spiega perché il
difetto è sopravvissuto due settimane senza che nulla diventasse rosso.

`supabase/verifica-rls-voto-al-buio.sql` (righe 53-55 e 63-65) inserisce le collezioni con
`(space_id, owner_id, name)` — **senza `blind`** — e accende il flag con un UPDATE
separato. Ha collaudato per intero un percorso che l'applicazione non usa. E
`verifica-rls-collezioni.sql:53-61` verifica i privilegi di INSERT contro un **elenco
scritto a mano** precedente a `blind`.

Conseguenza operativa, da non dimenticare: **i due script di verifica vanno corretti**
perché inseriscano come inserisce l'app. Restano comunque un controllo manuale con Docker,
cioè il meccanismo che questo difetto ha già superato una volta.

## PARTIZIONE

Definitiva. La numerazione parte da **02** perché `handoff/01-ricognizione-ui/` esiste già:
i numeri di cartella non si riusano.

| # | Unità | Perimetro | Dipende da | Stato |
|---|---|---|---|---|
| 02 | Privilegio INSERT collezioni | nuova migrazione `supabase/migrations/20260903000000_grant_insert_blind.sql`; `supabase/verifica-rls-collezioni.sql`; `supabase/verifica-rls-voto-al-buio.sql`; `Eton.Tests/PrivilegiInsertTests.cs` | — | **FATTO** — commit `8a1d438`. Gate riverificato dal capo: 267/267, 0 avvisi |
| 03 | Il contratto degli editor, **e il suo primo consumatore** | `Shared/PaginaEditor.cs` (nuovo), `Pages/NoteEdit.razor` | — | PIANIFICATA |
| 04 | Editor Collezione | `Pages/CollectionEdit.razor`, `Services/CollectionRepository.cs` | 03 | PIANIFICATA |
| 05 | Editor Elemento | `Pages/ItemEdit.razor` | 03 | PIANIFICATA |
| 06 | Editor Spesa | `Pages/SpesaEdit.razor` | 03 | PIANIFICATA |
| 07 | Home, spazio, profilo | `Pages/Home.razor`, `Pages/SpaceDetail.razor`, `Pages/Profile.razor` | 03 | PIANIFICATA |
| 08 | Conferma e registri vuoti | `Shared/ConfermaAzione.razor`, `Pages/Notes.razor`, `Pages/Collections.razor` | — | PIANIFICATA |
| 09 | Recensioni | `Shared/RecensioniElemento.razor` | — | PIANIFICATA |
| 10 | Foglio di stile e banner PWA | `wwwroot/css/app.css`, `wwwroot/index.html` | — | PIANIFICATA |

Copertura dei rilievi: 02→r0 · 03→il contratto, più r1, r2, r12 e P1 su NoteEdit ·
04→r1, r2, r12 su CollectionEdit, più r3, r9, r10 · 05 e 06→r1, r2, r12 sui rispettivi
editor · 07→r7, r11, r12 in parte e P2 in parte · 08→r8, r13 · 09→P2 in parte ·
10→r4, r5, r6, r14, P3.

**Perché la 03 porta anche `NoteEdit.razor`, cambiato il 3 set rispetto alla prima
stesura.** Un'astrazione con zero call-site non è verificabile: compila, i test passano, e
che il contratto non si incastri lo scopre il primo consumatore, quando correggerlo tocca
due unità già chiuse. Il precedente della casa lo conferma — `PaginaRegistro.cs` è nato
**estraendo** da tre copie esistenti, non in astratto. Se il contratto è sbagliato, ora si
scopre dentro l'unità che lo produce.
**r15 non è assegnato a nessuna unità**: deciso di non correggerlo (vedi DECISIONI). Il suo
testo in `rilievi.md` lo aggiorna il capo, non un'unità: è un documento, non codice.

`Services/AuthStateService.cs` **esce dal perimetro dell'unità 08** rispetto alla bozza
precedente: era lì solo per r15.

## RAZIONALE

**Perché un'unità 02 che nessun rilievo nomina.** I rilievi 1, 12 e (per costruzione) 2
sono lo stesso intervento applicato a quattro file. Quattro unità indipendenti
produrrebbero quattro guardie di uscita divergenti — esattamente il codice quadruplicato
che `Shared/PaginaRegistro.cs` ha appena finito di smontare in questo progetto. Un'unità
produce il contratto, tre lo consumano.

**File contesi, e a chi vanno.**

- `wwwroot/css/app.css` è **l'unico** foglio di stile del progetto: non esistono
  `.razor.css` per le pagine coinvolte. Lo possiede l'unità 10 e nessun altro. Le unità
  03-06 e 08, se hanno bisogno di stile nuovo, usano classi già esistenti (per esempio
  `.errore-campo`, già in `app.css:1876`) o stile inline. **Un'unità che scopre di aver
  bisogno di `app.css` torna `BLOCKED`, non lo modifica**: va serializzata dopo la 10.
- `Shared/TestataPagina.razor` lo **consumano** cinque unità con l'API esistente
  (`Titolo` / `Aiuto` / `Azione`) e non lo modifica nessuno. Se una scopre di aver bisogno
  di un'opzione nuova, è un'eccezione che torna al capo — non si risolve nell'unità.
- Le sei stringhe «Il database ha rifiutato…» vivono in due file di proprietà diversa
  (`SpaceDetail.razor` all'unità 07, `RecensioniElemento.razor` alla 09). Vanno tradotte
  **allo stesso modo**: è un contratto, e l'omologo già corretto da imitare è
  `NoteEdit.razor:278`.

**Perché l'unità 01 non blocca le altre.** Nessun altro gruppo tocca le migrazioni o
`Models/Collection.cs`. Blocca però il **collaudo** di tre cose, ed è un vincolo diverso
dall'ordine di implementazione: `/collections/{id}/items/{id}` non è raggiungibile senza
una collezione, quindi le unità 05 e 09 si scrivono ma non si provano nel browser; e il
medaglione dell'unità 10 (P3) compare solo con almeno una collezione in elenco.

**Trappola per l'unità 04.** Dopo che la 01 chiude il 42501, l'errore del rilievo 3 non
sarà più riproducibile da `/collections/new`: per collaudare la traduzione del messaggio
serve innescare un'altra eccezione Postgrest.

## CONTRATTO — `Shared/PaginaEditor.cs`

**Firma reale, dal file su disco dopo l'unità 03** (commit dell'unità 03). Prevale sulla
bozza che il capo aveva scritto prima: divergeva in tre punti, marcati `⚠`. Questa versione
va nei mandati 04, 05 e 06.

```csharp
public abstract class PaginaEditor : ComponentBase, IDisposable   // ⚠ IDisposable in più
{
    [Inject] private NavigationManager Navigation { get; set; }   // ⚠ private, non protected
    [Inject] private IJSRuntime JS { get; set; }                  // ⚠ private, non protected

    protected abstract bool Cambiata { get; }
    protected void Esci(string uri, bool replace = false);
    protected async Task GuardaUscita(LocationChangingContext ctx);
    public virtual void Dispose();                                // ⚠ nuovo
}
```

**Cosa cambia per chi lo consuma, e va scritto nei loro mandati.**

1. **`Navigation` è `private`: le pagine derivate non la vedono.** Tutti e cinque i
   `NavigateTo` rimasti nei tre editor sono post-`Crea()` o post-`Elimina()`, quindi
   diventano `Esci(...)` e nessuno resta scoperto; la riga `@inject NavigationManager
   Navigation` va tolta. Se un'unità scoprisse di aver bisogno di navigare per altro,
   **rimette** l'`@inject` nella pagina: con la base `private` questo **non** produce
   l'avviso CS0108, mentre con `protected` l'avrebbe prodotto rompendo il gate «0 avvisi».
   È il motivo per cui `private` batte `protected` qui.
2. **`JS` è `private`**, stessa via del punto 1. Nessuno dei tre editor inietta oggi
   `IJSRuntime`.
3. **La base implementa `IDisposable`.** Nessuno dei tre lo implementa oggi: lo ereditano e
   basta. Se una unità aggiunge una propria pulizia, la forma è
   `public override void Dispose() { base.Dispose(); … }` — e **`base.Dispose()` non è
   facoltativo**: senza, la guardia contro la navigazione tardiva smette di funzionare in
   silenzio.

**Perché esistono `smontata` e `Dispose`** — trovato da `bug-hunter` nell'unità 03, non era
nel mandato. `NavigationManager` è un **singleton dell'applicazione**: se `Crea()` resta
sospeso su una chiamata di rete e l'utente intanto esce, il `NavigateTo` tardivo dirotta la
pagina che sta guardando in quel momento, e fa scattare la guardia di *quella* pagina con
una domanda fuori contesto. Il difetto **preesisteva** — il codice chiamava `NavigateTo`
grezzo con lo stesso esito — ma ora c'è un posto solo dove correggerlo per quattro pagine.
`Esci` esce subito se il componente è smontato: l'oggetto creato resta creato, si abbandona
solo la navigazione.

**Verificato con `doc-checker` contro il sorgente ASP.NET Core al tag `v10.0.10`** (la
versione pinnata in `Eton.csproj:36`), e **non va riverificato**: `ComponentFactory` cerca
le proprietà `[Inject]` con `BindingFlags.Instance | Public | NonPublic` e risale la
gerarchia livello per livello, quindi le proprietà **`private` di una classe base vengono
trovate e popolate**. `NonPublic` non distingue `private` da `protected`.

Una riga di markup per editor, dentro il ramo del modulo:

```razor
<NavigationLock ConfirmExternalNavigation="@Cambiata" OnBeforeInternalNavigation="GuardaUscita" />
```

**Perché una classe base e non un componente `<GuardiaUscita Sporca="@Cambiata" />`.** Il
componente **non funziona**, e non per ragioni di stile: dopo `Crea()` ogni editor chiama
`NavigateTo` con `Cambiata` **ancora vera**, e fra quella decisione e la navigazione non
c'è nessun render. Un `[Parameter]` porta il valore catturato all'ultimo render, quindi la
guardia chiederebbe «hai modifiche non salvate» **subito dopo un salvataggio riuscito**.
Una classe base legge i campi vivi nell'istante dell'handler.

**Due trappole per i mandati 04-07.** (1) I quattro editor hanno già
`@inject NavigationManager Navigation`: con la base che lo dichiara `[Inject] protected`
quella riga va **tolta**, come fa `PaginaRegistro` con `Spazi`. (2) I `NavigateTo` da
sostituire con `Esci(...)` sono quelli che seguono `Crea()` ed `Elimina()`.

## PROSSIMA AZIONE

Unità 03 **aperta** (tetto 20 $). Quando rientra: auditare `CONTRATTI` — la firma reale di
`PaginaEditor` è ciò che finirà nei mandati 04, 05 e 06 — e `SCOSTAMENTI`, poi scrivere il
mandato dell'unità 04.

**Non** lanciare `live-testing` prima che le unità 03-06 siano tutte rientrate: il
comportamento della guardia va provato una volta sola sulla forma finale.

## APERTO

~~Da fare al capo: aggiornare il testo del rilievo 15.~~ **Fatto il 3 set 2026**: la
sezione è ora «ISTRUITO E CHIUSO — non era un difetto», col ragionamento per esteso e la
condizione che lo smentirebbe. Non sparisce dall'elenco: un rilievo cancellato senza motivo
viene riaperto da qualcuno fra sei mesi.

~~Da consegnare all'utente: la migrazione da eseguire in produzione.~~ **Consegnata ed
eseguita il 3 set 2026.**

**Da proporre all'utente, fuori dall'obiettivo attuale.** La ricognizione del 27 agosto non
ha mai visto `/collections/{id}`, `/collections/{id}/edit` e `/collections/{id}/items/{id}`
— cioè **tutta la parte di voti e recensioni**, la più grande del progetto — perché erano
irraggiungibili. Ora non lo sono più. I quindici rilievi in lavorazione non le riguardano:
nessuno le ha guardate, quindi nessuno sa se hanno attriti. Non è un buco della partizione,
è un buco dell'**elenco** da cui la partizione nasce, e colmarlo non rientra in «correggere
tutto». Proporre un secondo giro di ricognizione **breve** a lavoro finito — non prima: le
unità in corso cambiano proprio le schermate che andrebbe a guardare.

**Dubbi miei, non ancora domande.**

- L'unità 05 (`CollectionEdit.razor`) raccoglie sei rilievi e ora anche la tavolozza di
  emoji: è la più grande dell'elenco, 150-250 righe stimate, e la prima candidata a essere
  ripartizionata se torna `PARZIALE`.
- La forma del GRANT (`grant insert (blind)` minimale) **devia dal precedente** di
  `voto_al_buio.sql:109`, che ripete l'elenco completo. `conformity` la segnalerà: la
  deviazione va dichiarata nel commento della migrazione, non nascosta.
- `tech-advisor` dà confidenza **media** su un punto del contratto: su iOS Safari/PWA
  `beforeunload` è notoriamente inaffidabile, quindi la chiusura dell'app sul telefono
  resta best-effort. Il gesto Indietro, che è il caso frequente, è coperto dall'handler
  interno. Non è un motivo per cambiare forma, è una cosa da non promettere.
- Dopo che l'unità 02 chiude il 42501, l'errore del rilievo 3 **non sarà più riproducibile**
  da `/collections/new`: chi collauda l'unità 05 dovrà innescare un'altra eccezione
  Postgrest per verificare la traduzione del messaggio.

## FATTI OPERATIVI CHE COSTANO CARI SE DIMENTICATI

Ereditati dal piano precedente, tutti ancora validi.

- **Riavvia il server prima di ogni prova nel browser**, e non far compilare nessuno mentre
  è vivo. Il DevServer legge i manifest degli asset solo al proprio avvio: dopo qualche
  build annuncia nomi con impronta che non esistono più, e l'app non parte. Rimedio:
  `rm -rf obj bin`, ricompila, riavvia.
- **Il server lo avvia e lo ferma l'orchestratore**, annotando porta e PID **su disco**. Su
  Windows la morte del padre non uccide i figli: `dotnet run` lascia un processo DevServer
  separato, e vanno fermati **entrambi**.
- **Gli implementer non compilano.** `obj/` non ha lock fra processi: due build concorrenti
  sullo stesso `.csproj` si corrompono a vicenda. Compila l'orchestratore, a fine giro.
- **Il testo accentato non passa per gli argomenti della shell.** `printf`, `echo -e` e
  `git commit -m` mangiano gli accenti su questo setup. Heredoc quotato, o `git commit -F`
  su file UTF-8. I file si scrivono con `Write`, che scrive UTF-8 nativamente.
- **Il Chrome giusto** ha `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`. Identifica per
  deviceId, **mai** per nome visualizzato: i nomi si scambiano a ogni riconnessione, e
  l'altro non raggiunge `localhost`.
- **Il login su `localhost` non arriva all'agente da solo.** `launchBrowser: true` in
  `Properties/launchSettings.json` fa aprire a `dotnet run` il browser predefinito di
  sistema, che è un profilo diverso da quello dell'estensione. Apri **tu** la scheda con
  `navigate`, poi chiedi all'utente di accedere in quella scheda.
- **`resize_window` non scende sotto ~526px** su questo PC. Per misurare un layout stretto,
  restringi il contenitore via JS replicando a mano le media query attive a quella
  larghezza.
- **La spesa di prova «PROVA AGENTE»**, 12,50 € del 20 agosto, **la cancella l'utente**.
  Nessun agente la tocca.
- **Lo sviluppo gira contro il database vero.** `wwwroot/appsettings.json:3` è l'unico
  appsettings e punta a `fdqedhgvpneuybtykamf.supabase.co`; non esiste
  `appsettings.Development.json`. Ogni prova nel browser scrive sui dati reali.
- **Il budget di un'unità si prezza sui giri di protocollo, non sulle righe di diff.**
  Misurato il 3 settembre: l'unità 02 — una riga di SQL e un test — ha esaurito **4 dollari**
  completando due obiettivi su tre. Il costo fisso di un'unità (brief, `implementer`,
  revisori, istruttoria, adjudica) domina quello variabile. Regola pratica ricavata:
  **≥ 12 $ per un'unità a giro singolo**, di più se i file sono molti o i revisori sono
  quattro. Un tetto troppo stretto non protegge: fa perdere il lavoro non ancora scritto su
  disco, e il resoconto è la prima cosa che salta.

## COSA NON VA RI-VERIFICATO

- **La scena del grafo** (`3cb5924`): collaudata su sette scenari, 181 fps, zero difetti.
  Le tre cose non tarate — dissolvenza dell'occhiello, ultimo 8% della corsa fermo, onda al
  clic sottile — l'utente ha deciso di **non** toccarle.
- **La misura di `thewatch.60fps.fr`**: nessuna libreria di animazione, scroll nativo non
  addolcito, Three.js + WebGL2, 11,77 MB di cui 8,59 per il modello, zero
  `animation-timeline`. Le sue transizioni CSS sono più semplici di quelle che Eton ha già.
- **La testata a 360px**: +8,5px di margine sui 328 di area utile nel caso peggiore.
- **Il disarmo della conferma di eliminazione**: verificato contro la documentazione di
  .NET 10 e il sorgente del renderer.
- **Il banner PWA che riappare in sviluppo dopo il clic** non è un difetto:
  `service-worker.js` è no-op e non ha listener `message`, quindi lo SKIP_WAITING cade nel
  vuoto; il worker pubblicato il listener ce l'ha (`service-worker.published.js:15-16`).
  Chi collauda l'unità 10 non ci perda un'ora.

## VINCOLI EREDITATI DALLA SCENA DEL GRAFO — lavoro chiuso, non riaprire

Conservati dal piano precedente perché documentano **perché** la scena è fatta così, e
restano vincolanti per chiunque ci rimetta mano. L'idea di partenza era un canvas
`position: fixed` per tutta la vetrina, come fa il sito di riferimento. Fallisce in tre
modi, tutti verificabili nel codice:

1. **Il grafo è fatto di luce.** `grafo-spazio.js:230` disegna con
   `globalCompositeOperation = "lighter"`: i colori si sommano al fondo invece di coprirlo.
   Funziona su nero pieno; su fondo chiaro satura verso il bianco e l'oggetto sparisce. E
   la vetrina ha una `<section class="spazi chiara">` a fondo chiaro **per scelta
   documentata** (`Benvenuto.razor:92-95`).
2. **Il grafo è l'unica cosa toccabile della vetrina**, e ha `pointermove` / `pointerdown`.
   Un canvas fisso a piena pagina o sta sotto il contenuto con `pointer-events: none` — e
   perde ciò che lo rende interessante — o sta sopra, e intercetta i click sul pulsante
   «Entra con Google». Non c'è una terza configurazione.
3. **Il costo per fotogramma.** Oggi il canvas è grande quanto la sua sezione e si spegne
   fuori vista. A piena pagina significa full-viewport a `devicePixelRatio` 2, con una
   `createRadialGradient` per ogni alone a ogni frame, su **CPU** — è Canvas 2D, non il
   WebGL2 del riferimento.

Vincoli tecnici che restano attivi:

- **Nessuna dipendenza nuova.** `grafo-spazio.js:9-11` lo dichiara per iscritto: il sito sta
  su GitHub Pages e deve funzionare offline come PWA. Vale per tutto il progetto, e quindi
  anche per il selettore di icona del rilievo 10.
- **Non aggiungere altro `animation-timeline`.** Quello che c'è, incapsulato in `@supports`
  con stato di partenza visibile, è il pattern giusto per decorazione non critica.
- **`overflow: hidden` su un wrapper alto romperebbe `sticky`.** La sezione della scena non
  deve averlo.
- L'`IntersectionObserver` che sospende il ciclo fuori vista **resta valido** e va tenuto.
