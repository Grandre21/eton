# UNITÀ 03 — resoconto

```
UNITÀ: 3 — ESITO: FATTO
```

**Branch:** `worktree-unita-03-accesso-e-profilo` — **percorso:**
`G:\Sviluppo\Eton\.claude\worktrees\unita-03-accesso-e-profilo`
Nato da `43b232a`. Quattro commit, ognuno fatto appena il pezzo era coerente, come l'avviso
sulla sessione morta stanotte prescriveva. L'integrazione su `main` la fai tu.

**Nessuna riga di `wwwroot/css/app.css`**, come il mandato garantiva: `git diff 43b232a --stat`
non lo nomina. La garanzia non è stata spesa.

---

## TOCCATI

```
Services/BrowserSessionHandler.cs  → +34/−1
Services/AllineatoreProfilo.cs     → +16/−0
Services/AuthStateService.cs       → +13/−1
Pages/Profile.razor                → +1/−1
```

Quattro file, tutti del perimetro assegnato. `git status --porcelain` non ne elenca altri.
**`Services/SupabaseService.cs` non è stato toccato**, e il mandato me lo assegnava: v. la
decisione sulla voce 15, dove il non toccarlo è parte della risposta e non una rinuncia.

---

## REVIEW

Tre brief di implementazione, partizionati per proprietà dei file e senza sovrapposizioni.
Il gate del §3 è stato valutato **sul diff di ciascuna**, con `git add -N .` eseguito prima
delle misure e a implementer fermi.

**A** = `BrowserSessionHandler.cs` (voce 15) · **B** = `AllineatoreProfilo.cs` +
`AuthStateService.cs` (voce 17) · **C** = `Pages/Profile.razor` (voce 18).

```
review, unità B+C:
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  backend-expert  non lanciato — 3 files changed, 24 insertions(+), 2 deletions(-)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
  checker (fix)   VERDETTI: risolto 1 · non risolto 0 · non verificabili 0

review, unità A:
  bug-hunter      RILIEVI: 2
  conformity      RILIEVI: 2
  threat-hunter   RILIEVI: 0
  backend-expert  non lanciato — 1 file changed, 31 insertions(+), 1 deletion(-)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         VERDETTI: fondati 3 · infondati 1 · fuori scope 0 · non verificabili 0
  checker (fix)   VERDETTI: risolti 3 · non risolti 0 · non verificabili 0
```

`coverage` non compare perché dentro una sessione-unità non si lancia (§6).

Tre note sul tracciato, tutte necessarie perché sia verificabile:

1. **Le righe di `--stat` delle due voci `non lanciato` sono ricopiate dall'output dei comandi**,
   non riscritte come numeri miei: `git show --stat --summary --format="" a89b55e` e lo stesso su
   `fb39b00`. Nessuna delle due contiene `create mode`, perché nessuna unità ha creato file.
2. **`conformity` è stato lanciato su entrambe le unità benché il gate non lo imponesse**
   (24 e 31 righe, cioè sotto la soglia delle ~30 per B+C e al limite per A). Su A l'ho voluto
   perché quel diff **introduce deliberatamente un'asimmetria fra metodi fratelli**, che è
   esattamente la sua domanda; ha prodotto 2 rilievi, uno fondato. L'esenzione sarebbe costata
   una convenzione violata.
3. **`checker` compare due volte per unità**, ed è la stessa riga per due passi diversi: la prima
   è l'istruttoria del §4 sui rilievi, la seconda è il rilancio sul **fix** che il §5 impone. Le
   tengo distinte perché un fix istruito e uno dichiarato risolto da chi l'ha scritto non hanno
   lo stesso peso.

Fuori dal tracciato del §3, due agenti chiamati per **decidere** e per **verificare**, non per
revisionare il diff: `doc-checker` (la meccanica di Gotrue 6.3.0, prima di scrivere il brief A) e
`tech-advisor` (il bivio della voce 15).

### ⚠️ Due difetti nel lavoro dei revisori, che vanno detti perché pesano sul credito da dargli

- **`conformity` su A dichiara di aver consultato il pacchetto NuGet `gotrue-csharp` 4.2.7**,
  mentre il progetto installa **Supabase.Gotrue 6.3.0** (`Eton.csproj:38`). Il `checker` ha
  verificato che nessuno dei suoi due rilievi dipendeva da quella lettura — vertono su convenzioni
  interne — quindi non li ha declassati. Ma è una lettura fatta sulla versione sbagliata.
