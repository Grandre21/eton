FATTO

Implementati modello, calcoli, repository, pagine Budget ed editor; integrati navigazione, residui nel Registro e registrazione della spesa con ritorno ai budget. Aggiunti 14 test.

DEVIAZIONI

Nessuna. Comandi di verifica eseguiti:
- `dotnet test Eton.Tests/Eton.Tests.csproj` — prima e dopo l’implementazione.
- `dotnet build`
- `git diff --check`

VERIFICA

test rosso  CalcoliBudgetTests: CalcoliBudgetTests.cs(8,20): error CS0246: Il nome di tipo o di spazio dei nomi 'CategoryBudget' non è stato trovato. Probabilmente manca una direttiva using o un riferimento all'assembly.

Build riuscita; 432 test superati, zero fallimenti. Diff senza errori di spaziatura.

NOTE

Avvisi NU1900: servizio NuGet delle vulnerabilità non raggiungibile. App e Synapse.Server non avviati. File vietati e resoconti preesistenti non modificati.