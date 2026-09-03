UNITÀ: 06 — ESITO: FATTO

TOCCATI:
- `Pages/ItemEdit.razor` → +31/−17

Nessun `implementer` lanciato: il codice era già scritto dal tentativo morto per il 529, e la
verifica l'ha trovato completo e corretto. Il mio lavoro è stato **verificarlo, revisionarlo,
adjudicare e riferire** — cioè i tre punti che il mandato elencava come mancanti. Non ho
aggiunto né corretto una riga: sarebbe stato lavoro senza causa.

## VERIFICA DEL LAVORO EREDITATO

Il mandato diceva di non fidarmi dell'ispezione del capo. Ho letto `Pages/ItemEdit.razor`
per intero (459 righe, markup e `@code`) e le due gemelle come modello, prima di dispacciare
qualunque revisore. I quattro obiettivi risultano tutti soddisfatti, e il punto 3 — quello
che il capo dichiarava **non** controllato — è quello su cui ho trovato di più.

**Il punto 3, e il secondo canale d'esito che qui non esiste.** Ho censito tutti i punti di
scrittura e tutti i punti di render di `errore` e `avviso`:

- **Scritture**: `:196` (azzeramento in `Carica`), `:254` (catch del caricamento), `:317`
  (azzeramento in `Salva`), `:326` (nome vuoto), `:340`, `:352`, `:378`, `:391`, `:409`,
  `:416`, `:429`, `:439`, `:448`, `:452`. Quattordici in tutto.
- **Render**: `:41` (`errore`, nel ramo `collezione is null`), `:103` (`errore`) e `:107`
  (`avviso`), entrambi nel ramo del modulo e **subito sopra** `<div class="azioni">` a `:110`.

Non c'è nessun terzo canale. In particolare **la validazione del nome vuoto scrive su
`errore`** (`:326`, `errore = "Il nome dell'elemento non può essere vuoto.";`) e non su una
collezione separata: il pezzo che l'unità 04 aveva lasciato aperto — `erroriValidazione`
mostrato dentro la scheda «Campi», che la 05 ha dovuto chiudere dopo — **su questa pagina
non si presenta**. Il mandato chiedeva di cercarlo e di portarlo nello stesso posto: cercato,
non c'è, niente da portare.

**Un residuo delle gemelle che qui manca, e vale la pena saperlo.** `ItemEdit.razor:32` è

    else if (collezione is null)

**senza `&& !Nuovo`**, a differenza di `Pages/NoteEdit.razor:30` (`nota is null && !Nuova`) e
`Pages/CollectionEdit.razor:31` (`collezione is null && !Nuova`). Il `catch` di `Carica()`
azzera `collezione` (`:262`), quindi su `ItemEdit` **anche in creazione** un caricamento
fallito finisce nel ramo «Riprova» di `:32-43`, dove l'errore compare da solo sopra il
pulsante. Sulle due gemelle, in creazione, si resta invece nel ramo del modulo e l'errore di
caricamento compare **in fondo**: è esattamente il caso che l'unità 04 aveva isolato a mano,
non aveva potuto correggere, e aveva lasciato come prova 6 nel browser. **Qui quel caso non
può prodursi**, e la prova corrispondente non va ripetuta. Il ramo `:41` non può mai rendere
un div vuoto: gli unici percorsi che lasciano `collezione` nulla con `caricato` vero passano
tutti dal `catch`, che scrive `errore` prima di azzerare.

## CONTRATTI

`Shared/PaginaEditor.cs` **non è stato toccato**: nessuna deroga, nessun `BLOCKED`. Terza
adozione, terzo incastro senza attriti. Tutti i `file:line` qui sotto li ho riaperti io sul
file su disco dopo la build finale; nessuno viene dai revisori.

```razor
Pages/ItemEdit.razor:5      @inherits PaginaEditor
Pages/ItemEdit.razor:47     <NavigationLock ConfirmExternalNavigation="@Cambiata"
                                            OnBeforeInternalNavigation="GuardaUscita" />
```

```csharp
// Pages/ItemEdit.razor:177 — era 'private bool Cambiata', corpo invariato
protected override bool Cambiata => Nuovo
    ? !string.IsNullOrWhiteSpace(nome)
    : elemento is not null && ( … );

// Pages/ItemEdit.razor:364 — dopo Crea()
Esci($"collections/{Id}/items/{creato.Id}", replace: true);

// Pages/ItemEdit.razor:445 — dopo Elimina()
Esci($"collections/{Id}");
```

