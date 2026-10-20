FATTO — Correzioni applicate solo a `Pages/RicorrenteEdit.razor`: articoli, rinomina `Titolare`, proprietà `TipoNoto`, Aiuto e titolo neutri. Testi Spesa preservati salvo le eccezioni concordate.

DEVIAZIONI — Nessuna. Suite eseguite: `dotnet build`; `dotnet test`. Nessuno script lanciato.

VERIFICA — Build riuscita; 417 test superati. `git diff --check` superato. Eseguito `grep -n -i "spes\|la paga" Pages/RicorrenteEdit.razor`: residui conformi, inclusa la frase neutra esplicitamente richiesta.

NOTE — Avvisi NU1900 per NuGet non raggiungibile. Nessun altro file modificato da me; Synapse.Server non avviato.