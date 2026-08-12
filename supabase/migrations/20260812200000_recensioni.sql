-- =====================================================================================
-- Eton — recensioni: un voto e/o un commento per elemento, un voto a testa per utente.
-- Idempotente e rieseguibile.
--
-- Dipende dalla migrazione 20260811000000_initial_schema.sql, da cui riusa senza modificarle le
-- funzioni is_space_member() e is_space_owner(), e da 20260812120000_collections.sql, di cui
-- collection_items è il bersaglio della chiave esterna composita qui sotto.
-- =====================================================================================

-- ---------- reviews ----------
-- space_id è duplicato invece di essere ricavato dall'elemento: serve alle policy per controllare
-- l'appartenenza allo spazio SENZA join. Senza, la policy dovrebbe risalire
-- reviews → collection_items → collections → space_id, due join per ogni riga controllata. La FK
-- composita (item_id, space_id) qui sotto lo rende impossibile da far divergere: il database
-- rifiuta la riga incoerente. Stesso ragionamento di space_id in collection_items.
create table if not exists public.reviews (
    id         uuid primary key default gen_random_uuid(),
    item_id    uuid not null,
    space_id   uuid not null,

    -- default auth.uid() come rete di sicurezza: la policy di INSERT pretende comunque
    -- user_id = auth.uid(), quindi una recensione con l'autore sbagliato viene respinta invece
    -- che salvata storta (stesso ragionamento di owner_id in collections).
    user_id    uuid not null default auth.uid()
                   references auth.users (id) on delete cascade,

    -- Nullable: una recensione può essere solo un commento, senza voto (il vincolo
    -- reviews_non_vuota più sotto impedisce però la riga completamente vuota — senza, un
    -- salvataggio con entrambi i campi vuoti creerebbe righe fantasma che compaiono nel conteggio
    -- delle recensioni senza mostrare niente).
    --
    -- rating > 0 e non >= 0: lo zero non è un voto, è l'assenza di voto, e per quella c'è null.
    -- Ammetterli entrambi darebbe due modi di dire la stessa cosa e due comportamenti da tenere
    -- allineati.
    --
    -- numeric(3,1) e non smallint: mezzo punto. Il massimo rappresentabile è 99.9, ben oltre il
    -- 10 del vincolo.
    --
    -- Il vincolo "il voto non supera rating_max della collezione" NON è esprimibile in un
    -- check: attraverserebbe tre tabelle (reviews → collection_items → collections). Il database
    -- garantisce solo 0 < rating <= 10; la coerenza con rating_max è responsabilità
    -- dell'interfaccia ed è aggirabile da chi chiama PostgREST a mano. È accettato
    -- consapevolmente: un voto fuori scala è un fastidio estetico, non una falla — rinuncia
    -- deliberata, pesata, non una svista.
    rating     numeric(3,1) check (rating is null or (rating > 0 and rating <= 10)),
    comment    text check (comment is null or length(comment) <= 4000),

    -- Concorrenza ottimistica, stesso meccanismo di collections e collection_items.
    version    integer not null default 1,

    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),

    -- Un predicato positivo, non btrim: "togli il vuoto e vedi se resta qualcosa" richiede
    -- l'elenco completo dei caratteri da togliere, una lista da tenere aggiornata a mano — ed è
    -- proprio da lì che sfuggiva il carattere di tabulazione, che btrim() senza secondo argomento
    -- non rimuove (rimuove solo lo spazio ASCII 0x20). "~ '\S'" chiede invece "esiste almeno un
    -- carattere non-spazio", che non ha nessuna lista da dimenticare.
    constraint reviews_non_vuota
        check (rating is not null or coalesce(comment, '') ~ '\S'),

    foreign key (item_id, space_id)
        references public.collection_items (id, space_id) on delete cascade,

    -- Un voto a testa per elemento. È anche l'indice che serve alle query per elemento, quindi
    -- non ne va aggiunto un altro su item_id.
    unique (item_id, user_id)
);

