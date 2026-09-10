UNITÀ: 16 — ESITO: FATTO

TOCCATI:
- `Services/OAuthCallback.cs` → +48/−8
- `Services/SupabaseService.cs` → +16/−3
- `Eton.Tests/OAuthCallbackTests.cs` → +103/−10

I tre file del perimetro, nessun altro. `Pages/Benvenuto.razor` l'ho **letto e non toccato**, e non serviva: cambia ciò che le arriva, non come lo mostra.

`handoff/PIANO.md` risulta modificato nel working tree ma **non è mio**: non l'ho mai aperto in scrittura e il mandato non lo nominava. È la stessa cosa che l'unità 15 aveva registrato per la chiusura della 14.

Tutti i `file:line` di questo resoconto li ho riaperti io sul disco **dopo** l'ultima modifica e **dopo** i gate. Le due prove della sezione finale le ho eseguite davvero, con l'output riportato verbatim.

---

## CONTRATTI

### L'enum, verbatim — `Services/OAuthCallback.cs:3-14`

```csharp
/// <summary>Classificazione chiusa del rifiuto letto dal ritorno OAuth: mai testo libero del provider.</summary>
public enum OAuthRifiuto
{
    /// <summary>Nessun rifiuto: non c'è errore, oppure l'URL non è nemmeno un ritorno OAuth.</summary>
    Nessuno,
    /// <summary>Il permesso non è stato concesso sulla schermata di Google.</summary>
    Annullato,
    /// <summary>L'autorizzazione monouso non era più spendibile al ritorno.</summary>
    Scaduto,
    /// <summary>Rifiutato per un motivo che non sappiamo tradurre: ci cade ogni caso sconosciuto.</summary>
    Generico
}
```

E il record, `:20`:

```csharp
public sealed record OAuthCallbackEsito(string? Codice, OAuthRifiuto Errore, string? Diagnostica);
```

Il nome del membro **resta `Errore`**, come il mandato prescrive («`Errore` diventa un `enum`»): cambia il tipo, non il nome, e questo rende il call-site di `SupabaseService` un diff minimo e leggibile.

### La tabella di classificazione, verbatim — `Services/OAuthCallback.cs:55-62`

```csharp
            var rifiuto = (errore, codiceRifiuto) switch
            {
                (_, "bad_oauth_state" or "bad_oauth_callback" or "flow_state_already_used")
                    => OAuthRifiuto.Scaduto,   // codici Gotrue: state o verificatore PKCE consumato o scaduto
                ("access_denied", _) when string.IsNullOrWhiteSpace(codiceRifiuto)
                    => OAuthRifiuto.Annullato, // il no dell'utente sulla schermata di Google
                _ => OAuthRifiuto.Generico,    // access_denied CON error_code = policy del server; e tutto il resto
            };
```

Preceduta dalla guardia di `:44`, che è la regola che chiude il buco principale:

```csharp
        parametri.TryGetValue("error", out var errore);
        // …
        if (!string.IsNullOrWhiteSpace(errore))
```

**Le due righe applicabili della tabella del mandato non collidono davvero**, e la verifica non è mia ma di `bug-hunter`, che l'ha ricostruita per conto suo: la riga `Annullato` richiede `error_code` **vuoto**, la riga `Scaduto` richiede una fra tre stringhe **non vuote**. Le condizioni sono strutturalmente disgiunte, quindi l'ordine di valutazione non può produrre una classificazione diversa da quella corretta. `error=access_denied&error_code=bad_oauth_state` prende `Scaduto` — l'unico ramo applicabile, non una precedenza arbitraria.

### Le tre frasi, verbatim — `Services/SupabaseService.cs:117-121`

```csharp
OAuthRifiuto.Annullato => "L'accesso con Google non è stato autorizzato: sulla schermata di Google il permesso non è stato concesso. Prova di nuovo a entrare con Google e conferma quando te lo chiede.",
OAuthRifiuto.Scaduto => "Non è stato possibile completare l'accesso: l'autorizzazione avviata con Google vale una sola volta e per pochi minuti, e questa non era più valida al ritorno. Prova di nuovo a entrare con Google.",
_ => "L'accesso con Google non è riuscito: la richiesta è stata rifiutata, e può essere un problema temporaneo del servizio oppure una condizione del tuo account. Prova di nuovo a entrare con Google fra un momento.",
```

