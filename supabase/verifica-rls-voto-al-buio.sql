-- =====================================================================================
-- Eton -- verifica empirica del "voto al buio" (collections.blind), da TRE utenti diversi.
--
-- Perche' esiste: v. verifica-rls.sql, verifica-rls-note.sql, verifica-rls-collezioni.sql e
-- verifica-rls-recensioni.sql. In piu', qui si collauda la funzionalita' aggiunta da
-- 20260812230000_voto_al_buio.sql: su un elemento di una collezione cieca non si vedono le
-- recensioni altrui finche' non si e' messa la propria, ma si vede sempre QUANTI hanno votato.
--
-- Errori SQL attesi in tutto questo script: 1 (sezione 8, anon non puo eseguire review_counts).
-- Ogni altro esito atteso e' un conteggio di righe, non un errore -- lo dice l'intestazione di
-- ciascuna sezione.
--
-- Come si esegue (serve Docker Desktop avviato):
--     supabase start
--     supabase db reset
--     docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-voto-al-buio.sql
--
-- Scritto senza accenti di proposito: passa per psql dentro un container Linux, e la codifica
-- del terminale di Windows non e' garantita.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

\echo ''
\echo '=== 1. Impianto: Alice e Bruno in uno spazio condiviso, con una collezione normale e una ==='
\echo '=== cieca, un elemento in ciascuna. Carla esiste solo nel proprio spazio personale, e non ==='
\echo '=== fa mai parte dello spazio condiviso.                                                 ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('11111111-1111-1111-1111-111111111111','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice@esempio.it','{"full_name":"Alice"}'::jsonb, now(), now()),
       ('22222222-2222-2222-2222-222222222222','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno@esempio.it','{"full_name":"Bruno"}'::jsonb, now(), now()),
       ('33333333-3333-3333-3333-333333333333','00000000-0000-0000-0000-000000000000','authenticated','authenticated','carla@esempio.it','{"full_name":"Carla"}'::jsonb, now(), now());

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Vaping al buio') as condiviso \gset
commit;

select invite_code as codice from public.spaces where id = :'condiviso' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select public.join_space(:'codice') as entrato \gset
commit;

select :'entrato' as bruno_entrato_in;

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 'Liquidi')
returning id, name, blind;
commit;

select id as collezione_normale from public.collections where name = 'Liquidi' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 'Segreti')
returning id, name, blind;
commit;

select id as collezione_cieca from public.collections where name = 'Segreti' \gset

\echo '--- (scaffold) Alice accende blind sulla propria collezione (atteso: 1 riga, blind t) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.collections set blind = true where id = :'collezione_cieca' returning id, blind;
commit;

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name)
values (:'collezione_normale', :'condiviso', '11111111-1111-1111-1111-111111111111', 'Mela Rossa')
returning id, name;
commit;

select id as elemento_normale from public.collection_items where name = 'Mela Rossa' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name)
values (:'collezione_cieca', :'condiviso', '11111111-1111-1111-1111-111111111111', 'Mela Nera')
returning id, name;
commit;

select id as elemento_cieco from public.collection_items where name = 'Mela Nera' \gset

\echo ''
\echo '=== 2. Su collezione NON cieca: Bruno vede la recensione di Alice anche senza aver votato ==='
\echo '=== (atteso: 1 riga).                                                                    ==='
\echo '--- (scaffold) Alice vota l elemento normale (atteso: 1 riga) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating, comment)
values (:'elemento_normale', :'condiviso', '11111111-1111-1111-1111-111111111111', 8, 'Ottimo')
returning id, rating;
commit;

select id as recensione_alice_normale from public.reviews where item_id = :'elemento_normale' and user_id = '11111111-1111-1111-1111-111111111111' \gset

\echo '--- Bruno, che non ha votato, legge comunque la recensione di Alice (atteso: 1 riga) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, rating, comment from public.reviews where item_id = :'elemento_normale';
commit;

\echo ''
\echo '=== 3. Su collezione CIECA: Bruno NON vede la recensione di Alice se non ha votato        ==='
\echo '=== (atteso: 0 righe). Zero righe non e un errore, e il conteggio da verificare.          ==='
\echo '--- (scaffold) Alice vota l elemento cieco (atteso: 1 riga) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating, comment)
values (:'elemento_cieco', :'condiviso', '11111111-1111-1111-1111-111111111111', 9, 'Top segreto')
returning id, rating;
commit;

select id as recensione_alice_cieca from public.reviews where item_id = :'elemento_cieco' and user_id = '11111111-1111-1111-1111-111111111111' \gset