- **La prova del suo secondo rilievo era aritmeticamente impossibile**: «68 volte in 30 file sotto
  `Services/`», dove `Services/` contiene **28** file in tutto. L'ha notato il `checker`, non io e
  non il revisore.

---

## LA VOCE 16, VERIFICATA — la tua decisione regge

Il fatto su cui si regge la decisione di non toccarla è **confermato**, e la riga è questa:

```
Program.cs:22    builder.Services.AddSingleton(sp => new RottaRichiesta((IJSInProcessRuntime)sp.GetRequiredService<IJSRuntime>()));
Program.cs:23    builder.Services.AddSingleton(sp => new AllineatoreProfilo((IJSInProcessRuntime)sp.GetRequiredService<IJSRuntime>()));
```

Una riga sopra `AllineatoreProfilo`, `RottaRichiesta` è registrata con la **stessa identica forma**,
carattere per carattere salvo il nome del tipo: `AddSingleton` con una factory che fa il cast di
`IJSRuntime` a `IJSInProcessRuntime` e costruisce a mano. Ed è un servizio della **stessa famiglia**
— un wrapper su web storage che riceve il runtime JS sincrono: `RottaRichiesta.cs:16` dichiara
`private const string StorageKey = "eton.rotta-richiesta"`, come `AllineatoreProfilo.cs:21`
dichiara `"eton.profilo"`.

La premessa del rapporto di chiusura — «i suoi due fratelli identici sono costruiti a mano dentro
`SupabaseService`» — è vera **solo per due**: `BrowserSessionHandler` (`SupabaseService.cs:55`) e
`PkceStore` (`:57`). Il terzo fratello è registrato in DI esattamente come lui. **Le due convenzioni
coesistono davvero**, e la ricognizione non si sbaglia: non l'ho toccata.

---

## LA VOCE 15 — la decisione su ciascuno dei due metodi

**La premessa della voce era incompleta, e il fatto nuovo la ribalta.** Non è vero che i due metodi
fossero «scoperti»: erano **muti**. Verificato da `doc-checker` sul sorgente del commit
`d67eb156a41d1f1552c2016dafed036383bc018d`, che il `.nuspec` del pacchetto 6.3.0 installato dichiara
come propria origine, e riverificato indipendentemente dal `bug-hunter` su A:

> Gotrue invoca la persistenza **solo** da `PersistenceListener.EventHandler` — gli unici tre
> call-site di `.SaveSession(` in tutta la libreria stanno lì — e ogni invocazione passa dentro
> `Client.NotifyAuthStateChange`, che fa `try { handler.Invoke(...) } catch (Exception e) {
> _debugNotification?.Log(...) }` e **non rilancia mai**. Eton non registra nessun debug handler
> (`grep` su `AddDebugListener|DebugNotification|AddStateChangedListener` → **0**), quindi quel
> `?.Log` è un no-op reale.

**Nessuna eccezione di quei due metodi è mai risalita a noi, e nessuna è mai comparsa da nessuna
parte.** Il principio del ciclo scorso — si protegge dove esiste un ripiego onesto — presuppone che
*non* proteggere significhi **non partire**: l'eccezione risale e ferma qualcosa. Qui non ferma
niente. L'applicazione prosegue comunque, convinta di aver salvato, e in più nessuno lo sa.

Il bivio è stato portato a `tech-advisor` **prima** di scrivere il brief, con le opzioni in chiaro e
l'invito esplicito a smentirmi. Ha concordato sulla direzione e su `DestroySession` ha portato un
argomento **più forte del mio**, che ho adottato scartando il mio.

### `SaveSession` → **protetto**, e il `catch` non è una protezione

Un `try`/`catch` che registra su `Console.Error` col marcatore `[Auth]`, senza `throw;`. Non cambia
un solo comportamento osservabile — l'eccezione era già catturata a valle — ma è **l'unica traccia
possibile** di un fallimento che oggi sparisce del tutto. Il `<summary>` dichiara che il `catch` non
protegge, altrimenti mentirebbe sul proprio scopo.

Niente `throw;` perché non esiste un chiamante da informare: `grep` di `.SaveSession(` nel progetto
→ **0**. Il contratto della classe diventa uniforme: `LoadSession` e `SaveSession` non lanciano mai.

Il caso realistico non è l'archiviazione vietata — quella la intercetta prima `PkceStore` sul
percorso d'accesso, e il `bug-hunter` l'ha verificato su entrambi i rami, andata e ritorno — ma la
**quota piena**, in cui leggere riesce e scrivere no.

