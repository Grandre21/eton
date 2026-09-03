UNITÀ: 15 — ESITO: FATTO

TOCCATI:
- `Services/SupabaseService.cs` → +16/−1

Un solo file, come il perimetro prescriveva. `Pages/Benvenuto.razor` l'ho **letto e non toccato**.

`handoff/PIANO.md` risulta modificato nel working tree ma **non è mio**: non l'ho mai aperto in
scrittura e il mandato non lo nominava. È la chiusura dell'unità 14, scritta dal capo prima di
aprirmi — la stessa cosa che l'unità 14 aveva registrato per la chiusura della 13.

Tutti i `file:line` di questo resoconto li ho riaperti io sul disco dopo l'ultima modifica e dopo
i gate. Le righe che cito dai revisori le ho riverificate una per una.

---

## LE DUE PREMESSE DEL MANDATO: VERIFICATE ENTRAMBE, UNA CON UNA CORREZIONE

Il mandato chiedeva di verificarle prima di procedere, e di tornare `BLOCKED` se fosse comparso un
secondo consumatore.

**1. Un solo lettore — confermato.**

```
$ grep -rn "ErroreAccesso" --include=*.cs --include=*.razor .
./Pages/Benvenuto.razor:211:        errore = SupabaseService.ErroreAccesso;
./Services/SupabaseService.cs:32:    public string? ErroreAccesso { get; private set; }
./Services/SupabaseService.cs:110:                    ErroreAccesso = esito.Errore;
./Services/SupabaseService.cs:158:        ErroreAccesso = null;
./Services/SupabaseService.cs:182:            ErroreAccesso = "Accesso non completato: riprova dall'inizio.";
./Services/SupabaseService.cs:190:            ErroreAccesso = "Accesso non completato: sessione senza utente.";
./Services/SupabaseService.cs:194:            ErroreAccesso = $"Accesso non riuscito: {ex.Message}";
```

Una sola lettura in tutto il progetto, `Benvenuto.razor:211`. Le altre sei righe sono la
dichiarazione e cinque scritture, tutte dentro il file del mio perimetro. La premessa regge e il
divieto «niente traduzioni nei servizi» non si applica: non c'è nessuna ambiguità su chi legge.

**2. «C'è una sola schermata che lo mostra» — vero, ma NON perché l'utente ci navighi.** Ed è la
correzione che ha cambiato la frase.

Google non rimanda su `/benvenuto`: rimanda su `_navigation.BaseUri`, cioè la **radice**, che è
`Pages/Home.razor` — rotta **privata**. Il percorso reale è:

| passo | dove | cosa succede |
|---|---|---|
| 1 | `SupabaseService.cs:169` | il `redirect_to` è `_navigation.BaseUri`: si torna su `/`, non sulla vetrina |
| 2 | `AuthRedirect.razor:42` | `GetClientAsync()` esegue il bootstrap → il `catch` scatta e imposta `ErroreAccesso` |
| 3 | `AuthRedirect.razor:68` | non c'è sessione → `NavigateTo("benvenuto", forceLoad: false, replace: true)` |
| 4 | `Program.cs:12` | `AddSingleton<SupabaseService>()`: in WASM è il ciclo di vita dell'app, e la navigazione al passo 3 è client-side → **lo stato sopravvive** |
| 5 | `Benvenuto.razor:210-211` | `GetClientAsync()` esce subito su `_initialized` senza riazzerare nulla, e `:211` legge il messaggio |

**Conseguenza sulla frase**: chi legge **è già sulla pagina d'ingresso**, col pulsante «Entra con
Google» due righe di markup sotto il riquadro (`Benvenuto.razor:28` e `:31`). Il rimedio «già
pronto» lasciato dall'unità 14 in `FUORI SCOPE` 1 diceva *«Torna alla pagina di ingresso e prova di
nuovo a entrare con Google»* — e quel «torna» è esattamente il difetto che l'unità 14 stessa aveva
scoperto sul proprio giro: un gesto che non produce nulla, perché il lettore ci è già.

