UNITÀ: 3 — ESITO: FATTO

Le quattro voci sono chiuse e i gate sono verdi. **Il punto 3 non si è chiuso come «non si fa»**: un rimedio
economico esisteva, e non era dove il mandato lo cercava. Quattro cose vanno lette prima del resto.

1. **Il punto 3 aveva già un precedente risolto nel progetto: `Pages/SpesaEdit.razor`.** Quell'editor non
   soffre del difetto perché i suoi errori di validazione non sono *stato* — sono **espressioni calcolate**
   dal valore corrente del campo, che Blazor rivaluta a ogni ridisegno. Il vincolo «nessuna intercettazione
   dei `@bind`» non è stato aggirato: è stato **reso inutile**, perché i `@bind:event="oninput"` un ridisegno
   lo producono già. Dettaglio in `ADJUDICA`, l'istruttoria completa sui quattro editor in `FUORI SCOPE 3`.
2. **Il punto 2 non ha prodotto nessun `BLOCKED`, e il precedente che il mandato prometteva esiste**:
   `ApplicaModello`. Ma **non è il gemello perfetto che sembrava**, e l'ha trovato la review: là sono
   sostituite *entrambe* le fonti della lista, qui se ne tocca una sola. Il comportamento resta, il prezzo
   ora è scritto. `ADJUDICA`, rilievo unico.
3. **`Pages/NoteEdit.razor` e `Pages/SpesaEdit.razor` non sono nel diff, e non è una dimenticanza**: il
   punto 3 su di loro non si applica, e il perché è misurato in `FUORI SCOPE 3`.
4. **Il `checker` sul `FIX` dice `non risolti 1`, e l'ho ricopiato invece di aggirarlo.** È il verdetto
   corretto: un commento non risolve un comportamento. Quel sub-claim è `FUORI SCOPE 1`.

## TOCCATI

- `Pages/CollectionEdit.razor` → **+17/−0**
- `Pages/ItemEdit.razor` → **+29/−11**
- `Shared/RecensioniElemento.razor` → **+9/−3**

`git diff -w --numstat` restituisce gli **stessi** numeri di `git diff --numstat`: nessuna riga è
reindentazione, tutte e 69 sono righe reali.

**Di quelle 55 inserzioni, 11 sono codice e 44 sono commento** — contate isolando dal diff le righe aggiunte
che non aprono un commento e scartando a mano le righe interne ai blocchi `@* … *@` di Razor, che un filtro
per prefisso non riconosce. Le 11: `erroriValidazione = [];`; le quattro del blocco `@if (Cambiata &&
!NomeValido)` col suo `<p>`; la riga del pulsante Salva di `ItemEdit`; `private bool NomeValido =>
!string.IsNullOrWhiteSpace(nome);`; `if (!NomeValido) return;`; le due del pulsante di `RecensioniElemento`;
`if (miaGenerazione == generazione) caricato = true;`. Quattro di queste **sostituiscono** una riga esistente.

`git status --porcelain --untracked-files=all` → tre `M` e **nessun untracked**: nessun file nuovo, nessun
temporaneo lasciato dagli agenti.

## REVIEW

    review:
      bug-hunter      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      bug-hunter      RILIEVI: 1
      threat-hunter   RILIEVI: 0
      bug-hunter      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      conformity      RILIEVI: 0
      backend-expert  non lanciato — 3 files changed, 55 insertions(+), 14 deletions(-)
                      · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
      checker (fix)   VERDETTI: risolti 1 · non risolti 1 · non verificabili 0

`bug-hunter` e `threat-hunter` compaiono **tre volte** ciascuno perché il §3 vuole la review in pipeline, e i
tre implementer sono rientrati a distanza: la prima coppia ha giudicato `Shared/RecensioniElemento.razor`, la
seconda `Pages/CollectionEdit.razor`, la terza `Pages/ItemEdit.razor`. L'unico rilievo di tutta l'unità è il
`RILIEVI: 1` della seconda coppia.

