UNITÀ: 1 — ESITO: FATTO
TOCCATI:
  supabase/migrations/20261120000000_budget_periodo.sql → +69/−0 (nuovo)
  supabase/verifica-budget-periodo.sql → +185/−0 (nuovo)
  handoff/01-migrazione-periodo/brief-implementer-1.md → nuovo (brief)
  handoff/01-migrazione-periodo/resoconto.md → nuovo (questo file)

REVIEW:
review: brief-implementer-1 — migrazione e collaudo
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 3
  checker         nessuna istruttoria
(misure: `2 files changed, 254 insertions(+)` · 2 create mode · 0 dichiarazioni/endpoint; backend-expert lanciato per i file nuovi)

CONTRATTI:
  colonna — supabase/migrations/20261120000000_budget_periodo.sql:16
    `alter table public.category_budgets add column if not exists description text not null default '';`
  check — :19-20
    `add constraint category_budgets_description_max check (char_length(description) <= 200);`
  privilegi — :26-27 (stesso ruolo che scrive amount, `authenticated`; nessun altro grant, nessun revoke)
    `grant insert (description) on public.category_budgets to authenticated;`
    `grant update (description) on public.category_budgets to authenticated;`
  estensione — :33 `create extension if not exists btree_gist with schema extensions;`
  vincolo — :63-69
    `add constraint category_budgets_no_overlap exclude using gist ( space_id with =, category with =, daterange(starts_on, (ends_on + interval '1 month')::date, '[)') with && );`
  codice d'errore di una violazione, verificato sul Supabase locale via PostgREST (POST /rest/v1/category_budgets
  con JWT di un utente di prova): HTTP 400 (non 409), corpo esatto:
    {"code":"23P01","details":"Key conflicts with existing key.","hint":null,"message":"conflicting key value violates exclusion constraint \"category_budgets_no_overlap\""}
  Policy e indice unico category_budgets_aperto_uq invariati (nessuna riga che li tocca nella migrazione).

ADJUDICA:
- backend-expert BE1: infondato — propone di togliere il blocco DO e lasciare l'errore nativo di ADD CONSTRAINT, ma il controllo preliminare con messaggio chiaro è chiesto dal mandato (OBIETTIVO, righe 10-11) — `raise exception 'Trovate % coppie di budget con periodi sovrapposti ...'` (migrazione :51)
- backend-expert BE2: fondato → non corretto (bassa, sotto soglia) — lo script di collaudo mescola le prove dei due blocchi; se l'utente sceglie la versione senza blocco II lo script va ritoccato in più punti — `-- QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE 4 ERRORI: le sezioni 4a, 4c (seconda parte), 4f, 4g.` (verifica-budget-periodo.sql:5)
- backend-expert BE3: fuori scope — TIPO: progetto: gli script verifica-*.sql si giudicano a occhio (ON_ERROR_STOP off e conteggio degli errori); proposta di farli fallire da soli con ON_ERROR_STOP on e DO che pretendono l'errore, o pgTAP. Vale per tutti e dieci gli script, decisione dell'utente — `\set ON_ERROR_STOP off` (verifica-budget-periodo.sql:15)
Riverificato a campione l'infondato BE1: aperto io il mandato, la riga 10 chiede «preceduto da un controllo che fa fallire la migrazione con un messaggio chiaro».
threat-hunter (0 rilievi), aperto io il punto sicurezza: la prova locale conferma che dai privilegi solo description diventa scrivibile (sezione 1 dello script: description INSERT t e UPDATE t; space_id, created_by, category, starts_on, version in UPDATE f).

