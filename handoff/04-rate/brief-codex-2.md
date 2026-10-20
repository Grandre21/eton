# Brief per Codex — unità 04, spese a rate (Eton, Blazor WASM .NET, C#)

Lavori nel worktree `G:\Sviluppo\wt-Eton\04-rate`, ramo `unita-04-rate`. Applica esattamente ciò che segue:
non esplorare oltre i file citati e non prendere decisioni architetturali. Se qualcosa non torna, fermati e
rispondi `BLOCKED: <domanda>`.

## Cosa si costruisce

Un «piano a rate» è una regola ricorrente mensile (`recurring_expenses`) con due colonne in più, già presenti
nel database: `installments` (smallint, null) = numero di rate N, `plan_total` (numeric(12,2), null) = totale.
Vincoli del database: entrambe nulle o entrambe valorizzate; `installments >= 2`; `plan_total >= amount`; con
`installments` valorizzato `every_months = 1`. Per un piano: `every_months = 1`, `day_of_month` = giorno della
data d'inizio, `starts_on` = data della prima rata, `ends_on` = `starts_on` + (N−1) mesi, `amount` = totale/N
arrotondato **per difetto** a 2 decimali. L'occorrenza k si chiama `"<descrizione> · rata k/N"` (il carattere
è il punto mediano U+00B7, con uno spazio prima e uno dopo) e l'ultima (k = N) vale `plan_total − amount × (N−1)`.

## File e modifiche (solo questi)

