FATTO — Modificato solo `Pages/RicorrenteEdit.razor` secondo i punti 1–6. Testi Spesa invariati salvo le due frasi neutre; commenti esistenti intatti.

DEVIAZIONI — Nessuna. Suite eseguite: `dotnet build`; `dotnet test`. Nessuno script lanciato.

VERIFICA — Build riuscita; 417 test superati, zero falliti. `git diff --check` superato. Eseguito `grep -n -i "spes" Pages/RicorrenteEdit.razor`: occorrenze residue controllate, compresi i rami Spesa delle proprietà richieste.

NOTE — Avvisi NU1900: servizio NuGet non raggiungibile. Synapse.Server non avviato. File preesistenti in `handoff/` lasciati intatti.