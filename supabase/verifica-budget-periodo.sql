-- =====================================================================================
-- Eton -- verifica descrizione e periodi non sovrapposti dei budget dopo 20261120000000_budget_periodo.sql.
-- Collauda privilegi, vincoli, sovrapposizioni, contiguita, budget aperti e cancellazione in cascata.
-- Utenti finti in auth.users, impersonati con set local request.jwt.claims.
-- QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 4 ERRORI: le sezioni 4a, 4c (seconda parte), 4f, 4g.
-- Ogni errore ha una transazione propria con rollback.
-- Come si esegue, dopo il reset del database locale:
-- supabase db reset
-- docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-budget-periodo.sql
-- Gli utenti sono diversi da quelli di verifica-rls-budget.sql: i due script possono girare uno dopo l'altro.
-- Alla seconda esecuzione senza reset gli utenti esistono gia': un errore in piu'.
-- Senza accenti per attraversare il terminale Windows e psql nel container Linux.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

-- I claims restano qui per riusare le identita anche eseguendo lo script da standard input.
\set claims_alice '{"sub":"a5500000-0000-0000-0000-000000000001","role":"authenticated"}'
\set claims_bruno '{"sub":"b6600000-0000-0000-0000-000000000002","role":"authenticated"}'
\set claims_carla '{"sub":"c7700000-0000-0000-0000-000000000003","role":"authenticated"}'

\echo '=== 1. Privilegi e vincoli (atteso: description INSERT t e UPDATE t, poi 5 f; poi 2 vincoli; poi btree_gist nello schema extensions) ==='
select has_column_privilege('authenticated','public.category_budgets','description','INSERT') as description_ins,
       has_column_privilege('authenticated','public.category_budgets','description','UPDATE') as description_upd,
       has_column_privilege('authenticated','public.category_budgets','space_id','UPDATE') as space_id_upd,
       has_column_privilege('authenticated','public.category_budgets','created_by','UPDATE') as created_by_upd,
       has_column_privilege('authenticated','public.category_budgets','category','UPDATE') as category_upd,
       has_column_privilege('authenticated','public.category_budgets','starts_on','UPDATE') as starts_on_upd,
       has_column_privilege('authenticated','public.category_budgets','version','UPDATE') as version_upd;

select conname from pg_constraint
 where conrelid = 'public.category_budgets'::regclass
   and conname in ('category_budgets_description_max','category_budgets_no_overlap')
 order by conname;

select e.extname, n.nspname as schema
  from pg_extension e join pg_namespace n on n.oid = e.extnamespace
 where e.extname = 'btree_gist';

\echo '=== 2. Preparazione (atteso: 3 utenti, Alice e Bruno nello spazio condiviso, Carla nel suo) ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('a5500000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice-periodo@esempio.it','{"full_name":"Alice Periodo"}'::jsonb,now(),now()),
       ('b6600000-0000-0000-0000-000000000002','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno-periodo@esempio.it','{"full_name":"Bruno Periodo"}'::jsonb,now(),now()),
       ('c7700000-0000-0000-0000-000000000003','00000000-0000-0000-0000-000000000000','authenticated','authenticated','carla-periodo@esempio.it','{"full_name":"Carla Periodo"}'::jsonb,now(),now());

\echo '=== Alice crea lo spazio (atteso: uno spazio) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
select public.create_space('Periodi condivisi') as condiviso \gset
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
select public.create_space('Spazio Carla periodo') as spazio_carla \gset
commit;

\echo '=== 3. Alice crea Spesa da 2026-01-01 a 2026-03-01 con descrizione Trimestre (atteso: 1 riga, description Trimestre) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on, description)
values ('d8800000-0000-0000-0000-000000000001', :'condiviso', 'a5500000-0000-0000-0000-000000000001', 'Spesa', 100.00, '2026-01-01', '2026-03-01', 'Trimestre') returning id, description;
commit;

\echo '=== 4a. Periodo sovrapposto: Spesa da 2026-03-01 a 2026-05-01 (atteso: ERRORE exclusion category_budgets_no_overlap) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a5500000-0000-0000-0000-000000000001', 'Spesa', 100.00, '2026-03-01', '2026-05-01');
rollback;

\echo '=== 4b. Periodo contiguo: Spesa da 2026-04-01 a 2026-06-01 (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a5500000-0000-0000-0000-000000000001', 'Spesa', 100.00, '2026-04-01', '2026-06-01') returning id, starts_on, ends_on;
commit;

