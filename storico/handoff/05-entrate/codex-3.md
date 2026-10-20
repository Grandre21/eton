## FATTO

- **A** — Spostato `TipoMovimento` invariato in `Models/Expense.cs`; aggiornati riferimenti e import necessari.
- **B** — Semplificati `PerMese` e `ConSegno`.
- **C** — `CategorieSpesa.Tutte` usa `Union`.
- **D** — Riordinati i test indicati e sostituite le stringhe dei tipi con le costanti.
- **E** — Centralizzato `TipoSelezione`; eliminato il `First` pericoloso, protetto il cambio categoria e aggiornata la conferma eliminazione.
- **F** — Le entrate usano `ConSegno` nei tre punti indicati.
- **G** — Riscritto il getter del titolo e spostato `ScegliTipo` dopo i campi.
- **H** — Corretti i rientri richiesti.

## DEVIAZIONI

Nessuno scostamento. Comandi di verifica eseguiti: `dotnet build`, `dotnet test`, `git diff --check`.

## VERIFICA

- Build: **Errori: 0; Avvisi: 4**, tutti NU1900 per NuGet non raggiungibile.
- Test: **Non superati: 0. Superati: 417. Ignorati: 0. Totale: 417.**
- Diff: nessun errore di spaziatura.

## NOTE

Nessun file nuovo o CSS modificato. App e Synapse.Server non avviati. Brief preesistente lasciato intatto.