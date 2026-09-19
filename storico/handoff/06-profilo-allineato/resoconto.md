# UNITÀ 06 — resoconto

```
UNITÀ: 6 — ESITO: FATTO
```

**Branch:** `worktree-06-profilo-allineato` — **percorso:** `G:\Sviluppo\Eton\.claude\worktrees\06-profilo-allineato`
Nato da `b48bf3b`, identico a `main` al momento dell'apertura (`git diff --stat main HEAD` vuoto) e
allineato con `origin/main`, che il capo aveva già pushato. L'integrazione su `main` la fai tu.

---

## LA FORMA SCELTA PER IL VINCOLO DI RETE, E PERCHÉ

**Zero chiamate aggiuntive nel caso normale. Una sola quando serve davvero.**

Si tiene in `localStorage`, alla chiave `eton.profilo`, un'**impronta** di ciò che questo dispositivo
ha già scritto con successo: `utenteId ␟ nome ␟ foto`. Al termine del bootstrap si confronta con i
valori vivi della sessione, che il client sa già calcolare — e questo è il punto su cui poggia tutto:

> `handle_new_user` sceglie il nome con `coalesce(full_name, name, email)` e la foto da `avatar_url`.
> `IdentitaGoogle` applica **la stessa precedenza** sugli stessi metadati. Il client conosce quindi
> esattamente il valore che il database avrebbe scritto, e non ha bisogno di chiederglielo.

Quindi: **nessuna lettura di `profiles`, mai.** Se l'impronta combacia — il caso di ogni avvio di ogni
utente che non ha cambiato nulla — non parte **nessuna** chiamata di rete. Se differisce, parte **una**
scrittura, e l'impronta si aggiorna **solo se quella scrittura ha davvero toccato una riga**.

**Il correttivo che rende l'impronta onesta, e che non era nel disegno iniziale.** L'impronta non
afferma «il database contiene questo»: afferma «*questo dispositivo* ha scritto questo». Le due
divergono in uno scenario reale: scrivo `N1` dal telefono, cambio in `N2` e lo scrive il PC, torno a
`N1` — il telefono confronta `N1` con la **propria** impronta `N1`, non scrive, e il database resta a
`N2` **per sempre** se il PC non torna mai in uso. Per questo, **al ritorno da un accesso Google
l'impronta si ignora e si scrive comunque**: diventa una scrittura per *login*, non per *avvio*, e i
login sono rari perché il refresh token tiene viva la sessione per settimane.

Il vincolo del mandato regge in entrambi i rami: **0** chiamate quando si riapre l'applicazione, **1**
quando si fa l'accesso.

## IL LIMITE DICHIARATO

**Chi non riapre mai l'applicazione resta col nome e la foto vecchi agli occhi degli altri.** Questa
strada non lo copre e non può coprirlo: la scrittura parte dal client di chi ha cambiato. Un trigger
sul database coprirebbe anche loro, e resta la strada giusta il giorno in cui si toccherà lo schema
per altro.

⚠️ **E un secondo limite, che il mandato non nominava e che ho trovato strada facendo.** Il valore
fresco arriva dai metadati della **sessione viva**. Quei metadati li riscrive il server Gotrue quando
si fa l'accesso; **non ho potuto verificare** se un rinnovo del token li aggiorni, né se il server
faccia *merge* o *sostituzione* di `raw_user_meta_data` fra un accesso e l'altro. Da questo dipende un
comportamento osservabile: se il server fa merge, una foto **rimossa** su Google non si propaga (la
chiave vecchia sopravvive); se fa sostituzione, si propaga. È materia di collaudo, non di lettura.

---

## TOCCATI

```
Services/IdentitaGoogle.cs             → +40/−0   (nuovo)
Services/AllineatoreProfilo.cs         → +121/−0  (nuovo)
Services/AuthStateService.cs           → +1/−16
Services/SupabaseService.cs            → +15/−1
Program.cs                             → +1/−0
Eton.Tests/IdentitaGoogleTests.cs      → +63/−0   (nuovo)
Eton.Tests/AllineatoreProfiloTests.cs  → +73/−0   (nuovo)
```

`7 files changed, 314 insertions(+), 17 deletions(-)` · `4 create mode`. `git status --porcelain` non
elenca altro. **`Models/Profile.cs`, `Services/SpaceRepository.cs` e l'intera `supabase/` non
compaiono nel diff**: verificato con `git diff --stat` mirato, esito vuoto.