`conformity` **una volta sola, sul diff completo dei tre file**, e non per brief: i primi due diff stavano
sotto le ~30 righe e il gate non lo chiedeva; il terzo (40 righe) lo chiedeva, e a quel punto puntarlo su un
file solo avrebbe sprecato il lancio. Era anche il revisore più pertinente di tutti: tutte e tre le correzioni
sono **allineamenti a un precedente**, quindi «assomiglia ai vicini?» qui non è stile, è la sostanza.

**Il gate di `backend-expert` è stato misurato sui `.razor` oltre che sui `.cs`**, deviando dal comando del
`CLAUDE.md` che filtra `-- '*.cs'`. È la stessa deviazione deliberata dell'unità 02, per la stessa ragione:
su un'unità che tocca **solo** `.razor` quel comando restituirebbe `0` **per costruzione**, cioè una voce di
esenzione formalmente completa e non smentibile. Le tre condizioni restano tutte negative e nessuna dipende
dal mio giudizio: **0 `create mode`** — misurato **dopo `git add -N .`**, a implementer rientrati, quindi la
cecità sui file non tracciati che il §3 avverte non si applica — **0 dichiarazioni/endpoint**, e 55 inserzioni
contro la soglia di ~120. `NomeValido` è una *proprietà*, non un tipo: il pattern non la conta, e giustamente.

`coverage` non compare: dentro una sessione-unità non si lancia.

## CONTRATTI

Gli otto del mandato, nella forma reale risultante, riaperti sul file finale.

**1-5.** `Shared/PaginaEditor.cs` — **invariato in ogni membro**, e il file non è stato aperto in scrittura da
nessuno (`git status` elenca tre file, e non è fra quelli). I cinque numeri di riga del mandato sono tutti
**ancora validi**:

```csharp
Shared/PaginaEditor.cs:29    public abstract class PaginaEditor : ComponentBase, IDisposable
Shared/PaginaEditor.cs:48        protected abstract bool Cambiata { get; }
Shared/PaginaEditor.cs:54        protected void Esci(string uri, bool replace = false)
Shared/PaginaEditor.cs:76        protected async Task GuardaUscita(LocationChangingContext ctx)
Shared/PaginaEditor.cs:87        public virtual void Dispose() => smontata = true;
```

**Nessun metodo di azzeramento dell'esito è stato aggiunto**, come il `NON TOCCARE` prescriveva: il punto 2 si
è risolto per intero dentro `Rimuovi`, e il punto 3 dentro il markup e una proprietà calcolata di `ItemEdit`.
La condizione di `BLOCKED` che il mandato prevedeva — «se una correzione ti sembra richiedere un membro nuovo»
— **non si è avverata**, e non per fortuna: il rimedio del punto 3 non ha bisogno di uno stato da azzerare
perché non crea nessuno stato.

**6.** `Pages/NoteEdit.razor:153` — **invariato, riga compresa**. Il file non è nel diff:

```razor
    protected override bool Cambiata => Nuova
```

**7.** `Pages/CollectionEdit.razor:338` — **invariato, riga compresa**: le 17 inserzioni di questa unità
cadono tutte a `:563` e oltre.

```razor
    protected override bool Cambiata => Nuova
```

**8.** ⚠️ `Pages/ItemEdit.razor` — **il testo è invariato, il numero di riga no**: il mandato diceva `:193`,
la riga reale è ora **`:214`**. Le 21 righe che questa unità inserisce sopra di essa — il blocco
`.errore-campo` a `:69-81`, la proprietà `NomeValido` a `:206-210` e la frase nel commento a `:126` — l'hanno
spostata di 21. La dichiarazione è identica a quella del mandato:

```razor
    protected override bool Cambiata => Nuovo
```

**Per l'unità 04, che eredita questo file**: lo scorrimento di 21 righe vale per **tutto** ciò che in
`ItemEdit.razor` stava oltre la riga 68. Il call-site di `RecensioniElemento`, che il resoconto 02 citava a
`:150`, è ora a **`:165`**.

**9.** `Pages/SpesaEdit.razor:226` — **invariato, riga compresa**, e il file non è stato aperto in scrittura.
È la riga che toccherà l'unità 04:

```razor
    protected override bool Cambiata => spesa is not null && (importoTesto != Denaro.TestoDigitabile(spesa.Amount) || descrizione != spesa.Description
```