E la diagnosi, `:113`, una sola riga come il mandato chiede:

```csharp
Console.Error.WriteLine($"[Auth] Ritorno da Google rifiutato: {esito.Diagnostica}");
```

**Perché queste tre e non altre.** Il criterio del mandato era «una frase per ogni azione diversa». `backend-expert` ha corretto il criterio, e la correzione è giusta: le tre azioni **non** sono diverse — tutte e tre dicono «prova di nuovo a entrare con Google». Ciò che le separa è l'**attribuzione**: *tu* (annullato), *il collegamento* (scaduto), *il servizio o il tuo account* (generico). È questa la ragione per cui servono tre frasi e non una, e regge anche meglio del criterio originale: dire «hai annullato» a chi è stato rifiutato dal server è **falso**, ed è l'unico errore davvero grave dei tre.

**L'allineamento con `:222`, che il mandato chiedeva.** La frase `Scaduto` riusa **verbatim** il segmento «vale una sola volta e per pochi minuti» dalla frase dell'unità 15 (`SupabaseService.cs:222`), e ne condivide l'incipit «Non è stato possibile completare l'accesso». È deliberato: è lo stesso fatto — l'ingresso non riuscito — e il commento preesistente a `:214-215` lo aveva già enunciato («*per chi legge è una cosa sola: l'autorizzazione è monouso e dura poco*»). Divergono su ciò che il mandato imponeva di far divergere: **questa non nomina la connessione**, perché il redirect è arrivato e la rete c'era. `conformity` ha verificato entrambe le cose citando le righe.

---

## ADJUDICA

    istruttoria: 2 rilievi su 2 file → checker no

La soglia si calcola sui soli `bug-hunter` e `conformity`: 0 + 2 = **2**, sotto il ≥ 4; i file distinti citati sono **2** (`SupabaseService.cs`, `OAuthCallback.cs`), sotto il ≥ 3. Sotto soglia su entrambi i metri, quindi nessun `checker`. I tre rilievi di `backend-expert` non entrano nel conteggio per regola e si adjudicano sulla `RISCRITTURA`.

**`threat-hunter` — 0 rilievi.** È il revisore che conta di più qui, ed è rientrato con la catena percorsa per intero (`Analizza` → `ErroreAccesso` → `Benvenuto.razor:211` → `:28`) e con `Diagnostica` cercata per grep in tutto il repo: gli unici lettori sono `Console.Error.WriteLine` a `SupabaseService.cs:113` e le asserzioni dei test. Nessuna pagina, nessuna telemetria.

Due sue esclusioni motivate che vale la pena registrare, perché sono le obiezioni che chiunque solleverebbe:

- **La console non è il banner.** Il testo dell'estraneo sopravvive in `Console.Error`, ed è voluto. Ma il valore dello spoofing sta nell'autorità apparente della cornice: il `role="alert"` è chrome del sito e chi lo legge attribuisce la frase a Eton; la console DevTools la apre solo chi ha già cliccato quel link, e non porta il sigillo del dominio. Stesso dato, superficie diversa.
- **Niente log-forging.** `%0A` e le sequenze ANSI possono entrare in `Diagnostica`, ma il sink è `console.error(str)` con **un solo** argomento: le direttive di formato `%c`/`%s` richiedono un secondo argomento e qui non c'è, il pannello non interpreta HTML, e nessun sistema di audit legge quella console.

**`bug-hunter` — 0 rilievi.** Ha verificato per conto suo le otto cose del brief. Tre risposte che tengo perché sostengono conclusioni di questo resoconto: la disgiunzione delle due righe della tabella (sopra); il caso `?error_code=` con valore vuoto, che **entra** nel dizionario con valore `""` perché `LeggiQuery:93-94` scarta solo `separatore <= 0` — motivo per cui la guardia giusta è `IsNullOrWhiteSpace` e non `is null`; e il fatto che, per costruzione del `return` a `:64-67`, non esiste esito con `Errore != Nessuno` e `Codice != null` insieme, quindi l'`if`/`else if` di `:108`/`:126` non nasconde nessun caso.

