# Brief 1 — migrazione rate ed entrate (unità 03)

Sei l'autore di un diff nel worktree `G:\Sviluppo\wt-Eton\03-migrazione` (repo Eton, Supabase + Blazor).
Non esplorare oltre ciò che è citato qui, non prendere decisioni architetturali: applica il brief.

## File da creare (solo questi due)

1. `supabase/migrations/20261020000000_rate_entrate.sql`
2. `supabase/verifica-rls-rate-entrate.sql`

## NON TOCCARE

Tutto il resto: le migrazioni esistenti, `Models/`, `Services/`, `Pages/`, `Shared/`, `docs/`, `handoff/`
(salvo il tuo file `-o`), gli altri script `supabase/verifica-rls-*.sql`, `supabase/config.toml`.
Non avviare Synapse.Server. Non avviare Docker né Supabase. Non toccare database.

## 1 — La migrazione `20261020000000_rate_entrate.sql`

Stile da rispecchiare: `supabase/migrations/20260922000000_ricorrenti.sql` (intestazione a righe `-- ===`,
sezioni `-- ---------- … ----------`, commenti in italiano che spiegano il **perché** di ogni scelta,
idempotente e rieseguibile). Leggi quel file e `supabase/migrations/20260824000000_spese.sql` per intero.

Intestazione: «Eton — rate ed entrate: il tipo di un movimento e il piano a rate di una regola ricorrente.
Idempotente e rieseguibile.» + dipendenze (spese.sql, ricorrenti.sql, di cui estende le due funzioni
trigger). Dire che è **additiva**: un client già pubblicato, che non manda `kind`, scrive una spesa.

Contenuto, esattamente questo schema (nomi decisi, non rinominare):

### 1a. `kind` su `expenses` e su `recurring_expenses`
- `alter table … add column if not exists kind text not null default 'expense';` su entrambe.
- Il check come **vincolo nominato** con lo schema drop/add di ricorrenti.sql:76-83, così è rieseguibile:
  `expenses_kind_valido` e `recurring_expenses_kind_valido`, `check (kind in ('expense','income'))`.
- Commento: le entrate sono importi positivi con un tipo, non importi negativi; `amount > 0` resta.

### 1b. `installments` e `plan_total` su `recurring_expenses`
- `add column if not exists installments smallint null`, `add column if not exists plan_total numeric(12,2) null`.
- Vincoli nominati, ciascuno con drop if exists / add constraint:
  - `recurring_expenses_piano_coppia`: `check ((installments is null) = (plan_total is null))`
    (stesso ragionamento di `expenses_recurring_coppia` in ricorrenti.sql:95-100);
  - `recurring_expenses_installments_min`: `check (installments >= 2)`;
  - `recurring_expenses_plan_total_min`: `check (plan_total >= amount)`;
  - `recurring_expenses_piano_solo_spese_mensili`:
    `check (installments is null or (kind = 'expense' and every_months = 1))`.
  - Commento: con null i check valgono veri (semantica SQL), quindi una regola senza piano li passa tutti.

### 1c. Trigger
- `create or replace function public.handle_expense_update()`: corpo **identico** a ricorrenti.sql:133-150
  (stessa firma, `language plpgsql`, `set search_path = public`, non SECURITY DEFINER), con in più la riga
  `new.kind := old.kind;` dopo le due della provenienza. Commento: un movimento non cambia tipo; per
  cambiarlo lo si cancella e lo si riscrive. Il trigger esistente non va ricreato.
- `create or replace function public.handle_recurring_expense_update()`: corpo identico a
  ricorrenti.sql:108-122, con in più `new.kind := old.kind;` (stessa ragione, ultima riga di difesa).
  `installments` e `plan_total` NON si rimettono: si possono aggiornare.

### 1d. Privilegi (SOLO aggiuntivi)
- `grant insert (kind) on public.expenses to authenticated;`
- `grant insert (kind, installments, plan_total) on public.recurring_expenses to authenticated;`
- `grant update (installments, plan_total) on public.recurring_expenses to authenticated;`
- Commento: niente `grant update` su `kind`, di proposito; l'UPDATE di installments/plan_total esiste per
  modificare un piano non ancora partito, e il blocco a piano avviato lo fa il client.

