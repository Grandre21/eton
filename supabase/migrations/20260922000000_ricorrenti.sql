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