Il suo punto 7 non era un rilievo ma era sostanziale, **e l'ho accolto come mio** (v. C4 qui sotto).

**`conformity` — 2 rilievi, entrambi FONDATI, entrambi corretti.**

- **`SupabaseService.cs:111` — FONDATO, verificato da me.** Il commento citava `(v. :208)`, ma il diff stesso aveva inserito 13 righe sopra quel punto: il `[Auth]` a cui rimandava è ora a `:221`, e `:208` cade dentro un altro commento. Prova mia, `grep -n "Scambio del codice PKCE" Services/SupabaseService.cs` → `221:`. L'avevo trovato anch'io leggendo il diff prima della review, e l'ho tenuto da parte come metro sulla qualità della loro lettura: l'hanno trovato in due indipendentemente. **Corretto** in `(v. ScambiaCodiceAsync)`: dentro lo stesso file si cita il simbolo, che non scade.
- **`OAuthCallback.cs:4-14` — FONDATO, verificato da me aprendo i due omologhi.** L'enum aveva righe vuote fra i membri e la virgola dopo l'ultimo. `EsitoSalvataggio` (`Services/RisultatoSalvataggio.cs:5-17`, ultimo membro `Sparita`) ed `EsitoImporto` (`Services/Denaro.cs:9-28`, ultimo membro `TroppoGrande`) hanno membri contigui e nessuna virgola finale. **Corretto.** Non è formattazione: è il segnale che i tre enum si leggono come una famiglia.

**`backend-expert` — 3 rilievi, tutti `TIPO: unità`, nessuno `TIPO: progetto`.** Due accolti, uno scartato.

- **`OAuthCallback.cs:58` — la ternaria annidata → `switch` su tupla. FONDATO, accolto.** La riscrittura è nei `CONTRATTI` qui sopra. L'argomento decisivo non è estetico: è la riga che si toccherà quando Gotrue aggiungerà un codice, e nella forma a tabella si aggiunge una riga invece di rinnestare una ternaria. In più i tre commenti in coda dicono **da dove vengono le tre stringhe**, che era l'unica cosa che il codice non diceva.
- **`SupabaseService.cs:111` — il riferimento `:208`. FONDATO**, lo stesso di `conformity`, corretto una volta sola.
- **`SupabaseService.cs:115` — estrarre le tre frasi in un `FraseRifiuto(OAuthRifiuto)`. FONDATO NEL MERITO, SCARTATO PERCHÉ FUORI DAL BUDGET**, e la ragione non è formale. Il mandato dice «nessun helper»; ma soprattutto **il progetto stesso documenta il criterio opposto**: `Services/Testi.cs:99-103` spiega perché `MessaggioImporto` sta centralizzato — «*sta qui e non in una pagina perché due pagine … mostrano lo stesso errore … e devono dirlo con le stesse parole*». `OAuthRifiuto` ha **un solo** call-site. Applicando il criterio scritto dal progetto, questa mappatura va tenuta inline. È `conformity` ad avermi portato questo argomento, contro il rilievo di un altro revisore. **Resta però un pezzo vero nella proposta**, e lo giro al capo in `FUORI SCOPE` 1: l'helper è la sola forma in cui diventa possibile un test «ogni valore dell'enum ha una frase», perché `GetClientAsync` non è istanziabile in un test.

**Campione sugli infondati: non ce n'è nessuno da riverificare, perché nessun rilievo è stato scartato come infondato.** L'unico scartato lo è per budget e per un criterio del codebase, non perché il claim fosse falso — e il claim, infatti, l'ho dichiarato fondato nel merito. La dichiarazione che il protocollo impone vale quindi come dichiarazione di assenza. In compenso ho riaperto io, sul disco e dopo i gate: `Services/RisultatoSalvataggio.cs:5-17`, `Services/Denaro.cs:9-28`, `Services/Testi.cs:95-115`, `Services/SupabaseService.cs:221`, `Services/OAuthCallback.cs:76-102` (`LeggiQuery`, per le frontiere), `Pages/Benvenuto.razor:26-33`.

### Le quattro correzioni applicate dopo la review

