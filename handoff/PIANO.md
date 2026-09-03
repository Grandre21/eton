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
- **3 set 2026, sera** — **L'utente si è allontanato** dopo aver scritto: «procedi in autonomia
  fino a completamento, non guardo più il pc». Da qui in avanti **non si fanno domande**: le
  scelte che sarebbero andate a lui si prendono e si dichiarano in questo campo, col motivo.
  L'unica eccezione resta l'**SQL di produzione**, che nessun agente esegue: le migrazioni si
  scrivono, si committano, e si consegnano nel rapporto finale.
- **3 set 2026, sera** — **Unità 12, decisa da me in autonomia.** L'unità 07 ha trovato un
  difetto **funzionale bloccante** e preesistente: una spesa da 1.000 € in su non è modificabile
  da nessuno, perché `Denaro.Testo` produce `"1.284,50"` e `Denaro.Verifica` rifiuta le stringhe
  con più di un separatore (`Services/Denaro.cs:80`). Il §5 vorrebbe che un rilievo fondato fuori
  scope andasse all'utente: non c'è, e lasciare in produzione un difetto di questa classe sarebbe
  peggio che decidere. **Non è risolto di nascosto**: è scritto qui, ha un mandato proprio, e
  finisce nel rapporto di chiusura.
  Rimedio scelto — **additivo**: nasce `Denaro.TestoDigitabile`, `Verifica` non si tocca.
  `tech-advisor` (confidenza alta) concorda e aggiunge il punto che mancava: i call-site sono
  **quattro**, non tre — il quarto è `Cambiata`, e saltarlo renderebbe la pagina sempre
  «modificata». Il motivo di fondo: un campo modificabile deve contenere il valore nella
  grammatica di **input**, non in quella di visualizzazione. Allargare `Verifica` non elimina la
  classe di difetto, la sposta — «1.284,50» corretto in «1.2845,50» verrebbe rifiutato **mentre
  si digita un numero valido**.
  **Sul nome ho deciso contro `tech-advisor`**, che proponeva `TestoPerInput`: il progetto nomina
  in italiano (`Testo`, `Prova`, `Verifica`), e la coerenza linguistica è un fatto verificabile,
  non una preferenza.
- **3 set 2026, sera** — **`SpaceDetail.razor` accodato all'unità 13**, sciogliendo una
  contraddizione che ha trovato l'**unità 08**, non io: la prosa del RAZIONALE le assegnava le
  stringhe «Il database ha rifiutato…» di quel file, la MAPPA scritta poche ore prima assegnava
  il rilievo 3 a «05 + 13», il cui perimetro non lo comprendeva. L'unità ha obbedito al proprio
  mandato — il più specifico — e l'ha dichiarato invece di allargarsi da sola: era la mossa
  giusta. Il file ha **cinque** interpolazioni di `ex.Message` che l'utente legge (`:206`,
  `:236`, `:262`, `:290`, `:316`), più altre quattro che finiscono in console e non c'entrano.
- **3 set 2026, sera** — **Le unità NON committano: committa il capo.** L'unità 08 ha committato
  da sé, le unità 06, 07 e 12 no, e nessun mandato lo diceva. Il commit resta al capo perché è
  l'unico che vede il quadro e scrive nel registro del progetto. Il commit già fatto dalla 08
  (`bdd858a`) è buono e non si rifà; i mandati successivi lo dicono esplicitamente.
