Brief per Codex, autore del diff — seconda correzione, stesso worktree, stesso file.

Worktree: G:\Sviluppo\Eton\.claude\worktrees\chiusura-2-3

## Il difetto rimasto

La correzione di prima (`PrimaDiRicaricare()` che azzera `avvisoPiano`, Pages/Spese.razor:490-494) non
copre un caso: se l'utente cambia spazio MENTRE `Segna()` è in corso, `SuCambioSpazio` viene scartato da
`ScartaCambioSpazio() => rileggoMese` (Pages/Spese.razor:488; Shared/PaginaRegistro.cs:181), quindi
`PrimaDiRicaricare` non gira. `Carica()` rilegge lo spazio nuovo, ma l'avviso «Piano a rate creato» resta a
schermo riferito allo spazio di prima.

## La correzione, esatta

Un solo file: `Pages/Spese.razor`, dentro `Segna()`. Subito DOPO la riga 438

            await Carica();

e prima della graffa di chiusura del `try` (riga 439), aggiungi queste tre righe, rientrate a 12 spazi come
la riga 438:

            // Un cambio di spazio arrivato durante Segna è stato scartato (ScartaCambioSpazio), quindi
            // PrimaDiRicaricare non l'ha azzerato: l'avviso parlerebbe di un piano di un altro spazio.
            if (Spazi.Attivo?.Id != attivo.Id) avvisoPiano = null;

`attivo` è la variabile locale della riga 401 (`var attivo = await Spazi.AssicuraCaricatoAsync();`), già in
scope in quel punto.

## Cosa NON toccare

- Non toccare `PrimaDiRicaricare`, `ScartaCambioSpazio`, il ramo non a rate, `ultimaSpesaCreataId`, il
  `catch` né il `finally`.
- Nessun altro file. Nessun test nuovo. Nessun helper, nessun campo nuovo.

## Verifica

Non compilare e non lanciare test: build e test li lancia la sessione con `mcp__synapse__build`/`test
root=<worktree>` dopo il tuo rientro. Non avviare Synapse.Server.

## Ritorno

Formato fisso: `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

E su Windows nemmeno `Set-Content`, `Out-File`, `[System.IO.File]::Write*` o altri comandi PowerShell che scrivono file: si scrivono solo con apply_patch.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/<NN-slug>/resoconto.md`.

(Il «file `-o`» è il tuo messaggio finale: la riga di comando lo salva da sé. Non creare un file chiamato `-o` e non chiedere quale sia il percorso.)

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
