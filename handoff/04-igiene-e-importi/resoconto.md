UNITÀ: 4 — ESITO: FATTO

Le cinque voci sono chiuse e i gate sono verdi, **288 test contro i 287 di partenza**. Quattro cose vanno
lette prima del resto.

1. **Le voci 3 e 4 si sono chiuse come regola e come sua applicazione, e non era previsto.** Il mandato le
   dava indipendenti. La docstring che la voce 3 ha scritto su `Denaro.Testo` prescrive fra i suoi casi
   d'uso «un campo che chi guarda non può modificare» — che è *letteralmente* ciò che la voce 4 ha poi
   implementato in `SpesaEdit`. Sono state scritte da due implementer diversi che non si sono parlati, e
   combaciano perché il criterio veniva dalla stessa decisione in `PIANO.md`.
2. **L'unico rilievo di tutta l'istruttoria l'ha trovato `conformity`, ed era una forma, non un bug.** La
   tavolozza era stata scritta con una sintassi che in tutto il repository non compariva da nessun'altra
   parte. Il `grep` che lo dimostra è in `ADJUDICA`, ed è il tipo di prova che rende un rilievo di
   conformità non opinabile.
3. **La mia trascrizione delle ventiquattro emoji nel brief era sbagliata, e l'implementer ha fatto bene a
   ignorarla.** Gli avevo ordinato di ricopiarle dal file e non dal brief, dichiarando che in caso di
   divergenza vincesse il file. Ha trovato la divergenza, l'ha seguita, e me l'ha riportata. Dettaglio in
   `SCOSTAMENTI 1`.
4. **Ho chiuso io i due `non verificabili` del `checker` sul fix**, e la prova non poteva averla lui: sono
   due blocchi che avevo letto **prima** della correzione, nel mio contesto. Ricopio comunque la sua riga
   invece di ammorbidirla. Dettaglio in `ADJUDICA`.

## TOCCATI

- `Services/SchemaCampi.cs` → **+29/−0**
- `Services/Denaro.cs` → **+29/−4**
- `Pages/SpesaEdit.razor` → **+23/−7**
- `Pages/ItemEdit.razor` → **+14/−1**
- `Eton.Tests/SchemaCampiTests.cs` → **+14/−0**
- `Pages/CollectionEdit.razor` → **+1/−18**
- `Pages/CollectionDetail.razor` → **+0/−1**

`git diff --stat` → `7 files changed, 110 insertions(+), 31 deletions(-)`.

**`git diff -w --numstat` differisce su un file solo**, `Pages/SpesaEdit.razor`, dove dà `16 0` invece di
`23 7`: le sette righe tolte e sette delle ventitré aggiunte sono **la stessa riga reindentata**, quella
dell'`<input>` dell'importo e il suo commento, che rientrano di quattro spazi dentro il nuovo `@if`. Su
tutti gli altri sei file i due comandi danno numeri identici: nessuna riga è reindentazione.

`git status --porcelain --untracked-files=all` → sette `M` e **nessun untracked**; `git diff --summary`
dopo `git add -N .` → **0 `create mode`**. Nessun file nuovo, nessun temporaneo lasciato dagli agenti.

**Delle 110 inserzioni, 29 sono codice e 81 sono commento o test.** Le 29: le tre righe di emoji più le
quattro strutturali della tavolozza in `SchemaCampi` (7); la riga della dichiarazione (1); la riga
`@foreach` riscritta in `CollectionEdit` (1); la guardia `if (!NomeValido) { errore = …; return; }` di
`ItemEdit` (5); l'`@if (PuoIntervenire)`, l'`else`, il `<p>` e le tre graffe di `SpesaEdit` (6), più le due
righe dell'`<input>` che il diff conta come aggiunte benché siano solo reindentate (2); le sette righe del
test (7). Le due docstring di `Denaro.cs` sono **29 righe di cui zero codice**: misurato, non dichiarato —
v. `GATE`.

## REVIEW

    review:
      bug-hunter      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      bug-hunter      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      bug-hunter      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      conformity      RILIEVI: 1
      backend-expert  non lanciato — 7 files changed, 110 insertions(+), 31 deletions(-)
                      · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
      checker (fix)   VERDETTI: risolti 7 · non risolti 0 · non verificabili 2

`bug-hunter` e `threat-hunter` compaiono **tre volte** ciascuno perché il §3 vuole la review in pipeline e
i quattro implementer sono rientrati a distanza: la prima coppia ha giudicato il brief della tavolozza
(`SchemaCampi` + `CollectionEdit` + il test), la seconda `ItemEdit`, la terza `SpesaEdit` +
`CollectionDetail`.