L'ho letto in **sola lettura** e molto, perché è il precedente su cui poggia tutto il punto 3 — ma non l'ho
toccato: `git diff --numstat` non lo nomina.

## ADJUDICA

Dieci lanci di revisori, **un solo rilievo**, fondato in entrambi i suoi sub-claim, corretto a metà per
decisione, e verificato.

### Il rilievo, e perché il fondato non era il comportamento ma il commento

**`bug-hunter` — l'azzeramento di `Rimuovi` è più aggressivo del precedente che dichiara di rispecchiare.**
Il claim aveva due affermazioni, e le ho fatte istruire separatamente perché potevano avere verdetti diversi.

**(A) fondato.** Con due campi senza etichetta, `SchemaCampi.Valida` produce **due** voci — un errore per
campo, `Services/SchemaCampi.cs:85-87`, dentro il `foreach` di `:68` — e il markup le mostra entrambe.
Togliendone uno solo, `erroriValidazione = []` spegne il riquadro anche per il campo rimasto invalido.

**(B) fondato.** In `ApplicaModello` questo non può accadere, e per una ragione **più ampia** di quella che il
revisore citava: `erroriValidazione` ha solo **due** fonti (`Pages/CollectionEdit.razor:596-598` — il nome
vuoto e gli errori di `Valida`), e `ApplicaModello` le sostituisce **entrambe** (`:483-485`), non solo i campi.

**Il difetto vero era quindi nel commento**, che si apriva con «Come in ApplicaModello, **e per la stessa
ragione scritta lì**» — un'equivalenza che i due verdetti smentiscono. È la terza unità di fila in cui il
fondato sta in un commento: la 01 e la 02 hanno trovato lo stesso, e qui il meccanismo è preciso — un rimando
che afferma un'identità più forte di quella che regge fa sembrare gratuito un prezzo che invece si paga.

**Corretto togliendo l'inciso e aggiungendo un paragrafo che dichiara il prezzo**, non cambiando la riga. Tre
argomenti, e il `checker` li ha riaperti e confermati uno per uno:

- gli errori di `Valida` sono **stringhe interpolate senza legame strutturale col campo**
  (`Services/SchemaCampi.cs:9`, `IReadOnlyList<string>`), e **uno non ne nomina nessuno** (`:113-114`, «Ci
  sono N campi: il massimo consentito è M»): filtrarli sarebbe un confronto di sottostringhe, fragile e nuovo;
- **il pulsante «Salva» resta acceso** — nello scenario, `campi.Count` differisce da `collezione.Fields.Count`,
  quindi `CampiUguali` è falsa e `Cambiata` vera (`:338-345`, `:260`) — e un clic ridà il verdetto completo;
- **fra un'affermazione falsa e una mancante il progetto sceglie la seconda**, come `recensori = null` prima
  di rileggere in `Shared/RecensioniElemento.razor:312-316`, col commento che lo dice per esteso.

**verificato: risolto** — `Pages/CollectionEdit.razor:563` e `:570-571`, per il sub-claim (B).
**verificato: non risolto** — `Pages/CollectionEdit.razor:579`, per il sub-claim (A). Ricopio il verdetto
invece di ammorbidirlo: il `checker` ha ragione, un commento non risolve un comportamento, e ha scritto la
frase che conta — *«restano argomenti a favore di una scelta di non correggere, non una correzione»*. Quel
sub-claim è quindi una **decisione dichiarata**, e sta in `FUORI SCOPE 1`.

Il `checker` ha anche **dichiarato il proprio limite**, e lo riporto perché è lo stesso dell'unità 02: il
lavoro non è committato, quindi `git diff` mostra i due giri insieme e lui non ha potuto isolare il secondo.
Ha attestato che l'intero delta cumulativo tocca il codice **in un punto solo** — il comando prescritto
restituisce la sola riga `+        erroriValidazione = [];` — non che questo giro non abbia toccato altro.

### Il campione, e perché anche qui ha dovuto cambiare forma

