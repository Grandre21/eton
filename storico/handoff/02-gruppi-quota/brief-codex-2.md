Brief di correzione per l'autore del diff — Eton, unità 02 (stesso worktree G:\Sviluppo\wt-Eton\02-gruppi-quota)

Quattro correzioni al diff che hai appena scritto. Nient'altro: non toccare file diversi da
`Pages/Ricorrenti.razor` e `Services/CalcoliRicorrenti.cs`, non cambiare firme pubbliche, nomi dei tipi
(`GruppoRicorrenti`, `GruppoDiRicorrenti`) né test.

1. **Un solo «oggi» nella pagina** (`Pages/Ricorrenti.razor`). Oggi `Leggi` raggruppa con un
   `var oggi = DateTime.Today` locale, mentre `Prossima(r)` in fondo al `@code` usa un nuovo
   `DateTime.Today` a ogni render: dopo la mezzanotte una regola può restare nel gruppo «Mensili» e
   mostrare «terminata». Rimedio: un campo `private DateTime oggi = DateTime.Today;`, assegnato in testa a
   `Leggi` (`oggi = DateTime.Today;`) e usato sia da `Raggruppa`/`QuotaDaAccantonare` sia da `Prossima(r)`
   al posto di `DateTime.Today`. Aggiorna il commento sopra `Prossima` con mezza riga che dice perché
   usa il campo (lo stesso giorno con cui si sono fatti i gruppi).

2. **Indentazione** (`Pages/Ricorrenti.razor`, il blocco `@foreach (var r in g.Regole)` e tutto il suo
   corpo fino alla `}` di chiusura): rientralo di quattro spazi così che stia dentro `<ul>`, come in
   `Pages/Home.razor:176-178`. Solo spazi, nessun cambiamento al markup.

3. **Condizione della quota** (`Pages/Ricorrenti.razor`, la riga `@if (!datiNonLetti && regole.Count > 0 && quota > 0)`):
   diventa `@if (quota > 0)`, preceduta da un commento Razor di una riga:
   `@* quota resta 0 finché la lettura non riesce, e senza annuali o periodiche *@`.

4. **`Raggruppa` leggibile** (`Services/CalcoliRicorrenti.cs`): sostituisci la catena LINQ con un corpo a
   blocco, stesso comportamento:
   ```csharp
   var gruppi = new List<GruppoDiRicorrenti>();
   foreach (var g in regole.GroupBy(r => Gruppo(r, oggi)).OrderBy(g => g.Key))
   {
       var ordinate = g.OrderBy(r => SchemaCampi.RiduciAccenti(r.Description).ToUpperInvariant(), StringComparer.Ordinal)
           .ThenBy(r => r.Id).ToList();
       decimal? totale = g.Key == GruppoRicorrenti.Terminate ? null : Arrotonda(g.Sum(QuotaMensile));
       gruppi.Add(new GruppoDiRicorrenti(g.Key, ordinate, totale));
   }
   return gruppi;
   ```
   e aggiungi accanto a `QuotaMensile` `private static decimal Arrotonda(decimal importo) => Math.Round(importo, 2, MidpointRounding.AwayFromZero);`,
   usato anche da `QuotaDaAccantonare` al posto del suo `Math.Round(…)`.

## Verifica

`dotnet build` e `dotnet test` nel worktree (Eton.sln): tutti verdi. Non avviare Synapse.Server. Il gate
finale lo rilancia chi ti ha dato il brief.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/<NN-slug>/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

## Formato di ritorno

Nel file `-o`: `FATTO` (correzione per correzione, con `file:riga`) oppure `BLOCKED: <domanda>`;
`DEVIAZIONI`; `VERIFICA`; `NOTE`.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