**Il quarto brief — le due docstring di `Denaro.cs` — non ha avuto revisori, ed è la prima riga della
tabella del §3, applicata per misura e non per fiducia nel resoconto dell'implementer.** Il comando:

    git diff -U0 -- Services/Denaro.cs | grep -E '^[+-]' | grep -vE '^(\+\+\+|---)' | grep -vE '^[+-]\s*///'

restituisce **output vuoto**: nessuna delle 33 righe toccate in quel file è diversa da una docstring. La
riga corrispondente del tracciato sarebbe `review: nessuna — commenti`, ma non l'ho scritta al posto delle
altre voci perché riguarda **uno** dei quattro brief, non l'unità: le altre tre sono state revisionate
davvero, e una riga sola le cancellerebbe.

`conformity` **una volta sola, sul diff completo dei sette file**, e non per brief. Tre dei quattro diff
stavano sotto le ~30 righe (`ItemEdit` 15, `SpesaEdit`+`CollectionDetail` 31 grezze ma 17 reali con `-w`,
`Denaro` esente); solo quello della tavolozza (61 righe) lo chiedeva, e a quel punto puntarlo su un brief
solo avrebbe sprecato il lancio. Era anche il revisore più pertinente dell'unità: **quattro delle cinque
voci sono allineamenti a un precedente esistente** — la tavolozza a `TipiAmmessi`, `Sovrascrivi` a
`SpesaEdit.Sovrascrivi`, l'importo in sola lettura a `Spese.razor:198`, il `@using` a `_Imports.razor:9` —
quindi «assomiglia ai vicini?» qui non è stile, è la sostanza del mandato. **Ha prodotto l'unico rilievo
dell'unità**, e su un punto che nessuno dei sei lanci di `bug-hunter`/`threat-hunter` poteva vedere, perché
non era un difetto di comportamento.

**Il gate di `backend-expert` è stato misurato sui `.razor` oltre che sui `.cs`**, deviando dal comando del
`CLAUDE.md` che filtra `-- '*.cs'`. È la stessa deviazione deliberata delle unità 02 e 03, ma qui **per la
ragione opposta**: lì il comando prescritto avrebbe dato `0` per costruzione perché il diff non toccava
`.cs`; qui i `.cs` ci sono, il comando prescritto ha risposto `0`, e ho **rifatto la misura estesa ai
`.razor`** per non far dipendere l'esenzione dal filtro. Anche esteso: **0**. Le tre condizioni restano
tutte negative e nessuna dipende dal mio giudizio: **0 `create mode`** — misurato **dopo `git add -N .`**,
a implementer rientrati, quindi la cecità sui file non tracciati che il §3 avverte non si applica — **0
dichiarazioni/endpoint** su entrambe le misure, e **110 inserzioni contro la soglia di ~120**. Le due
misure sono state **rifatte dopo l'ultimo edit**: il fix ha portato le inserzioni da 109 a 110, e una voce
di esenzione calcolata prima dell'ultimo edit non descriverebbe il diff che consegno.

`TavolozzaIcone` è un *campo*, non un tipo: il pattern non la conta, e giustamente.

`coverage` non compare: dentro una sessione-unità non si lancia (§6).

## CONTRATTI

I quattro del mandato, nella forma reale risultante, riaperti da me sull'albero finale dopo l'ultimo edit.

**1-2.** Le due firme di `Denaro`, **identiche nel testo, spostate di riga**. Il mandato le dava a `:110` e
`:126`; le docstring riscritte, più lunghe delle precedenti, le hanno spinte in basso di 13 e 25 righe:

```csharp
Services/Denaro.cs:123    public static string Testo(decimal importo)
Services/Denaro.cs:151    public static string TestoDigitabile(decimal importo)
```

**Nome compreso: nessuna rinomina.** I sette consumatori fuori dai test e le quattordici asserzioni di
`Eton.Tests/DenaroTests.cs` che li nominano sono intatti — e la prova è che quel file **non è nel diff** e
i suoi test sono verdi fra i 288. Cambia solo ciò che precede le firme: due blocchi `///`, misurati sopra.

**3.** `Services/Denaro.cs:66` — **invariata, riga compresa**, e non allargata:

```csharp
    public static EsitoImporto Verifica(string? testo, out decimal importo)
```

Il corpo non è stato aperto in scrittura: il comando che isola le righe non-`///` del file restituisce
vuoto, quindi la riga `:80` che rifiuta più di un separatore è quella di prima. È il vincolo su cui poggia
l'intera voce 4: se `Verifica` avesse accettato le migliaia, il campo modificabile non avrebbe avuto
bisogno di restare su `TestoDigitabile`.

