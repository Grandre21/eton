UNITÀ: 04/11 — L'editor delle collezioni adotta il contratto, e «Chiudi» smette di essere premibile a metà salvataggio

## OBIETTIVO

Due risultati, su due file distinti.

**Su `Pages/CollectionEdit.razor`**: la stessa cosa che l'unità 03 ha fatto su `NoteEdit`.
Chiudere l'editor con modifiche non salvate chiede conferma; salvare, creare ed eliminare
**non** chiedono niente; la pagina ha una testata con l'infobutton; l'esito del salvataggio
compare sopra i pulsanti invece che in cima a un modulo alto tre schermate.

Quest'ultimo punto è il rilievo 2 e su questa pagina è **osservato dal vivo**, non dedotto:
il modulo con cinque campi è alto tre schermate, si preme «Salva» e la schermata non cambia
di un pixel — nessun messaggio in vista, nessuno stato del pulsante. Per accorgersi del
fallimento bisogna scorrere in su di tre schermate, e nulla suggerisce di farlo.

**Su `Pages/NoteEdit.razor`**: solo il gate di «Chiudi», descritto sotto. Nient'altro: il
resto di quel file è stato fatto dall'unità 03 ed è committato.

## PERIMETRO — file di tua proprietà esclusiva

- `Pages/CollectionEdit.razor`
- `Pages/NoteEdit.razor` — **solo** per il gate di «Chiudi»

## NON TOCCARE

- **`Shared/PaginaEditor.cs`.** È il contratto, prodotto dall'unità 03 e consumato da te e
  da altre due unità. Se scopri che va cambiato, è un `BLOCKED`: torna al capo. Cambiarlo
  da solo romperebbe silenziosamente le unità 06 e 07 che lo adotteranno dopo di te.
- **`Services/CollectionRepository.cs`** e i tre rilievi propri di questa pagina — il
  messaggio d'errore grezzo, il pulsante spento che non spiega, il campo icona. Sono
  dell'**unità 05**, che verrà dopo di te sullo stesso file. Non anticiparla.
- **`Pages/ItemEdit.razor`, `Pages/SpesaEdit.razor`**: unità 06 e 07.
- **`wwwroot/css/app.css`.** Appartiene all'unità 11. Se ti serve stile nuovo, usa classi
  esistenti o stile inline; se non basta, torna `BLOCKED`.
- `Shared/TestataPagina.razor`: lo consumi con l'API esistente, non lo modifichi.

## CONTRATTO — `Shared/PaginaEditor.cs`, firma reale dal file su disco

```csharp
public abstract class PaginaEditor : ComponentBase, IDisposable
{
    [Inject] private NavigationManager Navigation { get; set; }   // private: NON la vedi
    [Inject] private IJSRuntime JS { get; set; }                  // private: NON la vedi

    protected abstract bool Cambiata { get; }
    protected void Esci(string uri, bool replace = false);
    protected async Task GuardaUscita(LocationChangingContext ctx);
    public virtual void Dispose();
}
```

Cosa devi fare per adottarlo:

1. `@inherits PaginaEditor` sulla pagina.
2. Il campo `Cambiata` esistente diventa `protected override bool Cambiata`.
3. Aggiungi **una** riga di markup, **dentro il ramo del modulo** (non fuori dai rami
   condizionali — v. il divieto sotto):
   ```razor
   <NavigationLock ConfirmExternalNavigation="@Cambiata" OnBeforeInternalNavigation="GuardaUscita" />
   ```
4. **Togli** la riga `@inject NavigationManager Navigation`: la base la dichiara. Se
   scoprissi di dover navigare per qualcosa che non è un'uscita da editor, **rimettila** —
   con la base `private` questo non produce l'avviso CS0108, e il gate è a 0 avvisi.
5. I `NavigateTo` che seguono `Crea()` ed `Elimina()` — sono **due** in questo file —
   diventano `Esci(...)`.

**Divieto: non spostare `<NavigationLock>` fuori dai rami condizionali.** L'unità 03 ha
respinto questa stessa proposta di un revisore dopo averla verificata, e il motivo vale
identico qui: nel ramo in cui l'entità è sparita sotto i piedi, lo stato del modulo è
ancora «sporco», e la guardia chiederebbe «hai modifiche non salvate» su una collezione che
non esiste più e che non si può salvare — una domanda con una sola risposta possibile. Se
un revisore te lo propone, adjudica citando questo paragrafo.

**Se la base non implementasse `IDisposable` per te**: la implementa. Non aggiungere
`@implements IDisposable` alla pagina — genererebbe un `Dispose` che **nasconde** quello
della base (CS0108) invece di sovrascriverlo, e la guardia contro la navigazione tardiva
smetterebbe di funzionare **in silenzio**. Se ti serve una pulizia tua, la forma è
`public override void Dispose() { base.Dispose(); … }`, e `base.Dispose()` non è
facoltativo.

## IL GATE DI «CHIUDI» — su entrambi i file

Oggi «Chiudi» è un `<a href>` che **non guarda `occupato`**, mentre ogni input, «Salva»,
`SchedaConflitto` e `ConfermaAzione` lo guardano: è l'unico controllo del gruppo `.azioni`
che resta vivo mentre una scrittura è in volo. La forma è **una sola**, identica nei due
file, e cambia solo la destinazione:

