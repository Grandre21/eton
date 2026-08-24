-- =====================================================================================
-- Eton -- verifica empirica delle policy RLS su 'expenses', da DUE utenti diversi.
--
-- Perche' esiste: v. verifica-rls.sql e verifica-rls-note.sql, di cui questo file ricalca la
-- meccanica -- due utenti finti in auth.users, impersonati con 'set local request.jwt.claims'.
-- In piu' collauda che il trigger riesca a scrivere 'version' anche se il chiamante NON ha il
-- privilegio di colonna per scriverla direttamente: se quell'assunto fosse falso, ogni correzione
-- di una spesa fallirebbe con "permission denied for column version", e lo si scoprirebbe in
-- produzione.
--
-- QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 7 ERRORI: le sezioni 7, 8, 9, 11, 15, 16 e 18 qui sotto.
-- Ottenerne un numero diverso significa che qualcosa e' cambiato -- e' il senso stesso dello
-- script.
--
-- Come si esegue (serve Docker Desktop avviato):
--     supabase start
--     supabase db reset
--     docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-spese.sql
--
-- Scritto senza accenti di proposito: passa per psql dentro un container Linux, e la codifica
-- del terminale di Windows non e' garantita.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

\echo ''
\echo '=== 1a. Privilegi di UPDATE per colonna su expenses (atteso: t,t,t,t,f,f,f,f,f) ==='
\echo '=== paid_by e space_id contano quanto version: se fossero scrivibili, una spesa si    ==='
\echo '=== potrebbe SPOSTARE sotto un altro pagatore o un altro spazio, cioe una fuga di dati.==='
select has_column_privilege('authenticated','public.expenses','amount','UPDATE')      as upd_amount,
       has_column_privilege('authenticated','public.expenses','description','UPDATE') as upd_description,
       has_column_privilege('authenticated','public.expenses','category','UPDATE')    as upd_category,
       has_column_privilege('authenticated','public.expenses','spent_on','UPDATE')    as upd_spent_on,
       has_column_privilege('authenticated','public.expenses','version','UPDATE')     as upd_version,
       has_column_privilege('authenticated','public.expenses','paid_by','UPDATE')     as upd_paid_by,
       has_column_privilege('authenticated','public.expenses','space_id','UPDATE')    as upd_space,
       has_column_privilege('authenticated','public.expenses','created_at','UPDATE')  as upd_created,
       has_column_privilege('authenticated','public.expenses','updated_at','UPDATE')  as upd_updated;

\echo ''
\echo '=== 1b. Privilegi di INSERT per colonna su expenses (atteso: t,t,t,t,t,t,t,f,f,f) ==='
\echo '=== id e la deroga rispetto a notes: qui lo genera il client, non il database. ==='
select has_column_privilege('authenticated','public.expenses','id','INSERT')          as ins_id,
       has_column_privilege('authenticated','public.expenses','space_id','INSERT')    as ins_space,
       has_column_privilege('authenticated','public.expenses','paid_by','INSERT')     as ins_paid_by,
       has_column_privilege('authenticated','public.expenses','amount','INSERT')      as ins_amount,
       has_column_privilege('authenticated','public.expenses','description','INSERT') as ins_description,
       has_column_privilege('authenticated','public.expenses','category','INSERT')    as ins_category,
       has_column_privilege('authenticated','public.expenses','spent_on','INSERT')    as ins_spent_on,
       has_column_privilege('authenticated','public.expenses','version','INSERT')     as ins_version,
       has_column_privilege('authenticated','public.expenses','created_at','INSERT')  as ins_created,
       has_column_privilege('authenticated','public.expenses','updated_at','INSERT')  as ins_updated;

\echo ''
\echo '=== 2. anon non tocca expenses in alcun modo (atteso: f, f, f, f) ==='
\echo '=== La chiave anon e pubblica, sta nel JavaScript scaricato da chiunque: se un grant le  ==='
\echo '=== sfuggisse, la tabella sarebbe leggibile da Internet senza autenticazione, e nessuna  ==='
\echo '=== policy RLS lo impedirebbe -- RLS filtra, non concede.                                ==='
select has_table_privilege('anon','public.expenses','SELECT') as sel,
       has_table_privilege('anon','public.expenses','INSERT') as ins,
       has_table_privilege('anon','public.expenses','UPDATE') as upd,
       has_table_privilege('anon','public.expenses','DELETE') as del;

