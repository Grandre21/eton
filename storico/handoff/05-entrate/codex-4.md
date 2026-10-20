## FATTO

- A: nota del tipo spostata dentro `campo-lettura`, dopo il valore; testo invariato e `div` su più righe.
- B: applicate le maiuscole «Entrate» e «Saldo».
- C: aggiunto solo `NomeTipo`, usato nel titolo e nei testi indicati; spiegazione dell’importo distinta per entrata e spesa.

## DEVIAZIONI

Nessuno scostamento nelle modifiche. Nessuno script eseguito. Comandi delle suite: `dotnet build`, `dotnet test`.

## VERIFICA

- Build: **Errori: 0; Avvisi: 4**.
- Test: **Non superati: 0. Superati: 417. Ignorati: 0. Totale: 417**.
- `git diff --check`: nessun problema.

## NOTE

Avvisi NU1900: impossibile raggiungere NuGet per i dati sulle vulnerabilità.

Modificati soltanto i tre file richiesti. File preesistenti in `handoff/` lasciati intatti. App e Synapse.Server non avviati; GATE demandato alla sessione.