Il passo 4 è la maglia che regge tutto: se `SupabaseService` fosse stato `Transient`, il messaggio
non sarebbe mai arrivato a schermo e il diff sarebbe stato inutile. L'ho verificato io su
`Program.cs`; `bug-hunter` ci è arrivato per conto suo, e ha aggiunto la maglia che mi mancava —
`Benvenuto.razor:2` usa `@layout VetrinaLayout`, non `MainLayout`, quindi la vetrina **non** è
avvolta da `AuthRedirect` e si monta comunque, indipendentemente da `risolto`.

---

## CONTRATTI

### La frase, verbatim

`Services/SupabaseService.cs:209`:

```csharp
ErroreAccesso = "Non è stato possibile completare l'accesso: può essere la connessione, oppure l'autorizzazione appena rilasciata da Google, che vale una sola volta e per pochi minuti. Prova di nuovo a entrare con Google.";
```

Diagnosi a `:208`:

```csharp
Console.Error.WriteLine($"[Auth] Scambio del codice PKCE non riuscito: {ex.Message}");
```

Nessuna interpolazione sulla stringa pubblica: è una costante letterale, non una `$"…"`. È la
proprietà che rende il rilievo 3 non riproducibile su questa riga per costruzione, non per
attenzione.

### Perché non discende da nessuna delle 25 già scritte

Le venticinque sorelle parlano tutte a un utente **autenticato dentro l'applicazione**. Qui il
lettore è **anonimo**, e tre elementi della forma canonica cadono.

**1. Il fatto.** Le altre nominano l'oggetto che non si è potuto leggere o scrivere («i tuoi
spazi», «le recensioni», «la spesa»). Qui non c'è nessun oggetto: ciò che non è riuscito è
l'ingresso stesso. «Non è stato possibile completare l'accesso» — e *completare*, non *avviare*,
perché la frase gemella di `Benvenuto.razor:241` si è già presa *avviare* per il guasto opposto,
quello che accade **prima** di partire verso Google. Le due stanno sullo stesso schermo e non
devono confondersi.

**2. La causa. Aperto il metodo, e una delle tre che il mandato elencava non può arrivarci.**

| causa candidata | può finire in questo `catch`? | prova |
|---|---|---|
| la connessione | **sì** | `:188` è `await _auth.ExchangeCodeForSession(…)`, una POST a `/auth/v1/token`. È l'unica chiamata di rete del metodo |
| il rifiuto dello scambio da parte dell'Auth server | **sì** | stessa riga: codice già speso, scaduto, verificatore non corrispondente, 5xx |
| **il permesso negato sulla schermata di Google** | **NO — mai** | torna come `?error=`/`?error_description=`, e `OAuthCallback.cs:24-28` lo intercetta **prima** del `?code=`; finisce a `SupabaseService.cs:110`, in un ramo che non chiama nemmeno `ScambiaCodiceAsync` |

Il mandato citava il permesso negato fra le cause plausibili. **Non lo è**, e metterlo nella frase
avrebbe suggerito all'utente di aver fatto una cosa che non ha fatto. È il motivo per cui il
mandato diceva «apri il metodo invece di elencare cause plausibili».

Il rifiuto dello scambio, per chi legge, si riassume in una cosa sola e comprensibile:
**l'autorizzazione di Google è monouso e dura pochi minuti**. Nessuna delle venticinque nomina un
oggetto simile — le loro cause seconde sono sempre «la sessione scaduta» o «il tuo accesso allo
spazio che è cambiato», e nessuna delle due esiste per chi non è mai entrato.

Su «la connessione» c'è una simmetria che vale la pena registrare, perché è l'inverso del caso
vicino: l'unità 14 dovette **toglierla** da `Benvenuto.razor:241`, perché `AvviaAccessoGoogleAsync`
non fa nessuna chiamata di rete. Qui la rete c'è davvero, quindi la causa torna lecita. Due frasi
sullo stesso schermo, una che nomina la connessione e una che non la nomina, e in entrambi i casi
per una ragione letta nel codice.

