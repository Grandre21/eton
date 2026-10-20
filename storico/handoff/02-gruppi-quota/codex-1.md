FATTO
- Services/CalcoliRicorrenti.cs: aggiunti enum, record, classificazione, raggruppamento ordinato, totali e quota mensile; firme e logica esistenti conservate.
- Pages/Ricorrenti.razor: gruppi con totali e quota in testa; contenuto dei li e metodo Prossima conservati.
- Pages/Ricorrenti.razor.css: aggiunto il CSS isolation richiesto.
- Eton.Tests/GruppiRicorrentiTests.cs: sei test scritti prima della produzione; classificazione, ordine, accenti, stabilità per Id, totali, arrotondamenti e quota con regole future.

DEVIAZIONI
- Nessun percorso di output fornito dopo -o: resoconto scritto nel file letterale -o del worktree.
- Errori di accesso in scrittura hanno richiesto tentativi con compilazione condivisa disabilitata e percorso degli artefatti alternativo.
- Comandi di suite/build, nell'ordine eseguito (nessuno script lanciato):
  dotnet test Eton.sln --filter FullyQualifiedName~GruppiRicorrentiTests
  dotnet test Eton.sln --filter FullyQualifiedName~GruppiRicorrentiTests --artifacts-path .artifacts-gruppi
  dotnet test Eton.sln --filter FullyQualifiedName~GruppiRicorrentiTests -p:UseSharedCompilation=false
  dotnet build Eton.sln -p:UseSharedCompilation=false
  dotnet test Eton.sln -p:UseSharedCompilation=false
  dotnet build Eton.sln -p:UseSharedCompilation=false
  dotnet build Eton.sln
  dotnet test Eton.sln
- Altri comandi di lettura/verifica eseguiti:
  Get-Content Services/CalcoliRicorrenti.cs; Get-Content Pages/Ricorrenti.razor; Get-Content Eton.Tests/CalcoliRicorrentiTests.cs; Get-Content Models/RecurringExpense.cs; Get-Content Services/CalcoliGriglia.cs
  rg -n 'enum Gruppo|gruppi|Titolo' Services/CalcoliRicorrenti.cs Pages/Ricorrenti.razor
  git diff --check; git diff --stat; git status --short

VERIFICA
test rosso  GruppiRicorrentiTests: G:\Sviluppo\wt-Eton\02-gruppi-quota\Eton.Tests\GruppiRicorrentiTests.cs(13,22): error CS0103: Il nome 'GruppoRicorrenti' non esiste nel contesto corrente [G:\Sviluppo\wt-Eton\02-gruppi-quota\Eton.Tests\Eton.Tests.csproj]
- dotnet test Eton.sln: 398 superati, 0 falliti, 0 ignorati. Identico esito con UseSharedCompilation=false.
- dotnet build Eton.sln: applicazione compilata, ma soluzione fallita con MSB3491 nel target Microsoft.CodeCoverage.targets: accesso negato alla scrittura del file temporaneo per .msCoverageSourceRootsMapping_Eton.Tests. Stesso impedimento con UseSharedCompilation=false.
- git diff --check: superato.

NOTE
- Synapse.Server non avviato. Nessun file handoff modificato.
- Gate finale di build da rilanciare nell'ambiente del committente per il problema di permessi del target coverage.
