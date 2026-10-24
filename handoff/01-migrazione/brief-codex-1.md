# Brief 1 — migrazione dei budget per categoria e tetto alle rate

Sei l'autore del diff. Lavori nel worktree `G:\Sviluppo\wt-Eton\01-migrazione`. Crei **due file nuovi** e non tocchi nient'altro.

## File da creare

1. `supabase/migrations/20261024000000_budget.sql`
2. `supabase/verifica-rls-budget.sql`

## NON TOCCARE

Le migrazioni esistenti, gli script `supabase/verifica-rls-*.sql` esistenti, `Models/`, `Services/`, `Pages/`, `Shared/`, `Eton.Tests/`, `docs/`, `handoff/`. Nessun file C#. Non avviare Docker, Supabase né Synapse.Server: la prova sul database locale la fa la sessione che ti lancia.

## Pattern da rispecchiare (leggili prima di scrivere)

- `supabase/migrations/20260922000000_ricorrenti.sql`: intestazione (righe 1-10), tabella (13-57), indice (60-61), funzione e trigger di update (108-127), RLS (156-177), privilegi (202-219). La tabella nuova è una **copia per forma** di `recurring_expenses`, con i commenti in italiano allo stesso livello di densità: un commento dice il *perché*, non ripete il codice.
- `supabase/migrations/20261020000000_rate_entrate.sql`: forma `drop constraint if exists` + `add constraint` (righe 36-38), commenti sulla semantica di NULL nei check (33-35).
- `supabase/verifica-rls-rate-entrate.sql`: struttura intera dello script di collaudo — intestazione con il numero esatto di errori attesi e come si esegue (1-16), `\set ON_ERROR_STOP off`, utenti finti in `auth.users`, `create_space`/`join_space` (31-47), ogni caso in una transazione propria con `set local role authenticated` e `set local request.jwt.claims`, `\echo` con l'atteso, riepilogo finale (269-278). **Senza accenti** nello script, come quello.

## 1. La migrazione `20261024000000_budget.sql`

Idempotente e rieseguibile, additiva: un client già pubblicato non legge la tabella nuova e non ne è toccato.

Intestazione nello stile delle altre: dipende da `20260811000000_initial_schema.sql` (riusa senza modificarle `is_space_member()` e `is_space_owner()`) e da `20261020000000_rate_entrate.sql` (la colonna `installments`).

### 1a. Tabella `public.category_budgets` — nomi del contratto, non rinominarli

| colonna | definizione |
|---|---|
| `id` | `uuid primary key default gen_random_uuid()` |
| `space_id` | `uuid not null references public.spaces (id) on delete cascade` |
| `created_by` | `uuid not null default auth.uid() references auth.users (id) on delete cascade` — stesso tipo, default e FK di `recurring_expenses.paid_by`; cambia solo il nome, perché un budget non lo paga nessuno |
| `category` | `text not null check (length(btrim(category)) between 1 and 40)` — come `recurring_expenses.category` |
| `amount` | `numeric(12,2) not null check (amount > 0)` |
| `starts_on` | `date not null check (starts_on between '2000-01-01' and '2999-12-31')` |
| `ends_on` | `date null check (ends_on <= '2999-12-31')` |
| `version` | `integer not null default 1` |
| `created_at` | `timestamptz not null default now()` |
| `updated_at` | `timestamptz not null default now()` |

Vincoli di tabella, con questi nomi esatti, nella forma `drop constraint if exists` + `alter table ... add constraint` (così la migrazione resta rieseguibile anche se la tabella esiste già):

- `category_budgets_starts_on_primo_del_mese`: `check (extract(day from starts_on) = 1)`
- `category_budgets_ends_on_primo_del_mese`: `check (extract(day from ends_on) = 1)` — con `ends_on` null è soddisfatto
- `category_budgets_ends_on_dopo_inizio`: `check (ends_on >= starts_on)`

Commento da dare: un budget è una regola che vale ogni mese da `starts_on` finché non si chiude; `ends_on` è l'**ultimo** mese in cui vale (primo giorno di quel mese), e chiuderlo è scrivere `ends_on`, non cancellare.

Indici:
- `category_budgets_space_idx` su `(space_id)`, come `recurring_expenses_space_idx`.
- `category_budgets_aperto_uq`: `create unique index if not exists ... on public.category_budgets (space_id, category) where ends_on is null` — al più un budget aperto per categoria e spazio; un budget chiuso non impedisce di aprirne uno nuovo sulla stessa categoria.

### 1b. Trigger di update

