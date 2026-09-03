-- =====================================================================================
-- Eton -- verifica empirica delle policy RLS su 'collections' e 'collection_items', da DUE
-- utenti diversi.
--
-- Perche' esiste: v. verifica-rls.sql e verifica-rls-note.sql. In piu', qui si collaudano due
-- cose che a occhio non si vedono affatto:
--   1) che il trigger riesca a scrivere 'version' anche se il chiamante NON ha il privilegio di
--      colonna per scriverla (come per notes);
--   2) che la chiave esterna composita (collection_id, space_id) impedisca davvero a un elemento
--      di avere uno space_id diverso da quello della propria collezione -- l'invariante da cui
--      dipende la sicurezza di TUTTE le policy di collection_items, perche' sono scritte
--      confrontando space_id senza mai fare join con collections.
--
-- Regola valida per tutto questo script: ogni 'insert into public.collections' elenca
-- esplicitamente 'blind', perche' e' cio' che invia il client (Models/Collection.cs). Uno
-- script che inserisce un sottoinsieme delle colonne collauda un percorso che l'applicazione
-- non usa, ed e' esattamente il motivo per cui il difetto 42501 sulle collezioni e'
-- sopravvissuto due settimane.
--
-- Come si esegue (serve Docker Desktop avviato):
--     supabase start
--     supabase db reset
--     docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-collezioni.sql
--
-- Scritto senza accenti di proposito: passa per psql dentro un container Linux, e la codifica
-- del terminale di Windows non e' garantita.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

\echo ''
\echo '=== 1. Privilegi di UPDATE per colonna su collections (atteso: t, t, t, t, f, f, f) ==='
\echo '=== space_id conta quanto owner_id e version: se fosse scrivibile, una collezione si   ==='
\echo '=== potrebbe SPOSTARE in un altro spazio, cioe cambiare chi ha il diritto di leggerla. ==='
select has_column_privilege('authenticated','public.collections','name','UPDATE')       as upd_name,
       has_column_privilege('authenticated','public.collections','icon','UPDATE')       as upd_icon,
       has_column_privilege('authenticated','public.collections','fields','UPDATE')     as upd_fields,
       has_column_privilege('authenticated','public.collections','rating_max','UPDATE') as upd_rating,
       has_column_privilege('authenticated','public.collections','version','UPDATE')    as upd_version,
       has_column_privilege('authenticated','public.collections','owner_id','UPDATE')   as upd_owner,
       has_column_privilege('authenticated','public.collections','space_id','UPDATE')   as upd_space;

\echo ''
\echo '=== 2. Privilegi di UPDATE per colonna su collection_items (atteso: t, t, t, f, f, f, f) ==='
\echo '=== collection_id conta quanto space_id: spostare un elemento in una collezione diversa ==='
\echo '=== cambia CHI HA IL DIRITTO DI LEGGERLO tanto quanto spostarlo in un altro spazio.      ==='
select has_column_privilege('authenticated','public.collection_items','name','UPDATE')            as upd_name,
       has_column_privilege('authenticated','public.collection_items','image_url','UPDATE')        as upd_image,
       has_column_privilege('authenticated','public.collection_items','data','UPDATE')             as upd_data,
       has_column_privilege('authenticated','public.collection_items','version','UPDATE')          as upd_version,
       has_column_privilege('authenticated','public.collection_items','added_by','UPDATE')         as upd_added,
       has_column_privilege('authenticated','public.collection_items','space_id','UPDATE')         as upd_space,
       has_column_privilege('authenticated','public.collection_items','collection_id','UPDATE')    as upd_collection;

\echo ''
\echo '=== 3. Privilegi di INSERT per colonna, su entrambe le tabelle ==='
\echo '--- collections (atteso: t t t t t t t f f f) ---'
select has_column_privilege('authenticated','public.collections','space_id','INSERT')    as ins_space,
       has_column_privilege('authenticated','public.collections','owner_id','INSERT')    as ins_owner,
       has_column_privilege('authenticated','public.collections','name','INSERT')        as ins_name,
       has_column_privilege('authenticated','public.collections','icon','INSERT')        as ins_icon,
       has_column_privilege('authenticated','public.collections','fields','INSERT')      as ins_fields,
       has_column_privilege('authenticated','public.collections','rating_max','INSERT')  as ins_rating,
       has_column_privilege('authenticated','public.collections','blind','INSERT')       as ins_blind,
       has_column_privilege('authenticated','public.collections','version','INSERT')     as ins_version,
       has_column_privilege('authenticated','public.collections','created_at','INSERT')  as ins_created,
       has_column_privilege('authenticated','public.collections','updated_at','INSERT')  as ins_updated;

