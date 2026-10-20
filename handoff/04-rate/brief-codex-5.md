# Correzione dalla prova nel browser — unità 04, stesso worktree

Difetto visto da live-testing: in `Pages/Spese.razor`, dopo «Segna» con «a rate» acceso, il campo «Quando» resta sulla
data della prima rata (es. il mese prossimo); una spesa singola segnata subito dopo, a interruttore spento, prende quella
data e finisce in un mese futuro, invisibile nel registro di oggi.

Unica modifica, in `Segna()` dentro il ramo `if (aRate)`, accanto a `aRate = false;` (riga ~395):
`nuovaData = DateTime.Today;` con un commento di una riga: la data era quella della prima rata, non di una spesa, e la
spesa successiva è quasi sempre di oggi. Il commento esistente «Importo e descrizione si svuotano, data e categoria
restano» (riga ~403) va ritoccato con una frase: «tranne la data dopo un piano a rate, v. sopra».

Nient'altro, nessun altro file.

## Verifica
`dotnet build` nel worktree. Non avviare Synapse.Server né l'applicazione (il server di sviluppo è acceso dalla sessione:
non fermarlo, non lanciare `dotnet run`).

## Formato del ritorno
Il tuo resoconto è il tuo messaggio finale (il file `-o` lo scrive la riga di comando): `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/04-rate/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

In più, su Windows: non scrivere file con `Set-Content`, `Out-File`, `[System.IO.File]::Write*` o altri comandi PowerShell; i file si modificano solo con apply_patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
