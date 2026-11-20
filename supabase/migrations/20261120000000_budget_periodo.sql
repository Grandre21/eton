-- =====================================================================================
-- Eton — budget: descrizione libera e divieto di periodi sovrapposti.
-- Idempotente e rieseguibile, additiva: il client già pubblicato non legge né scrive
-- description, e il default '' copre i suoi INSERT.
--
-- Dipende da 20261024000000_budget.sql per la tabella category_budgets, il suo indice
-- unico category_budgets_aperto_uq, le policy e i grant di colonna.
--
-- Due blocchi indipendenti: il BLOCCO II si può togliere per intero senza toccare il
-- BLOCCO I.
-- =====================================================================================

-- =====================================================================================
-- BLOCCO I — descrizione
-- =====================================================================================
alter table public.category_budgets add column if not exists description text not null default '';

alter table public.category_budgets drop constraint if exists category_budgets_description_max;
alter table public.category_budgets
    add constraint category_budgets_description_max check (char_length(description) <= 200);

-- Forma minima come 20260903000000_grant_insert_blind.sql: una sola colonna, senza
-- ripetere l'elenco completo di 20261024000000_budget.sql, e INSERT e UPDATE affiancati.
-- Nessun altro grant e nessun revoke. Il trigger handle_category_budget_update non si
-- tocca: description resta modificabile, come amount.
grant insert (description) on public.category_budgets to authenticated;
grant update (description) on public.category_budgets to authenticated;

-- =====================================================================================
-- BLOCCO II — nessun periodo sovrapposto; si può togliere per intero
-- =====================================================================================
-- btree_gist serve per usare `=` su uuid e text dentro un indice GiST.
create extension if not exists btree_gist with schema extensions;

-- Controllo preliminare: se i dati già presenti contengono periodi sovrapposti per lo
-- stesso spazio e la stessa categoria, il vincolo non si può aggiungere. L'espressione
-- del periodo è la stessa del vincolo qui sotto.
do $$
declare
    coppie bigint;
begin
    select count(*) into coppie
      from public.category_budgets a
      join public.category_budgets b
        on a.id < b.id
       and a.space_id = b.space_id
       and a.category = b.category
       and daterange(a.starts_on, (a.ends_on + interval '1 month')::date, '[)')
        && daterange(b.starts_on, (b.ends_on + interval '1 month')::date, '[)');
    if coppie > 0 then
        raise exception 'Trovate % coppie di budget con periodi sovrapposti per lo stesso spazio e la stessa categoria: il vincolo non si può aggiungere finché non sono corrette.', coppie;
    end if;
end;
$$;

-- Il periodo va da starts_on al giorno prima del mese successivo a ends_on, quindi
-- l'ultimo giorno del mese di ends_on è incluso. Con ends_on null il limite superiore è
-- null, cioè senza fine. Con '[)' due budget contigui (uno finisce a gennaio, l'altro
-- inizia a febbraio) non si toccano.
-- L'indice unico category_budgets_aperto_uq resta e le policy non cambiano. Una
-- violazione dà il codice 23P01 (exclusion_violation).
alter table public.category_budgets drop constraint if exists category_budgets_no_overlap;
alter table public.category_budgets
    add constraint category_budgets_no_overlap
    exclude using gist (
        space_id with =,
        category with =,
        daterange(starts_on, (ends_on + interval '1 month')::date, '[)') with &&
    );
