# Brief 2 — entrate: le pagine (unità 05 di Eton)

Lavori nel worktree `G:\Sviluppo\wt-Eton\05-entrate` (Blazor WebAssembly .NET). Il brief 1 è già fatto e
committato: esistono `TipoMovimento` (`Spesa = "expense"`, `Entrata = "income"`, `Testo(tipo)`) e
`CategorieSpesa.Entrate`/`Tutte`/`Di(tipo)`/`Conosciuta(categoria, tipo)` in `Services/CategorieSpesa.cs`;
`Expense.Kind` e `RecurringExpense.Kind`; `ExpenseRepository.CreaAsync(..., DateTime data, string tipo)`;
`DatiRegola(..., string Kind = TipoMovimento.Spesa)`; in `Services/CalcoliSpese.cs` il record `Bilancio(Spese,
Entrate)` con `Saldo`, `CalcoliSpese.BilancioDi(movimenti)`, `RiepilogoMese.Entrate`/`.Saldo` (`Totale` è il
totale delle sole spese) e `CalcoliSpese.ConSegno(importo)` («+1.500,00», «−20,50», «0,00»); in
`Services/TabellaSpese.cs` `FiltriSpese.Tipo` (null = entrambi), il parametro URL `tipo`, `Colonne.Tipo`; in
`Services/CalcoliRicorrenti.cs` il gruppo `GruppoRicorrenti.Entrate`. **Leggi quelle firme nel codice prima di
usarle**: se una differisce da qui, vale il codice e lo scrivi in `DEVIAZIONI`.

Questo brief porta tutto nelle pagine. La resa è **provvisoria**: nessun CSS nuovo, nessun file `.css` toccato,
solo le classi che esistono già (`pastiglia`, `accesa`, `scelta-categoria`, `etichetta-campo`, `campo`,
`campo-lettura`, `spiega`, `testo-tenue`, `dato`, `griglia-gruppo-somma`, `griglia-nota`, `griglia-insieme`).

## 1. `Pages/Spese.razor` — il Registro

1. Campo `private string nuovoTipo = TipoMovimento.Spesa;` accanto a `nuovaCategoria` (riga ~296).
2. In testa a `<section class="modulo-spesa">` (riga 59), **prima** di `<div class="campi-spesa ...">`, una scelta
   «Spesa / Entrata» con lo stesso markup del gruppo Categoria (righe 131-139): `<fieldset class="scelta-categoria">`,
   `<legend class="etichetta-campo">Tipo</legend>`, due `<button type="button" class="pastiglia ...">` con
   `aria-pressed`, `disabled="@occupato"`, che chiamano un metodo `ScegliTipo(string tipo)`. «Spesa» preselezionata.
3. `ScegliTipo`: imposta `nuovoTipo`; se è `Entrata` spegne `aRate` (`aRate = false`); se `nuovaCategoria` non è
   `CategorieSpesa.Conosciuta(nuovaCategoria, tipo)` la porta a `CategorieSpesa.Di(tipo)[0]`.
4. L'interruttore «A rate» (righe 96-102) e tutto il blocco `@if (aRate)` che segue si rendono **solo** se
   `nuovoTipo == TipoMovimento.Spesa`.
5. Le pastiglie della categoria (riga 134) iterano `CategorieSpesa.Di(nuovoTipo)`.
6. `Segna` (riga 402): `Repository.CreaAsync(..., nuovaData!.Value, nuovoTipo)`. Il ramo `aRate` resta com'è (con
   «Entrata» `aRate` è sempre falso). Dopo «Segna» il tipo resta quello scelto, come la categoria; aggiorna in
   mezza riga il commento «Importo e descrizione si svuotano, data e categoria restano» perché nomini anche il tipo.
7. Il riepilogo del mese: subito sotto `<p class="totale-mese ...">` (riga 186) e prima di `confronto-mese`, una
   riga `<p class="testo-tenue">Entrate <span class="dato">@CalcoliSpese.ConSegno(riepilogo.Entrate)&#160;€</span> · Saldo <span class="dato">@CalcoliSpese.ConSegno(riepilogo.Saldo)&#160;€</span></p>`.
   Il totale grande resta quello delle spese; barre e confronto restano solo spese (già così da `PerMese`).