\echo '--- collection_items (atteso: t t t t t t f f f) ---'
select has_column_privilege('authenticated','public.collection_items','collection_id','INSERT') as ins_collection,
       has_column_privilege('authenticated','public.collection_items','space_id','INSERT')       as ins_space,
       has_column_privilege('authenticated','public.collection_items','added_by','INSERT')       as ins_added,
       has_column_privilege('authenticated','public.collection_items','name','INSERT')           as ins_name,
       has_column_privilege('authenticated','public.collection_items','image_url','INSERT')      as ins_image,
       has_column_privilege('authenticated','public.collection_items','data','INSERT')           as ins_data,
       has_column_privilege('authenticated','public.collection_items','version','INSERT')        as ins_version,
       has_column_privilege('authenticated','public.collection_items','created_at','INSERT')     as ins_created,
       has_column_privilege('authenticated','public.collection_items','updated_at','INSERT')     as ins_updated;

\echo ''
\echo '=== 4. anon non tocca ne collections ne collection_items (atteso: f f f f per tabella) ==='
\echo '--- collections ---'
select has_table_privilege('anon','public.collections','SELECT') as sel,
       has_table_privilege('anon','public.collections','INSERT') as ins,
       has_table_privilege('anon','public.collections','UPDATE') as upd,
       has_table_privilege('anon','public.collections','DELETE') as del;

\echo '--- collection_items ---'
select has_table_privilege('anon','public.collection_items','SELECT') as sel,
       has_table_privilege('anon','public.collection_items','INSERT') as ins,
       has_table_privilege('anon','public.collection_items','UPDATE') as upd,
       has_table_privilege('anon','public.collection_items','DELETE') as del;

\echo ''
\echo '=== Due utenti + uno spazio condiviso di Alice, in cui entra Bruno ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('11111111-1111-1111-1111-111111111111','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice@esempio.it','{"full_name":"Alice"}'::jsonb, now(), now()),
       ('22222222-2222-2222-2222-222222222222','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno@esempio.it','{"full_name":"Bruno"}'::jsonb, now(), now());

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Musica') as condiviso \gset
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
\echo '=== 5. ALICE crea una collezione nello spazio condiviso, con dei fields veri (atteso: 1 riga, version 1, blind f) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name, icon, fields, rating_max, blind)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 'Vinili', 'disco',
        '[{"name":"artista","type":"text"},{"name":"anno","type":"number"}]'::jsonb, 10, false)
returning id, version, name, blind;
commit;

select id as collezione from public.collections where name = 'Vinili' \gset

\echo ''
\echo '=== 6. IL PUNTO CRITICO: Alice rinomina la collezione. Il trigger deve alzare version a 2 ==='
\echo '=== anche se authenticated NON ha il privilegio di colonna su version (atteso: 1 riga, version 2) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.collections set name = 'Vinili anni 70' where id = :'collezione'
returning id, version, name;
commit;

\echo ''
\echo '=== 7. Concorrenza ottimistica: UPDATE con la versione VECCHIA non tocca nulla (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.collections set name = 'Sovrascrittura cieca' where id = :'collezione' and version = 1
returning id, version;
commit;

\echo '--- ...e con la versione GIUSTA invece passa (atteso: 1 riga, version 3) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.collections set name = 'Vinili anni Settanta' where id = :'collezione' and version = 2
returning id, version;
commit;

\echo ''
\echo '=== 8. Alice NON puo scrivere version a mano (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.collections set version = 99 where id = :'collezione';
rollback;

\echo ''
\echo '=== 9. Alice NON puo spostare la collezione nel proprio spazio personale (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.collections
   set space_id = (select id from public.spaces
                    where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal)
 where id = :'collezione';
rollback;