### `DestroySession` → **non protetto**, e non esiste un ripiego onesto da inventare

Non l'ho lasciato nudo per inerzia: l'ho lasciato nudo perché proteggerlo **peggiorerebbe** tre cose,
tutte verificate.

1. **Per la cancellazione l'applicazione già sa se è fallita.** `SignOutAsync` dopo la
   `DestroySession` diretta richiama `_auth.LoadSession()`: se il `removeItem` non ha funzionato la
   sessione viene riletta, `CurrentSession` torna non-null, `uscito` è `false`, la riga
   «Logout NON riuscito» viene stampata e `LogoutAsync` naviga con `forceLoad: true`. Catena
   verificata anello per anello dal `bug-hunter` e dal `threat-hunter`, che ha cercato — senza
   trovarlo — un percorso in cui `removeItem` fallisca **e** `uscito` risulti `true`.
2. **Renderebbe morti due `catch` esistenti**, uno dei quali stampa un messaggio mirato al logout.
3. **In un solo logout stamperebbe la stessa riga quattro volte**, perché il listener di Gotrue
   invoca questo metodo **tre** volte per ogni uscita — conteggio tracciato passo-passo dal
   `bug-hunter` sul sorgente — più la chiamata diretta.

E c'è una quarta ragione che vale da sola: proteggerlo **falsificherebbe** il commento di
`SupabaseService.cs`, che afferma testualmente che i chiamati «`SignOut`, `DestroySession`,
`LoadSession` — lanciano davvero». Quel file il mandato mi diceva di trattare con cautela: non
toccarlo è parte della risposta.

**Resta quindi una dichiarazione, non una protezione**: un `<summary>` che spiega perché
l'asimmetria non è una svista da uniformare. Senza, la regressione sarebbe a un passo e sembrerebbe
una pulizia — è la stessa lezione che il ciclo scorso ha imparato su `PkceStore.Salva`.

---

## CONTRATTI

Il mandato non esponeva firme nuove ad altre unità, e nessuna ne è nata. Una firma **interna** è
cambiata, e la dichiaro perché è l'unica del diff:

```
Services/AuthStateService.cs:16   public AuthStateService(SupabaseService supabase, NavigationManager navigation, SpaceStateService spazi, AllineatoreProfilo profilo)
```

Un parametro in più, in coda. **Nessun consumatore da aggiornare**: `AuthStateService` è costruito
solo dalla DI (`Program.cs:13`), e `grep` di `new AuthStateService(` in tutto il progetto → **0**.
`Program.cs` non è stato toccato, perché `AllineatoreProfilo` era già registrato a `:23`.

L'altra firma nuova, non esposta fuori dal proprio file più il suo unico chiamante:

```
Services/AllineatoreProfilo.cs:94   public void Dimentica()
```

### Il vincolo di forma ereditato — rispettato, e contato

**Zero rimandi per numero di riga** fra le righe aggiunte, su tutti e quattro i file. Misurato da
`conformity` con due pattern diversi su B+C e riverificato su A. I `file:line` di questo resoconto e
dei brief sono per te e per me, e non compaiono in nessun commento del codice.

---

## ADJUDICA

**Quattro rilievi, tutti aperti da me di persona**, perché tre toccano dati o concorrenza — che il
§5 impone di aprire qualunque sia il verdetto — e il quarto è l'infondato del campione.

### B+C · `bug-hunter`, severity alta — il commento prometteva una garanzia assoluta. FONDATO → corretto

Il commento che avevo dettato io diceva che l'impronta del profilo «viene scritta solo dal bootstrap
di `GetClientAsync`, **mai** da una navigazione a sessione ancora viva». Il `checker` l'ha istruito
così, ed è la formula esatta: **vera alla lettera, fuorviante per omissione**.

Vera, perché dentro la scheda la corsa non si dà davvero: `AuthRedirect` attende `GetClientAsync()`
per intero prima di rendere il contenuto privato, e `primoAvvio` è vero una volta sola per istanza.
Fuorviante, perché il canale scoperto non è «una navigazione» ma **un'altra scheda**, il cui
bootstrap può concludere la propria scrittura dopo questo logout e rimettere `eton.profilo` in
`localStorage`.

Il commento ora nomina quel canale invece di tacerlo.
`verificato: risolto — Services/AuthStateService.cs:67-74`

