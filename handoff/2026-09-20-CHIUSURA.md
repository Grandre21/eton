# CHIUSURA — il rapporto

*Scritto dalla sessione di chiusura il **20 settembre 2026**, a collaudo fatto. Il mandato sta sotto,
invariato. Il lavoro sta nel branch `worktree-chiusura-tutto-cio-che-rimane`.*

```
CHIUSURA: 6 unità — FATTO 5 · PARZIALE 1 · BLOCKED 0
COPERTURA: 38 clausole — coperte 31 · scoperte 0 · rinviate 7
```

**La colonna di stato della `PARTIZIONE` combacia con gli `ESITO` reali su disco**, tutti e sei,
verificati aprendo i resoconti: `01` PARZIALE, `01b` `02` `03` `04` `05` FATTO. Nessuna divergenza:
il piano è stato tenuto al passo fino all'ultimo resoconto.

---

## 1 — COPERTURA: dove cade ognuna delle 38, e le due che il piano dava per perse

**Il criterio, dichiarato perché il conteggio sia auditabile.** *Coperta* = la clausola ha ricevuto
l'atto che la sua fonte chiedeva — codice, oppure una dichiarazione motivata, oppure l'istruttoria,
per le tre voci del gruppo C che chiedevano di essere istruite «prima di essere credute». *Rinviata* =
ha una destinazione dichiarata fuori da questo goal e la prova o il gesto già scritti. *Scoperta* =
nessuno l'ha fatta **e nessuno ha detto di non averla fatta**. È quest'ultima la sola che questo
anello esiste per trovare, ed è a zero.

### ⚠️ Due delle sette «non chiudibili» risultano chiuse, ed è il ritrovamento principale

Il piano dichiarava all'apertura sette clausole non chiudibili, fra cui **1, 3, 5** — le tre voci di
scala — con questo motivo: *«non sono difetti ma decisioni di sistema di design: chiuderle* è *la fase
2.1-bis, che è un progetto di settimane»*.

**La 3 e la 5 sono chiuse, con codice, dall'unità 05**, e il motivo per cui la previsione è caduta sta
scritto nel suo stesso resoconto: *«sotto due delle tre voci di scala c'erano difetti funzionali
veri»*. Non è una forzatura del perimetro — è che la premessa era sbagliata:

- **3** — le due voci a 44px non erano una scelta di design: il commento di `.voce-profilo-stretto`
  giustificava il passaggio da misure **fisse** a `min-*`, **non** il 44 contro 48, e per `.nav-app
  .voce` non c'era ragione alcuna. Entrambe portate a `var(--tocco)`. `ui-critic` al collaudo:
  *«chiuso. `a.voce` 223×48 ×5; il profilo stretto 48×109,5 a 360»*.
- **5** — sotto «il titolo non segue una regola sola» c'era una **menzogna misurabile**: dentro
  `@media (min-width: 40rem)` il titolo saliva a 52px mentre l'ancora del «?» restava tarata sul corpo
  più piccolo, e il pulsante stava **8,64px più in alto** del centro della prima riga. `ui-critic`:
  *«chiuso come regola: `h1` 52/36/26 a 1128/600/360; centro del «?» sulla prima riga con Δ 0 e
  0,01»*.

**La 1 resta rinviata**, e correttamente: l'unità 05 l'ha istruita
(`storico/handoff/2026-09-20-05-scala-e-metro/istruttoria-superfici.md`) **smentendone la premessa** —
non «una ventina di dichiarazioni di fondo senza token», ma 44 su 49 che un token lo usano, e cinque
`transparent`, che non è un colore — e ha consegnato la decisione con le opzioni e i costi.

### ⚠️ E l'elenco delle «sette» ne nominava otto

Il piano scrive *«Sette clausole non sono chiudibili stanotte»* e sotto ne elenca **otto**: 29, 30, 28,
34, 35, **1, 3, 5**. Il numero e l'elenco non combaciavano già all'apertura. Con la 3 e la 5 chiuse le
non chiudibili scendono a sei — 1, 28, 29, 30, 34, 35 — e tornano sette solo aggiungendo la **37**, che
il piano teneva in un'altra tabella («il capo prepara, **l'utente applica**») e che non è chiudibile da
nessuna sessione per costruzione. **Il sette finale è esatto e la sua composizione è un'altra**: è la
differenza fra un numero che torna e un numero che regge.

Della stessa famiglia, e minore: il piano scrive *«Cinque clausole sono già chiuse mentre questo piano
nasce»* e ne elenca **sette** (36, 38, 11, 12, 13, 14, 16).

### Le 31 coperte

