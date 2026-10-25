-- =====================================================================================
-- Eton -- verifica budget e tetto rate dopo 20261024000000_budget.sql.
-- Collauda privilegi, vincoli, concorrenza, RLS, trigger e cancellazione in cascata.
-- Utenti finti in auth.users, impersonati con set local request.jwt.claims.
-- QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 14 ERRORI: le sezioni 4a, 4b, 4c, 4d, 4e, 4f, 4g, 5b, 6a, 6b, 6c, 6d, 6e, 11a.
-- Ogni errore ha una transazione propria con rollback.
-- Come si esegue, dopo il reset del database locale:
-- supabase db reset
-- docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-budget.sql
-- Alla seconda esecuzione senza reset gli utenti esistono gia': un errore in piu'.
-- Senza accenti per attraversare il terminale Windows e psql nel container Linux.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

-- I claims restano qui per riusare le identita anche eseguendo lo script da standard input.
\set claims_alice '{"sub":"a1100000-0000-0000-0000-000000000001","role":"authenticated"}'
\set claims_bruno '{"sub":"b2200000-0000-0000-0000-000000000002","role":"authenticated"}'
\set claims_carla '{"sub":"c3300000-0000-0000-0000-000000000003","role":"authenticated"}'

\echo '=== 1. Privilegi (atteso: 8 t, 6 f, poi RLS t) ==='
select has_column_privilege('authenticated','public.category_budgets','space_id','INSERT') as space_id_ins,
       has_column_privilege('authenticated','public.category_budgets','created_by','INSERT') as created_by_ins,
       has_column_privilege('authenticated','public.category_budgets','category','INSERT') as category_ins,
       has_column_privilege('authenticated','public.category_budgets','starts_on','INSERT') as starts_on_ins,
       has_column_privilege('authenticated','public.category_budgets','amount','INSERT') as amount_ins,
       has_column_privilege('authenticated','public.category_budgets','ends_on','INSERT') as ends_on_ins,
       has_column_privilege('authenticated','public.category_budgets','amount','UPDATE') as amount_upd,
       has_column_privilege('authenticated','public.category_budgets','ends_on','UPDATE') as ends_on_upd,
       has_column_privilege('authenticated','public.category_budgets','space_id','UPDATE') as space_id_upd,
       has_column_privilege('authenticated','public.category_budgets','created_by','UPDATE') as created_by_upd,
       has_column_privilege('authenticated','public.category_budgets','category','UPDATE') as category_upd,
       has_column_privilege('authenticated','public.category_budgets','starts_on','UPDATE') as starts_on_upd,
       has_column_privilege('authenticated','public.category_budgets','version','UPDATE') as version_upd,
       has_table_privilege('anon','public.category_budgets','SELECT') as anon_select,
       (select relrowsecurity from pg_class where oid = 'public.category_budgets'::regclass) as rls;

\echo '=== 2. Preparazione (atteso: 3 utenti, Alice e Bruno nello spazio condiviso, Carla nel suo) ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('a1100000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice-budget@esempio.it','{"full_name":"Alice Budget"}'::jsonb,now(),now()),
       ('b2200000-0000-0000-0000-000000000002','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno-budget@esempio.it','{"full_name":"Bruno Budget"}'::jsonb,now(),now()),
       ('c3300000-0000-0000-0000-000000000003','00000000-0000-0000-0000-000000000000','authenticated','authenticated','carla-budget@esempio.it','{"full_name":"Carla Budget"}'::jsonb,now(),now());

\echo '=== Alice crea lo spazio (atteso: uno spazio) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
select public.create_space('Budget condivisi') as condiviso \gset
commit;
-- Fuori RLS: Bruno non legge il codice prima di entrare.
select invite_code as codice from public.spaces where id = :'condiviso' \gset

\echo '=== Bruno entra (atteso: uno spazio) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_bruno';
select public.join_space(:'codice') as entrato;
commit;

\echo '=== Carla crea lo spazio (atteso: uno spazio) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
select public.create_space('Spazio Carla budget') as spazio_carla \gset
commit;

\echo '=== 3. Alice crea Uscite (atteso: 1 riga, version 1, created_by Alice) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values ('d4400000-0000-0000-0000-000000000001', :'condiviso', 'a1100000-0000-0000-0000-000000000001', 'Uscite', 50.00, '2026-10-01', null) returning id, version, created_by;
commit;

\echo '=== 4a. Inserimento non valido (atteso: ERRORE unique category_budgets_aperto_uq) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', 'Uscite', 50.00, '2026-10-01', null);
rollback;

\echo '=== 4b. Inserimento non valido (atteso: ERRORE check amount) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', 'Zero', 0, '2026-10-01', null);
rollback;

\echo '=== 4c. Inserimento non valido (atteso: ERRORE check category_budgets_starts_on_primo_del_mese) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', 'Inizio', 50, '2026-10-15', null);
rollback;

\echo '=== 4d. Inserimento non valido (atteso: ERRORE check category_budgets_ends_on_dopo_inizio) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', 'Fine prima', 50, '2026-10-01', '2026-09-01');
rollback;

\echo '=== 4e. Inserimento non valido (atteso: ERRORE check category_budgets_ends_on_primo_del_mese) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', 'Fine giorno', 50, '2026-10-01', '2026-11-15');
rollback;

\echo '=== 4f. Inserimento non valido (atteso: ERRORE check category) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', '   ', 50.00, '2026-10-01', null);
rollback;

\echo '=== 4g. Inserimento non valido (atteso: ERRORE RLS) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'b2200000-0000-0000-0000-000000000002', 'Autore falso', 50, '2026-10-01', null);
rollback;

\echo '=== 5a. Carla legge (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
select * from public.category_budgets where space_id = :'condiviso';
commit;

