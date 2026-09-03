UNITÀ: 07/11 — L'editor delle spese adotta il contratto

## OBIETTIVO

La stessa cosa che le unità 03, 04 e 06 hanno fatto su `NoteEdit`, `CollectionEdit` e
`ItemEdit`, ora su `Pages/SpesaEdit.razor`. **Sei la quarta e ultima delle gemelle**: il
contratto ha retto tre adozioni **senza una deroga e senza un `BLOCKED`**, e non c'è niente
da progettare.

1. Chiudere l'editor con modifiche non salvate chiede conferma; salvare ed eliminare **non**
   chiedono niente.
2. La pagina ha una testata con titolo e infobutton. Oggi non ce l'ha: **zero occorrenze di
   `TestataPagina`** nel file, verificato dal capo.
3. L'esito del salvataggio compare sopra i pulsanti, non in cima al modulo.
4. «Chiudi» smette di essere premibile mentre una scrittura è in volo.

Dopo di te il capo prova nel browser: sei l'ultima che tiene fermo `live-testing`.

## SEI LA PIÙ PICCOLA DELLE QUATTRO, E IL MOTIVO CONTA

Verificato dal capo con un `grep` sul file, **non** ipotizzato:

- `Pages/SpesaEdit.razor:1` è `@page "/expenses/{Id:guid}"` ed è **l'unica** direttiva
  `@page`. Non esiste una rotta di creazione: le spese si creano altrove. **Non c'è nessun
  `Nuova`**, nessun ramo di creazione, nessun `Crea()`.
- C'è **un solo** `NavigateTo`, a `:386`, e diventerà **un solo** `Esci(...)`. Le gemelle ne
  avevano due.
- Nessun `replace: true` da valutare: quello serviva a non lasciare `/notes/new` nella
  cronologia, e qui `/notes/new` non ha equivalente.

Se trovassi un secondo `NavigateTo` o un ramo di creazione, il capo ha sbagliato la
ricognizione: **dichiaralo in `SCOSTAMENTI`** e trattalo come le gemelle.

**Il caso «errore di caricamento in creazione» non ti riguarda.** Su `ItemEdit` porta al
«Riprova» e sulle altre due no, ma è una differenza che esiste solo perché quelle pagine
hanno una modalità di creazione. Tu non ce l'hai: non cercarlo, non uniformare niente.

## PERIMETRO — file di tua proprietà esclusiva

- `Pages/SpesaEdit.razor`

Un file solo. Se ti servisse toccarne un altro, è un `BLOCKED`.

## NON TOCCARE

- **`Shared/PaginaEditor.cs`**: il contratto. Ha retto su tre pagine, fra cui una da 755
  righe con quattro stati in più della tua. Se pensi vada cambiato, è un `BLOCKED`.
- **`Pages/NoteEdit.razor`, `Pages/CollectionEdit.razor`, `Pages/ItemEdit.razor`**: già fatte
  e committate. Le **leggi** come modello, non le modifichi.
- **`Pages/Spese.razor`**: è il registro, non l'editor. Non è tuo, e non lo sarà di nessuna
  unità di questo piano: lo aprirà un lavoro successivo sulle spese ricorrenti.
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

I cinque passi dell'adozione, gli stessi che hanno funzionato tre volte:

1. `@inherits PaginaEditor` sulla pagina.
2. Il `Cambiata` esistente diventa `protected override bool Cambiata`. **Se su questa pagina
   non esistesse**, va scritto: è l'unico membro che il contratto pretende, e senza di esso
   la classe non compila. Guarda come lo calcolano le gemelle e fai la cosa analoga sui campi
   di questo modulo.
3. Una riga di markup, **dentro il ramo del modulo**:
   ```razor
   <NavigationLock ConfirmExternalNavigation="@Cambiata" OnBeforeInternalNavigation="GuardaUscita" />
   ```
4. **Togli** `@inject NavigationManager Navigation` (`:7`). Se scoprissi di dover navigare per
   qualcosa che non è un'uscita da editor, **rimettila**: con la base `private` non produce
   CS0108, e il gate è a 0 avvisi.
5. Il `NavigateTo` di `:386` diventa `Esci(...)`, **con la destinazione che ha già**.

**Non aggiungere `@implements IDisposable` alla pagina.** Genererebbe un `Dispose` che
**nasconde** quello della base invece di sovrascriverlo, e la guardia contro la navigazione
tardiva smetterebbe di funzionare **in silenzio**. Se ti serve una pulizia tua:
`public override void Dispose() { base.Dispose(); … }`, e `base.Dispose()` non è facoltativo.

