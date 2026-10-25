-- =====================================================================================
-- Eton -- verifica di rate ed entrate dopo 20261020000000_rate_entrate.sql.
-- Perche' esiste: collauda privilegi di colonna, compatibilita' del client vecchio,
-- vincoli del piano, immutabilita' del tipo anche come postgres e filtro RLS.
-- Utenti finti in auth.users, impersonati con set local request.jwt.claims.
-- Gli INSERT elencano tutte le colonne del client nuovo; solo i test 2 e 12b omettono kind
-- deliberatamente, per riprodurre il client gia' pubblicato.
-- QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 11 ERRORI: le sezioni 4a, 4b, 5, 6,
-- 8a, 8b, 8c, 8d, 8e, 8f, 12c. Ogni errore ha una transazione propria con rollback.
-- Come si esegue (serve Docker Desktop avviato):
-- supabase start
-- supabase db reset
-- docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-rate-entrate.sql
-- Alla seconda esecuzione senza reset gli utenti finti esistono gia': un errore in piu'.
-- Senza accenti per attraversare il terminale Windows e psql nel container Linux.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

-- I claims restano qui per riusare le identita anche eseguendo lo script da standard input.
\set claims_alice '{"sub":"a1000000-0000-0000-0000-000000000001","role":"authenticated"}'
\set claims_bruno '{"sub":"b2000000-0000-0000-0000-000000000002","role":"authenticated"}'
\set claims_carla '{"sub":"c3000000-0000-0000-0000-000000000003","role":"authenticated"}'

\echo '=== 1. Privilegi authenticated (atteso: t,f,t,f,t,t,t,t) ==='
select has_column_privilege('authenticated','public.expenses','kind','INSERT') as expense_kind_ins,
       has_column_privilege('authenticated','public.expenses','kind','UPDATE') as expense_kind_upd,
       has_column_privilege('authenticated','public.recurring_expenses','kind','INSERT') as recurring_kind_ins,
       has_column_privilege('authenticated','public.recurring_expenses','kind','UPDATE') as recurring_kind_upd,
       has_column_privilege('authenticated','public.recurring_expenses','installments','INSERT') as installments_ins,
       has_column_privilege('authenticated','public.recurring_expenses','installments','UPDATE') as installments_upd,
       has_column_privilege('authenticated','public.recurring_expenses','plan_total','INSERT') as plan_total_ins,
       has_column_privilege('authenticated','public.recurring_expenses','plan_total','UPDATE') as plan_total_upd;

\echo '=== Preparazione: Alice proprietaria, Bruno membro, Carla estranea (atteso: 3 utenti) ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('a1000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice-rate@esempio.it','{"full_name":"Alice Rate"}'::jsonb,now(),now()),
       ('b2000000-0000-0000-0000-000000000002','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno-rate@esempio.it','{"full_name":"Bruno Rate"}'::jsonb,now(),now()),
       ('c3000000-0000-0000-0000-000000000003','00000000-0000-0000-0000-000000000000','authenticated','authenticated','carla-rate@esempio.it','{"full_name":"Carla Rate"}'::jsonb,now(),now());
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
select public.create_space('Rate ed entrate condivise') as condiviso \gset
commit;
-- Fuori RLS: Bruno non puo' leggere il codice prima di essere membro.
select invite_code as codice from public.spaces where id = :'condiviso' \gset
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_bruno';
select public.join_space(:'codice') as entrato;
commit;

\echo '=== 2. Client vecchio senza kind (atteso: 1 riga, kind expense) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period)
values ('d4000000-0000-0000-0000-000000000001', :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        10.00, 'Client vecchio', 'Altro', '2026-10-01', null, null)
returning id, kind;
commit;

\echo '=== 3. Alice crea entrata, spesa, regola income e piano (atteso: 2 + 2 righe, version 1) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period, kind)
values ('d4000000-0000-0000-0000-000000000002', :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        100.00, 'Entrata', 'Altro', '2026-10-01', null, null, 'income'),
       ('d4000000-0000-0000-0000-000000000003', :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        10.00, 'Spesa', 'Altro', '2026-10-01', null, null, 'expense')
returning id, kind, version;
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values ('e5000000-0000-0000-0000-000000000001', :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        100.00, 'Entrata mensile', 'Altro', 1, 1, '2026-10-01', null, null, 'income', null, null),
       ('e5000000-0000-0000-0000-000000000002', :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        33.33, 'Piano', 'Altro', 1, 1, '2026-10-01', null, null, 'expense', 3, 100.00)
returning id, kind, installments, plan_total, version;
commit;

\echo '=== 4a. kind altro su expenses (atteso: ERRORE check expenses_kind_valido) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period, kind)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        10.00, 'Tipo errato', 'Altro', '2026-10-01', null, null, 'altro');
rollback;

\echo '=== 4b. kind altro su recurring_expenses (atteso: ERRORE check recurring_expenses_kind_valido) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        10.00, 'Tipo errato', 'Altro', 1, 1, '2026-10-01', null, null, 'altro', null, null);
rollback;

\echo '=== 5. Alice cambia kind della propria spesa (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.expenses set kind = 'income' where id = 'd4000000-0000-0000-0000-000000000003';
rollback;

\echo '=== 6. Bruno cambia kind della spesa di Alice (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_bruno';
update public.expenses set kind = 'income' where id = 'd4000000-0000-0000-0000-000000000003';
rollback;

\echo '=== 7. Postgres senza set local role: trigger rimette kind (atteso: expense e income invariati) ==='
update public.expenses set kind = 'income'
 where id = 'd4000000-0000-0000-0000-000000000003' returning id, kind;
-- Si usa la regola income, cosi' il piano resta a version 1 fino alla sezione 9.
update public.recurring_expenses set kind = 'expense'
 where id = 'e5000000-0000-0000-0000-000000000001' returning id, kind;

