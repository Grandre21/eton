UNITÀ: 15/15 — L'undicesimo punto, e il rilievo 3 si chiude

## OBIETTIVO

**Una riga.** `Services/SupabaseService.cs:194`:

```csharp
ErroreAccesso = $"Accesso non riuscito: {ex.Message}";
```

È resa a `Pages/Benvenuto.razor:28` a un utente **non ancora autenticato**, e mostra il testo
grezzo dell'eccezione. È l'ultima occorrenza del rilievo 3 in tutto il progetto.

L'hanno trovata due strade indipendenti nell'unità 14: il suo `grep` allargato e `threat-hunter`.
Entrambe l'hanno lasciata lì perché `Services/**` era nel suo `NON TOCCARE`.

## PERCHÉ QUESTA VOLTA `Services/` SI TOCCA

Le unità 05, 10, 13 e 14 avevano tutte il divieto di mettere traduzioni nei servizi, e il motivo
è buono: dentro un repository sai **quale query** è fallita, non **quale schermata** la stava
aspettando. L'unità 05 rifiutò metà del proprio perimetro proprio su questo.

**Qui non si applica**, e la differenza va capita prima di scrivere:

- `ErroreAccesso` **non è un errore di repository**: è una **proprietà di stato del servizio di
  autenticazione**, e il servizio *è* il posto dove quel messaggio nasce.
- **C'è una sola schermata che lo mostra**, `Benvenuto.razor`. L'ambiguità che il divieto
  previene — un messaggio scritto senza sapere chi lo leggerà — qui non esiste: si sa esattamente
  chi lo legge.

**Verifica tu entrambe le affermazioni** prima di procedere: cerca chi legge `ErroreAccesso` in
tutto il progetto. Se trovassi **un secondo consumatore**, la premessa cade e il caso torna
ambiguo: allora torna `BLOCKED` invece di scrivere una frase che va bene per uno e male per
l'altro.

## PERIMETRO — file di tua proprietà esclusiva

- `Services/SupabaseService.cs`

Un file, e in pratica una riga più la sua diagnostica. **`Pages/Benvenuto.razor` lo leggi** — per
sapere cosa la pagina offre davvero a chi legge il messaggio — **e non lo modifichi**: l'ha
appena chiuso l'unità 14.

## COSA FARE

**Chi legge questo messaggio non è autenticato**, e questo cambia tutto rispetto alle
venticinque frasi già scritte nel progetto: non ha una sessione da ricaricare, non ha uno spazio
il cui accesso può essere cambiato, non ha un lavoro in corso da perdere. **Le frasi degli altri
file non si trasferiscono.**

La forma resta **fatto, causa, azione**, ma:

- il **fatto** è che l'accesso non è riuscito;
- la **causa** va scelta guardando cosa può davvero fallire lì — la connessione, il provider
  d'identità, un permesso negato dall'utente sulla schermata di Google. **Apri il metodo e guarda
  cosa può lanciare** invece di elencare cause plausibili;
- l'**azione** dev'essere una che la pagina offre **davvero**. È il punto su cui l'unità 14 ha
  trovato il difetto peggiore del suo giro: aveva scritto «scegli di nuovo lo spazio», e quel
  gesto usciva alla seconda riga del metodo senza produrre nulla. **Guarda `Benvenuto.razor` e
  scrivi il gesto che esiste**, con le parole con cui è scritto sul pulsante.

**Il dettaglio tecnico non si perde**: se il `catch` non ha già un `Console.Error.WriteLine`,
aggiungilo con il marcatore che il file già usa — e se il file non ne usa nessuno, guarda quale
usano i servizi vicini. Tradurre senza registrare baratterebbe un'indiscrezione con una cecità.

**Conta i `catch` e le righe di diagnosi del file e riporta i due numeri**, come hanno fatto le
unità 10, 13 e 14.

## NON TOCCARE

- **Qualunque altro file.** Nemmeno `Benvenuto.razor`, nemmeno per una riga.
- **La logica di autenticazione.** Cambi un messaggio, non un flusso. Se ti accorgi che il flusso
  ha un difetto, **scrivilo in `FUORI SCOPE`** e non toccarlo: l'autenticazione è la superficie
  su cui un errore costa di più.

## BUDGET DI COMPLESSITÀ

Nessun tipo, nessun metodo, nessuna astrazione, nessun file. Una stringa e forse una riga di
console. Se il tuo diff supera le venti righe, ti sei allargato.

## STATO

Sei l'ultima unità che tocca il C#. Resta solo l'unità 11, il foglio di stile.

Unità chiuse e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`), 05 (`e139ce8`),
06 (`f4f2dbd`), 07 (`4327598`), 12 (`8a4a89f`), 08 (`bdd858a`), 09 (`d05416b`), 10 (`2650dc7`),
13 (`459a2fc`), 14 (`b3ca1be`).

**I modelli**, in ordine di vicinanza al tuo caso: `handoff/14-errori-rimasti/resoconto.md`
(il più recente, e contiene il caso di `Benvenuto`), poi `handoff/13-errori-tradotti/` e
`handoff/10-recensioni-errori/`.

**Non committare.** Committa il capo, a resoconto letto.

**L'utente non è raggiungibile**: qualunque domanda tu abbia, portala nel resoconto, non
aspettarla.

## IL GATE DELLA REVIEW

Il tuo diff tocca **il messaggio d'errore dell'autenticazione**, cioè la superficie di fiducia
più sensibile del progetto: `bug-hunter`, `conformity` e `threat-hunter`, tutti e tre nello
stesso messaggio. `threat-hunter` **non è opzionale** qui — l'ha già segnalata lui questa riga
nell'unità 14, e vuole sapere se la traduzione risolve.

`backend-expert` no: nessuna superficie nuova, diff minuscolo.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**.
- `dotnet test` → **273 superati**, com'erano quando parti.

Compili **tu**, una volta, a fine giro.

**Non avviare il server di sviluppo e non provare nel browser.**

BUDGET: 12 dollari

RESOCONTO IN: `handoff/15-accesso-non-riuscito/resoconto.md`

## SCHELETRO DEL RESOCONTO

```
UNITÀ: 15 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y>
CONTRATTI: <la frase verbatim, e perché non discende da nessuna delle 25 già scritte>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <cosa resta e a chi appartiene>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

**Chiudi con una sezione `IL RILIEVO 3 È CHIUSO?`** e rispondi rifacendo **entrambi** i `grep`
dell'unità 14 — quello sulla sorgente e quello sui **sink**, che è il migliore dei due:

```
grep -rn 'errore = \$"' Pages Shared Services
grep -rEn '(errore|avviso|ErroreAccesso|Messaggio)\s*=\s*[^;]*\bex\b' --include=*.razor --include=*.cs
```

Riporta l'output di entrambi. **Se resta anche una riga, il rilievo non è chiuso e va detto**: è
la terza volta che questa domanda si pone, e le prime due volte la risposta creduta era
sbagliata. Una risposta senza il comando e il suo output non vale.

Aggiungi `DA PROVARE NEL BROWSER` col testo esatto e come provocarlo — e se non è provocabile a
mano, **dichiaralo come limite**: far fallire un accesso OAuth a comando non è ovvio.
