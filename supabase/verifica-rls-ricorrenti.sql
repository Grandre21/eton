-- =====================================================================================
-- Eton -- verifica empirica delle policy RLS su 'recurring_expenses' e delle regole aggiunte a
-- 'expenses' dalla migrazione 20260922000000_ricorrenti.sql, da TRE utenti diversi.
--
-- Perche' esiste: v. verifica-rls-spese.sql, di cui questo file ricalca la meccanica -- utenti
-- finti in auth.users, impersonati con 'set local request.jwt.claims'. In piu' collauda: i
-- privilegi di colonna su recurring_expenses, la policy di INSERT ristretta su expenses che
-- guarda expenses.recurring_id, il vincolo unique (recurring_id, recurring_period) col conflict
-- target esatto che usa il client, il check sul primo del mese, la coppia
-- recurring_id/recurring_period, la FK no action che distingue una regola chiusa da una regola
-- cancellabile, e che il trigger rimette la provenienza anche fuori dai privilegi normali.
--
-- OGNI insert in expenses elenca TUTTE le colonne che il client invia -- id, space_id, paid_by,
-- amount, description, category, spent_on, recurring_id, recurring_period -- anche quando le
-- ultime due sono null. OGNI insert in recurring_expenses elenca: id, space_id, paid_by, amount,
-- description, category, every_months, day_of_month, starts_on, ends_on, materialized_through.
-- Uno script che ne inserisce un sottoinsieme collauda un percorso che l'app non usa: e' il
-- difetto che ha tenuto rotte le collezioni due settimane (v. 20260903000000_grant_insert_blind.sql).
--
-- QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 10 ERRORI: le sezioni 7, 9, 10, 10b, 13, 14, 15, 16, 17
-- e 19 qui sotto. Ottenerne un numero diverso significa che qualcosa e' cambiato -- e' il senso
-- stesso dello script.
--
-- Come si esegue (serve Docker Desktop avviato):
--     supabase start
--     supabase db reset
--     docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-ricorrenti.sql
--
-- Scritto senza accenti di proposito: passa per psql dentro un container Linux, e la codifica
-- del terminale di Windows non e' garantita.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

\echo ''
\echo '=== 1a. Privilegi di UPDATE per colonna su recurring_expenses ==='
\echo '=== (atteso: t,t,t,t,t,t,t,t,f,f,f,f,f) ==='
\echo '=== Il contenuto si aggiorna; version, paid_by, space_id, created_at e updated_at no: li ==='
\echo '=== scrive solo il trigger, come per expenses.                                           ==='
select has_column_privilege('authenticated','public.recurring_expenses','amount','UPDATE')                as upd_amount,
       has_column_privilege('authenticated','public.recurring_expenses','description','UPDATE')           as upd_description,
       has_column_privilege('authenticated','public.recurring_expenses','category','UPDATE')              as upd_category,
       has_column_privilege('authenticated','public.recurring_expenses','every_months','UPDATE')          as upd_every_months,
       has_column_privilege('authenticated','public.recurring_expenses','day_of_month','UPDATE')          as upd_day_of_month,
       has_column_privilege('authenticated','public.recurring_expenses','starts_on','UPDATE')             as upd_starts_on,
       has_column_privilege('authenticated','public.recurring_expenses','ends_on','UPDATE')               as upd_ends_on,
       has_column_privilege('authenticated','public.recurring_expenses','materialized_through','UPDATE')  as upd_materialized_through,
       has_column_privilege('authenticated','public.recurring_expenses','version','UPDATE')               as upd_version,
       has_column_privilege('authenticated','public.recurring_expenses','paid_by','UPDATE')               as upd_paid_by,
       has_column_privilege('authenticated','public.recurring_expenses','space_id','UPDATE')              as upd_space,
       has_column_privilege('authenticated','public.recurring_expenses','created_at','UPDATE')            as upd_created,
       has_column_privilege('authenticated','public.recurring_expenses','updated_at','UPDATE')            as upd_updated;