**4.** `Services/SchemaCampi.cs:229` — `Modelli` **invariata, riga compresa**, non spostata e non
rinominata. La tavolozza è **accanto**, dopo di lei:

```csharp
Services/SchemaCampi.cs:229    public static IReadOnlyList<ModelloCollezione> Modelli =>
Services/SchemaCampi.cs:278    public static readonly IReadOnlyList<string> TavolozzaIcone =
```

L'invariante che le lega è ora asserito da `Eton.Tests/SchemaCampiTests.cs:253`, che **confronta le due
liste fra loro** e non contro tre emoji ricopiate:

```csharp
        Assert.Equal(icone, SchemaCampi.TavolozzaIcone.Take(icone.Count));
```

**5.** `_Imports.razor:9` — **intatta**, ed è la riga che doveva restare:

```razor
@using Eton.Services
```

⚠️ **E questa è la prova che la voce 1 è chiusa, in una forma che non si può simulare**: un `Grep` di
`@using Eton.Services` su tutti i `.razor` del progetto restituisce oggi **una sola riga**, questa. Prima
del diff ne restituiva quattro. Non è il conteggio di ciò che ho tolto — è il conteggio di ciò che resta.

## ADJUDICA

Otto lanci di revisori, **un solo rilievo**, fondato in tutte e quattro le sue affermazioni, corretto per
intero, e verificato. Più un rilievo mio, trovato rileggendo il risultato.

### Il rilievo di `conformity`, e perché il fondato non era la forma ma il silenzio del commento

**`Services/SchemaCampi.cs:277` — `TavolozzaIcone` usa una forma che il progetto non usa da nessun'altra
parte.** Era stata scritta come auto-property get-only, `public static IReadOnlyList<string>
TavolozzaIcone { get; } = [...]`, mentre il progetto ha già una forma per «elenco costante pubblico di
stringhe» e la usa due volte **con lo stesso identico tipo**:

- `Services/SchemaCampi.cs:26` — `public static readonly IReadOnlyList<string> TipiAmmessi = [...]`,
  **venti righe sopra, nello stesso file**;
- `Services/CategorieSpesa.cs:16` — `public static readonly IReadOnlyList<string> Elenco =`.

Ho fatto istruire il claim in **quattro affermazioni separate**, perché potevano avere verdetti diversi.
Tutte e quattro fondate, e la più importante è la seconda:

- **(A) fondato.** Le due dichiarazioni omologhe hanno davvero quella forma, citate testualmente ai
  `file:line` sopra.
- **(B) fondato, ed è quella che decide.** Un `grep` sull'intero albero, esclusi `obj/`, `bin/` e
  `storico/`, sulla forma `{ get; } = [` non restituisce **nessuna** occorrenza oltre a quella in esame.
  Non era la seconda istanza di un pattern esistente: era una **terza forma** introdotta per un pattern che
  il progetto ha già risolto una volta sola. È la differenza fra «una scelta diversa» e «una scelta nuova»,
  e solo un conteggio poteva dirla.
- **(C) fondato, con una riserva dichiarata dal `checker`.** Le due forme sono equivalenti per questo caso
  d'uso: una sola inizializzazione, nessun setter, stesso tipo esposto, stesso comportamento dentro il
  `@foreach`. Il `checker` ha dichiarato di non aver verificato se il getter venga inlineato dal JIT —
  dettaglio che non cambia né l'allocazione né la mutabilità. **Ha dichiarato il limite invece di
  affermare**, che era ciò che gli avevo chiesto.
- **(D) fondato.** Il commento `//` che precedeva la dichiarazione motivava la scelta **solo** contro
  `Modelli` — il vicino con tipo diverso, che rialloca di proposito — e non nominava `TipiAmmessi` in
  nessuna riga (`grep TipiAmmessi` su quel file: solo `:26` e `:91`).

**Il difetto vero era quindi il silenzio del commento, non la forma in sé.** Una forma diversa e motivata
sarebbe stata una decisione; una forma diversa motivata **contro il confronto meno pertinente** fa sembrare
la questione già risolta a chi legge, ed è più difficile da correggere dopo, perché sembra che qualcuno
l'abbia considerata. È la quarta unità di fila in cui il fondato sta in un commento, e qui il meccanismo è
preciso: **un commento che difende una scelta contro l'alternativa sbagliata è peggio di nessun commento.**

**Corretto per intero, scegliendo la prima delle due strade che il `checker` offriva** — riscrivere come
campo, invece di tenere la property e documentarla meglio. La seconda avrebbe lasciato nel repository una
forma unica con un commento a difenderla: il commento sarebbe stato migliore, il codebase no. Il commento
è stato riscritto comunque, e ora nomina **entrambi** i vicini, `Services/SchemaCampi.cs:272-277`.