**3. L'azione.** «Prova di nuovo a entrare con Google» riusa **verbatim** l'incipit dell'azione di
`Benvenuto.razor:241`, ed è deliberato: è lo stesso pulsante, sullo stesso schermo, per due guasti
diversi. Nomina l'etichetta letterale di `Benvenuto.razor:32` e `:178`
(`@(occupato ? "Attendere…" : "Entra con Google")`).

Tre cose che le altre frasi dicono e che **qui non si possono dire**:
- niente «esci e rientra»: non c'è nessuna sessione da rifare;
- niente «ricarica la pagina»: il messaggio vive nella memoria del singleton, e un ricaricamento
  vero lo cancellerebbe insieme al resto — l'azione suggerita distruggerebbe il proprio messaggio;
- niente «torna alla pagina d'ingresso»: ci si è già, per il passo 3 della tabella sopra.

### Il marcatore e il lessico della diagnosi

`[Auth]` non è nuovo: il file lo usa già per tutte le proprie diagnosi. Anche «codice PKCE» non è
inventato — è testuale dal commento XML a `SupabaseService.cs:81` («*scambio del codice PKCE se
siamo appena tornati da Google*»). È l'unico punto in cui mi sono scostato dalla riga suggerita
dall'unità 14, che diceva «Scambio del codice non riuscito»: il termine del file è più preciso, e
`conformity` lo ha confermato indipendentemente.

### I due numeri richiesti

| | prima | dopo |
|---|---|---|
| `catch` nel file | 7 | 7 |
| righe di diagnosi (`Console.Error.WriteLine`) | 7 | **8** |
| `catch` **senza** diagnosi | **1** (quello di `ScambiaCodiceAsync`) | **0** |

Appaiati uno a uno: `192→208`, `265→269`, `272→275`, `303→305`, `312→314`, `321→323`, `330→332`.
L'ottava riga (`:337`, «Logout NON riuscito») non sta in un `catch`: è la diagnosi del logout che
fallisce senza lanciare. Il `catch` di `ScambiaCodiceAsync` era l'unico muto del file, e non lo è
più.

---

## ADJUDICA

    istruttoria: 0 rilievi su 0 file → checker no

La soglia si calcola sui soli `bug-hunter` e `conformity`, che hanno riportato `RILIEVI: 0`
entrambi. Sotto soglia su tutti e due i metri, quindi nessun `checker`.

**`bug-hunter` — 0 rilievi.** Ha verificato per conto suo le cinque cose che gli ho chiesto, e la
maglia di `@layout VetrinaLayout` è sua: non l'avevo controllata. Nessun verdetto da adjudicare.

**`conformity` — 0 rilievi.** Ha falsificato una per una le tre divergenze che rivendicavo,
citando `AuthRedirect.razor:68`, `Benvenuto.razor:31`/`:177` e `OAuthCallback.cs:24-31`, e ha
confrontato la densità del commento con `Benvenuto.razor:227-239` (13 righe per le stesse 2 righe
di codice). Nessun verdetto da adjudicare.

**`threat-hunter` — 1 rilievo, e non è sulla riga che ho toccato.**

*Sul mio diff*: dichiara chiuso il rilievo che aveva aperto lui nel giro 14. `ex.Message` resta
solo in `Console.Error`, e la console client-side è letta dallo stesso utente che ha generato
l'errore. Ha anche escluso che la frase riveli infrastruttura (non nomina Supabase né Gotrue: «monouso
e pochi minuti» è semantica pubblica di OAuth 2.0) e che i tre esiti distinguibili del metodo siano
un oracolo — PKCE rende il codice non spendibile fuori dal browser che ha generato la sfida, quindi
per un estraneo il ramo è sempre `:182` a prescindere.

*Il rilievo* — **`Services/SupabaseService.cs:110`. FONDATO, FUORI DAL MIO PERIMETRO, con
un'etichetta da correggere.**