FUORI SCOPE:
- BE2 (bassa): separare nello script di collaudo le prove del blocco I da quelle del blocco II. Serve solo se l'utente sceglie la versione senza blocco II.
- BE3 (progetto, per l'utente): script di collaudo che danno da soli l'esito (codice d'uscita di psql) invece del confronto a occhio. Riguarda tutti gli script supabase/verifica-*.sql.
- Nota di threat-hunter, preesistente e non provata: la policy category_budgets_update non richiede is_space_member, quindi un ex membro autore di un budget potrebbe forse aggiornarne amount/ends_on/description dopo aver lasciato lo spazio. Non nasce da questa migrazione.

COLLAUDO SUL SUPABASE LOCALE (supabase db reset, poi gli script; output in scratchpad della sessione):
- `supabase db reset`: tutte le migrazioni applicate, la nuova compresa; btree_gist accettata nello schema extensions.
- supabase/verifica-budget-periodo.sql: attesi 4 errori, ottenuti 4, nelle sezioni giuste:
  - 4a sovrapposto (Spesa 2026-03→2026-05 contro 2026-01→2026-03): atteso ERRORE exclusion; ottenuto `ERROR: conflicting key value violates exclusion constraint "category_budgets_no_overlap"` / `DETAIL: Key conflicts with existing key.`
  - 4b contiguo (2026-04→2026-06 dopo 2026-01→2026-03): atteso 1 riga; ottenuta 1 riga.
  - 4c aperto da 2026-07: 1 riga; seconda parte, chiuso 2027-01→2027-02 dopo un budget aperto: atteso ERRORE exclusion; ottenuto lo stesso errore di 4a.
  - 4d stessa categoria in altro spazio: 1 riga. 4e altra categoria stesso periodo: 1 riga.
  - 4f UPDATE di ends_on che allunga dentro il successivo: atteso ERRORE exclusion; ottenuto lo stesso errore.
  - 4g descrizione di 201 caratteri: atteso ERRORE check; ottenuto `ERROR: new row for relation "category_budgets" violates check constraint "category_budgets_description_max"`.
  - 4h descrizione di 200 caratteri via UPDATE: 1 riga, lunghezza 200. 4i insert senza description (come il client pubblicato): 1 riga, description vuota.
  - sezione 1: description INSERT t, UPDATE t; space_id, created_by, category, starts_on, version in UPDATE f; i due vincoli presenti; btree_gist nello schema extensions. Pulizia: 5 budget prima e 0 dopo nello spazio condiviso, 1 e 0 in quello di Carla.
- supabase/verifica-rls-budget.sql (regressione 2.4): attesi 14 errori, ottenuti 14, gli stessi di prima; il secondo budget aperto (4a) continua a fermarsi sull'indice unico category_budgets_aperto_uq.
- Controllo preliminare: in una transazione, tolto il vincolo e inserite tre righe con una coppia sovrapposta, poi eseguito il file vero della migrazione: atteso fallimento con messaggio chiaro; ottenuto `ERROR: Trovate 1 coppie di budget con periodi sovrapposti per lo stesso spazio e la stessa categoria: il vincolo non si può aggiungere finché non sono corrette.`; dopo il rollback 0 righe, vincolo presente, utente di prova assente.
- Seconda esecuzione della migrazione sul database già migrato: termina senza errori (idempotente).
- Le due query per DBeaver qui sotto, eseguite in locale: «prima» → 0 | 0; «dopo» → tutti i valori uguali all'atteso.

GATE: mcp__synapse__build root=G:\Sviluppo\Eton → build: succeeded — 0 errors, 0 warnings — 9.9s — root: G:/Sviluppo/Eton
GATE: mcp__synapse__test root=G:\Sviluppo\Eton → test: 0 failed, 442 passed, 0 skipped — 6.2s — root: G:/Sviluppo/Eton

SCOSTAMENTI:
- Docker non spento a fine lavoro: alle 19:58 circa era già acceso, con container non avviati da questa sessione (pihole e un altro stack Supabase «Andre_» sulle porte 54321/54322). Spegnerlo li avrebbe fermati; ho fermato solo lo stack di Eton (`supabase stop`, nessun container Eton rimasto).
- Per leggere il messaggio di PostgREST ho creato sul login locale un utente di prova con password generata e mai scritta, e l'ho cancellato subito dopo insieme al suo spazio.
- Il codice HTTP di una violazione è 400, non 409: la 03 deve intercettare il `code` 23P01 nel corpo, non lo stato HTTP.

DECISIONI DELL'UTENTE QUI: nessuna

DA RATIFICARE:
- Nome del check: `category_budgets_description_max` (il mandato fissava solo l'espressione). Raccomandazione: tenerlo, segue `recurring_expenses_installments_max`.

SEGNALAZIONI: nessuna

SQL PER L'UTENTE
(per DBeaver, sul database di produzione, nell'ordine; ogni blocco si esegue con «Esegui script», Alt+X)

(2) PRIMA — sola lettura, dice se ci sono già periodi sovrapposti. Atteso: coppie_sovrapposte = 0.
Se esce più di 0, non applicare la migrazione: va corretto prima il dato.

select count(*) as coppie_sovrapposte, 0 as atteso
  from public.category_budgets a
  join public.category_budgets b
    on a.id < b.id
   and a.space_id = b.space_id
   and a.category = b.category
   and daterange(a.starts_on, (a.ends_on + interval '1 month')::date, '[)')
    && daterange(b.starts_on, (b.ends_on + interval '1 month')::date, '[)');

(1) LA MIGRAZIONE — testo integrale di supabase/migrations/20261120000000_budget_periodo.sql.
Se il controllo preliminare trova sovrapposizioni si ferma con il messaggio «Trovate N coppie di budget…».

-- =====================================================================================
-- Eton — budget: descrizione libera e divieto di periodi sovrapposti.
-- Idempotente e rieseguibile, additiva: il client già pubblicato non legge né scrive
-- description, e il default '' copre i suoi INSERT.
--
-- Dipende da 20261024000000_budget.sql per la tabella category_budgets, il suo indice
-- unico category_budgets_aperto_uq, le policy e i grant di colonna.
--
-- Due blocchi indipendenti: il BLOCCO II si può togliere per intero senza toccare il
-- BLOCCO I.
-- =====================================================================================

-- =====================================================================================
-- BLOCCO I — descrizione
-- =====================================================================================
alter table public.category_budgets add column if not exists description text not null default '';

alter table public.category_budgets drop constraint if exists category_budgets_description_max;
alter table public.category_budgets
    add constraint category_budgets_description_max check (char_length(description) <= 200);

-- Forma minima come 20260903000000_grant_insert_blind.sql: una sola colonna, senza
-- ripetere l'elenco completo di 20261024000000_budget.sql, e INSERT e UPDATE affiancati.
-- Nessun altro grant e nessun revoke. Il trigger handle_category_budget_update non si
-- tocca: description resta modificabile, come amount.
grant insert (description) on public.category_budgets to authenticated;
grant update (description) on public.category_budgets to authenticated;

-- =====================================================================================
-- BLOCCO II — nessun periodo sovrapposto; si può togliere per intero
-- =====================================================================================
-- btree_gist serve per usare `=` su uuid e text dentro un indice GiST.
create extension if not exists btree_gist with schema extensions;

-- Controllo preliminare: se i dati già presenti contengono periodi sovrapposti per lo
-- stesso spazio e la stessa categoria, il vincolo non si può aggiungere. L'espressione
-- del periodo è la stessa del vincolo qui sotto.
do $$
declare
    coppie bigint;
begin
    select count(*) into coppie
      from public.category_budgets a
      join public.category_budgets b
        on a.id < b.id
       and a.space_id = b.space_id
       and a.category = b.category
       and daterange(a.starts_on, (a.ends_on + interval '1 month')::date, '[)')
        && daterange(b.starts_on, (b.ends_on + interval '1 month')::date, '[)');
    if coppie > 0 then
        raise exception 'Trovate % coppie di budget con periodi sovrapposti per lo stesso spazio e la stessa categoria: il vincolo non si può aggiungere finché non sono corrette.', coppie;
    end if;
end;
$$;

-- Il periodo va da starts_on al giorno prima del mese successivo a ends_on, quindi
-- l'ultimo giorno del mese di ends_on è incluso. Con ends_on null il limite superiore è
-- null, cioè senza fine. Con '[)' due budget contigui (uno finisce a gennaio, l'altro
-- inizia a febbraio) non si toccano.
-- L'indice unico category_budgets_aperto_uq resta e le policy non cambiano. Una
-- violazione dà il codice 23P01 (exclusion_violation).
alter table public.category_budgets drop constraint if exists category_budgets_no_overlap;
alter table public.category_budgets
    add constraint category_budgets_no_overlap
    exclude using gist (
        space_id with =,
        category with =,
        daterange(starts_on, (ends_on + interval '1 month')::date, '[)') with &&
    );

(3) DOPO — sola lettura, una riga sola; ogni valore deve essere uguale all'«atteso» accanto
(true = spunta, oppure il testo extensions):

select
    exists (select 1 from information_schema.columns
             where table_schema = 'public' and table_name = 'category_budgets'
               and column_name = 'description' and is_nullable = 'NO'
               and column_default = '''''::text')                                    as colonna_presente,   true as atteso_1,
    exists (select 1 from pg_constraint
             where conrelid = 'public.category_budgets'::regclass
               and conname = 'category_budgets_description_max' and contype = 'c')  as check_presente,     true as atteso_2,
    has_column_privilege('authenticated', 'public.category_budgets', 'description', 'INSERT') as grant_insert, true as atteso_3,
    has_column_privilege('authenticated', 'public.category_budgets', 'description', 'UPDATE') as grant_update, true as atteso_4,
    (select n.nspname from pg_extension e join pg_namespace n on n.oid = e.extnamespace
      where e.extname = 'btree_gist')                                               as schema_estensione,  'extensions' as atteso_5,
    exists (select 1 from pg_constraint
             where conrelid = 'public.category_budgets'::regclass
               and conname = 'category_budgets_no_overlap' and contype = 'x')       as vincolo_presente,   true as atteso_6;

(Con la versione senza blocco II, atteso_5 diventa vuoto e vincolo_presente false.)

(4) RIPIEGO — se l'app, dopo la migrazione, dà un errore di «colonna sconosciuta» su description:

NOTIFY pgrst, 'reload schema';

(5) VERSIONE SENZA BLOCCO II — se non si vuole l'estensione btree_gist: solo la descrizione, nessun divieto di
sovrapposizione nel database. In quel caso la query (2) non serve.

alter table public.category_budgets add column if not exists description text not null default '';

alter table public.category_budgets drop constraint if exists category_budgets_description_max;
alter table public.category_budgets
    add constraint category_budgets_description_max check (char_length(description) <= 200);

grant insert (description) on public.category_budgets to authenticated;
grant update (description) on public.category_budgets to authenticated;

COMMIT: 6b0f857

COMPLETATO 6b0f857