8. Le righe del registro (righe 228-252): per un'entrata (`s.Kind == TipoMovimento.Entrata`) una
   `<span class="pastiglia">Entrata</span>` come **prima** voce di `.meta`, e l'importo con il «+» davanti:
   `+@Denaro.Testo(s.Amount)`. Le spese restano identiche.
9. La testata dell'elenco (riga 214): `Testi.Conteggio(speseDelMese.Count, "movimento", "movimenti")`.
10. L'`<Aiuto>` (righe 11-15) acquista un paragrafo: «Un'entrata — lo stipendio, un rimborso — si segna scegliendo
    «Entrata». Non entra nel totale delle spese né nelle categorie: conta nelle entrate e nel saldo del mese.»

## 2. `Pages/Home.razor` — la striscia delle spese (righe 117-146)

- La condizione del «niente» diventa `riepilogoSpese.Totale == 0m && riepilogoSpese.Entrate == 0m`; il testo resta
  «Nessuna spesa questo mese» se non c'è nessun movimento.
- Dopo lo span del totale (riga 130) e prima di `CimeSpese`, uno span:
  `<span>entrate <span class="dato">@CalcoliSpese.ConSegno(riepilogoSpese.Entrate)&#160;€</span> · saldo <span class="dato">@CalcoliSpese.ConSegno(riepilogoSpese.Saldo)&#160;€</span></span>`.
- `CimeSpese` resta sulle sole categorie delle spese (già così). Nient'altro nella pagina.

## 3. `Shared/Griglia.razor` — solo il riepilogo

Un parametro nuovo, accanto a `NotaRiepilogo` (riga 197):

```csharp
    /// <summary>Voci di riepilogo calcolate dalla pagina sulle righe che contano nei totali — le mostrate, o la
    /// selezione — al posto della somma per colonna: per esempio Spese, Entrate e Saldo. Null = solo le
    /// aggregazioni delle colonne.</summary>
    [Parameter] public Func<IReadOnlyList<T>, IReadOnlyList<(string Etichetta, string Valore)>>? VociRiepilogo { get; set; }
```

- `tfoot` (riga 129): si rende se `Colonne.Any(c => c.Aggregazioni != Aggregazione.Nessuna) || VociRiepilogo is not null`.
- Subito dopo la riga `griglia-insieme` (righe 136-138), se `VociRiepilogo` non è null, una riga
  `<tr><td colspan="@ColonneTotali">` con, per ogni voce, `<span class="griglia-gruppo-somma">@Etichetta <span class="dato">@Valore</span></span>`,
  e dopo le voci, se `NotaRiepilogo` non è null, `<span class="griglia-nota">@NotaRiepilogo</span>`. Le voci si
  calcolano una volta per render su `righeTotali` (la lista già filtrata da `ContaNeiTotali` e dalla selezione).
- La testata di un gruppo (righe 58-62): se `VociRiepilogo` non è null, al posto dei subtotali per colonna rende le
  voci calcolate su `gruppo.Righe.Where(ContaNeiTotali).ToList()`, con lo stesso markup
  `griglia-gruppo-somma`; altrimenti resta com'è.
- Nient'altro in questo file: ordinamento, modifica, selezione, tastiera ed esportazione non si toccano.

## 4. `Pages/SpeseTabella.razor` — la tabella

1. `CostruisciColonne` (riga 403):
   - `Importo`: niente più `Aggregazioni` (la somma unica non ha senso su spese ed entrate insieme).
   - nuova colonna, subito dopo `Importo`:
     `new(TabellaSpese.Colonne.Tipo, "Tipo", TipoColonna.Testo, r => TipoMovimento.Testo(r.Spesa.Kind))` — non
     modificabile (il tipo non cambia dopo la creazione), ordinabile.
   - `Categoria`: `Modificabile = r => modificabile(r) && r.Spesa.Kind == TipoMovimento.Spesa`, `Opzioni` resta
     `CategorieSpesa.Elenco`. La categoria di un'entrata si cambia dall'editor della singola riga, non dalla
     tabella: una colonna ha un solo elenco di opzioni. Scrivilo in un commento di una riga.