- `@inject NavigationManager Navigation` **tolto**. Nessun residuo: l'unica occorrenza della
  stringa «Navigation» nel file è ora `<NavigationLock>` a `:47`. I due `NavigateTo` erano
  gli unici due usi, entrambi post-`Crea()` e post-`Elimina()` come il contratto prevedeva:
  nessun bisogno di rimettere l'`@inject`.
- `IDisposable`: **non implementato dalla pagina**, come il mandato vieta. Nessuna occorrenza
  di `IDisposable` né di `Dispose` nel file: nessun rischio di CS0108, e la guardia contro la
  navigazione tardiva della base resta intera.
- `<NavigationLock>` è **dentro il ramo del modulo** (prima riga del ramo `else`), unica
  istanza nel file. Nessun revisore ne ha proposto lo spostamento fuori dai rami: il
  paragrafo del mandato non ha dovuto essere citato in adjudica, per la terza volta.
- `TitoloSchermata` (`:165`, `private string`) ha due call-site: `<PageTitle>` a `:11` e
  `<TestataPagina Titolo>` a `:13`. Il titolo è **riusato**, non ricalcolato, come chiesto.
  Stessa forma, stesso nome e stessa posizione relativa nel blocco `@code` che hanno
  `NoteEdit.razor:130-132` e `CollectionEdit.razor:325-327`.

**Per l'unità 07: il contratto regge, e le tre adozioni sono ormai identiche.** Nessuna delle
tre pagine ha richiesto un'aggiunta né una deroga. `SpesaEdit` può seguire lo stesso schema
senza verifiche nuove.

## GATE DI «CHIUDI»

```razor
Pages/ItemEdit.razor:119   <a class="btn" href="@(occupato ? null : $"collections/{Id}")">Chiudi</a>
```

`null` letterale, nessun `?? ""`, nessun `disabled`, nessun `aria-disabled`, nessun
`tabindex`, nessuno stile inline, nessuna trasformazione in `<button>`. **La destinazione è
quella che il link aveva già** (`collections/@Id`): l'unica differenza rispetto alle gemelle è
che qui è interpolata invece che costante, perché dipende dal parametro di rotta. La stessa
forma interpolata è già usata due volte nello stesso file per lo stesso indirizzo (`:29`,
`:85`), quindi non è un pattern nuovo.

I due `<a class="btn" href="collections/@Id">` dei rami `sparito` (`:29`) e di fallimento del
caricamento non sono stati toccati: lì `occupato` non è in gioco.

**Il selettore `a.btn:not([href])` resta all'unità 11**, come da mandato. Fino ad allora il
link è funzionalmente inerte ma non spento visivamente: stato intermedio atteso, non difetto.

## ADJUDICA

Revisori lanciati, tutti e tre nello stesso messaggio: `bug-hunter`, `conformity`,
`threat-hunter`. Ciascuno con diff, brief, e in più i propri materiali di contesto —
a `conformity` i file omologhi (`NoteEdit`, `CollectionEdit`, `TestataPagina`, `PaginaEditor`,
`CampoInput`, `SchedaConflitto`) e gli altri consumatori di `<Aiuto>`; a `bug-hunter` i due
call-site che entrano nella pagina (`Pages/CollectionDetail.razor:46` e `:125`, gli unici
ingressi) più i componenti figli; a `threat-hunter` `Program.cs` e gli endpoint omologhi.

**`backend-expert` non lanciato, ed è una scelta dichiarata.** Il gate lo vuole su superficie
nuova, diff > ~120 righe, o richiesta esplicita: qui il diff è di 48 righe, non nasce nessun
tipo, nessun servizio, nessuna astrazione — `TitoloSchermata` è una proprietà privata a due
call-site già stabilita dalle due unità precedenti, non una superficie. L'unità *consuma* un
contratto scritto altrove e il budget vietava di ridisegnare. Stessa scelta e stesso
ragionamento dell'unità 04.

**`threat-hunter` lanciato** benché il pattern fosse già passato due volte, perché il diff
porta testo scritto dall'utente (`nome`) in un punto di render **nuovo** — l'`<h1>` di
`TestataPagina` — e compone un `href` dinamicamente. Il mandato dice di lanciarlo in caso di
esitazione. **0 rilievi.**

    istruttoria: 0 rilievi su 0 file → checker no