---

## REVIEW

Due unità di implementazione, partizionate per proprietà dei file, senza sovrapposizioni:
**A** = `IdentitaGoogle.cs` + `AuthStateService.cs` + il suo test; **B** = `AllineatoreProfilo.cs` +
`Program.cs` + `SupabaseService.cs` + il suo test. Il gate del §3 è stato valutato **sul diff di
ciascuna**, e per entrambe scattavano tutti e quattro i revisori: file nuovi e dichiarazioni di tipo.

```
review, unità A:
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 3

review, unità B:
  bug-hunter      RILIEVI: 2
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 3

istruttoria:
  checker         VERDETTI: fondati 4 · infondati 0 · fuori scope 0 · non verificabili 0
  checker (fix)   VERDETTI: risolti 7 · non risolti 0 · non verificabili 0
```

**Nessuna voce «non lanciato»:** gli otto revisori previsti sono stati lanciati tutti e otto.
`coverage` non compare perché il §6 ne vieta il lancio dentro una sessione-unità, e il mandato lo
conferma chiedendo il tracciato senza.

Le due voci `checker` sono passi diversi, entrambi dovuti: la prima è l'istruttoria del §4 sui rilievi
di `bug-hunter` e `conformity` (che sommano **4**, quindi si lancia); la seconda è quella del §5 sui
**fix**, dove il claim da istruire è la correzione e il verdetto è `risolto | non risolto`.

Fuori dal tracciato, due agenti chiamati per decidere e per verificare, non per revisionare il diff:
`tech-advisor` sul bivio della forma, e `doc-checker` sulle firme di Gotrue 6.3.0
(`3 claim — confermati 3 · smentiti 0`).

---

## CONTRATTI

### Le due firme di `AuthStateService` — **invariate, e perfino alle stesse righe**

```
Services/AuthStateService.cs:30     public async Task<string?> GetUserIdAsync()
Services/AuthStateService.cs:43     public async Task<string?> GetDisplayNameAsync()
```

Identiche a quelle del mandato, carattere per carattere. `GetDisplayNameAsync` ha cambiato **corpo**,
non firma: da 16 righe inline a una delega. `GetUserIdAsync` non è stata toccata affatto.

**La firma d'istanza per l'immagine che il mandato autorizzava non è stata aggiunta** — v. scostamento 4.

### `Models/Profile.cs` — **intatto, e non nel diff**

```
Models/Profile.cs:11        [Column("display_name")] public string? DisplayName { get; set; }
Models/Profile.cs:12        [Column("avatar_url")]   public string? AvatarUrl { get; set; }
```

Letto prima di concludere, come il mandato chiedeva. L'attributo su `updated_at` (`:14-17`) riguarda
`Update(modello)` sull'intero modello; qui si usa `.Set()` su due colonne nominate, quindi la
questione non si pone — confermato da `threat-hunter`, che ha verificato che nel `PATCH` non parte
nessuna colonna oltre alle due concesse dal `grant`.

### `SpaceRepository.MembriAsync` — **invariata: è il consumatore**

```
Services/SpaceRepository.cs:103     public async Task<IReadOnlyList<Membro>> MembriAsync(Guid spazioId)
```

File non toccato. Il suo ripiego a «utente» resta. **Verificato di persona che è l'unico punto
dell'applicazione che tocca quella tabella**: `grep` di `From<Profile>` su tutto il progetto restituisce
**una** occorrenza, `SpaceRepository.cs:116`, ed è una `Get()`. Il claim su cui poggia il mandato —
«nessun punto dell'applicazione li aggiorna mai» — è quindi confermato, non assunto.

### Il call-site nel bootstrap — **uno solo**

```
Services/SupabaseService.cs:156                 await _profilo.AllineaAsync(_facade, dopoAccesso);
```

Sta **dopo** il `finally` che rilascia `_initLock` e prima del `return _facade`, protetto da
`if (primoAvvio)`. Passa `_facade` e non `GetClientAsync()`.

⚠️ **Il resoconto dell'unità 05 è stato letto per intero prima di aprire il file**, campi `TOCCATI` e
`CONTRATTI` compresi, come il mandato ordinava. Nulla del suo lavoro è stato toccato: le quattro
chiamate protette, le due frasi riscritte e lo `switch` estratto sono dove le ha lasciate.