**verificato: risolto** — `Services/SchemaCampi.cs:278`, per (1a): la dichiarazione è oggi
`public static readonly IReadOnlyList<string> TavolozzaIcone =`, nella stessa forma delle altre due.
**verificato: risolto** — il `grep` sulla forma `{ get; } = [` sull'intero albero è tornato **senza
output**, per (1b): la forma è sparita dal repository, non solo da questo punto.
**verificato: risolto** — `Services/SchemaCampi.cs:272-277`, per (1c): il commento nomina `TipiAmmessi`,
`CategorieSpesa.Elenco` e `Modelli`, con la ragione per cui segue i primi due.
**verificato: risolto** — `Services/SchemaCampi.cs:280-282` contro `git show HEAD:Pages/CollectionEdit.razor`
righe 313-315, per (1d): le ventiquattro emoji sono **identiche, carattere per carattere**. È il controllo
che conta di più in un diff fatto di sole forme, ed è quello che nessuno avrebbe notato fallire.
**verificato: risolto** — `Pages/CollectionEdit.razor:101` e `:296-298`, per (1f): il commento nuovo afferma
che questa forma rende sicuro leggere l'elenco direttamente nel `@foreach` mentre `Modelli` va catturato, e
**entrambe le metà reggono** contro il codice reale.

### Il rilievo mio, e perché un commento può essere falso senza dire niente di sbagliato

**`Pages/ItemEdit.razor:469` — «e il call-site poco sopra», per una riga che sta 416 righe più su.** Il
commento della guardia nuova rimandava al call-site di `<SchedaConflitto>` chiamandolo «poco sopra». Il
call-site è a `:53`, il commento a `:469`, e in un `.razor` tutto il markup precede il blocco `@code`, che
qui comincia a `:171`. Non l'ha trovato nessun revisore, e non è un loro buco: nessuno dei quattro ha il
mandato di verificare che un rimando dica il vero sulla propria distanza.

**Il difetto non è l'imprecisione, è il costo che impone al lettore.** «Poco sopra» manda a cercare in una
schermata qualcosa che sta quattrocento righe più su: chi non lo trova conclude di aver capito male il
commento, non che il commento sbagliasse. E il file ha già una convenzione per questo, che **nomina il
punto invece della distanza** — `:51` «v. il commento su `PuoIntervenire` più sotto», `:320` «v. il
commento di `PerModifica`», `:380` «v. il commento nel catch di `OnParametersSetAsync`».

Corretto in «e il suo call-site nel markup»: nomina la regione strutturale invece della distanza, ed è
inequivocabile perché `<SchedaConflitto>` compare una volta sola nel file.

**verificato: risolto** — `Pages/ItemEdit.razor:53` contro `:171`, per (2a): il call-site è nel markup,
perché precede l'apertura di `@code`.
**verificato: risolto** — `Pages/ItemEdit.razor:50-52`, `:319-320`, `:380`, per (2b): gli esempi citati
nominano tutti un simbolo o una regione, mai una distanza relativa.

### I due `non verificabili` del `checker`, chiusi da me con una prova che lui non poteva avere

Il `checker` ha dichiarato **non verificabili** (1e) — che il blocco `///` sopra la tavolozza non fosse
stato toccato dal fix — e (2c) — che in `ItemEdit` fosse cambiata solo la riga `:469`. **Il suo motivo è
corretto e l'ho ricopiato invece di aggirarlo**: il lavoro non è committato, quindi non esiste su disco uno
stato «dopo le cinque voci, prima del fix» con cui confrontare, e lui l'ha verificato anche in `git stash`
e `git reflog` prima di dirlo. È lo stesso limite che le unità 02 e 03 hanno riportato.

**Ma quella prova ce l'avevo io, e non è un aggiramento: è una lettura fatta prima.** Entrambi i blocchi li
avevo aperti **prima** di lanciare l'implementer del fix, per decidere cosa fargli correggere, e li ho
riaperti dopo:

- **(1e) risolto.** Il blocco `///` sta a `Services/SchemaCampi.cs:256-271` **prima e dopo**: identico nel
  testo e **nello stesso intervallo di righe**. Quest'ultimo fatto non è un di più — il fix ha sostituito
  cinque righe `//` con sei, tutte *sotto* il blocco, quindi se il `///` non si è spostato di una riga è
  perché nulla sopra di esso è cambiato.