| # | Clausola | Chi l'ha chiusa, e con che prova |
|---|---|---|
| **2** | nove controlli senza nome accessibile | **02** (`2a`, il cursore: `id` = `for`) + **04** (`2b`, `aria-label` **per riga** su cinque `<select>` e tre `<input>`). ⚠️ **Vale una clausola e le due metà sono chiuse entrambe**, come il piano imponeva. `ui-critic`: controlli senza nome `[]` su **tutte** le 15 rotte |
| **3** | due voci di navigazione a 44px | **05**, v. sopra |
| **4** | controlli con altezze diverse | **04**. Collaudo: `.icona-input` 48, **24** pastiglie tutte 48 |
| **5** | il titolo non segue una regola sola | **05**, v. sopra |
| **6** | il «?» non risponde al primo clic | **01** (diagnosi e causa) + **01b** (rimedio). Collaudo: `.aiuto-apri` non nullo **subito**, su Home ed **entrambi** gli editor — le due rotte su cui la catena della Home non valeva |
| **7** | l'aggregato del voto mostra un frammento rotto | **02**. Collaudo: «—», 36px, `rgb(138,138,138)`, `textShadow: none` |
| **8** | un conflitto annunciato dove non c'è nessun altro | **02**, con la premessa della ricognizione **smentita**: l'aiuto non mentiva, mancava un paragrafo. Collaudo: la textarea conserva il testo digitato |
| **9** | il «?» fluttua accanto a un titolo lungo | **01**. Collaudo a due larghezze: scarto **0,013px** e **0,0019px** |
| **10** | «Ordina per» offre solo «Nome» | **02**, chiusa **dichiarando che non è un difetto**: il menù ha già l'ordinamento per voto, spento dalla sola condizione del voto al buio, e la schermata della ricognizione era cieca |
| **11 · 12 · 13 · 14 · 16** | le cinque decisioni del capo precedente | **dichiarate, non toccate**. La **13** e la **14** riverificate dall'unità 02 sul codice finale: `SovrascriviAbilitato="@PuoIntervenire"` non toccato, e il messaggio sopravvive ancora — cambiano **solo le parole**. La **16** verificata dalla 03: `Program.cs:22-23` registra `RottaRichiesta` e `AllineatoreProfilo` con la **stessa identica forma**, quindi le due convenzioni coesistono e uniformare è una decisione, non un fix |
| **15** | i due metodi senza `try` | **03**, con un'**asimmetria dichiarata** invece che uniformata, e la premessa ribaltata: non erano «scoperti», erano **muti** |
| **17** | `eton.profilo` sopravvive al logout | **03**. Collaudo: prima `["eton.profilo","eton.session","eton.spazio"]`, dopo **`[]`** |
| **18** | il testo d'aiuto del Profilo | **03**. ⚠️ Prova nel browser **mancante**, verificata da me sul codice: `Pages/Profile.razor:15` porta «finché non lo riapri per gli altri resti com'eri». V. §1-bis |
| **19** | il paragrafo dell'importo alto ~72px di troppo | **04** (`margin: 0` su `.importo-spesa`, il più stretto dei due candidati). La prova nel browser è la clausola **30**, rinviata |
| **20** | tre ancore per numero di riga scadute | **04**. `grep -cE "app\.css:[0-9]+" Pages/CollectionEdit.razor` → **0**. Il «fatto largo» (21-22 rimandi superstiti in 10 file) è proposta, e resta in `FUORI SCOPE` |
| **21** | due proprietà ridondanti in `.barra-elenco .pastiglia` | **04**. ⚠️ Prova nel browser **mancante**, verificata da me: la regola porta ora solo `flex` e `transition`, `button.pastiglia` dà `cursor: pointer` e `touch-action: manipulation`, e l'**unico** `.pastiglia` dentro una `.barra-elenco` in tutto il progetto è un `<button>` (`Pages/CollectionDetail.razor:91`). V. §1-bis |
| **22** | il commento di `.btn.compatto` | **04**, riflow del solo paragrafo, contenuto invariato |
| **23** | `.dato`: tre corpi per una classe sola | **04** l'ha istruita (sono **sei** corpi su **sedici** punti d'uso, non tre su quattordici) e **05** ha chiuso `23a` — la metà indipendente dalla scala. Collaudo: `.data-spesa` a **16px**, la protezione iOS è attiva. `23b` è consegnata come decisione |
| **24** | il primo campo a 0px dal secondo | **04**, e il titolo del rilievo era sbagliato: la classe era giusta, era il **lato**. Collaudo: `[12,12,12,12,12,12]`, nessuno 0 e nessuno 24 |
| **25** | lo stesso pulsante in due forme | **04** l'ha istruita (sei punti d'uso, non due), **05** si è rifiutata di dichiararla intenzionale prima che qualcuno guardasse, e il collaudo **ha guardato**: 134px in testata, 182px al centro, entrambi alti 48, screenshot acquisito. ⚠️ **La decisione che l'osservazione doveva sbloccare non l'ha presa nessuno**: resta in `FUORI SCOPE` |
| **26** | la stessa frase due volte a schermo | **02**, e **dentro** entrambe le decisioni del 19 settembre: nessuna revoca da adjudicare. Collaudo: compare **1** volta, prima 2 |
| **27** | la coppia «Sì, elimina» / «Annulla» | **collaudo**. La clausola chiedeva di **provare** e la prova è stata fatta; l'esito è un difetto **nuovo**, che per decisione dell'utente va nel rapporto |
| **31** | `Sovrascrivi()` col nome vuoto in due schede | **collaudo**: messaggio una volta sola, **scheda di conflitto che resta aperta** |
| **32** | i collegamenti «Tutte» alti 48 | **collaudo**: `[48, 48]` |
| **33** | la riserva del giro A sulla misura della colonna | **chiusa dichiarando**, e la dichiarazione regge sul codice: `wwwroot/css/app.css:281` porta `html { scrollbar-gutter: stable; }`, che riserva la colonna **sempre** — i due casi non possono più divergere per costruzione |
| **36** | i quattro branch remoti | chiusa all'apertura, su autorizzazione esplicita dell'utente |
| **38** | il §3 `COMBINATO` rimasto un segnaposto | chiusa all'apertura, completata e integrata |

### Le 7 rinviate, ognuna con la sua destinazione

| # | Clausola | Perché non si chiude qui, e cosa la chiude |
|---|---|---|
| **1** | le superfici come scala | **decisione di sistema di design → fase 2.1-bis.** Istruita, premessa smentita, raccomandazione scritta: fare per primi i due interventi che **non cambiano un pixel**, che rendono decidibili in dieci minuti i due che cambiano la resa |
| **28** | la barra gialla di Blazor | richiede un giro a **rete bloccata** riportando **verbatim** lo snippet di sovrascrittura di `window.fetch`. Sei cause già escluse, ognuna con la riga |
| **29** | il banner di aggiornamento | si vede **solo sul sito pubblicato**: tre osservazioni, un rilascio. In sviluppo il worker è un no-op |
| **30** | l'importo in sola lettura | serve un **secondo account**: ogni spazio raggiungibile ha un solo membro. Dichiarata non eseguibile dal collaudo, non «verde» |
| **34** | i tre dati di prova sul database | **tre gesti dell'utente**: passano da dialoghi nativi, che bloccano il plugin |
| **35** | il dialogo nativo comparso su «Elimina» di una **spesa** | **gesto dell'utente.** ⚠️ Il collaudo ha confermato che la conferma su una **collezione** è un componente **in pagina** — quindi il caso della spesa resta inspiegato, non spiegato |
| **37** | la contraddizione d'ambiente | **preparata e non applicata**: i due testi stanno in `handoff/configurazione-da-applicare.md` e toccano `~/.claude/`, cioè una superficie di configurazione. **È un gesto dell'utente** |

## 1-bis — Due prove sono uscite dal collaudo prima di entrarci, e lo scarto nasce nel brief

Le clausole **18** e **21** non compaiono nell'esito del collaudo: **né fra le verdi, né fra le cinque
non eseguibili.** Non sono sparite lì — sono sparite un passo prima.

Il brief di `live-testing` compone le prove *«ricopiando le misure dalle sei sezioni `LA MISURA ATTESA
PER IL COLLAUDO`»*. La riga 4 della sua tabella riassume il resoconto dell'unità 03 con *«al logout non
resta nessuna chiave»* — che è la voce **17** — e **la voce 18 non passa**, benché il resoconto la porti
con la frase esatta da ritrovare a schermo. La riga 5 riassume l'unità 04 con quattro misure su otto, e
l'unica **di browser** fra quelle omesse è la voce **21**.