---

## ADJUDICA

Dieci rilievi in tutto. **Otto fondati e corretti, uno infondato, uno fondato ma fuori scope.**

1. **`IdentitaGoogle.cs` — predicato duplicato fra i due metodi (`backend-expert` A). FONDATO →
   corretto.** Lo stesso «chiave presente, è stringa, non vuota» compariva due volte, e il controllo
   `UserMetadata is not null` due volte ma commentato una sola. Estratto in un helper **privato** con
   **tre** call-site: non viola il budget «niente helper con un solo call-site», lo serve.
   `NomeDa` è ora una riga che rispecchia visivamente il `coalesce` della SQL che il commento cita.
   `verificato: risolto — Services/IdentitaGoogle.cs:35-39`
2. **`IdentitaGoogle.cs:9-18` — il commento di classe sovradichiarava (`backend-expert` A). FONDATO →
   corretto.** Diceva che la cascata C# «è la stessa» della SQL. Non lo è: `coalesce` salta solo i
   `NULL`, il C# scarta anche vuoti, soli spazi e non-stringa. Detto così, induceva a «allineare» il
   codice togliendo `IsNullOrWhiteSpace`. Ora dichiara che è la **precedenza** a essere condivisa e che
   il C# è più severo.
   `verificato: risolto — Services/IdentitaGoogle.cs:9-18`
3. **Manca il test sul ramo `UserMetadata` null (`bug-hunter` A **e** `backend-expert` A). FONDATO →
   corretto.** Due famiglie diverse arrivate indipendentemente allo stesso punto. Il controllo
   difensivo era l'unica riga difesa **solo dalla prosa**, e la prosa non fallisce.
   ⚠️ **Il fatto è stato misurato, non argomentato**: `bug-hunter` ha costruito un harness fuori dal
   progetto che carica l'assembly reale e deserializza `{"user_metadata": null}` — la proprietà
   risulta `null` a runtime, nonostante l'inizializzatore di campo e l'annotazione non-nullable; con
   la chiave **assente** resta il dizionario vuoto. Il controllo non è codice morto.
   `verificato: risolto — Eton.Tests/IdentitaGoogleTests.cs:56-62`
4. **`AllineatoreProfilo.cs` — il commento della guardia sul nome diceva il falso (`bug-hunter` B,
   `SEVERITY: alta`, più `backend-expert` B punto 6). FONDATO → corretto, e il difetto era nel mio
   brief.** Il commento affermava che senza la guardia «una sessione con metadati vuoti cancellerebbe
   un nome buono». Falso: con metadati vuoti `NomeDa` ripiega sull'email, quindi la guardia **non
   scatta** e la scrittura procede mettendo l'email al posto del nome — che è esattamente ciò che
   `handle_new_user` fa al primo accesso. La guardia scatta solo quando manca **anche** l'email.
   **Il codice non è stato cambiato**: v. il punto sotto.
   `verificato: risolto — Services/AllineatoreProfilo.cs:57-68`
5. **L'asimmetria nome/foto era reale ma non documentata (parte del rilievo 4). FONDATO sulla
   documentazione, INFONDATO sul comportamento — e la distinzione l'ho chiesta io al `checker`.**
   `bug-hunter` chiedeva una guardia anche su `foto`. Non è stata messa, e il motivo è il mandato:
   chiede di propagare il valore nuovo quando l'utente cambia «nome **o** foto», e se la foto è stata
   rimossa il valore nuovo *è* l'assenza. Una guardia che non scrivesse mai `null` lascerebbe sulla
   foto lo stesso difetto che l'unità chiude sul nome. `foto` non ha ripiego neanche in
   `handle_new_user`. Lo scenario dannoso richiederebbe che la chiave ci fosse al primo accesso e
   sparisse dopo senza una rimozione volontaria: **il `checker` non ha potuto escluderlo**, perché
   dipende da un fatto del server esterno — lo stesso che sta nel secondo limite dichiarato sopra.
   Corretto quindi documentando l'asimmetria, non cambiando il codice.
   `verificato: risolto — Services/AllineatoreProfilo.cs:62-67`
