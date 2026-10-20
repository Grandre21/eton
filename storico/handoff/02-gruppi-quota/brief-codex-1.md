Brief per l'autore del diff — Eton, unità 02: ricorrenti divise in gruppi e quota da accantonare

Lavori nel worktree G:\Sviluppo\wt-Eton\02-gruppi-quota (Blazor WebAssembly .NET, test xUnit in Eton.Tests/).
Non esplorare il codebase oltre ai file citati qui: tutto ciò che ti serve è sotto.

## Obiettivo

La pagina Ricorrenti (Pages/Ricorrenti.razor) oggi mostra un solo elenco di regole, nell'ordine in cui
arrivano dal database. Deve mostrarle divise in gruppi, in quest'ordine: Mensili, Annuali, Ogni N mesi,
Terminate; ciascun gruppo con il proprio totale mensile (le Terminate senza totale), le regole ordinate
per descrizione dentro il gruppo, i gruppi vuoti non mostrati. In testa una riga
«Per annuali e periodiche, metti da parte X € al mese». Il calcolo è puro e testato.

Più avanti altre due unità aggiungeranno i gruppi «Entrate» (prima di Mensili) e «A rate» (dopo Mensili):
il raggruppamento deve accoglierli aggiungendo un membro all'enum e un ramo alla classificazione, senza
riscrivere il resto. Non aggiungerli tu.

## File da toccare (solo questi)

1. `Services/CalcoliRicorrenti.cs` — aggiungi, senza toccare nulla di esistente:

   - accanto ai due record in testa al file (righe 6-16, `PeriodoDovuto` e `SpeseDelPeriodo`, stesso stile
     di commento `/// <summary>`):
     ```csharp
     /// <summary>I gruppi della pagina Ricorrenti. L'ordine di dichiarazione è l'ordine in pagina: i gruppi
     /// futuri (Entrate prima di Mensili, A rate dopo) si inseriscono al loro posto.</summary>
     public enum GruppoRicorrenti { Mensili, Annuali, OgniNMesi, Terminate }

     /// <summary>… <c>TotaleMensile</c> è null per le Terminate, che stanno fuori dai totali.</summary>
     public sealed record GruppoDiRicorrenti(GruppoRicorrenti Gruppo, IReadOnlyList<RecurringExpense> Regole,
         decimal? TotaleMensile);
     ```
   - dentro `CalcoliRicorrenti`, dopo `Fondi` (riga 182), tre metodi pubblici e uno privato:
     ```csharp
     public static GruppoRicorrenti Gruppo(RecurringExpense r, DateTime oggi)
     public static IReadOnlyList<GruppoDiRicorrenti> Raggruppa(IReadOnlyList<RecurringExpense> regole, DateTime oggi)
     public static decimal QuotaDaAccantonare(IReadOnlyList<RecurringExpense> regole, DateTime oggi)
     private static decimal QuotaMensile(RecurringExpense r) => r.Amount / r.EveryMonths;
     ```
     Semantica:
     - `Gruppo`: `Terminate` se `Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, oggi)` è null
       (lo stesso criterio che la pagina usa oggi, `Pages/Ricorrenti.razor:117-118`); altrimenti
       `EveryMonths == 1` → `Mensili`, `== 12` → `Annuali`, ogni altro valore → `OgniNMesi`. Usa uno
       `switch` (o una catena di `if`) in cui un ramo nuovo si aggiunga con una riga.
     - `Raggruppa`: raggruppa per `Gruppo(r, oggi)`, ordina i gruppi per valore dell'enum, omette quelli
       vuoti. Dentro il gruppo ordina per descrizione senza accenti e senza maiuscole, poi per `Id` per
       stabilità. Il confronto si fa come in `Services/CalcoliGriglia.cs:32-34`:
       `StringComparer.Ordinal` su `SchemaCampi.RiduciAccenti(testo).ToUpperInvariant()` (il progetto
       gira con `InvariantGlobalization=true`: niente confronti culture-aware).
       `TotaleMensile` = `null` per `Terminate`; per gli altri la somma di `QuotaMensile` delle regole del
       gruppo, arrotondata con `Math.Round(somma, 2, MidpointRounding.AwayFromZero)`. Le regole non ancora
       partite contano (si mette da parte prima).
     - `QuotaDaAccantonare`: somma di `QuotaMensile` delle regole il cui `Gruppo` è `Annuali` o
       `OgniNMesi`, arrotondata come sopra. Definita sul gruppo, così Entrate e A rate, quando arriveranno,
       ne restano fuori da sé.
     Commenti: uno `/// <summary>` per metodo pubblico, in italiano, breve, come quelli già nel file.

