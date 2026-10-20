**FATTO**

Applicate le correzioni nei quattro file autorizzati: limite descrizione e guardie, `RataValida`, validazione unica nell’editor, blocco Termina unico, rientri e test di caratterizzazione del suffisso.

**DEVIAZIONI**

Nessuna. Comandi eseguiti:

- `dotnet test --filter Quota_somma_i_totali_di_gruppo_arrotondati` da `Eton.Tests`
- `dotnet build` dalla radice
- `dotnet test` da `Eton.Tests`

**VERIFICA**

Prima delle altre correzioni, con il vecchio calcolo della quota:

```text
test rosso  GruppiRicorrentiTests.Quota_somma_i_totali_di_gruppo_arrotondati: Assert.Equal() Failure: Values differ
Expected: 41.66
Actual:   41.67
```

Ripristinato il calcolo corretto. Build riuscita; **407 test superati, 0 falliti, 0 ignorati**, incluso `Suffisso_entra_nei_200_caratteri`.

**NOTE**

Restano gli avvisi NU1900 per l’indice NuGet non raggiungibile. Applicazione e Synapse.Server non avviati; nessun file nuovo.