**Non ho corretto il codice, e il perché è nella sezione `FUORI SCOPE`**: la corsa è una proprietà
del modello a più schede e vale identica su `eton.spazio`, che questo diff non tocca.

### A · `bug-hunter`, severity media — «entrambi i chiamanti registrano» era falso. FONDATO → corretto

Il `catch` annidato in `LoadSession` è **vuoto**: nello scenario descritto il fallimento della
cancellazione non stampa nulla, e in console comparirebbe solo la riga sul problema originale. Avevo
generalizzato da uno a due senza guardare. Il testo ora distingue i due chiamanti e dice perché il
secondo ingoia di proposito — la deliberatezza è dichiarata nel codice, non inventata dal commento.
`verificato: risolto — Services/BrowserSessionHandler.cs:52-57`

### A · `bug-hunter`, severity bassa — «non sopravvivrà alla ricarica» era impreciso. FONDATO → corretto

Falso proprio nel caso che il commento indica come realistico: se a fallire è il salvataggio di un
token **rinnovato**, in `localStorage` resta la sessione precedente e alla ricarica viene riletta.
Il messaggio ora dice cosa resta davvero.
`verificato: risolto — Services/BrowserSessionHandler.cs:43`

⚠️ **Il `checker` ha aggiunto un fatto che non chiude del tutto**: quella sopravvivenza è al più
transitoria, perché al bootstrap successivo il refresh con un token già ruotato cade su
`InvalidRefreshToken` e `RinnovaSessioneAsync` slogga da sé — **a meno che** il backend GoTrue non
abbia una finestra di riuso, che non è verificabile da questo repository. Il messaggio nuovo è vero
in entrambi i casi, quindi la sfumatura non lo tocca; la annoto perché è l'unico punto
dell'istruttoria che dipende da una configurazione server.

### A · `conformity`, severity media — `<c>` invece di `<see cref>` su un pubblico di progetto. FONDATO → corretto

`verificato: risolto — Services/BrowserSessionHandler.cs:50 e :53`

⚠️ **Ma la prova del revisore non reggeva, e l'ho riverificata io.** Diceva «zero eccezioni per
simboli di progetto»: è falso, ce ne sono due, entrambe nel file che lui stesso porta come
precedente — `PkceStore.cs:22` (`<c>Benvenuto.Accedi</c>`) e `:30`
(`<c>SupabaseService.ScambiaCodiceAsync</c>`). Il `checker` ha trovato la regola vera, che è più
fine: **entrambi i controesempi puntano a metodi privati**, mentre tutti i `<see cref>` puntano a
pubblici. `SignOutAsync` è pubblico, quindi il rilievo regge — per la regola giusta, non per quella
addotta.

⚠️ **E l'argomento tecnico è caduto.** Il revisore sosteneva che un `cref` sarebbe verificato dal
compilatore e che con `-warnaserror` un rinominamento romperebbe la build. **Falso in questo
progetto**: `Eton.csproj` non ha `GenerateDocumentationFile` — verificato io, `grep` → **0** — quindi
CS1574 non viene mai emesso. La convenzione resta; la garanzia automatica non c'è. Lo dico perché
l'avevo ripreso anch'io prima di verificarlo.

### A · `conformity`, severity bassa — i `<summary>` lunghi senza `<para>`. INFONDATO → scartato

**È l'infondato che il §5 mi impone di riverificare a campione, ed è l'unico: l'ho riverificato io**,
non mi sono fermato al verdetto.

La convenzione di progetto **esiste**, e più di quanto il revisore stesso dicesse: `<para>` compare
in **26 file su 28** sotto `Services/`. Ma i due che non lo usano sono esattamente
`BrowserSessionHandler.cs` e `PkceStore.cs` — `grep -c "<para>"` → **0** su entrambi — e il
`<summary>` di `LoadSession`, **otto righe e tre temi**, preesistente e nello stesso file, ne fa a
meno. Un rilievo che imponga a un file una convenzione che il file non ha mai seguito, e che il suo
vicino più prossimo non segue, non regge: **infondato per questo file**, non per il progetto. Se un
domani si volesse uniformare, il posto giusto è una decisione di progetto su entrambi i file, non
una correzione dentro questa unità.

---

## FUORI SCOPE

### 1. La corsa fra schede riscrive dopo il logout, e vale anche su una chiave che non ho toccato

**Fondato, istruito, non risolto**, e non è un rilievo lasciato cadere: è la parte del rilievo di
`bug-hunter` che non appartiene a questa unità.