- **(2c) risolto.** Le altre sette righe del commento (`:465-468`, `:470-472`) e le cinque della guardia
  (`:473-477`) sono identiche a quelle che avevo letto prima del fix, **numero di riga compreso**. L'unica
  differenza in tutto il blocco è la sostituzione di tre parole a `:469`.

Ricopio comunque la riga `non verificabili 2` nel tracciato e non la riscrivo a `0`: **il conteggio è il
suo, e il suo punto di osservazione era davvero cieco su quei due punti.** Riscriverlo con ciò che so io
trasformerebbe una misura in un'opinione, e renderebbe il tracciato non più confrontabile con il report.

### Il campione, e perché anche qui ha dovuto cambiare forma

Il §5 chiede di riverificare **almeno un infondato per unità**. **Non ce n'è nessuno**: il `checker` ha
istruito quattro sotto-claim e li ha dichiarati tutti fondati, e i sei lanci di `bug-hunter`/`threat-hunter`
sono tornati a zero. Come nelle unità 02 e 03, ho riaperto io i **fatti che sostengono gli zero**, che è
l'unico controllo sensato quando non c'è niente da scartare. Cinque, e due hanno cambiato ciò che ho
scritto qui:

- **Le ventiquattro emoji, contro `HEAD`** — v. sopra. È il controllo che ho voluto fare due volte, da me e
  dal `checker`, perché è l'unico errore di questo diff che nessun test e nessuna build avrebbero preso.
- **Il fatto su cui poggia l'intera voce 4** — che `importoTesto` continui a essere riempito anche quando
  l'`<input>` non è più reso — **è vero, riaperto da me**: l'assegnazione è a `Pages/SpesaEdit.razor:304`,
  dentro `OnParametersSetAsync`, **incondizionata** e indipendente dal rendering. Se dipendesse dal markup,
  `Cambiata` (`:242`) sarebbe **vera all'apertura** per chi non può intervenire, e la guardia d'uscita
  chiederebbe conferma a ogni singola uscita: sarebbe il difetto peggiore di quello corretto, ed è
  esattamente quello che l'unità 12 aveva chiuso.
- **Il vincolo più stretto del brief — che il campo modificabile non cambi — l'ho misurato invece di
  crederlo**: `git diff -w -- Pages/SpesaEdit.razor` non elenca la riga dell'`<input>` fra quelle cambiate.
  Chi può digitare vede oggi ciò che vedeva ieri, e la misura lo prova senza aprire il browser.
- **Il claim più falsificabile di `threat-hunter`/`SpesaEdit`** — che `PuoIntervenire` **fallisca chiuso**
  anche ora che decide *quale markup esiste* e non più solo se è spento — regge su
  `Services/Permessi.cs:63-66`: con `mioId` nullo o l'elenco degli spazi vuoto si finisce sempre nel ramo
  di **sola lettura**. Il ramo pericoloso — un guasto che mostra il campo modificabile a chi non ne ha
  diritto — non è raggiungibile.
- **Il fatto che rende chiusa la voce 1 in forma positiva**: `@using Eton.Services` compare oggi **una sola
  volta** in tutto il progetto, in `_Imports.razor:9`. Contare ciò che resta è più forte che contare ciò
  che si è tolto, perché non dipende da quante occorrenze si credeva ce ne fossero — ed era proprio il
  punto su cui il mandato avvertiva, dato che il resoconto d'origine ne contava due invece di tre.

## FUORI SCOPE

**Nessun rilievo fondato è rimasto aperto.** L'unico dell'unità è stato corretto per intero e verificato.

Tre osservazioni che **non** sono rilievi, e che riporto perché il capo le incontrerà:

**1. `Pages/SpesaEdit.razor` — il ramo di sola lettura lascia un `<label class="campo">` che non etichetta
più un controllo.** Quando `PuoIntervenire` è falso, dentro il `<label>` c'è un `<p>`, non un `<input>`.
Non è invalido e il testo resta leggibile, ma un `<label>` senza controllo associato non fa il proprio
mestiere per chi ascolta la pagina. **Non l'ho corretto**, e il motivo è che l'alternativa costa più di
quanto renda: rendere condizionale anche l'elemento contenitore significherebbe duplicare l'etichetta in
entrambi i rami, e il mandato vietava di allargarsi sul campo. `conformity` non l'ha sollevato, e nessuno
dei tre revisori di quel brief neppure. **È una decisione dichiarata, non una svista** — e se il capo la
giudica diversamente, la strada è un `<div class="campo">` nel ramo `else`, che è un diff di due righe.

