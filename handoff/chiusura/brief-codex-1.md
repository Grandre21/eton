Brief per Codex, autore del diff — chiusura della fase 2.3 di Eton (Blazor WebAssembly, .NET).

Worktree: G:\Sviluppo\Eton\.claude\worktrees\chiusura-2-3

## Il difetto

In `Pages/Spese.razor` l'avviso «Piano a rate creato: …» (campo `avvisoPiano`, dichiarato alla riga 306,
impostato alla 420, mostrato alle 156-158) si azzera solo in `Segna()` (riga 392). Se l'utente cambia
spazio dopo aver creato un piano, l'avviso resta sopra il registro del nuovo spazio, e il suo link
«Vai alle ricorrenti» porta alle ricorrenti di uno spazio dove quel piano non c'è.

Il cambio di spazio passa da `PaginaRegistro.SuCambioSpazio` (`Shared/PaginaRegistro.cs:184`), che chiama
`PrimaDiRicaricare()`. La pagina lo sovrascrive alla riga 490 per azzerare solo `ultimaSpesaCreataId`.

## La correzione, esatta

Un solo file, una sola riga: `Pages/Spese.razor:490`.

prima:

    protected override void PrimaDiRicaricare() => ultimaSpesaCreataId = null;

dopo:

    protected override void PrimaDiRicaricare()
    {
        ultimaSpesaCreataId = null;
        avvisoPiano = null;
    }

Rientro a 4 spazi come i membri vicini (righe 488 e 492).

## Cosa NON toccare

- Non toccare `MesePrecedente`, `MeseSuccessivo`, `Riprova` né il commento delle righe 344-349: cambiando
  mese l'avviso resta, di proposito (il piano non è legato a un mese).
- Nessun altro file. Nessun test nuovo. Nessuna astrazione nuova.

## Budget di complessità

Zero tipi nuovi, zero metodi nuovi, nessun helper.

## Verifica

Non compilare e non lanciare test: la build e i test li lancia la sessione con
`mcp__synapse__build`/`test root=<worktree>` dopo il tuo rientro (compilazioni concorrenti corrompono
`obj/`). Non avviare Synapse.Server.

## Ritorno

Formato fisso: `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

E su Windows nemmeno `Set-Content`, `Out-File`, `[System.IO.File]::Write*` o altri comandi PowerShell che scrivono file: si scrivono solo con apply_patch.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/<NN-slug>/resoconto.md`.

(Il «file `-o`» è il tuo messaggio finale: la riga di comando lo salva da sé. Non creare un file chiamato `-o` e non chiedere quale sia il percorso.)

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
