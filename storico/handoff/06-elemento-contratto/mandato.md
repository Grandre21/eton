UNITÀ: 06/11 — L'editor degli elementi adotta il contratto

## OBIETTIVO

La stessa cosa che le unità 03 e 04 hanno fatto su `NoteEdit` e `CollectionEdit`, ora su
`Pages/ItemEdit.razor`. **Sei la terza di tre gemelle**: il lavoro è già stato fatto due
volte, il contratto ha retto entrambe le volte senza deroghe, e non c'è niente da
progettare.

1. Chiudere l'editor con modifiche non salvate chiede conferma; salvare, creare ed
   eliminare **non** chiedono niente.
2. La pagina ha una testata con titolo e infobutton: oggi si apre «nuda», senza dire dove
   sei.
3. L'esito del salvataggio compare sopra i pulsanti, non in cima al modulo.
4. «Chiudi» smette di essere premibile mentre una scrittura è in volo.

## STATO AL RILANCIO — leggi questo per primo

**Un primo tentativo su questo stesso mandato è morto per un errore 529 del servizio**, non
per budget e non per un problema del lavoro. Ha lasciato `Pages/ItemEdit.razor` modificato
(+31/−17) e **nessun resoconto**.

Il capo ha ispezionato lo stato e **il codice risulta completo**: `@inherits PaginaEditor`,
`<TestataPagina>`, `<NavigationLock>` dentro il ramo del modulo, i due `Esci(...)`, il gate
di «Chiudi» nella forma giusta con `null` letterale, e l'`@inject NavigationManager`
rimosso. `dotnet build -warnaserror` → 0 errori.

**Quello che manca, ed è il tuo lavoro:**

1. **Verifica** che il lavoro sia davvero completo rispetto agli obiettivi qui sotto — in
   particolare il punto 3, lo spostamento del blocco `errore`/`avviso` sopra i pulsanti, e
   l'eventuale secondo canale d'esito, che il capo **non** ha controllato. Non fidarti della
   sua ispezione: era superficiale e fatta da fuori.
2. **La revisione non è stata fatta, o non se ne sa nulla.** Lanciala da zero: `bug-hunter`
   e `conformity` sul diff, più gli altri se il gate del protocollo li richiede. Fai
   l'istruttoria e adjudica.
3. **Il resoconto**, che non esiste.

Se durante la verifica trovi che qualcosa è a metà, **completalo**: è più economico che
ripartire. Se trovi che qualcosa è sbagliato, correggilo e dichiaralo in `SCOSTAMENTI`.

## PERIMETRO — file di tua proprietà esclusiva

- `Pages/ItemEdit.razor`

Un file solo. Se ti servisse toccarne un altro, è un `BLOCKED`.

## NON TOCCARE

- **`Shared/PaginaEditor.cs`**: il contratto. Ha retto su due pagine, fra cui una da 755
  righe con quattro stati in più della tua. Se pensi vada cambiato, è un `BLOCKED`.
- **`Pages/NoteEdit.razor`, `Pages/CollectionEdit.razor`**: già fatte e committate. Le
  **leggi** come modello, non le modifichi.
- **`Pages/SpesaEdit.razor`**: unità 07.
- **`wwwroot/css/app.css`**: unità 11. Usa classi esistenti o stile inline; se non basta,
  torna `BLOCKED` e la voce si accoda alle quattro già in attesa nel piano.
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

I cinque passi dell'adozione, gli stessi che hanno funzionato due volte:

1. `@inherits PaginaEditor` sulla pagina.
2. Il `Cambiata` esistente diventa `protected override bool Cambiata`.
3. Una riga di markup, **dentro il ramo del modulo**:
   ```razor
   <NavigationLock ConfirmExternalNavigation="@Cambiata" OnBeforeInternalNavigation="GuardaUscita" />
   ```
4. **Togli** `@inject NavigationManager Navigation`. Se scoprissi di dover navigare per
   qualcosa che non è un'uscita da editor, **rimettila**: con la base `private` non produce
   CS0108, e il gate è a 0 avvisi.
5. I `NavigateTo` che seguono `Crea()` ed `Elimina()` — **due** in questo file — diventano
   `Esci(...)`.

**Non aggiungere `@implements IDisposable` alla pagina.** Genererebbe un `Dispose` che
**nasconde** quello della base invece di sovrascriverlo, e la guardia contro la navigazione
tardiva smetterebbe di funzionare **in silenzio**. Se ti serve una pulizia tua:
`public override void Dispose() { base.Dispose(); … }`, e `base.Dispose()` non è
facoltativo.

**Divieto già istruito due volte: non spostare `<NavigationLock>` fuori dai rami
condizionali.** Nel ramo in cui l'elemento è sparito sotto i piedi lo stato del modulo è
ancora «sporco», e la guardia chiederebbe «hai modifiche non salvate» su un elemento che non
esiste più e non si può salvare — una domanda con una sola risposta possibile. Se un
revisore lo propone, adjudica citando questo paragrafo: è già stato verificato dall'unità
03 e riconfermato dalla 04.

## IL GATE DI «CHIUDI»