### 1. `Models/RecurringExpense.cs`
Dopo `MaterializedThrough` (riga 33), due proprietà nello stile delle vicine, con un commento di una riga:
```csharp
// Solo per i piani a rate, entrambe nulle per le altre regole (check del database).
[Column("installments")] public int?     Installments { get; set; }
[Column("plan_total")]   public decimal? PlanTotal    { get; set; }
```
Nessun `ignoreOnUpdate`: sono concesse in INSERT e UPDATE. Non aggiungere `Kind` (è di un'altra unità).

### 2. `Services/RecurringExpenseRepository.cs`
- `DatiRegola` (riga 11) acquista due parametri **opzionali in coda**, così i call-site esistenti restano
  identici: `public sealed record DatiRegola(decimal Importo, string Descrizione, string Categoria, int OgniMesi, int Giorno, DateTime Inizio, int? Rate = null, decimal? TotalePiano = null);`
  Aggiorna il `<summary>` con una frase: Rate e TotalePiano valorizzati = piano a rate (v. `CalcoliRicorrenti.DatiPiano`).
- `CreaAsync` (riga 49): nell'inizializzatore aggiungi `Installments = dati.Rate`, `PlanTotal = dati.TotalePiano`
  e cambia `EndsOn = null` in `EndsOn = dati.Rate is int n ? ExpenseRepository.PerIlDatabase(CalcoliRicorrenti.FinePiano(dati.Inizio, n)) : null`.
- `SalvaAsync` (riga 78): la catena diventa una variabile `var query = client.From<RecurringExpense>().Where(...).Set(...)...;`
  con gli stessi sei `Set` di oggi; poi **solo se** `dati.Rate is int n`, aggiungi
  `.Set(r => r.Installments!, n)`, `.Set(r => r.PlanTotal!, dati.TotalePiano!.Value)` e
  `.Set(r => r.EndsOn!, ExpenseRepository.PerIlDatabase(CalcoliRicorrenti.FinePiano(dati.Inizio, n)))`
  (stessa forma di `TerminaAsync`, riga 101); poi `var risposta = await query.Update();`. Commento di una riga:
  senza Rate non si tocca ends_on, altrimenti salvare una regola terminata la riaprirebbe. Se il tipo del
  builder rende scomoda la variabile, usa `var` e basta; non introdurre helper.
- Nient'altro in questo file.

### 3. `Services/CalcoliRicorrenti.cs`
- Enum (riga 20): `public enum GruppoRicorrenti { Mensili, ARate, Annuali, OgniNMesi, Terminate }` — il nome
  `OgniNMesi` resta (è un contratto con un'altra unità); aggiorna il summary (riga 18-19): «A rate» ora c'è.
- Un record nuovo accanto agli altri in testa al file:
  `/// <summary>Lo stato di un piano a rate attivo: il numero della prossima rata, il numero di rate e quanto resta da pagare, prossima compresa.</summary>`
  `public sealed record StatoPiano(int Prossima, int Rate, decimal Residuo);`
- Metodi statici nuovi, nella classe, dopo `Occorrenza` (stile: expression-bodied dove sta in una riga, `<summary>` di una o due frasi in italiano come i vicini):
  - `public static decimal ImportoRata(decimal totale, int rate) => Math.Floor(totale * 100 / rate) / 100;` (per difetto)
  - `public static DateTime FinePiano(DateTime inizio, int rate) => inizio.Date.AddMonths(rate - 1);`
    (AddMonths taglia all'ultimo giorno del mese come fa `Dovuti` con `Math.Min`: lo dice il summary)
  - `public static DatiRegola DatiPiano(decimal totale, int rate, string descrizione, string categoria, DateTime inizio)`
    → `new(ImportoRata(totale, rate), descrizione, categoria, OgniMesi: 1, Giorno: inizio.Day, inizio, Rate: rate, TotalePiano: totale)`.
    Il summary dice che è l'unico punto che traduce totale e numero di rate in una regola, usato dal Registro e dall'editor.
  - `public static int NumeroRata(RecurringExpense r, DateTime data) => (data.Year * 12 + data.Month) - (r.StartsOn.Year * 12 + r.StartsOn.Month) + 1;`
  - `public static StatoPiano? Stato(RecurringExpense r, DateTime oggi)`: null se `r.Installments is null`
    o se `Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, oggi)` è null; altrimenti
    `k = NumeroRata(r, prossima)`, `Residuo = r.PlanTotal!.Value - r.Amount * (k - 1)`, `new StatoPiano(k, r.Installments.Value, residuo)`.
    Nota: `Prossima` cerca dal giorno dopo oggi, quindi una rata che cade oggi conta come già pagata: va bene, è la stessa regola del gruppo.
  - `public static bool PianoPartito(RecurringExpense r, DateTime oggi) => r.MaterializedThrough is not null || r.StartsOn.Date <= oggi.Date;`
    Summary: partito = almeno una rata segnata, oppure la data della prima è già arrivata (una rata dovuta ma non ancora scritta da chi paga conta come partita: il passato non si riscrive).
- `Occorrenza` (riga 143): resta l'unico punto del calcolo. Per un piano (`r.Installments is int n`):
  `k = NumeroRata(r, d.Data)`; `Description = $"{r.Description} · rata {k}/{n}"`;
  `Amount = k == n ? r.PlanTotal!.Value - r.Amount * (n - 1) : r.Amount`. Per le altre regole invariato.
  Scrivilo con un corpo a blocco e due variabili locali, senza helper; aggiorna il summary con una frase.
- `Gruppo` (riga 194): dopo il controllo `Terminate`, prima dello `switch`:
  `if (r.Installments is not null) return GruppoRicorrenti.ARate;`
- `QuotaDaAccantonare` (riga 222): diventa la **somma dei totali di gruppo già arrotondati** di `Annuali` e `OgniNMesi`:
  `=> Raggruppa(regole, oggi).Where(g => g.Gruppo is GruppoRicorrenti.Annuali or GruppoRicorrenti.OgniNMesi).Sum(g => g.TotaleMensile!.Value);`
  Summary: così la riga della quota torna al centesimo con le testate dei gruppi.
- Nient'altro in questo file.

### 4. `Pages/Ricorrenti.razor` (solo questo)
- `Titolo` (riga 119): aggiungi `GruppoRicorrenti.ARate => "A rate",` dopo Mensili, e `OgniNMesi => "Periodiche"`.
- Nella riga dell'elenco (righe 86-103), dentro `<span class="meta">`, **dopo** lo `<span>` della prossima data
  (dentro il ramo `else`, cioè solo per regole non terminate): se `CalcoliRicorrenti.Stato(r, oggi) is { } piano`,
  `<span>rata <span class="dato">@piano.Prossima/@piano.Rate</span></span>` e
  `<span>residuo <span class="dato">@Denaro.Testo(piano.Residuo)&#160;€</span></span>`. Nessuna classe CSS nuova.
- Nient'altro: niente CSS, niente altre righe.

### 5. `Pages/RicorrenteEdit.razor` — editor di un piano esistente
Piano = `regola?.Installments is not null` (proprietà `private bool Piano => !Nuova && regola?.Installments is not null;`).
Una regola nuova da questo editor non è mai un piano. Le regole non-piano devono restare **identiche** a oggi.

Campi nuovi in `@code`: `private string totaleTesto = ""; private int rate = 2;`, valorizzati da
`regola.PlanTotal`/`regola.Installments` negli **stessi quattro punti** dove oggi si valorizza `importoTesto`
dalla regola (`OnParametersSetAsync` ramo lettura, `SalvaCon` caso Salvata, `Ricarica`): usa
`Denaro.TestoDigitabile(regola.PlanTotal ?? 0)` e `regola.Installments ?? 2`. Aggiungi
`private bool Bloccato => Piano && CalcoliRicorrenti.PianoPartito(conflitto ?? regola!, DateTime.Today);`.

Markup, quando `Piano`:
- al posto del blocco Importo (righe 63-82): un campo «Totale del piano» (`totaleTesto`, stesso markup
  dell'importo, disabilitato se `occupato || !PuoIntervenire || Bloccato`, e con `Testi.MessaggioImporto(Denaro.Verifica(totaleTesto, out _))`
  come errore-campo); poi un campo «Numero di rate» con `<select @bind="rate">` da 2 a 60 (stesso stile del
  select del giorno, righe 112-117), stesso `disabled`; se il totale è valido e `CalcoliRicorrenti.ImportoRata(totale, rate) < 0.01m`
  un `<p class="errore-campo">Diviso in @rate rate fa meno di un centesimo a rata.</p>`; altrimenti, con totale
  valido, `<p class="spiega">@rate rate da @Denaro.Testo(ImportoRata) €, l'ultima prende i centesimi avanzati.</p>`.
- i blocchi «Ogni quanto» e «Giorno del mese» (righe 90-119) non si mostrano (sono derivati).
- il campo data mantiene il markup ma l'etichetta è «Prima rata» invece di «Dal», e `disabled` include `Bloccato`.
- descrizione e categoria restano modificabili come oggi.
- se `Bloccato`: subito sopra i campi, `<p class="spiega">Il piano è partito: si possono cambiare solo descrizione e categoria, oppure terminarlo se l'hai estinto prima.</p>`.
- la sezione «Fermare questa spesa ricorrente» (righe 193-227): per un piano **non** bloccato il blocco Termina
  non si mostra (prima della prima rata si elimina); per un piano bloccato resta com'è. Elimina invariato.
- la riga «Le spese già segnate non cambiano.» e il resto invariati.

Logica, quando `Piano`:
- `Valido`: per un piano bloccato basta descrizione non vuota; per un piano non bloccato
  `Denaro.Prova(totaleTesto, out var t) && ImportoRata(t, rate) >= 0.01m && descrizione non vuota && DataRagionevole(inizio)`.
- `Cambiata` (ramo regola esistente): per un piano confronta `totaleTesto != Denaro.TestoDigitabile(regola.PlanTotal ?? 0)`,
  `rate != regola.Installments`, `descrizione`, `categoria`, `inizio?.Date != regola.StartsOn.Date`; non importo/cadenza/giorno.
- Il salvataggio passa da **un solo** metodo che costruisce i dati, usato da `Salva` e `Sovrascrivi`
  (oggi entrambi fanno `Denaro.Prova(importoTesto, ...)` e poi `SalvaCon(versione, importo)`): cambia `SalvaCon` in
  `SalvaCon(int versione, DatiRegola dati)` e aggiungi `private DatiRegola? DatiDaSalvare()` che restituisce null se non valido:
  - regola non-piano: `Denaro.Prova(importoTesto, out var i)` e `DataRagionevole(inizio)` → `new DatiRegola(i, descrizione, categoria, ogniMesi, giorno, inizio!.Value)` (come oggi);
  - piano bloccato: `new DatiRegola(r.Amount, descrizione, categoria, r.EveryMonths, r.DayOfMonth, r.StartsOn)` con `r = conflitto ?? regola!` — senza Rate, così il repository non tocca ends_on (un piano terminato in anticipo resta terminato);
  - piano non bloccato: `Denaro.Prova(totaleTesto, out var t)` e `DataRagionevole(inizio)` → `CalcoliRicorrenti.DatiPiano(t, rate, descrizione, categoria, inizio!.Value)`.
  In `Salva` il ramo `Nuova` continua a chiamare `Crea(importo)` come oggi. Il messaggio d'errore di `Sovrascrivi` resta.
- Nient'altro.

### 6. `Pages/Spese.razor` — solo il modulo di inserimento (righe 60-108) e il suo `@code`
- `@inject RecurringExpenseRepository Ricorrenti` dopo gli altri `@inject`.
- Campi: `private bool aRate; private int numeroRate = 3; private string? avvisoPiano;`
- Markup, dentro `<div class="campi-spesa">`, dopo il blocco «Quando» (riga 93): l'interruttore con il pattern
  di `Pages/CollectionEdit.razor:124-134` (`<label class="interruttore">`, checkbox `@bind="aRate"`, `disabled="@occupato"`,
  titolo «A rate», spiega «Diventa un piano fra le ricorrenti: una rata al mese dalla data qui sopra, e l'ultima prende i centesimi avanzati.»).
  Se `aRate`: un campo «Numero di rate» con `<select @bind="numeroRate" disabled="@occupato">` da 2 a 60, e sotto, se
  l'importo è valido, o `<p class="errore-campo">Diviso in @numeroRate rate fa meno di un centesimo a rata.</p>` (se `ImportoRata < 0.01m`)
  o `<p class="spiega">@numeroRate rate da @Denaro.Testo(...) €</p>`.
- Con `aRate` acceso l'etichetta «Importo» diventa «Totale» e «Quando» diventa «Prima rata» (stesso markup, testo condizionale).
- Dopo `<div class="azioni">`: `@if (avvisoPiano is not null) { <div class="avviso" role="status">@avvisoPiano <a href="expenses/recurring">Vai alle ricorrenti</a></div> }`.
- `ModuloValido` (riga 272): aggiungi `&& (!aRate || CalcoliRicorrenti.ImportoRata(importo, numeroRate) >= 0.01m)` (riscrivila per avere l'importo in mano, senza helper nuovi).
- `Segna()` (riga 308): `avvisoPiano = null;` accanto a `errore = null;`; ripeti la guardia del centesimo dopo le tre guardie
  esistenti; poi al posto della sola riga `var creata = await Repository.CreaAsync(...)` + `ultimaSpesaCreataId = creata.Id;`:
  ```csharp
  if (aRate)
  {
      await Ricorrenti.CreaAsync(attivo.Id, pagante, CalcoliRicorrenti.DatiPiano(importo, numeroRate, nuovaDescrizione, nuovaCategoria, nuovaData!.Value));
      avvisoPiano = $"Piano a rate creato: {numeroRate} rate da {Denaro.Testo(CalcoliRicorrenti.ImportoRata(importo, numeroRate))} €.";
      aRate = false;
  }
  else
  {
      var creata = await Repository.CreaAsync(...);   // la riga di oggi, invariata
      ultimaSpesaCreataId = creata.Id;
  }
  ```
  Il resto di `Segna` invariato (svuotamento dei campi, `rimettiFuocoImporto`, `await Carica()` che materializza subito la prima rata se è di oggi o passata). Il messaggio del catch resta.
  Commento di una riga sopra l'`if`: con «a rate» non nasce una spesa ma una regola, e la prima rata la scrive la rilettura del mese.
- Nient'altro in questa pagina (riepilogo, elenco, frecce: non toccare).

### 7. Test — `Eton.Tests/PianiARateTests.cs` (nuovo) e `Eton.Tests/GruppiRicorrentiTests.cs`
TEST NUOVO: Eton.Tests/PianiARateTests.cs
TEST NUOVO: GruppiRicorrentiTests.Quota_somma_i_totali_di_gruppo_arrotondati
Stile di `Eton.Tests/GruppiRicorrentiTests.cs` (xUnit, `[Fact]`, nomi in italiano con underscore, helper `Regola(...)` privato). In `PianiARateTests`:
- `ImportoRata_per_difetto`: 100/3 → 33.33m; 0.05/2 → 0.02m; 0.01/2 → 0m.
- `DatiPiano_compone_la_regola`: totale 100, 3 rate, inizio 2026-11-15 → Importo 33.33, OgniMesi 1, Giorno 15, Inizio, Rate 3, TotalePiano 100; e `FinePiano(2026-11-15, 3)` = 2027-01-15; `FinePiano(2027-01-31, 2)` = 2027-02-28.
- `Occorrenza_suffisso_e_resto_sull_ultima`: piano 100/3 dal 2026-11-15 (Amount 33.33, PlanTotal 100, Installments 3, DayOfMonth 15, EveryMonths 1, Description «Divano»): occorrenza del 2026-11-15 → «Divano · rata 1/3», 33.33; del 2027-01-15 → «Divano · rata 3/3», 33.34. Una regola non-piano → descrizione e importo invariati.
- `Materializzazione_e_previsione_coincidono`: con `Dovuti` + `Occorrenza` sul piano sopra da 2026-11-01 a 2027-03-31, tre occorrenze, somma importi = 100m, descrizioni rata 1/3, 2/3, 3/3.
- `Gruppo_a_rate_e_fuori_dalla_quota`: piano attivo → `GruppoRicorrenti.ARate`; piano con EndsOn passato → Terminate; `Raggruppa` con una mensile, un piano e un'annuale dà l'ordine Mensili, ARate, Annuali; `QuotaDaAccantonare` con piano + annuale 120 = 10m.
- `Stato_prossima_rata_e_residuo`: piano 100/3 dal 2026-10-15, oggi 2026-10-20 → Prossima 2, Rate 3, Residuo 66.67m; oggi 2026-10-10 → Prossima 1, Residuo 100m; regola non-piano → null.
- `PianoPartito`: MaterializedThrough null e inizio domani → false; inizio oggi → true; watermark valorizzato → true.
In `GruppiRicorrentiTests`, test nuovo `Quota_somma_i_totali_di_gruppo_arrotondati`: annuale 100 + trimestrale 100 → 41.66m (8.33 + 33.33).
Esegui i test **prima** del fix: per ogni TEST NUOVO ricopia in `VERIFICA` una riga
`test rosso  <nome>: <riga di fallimento ricopiata dall'output>` (un errore del compilatore vale). Poi il fix, poi riesegui.

## Cosa NON toccare
`supabase/`, `Models/Expense.cs`, `Pages/Home.razor`, `Pages/SpeseTabella.razor`, `Services/TabellaSpese.cs`,
`Services/ExpenseRepository.cs`, `docs/`, `handoff/` (salvo il tuo file `-o`), qualunque `.css`. Nessun file
fuori da quelli elencati.

## Budget di complessità
Nessuna astrazione nuova oltre al record `StatoPiano`; nessun helper con un solo call-site oltre a quelli
elencati; nessuna classe CSS nuova; nessun JavaScript. Commenti nella densità dei vicini, in italiano.

## Verifica
`dotnet build` e `dotnet test` nel worktree (`G:\Sviluppo\wt-Eton\04-rate\Eton.Tests`); il GATE lo rifà la
sessione con `mcp__synapse__build root=<worktree>`. Non avviare Synapse.Server. Non avviare l'applicazione.

## Formato del ritorno
`FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/04-rate/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

In più, su Windows: non scrivere file con `Set-Content`, `Out-File`, `[System.IO.File]::Write*` o altri comandi PowerShell; i file si modificano solo con apply_patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.

## Risposta alla tua domanda (BLOCKED del primo lancio)

Il file `-o` lo scrive da sé la riga di comando con cui sei stato lanciato (`-o handoff/04-rate/codex-2.md`): non devi crearlo né scriverci. Il tuo resoconto è semplicemente il tuo **messaggio finale**, nel formato `FATTO / DEVIAZIONI / VERIFICA / NOTE`. Procedi con tutto il brief.