**Divieto già istruito tre volte: non spostare `<NavigationLock>` fuori dai rami
condizionali.** Nel ramo in cui la spesa è sparita sotto i piedi lo stato del modulo è ancora
«sporco», e la guardia chiederebbe «hai modifiche non salvate» su una spesa che non esiste più
e non si può salvare — una domanda con una sola risposta possibile. Se un revisore lo propone,
adjudica citando questo paragrafo: verificato dall'unità 03, riconfermato dalla 04 e dalla 06,
e in nessuna delle tre un revisore ha provato a proporlo.

## IL GATE DI «CHIUDI»

Oggi «Chiudi» è `Pages/SpesaEdit.razor:131`, `<a class="btn" href="expenses">`, e **non
guarda `occupato`** mentre gli altri controlli del modulo lo guardano. La forma è:

```razor
<a class="btn" href="@(occupato ? null : "expenses")">Chiudi</a>
```

**Usa il nome della variabile di stato che questa pagina ha davvero**: `occupato` è il nome
sulle gemelle, qui potrebbe chiamarsi altrimenti. Guarda cosa disabilita «Salva» e usa quello.

**I due `<a class="btn" href="expenses">Torna alle spese</a>` di `:19` e `:26` non sono
questo link** e non vanno toccati: sono i rami di errore e di spesa sparita, dove non c'è
nessuna scrittura in volo da proteggere. Le unità 04 e 06 hanno fatto la stessa distinzione.

**Verificato contro il sorgente di ASP.NET Core 10.0.10 — non riverificarlo.**
`RenderTreeBuilder.AddAttribute(int, string, string?)` non aggiunge il frame quando il valore
è `null` e il target non è un componente: l'attributo `href` **non compare affatto**, e un
`<a>` senza `href` non è un link, non naviga, non prende focus.

Tre trappole dalla stessa verifica:

- **Il valore dev'essere letteralmente `null`.** La condizione è `value != null`: la stringa
  vuota **non** viene omessa. Un `?? ""` produrrebbe `href=""`, un link valido verso la
  **radice dell'applicazione** — peggio del difetto di partenza e invisibile in revisione.
- **Non generalizzare ai componenti**: su `<TestataPagina>` e simili `null` non viene mai
  omesso, è un valore legittimo del parametro.
- **Non trasformare «Chiudi» in un `<button>`.** Servirebbe navigare dalla pagina, e l'unica
  navigazione che la base espone è `Esci`, che **disarma la guardia**: quel «Chiudi»
  uscirebbe senza chiedere niente.

**Il selettore `a.btn:not([href])` non è tuo**: è accodato alle voci in attesa per l'unità 11.
Fino ad allora il link è funzionalmente inerte ma non spento visivamente. È atteso: non
segnalarlo come difetto e non aggirarlo con stile inline.

## I TRE COMMENTI STANTII A `:242-243` — sono tuoi, e sono tre

`Pages/SpesaEdit.razor:242-243` porta un commento che addita, come esempi di «scrittura di
stato fra i due `await`», tre righe delle pagine gemelle. **Tutte e tre sono ora sbagliate**,
verificate una per una dall'unità 06 sul file su disco:

| Citata dal commento | Cos'è oggi a quella riga | Chi l'ha spostata |
|---|---|---|
| `NoteEdit.razor:200` | una **riga vuota** | unità 03 |
| `CollectionEdit.razor:309` | un commento sulla **tavolozza di emoji** | unità 04/05 |
| `ItemEdit.razor:214` | la riga cercata è oggi la **226** | unità 06 |

Correggile **tutte e tre**, riaprendo tu i tre file per trovare il numero giusto: due erano
già rotte prima di questo lavoro, e correggerne una sola lascerebbe il commento sbagliato per
due terzi. È l'unico motivo per cui hai il permesso di **leggere** le gemelle oltre che come
modello.

Se una delle tre citazioni non ha più un bersaglio sensato — la riga che descriveva non esiste
più in quella forma — **toglila** invece di puntarla a caso, e dichiaralo.

## ALTRO SU `SpesaEdit.razor`