\echo ''
\echo '=== 3. Due utenti + uno spazio condiviso di Alice, in cui entra Bruno ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('11111111-1111-1111-1111-111111111111','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice@esempio.it','{"full_name":"Alice"}'::jsonb, now(), now()),
       ('22222222-2222-2222-2222-222222222222','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno@esempio.it','{"full_name":"Bruno"}'::jsonb, now(), now());

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Spese condivise') as condiviso \gset
commit;

select :'condiviso' as spazio_condiviso;

-- Il codice va letto da fuori RLS: Bruno non puo' vedere lo spazio finche' non ne fa parte.
select invite_code as codice from public.spaces where id = :'condiviso' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select public.join_space(:'codice') as entrato \gset
commit;

select :'entrato' as bruno_entrato_in;

\echo ''
\echo '=== 4. ALICE registra una spesa nello spazio condiviso (atteso: 1 riga, version 1) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses (space_id, paid_by, amount, description, category)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 42.50, 'Spesa di Alice', 'Casa')
returning id, version, amount;
commit;

select id as spesa_alice from public.expenses where description = 'Spesa di Alice' \gset

\echo ''
\echo '=== 5. IL PUNTO CRITICO: Alice corregge amount. Il trigger deve alzare version a 2 ==='
\echo '=== anche se authenticated NON ha il privilegio di colonna su version (atteso: 1 riga, version 2) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.expenses set amount = 45.00 where id = :'spesa_alice'
returning id, version, amount;
commit;

\echo ''
\echo '=== 6. Concorrenza ottimistica: UPDATE con la versione VECCHIA non tocca nulla (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.expenses set amount = 50.00 where id = :'spesa_alice' and version = 1
returning id, version;
commit;

\echo '--- ...e con la versione GIUSTA invece passa (atteso: 1 riga, version 3) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.expenses set amount = 50.00 where id = :'spesa_alice' and version = 2
returning id, version;
commit;

\echo ''
\echo '=== 7. Alice NON puo scrivere version a mano (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.expenses set version = 99 where id = :'spesa_alice';
rollback;

\echo ''
\echo '=== 8. Alice NON puo intestare la spesa a un altro cambiando paid_by (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.expenses set paid_by = '22222222-2222-2222-2222-222222222222' where id = :'spesa_alice';
rollback;

\echo ''
\echo '=== 9. Alice NON puo spostare la spesa in un altro spazio cambiando space_id (atteso: ERRORE permission denied for column) ==='
\echo '=== E il caso peggiore fra quelli bloccati dai privilegi di colonna: spostare una spesa   ==='
\echo '=== cambia CHI PUO LEGGERLA, quindi sarebbe una fuga di dati e non un dispetto.           ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.expenses
   set space_id = (select id from public.spaces
                    where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal)
 where id = :'spesa_alice';
rollback;

\echo ''
\echo '=== 10. BRUNO registra una PROPRIA spesa nello stesso spazio (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.expenses (space_id, paid_by, amount, description, category)
values (:'condiviso', '22222222-2222-2222-2222-222222222222', 10.00, 'Spesa di Bruno', 'Trasporti')
returning id, version, amount;
commit;

select id as spesa_bruno from public.expenses where description = 'Spesa di Bruno' \gset

\echo ''
\echo '=== 11. BRUNO tenta di intestare una spesa ad ALICE, cioe paid_by di un altro ==='
\echo '=== (atteso: ERRORE violates row-level security) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.expenses (space_id, paid_by, amount, description, category)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 5.00, 'Intestata ad Alice', 'Svago');
rollback;

\echo ''
\echo '=== 12. BRUNO, membro ma NON proprietario, NON modifica la spesa di ALICE (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
update public.expenses set amount = 1.00 where id = :'spesa_alice' returning id;
commit;

\echo ''
\echo '=== 13. ALICE, proprietaria dello spazio, PUO modificare la spesa di BRUNO (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.expenses set amount = 12.00 where id = :'spesa_bruno' returning id, amount;
commit;

