## FATTO

- `Pages/RicorrenteEdit.razor`: aggiunta `NomeTipoPlurale`, aggiornati solo i cinque testi indicati e la chiamata a `Prossima`.
- `Services/CalcoliRicorrenti.cs`: aggiunto l’overload e aggiornati `Stato` e `Gruppo`; invariata la forma a cinque argomenti.
- `Pages/Ricorrenti.razor`: rimosso il wrapper, aggiornati chiamata e commento.

## DEVIAZIONI

Nessuno scostamento. Comandi di build e suite eseguiti: `dotnet build`, `dotnet test`. Nessuno script eseguito.

## VERIFICA

- Build: **Errori: 0; Avvisi: 4**.
- Test: **Non superati: 0; Superati: 417; Ignorati: 0; Totale: 417**.
- `git -c core.excludesFile=NUL diff --check`: nessun problema.

## NOTE

Avvisi NU1900: indice NuGet non raggiungibile per il controllo vulnerabilità. App e Synapse.Server non avviati. Nessun resoconto scritto su file; GATE lasciato alla sessione.