### Vincoli di forma (un test C# legge i file delle migrazioni, `Eton.Tests/PrivilegiInsertTests.cs`)
- **Nessun `revoke`** in questa migrazione: il parser del test, a una `revoke` su una tabella, dimentica
  tutti i grant precedenti di quella tabella e il test fallisce.
- Ogni `grant` termina con `;` e ha la forma `grant <privilegi> on public.<tabella> to authenticated`.
- Le parole `grant` e `revoke` compaiono **solo** negli statement di privilegio e nei commenti `--`:
  non nei corpi delle funzioni, non in commenti `/* */`, non in stringhe. Niente `--` dentro stringhe.
- Nessuna policy RLS cambia: non scrivere `create policy`.

## 2 — Lo script `supabase/verifica-rls-rate-entrate.sql`

Ricalca la meccanica di `supabase/verifica-rls-ricorrenti.sql` (leggilo per intero): intestazione con
perché esiste, come si esegue (`docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - <
supabase/verifica-rls-rate-entrate.sql`), **senza accenti**, `\set ON_ERROR_STOP off`, utenti finti in
`auth.users` (usa id diversi da quelli di verifica-rls-ricorrenti, per esempio prefissi `a1…`, `b2…`, `c3…`),
impersonati con `set local request.jwt.claims`, `\echo` con l'atteso accanto a ogni sezione, riepilogo
finale e la riga «QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE N ERRORI: le sezioni …» con N contato davvero.

Ogni INSERT in `expenses` elenca tutte le colonne che il client invierà dopo le unità 4-5: id, space_id,
paid_by, amount, description, category, spent_on, recurring_id, recurring_period, kind. Ogni INSERT in
`recurring_expenses`: id, space_id, paid_by, amount, description, category, every_months, day_of_month,
starts_on, ends_on, materialized_through, kind, installments, plan_total.

Sezioni minime:
1. `has_column_privilege` di authenticated: `kind` INSERT t / UPDATE f su entrambe le tabelle;
   `installments` e `plan_total` INSERT t / UPDATE t su recurring_expenses.
2. Il client vecchio: un INSERT in expenses **senza** la colonna kind (le 9 colonne di prima) riesce e la
   riga ha `kind = 'expense'`.
3. Alice crea un'entrata (`kind='income'`) e una spesa; Alice crea una regola income e un piano a rate
   valido (installments 3, plan_total 100.00, amount 33.33, every_months 1).
4. ERRORE: `kind = 'altro'` respinto (check) — una volta per tabella.
5. ERRORE: Alice tenta `update … set kind = 'income'` sulla propria spesa (permission denied for column).
6. ERRORE: Bruno, membro, tenta `update … set kind` sulla spesa di Alice (permission denied for column).
7. Come postgres, senza set local role, `update expenses set kind = 'income'` sulla spesa: il trigger lo
   rimette (atteso: kind invariato). Lo stesso su recurring_expenses.
8. ERRORE ciascuno, sui vincoli del piano: installments senza plan_total; installments = 1;
   plan_total < amount; installments con kind = 'income'; installments con every_months = 12.
9. Alice aggiorna installments/plan_total del proprio piano (atteso: 1 riga, version 2).
10. Bruno, membro non pagante, aggiorna il piano di Alice (atteso: 0 righe, la RLS filtra).
11. Pulizia: cancella lo spazio come nel §21 di verifica-rls-ricorrenti (atteso: 0 regole e 0 spese dopo).

## Budget di complessità
Due file nuovi, nient'altro. Nessuna funzione nuova oltre alle due `create or replace` già esistenti.
Nessuna policy nuova. Nessun indice nuovo.

## Verifica
- `dotnet build` e `dotnet test` nel worktree: devono restare verdi (0 errori, 0 failed). In particolare
  `PrivilegiInsertTests`. Il GATE lo rilancia comunque la sessione con Synapse.
- Rileggi i due file e conta: ogni sezione attesa come ERRORE è nella riga «ESATTAMENTE N ERRORI».

## Formato di ritorno
`FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/<NN-slug>/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
