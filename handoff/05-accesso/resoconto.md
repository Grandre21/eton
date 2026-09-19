# UNITÀ 05 — resoconto

```
UNITÀ: 5 — ESITO: FATTO
```

**Branch:** `worktree-05-accesso` — **percorso:** `G:\Sviluppo\Eton\.claude\worktrees\05-accesso`
Nato da `201fc11`, identico a `main` al momento dell'apertura (`git diff --stat main HEAD` vuoto).
L'integrazione su `main` la fai tu.

---

## TOCCATI

```
Services/PkceStore.cs             → +33/−3
Services/SupabaseService.cs       → +14/−19
Services/OAuthCallback.cs         → +22/−0
Eton.Tests/OAuthCallbackTests.cs  → +39/−0
```

Quattro file, tutti del perimetro assegnato. `git status --porcelain` non ne elenca altri.
**Nessuna pagina toccata**, come il mandato vieta.

---

## REVIEW

Due unità di implementazione, partizionate per proprietà dei file e senza sovrapposizioni:
**A** = `PkceStore.cs` + `SupabaseService.cs`; **B** = `OAuthCallback.cs` + `OAuthCallbackTests.cs`.
Il gate del §3 è stato valutato **sul diff di ciascuna**.

```
review, unità A:
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0

review, unità B:
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0

review, diff complessivo:
  backend-expert  RILIEVI: 5
  checker         VERDETTI: risolti 7 · non risolti 0
```

Due note sul tracciato, entrambe necessarie perché il conteggio sia verificabile:

1. **Non c'è nessuna voce «non lanciato».** Per singola unità `backend-expert` non sarebbe scattato —
   A: `2 files changed, 47 insertions(+), 22 deletions(-)`; B: `2 files changed, 61 insertions(+)`;
   `0 create mode`; `0 dichiarazioni/endpoint` — ma il **totale** è `4 files changed, 108
   insertions(+), 22 deletions(-)`, cioè sopra la soglia dei ~120 nel conteggio al momento della
   valutazione (126 prima che le correzioni togliessero righe). L'ho lanciato sul diff complessivo
   invece di scrivere un'esenzione formalmente corretta ma contestabile. **Ha prodotto 5 rilievi,
   tutti fondati**: l'esenzione sarebbe costata un giro.
2. **La voce `checker` non è l'istruttoria del §4 ma quella del §5.** `bug-hunter` e `conformity`
   sommano **zero** rilievi su entrambe le unità, quindi per loro la voce sarebbe `nessuna
   istruttoria`; il `checker` è stato lanciato sui **fix** di `backend-expert`, come il §5 prescrive.
   La sua riga lo dichiara: `ESAMINATI: 7 (bug-hunter: 0, conformity: 0, fix: 7)`.

Fuori dal tracciato del §3, due agenti chiamati per decidere e per istruire, non per revisionare il diff:
`tech-advisor` (i due bivi del punto 1 e del punto 4) e `doc-checker` (il punto 3).

---

## CONTRATTI

### Il tipo di esito OAuth e la sua enumerazione — **intatti**

```
Services/OAuthCallback.cs:4     public enum OAuthRifiuto
Services/OAuthCallback.cs:20    public sealed record OAuthCallbackEsito(string? Codice, OAuthRifiuto Errore, string? Diagnostica);
Services/OAuthCallback.cs:33    public static OAuthCallbackEsito Analizza(string uri)
```

Le tre forme sono **identiche** a quelle del mandato, carattere per carattere. Nessun membro
aggiunto all'enum, nessun campo al record, nessun cambio alla firma di `Analizza`.

**La diagnostica non raggiunge lo schermo in nessun ramo.** `esito.Diagnostica` compare una volta
sola in tutto il diff, a `Services/SupabaseService.cs:113`, dentro `Console.Error.WriteLine`. Il
metodo nuovo del punto 4 riceve **solo** un valore dell'enumerazione:

```
Services/OAuthCallback.cs:88    public static string? FraseRifiuto(OAuthRifiuto rifiuto) => rifiuto switch
```

Non ha accesso né alla query grezza né al record. Verificato da me aprendo il file, e confermato
indipendentemente da `threat-hunter` su entrambe le unità.

### Lo `switch` spostato — nessuna copia lasciata indietro

```
Services/SupabaseService.cs:115                    ErroreAccesso = OAuthCallback.FraseRifiuto(esito.Errore);
```

`grep` delle tre frasi in `SupabaseService.cs` restituisce **0**: la loro unica sede è ora
`OAuthCallback.cs`. Le tre stringhe sono state spostate **verbatim** — confronto carattere per
carattere contro `git show HEAD:Services/SupabaseService.cs`, fatto da `bug-hunter`, `conformity` e
`threat-hunter` indipendentemente.

### Le tre firme di `PkceStore` — **nessuna cambiata**

```
Services/PkceStore.cs:25        public void Salva(string verificatore)
Services/PkceStore.cs:32        public string? Leggi()
Services/PkceStore.cs:49        public void Cancella()
```

Il mandato le autorizzava a cambiare «se la protezione lo richiede». **Non lo ha richiesto**, e il
perché è nello scostamento 1: la protezione utile sta dentro il corpo di due metodi su tre, e il
terzo non va protetto affatto. Il consumatore unico — `SupabaseService` — non ha quindi nessun
disallineamento possibile.

### Il precedente rispecchiato

```
Services/BrowserSessionHandler.cs:39    public Session? LoadSession()
```

Non toccato. `Leggi()` e `Cancella()` ne riproducono la forma: `try` attorno alla chiamata JS,
`catch (Exception ex)`, una riga su `Console.Error` col marcatore `[Auth]` e `{ex.Message}`, un
ripiego. **Una divergenza c'è, ed è dichiarata**: `Salva()` resta scoperto — v. scostamento 1.

---

## ADJUDICA

I sei revisori del §3 hanno prodotto **zero rilievi**. Non c'è quindi nessun infondato da
riverificare a campione: la dichiarazione che il §5 chiede vale come **dichiarazione di assenza**,
non come passo saltato. Ho comunque riaperto io i `file:line` portanti — `PkceStore.cs` per intero,
`SupabaseService.cs:105-125` e `:186-214`, `OAuthCallback.cs:74-102`, `OAuthCallbackTests.cs:189-222`,
`SignOutAsync` per intero — perché il rilievo tocca il percorso d'accesso, che il §5 impone di aprire
di persona qualunque sia il verdetto.

**I cinque rilievi di `backend-expert`, tutti `TIPO: unità`, tutti fondati, tutti corretti.**
Nessuno è `TIPO: progetto`, quindi nessuno va rimesso a te per decisione.

1. **`OAuthCallback.cs:79-80` e `SupabaseService.cs:115-118` — il commento dava una ragione falsa.
   FONDATO → corretto.** Affermava che fosse la *collocazione accanto all'enum* a rendere scrivibile
   il test su `Enum.GetValues`. È falso: il test funzionerebbe identico col metodo in qualsiasi file;
   ciò che lo rende chiamabile senza browser è l'estrazione in un **metodo puro** fuori da
   `SupabaseService`. **L'errore era nel mio brief**, e da lì si era propagato in due file. La ragione
   vera della collocazione è lo statuto di `Testi.cs:6` — «I testi che compaiono identici in più
   pagine» — che `FraseRifiuto`, con un solo chiamante, non soddisfa: la convenzione del progetto
   resta **una sola**. Aperto io `Testi.cs:6`: il claim regge.
   `verificato: risolto — Services/OAuthCallback.cs:76-87`