- **3 set 2026, sera** — **Rilievo 14 assegnato all'unità 11**, che già possiede `app.css`. Non
  era in nessun perimetro della tabella: la barra laterale è `Shared/Navigazione.razor` +
  `Shared/SelettoreSpazio.razor`, e il difetto (basi disallineate, 820 contro 838) è
  presumibilmente CSS puro. L'unità 11 prova col solo foglio di stile e ha i due file in
  perimetro **solo se non basta**.
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
| 03 | Il contratto degli editor, **e il suo primo consumatore** | `Shared/PaginaEditor.cs` (nuovo), `Pages/NoteEdit.razor` | — | **FATTO** — commit `d101fdf` |
| 04 | Collezione adotta il contratto, **più il gate di «Chiudi» su NoteEdit** | `Pages/CollectionEdit.razor`, `Pages/NoteEdit.razor` | 03 | **FATTO** — commit `3206150`. Gate riverificato dal capo |
| 05 | Collezione, i suoi tre rilievi propri **più l'esito di validazione** | `Pages/CollectionEdit.razor`, `Services/CollectionRepository.cs` | 04 | **FATTO** — commit `e139ce8`. Perimetro rifiutato a metà, e aveva ragione: v. RAZIONALE |
| 06 | Editor Elemento | `Pages/ItemEdit.razor` | 03 | **FATTO** — commit `f4f2dbd`. 0 rilievi da tre revisori, più tre verifiche proprie dell'unità |
| 07 | Editor Spesa | `Pages/SpesaEdit.razor` | 03 | **FATTO** — commit `4327598`. Quarta e ultima gemella; ha trovato il difetto che diventa l'unità 12 |
| 08 | Home, spazio, profilo | `Pages/Home.razor`, `Pages/SpaceDetail.razor`, `Pages/Profile.razor` | 03 | **FATTO** — commit `bdd858a`. Ha trovato la contraddizione fra prosa e tabella sul rilievo 3 |
| 09 | Conferma e registri vuoti | `Shared/ConfermaAzione.razor`, `Pages/Notes.razor`, `Pages/Collections.razor` | — | PIANIFICATA |
| 10 | Recensioni | `Shared/RecensioniElemento.razor` | — | PIANIFICATA |
| 11 | Foglio di stile, banner PWA **e barra laterale** | `wwwroot/css/app.css`, `wwwroot/index.html`, e **solo se il CSS non basta** `Shared/Navigazione.razor`, `Shared/SelettoreSpazio.razor` | tutte | PIANIFICATA |
| 12 | **Una spesa da mille euro in su torna modificabile** | `Services/Denaro.cs`, `Pages/SpesaEdit.razor`, `Eton.Tests/DenaroTests.cs` | 07 | **IN CORSO** — eseguita fuori numero, subito dopo la 07 |
| 13 | **I tre editor rimasti, e lo spazio, traducono l'errore e dicono cosa manca** | `Pages/NoteEdit.razor`, `Pages/ItemEdit.razor`, `Pages/SpesaEdit.razor`, **`Pages/SpaceDetail.razor`** | 05, 08, 12 | PIANIFICATA — nata dal censimento del 3 set sera |

### MAPPA RILIEVO → UNITÀ

*Aggiunta il 3 set 2026 sera. Non c'era, e la sua assenza aveva lasciato **quattro rilievi
senza proprietario** — scoperti solo censendo prima di scrivere il mandato 09. Chi tocca la
partizione aggiorna anche questa tabella.*

| Rilievo | Unità | Stato |
|---|---|---|
| 0 — creare una collezione è impossibile | 02 | **chiuso** `8a1d438` |
| 1 — il lavoro non salvato si perde | 03, 04, 06, 07 | **chiuso** su tutti e quattro gli editor |
| 2 — l'esito compare dove non guardi | 03, 04, 05, 06, 07 | **chiuso** |
| 3 — il messaggio d'errore è JSON grezzo | 05 (`CollectionEdit`) + **10** (`RecensioniElemento`) + **13** (i tre editor e `SpaceDetail`) | **parziale** |
| 4 — l'avviso di aggiornamento non si rimanda | 11 | pianificato |
| 5 — bersagli categorie alti 22px | 11 | pianificato |
| 6 — «Anteprima» fa saltare il layout di 358px | 11 | pianificato |
| 7 — «Gestisci questo spazio» non gestisce niente | 08 | pianificato |
| 8 — «Elimina» a 55px da «Chiudi» | 11 | pianificato |
| 9 — pulsante spento che non dice cosa manca | 05 (`CollectionEdit`) + **13** (gli altri) + 11 (la metà visiva) | **parziale** |
| 10 — l'icona è un campo di testo libero | 05 | **chiuso** `e139ce8` |
| 11 — «Spesa 100%» non dice di essere una categoria | 08 | pianificato |
| 12 — due schermate non si spiegano | 03-07 (editor) + 08 (`/spaces`, `/profile`) | **parziale** |
| 13 — lo stato vuoto invita all'azione lontano | 09 | pianificato |
| 14 — selettore spazio e «Profilo» accavallati | 11 | pianificato |
| 15 — logout non riuscito | — | **ritirato**, non era un difetto |
| *(fuori elenco)* spese ≥ 1.000 € non modificabili | 12 | in corso |

