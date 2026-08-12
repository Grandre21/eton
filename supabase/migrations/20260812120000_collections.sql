-- =====================================================================================
-- Eton — collezioni: raccolte tematiche per spazio, con elementi e concorrenza ottimistica.
-- Idempotente e rieseguibile.
--
-- Dipende dalla migrazione 20260811000000_initial_schema.sql, da cui riusa senza modificarle
-- le funzioni is_space_member() e is_space_owner(): la regola d'accesso di una collezione e dei
-- suoi elementi è già interamente contenuta nell'appartenenza allo spazio, esattamente come per
-- notes (v. 20260812000000_note.sql).
-- =====================================================================================

-- ---------- collections ----------
create table if not exists public.collections (
    id          uuid primary key default gen_random_uuid(),
    space_id    uuid not null references public.spaces (id) on delete cascade,

    -- default auth.uid() come rete di sicurezza: la policy di INSERT pretende comunque
    -- owner_id = auth.uid(), quindi una collezione con l'autore sbagliato viene respinta invece
    -- che salvata storta (stesso ragionamento di owner_id in notes).
    owner_id    uuid not null default auth.uid()
                    references auth.users (id) on delete cascade,

    -- btrim e non la lunghezza grezza: un nome di soli spazi supererebbe il controllo e
    -- comparirebbe nell'elenco come una riga vuota su cui si può cliccare — visibilmente rotta e
    -- impossibile da ritrovare. length() conta caratteri, non byte, quindi il limite resta
    -- espresso in caratteri come per spaces.name.
    name        text not null check (length(btrim(name)) between 1 and 100),
    icon        text check (icon is null or length(icon) <= 16),
    fields      jsonb not null default '[]'::jsonb,
    rating_max  smallint not null default 10 check (rating_max in (5, 10)),

    -- Concorrenza ottimistica, stesso meccanismo di notes: il client rimanda la versione che
    -- aveva letto come FILTRO (?version=eq.N), non come valore da scrivere.
    version     integer not null default 1,

    created_at  timestamptz not null default now(),
    updated_at  timestamptz not null default now(),

    -- jsonb_array_length su un valore che non è un array solleva un'eccezione, e SQL non
    -- garantisce che l'AND valuti da sinistra a destra: senza il ripiego del CASE, un fields di
    -- tipo sbagliato darebbe un errore di runtime invece di una violazione di vincolo leggibile.
    constraint collections_fields_shape check (
        jsonb_typeof(fields) = 'array'
        and jsonb_array_length(
                case when jsonb_typeof(fields) = 'array' then fields else '[]'::jsonb end
            ) <= 40
    ),

    -- Chiave composita referenziata dalla foreign key composita di collection_items qui sotto:
    -- è quello che rende impossibile a un elemento avere uno space_id diverso da quello della
    -- propria collezione.
    unique (id, space_id)
);

-- Composito e non due indici separati, per lo stesso motivo di notes_space_updated_idx: serve
-- "le collezioni di questo spazio, dalla più recente", e questo indice copre sia il filtro sia
-- l'ordinamento.
create index if not exists collections_space_updated_idx
    on public.collections (space_id, updated_at desc);

-- ---------- collection_items ----------
-- space_id è duplicato invece di essere ricavato dalla collezione: serve alle policy per
-- controllare l'appartenenza allo spazio SENZA join, e la chiave esterna composita
-- (collection_id, space_id) qui sotto rende impossibile che diverga dal valore della collezione
-- — il database rifiuta la riga incoerente. È denormalizzazione resa sicura dal vincolo, non
-- copia sorvegliata a mano.
create table if not exists public.collection_items (
    id            uuid primary key default gen_random_uuid(),
    collection_id uuid not null,
    space_id      uuid not null,

    added_by      uuid not null default auth.uid()
                      references auth.users (id) on delete cascade,

    name          text not null check (length(btrim(name)) between 1 and 200),
    image_url     text check (image_url is null or length(image_url) <= 2000),
    data          jsonb not null default '{}'::jsonb,

    version       integer not null default 1,

    created_at    timestamptz not null default now(),
    updated_at    timestamptz not null default now(),

    constraint collection_items_data_shape check (jsonb_typeof(data) = 'object'),

    foreign key (collection_id, space_id)
        references public.collections (id, space_id) on delete cascade,

    unique (id, space_id)
);

-- Composito su (collection_id, name): serve "gli elementi di questa collezione, in ordine
-- alfabetico" — non per data, a differenza di ogni altro indice del progetto. Un catalogo si
-- consulta cercando un nome che si ha già in mente; note e collezioni si consultano invece per
-- vedere cosa è cambiato di recente. Sono due modi di leggere diversi, e collection_items è
-- l'unica tabella che segue il primo.
create index if not exists collection_items_collection_name_idx
    on public.collection_items (collection_id, name);

-- ---------- versione e data di modifica ----------
-- Due funzioni separate, una per tabella, invece di una sola generica: i nomi delle colonne da
-- fissare sono diversi fra collections e collection_items, e una funzione generica dovrebbe
-- manipolare NEW come jsonb per riuscirci — più fragile e molto meno leggibile del doppione.
--
-- Come handle_note_update in 20260812000000_note.sql: non è SECURITY DEFINER e non deve
-- esserlo, perché i privilegi di colonna si verificano sulle colonne NOMINATE nell'istruzione
-- UPDATE, non su quelle che un trigger BEFORE tocca dopo. Un BEFORE UPDATE può quindi scrivere
-- version anche se il chiamante non avrebbe il diritto di scriverla direttamente.
create or replace function public.handle_collection_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Rimesse a forza al valore precedente: una collezione non cambia proprietario, non cambia
    -- spazio e non ringiovanisce, anche se un domani una funzione SECURITY DEFINER scrivesse su
    -- collections scavalcando i privilegi.
    new.owner_id   := old.owner_id;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;

    return new;