Il protocollo impone di aprire io il codice su ogni rilievo di sicurezza, qualunque sia il verdetto.
L'ho fatto, e non di seconda mano: questo punto l'avevo isolato **prima** di scrivere il diff,
leggendo `OAuthCallback.cs` per capire quali cause potessero raggiungere il mio `catch`. Le tre
righe, riaperte sul disco:

```csharp
// Services/OAuthCallback.cs:24 — il testo entra dalla query, non validato
if (parametri.TryGetValue("error_description", out var descrizione) && !string.IsNullOrWhiteSpace(descrizione))
    return new OAuthCallbackEsito(null, descrizione);

// Services/SupabaseService.cs:110 — e viene assegnato tale e quale
ErroreAccesso = esito.Errore;

// Pages/Benvenuto.razor:28 — e reso, sopra il pulsante d'accesso
<div class="errore" role="alert">@errore</div>
```

**Verdetto: fondato.** Chiunque può costruire `https://<dominio>/?error_description=<testo a
piacere>` e mandarlo a qualcuno: il testo compare dentro il riquadro d'errore del sito legittimo,
in un `role="alert"`, sopra «Entra con Google».

**Una correzione all'etichetta**: il report lo marca `TIPO: xss`, e non lo è. Razor codifica
`@errore`, quindi non c'è esecuzione di script né iniezione di markup — `threat-hunter` lo
riconosce lui stesso nel corpo del rilievo. È **content spoofing**: testo scelto da un estraneo
dentro il chrome del dominio di fiducia, più credibile di un'email di phishing perché arriva
sull'URL vero. La severità «media» regge, l'etichetta no.

**Non l'ho toccato**, e non per timidezza: il mio obiettivo è la riga 194, il mandato vieta di
cambiare il flusso di autenticazione, e la correzione giusta — mappare `error`/`error_description`
su un insieme chiuso di messaggi interni — è una modifica di logica, non di stringa. Va in
`FUORI SCOPE` 1.

**Campione sugli infondati: nessuno da riverificare, perché non ce n'è nessuno.** In tutto il giro
non è stato scartato alcun rilievo — due revisori su tre ne hanno riportati zero, e l'unico
esistente è fondato. La dichiarazione che il protocollo chiede vale quindi come dichiarazione di
assenza, non come omissione. In compenso i quattro `file:line` portanti li ho riaperti io:
`Program.cs:12`, `AuthStateService.cs:23-25`, `OAuthCallback.cs:24-28`, `Benvenuto.razor:26-33`.

---

## FUORI SCOPE

### 1. `Services/SupabaseService.cs:110` — content spoofing sul riquadro d'errore

Fondato, trovato due volte in modo indipendente (dalla mia lettura di `OAuthCallback.cs` e da
`threat-hunter`). **Il file è del mio perimetro, la riga no**: il mandato assegnava la 194 e vietava
esplicitamente di toccare la logica di autenticazione, e questa è logica.

- **Percorso**: `OAuthCallback.cs:24-28` (query non validata) → `SupabaseService.cs:110` →
  `Benvenuto.razor:28`.
- **Perché conta più della riga che ho appena corretto**: la 194 esponeva un messaggio *nostro* a
  un utente sfortunato; la 110 rende un messaggio *di un estraneo* a un utente preso di mira. La
  prima è un'indiscrezione, la seconda è una superficie.
- **Non è XSS**: Razor codifica. È testo, ed è sufficiente — «Il tuo accesso è stato bloccato,
  contatta …» dentro il riquadro rosso del sito vero.
- **Rimedio suggerito**, che richiede una riga di mandato: non riflettere il testo libero del
  provider. Mappare su un insieme chiuso — `access_denied` → una frase fissa italiana, tutto il
  resto → una frase generica — e scartare `error_description`. Il posto naturale è
  `OAuthCallback.Analizza`, che è già la classe pura del flusso e **ha già la sua suite**:
  `Eton.Tests/OAuthCallbackTests.cs`. Chi la corregge ha dove scrivere la prova senza inventarsi
  un'impalcatura.

### 2. `SupabaseService.cs:182` e `:190` — le due frasi che dicono solo il fatto

