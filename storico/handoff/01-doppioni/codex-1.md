FATTO
Estratto LeggiABlocchiAsync, con offset sulle righe lette, duplicati compresi. Deduplicazione finale per Id: prima occorrenza e ordine di lettura conservati. Aggiunti i due test richiesti e aggiornate solo le tre righe del commento. Query e firme pubbliche invariate; ElencaAsync resta private.

DEVIAZIONI
Nessuna deviazione funzionale. Resoconto nel file letterale -o del worktree.
Comandi eseguiti (PowerShell, dalla radice del worktree):
- Get-Location; rg --files -g AGENTS.md -g '*ExpenseRepository*' -g '*.csproj' -g '*AssemblyInfo*' -g '*InternalsVisible*' -g '*sln*'
- git status --short; Get-Content Services/ExpenseRepository.cs; Get-Content Eton.Tests/ExpenseRepositoryTests.cs
- dotnet test --filter FullyQualifiedName~ExpenseRepositoryTests
- dotnet build
- dotnet test
- git diff --check; git diff --stat; git diff -- Services/ExpenseRepository.cs Eton.Tests/ExpenseRepositoryTests.cs
Modifiche ai file applicate con apply_patch, senza interpreti inline.

VERIFICA
test rosso  Una_spesa_ripetuta_fra_due_blocchi_torna_una_volta_sola: Assert.Equal() Failure: Values differ
Prima del fix: Expected: 1002; Actual: 1003. Test filtrati: 5 superati, 1 fallito.
dotnet build: compilazione completata, 0 avvisi, 0 errori.
Ultimo dotnet test: Superato! - Non superati: 0. Superati: 392. Ignorati: 0. Totale: 392.
git diff --check: nessun errore.

NOTE
Restano accettate le righe saltate durante modifiche concorrenti. Synapse.Server non avviato. I due file già presenti in handoff/01-doppioni non sono stati modificati.
