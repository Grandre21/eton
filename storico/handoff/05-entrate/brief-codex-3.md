# Brief 3 — correzioni dopo la revisione (unità 05 di Eton)

Worktree `G:\Sviluppo\wt-Eton\05-entrate`. I brief 1 e 2 sono committati (c191eea, cf40a76). Qui solo le
correzioni elencate, ciascuna con il rilievo da cui nasce. Nessun cambiamento di comportamento oltre a quello
detto in ciascun punto.

## A. `TipoMovimento` va nei modelli (backend-expert BE1 del brief 1, conformity C1)

Oggi sta in `Services/CategorieSpesa.cs` e costringe `Models/` a dipendere da `Services/` (`using Eton.Services;`
in `Models/Expense.cs`, nome qualificato in `Models/RecurringExpense.cs:22`). Sposta la classe **identica**
(costanti, `Testo`, commento) in fondo a `Models/Expense.cs`, namespace `Eton.Models`. Togli `using Eton.Services;`
da `Models/Expense.cs`; in `Models/RecurringExpense.cs` il default diventa `TipoMovimento.Spesa`. Nessun file
nuovo. Aggiungi `using Eton.Models;` (o `@using Eton.Models` nelle pagine) solo dove il compilatore lo chiede
(per esempio `Eton.Tests/CategorieSpesaTests.cs`, `Services/CategorieSpesa.cs` se non ce l'ha).

## B. `Services/CalcoliSpese.cs`

- **PerMese** (backend-expert BE2 del brief 1): la distinzione spesa/entrata si decide una volta. Forma:
  ```csharp
  var delMese = lista.Where(s => s.SpentOn.Year == anno && s.SpentOn.Month == mese).ToList();
  var speseDelMese = delMese.Where(s => s.Kind != TipoMovimento.Entrata).ToList();
  var totale = speseDelMese.Sum(s => s.Amount);
  var entrate = delMese.Where(s => s.Kind == TipoMovimento.Entrata).Sum(s => s.Amount);
  var totalePrecedente = lista
      .Where(s => s.SpentOn.Year == annoPrecedente && s.SpentOn.Month == mesePrecedente && s.Kind != TipoMovimento.Entrata)
      .Sum(s => s.Amount);
  // categorie da speseDelMese, senza un altro Where
  ```
  `PerMese` non chiama più `BilancioDi`, che resta per la tabella.
- **ConSegno** (backend-expert BE4 del brief 1): `switch` espressione con `> 0`, `< 0`, `_` al posto del ternario
  annidato. Stesso risultato.

## C. `Services/CategorieSpesa.cs` (backend-expert BE3 del brief 1)

`Tutte = [.. Elenco.Union(Entrate)]` al posto di `Where` + `Contains`. Stesso risultato.

## D. Test (conformity C2 e C3 del brief 1)

- In `GruppiRicorrentiTests.cs`, `TabellaSpeseTests.cs`, `PrivilegiInsertTests.cs`, `CalcoliSpeseTests.cs`,
  `FusioneRicorrentiTests.cs`: i test aggiunti dal brief 1 si spostano **dopo** i campi, le costanti e gli helper
  privati che aprono la classe, come negli omologhi. Contenuto invariato.
- Nei test aggiunti dal brief 1 e negli helper che hanno acquistato il parametro `tipo`, usa
  `TipoMovimento.Spesa` / `TipoMovimento.Entrata` al posto di `"expense"` / `"income"`. **Eccezione**:
  `PrivilegiInsertTests.Il_tipo_si_scrive_solo_in_creazione` può lasciare `"kind"` come nome di colonna.

## E. `Pages/SpeseTabella.razor` — tipo della selezione (bug-hunter BH1 e conformity C1 del brief 2, backend-expert BE1 del brief 2)

**Difetto**: riga 194 `righe.First(r => selezione.Contains(r.Chiave))`. Cambiando periodo con delle righe
selezionate, `Leggi` svuota `righe` (riga ~330) mentre `selezione` è ancora piena; nel render intermedio il blocco
delle azioni di massa arriva a `First` su una sequenza vuota e lancia: la pagina si rompe. Prima del brief 2 quel
blocco leggeva solo `selezione.Count`.

**Correzione**: un solo membro calcola il tipo della selezione e lo usano i tre punti (righe 185, 194, 649):
```csharp
    // null = selezione vuota, mista (spese ed entrate) o con chiavi che non sono più fra le righe lette.
    private string? TipoSelezione
    {
        get
        {
            var tipi = righe.Where(r => selezione.Contains(r.Chiave)).Select(r => r.Spesa.Kind).Distinct().ToList();
            return tipi.Count == 1 ? tipi[0] : null;
        }
    }
```
Nel markup: `@if (TipoSelezione is not { } tipoSelezione)` → la frase «La selezione ha spese ed entrate: la
categoria si cambia a un tipo alla volta.» **solo se** la selezione ha davvero due tipi; se `TipoSelezione` è null
per selezione non ritrovata fra le righe (rilettura in corso), non mostrare né la frase né il select né il
pulsante «Cambia categoria». Scegli la forma più semplice che dà questi tre casi senza mai chiamare `First`/`Single`.
`CategoriaMassa` usa `TipoSelezione`; `CambiaCategoria` non parte se `TipoSelezione` è null. Nessuna espressione
del tipo della selezione resta fuori da `TipoSelezione`.

Anche (checker, nota): l'etichetta di conferma dell'eliminazione (riga ~204) diventa
`Testi.Conteggio(selezione.Count, "movimento", "movimenti")`.

## F. Il «+» delle entrate passa da `ConSegno` (conformity C2 e backend-expert BE3 del brief 2)

In `Pages/Spese.razor:267` e `Pages/Ricorrenti.razor:79,107` il ternario `"+" : ""` davanti a `Denaro.Testo`
diventa: per un'entrata `CalcoliSpese.ConSegno(importo)` (gli importi delle entrate sono sempre > 0, quindi dà
«+…»), per una spesa `Denaro.Testo(importo)` come prima. Un'espressione per punto, niente helper nuovo.