\echo ''
\echo '=== 1b. Privilegi di INSERT per colonna su recurring_expenses ==='
\echo '=== (atteso: t,t,t,t,t,t,t,t,t,t,t,f,f,f) ==='
\echo '=== id lo genera il client, come in expenses: ritentativo innocuo dopo una rete che va e ==='
\echo '=== viene. Mai version, created_at, updated_at.                                          ==='
select has_column_privilege('authenticated','public.recurring_expenses','id','INSERT')                    as ins_id,
       has_column_privilege('authenticated','public.recurring_expenses','space_id','INSERT')              as ins_space,
       has_column_privilege('authenticated','public.recurring_expenses','paid_by','INSERT')               as ins_paid_by,
       has_column_privilege('authenticated','public.recurring_expenses','amount','INSERT')                as ins_amount,
       has_column_privilege('authenticated','public.recurring_expenses','description','INSERT')           as ins_description,
       has_column_privilege('authenticated','public.recurring_expenses','category','INSERT')              as ins_category,
       has_column_privilege('authenticated','public.recurring_expenses','every_months','INSERT')          as ins_every_months,
       has_column_privilege('authenticated','public.recurring_expenses','day_of_month','INSERT')          as ins_day_of_month,
       has_column_privilege('authenticated','public.recurring_expenses','starts_on','INSERT')             as ins_starts_on,
       has_column_privilege('authenticated','public.recurring_expenses','ends_on','INSERT')               as ins_ends_on,
       has_column_privilege('authenticated','public.recurring_expenses','materialized_through','INSERT')  as ins_materialized_through,
       has_column_privilege('authenticated','public.recurring_expenses','version','INSERT')               as ins_version,
       has_column_privilege('authenticated','public.recurring_expenses','created_at','INSERT')            as ins_created,
       has_column_privilege('authenticated','public.recurring_expenses','updated_at','INSERT')            as ins_updated;

\echo ''
\echo '=== 1c. Su expenses: recurring_id e recurring_period si scrivono solo in INSERT ==='
\echo '=== (atteso: t,t,f,f) ==='
\echo '=== Nessun grant update gemello, di proposito: la provenienza si scrive una volta sola.  ==='
select has_column_privilege('authenticated','public.expenses','recurring_id','INSERT')     as ins_recurring_id,
       has_column_privilege('authenticated','public.expenses','recurring_period','INSERT') as ins_recurring_period,
       has_column_privilege('authenticated','public.expenses','recurring_id','UPDATE')     as upd_recurring_id,
       has_column_privilege('authenticated','public.expenses','recurring_period','UPDATE') as upd_recurring_period;

\echo ''
\echo '=== 2. anon non tocca recurring_expenses in alcun modo (atteso: f, f, f, f) ==='
select has_table_privilege('anon','public.recurring_expenses','SELECT') as sel,
       has_table_privilege('anon','public.recurring_expenses','INSERT') as ins,
       has_table_privilege('anon','public.recurring_expenses','UPDATE') as upd,
       has_table_privilege('anon','public.recurring_expenses','DELETE') as del;

\echo ''
\echo '=== 3. Tre utenti: ALICE, BRUNO (entra nello spazio condiviso di Alice), CARLA (estranea) ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('11111111-1111-1111-1111-111111111111','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice@esempio.it','{"full_name":"Alice"}'::jsonb, now(), now()),
       ('22222222-2222-2222-2222-222222222222','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno@esempio.it','{"full_name":"Bruno"}'::jsonb, now(), now()),
       ('44444444-4444-4444-4444-444444444444','00000000-0000-0000-0000-000000000000','authenticated','authenticated','carla@esempio.it','{"full_name":"Carla"}'::jsonb, now(), now());

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Spese ricorrenti condivise') as condiviso \gset
commit;

select :'condiviso' as spazio_condiviso;

-- Il codice va letto da fuori RLS: Bruno non lo puo leggere finche non e membro.
select invite_code as codice from public.spaces where id = :'condiviso' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select public.join_space(:'codice') as entrato \gset
commit;

select :'entrato' as bruno_entrato_in;

\echo ''
\echo '=== 4. ALICE crea una regola ricorrente nello spazio condiviso (atteso: 1 riga, version 1) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through)
values ('55555555-5555-5555-5555-555555555555', :'condiviso', '11111111-1111-1111-1111-111111111111',
        15.00, 'Abbonamento palestra', 'Salute', 1, 5, '2026-01-05', null, null)
returning id, version, amount;
commit;

\echo ''
\echo '=== 5. BRUNO, membro, vede la regola di Alice (atteso: 1 riga); CARLA, estranea, no (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, description from public.recurring_expenses where id = '55555555-5555-5555-5555-555555555555';
commit;

\echo '--- Carla cerca la regola per id (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"44444444-4444-4444-4444-444444444444","role":"authenticated"}';
select id, description from public.recurring_expenses where id = '55555555-5555-5555-5555-555555555555';
commit;