\echo '=== 5b. Carla inserisce nello spazio condiviso (atteso: ERRORE RLS) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'c3300000-0000-0000-0000-000000000003', 'Estranea', 50, '2026-10-01', null);
rollback;

\echo '=== 5c. Carla aggiorna (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
update public.category_budgets set amount = 70 where id = 'd4400000-0000-0000-0000-000000000001' returning id;
commit;

\echo '=== 5d. Carla cancella (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
delete from public.category_budgets where id = 'd4400000-0000-0000-0000-000000000001' returning id;
commit;

\echo '=== 6a. Alice cambia space_id (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set space_id = :'spazio_carla' where id = 'd4400000-0000-0000-0000-000000000001';
rollback;

\echo '=== 6b. Alice cambia created_by (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set created_by = 'b2200000-0000-0000-0000-000000000002' where id = 'd4400000-0000-0000-0000-000000000001';
rollback;

\echo '=== 6c. Alice cambia category (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set category = 'Altro' where id = 'd4400000-0000-0000-0000-000000000001';
rollback;

\echo '=== 6d. Alice cambia starts_on (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set starts_on = '2026-11-01' where id = 'd4400000-0000-0000-0000-000000000001';
rollback;

\echo '=== 6e. Alice cambia version (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set version = 99 where id = 'd4400000-0000-0000-0000-000000000001';
rollback;

\echo '=== 7. Concorrenza (atteso: 1 riga version 2, poi 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set amount = 60.00 where id = 'd4400000-0000-0000-0000-000000000001' and version = 1 returning version;
update public.category_budgets set amount = 60.00 where id = 'd4400000-0000-0000-0000-000000000001' and version = 1 returning version;
commit;

\echo '=== 8a. Bruno legge il budget di Alice (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_bruno';
select * from public.category_budgets where id = 'd4400000-0000-0000-0000-000000000001';
commit;

\echo '=== 8b. Bruno aggiorna il budget di Alice (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_bruno';
update public.category_budgets set amount = 80 where id = 'd4400000-0000-0000-0000-000000000001' returning id;
commit;

\echo '=== 8c. Bruno crea Ristoranti (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_bruno';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values ('d4400000-0000-0000-0000-000000000002', :'condiviso', 'b2200000-0000-0000-0000-000000000002', 'Ristoranti', 50, '2026-10-01', null) returning id;
commit;

\echo '=== 8d. Alice proprietaria chiude il budget di Bruno (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set ends_on = '2026-10-01' where id = 'd4400000-0000-0000-0000-000000000002' returning id, ends_on;
commit;

\echo '=== 9. Chiusura e riapertura Uscite (atteso: 1 riga chiusa e 1 nuova aperta) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set ends_on = '2026-10-01' where id = 'd4400000-0000-0000-0000-000000000001' returning id, ends_on;
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', 'Uscite', 50, '2026-11-01', null) returning id, ends_on;
commit;

\echo '=== 10. Postgres: ultima difesa (atteso: spazio condiviso, Alice, Uscite, 2026-10-01 invariati) ==='
begin;
update public.category_budgets
   set space_id = :'spazio_carla', created_by = 'c3300000-0000-0000-0000-000000000003', category = 'Riscritto', starts_on = '2027-01-01'
 where id = 'd4400000-0000-0000-0000-000000000001'
returning space_id, created_by, category, starts_on;
commit;

\echo '=== 11a. Piano 61 rate (atteso: ERRORE check recurring_expenses_installments_max) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', 10.00, 'Piano budget', 'Uscite', 1, 1, '2026-10-01', null, null, 'expense', 61, 610.00);
rollback;

\echo '=== 11b. Piano 60 rate (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values (gen_random_uuid(), :'condiviso', 'a1100000-0000-0000-0000-000000000001', 10.00, 'Piano budget', 'Uscite', 1, 1, '2026-10-01', null, null, 'expense', 60, 600.00) returning id, installments;
commit;

\echo '=== 12. Pulizia condiviso (atteso: 3 budget prima, 1 spazio cancellato, 0 budget dopo) ==='
select count(*) as budget_prima from public.category_budgets where space_id = :'condiviso';

\echo '=== Alice cancella lo spazio (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
delete from public.spaces where id = :'condiviso' returning id;
commit;
select count(*) as budget_dopo from public.category_budgets where space_id = :'condiviso';

\echo '=== Pulizia Carla (atteso: 0 budget prima, 1 spazio cancellato, 0 budget dopo) ==='
select count(*) as budget_carla_prima from public.category_budgets where space_id = :'spazio_carla';

\echo '=== Carla cancella lo spazio (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
delete from public.spaces where id = :'spazio_carla' returning id;
commit;
select count(*) as budget_carla_dopo from public.category_budgets where space_id = :'spazio_carla';

\echo '=== 13. FINE. Riepilogo di cosa doveva succedere ==='
\echo '1: 8 t, 6 f, RLS t; 2: tre utenti e due spazi; 3: una riga version 1 di Alice'
\echo '4a: ERRORE unique; 4b-4f: ERRORE check; 4g e 5b: ERRORE RLS'
\echo '5a, 5c, 5d: zero righe; 6a-6e: ERRORE permission denied'
\echo '7: una riga version 2, poi zero; 8a: una riga; 8b: zero; 8c e 8d: una riga'
\echo '9: una chiusura e una nuova regola aperta; 10: identita e inizio invariati'
\echo '11a: ERRORE tetto 60 rate; 11b: una riga con 60 rate'
\echo '12: condiviso tre budget prima e zero dopo; Carla zero prima e dopo; due spazi cancellati'
\echo 'QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 14 ERRORI: le sezioni 4a, 4b, 4c, 4d, 4e, 4f, 4g, 5b, 6a, 6b, 6c, 6d, 6e, 11a.'
