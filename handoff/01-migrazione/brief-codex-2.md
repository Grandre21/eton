# Brief 2 — correzione dopo la revisione

Stesso worktree, stessi divieti e stesso perimetro del brief 1. Tocchi **solo** `supabase/migrations/20261024000000_budget.sql`.

## Cosa cambiare

I tre vincoli `category_budgets_starts_on_primo_del_mese`, `category_budgets_ends_on_primo_del_mese` e `category_budgets_ends_on_dopo_inizio` oggi stanno alle righe 33-43, aggiunti dopo la tabella con `drop constraint if exists` + `add constraint`. Spostali **dentro** il `create table`, come vincoli di tabella nominati, dopo `updated_at`, con gli stessi nomi esatti e le stesse espressioni:

```
    updated_at   timestamptz not null default now(),

    -- <commento attuale della riga 33, ricopiato>
    constraint category_budgets_starts_on_primo_del_mese check (extract(day from starts_on) = 1),
    -- <commento attuale della riga 37, ricopiato>
    constraint category_budgets_ends_on_primo_del_mese   check (extract(day from ends_on) = 1),
    constraint category_budgets_ends_on_dopo_inizio      check (ends_on >= starts_on)
);
```

Poi elimina le righe 33-43 (commenti compresi) e le loro nove istruzioni. Il perché: la tabella nasce in questa migrazione, e `create table if not exists` salta tutto lo statement, vincoli compresi, quindi la forma `drop/add` non dava nulla in più; i check delle colonne della stessa tabella sono già inline. Il resto del file non cambia: il blocco `do` del tetto alle rate resta com'è.

## Verifica

Rileggi il file e controlla che i tre nomi siano identici a prima e che il `create table` si chiuda correttamente. Non avviare Docker né Supabase: la prova la rifà la sessione.

## Regole di scrittura

I file si creano e si modificano solo con le tue patch. Vietati gli interpreti con codice inline (`python -`, `python -c`, `node -e`, here-string passate a un interprete) e vietato scrivere file da PowerShell: niente comandi `powershell.exe -Command` né script `.ps1` usati per creare o riscrivere file, con `Set-Content`, `Out-File`, `Add-Content`, `[System.IO.File]::Write*` o sostituzioni a regex; leggere, compilare e lanciare i test da PowerShell resta permesso. Se una modifica non si può fare con una patch, fermati e scrivi `BLOCKED:` col motivo; se l'hai fatta in un altro modo, scrivi in `DEVIAZIONI` il comando esatto e i file che ha toccato.

Il tuo resoconto è il tuo messaggio finale: Codex lo salva da solo in `G:\Sviluppo\wt-Eton\01-migrazione\handoff\01-migrazione\codex-2.md` quando finisci (è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`). Non scrivere tu quel file, non creare altri file di resoconto e non scrivere in `handoff/01-migrazione/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

## Formato di ritorno

```
FATTO: <file e righe cambiate>
DEVIAZIONI: <...> | nessuna
VERIFICA: <...>
NOTE: <...> | nessuna
```
Oppure `BLOCKED: <motivo>`.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