2. **`SupabaseService.cs:338-345` — `try`/`catch` morto in `SignOutAsync`. FONDATO → corretto.**
   Avevo deciso di **tenerlo**, sull'argomento che un metodo il quale dichiara di non propagare mai
   eccezioni non dovrebbe appoggiarsi alla garanzia di un'altra classe. `backend-expert` ha portato
   un argomento migliore, che non avevo considerato: `_pkce.Cancella()` è già chiamato **nudo** nello
   stesso file a `:121` e `:235`, quindi tenerlo protetto solo lì dà al lettore due forme per lo
   stesso metodo senza una regola. Aperto io il file: i tre call-site sono a `:121`, `:235`, `:340`,
   il claim regge. **Ho cambiato decisione.** Il blocco è stato sostituito dalla chiamata nuda con un
   commento che dichiara che non è una svista, e il `<summary>` del metodo è stato corretto
   («Ogni passo **che può lanciare** ha il proprio `try`»), perché il fix lo rendeva falso.
   `verificato: risolto — Services/SupabaseService.cs:329-332`
3. **`SupabaseService.cs:198` e `:211` — le due frasi spiegavano il meccanismo interno. FONDATO →
   corretto.** 82 e 54 parole, contro le 32 della frase gemella dello stesso metodo (`:230`). La prima
   apriva con una parafrasi di PKCE («la prova che l'accesso era partito da questo browser»), la
   seconda descriveva il payload della risposta — cioè «sessione senza utente» riscritto più lungo.
   Accorciate a due periodi con la struttura fatto → causa → azione. ⚠️ **La prima nomina ancora
   entrambe le cause**, come il mandato impone: non è un'omissione recuperabile accorciando.
   `verificato: risolto — Services/SupabaseService.cs:192 e :202`
4. **`PkceStore.cs:20-41` — commenti lunghi con riferimenti che invecchiano. FONDATO → corretto.**
   Citavano per **numero di riga** la struttura di tre file esterni: falsi al primo commit su quei
   file. Qui c'era un conflitto fra due pareri — `tech-advisor` voleva che il commento di `Salva`
   nominasse *dove* sta la copertura, `backend-expert` voleva via i `file:line`. Risolto nominando il
   **metodo** (`Benvenuto.Accedi`), che è stabile, invece della riga. L'avvertimento sul secondo
   chiamante futuro è stato conservato: era l'informazione che rende il commento utile.
   `grep -cE "razor:[0-9]" Services/PkceStore.cs` → **0**.
   `verificato: risolto — Services/PkceStore.cs:20-24`
5. **`OAuthCallbackTests.cs:211-214` — il messaggio diagnostico sull'asserzione sbagliata. FONDATO →
   corretto.** ⚠️ **Questo rilievo smentisce una correzione che avevo chiesto io**, e va detto per
   intero. Aprendo il test avevo trovato che `Assert.NotNull(frase)` è più debole della specifica —
   `""` non è null, non è uguale alla generica, e non è una frase — e avevo fatto sostituire con
   `Assert.False(string.IsNullOrWhiteSpace(frase), "<messaggio>")`. Il fix era giusto sul caso della
   stringa vuota e **sbagliato sul caso principale**: un valore nuovo cade sul ramo `_` e riceve la
   frase *generica*, che non è vuota, quindi quell'asserzione **passa** e a fallire è `Assert.NotEqual`,
   che non porta messaggio. Avevo messo la diagnostica dove nel caso realistico non scatta. Le due
   asserzioni sono state unite in una sola: erano una proprietà sola.
   `verificato: risolto — Eton.Tests/OAuthCallbackTests.cs:215-217`

---

## I QUATTRO PUNTI

### 1 — Le chiamate a `localStorage`

La protezione sta **dentro `PkceStore`**, come in `BrowserSessionHandler`: un punto solo copre i tre
call-site scoperti (`SupabaseService.cs:121`, `:189`, `:235` nella numerazione finale).

**Come ho verificato la protezione: leggendo.** Quella parola, come il mandato chiede. Nessuno dei
due gate la prova — una chiamata non protetta compila e i test passano — e provarla davvero
richiederebbe un browser con l'archiviazione bloccata, che il mandato mi vieta. Resta **da provare
nel browser**, ed è nel tuo ciclo chiuso.

⚠️ **Le quattro non erano quattro difetti: erano tre.** V. scostamento 1.

### 2 — Le due frasi

Riscritte entrambe, poi accorciate dopo la review. La prima (`:192`) nomina **due** cause perché,
dopo il punto 1, `Leggi()` restituisce `null` sia per verificatore già speso sia per archiviazione
bloccata, e da valle sono indistinguibili: indovinarne una renderebbe la frase falsa nell'altro caso
— che è esattamente l'errore che il mandato dice essere già stato respinto una volta. La seconda
(`:202`) ha causa unica e può essere netta. «Sessione senza utente» non compare più in nessun ramo.

### 3 — La barra gialla

> **Non determinato, e ho escluso:** i percorsi di salvataggio delle tre schermate del giro C, che
> sono interamente dentro `try`/`catch` e producono il messaggio italiano osservato
> (`SpesaEdit.razor:336-345`, `SpaceDetail.razor:252-260`, `CollectionEdit.razor:605-614`);
> `GetClientAsync()`, il tratto comune ai 52 call-site, che non propaga mai eccezioni di rete;
> `SpaceStateService`, l'altro servizio condiviso che tocca `localStorage`, protetto su tutte e tre
> le chiamate; il **refresh automatico interno di Gotrue**, che era l'ipotesi più forte e che
> `doc-checker` ha **smentito** sul sorgente della versione installata; il service worker, che non
> passa dal `window.fetch` della pagina; e i componenti di `Shared/` montati su ogni pagina privata,
> che dove fanno rete hanno il proprio `catch`.

Il dettaglio che regge l'esclusione più importante: `AutoRefreshToken` è `true` di default
(confermato sul commit `d67eb156`, inciso nel `.nuspec` di Supabase.Gotrue 6.3.0) e il timer esiste,
**ma** `HandleRefreshTimerTick` — benché sia `async void` — avvolge tutto in un `try`/`catch
(Exception)` che non rilancia mai. Il percorso è progettato per fallire in silenzio, quindi non può
produrre l'eccezione non gestita che mostra la barra.

**E il punto 1 non la fa sparire, contrariamente a quanto la voce ipotizzava.** La barra non è mai
stata osservata sul percorso d'accesso: `C-esito.md` dichiara le prove OAuth fra i **NON PROVATO**,
perché richiedevano di essere disconnessi. Le tre osservazioni vengono da Spese, Spazi e Collezione.
**La voce non si chiude da sé e non appartiene a questa unità**: v. `FUORI SCOPE` 1.

### 4 — Una frase per ogni valore

Fatto, e **provato per mutazione**, perché il gate verde dice solo che il test passa — non che
fallirebbe quando deve.

Aggiunto `OAuthRifiuto.Sospeso` all'enumerazione senza un caso nello `switch`, ricompilato ed
eseguito:

```
Non superato  Eton.Tests.OAuthCallbackTests.Ogni_rifiuto_dichiarato_ha_una_frase_propria [4 ms]
  Sospeso non ha una frase propria: aggiungila in OAuthCallback.FraseRifiuto.
Non superati: 1. Superati: 289. Totale: 290.
```

Il test fallisce **e nomina il valore**. È anche la prova che il rilievo 5 serviva: senza quella
correzione il rosso sarebbe arrivato da `Assert.NotEqual`, che avrebbe stampato due stringhe identiche
senza dire quale valore mancava.

**File ripristinato**: `md5sum` di `Services/OAuthCallback.cs` tornato a
`e1a4f931d7f27d3581b3709ede2f7ae7`, identico all'impronta presa prima della mutazione. Il ripristino
è stato fatto **togliendo le righe a mano**, non con `git checkout`, che avrebbe cancellato tutto il
lavoro non ancora committato.

---

## FUORI SCOPE

### 1. La barra gialla di Blazor vive fuori dal percorso d'accesso — istruttoria chiusa, difetto aperto

Non è un rilievo che ho lasciato cadere: è la **conclusione** del punto 3. Le tre osservazioni del
giro C vengono da schermate di registro, non dall'accesso, e nessuna delle cause candidate che ho
potuto esaminare regge. Ciò che **non** ho potuto escludere è la forma esatta della sovrascrittura di
`window.fetch` usata nella simulazione: se sostituiva `fetch` con una funzione che **lancia
sincronamente** invece di restituire una Promise rifiutata, il punto di fallimento si sposta dentro
il marshalling di Blazor e può non essere catturabile dal `catch` C# attorno all'`await`. Non ho il
codice della simulazione, e senza quello l'ipotesi non è né confermabile né escludibile.

**Il modo economico di chiuderla** è una riga in più nel brief del prossimo giro di collaudo: riportare
**verbatim** lo snippet usato per bloccare la rete. Costa una riga e decide fra due ipotesi.

### 2. `BrowserSessionHandler.SaveSession` e `DestroySession` restano scoperti

`Services/BrowserSessionHandler.cs:25` e `:28` chiamano `localStorage` senza `try`. **Non sono un
difetto da correggere alla cieca**, ed è per questo che non li ho toccati: sono scoperti *di proposito*
nello stesso file che contiene il precedente protetto, e il mandato me lo assegnava come modello, non
come file da riscrivere. Lo segnalo solo perché il ragionamento che ho applicato a `PkceStore` —
proteggere dove esiste un ripiego onesto — su `SaveSession` darebbe probabilmente la stessa risposta
(nessun ripiego onesto: una sessione non salvata è una sessione persa al reload), ma è una verifica
che nessuno ha fatto e che richiede il suo giro.

---

## GATE

```
dotnet build Eton.sln -warnaserror --no-incremental  →  Avvisi: 0, Errori: 0
dotnet test Eton.sln                                 →  Superati: 290, Non superati: 0, Totale: 290
```

Il mandato chiedeva «almeno 288/288»; il punto 4 ne ha aggiunti **due** — il test di completezza e
l'invariante di `Nessuno` — da 288 a 290.

Compilato **io**, a implementer fermi: `obj/` non ha lock fra processi. Ogni implementer aveva il
divieto esplicito di compilare, e nessuno lo ha violato. **Server di sviluppo non avviato, browser non
aperto**, come il mandato vieta: nessun processo lasciato vivo, nessuna porta occupata.

---

## SCOSTAMENTI

### 1. ⚠️ Le chiamate scoperte sono **tre**, non quattro — e la quarta non andava protetta

Il mandato dice «il tuo perimetro sono tutte e quattro» e «allinea le quattro chiamate a lui».
L'ho eseguito su tre, e sulla quarta ho fatto l'opposto. Il motivo è un fatto che il mandato non
conosceva, ed è verificabile in due righe.

La premessa della voce 1 — «la chiamata JavaScript lancia, nessuno la raccoglie, e l'utente finisce
sulla barra d'errore generica di Blazor» — è **vera per tre** e **falsa per la quarta**.
`_pkce.Salva()` sta in `AvviaAccessoGoogleAsync()`, che davvero non ha alcun `try` in tutta la sua
lunghezza, come il mandato osserva. Ma quel metodo ha **un solo call-site in tutto il progetto**,
`Pages/Benvenuto.razor:223`, e lì è invocato **dentro** un `try`/`catch` (`:221-243`) che già scrive
in console col marcatore `[Auth]`, già rimette il pulsante in stato premibile, e già mostra una frase
**mirata proprio a questo caso**: «può essere il browser che non lascia salvare i dati di questo sito
— succede con la navigazione anonima…». Il commento a `:229-236` dichiara di sapere esattamente
questo.

**Proteggere `Salva()` sarebbe stato un peggioramento misurabile**, non un'esecuzione fedele: avrebbe
reso morto quel `catch`, l'applicazione sarebbe partita verso Google con un verificatore mai salvato, e
l'utente avrebbe ricevuto al ritorno un messaggio generico invece della frase mirata che oggi riceve
**subito**, senza nemmeno uscire dall'applicazione.

Il precedente che il mandato indica conferma la lettura: `BrowserSessionHandler` protegge `LoadSession`
e lascia scoperti `SaveSession` e `DestroySession`. **Non dice «proteggi tutto»: dice «proteggi dove
esiste un ripiego onesto».** Per `Leggi` il ripiego è `null` ed è onesto; per `Cancella` è «niente», e
il verificatore è monouso; per `Salva` non esiste, e l'unica cosa giusta è non partire.

Il bivio è stato portato a `tech-advisor` **prima** di scrivere i brief, con le tre opzioni in chiaro e
l'invito esplicito a smentirmi. Ha confermato, ha verificato che non resti nessun percorso verso la
barra gialla, e ha aggiunto la condizione che ho eseguito: che il commento su `Salva` nomini **dove**
vive la copertura, perché sta in un file che lo store non vede.

**Cosa la voce 1 produce, quindi:** tre protezioni scritte, e sulla quarta una **dichiarazione** —
`PkceStore.cs:20-24` — che impedisce al prossimo lettore di «uniformare» i tre metodi spegnendo la
frase. Senza quel commento la regressione sarebbe stata a un passo, e sarebbe sembrata una pulizia.

### 2. `backend-expert` lanciato benché il gate per unità non lo imponesse

V. la nota 1 del tracciato. Per singola unità non scattava; sul totale sì, al conteggio del momento.
Ha prodotto 5 rilievi fondati, il più acuto dei quali ha smentito una mia correzione.

### 3. Due implementer in due messaggi separati invece che in uno

Il §2 chiede che gli implementer partano **nello stesso messaggio**. Ho lanciato A e poi B nel
messaggio successivo. In questo harness gli agenti sono asincroni e B è partito mentre A lavorava —
il parallelismo c'è stato — ma la regola dice «stesso messaggio» e non l'ho rispettata alla lettera.
Nessuna conseguenza osservabile: nessun file condiviso, e il contratto fra i due era una firma fissata
testualmente in entrambi i brief.

### 4. Una correzione è nata da me, non da un revisore

Il rilievo sull'asserzione `Assert.NotNull` l'ho trovato io aprendo il test, e l'ho fatto correggere
prima della review di `backend-expert`. Lo dichiaro perché nel tracciato non compare da nessuna parte:
un difetto trovato aprendo il codice di persona e uno tornato da un agente non hanno lo stesso peso
probatorio, e il secondo giro su quella stessa riga (rilievo 5) è la ragione per cui la distinzione
conta.

---

## PER TE, PRIMA DELLA PROVA NEL BROWSER

Tre cose che il mio perimetro non poteva provare e che il tuo ciclo chiuso sì:

1. **`localStorage` bloccato + «Entra con Google»** → deve comparire la frase di
   `Benvenuto.razor:241` **subito**, senza uscire dall'applicazione, e il pulsante deve tornare
   premibile. Se invece parte verso Google, lo scostamento 1 è sbagliato e `Salva()` va protetto.
2. **`localStorage` bloccato + ritorno con `?code=…`** → deve comparire la frase nuova di
   `SupabaseService.cs:192`, **non** la barra gialla di Blazor. È la prova vera del punto 1, quella
   che i gate non danno.
3. **`?error=access_denied`** → la frase di `Annullato`, invariata, che ora viene da
   `OAuthCallback.FraseRifiuto`. Serve a confermare che lo spostamento del punto 4 non ha rotto il
   percorso reale.

Per il punto 3 della voce, se rifai un giro a rete bloccata: **riporta verbatim lo snippet** usato per
sovrascrivere `window.fetch`. È l'unico dato che manca per chiudere `FUORI SCOPE` 1.