2. `Pages/Ricorrenti.razor` — solo il ramo `else` delle righe 63-93 e il blocco `@code`:
   - subito dopo `<p class="sottotitolo">…</p>` (riga 45), e solo quando la quota è > 0 e i dati sono
     stati letti (`!datiNonLetti && regole.Count > 0`), una riga:
     `<p class="quota">Per annuali e periodiche, metti da parte <strong class="dato">@Denaro.Testo(quota)&#160;€</strong> al mese</p>`
   - al posto del `testa-registro` unico con il conteggio (righe 65-67) e dell'unico `<ul>` (68-92): un
     `@foreach (var g in gruppi)` che produce
     ```razor
     <section class="gruppo-ricorrenti">
         <div class="testa-registro">
             <span>@Titolo(g.Gruppo)</span>
             @if (g.TotaleMensile is { } totale)
             {
                 <span class="totale dato">@Denaro.Testo(totale)&#160;€ al mese</span>
             }
         </div>
         <ul class="registro elenco-ricorrenti"> … il <li> di oggi, identico, per ogni r in g.Regole … </ul>
     </section>
     ```
     Il markup del `<li>` (righe 71-90) resta com'è, compreso `riga-terminata` e la pastiglia «terminata».
   - nel `@code`: campi `gruppi` (`IReadOnlyList<GruppoDiRicorrenti>`, inizializzato `[]`) e `quota`
     (`decimal`), calcolati in `Leggi` subito dopo `ElencaAsync` con `DateTime.Today` (e azzerati
     insieme a `regole = []` in testa a `Leggi`); un `private static string Titolo(GruppoRicorrenti g)`
     con uno `switch` di espressione: Mensili → "Mensili", Annuali → "Annuali", OgniNMesi → "Ogni N mesi",
     Terminate → "Terminate". Il metodo `Prossima` resta.

3. `Pages/Ricorrenti.razor.css` — file nuovo (CSS isolation, il progetto la usa già: `Pages/Benvenuto.razor.css`).
   Solo token esistenti di `wwwroot/css/app.css`, nessun colore o misura letterale:
   ```css
   .gruppo-ricorrenti + .gruppo-ricorrenti { margin-top: var(--s6); }
   /* Il totale sta sopra la colonna degli importi; maiuscolo e spaziatura larga della testata lo
      renderebbero illeggibile come cifra. */
   .totale { margin-left: auto; text-transform: none; letter-spacing: 0; }
   .quota { margin: 0 0 var(--s6); color: var(--testo-tenue); }
   .quota strong { color: var(--testo); font-weight: 600; }
   ```

4. `Eton.Tests/GruppiRicorrentiTests.cs` — file nuovo, stile di `Eton.Tests/CalcoliRicorrentiTests.cs`
   (`using Eton.Services;`, `namespace Eton.Tests;`, `[Fact]`, nomi in italiano con underscore). Con un
   helper privato che costruisce una `RecurringExpense` (modello in `Models/RecurringExpense.cs`: `Id`,
   `Amount`, `Description`, `EveryMonths`, `DayOfMonth`, `StartsOn`, `EndsOn`). Usa un `oggi` fisso,
   es. `new DateTime(2026, 10, 20)`. Casi:
   - classificazione: mensile → Mensili, ogni 12 → Annuali, ogni 3 → OgniNMesi, una con `EndsOn` passato
     → Terminate, una con `EndsOn` futuro e occorrenze ancora davanti → non Terminate;
   - ordine dei gruppi Mensili, Annuali, OgniNMesi, Terminate, e un gruppo senza regole non compare;
   - ordine per descrizione dentro il gruppo ignorando maiuscole e accenti («bolletta», «Affitto»,
     «Ècole» → Affitto, bolletta, Ècole);
   - totali: annuale 120 → 10,00; ogni 3 mesi 30 + ogni 2 mesi 50 → 35,00; annuale 100 → 8,33;
     Terminate → null;
   - quota: annuale 120 + ogni 3 mesi 30 + mensile 500 + annuale terminata 1200 + annuale che parte il
     mese prossimo 240 → 10 + 10 + 20 = 40,00;
   - quota con sole mensili → 0.

TEST NUOVO: Eton.Tests/GruppiRicorrentiTests.cs

Scrivi il test PRIMA del codice di produzione, eseguilo sul codice di prima (non compilerà: va bene, è
il suo fallimento), e nel tuo `VERIFICA` ricopia una riga a formato fisso:

    test rosso  GruppiRicorrentiTests: <riga di errore ricopiata dall'output del compilatore o del test>

Poi scrivi il codice e riesegui.

## Cosa NON toccare

`supabase/`, `Models/`, `Services/ExpenseRepository.cs`, `Services/RecurringExpenseRepository.cs`,
`Pages/Spese.razor`, `Pages/Home.razor`, `Pages/SpeseTabella.razor`, `Pages/RicorrenteEdit.razor`,
`wwwroot/css/app.css`, `docs/`, `handoff/`. Le firme esistenti di `CalcoliRicorrenti` (`Cadenza`, `Fondi`,
`Prossima`, `Dovuti`, `Occorrenza`) restano identiche.

## Budget di complessità

Due tipi nuovi (l'enum e il record), nessun altro. Nessuna interfaccia, nessun servizio iniettato,
nessuna classe nuova in `Services/`. Nessun helper con un solo call-site fuori da `QuotaMensile`.

## Verifica

`dotnet build` e `dotnet test` nel worktree (Eton.sln). Non avviare Synapse.Server. Il gate finale lo
rilancia chi ti ha dato il brief.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/<NN-slug>/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

## Formato di ritorno

Nel file `-o`, in quest'ordine: `FATTO` (cosa hai fatto, file per file) oppure `BLOCKED: <domanda>`;
`DEVIAZIONI`; `VERIFICA` (con la riga `test rosso`); `NOTE`.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