⚠️ **È la stessa classe di difetto che il rapporto precedente aveva censito** — *«ciò che il piano del
collaudo prescrive e l'esito non nomina non viene contato da nessuno»* — **in una forma nuova e più
difficile da vedere**: lì una prova pianificata spariva fra il piano e l'esito, qui sparisce fra il
**resoconto** e il **brief**, cioè nel passaggio che il piano descrive come una pura trascrizione. Un
riassunto che comprime sei sezioni in sei righe di tabella **è** il punto in cui una misura si perde, e
nessuno se ne accorge perché la riga della tabella sembra completa.

**Non cambia la copertura**, e il motivo è quello già stabilito: entrambe le clausole sono chiuse **nel
codice** e verificabili leggendolo. **Le ho verificate io**, e reggono tutte e due — le righe stanno
nella tabella del §1. Ma andava detto, perché una prova che non esiste e una prova che nessuno ha
chiesto si assomigliano solo finché non si va a guardare.

## 2 — CONTRATTI: sei, aperti sul codice reale

**1. `wwwroot/css/app.css`, riscritto da quattro unità — CONVERGENTE, e nessuna ha scritto fuori dal
proprio blocco.** È il contratto più delicato del goal, perché il perimetro non era per file ma **per
regole dentro un file**. Verificato mappando **ogni hunk di ogni commit** sulla sezione del foglio in
cui cade, non sui resoconti:

| Unità | Perimetro scritto nel mandato | Dove ha scritto davvero |
|---|---|---|
| **01** (`8b06a8f`) | solo il blocco della testata e dell'aiuto, più l'animazione di pagina | **tutte** le righe toccate (950, 957-965, 988-1020) dentro `/* --- testata di schermata --- */` (946-1097). L'animazione non l'ha toccata affatto |
| **02** (`4f0840f`) | solo le regole del voto | **un solo hunk**, sulla regola `.voto-coperto, .voto-assente` |
| **04** (`1b91aeb` `fdeaf50` `7281623`) | pastiglia, bottone compatto, blocco campo, classe dei dati, selettore di icone, più il reset dei paragrafi | cinque sezioni: *pulsanti* (il commento di `.btn.compatto`), *campi*, *editor di collezione* (`.scelta-categoria`, `.icona-input`), *barra di ordinamento* (`.barra-elenco .pastiglia`), *modulo spesa* (`.importo-spesa` e `.campo-dinamico`). L'ultima è il **reset dei paragrafi nella forma stretta**, che il perimetro nominava |
| **05** (`5df46af`) | tutto ciò che resta, `:root` e la regola del titolo compresi | `:root`, *tipografia*, *testata di schermata*, *riepilogo Home*, e le due media query |

⚠️ **L'unica sovrapposizione è la 05 dentro il blocco della 01, ed era prevista, non subìta**: il suo
mandato le dava «tutto ciò che resta» dichiarandola **l'ultima a toccare il foglio**, e il `RAZIONALE`
del piano lo diceva in anticipo — *«rivendica righe che la 01 e la 02 possiedono»*. La partizione per
vicinato di righe ha retto: **nessuna unità ha trovato spostato ciò che un'altra aveva già scritto**, e
nessun hunk cade fuori dal territorio dichiarato.

**2. L'ancora del pulsante d'aiuto, fra 01 e 05 — CONVERGENTE, e su UNA regola sola.** È la verifica
che il mandato chiedeva per nome («che non siano atterrate su due regole che si sovrappongono»), e il
codice finale la regge perché le due unità hanno toccato **cose diverse dello stesso meccanismo**:

    :root      --corpo-titolo: var(--t-2xl);  --interlinea-titolo: 1.08;        (05, :196-197)
    h1         font-size: var(--corpo-titolo); line-height: var(--interlinea-titolo);  (05, :343 :347)
    .testata   --riga-titolo: calc(var(--corpo-titolo) * var(--interlinea-titolo));    (05, :1031)
    .aiuto-apri, .testata-azione { margin-block: calc((var(--riga-titolo) - var(--tocco)) / 2) … }
                                                                                 (01, :1082-1085)
    @media 40rem  :root { --corpo-titolo: var(--t-3xl); }                        (05, :2371)

La **01** ha scritto la formula del margine; la **05** ha reso vero il valore che la formula legge,
ridichiarando **il token** e non il `font-size` — così la media query che alzava il titolo a 52px alza
adesso anche l'ancora. Esiste **una** regola che definisce l'ancora e **una** che la consuma: non c'è
nessuna coppia che si sovrappone. Il collaudo l'ha misurata verde a due larghezze e `ui-critic` a tre.

**3. Il ciclo di vita del montaggio, unità 01b — CONVERGENTE, e nessuna guardia è stata tolta.**
Rimosso da `Layout/MainLayout.razor` esattamente e soltanto ciò che il resoconto dichiara: il campo
`chiaveRotta`, `OnInitialized`, il gestore `SuCambioRotta`, `@implements IDisposable` e `Dispose`. Da
`Pages/Home.razor` è uscito solo il blocco `<TestataPagina>` spostato fuori dal condizionale.
**Le guardie contro le doppie letture sono tutte vive**: `caricamentoIniziale`
(`Pages/Home.razor:281, 299, 321, 457`), `_generazione` (`Services/SpaceStateService.cs:25` e quattro
usi), `primoAvvio` (`Services/SupabaseService.cs:99, 137, 155`). Nessuna è stata rimossa e nessuna è
diventata morta: quella della Home protegge una corsa **diversa** — fra l'evento di cambio spazio e la
`Carica()` esplicita — non fra due montaggi.

**4. `AuthStateService` e i servizi del profilo, unità 03 — CONVERGENTE.**
`Services/AuthStateService.cs:17` porta il parametro in più in coda, come dichiarato; `new
AuthStateService(` in tutto il progetto → **0**, quindi il solo costruttore è la DI; `Program.cs:13` e
`:23` registrano i due servizi e l'ordine non conta, perché il container risolve alla prima richiesta.
`AllineatoreProfilo.Dimentica()` ha **un solo chiamante**, `AuthStateService.cs:80`, ed è affiancato a
`_spazi.Dimentica()` alla riga sopra: il metodo nuovo **ricalca un fratello che esisteva già**.
*(Il resoconto cita la firma a `:16` e oggi sta a `:17` — uno scarto di una riga, non di firma: è la
ragione per cui questo progetto ha vietato i rimandi per numero di riga.)*

**5. Gli `aria-label` in ciclo, unità 04 — CONVERGENTI, e sono per riga.** Verificato sul codice come
il mandato chiedeva, non sul verde del collaudo: `Pages/CollectionEdit.razor:158` e `:175` costruiscono
il nome con `NomeCampo(campo)`, che dipende dall'**istanza** e ha tre livelli — `Label`, poi `Key`, poi
`numero {indice+1}`. Il terzo garantisce nomi **diversi** anche a due campi senza etichetta e senza
chiave, e non può restituire vuoto: entrambi i rami testano `IsNullOrWhiteSpace` e l'ultimo è
un'interpolazione sempre popolata.