\echo ''
\echo '=== 10. LA CHIAVE ESTERNA COMPOSITA: un elemento con space_id INCOERENTE viene RESPINTO ==='
\echo '=== (atteso: ERRORE violates foreign key constraint). Lo spazio incoerente usato qui e   ==='
\echo '=== quello personale di Alice: lei ne e membro, quindi la RLS lo lascerebbe passare, e   ==='
\echo '=== a fermarlo e SOLO il vincolo: la prova che questa garanzia regge da sola.            ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name)
select :'collezione', id, '11111111-1111-1111-1111-111111111111', 'Incoerente'
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
rollback;

\echo ''
\echo '=== 11. BRUNO, membro dello spazio, legge la collezione di Alice ma non la modifica ne la cancella ==='
\echo '--- legge (atteso: 1 riga) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, name, version from public.collections where space_id = :'condiviso';
commit;

\echo '--- non la modifica (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
update public.collections set name = 'Manomessa da Bruno' where id = :'collezione' returning id;
commit;

\echo '--- ne la cancella (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
delete from public.collections where id = :'collezione' returning id;
commit;

\echo ''
\echo '=== 12. BRUNO aggiunge un proprio elemento nella collezione di Alice, ma non puo firmarlo come Alice ==='
\echo '--- (scaffold) Alice aggiunge un proprio elemento, servira ai test 14, 15 e 16 ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name, data)
values (:'collezione', :'condiviso', '11111111-1111-1111-1111-111111111111', 'Disco di Alice', '{"artista":"Pink Floyd"}'::jsonb)
returning id, name;
commit;

select id as elemento_alice from public.collection_items where name = 'Disco di Alice' \gset

\echo '--- BRUNO aggiunge un proprio elemento (atteso: 1 riga) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name, data)
values (:'collezione', :'condiviso', '22222222-2222-2222-2222-222222222222', 'Disco di Bruno', '{"artista":"Vari"}'::jsonb)
returning id, name;
commit;

select id as elemento_bruno from public.collection_items where name = 'Disco di Bruno' \gset

\echo '--- ...ma NON puo firmarlo come Alice (atteso: ERRORE violates row-level security) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name)
values (:'collezione', :'condiviso', '11111111-1111-1111-1111-111111111111', 'Falso');
rollback;

\echo ''
\echo '=== 13. ALICE, proprietaria dello spazio, cancella il disco di Bruno (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.collection_items where id = :'elemento_bruno' returning id, name;
commit;

\echo ''
\echo '=== 14. Un elemento NON si sposta in una collezione diversa (atteso: ERRORE permission denied) ==='
\echo '=== Come per space_id sulle collezioni: spostarlo cambia CHI HA IL DIRITTO DI LEGGERLO.  ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.collection_items set collection_id = gen_random_uuid() where id = :'elemento_alice';
rollback;

\echo ''
\echo '=== 15. BRUNO non vede collezioni ne elementi di uno spazio a cui non appartiene ==='
\echo '--- (scaffold) Alice crea uno spazio privato, con una sua collezione e un suo elemento dentro ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Privatissimo') as segreto \gset
commit;

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name, blind)
values (:'segreto', '11111111-1111-1111-1111-111111111111', 'Segreta', false)
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

\echo '--- Bruno cerca le collezioni dello spazio segreto (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, name from public.collections where space_id = :'segreto';
commit;

\echo '--- Bruno cerca gli elementi dello spazio segreto (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, name from public.collection_items where space_id = :'segreto';
commit;

\echo '--- Bruno conta TUTTE le collezioni e TUTTI gli elementi che riesce a vedere (atteso: 1 e 1) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select count(*) as collezioni_visibili_a_bruno from public.collections;
select count(*) as elementi_visibili_a_bruno   from public.collection_items;
commit;

\echo ''
\echo '=== 16. Cancellando la collezione spariscono i suoi elementi (atteso: prima 1, dopo 0) ==='
select count(*) as elementi_prima from public.collection_items where collection_id = :'collezione';

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.collections where id = :'collezione';
commit;

select count(*) as elementi_dopo from public.collection_items where collection_id = :'collezione';