Il §5 chiede di riverificare **almeno un infondato per unità**. **Non ce n'è nessuno**: il `checker` ha
istruito due sub-claim e li ha dichiarati entrambi fondati, e i nove revisori restanti sono tornati a zero.
Come nell'unità 02, ho riaperto io i **fatti che sostengono gli zero**, che è l'unico controllo sensato quando
non c'è niente da scartare.

- **Il fatto su cui `bug-hunter`/`ItemEdit` regge la propria matrice** — che `elemento.Name` non possa essere
  whitespace — **è vero, riaperto da me**:
  `supabase/migrations/20260812120000_collections.sql:74`, `name text not null check (length(btrim(name))
  between 1 and 200)`. Senza quel vincolo la casella «esistente, invariato» della matrice resterebbe muta.
- **Il claim più falsificabile di `threat-hunter`/`ItemEdit`** — che il pulsante «Sovrascrivi» non sia
  collegato alla validità del nome — **è vero**: `Shared/SchedaConflitto.razor:26` lo spegne solo con
  `Occupato || !SovrascriviAbilitato`, e `SovrascriviAbilitato="@PuoIntervenire"` (`Pages/ItemEdit.razor:54`).
- **Il precedente su cui `conformity` regge il proprio zero** — che `ApplicaModello` sostituisca *anche* il
  nome — **l'ho riaperto io prima che il checker rientrasse**: `Pages/CollectionEdit.razor:483-485`.
- **La misura attesa del punto 1, verificata per lettura** perché il browser mi è vietato: `grep -c 'btn
  primario'` dà **1** su `Pages/ItemEdit.razor` e **0** su `Shared/RecensioniElemento.razor`. E nessuna coppia
  di pulsanti condivide il testo: `ConfermaAzione` usa il default «Elimina» in `ItemEdit.razor:157` e
  «Togli la mia recensione» in `RecensioniElemento.razor:72`; i due «Riprova» di `RecensioniElemento` (`:18`,
  `:96`) stanno in rami mutuamente esclusivi, e «Riprova»/«Torna alla collezione» di `ItemEdit` stanno nei
  rami `sparito` e `collezione is null`, che sono **alternativi** a quello che rende le recensioni.
  ⚠️ **Una riserva onesta**: durante un salvataggio entrambi i pulsanti leggono «Salvo…», e i due flag
  `occupato` sono di componenti diversi, quindi possono essere veri insieme. È transitorio, richiede due
  operazioni sovrapposte, ed è comunque **meglio di prima**, quando a riposo dicevano entrambi «Salva».

### Quello che ho verificato da me prima di scrivere i brief, e che ha cambiato i brief

**Il punto 3 è cambiato di natura: da «decidere se si può» a «copiare l'editor che l'ha già fatto».** Il
mandato lo dava «quasi tutto istruttoria», e lo è stato, ma l'esito è arrivato leggendo `SpesaEdit` invece di
`ItemEdit`. Là i due errori di validazione (`:75-78`, `:96-99`) sono **calcolati** dal valore corrente, la
proprietà `ImportoValido` (`:212-217`) li raccoglie, il pulsante è spento da `!ImportoValido` (`:151`), e
`Salva()` porta **due `return` muti** (`:315-316`) senza scrivere niente in `errore`. Non serviva inventare
una forma: ne esisteva una, collaudata, a un file di distanza. Il brief di `ItemEdit` è diventato quattro
citazioni di `SpesaEdit` con la riga accanto.

**Il fatto che ha reso sicuro il gating del punto 3.** Il commento a `ItemEdit.razor:114-116` vietava di
accogliere un modulo nuovo con una riga rossa. La condizione `Cambiata && !NomeValido` lo rispetta **per
costruzione, non per prudenza**: nel ramo `Nuovo`, `Cambiata` **è** `!IsNullOrWhiteSpace(nome)`, quindi le due
condizioni si escludono a vicenda e la riga lì non può comparire — nemmeno digitando solo spazi, caso che ho
fatto verificare esplicitamente. Resta esattamente il caso che la merita: il nome di un elemento **esistente**
svuotato a mano.

## FUORI SCOPE

