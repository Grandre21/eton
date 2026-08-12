-- =====================================================================================
-- Eton -- verifica empirica delle policy RLS su 'reviews', da DUE utenti diversi.
--
-- Perche' esiste: v. verifica-rls.sql, verifica-rls-note.sql e verifica-rls-collezioni.sql. In
-- piu', qui si collauda la differenza voluta rispetto a collections e collection_items: su una
-- recensione il proprietario dello spazio NON puo modificare ne cancellare quella di un altro
-- membro, perche un voto e una opinione personale e riscriverla sarebbe falsificarla, non
-- moderarla.
--
-- Come si esegue (serve Docker Desktop avviato):
--     supabase start
--     supabase db reset
--     docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-recensioni.sql
--
-- Scritto senza accenti di proposito: passa per psql dentro un container Linux, e la codifica
-- del terminale di Windows non e' garantita.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

\echo ''
\echo '=== 1. Impianto: Alice e Bruno in uno spazio condiviso, con una collezione e un elemento ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('11111111-1111-1111-1111-111111111111','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice@esempio.it','{"full_name":"Alice"}'::jsonb, now(), now()),
       ('22222222-2222-2222-2222-222222222222','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno@esempio.it','{"full_name":"Bruno"}'::jsonb, now(), now());

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Vaping') as condiviso \gset
commit;

-- Il codice va letto da fuori RLS: Bruno non puo vedere lo spazio finche non ne fa parte.
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
returning id, name;
commit;

select id as collezione from public.collections where name = 'Liquidi' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name)
values (:'collezione', :'condiviso', '11111111-1111-1111-1111-111111111111', 'Mela Rossa')
returning id, name;
commit;

select id as elemento from public.collection_items where name = 'Mela Rossa' \gset

\echo ''
\echo '=== 2. Alice vota il proprio elemento (atteso: 1 riga, version 1) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating, comment)
values (:'elemento', :'condiviso', '11111111-1111-1111-1111-111111111111', 8.5, 'Buono')
returning id, version, rating;
commit;

select id as recensione_alice from public.reviews where user_id = '11111111-1111-1111-1111-111111111111' \gset

\echo ''
\echo '=== 3. Alice non puo votare due volte lo stesso elemento (atteso: ERRORE violates unique constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating)
values (:'elemento', :'condiviso', '11111111-1111-1111-1111-111111111111', 5);
rollback;

\echo ''
\echo '=== 4. Alice non puo firmare una recensione come Bruno (atteso: ERRORE violates row-level security) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating)
values (:'elemento', :'condiviso', '22222222-2222-2222-2222-222222222222', 3);
rollback;

\echo ''
\echo '=== 5. Bruno, membro dello spazio, legge la recensione di Alice (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, rating, comment from public.reviews where id = :'recensione_alice';
commit;

\echo ''
\echo '=== 6. Bruno NON modifica la recensione di Alice (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
update public.reviews set rating = 1 where id = :'recensione_alice' returning id;
commit;

\echo ''
\echo '=== 7. Bruno NON cancella la recensione di Alice (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
delete from public.reviews where id = :'recensione_alice' returning id;
commit;

\echo ''
\echo '=== 8. ALICE, PROPRIETARIA DELLO SPAZIO, NON tocca la recensione di Bruno ==='
\echo '=== Differenza voluta rispetto a collections e collection_items: qui il proprietario   ==='
\echo '=== NON puo fare pulizia a casa propria, perche un voto e una opinione personale e      ==='
\echo '=== riscriverla sarebbe falsificarla, non moderarla. Per togliere una recensione altrui ==='
\echo '=== bisogna cancellare l elemento (v. la cascata piu sotto).                            ==='
\echo '--- (scaffold) Bruno recensisce lo stesso elemento (atteso: 1 riga) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating, comment)
values (:'elemento', :'condiviso', '22222222-2222-2222-2222-222222222222', 6, 'Nella media')
returning id, rating;
commit;

select id as recensione_bruno from public.reviews where user_id = '22222222-2222-2222-2222-222222222222' \gset

\echo '--- Alice non la modifica (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.reviews set rating = 1 where id = :'recensione_bruno' returning id;
commit;

\echo '--- ne la cancella (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.reviews where id = :'recensione_bruno' returning id;
commit;