**2. `Pages/ItemEdit.razor` — il pulsante «Sovrascrivi» resta spento dal solo permesso, non dalla validità
del nome.** L'omologo completo di `SpesaEdit` ha **due** protezioni: il pulsante spento nel markup
(`SovrascriviAbilitato="@(ImportoValido && PuoIntervenire)"`) **e** il controllo dentro il metodo. Qui ho
applicato **solo la seconda**, che è ciò che il mandato prescriveva alla lettera («il rimedio è tre righe
ricalcate da quelle») e ciò che chiude il buco: il nome vuoto non raggiunge più il database. Il
`NON TOCCARE` nominava i pulsanti degli editor, e allargarmi al markup avrebbe toccato il call-site di
`<SchedaConflitto>` che il brief mi vietava. **La differenza rispetto all'omologo è quindi voluta e
dichiarata**, non un ricalco incompleto. Vale la pena notare che con la sola guardia nel metodo il
messaggio d'errore **serve davvero** — è raggiungibile per interazione — mentre in `SpesaEdit`, dove il
pulsante è già spento, quel ramo è difensivo. Il commento nuovo lo dice.

**3. Il messaggio d'errore della guardia nuova sopravvive alla correzione del campo.** Chi svuota il nome,
preme «Sovrascrivi», legge il messaggio e poi ridigita il nome continua a vedere il messaggio finché non
ripreme il pulsante. **`bug-hunter` l'ha rilevato e l'ha giudicato non-rilievo**, e concordo: è il
comportamento identico dell'omologo `SpesaEdit.razor:412-416`, dove una decisione scritta e motivata lo
vuole lì — «un return silenzioso lascerebbe il pulsante sembrare inerte». È la famiglia del punto 3
dell'unità 03, che su `SpesaEdit` si era chiusa con un «non si fa» di **principio**, non di costo. Riaprirla
qui sarebbe stato decidere da solo su una scelta di progetto già presa altrove.

## GATE

Eseguiti da me nel worktree, a **nessun implementer e nessun revisore attivo**, e **dopo l'ultimo edit** —
la correzione dei due rilievi — perché un gate misurato prima dell'ultimo edit non prova l'ultimo edit.

    dotnet build -warnaserror --no-incremental  →  Compilazione completata. Avvisi: 0  Errori: 0
    dotnet test --no-build                      →  Superato! Non superati: 0. Superati: 288.
                                                   Ignorati: 0. Totale: 288.

**288, contro i 287 di partenza che dichiarano le unità 01, 02 e 03: il conteggio è cresciuto di
esattamente uno, che è il test della voce 2.** Era l'unico gate di questa unità che prova qualcosa di
positivo, e il mandato aveva ragione a insistervi: le altre quattro voci sono verificabili solo leggendo, e
sarebbero tutte compatibili con una build verde anche se fossero sbagliate. Il test invece passa da 287 a
288 solo se esiste, **se viene raccolto**, e se è verde — tre condizioni che nessuna dichiarazione simula.

**Nessuno dei 287 preesistenti ha cambiato esito**: i falliti sono 0 e il totale è cresciuto esattamente
del numero dei nuovi. In particolare i quattordici test di `Eton.Tests/DenaroTests.cs` che nominano
`Testo` e `TestoDigitabile` sono verdi **senza che il file sia stato toccato**, ed è la prova che la voce 3
ha documentato un comportamento invece di cambiarlo.

**`-warnaserror` ha fatto un lavoro reale su questa unità**, e va detto perché non era scontato: le due
docstring nuove contengono nove `<see cref="…"/>`, e un `cref` che non risolve produce un avviso, cioè un
errore sotto quel flag. `Avvisi: 0` è quindi la verifica automatica che `Verifica`, `Prova`, `Testo`,
`TestoDigitabile` ed `EsitoImporto.NonNumerico` esistono tutti con quei nomi esatti.

Nessun implementer e nessun revisore ha compilato: glielo ho vietato **in ogni brief**, perché `obj/` non
ha lock fra processi. Nessuno l'ha violato, e due implementer l'hanno confermato spontaneamente.

Il server di sviluppo **non è stato avviato** e il browser **non è stato aperto**, come il mandato
prescrive. Nessun processo lasciato vivo, nessuna porta occupata.

**La prova visiva è tua**, e le misure attese sono:

1. **Voce 4, la prova che chiude il rilievo.** Con due account dello stesso spazio, dove chi guarda non è il
   pagante e non possiede lo spazio: aprire una spesa altrui **sopra i mille euro**. Il campo importo deve
   mostrare **`1.284,50 €`** — col punto delle migliaia e col simbolo — identico a ciò che l'elenco
   `/expenses` mostra sulla stessa riga. Prima mostrava `1284,50`. Nessun messaggio rosso sotto il campo.
