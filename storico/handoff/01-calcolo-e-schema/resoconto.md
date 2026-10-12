UNITÀ: 1 — ESITO: FATTO — verifica: `dotnet test Eton.sln --no-build` → Superati: 319, Non superati: 0

Branch: `worktree-01-calcolo-e-schema` (worktree `.claude/worktrees/01-calcolo-e-schema`). Non pushato su `main`: integra il capo.

TOCCATI:
- `Services/CalcoliRicorrenti.cs` → +78/−0 (nuovo)
- `Eton.Tests/CalcoliRicorrentiTests.cs` → +106/−0 (nuovo)
- `supabase/migrations/20260922000000_ricorrenti.sql` → +223/−0 (nuovo; lessicalmente l'ultimo della cartella)
- `supabase/verifica-rls-ricorrenti.sql` → +362/−0 (nuovo; scritto, **non eseguito**)
- `handoff/01-calcolo-e-schema/resoconto.md` → questo file

REVIEW:

review: A — calcolo (CalcoliRicorrenti.cs + test)
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 1
  backend-expert  RILIEVI: 2
  checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 4 · non risolti 0 · non verificabili 0

review: B — schema (migrazione + verifica RLS)
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 4
  checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 3 · non risolti 0 · non verificabili 0

Misura del gate (a implementer rientrati, dopo `git add -N .`):
`4 files changed, 769 insertions(+)` · 4 create mode · 3 dichiarazioni/endpoint → `backend-expert` dovuto, lanciato su entrambi i brief.

CONTRATTI:
- Firme del calcolo, `Services/CalcoliRicorrenti.cs`, identiche carattere per carattere al piano (task 1, «Interfacce»):
  - `:7` `public sealed record PeriodoDovuto(string Periodo, DateTime Data);`
  - `:22-24` `public static IReadOnlyList<PeriodoDovuto> Dovuti(DateTime inizio, DateTime? fine, int ogniMesi, int giorno, string? materializzatoFinoA, DateTime da, DateTime a)`
  - `:60` `public static string Periodo(DateTime data) => data.ToString("yyyy-MM", CultureInfo.InvariantCulture);`
  - `:64-65` `public static DateTime? Prossima(DateTime inizio, DateTime? fine, int ogniMesi, int giorno, DateTime oggi)`
  - Semantica che l'unità 2 deve sapere: `Dovuti` restituisce le occorrenze con **data** in `[da.Date, a.Date]` inclusi, in ordine crescente; lancia `ArgumentOutOfRangeException` se `ogniMesi` è fuori 1..12 o `giorno` fuori 1..31 (stessi intervalli dei check del database). `Prossima` ignora il watermark.
- Colonne di `public.recurring_expenses` (`20260922000000_ricorrenti.sql:13-57`), testuali:
  `id` uuid · `space_id` uuid · `paid_by` uuid · `amount` numeric(12,2) · `description` text · `category` text · `every_months` smallint · `day_of_month` smallint · `starts_on` date · `ends_on` date null · `materialized_through` text null · `version` integer · `created_at` timestamptz · `updated_at` timestamptz
  - grant INSERT (`:210`): `id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through`. Quindi `id` è `[PrimaryKey("id", true)]` come su `Expense`, e `materialized_through` **può** essere inviata in insert.
  - grant UPDATE (`:216`): `amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through`. Quindi `space_id`/`paid_by` sono `ignoreOnUpdate: true`, e `version`/`created_at`/`updated_at` sono `ignoreOnInsert: true, ignoreOnUpdate: true`.
- Colonne nuove su `public.expenses` (`:64-66`), testuali: `recurring_id` uuid null · `recurring_period` date null. Grant solo INSERT (`:223`), **nessun** UPDATE: sul modello vanno `ignoreOnUpdate: true`.
- ⚠️ **Quale data rappresenta un periodo** in `recurring_period`: **il primo giorno del mese** del periodo (`yyyy-MM-01`), **non** la data dell'occorrenza. Dal `Periodo` `"2026-09"` si scrive `2026-09-01`, anche se l'occorrenza cade il 5. Lo impone il check `expenses_recurring_period_primo_del_mese` (`:91-93`): se il client scrive la data dell'occorrenza, l'inserimento fallisce con 23514 invece di creare un doppione. Motivo: se la regola cambia giorno, la chiave del vincolo unique non cambia.
- Scrittura del client (unità 2): `Upsert` con `OnConflict = "recurring_id,recurring_period"` + `IgnoreDuplicates`. Il vincolo arbitro è `expenses_recurring_period_uq unique (recurring_id, recurring_period)` (`:82-83`), sulle stesse colonne nello stesso ordine. La sezione 12 dello script lo collauda nella forma SQL equivalente (`on conflict (recurring_id, recurring_period) do nothing` → 0 righe, nessun errore).
- La policy `expenses_insert` (`:184-193`) accetta una riga con `recurring_id` solo se la regola è **dello stesso spazio e dello stesso pagante**. Il client deve quindi materializzare solo le regole con `paid_by = utente corrente`, come già prescrive la specifica §4.3.

ADJUDICA:
- A · conformity · guardie che lanciano (`ThrowIfLessThan`/`ThrowIfGreaterThan`), estranee ai vicini che degradano → **fondato → corretto**, nella seconda forma proposta: guardie mantenute (toglierle darebbe risultati plausibili e sbagliati: `giorno` 99 troncato, `ogniMesi` negativo che si comporta da positivo) e scostamento documentato nello xmldoc; aggiunto il limite superiore 12 su `ogniMesi`.
  `ArgumentOutOfRangeException.ThrowIfGreaterThan(ogniMesi, 12);`
  verificato: risolto — Services/CalcoliRicorrenti.cs:17-21, 26-29
- A · threat-hunter · `Prossima` con `starts_on = 9999-12-01` → `da.AddMonths(3)` supera `DateTime.MaxValue` e fa cadere la pagina di un **altro** membro → **fondato → corretto** su due livelli: guardia di saturazione nel C# e check di intervallo nel database.
  `var a = da <= DateTime.MaxValue.AddMonths(-(ogniMesi + 1)) ? da.AddMonths(ogniMesi + 1) : DateTime.MaxValue.Date;`
  `starts_on    date not null check (starts_on between '2000-01-01' and '2999-12-31'),`
  verificato: risolto — Services/CalcoliRicorrenti.cs:70-77 (C#); risolto — supabase/migrations/20260922000000_ricorrenti.sql:40,46 e verifica-rls-ricorrenti.sql:195-203 (SQL)
- A · backend-expert 1 · guardie ridondanti (`a < da`, `distanza >= 0`, quattro clausole equivalenti a due) → **fondato → corretto**, equivalenza verificata.
  `if (data < inizioIterazione || data > fineIterazione) continue;`
  verificato: risolto — Services/CalcoliRicorrenti.cs:33-36, 48
- A · backend-expert 2 · ciclo con riporto manuale e annidamento a 4 livelli → **fondato → corretto**, ma **non** con la `RISCRITTURA` proposta: quella avanzava con `m.AddMonths(1)`, che va in overflow a dicembre 9999, cioè proprio sul caso del rilievo di threat-hunter. Il ciclo usa l'indice intero del mese.
  `for (var indice = inizioIterazione.Year * 12 + inizioIterazione.Month - 1; indice <= ultimo; indice++)`
  verificato: risolto — Services/CalcoliRicorrenti.cs:38-48
- B · conformity · tre insert su `recurring_expenses` (sezioni 9, 10, 10b) senza `id`, contro la regola dello script stesso → **fondato → corretto**; esiti invariati.
  `(id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through)`
  verificato: risolto — supabase/verifica-rls-ricorrenti.sql:178-202 (tutte le 5 insert su recurring_expenses e le 6 su expenses elencano l'insieme completo)
- B · backend-expert 4 (TIPO struttura) · migrazione non ordinata né per tabella né per tipo → **fondato → corretto**: solo riordino per tipo, come `spese.sql` (DDL → trigger → RLS → privilegi); conteggio degli statement invariato (10 create, 8 alter, 6 drop, 5 grant, 1 revoke).
  `-- RLS` (banner a `:152-154`)
  verificato: risolto — supabase/migrations/20260922000000_ricorrenti.sql:13,64,108,133,156,184,202,223
- B · backend-expert 1, 2, 3 (TIPO progetto) → **all'utente**, vedi FUORI SCOPE.
- Aperti da me senza intermediari, perché toccano sicurezza e dati: la migrazione intera (letta due volte, prima e dopo il riordino) e `CalcoliRicorrenti.cs`. Da quella lettura sono venute due correzioni che non vengono da rilievi: il check di intervallo sulle date (metà SQL del rilievo di threat-hunter) e `expenses.recurring_id` qualificato nella sottoquery della policy (`:190`), che prima era un `recurring_id` nudo. Non ci sono stati rilievi infondati da ricampionare.

FUORI SCOPE (rilievi `TIPO: progetto` di backend-expert: decisioni dell'utente, non fix di unità; riguardano la forma di **tutti** gli script `supabase/verifica-rls-*.sql`, non solo di questo):
1. L'esito dei `verifica-rls-*.sql` è «conta gli ERROR nell'output e confrontali con l'elenco in testa». Proposta: ogni sezione si auto-verifica con `\if :ERROR` e stampa `!!!` quando l'esito è diverso dall'atteso (a memoria di backend-expert: psql ≥ 11; da far verificare a `doc-checker`). L'esito diventerebbe «zero righe `!!!` = verde».
2. Le sezioni 1a/1b/1c sono 31 chiamate `has_column_privilege` confrontate a posizione con `t,t,…,f`. Proposta: una query su `pg_attribute` che dà una riga per colonna.
3. Gli UUID letterali ripetuti (54 nello script, 164 preamboli di impersonazione nei sette script). Proposta: variabili psql `\set alice …` e `:'jwt_bruno'`.

GATE:
dotnet build Eton.sln -warnaserror → Compilazione completata. Avvisi: 0, Errori: 0
dotnet test Eton.sln --no-build → Superato! Non superati: 0. Superati: 319. Totale: 319 (310 + 9)
La migrazione è letta dal parser di `PrivilegiInsertTests`, che passa: i grant/revoke nuovi sono riconosciuti.
**Non eseguiti, per mandato**: la migrazione e lo script di verifica RLS. Nessun agente si è connesso a un database. Lo script dichiara **10 errori attesi** (sezioni 7, 9, 10, 10b, 13, 14, 15, 16, 17, 19); il conteggio è stato ripercorso a mano da due agenti distinti (bug-hunter e checker), non da un'esecuzione.

SCOSTAMENTI:
- **Aggiunte alla specifica §3.1/§3.2, da ratificare** (sono tutte nel SQL da consegnare, e ognuna si toglie con una riga):
  1. `recurring_period` = primo del mese, con check `expenses_recurring_period_primo_del_mese`. La specifica diceva `date` senza precisare quale data: è la scelta che il mandato chiedeva di dichiarare.
  2. check `expenses_recurring_coppia`: `recurring_id` e `recurring_period` sono entrambi nulli o entrambi valorizzati. Senza, un `recurring_id` con periodo nullo sfugge al vincolo unique.
  3. check di formato su `materialized_through` (`^[0-9]{4}-(0[1-9]|1[0-2])$`): il client lo confronta come stringa.
  4. check di intervallo su `starts_on` (2000-01-01..2999-12-31) e `ends_on` (≤ 2999-12-31): dal rilievo di threat-hunter.
  5. **policy `expenses_insert` ristretta**: una spesa si aggancia solo a una regola dello stesso spazio e dello stesso pagante. È una modifica a una policy esistente di `expenses`, fatta nella migrazione nuova. Senza, un ex membro che conosce l'id di una regola potrebbe agganciarvi una spesa nel proprio spazio, e con la FK `no action` lo spazio del proprietario non si potrebbe più cancellare.
  6. `materialized_through` concessa anche in INSERT, oltre che in UPDATE (la specifica §7 nomina solo l'update): così il modello del task 3 può inviarla nulla alla creazione senza `ignoreOnInsert`.
- Nei test è stato aggiunto `namespace Eton.Tests;`, che il testo del piano non aveva: è la convenzione di tutti i file di test (`CalcoliSpeseTests.cs:4`). I nove test sono per il resto identici al piano.
- Un implementer (brief B) ha scritto ed eseguito uno **script Python temporaneo** dentro il worktree per simulare la regex di `PrivilegiInsertTests`, e poi l'ha rimosso. Non era nel brief. Il classificatore l'ha lasciato passare; lo riporto perché è un comando non richiesto. Nella correzione successiva gliel'ho vietato esplicitamente.
- Lavoro nuovo arrivato durante l'unità: nessuno.
- **Per il capo, sul gate della migrazione**: il SQL qui sotto è quello da consegnare. Resta aperta, e non è di questa unità, la verifica di `doc-checker` sulla ricarica della cache dello schema di PostgREST (`notify pgrst, 'reload schema';`), che va nei passi per l'utente.

DA CONSEGNARE ALL'UTENTE: il testo integrale di `supabase/migrations/20260922000000_ricorrenti.sql`, qui sotto.

```sql
-- =====================================================================================
-- Eton — spese ricorrenti: la regola che genera occorrenze in expenses.
-- Idempotente e rieseguibile.
--
-- Dipende dalla migrazione 20260811000000_initial_schema.sql, da cui riusa senza modificarle
-- le funzioni is_space_member() e is_space_owner(): la regola d'accesso di una regola ricorrente
-- è la stessa di una spesa, l'appartenenza allo spazio. Dipende anche da
-- 20260824000000_spese.sql, da cui riusa la tabella expenses e ne estende la funzione trigger
-- handle_expense_update().
-- =====================================================================================

-- ---------- recurring_expenses ----------
create table if not exists public.recurring_expenses (
    id           uuid primary key default gen_random_uuid(),
    space_id     uuid not null references public.spaces (id) on delete cascade,

    -- Chi anticipa i soldi ogni volta che la regola genera un'occorrenza. Stesso ruolo di
    -- expenses.paid_by, e per lo stesso motivo separato dal concetto di proprietario dello spazio.
    paid_by      uuid not null default auth.uid()
                    references auth.users (id) on delete cascade,

    amount       numeric(12,2) not null check (amount > 0),

    description  text not null check (length(btrim(description)) between 1 and 200),
    category     text not null check (length(btrim(category)) between 1 and 40),

    -- Intero e non interval, e non un enum: nessun tipo .NET rappresenta "ogni N mesi" senza
    -- ambiguità sui giorni, e un interval attraverserebbe Newtonsoft.Json sotto trimming solo con
    -- un converter scritto a mano — uno smallint è già un numero JSON, senza alcuna conversione.
    every_months smallint not null default 1 check (every_months between 1 and 12),

    -- Arriva fino a 31: il client tronca all'ultimo giorno del mese in lettura
    -- (Math.Min(day_of_month, DateTime.DaysInMonth(...))), quindi qui non serve fermarsi a 28 per
    -- restare valido anche in febbraio.
    day_of_month smallint not null check (day_of_month between 1 and 31),

    -- Una regola la scrive il pagante e la leggono tutti i membri dello spazio: una data estrema
    -- (9999-12-01) farebbe andare in overflow l'aritmetica sui mesi nel client di un altro membro.
    -- Il limite la tiene lontana dal bordo di DateTime.
    starts_on    date not null check (starts_on between '2000-01-01' and '2999-12-31'),

    -- Una regola non si cancella per smettere di generare occorrenze future: si chiude con
    -- ends_on, così le occorrenze già materializzate restano collegate (v. la FK no action più
    -- sotto su expenses.recurring_id). Un DELETE le orfanerebbe o le trascinerebbe via, a seconda
    -- della on delete scelta — nessuna delle due va bene per una spesa già registrata.
    ends_on      date null check (ends_on <= '2999-12-31'),

    -- Il watermark 'yyyy-MM' dell'ultimo periodo già generato. Il check sul formato esiste perché
    -- il client lo confronta come STRINGA, non come data: l'ordine lessicografico coincide con
    -- l'ordine cronologico solo in questo formato a larghezza fissa.
    materialized_through text null check (materialized_through ~ '^[0-9]{4}-(0[1-9]|1[0-2])$'),

    version      integer not null default 1,

    created_at   timestamptz not null default now(),
    updated_at   timestamptz not null default now()
);

-- L'unica lettura che esiste è "le regole di questo spazio", per generare le occorrenze dovute.
create index if not exists recurring_expenses_space_idx
    on public.recurring_expenses (space_id);

-- ---------- expenses: provenienza dalla regola che le ha generate ----------
alter table public.expenses
    add column if not exists recurring_id     uuid references public.recurring_expenses (id),  -- on delete no action, il default
    add column if not exists recurring_period date;

-- no action e non set null: set null perderebbe la provenienza, cioè la distinzione fra spesa
-- fissa e spesa a mano sui mesi passati. E non restrict: restrict controlla SUBITO, quindi
-- cancellare uno spazio fallirebbe perché le occorrenze già cascadate da expenses.space_id
-- esistono ancora nel momento in cui il vincolo viene verificato. no action controlla a fine
-- istruzione, quando la cascata è già completa. Conseguenza pratica: una regola che ha generato
-- occorrenze non si cancella, si chiude con ends_on.

-- ---------- vincoli su expenses ----------
alter table public.expenses drop constraint if exists expenses_recurring_period_uq;
-- Risponde a "questa occorrenza esiste già?" e arbitra le corse fra due schede che tentano di
-- materializzare lo stesso mese: il client scrive con
-- on conflict (recurring_id, recurring_period) do nothing, quindi il conflict target DEVE
-- coincidere esattamente con queste colonne. NULL sono distinti in un vincolo unique, quindi le
-- spese a mano (recurring_id null) non collidono mai fra loro.
alter table public.expenses
    add constraint expenses_recurring_period_uq unique (recurring_id, recurring_period);

alter table public.expenses drop constraint if exists expenses_recurring_period_primo_del_mese;
-- Un periodo si rappresenta col primo giorno del mese ('yyyy-MM-01'), non con la data
-- dell'occorrenza: se la regola cambia giorno (dal 5 al 10), la chiave del periodo resta la
-- stessa e il vincolo unique qui sopra continua a riconoscere l'occorrenza già scritta. Il check
-- fa fallire rumorosamente un client che scrivesse la data dell'occorrenza, invece di lasciar
-- nascere un doppione silenzioso.
alter table public.expenses
    add constraint expenses_recurring_period_primo_del_mese
    check (extract(day from recurring_period) = 1);

alter table public.expenses drop constraint if exists expenses_recurring_coppia;
-- Un recurring_id senza recurring_period sfuggirebbe al vincolo unique qui sopra, perché NULL
-- sono distinti: due occorrenze con lo stesso recurring_id e periodo null non collidono mai.
alter table public.expenses
    add constraint expenses_recurring_coppia
    check ((recurring_id is null) = (recurring_period is null));

-- ---------- versione e data di modifica ----------
-- Fotocopia di handle_expense_update() in 20260824000000_spese.sql, spiegazioni comprese: stesso
-- meccanismo, stesso motivo per cui NON è SECURITY DEFINER — i privilegi di colonna si valutano
-- sulle colonne nominate nell'UPDATE del chiamante, non su quelle che il trigger tocca dopo, e
-- un BEFORE UPDATE può quindi scrivere version anche se il chiamante non potrebbe scriverla a
-- mano. È il perno di tutto il meccanismo, qui come là.
create or replace function public.handle_recurring_expense_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Rimesse a forza al valore precedente, ultima riga di difesa come in expenses.
    new.paid_by    := old.paid_by;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;

    return new;
end;
$$;

drop trigger if exists recurring_expenses_before_update on public.recurring_expenses;
create trigger recurring_expenses_before_update
    before update on public.recurring_expenses
    for each row execute function public.handle_recurring_expense_update();

-- ---------- trigger di expenses, esteso ----------
-- Corpo identico a handle_expense_update() in 20260824000000_spese.sql, con due righe in più: la
-- provenienza di una spesa non si riscrive, esattamente come non si riscrivono paid_by e
-- space_id. Il trigger esistente non va ricreato: punta già a questa funzione.
create or replace function public.handle_expense_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Rimesse a forza al valore precedente, ultima riga di difesa (v. spese.sql per il dettaglio).
    new.paid_by    := old.paid_by;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;

    new.recurring_id     := old.recurring_id;
    new.recurring_period := old.recurring_period;

    return new;
end;
$$;

-- =====================================================================================
-- RLS
-- =====================================================================================

alter table public.recurring_expenses enable row level security;

-- Leggere: basta essere membri dello spazio, come per expenses.
drop policy if exists recurring_expenses_select on public.recurring_expenses;
create policy recurring_expenses_select on public.recurring_expenses
    for select using (public.is_space_member(space_id));

-- Creare una regola: nello spazio di cui si è membri, e solo intestata a se stessi.
drop policy if exists recurring_expenses_insert on public.recurring_expenses;
create policy recurring_expenses_insert on public.recurring_expenses
    for insert with check (public.is_space_member(space_id) and paid_by = auth.uid());

-- Modificare e cancellare: chi paga la regola, oppure il proprietario dello spazio, come per
-- expenses. with check esplicito e identico a using per lo stesso motivo: leggibilità.
drop policy if exists recurring_expenses_update on public.recurring_expenses;
create policy recurring_expenses_update on public.recurring_expenses
    for update using      (paid_by = auth.uid() or public.is_space_owner(space_id))
              with check  (paid_by = auth.uid() or public.is_space_owner(space_id));

drop policy if exists recurring_expenses_delete on public.recurring_expenses;
create policy recurring_expenses_delete on public.recurring_expenses
    for delete using (paid_by = auth.uid() or public.is_space_owner(space_id));

-- ---------- policy di insert su expenses, ristretta ----------
-- Senza la condizione aggiunta, chi conosce l'id di una regola altrui (per esempio un ex membro
-- dello spazio del proprietario) potrebbe agganciarvi una spesa nel proprio spazio; con la FK no
-- action quella spesa impedirebbe per sempre di cancellare lo spazio del proprietario della
-- regola. Si materializza solo le proprie regole, nello stesso spazio.
drop policy if exists expenses_insert on public.expenses;
create policy expenses_insert on public.expenses
    for insert with check (
        public.is_space_member(space_id) and paid_by = auth.uid()
        and (recurring_id is null or exists (
                select 1 from public.recurring_expenses r
                 where r.id = expenses.recurring_id
                   and r.space_id = expenses.space_id
                   and r.paid_by = auth.uid()))
    );

-- =====================================================================================
-- Privilegi.
--
-- La RLS FILTRA, non concede: senza i GRANT qui sotto ogni query fallirebbe con
-- "permission denied for table recurring_expenses" prima ancora che una policy venga consultata.
-- =====================================================================================

revoke all on public.recurring_expenses from anon, authenticated;

grant select, delete on public.recurring_expenses to authenticated;

-- id in inserimento per lo stesso motivo di expenses: lo genera il client, ed è la fondazione
-- del ritentativo innocuo dopo una rete che va e viene. materialized_through è presente perché il
-- modello la invia (null) alla creazione — un valore sbagliato in creazione danneggia solo la
-- propria regola, e i doppioni li ferma comunque il vincolo unique su expenses.
grant insert (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through)
    on public.recurring_expenses to authenticated;

-- In aggiornamento il client tocca il contenuto e materialized_through, che lo avanza dopo aver
-- materializzato un'occorrenza. Mai version/created_at/updated_at, li scrive solo il trigger. Mai
-- paid_by/space_id: cambiarli sposterebbe la regola sotto un'altra visibilità.
grant update (amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through)
    on public.recurring_expenses to authenticated;

grant all on public.recurring_expenses to service_role;

-- Nessun grant update gemello, deliberatamente: la provenienza di una spesa si scrive una volta
-- sola, alla materializzazione, e non si riscrive più (v. handle_expense_update() più sopra).
grant insert (recurring_id, recurring_period) on public.expenses to authenticated;
```