```csharp
:182  ErroreAccesso = "Accesso non completato: riprova dall'inizio.";
:190  ErroreAccesso = "Accesso non completato: sessione senza utente.";
```

Già italiane e senza fughe, quindi **non sono rilievo 3** — appartengono al rilievo sulla forma
«fatto e nient'altro» che la regola 2 dell'unità 05 esclude. L'unità 14 le aveva già segnalate.
Non le ho toccate perché il mandato dice «una riga più la sua diagnostica» e «una stringa e forse
una riga di console»: allargarmi a due frasi in più sarebbe stato decidere al posto del capo.

Sono nello stesso metodo che ho appena aperto, quindi costerebbero pochissimo a chi le facesse
adesso. La `:190` in particolare è la meno difendibile: «sessione senza utente» è gergo interno.
Serve una riga di mandato, com'è stato per `CollectionEdit:748`.

### 3. Due JS interop fuori dalla rete di sicurezza — difetto di flusso, non di messaggio

`_pkce.Leggi()` sta **fuori** dal `try` (`:179`) e `_pkce.Cancella()` sta nel `finally` (`:214`).
Sono entrambi `localStorage` via JS interop e possono lanciare `JSException`. Nessuno dei due è
coperto: né `ScambiaCodiceAsync`, né `GetClientAsync` (che ha `try/finally`, non `try/catch`), né i
chiamanti `Benvenuto.razor:210` e `AuthRedirect.razor:42`.

Se scoppia, si finisce sulla barra d'errore di default di Blazor. `threat-hunter` ha verificato
`wwwroot/index.html:66-70`: è **testo fisso**, quindi non è un'esposizione — ma è un blocco
funzionale, e colpisce proprio chi ha il `localStorage` chiuso, cioè lo stesso profilo di utente
per cui l'unità 14 ha scritto la frase di `:241`.

**Il mandato vieta di toccare il flusso** («Se ti accorgi che il flusso ha un difetto, scrivilo in
`FUORI SCOPE` e non toccarlo»), e questa è una modifica di flusso. Riportato, non risolto.

---

## GATE

- `dotnet build -warnaserror` → **Avvisi: 0, Errori: 0**. Compilazione incrementale: il diff tocca
  un solo `.cs` del progetto principale, nessun markup Razor.
- `dotnet test --no-build` → **Superato! Non superati: 0. Superati: 273. Ignorati: 0. Totale: 273.**
  Esattamente i 273 di partenza. Nessun test copre le stringhe d'errore, e non ne ho aggiunti.

Compilato **io**, una volta sola, a fine giro, come il mandato prescrive.

**Server di sviluppo non avviato, browser non usato**, come il mandato vieta. Nessun processo
lasciato vivo, nessuna porta occupata. **Nessun commit**: il file è nel working tree.

---

## SCOSTAMENTI

1. **Non ho usato la frase «già pronta» dell'unità 14**, che il suo `FUORI SCOPE` 1 lasciava
   scritta e che il mandato indicava come modello più vicino. Il motivo è nella tabella delle due
   premesse: diceva «*Torna alla pagina di ingresso*», e chi legge ci è già stato portato da
   `AuthRedirect.razor:68`. Avrei riprodotto esattamente il difetto che l'unità 14 aveva scoperto
   sul proprio giro — un gesto suggerito che non produce nulla. Ho tenuto la sua struttura e la sua
   seconda causa; ho cambiato l'azione e ho reso più preciso il lessico della diagnosi.

2. **Ho riscritto il commento una volta, per rientrare nel budget.** La prima stesura portava il
   diff a `+21/−1`, cioè oltre il tetto di venti righe. Ho condensato la prosa da 18 a 13 righe
   senza togliere nessuno dei tre fatti non ovvi. Finale: `+16/−1`.

3. **Ho eseguito quattro `grep` invece dei due prescritti.** È un allargamento, non una riduzione,
   e il motivo è nella sezione qui sotto: i due del mandato non bastano a sostenere la conclusione.

---

## IL RILIEVO 3 È CHIUSO?

