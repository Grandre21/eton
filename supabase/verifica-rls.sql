-- =====================================================================================
-- Eton -- verifica empirica delle policy RLS, dal punto di vista di DUE utenti diversi.
--
-- Perche' esiste: in un'app senza server la sicurezza vive tutta nelle policy, e una policy
-- sbagliata non fa fallire nessuna compilazione ne' nessun collaudo manuale -- perche' chi prova
-- l'app i propri dati li vede giustamente. Il buco si vede solo dal punto di vista di un secondo
-- utente, che nei test manuali non c'e' mai.
--
-- Come si esegue (serve Docker Desktop avviato):
--     supabase start
--     supabase db reset
--     docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls.sql
--
-- Nota sulle porte: su Windows l'intervallo 54320-54419 puo' essere riservato dal sistema, e ci
-- cadono dentro tutte le porte di default di Supabase CLI. In questo progetto sono spostate a
-- 55320+ in config.toml. Gli intervalli riservati si elencano con:
--     netsh interface ipv4 show excludedportrange protocol=tcp
--
-- Questo file e' scritto senza accenti di proposito: passa per psql dentro un container Linux,
-- e la codifica del terminale di Windows non e' garantita.
--
-- Diventera' Eton.Tests.Integration nella fetta 2, quando ci sara' abbastanza da asserire da
-- giustificare un progetto xUnit. Fino ad allora si legge l'output a occhio: ogni sezione
-- dichiara il risultato atteso.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

\echo '=== 1. Dove sta pgcrypto ==='
select extname, extnamespace::regnamespace as schema from pg_extension where extname = 'pgcrypto';

\echo '=== 2. Privilegi di colonna su spaces (atteso: f, f, t, f) ==='
select has_column_privilege('authenticated','public.spaces','is_personal','UPDATE') as auth_upd_is_personal,
       has_column_privilege('authenticated','public.spaces','owner_id','UPDATE')    as auth_upd_owner_id,
       has_column_privilege('authenticated','public.spaces','name','UPDATE')        as auth_upd_name,
       has_column_privilege('anon','public.spaces','name','UPDATE')                 as anon_upd_name;

\echo '=== 3. generate_invite_code funziona e produce 8 caratteri ==='
select public.generate_invite_code() as codice, length(public.generate_invite_code()) as lunghezza;

\echo '=== 4. generate_invite_code NON e eseguibile da authenticated (atteso: f) ==='
select has_function_privilege('authenticated','public.generate_invite_code()','EXECUTE') as auth_puo_generare,
       has_function_privilege('authenticated','public.create_space(text)','EXECUTE')     as auth_puo_creare,
       has_function_privilege('authenticated','public.is_space_member(uuid)','EXECUTE')  as auth_puo_is_member,
       has_function_privilege('anon','public.create_space(text)','EXECUTE')              as anon_puo_creare;

\echo '=== 5. Creo due utenti: il trigger deve creare profilo e spazio personale ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('11111111-1111-1111-1111-111111111111','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice@esempio.it','{"full_name":"Alice"}'::jsonb, now(), now()),
       ('22222222-2222-2222-2222-222222222222','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno@esempio.it','{"full_name":"Bruno"}'::jsonb, now(), now());

select p.display_name, s.name, s.is_personal, s.invite_code is null as senza_codice,
       (select count(*) from space_members m where m.space_id = s.id) as membri
from profiles p join spaces s on s.owner_id = p.id order by p.display_name;

\echo '=== 6. ALICE: vede solo il proprio spazio personale (atteso: 1) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select count(*) as spazi_visti_da_alice from spaces;
select count(*) as profili_visti_da_alice from profiles;

\echo '--- Alice crea uno spazio condiviso ---'
select public.create_space('Svapo con gli amici') as nuovo_spazio;
select count(*) as spazi_dopo_creazione from spaces;
commit;

\echo '=== 7. BRUNO: non vede lo spazio di Alice (atteso: 1) ne il suo profilo (atteso: 1, solo il proprio) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select count(*) as spazi_visti_da_bruno from spaces;
select count(*) as profili_visti_da_bruno from profiles;

\echo '--- Bruno prova a inserire direttamente in spaces (atteso: ERRORE RLS) ---'
insert into spaces (name, owner_id, is_personal) values ('Abusivo','22222222-2222-2222-2222-222222222222', false);
commit;

\echo '=== 8. Catturo il codice invito di Alice (come postgres: Bruno non lo vedrebbe) ==='
select invite_code as codice from spaces where name = 'Svapo con gli amici' \gset
\echo 'codice catturato:' :codice

\echo '=== 9. BRUNO entra col codice: ora vede 2 spazi e 2 profili ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select public.join_space(:'codice') is not null as entrato;
commit;

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select count(*) as spazi_visti_da_bruno_dopo from spaces;
select count(*) as profili_visti_da_bruno_dopo from profiles;

\echo '--- Bruno (non proprietario) prova a rinominare lo spazio di Alice (atteso: 0 righe) ---'
with rinominati as (update spaces set name = 'Mio ora' where name = 'Svapo con gli amici' returning 1)
select count(*) as rinominato_da_bruno from rinominati;

\echo '--- Bruno prova a cancellare lo spazio di Alice (atteso: 0 righe) ---'
with cancellati as (delete from spaces where name = 'Svapo con gli amici' returning 1)
select count(*) as cancellato_da_bruno from cancellati;
commit;

\echo '=== 10. ALICE prova ad azzerare is_personal sul proprio spazio personale (atteso: ERRORE di permesso) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update spaces set is_personal = false where name = 'Personale' and owner_id = '11111111-1111-1111-1111-111111111111';
commit;

\echo '=== 11. ALICE prova a cancellare il proprio spazio personale (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
with cancellati as (delete from spaces where is_personal returning 1)
select count(*) as spazi_personali_cancellati from cancellati;

\echo '--- Alice (proprietaria) prova a uscire dal proprio spazio condiviso (atteso: 0 righe) ---'
with usciti as (
    delete from space_members
    where user_id = '11111111-1111-1111-1111-111111111111'
      and space_id = (select id from spaces where name = 'Svapo con gli amici')
    returning 1)
select count(*) as proprietaria_uscita from usciti;

\echo '--- Alice rinomina lo spazio condiviso (atteso: 1 riga, e il nome cambia) ---'
with rinominati as (
    update spaces set name = 'Svapo' where name = 'Svapo con gli amici' returning 1)
select count(*) as rinominati from rinominati;
commit;

\echo '=== 12. BRUNO prova a espellere Alice (atteso: 0 righe) e poi esce lui (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
with espulsi as (
    delete from space_members
    where user_id = '11111111-1111-1111-1111-111111111111'
    returning 1)
select count(*) as alice_espulsa_da_bruno from espulsi;

with usciti as (
    delete from space_members
    where user_id = '22222222-2222-2222-2222-222222222222'
      and space_id = (select id from spaces where name = 'Svapo')
    returning 1)
select count(*) as bruno_uscito from usciti;
commit;

\echo '=== 13. Dopo l uscita di Bruno: non vede piu lo spazio ne il profilo di Alice (atteso: 1 e 1) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select count(*) as spazi_visti_da_bruno_finale from spaces;
select count(*) as profili_visti_da_bruno_finale from profiles;
commit;

\echo '=== FINE ==='
