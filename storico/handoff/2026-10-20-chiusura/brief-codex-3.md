Brief per Codex, autore del diff — terza correzione, stesso worktree, stesso file. Sostituisce le due
correzioni precedenti con una sola.

Worktree: G:\Sviluppo\Eton\.claude\worktrees\chiusura-2-3

## Perché

L'avviso «Piano a rate creato» (`avvisoPiano`) deve sparire quando a schermo arriva un altro spazio. Le due
toppe precedenti (in `PrimaDiRicaricare` e dopo `await Carica()` in `Segna`) coprono due percorsi, ma il
revisore ne ha trovato un terzo (cambio di spazio durante Riprova / mese precedente / mese successivo, scartato
da `ScartaCambioSpazio`). Tutti i percorsi passano da `Leggi(Space attivo)` (chiamato da
`Shared/PaginaRegistro.cs:173`, con lo spazio davvero mostrato): il controllo va lì, una volta sola.

## La correzione, esatta — solo `Pages/Spese.razor`

1. **Togli la toppa in `Segna()`**: elimina le tre righe aggiunte dopo `await Carica();` (oggi righe 439-441):
   i due commenti che iniziano con «// Un cambio di spazio arrivato durante Segna…» e
   `if (Spazi.Attivo?.Id != attivo.Id) avvisoPiano = null;`.

2. **Riporta `PrimaDiRicaricare` com'era in HEAD** (oggi righe 493-497):

       protected override void PrimaDiRicaricare() => ultimaSpesaCreataId = null;

3. **Campo nuovo**, subito sotto `private string? avvisoPiano;` (riga 306):

       // Lo spazio in cui il piano dell'avviso è nato: Leggi toglie l'avviso appena a schermo c'è un altro spazio.
       private Guid? spazioDelPiano;

4. **In `Segna()`**, subito dopo la riga che imposta l'avviso
   (`avvisoPiano = $"Piano a rate creato: …";`, oggi riga 420), aggiungi, con lo stesso rientro:

                spazioDelPiano = attivo.Id;

5. **In `Leggi(Space attivo)`** (oggi riga 499), come PRIMA istruzione del corpo, prima di
   `riepilogo = new RiepilogoMese(0, [], 0, null);`, aggiungi con rientro a 8 spazi:

        if (attivo.Id != spazioDelPiano) avvisoPiano = null;

Alla fine `git diff HEAD -- Pages/Spese.razor` deve mostrare SOLO i punti 3, 4 e 5 (il 1 e il 2 annullano
modifiche non committate).

## Cosa NON toccare

- `ScartaCambioSpazio`, `MesePrecedente`, `MeseSuccessivo`, `Riprova`, `ultimaSpesaCreataId`, il commento
  delle righe 344-349, il `catch` e il `finally` di `Segna`.
- Nessun altro file. Nessun test nuovo. Un solo campo nuovo, nessun metodo nuovo.

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