| | dove | origine |
|---|---|---|
| **C1** | `OAuthCallback.cs:55-62` | `backend-expert` — la tabella scritta come tabella |
| **C2** | `OAuthCallback.cs:4-14` | `conformity` — stile allineato ai due enum esistenti |
| **C3** | `SupabaseService.cs:111` | `conformity` + `backend-expert` — `:208` → `ScambiaCodiceAsync` |
| **C4** | `OAuthCallbackTests.cs:167-180` | **mia**, dal punto 7 di `bug-hunter` |

**Su C4, perché è la più importante delle quattro.** `bug-hunter` ha notato che nel test per reflection la sequenza `visibili` era **vuota** in 4 casi su 5 — `Codice` è `null` nel ramo d'errore ed `Errore` è un enum, che `as string` scarta — e `Assert.All` su una sequenza vuota **passa sempre**. Il test non asseriva nulla proprio nei casi d'attacco. Si chiude con un token: `as string` → `?.ToString()`. Così `Errore` rientra nell'esame, la sequenza non è mai vuota, e la guardia copre anche una proprietà futura **non**-stringa il cui `ToString()` portasse il testo del provider. L'output della prova qui sotto lo conferma da sé: «*1 out of **2** items*».

---

## FUORI SCOPE

### 1. `SupabaseService.cs:115` — l'helper `FraseRifiuto`, e il test che abiliterebbe

Non è un difetto: è una decisione di budget che appartiene al capo, non a me. La proposta di `backend-expert`:

```csharp
    ErroreAccesso = FraseRifiuto(esito.Errore);
    // …
    private static string FraseRifiuto(OAuthRifiuto rifiuto) => rifiuto switch { … };
```

**Contro**: il mandato vieta gli helper, e `Services/Testi.cs:99-103` fissa a **due** call-site la soglia del progetto per centralizzare una mappatura enum→frase. Qui il call-site è uno.

**A favore**, ed è la parte che il capo deve vedere: il bootstrap `GetClientAsync` passa da 54 a 67 righe, e l'aggiunta è **dato**, non logica — chi legge il flusso (`LoadSession` → callback → `_initialized` → `NavigateTo`) deve scavalcare tre stringhe da duecento caratteri. Soprattutto: con l'helper diventa scrivibile un test sul modello di `PermessiTests:102-120` che itera `Enum.GetValues<OAuthRifiuto>()` e verifica che **ogni** valore abbia una frase — la sola forma di obbligo che il compilatore non può dare (v. la nota su `CS8509` negli `SCOSTAMENTI`). Oggi quel test è impossibile, perché `GetClientAsync` non è istanziabile in un test.

Costa poche righe a chi apre il file per un altro motivo. Non l'ho fatto di nascosto.

### 2. `?error_description=x` **senza** `error`: l'URL non viene più ripulito

È un cambiamento di comportamento reale, l'unico del diff, e l'ha isolato `bug-hunter`. Prima, quell'URL apriva il ramo d'errore e quindi passava da `NavigateTo(BaseUri, replace: true)` a `:143`; ora è classificato `Nessuno` e la barra degli indirizzi conserva la query.

**Non l'ho considerato un difetto**, e la ragione è che ripulirlo non chiuderebbe niente: la stringa è nella barra perché ce l'ha messa l'attaccante nel link, e la vittima l'ha già vista cliccandolo. Ripulirla richiederebbe di riconoscere quell'URL come ritorno OAuth — cioè riaprire esattamente il ramo che l'unità esiste per chiudere. Il comportamento nuovo è coerente con la classificazione: non è un ritorno OAuth, e Eton non tocca l'URL delle navigazioni che non lo sono. **Lo segnalo perché è un fatto osservabile nel browser**, non perché vada corretto.

### 3. Due JS interop fuori dalla rete di sicurezza — ereditato dall'unità 15, non toccato

`_pkce.Leggi()` fuori dal `try` e `_pkce.Cancella()` nel `finally` possono lanciare `JSException` senza copertura. Il difetto è di flusso, il mandato vieta di toccare il flusso, ed è già registrato in `handoff/15-accesso-non-riuscito/resoconto.md`, `FUORI SCOPE` 3. Lo ripeto solo perché questa è l'ultima unità del piano e non voglio che si perda con la chiusura del ciclo.

---

## GATE

