-- =====================================================================================
-- Eton -- verifica empirica delle policy RLS su 'notes', da DUE utenti diversi.
--
-- Perche' esiste: v. verifica-rls.sql. In piu', qui si collauda una cosa che a occhio non si
-- vede affatto -- che il trigger riesca a scrivere 'version' anche se il chiamante NON ha il
-- privilegio di colonna per scriverla. Se quell'assunto fosse falso, ogni salvataggio di una
-- nota fallirebbe con "permission denied for column version", e lo si scoprirebbe in produzione.
--
-- Come si esegue (serve Docker Desktop avviato):
--     supabase start
--     supabase db reset
--     docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-rls-note.sql
--
-- Scritto senza accenti di proposito: passa per psql dentro un container Linux, e la codifica
-- del terminale di Windows non e' garantita.
-- =====================================================================================

\set ON_ERROR_STOP off
\pset pager off

\echo ''
\echo '=== 1a. Privilegi di UPDATE per colonna su notes (atteso: t, t, f, f, f, f) ==='
\echo '=== space_id conta quanto gli altri: se fosse scrivibile, una nota si potrebbe    ==='
\echo '=== SPOSTARE in un altro spazio, cioe cambiare chi ha il diritto di leggerla.     ==='
select has_column_privilege('authenticated','public.notes','title','UPDATE')      as upd_title,
       has_column_privilege('authenticated','public.notes','body','UPDATE')       as upd_body,
       has_column_privilege('authenticated','public.notes','version','UPDATE')    as upd_version,
       has_column_privilege('authenticated','public.notes','owner_id','UPDATE')   as upd_owner,
       has_column_privilege('authenticated','public.notes','space_id','UPDATE')   as upd_space,
       has_column_privilege('authenticated','public.notes','updated_at','UPDATE') as upd_updated;

\echo ''
\echo '=== 1b. Privilegi di INSERT per colonna su notes (atteso: t, t, t, t, f, f, f) ==='
select has_column_privilege('authenticated','public.notes','space_id','INSERT')   as ins_space,
       has_column_privilege('authenticated','public.notes','owner_id','INSERT')   as ins_owner,
       has_column_privilege('authenticated','public.notes','title','INSERT')      as ins_title,
       has_column_privilege('authenticated','public.notes','body','INSERT')       as ins_body,
       has_column_privilege('authenticated','public.notes','version','INSERT')    as ins_version,
       has_column_privilege('authenticated','public.notes','created_at','INSERT') as ins_created,
       has_column_privilege('authenticated','public.notes','updated_at','INSERT') as ins_updated;

\echo ''
\echo '=== 2. anon non tocca notes in alcun modo (atteso: f, f, f, f) ==='
select has_table_privilege('anon','public.notes','SELECT') as sel,
       has_table_privilege('anon','public.notes','INSERT') as ins,
       has_table_privilege('anon','public.notes','UPDATE') as upd,
       has_table_privilege('anon','public.notes','DELETE') as del;

\echo ''
\echo '=== 3. Due utenti + uno spazio condiviso di Alice, in cui entra Bruno ==='
insert into auth.users (id, instance_id, aud, role, email, raw_user_meta_data, created_at, updated_at)
values ('11111111-1111-1111-1111-111111111111','00000000-0000-0000-0000-000000000000','authenticated','authenticated','alice@esempio.it','{"full_name":"Alice"}'::jsonb, now(), now()),
       ('22222222-2222-2222-2222-222222222222','00000000-0000-0000-0000-000000000000','authenticated','authenticated','bruno@esempio.it','{"full_name":"Bruno"}'::jsonb, now(), now());

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Liquidi') as condiviso \gset
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
\echo '=== 4. ALICE scrive una nota nello spazio condiviso (atteso: 1 riga, version 1) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 'Prova', 'Testo **iniziale**')
returning id, version, title;
commit;

select id as nota from public.notes where title = 'Prova' \gset

\echo ''
\echo '=== 5. IL PUNTO CRITICO: Alice aggiorna il corpo. Il trigger deve alzare version a 2 ==='
\echo '=== anche se authenticated NON ha il privilegio di colonna su version (atteso: 1 riga, version 2) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.notes set body = 'Testo riveduto' where id = :'nota'
returning id, version, body;
commit;