**6. `Shared/PaginaEditor.cs`, dichiarato invariante da sei unità in due cicli — CONFERMATO SUL DIFF.**
Non sui resoconti, come il mandato impone: `git log -- Shared/PaginaEditor.cs` si ferma a **`3cf84ff`,
10 settembre 2026**. Né questo goal né il precedente l'hanno aperto in scrittura.

## 3 — COMBINATO

```
bug-hunter (diff combinato, sette cuciture)   RILIEVI: 0
```

Lanciato sul diff `cdb0999..main` — **11 file, +396/−114** — con il mandato esplicito di **ignorare i
commenti** e di cercare solo ciò che emerge dalla **somma** delle unità. Ha verificato le sette
cuciture una per una, citando per ognuna la riga che la regge: la specificità fra le regole di quattro
unità in `app.css`; l'isolamento fra il `line-height` dei campi (04) e i token del titolo (05) — non
esiste markup in cui un `<h1>` e un campo stiano sulla stessa riga; `.importo-spesa` che continua a
vincere su `:where(input, select, textarea).dato`; le guardie del montaggio; la DI del servizio nuovo;
`Registra`/`Cambiata` che leggono la **stessa** espressione del pulsante, quindi non possono divergere;
e `campi.IndexOf(campo)` che usa l'uguaglianza **per riferimento**, quindi due righe vuote restano
distinguibili.

⚠️ **Una sua prova non reggeva, e l'ho riaperta io** — è il controllo che il §5 impone quando la riga
citata non sostiene il claim. Per dimostrare che `.importo-spesa` vince, ha citato
`handoff/05-scala-e-metro/resoconto.md:349` chiamandola *«una misura già eseguita»* e *«`getComputedStyle`
reale»*: quella riga sta nella sezione **`LA MISURA ATTESA`**, cioè è un valore **previsto**, non
osservato. **Il claim regge lo stesso**, e l'ho verificato per una via che non ha bisogno del browser:
`:where(input, select, textarea).dato` (`app.css:436`) vale **0-1-0** perché `:where()` azzera la
specificità, `.importo-spesa` (`app.css:2209`) vale **0-1-0** ed è dichiarata **dopo** — vince per
ordine di sorgente. E l'osservazione vera esiste: `esito-ui-critic.md:35` legge nel browser
`importo-spesa h71 fs36px`.

**Vale la pena notarlo** perché è, in miniatura, **lo stesso difetto che il collaudo ha appena trovato
nelle unità**: una misura attesa presa per osservata. Stavolta l'ha commesso il revisore della
chiusura, il giorno dopo che il goal ne aveva fatto la propria lezione principale.

**Niente da correggere e niente da rimandare al capo.**

## 4 — GATE, rieseguiti sullo stato finale

```
dotnet build Eton.sln -warnaserror --no-incremental   → Compilazione completata. Avvisi: 0  Errori: 0
dotnet test Eton.sln                                  → Superato! Non superati: 0. Superati: 310. Totale: 310
```

Eseguiti da me sul worktree della chiusura, a `bug-hunter` rientrato — **mai in parallelo**, perché
`obj/` non ha lock fra processi. Il server di sviluppo è vivo su un **altro albero**, quindi non è
stato disturbato: non l'ho avviato, non l'ho fermato, non ho aperto il browser.

## 5 — FUORI SCOPE — aggregato

**24 voci.** Nessuna è stata corretta, e per le prime sette è un'istruzione esplicita del mandato.

### A — dal collaudo, destinazione dichiarata **fase 2.1-bis** (7)

I cinque di `ui-critic`, più i due difetti di `live-testing`. **Due sono minuscoli e oggettivi**, e il
capo li ha già portati all'utente invece di seppellirli nel mucchio:

1. **Il campo della data è alto 71px e non 48, sopra i 640px** · media · allineamento. Ha ora **causa
   completa e due fix scritti**: (a) l'importo da solo sulla prima riga, descrizione e data affiancate
   sotto; (b) tenere la coppia e fermare lo stiramento, al prezzo di due altezze sulla stessa riga. È
   l'unico dei cinque che **smentisce una misura attesa scritta in due resoconti** — v. §7.
2. **Il corpo della nota è reso in una terza famiglia tipografica** che il metro non prevede — il mono
   di sistema, né Inter né Plex Mono · media · `METRO: PIANO-DESIGN`. Stesso selettore che porta anche
   l'eccezione non scritta sul fondo: **due decisioni non scritte su un elemento solo**.
3. **I due segmenti «Scrivi / Anteprima» sono bersagli alti 40px**, sotto il pavimento di 48 — **e il
   commento che li precede dichiara l'opposto** · media.
4. **Il «?» è l'unico glifo dell'applicazione reso in Arial**, su 14 rotte su 15 · bassa. **Il fix è una
   riga** (`font: inherit`), e riguarda proprio il pulsante che questo goal ha passato la notte a
   sistemare sotto ogni altro aspetto.
5. **Il «?» è ancorato alla larghezza del titolo**, e in sei schermate su undici il titolo al primo
   render è una parola di ripiego: quando il dato arriva, il pulsante **si sposta** (Δ fino a 302px) ·
   bassa · progetto. ⚠️ **È un effetto del lavoro dell'unità 01b**: prima il pulsante non esisteva
   affatto in quella finestra, quindi non poteva spostarsi. **Il rimedio ha reso visibile un difetto che
   prima era nascosto da un difetto peggiore.** Insieme: il margine di `p.avvio` da azzerare.
6. **I due pulsanti di conferma non stanno sulla stessa riga, e l'ordine è invertito** — «Sì, elimina»
   si affianca al testo di avviso e «Annulla» va a capo, cioè **il distruttivo sta sopra quello di
   sicurezza**. Misurato a 594px (Δ 56px); a 360 lo sarà a maggior ragione. **Decisione dell'utente: va
   nel rapporto**, non in un'unità.
7. **Due residui della voce 5** che l'unità 05 non ha toccato: il dettaglio di una collezione tiene il
   titolo fuori dalla testata condivisa — **è l'unica schermata senza «?»** — e su `/expenses` titolo e
   totale restano entrambi in cima alla scala sopra i 640px, con un commento del foglio ancora falso.

### B — aperti dalle unità, ancora vivi sullo stato finale (10)

8. **Il titolo della scheda del browser sulla Home** resta il letterale «Eton», mentre nelle altre
   **dieci** pagine è «titolo — Eton»: unica eccezione dell'applicazione, verificata aprendo tutti e
   undici i file. **Il rimedio è una riga**, già istruito, e riusa un simbolo che il diff della 01b ha
   introdotto. ⚠️ **È la trentanovesima clausola in attesa, e non è stata presa per comodità**: basta
   un sì dell'utente. (`APERTO` del piano)