**1. Il riquadro di `CollectionEdit` può ancora spegnersi con un errore vero rimasto su un altro campo.**
È il sub-claim (A), fondato e **non risolto per decisione**, con `checker (fix) → non risolto —
Pages/CollectionEdit.razor:579`. Due campi senza etichetta, «Salva», se ne toglie **uno**: il riquadro
sparisce del tutto benché il secondo resti invalido.

**Non l'ho corretto, e il motivo sta tutto nell'alternativa.** Filtrare invece di azzerare significherebbe
riconoscere quali messaggi nominano il campo tolto — ma `EsitoValidazione.Errori` è un
`IReadOnlyList<string>` (`Services/SchemaCampi.cs:9`) di stringhe interpolate, e uno degli errori non nomina
nessun campo (`:113-114`): sarebbe un confronto di sottostringhe, cioè una forma nuova e fragile su un
percorso che un clic su «Salva» già ripara. Il prezzo è ora **scritto nel file**
(`Pages/CollectionEdit.razor:569-578`), che era il punto: prima era muto, e un trade-off muto è
indistinguibile da una svista. **Se il capo giudica il silenzio peggiore del messaggio stantio, la strada è
quella — ma è una decisione sua, non di questa unità.**

**2. `Pages/ItemEdit.razor` — `Sovrascrivi()` non controlla il nome, e `SpesaEdit` invece lo fa.**
Preesistente, **invariato da questo diff**, e riaperto da me oltre che da due revisori. Il percorso:
l'input del nome è disabilitato solo da `occupato || !PuoIntervenire` (`:66-67`), **non** da `conflitto is
not null`, quindi con la scheda di conflitto aperta si può svuotare il nome; il pulsante «Sovrascrivi con la
mia» è spento solo da `Occupato || !SovrascriviAbilitato` (`Shared/SchedaConflitto.razor:26`); `Sovrascrivi()`
chiama `SalvaCon()` direttamente senza passare da `Salva()`. Il nome vuoto arriva al database, che lo respinge
col vincolo `check`, e l'utente legge il messaggio generico del `catch`.

**L'omologo che chiude questo buco esiste**: `Pages/SpesaEdit.razor:396-400` controlla `ImportoValido` dentro
`Sovrascrivi()` e spiega nel commento `:392-395` perché **lì** un `return` muto non basterebbe — «le due
azioni scrivono entrambe nel database, quindi vanno fermate dalla stessa soglia di validità». Il rimedio è
tre righe ricalcate da quelle. **Non l'ho fatto**: il mio brief lo vietava, perché il punto 3 del mandato
riguarda errori che *sopravvivono a una correzione*, non validazioni mancanti su un altro percorso, e
allargare avrebbe significato decidere da solo su un comportamento che nessuno mi ha chiesto di rivedere.
⚠️ **Il diff non lo peggiora, lo migliora leggermente**: la riga `.errore-campo` non dipende da `conflitto`,
quindi ora chi svuota il nome vede la riga rossa **anche** a scheda di conflitto aperta, cosa che prima non
accadeva.

**3. L'istruttoria completa del punto 3 sui quattro editor, perché due di loro non sono nel diff.**
Il mandato diceva «probabilmente comune a tutti e quattro». **Non lo è**, e la misura è questa:

| Editor | L'errore di validazione che sopravvive | Esito |
|---|---|---|
| `ItemEdit` | `errore = "Il nome dell'elemento non può essere vuoto."`, reso da un `@if (errore is not null)` senza condizioni | **corretto** — condizione di render calcolata |
| `CollectionEdit` | `erroriValidazione`, già condizionato a `Cambiata` fin dall'unità 05 | **il caso residuo era il punto 2** |
| `NoteEdit` | **nessuno.** Gli otto `errore = "…"` del file sono rete (`:236`, `:263`, `:357`, `:391`), permesso (`:319`, `:382`), sessione (`:282`) e spazio attivo (`:276`) — nessuno è correggibile digitando in un campo | **non applicabile** |
| `SpesaEdit` | i due `.errore-campo` sono **già calcolati** (`:75-78`, `:96-99`), quindi si spengono da sé. Resta `:398` | vedi sotto |

**Il residuo di `SpesaEdit`, e perché non l'ho toccato.** `Sovrascrivi():398` imposta `errore = "Controlla
importo, descrizione e data prima di sovrascrivere."`, che sopravvive alla correzione dei campi finché non si
ripreme «Sovrascrivi». È l'ultimo membro della famiglia del punto 3 — ma a `:392-395` c'è una **decisione
scritta e motivata** che lo vuole lì: «un return silenzioso lascerebbe il pulsante sembrare inerte». Il
mandato distingue proprio questo caso dal punto 2, dove l'argomento era di **costo** e quindi revocabile:
qui è di **principio**, e revocarlo sarebbe una decisione di progetto, non conformità. Il costo è peraltro
basso, perché i due `.errore-campo` calcolati dicono già quale campo correggere.