**Sì.** Ed è la terza volta che questa domanda si pone, quindi la risposta è per comandi e output,
non per dichiarazione.

### I due `grep` del mandato, riprodotti alla lettera

```
$ grep -rn 'errore = \$"' Pages Shared Services
Pages/Home.razor:379:            errore = $"{premessa}, ma non è stato possibile leggerne i dettagli: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Ricarica la pagina per riprovare.";
Pages/SpaceDetail.razor:277:                errore = $"Non è stato possibile togliere {m.Nome} dallo spazio: può rimuovere un membro solo chi possiede lo spazio, e da quando hai aperto la pagina il tuo accesso può essere cambiato. Ricarica la pagina per vedere com'è adesso.";
Pages/SpaceDetail.razor:299:            errore = $"Non è stato possibile togliere {m.Nome} dallo spazio: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere chi ne fa parte adesso.";
Shared/PaginaRegistro.cs:136:        errore = $"Lo spazio si è caricato, ma non è stato possibile leggerne {NomePlurale}: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova.";
Shared/PaginaRegistro.cs:209:            errore = $"Non è stato possibile aggiornare {NomePlurale} dopo il cambio di spazio: può essere la connessione, oppure il tuo accesso al nuovo spazio che è cambiato. Ricarica la pagina.";
```

**Cinque righe, e nessuna è il rilievo 3.** Questo `grep` cerca l'interpolazione, non l'eccezione:
tutte e cinque interpolano un **sostantivo di dominio** — `{premessa}`, `{m.Nome}`,
`{NomePlurale}` — dentro frasi italiane già scritte dalle unità precedenti. Il pattern è rumoroso
per costruzione, ed è il motivo per cui il mandato lo definisce il peggiore dei due.

```
$ grep -rEn '(errore|avviso|ErroreAccesso|Messaggio)\s*=\s*[^;]*\bex\b' --include=*.razor --include=*.cs
(nessun output — uscita 1)
```

**Zero righe.** È il `grep` sui sink, quello che l'unità 14 indicava come il buono, e non trova
più nulla.

### I due `grep` che ho aggiunto, perché i primi due non bastano a chiudere

Il `grep` sui sink vale **solo se** ogni `catch` del progetto chiama `ex` la propria eccezione: il
pattern contiene `\bex\b`. Nessuno l'aveva mai verificato, ed è esattamente la forma di punto cieco
che ha fatto sbagliare le prime due chiusure — la prima due volte su una mappa *file → unità*, la
terza sul nome del campo.

```
$ grep -rEn 'catch \(' --include=*.razor --include=*.cs Pages Shared Services Layout Models \
    | grep -oE 'catch \([A-Za-z.]+( [a-z]+)?' | sort | uniq -c | sort -rn
     70 catch (Exception ex
      1 catch (Supabase.Gotrue.Exceptions.GotrueException ex
```

**71 `catch`, 71 chiamano `ex`.** Il pattern non ha punti ciechi sul nome della variabile.

E il controllo che chiude ogni via d'uscita — ogni uso di `ex` che non sia una riga di console né
la firma di un `catch`:

```
$ grep -rn '\bex\b' --include=*.razor --include=*.cs Pages Shared Services Layout Models App.razor Program.cs \
    | grep -v 'Console.Error.WriteLine' | grep -v 'catch (' | grep -vE ':[0-9]+:\s*//'
Pages/Collections.razor:115:            SegnalaNonLetti(ex);
Pages/Notes.razor:121:            SegnalaNonLetti(ex);
Pages/Spese.razor:403:            SegnalaNonLetti(ex);
Shared/PaginaRegistro.cs:133:    protected void SegnalaNonLetti(Exception ex)
Services/SupabaseService.cs:266:            ex.Reason == Supabase.Gotrue.Exceptions.FailureHint.Reason.ExpiredRefreshToken
Services/SupabaseService.cs:267:            || ex.Reason == Supabase.Gotrue.Exceptions.FailureHint.Reason.InvalidRefreshToken)
```

**Sei righe residue, tutte innocue, tutte riaperte:**