\echo ''
\echo '=== 17. Cancellando lo spazio spariscono le sue collezioni E i loro elementi, a cascata su due livelli (atteso: 1,1 prima; 0,0 dopo) ==='
\echo '=== Contati fuori da authenticated: da postgres si vede la tabella reale, mentre uno zero  ==='
\echo '=== visto come authenticated sarebbe ambiguo -- non distinguerebbe cancellati da nascosti. ==='
select count(*) as collezioni_nel_segreto_prima from public.collections where space_id = :'segreto';
select count(*) as elementi_nel_segreto_prima   from public.collection_items where space_id = :'segreto';

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.spaces where id = :'segreto';
commit;

select count(*) as collezioni_nel_segreto_dopo from public.collections where space_id = :'segreto';
select count(*) as elementi_nel_segreto_dopo   from public.collection_items where space_id = :'segreto';

\echo ''
\echo '=== 18. Vincoli di forma del jsonb (atteso: ERRORE violates check constraint, tre volte) ==='
\echo '--- fields NON e un array (atteso: ERRORE) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name, fields, blind)
select id, '11111111-1111-1111-1111-111111111111', 'Forma sbagliata', '{"non":"un array"}'::jsonb, false
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
rollback;

\echo '--- fields con piu di 40 elementi (atteso: ERRORE) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name, fields, blind)
select id, '11111111-1111-1111-1111-111111111111', 'Troppi campi',
       (select jsonb_agg(jsonb_build_object('name', 'campo' || n)) from generate_series(1,41) as n), false
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
rollback;

\echo '--- (scaffold) Alice crea una collezione nel proprio spazio personale, per il test su data ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name, blind)
select id, '11111111-1111-1111-1111-111111111111', 'Bozza', false
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal
returning id, name;
commit;

select id as collezione_personale from public.collections where name = 'Bozza' \gset

\echo '--- data NON e un oggetto (atteso: ERRORE) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collection_items (collection_id, space_id, added_by, name, data)
values (:'collezione_personale',
        (select id from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal),
        '11111111-1111-1111-1111-111111111111', 'Forma sbagliata', '[1,2,3]'::jsonb);
rollback;

\echo ''
\echo '=== 19. rating_max fuori da (5,10) e respinto (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.collections (space_id, owner_id, name, rating_max, blind)
select id, '11111111-1111-1111-1111-111111111111', 'Voto strano', 7, false
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
rollback;

\echo ''
\echo '=== FINE. Riepilogo di cosa doveva succedere ==='
\echo '  1  t t t t f f f -- in UPDATE si scrive solo name/icon/fields/rating_max'
\echo '  2  t t t f f f f -- in UPDATE si scrive solo name/image_url/data'
\echo '  3  collections t*7 f*3, collection_items t*6 f*3 -- in INSERT mai version ne le date'
\echo '  4  f f f f (x2)  -- anon non esiste ne per collections ne per collection_items'
\echo '  5  version 1, blind f -- la collezione nasce con version 1, non cieca'
\echo '  6  version 2     -- IL TRIGGER SCRIVE VERSION SENZA IL PRIVILEGIO DI COLONNA'
\echo '  7  0 righe, poi version 3 -- versione vecchia respinta, versione giusta accettata'
\echo '  8  ERRORE        -- version non si scrive a mano'
\echo '  9  ERRORE        -- una collezione non si sposta in un altro spazio'
\echo ' 10  ERRORE        -- la FK composita respinge uno space_id incoerente con la collezione'
\echo ' 11  1, 0, 0 righe -- un membro legge ma non modifica ne cancella le collezioni altrui'
\echo ' 12  1 riga, ERRORE -- si aggiunge un elemento proprio, non si firma come un altro'
\echo ' 13  1 riga        -- il proprietario dello spazio fa pulizia sugli elementi altrui'
\echo ' 14  ERRORE        -- un elemento non si sposta in una collezione diversa'
\echo ' 15  0, 0, 1 e 1   -- niente collezioni ne elementi dagli spazi altrui'
\echo ' 16  1 poi 0       -- cascata dalla cancellazione della collezione'
\echo ' 17  1,1 poi 0,0   -- cascata dalla cancellazione dello spazio, a due livelli fino agli elementi'
\echo ' 18  ERRORE x3     -- fields non-array, fields troppo lungo, data non-oggetto: tutti respinti'
\echo ' 19  ERRORE        -- rating_max ammette solo 5 o 10'
