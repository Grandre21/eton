# Brief codex-2 — correzioni dopo la revisione (C1, C2, C3, BE2, BE4)

Stesso worktree `G:\Sviluppo\wt-Eton\03-residui`, sul diff che hai appena scritto. Quattro correzioni,
nessun cambio di comportamento.

1. **`Services/ExpenseRepository.cs`, `CambiaCategoriaAsync`** (C1, BE2). Togli il ternario con
   `Enumerable.Empty<Guid[]>()` dalla sorgente del `foreach` (`:370`). Prima del ciclo, con un commento di
   due righe sul perché:
   ```csharp
   // Categoria che nessun tipo ammette: nessun UPDATE (un `kind=in.()` vuoto filtrerebbe male), e la
   // rilettura sotto smista tutta la selezione fra rifiutate e sparite.
   IReadOnlyCollection<Guid> daAggiornare = tipi.Count > 0 ? richieste : [];
   var kindAmmessi = tipi.Cast<object>().ToList();
   ```
   poi `foreach (var blocco in daAggiornare.Chunk(IdPerRichiesta))` come i vicini (`:402`, `:417`) e
   `.Filter("kind", Constants.Operator.In, kindAmmessi)` dentro. Se `[]` non compila nel ternario, usa la
   forma più vicina che compila e dillo in `DEVIAZIONI`.

2. **`Eton.Tests/RecurringExpenseRepositoryTests.cs`, test delle colonne** (C2). Togli `"kind"` dall'array
   (`:97`): è già coperto da `Eton.Tests/PrivilegiInsertTests.cs:136-145`. Nel commento del test una
   riga che lo dica.

3. **`Services/RecurringExpenseRepository.cs`, `NuovaRegola`** (C3). Sopra la dichiarazione (`:57`) una
   riga di commento nello stile di `Services/ExpenseRepository.cs:34`: separata da `CreaAsync` per provare
   senza database come i dati diventano riga (date a mezzanotte UTC, fine del piano, testi senza spazi).

4. **`Eton.Tests/RecurringExpenseRepositoryTests.cs`, il test di `NuovaRegola`** (BE4). Spezza
   `NuovaRegola_prepara_i_dati_senza_spostare_le_date` in tre `[Fact]`, uno scenario ciascuno, con i dati
   comuni come campi `static readonly` della classe (spazio, pagante, inizio, `DatiRegola`):
   - `NuovaRegola_copia_i_dati_rifila_i_testi_e_porta_la_data_a_mezzanotte_UTC` — regola senza rate:
     testi rifilati, `StartsOn` a mezzanotte UTC, `Kind`, `MaterializedThrough` null, `SpaceId`, `PaidBy`,
     `Amount`, `EveryMonths`, `DayOfMonth`, e `EndsOn`/`Installments`/`PlanTotal` null;
   - `NuovaRegola_con_rate_calcola_la_fine_del_piano_in_UTC` — `EndsOn` = `FinePiano(inizio, 3)` a
     mezzanotte UTC, `Installments` 3, `PlanTotal` 360;
   - `NuovaRegola_genera_un_id_nuovo_a_ogni_chiamata` — due id diversi e non vuoti.
   Nessun `foreach` dentro un test. Gli assert restano gli stessi di oggi, solo distribuiti.

NON TOCCARE nient'altro: in particolare `Services/CategorieSpesa.cs`, `Services/RisultatoSalvataggio.cs`,
gli altri repository, `supabase/**`, `Pages/**`, `Shared/**`, `docs/**`, `handoff/**`.

Budget di complessità: nessun tipo nuovo, nessun helper nuovo.

## Verifica

`dotnet build -m:1` e `dotnet test Eton.Tests/Eton.Tests.csproj` nel worktree (il GATE lo rilancia la
sessione con synapse). Non avviare Synapse.Server. Non avviare l'app. Non avviare Docker né Supabase.

## Ritorno

Formato: `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <motivo>`.

I file si creano e si modificano solo con le tue patch. Vietati gli interpreti con codice inline (`python -`, `python -c`, `node -e`, here-string passate a un interprete) e vietato scrivere file da PowerShell: niente comandi `powershell.exe -Command` né script `.ps1` usati per creare o riscrivere file, con `Set-Content`, `Out-File`, `Add-Content`, `[System.IO.File]::Write*` o sostituzioni a regex; leggere, compilare e lanciare i test da PowerShell resta permesso. Se una modifica non si può fare con una patch, fermati e scrivi `BLOCKED:` col motivo; se l'hai fatta in un altro modo, scrivi in `DEVIAZIONI` il comando esatto e i file che ha toccato.

Il tuo resoconto è il tuo messaggio finale: Codex lo salva da solo in `G:\Sviluppo\wt-Eton\03-residui\handoff\03-residui\codex-2.md` quando finisci (è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`). Non scrivere tu quel file, non creare altri file di resoconto e non scrivere in `handoff/03-residui/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
