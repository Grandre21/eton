UNITÀ: 16/16 — Il ritorno da Google non detta più il testo a schermo

## IL DIFETTO — verificato dal capo in prima persona

```csharp
// Services/OAuthCallback.cs:24 — il testo entra dalla query, non validato
if (parametri.TryGetValue("error_description", out var descrizione) && !string.IsNullOrWhiteSpace(descrizione))
    return new OAuthCallbackEsito(null, descrizione);

// Services/SupabaseService.cs:110 — assegnato tale e quale
ErroreAccesso = esito.Errore;

// Pages/Benvenuto.razor:28 — reso sopra il pulsante «Entra con Google»
<div class="errore" role="alert">@errore</div>
```

Chiunque può costruire `https://<dominio>/?error_description=<testo a piacere>` e mandarlo a
qualcuno: il testo compare dentro il riquadro d'errore del **sito legittimo**, in un
`role="alert"`, sopra «Entra con Google».

**Non è XSS.** Razor codifica `@errore`: niente script, niente markup. È **content spoofing** —
testo di un estraneo dentro il chrome del dominio di fiducia, più credibile di un'email di
phishing perché arriva sull'URL vero. Se un revisore lo etichetta `xss`, correggi l'etichetta
nell'adjudica: la severità media regge, il nome no.

## LA DECISIONE, GIÀ PRESA — non riaprirla

**Insieme chiuso.** Il testo che arriva dalla query **non compare mai a schermo**: si classifica
il rifiuto e si mostra una frase scritta da noi.

Posizione di `tech-advisor`, confidenza alta, adottata: l'alternativa — sanificare e mostrare —
**non chiude il difetto**, perché una frase persuasiva sta in quaranta caratteri ASCII puliti
(«Account bloccato per attività sospetta, chiama il 3xx»), e in più produrrebbe l'unico riquadro
non italiano del progetto: ogni descrizione *legittima* di Gotrue è inglese da sviluppatore
(«OAuth state parameter is invalid»). Costa di più e protegge meno.

## LAVORO 1 — il contratto cambia tipo, ed è la parte che conta

**Finché `OAuthCallbackEsito.Errore` è una `string?`, il difetto può regredire per semplice
assegnazione.** Un'unità futura che scrive `Errore = descrizione` lo riapre senza che nulla
diventi rosso.

Quindi: **`Errore` diventa un `enum` a quattro valori** — nessuno, annullato, scaduto, generico —
più un campo **stringa separato per la sola diagnostica**, col grezzo concatenato, che **nessuna
schermata legge**.

Il tipo è il presidio vero: dopo, mostrare il testo del provider non è più una svista possibile,
è un errore di compilazione.

Budget: **un enum**. Nessuna classe nuova, nessun helper, nessun servizio.

## LAVORO 2 — la classificazione, su `(error, error_code)` e non su `error` da solo

**Il redirect porta tre parametri, non due**: `error`, `error_description`, **`error_code`**.
Quest'ultimo è il canale enumerato che Gotrue ha aggiunto proprio per non far dipendere il client
dal testo libero. Oggi il codice non lo legge affatto.

**La distinzione che rende necessaria la coppia:**

- `access_denied` **senza** `error_code` → **l'utente ha annullato** o negato il permesso su
  Google;
- `access_denied` **con** `error_code` → **una policy del server** (iscrizioni chiuse, account
  sospeso, email non verificata).

Guardando il solo `error` si direbbe «hai annullato» a chi è stato rifiutato dal server.

**Tre frasi bastano**, e il criterio è: una frase per ogni **azione diversa** che l'utente può
fare.

| esito | quando | l'azione da suggerire |
|---|---|---|
| **annullato** | `error = access_denied` **e** `error_code` vuoto | premere di nuovo «Entra con Google» e confermare |
| **scaduto** | `error_code` ∈ `{bad_oauth_state, bad_oauth_callback, flow_state_already_used}` | riprovare l'accesso da capo |
| **generico** | tutto il resto, compreso `access_denied` **con** `error_code` | riprovare fra un momento |