`bug-hunter` 0, `conformity` 0, `threat-hunter` 0 (e i suoi non entrerebbero comunque nella
somma per regola). Sotto entrambe le soglie, e non per stretta misura: la somma è zero.

**Nessun rilievo da adjudicare, e nessun campione da riverificare.** Il §5 chiede almeno un
infondato riaperto per unità *quando ce ne sono*: qui non ne è arrivato nessuno, né fondato
né infondato. Lo dichiaro invece di ometterlo. Ma tre «0 rilievi» su un diff che nessuno
aveva mai istruito sono un esito che non ho accettato così com'è.

### Le tre verifiche indipendenti che ho fatto io, e i loro esiti

**1. I tre riferimenti `file:line` dentro i commenti — CONFERMATI, riaperti da me.** Il diff
aggiorna tre citazioni che le unità 03 e 04 avevano reso stantie spostando le righe di
`NoteEdit`. `conformity` le dà corrette; ne rispondo io, quindi le ho riaperte una per una su
`Pages/NoteEdit.razor` come sta oggi su disco:

| Citato da `ItemEdit` | Cosa c'è davvero a quella riga | Esito |
|---|---|---|
| `NoteEdit.razor:91-94` | il commento «Anche 'conflitto is null': con la scheda del conflitto aperta…» | corretto |
| `NoteEdit.razor:213` | `nota = null;` dentro il `catch` di caricamento | corretto |
| `NoteEdit.razor:260-263` | il commento «replace: true perché /notes/new non è un posto in cui tornare…» | corretto |

**2. La domanda più rischiosa del diff, posta a `bug-hunter` e riaperta da me: `disarmata`
può restare alzato e disarmare la navigazione *successiva*?** La sua risposta era corretta
ma compattava il passaggio che conta, e l'ho verificato sul contratto: in
`Shared/PaginaEditor.cs` il `return` per `smontata` sta **prima** dell'assegnazione —

    if (smontata) return;
    disarmata = true;

— quindi il percorso in cui `Esci` non naviga è anche quello in cui il flag non viene alzato,
e non resta mai un disarmo orfano da spendere su un'uscita successiva. Nei due call-site di
questa pagina la chiamata avviene mentre il ramo `else` è reso e `<NavigationLock>` montata,
quindi `GuardaUscita` consuma il flag sulla stessa navigazione che l'ha alzato. **Regge.**

**3. Un caso che nessuno dei tre ha isolato, cercato da me, ed esito negativo — dichiarato
perché tacerlo sarebbe peggio.** Il `catch` di `Carica()` azzera `collezione`, `elemento`,
`campi` e `valori` (`:262-265`) ma **non** `nome`. Poiché `<TestataPagina>` sta fuori da tutti
i rami, un fallimento di caricamento in *modifica* mostrerebbe l'`<h1>` col nome dell'elemento
**precedente**. Ho verificato se sia raggiungibile: perché Blazor riusi l'istanza servono due
rotte dello stesso componente in sequenza senza componenti in mezzo, e l'unico percorso simile
nell'applicazione è `/items/new` → `/items/{id}` dopo `Crea()`, dove `nome` è quello appena
scritto dall'utente ed è **giusto**. Da elemento a elemento si passa sempre per
`CollectionDetail`, che smonta la pagina. In creazione `nome` è `""` e il titolo è il generico
«Elemento». In più il comportamento è **identico in tutte e tre le gemelle** — `NoteEdit` e
`CollectionEdit` non azzerano titolo e nome nei rispettivi `catch` — quindi non sarebbe
comunque un difetto introdotto da questa unità. **Non è un rilievo e non ho corretto niente**;
lo scrivo perché è l'unico punto del diff in cui ho dovuto ragionare per escludere un difetto
invece che per confermarne l'assenza.

## FUORI SCOPE

**1. `Pages/SpesaEdit.razor:242-243` cita tre `file:line` ormai tutte e tre stantie.** Il
commento dice che nelle pagine gemelle esiste una scrittura di stato fra i due `await`, e cita
`NoteEdit.razor:200`, `CollectionEdit.razor:309`, `ItemEdit.razor:214`. Verificate da me sui
file su disco:

- `NoteEdit.razor:200` è oggi una **riga vuota** — resa stantia dall'unità 03.
- `CollectionEdit.razor:309` è oggi un commento sulla **tavolozza di emoji** — resa stantia
  dalle unità 04/05.
