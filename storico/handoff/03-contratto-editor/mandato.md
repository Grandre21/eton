UNITÀ: 03/10 — Il contratto degli editor, e il suo primo consumatore

## OBIETTIVO

Chiudere un editor con modifiche non salvate smette di perdere il lavoro in silenzio, e
l'editor delle note smette di aprirsi nudo.

Oggi, in tutti e quattro gli editor, si scrive del testo, si preme «Chiudi», e il testo
sparisce senza una domanda; il tasto Indietro restituisce un editor vuoto. In tutto il
progetto ci sono **zero** occorrenze di `beforeunload`, `NavigationLock`,
`LocationChanging`, `confirm(`: nessun editor protegge il lavoro in corso.

Tre risultati osservabili, tutti su `/notes/new` e `/notes/{id}`:

1. Scritto del testo e premuto «Chiudi», compare una domanda. Annullando, si resta
   nell'editor col testo intatto. Vale anche per il tasto Indietro del browser e per i link
   della barra di navigazione.
2. Salvato e premuto «Chiudi», **non** compare nessuna domanda. Vale anche subito dopo
   aver creato una nota nuova e subito dopo averla eliminata: sono i due casi in cui
   l'implementazione ingenua sbaglia.
3. L'editor ha una testata con il titolo e l'infobutton «?», come le cinque schermate che
   già ce l'hanno; e il segnaposto del corpo va a capo invece di mostrare `&#10;`.

L'astrazione che produci la consumeranno altre tre unità. È il vero prodotto di questa
unità: `NoteEdit.razor` è il primo consumatore, e serve a provare che il contratto si
incastra davvero.

## PERIMETRO — file di tua proprietà esclusiva

- `Shared/PaginaEditor.cs` — **da creare**, con questo nome esatto.
- `Pages/NoteEdit.razor`

## NON TOCCARE

- **`Pages/CollectionEdit.razor`, `Pages/ItemEdit.razor`, `Pages/SpesaEdit.razor`.**
  Consumeranno la tua classe base, ma lo faranno le unità 04, 05 e 06. Non anticipare il
  loro lavoro: due sessioni sullo stesso file si sovrascrivono.
- **`Shared/TestataPagina.razor`.** Lo **consumi** con l'API esistente
  (`Titolo` / `Aiuto` / `Azione`) e non lo modifichi. Se scopri di aver bisogno di
  un'opzione nuova, è un `BLOCKED`: torna al capo, non allargare l'API da solo.
- **`wwwroot/css/app.css`.** È l'unico foglio di stile del progetto e appartiene
  all'unità 10. Se ti serve stile nuovo, usa classi già esistenti o stile inline; se non
  basta, torna `BLOCKED`. Non modificarlo.
- `Shared/PaginaRegistro.cs`, che è il modello da imitare ma non da cambiare.

## CONTRATTI

**Questa è la firma che le unità 04, 05 e 06 riceveranno nel proprio mandato.** Se devi
scostartene, il resoconto deve riportare la firma reale citata testualmente: è l'unica
cosa che impedisce a tre unità successive di divergere.

```csharp
public abstract class PaginaEditor : ComponentBase
{
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    // Ogni editor ha già 'private bool Cambiata': diventa 'protected override'.
    protected abstract bool Cambiata { get; }

    // Sostituisce Navigation.NavigateTo dopo Crea() ed Elimina(): esce senza far
    // scattare la guardia.
    protected void Esci(string uri, bool replace = false);

    // Handler per NavigationLock.OnBeforeInternalNavigation.
    protected async Task GuardaUscita(LocationChangingContext ctx);
}
```

Una riga di markup per editor, dentro il ramo del modulo:

```razor
<NavigationLock ConfirmExternalNavigation="@Cambiata" OnBeforeInternalNavigation="GuardaUscita" />
```

**Perché una classe base e non un componente `<GuardiaUscita Sporca="@Cambiata" />`.** Il
componente **non funziona**, e non per ragioni di stile: dopo `Crea()` l'editor chiama
`NavigateTo` con `Cambiata` **ancora vera**, e fra quella decisione e la navigazione non
c'è nessun render. Un `[Parameter]` porta il valore catturato all'ultimo render, quindi la
guardia chiederebbe «hai modifiche non salvate» **subito dopo un salvataggio riuscito**.
Una classe base legge i campi vivi nell'istante dell'handler. È anche il motivo per cui
`Esci(...)` esiste: disarma la guardia per le uscite volute.

**Semantica verificata contro la documentazione .NET 10 e il sorgente di
`NavigationLock`** — non riverificarla, è costata già una consultazione:

- `OnBeforeInternalNavigation` copre i link interni, `NavigateTo`, **e il tasto
  Indietro/Avanti del browser**. Il doc lo dice testualmente.