- `dotnet build -warnaserror` → **Avvisi: 0, Errori: 0.**
- `dotnet test --no-build` → **Superato! Non superati: 0. Superati: 287. Ignorati: 0. Totale: 287.**

**Il numero e la differenza, come il mandato chiede: da 273 a 287, cioè +14.** Il conto torna sul file, non per sottrazione: `Eton.Tests/OAuthCallbackTests.cs` aveva **12** `[Fact]` e nessuna `[Theory]`; ora ha **18** `[Fact]` e **2** `[Theory]` con **8** `[InlineData]` complessivi, cioè **26 casi eseguiti**. 26 − 12 = 14. **Nessuno dei 273 è rosso**: i quattro test che asserivano sul testo grezzo contano come modificati, non come persi — tre sono stati adeguati al nuovo tipo e `Decodifica_i_valori_percent_encoded` è stato spostato su `code`, che è l'altro parametro che usa la decodifica percent-encoded (`?code=ab%20c` → `"ab c"`).

Compilato **io**, a fine giro. Le compilazioni intermedie sono solo quelle delle due prove qui sotto, eseguite in sequenza e mai in parallelo con un implementer al lavoro.

**Server di sviluppo non avviato, browser non usato**, come il mandato vieta. Nessun processo lasciato vivo, nessuna porta occupata. **Nessun commit**: i tre file sono nel working tree.

---

## SCOSTAMENTI

1. **Il `Console.Error.WriteLine` col grezzo sta in `SupabaseService`, non in `OAuthCallback`.** Il mandato diceva «un solo `Console.Error.WriteLine` col marcatore che il file già usa», e il marcatore `[Auth]` è di `SupabaseService`: `OAuthCallback.cs` non ne ha mai avuto uno, perché non scrive da nessuna parte. Metterlo lì avrebbe rotto la proprietà che il file dichiara di sé in testa — «*analisi **pura** dell'URL*» (`:22-30`) — che è la ragione per cui la classe esiste separata ed è testabile senza browser. Quindi: `Analizza` resta pura e **riempie** `Diagnostica`; a registrarla è il chiamante. La riga è **una sola**, come prescritto, e porta il marcatore giusto. Effetto collaterale utile: i test possono asserire sul grezzo senza catturare lo stream della console. `backend-expert` ha giudicato la linea corretta su entrambi i fronti.

2. **Il ramo di default dello `switch` delle frasi è `_`, non `OAuthRifiuto.Generico`.** Avevo chiesto a `backend-expert` se non fosse meglio uno switch esaustivo che il compilatore obbliga ad aggiornare. **Non esiste in C#**: `CS8509` scatta anche elencando tutti i membri, perché un `enum` è un `int` e `(OAuthRifiuto)99` è legale. La scelta reale è fra `_` e `throw`, e un `throw` qui cadrebbe dentro `_initLock` **prima** di `_initialized = true`: risalirebbe fino a `Benvenuto.OnInitializedAsync` e romperebbe la pagina d'ingresso invece di mostrare un messaggio. Il progetto usa già lo stesso ramo `_` in `Testi.cs:113`.

3. **Un secondo giro di scrittura dopo la review**, per le quattro correzioni della tabella sopra, invece di un solo passaggio. Il diff finale è quello, non c'è codice intermedio residuo.

---

## IL DIFETTO È CHIUSO?

**Sì, e la risposta è per output, non per dichiarazione.** Ho eseguito due mutazioni deliberate, con copia di sicurezza del file fuori dal repository e ripristino verificato per hash. Il mandato chiedeva due cose distinte, e sono due prove distinte.

### Prova 1 — quale test fallirebbe se qualcuno rimettesse la descrizione nel canale visibile

Reintrodotto il difetto nell'unico canale stringa che resta visibile, `Codice` — `OAuthCallback.cs:64`, primo argomento da `null` a `descrizione`. Il codice **compila** (`Avvisi: 0, Errori: 0`): è una regressione plausibile, non un refuso grossolano. E i test:

