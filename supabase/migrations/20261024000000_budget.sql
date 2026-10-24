-- =====================================================================================
-- Eton — budget per categoria e tetto alle rate delle regole ricorrenti.
-- Idempotente e rieseguibile, additiva: il client già pubblicato non legge la nuova tabella.
--
-- Dipende da 20260811000000_initial_schema.sql, da cui riusa senza modificarle
-- is_space_member() e is_space_owner(): la visibilità segue l'appartenenza allo spazio.
-- Dipende anche da 20261020000000_rate_entrate.sql per la colonna installments.
-- =====================================================================================

-- ---------- category_budgets ----------
create table if not exists public.category_budgets (
    id           uuid primary key default gen_random_uuid(),
    space_id     uuid not null references public.spaces (id) on delete cascade,

    -- Il budget non ha un pagante: l'autore serve a distinguere chi può modificarlo.
    created_by   uuid not null default auth.uid()
                    references auth.users (id) on delete cascade,
    category     text not null check (length(btrim(category)) between 1 and 40),
    amount       numeric(12,2) not null check (amount > 0),

    -- Il limite evita date estreme che manderebbero in overflow i calcoli mensili del client.
    starts_on    date not null check (starts_on between '2000-01-01' and '2999-12-31'),

    -- Un budget è una regola valida ogni mese da starts_on finché non si chiude.
    -- ends_on è l'ultimo mese in cui vale, rappresentato dal suo primo giorno:
    -- chiuderlo significa scrivere ends_on, non cancellare la storia della regola.
    ends_on      date null check (ends_on <= '2999-12-31'),
    version      integer not null default 1,
    created_at   timestamptz not null default now(),
    updated_at   timestamptz not null default now(),

    -- Il periodo è mensile: usare sempre il primo giorno evita confini ambigui nel client.
    constraint category_budgets_starts_on_primo_del_mese check (extract(day from starts_on) = 1),
    -- Con ends_on NULL i check sono soddisfatti (UNKNOWN è ammesso): il budget resta aperto.
    constraint category_budgets_ends_on_primo_del_mese   check (extract(day from ends_on) = 1),
    constraint category_budgets_ends_on_dopo_inizio      check (ends_on >= starts_on)
);

-- La lettura dei budget parte sempre dallo spazio corrente.
create index if not exists category_budgets_space_idx
    on public.category_budgets (space_id);
-- Al più una regola aperta per categoria e spazio; chiuderla permette di aprirne una nuova.
create unique index if not exists category_budgets_aperto_uq
    on public.category_budgets (space_id, category) where ends_on is null;

-- ---------- versione e data di modifica ----------
-- Non SECURITY DEFINER: come handle_recurring_expense_update(), il BEFORE scrive NEW senza grant di colonna.
create or replace function public.handle_category_budget_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Ultima riga di difesa: identità e periodo iniziale si decidono solo alla creazione.
    new.created_by := old.created_by;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;
    new.category   := old.category;
    new.starts_on  := old.starts_on;

    return new;
end;
$$;

drop trigger if exists category_budgets_before_update on public.category_budgets;
create trigger category_budgets_before_update
    before update on public.category_budgets
    for each row execute function public.handle_category_budget_update();

-- =====================================================================================
-- RLS
-- =====================================================================================
alter table public.category_budgets enable row level security;

-- Tutti i membri leggono il budget condiviso, come le regole ricorrenti.
drop policy if exists category_budgets_select on public.category_budgets;
create policy category_budgets_select on public.category_budgets
    for select using (public.is_space_member(space_id));

-- Un membro può creare solo regole intestate a se stesso.
drop policy if exists category_budgets_insert on public.category_budgets;
create policy category_budgets_insert on public.category_budgets
    for insert with check (public.is_space_member(space_id) and created_by = auth.uid());

-- Autore e proprietario possono gestire la regola; with check esplicito per leggibilità.
drop policy if exists category_budgets_update on public.category_budgets;
create policy category_budgets_update on public.category_budgets
    for update using (created_by = auth.uid() or public.is_space_owner(space_id))
              with check (created_by = auth.uid() or public.is_space_owner(space_id));
drop policy if exists category_budgets_delete on public.category_budgets;
create policy category_budgets_delete on public.category_budgets
    for delete using (created_by = auth.uid() or public.is_space_owner(space_id));

-- =====================================================================================
-- Privilegi: la RLS filtra, non concede; senza GRANT la query fallisce prima delle policy.
-- =====================================================================================
revoke all on public.category_budgets from anon, authenticated;
grant select, delete on public.category_budgets to authenticated;
-- id lo genera il client, per poter ritentare un inserimento dopo un errore di rete.
grant insert (id, space_id, created_by, category, amount, starts_on, ends_on)
    on public.category_budgets to authenticated;
-- Si aggiornano solo importo e chiusura. Mai version/created_at/updated_at: li scrive il trigger.
-- La concorrenza ottimistica usa version come filtro nel WHERE, che richiede solo SELECT.
-- Mai space_id/created_by: sposterebbero il budget sotto un'altra visibilità.
-- Mai category/starts_on: si decidono alla creazione, come kind nei movimenti.
grant update (amount, ends_on) on public.category_budgets to authenticated;
grant all on public.category_budgets to service_role;

-- ---------- tetto alle rate ----------
-- Il client limita già i piani a 60 rate: si rifiuta solo ciò che già rifiuta il client.
-- Il vincolo difende anche dall'API diretta e dai client vecchi; NULL resta senza piano.
do $$
declare
    righe bigint;
    massimo smallint;
begin
    select count(*), max(installments) into righe, massimo
      from public.recurring_expenses where installments > 60;
    if righe > 0 then
        raise exception 'Trovati % piani con più di 60 rate (massimo %): il vincolo non si può aggiungere finché quei piani non sono corretti.', righe, massimo;
    end if;
end;
$$;

alter table public.recurring_expenses drop constraint if exists recurring_expenses_installments_max;
alter table public.recurring_expenses
    add constraint recurring_expenses_installments_max check (installments is null or installments <= 60);