```razor
<a class="btn" href="@(occupato ? null : "notes")">Chiudi</a>
```

**Verificato contro il sorgente di ASP.NET Core 10.0.10, la versione pinnata in
`Eton.csproj:36` — non riverificarlo.** `RenderTreeBuilder.AddAttribute(int, string,
string?)` non aggiunge il frame quando il valore è `null` e il target non è un componente:
l'attributo `href` **non compare affatto**. Un `<a>` senza `href` non è un link, non naviga
e non prende focus.

Tre trappole che vengono dalla stessa verifica:

- **Il valore dev'essere letteralmente `null`.** La condizione nel sorgente è `value != null`:
  una stringa vuota **non** viene omessa. Un `?? ""` messo per prudenza produrrebbe
  `href=""`, cioè un link valido verso la **radice dell'applicazione** — peggio del difetto
  di partenza, e invisibile in revisione.
- **Non generalizzare la regola ai componenti.** Su `<TestataPagina>` e simili, `null` non
  viene mai omesso: è un valore legittimo passato al parametro.
- **Non trasformare «Chiudi» in un `<button>`.** È la via che sembra più pulita e introduce
  un difetto: servirebbe navigare dalla pagina, e l'unica navigazione che la base espone è
  `Esci`, che **disarma la guardia** — quel «Chiudi» uscirebbe senza chiedere niente, cioè
  rimetterebbe il difetto che questo lavoro sta correggendo.

**Perché non basta dire «tanto la scrittura arriva comunque»**, se un revisore obietta: con
esito `Salvata` è solo incertezza, ma con `Conflitto` o `Rifiutata` la modifica **non** è
stata scritta, la pagina è morta e nessuno lo dirà mai all'utente — che ha appena letto «se
esci le perdi» e l'ha creduta falsa perché aveva premuto Salva. Su `Crea`, uscire produce
una collezione che l'utente crede scartata, e un duplicato al secondo tentativo.

**Il selettore CSS non è tuo.** `a.btn:not([href])` va accodato a `.btn:disabled` in
`app.css`, e quel file è dell'unità 11. Fino ad allora il link è funzionalmente inerte ma
non spento visivamente: è atteso, non segnalarlo come difetto e non aggirarlo con stile
inline sul colore.

## ALTRO SU `CollectionEdit.razor`

- **Testata** (rilievo 12): `<TestataPagina>` in testa con titolo e `<Aiuto>`, come le
  cinque schermate che ce l'hanno già e come `NoteEdit` dopo l'unità 03 — **guarda come l'ha
  fatto lei e fai uguale**. Il titolo è già calcolato per `<PageTitle>`: riusalo.
- **L'esito dove si guarda** (rilievo 2): sposta il blocco `errore`/`avviso` da sopra il
  modulo a **subito sopra** il blocco delle azioni. Niente barra sticky: interagirebbe con
  la barra inferiore su telefono e col banner PWA dell'unità 11.

## BUDGET DI COMPLESSITÀ

Nessuna astrazione nuova, nessun tipo nuovo, nessun file `.js`, nessun servizio iniettato
nuovo. Questa unità **applica** un contratto che esiste: se ti trovi a progettare qualcosa,
sei fuori strada. Un helper con un solo call-site va inline.

## STATO

Unità precedenti, entrambe `FATTO` e committate:

- **02** — `handoff/02-collezioni-insert/resoconto.md`. La migrazione che sbloccava le
  collezioni **è stata eseguita in produzione dall'utente**: `/collections/new` funziona di
  nuovo, e questa pagina è di nuovo raggiungibile per intero.
- **03** — `handoff/03-contratto-editor/resoconto.md`. Ha prodotto il contratto e l'ha
  applicato a `NoteEdit`. **Leggi il suo resoconto**: la sezione `CONTRATTI` spiega i tre
  punti in cui la firma reale diverge dalla bozza iniziale, e `NoteEdit.razor` è il modello
  vivo da imitare — hai lo stesso lavoro da fare su una pagina diversa.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: se ci trovi una riga che contraddice
questo mandato, vince la più recente.

## GATE

- `dotnet build` → **0 errori, 0 avvisi**.
- `dotnet test` → tutti verdi. Erano **267** all'ultimo giro.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai.

**Non avviare il server di sviluppo e non provare nel browser.** Lo fa il capo con
`live-testing` quando tutti e quattro gli editor hanno adottato il contratto: il
comportamento va provato una volta sola sulla forma finale. Se lo avviassi tu, lasceresti
un processo DevServer vivo sulla porta 5000 che il capo non sa di dover fermare.

BUDGET: 22 dollari

RESOCONTO IN: `handoff/04-collezione-contratto/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 04 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <come hai consumato PaginaEditor: la riga di @inherits, la firma di Cambiata,
            i due Esci(...), con file:line. Se hai dovuto deviare, qui si vede>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

Aggiungi una sezione `DA PROVARE NEL BROWSER` con i comportamenti da far verificare e il
criterio di accettazione di ciascuno. Includi **il gate di «Chiudi» su entrambe le pagine**,
e dichiara quali prove non sono praticabili a mano e perché — un limite dichiarato vale più
di una prova data per fatta.
