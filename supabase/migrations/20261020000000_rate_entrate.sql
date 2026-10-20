-- =====================================================================================
-- Eton — rate ed entrate: il tipo di un movimento e il piano a rate di una regola ricorrente.
-- Idempotente e rieseguibile.
--
-- Dipende da 20260824000000_spese.sql e 20260922000000_ricorrenti.sql, di cui estende
-- le due funzioni trigger. È additiva: un client già pubblicato, che non manda kind,
-- scrive una spesa grazie al default.
-- =====================================================================================

-- ---------- tipo del movimento ----------
-- Le entrate sono importi positivi con un tipo, non importi negativi: amount > 0 resta.
alter table public.expenses add column if not exists kind text not null default 'expense';
alter table public.recurring_expenses add column if not exists kind text not null default 'expense';

alter table public.expenses drop constraint if exists expenses_kind_valido;
alter table public.expenses
    add constraint expenses_kind_valido check (kind in ('expense','income'));
alter table public.recurring_expenses drop constraint if exists recurring_expenses_kind_valido;
alter table public.recurring_expenses
    add constraint recurring_expenses_kind_valido check (kind in ('expense','income'));

-- ---------- piano a rate ----------
alter table public.recurring_expenses
    add column if not exists installments smallint null,
    add column if not exists plan_total numeric(12,2) null;

-- La coppia deve essere completa: un solo valore non descriverebbe un piano utilizzabile.
alter table public.recurring_expenses drop constraint if exists recurring_expenses_piano_coppia;
alter table public.recurring_expenses
    add constraint recurring_expenses_piano_coppia
    check ((installments is null) = (plan_total is null));

-- Con null i check sono soddisfatti (UNKNOWN è ammesso dalla semantica SQL): una regola
-- senza piano li passa tutti. Un piano presente richiede almeno due rate e un totale
-- sufficiente per tutte le rate; si applica soltanto alle spese mensili.
alter table public.recurring_expenses drop constraint if exists recurring_expenses_installments_min;
alter table public.recurring_expenses
    add constraint recurring_expenses_installments_min check (installments >= 2);
alter table public.recurring_expenses drop constraint if exists recurring_expenses_plan_total_min;
-- L'ultima rata vale plan_total − amount × (installments − 1) e deve restare positiva.
-- Con il solo plan_total >= amount, un UPDATE di amount da client vecchio o API diretta
-- può portarla a zero o sotto: materializzarla fallirebbe per sempre su expenses.amount > 0.
-- Il client divide il totale per N per difetto, quindi amount × N ≤ totale vale sempre.
-- Con installments null il check è soddisfatto: UNKNOWN è ammesso dalla semantica SQL.
alter table public.recurring_expenses
    add constraint recurring_expenses_plan_total_min check (plan_total >= amount * installments);
alter table public.recurring_expenses drop constraint if exists recurring_expenses_piano_solo_spese_mensili;
alter table public.recurring_expenses
    add constraint recurring_expenses_piano_solo_spese_mensili
    check (installments is null or (kind = 'expense' and every_months = 1));

-- ---------- trigger di expenses, esteso ----------
-- Un movimento non cambia tipo: per cambiarlo lo si cancella e lo si riscrive.
-- Il trigger esistente punta già a questa funzione e non va ricreato.
-- Non SECURITY DEFINER: come prima, il BEFORE UPDATE modifica NEW in memoria.
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
    new.kind := old.kind;

    return new;
end;
$$;

-- ---------- trigger di recurring_expenses, esteso ----------
-- Stessa ultima riga di difesa sul tipo; installments e plan_total restano aggiornabili.
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
    new.kind := old.kind;

    return new;
end;
$$;

-- ---------- tipo dell'occorrenza ----------
-- Il client già pubblicato materializza senza kind: una regola income produrrebbe una
-- expense per default, senza poterla più correggere (nessun UPDATE su kind e il trigger
-- di update lo rimette). Copiarlo qui dà sempre all'occorrenza il tipo della sua regola,
-- qualunque cosa mandi il client.
-- Non è SECURITY DEFINER, di proposito: la SELECT passa dalla RLS di chi inserisce.
-- La policy di insert su expenses già pretende il pagante della regola, quindi una regola
-- legittima è sempre leggibile. Se non è leggibile o non esiste, l'INSERT fallisce invece
-- di passare con il tipo di default. Il BEFORE scatta prima della with check della policy
-- e del controllo di on conflict, ed è innocuo con do nothing.
create or replace function public.handle_expense_insert()
returns trigger language plpgsql
set search_path = public as $$
begin
    if new.recurring_id is not null then
        select r.kind into new.kind
          from public.recurring_expenses r
         where r.id = new.recurring_id;
        if not found then
            raise exception 'regola ricorrente % non trovata o non leggibile', new.recurring_id
                using errcode = '23503';
        end if;
    end if;
    return new;
end;
$$;

drop trigger if exists expenses_before_insert on public.expenses;
create trigger expenses_before_insert
    before insert on public.expenses
    for each row execute function public.handle_expense_insert();

-- ---------- privilegi aggiuntivi ----------
-- Niente grant update su kind, di proposito: il tipo si decide solo alla creazione.
-- L'UPDATE del piano serve a modificarlo prima che parta; il blocco a piano avviato
-- lo fa il client. Si aggiungono solo i privilegi delle nuove colonne.
grant insert (kind) on public.expenses to authenticated;
grant insert (kind, installments, plan_total) on public.recurring_expenses to authenticated;
grant update (installments, plan_total) on public.recurring_expenses to authenticated;