\echo ''
\echo '=== 6. Concorrenza ottimistica: UPDATE con la versione VECCHIA non tocca nulla (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.notes set body = 'Sovrascrittura cieca' where id = :'nota' and version = 1
returning id, version;
commit;

\echo ''
\echo '=== 7. ...e con la versione GIUSTA invece passa (atteso: 1 riga, version 3) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.notes set body = 'Terza stesura' where id = :'nota' and version = 2
returning id, version;
commit;

\echo ''
\echo '=== 8. Alice NON puo scrivere version a mano (atteso: ERRORE permission denied for column) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.notes set version = 99 where id = :'nota';
rollback;

\echo ''
\echo '=== 9. Alice NON puo regalare la nota a Bruno cambiando owner_id (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.notes set owner_id = '22222222-2222-2222-2222-222222222222' where id = :'nota';
rollback;

\echo ''
\echo '=== 10. BRUNO, membro dello spazio, LEGGE la nota di Alice (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, title, version from public.notes where space_id = :'condiviso';
commit;

\echo ''
\echo '=== 11. BRUNO, membro ma NON proprietario dello spazio, NON modifica la nota di Alice (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
update public.notes set body = 'Manomessa da Bruno' where id = :'nota' returning id;
commit;

\echo ''
\echo '=== 12. ...ne la cancella (atteso: 0 righe) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
delete from public.notes where id = :'nota' returning id;
commit;

\echo ''
\echo '=== 13. Il corpo e ancora quello di Alice (atteso: Terza stesura, version 3) ==='
select title, body, version from public.notes where id = :'nota';

\echo ''
\echo '=== 14. BRUNO scrive una PROPRIA nota nello spazio condiviso (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
values (:'condiviso', '22222222-2222-2222-2222-222222222222', 'Nota di Bruno', 'Mia')
returning id, title;
commit;

select id as nota_bruno from public.notes where title = 'Nota di Bruno' \gset

\echo ''
\echo '=== 15. ...ma NON puo firmarla come Alice (atteso: ERRORE violates row-level security) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
values (:'condiviso', '11111111-1111-1111-1111-111111111111', 'Falsa', 'Firmata Alice');
rollback;

\echo ''
\echo '=== 16. ALICE, proprietaria dello spazio, PUO cancellare la nota di Bruno (atteso: 1 riga) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.notes where id = :'nota_bruno' returning id, title;
commit;

\echo ''
\echo '=== 17. Uno spazio a cui Bruno NON appartiene: la nota di Alice li dentro e invisibile ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
select public.create_space('Privatissimo') as segreto \gset
commit;

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
values (:'segreto', '11111111-1111-1111-1111-111111111111', 'Segreta', 'Non per Bruno');
commit;

\echo '--- Bruno cerca la nota segreta (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, title from public.notes where space_id = :'segreto';
commit;

\echo '--- Bruno conta TUTTE le note che riesce a vedere (atteso: 1, solo quella di Alice nel condiviso) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select count(*) as note_visibili_a_bruno from public.notes;
commit;

\echo ''
\echo '=== 18. Bruno NON puo scrivere note in uno spazio di cui non e membro (atteso: ERRORE violates RLS) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
values (:'segreto', '22222222-2222-2222-2222-222222222222', 'Intrusione', 'Ciao');
rollback;

\echo ''
\echo '=== 19. Nota nello spazio PERSONALE: la vede solo il suo proprietario ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
select id, '11111111-1111-1111-1111-111111111111', 'Personale di Alice', 'Solo mia'
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
commit;

\echo '--- Bruno la cerca (atteso: 0 righe) ---'
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"22222222-2222-2222-2222-222222222222","role":"authenticated"}';
select id, title from public.notes where title = 'Personale di Alice';
commit;

\echo ''
\echo '=== 20. Cancellando lo spazio spariscono le sue note (atteso: prima 1, dopo 0) ==='
select count(*) as note_nel_condiviso_prima from public.notes where space_id = :'condiviso';

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
delete from public.spaces where id = :'condiviso';
commit;