\echo '=== 4c. Budget aperto: Spesa aperta da 2026-07-01 (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a5500000-0000-0000-0000-000000000001', 'Spesa', 100.00, '2026-07-01', null) returning id, starts_on, ends_on;
commit;

\echo '=== 4c (seconda parte). Spesa chiusa da 2027-01-01 a 2027-02-01, dopo un budget aperto (atteso: ERRORE exclusion category_budgets_no_overlap) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a5500000-0000-0000-0000-000000000001', 'Spesa', 100.00, '2027-01-01', '2027-02-01');
rollback;

\echo '=== 4d. Stessa categoria in un altro spazio: Carla crea Spesa aperta da 2026-01-01 (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'spazio_carla', 'c7700000-0000-0000-0000-000000000003', 'Spesa', 100.00, '2026-01-01', null) returning id, starts_on, ends_on;
commit;

\echo '=== 4e. Altra categoria, stesso periodo, spazio condiviso: Casa da 2026-01-01 a 2026-03-01 (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a5500000-0000-0000-0000-000000000001', 'Casa', 100.00, '2026-01-01', '2026-03-01') returning id, category;
commit;

\echo '=== 4f. Alice allunga il budget di Trimestre fino a 2026-04-01, dentro il successivo (atteso: ERRORE exclusion category_budgets_no_overlap) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set ends_on = '2026-04-01' where id = 'd8800000-0000-0000-0000-000000000001' returning id, ends_on;
rollback;

\echo '=== 4g. Descrizione di 201 caratteri (atteso: ERRORE check category_budgets_description_max) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on, description)
values (gen_random_uuid(), :'condiviso', 'a5500000-0000-0000-0000-000000000001', 'Lunga', 100.00, '2026-01-01', null, repeat('x', 201));
rollback;

\echo '=== 4h. Alice porta la descrizione di Trimestre a 200 caratteri (atteso: 1 riga, lunghezza 200) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.category_budgets set description = repeat('x', 200) where id = 'd8800000-0000-0000-0000-000000000001' returning id, char_length(description) as lunghezza;
commit;

\echo '=== 4i. Insert senza la colonna description, come il client pubblicato: Svago aperta da 2026-01-01 (atteso: 1 riga, description vuota) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.category_budgets (id, space_id, created_by, category, amount, starts_on, ends_on)
values (gen_random_uuid(), :'condiviso', 'a5500000-0000-0000-0000-000000000001', 'Svago', 100.00, '2026-01-01', null) returning id, description;
commit;

\echo '=== 5. Pulizia condiviso (atteso: 5 budget prima, 1 spazio cancellato, 0 budget dopo) ==='
select count(*) as budget_prima from public.category_budgets where space_id = :'condiviso';

\echo '=== Alice cancella lo spazio (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
delete from public.spaces where id = :'condiviso' returning id;
commit;
select count(*) as budget_dopo from public.category_budgets where space_id = :'condiviso';

\echo '=== Pulizia Carla (atteso: 1 budget prima, 1 spazio cancellato, 0 budget dopo) ==='
select count(*) as budget_carla_prima from public.category_budgets where space_id = :'spazio_carla';

\echo '=== Carla cancella lo spazio (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
delete from public.spaces where id = :'spazio_carla' returning id;
commit;
select count(*) as budget_carla_dopo from public.category_budgets where space_id = :'spazio_carla';

\echo '=== 6. FINE. Riepilogo di cosa doveva succedere ==='
\echo '1: description INSERT t e UPDATE t, poi 5 f; due vincoli; btree_gist in extensions'
\echo '2: tre utenti e due spazi; 3: una riga con description Trimestre'
\echo '4a: ERRORE exclusion; 4b: una riga contigua; 4c: una riga aperta; 4c seconda parte: ERRORE exclusion'
\echo '4d: una riga; 4e: una riga; 4f: ERRORE exclusion; 4g: ERRORE check description_max'
\echo '4h: una riga lunghezza 200; 4i: una riga con description vuota'
\echo '5: condiviso cinque budget prima e zero dopo; Carla uno prima e zero dopo; due spazi cancellati'
\echo 'QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 4 ERRORI: le sezioni 4a, 4c (seconda parte), 4f, 4g.'