Oggi «Chiudi» è un `<a href>` che **non guarda `occupato`**, mentre ogni input, «Salva»,
`SchedaConflitto` e `ConfermaAzione` lo guardano. La forma è:

```razor
<a class="btn" href="@(occupato ? null : "<la destinazione attuale dell'href>")">Chiudi</a>
```

**Usa la destinazione che il link ha già**: non cambiarla, non indovinarla.

**Verificato contro il sorgente di ASP.NET Core 10.0.10 — non riverificarlo.**
`RenderTreeBuilder.AddAttribute(int, string, string?)` non aggiunge il frame quando il
valore è `null` e il target non è un componente: l'attributo `href` **non compare affatto**,
e un `<a>` senza `href` non è un link, non naviga, non prende focus.

Tre trappole dalla stessa verifica:

- **Il valore dev'essere letteralmente `null`.** La condizione è `value != null`: la stringa
  vuota **non** viene omessa. Un `?? ""` produrrebbe `href=""`, un link valido verso la
  **radice dell'applicazione** — peggio del difetto di partenza e invisibile in revisione.
- **Non generalizzare ai componenti**: su `<TestataPagina>` e simili `null` non viene mai
  omesso, è un valore legittimo del parametro.
- **Non trasformare «Chiudi» in un `<button>`.** Servirebbe navigare dalla pagina, e l'unica
  navigazione che la base espone è `Esci`, che **disarma la guardia**: quel «Chiudi»
  uscirebbe senza chiedere niente.

**Il selettore `a.btn:not([href])` non è tuo**: è accodato alle voci in attesa per l'unità
11. Fino ad allora il link è funzionalmente inerte ma non spento visivamente. È atteso: non
segnalarlo come difetto e non aggirarlo con stile inline.

## ALTRO SU `ItemEdit.razor`

- **Testata**: `<TestataPagina>` con titolo e `<Aiuto>`. **Guarda come l'hanno fatta
  `NoteEdit` e `CollectionEdit`** e fai uguale. Il titolo è già calcolato per `<PageTitle>`:
  riusalo, non ricalcolarlo.
  Il pannello `<Aiuto>` deve dire ciò che la pagina **non** dice già da sé: l'unità 04 si è
  presa un rilievo fondato per aver ripetuto nell'aiuto una frase che era visibile due righe
  sotto. Leggi la pagina prima di scrivere il testo.
- **L'esito dove si guarda**: sposta il blocco `errore`/`avviso` da sopra il modulo a
  **subito sopra** il blocco delle azioni. Niente barra sticky. Se in questa pagina esiste
  un secondo canale d'esito (per esempio errori di validazione mostrati altrove), **portalo
  nello stesso posto**: è il pezzo che l'unità 04 aveva lasciato aperto e la 05 ha dovuto
  chiudere dopo.

## BUDGET DI COMPLESSITÀ

Nessuna astrazione nuova, nessun tipo nuovo, nessun file `.js`, nessun servizio nuovo.
Questa unità **applica** un contratto che esiste ed è stato provato due volte: se ti trovi a
progettare qualcosa, sei fuori strada.

## STATO

Unità precedenti, tutte `FATTO` e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04
(`3206150`), 05 (`e139ce8`).

**Leggi `handoff/04-collezione-contratto/resoconto.md`**: è il tuo gemello più vicino, ha
fatto esattamente il tuo lavoro su un'altra pagina, e la sua sezione `CONTRATTI` dice come
il contratto si è incastrato. La sezione `DA PROVARE NEL BROWSER` è il modello per la tua.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: se ci trovi una riga che contraddice
questo mandato, vince la più recente.

**Due fatti operativi che ti risparmiano un errore.**

- Le `file:line` di `threat-hunter` sono risultate **sfasate** sui diff delle unità 04 e 05.
  Accogli i suoi verdetti se reggono per contenuto, ma **non riportare un suo numero di riga**
  senza averlo riaperto. Quelle di `bug-hunter` e `conformity` tornano.
- Se un tuo obiettivo e un tuo divieto si contraddicono, **obbedisci al più specifico e
  dichiaralo** nel resoconto. È successo all'unità 05 e l'ha gestito bene: un limite
  dichiarato vale più di una prova data per fatta.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**. Su un file con `@inherits` e
  `override`, `-warnaserror` intercetta il CS0108 che una build permissiva declasserebbe.
- `dotnet test` → tutti verdi. Erano **267** all'ultimo giro.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai.

**Non avviare il server di sviluppo e non provare nel browser.** Lo fa il capo con
`live-testing` quando anche l'unità 07 è rientrata e tutti e quattro gli editor hanno
adottato il contratto.

BUDGET: 20 dollari

RESOCONTO IN: `handoff/06-elemento-contratto/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 06 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <come hai consumato PaginaEditor, con file:line riaperti da te>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti, e a chi appartiene il rimedio>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

Aggiungi `DA PROVARE NEL BROWSER` con i criteri di accettazione. Se una voce coincide con
una già scritta dalle unità 03 o 04, **non ripeterla**: rimandaci e scrivi solo ciò che è
specifico di questa pagina.