select count(*) as note_nel_condiviso_dopo from public.notes where space_id = :'condiviso';

\echo ''
\echo '=== 21. Vincoli di lunghezza (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
select id, '11111111-1111-1111-1111-111111111111', repeat('x', 201), ''
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
rollback;

\echo ''
\echo '=== 22. Alice NON puo spostare la nota in un altro spazio (atteso: ERRORE permission denied) ==='
\echo '=== E il caso peggiore fra quelli bloccati dai privilegi di colonna: spostare una nota    ==='
\echo '=== cambia CHI PUO LEGGERLA, quindi sarebbe una fuga di dati e non un dispetto.           ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
update public.notes set space_id = (select id from public.spaces
                                     where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal)
 where title = 'Segreta';
rollback;

\echo ''
\echo '=== 23. Alice NON puo falsificare version gia alla creazione (atteso: ERRORE permission denied) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body, version)
select id, '11111111-1111-1111-1111-111111111111', 'Falsificata', '', 999
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
rollback;

\echo ''
\echo '=== 24. Il limite di lunghezza del corpo (atteso: ERRORE violates check constraint) ==='
begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
select id, '11111111-1111-1111-1111-111111111111', 'Troppo lunga', repeat('x', 100001)
from public.spaces where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal;
rollback;

\echo ''
\echo '=== 25. length() conta CARATTERI, non byte: 200 emoji devono passare (atteso: 1 riga) ==='
\echo '=== Se qualcuno sostituisse length() con octet_length(), il limite crollerebbe a un   ==='
\echo '=== quarto per il testo non ASCII e nessun altro passo se ne accorgerebbe. Lemoji si  ==='
\echo '=== costruisce con chr(): questo file resta volutamente di soli caratteri ASCII,      ==='
\echo '=== perche passa per psql dentro un container e la codifica non e garantita.          ==='
select length(repeat(chr(128512), 200))       as caratteri_attesi_200,
       octet_length(repeat(chr(128512), 200)) as byte_attesi_800;

begin;
set local role authenticated;
set local request.jwt.claims = '{"sub":"11111111-1111-1111-1111-111111111111","role":"authenticated"}';
insert into public.notes (space_id, owner_id, title, body)
select id, '11111111-1111-1111-1111-111111111111', repeat(chr(128512), 200), ''
from public.spaces
where owner_id = '11111111-1111-1111-1111-111111111111' and is_personal
returning length(title) as caratteri_salvati;
rollback;

\echo ''
\echo '=== FINE. Riepilogo di cosa doveva succedere ==='
\echo ' 1a  t t f f f f   -- in UPDATE si scrive solo title e body'
\echo ' 1b  t t t t f f f -- in INSERT anche space_id e owner_id, mai version ne le date'
\echo '  2  f f f f       -- anon non esiste per notes'
\echo '  5  version 2     -- IL TRIGGER SCRIVE VERSION SENZA IL PRIVILEGIO DI COLONNA'
\echo '  6  0 righe       -- versione vecchia respinta'
\echo '  7  version 3     -- versione giusta accettata'
\echo '  8  ERRORE        -- version non si scrive a mano'
\echo '  9  ERRORE        -- owner_id non si scrive a mano'
\echo ' 11  0 righe       -- un membro non modifica le note altrui'
\echo ' 12  0 righe       -- ne le cancella'
\echo ' 15  ERRORE        -- non si firma una nota col nome di un altro'
\echo ' 16  1 riga        -- il proprietario dello spazio fa pulizia'
\echo ' 17  0 righe, 1    -- niente note dagli spazi altrui'
\echo ' 18  ERRORE        -- niente scritture negli spazi altrui'
\echo ' 20  1 poi 0       -- cascata dalla cancellazione dello spazio'
\echo ' 21  ERRORE        -- titolo oltre 200 caratteri respinto'
\echo ' 22  ERRORE        -- una nota non si sposta in un altro spazio'
\echo ' 23  ERRORE        -- version non si falsifica nemmeno alla creazione'
\echo ' 24  ERRORE        -- corpo oltre 100000 caratteri respinto'
\echo ' 25  200 e 800     -- length() conta caratteri, octet_length() byte: il limite e in caratteri'