9. **«Salva recensione» contro «premi *Salva* di nuovo»**: due avvisi nominano l'azione con un nome che
   il pulsante non porta. Allinearli richiede di toccare **anche** un messaggio preesistente che nessuna
   voce ha segnalato — due stringhe, nessun rischio, ma scope nuovo.
10. **La corsa fra schede riscrive dopo il logout**, e vale **identica** su `eton.spazio`, che questo
    goal non tocca: la protezione esistente è un campo **di istanza**, e nel progetto non esiste alcun
    meccanismo di sincronizzazione fra schede. Il rimedio ovvio chiuderebbe **metà** del problema dando
    alla pulizia un'apparenza di simmetria che non avrebbe. **Candidato forte per il goal successivo.**
11. **Con la sessione non salvata, l'impronta del profilo viene scritta lo stesso**: caso stretto
    (quota che si riempie fra due scritture), preesistente, stessa famiglia della voce 17.
12. **Il reset globale dei margini dei paragrafi**, con il conteggio che lo rende decidibile — e con
    l'avvertenza che **due censimenti ereditati non reggevano** alla verifica dell'unità 05.
13. **I rimandi `file:riga` fuori dal foglio di stile**: **21 righe / 22 occorrenze in 10 file**. Lo
    scarto fra i due numeri non è un errore — `grep -c` conta le righe — ed è **l'argomento più forte
    della proposta**: un controllo automatico deve fissare **anche il modo di contare**, non solo la
    soglia. Una convenzione che nessuno misura scade di nuovo in un ciclo.
14. **Lo stesso difetto di blockification è già vivo su `.testo-tenue`, in due punti non rattoppati**,
    entrambi in `Pages/CollectionEdit.razor` dentro un `.azioni` flex. Il rimedio è quello già applicato
    a `.spiega`, ma tocca 29 punti d'uso e va guardato prima.
15. **Il `<p role="alert">` di `App.razor`** è l'unico alert del progetto che non usa la classe degli
    altri: il rimedio non è un margine, è la classe.
16. **Il punto mediano come separatore di meta** è, delle cose che Eton fa, la più *templated*, e —
    a differenza degli altri due tratti — **non ha nessuna giustificazione nel foglio**.
17. **`23b` — i quattro `<span>` a 12,35px**, con la premessa **ribaltata**: le metriche dicono che
    Plex Mono è già più basso di Inter del 4-5%, quindi il `.95em` non è una correzione ottica ma un
    peggioramento. ⚠️ **Confidenza media, dichiarata**: chi decide legga `sxHeight` e `sCapHeight` dai
    due woff2. Raccomandazione: togliere `font-size` da `.dato`.

### C — la decisione che l'osservazione doveva sbloccare, e che nessuno ha preso (1)

18. **La voce 25.** L'unità 04 raccomandava di dichiarare intenzionale la doppia forma; l'unità 05 si è
    rifiutata di scriverlo *«il giorno prima di guardarlo»*; il collaudo **ha guardato** e ha misurato
    134px in testata e 182px al centro, entrambi alti 48, con screenshot. **Ma alla domanda — vederli
    insieme stona? — non ha risposto nessuno.** La catena si è fermata sull'ultimo anello, e la
    decisione è di prodotto: se non stona, una frase di commento chiude la voce in trenta secondi; se
    stona, la domanda diventa **quale dei due togliere**.

### D — misurati da `ui-critic` oltre il proprio tetto, non adjudicati (5)

19. Il «?» e l'azione di testata **si stringono a 360px con una barra di scorrimento classica**: il
    pulsante scende a 44,7px e «Collezioni» si spezza in due righe. Senza barra resta **3,2px** di
    margine, non gli 8,5 che il commento dichiara.
20. Un'emoji-icona a 22,4px, unico corpo fuori scala che non sia un dato.
21. Una casella di spunta col margine dello user agent, unico controllo non azzerato.
22. Nell'editor di elemento **il titolo e il campo del nome mostrano lo stesso testo**, a 52px e a 20px,
    a 8px di distanza. *«La regola che lo vieterebbe non ha una soglia nominabile.»*
23. **Il fuoco da tastiera non è stato riprovabile**: i `Tab` inviati dal plugin non spostano il fuoco
    dall'`h1`. La regola esiste nel foglio e l'istanza del 19 settembre l'aveva vista.

### E — censito e lasciato fuori di proposito (1)

24. **Sulla Home ogni query al database partiva due volte** — spese, spazi, membri, profili, note,
    collezioni — e **nessuno l'aveva mai censito**. Non è stato trasformato in clausola per non far
    crescere l'obiettivo. ✅ **Il collaudo lo dà per chiuso dal rimedio dell'unità 01b**: navigazione
    fresca sulla Home, **4 chiamate REST, ciascuna una volta sola**. Resta qui perché la misura copre le
    chiamate osservate, non un censimento completo.

## 6 — SETTE COSE CHE NESSUN RESOCONTO CONTIENE

1. **Il browser è rimasto disconnesso da Eton** dopo il collaudo, su `/benvenuto`: la prova del logout
   richiede di premere «Esci» e l'agente non aveva credenziali per rientrare. L'utente ha poi rifatto
   l'accesso per `ui-critic`. **Non è un difetto, è uno stato** — e chi riprende deve saperlo.
2. **Tre dati di prova del ciclo precedente sono ancora sul database di sviluppo**: la spesa «COLLAUDO
   GIRO B», lo spazio «COLLAUDO GIRO B TEST», la collezione «Collaudo Ricognizione». **Servono tre gesti
   dell'utente**, perché passano da dialoghi nativi. ⚠️ **Il collaudo di stanotte non ha aggiunto
   residui**: ha creato **e poi eliminato** una propria collezione di prova, e ha modificato **e
   ripristinato** quattro campi su un elemento, verificando il ripristino con un refresh.
3. **Cinque prove sono state dichiarate non eseguibili**, e **una sola delle cinque è stata saltata per
   mancanza di tempo e non per impossibilità**: il **pavimento di tocco della navigazione sopra i
   1024px**. Va rifatta, e costa una riga. Le altre quattro hanno un ostacolo reale: il secondo account,
   il limite di ridimensionamento dell'ambiente, gli stati «nessuno spazio» ed «errore», e un
   collegamento con `#` che nei dati di prova non esiste.
   ⚠️ **E il limite di ridimensionamento è già stato superato**, dopo: `ui-critic` ha misurato a 360,
   600 e 800px **dentro un `<iframe>` same-origin con Blazor avviato dentro**, perché `resize_window` in
   quell'ambiente non ha avuto effetto a nessun valore. **È la via da usare al prossimo collaudo**,
   invece di dichiarare un limite.