**La frase «scaduto» ha la stessa semantica — monouso, pochi minuti — di
`SupabaseService.cs:209`: guardala e allineati.** Ma **senza** nominare la connessione: il
redirect è arrivato, la rete c'era. È la differenza che rende quella frase diversa da tutte le
altre venticinque del progetto.

**La regola che chiude il buco principale:** `error_description` **senza** `error` **non è un
rifiuto**. In OAuth 2.0 `error` è obbligatorio e Gotrue lo mette sempre. Quindi
`?error_description=x` da solo dà esito **nessuno** e non apre nemmeno il riquadro generico —
altrimenti l'attaccante otterrebbe comunque un riquadro d'allarme sul dominio vero, solo con
parole nostre. E `?code=abc&error_description=x` **resta un codice valido**: l'errore ha la
precedenza solo se c'è davvero un errore.

**Tutto il grezzo** — `error`, `error_code`, `error_description` — va in **un solo**
`Console.Error.WriteLine` col marcatore che il file già usa. Tradurre senza registrare
baratterebbe un'indiscrezione con una cecità.

## LAVORO 3 — i test esistenti non compilano più, ed è voluto

`Eton.Tests/OAuthCallbackTests.cs:30-51`: **quattro test asseriscono sul testo grezzo**
(`"Accesso negato"`, `"access_denied"`). Col nuovo tipo non compilano, e questa è la conferma che
il contratto è cambiato davvero.

- **`Decodifica_i_valori_percent_encoded` (`:33`)** usa `error_description` per provare la
  decodifica percent-encoded. La decodifica va ancora provata: **sposta quel test su `code`**, che
  è l'altro parametro che la usa.
