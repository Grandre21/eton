# Brief 4 — correzioni dopo la critica della schermata (unità 05 di Eton)

Worktree `G:\Sviluppo\wt-Eton\05-entrate`, ultimo commit 2f411e7. Solo markup e testi in tre pagine, nessun CSS,
nessun cambiamento di comportamento oltre a quello detto.

## A. `Pages/SpesaEdit.razor:65-66` — la nota del tipo sta nel suo campo (ui-critic, rilievo 1)

Oggi `<p class="spiega">Il tipo non si cambia: …</p>` è un figlio diretto della griglia `.campi-spesa` e finisce nella
seconda colonna, accanto a «Quando». Spostalo **dentro** `<div class="campo campo-lettura">`, dopo il `<p>` del valore.
Testo invariato. Scrivi il `div` su più righe come gli altri `campo-lettura` della stessa pagina (riga ~99).

## B. `Pages/Home.razor:131` — stessa grafia del Registro (ui-critic, rilievo 2, parte di testo)

`entrate … · saldo …` diventa `Entrate … · Saldo …` (maiuscole come `Pages/Spese.razor:199`). Il resto della riga
invariato; nessuna classe nuova.

## C. `Pages/RicorrenteEdit.razor` — i testi seguono il tipo (ui-critic, rilievo 5)

Con `tipo == TipoMovimento.Entrata` la pagina dice ancora «spesa». Aggiungi accanto a `TitoloSchermata` (riga ~338)
un membro:
```csharp
    // «spesa» o «entrata», per i testi della pagina che nominano il tipo della regola.
    private string NomeTipo => TipoMovimento.Testo(tipo).ToLowerInvariant();
```
e usalo in `TitoloSchermata` al posto di `TipoMovimento.Testo(tipo).ToLowerInvariant()`. Poi questi testi:

- riga 47, `Premessa`: `$"Mentre la modificavi, questa {NomeTipo} ricorrente è cambiata."`
- riga 54: `Questa @NomeTipo ricorrente l'ha dichiarata qualcun altro: …` (il resto invariato)
- riga 134: per una spesa resta il testo di oggi; per un'entrata
  «L'importo è quello atteso: se arriva diverso, lo correggi sull'entrata del mese.» (un `@if`/`else` o un ternario
  semplice sul testo, a tua scelta: il più corto leggibile)
- riga 262: `<h2>Fermare questa @NomeTipo ricorrente</h2>`
- riga 288: `Non ha ancora segnato nessuna @NomeTipo, quindi si può anche eliminare.`

Non toccare l'`Aiuto` in testa (righe 14-15), i messaggi d'errore in `@code`, né altri testi.

## Budget di complessità

Un solo membro nuovo (`NomeTipo`). Nessun file nuovo, nessun CSS, nessun test nuovo (sono testi di markup).
Commenti in italiano.

## Non toccare

`supabase/`, `docs/`, `handoff/` (salvo il tuo file `-o`), `wwwroot/`, i file `.css`, ogni file non nominato qui.
Non avviare l'app (niente `dotnet run`) e non avviare Synapse.Server.

## Verifica

`dotnet build` e `dotnet test` nel worktree: 0 errori, 0 test falliti (oggi 417). Il GATE lo rilancia la sessione
con `mcp__synapse__build root=<worktree>`.

## Formato del ritorno

Il tuo messaggio finale, in quattro sezioni: `FATTO` (per lettera A-C), `DEVIAZIONI` (ogni scostamento col perché,
e per ogni script o suite che lanci il comando esatto), `VERIFICA` (esito di build e test con i conteggi
ricopiati), `NOTE`. Oppure una riga sola `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.
Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/05-entrate/resoconto.md`.
(Il file `-o` non lo crei tu: è il tuo **messaggio finale**, che la riga di comando salva da sé in
`handoff/05-entrate/codex-4.md`. Non creare file chiamati `-o`.)
Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.
Non scrivere file con `Set-Content`, `Out-File` o `[System.IO.File]::Write*`: modifica i file con le tue patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