\echo ''
\echo '=== 9. Alice NON puo scrivere version a mano (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.reviews set version = 99 where id = :'recensione_alice';
rollback;

\echo ''
\echo '=== 10. Una recensione non si sposta su un altro elemento (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.reviews set item_id = gen_random_uuid() where id = :'recensione_alice';
rollback;

\echo ''
\echo '=== 11. LA CHIAVE ESTERNA COMPOSITA: uno space_id incoerente con l elemento e RESPINTO ==='
\echo '=== (atteso: ERRORE violates foreign key constraint). Lo spazio incoerente usato qui e  ==='
\echo '=== quello personale di Alice: lei ne e membro, quindi la RLS lo lascerebbe passare, e  ==='
\echo '=== a fermarlo e SOLO il vincolo: la prova che questa garanzia regge da sola.           ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';

-- Alice toglie prima la propria recensione su questo elemento, dentro la stessa transazione.
-- Senza, il test misurerebbe il vincolo sbagliato: Alice ha gia recensito :'elemento' nella
-- sezione 2, quindi l unique (item_id, user_id) scatterebbe PRIMA della chiave esterna e
-- l errore arriverebbe lo stesso -- ma per un altro motivo. Un test che fallisce per la ragione
-- sbagliata e peggio di nessun test: qui sembrerebbe verde mentre la FK composita resta non
-- provata. Il rollback in fondo rimette tutto a posto.
delete from public.reviews
 where item_id = :'elemento' and user_id = '11111111-1111-1111-1111-111111111111';

insert into public.reviews (item_id, space_id, user_id, rating)
select :'elemento', id, '11111111-1111-1111-1111-111111111111', 4
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
rollback;

\echo ''
\echo '=== 12. Vincoli sul voto: valori fuori scala sono respinti (atteso: ERRORE x3) ==='
\echo '--- rating = 0, lo zero non e un voto (atteso: ERRORE) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating)
values (:'elemento', :'condiviso', '11111111-1111-1111-1111-111111111111', 0);
rollback;

\echo '--- rating = 10.5, sopra il massimo (atteso: ERRORE) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating)
values (:'elemento', :'condiviso', '11111111-1111-1111-1111-111111111111', 10.5);
rollback;

\echo '--- rating = -1, negativo (atteso: ERRORE) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating)
values (:'elemento', :'condiviso', '11111111-1111-1111-1111-111111111111', -1);
rollback;

\echo ''
\echo '=== 13. reviews_non_vuota: rating e comment entrambi vuoti sono respinti (atteso: ERRORE) ==='
\echo '=== una recensione con solo commento e accettata; una con solo voto e accettata pure.   ==='
\echo '--- (scaffold) un secondo elemento, per non scontrarsi col vincolo unique (item_id, user_id) gia usato da Alice ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name)
values (:'collezione', :'condiviso', '11111111-1111-1111-1111-111111111111', 'Mela Verde')
returning id, name;
commit;

select id as elemento_due from public.collection_items where name = 'Mela Verde' \gset

\echo '--- rating e comment entrambi vuoti (atteso: ERRORE violates check constraint reviews_non_vuota) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id)
values (:'elemento_due', :'condiviso', '11111111-1111-1111-1111-111111111111');
rollback;

\echo '--- comment di soli spazi, senza voto (atteso: ERRORE violates check constraint reviews_non_vuota) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, comment)
values (:'elemento_due', :'condiviso', '11111111-1111-1111-1111-111111111111', '   ');
rollback;

\echo '--- comment di un solo carattere di tabulazione, senza voto: e il caso per cui questo   ---'
\echo '--- test esiste. btrim() senza secondo argomento toglie solo lo spazio ASCII 0x20, non   ---'
\echo '--- la tabulazione: con la vecchia forma del vincolo questa riga sarebbe passata invece  ---'
\echo '--- di essere respinta (atteso: ERRORE violates check constraint reviews_non_vuota) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, comment)
values (:'elemento_due', :'condiviso', '11111111-1111-1111-1111-111111111111', E'\t');
rollback;

\echo '--- solo commento, senza voto (atteso: 1 riga) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, comment)
values (:'elemento_due', :'condiviso', '11111111-1111-1111-1111-111111111111', 'Solo un commento')
returning id, rating, comment;
commit;

\echo '--- solo voto, senza commento (atteso: 1 riga) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating)
values (:'elemento_due', :'condiviso', '22222222-2222-2222-2222-222222222222', 9)
returning id, rating, comment;
commit;

