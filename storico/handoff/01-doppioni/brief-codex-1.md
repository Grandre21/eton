# Brief — i doppioni della lettura a blocchi

Lavori nel worktree `G:\Sviluppo\wt-Eton\01-doppioni` (progetto Blazor .NET, test xUnit in `Eton.Tests/`).

## Il problema

`Services/ExpenseRepository.cs:87-106`, `ElencaAsync`, legge le spese a blocchi da `RighePerBlocco` (1000,
riga 32) con `.Range(spese.Count, spese.Count + RighePerBlocco - 1)`. Se un altro membro inserisce una spesa
fra due blocchi, l'offset si sposta e la stessa spesa (stesso `Id`, `Models/Expense.cs:23`, `Guid`) torna
due volte nella lista restituita. Rimedio deciso: in fondo alla lettura le righe si deduplicano per `Id`, la
**prima occorrenza vince**, l'ordine delle righe resta quello di lettura. Il caso opposto (una riga
*saltata*) resta accettato e non si tocca.

## Cosa fai, in quest'ordine

1. **Estrai il ciclo** in un metodo nuovo, `internal static`, in `ExpenseRepository`, subito sopra
   `ElencaAsync`:

       internal static async Task<IReadOnlyList<Expense>> LeggiABlocchiAsync(Func<int, int, Task<IReadOnlyList<Expense>>> blocco)

   `blocco(da, a)` restituisce le righe dell'intervallo `[da, a]` (estremi inclusi, come `.Range`). Il
   ciclo è identico a quello di oggi: offset = numero di righe lette finora (**duplicati compresi**: è
   l'offset del database, non il numero di spese distinte), stop al primo blocco con meno di
   `RighePerBlocco` righe. `ElencaAsync` diventa: prende il client, poi
   `return await LeggiABlocchiAsync(async (da_, a_) => (await client.From<Expense>()…Range(da_, a_).Get(default, Constants.CountType.Exact)).Models);`
   con la stessa catena di `.Where/.Filter/.Order` di oggi, invariata. Scegli tu nomi dei parametri della
   lambda che non collidano con `da`/`a` del metodo. In questo passo **nessuna deduplicazione**.
2. **TEST NUOVO** in `Eton.Tests/ExpenseRepositoryTests.cs` (stesso file, stessa classe, stessa forma dei
   test esistenti: `[Fact]`, nomi in italiano con underscore, un commento sopra che dice il perché):
   `Una_spesa_ripetuta_fra_due_blocchi_torna_una_volta_sola`. Un `blocco` finto: il primo blocco ha
   `RighePerBlocco` (1000) spese con `Id` distinti; il secondo blocco contiene di nuovo l'ultima spesa del
   primo (stesso `Id`) più 2 spese nuove, quindi è corto e chiude il ciclo. Asserisci: 1002 righe, `Id`
   tutti distinti, l'ordine è quello di lettura, e la spesa ripetuta compare nella posizione della sua
   prima occorrenza. Il 1000 nel test non va scritto a mano due volte: se `RighePerBlocco` è `private
   const`, rendila `internal const` (unica modifica di visibilità ammessa). Un secondo test, stesso file:
   `Sotto_un_blocco_la_lettura_resta_quella_di_prima` — 3 spese distinte in un solo blocco corto, tornano
   le stesse 3 nello stesso ordine, e `blocco` è stato chiamato una volta sola con `(0, 999)`.
3. **Esegui i test sul codice del passo 1** (senza deduplicazione): `dotnet test` nel worktree, filtrato su
   `ExpenseRepositoryTests`. Il primo test deve **fallire**. Ricopia in `VERIFICA` una riga a formato fisso:

       test rosso  Una_spesa_ripetuta_fra_due_blocchi_torna_una_volta_sola: <riga di fallimento ricopiata dall'output>

   Se passa, fermati: `BLOCKED:` con il motivo.
4. **Il fix**: in `LeggiABlocchiAsync`, al ritorno, deduplica per `Id` tenendo la prima occorrenza
   (`DistinctBy(e => e.Id).ToList()` va bene: conserva l'ordine e la prima). Riesegui: tutti i test verdi.
5. **Il commento di `ElencaAsync`** (righe 43-45, «Una spesa inserita o eliminata…»): riscrivilo perché dica
   il vero dopo il fix — una riga *ripetuta* fra due blocchi ora si scarta per `Id` (la prima vince); una
   riga *saltata* resta possibile in quella sola lettura, oltre 1000 spese nel periodo, e si accetta. Stesso
   registro e stessa lunghezza del commento che sostituisci. Il resto del commento non si tocca.

## Cosa NON toccare

Solo `Services/ExpenseRepository.cs` e `Eton.Tests/ExpenseRepositoryTests.cs`. Non toccare `supabase/`,
`Pages/`, `Shared/`, `Models/`, `Services/CalcoliRicorrenti.cs`, `docs/`, `handoff/`. Nessuna firma pubblica
cambia: `public async Task<SpeseDelPeriodo> ElencaConPrevisteAsync(Guid spazioId, DateTime da, DateTime a)`
(riga 113) resta identica, e `ElencaAsync` resta `private`.

## Budget di complessità

Nessun tipo nuovo, nessuna astrazione nuova oltre a `LeggiABlocchiAsync` (che esiste per il test: ha un
call-site in produzione e uno nei test). Nessuna interfaccia o mock di `SupabaseService`. Nessun pacchetto
NuGet nuovo. Diff atteso: ~15 righe in `ExpenseRepository.cs`, ~50 nei test.

## Verifica

`dotnet build` e `dotnet test` nel worktree (il gate finale lo rilancia la sessione). Non avviare
Synapse.Server.

## Formato del ritorno

`FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <motivo>`. In `VERIFICA` la riga `test rosso` del
passo 3 e il riepilogo dell'ultimo `dotnet test` (superati / falliti).

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/01-doppioni/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