- le tre `SegnalaNonLetti(ex)` passano l'eccezione a `PaginaRegistro.cs:133`, che la consuma a
  `:135` in `Console.Error` e a `:136` assegna una frase fissa in cui l'unica interpolazione è
  `{NomePlurale}` — visibile nell'output del primo `grep` qui sopra. L'eccezione muore in console;
- le due `ex.Reason` di `SupabaseService.cs:266-267` sono un confronto dentro un filtro `when`, non
  un rendering.

**Conclusione, per costruzione e non per fiducia**, contata e non stimata:

```
$ ... | wc -l                              # .Message nel sorgente, esclusi i commenti
69
$ ... | grep -c 'Console.Error.WriteLine'  # di cui dentro una riga di console
69
```

Nel codice sorgente ci sono **69 occorrenze di `.Message`, e tutte e 69 stanno dentro un
`Console.Error.WriteLine`**. Nessuna eccezione raggiunge
più un campo reso all'utente, per nessuna via, sotto nessun nome di variabile. Il rilievo 3 è
chiuso, e questa volta la chiusura non dipende da un censimento di file.

---

## DA PROVARE NEL BROWSER

**Testo esatto atteso**, dentro `<div class="errore" role="alert">` di `Benvenuto.razor:28`, sopra
il pulsante «Entra con Google»:

> Non è stato possibile completare l'accesso: può essere la connessione, oppure l'autorizzazione
> appena rilasciata da Google, che vale una sola volta e per pochi minuti. Prova di nuovo a entrare
> con Google.

E in console, contemporaneamente:
`[Auth] Scambio del codice PKCE non riuscito: <dettaglio dell'eccezione>`

**È provocabile a mano, senza toccare il codice**, ed è la notizia buona: far fallire un accesso
OAuth a comando sembra difficile, ma qui basta sfruttare il fatto che il verificatore PKCE
sopravvive nel `localStorage` finché non lo si spende.

1. Apri l'app da anonimo: il rimbalzo di `AuthRedirect.razor:68` ti porta su `/benvenuto`.
2. Premi **«Entra con Google»**. Questo salva il verificatore in `localStorage` sotto la chiave
   `eton.pkce` (`PkceStore.cs:20-21`) *prima* di uscire dall'applicazione.
3. Sulla schermata di Google **non completare l'accesso**. Punta invece il browser su
   `http://localhost:<PORTA>/?code=codice-inventato`.
4. Il verificatore c'è ancora, quindi la guardia di `:180` non scatta e si arriva davvero a
   `ExchangeCodeForSession`: l'Auth server rifiuta il codice inventato, l'eccezione cade nel
   `catch`, e il messaggio compare **su `/benvenuto`** — non su `/`, dove sei atterrato.

**Variante per il ramo «connessione»**: identica, ma prima del passo 3 metti DevTools → Network →
Offline. L'eccezione è di rete e cade nello stesso `catch`, con lo stesso messaggio.

**Avvertenza che rende falsa una seconda prova consecutiva**: il `finally` a `:214` cancella il
verificatore a ogni tentativo, riuscito o no. Ripetere il passo 3 senza rifare il passo 2 fa
scattare la guardia di `:180` e mostra un messaggio **diverso** — «Accesso non completato: riprova
dall'inizio.», che è la riga `:182` e non la mia. Ogni prova va ricominciata dal pulsante.

**Cosa NON si riesce a provocare a mano**: il ramo di `:190` (`session?.User is null`, cioè uno
scambio riuscito che restituisce una sessione senza utente) richiede un server che risponda `200`
con un corpo anomalo. Lo dichiaro come limite.

**Da guardare, oltre al testo**: che il pulsante «Entra con Google» sia **premibile** — deve
mostrare l'etichetta e non «Attendere…», perché `occupato` è al suo default `false` su quella
istanza della pagina — e che premendolo il flusso riparta davvero da capo.

Il mandato vieta di avviare il server e di provare nel browser, quindi **non l'ho fatto**: quanto
sopra è la ricetta, non un esito.