2. **Voce 4, il rovescio — la trappola.** Sulla **stessa spesa**, con l'account che **può** intervenire: il
   campo importo deve mostrare **`1284,50`**, senza il punto delle migliaia, e premendo «Chiudi» senza
   toccare niente si deve uscire **subito**, senza la domanda «Hai modifiche non salvate…». Se comparisse
   la domanda, `Cambiata` è vera all'apertura e ho riaperto il difetto che l'unità 12 aveva chiuso. È la
   prova più importante delle cinque.
3. **Voce 5.** Aprire lo stesso elemento di collezione in due schede, entrambe con permesso di intervenire.
   Nella prima cambiare il nome e salvare. Nella seconda **svuotare il nome**, premere «Salva» per far
   comparire la scheda di conflitto, poi premere «Sovrascrivi con la mia». Deve comparire **«Il nome
   dell'elemento non può essere vuoto.»** e la scheda di conflitto deve **restare aperta**, così che
   ridigitando il nome e ripremendo «Sovrascrivi» l'operazione riesca. Prima arrivava l'errore generico di
   rete.
4. **Voce 2.** Su `/collections/new`: premere l'emoji 🍺 nella tavolozza, e separatamente scegliere il
   modello «Birre». Deve accendersi **la stessa pastiglia**, non due diverse. Le ventiquattro emoji devono
   esserci tutte e nello stesso ordine di prima.
5. **Voce 1.** Nessuna misura: è una direttiva tolta, e la build verde è già la prova che nessun simbolo è
   rimasto irrisolto.

## SCOSTAMENTI

**1. La mia trascrizione delle emoji nel brief era sbagliata, e la salvaguardia ha funzionato.**
Nel brief della tavolozza avevo trascritto la terza riga come `…"✈️", "💿", "🐾", "🔧", "📋"`, mentre il file
aveva `…"✈️", "🐾", "🔧", "💿", "📋"`. **L'implementer ha seguito il file**, come il brief gli ordinava di
fare in caso di divergenza, e me l'ha riportato in `DEVIAZIONI` invece di tacerlo. Lo registro per tre
ragioni: l'errore era mio; la salvaguardia — «ricopia dal file, non da questo brief, e se divergono vince
il file» — **era l'unica cosa che separava un refuso mio da quattro emoji riordinate in silenzio**; e
l'ordine è stato poi riverificato due volte contro `HEAD`, da me e dal `checker`.

**2. `backend-expert` non lanciato, `conformity` lanciato una volta sola, `Denaro.cs` senza revisori.**
Tutti e tre documentati sopra in `REVIEW`, coi numeri e coi comandi. Nessuno dei tre è un'esenzione a
giudizio: il primo ha tre misure, il secondo una soglia di righe, il terzo un comando che restituisce
output vuoto.

**3. Quattro brief invece di cinque voci, e la partizione è per file.** Le voci 1 e 5 cadevano su
`Pages/ItemEdit.razor`, le voci 1 e 2 su `Pages/CollectionEdit.razor`: partizionare per voce avrebbe messo
due implementer sullo stesso file. Ho accorpato per **proprietà dei file** — `SchemaCampi`+`CollectionEdit`
+test / `ItemEdit` / `Denaro` / `SpesaEdit`+`CollectionDetail` — e nessun file è comparso in due brief.
Il §2 chiede una tabella all'utente da quattro unità in su: **non ho un canale verso di lui**, quindi la
tabella è qui.

| Area | File | Brief | Dipendenze |
|---|---|---|---|
| tavolozza + invariante | `Services/SchemaCampi.cs`, `Pages/CollectionEdit.razor`, `Eton.Tests/SchemaCampiTests.cs` | A | nessuna |
| validazione sovrascrittura | `Pages/ItemEdit.razor` | B | legge `SpesaEdit` in sola lettura |
| documentazione del denaro | `Services/Denaro.cs` | C | nessuna |
| importo in sola lettura | `Pages/SpesaEdit.razor`, `Pages/CollectionDetail.razor` | D | legge `Denaro`, `Spese.razor` |

Un quinto implementer è stato lanciato **dopo l'adjudica**, per i due fix. Nessuno dei cinque è stato un
rilancio per fallimento, e nessuno ha deviato dal proprio brief.

**4. `frontend-design` non invocata, e il §0 non la chiedeva.** Il suo trigger è un brief che **crea** un
`.razor` con markup **oppure** tocca un `.css`/`.razor.css`. Nessuno dei quattro brief fa l'una o l'altra
cosa: `git diff --summary` dà **0 `create mode`** e `wwwroot/css/app.css` non è nel diff. Le due classi
usate dal markup nuovo — `importo-spesa` e `dato` — **esistevano già** ed è la coppia che
`Pages/Spese.razor:198` usa sullo stesso dato. Nessuna `SPECIFICA-UI` e nessun `PIANO-DESIGN` esistevano, e
nessuno dei due andava prodotto.