6. **`Leggi()` non normalizzava la stringa vuota (`conformity` B). FONDATO → corretto.** Era l'unica
   lettura di stringa da `localStorage` del progetto senza `string.IsNullOrEmpty(valore) ? null :
   valore`. Impatto funzionale nullo — il `checker` ha verificato che l'esito non cambia nemmeno con
   un `localStorage` manomesso a mano — ma è conformità a uno schema che il progetto applica ovunque.
   `verificato: risolto — Services/AllineatoreProfilo.cs:94-98`
7. **Collisione del separatore dell'impronta (`bug-hunter` B). FONDATO → corretto.** Con `\n`,
   `(nome="A", foto="B\nC")` e `(nome="A\nB", foto="C")` producevano la **stessa** impronta: due valori
   vivi diversi con la stessa impronta significano **una scrittura dovuta saltata in silenzio**, cioè
   il difetto che l'unità esiste per chiudere. Separatore cambiato in `\u001F`, e **aggiunto il test
   che il `FIX` chiedeva** — che avevo omesso dal primo brief di correzione, applicando a me stesso
   l'errore contestato al punto 3. Il `checker` ha calcolato a mano le due impronte e confermato che
   il test fallirebbe se qualcuno rimettesse `\n`.
   `verificato: risolto — Services/AllineatoreProfilo.cs:33-34 e Eton.Tests/AllineatoreProfiloTests.cs:69-72`
8. **Il commento del call-site era fuorviante (`backend-expert` B). FONDATO → corretto, e anche questo
   veniva dal mio brief.** Sosteneva che passare `_facade` evitasse un «deadlock silenzioso» via
   `RinnovaSessioneSeServeAsync`. **Falso nella collocazione scelta**: quel codice sta dopo il
   `finally` che ha già rilasciato il semaforo, e lì un deadlock non può accadere — sarebbe reale solo
   *dentro* il lock, cioè nella posizione che il commento stava escludendo. Chi leggeva ne usciva con
   un modello sbagliato. Sostituito dalle due ragioni che reggono.
   `verificato: risolto — Services/SupabaseService.cs:152-156`
9. **Togliere `Impronta`/`VaScritto` pubblici e i nove test (`backend-expert` B). INFONDATO.**
   L'argomento era che i test provano `!=` e `||`. **L'infondato riverificato a campione come impone il
   §5 è questo**, e l'ho riverificato aprendo `Services/SessionFreshness.cs`: `VaRinfrescata` è **una
   riga con un solo operatore di confronto** (`adessoUtc + Margine >= scadenzaUtc`, `:21-22`), pubblica
   e statica, con **sette** test; `SiPuoRitentare` è un `||` con un confronto, con tre. È lo stesso
   schema, e il progetto l'ha già deciso nel senso opposto al rilievo — `conformity` B ha esaminato
   proprio questo punto e non l'ha marcato. Il valore di quei test non è provare `||`: è impedire che
   `dopoAccesso ||` sparisca in una futura semplificazione, e quel termine è il correttivo descritto in
   testa a questo resoconto — la parte meno ovvia dell'unità, e l'unica che nessun gate rileverebbe se
   sparisse.
10. **Togliere la registrazione in DI (`backend-expert` B). FONDATO, ma FUORI SCOPE** — v. sotto.

---

## FUORI SCOPE

### 1. Il servizio starebbe meglio costruito in linea, ma il perimetro non è mio

**Due agenti, indipendentemente, dicono la stessa cosa**: `tech-advisor` prima che scrivessi i brief, e
`backend-expert` B dopo averli letti. `AllineatoreProfilo` ha **un solo consumatore**, è un tipo
concreto senza interfaccia, e i suoi due fratelli identici — `BrowserSessionHandler` e `PkceStore`,
stesso `IJSInProcessRuntime`, stesso cast — sono costruiti a mano dentro `SupabaseService`
(`:53-55`) e **non stanno in `Program.cs`**. `RottaRichiesta` ci sta perché una pagina la `@inject`a;
questo servizio no.

**Non l'ho fatto**, perché il mandato prescrive testualmente «`Program.cs` — **la sola riga di
registrazione** del servizio nuovo», e il perimetro è una decisione del capo, non un dettaglio di
forma che un'unità possa correggere da sé. `backend-expert` chiude allo stesso modo: «se il brief
prescriveva la registrazione in DI, è una decisione del brief e non dell'unità».

**Se vuoi chiuderla** costa tre righe: via `Program.cs:23`, il costruttore torna a tre parametri, e
`_profilo = new AllineatoreProfilo((IJSInProcessRuntime)js);` accanto a `_pkce`.