\echo '=== 8a. installments senza plan_total (atteso: ERRORE check recurring_expenses_piano_coppia) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        33.33, 'Coppia incompleta', 'Altro', 1, 1, '2026-10-01', null, null, 'expense', 3, null);
rollback;

\echo '=== 8b. installments 1 (atteso: ERRORE check recurring_expenses_installments_min) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        33.33, 'Una sola rata', 'Altro', 1, 1, '2026-10-01', null, null, 'expense', 1, 100.00);
rollback;

\echo '=== 8c. plan_total minore di amount (atteso: ERRORE check recurring_expenses_plan_total_min) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        33.33, 'Totale insufficiente', 'Altro', 1, 1, '2026-10-01', null, null, 'expense', 3, 30.00);
rollback;

\echo '=== 8d. Piano income (atteso: ERRORE check recurring_expenses_piano_solo_spese_mensili) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        33.33, 'Entrata a rate', 'Altro', 1, 1, '2026-10-01', null, null, 'income', 3, 100.00);
rollback;

\echo '=== 8e. Piano ogni 12 mesi (atteso: ERRORE check recurring_expenses_piano_solo_spese_mensili) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through, kind, installments, plan_total)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        33.33, 'Piano annuale', 'Altro', 12, 1, '2026-10-01', null, null, 'expense', 3, 100.00);
rollback;

\echo '=== 8f. Alice porta amount a 50 su 3 rate e totale 100 (atteso: ERRORE check recurring_expenses_plan_total_min) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.recurring_expenses set amount = 50.00
 where id = 'e5000000-0000-0000-0000-000000000002';
rollback;

\echo '=== 9. Alice aggiorna il proprio piano non partito (atteso: 1 riga, version 2, 4 rate, totale 133.32) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
update public.recurring_expenses set installments = 4, plan_total = 133.32
 where id = 'e5000000-0000-0000-0000-000000000002'
returning id, version, installments, plan_total;
commit;

\echo '=== 10. Bruno membro non pagante aggiorna il piano di Alice (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_bruno';
update public.recurring_expenses set installments = 5, plan_total = 166.65
 where id = 'e5000000-0000-0000-0000-000000000002' returning id;
commit;

\echo '=== 12a. Alice manda expense per la regola income (atteso: 1 riga, kind income) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period, kind)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        100.00, 'Entrata ottobre', 'Altro', '2026-10-01',
        'e5000000-0000-0000-0000-000000000001', '2026-10-01', 'expense')
returning id, kind;
commit;

\echo '=== 12b. Client vecchio materializza senza kind (atteso: 1 riga, kind income) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        100.00, 'Entrata novembre', 'Altro', '2026-11-01',
        'e5000000-0000-0000-0000-000000000001', '2026-11-01')
returning id, kind;
commit;

\echo '=== Preparazione 12c. Carla crea il proprio spazio (atteso: uno spazio) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
select public.create_space('Spazio di Carla rate') as spazio_carla \gset
commit;

\echo '=== 12c. Carla usa la regola di Alice (atteso: ERRORE regola ricorrente non trovata o non leggibile, 23503) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period, kind)
values (gen_random_uuid(), :'spazio_carla', 'c3000000-0000-0000-0000-000000000003',
        100.00, 'Regola non leggibile', 'Altro', '2026-12-01',
        'e5000000-0000-0000-0000-000000000001', '2026-12-01', 'expense');
rollback;

\echo '=== 12d. Entrata a mano senza regola (atteso: 1 riga, kind income) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period, kind)
values (gen_random_uuid(), :'condiviso', 'a1000000-0000-0000-0000-000000000001',
        20.00, 'Entrata a mano', 'Altro', '2026-10-20', null, null, 'income')
returning id, kind;
commit;

\echo '=== 11. Pulizia: cancella lo spazio (atteso: 2 regole e 6 spese prima, 0 e 0 dopo) ==='
select count(*) as regole_prima from public.recurring_expenses where space_id = :'condiviso';
select count(*) as spese_prima from public.expenses where space_id = :'condiviso';
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_alice';
delete from public.spaces where id = :'condiviso';
commit;
select count(*) as regole_dopo from public.recurring_expenses where space_id = :'condiviso';
select count(*) as spese_dopo from public.expenses where space_id = :'condiviso';

\echo '=== Pulizia spazio di Carla (atteso: 1 spazio cancellato, 0 regole e 0 spese) ==='
begin;
set local role authenticated;
set local request.jwt.claims = :'claims_carla';
delete from public.spaces where id = :'spazio_carla';
commit;
select count(*) as regole_carla_dopo from public.recurring_expenses where space_id = :'spazio_carla';
select count(*) as spese_carla_dopo from public.expenses where space_id = :'spazio_carla';

\echo '=== FINE. Riepilogo di cosa doveva succedere ==='
\echo '1: t f t f t t t t; 2: expense; 3: due movimenti e due regole, version 1'
\echo '4a, 4b: ERRORE check sul tipo; 5, 6: ERRORE privilegio di colonna'
\echo '7: tipi invariati anche come postgres'
\echo '8a, 8b, 8c, 8d, 8e, 8f: ERRORE sui sei casi di piano non valido'
\echo '9: una riga, version 2, quattro rate e totale 133.32; 10: zero righe'
\echo '12a, 12b: una riga income ciascuna; 12c: ERRORE del trigger per regola non leggibile'
\echo '12d: una riga income senza regola'
\echo '11: due regole e sei spese prima, zero e zero dopo; spazio di Carla cancellato'
\echo 'QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 11 ERRORI: le sezioni 4a, 4b, 5, 6, 8a, 8b, 8c, 8d, 8e, 8f, 12c.'
