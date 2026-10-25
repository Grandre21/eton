FATTO

Completati i quattro punti: filtro categoria per tipo, estrazioni e test delle ricorrenti, chiave di confronto unica, claims centralizzati nei nove SQL. Modifiche effettuate esclusivamente con patch.

DEVIAZIONI

Comandi di compilazione e suite eseguiti:

- `dotnet test Eton.Tests/Eton.Tests.csproj` — due lanci: rosso iniziale e verde finale.
- `dotnet build` — due tentativi falliti per accesso negato al file temporaneo di code coverage.
- `dotnet build -m:1` — riuscito.

VERIFICA

```text
test rosso  RecurringExpenseRepositoryTests: RecurringExpenseRepositoryTests.cs(20,49): error CS0117: 'RecurringExpenseRepository' non contiene una definizione per 'NuovaRegola'
test rosso  ExpenseRepositoryTests.TipiAmmessi: ExpenseRepositoryTests.cs(140,63): error CS0117: 'ExpenseRepository' non contiene una definizione per 'TipiAmmessi'
```

- Suite finale: **440 superati**, 0 falliti.
- Build con `-m:1`: 0 errori.
- `git diff --check`: superato.
- Conteggio `request.jwt.claims = '{`: **0**.
- Confronto SQL: soltanto blocchi variabili e sostituzioni richieste.

NOTE

Resta il warning NU1900 per il servizio vulnerabilità NuGet non raggiungibile. Script SQL, app, Docker, Supabase e Synapse non avviati. File preesistenti in `handoff` lasciati invariati.