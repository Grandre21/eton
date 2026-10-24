UNITÀ: 1 — ESITO: FATTO
TOCCATI:
  supabase/migrations/20261024000000_budget.sql → +128/−0 (nuovo)
  supabase/verifica-rls-budget.sql → +293/−0 (nuovo)
  handoff/01-migrazione/ → brief-codex-1.md, brief-codex-2.md, codex-1.md, codex-2.md, resoconto.md (registro dell'unità)
REVIEW:
  review: brief-codex-1 — migrazione e collaudo
    autore          codex exec codex-cli 0.161.0 — 2 files changed, 427 insertions(+)
    bug-hunter      RILIEVI: 1
    conformity      RILIEVI: 0
    threat-hunter   RILIEVI: 0
    backend-expert  RILIEVI: 4
    checker         VERDETTI: fondati 0 · infondati 0 · fuori scope 1 · non verificabili 0
                    fix: risolti 1 · non risolti 0 · non verificabili 0
  review: brief-codex-2 — vincoli dentro il create table (correzione di BE1)
    autore          codex exec codex-cli 0.161.0 (resume) — 2 files changed, 421 insertions(+) (diff cumulativo dell'unità)
CONTRATTI:
  tabella `category_budgets` — supabase/migrations/20261024000000_budget.sql:11-37:
    `id uuid primary key default gen_random_uuid()` (:12)
    `space_id uuid not null references public.spaces (id) on delete cascade` (:13)
    `created_by uuid not null default auth.uid() references auth.users (id) on delete cascade` (:16-17) ← **nome della colonna autore: `created_by`** (deciso dal capo, v. SCOSTAMENTI)
    `category text not null check (length(btrim(category)) between 1 and 40)` (:18)
    `amount numeric(12,2) not null check (amount > 0)` (:19)
    `starts_on date not null check (starts_on between '2000-01-01' and '2999-12-31')` (:22), `constraint category_budgets_starts_on_primo_del_mese check (extract(day from starts_on) = 1)` (:33)
    `ends_on date null check (ends_on <= '2999-12-31')` (:27), `constraint category_budgets_ends_on_primo_del_mese check (extract(day from ends_on) = 1)` (:35), `constraint category_budgets_ends_on_dopo_inizio check (ends_on >= starts_on)` (:36)
    `version integer not null default 1` (:28); `created_at timestamptz not null default now()` (:29); `updated_at timestamptz not null default now()` (:30)
  indice `create unique index if not exists category_budgets_aperto_uq on public.category_budgets (space_id, category) where ends_on is null;` (:43-44)
  privilegi `grant insert (id, space_id, created_by, category, amount, starts_on, ends_on)` (:101-102); `grant update (amount, ends_on)` (:107) — `version` NON aggiornabile dal client, v. DA RATIFICARE
  concorrenza: trigger `category_budgets_before_update` → `handle_category_budget_update()` (:48-69): version+1, updated_at, e rimette created_by, space_id, created_at, category, starts_on. Il client della 02 filtra con `WHERE id = x AND version = letta` e fa `.Set` solo su `amount`/`ends_on`, come `Services/RecurringExpenseRepository.cs:81-115`.
  check `alter table public.recurring_expenses add constraint recurring_expenses_installments_max check (installments is null or installments <= 60);` (:126-128)
ADJUDICA:
  - bug-hunter BH1: fuori scope — reale (provato dal checker sul locale): l'indice impedisce due budget *aperti*, non la sovrapposizione di periodi; chiudendo «Uscite» con ends_on = mese corrente e aprendone uno nuovo con starts_on = stesso mese, quel mese ha due budget validi. Il contratto chiede solo l'indice parziale; il vincolo completo (EXCLUDE con daterange) richiede l'estensione btree_gist e cambia il contratto → al capo, v. FUORI SCOPE — `create unique index if not exists category_budgets_aperto_uq on public.category_budgets (space_id, category) where ends_on is null;`
  - backend-expert BE1: fondato → corretto — tre check di una tabella nuova aggiunti fuori dal create table con drop/add, forma inutile su una tabella nata qui — `constraint category_budgets_starts_on_primo_del_mese check (extract(day from starts_on) = 1),`
    verificato: risolto — supabase/migrations/20261024000000_budget.sql:33-36
  - backend-expert BE2: infondato — il blocco `do` rifà il controllo che ADD CONSTRAINT fa da sé, ma il mandato chiede che la migrazione «regga su dati esistenti» e la applica a mano l'utente dall'editor SQL: il messaggio in italiano con quanti piani e il massimo gli dice cosa correggere, l'errore di Postgres no. Riverificato da me sul locale (piano da 72 rate → «Trovati 1 piani con più di 60 rate (massimo 72)…») — `raise exception 'Trovati % piani con più di 60 rate (massimo %): …', righe, massimo;`
  - backend-expert BE3: fuori scope — TIPO: progetto (privilegi del collaudo su una riga di 15 booleani invece di una riga per controllo); forma comune a tutti gli script `verifica-rls-*.sql`, va all'utente — `\echo '=== 1. Privilegi (atteso: 8 t, 6 f, poi RLS t) ==='`
  - backend-expert BE4: fuori scope — TIPO: progetto (claims JWT ripetuti 31 volte); è il punto 4 dei residui della spec §5, già assegnato all'unità 03 — `set local request.jwt.claims = '{"sub":"a1100000-…","role":"authenticated"}';`
  Campione sugli infondati (§5): BE2 riverificato da me eseguendo la guardia sul locale, esito sopra. Rilievi di sicurezza: threat-hunter 0; ho riletto io RLS e privilegi (:74-108) e il collaudo li conferma (sezioni 4g, 5a-5d, 6a-6e, 8a-8b).
FUORI SCOPE:
  - BH1, sovrapposizione di periodi per la stessa categoria. Raccomandazione per il mandato della 02: un budget nuovo su una categoria il cui ultimo budget si chiude nel mese corrente parte dal mese successivo (oppure: chiudere e riaprire nello stesso mese = cambiare l'importo). In alternativa un vincolo di esclusione nel database con btree_gist, che è un'estensione da abilitare: decisione dell'utente.
  - BE3, collaudo dei privilegi una riga per controllo: decisione di progetto, da proporre all'utente insieme al riordino degli script della 03.
  - BE4, claims JWT in un posto solo: già nel mandato atteso della 03 (spec §5 punto 4).
  - Nota di threat-hunter (non contata come rilievo): un ex membro che ha creato un budget conserva in teoria UPDATE/DELETE sulle sue righe senza WHERE, ma PostgREST lo impedisce (safeupdate e policy di SELECT su ogni filtro). È lo stesso schema di expenses, recurring_expenses, note e collections.
GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\01-migrazione → build: succeeded — 0 errors, 0 warnings — 2.6s
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\01-migrazione → test: 0 failed, 417 passed, 0 skipped — 4.2s
PROVA SUL SUPABASE LOCALE (dopo la correzione, sul file committato):
  - `supabase db reset`: applicate tutte le migrazioni fino a 20261024000000_budget.sql, «Finished supabase db reset».
  - `docker exec -i supabase_db_Eton psql … < supabase/verifica-rls-budget.sql`: errori attesi 14 (4a, 4b, 4c, 4d, 4e, 4f, 4g, 5b, 6a, 6b, 6c, 6d, 6e, 11a), ottenuti 14: 1 unique category_budgets_aperto_uq, 1 check amount, 1 check category, 1 ends_on_dopo_inizio, 1 ends_on_primo_del_mese, 1 starts_on_primo_del_mese, 1 recurring_expenses_installments_max, 2 RLS, 5 permission denied. Sezione 1: 8 t, 6 f, RLS t. Sezione 7: version 2, poi 0 righe. Sezione 10: space_id, created_by Alice, Uscite, 2026-10-01 invariati. Pulizia: 3 budget prima, 0 dopo.
  - Seconda esecuzione della migrazione sullo stesso database: exit 0, solo NOTICE «already exists, skipping».
  - Guardia: con un piano da 72 rate (vincolo tolto in transazione) la migrazione si ferma con «Trovati 1 piani con più di 60 rate (massimo 72): il vincolo non si può aggiungere finché quei piani non sono corretti.», poi rollback.
  - Query di controllo qui sotto, sul locale: `t | t | 4 | t | t | t | 0`.
  - Docker avviato e spento da questa sessione (`docker desktop stop`, nessun processo Docker rimasto). All'avvio Docker ha fatto ripartire da sé i container `supabase_*_Andre_` di un altro progetto (porte 54321/54322): non li ho toccati, si sono fermati con Docker.

SQL PER L'UTENTE

Ordine: (0) controllo preliminare, (1) migrazione, (2) query di controllo, (3) ripiego.

(0) Controllo preliminare, facoltativo, prima della migrazione — dice se il tetto alle rate passerà:

    select count(*) filter (where installments > 60) as piani_oltre_60,
           coalesce(max(installments), 0) as max_rate
      from public.recurring_expenses;

  Atteso: `piani_oltre_60 = 0`, `max_rate ≤ 60`. Se `piani_oltre_60` è più di zero, la migrazione si fermerebbe con «Trovati N piani con più di 60 rate…» e non cambierebbe niente (l'editor SQL la esegue in un blocco solo). Cosa fare in quel caso: vedere quali sono con
  `select id, space_id, description, installments, plan_total from public.recurring_expenses where installments > 60;`
  e decidere piano per piano (accorciarlo, o togliere il piano dalla regola); il client non permette di crearne di così lunghi, quindi una riga così può venire solo dall'API diretta. Poi rilanciare la migrazione.

(1) La migrazione, testo integrale di supabase/migrations/20261024000000_budget.sql (commit 6fb3bfb):

    -- =====================================================================================
    -- Eton — budget per categoria e tetto alle rate delle regole ricorrenti.
    -- Idempotente e rieseguibile, additiva: il client già pubblicato non legge la nuova tabella.
    --
    -- Dipende da 20260811000000_initial_schema.sql, da cui riusa senza modificarle
    -- is_space_member() e is_space_owner(): la visibilità segue l'appartenenza allo spazio.
    -- Dipende anche da 20261020000000_rate_entrate.sql per la colonna installments.
    -- =====================================================================================

    -- ---------- category_budgets ----------
    create table if not exists public.category_budgets (
        id           uuid primary key default gen_random_uuid(),
        space_id     uuid not null references public.spaces (id) on delete cascade,

        -- Il budget non ha un pagante: l'autore serve a distinguere chi può modificarlo.
        created_by   uuid not null default auth.uid()
                        references auth.users (id) on delete cascade,
        category     text not null check (length(btrim(category)) between 1 and 40),
        amount       numeric(12,2) not null check (amount > 0),

        -- Il limite evita date estreme che manderebbero in overflow i calcoli mensili del client.
        starts_on    date not null check (starts_on between '2000-01-01' and '2999-12-31'),

        -- Un budget è una regola valida ogni mese da starts_on finché non si chiude.
        -- ends_on è l'ultimo mese in cui vale, rappresentato dal suo primo giorno:
        -- chiuderlo significa scrivere ends_on, non cancellare la storia della regola.
        ends_on      date null check (ends_on <= '2999-12-31'),
        version      integer not null default 1,
        created_at   timestamptz not null default now(),
        updated_at   timestamptz not null default now(),

        -- Il periodo è mensile: usare sempre il primo giorno evita confini ambigui nel client.
        constraint category_budgets_starts_on_primo_del_mese check (extract(day from starts_on) = 1),
        -- Con ends_on NULL i check sono soddisfatti (UNKNOWN è ammesso): il budget resta aperto.
        constraint category_budgets_ends_on_primo_del_mese   check (extract(day from ends_on) = 1),
        constraint category_budgets_ends_on_dopo_inizio      check (ends_on >= starts_on)
    );

    -- La lettura dei budget parte sempre dallo spazio corrente.
    create index if not exists category_budgets_space_idx
        on public.category_budgets (space_id);
    -- Al più una regola aperta per categoria e spazio; chiuderla permette di aprirne una nuova.
    create unique index if not exists category_budgets_aperto_uq
        on public.category_budgets (space_id, category) where ends_on is null;

    -- ---------- versione e data di modifica ----------
    -- Non SECURITY DEFINER: come handle_recurring_expense_update(), il BEFORE scrive NEW senza grant di colonna.
    create or replace function public.handle_category_budget_update()
    returns trigger language plpgsql
    set search_path = public as $$
    begin
        new.version    := old.version + 1;
        new.updated_at := now();

        -- Ultima riga di difesa: identità e periodo iniziale si decidono solo alla creazione.
        new.created_by := old.created_by;
        new.space_id   := old.space_id;
        new.created_at := old.created_at;
        new.category   := old.category;
        new.starts_on  := old.starts_on;

        return new;
    end;
    $$;

    drop trigger if exists category_budgets_before_update on public.category_budgets;
    create trigger category_budgets_before_update
        before update on public.category_budgets
        for each row execute function public.handle_category_budget_update();

    -- =====================================================================================
    -- RLS
    -- =====================================================================================
    alter table public.category_budgets enable row level security;

    -- Tutti i membri leggono il budget condiviso, come le regole ricorrenti.
    drop policy if exists category_budgets_select on public.category_budgets;
    create policy category_budgets_select on public.category_budgets
        for select using (public.is_space_member(space_id));

    -- Un membro può creare solo regole intestate a se stesso.
    drop policy if exists category_budgets_insert on public.category_budgets;
    create policy category_budgets_insert on public.category_budgets
        for insert with check (public.is_space_member(space_id) and created_by = auth.uid());

    -- Autore e proprietario possono gestire la regola; with check esplicito per leggibilità.
    drop policy if exists category_budgets_update on public.category_budgets;
    create policy category_budgets_update on public.category_budgets
        for update using (created_by = auth.uid() or public.is_space_owner(space_id))
                  with check (created_by = auth.uid() or public.is_space_owner(space_id));
    drop policy if exists category_budgets_delete on public.category_budgets;
    create policy category_budgets_delete on public.category_budgets
        for delete using (created_by = auth.uid() or public.is_space_owner(space_id));

    -- =====================================================================================
    -- Privilegi: la RLS filtra, non concede; senza GRANT la query fallisce prima delle policy.
    -- =====================================================================================
    revoke all on public.category_budgets from anon, authenticated;
    grant select, delete on public.category_budgets to authenticated;
    -- id lo genera il client, per poter ritentare un inserimento dopo un errore di rete.
    grant insert (id, space_id, created_by, category, amount, starts_on, ends_on)
        on public.category_budgets to authenticated;
    -- Si aggiornano solo importo e chiusura. Mai version/created_at/updated_at: li scrive il trigger.
    -- La concorrenza ottimistica usa version come filtro nel WHERE, che richiede solo SELECT.
    -- Mai space_id/created_by: sposterebbero il budget sotto un'altra visibilità.
    -- Mai category/starts_on: si decidono alla creazione, come kind nei movimenti.
    grant update (amount, ends_on) on public.category_budgets to authenticated;
    grant all on public.category_budgets to service_role;

    -- ---------- tetto alle rate ----------
    -- Il client limita già i piani a 60 rate: si rifiuta solo ciò che già rifiuta il client.
    -- Il vincolo difende anche dall'API diretta e dai client vecchi; NULL resta senza piano.
    do $$
    declare
        righe bigint;
        massimo smallint;
    begin
        select count(*), max(installments) into righe, massimo
          from public.recurring_expenses where installments > 60;
        if righe > 0 then
            raise exception 'Trovati % piani con più di 60 rate (massimo %): il vincolo non si può aggiungere finché quei piani non sono corretti.', righe, massimo;
        end if;
    end;
    $$;

    alter table public.recurring_expenses drop constraint if exists recurring_expenses_installments_max;
    alter table public.recurring_expenses
        add constraint recurring_expenses_installments_max check (installments is null or installments <= 60);

(2) Query di controllo, da lanciare dopo — restituisce una riga sola:

    select
        to_regclass('public.category_budgets') is not null as tabella_esiste,
        coalesce((select relrowsecurity from pg_class
                   where oid = to_regclass('public.category_budgets')), false) as rls_attiva,
        (select count(*) from pg_policies
          where schemaname = 'public' and tablename = 'category_budgets') as policy,
        exists (select 1 from pg_indexes
                 where schemaname = 'public' and indexname = 'category_budgets_aperto_uq') as indice_aperto,
        exists (select 1 from pg_trigger
                 where tgname = 'category_budgets_before_update') as trigger_versione,
        exists (select 1 from pg_constraint
                 where conname = 'recurring_expenses_installments_max'
                   and conrelid = 'public.recurring_expenses'::regclass) as tetto_rate,
        (select coalesce(max(installments), 0) from public.recurring_expenses) as max_rate;

  Risultato atteso: `tabella_esiste = true` · `rls_attiva = true` · `policy = 4` · `indice_aperto = true` · `trigger_versione = true` · `tetto_rate = true` · `max_rate ≤ 60` (sul locale vuoto: 0).

(3) Ripiego: se l'app, dopo la migrazione, dà un errore di «tabella sconosciuta» (PostgREST non ha ancora ricaricato lo schema), lanciare:

    NOTIFY pgrst, 'reload schema';

SCOSTAMENTI:
  - Domanda al capo (inviata 2026-10-24 sera, prima delle 22:21; risposta ricevuta prima delle 22:21, orari esatti non annotati — il primo `date` della sessione è delle 22:21, subito dopo la risposta): «la colonna autore si chiama paid_by (come recurring_expenses) o created_by, e update/delete restano ad autore o proprietario?». Risposta di coordinatore-Eton: «A: created_by — B: autore-o-proprietario. A: stesso tipo, default auth.uid(), FK e privilegio (solo INSERT) di paid_by delle ricorrenti; cambia solo il nome. […] B: copia la forma delle ricorrenti: SELECT a ogni membro, INSERT a un membro con created_by = auth.uid(), UPDATE/DELETE all'autore o al proprietario dello spazio. Scrivi la scelta in SCOSTAMENTI, perché la spec dice solo «membri dello spazio».»
  - Il contratto diceva «colonne … autore … con gli stessi nomi di recurring_expenses»: la colonna è `created_by`, non `paid_by`, per la risposta del capo qui sopra.
  - Policy: UPDATE/DELETE all'autore o al proprietario dello spazio, non a ogni membro (la spec diceva solo «membri dello spazio»), per la risposta del capo qui sopra.
  - Il contratto diceva «`amount`, `ends_on`, `version` aggiornabili»: `version` NON ha il privilegio di UPDATE, perché il contratto dice anche «concorrenza ottimistica su version come le ricorrenti», e nelle ricorrenti version è solo un filtro e la scrive il trigger (`Services/RecurringExpenseRepository.cs:75-76`; `20260922000000_ricorrenti.sql:213-217`). Anche col privilegio il trigger la sovrascriverebbe. V. DA RATIFICARE.
  - Il trigger rimette anche `category` e `starts_on` (oltre a created_by, space_id, created_at), sul modello di `kind` in rate_entrate.sql: difesa in più rispetto alla copia letterale delle ricorrenti.
  - Il log di Codex sta fuori dal repo (C:\Users\Andre!\.claude\jobs\51865700\tmp\codex-1.log, codex-2.log); il checker non vi ha trovato forme vietate (scritture solo via apply_patch).
DECISIONI DELL'UTENTE QUI: nessuna
DA RATIFICARE:
  - `version` non aggiornabile dal client (solo filtro, la scrive il trigger), contro la lettera del contratto. Raccomandazione: confermare, è la forma delle ricorrenti e la 02 la usa identica.
  - Il trigger blocca anche la modifica di `category` e `starts_on`. Raccomandazione: confermare; cambiare categoria = chiudere e aprire un budget nuovo.
  - BE2 tenuto: blocco di guardia sul tetto alle rate con il messaggio in italiano. Raccomandazione: confermare.
SEGNALAZIONI: nessuna
COMMIT: 6fb3bfb
