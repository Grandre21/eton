Brief di correzione per l'autore del diff — Eton, unità 02 (stesso worktree G:\Sviluppo\wt-Eton\02-gruppi-quota)

Una sola correzione, solo spazi, solo in `Pages/Ricorrenti.razor`.

Il punto 2 del brief precedente non è applicato: oggi `<ul class="registro elenco-ricorrenti">` (riga 81) e
`@foreach (var r in g.Regole)` (riga 82) partono entrambi alla colonna 17 (16 spazi), e la `}` di chiusura
del foreach (riga 104) pure. Le righe 82-104 comprese vanno spostate a destra di **quattro spazi in più**:
`@foreach` a 20 spazi, `{` e `}` a 20, `// null = …`, `var prossima`, `<li>` a 24, e così via per tutto
il corpo. `<ul>` (riga 81) e `</ul>` (riga 105) restano a 16. Nessun altro carattere cambia: controlla con
`git diff HEAD -w -- Pages/Ricorrenti.razor` che, ignorando gli spazi, il diff sia identico a prima di
questa correzione, e con `Get-Content` delle righe 81-105 che le colonne siano quelle scritte qui.

## Verifica

Non compilare e non lanciare test: un server di sviluppo è in ascolto su questo worktree. La build la
rilancia chi ti ha dato il brief.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/<NN-slug>/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

## Formato di ritorno

Nel file `-o`: `FATTO` oppure `BLOCKED: <domanda>`; `DEVIAZIONI`; `VERIFICA` (le colonne lette di
righe 81, 82, 85, 104, 105); `NOTE`.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
