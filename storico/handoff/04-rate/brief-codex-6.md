# Correzione dalla critica della schermata — unità 04, stesso worktree

Rilievo di ui-critic: nel Registro a schermo largo, con «A rate» acceso, la griglia a due colonne di `.campi-spesa`
(celle fisse per importo, data e descrizione) sparpaglia i figli nuovi: la riga «3 rate da 33,33 €» e l'errore del
centesimo finiscono nella colonna sinistra, il select «Numero di rate» in quella destra. L'editor delle ricorrenti ha
già risolto lo stesso problema con la classe `campi-ricorrente` (wwwroot/css/app.css:2708-2715: una colonna a ogni
larghezza, aiuti ed errori a 8 px dal loro campo).

Unica modifica, in `Pages/Spese.razor` riga ~62: `<div class="campi-spesa">` diventa
`<div class="campi-spesa @(aRate ? "campi-ricorrente" : null)">`, con un commento Razor di una riga sopra:
con «a rate» il modulo ha più figli dei tre della griglia, e prende la colonna unica dell'editor delle ricorrenti
(app.css, `.campi-ricorrente`). Nessun CSS, nessun altro file.

## Verifica
`dotnet build` nel worktree. Non avviare Synapse.Server né l'applicazione (il server di sviluppo è della sessione:
non fermarlo, non lanciare `dotnet run`).

## Formato del ritorno
Il tuo resoconto è il tuo messaggio finale (il file `-o` lo scrive la riga di comando): `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/04-rate/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

In più, su Windows: non scrivere file con `Set-Content`, `Out-File`, `[System.IO.File]::Write*` o altri comandi PowerShell; i file si modificano solo con apply_patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