```
Non superato Eton.Tests.OAuthCallbackTests.La_descrizione_del_provider_non_esce_mai_dal_canale_diagnostico(uri: "https://esempio.it/?error=chissa&error_code=chissa"···)
   Assert.All() Failure: 1 out of 2 items in the collection did not pass.
     Error: Assert.DoesNotContain() Failure: Sub-string found
     at …\OAuthCallbackTests.cs:line 180
Non superato! - Non superati: 3. Superati: 284. Ignorati: 0. Totale: 287.
```

**Tre casi su cinque della `[Theory]` diventano rossi** — tutti e tre quelli con `error` presente, cioè tutti quelli che classificano davvero un rifiuto. Gli altri due non hanno un rifiuto da classificare, quindi non c'è descrizione da far uscire.

Il «**1 out of 2 items**» è la prova che la correzione C4 serviva: la collezione esaminata ha due elementi (`Codice` e `Errore`) e non zero. Con l'`as string` di prima quei tre casi avrebbero avuto una collezione **vuota**, `Assert.All` sarebbe passato, e questa prova sarebbe stata **verde con il difetto dentro**.

File ripristinato: `md5sum` di `Services/OAuthCallback.cs` tornato a `c20555d5c80da5f76e4f034e91fb7ac1`, identico alla copia presa prima della mutazione.

### Prova 2 — perché il tipo lo rende impossibile senza toccare quel test

La regressione più probabile non è quella di sopra: è che un'unità futura riscriva la riga che il difetto **aveva davvero**, `SupabaseService.cs:110` nella numerazione pre-diff. L'ho rimessa verbatim al posto dello `switch`:

```csharp
                    ErroreAccesso = esito.Errore;
```

```
G:\Sviluppo\Eton\Services\SupabaseService.cs(115,37): error CS0029: Non è possibile convertire
in modo implicito il tipo 'Eton.Services.OAuthRifiuto' in 'string'
    Errori: 1
```

**Non compila.** Nessun test viene eseguito, perché non c'è niente da eseguire: la regressione è intercettata dal compilatore, prima della suite. È il motivo per cui il `LAVORO 1` del mandato era «la parte che conta»: finché `Errore` era una `string?`, quella riga era una svista possibile; ora è `CS0029`.

File ripristinato: `md5sum` di `Services/SupabaseService.cs` tornato a `a7c04c82aa83e1e86db28852b45e69a1`. Build e test rieseguiti dopo il ripristino: **0 avvisi, 0 errori, 287/287 verdi**. Le copie di sicurezza sono state cancellate.

### Le due prove insieme

Il tipo presidia il canale **vecchio** — quello che il difetto usava, e lo fa a tempo di compilazione. Il test presidia i canali **nuovi**, cioè le proprietà che qualcuno aggiungerà in futuro, e lo fa per reflection su tutte le proprietà pubbliche tranne `Diagnostica`. Nessuna delle due basta da sola, ed è scritto nel file, in coda a `OAuthCallbackTests.cs:182-187`.

### Il ramo che prende un URL senza nessuno dei parametri attesi

Il mandato chiede di dichiararlo esplicitamente. `https://esempio.it/`, oppure `https://esempio.it/?qualunque=cosa`: `errore` è `null`, la guardia di `:44` non scatta, non c'è `code`, e si esce da `:73` con `new OAuthCallbackEsito(null, OAuthRifiuto.Nessuno, null)`. In `SupabaseService` non entra né nel ramo dell'errore (`:108`) né in quello del codice (`:126`): cade nell'`else` di `:130` e chiama `RinnovaSessioneAsync()`. **Cioè: non è un ritorno OAuth, ed è esattamente il comportamento di prima del diff.** Il test che lo copre è `Url_normale_non_e_un_ritorno_oauth`.

---

## DA PROVARE NEL BROWSER

Il mandato vieta di avviare il server e di provare nel browser, quindi **non l'ho fatto**: questa è la ricetta, non un esito. Il messaggio compare in `<div class="errore" role="alert">` di `Pages/Benvenuto.razor:28`, sopra il pulsante «Entra con Google».

Attenzione a **dove** compare: Google riporta su `_navigation.BaseUri`, cioè la radice, che è rotta privata; `AuthRedirect.razor:68` rimbalza l'anonimo su `/benvenuto`, e il messaggio sopravvive perché `SupabaseService` è `AddSingleton` (`Program.cs:12`) e la navigazione è client-side. Il riquadro si legge su `/benvenuto`, non sull'URL che si è incollato.