**Tre assegnazioni decise dal capo il 3 set sera**, perché nessuna era nel piano originale:

- **Rilievo 8 all'unità 11, non agli editor.** `.azioni` è governato da una sola regola
  (`wwwroot/css/app.css:729`) che vale per tutti e sei i blocchi delle quattro pagine:
  separare l'azione distruttiva lì la separa ovunque, in un punto solo. Se servisse una classe
  nuova sul markup, l'unità 11 torna `BLOCKED` e allora — e solo allora — il perimetro si
  estende agli editor.
- **Rilievo 6 all'unità 11**, con `Pages/NoteEdit.razor` in perimetro **solo se il CSS non
  basta**: un'anteprima che fa saltare il layout di 358px è un'altezza che non è riservata,
  non un difetto di logica.
- **Unità 13, nuova.** I rilievi 3 e 9 sono stati chiusi dall'unità 05 sul **solo**
  `CollectionEdit`, che era il suo perimetro. Le sei frasi che traducono l'errore di
  PostgreSQL e la riga che spiega perché «Salva» è spento **esistono in un posto solo**, e
  sulle altre tre pagine il JSON grezzo è ancora a schermo — l'unità 07 ne ha contate quattro
  interpolazioni di `ex.Message` sulla sola `SpesaEdit`. Perimetro: `NoteEdit`, `ItemEdit`,
  `SpesaEdit`; `CollectionEdit` **si legge come modello e non si tocca**.

**Ordine di esecuzione**, che non coincide con la numerazione: 12 → 08 → 09 → 10 → 13 → 11 →
`live-testing` → chiusura. La 13 sta **prima** della 11 perché può accodarle voci di CSS,
come hanno fatto le unità 04, 05 e 06.

**`CollectionEdit.razor` spezzato in due unità sequenziali, deciso il 3 set 2026.** Il piano
segnalava già la 04 come «la più grande dell'elenco e la prima candidata a essere
ripartizionata se torna `PARZIALE`»: ripartizionarla **prima** costa un giro in più e
risparmia di scoprirlo dopo. Il taglio non è per dimensione ma per **tema**: la 04 fa
l'adozione del contratto, identica per forma alle unità 06 e 07 sugli altri due editor; la
05 fa i tre rilievi che appartengono solo a questa pagina. Due unità sullo stesso file non
si contendono nulla perché sono sequenziali, e la seconda trova la prima già committata.

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
  `.razor.css` per le pagine coinvolte. **Lo possiede l'unità 11** e nessun altro — *«unità 10»
  qui era un residuo della numerazione precedente alla scissione dell'unità 04, corretto il 3
  set sera; la tabella della PARTIZIONE è sempre stata la fonte giusta.* Le altre unità, se
  hanno bisogno di stile nuovo, usano classi già esistenti (per esempio `.errore-campo`, già in
  `app.css:1876`) o stile inline. **Un'unità che scopre di aver bisogno di `app.css` torna
  `BLOCKED`, non lo modifica**: la voce si accoda a quelle in attesa per l'unità 11.
- `Shared/TestataPagina.razor` lo **consumano** cinque unità con l'API esistente
  (`Titolo` / `Aiuto` / `Azione`) e non lo modifica nessuno. Se una scopre di aver bisogno
  di un'opzione nuova, è un'eccezione che torna al capo — non si risolve nell'unità.
- Le sei stringhe «Il database ha rifiutato…» vivono in due file di proprietà diversa:
  `SpaceDetail.razor` all'unità **08**, `RecensioniElemento.razor` all'unità **10**. Vanno
  tradotte **allo stesso modo**: è un contratto, e il modello ora non è più una descrizione
  ma **codice da aprire** — le sei frasi che l'unità 05 ha scritto in
  `Pages/CollectionEdit.razor`, elencate verbatim nel suo resoconto, sezione `CONTRATTI`.

  *(Questi due numeri erano sbagliati fino al 3 set: dicevano 07 e 09, residuo della
  numerazione precedente alla scissione dell'unità 04. Segnalato dall'unità 05 nel suo
  `FUORI SCOPE`, punto 4. La **tabella della PARTIZIONE** è sempre stata la fonte giusta, e
  resta la fonte in caso di dubbio: se una prosa la contraddice, vince la tabella.)*

