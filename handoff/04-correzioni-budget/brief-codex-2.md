# Brief per Codex — unità 04, giro 2: un commento

Stesso worktree. Una sola modifica, in `Pages/BudgetEdit.razor`, riga 158 (il commento sopra
`protected override bool Cambiata`). Oggi è:

```csharp
    // In creazione conta solo l'importo scritto: una categoria scelta senza importo non è lavoro che si perde.
```

Diventa due righe, che conservano anche la ragione del ramo `!Nuova` che il giro 1 aveva tolto:

```csharp
    // In creazione conta solo l'importo scritto: una categoria scelta senza importo non è lavoro che si perde.
    // Per un budget esistente, stessa conversione del campo, così sopra mille euro il modulo non nasce già modificato.
```

Nient'altro, in nessun file. Non serve compilare (il GATE lo rifà la sessione); non avviare l'app né
Synapse.Server.

I file si creano e si modificano solo con le tue patch. Vietati gli interpreti con codice inline (`python -`, `python -c`, `node -e`, here-string passate a un interprete) e vietato scrivere file da PowerShell: niente comandi `powershell.exe -Command` né script `.ps1` usati per creare o riscrivere file, con `Set-Content`, `Out-File`, `Add-Content`, `[System.IO.File]::Write*` o sostituzioni a regex; leggere, compilare e lanciare i test da PowerShell resta permesso. Se una modifica non si può fare con una patch, fermati e scrivi `BLOCKED:` col motivo; se l'hai fatta in un altro modo, scrivi in `DEVIAZIONI` il comando esatto e i file che ha toccato.

Il tuo resoconto è il tuo messaggio finale: Codex lo salva da solo in `G:\Sviluppo\wt-Eton\04-correzioni-budget\handoff\04-correzioni-budget\codex-2.md` quando finisci. Non scrivere tu quel file, non creare altri file di resoconto e non scrivere in `handoff/04-correzioni-budget/resoconto.md`. (È il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`.)

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Formato del messaggio finale: `FATTO: … / DEVIAZIONI: … | nessuna / VERIFICA: … / NOTE: … | nessuna`, oppure `BLOCKED: <motivo>`.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