\echo ''
\echo '=== 6. ALICE corregge amount e avanza materialized_through a 2026-09 (atteso: 1 riga, version 2) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.recurring_expenses
   set amount = 20.00, materialized_through = '2026-09'
 where id = '55555555-5555-5555-5555-555555555555'
returning id, version, amount, materialized_through;
commit;

\echo ''
\echo '=== 7. ALICE NON puo scrivere version a mano (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.recurring_expenses set version = 99 where id = '55555555-5555-5555-5555-555555555555';
rollback;

\echo ''
\echo '=== 8. BRUNO, membro ma non pagante ne proprietario dello spazio, NON modifica la regola di ==='
\echo '=== Alice (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
update public.recurring_expenses set amount = 1.00 where id = '55555555-5555-5555-5555-555555555555' returning id;
commit;

\echo ''
\echo '=== 9. BRUNO tenta di creare una regola intestata ad Alice, cioe paid_by di un altro ==='
\echo '=== (atteso: ERRORE violates row-level security) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through)
values (gen_random_uuid(), :'condiviso', '11111111-1111-1111-1111-111111111111', 5.00, 'Intestata ad Alice', 'Svago', 1, 1, '2026-01-01', null, null);
rollback;

\echo ''
\echo '=== 10. materialized_through malformato, 2026-9 invece di 2026-09 ==='
\echo '=== (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through)
values (gen_random_uuid(), :'condiviso', '11111111-1111-1111-1111-111111111111', 5.00, 'Formato sbagliato', 'Altro', 1, 1, '2026-01-01', null, '2026-9');
rollback;

\echo ''
\echo '=== 10b. starts_on troppo lontana nel futuro, 9999-12-01 ==='
\echo '=== (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through)
values (gen_random_uuid(), :'condiviso', '11111111-1111-1111-1111-111111111111', 5.00, 'Data estrema', 'Altro', 1, 1, '9999-12-01', null, null);
rollback;

\echo ''
\echo '=== 11. ALICE materializza la spesa di settembre dalla regola (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period)
values ('66666666-6666-6666-6666-666666666666', :'condiviso', '11111111-1111-1111-1111-111111111111',
        20.00, 'Abbonamento palestra - settembre', 'Salute', '2026-09-05',
        '55555555-5555-5555-5555-555555555555', '2026-09-01')
returning id;
commit;

\echo ''
\echo '=== 12. LA SCRITTURA DEL CLIENT: stesso periodo, id diverso, on conflict do nothing ==='
\echo '=== (atteso: 0 righe, nessun errore) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period)
values ('77777777-7777-7777-7777-777777777777', :'condiviso', '11111111-1111-1111-1111-111111111111',
        20.00, 'Abbonamento palestra - settembre', 'Salute', '2026-09-05',
        '55555555-5555-5555-5555-555555555555', '2026-09-01')
on conflict (recurring_id, recurring_period) do nothing
returning id;
commit;

\echo ''
\echo '=== 13. La stessa scrittura SENZA on conflict fallisce per chiave duplicata ==='
\echo '=== (atteso: ERRORE duplicate key, vincolo expenses_recurring_period_uq) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period)
values ('77777777-7777-7777-7777-777777777777', :'condiviso', '11111111-1111-1111-1111-111111111111',
        20.00, 'Abbonamento palestra - settembre', 'Salute', '2026-09-05',
        '55555555-5555-5555-5555-555555555555', '2026-09-01');
rollback;

\echo ''
\echo '=== 14. recurring_period non e il primo del mese, 2026-10-05 ==='
\echo '=== (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period)
values (gen_random_uuid(), :'condiviso', '11111111-1111-1111-1111-111111111111',
        20.00, 'Abbonamento palestra - ottobre', 'Salute', '2026-10-05',
        '55555555-5555-5555-5555-555555555555', '2026-10-05');
rollback;

\echo ''
\echo '=== 15. recurring_id senza recurring_period (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period)
values (gen_random_uuid(), :'condiviso', '11111111-1111-1111-1111-111111111111',
        20.00, 'Senza periodo', 'Salute', '2026-10-05',
        '55555555-5555-5555-5555-555555555555', null);
rollback;

\echo ''
\echo '=== 16. BRUNO aggancia una propria spesa alla regola di ALICE, stesso spazio ==='
\echo '=== (atteso: ERRORE violates row-level security) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.expenses
    (id, space_id, paid_by, amount, description, category, spent_on, recurring_id, recurring_period)
values (gen_random_uuid(), :'condiviso', '22222222-2222-2222-2222-222222222222',
        20.00, 'Bruno si aggancia alla regola di Alice', 'Salute', '2026-11-01',
        '55555555-5555-5555-5555-555555555555', '2026-11-01');