- `ItemEdit.razor:214` era `campi = SchemaCampi.Ordina(collezione.Fields);`, cioè proprio la
  scrittura di stato che il commento voleva citare. **Oggi quella riga è la 226**: l'ha
  spostata di 12 righe il diff di questa unità.

`Pages/SpesaEdit.razor` è **perimetro dell'unità 07** e il mandato me lo vieta esplicitamente:
non l'ho toccato. Il rimedio naturale è dell'unità 07, che quel file lo riscrive comunque —
una riga di commento, tre numeri. Segnalo che due delle tre erano già rotte prima di me:
correggendo solo la mia si lascerebbe il commento sbagliato per due terzi.

**2. `@using Eton.Services` a `Pages/ItemEdit.razor:4` è ridondante**, perché `_Imports.razor:9`
lo importa già globalmente. `@using Eton.Models` a `:3` invece serve, non è negli import
globali. **Preesistente al diff** e non toccato. È la stessa osservazione che l'unità 04 ha
fatto sulla riga omologa di `CollectionEdit.razor`: se si decide di ripulirla, vale su almeno
due file e conviene farlo in un colpo solo, non un'unità per volta.

## GATE

- `dotnet build -warnaserror --no-incremental` → **0 errori, 0 avvisi**. Ricompilazione
  completa e non incrementale, di proposito: la build che avevo trovato era incrementale, e su
  un file con `@inherits` e `override` il gate serve proprio a intercettare il CS0108 che una
  build permissiva declasserebbe. Con la ricompilazione forzata il risultato non dipende dai
  timestamp lasciati dalla sessione morta.
- `dotnet test --no-build` → **267/267 superati**, 0 non superati, 0 ignorati (210 ms).

Compilato **io**, due volte in tutto e sempre con nessun agente vivo: una prima del dispaccio
dei revisori, una dopo. Ai tre revisori ho vietato esplicitamente nel brief di eseguire
`dotnet build` e `dotnet test`, perché `obj/` non ha lock fra processi.

**Il lavoro non è committato**: il working tree porta `Pages/ItemEdit.razor` modificato e
`handoff/06-elemento-contratto/` non tracciato. Il mandato elencava tre deliverable — verifica,
revisione, resoconto — e il commit non è fra questi; le unità 02-05 risultano committate da
fuori. Lo lascio pronto e lo dichiaro invece di deciderlo da solo.

## SCOSTAMENTI

**Nessuno.** In particolare **non** divergono: la posizione di `<NavigationLock>` dentro il
ramo del modulo, la firma di `Cambiata`, i due `Esci(...)` con le destinazioni e il `replace:
true` invariati, la forma del gate di «Chiudi» col `null` letterale e la destinazione
preesistente, la posizione e l'API di `<TestataPagina>`, lo spostamento del blocco
`errore`/`avviso` a markup invariato. Nessuna astrazione nuova, nessun tipo nuovo, nessun
metodo nuovo oltre alla proprietà `TitoloSchermata`, nessun file `.js`, nessun servizio
iniettato nuovo, nessun pacchetto, nessuna riga di `app.css`, nessuno stile inline. Nessun
file fuori dal perimetro è stato toccato.

Il mandato prevedeva il caso di un obiettivo in contraddizione con un divieto: **non si è
presentato**. La sola contraddizione potenziale — «porta il secondo canale d'esito nello
stesso posto» contro «non toccare altro» — si è sciolta da sé, perché il secondo canale su
questa pagina non esiste (v. VERIFICA).

**Il server di sviluppo non è stato avviato** e nessuna prova è stata fatta nel browser, come
da mandato. Nessun processo lasciato vivo, nessuna porta occupata.

## DA PROVARE NEL BROWSER

Le prove generali dell'adozione del contratto **non le ripeto**: valgono identiche e sono già
scritte in `handoff/03-contratto-editor/resoconto.md` (le dieci di `NoteEdit`) e in
`handoff/04-collezione-contratto/resoconto.md` (prove 1-5). Su questa pagina cambiano solo gli
indirizzi: `/collections/{c}/items/new` e `/collections/{c}/items/{i}`, con destinazione di
«Chiudi» `/collections/{c}`. Qui sotto **solo ciò che è specifico di `ItemEdit`**.