## G. `Pages/RicorrenteEdit.razor` (backend-expert BE4 e BE5 del brief 2, conformity C3)

- `TitoloSchermata` (riga ~343): niente ternario annidato. Con descrizione vuota:
  `Nuova ? $"Nuova {TipoMovimento.Testo(tipo).ToLowerInvariant()} ricorrente" : $"{TipoMovimento.Testo(tipo)} ricorrente"`,
  in un getter con `if` per la descrizione. Stesso testo di oggi.
- `ScegliTipo` (righe 313-317) esce dall'elenco dei campi: va dopo i campi, accanto agli altri metodi del modulo,
  separato da righe vuote.

## H. Rientri (conformity C3 e backend-expert BE2 del brief 2)

Rientra di un livello il contenuto dei blocchi `@if`/`else` aggiunti dal brief 2 in `Pages/Spese.razor` (blocco
`@if (nuovoTipo == TipoMovimento.Spesa)`, righe ~109-141), `Pages/SpeseTabella.razor` (ramo `else` delle azioni di
massa, righe ~189-201, che il punto E riscrive comunque) e `Shared/Griglia.razor` (ramo `else` della testata del
gruppo, righe ~70-73). Solo spazi.

## Budget di complessità

Nessun tipo nuovo, nessun file nuovo, nessun helper oltre a `TipoSelezione`. Nessun CSS. Commenti in italiano,
uno per decisione non ovvia.

## Non toccare

`supabase/`, `docs/`, `handoff/` (salvo il tuo file `-o`), `wwwroot/`, i file `.css`, ogni file non nominato qui.
Non avviare l'app e non avviare Synapse.Server.

## Verifica

`dotnet build` e `dotnet test` nel worktree: 0 errori, 0 test falliti (oggi 417). Il GATE lo rilancia la sessione
con `mcp__synapse__build root=<worktree>`.

## Formato del ritorno

Il tuo messaggio finale, in quattro sezioni: `FATTO` (per lettera A-H), `DEVIAZIONI` (ogni scostamento col perché,
e per ogni script o suite che lanci il comando esatto), `VERIFICA` (esito di build e test con i conteggi
ricopiati), `NOTE`. Oppure una riga sola `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.
Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/05-entrate/resoconto.md`.
(Il file `-o` non lo crei tu: è il tuo **messaggio finale**, che la riga di comando salva da sé in
`handoff/05-entrate/codex-3.md`. Non creare file chiamati `-o`.)
Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.
Non scrivere file con `Set-Content`, `Out-File` o `[System.IO.File]::Write*`: modifica i file con le tue patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