**La proprietà di un file si riassegna quando l'unità che lo teneva chiude.** È il motivo
per cui `Pages/NoteEdit.razor`, prodotto dell'unità 03, entra nel perimetro dell'unità 04
per il gate di «Chiudi». La proprietà esclusiva serve a impedire che **due unità vive**
scrivano lo stesso file; non è un vincolo perpetuo, e trattarla come tale produrrebbe
un'unità in più per ogni ripensamento. Le unità sono sequenziali: la 03 è chiusa e
committata, quindi non c'è contesa.

**L'unità 02 non bloccava le altre**, e ora è chiusa: la migrazione è stata eseguita in
produzione il 3 settembre, quindi anche il vincolo di **collaudo** è caduto.
`/collections/{id}/items/{id}` è di nuovo raggiungibile, e il medaglione P3 dell'unità 10
torna visibile con almeno una collezione in elenco.

**Trappola per l'unità 04.** Ora che il 42501 è chiuso, l'errore del rilievo 3 non è più
riproducibile da `/collections/new`: per collaudare la traduzione del messaggio serve
innescare un'altra eccezione Postgrest.

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

### Il gate di «Chiudi» durante un salvataggio — deciso il 3 set 2026

Vale per **tutti e quattro** gli editor, `NoteEdit` compreso, che l'unità 03 ha chiuso senza
questo pezzo. In tutti e quattro, «Chiudi» è **l'unico** controllo del gruppo `.azioni` che
non guarda `occupato`: ogni input, «Salva», `SchedaConflitto` e `ConfermaAzione` lo leggono.

**La forma, e la base non c'entra:**

```razor
<a class="btn" href="@(occupato ? null : "notes")">Chiudi</a>
```

Il gate sta nel **markup**, nella stessa riga in cui sta quello di «Salva». `PaginaEditor`
**non deve vedere `occupato`**: costerebbe un membro astratto in più in quattro pagine per
una finestra di un round-trip.

**Divieto esplicito, ed è la via che un implementer prenderebbe da solo:** non trasformare
«Chiudi» in un `<button>`. Servirebbe navigare dalla pagina, e l'unica navigazione che la
base espone è `Esci`, che **disarma la guardia** — un «Chiudi» via `Esci` uscirebbe senza
chiedere niente, cioè reintrodurrebbe il difetto che questo lavoro sta correggendo.

**Perché non basta dire «tanto la scrittura arriva comunque».** Con esito `Salvata` è solo
incertezza. Ma con `Conflitto` o `Rifiutata` la modifica **non** è stata scritta, la pagina
è morta e nessuno lo dirà mai all'utente — che per giunta ha appena letto una domanda («se
esci le perdi») che gli è sembrata falsa perché aveva premuto Salva. Su `Crea`, uscire
produce una nota che l'utente crede scartata, e un duplicato al secondo tentativo.

**Verificato da `doc-checker` contro il sorgente di ASP.NET Core 10.0.10** — la versione
pinnata in `Eton.csproj:36` — e **non va riverificato**. `RenderTreeBuilder.AddAttribute(int,
string, string?)` chiama `TrackAttributeName` invece di aggiungere il frame quando il valore
è `null` **e** il target non è un componente: `href` non compare affatto nel markup. Due
trappole che vengono dallo stesso sorgente e vanno nei mandati:

- **`""` non viene omesso.** La condizione è `value != null`, non un controllo su stringa
  vuota: un `?? ""` messo per prudenza produrrebbe `href=""`, cioè un link valido **verso la
  radice dell'applicazione**. Il valore dev'essere letteralmente `null`.
- **Per `bool` il trigger di omissione è `false`, non `null`.** Le due regole si somigliano
  ma non sono interscambiabili.
- Su un **componente** (non un elemento HTML) `null` e `false` non vengono **mai** omessi:
  sono valori legittimi passati al parametro. Non generalizzare la regola a
  `<TestataPagina>` e simili.

**Il selettore CSS appartiene all'unità 11**, che possiede `app.css`: la regola a
`app.css:704` diventa `.btn:disabled, a.btn:not([href]) { … }`. Fino ad allora il link è
**funzionalmente inerte ma non spento visivamente**: stato intermedio accettabile perché il
collaudo nel browser avviene alla fine. Se l'unità 10 dimentica quel selettore, resta un
link che sembra premibile e non lo è — va nel suo mandato come voce esplicita.