### 1. L'URL dell'attacco — **non deve mostrare niente**

```
http://localhost:<PORTA>/?error_description=Account+bloccato,+chiama+il+numero
```

Atteso: **nessun riquadro d'errore**, nessuna riga in console, e la query **resta** nella barra degli indirizzi (v. `FUORI SCOPE` 2). Se compare un riquadro — anche con parole nostre — la regola di `:44` non sta reggendo.

### 2. L'annullamento simulato — deve dire che è stato annullato

```
http://localhost:<PORTA>/?error=access_denied
```

Atteso, verbatim:

> L'accesso con Google non è stato autorizzato: sulla schermata di Google il permesso non è stato concesso. Prova di nuovo a entrare con Google e conferma quando te lo chiede.

In console, contemporaneamente:
`[Auth] Ritorno da Google rifiutato: error=access_denied; error_code=; error_description=`

### 3. Le altre due frasi, per completezza della tabella

```
http://localhost:<PORTA>/?error=invalid_request&error_code=bad_oauth_state     → «scaduto»
http://localhost:<PORTA>/?error=access_denied&error_code=signup_disabled       → «generico», NON «annullato»
```

Il secondo è il caso che giustifica l'intera coppia `(error, error_code)`: guardando il solo `error` direbbe «hai annullato» a chi è stato rifiutato dal server.

### 4. L'annullamento **vero** su Google — l'unica prova che conta davvero

`tech-advisor` dichiara **confidenza media** sulla tabella dei valori di Gotrue: ha letto il sorgente di `master`, non la versione che gira sul progetto. È il rischio che il mandato segnala, e le prove 2 e 3 non lo tolgono, perché costruiscono la query a mano.

1. Apri l'app da anonimo, arrivi su `/benvenuto`.
2. Premi **«Entra con Google»**.
3. Sulla schermata di Google premi **«Annulla»** / nega il permesso — non completare l'accesso.
4. Torni sull'app. **Guarda la console prima del riquadro**, e annota la riga `[Auth] Ritorno da Google rifiutato: …` **per esteso**.

**Che cosa si sta verificando**: che quella riga mostri `error=access_denied` con `error_code=` **vuoto**. Se `error_code` risultasse valorizzato, l'annullamento verrebbe classificato **generico** invece che annullato — l'utente leggerebbe «riprova fra un momento» invece di «conferma quando te lo chiede». Sarebbe impreciso, non falso, ed è il verso in cui la classificazione è stata costruita per sbagliare: la riga di console dice al capo esattamente quale valore aggiungere alla tabella per correggerla.

L'errore opposto — un rifiuto del server classificato «annullato» — richiederebbe un `access_denied` **senza** `error_code` proveniente da una policy, e non è provocabile a mano da questa parte.

### Cosa NON si riesce a provocare

Un rifiuto di policy reale (`signup_disabled`, account sospeso) richiede di cambiare la configurazione del progetto Supabase. La prova 3 ne simula la forma dell'URL, non la provenienza: se un giorno le iscrizioni verranno chiuse davvero, la riga di console dirà se il codice arriva come previsto.

---

## DOMANDE PER IL CAPO

L'utente non è raggiungibile, quindi non ho chiesto niente in chat. Tre cose che decide lui:

1. **L'helper `FraseRifiuto` e il test su `Enum.GetValues`** — `FUORI SCOPE` 1. Fuori dal budget di questa unità; a favore c'è un test che oggi è impossibile scrivere. Costa poche righe a chi riapre il file.
2. **L'URL non più ripulito** nel caso `?error_description=` senza `error` — `FUORI SCOPE` 2. L'ho lasciato così di proposito; se il capo lo considera un difetto, la correzione **non** va fatta in `Analizza` ma nella condizione di `SupabaseService.cs:142`, altrimenti si riapre il ramo che l'unità chiude.
3. **La prova 4 nel browser è l'unica che chiude la confidenza media di `tech-advisor`.** Finché non la fa qualcuno, la classificazione «annullato» resta un'ipotesi ben costruita, non un fatto misurato. La riga di console è già lì apposta per rispondere in un colpo solo.