-- L'elenco degli elementi di una collezione carica le recensioni FILTRANDO PER SPAZIO, non per un
-- elenco di item_id: con qualche centinaio di elementi un filtro item_id=in.(...) produrrebbe un
-- URL da decine di kilobyte.
create index if not exists reviews_space_idx on public.reviews (space_id);

-- ---------- versione e data di modifica ----------
-- Come handle_collection_item_update in 20260812120000_collections.sql: non è SECURITY DEFINER e
-- non deve esserlo, perché i privilegi di colonna si verificano sulle colonne NOMINATE
-- nell'istruzione UPDATE, non su quelle che un trigger BEFORE tocca dopo. Un BEFORE UPDATE può
-- quindi scrivere version anche se il chiamante non avrebbe il diritto di scriverla direttamente.
create or replace function public.handle_review_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Rimesse a forza al valore precedente: una recensione non cambia autore, non cambia
    -- elemento, non cambia spazio, e non ringiovanisce, anche se un domani una funzione SECURITY
    -- DEFINER scrivesse su reviews scavalcando i privilegi.
    new.user_id    := old.user_id;
    new.item_id    := old.item_id;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;

    return new;
end;
$$;

drop trigger if exists reviews_before_update on public.reviews;
create trigger reviews_before_update
    before update on public.reviews
    for each row execute function public.handle_review_update();

-- =====================================================================================
-- RLS
-- =====================================================================================

alter table public.reviews enable row level security;

drop policy if exists reviews_select on public.reviews;
create policy reviews_select on public.reviews
    for select using (public.is_space_member(space_id));

drop policy if exists reviews_insert on public.reviews;
create policy reviews_insert on public.reviews
    for insert with check (user_id = auth.uid() and public.is_space_member(space_id));

-- Modificare e cancellare: SOLO l'autore, mai il proprietario dello spazio — a differenza di
-- collections e collection_items, dove il proprietario può fare pulizia a casa propria. Qui no,
-- perché un voto è un'opinione personale e riscriverla sarebbe falsificarla, non moderare. Chi
-- vuole togliere una recensione altrui deve cancellare l'elemento (v. l'ON DELETE CASCADE della
-- chiave esterna composita sopra).
--
-- `with check` esplicito e identico a `using`, come nelle migrazioni precedenti: omettendolo
-- Postgres userebbe comunque `using` per entrambe le fasi, quindi scriverlo non aggiunge
-- protezione, rende solo la regola leggibile senza conoscere quel dettaglio.
drop policy if exists reviews_update on public.reviews;
create policy reviews_update on public.reviews
    for update using      (user_id = auth.uid())
              with check  (user_id = auth.uid());

drop policy if exists reviews_delete on public.reviews;
create policy reviews_delete on public.reviews
    for delete using (user_id = auth.uid());

-- =====================================================================================
-- Privilegi.
--
-- Stesso doppio livello di collections e collection_items: la RLS FILTRA, non concede, e i
-- privilegi di COLONNA fermano ciò che una policy non può — in una policy non esiste OLD, quindi
-- nessuna WITH CHECK può impedire che version torni indietro o che i legami fra le righe cambino
-- di nascosto.
-- =====================================================================================

revoke all on public.reviews from anon, authenticated;

-- version, created_at e updated_at non compaiono in nessun grant: li scrive il trigger, e il
-- client non deve poterli falsificare (v. handle_review_update sopra).
grant select, delete on public.reviews to authenticated;
grant insert (item_id, space_id, user_id, rating, comment) on public.reviews to authenticated;

-- item_id e space_id NON sono aggiornabili: spostare una recensione su un altro elemento
-- cambierebbe CHI HA IL DIRITTO DI LEGGERLA, cioè sarebbe una fuga di dati — non diverso dal
-- motivo per cui collection_items non concede l'UPDATE su collection_id e space_id.
grant update (rating, comment) on public.reviews to authenticated;

grant all on public.reviews to service_role;