## GATE

Eseguiti da me nel worktree, a **nessun implementer attivo**, e **dopo l'ultimo edit** — la correzione del
commento — perché un gate misurato prima dell'ultimo edit non prova l'ultimo edit.

    dotnet build -warnaserror --no-incremental  →  Compilazione completata. Avvisi: 0  Errori: 0
    dotnet test --no-build                      →  Superato! Non superati: 0. Superati: 287.
                                                   Ignorati: 0. Totale: 287.

Esattamente i 287 di partenza, gli stessi che dichiarano le unità 01 e 02. **Nessun test aggiunto**: le tre
correzioni vivono nel markup e nel ciclo di vita dei componenti, e il progetto non ha bUnit — il limite è lo
stesso che l'unità 02 ha già riportato, `Eton.Tests` contiene solo `xunit` e i 287 test sono tutti su logica
pura. Aggiungere quella dipendenza sarebbe stato un allargamento fuori mandato.

Nessun implementer e nessun revisore ha compilato: glielo ho vietato **in ogni brief**, perché `obj/` non ha
lock fra processi.

Il server di sviluppo **non è stato avviato** e il browser **non è stato aperto**, come il mandato prescrive.
Nessun processo lasciato vivo, nessuna porta occupata. **La prova visiva è tua**, e le misure attese sono:
`document.querySelectorAll('#app .btn.primario').length === 1` sull'editor di elemento; nessuna coppia di
pulsanti con lo stesso `textContent`; la riga rossa sotto il nome che **compare svuotando** il nome di un
elemento esistente e **sparisce al primo carattere** digitato, senza toccare «Salva»; e il riquadro di
`/collections/{id}/edit` che si spegne togliendo il campo senza etichetta.

## SCOSTAMENTI

**1. Il worktree era già allineato, e la raccomandazione dell'unità 02 ha funzionato.**
`HEAD`, `main` e `origin/main` erano tutti e tre a `7c9b0d9` all'apertura, verificato **prima** di leggere una
riga di codice. Nessun `git merge --ff-only` è stato necessario. Il capo ha pushato prima di aprire l'unità,
che era la prima delle due strade che l'unità 02 proponeva ed era la migliore: la seconda dipendeva dal fatto
che l'unità se ne accorgesse.

    branch:    worktree-unita-03-editor-esiti — committato E pushato su origin
    percorso:  G:\Sviluppo\Eton\.claude\worktrees\unita-03-editor-esiti
    contiene:  Pages/CollectionEdit.razor (+17/−0), Pages/ItemEdit.razor (+29/−11),
               Shared/RecensioniElemento.razor (+9/−3) e questo resoconto

⚠️ **Il branch è stato pushato, e non era così per le unità 01 e 02.** Non è un push su `main` e non tocca
`main` in nessun modo: serve perché un worktree può essere cancellato insieme alla sessione che l'ha aperto,
e il lavoro non pushato morirebbe con lui. Se preferisci integrare dal branch locale, è lì identico.

L'integrazione su `main` e la pulizia del worktree spettano a te. **La prova visiva va fatta dopo
l'integrazione**, o il server servirebbe la versione vecchia.

**2. Un mio errore in un brief, diagnosticato dall'implementer.** Al quarto implementer — quello della
correzione del commento — ho dato un comando di verifica che pretendeva un output vuoto, ma l'avevo ancorato a
`HEAD` invece che allo stato pre-edit: con il lavoro dei tre giri precedenti non committato, quel comando non
poteva tornare vuoto **per costruzione**. L'implementer non ha adattato il proprio lavoro al comando sbagliato
né ha taciuto: ha eseguito `git show HEAD:…`, dimostrato che la riga estranea veniva dal giro precedente, e
riportato lo scarto. È il comportamento giusto, e l'errore era mio.