rollback;

\echo ''
\echo '=== 17. ALICE NON riscrive recurring_id con un update (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.expenses set recurring_id = null where id = '66666666-6666-6666-6666-666666666666';
rollback;

\echo ''
\echo '=== 18. Anche fuori dai privilegi normali, come postgres e senza set local role, il ==='
\echo '=== trigger rimette la provenienza (atteso: recurring_id e recurring_period invariati) ==='
update public.expenses
   set recurring_id = null, recurring_period = null
 where id = '66666666-6666-6666-6666-666666666666';
select recurring_id, recurring_period from public.expenses where id = '66666666-6666-6666-6666-666666666666';

\echo ''
\echo '=== 19. ALICE NON cancella la regola che ha generato la occorrenza ==='
\echo '=== (atteso: ERRORE foreign key, 23503) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.recurring_expenses where id = '55555555-5555-5555-5555-555555555555';
rollback;

\echo ''
\echo '=== 20. Una seconda regola, senza occorrenze materializzate, si cancella senza problemi ==='
\echo '=== (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.recurring_expenses
    (id, space_id, paid_by, amount, description, category, every_months, day_of_month, starts_on, ends_on, materialized_through)
values (gen_random_uuid(), :'condiviso', '11111111-1111-1111-1111-111111111111',
        9.90, 'Regola senza occorrenze', 'Abbonamenti', 1, 1, '2026-01-01', null, null)
returning id as regola_senza_occorrenze \gset
delete from public.recurring_expenses where id = :'regola_senza_occorrenze' returning id;
commit;

\echo ''
\echo '=== 21. Cancellare lo spazio condiviso funziona lo stesso: distingue no action da restrict ==='
\echo '=== (atteso: 1 regola e 1 spesa prima, 0 e 0 dopo) ==='
select count(*) as regole_nel_condiviso_prima from public.recurring_expenses where space_id = :'condiviso';
select count(*) as spese_nel_condiviso_prima from public.expenses where space_id = :'condiviso';

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.spaces where id = :'condiviso';
commit;

select count(*) as regole_nel_condiviso_dopo from public.recurring_expenses where space_id = :'condiviso';
select count(*) as spese_nel_condiviso_dopo from public.expenses where space_id = :'condiviso';

\echo ''
\echo '=== FINE. Riepilogo di cosa doveva succedere ==='
\echo ' 1a  t t t t t t t t f f f f f -- in UPDATE si scrive il contenuto, mai version ne le date'
\echo ' 1b  t t t t t t t t t t t f f f -- in INSERT anche id, mai version ne le date'
\echo ' 1c  t t f f      -- recurring_id e recurring_period su expenses solo in INSERT'
\echo '  2  f f f f      -- anon non esiste per recurring_expenses'
\echo '  4  1 riga, version 1  -- Alice crea la regola'
\echo '  5  1 riga poi 0 righe -- Bruno membro la vede, Carla estranea no'
\echo '  6  version 2    -- IL TRIGGER SCRIVE VERSION SENZA IL PRIVILEGIO DI COLONNA'
\echo '  7  ERRORE       -- version non si scrive a mano'
\echo '  8  0 righe       -- un membro non proprietario non modifica la regola altrui'
\echo '  9  ERRORE       -- non si intesta una regola a un altro in inserimento'
\echo ' 10  ERRORE       -- materialized_through malformato respinto'
\echo ' 10b ERRORE       -- starts_on troppo lontana nel futuro respinta'
\echo ' 11  1 riga        -- la spesa di settembre viene materializzata'
\echo ' 12  0 righe        -- il ritentativo del client con on conflict non duplica'
\echo ' 13  ERRORE        -- lo stesso periodo senza on conflict viola la chiave unica'
\echo ' 14  ERRORE        -- il periodo deve essere il primo del mese'
\echo ' 15  ERRORE        -- recurring_id senza recurring_period respinto'
\echo ' 16  ERRORE        -- non ci si aggancia alla regola di un altro'
\echo ' 17  ERRORE        -- recurring_id non si riscrive con un update'
\echo ' 18  invariati      -- il trigger rimette la provenienza anche fuori dai privilegi normali'
\echo ' 19  ERRORE        -- una regola con occorrenze non si cancella'
\echo ' 20  1 riga         -- una regola senza occorrenze si cancella'
\echo ' 21  1 e 1 poi 0 e 0 -- cascata dalla cancellazione dello spazio, no action non la blocca'
