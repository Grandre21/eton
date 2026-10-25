FATTO: 1 — Riordinati i figli della riga budget e raccolti i link in `.azioni`; aggiunta la sola regola CSS richiesta.
FATTO: 2 — In creazione la guardia considera solo l’importo scritto; aggiornato il commento.
FATTO: 3 — Spostato il blocco vuoto dentro il riepilogo mensile, mantenendo mese e frecce visibili.

DEVIAZIONI: Nessuno scostamento nelle modifiche. Comandi eseguiti:
```powershell
Get-Content Pages/Budget.razor | Select-Object -First 140; Get-Content Pages/BudgetEdit.razor | Select-Object -Skip 145 -First 25; Get-Content wwwroot/css/app.css | Select-Object -Skip 2070 -First 45
dotnet build
git diff --check; git diff --stat; git diff -- Pages/Budget.razor Pages/BudgetEdit.razor wwwroot/css/app.css
dotnet test
```

VERIFICA: `git diff --check` superato. `dotnet build` fallito:
```text
Compilazione NON RIUSCITA.
    Avvisi: 4
    Errori: 1
```
`dotnet test` fallito prima dell’esecuzione dei test. Entrambi riportano:
```text
CSC : error CS2012: Non è possibile aprire 'G:\Sviluppo\wt-Eton\04-correzioni-budget\obj\Debug\net10.0\Eton.dll' per la scrittura -- Access to the path 'G:\Sviluppo\wt-Eton\04-correzioni-budget\obj\Debug\net10.0\Eton.dll' is denied.
```

NOTE: Avvisi NU1900 per indice NuGet non raggiungibile. Diff: 3 file, 21 inserimenti e 14 rimozioni. App e Synapse.Server non avviati.