- `ConfirmExternalNavigation` copre solo chiusura scheda, ricarica, barra indirizzi, link
  esterni e `forceLoad`. **Si può legare dinamicamente a `@Cambiata`**: `NavigationLock`
  confronta il valore col precedente in `OnAfterRenderAsync` e chiama
  `enable/disableNavigationPrompt` a ogni cambio, quindi il prompt del browser compare solo
  quando c'è qualcosa da salvare.
- Il testo del dialogo esterno **non è personalizzabile**: è `beforeunload`. Non provarci.
- Per il caso interno usa `confirm()` via `IJSRuntime`, che è l'esempio ufficiale del doc e
  non richiede nessun file `.js` — è una chiamata diretta alla globale, come fa già
  `SpaceDetail.razor` con `navigator.clipboard.writeText`. **Non costruire un dialogo
  in-app**: il caso esterno è per forza il dialogo nativo del browser, e due stili diversi
  per la stessa domanda leggono come due cose diverse.

**Una trappola che ti costa un giro se la scopri tardi.** `NoteEdit.razor` ha già
`@inject NavigationManager Navigation`. Con la base che lo dichiara `[Inject] protected`,
quella riga va **tolta**: è esattamente ciò che fa `PaginaRegistro` con `Spazi`, guardalo.

## COSA FARE SU `NoteEdit.razor`, oltre alla guardia

- **Testata** (rilievo 12): `<TestataPagina>` in testa con titolo e `<Aiuto>`, come nelle
  cinque schermate che ce l'hanno già. Il titolo è **già calcolato** per `<PageTitle>`:
  riusalo, non ricalcolarlo.
- **L'esito dove si guarda** (rilievo 2): oggi il blocco `errore`/`avviso` sta sopra il
  modulo, e il pulsante Salva in fondo; su un modulo lungo si preme Salva e non cambia
  nulla in vista. Sposta il blocco **subito sopra** il blocco delle azioni. Niente barra
  sticky: interagirebbe con la barra inferiore su telefono e col banner PWA dell'unità 10.
- **Il segnaposto** (pendenza P1): il `placeholder` del corpo mostra letteralmente `&#10;`.
  Non è un refuso: il compilatore Razor emette l'attributo come stringa letterale e il
  renderer lo passa a `setAttribute`, quindi nessuno decodifica le entità HTML. Si risolve
  dicendo l'a-capo in C#: `placeholder="@("… \n\n …")"`. È l'unico caso nel progetto.

## BUDGET DI COMPLESSITÀ

Nessuna astrazione nuova oltre a `PaginaEditor`. Nessun file `.js`. Nessun servizio
iniettato nuovo, nessun tipo nuovo oltre alla classe base. Se un helper ha un solo
call-site, va inline.

## STATO

Unità precedente: **02, `FATTO`** — resoconto in `handoff/02-collezioni-insert/resoconto.md`.
Ha corretto il privilegio di INSERT sulle collezioni; non tocca nulla di tuo. La sua
migrazione **non è ancora stata eseguita in produzione** dall'utente, ma non ti riguarda:
questa unità non passa dalle collezioni.

Il piano del lavoro è in `handoff/PIANO.md`. La sezione `DECISIONI` contiene le scelte
dell'utente già prese: **rileggila**, e se ci trovi una riga che contraddice questo
mandato, vince la più recente.

## GATE

- `dotnet build` → **0 errori, 0 avvisi**.
- `dotnet test` → tutti verdi. Erano **267** all'ultimo giro.

Compili **tu**, una volta, a fine giro: gli `implementer` non compilano mai — `obj/` non ha
lock fra processi e due build concorrenti si corrompono a vicenda.

**Non avviare il server di sviluppo e non provare nel browser.** La prova nel browser la
fa il capo, con `live-testing`, quando le quattro unità sugli editor sono rientrate: il
comportamento va provato una volta sola sulla forma finale, non quattro volte su forme
intermedie. Se lo avviassi tu, lasceresti un processo DevServer vivo sulla porta 5000 che
il capo non sa di dover fermare.

BUDGET: 20 dollari

RESOCONTO IN: `handoff/03-contratto-editor/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 03 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <la firma reale di PaginaEditor, citata testualmente con file:line, membro per
            membro. La leggeranno tre unità: se hai deviato, qui si vede>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge da questo mandato e perché> | nessuno
```

Aggiungi una sezione `DA PROVARE NEL BROWSER`: l'elenco dei comportamenti che il capo dovrà
far verificare a `live-testing`, con il criterio di accettazione di ciascuno. Tu non li
provi, ma sai meglio di chiunque altro dove il tuo codice può sbagliare.