Il fatto: ogni scheda ha la **propria** istanza dei servizi. Il bootstrap di una seconda scheda può
concludere la propria scrittura dopo il logout della prima e rimettere in `localStorage` la chiave
appena cancellata. Vale per `eton.profilo`, e vale **identico** per `eton.spazio`:
`SpaceStateService.cs:25` dichiara `private int _generazione;` — un campo **di istanza**, verificato
da me — che protegge dalla corsa interna alla scheda e non da quella fra schede. Il commento a
`:99-105` lo dichiara esplicitamente per il caso intra-scheda, e infatti non copre l'altro.

Nessun meccanismo di sincronizzazione fra schede esiste nel progetto: il `checker` ha cercato
`addEventListener('storage')` e `BroadcastChannel` senza trovarne.

**Perché non l'ho risolto io.** Il rimedio che il revisore proponeva — controllare che `eton.session`
esista ancora subito prima di scrivere l'impronta — chiuderebbe lo scenario come descritto, ma
tocca `AllineaAsync`, che è codice preesistente fuori dalle tre voci del mandato; non sarebbe
ermetico, perché controllo e scrittura non sono atomici fra schede; e soprattutto **chiuderebbe metà
del problema lasciando l'altra aperta**, dando alla pulizia al logout un'apparenza di simmetria che
non avrebbe. È una decisione di progetto su due servizi, non un fix di unità.

**Il fatto è comunque scritto dove serve**: il commento di `LogoutAsync` lo dichiara, così chi legge
non ricava più una garanzia che il codice non dà.

### 2. Con la sessione non salvata, l'impronta del profilo viene scritta lo stesso

Notato dal `threat-hunter` su A, e non elevato a rilievo perché è fuori dal diff. `GetClientAsync`
chiama `AllineaAsync` indipendentemente dall'esito di `SaveSession`: se il salvataggio della sessione
è fallito per quota piena, sul dispositivo può restare `eton.profilo` — id utente, nome, foto — senza
la sessione che lo giustifica. Il caso è stretto (serve una quota che si riempia fra le due
scritture) e preesistente al diff; lo riporto perché appartiene alla stessa famiglia della voce 17.

### 3. `eton.rotta-richiesta` non viene dimenticata al logout, ed è corretto così

Inventario completo fatto dal `threat-hunter`: le chiavi sono **cinque**, non quattro. La quinta sta
in `sessionStorage`, la scrive `AuthRedirect` **solo** nel ramo in cui l'utente non è autenticato, e
`Consuma()` la legge e la rimuove nello stesso gesto. Contiene un percorso interno validato, nessun
identificatore. **Non è un artefatto dell'utente uscente**: resta fuori per costruzione, non per
dimenticanza. Lo scrivo perché il prossimo che conti le chiavi non la prenda per una quinta
dimenticanza.

---

## GATE

```
dotnet build Eton.sln -warnaserror --no-incremental  →  Avvisi: 0, Errori: 0
dotnet test Eton.sln                                 →  Superati: 310, Non superati: 0, Totale: 310
```

Eseguiti da me sullo stato finale committato, **a implementer fermi**: `obj/` non ha lock fra
processi, e ogni brief portava il divieto esplicito di compilare. Nessun implementer lo ha violato.

**Baseline misurata sul worktree appena aperto, prima di toccare qualunque cosa**: 0 avvisi,
310/310. Il mandato chiedeva «310/310, o più se ne aggiungi»: **non ne ho aggiunti**, e il perché è
nello scostamento 2.

**Server di sviluppo non avviato, browser non aperto**, come il mandato vieta: nessun processo
lasciato vivo, nessuna porta occupata. Il `dotnet` sulla 5000 è il tuo, sull'albero principale.

---

## SCOSTAMENTI

### 1. I tre implementer non sono partiti nello stesso messaggio

Il §2 lo chiede. B e C sono partiti insieme; **A è partito dopo**, e non per dimenticanza: il suo
brief **non era scrivibile** finché non sapevo dove finisse un'eccezione di `SaveSession`, e quel
fatto è arrivato da `doc-checker` mentre B e C erano già in volo. Un brief che non esiste non può
partire nello stesso messaggio di uno che esiste; l'alternativa era tenere ferme anche B e C per
attendere una verifica che non le riguardava.

### 2. Nessun test nuovo, e la voce 17 ne avrebbe meritato uno