2. Alla `<Griglia>` (riga 223) passa `VociRiepilogo="Bilancio"`, con
   `private IReadOnlyList<(string, string)> Bilancio(IReadOnlyList<RigaSpesa> righe)` che usa
   `CalcoliSpese.BilancioDi(righe.Select(r => r.Spesa))` e restituisce
   `("Spese", Denaro.Testo(b.Spese) + " €")`, `("Entrate", CalcoliSpese.ConSegno(b.Entrate) + " €")`,
   `("Saldo", CalcoliSpese.ConSegno(b.Saldo) + " €")` (con lo spazio indivisibile `\u00A0` al posto dello spazio
   prima di «€», come `&#160;` nel markup). `NotaRiepilogo="@NotaPreviste()"` resta.
3. Filtro «Tipo»: nella `<fieldset class="strumenti-riga" data-tutorial="filtri">`, prima del gruppo delle
   categorie, un `<label class="campo">` con `<span class="etichetta-campo">Tipo</span>` e un `<select>` con le
   opzioni `Tutti` (valore vuoto), `Spese` (`TipoMovimento.Spesa`), `Entrate` (`TipoMovimento.Entrata`), legato a
   `vista.Filtri.Tipo` con un metodo `CambiaTipo(ChangeEventArgs e)` sul modello di `CambiaPagante` (riga 497).
4. Le pastiglie del filtro categorie (riga 119) iterano `CategorieSpesa.Tutte`.
5. Azioni di massa (righe 162-189): calcola il tipo della selezione dalle righe selezionate (`righe` con
   `selezione.Contains(r.Chiave)`).
   - Se la selezione contiene **sia** spese **sia** entrate: niente select «Nuova categoria» né pulsante «Cambia
     categoria»; al loro posto `<p class="testo-tenue">La selezione ha spese ed entrate: la categoria si cambia a un tipo alla volta.</p>`.
     «Elimina» resta.
   - Altrimenti il select offre `CategorieSpesa.Di(tipo)`, e il valore effettivo è una proprietà
     `CategoriaMassa` che vale `categoriaMassa` se è conosciuta per quel tipo, altrimenti `CategorieSpesa.Di(tipo)[0]`:
     il select usa `value="@CategoriaMassa"` e `@onchange` che scrive `categoriaMassa`; `CambiaCategoria`
     (riga 625) passa `CategoriaMassa` al repository. Così un valore rimasto da una selezione di spese non può
     finire su un'entrata.
6. Testi:
   - `<Aiuto>` riga 19: «Ogni riga è una spesa o un'entrata.» al posto di «Ogni riga è una spesa.», e un paragrafo
     in più: «Il riepilogo in fondo dà spese, entrate e saldo; le entrate non si sommano alle spese.»
   - Il passo `riepilogo` del tutorial (riga 251): «In fondo, spese, entrate e saldo delle righe mostrate, o della
     selezione se ce n'è una. Le previste contano, perché la spesa c'è già; le in arrivo no, perché non sono ancora dovute.»
   - Il passo `filtri` (riga 241): «Tipo, categorie, pagante, testo e stato restringono le righe mostrate. …» (il resto
     invariato).
   - «Nessuna spesa in questo periodo con questi filtri.» (riga 220) → «Nessun movimento in questo periodo con questi filtri.»

## 5. `Pages/SpesaEdit.razor` — l'editor di un movimento esistente

- Una proprietà `private string Tipo => spesa?.Kind ?? TipoMovimento.Spesa;`.
- In testa a `<div class="campi-spesa">` (riga 68), un campo in sola lettura col markup di `campo-lettura` delle
  righe 95-100: `<div class="campo campo-lettura"><span class="etichetta-campo">Tipo</span><p>@TipoMovimento.Testo(Tipo)</p></div>`,
  seguito da `<p class="spiega">Il tipo non si cambia: per farne una spesa o un'entrata, eliminala e segnala di nuovo.</p>`.
- Le pastiglie della categoria (riga 135) iterano `CategorieSpesa.Di(Tipo)`, e la pastiglia in più (riga 145) usa
  `CategorieSpesa.Conosciuta(categoria, Tipo)`.
- `TitoloSchermata` (riga 237): il ripiego «Spesa» diventa `TipoMovimento.Testo(Tipo)`.

## 6. `Pages/RicorrenteEdit.razor` — l'editor delle regole