- **Testata**: `<TestataPagina>` con titolo e `<Aiuto>`. **Guarda come l'hanno fatta le tre
  gemelle** e fai uguale. Se un titolo è già calcolato per `<PageTitle>`, riusalo invece di
  ricalcolarlo: le tre gemelle hanno tutte una proprietà privata con due call-site, stessa
  forma e stessa posizione nel blocco `@code`.
  Il pannello `<Aiuto>` deve dire ciò che la pagina **non** dice già da sé: l'unità 04 si è
  presa un rilievo fondato per aver ripetuto nell'aiuto una frase visibile due righe sotto.
  Leggi la pagina prima di scrivere il testo. Su una spesa i candidati sono i comportamenti
  **invisibili** — cosa succede alle quote quando l'importo cambia, cosa vede l'altro membro,
  cosa comporta la data — non la spiegazione di cosa sia un importo.
- **L'esito dove si guarda**: sposta il blocco `errore`/`avviso` da sopra il modulo a **subito
  sopra** il blocco delle azioni. Niente barra sticky. **Censisci tutti i punti di scrittura e
  tutti i punti di render** dei messaggi d'esito prima di spostare: se esiste un secondo canale
  (per esempio errori di validazione mostrati dentro una scheda), **portalo nello stesso
  posto**. È il pezzo che l'unità 04 aveva lasciato aperto e la 05 ha dovuto chiudere dopo; su
  `ItemEdit` il secondo canale è stato cercato e non c'era. Su questa pagina **non si sa**:
  guarda, e dichiara cosa hai trovato.

## BUDGET DI COMPLESSITÀ

Nessuna astrazione nuova, nessun tipo nuovo, nessun file `.js`, nessun servizio nuovo.
Questa unità **applica** un contratto che esiste ed è stato provato tre volte: se ti trovi a
progettare qualcosa, sei fuori strada.

## STATO

Unità precedenti, tutte `FATTO` e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`),
05 (`e139ce8`), 06 (`f4f2dbd`).

**Leggi `handoff/06-elemento-contratto/resoconto.md`**: è il tuo gemello più recente e il
meglio istruito dei tre. La sua sezione `CONTRATTI` dice come il contratto si incastra, e la
sezione `ADJUDICA` mostra cosa fare quando i revisori tornano tutti a zero — che è l'esito
più probabile anche per te, ed è quello in cui è più facile fermarsi troppo presto.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: se ci trovi una riga che contraddice
questo mandato, vince la più recente.

**Due fatti operativi che ti risparmiano un errore.**

- Le `file:line` di `threat-hunter` sono risultate **sfasate** sui diff delle unità 04 e 05.
  Accogli i suoi verdetti se reggono per contenuto, ma **non riportare un suo numero di riga**
  senza averlo riaperto. Quelle di `bug-hunter` e `conformity` tornano.
- Se un tuo obiettivo e un tuo divieto si contraddicono, **obbedisci al più specifico e
  dichiaralo** nel resoconto. È successo all'unità 05 e l'ha gestito bene: un limite dichiarato
  vale più di una prova data per fatta.

**Se i revisori tornano tutti a zero rilievi, non è finita.** Scrivi comunque la riga di
istruttoria, dichiara che non c'è nessun campione da riverificare perché non è arrivato nessun
infondato, e poi **verifica tu almeno la domanda più rischiosa del tuo diff**. L'unità 06 ne ha
fatte tre e una ha prodotto un caso che nessuno dei tre revisori aveva isolato.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**. Su un file con `@inherits` e
  `override`, `-warnaserror` intercetta il CS0108 che una build permissiva declasserebbe.
- `dotnet test` → tutti verdi. Erano **267** all'ultimo giro.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai: `obj/` non ha
lock fra processi, e due build in parallelo si corrompono a vicenda.

**Non avviare il server di sviluppo e non provare nel browser.** Lo fa il capo con
`live-testing` appena rientri: sei l'ultima delle quattro.

BUDGET: 20 dollari

RESOCONTO IN: `handoff/07-spesa-contratto/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 07 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <come hai consumato PaginaEditor, con file:line riaperti da te>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti, e a chi appartiene il rimedio>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

Aggiungi `DA PROVARE NEL BROWSER` con i criteri di accettazione. **Non ripetere** le prove già
scritte dalle unità 03 (dieci, su `NoteEdit`), 04 (cinque) e 06 (sei): rimandaci e scrivi solo
ciò che è specifico di questa pagina. Le prove generali dell'adozione sono già coperte tre
volte; ciò che nessuno ha ancora provato è come la guardia si comporta su un modulo di **spesa
condivisa**, dove il salvataggio ha effetti che l'altro membro vede.

Aggiungi in fondo una riga sola, per il capo: **`PRONTO PER LIVE-TESTING: sì|no`**, e se «no»
il perché in una frase. È il segnale che fa partire la prova nel browser su tutte e quattro le
pagine.