Funzione `public.handle_category_budget_update()`, fotocopia di `handle_recurring_expense_update()` (`20260922000000_ricorrenti.sql:108-122`, stesso `set search_path = public`, **non** SECURITY DEFINER, con il commento del perché in una riga che rimanda a quella): `new.version := old.version + 1`, `new.updated_at := now()`, e rimesse a forza al valore precedente `created_by`, `space_id`, `created_at`, **e anche** `category` e `starts_on` (si decidono solo alla creazione, come `kind` in `20261020000000_rate_entrate.sql:89`).

Trigger `category_budgets_before_update`, `before update ... for each row`, con `drop trigger if exists` prima.

### 1c. RLS — forma delle ricorrenti, colonna `created_by` al posto di `paid_by`

`alter table public.category_budgets enable row level security;` e quattro policy, ciascuna con `drop policy if exists` prima:

- `category_budgets_select`: `for select using (public.is_space_member(space_id))`
- `category_budgets_insert`: `for insert with check (public.is_space_member(space_id) and created_by = auth.uid())`
- `category_budgets_update`: `for update using (created_by = auth.uid() or public.is_space_owner(space_id)) with check (created_by = auth.uid() or public.is_space_owner(space_id))`
- `category_budgets_delete`: `for delete using (created_by = auth.uid() or public.is_space_owner(space_id))`

### 1d. Privilegi

```
revoke all on public.category_budgets from anon, authenticated;
grant select, delete on public.category_budgets to authenticated;
grant insert (id, space_id, created_by, category, amount, starts_on, ends_on) on public.category_budgets to authenticated;
grant update (amount, ends_on) on public.category_budgets to authenticated;
grant all on public.category_budgets to service_role;
```