**Una crepa nota di `smontata`, da annotare nel commento e non da correggere ora.** I
quattro editor **riusano l'istanza** sulla stessa rotta con parametro diverso. Andando
Indietro da `/notes/a` a `/notes/b` mentre un Elimina è in volo, l'istanza è riusata,
`smontata` resta falso, e l'`Esci` tardivo scarica sull'elenco l'utente appena arrivato su
b — a guardia disarmata. Perdita reale solo se ha digitato entro il round-trip: finestra
minuscola. Il commento a `PaginaEditor.cs:56-62` oggi promette più di quanto il codice
mantenga, ed è quello che va corretto.

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

**Due trappole per i mandati 04, 05 e 06.** (1) I tre editor rimasti hanno già
`@inject NavigationManager Navigation`: la riga va **tolta**, come fa `PaginaRegistro` con
`Spazi`. Se una pagina scoprisse di dover navigare per altro, la **rimette** — con la base
`private` questo non produce CS0108. (2) I `NavigateTo` da sostituire con `Esci(...)` sono
quelli che seguono `Crea()` ed `Elimina()`, e sono cinque in tutto: due in
`CollectionEdit`, due in `ItemEdit`, uno in `SpesaEdit`, che non ha `Crea`.

## DA PORTARE NEL MANDATO DELL'UNITÀ 11 — il foglio di stile

Si accumulano qui man mano che le unità le segnalano, perché `app.css` ha un solo
proprietario e ogni unità che ne ha bisogno deve rinunciare e dirlo.

1. **`a.btn:not([href])` accodato a `.btn:disabled`** (`app.css:704-709`). Senza, i due
   link «Chiudi» delle unità 04 e 05 sono funzionalmente inerti ma **non spenti
   visivamente**: sembrano premibili e non lo sono.
2. **La stessa regola `.btn:disabled` risolve la metà visiva del rilievo 9**: «Salva» spento
   è reso con `opacity: .5` su fondo blu pieno, e su nero resta saturo. L'unità 05 ha fatto
   la metà che si poteva fare senza CSS — dire *cosa manca* — quindi oggi il pulsante spiega
   il perché ma non sembra spento.
3. **`min-height: var(--tocco)` su `.scelta-categoria .pastiglia`.** Le pastiglie sono alte
   ~21px contro i 48px che il progetto stesso dichiara in `--tocco` (`app.css:190`) e già
   applica a `.barra-elenco .pastiglia`. **Vale per tre file** — `Spese.razor`,
   `SpesaEdit.razor` e ora `CollectionEdit.razor` — ed è lo stesso difetto del rilievo 5. Una
   riga in `app.css` lo chiude in tutti e tre; uno stile inline lo chiuderebbe in uno solo,
   lasciando due misure diverse della stessa pastiglia. L'unità 05 ha messo un
   `font-size: var(--t-lg)` inline come rimedio parziale dichiarato: **valutare se togliere**
   quando arriva la regola vera.
4. **`.scelta-categoria` ha ora due domini e un nome solo**: la usano le categorie di spesa e
   le icone di collezione, ma il nome parla di spese. Cosmesi di nomenclatura, non urgente —
   `.scelta-pastiglie` costerebbe tre sostituzioni. Decide chi possiede il file.

## PROSSIMA AZIONE

Unità 07 (`SpesaEdit`) — **è l'ultima delle quattro gemelle**, e dopo di lei si prova nel
browser. Il contratto ha retto tre adozioni senza una deroga né un `BLOCKED`: il suo mandato
è quello della 06 con il file cambiato, più i due punti qui sotto che sono suoi soltanto.

**Due cose che l'unità 06 le lascia in eredità, entrambe verificate da lei sul disco:**

1. `Pages/SpesaEdit.razor:242-243` cita tre `file:line` come esempi di «scrittura di stato
   fra i due `await`», e **tutte e tre sono ora stantie**: `NoteEdit.razor:200` è una riga
   vuota (unità 03), `CollectionEdit.razor:309` è un commento sulla tavolozza di emoji
   (unità 04/05), `ItemEdit.razor:214` è diventata la **226** (unità 06). Le prime due erano
   già rotte prima della 06. Il file è perimetro della 07, che lo riscrive comunque:
   correggerne una sola lascerebbe il commento sbagliato per due terzi.