### 2. L'impronta sopravvive al logout, ed è l'unico artefatto locale che lo fa

Segnalato da `threat-hunter` B in prosa, che non l'ha elevato a rilievo perché non è sfruttabile — ma
il §5 mi impone di aprire di persona ciò che tocca i dati, e l'ho fatto. **Il fatto è verificabile in
una riga**: al logout vengono cancellati `eton.session` (`SupabaseService.cs:337`), `eton.pkce`
(`:347`) e `eton.spazio` (`AuthStateService.cs:69` → `SpaceStateService.Dimentica()`).
**`eton.profilo` no.** Su un dispositivo condiviso resta leggibile l'identificatore, il nome e la foto
di chi è uscito.

Non l'ho corretto perché richiede un **secondo** call-site in `SupabaseService`, dentro `SignOutAsync`,
e il mandato me ne concede **uno solo**. Non c'è danno funzionale — al prossimo accesso l'impronta non
combacia e si riscrive — ma il difetto è che il prossimo che aggiunga un dato più sensibile
all'impronta erediterebbe la dimenticanza. Costa un metodo `Dimentica()` e una riga.

### 3. Il testo d'aiuto della pagina di profilo può ora promettere di più

`Pages/Profile.razor:14-15` dice «Nome ed email arrivano dall'account Google…: Eton li legge e basta,
non ha un posto dove cambiarli». **Non diventa falso** — resta vero che Eton non offre un posto dove
cambiarli. Diventa incompleto in senso favorevole: adesso può dire che una correzione fatta su Google
arriva anche agli altri. La pagina non è nel mio perimetro e non l'ho toccata.

---

## GATE

```
dotnet build Eton.sln -warnaserror --no-incremental  →  Avvisi: 0, Errori: 0
dotnet test Eton.sln                                 →  Superati: 310, Non superati: 0, Totale: 310
```

Il mandato chiedeva «almeno quanti ne ha lasciati l'unità 05», cioè 290: **misurata la baseline sul
worktree appena aperto prima di toccare qualunque cosa** (290/290, 0 avvisi), e chiusa a **310**.
I venti nuovi sono le due suite di decisioni pure più i due test difensivi dei punti 3 e 7.

Compilato **io**, a implementer fermi: `obj/` non ha lock fra processi. Ogni brief portava il divieto
esplicito di compilare, e nessun implementer lo ha violato. **Server di sviluppo non avviato, browser
non aperto**, come il mandato vieta.

### ⚠️ Cosa NON provano, detto con le parole che il mandato chiede

**Nessuno dei due gate prova il lavoro di questa unità.** Il difetto si manifesta **solo con due
account diversi nello stesso spazio**, uno dei quali abbia cambiato nome o foto su Google: niente di
tutto ciò è riproducibile da qui.

Ho potuto verificare **leggendo**: che il difetto esista (un solo accesso a `profiles`, in lettura);
che le due cascate coincidano (migrazione contro codice); che il call-site non possa auto-bloccarsi;
che la scrittura mandi solo le due colonne concesse; che la policy valuti `auth.uid()` lato server;
che con l'anon key la scrittura fallisca chiusa. Ho potuto verificare **misurando**: il comportamento
di Newtonsoft sui metadati null, e la collisione del separatore.

**Non ho potuto verificare**, e resta al collaudo:

1. **Che un altro membro veda davvero il valore nuovo.** È il risultato osservabile che il mandato
   chiede, e richiede due account.
2. **Che la prima scrittura passi la RLS.** Il codice non salva l'impronta se `Models.Count == 0`,
   quindi un rifiuto si ritenta all'avvio dopo — ma un rifiuto *sistematico* sarebbe invisibile senza
   guardare la console, dove compare `[Profilo]`.
3. **Che il caso normale costi davvero zero chiamate.** Si vede solo nel pannello di rete: al secondo
   avvio consecutivo senza cambiamenti **non deve** comparire nessun `PATCH` su `/rest/v1/profiles`.
   È la prova dell'intero vincolo, ed è di una riga nel tuo ciclo chiuso.
4. **Se rimuovere la foto su Google si propaghi** — dipende dal merge dei metadati lato server, v. il
   secondo limite dichiarato in testa.

---

## SCOSTAMENTI

### 1. Due tipi nuovi di produzione invece di uno

