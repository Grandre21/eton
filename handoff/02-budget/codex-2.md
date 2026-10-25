FATTO

Applicate tutte le sei correzioni nei soli `Pages/Budget.razor`, `Pages/BudgetEdit.razor` e `Pages/Spese.razor`.

DEVIAZIONI

Nessuna. Comandi di verifica eseguiti:
- `dotnet build`
- `dotnet test Eton.Tests/Eton.Tests.csproj`
- `git diff --check`

VERIFICA

Build riuscita; 432 test superati, zero fallimenti. Nessun errore di spaziatura.

NOTE

Restano gli avvisi NU1900 per il servizio NuGet non raggiungibile. App e Synapse.Server non avviati.