`Dimentica()` passa da `IJSInProcessRuntime` e il progetto **non ha un doppio di test** per quella
interfaccia: `Eton.Tests` copre le decisioni pure (`Impronta`, `VaScritto`, `SessionFreshness`,
`PkceChallenge`), mai il confine JS. Costruirne uno adesso avrebbe significato introdurre
un'astrazione di test in un'unità dichiarata «piccola», e il budget del brief lo vietava. **Quindi
la voce 17 è verificata leggendo e sarà verificata nel browser, non dai test** — la misura attesa è
qui sotto.

### 3. Una correzione è nata da me, non da un revisore

Prima di scrivere il brief A avevo io il dubbio sulla corsa di `AllineaAsync` fuori dal lock, e
l'avevo messo fra le domande del `bug-hunter`. Lui ha trovato la versione **giusta** del problema,
che non era la mia: io guardavo alla stessa scheda, dove `AuthRedirect` chiude la finestra, lui a una
seconda scheda. Lo dichiaro perché nel tracciato non si vede, e perché è il caso in cui il revisore
non ha confermato il mio sospetto ma l'ha corretto.

### 4. Il §7 non è stato eseguito, e non poteva esserlo da qui

Il diff tocca l'interfaccia — un testo d'aiuto in `Pages/Profile.razor` — quindi il §7 chiederebbe
`live-testing`. **Non l'ho lanciato**: il mandato mi vieta di avviare il server, e quello vivo sulla
5000 serve l'**albero principale**, cioè una build che non contiene questo branch. Provarlo lì
riporterebbe un esito falso sul testo vecchio. È un limite da riportare, non da aggirare: la prova
appartiene al tuo ciclo chiuso, dopo l'integrazione.

`ui-critic` non scatta comunque: markup modificato in un `.razor` esistente, nessun file nuovo,
nessun metro dal §0 — e il §0 non si applicava, perché il brief non crea un `.razor` con markup e
non tocca CSS.

---

## LA MISURA ATTESA PER IL COLLAUDO

Da ricopiare nel brief della prova nel browser invece di inventarla.

### Voce 17 — al logout non resta niente

Lo strumento è la **console del browser** (o DevTools → Application → Local Storage, che mostra le
stesse chiavi). Prima di premere «Esci», da loggati:

```js
Object.keys(localStorage).filter(k => k.startsWith('eton.'))
// atteso PRIMA del logout: ["eton.session", "eton.spazio", "eton.profilo"]  (in ordine qualunque)
```

`eton.pkce` normalmente **non** c'è: è monouso e viene cancellato alla fine dello scambio del codice.
Compare solo se si guarda durante il giro d'accesso.

Subito dopo aver premuto «Esci» e dopo il rimbalzo su `/benvenuto`:

```js
Object.keys(localStorage).filter(k => k.startsWith('eton.'))
// atteso DOPO il logout: []
```

**È `eton.profilo` la chiave che decide la prova**: prima di questa unità restava lì. Le altre tre
sparivano già.

⚠️ **Da fare in una scheda sola.** Con due schede di Eton aperte la prova può fallire per il motivo
scritto nei `FUORI SCOPE` 1, che non è un difetto di questa unità: il bootstrap dell'altra scheda può
riscrivere `eton.profilo` dopo il logout. Se la prova va fatta a più schede, allora l'esito atteso è
proprio quello — e serve a **misurare** quel difetto, non a bocciare questa voce.

`eton.rotta-richiesta` non va cercata qui: sta in `sessionStorage`, non in `localStorage`, e non
dev'essere cancellata (v. `FUORI SCOPE` 3).

### Voce 18 — il testo d'aiuto

Aprire il «?» sulla pagina Profilo: il secondo paragrafo deve contenere il limite, non solo la
promessa. La frase esatta da ritrovare a schermo è «finché non lo riapri per gli altri resti
com'eri». Se compare solo la parte che promette l'aggiornamento, il testo è quello vecchio e la
build servita è superata.

### Voce 15 — quello che il browser **non** mostrerà

Non c'è nulla da vedere a schermo, e va detto perché nessuno perda tempo a cercarlo: il diff non
cambia un solo comportamento osservabile. Se si volesse provarlo davvero, serve un `localStorage`
con la **quota piena** — non bloccato, che è un altro caso già intercettato prima — e l'osservazione
è una riga in console che comincia con `[Auth] Salvataggio della sessione fallito`. È l'unica
manifestazione possibile, ed è esattamente il punto della voce: prima non esisteva nemmeno quella.
