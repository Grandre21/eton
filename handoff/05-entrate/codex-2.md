## FATTO

- `Spese.razor`: scelta tipo, categorie coerenti, entrate senza rate, riepilogo e importi aggiornati.
- `Home.razor`: entrate e saldo nella striscia mensile.
- `Griglia.razor`: parametro `VociRiepilogo`, riepilogo finale e subtotali dei gruppi.
- `SpeseTabella.razor`: colonna e filtro Tipo, bilancio, categorie di massa per selezioni omogenee, testi aggiornati.
- `SpesaEdit.razor`: tipo in sola lettura e categorie coerenti.
- `RicorrenteEdit.razor`: tipo selezionabile in creazione, immutabile sulle regole esistenti.
- `Ricorrenti.razor`: segno positivo sulle entrate e testi aggiornati.

## DEVIAZIONI

Nessuno scostamento funzionale; firme conformi al brief. Nessuno script lanciato.

Comandi di verifica eseguiti: `dotnet build`, `dotnet test`, `git diff --check`.

## VERIFICA

- Build: **Errori: 0; Avvisi: 4**, tutti NU1900 per NuGet non raggiungibile.
- Test: **Non superati: 0. Superati: 417. Ignorati: 0. Totale: 417.**
- `git diff --check`: nessun problema.

## NOTE

Nessun CSS o altro file escluso modificato. App e Synapse.Server non avviati. Il brief già presente in staging è rimasto intatto.