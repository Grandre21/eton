**FATTO**

Implementato il brief negli otto file autorizzati: creazione e modifica dei piani, rate con resto sull’ultima, stato e residuo, gruppo «A rate», quota arrotondata e otto nuovi test. Inclusa la correzione autorizzata per l’estinzione anticipata.

**DEVIAZIONI**

Nessuna ulteriore deviazione funzionale. Comandi di verifica eseguiti:

- `dotnet test --filter Quota_somma_i_totali_di_gruppo_arrotondati` — da `Eton.Tests`, prima del fix.
- `dotnet build` — dalla radice, due volte; errore Razor iniziale corretto.
- `dotnet test` — da `Eton.Tests`, dopo il fix.
- `git diff --check`
- `git diff --stat`
- `git diff -- Services/CalcoliRicorrenti.cs Services/RecurringExpenseRepository.cs Pages/RicorrenteEdit.razor`

**VERIFICA**

L’esecuzione iniziale è fallita in compilazione, impedendo anche l’esecuzione del test sulla quota. Righe di errore ricopiate dall’output:

```text
test rosso  PianiARateTests: G:\Sviluppo\wt-Eton\04-rate\Eton.Tests\PianiARateTests.cs(11,48): error CS0117: 'CalcoliRicorrenti' non contiene una definizione per 'ImportoRata' [G:\Sviluppo\wt-Eton\04-rate\Eton.Tests\Eton.Tests.csproj]
test rosso  GruppiRicorrentiTests.Quota_somma_i_totali_di_gruppo_arrotondati: G:\Sviluppo\wt-Eton\04-rate\Eton.Tests\PianiARateTests.cs(11,48): error CS0117: 'CalcoliRicorrenti' non contiene una definizione per 'ImportoRata' [G:\Sviluppo\wt-Eton\04-rate\Eton.Tests\Eton.Tests.csproj]
```

Dopo il fix: build riuscita; **406 test superati, 0 falliti, 0 ignorati**. `git diff --check` senza errori.

**NOTE**

Avvisi NU1900: indice NuGet delle vulnerabilità non raggiungibile. Applicazione e Synapse.Server non avviati. Nessuna scrittura manuale nei file di resoconto.