\echo ''
\echo '=== 14. Bruno non vede le recensioni di uno spazio a cui non appartiene (atteso: 0 righe) ==='
\echo '--- (scaffold) Alice crea uno spazio privato, con una collezione, un elemento e un voto ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Privatissimo') as segreto \gset
commit;

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name)
values (:'segreto', '11111111-1111-1111-1111-111111111111', 'Segreta')
returning id, name;
commit;

select id as collezione_segreta from public.collections where name = 'Segreta' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name)
values (:'collezione_segreta', :'segreto', '11111111-1111-1111-1111-111111111111', 'Segreto')
returning id, name;
commit;

select id as elemento_segreto from public.collection_items where name = 'Segreto' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating)
values (:'elemento_segreto', :'segreto', '11111111-1111-1111-1111-111111111111', 10)
returning id, rating;
commit;

\echo '--- Bruno cerca le recensioni dello spazio segreto (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, rating from public.reviews where space_id = :'segreto';
commit;

\echo ''
\echo '=== 15. Cancellando l elemento spariscono le sue recensioni (atteso: prima 1, dopo 0) ==='
\echo '--- (scaffold) un elemento nuovo con una sola recensione, nello spazio condiviso ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name)
values (:'collezione', :'condiviso', '11111111-1111-1111-1111-111111111111', 'Effimero')
returning id, name;
commit;

select id as elemento_effimero from public.collection_items where name = 'Effimero' \gset

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.reviews (item_id, space_id, user_id, rating)
values (:'elemento_effimero', :'condiviso', '11111111-1111-1111-1111-111111111111', 5)
returning id, rating;
commit;

select count(*) as recensioni_prima from public.reviews where item_id = :'elemento_effimero';

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.collection_items where id = :'elemento_effimero';
commit;

select count(*) as recensioni_dopo from public.reviews where item_id = :'elemento_effimero';

\echo ''
\echo '=== 16. Cancellando lo spazio spariscono a cascata su TRE livelli: collezioni, elementi, ==='
\echo '=== recensioni (atteso: 1,1,1 prima; 0,0,0 dopo). Contati da postgres, non da            ==='
\echo '=== authenticated: uno zero visto come authenticated sarebbe ambiguo, non               ==='
\echo '=== distinguerebbe cancellati da nascosti.                                               ==='
select count(*) as collezioni_nel_segreto_prima from public.collections where space_id = :'segreto';
select count(*) as elementi_nel_segreto_prima   from public.collection_items where space_id = :'segreto';
select count(*) as recensioni_nel_segreto_prima from public.reviews where space_id = :'segreto';

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.spaces where id = :'segreto';
commit;

select count(*) as collezioni_nel_segreto_dopo from public.collections where space_id = :'segreto';
select count(*) as elementi_nel_segreto_dopo   from public.collection_items where space_id = :'segreto';
select count(*) as recensioni_nel_segreto_dopo from public.reviews where space_id = :'segreto';

\echo ''
\echo '=== 17. Privilegi di UPDATE per colonna su reviews (atteso: t, t, f, f, f, f, f, f) ==='
\echo '=== item_id e space_id contano quanto version: se fossero scrivibili, una recensione si  ==='
\echo '=== potrebbe SPOSTARE su un altro elemento, cioe cambiare chi ha il diritto di leggerla.  ==='
select has_column_privilege('authenticated','public.reviews','rating','UPDATE')     as upd_rating,
       has_column_privilege('authenticated','public.reviews','comment','UPDATE')    as upd_comment,
       has_column_privilege('authenticated','public.reviews','version','UPDATE')    as upd_version,
       has_column_privilege('authenticated','public.reviews','user_id','UPDATE')    as upd_user,
       has_column_privilege('authenticated','public.reviews','item_id','UPDATE')    as upd_item,
       has_column_privilege('authenticated','public.reviews','space_id','UPDATE')   as upd_space,
       has_column_privilege('authenticated','public.reviews','created_at','UPDATE') as upd_created,
       has_column_privilege('authenticated','public.reviews','updated_at','UPDATE') as upd_updated;