4. **Il rilievo sull'impianto più importante del goal.** Due unità hanno scritto una misura attesa **che
   non potevano verificare**, perché il loro mandato vietava di avviare il server ed è il server l'unico
   posto dove quel grid esiste. Il calcolo era **giusto in isolamento e irrilevante nel contesto**. Le
   misure attese scritte da ogni unità sono state il **guadagno più grande** di questo collaudo — hanno
   reso il brief un atto di **trascrizione** invece che di invenzione — **e questo ne è il limite**: una
   misura calcolata invece che osservata è un'ipotesi travestita da criterio. ⚠️ **E il difetto si è
   riprodotto dentro questa stessa sessione**, nel `bug-hunter` finale che ha citato un'attesa come
   osservazione (§3): non è un errore di due unità, è una proprietà del formato.
5. **Un secondo rilievo sull'impianto**: un passo del §4 è saltato nell'unità 04 — il `checker`
   sull'istruttoria del secondo giro — e **l'unità l'ha dichiarato**. È il **secondo caso in due cicli**
   con la stessa forma: il ciclo precedente aveva misurato che `coverage` si perdeva perché cadeva
   **dopo** il momento in cui si scrive il tracciato. **Un passo che cade fra due momenti in cui si
   scrive il tracciato tende a saltare**, e nessuna delle due volte è stata pigrizia. ⚠️ **Il §1-bis di
   questo rapporto ne è la terza istanza, in un altro anello**: due misure attese si sono perse nel
   passaggio dal resoconto al brief, cioè in un altro punto in cui un artefatto viene ricopiato dentro
   un altro. **La classe non è «il §4»: è la trascrizione fra due documenti, ovunque avvenga.**
6. **La clausola 37 è preparata ma non applicata**: i due testi stanno in
   `handoff/configurazione-da-applicare.md` e toccano `~/.claude/`, cioè una superficie di
   configurazione. **È un gesto dell'utente.** ⚠️ E va detto che **il capo stesso ha violato quella
   regola una volta**, usando `sed -i` su un file di progetto e dichiarandolo nel messaggio di commit:
   zero violazioni su undici implementer nel ciclo precedente, **una su un capo**. La direttiva non
   vince contro chi ha il divieto fresco davanti in un mandato appena letto: **vince nell'attrito di
   un'operazione meccanica fatta di fretta.** ⚠️ **La contraddizione che la 37 cura è ancora viva e ha
   colpito di nuovo**: le unità 04 e 05 l'hanno entrambe riportata nei propri `SCOSTAMENTI`, e due
   implementer su tre della 04 l'hanno ricevuta e **disapplicata da soli**. È arrivata anche in questa
   sessione. Una regola che ogni sessione deve disapplicare da sé, ciclo dopo ciclo, non è un
   fastidio: è un logoramento che prima o poi qualcuno non regge.
7. **Quattro premesse del rapporto precedente sono cadute su quattro verificate.** Nessuna è stata
   scoperta rileggendolo: sono emerse perché **ogni mandato chiedeva di verificare il fatto prima di
   correggere**, dichiarando che se il fatto fosse caduto la decisione sarebbe caduta con lui. La voce
   10 non era un difetto; il testo d'aiuto della voce 8 non mentiva; la voce 16 aveva una premessa vera
   solo per due dei tre «fratelli»; la voce 15 parlava di metodi «scoperti» che erano **muti**, e il
   fatto nuovo **ha capovolto il principio invece di applicarlo**. **È il risultato metodologico del
   goal, e vale più delle singole voci.**
   ⚠️ **Questo rapporto ne aggiunge una quinta, sul rapporto stesso**: la premessa del rilievo 1 di
   `ui-critic` del 19 settembre — «le superfici esistono come token ma non come scala» — è caduta su
   misura dell'unità 05 (44 dichiarazioni su 49 usano un token). **Cinque su cinque.**

## 7 — LA CORREZIONE CHE IL MANDATO CHIEDEVA

**Fatta: due resoconti, nessuna riga di codice.** Il collaudo ha dimostrato che il campo della data non
è alto 48px come due unità avevano dichiarato, ma **71**, e che a sbagliare era **la misura attesa**.

- `storico/handoff/2026-09-20-04-controlli-e-campi/resoconto.md` — rettificate **entrambe** le
  occorrenze: il capoverso che annunciava il passaggio «da 49,6 a 48px» e il blocco di codice della
  `MISURA ATTESA`, che è **la forma in cui qualcuno l'avrebbe ricopiata in un brief**.
- `storico/handoff/2026-09-20-05-scala-e-metro/resoconto.md` — rettificata la misura e **presa in
  parola la sua stessa avvertenza**: quel resoconto scriveva *«se leggi un numero diverso da 48, il
  conto che ho fatto è sbagliato e va riaperto»*. Il collaudo ha letto un numero diverso.