2. ~~Il caso «errore di caricamento in creazione».~~ **Non si applica**, verificato dal capo
   prima di scrivere il mandato: `SpesaEdit.razor:1` ha la sola rotta `/expenses/{Id:guid}`
   e **nessuna variante di creazione** — le spese si creano altrove. Di conseguenza la 07 è
   la gemella **più piccola** delle quattro: un solo `NavigateTo` (`:386`) e quindi un solo
   `Esci(...)`, nessun `Nuova`, nessun `replace: true`.

Poi: 08, 09, 10, 11 nell'ordine della tabella. L'unità 11 raccoglie le **quattro voci di
`app.css` già accodate** (in fondo a questo file) più quelle che 07-10 aggiungeranno.

**Non** lanciare `live-testing` prima che anche l'unità 07 sia rientrata: la guardia va
provata una volta sola sulla forma finale, non quattro volte su forme intermedie. I criteri
di accettazione sono già scritti e **non si riscrivono**: dieci in
`handoff/03-contratto-editor/resoconto.md`, cinque in `handoff/04-collezione-contratto/`
(prove 1-5), sei in `handoff/06-elemento-contratto/` — queste ultime specifiche di
`ItemEdit`, con la **prova 6 della 04 esplicitamente non ripetibile** lì.

**Filone parallelo, spese ricorrenti.** Design in
`docs/superpowers/specs/2026-09-03-spese-ricorrenti-design.md` (commit `c4a56ee`), piano in
`docs/superpowers/plans/2026-09-03-spese-ricorrenti.md` (commit `03c61e9`): sei task, il
primo indipendente da tutto. **Parte a rilievi finiti**, non prima, e il motivo non è la
collisione di file — `SpesaEdit.razor` (unità 07) e `Spese.razor` (task 4) sono diversi. È
che il **task 6 crea un quinto editor**, `RicorrenteEdit.razor`, che deve adottare lo stesso
`PaginaEditor` che l'unità 07 sta finendo di provare sulla quarta pagina. Scriverlo prima
significherebbe adottare un contratto ancora in collaudo, e riaprirlo dopo.

## APERTO

~~Da fare al capo: aggiornare il testo del rilievo 15.~~ **Fatto il 3 set 2026**: la
sezione è ora «ISTRUITO E CHIUSO — non era un difetto», col ragionamento per esteso e la
condizione che lo smentirebbe. Non sparisce dall'elenco: un rilievo cancellato senza motivo
viene riaperto da qualcuno fra sei mesi.

~~Da consegnare all'utente: la migrazione da eseguire in produzione.~~ **Consegnata ed
eseguita il 3 set 2026.**

**Da riportare all'utente — difetto vero, fuori dai sedici rilievi, trovato dall'unità 08.**
`profiles.display_name` e `profiles.avatar_url` sono **congelati al primo accesso**:
`handle_new_user` (`supabase/migrations/20260811000000_initial_schema.sql:187-193`) li scrive con
`on conflict (id) do nothing`, e **nessun punto dell'applicazione li aggiorna mai** — l'unico
consumo è in lettura (`Services/SpaceRepository.cs:122-123`). Chi cambia nome o foto su Google li
vede aggiornati sulla **propria** `/profile`, che legge la sessione viva, ma **gli altri membri
continuano a vedere quelli del primo accesso, per sempre**. L'unità l'ha scoperto verificando cosa
poteva onestamente scrivere nell'aiuto, ed è la ragione per cui quell'aiuto **non** promette che
una correzione fatta su Google arrivi agli altri. Non è nel perimetro di nessuna unità: il rimedio
sta nel database o in un servizio. **Non l'ho corretto**: è fuori dall'obiettivo «correggere i
quindici rilievi», e inventarsi un'unità per ogni difetto che spunta trasformerebbe il lavoro in
un altro lavoro.

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
- **Le `file:line` di `threat-hunter` sono risultate sfasate**, in modo consistente, sul
  diff dell'unità 04: dava `<PageTitle>` a `:16` e «Chiudi» a `:68` mentre stanno a `:10` e
  `:201`. I suoi **verdetti** erano corretti e verificabili senza le righe. Regola per le
  unità successive: accogli i suoi verdetti se reggono per contenuto, ma **non riportare mai
  un suo numero di riga** in un resoconto o in un commento senza averlo riaperto. Quelle di
  `conformity` e `bug-hunter` tornano.
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