**1. L'esito sopra i pulsanti, con la scheda alta — il caso che questa pagina ha e le gemelle
no.** Aprire un elemento di una collezione con **molti campi** (crearne una decina da
`/collections/{c}/edit`, così il modulo scorre oltre una schermata), modificare un valore e
premere «Salva».
*Accettazione*: «Salvato.» compare **appena sopra** i pulsanti «Salva»/«Chiudi»/«Elimina», in
vista senza scorrere. Sotto ai pulsanti c'è il blocco delle recensioni: verificare che il
messaggio stia **fra** i campi e i pulsanti, non dopo le recensioni.

**2. L'errore di validazione arriva nello stesso posto degli altri — la differenza rispetto
alla 04.** Sullo stesso elemento con molti campi: **svuotare il nome** e premere «Salva».
*Accettazione*: «Il nome dell'elemento non può essere vuoto.» compare **nello stesso identico
punto** di «Salvato.», sopra i pulsanti. Se comparisse altrove, la verifica che ho fatto sul
codice è sbagliata e va riaperta. È la prova 7 dell'unità 04 con l'esito **atteso opposto**:
lì l'errore di validazione stava altrove per costruzione, qui deve stare qui.

**3. Il caricamento fallito porta al «Riprova», anche in creazione.** Aprire
`/collections/{c}/items/new` **con la rete disattivata** dagli strumenti per sviluppatori.
*Accettazione*: compare il riquadro con «Non è stato possibile aprire l'elemento: …» e il
pulsante **«Riprova»**, non il modulo con l'errore in fondo. Riattivando la rete e premendo
«Riprova» il modulo si apre. **Questo è il punto in cui `ItemEdit` si comporta meglio delle
due gemelle** (v. VERIFICA): la prova 6 dell'unità 04 non va ripetuta qui.

**4. La testata dice ciò che la pagina non dice.** Su `/collections/{c}/items/{i}`.
*Accettazione*: il pannello «?» apre **tre** paragrafi. Il primo — il valore non interpretabile
che non viene salvato — è l'unico dei tre che descrive un comportamento **invisibile**: va
provato davvero. Scrivere `dodici` in un campo di tipo numero, o `ieri` in un campo data,
premere «Salva».
*Accettazione*: il salvataggio **riesce** («Salvato.»), nessun errore, e quel campo torna
**vuoto**. Se invece comparisse un errore, o il testo restasse a schermo, il paragrafo
dell'aiuto sta mentendo e va riscritto.

**5. Il campo «rimosso» e il campo tolto dalla collezione.** Specifico di questa pagina, non
esiste sulle gemelle. Su una collezione con un campo `select`: scegliere un'opzione in un
elemento, poi **togliere quell'opzione** dalla collezione da `/collections/{c}/edit`, poi
riaprire l'elemento.
*Accettazione*: il menù mostra in coda la voce «<valore> (opzione rimossa)» e il valore **non**
viene sostituito da quello mostrato per caso al primo salvataggio. È comportamento
preesistente e non toccato dal diff, ma il diff ha spostato le righe attorno: vale un'occhiata
di conferma, non un'indagine.

**6. Il conflitto ottimistico, con la testata nuova sopra.** Due schede sullo stesso elemento,
modificare e salvare nella prima, poi modificare e salvare nella seconda.
*Accettazione*: compare `SchedaConflitto` con «Mentre modificavi, questo elemento è cambiato.»
e le due scelte. **La testata resta in cima e l'`<h1>` continua a mostrare il nome**: se il
titolo sparisse o si sovrapponesse alla scheda, è un difetto di impaginazione da riportare.
Premendo «Ricarica», «Caricata la versione più recente.» deve comparire **sopra i pulsanti**,
non in cima.

**Cosa non è praticabile a mano, e perché.** L'elemento sparito sotto i piedi — il caso su cui
si regge il divieto di spostare `<NavigationLock>` fuori dai rami — richiede due schede,
eliminarlo nella prima e salvare nella seconda. Se c'è occasione, l'accettazione è: compare
«Questo elemento non esiste più…», e premendo «Torna alla collezione» **nessuna domanda**,
benché il modulo modificato sia ancora in memoria. Se non si fa, va detto come limite.
Il dirottamento della navigazione tardiva (`smontata`/`Dispose`) resta non riproducibile a
mano in modo affidabile: è già dichiarato come limite nel resoconto dell'unità 03 e non lo
riapro.