Le rettifiche sono **aggiunte e datate, non sostituzioni silenziose**: dicono il valore vero (48 sotto i
640px, 71 sopra), la causa (grid a due colonne, *stretch* implicito della riga, `.campo input { flex: 1 }`
scritta per un'altra variante), e **perché la misura era scrivibile ma non verificabile**.

**Il codice non è stato toccato, ed è la parte che conta**: la protezione che quella voce introduceva
**funziona** — il corpo è 16px, misurato — e 71px è ben sopra il pavimento di tocco.

## 8 — ARCHIVIATO

```
handoff/01b-home-e-montaggio  →  storico/handoff/2026-09-20-01b-home-e-montaggio
handoff/02-voti-e-recensioni  →  storico/handoff/2026-09-20-02-voti-e-recensioni
handoff/03-accesso-e-profilo  →  storico/handoff/2026-09-20-03-accesso-e-profilo
handoff/04-controlli-e-campi  →  storico/handoff/2026-09-20-04-controlli-e-campi
handoff/05-scala-e-metro      →  storico/handoff/2026-09-20-05-scala-e-metro
```

**Cinque cartelle, con `git mv` e col prefisso di data**, perché `storico/handoff/` contiene già tre
cicli e i prefissi numerici si ripetono: senza la data, `01-`, `02-` e `03-` sono ambigui fra tre
cartelle ciascuno.

**L'unità 01 NON è archiviata, ed è una decisione che il mandato mi lasciava.** È `PARZIALE`, e il
`CLAUDE.md` nomina il caso per nome: *«le `BLOCKED` e le `PARZIALE` restano vive in `handoff/`»*.
**Il merito direbbe di archiviarla** — non le resta nulla di aperto: la voce 9 è chiusa e misurata
verde, la voce 6 è passata alla 01b che l'ha chiusa, e tenerla separata dalla 01b spezza la catena fra
la diagnosi e il rimedio che ne è nato. **Ma la delega a decidere viene dal mandato del capo, che è una
sessione, non l'utente**, e il `CLAUDE.md` apre dicendo che un livello inferiore può essere *più
restrittivo, mai più permissivo*: usare quella delega per allentare una regola di chi sta sopra sarebbe
stato il precedente sbagliato, e il costo della scelta opposta è **una cartella in più**.
**All'utente basta un `git mv` per ribaltarla**, e il suo residuo è chiuso.

**Non archiviati**, come il mandato prescrive: `handoff/collaudo/`, `handoff/CHIUSURA.md` (questo file,
che non si archivia finché l'utente non l'ha letto), `handoff/configurazione-da-applicare.md` (lavoro
dell'utente ancora da fare), `handoff/PIANO.md` (il capo deve ancora scriverci `PROSSIMA AZIONE: GOAL
CHIUSO`) e `handoff/server.md`, **perché il server è vivo**.

## 9 — DUE COSE CHE ASPETTANO IL CAPO, E NON ME

1. **Il server di sviluppo è VIVO e lo fermo non io**: PID **27180** (padre, `dotnet run`) e **19824**
   (figlio, il devserver, **è lui che ascolta sulla 5000**). Vanno fermati **entrambi** — fermare solo
   il padre lascia la porta occupata — e poi va verificato che la 5000 sia libera.
2. **`PROSSIMA AZIONE: GOAL CHIUSO — apri una sessione nuova`** in `handoff/PIANO.md`. È il capo a
   scriverla, leggendo questo rapporto: il goal è chiuso quando esistono **due** file, non uno.

---

# CHIUSURA — il mandato

*Scritto dal capo il **20 settembre 2026**, a collaudo fatto. Il rapporto lo scrive la sessione di
chiusura, in testa a questo file, sopra questo mandato.*

⚠️ **Il rapporto del ciclo precedente stava qui e ora sta in
`storico/handoff/2026-09-19-CHIUSURA-sedici-rilievi.md`**, accanto al proprio piano. Il capo stava
per sovrascriverlo e se n'è accorto perché la scrittura è stata rifiutata: il file era cambiato
dopo l'ultima lettura. **Il path `handoff/CHIUSURA.md` è fisso per architettura, quindi due cicli
consecutivi se lo contendono** — chi apre il ciclo successivo archivi il rapporto precedente prima
di scrivere il proprio mandato, con un prefisso di data, come è stato fatto per il piano.

## CHI SEI

Sei la **sessione di chiusura** del goal «tutto ciò che rimane da chiudere». Non sei un quarto
revisore e non sei un riassuntore.

**Leggi `~/.claude/architettura-sessioni.md` come prima azione**, sezione «La sessione di chiusura»:
contiene il formato del tuo rapporto, che non conosci altrimenti, e il confine di cosa puoi
correggere.

## L'OBIETTIVO, VERBATIM

> «tutto cio che rimane da chiudere voglio che sia ciuso in questa sessione»
> — utente, 19 settembre 2026, notte

**Lo leggi da `handoff/PIANO.md`, dove sta verbatim, non da questa copia.** La glossa che lo
decompone sta lì sotto, con la tabella che dice da dove viene ognuna delle clausole.

⚠️ **Conti contro trentotto.** E ⚠️ **sette di quelle trentotto erano dichiarate non chiudibili fin
dall'apertura**, con il motivo scritto: è la differenza fra una copertura onesta e una che si
sgonfia al conteggio finale. **Verificale**: se una di quelle sette risultasse chiusa, o se una
delle altre trentuno non lo fosse, è esattamente ciò che devi trovare.

## COSA DEVI FARE, NELL'ORDINE

1. **Copertura dell'obiettivo.** Confronta le 38 clausole con l'unione di ciò che i **sei
   resoconti** dichiarano — più l'esito del collaudo in `handoff/collaudo/esito-live-testing.md` —
   ed elenca **ciò che nessuno ha fatto e nessuno ha detto di non aver fatto**. Ogni altro anello
   verifica i claim *fatti*; tu sei il solo che cerca quelli **non fatti**, e il capo — che ha
   disegnato la partizione — è il candidato peggiore a trovarne i buchi.
   Nello stesso passaggio confronta la colonna di stato della `PARTIZIONE` con gli `ESITO` reali.
   ⚠️ **Avvertenza specifica di questo goal**: la partizione è cambiata **due volte in corsa** — è
   nata un'unità `01b` che il piano iniziale non aveva, e la voce 2 è stata **spezzata in `2a` e
   `2b`** fra due unità. Il piano lo dichiara in `DECISIONI` con il motivo, e il conteggio non è
   cambiato: la voce 2 vale **una** clausola coperta, e solo se **entrambe** le metà sono chiuse.

2. **Convergenza dei contratti.** Apri il codice e verifica che produttore e consumatore siano
   atterrati sulla **stessa firma reale**, non solo che entrambi l'abbiano dichiarata:
   - **`wwwroot/css/app.css`, riscritto da quattro unità** — 01 (blocco della testata), 02 (regole
     del voto), 04 (sette punti), 05 (token e scala). **È il contratto più delicato del goal**,
     perché il perimetro non era per file ma **per regole dentro un file**: è la prima volta che
     questo progetto partiziona così. Verifica che nessuna unità abbia scritto fuori dal proprio
     blocco.
   - **L'ancora del pulsante d'aiuto**, fra 01 e 05: la 01 l'ha costruita, la 05 ha scoperto che era
     **rotta sopra i 640px** e l'ha rifatta derivandola da due token. Il collaudo l'ha misurata
     verde a **entrambe** le larghezze — verifica che le due unità siano atterrate sulla stessa
     regola e non su due che si sovrappongono.
   - **Il ciclo di vita del montaggio**, unità 01b: ogni pagina si monta ora **una volta sola**.
     Verifica che nessuna guardia contro le doppie letture sia stata tolta altrove credendola
     inutile.
   - **`AuthStateService`** e i servizi del profilo, unità 03: un parametro in più nel costruttore,
     un metodo nuovo con un solo chiamante.
   - **Gli `aria-label` in ciclo**, unità 04: verifica che siano **per riga** e non tutti uguali —
     il collaudo l'ha misurato verde, tu verificalo sul codice.
   - **`Shared/PaginaEditor.cs`** — dichiarato invariante da **sei** unità in due cicli e mai aperto
     in scrittura. Verificalo sul diff complessivo, non sui resoconti.

3. **`bug-hunter` sul diff combinato**, con i call-site di cucitura, più i gate rieseguiti sullo
   stato finale.
   ⚠️ **Dagli il mandato esplicito di ignorare i commenti.** Il ciclo precedente ne ha lanciati due
   sullo stesso diff: quello della chiusura trovò **quattro** rilievi, tre dei quali erano commenti
   falsi; quello lanciato in parallelo, con l'ordine di ignorarli e cercare solo ciò che emerge
   dalla **somma** delle unità, chiuse con **zero** verificando le cuciture una per una. I due
   esiti non si contraddicevano. **Qui interessa il secondo tipo.**

## COSA NON DEVI FARE

1. ⚠️ **Non correggere i rilievi che `ui-critic` ha prodotto al collaudo.** Destinazione dichiarata:
   la **fase 2.1-bis** del piano di prodotto. Il tuo compito su di essi è **aggregarli in
   `FUORI SCOPE`**, non chiuderli.
2. **Non correggere il difetto dei pulsanti di conferma** trovato dal collaudo — a schermo stretto
   non stanno sulla stessa riga e il distruttivo sta sopra quello di sicurezza. **L'utente ha deciso
   che va nel rapporto**, ed è in `DECISIONI`.
3. **Non avviare il server e non aprire il browser.** Il collaudo è fatto. ⚠️ **Il server è vivo**:
   i PID sono in `handoff/server.md`, e **lo ferma il capo**, non tu.
4. **Non toccare `supabase/migrations/`.** Nessuna unità l'ha toccata, e un file nuovo lì dentro
   finisce nel parser di `PrivilegiInsertTests`, che può diventare rosso senza che nessuno abbia
   toccato il test.
5. **Tutto ciò che tocca un contratto fra unità torna al capo**, e non lo risolvi tu.

## COSA PUOI CORREGGERE

Ciò che il `bug-hunter` finale trova **dentro** il perimetro già toccato, dispacciando `implementer`
col protocollo normale.

⚠️ **E una cosa in più, specifica di questo goal**: il collaudo ha dimostrato che **due resoconti
portano una misura attesa sbagliata** — il campo della data dichiarato a 48px, misurato a **71**
perché sta in un grid affiancato a un campo alto 71, dove l'altezza la decide la riga e non il
contenuto. **Correggi quei due resoconti**: sono documenti che qualcuno rileggerà, e una misura
attesa falsa in un resoconto archiviato è peggio di una misura assente. **Il codice non va toccato**:
la protezione che quella voce introduceva funziona, il corpo è 16px misurato.

## SETTE COSE CHE VANNO NEL RAPPORTO E CHE NESSUN RESOCONTO CONTIENE

1. **Il browser è rimasto disconnesso da Eton** dopo il collaudo — la prova del logout richiede di
   premere «Esci» e l'agente non aveva credenziali. L'utente ha poi rifatto l'accesso per
   `ui-critic`. Non è un difetto, è uno stato.
2. **Tre dati di prova del ciclo precedente sono ancora sul database di sviluppo**: la spesa
   «COLLAUDO GIRO B», lo spazio «COLLAUDO GIRO B TEST», la collezione «Collaudo Ricognizione».
   **Servono tre gesti dell'utente**, perché passano da dialoghi nativi. ⚠️ Il collaudo di stanotte
   ha creato **e poi eliminato** una propria collezione di prova, e ha modificato **e ripristinato**
   quattro campi su un elemento verificando il ripristino con un refresh: **non ha aggiunto
   residui**.
3. **Cinque prove sono state dichiarate non eseguibili**, in `handoff/collaudo/esito-live-testing.md`.
   ⚠️ **Una sola delle cinque è stata saltata per mancanza di tempo e non per impossibilità** — il
   pavimento di tocco della navigazione sopra i 1024px — e va riportata come tale, distinta dalle
   altre quattro.
4. **Il rilievo sull'impianto più importante del goal.** Due unità hanno scritto una misura attesa
   **che non potevano verificare**, perché il loro mandato vietava di avviare il server ed è il
   server l'unico posto dove quel grid esiste. Il calcolo era **giusto in isolamento e irrilevante
   nel contesto**. Le misure attese scritte da ogni unità sono state il **guadagno più grande** di
   questo collaudo — hanno reso il brief un atto di trascrizione invece che di invenzione — **e
   questo ne è il limite**: una misura calcolata invece che osservata è un'ipotesi travestita da
   criterio.
5. **Un secondo rilievo sull'impianto**: un passo del §4 è saltato nell'unità 04 — il `checker`
   sull'istruttoria del secondo giro di revisione — e **l'unità l'ha dichiarato**. È il **secondo
   caso in due cicli** con la stessa forma: il ciclo precedente aveva misurato che `coverage` si
   perdeva perché cadeva **dopo** il momento in cui si scrive il tracciato. **Un passo che cade fra
   due momenti in cui si scrive il tracciato tende a saltare**, e nessuna delle due volte è stata
   pigrizia.
6. **La clausola 37 è preparata ma non applicata**: i due testi stanno in
   `handoff/configurazione-da-applicare.md` e toccano `~/.claude/`, cioè una superficie di
   configurazione. **È un gesto dell'utente.** ⚠️ E va detto che **il capo stesso ha violato quella
   regola una volta**, usando `sed -i` su un file di progetto e dichiarandolo nel messaggio di
   commit: zero violazioni su undici implementer nel ciclo precedente, **una su un capo**. La
   direttiva non vince contro chi ha il divieto fresco davanti in un mandato appena letto: vince
   nell'attrito di un'operazione meccanica fatta di fretta.
7. **Quattro premesse del rapporto precedente sono cadute su quattro verificate.** Nessuna è stata
   scoperta rileggendolo: sono emerse perché ogni mandato chiedeva di **verificare il fatto** prima
   di correggere, dichiarando che se il fatto fosse caduto la decisione sarebbe caduta con lui.
   **È il risultato metodologico del goal**, e vale più delle singole voci.

## POI

Archivia in `storico/handoff/` **solo** le unità `FATTO` — con `git mv`, senza chiedere: è
l'eccezione dichiarata nel `CLAUDE.md`.

⚠️ **L'unità 01 è `PARZIALE`**, e il suo lavoro è stato completato dalla `01b`: **decidi tu se
archiviarla, e dichiara la decisione.** Le altre cinque sono `FATTO`.

⚠️ **Attenzione ai nomi.** `storico/handoff/` contiene già i cicli precedenti, e il ciclo del 19
settembre è archiviato con un **prefisso di data** — `2026-09-19-PIANO-sedici-rilievi.md`,
`2026-09-19-CHIUSURA-sedici-rilievi.md`, `2026-09-19-collaudo/` — proprio perché i nomi si
ripetono. **Fai lo stesso.**

**Non archiviare** `handoff/collaudo/`, né questo file, né
`handoff/configurazione-da-applicare.md`, che è lavoro dell'utente ancora da fare.

Lascia il rapporto **in testa a questo file**, nel formato che `architettura-sessioni.md` prescrive.
`COPERTURA:` porta **tre numeri che devono sommare a 38**, e **non ammette il valore `completa`**.
