# Risposta al tuo BLOCKED — prosegui con tutto il brief-codex-2

Sì, giusto il rilievo. Nella sezione «Fermare questa spesa ricorrente» di `Pages/RicorrenteEdit.razor`, **solo per un piano** (`Piano`):
- piano non bloccato: nessun blocco Termina (come già detto);
- piano bloccato e ancora attivo, cioè `CalcoliRicorrenti.Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, DateTime.Today)` non null con `r = conflitto ?? regola!`: la riga «Terminandola, da domani non segna più spese. Le spese già segnate restano.» e il `ConfermaAzione` Termina, identici a oggi;
- piano bloccato e concluso (Prossima null): `<p class="testo-tenue">Concluso il @Testi.DataSola(regola!.EndsOn!.Value): non segna più rate. Le spese già segnate restano.</p>`, senza pulsante.
Per le regole non-piano il controllo su `regola!.EndsOn is DateTime fine` resta **identico**. Scegli tu la forma più semplice del markup (`@if/else if/else`), senza helper.

Il tuo resoconto è il tuo **messaggio finale** (il file `-o` lo scrive la riga di comando). Nel `VERIFICA` ricopia le righe `test rosso` dell'esecuzione sul codice di prima che hai già fatto.

Restano valide, verbatim:

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/04-rate/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

In più, su Windows: non scrivere file con `Set-Content`, `Out-File`, `[System.IO.File]::Write*` o altri comandi PowerShell; i file si modificano solo con apply_patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
