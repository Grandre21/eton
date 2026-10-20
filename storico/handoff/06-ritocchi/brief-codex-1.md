# Brief per Codex — unità 06, ritocchi

Progetto Blazor WebAssembly .NET (Eton). Lavori nel worktree `G:\Sviluppo\wt-Eton\06-ritocchi`. Due ritocchi
piccoli e indipendenti, nello stesso diff. Nessun comportamento cambia salvo i cinque testi della parte A quando
la regola è un'entrata.

## A — Testi dell'editor che seguono il tipo (`Pages/RicorrenteEdit.razor`)

L'editor di una regola ricorrente ha un campo `tipo` (`TipoMovimento.Spesa` = "expense" o `TipoMovimento.Entrata`
= "income", riga 313; caricato da `regola.Kind` alla riga 426) e già un membro per il nome singolare, riga 340:

```csharp
    // «spesa» o «entrata», per i testi della pagina che nominano il tipo della regola.
    private string NomeTipo => TipoMovimento.Testo(tipo).ToLowerInvariant();
```

1. Subito sotto, aggiungi il plurale, con un commento di una riga nello stesso stile:

```csharp
    // «spese» o «entrate», per i testi che parlano dei movimenti già segnati.
    private string NomeTipoPlurale => tipo == TipoMovimento.Entrata ? "entrate" : "spese";
```

2. Sostituisci «spese» con `@NomeTipoPlurale` in **queste cinque righe e in nessun'altra** (testo attuale →
   testo nuovo; con tipo Spesa il testo reso resta identico a oggi, carattere per carattere):

- riga 200: `Se la data è già passata, le spese dei mesi trascorsi si segnano da sole: controlla che sia quella giusta.`
  → `Se la data è già passata, le @NomeTipoPlurale dei mesi trascorsi si segnano da sole: controlla che sia quella giusta.`
- riga 245: `Le spese già segnate non cambiano.` → `Le @NomeTipoPlurale già segnate non cambiano.`
- riga 267: `Terminandola, da domani non segna più spese. Le spese già segnate restano.`
  → `Terminandola, da domani non segna più @NomeTipoPlurale. Le @NomeTipoPlurale già segnate restano.`
- riga 278: `Terminata il @Testi.DataSola(regola!.EndsOn!.Value): dopo quella data non segna più spese. Le spese già segnate restano.`
  → `Terminata il @Testi.DataSola(regola!.EndsOn!.Value): dopo quella data non segna più @NomeTipoPlurale. Le @NomeTipoPlurale già segnate restano.`
- riga 297: `Non si può eliminare: ha già segnato delle spese, e cancellarla toglierebbe loro la provenienza. Terminarla basta a fermarla.`
  → `Non si può eliminare: ha già segnato delle @NomeTipoPlurale, e cancellarla toglierebbe loro la provenienza. Terminarla basta a fermarla.`

Attenzione Razor: `@NomeTipoPlurale.` seguito da punto e spazio va bene (il punto non è un identificatore valido
dopo); `@NomeTipoPlurale,` idem. Se il compilatore protesta, usa `@(NomeTipoPlurale)`.

**Non toccare** la riga 274 (piano a rate: è sempre una spesa) né nessun altro testo che dice «spesa»/«spese»
(righe 14, 15, 27, i messaggi d'errore in `@code`, la riga 610): non fanno parte di questo lavoro.

## B — Overload `Prossima(RecurringExpense r, DateTime oggi)` (`Services/CalcoliRicorrenti.cs`)

Oggi `Prossima` ha una forma sola, riga 90:

```csharp
    public static DateTime? Prossima(
        DateTime inizio, DateTime? fine, int ogniMesi, int giorno, DateTime oggi)
```

e quattro chiamanti la invocano con i cinque argomenti presi dalla stessa regola.

1. Subito dopo la fine di quel metodo (dopo la riga 103 circa, la `}` che chiude `Prossima`), aggiungi:

```csharp
    /// <summary>La prossima occorrenza di una regola: v. l'overload sui campi.</summary>
    public static DateTime? Prossima(RecurringExpense r, DateTime oggi)
        => Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, oggi);
```

   La forma a cinque argomenti **resta** e non cambia (la usa `Eton.Tests/CalcoliRicorrentiTests.cs:100` e la
   chiama il nuovo overload).

2. Usa l'overload nei chiamanti:
- `Services/CalcoliRicorrenti.cs:194` (in `Stato`): `Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, oggi)` → `Prossima(r, oggi)`
- `Services/CalcoliRicorrenti.cs:245` (in `Gruppo`): stessa sostituzione.
- `Pages/RicorrenteEdit.razor:262`: `CalcoliRicorrenti.Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, DateTime.Today)` → `CalcoliRicorrenti.Prossima(r, DateTime.Today)`
- `Pages/Ricorrenti.razor`: **togli** il wrapper privato alle righe 155-158 (il commento di due righe e il metodo
  `private DateTime? Prossima(RecurringExpense r)`), e alla riga 86 scrivi
  `var prossima = CalcoliRicorrenti.Prossima(r, oggi);`. Il commento della riga 85
  `// null = regola terminata (v. Prossima in @code)` diventa
  `// null = regola terminata; 'oggi' è lo stesso giorno dei gruppi` (il wrapper non c'è più).
  Togli la riga vuota che resterebbe in fondo al blocco `@code` se ne lascia una doppia.

`Dovuti` e ogni altro metodo non si toccano.

## Budget di complessità

Un overload nuovo e una proprietà nuova; nessun tipo nuovo, nessun file nuovo, nessun test nuovo, nessun altro
helper. Commenti in italiano, solo quelli indicati.

## Non toccare

`supabase/`, `docs/`, `handoff/` (salvo il tuo file `-o`), `Models/`, `Services/ExpenseRepository.cs`, `wwwroot/`,
ogni `.css` e `.razor.css`, `Eton.Tests/`, ogni file non nominato qui. Non avviare l'app e non avviare
Synapse.Server.

## Verifica

`dotnet build` e `dotnet test` nel worktree: 0 errori, 0 test falliti. Il GATE lo rilancia la sessione con
`mcp__synapse__build root=<worktree>`.

## Formato del ritorno

Il tuo messaggio finale, in quattro sezioni: `FATTO` (cosa, per file), `DEVIAZIONI` (ogni scostamento da questo
brief col perché, e per ogni script o suite che lanci il comando esatto), `VERIFICA` (esito di build e test con i
conteggi ricopiati), `NOTE`. Oppure una riga sola `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.
Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/06-ritocchi/resoconto.md`.
(è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`)
Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.
Non scrivere file con `Set-Content`, `Out-File` o `[System.IO.File]::Write*`: modifica i file solo con apply_patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