\echo ''
\echo '=== 14. Una spesa nello spazio PERSONALE di Alice: Bruno non la vede, nemmeno conoscendo il suo id ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses (space_id, paid_by, amount, description, category)
select id, '11111111-1111-1111-1111-111111111111', 8.00, 'Spesa personale di Alice', 'Salute'
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal
returning id;
commit;

select id as spesa_segreta from public.expenses where description = 'Spesa personale di Alice' \gset

\echo '--- Bruno cerca la spesa segreta per id (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, description from public.expenses where id = :'spesa_segreta';
commit;

\echo ''
\echo '=== 15. amount <= 0 viene rifiutato (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses (space_id, paid_by, amount, description, category)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 0, 'Importo nullo', 'Altro');
rollback;

\echo ''
\echo '=== 16. Una description di soli spazi viene rifiutata (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses (space_id, paid_by, amount, description, category)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 3.00, '   ', 'Altro');
rollback;

\echo ''
\echo '=== 17. Inserimento con id scelto dal client funziona (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses (id, space_id, paid_by, amount, description, category)
values ('33333333-3333-3333-3333-333333333333', :'condiviso', '11111111-1111-1111-1111-111111111111', 7.00, 'Ritentativo innocuo', 'Ristoranti')
returning id;
commit;

\echo ''
\echo '=== 18. ...e ripeterlo con LO STESSO id fallisce per violazione di chiave primaria ==='
\echo '=== (atteso: ERRORE duplicate key value violates unique constraint) ==='
\echo '=== E precisamente la proprieta su cui si regge il ritentativo innocuo dopo una rete   ==='
\echo '=== che va e viene: lo stesso uuid, rimandato due volte, produce una riga sola.        ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.expenses (id, space_id, paid_by, amount, description, category)
values ('33333333-3333-3333-3333-333333333333', :'condiviso', '11111111-1111-1111-1111-111111111111', 7.00, 'Ritentativo innocuo', 'Ristoranti');
rollback;

\echo ''
\echo '=== 19. Cancellando lo spazio spariscono le sue spese (atteso: prima 3, dopo 0) ==='
select count(*) as spese_nel_condiviso_prima from public.expenses where space_id = :'condiviso';

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.spaces where id = :'condiviso';
commit;

select count(*) as spese_nel_condiviso_dopo from public.expenses where space_id = :'condiviso';

\echo ''
\echo '=== FINE. Riepilogo di cosa doveva succedere ==='
\echo ' 1a  t t t t f f f f f -- in UPDATE si scrive solo amount, description, category, spent_on'
\echo ' 1b  t t t t t t t f f f -- in INSERT anche id (la deroga), space_id e paid_by, mai version ne le date'
\echo '  2  f f f f       -- anon non esiste per expenses'
\echo '  5  version 2     -- IL TRIGGER SCRIVE VERSION SENZA IL PRIVILEGIO DI COLONNA'
\echo '  6  0 righe, poi version 3 -- versione vecchia respinta, versione giusta accettata'
\echo '  7  ERRORE        -- version non si scrive a mano'
\echo '  8  ERRORE        -- paid_by non si scrive a mano, non si intesta la spesa a un altro'
\echo '  9  ERRORE        -- space_id non si scrive a mano, non si sposta la spesa in un altro spazio'
\echo ' 11  ERRORE        -- non si intesta una spesa a un altro in inserimento'
\echo ' 12  0 righe        -- un membro non modifica la spesa altrui'
\echo ' 13  1 riga         -- il proprietario dello spazio la modifica comunque'
\echo ' 14  0 righe        -- niente spese dagli spazi altrui, nemmeno conoscendo il suo id'
\echo ' 15  ERRORE        -- amount non positivo respinto'
\echo ' 16  ERRORE        -- description di soli spazi respinta'
\echo ' 17  1 riga        -- id scelto dal client accettato'
\echo ' 18  ERRORE        -- lo stesso id una seconda volta viola la chiave primaria'
\echo ' 19  3 poi 0        -- cascata dalla cancellazione dello spazio'