\echo '--- Bruno cerca la recensione di Alice, riga intera (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, rating, comment from public.reviews where item_id = :'elemento_cieco';
commit;

\echo '--- e nemmeno selezionando solo rating e comment, senza id (atteso: 0 righe): non deve   ---'
\echo '--- esistere nessuna via, nemmeno indiretta, per leggere il voto o il commento di Alice   ---'
\echo '--- prima di aver votato.                                                                ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select rating, comment from public.reviews where item_id = :'elemento_cieco';
commit;

\echo ''
\echo '=== 4. La propria recensione si vede sempre, anche su una collezione cieca (atteso: 1     ==='
\echo '=== riga): Alice rilegge la propria recensione sull elemento cieco.                      ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select id, rating, comment from public.reviews where id = :'recensione_alice_cieca';
commit;

\echo ''
\echo '=== 5. review_counts da il numero giusto anche per l elemento che Bruno non puo ancora   ==='
\echo '=== vedere (atteso: elemento_cieco con voters = 1, chiamata fatta da Bruno stesso).      ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select * from public.review_counts(:'condiviso') order by item_id;
commit;

\echo ''
\echo '=== 6. Su collezione cieca: Bruno vota, e ORA vede la recensione di Alice (atteso: 2      ==='
\echo '=== righe: la propria e quella di Alice, entrambe con rating e comment visibili).         ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating, comment)
values (:'elemento_cieco', :'condiviso', '22222222-2222-2222-2222-222222222222', 7, 'Anche a me piace')
returning id, rating;
commit;

select id as recensione_bruno_cieca from public.reviews where item_id = :'elemento_cieco' and user_id = '22222222-2222-2222-2222-222222222222' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, rating, comment from public.reviews where item_id = :'elemento_cieco' order by created_at;
commit;

\echo ''
\echo '=== 7. review_counts chiamata da Carla, membro di un altro spazio (il proprio, personale) ==='
\echo '=== ma non dello spazio condiviso (atteso: 0 righe).                                      ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"33333333-3333-3333-3333-333333333333","role":"authenticated"}';
select * from public.review_counts(:'condiviso');
commit;

\echo ''
\echo '=== 8. anon non puo eseguire review_counts (atteso: ERRORE permission denied). Questo e  ==='
\echo '=== l UNICO errore SQL atteso in tutto lo script.                                         ==='
begin;
set local role anon;
select * from public.review_counts(:'condiviso');
rollback;

\echo ''
\echo '=== 9. Un membro non proprietario non puo accendere blind su una collezione altrui: ne il  ==='
\echo '=== privilegio di colonna da solo (che e di ruolo, non di riga, quindi resta vero) ne la   ==='
\echo '=== policy fermano davvero il tentativo -- e la policy che lo ferma.                       ==='
\echo '--- privilegio di colonna: authenticated PUO scrivere blind (atteso: t, informativo, non  ---'
\echo '--- basta da solo a proteggere la collezione di un altro) ---'
select has_column_privilege('authenticated','public.collections','blind','UPDATE') as upd_blind;

\echo '--- Bruno, membro ma non proprietario ne della collezione ne dello spazio, prova ad        ---'
\echo '--- accendere blind sulla collezione di Alice (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
update public.collections set blind = true where id = :'collezione_normale' returning id, blind;
commit;

\echo '--- controprova, fuori RLS: la collezione di Alice e ancora normale (atteso: 1 riga, blind f) ---'
select id, blind from public.collections where id = :'collezione_normale';

\echo ''
\echo '=== FINE. Riepilogo di cosa doveva succedere ==='
\echo '  1  (impianto)     -- Alice e Bruno nello spazio condiviso; Carla solo nel proprio'
\echo '  2  1 riga          -- collezione normale: Bruno vede senza aver votato'
\echo '  3  0 righe, poi 0 righe -- collezione cieca: Bruno non vede, ne riga intera ne colonne singole'
\echo '  4  1 riga          -- la propria recensione si vede sempre, anche cieca'
\echo '  5  voters = 1     -- review_counts giusto anche per l elemento che Bruno non vede ancora'
\echo '  6  2 righe          -- Bruno vota, e ora vede anche la recensione di Alice'
\echo '  7  0 righe          -- review_counts chiamata da chi non e membro dello spazio'
\echo '  8  ERRORE           -- anon non puo eseguire review_counts'
\echo '  9  t, poi 0 righe, poi blind f -- privilegio di colonna vero ma la policy blocca Bruno'