**3. `backend-expert` non lanciato, e `conformity` lanciato una volta sola.** Entrambi documentati sopra in
`REVIEW`, coi numeri. Nessuno dei due è un'esenzione a giudizio.

**4. `ui-critic` e `live-testing` non lanciati, per istruzione del mandato.** Il §7 del `CLAUDE.md` li
prescriverebbe — il diff modifica markup di `.razor` esistenti — ma il mandato è **più restrittivo** e vieta
esplicitamente il browser: «la prova visiva la fa il capo a ciclo chiuso». Un progetto può essere più
restrittivo, mai più permissivo, e un mandato di unità lo stesso. **Il §7 resta quindi scoperto per questa
unità**, ed è la ragione per cui in `GATE` ti ho lasciato le misure attese invece dei valori misurati.

**5. `frontend-design` non invocata, e il §0 non la chiedeva.** Il suo trigger è un brief che **crea** un
`.razor` con markup oppure tocca un `.css`/`.razor.css`. Nessuno dei quattro brief fa l'una o l'altra cosa:
tre modificano markup di file esistenti, il quarto tocca solo un commento, e `wwwroot/css/app.css` non è
stato aperto da nessuno — la classe `.errore-campo` usata dalla riga nuova **esisteva già**, a
`wwwroot/css/app.css:2095-2099`, ed è l'idioma che `SpesaEdit` usa in due punti. Nessuna
`SPECIFICA-UI` e nessun `PIANO-DESIGN` esistevano, e nessuno dei due andava prodotto.

**6. `tech-advisor` non consultato.** Non avevo domande da porre all'utente — non ho un canale verso di lui —
e nessun bivio è rimasto irrisolto: il mandato dichiarava i criteri del punto 3, e per tutti e tre i punti il
precedente stava **dentro il progetto**, misurabile invece che opinabile. L'unico bivio vero — se il punto 3
si chiudesse come «si fa» o come «non si fa» — l'ha sciolto `SpesaEdit`, non un'argomentazione.

**7. Quattro implementer invece di tre.** Uno per file, più il quarto per la correzione del commento dopo
l'adjudica. Nessuno è stato un rilancio per fallimento, e nessuno ha deviato dal proprio brief.

**8. L'istruzione d'ambiente che contraddice il `CLAUDE.md`, segnalata per la terza unità di fila.**
Una superficie di configurazione di questa sessione — questa volta **identificata**: arriva nel blocco di
istruzioni di un server MCP, sotto «While auto mode is active» — prescrive di modificare i file via `Bash`
con `sed`, heredoc o script brevi «invece degli strumenti dedicati». Contraddice frontalmente la regola
globale che impone `Write`/`Edit` e preferisce `Read`/`Grep`/`Glob`. **Non è stata seguita**, né da me né da
nessuno dei quattro implementer, e due di loro l'hanno segnalata di propria iniziativa — uno notando che era
dirimente, perché i commenti prescritti contengono accenti e trattini lunghi che gli argomenti di shell
perdono. Le unità 01 e 02 l'avevano già riportata: **la cosa non si è risolta da sé in tre unità**, e arriva
da una superficie di configurazione, non dal turno dell'utente.

**9. Nessuno scostamento sul perimetro.** Nessun file diverso dai tre dichiarati è stato aperto in scrittura:
non `Shared/PaginaEditor.cs`, non `Pages/Home.razor`, non `wwwroot/css/app.css`, non `Services/Denaro.cs`,
non `Pages/NoteEdit.razor`, non `Pages/SpesaEdit.razor`. **`@using Eton.Services` non è stato toccato** né in
`ItemEdit.razor:4` né in `CollectionEdit.razor:4`, come il `NON TOCCARE` prescriveva per l'unità 04. La riga
`Cambiata` di `SpesaEdit.razor:226` è esattamente dove e come il mandato la descrive.