Commenti come in `ricorrenti.sql:196-219`: la RLS filtra e non concede; `id` in inserimento perché lo genera il client; in aggiornamento solo importo e chiusura; mai `version/created_at/updated_at` (li scrive il trigger: la concorrenza ottimistica usa `version` come **filtro** nel WHERE, che chiede solo SELECT); mai `space_id/created_by` (sposterebbero il budget sotto un'altra visibilità); mai `category/starts_on` (si decidono alla creazione).

### 1e. Tetto alle rate su `recurring_expenses`

Prima del vincolo, un blocco `do $$ ... $$` che, se esiste una riga con `installments > 60`, fa `raise exception` con un messaggio in italiano che dice quante righe e il massimo trovato, e che il vincolo non si può aggiungere finché quei piani non sono corretti. Poi:

```
alter table public.recurring_expenses drop constraint if exists recurring_expenses_installments_max;
alter table public.recurring_expenses
    add constraint recurring_expenses_installments_max check (installments is null or installments <= 60);
```

Commento: il client limita già il piano a 60 rate; il vincolo rifiuta solo ciò che il client già rifiuta, ed esiste per l'API diretta e per i client vecchi.

**Nessun** vincolo fra categoria e tipo del movimento (deciso: il controllo resta nel client).

## 2. Lo script `supabase/verifica-rls-budget.sql`

Sul modello di `verifica-rls-rate-entrate.sql`, senza accenti. Utenti finti con id e email propri (es. `a1100000-0000-0000-0000-000000000001` / `alice-budget@esempio.it`, Bruno `b2200000-…-0002` membro, Carla `c3300000-…-0003` estranea con uno spazio suo). Ogni caso che deve fallire sta in una transazione propria chiusa da `rollback`. Sezioni, in quest'ordine, con l'atteso nell'`\echo`:

1. Privilegi (una `select` di `has_column_privilege`/`has_table_privilege`): authenticated INSERT su `space_id`, `created_by`, `category`, `starts_on`, `amount`, `ends_on` = t; UPDATE su `amount`, `ends_on` = t; UPDATE su `space_id`, `created_by`, `category`, `starts_on`, `version` = f; `anon` SELECT sulla tabella = f; e `relrowsecurity` della tabella = t.
2. Preparazione: Alice crea lo spazio condiviso, Bruno entra col codice, Carla crea il suo spazio.
3. Alice crea il budget `Uscite` da 50.00 dal `2026-10-01` (id fisso): 1 riga, `version` 1, `created_by` Alice.
4. Casi che devono fallire, uno per transazione: **4a** secondo budget aperto `Uscite` nello stesso spazio (unique `category_budgets_aperto_uq`); **4b** `amount` 0; **4c** `starts_on` `2026-10-15`; **4d** `ends_on` `2026-09-01` con inizio `2026-10-01`; **4e** `ends_on` `2026-11-15`; **4f** `category` di soli spazi; **4g** Alice inserisce con `created_by` = Bruno (violazione della policy RLS).
5. Carla, estranea: **5a** `select` dei budget dello spazio condiviso → 0 righe; **5b** inserisce un budget nello spazio condiviso con `created_by` = se stessa → ERRORE RLS; **5c** `update` di `amount` del budget di Alice → 0 righe; **5d** `delete` dello stesso → 0 righe.
6. Alice tenta di spostare o riscrivere ciò che è solo in INSERT, un errore per transazione: **6a** `space_id` = spazio di Carla; **6b** `created_by`; **6c** `category`; **6d** `starts_on`; **6e** `version` → tutti ERRORE permission denied.
7. Concorrenza: Alice `update amount = 60.00 where id = … and version = 1 returning version` → 1 riga, version 2; subito dopo lo stesso update con `version = 1` → 0 righe.
8. Bruno, membro: **8a** `select` → vede il budget di Alice (1 riga); **8b** `update` di `amount` del budget di Alice → 0 righe; **8c** Bruno crea il proprio budget `Ristoranti` → 1 riga; **8d** Alice, proprietaria, chiude il budget di Bruno (`ends_on = '2026-10-01'`) → 1 riga.
9. Chiusura e riapertura: Alice chiude `Uscite` (`ends_on = '2026-10-01'`) → 1 riga; poi crea un nuovo `Uscite` aperto dal `2026-11-01` → 1 riga (l'indice parziale lo ammette).
10. Ultima riga di difesa, come `postgres` senza `set local role`: `update` del budget di Alice che prova a cambiare `space_id`, `created_by`, `category`, `starts_on` → `returning` mostra i valori invariati.
11. Tetto alle rate: **11a** Alice inserisce una ricorrente con `installments` 61 (e `plan_total` coerente, `kind` `expense`, `every_months` 1) → ERRORE check `recurring_expenses_installments_max`; **11b** la stessa con 60 rate → 1 riga.
12. Pulizia: conteggio budget dello spazio condiviso prima, Alice cancella lo spazio, conteggio dopo = 0 (cascade); stessa cosa per lo spazio di Carla.
13. Riepilogo finale con `\echo`, e l'ultima riga: `QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 14 ERRORI: le sezioni 4a, 4b, 4c, 4d, 4e, 4f, 4g, 5b, 6a, 6b, 6c, 6d, 6e, 11a.` — lo stesso numero e lo stesso elenco anche in intestazione. Il comando in intestazione è `docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-budget.sql`, dopo `supabase db reset`.

## Budget di complessità

Due file SQL, nessun file C#. Nessuna funzione oltre a `handle_category_budget_update()` e al blocco `do` del tetto. Nessuna vista, nessuna RPC, nessuna colonna oltre la tabella sopra.

## Verifica

Non c'è codice .NET nel diff: non serve `dotnet build` né `dotnet test`; il GATE lo rilancia la sessione con `mcp__synapse__build root=G:\Sviluppo\wt-Eton\01-migrazione`. Non avviare Synapse.Server. Rileggi i due file e controlla a mano che ogni nome del contratto (tabella, colonne, vincoli, indici, policy, funzione, trigger) sia scritto esattamente come qui.

## Regole di scrittura

I file si creano e si modificano solo con le tue patch. Vietati gli interpreti con codice inline (`python -`, `python -c`, `node -e`, here-string passate a un interprete) e vietato scrivere file da PowerShell: niente comandi `powershell.exe -Command` né script `.ps1` usati per creare o riscrivere file, con `Set-Content`, `Out-File`, `Add-Content`, `[System.IO.File]::Write*` o sostituzioni a regex; leggere, compilare e lanciare i test da PowerShell resta permesso. Se una modifica non si può fare con una patch, fermati e scrivi `BLOCKED:` col motivo; se l'hai fatta in un altro modo, scrivi in `DEVIAZIONI` il comando esatto e i file che ha toccato.

Il tuo resoconto è il tuo messaggio finale: Codex lo salva da solo in `G:\Sviluppo\wt-Eton\01-migrazione\handoff\01-migrazione\codex-1.md` quando finisci (è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`). Non scrivere tu quel file, non creare altri file di resoconto e non scrivere in `handoff/01-migrazione/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

## Formato di ritorno

```
FATTO: <una riga per file creato, con il numero di righe>
DEVIAZIONI: <ogni scostamento da questo brief e ogni comando lanciato> | nessuna
VERIFICA: <cosa hai controllato e come; il numero di errori attesi dello script e l'elenco delle sezioni>
NOTE: <dubbi> | nessuna
```
Oppure `BLOCKED: <motivo>`.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