\echo ''
\echo '=== 18. Privilegi di INSERT per colonna su reviews (atteso: t, t, t, t, t, f, f, f) ==='
select has_column_privilege('authenticated','public.reviews','item_id','INSERT')    as ins_item,
       has_column_privilege('authenticated','public.reviews','space_id','INSERT')   as ins_space,
       has_column_privilege('authenticated','public.reviews','user_id','INSERT')    as ins_user,
       has_column_privilege('authenticated','public.reviews','rating','INSERT')     as ins_rating,
       has_column_privilege('authenticated','public.reviews','comment','INSERT')    as ins_comment,
       has_column_privilege('authenticated','public.reviews','version','INSERT')    as ins_version,
       has_column_privilege('authenticated','public.reviews','created_at','INSERT') as ins_created,
       has_column_privilege('authenticated','public.reviews','updated_at','INSERT') as ins_updated;

\echo ''
\echo '=== 19. anon non tocca reviews in alcun modo (atteso: f, f, f, f) ==='
select has_table_privilege('anon','public.reviews','SELECT') as sel,
       has_table_privilege('anon','public.reviews','INSERT') as ins,
       has_table_privilege('anon','public.reviews','UPDATE') as upd,
       has_table_privilege('anon','public.reviews','DELETE') as del;

\echo ''
\echo '=== 20. IL PUNTO CRITICO: Alice aggiorna la propria recensione. Il trigger deve alzare   ==='
\echo '=== version da 1 a 2 anche se authenticated NON ha il privilegio di colonna su version   ==='
\echo '=== (atteso: 1 riga, version 2). Senza questo test lo script proverebbe solo divieti: un  ==='
\echo '=== authenticated a cui fosse tolta ogni scrittura li passerebbe comunque tutti quanti.   ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.reviews set rating = 9, comment = 'Rivisto' where id = :'recensione_alice'
returning id, version, rating, comment;
commit;

\echo ''
\echo '=== 21. Concorrenza ottimistica: UPDATE con la versione VECCHIA non tocca nulla (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.reviews set rating = 1 where id = :'recensione_alice' and version = 1
returning id, version;
commit;

\echo '--- ...e con la versione GIUSTA invece passa (atteso: 1 riga, version 3) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.reviews set rating = 7 where id = :'recensione_alice' and version = 2
returning id, version;
commit;

\echo ''
\echo '=== FINE. Riepilogo di cosa doveva succedere ==='
\echo '  1  (impianto)    -- Alice e Bruno nello spazio condiviso, con una collezione e un elemento'
\echo '  2  1 riga, version 1 -- Alice vota il proprio elemento'
\echo '  3  ERRORE        -- Alice non vota due volte lo stesso elemento (unique item_id, user_id)'
\echo '  4  ERRORE        -- Alice non firma una recensione come Bruno'
\echo '  5  1 riga         -- Bruno legge la recensione di Alice'
\echo '  6  0 righe        -- Bruno non modifica la recensione di Alice'
\echo '  7  0 righe        -- Bruno non cancella la recensione di Alice'
\echo '  8  1 riga, poi 0 e 0 -- Alice, proprietaria, NON tocca la recensione di Bruno'
\echo '  9  ERRORE        -- version non si scrive a mano'
\echo ' 10  ERRORE        -- una recensione non si sposta su un altro elemento'
\echo ' 11  ERRORE        -- la FK composita respinge uno space_id incoerente con l elemento'
\echo ' 12  ERRORE x3     -- rating 0, rating 10.5, rating -1 tutti respinti'
\echo ' 13  ERRORE x3, poi 1 riga, poi 1 riga -- vuota, soli spazi e tab tutte respinte; solo commento ok, solo voto ok'
\echo ' 14  0 righe        -- Bruno non vede le recensioni di uno spazio a cui non appartiene'
\echo ' 15  1 poi 0        -- cancellando l elemento spariscono le sue recensioni'
\echo ' 16  1,1,1 poi 0,0,0 -- cascata dalla cancellazione dello spazio, a tre livelli fino alle recensioni'
\echo ' 17  t t f f f f f f -- in UPDATE si scrive solo rating e comment'
\echo ' 18  t t t t t f f f -- in INSERT mai version ne le date'
\echo ' 19  f f f f       -- anon non esiste per reviews'
\echo ' 20  version 2     -- IL TRIGGER SCRIVE VERSION SENZA IL PRIVILEGIO DI COLONNA'
\echo ' 21  0 righe, poi version 3 -- versione vecchia respinta, versione giusta accettata'