Il mandato prevedeva «un servizio piccolo, una registrazione, un call-site». Ne ho scritti **due**:
`AllineatoreProfilo` (il servizio) e `IdentitaGoogle` (la regola pura). Il motivo è che la regola
«dai metadati Google ricava nome e foto» serve a **due** chiamanti — la pagina di profilo attraverso
`AuthStateService`, e il servizio nuovo — e tenerla in un posto solo era l'unica alternativa a
duplicarla, che è il difetto che un revisore avrebbe segnalato per primo.

La **sede** l'ha decisa il progetto, non io: `conformity` A ha verificato che tutte le decisioni pure
stanno in classi statiche separate (`SessionFreshness`, `OAuthCallback`, `PkceChallenge`,
`SchemaCampi`) e che **l'alternativa — metodi statici dentro `AuthStateService` — non ha precedenti**.
`tech-advisor` aveva indicato la stessa sede prima dei brief.

### 2. Gli implementer A e B lanciati in due messaggi separati

Il §2 chiede che partano **nello stesso messaggio**. È lo stesso scostamento dichiarato dall'unità 05,
e non l'ho evitato benché l'avessi letto. Gli agenti sono asincroni e B è partito mentre A lavorava —
il parallelismo c'è stato — ma la regola dice «stesso messaggio». Nessuna conseguenza osservabile:
nessun file condiviso, e il contratto fra i due era una firma fissata testualmente in entrambi i brief
(B ha trovato `IdentitaGoogle` già sul disco e l'ha solo chiamata).

### 3. I revisori dell'unità A in due messaggi

Il §3 chiede lo stesso. Ho lanciato `bug-hunter` da solo e poi gli altri tre insieme. Quelli
dell'unità B sono partiti tutti e quattro nello stesso messaggio, come previsto.

### 4. La firma d'istanza per l'immagine non è stata aggiunta

Il mandato la autorizzava esplicitamente: «puoi aggiungerne una accanto per l'indirizzo
dell'immagine». **Non l'ho fatto**: nessuno l'avrebbe chiamata — il servizio nuovo usa la versione
pura — e sarebbe stata codice morto, che `backend-expert` avrebbe segnalato a ragione. La capacità
c'è comunque, in `IdentitaGoogle.FotoDa`; manca solo il metodo d'istanza che la esponga, e si aggiunge
in tre righe il giorno che una pagina ne avrà bisogno.

### 5. Un residuo di un agente trovato nel worktree, e rimosso da sé

`git add -N .` ha mostrato per un momento una cartella `tmp-inspect-gotrue/` con due DLL: era
l'harness con cui `bug-hunter` ha misurato il comportamento di Newtonsoft (punto 3 dell'adjudica),
costruito **fuori** da `obj/` del progetto e ripulito da lui al termine. Lo dichiaro perché per qualche
minuto è stato in indice, e perché è la ragione per cui quel rilievo porta una misura invece di
un'inferenza. Il diff finale non ne contiene traccia: `git status --porcelain` elenca i sette file e
nient'altro.

---

## PER TE, PRIMA DEL COLLAUDO

Quattro cose in ordine di importanza, tutte con l'osservabile già scritto:

1. **Due account nello stesso spazio, uno cambia nome su Google, riapre Eton.** L'altro, ricaricando,
   deve vedere il nome nuovo nell'elenco dei membri di `SpaceDetail`. È il risultato che il mandato
   chiede, e nessuno l'ha ancora visto.
2. **Il costo a regime.** Riapri due volte di fila senza cambiare nulla: al secondo avvio **non deve**
   partire nessun `PATCH` su `/rest/v1/profiles`. Se parte a ogni avvio, l'impronta non combacia mai —
   il sospetto è un URL di avatar con parametri volatili — e allora lo store dell'impronta va tolto e
   si scrive incondizionatamente, perché a quel punto non sta comprando niente.
3. **La console.** Un `[Profilo]` a ogni avvio significa che la scrittura viene rifiutata: l'impronta
   non si salva e si ritenta per sempre, in silenzio.
4. **La foto rimossa.** Togli la foto su Google e rifai l'accesso: se gli altri continuano a vederla,
   il server fa *merge* dei metadati, e il secondo limite dichiarato in testa è confermato. È un dato
   che vale la pena scrivere da qualche parte, perché decide se questa strada copre la foto come copre
   il nome.
