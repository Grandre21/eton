FATTO

Applicate le quattro correzioni nei soli tre file autorizzati, esclusivamente tramite patch. Il ternario con `[]` compila; i test di `NuovaRegola` sono separati nei tre scenari richiesti.

DEVIAZIONI

Nessuna. Comandi eseguiti:

- `dotnet build -m:1`
- `dotnet test Eton.Tests/Eton.Tests.csproj`
- `git diff --check`

VERIFICA

Build riuscita, 0 errori. Test: **442 superati**, 0 falliti, 0 ignorati. Controllo whitespace superato.

NOTE

Persistono i warning NU1900 per il servizio vulnerabilità NuGet non raggiungibile. Nessuna app o servizio avviato.