- **Test nuovi obbligatori**, uno per riga della tabella più i tre casi di frontiera:
  - `error_description` senza `error` → **nessuno** (è il caso dell'attacco);
  - `error=access_denied` → **annullato**;
  - `error=access_denied&error_code=signup_disabled` → **generico**, non annullato;
  - `error=invalid_request&error_code=bad_oauth_state` → **scaduto**;
  - `error=<valore mai visto>` → **generico**;
  - `code=abc&error_description=x` → **codice valido**, nessun errore;
  - **e il test che difende il difetto**: comunque sia costruita la query, **la descrizione non
    compare mai nell'esito** se non nel campo diagnostico. Scrivilo in modo che fallisca se
    qualcuno rimettesse la stringa nel canale visibile.

## PERIMETRO — file di tua proprietà esclusiva

- `Services/OAuthCallback.cs`
- `Services/SupabaseService.cs` (le righe `:108-112` e `:129`)
- `Eton.Tests/OAuthCallbackTests.cs`

## NON TOCCARE

- **`Pages/Benvenuto.razor`.** Non serve: cambia ciò che le arriva, non come lo mostra. L'ha
  chiusa l'unità 14 e non va riaperta.
- **Il flusso di autenticazione oltre la classificazione dell'errore.** Non tocchi PKCE, non
  tocchi lo scambio del codice, non tocchi il redirect. Se vedi un difetto lì, **`FUORI SCOPE`**:
  è la superficie su cui un errore costa di più.
- **`Shared/AuthRedirect.razor`**, **`Services/AuthStateService.cs`**: li leggi se ti servono,
  non li modifichi.
- **`wwwroot/css/app.css`**: unità 11.

## LA COSA CHE PUÒ ESSERE SBAGLIATA, E COME PROTEGGERTI

`tech-advisor` dichiara **confidenza media** sulla tabella dei valori di Gotrue: ha letto il
sorgente di `master`, non la versione che gira sul progetto. Il rischio concreto:

- se un annullamento su Google arrivasse **con** `error_code` valorizzato, verrebbe classificato
  «generico» invece che «annullato»;
- se un rifiuto arrivasse **senza** `error`, la regola del `LAVORO 2` lo nasconderebbe.

**Costruisci la classificazione perché il caso sconosciuto cada sempre sul generico**, mai su una
frase specifica. Un «riprova fra un momento» a chi ha annullato è impreciso; un «hai annullato» a
chi è stato rifiutato dal server è **falso**, ed è l'errore da evitare.

**Scrivi nel resoconto quale ramo prende un URL che non contiene nessuno dei parametri attesi.**

## BUDGET DI COMPLESSITÀ

Un `enum`. Nessuna classe, nessun servizio, nessun helper, nessun file nuovo, nessun pacchetto.
Se ti trovi a scrivere un parser di query — ce n'è già uno, `LeggiQuery` — o una tabella di
mappatura configurabile, sei fuori strada.

## STATO

Sei l'ultima unità del piano. L'unità 11 (foglio di stile) gira **prima** di te o è già rientrata:
perimetri disgiunti, non ti riguarda.

Unità chiuse e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`), 05 (`e139ce8`),
06 (`f4f2dbd`), 07 (`4327598`), 12 (`8a4a89f`), 08 (`bdd858a`), 09 (`d05416b`), 10 (`2650dc7`),
13 (`459a2fc`), 14 (`b3ca1be`), 15 (`84217ec`).

**Il modello per le frasi**: `handoff/15-accesso-non-riuscito/resoconto.md` è il più vicino — è
la stessa schermata, e chi legge **non è autenticato**: non ha una sessione da ricaricare, non ha
uno spazio, non ha un lavoro in corso da perdere.

**Non committare.** Committa il capo, a resoconto letto.

**L'utente non è raggiungibile**: qualunque domanda tu abbia, portala nel resoconto.

## IL GATE DELLA REVIEW

Tutti e quattro, ed è l'unica unità del piano in cui lo sono:

- **`threat-hunter`** — è un difetto di sicurezza, e vuole sapere se il rimedio chiude davvero.
  **Dagli il difetto originale per esteso**, così giudica il rimedio contro di esso e non in
  astratto.
- **`bug-hunter`** — la classificazione ha rami e frontiere.
- **`conformity`** — le frasi devono assomigliare alle venticinque già scritte.
- **`backend-expert`** — **nasce una superficie nuova**, l'`enum`, ed è il gate del §3. È anche il
  revisore giusto per dire se quattro valori sono i quattro giusti.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**.
- `dotnet test` → tutti verdi. Erano **273**; i tuoi nuovi li portano più in alto, e **nessuno dei
  273 può restare rosso**. I quattro test che riscrivi contano come modificati, non come persi:
  **dichiara il numero finale e la differenza**.

Compili **tu**, una volta, a fine giro.

**Non avviare il server di sviluppo e non provare nel browser.**

BUDGET: 20 dollari

RESOCONTO IN: `handoff/16-oauth-insieme-chiuso/resoconto.md`

## SCHELETRO DEL RESOCONTO

```
UNITÀ: 16 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y>
CONTRATTI: <l'enum verbatim, la tabella di classificazione, le tre frasi>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <cosa resta e a chi appartiene>
GATE: <comando → esito, col numero di test e la differenza>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

**Chiudi con una sezione `IL DIFETTO È CHIUSO?`** e rispondi con una prova, non con una
dichiarazione: **quale test fallirebbe** se qualcuno rimettesse la descrizione nel canale
visibile, e **perché il tipo lo rende impossibile** senza toccare quel test.

Aggiungi `DA PROVARE NEL BROWSER`. Almeno queste, con l'URL esatto da incollare:
`?error_description=Account+bloccato,+chiama+il+numero` (deve **non** mostrare niente),
`?error=access_denied` (deve dire che è stato annullato), e un **annullamento vero** sulla
schermata di Google — che è l'unico modo di verificare la parte su cui `tech-advisor` dichiara
confidenza media.