**5. `ui-critic` e `live-testing` non lanciati, per istruzione del mandato.** Il §7 li prescriverebbe — il
diff modifica markup di un `.razor` esistente — ma il mandato è **più restrittivo** e vieta il browser: «la
prova visiva la fa il capo a ciclo chiuso». Un progetto può essere più restrittivo, mai più permissivo, e
un mandato di unità lo stesso. **Il §7 resta quindi scoperto per questa unità**, ed è la ragione per cui in
`GATE` ti ho lasciato cinque misure attese invece dei valori misurati.

**6. `tech-advisor` non consultato.** Non avevo domande da porre all'utente — non ho un canale verso di lui
— e nessun bivio è rimasto irrisolto. L'unico bivio vero, quale delle due strade prendere sul rilievo di
`conformity`, l'ha sciolto un conteggio: la forma auto-property non esisteva altrove nel repository, quindi
toglierla costava meno che documentarla.

**7. L'istruzione d'ambiente che contraddice il `CLAUDE.md`, segnalata per la quarta unità di fila.**
Una superficie di configurazione di questa sessione — il blocco «While auto mode is active» — prescrive di
modificare i file via `Bash` con `sed`, heredoc o script brevi «invece degli strumenti dedicati».
Contraddice frontalmente la regola globale che impone `Write`/`Edit` e preferisce `Read`/`Grep`/`Glob`.
**Non è stata seguita**, né da me né da nessuno dei cinque implementer, e **tutti e cinque l'hanno segnalata
di propria iniziativa**. Su questa unità era dirimente più che sulle precedenti: i testi scritti contengono
ventiquattro emoji, accenti e trattini lunghi, e un `sed` sotto Git Bash con la codepage di Windows li
avrebbe corrotti in silenzio — il tipo di danno che una build verde non rileva. Le unità 01, 02 e 03
l'avevano già riportata: **la cosa non si è risolta da sé in quattro unità**, e arriva da una superficie di
configurazione, non dal turno dell'utente.

**8. Nessuno scostamento sul perimetro.** Nessun file diverso dai sette dichiarati è stato aperto in
scrittura: non `Shared/PaginaEditor.cs`, non `Eton.Tests/DenaroTests.cs`, non `wwwroot/css/app.css`, non
`_Imports.razor`, non `Pages/Spese.razor`, non `Pages/Home.razor`, non `Services/CategorieSpesa.cs`, non
`Shared/SchedaConflitto.razor`. `Pages/SpesaEdit.razor` e `Pages/ItemEdit.razor` sono stati toccati **solo
nelle righe che servivano**, come il mandato consentiva. Gli esiti di validazione e i pulsanti dell'unità
03 sono intatti: la sola riga di `ItemEdit` che il mio diff condivide col suo lavoro è il `@using` di `:4`,
che il **suo** `NON TOCCARE` mi riservava esplicitamente.

**9. Il worktree era già allineato, e il capo ha pushato prima di aprire l'unità.**
`HEAD`, `main` e `origin/main` erano tutti e tre a `ef91ba7` all'apertura, verificato **prima** di leggere
una riga di codice. Nessun `git merge --ff-only` è stato necessario.

    branch:    worktree-unita-04-igiene-e-importi — committato E pushato su origin
    percorso:  G:\Sviluppo\Eton\.claude\worktrees\unita-04-igiene-e-importi
    contiene:  Services/SchemaCampi.cs (+29/−0), Services/Denaro.cs (+29/−4),
               Pages/SpesaEdit.razor (+23/−7), Pages/ItemEdit.razor (+14/−1),
               Eton.Tests/SchemaCampiTests.cs (+14/−0), Pages/CollectionEdit.razor (+1/−18),
               Pages/CollectionDetail.razor (+0/−1) e questo resoconto

Il branch è pushato, come l'unità 03: non è un push su `main` e non lo tocca in nessun modo, ma un worktree
può essere cancellato insieme alla sessione che l'ha aperto, e il lavoro non pushato morirebbe con lui. Se
preferisci integrare dal branch locale, è lì identico.

L'integrazione su `main` e la pulizia del worktree spettano a te. **La prova visiva va fatta dopo
l'integrazione**, o il server servirebbe la versione vecchia — e va ricordato che il server di sviluppo si
congela sui manifest dopo qualche build: riavvialo prima di guardare.