end;
$$;

drop trigger if exists collections_before_update on public.collections;
create trigger collections_before_update
    before update on public.collections
    for each row execute function public.handle_collection_update();

create or replace function public.handle_collection_item_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version       := old.version + 1;
    new.updated_at    := now();

    -- Come sopra: chi ha aggiunto l'elemento, la sua collezione e il suo spazio non cambiano da
    -- soli. Spostare un elemento cambierebbe CHI HA IL DIRITTO DI LEGGERLO (v. i privilegi di
    -- colonna più sotto), quindi è bloccato su due livelli e non solo qui.
    new.added_by      := old.added_by;
    new.space_id      := old.space_id;
    new.collection_id := old.collection_id;
    new.created_at    := old.created_at;

    return new;
end;
$$;

drop trigger if exists collection_items_before_update on public.collection_items;
create trigger collection_items_before_update
    before update on public.collection_items
    for each row execute function public.handle_collection_item_update();

-- =====================================================================================
-- RLS
-- =====================================================================================

alter table public.collections      enable row level security;
alter table public.collection_items enable row level security;

-- ---------- collections ----------
drop policy if exists collections_select on public.collections;
create policy collections_select on public.collections
    for select using (public.is_space_member(space_id));

drop policy if exists collections_insert on public.collections;
create policy collections_insert on public.collections
    for insert with check (public.is_space_member(space_id) and owner_id = auth.uid());

-- Modificare e cancellare: l'autore, oppure il proprietario dello spazio — che deve poter fare
-- pulizia a casa propria. `with check` esplicito e identico a `using`: omettendolo Postgres
-- userebbe comunque `using` per entrambi, ma scriverlo rende la regola leggibile senza dover
-- conoscere quel dettaglio. Non è questa la riga a impedire che una collezione si sposti fuori
-- dal proprio spazio: a impedirlo è l'assenza del privilegio di colonna su space_id (v. sotto).
drop policy if exists collections_update on public.collections;
create policy collections_update on public.collections
    for update using      (owner_id = auth.uid() or public.is_space_owner(space_id))
              with check  (owner_id = auth.uid() or public.is_space_owner(space_id));

drop policy if exists collections_delete on public.collections;
create policy collections_delete on public.collections
    for delete using (owner_id = auth.uid() or public.is_space_owner(space_id));

-- ---------- collection_items ----------
drop policy if exists collection_items_select on public.collection_items;
create policy collection_items_select on public.collection_items
    for select using (public.is_space_member(space_id));

drop policy if exists collection_items_insert on public.collection_items;
create policy collection_items_insert on public.collection_items
    for insert with check (public.is_space_member(space_id) and added_by = auth.uid());

-- Modificare e cancellare: chi ha aggiunto l'elemento, oppure il proprietario dello spazio — per
-- lo stesso motivo, e con lo stesso `with check` ridondante per leggibilità, di collections_update
-- qui sopra. E non è questa la riga a impedire che un elemento si sposti in un'altra collezione o
-- in un altro spazio: a impedirlo è l'assenza del privilegio di colonna su collection_id e
-- space_id (v. sotto).
drop policy if exists collection_items_update on public.collection_items;
create policy collection_items_update on public.collection_items
    for update using      (added_by = auth.uid() or public.is_space_owner(space_id))
              with check  (added_by = auth.uid() or public.is_space_owner(space_id));

drop policy if exists collection_items_delete on public.collection_items;
create policy collection_items_delete on public.collection_items
    for delete using (added_by = auth.uid() or public.is_space_owner(space_id));

-- =====================================================================================
-- Privilegi.
--
-- Stesso doppio livello di notes: la RLS FILTRA, non concede, e i privilegi di COLONNA fermano
-- ciò che una policy non può — in una policy non esiste OLD, quindi nessuna WITH CHECK può
-- impedire che version torni indietro o che i legami fra le righe cambino di nascosto.
-- =====================================================================================

revoke all on public.collections, public.collection_items from anon, authenticated;

-- version, created_at e updated_at non compaiono in nessun grant: li scrive il trigger, e il
-- client non deve poterli falsificare (v. handle_collection_update sopra).
grant select, delete on public.collections to authenticated;
grant insert (space_id, owner_id, name, icon, fields, rating_max) on public.collections to authenticated;
grant update (name, icon, fields, rating_max)                     on public.collections to authenticated;

-- Stesso ragionamento per collection_items: version, created_at e updated_at li scrive il
-- trigger. In più, collection_id e space_id NON sono aggiornabili: spostare un elemento in
-- un'altra collezione (o in un altro spazio) cambierebbe CHI HA IL DIRITTO DI LEGGERLO, quindi
-- sarebbe una fuga di dati e non un dispetto — non diverso dal motivo per cui notes non concede
-- l'UPDATE su space_id.
grant select, delete on public.collection_items to authenticated;
grant insert (collection_id, space_id, added_by, name, image_url, data) on public.collection_items to authenticated;
grant update (name, image_url, data)                                   on public.collection_items to authenticated;

grant all on public.collections, public.collection_items to service_role;