- Campo `private string tipo = TipoMovimento.Spesa;` accanto a `categoria` (riga 295); nel ramo `Nuova` di
  `OnParametersSetAsync` (riga 365) torna a `TipoMovimento.Spesa`; nel ramo della regola letta (riga ~395)
  `tipo = regola.Kind`.
- **Regola nuova**: prima di `<div class="campi-spesa campi-ricorrente">` (riga 62) la stessa scelta «Spesa /
  Entrata» del Registro (fieldset `scelta-categoria`, legenda «Tipo», due pastiglie), con un `ScegliTipo` che
  imposta `tipo` e riallinea `categoria` a `CategorieSpesa.Di(tipo)[0]` se non è conosciuta per quel tipo.
  **Regola esistente**: al suo posto il campo in sola lettura «Tipo» con il markup `campo campo-lettura` (come al §5)
  e nessuna frase in più.
- Le pastiglie della categoria (riga 191) iterano `CategorieSpesa.Di(tipo)`; la pastiglia in più (riga 196)
  usa `CategorieSpesa.Conosciuta(categoria, tipo)`.
- `Crea` (riga 455): `new DatiRegola(importo, descrizione, categoria, ogniMesi, giorno, inizio!.Value, Kind: tipo)`.
- `Cambiata`, ramo `Nuova` (riga 332): aggiungi `|| tipo != TipoMovimento.Spesa`.
- `TitoloSchermata` (riga 315): con descrizione vuota, «Nuova entrata ricorrente» / «Nuova spesa ricorrente» per una
  nuova e «Entrata ricorrente» / «Spesa ricorrente» per una esistente, secondo `tipo`.
- I piani a rate non cambiano: sono sempre spese (vincolo del database), e la scelta del tipo c'è solo per una regola
  nuova, che dall'editor non è mai un piano.

## 7. `Pages/Ricorrenti.razor`

- Il ramo `Entrate` di `Titolo` c'è già dal brief 1.
- Nella riga di una regola (riga 106) l'importo di un'entrata ha il «+» davanti; nella testata del gruppo
  `Entrate` (riga 78) anche il totale: `+X € al mese`.
- `<Aiuto>` (righe 15-18) acquista: «Le entrate ricorrenti, come lo stipendio, stanno in un gruppo a sé: non entrano
  nei totali delle spese né nella quota da mettere da parte.»
- Lo stato vuoto (riga 65): «L'affitto, una bolletta, un abbonamento, lo stipendio: la dichiari una volta, e compare
  da sola nel registro nel mese in cui è dovuta.»

## Budget di complessità

Nessun tipo nuovo, nessun componente nuovo, nessun CSS. Nella Griglia un solo parametro nuovo. Un helper con un solo
call-site va inline, salvo `ScegliTipo` (uno per pagina, dove serve) e `Bilancio`/`CategoriaMassa` nella tabella.
Commenti in italiano, solo dove una scelta non è ovvia, densità come i vicini (le pagine sono molto commentate: non
serve imitarle in lunghezza).

## Non toccare

`supabase/`, `docs/`, `handoff/` (salvo il tuo file `-o`), `wwwroot/`, i file `.css` e `.razor.css`, `Services/`,
`Models/`, `Eton.Tests/` salvo che un test non compili più per colpa di questo brief (scrivilo in `DEVIAZIONI`),
ogni file non nominato qui. Non avviare l'app e non avviare Synapse.Server.

## Verifica

`dotnet build` e `dotnet test` nel worktree: 0 errori, 0 test falliti. Il GATE lo rilancia la sessione con
`mcp__synapse__build root=<worktree>`.

## Formato del ritorno

Il tuo messaggio finale, in quattro sezioni: `FATTO` (cosa, per file), `DEVIAZIONI` (ogni scostamento da questo
brief col perché, e per ogni script o suite che lanci il comando esatto), `VERIFICA` (esito di build e test con i
conteggi ricopiati), `NOTE`. Oppure una riga sola `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.
Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/05-entrate/resoconto.md`.
(Il file `-o` non lo crei tu: è il tuo **messaggio finale**, che la riga di comando salva da sé in
`handoff/05-entrate/codex-2.md`. Non creare file chiamati `-o`.)
Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.
Non scrivere file con `Set-Content`, `Out-File` o `[System.IO.File]::Write*`: modifica i file con le tue patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
