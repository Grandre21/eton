-- =====================================================================================
-- Eton — schema iniziale: identità, spazi, membri.
-- Idempotente e rieseguibile.
-- =====================================================================================

-- pgcrypto fornisce gen_random_bytes, usata da generate_invite_code. Va dichiarata qui e non
-- data per scontata: PL/pgSQL non risolve le chiamate contenute nel corpo di una funzione al
-- momento della creazione, quindi senza questa riga generate_invite_code verrebbe creata senza
-- errori e fallirebbe soltanto alla prima invocazione reale — cioè al primo spazio condiviso
-- creato da un utente, in produzione. Meglio fallire qui, applicando la migrazione.
create schema if not exists extensions;
create extension if not exists pgcrypto with schema extensions;

-- ---------- profiles ----------
create table if not exists public.profiles (
    id           uuid primary key references auth.users (id) on delete cascade,
    display_name text,
    avatar_url   text,
    updated_at   timestamptz not null default now()
);

-- ---------- spaces ----------
create table if not exists public.spaces (
    id          uuid primary key default gen_random_uuid(),
    name        text not null check (length(btrim(name)) between 1 and 60),
    owner_id    uuid not null references auth.users (id) on delete cascade,
    invite_code text unique,                       -- null sullo spazio personale
    is_personal boolean not null default false,
    created_at  timestamptz not null default now()
);

-- Un solo spazio personale per utente, garantito dall'indice invece che dal codice.
create unique index if not exists spaces_one_personal_per_owner
    on public.spaces (owner_id) where is_personal;

-- ---------- space_members ----------
-- NON contiene il ruolo: l'unica fonte di verità sul proprietario è spaces.owner_id.
create table if not exists public.space_members (
    id        uuid primary key default gen_random_uuid(),
    space_id  uuid not null references public.spaces (id) on delete cascade,
    user_id   uuid not null references auth.users (id) on delete cascade,
    joined_at timestamptz not null default now(),
    unique (space_id, user_id)
);

create index if not exists space_members_user_idx on public.space_members (user_id);

-- =====================================================================================
-- Funzioni. Tutte SECURITY DEFINER con search_path fissato: senza, chi può creare tabelle
-- in uno schema che precede public potrebbe sostituire space_members con una tabella finta
-- e farsi rispondere "sì, è membro".
-- =====================================================================================

create or replace function public.is_space_member(p_space uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select exists (
        select 1 from space_members
        where space_id = p_space and user_id = auth.uid()
    );
$$;

-- Restituisce un booleano, non l'owner_id. Queste funzioni sono SECURITY DEFINER e devono
-- restare eseguibili da 'authenticated', perché le policy RLS le invocano a nome di chi
-- interroga: sono quindi raggiungibili anche direttamente come /rest/v1/rpc. Una funzione che
-- restituisse l'owner_id permetterebbe, a chi indovina l'UUID di uno spazio, di scoprire chi ne
-- è il proprietario; così di UUID bisogna indovinarne due.
create or replace function public.is_space_owner_of(p_space uuid, p_user uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select exists (
        select 1 from spaces
        where id = p_space and owner_id = p_user
    );
$$;

create or replace function public.is_space_owner(p_space uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select public.is_space_owner_of(p_space, auth.uid());
$$;

-- Serve nella policy di delete su space_members. Senza, quella policy dovrebbe interrogare
-- 'spaces' con una sottoquery grezza, che è a sua volta soggetta alla RLS di 'spaces': il
-- risultato cambierebbe a seconda di chi la valuta, ed è esattamente ciò che una regola di
-- sicurezza non deve fare.
create or replace function public.space_is_personal(p_space uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select coalesce((select is_personal from spaces where id = p_space), false);
$$;

create or replace function public.shares_space_with(p_user uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select exists (
        select 1
        from space_members mine
        join space_members theirs on theirs.space_id = mine.space_id
        where mine.user_id = auth.uid() and theirs.user_id = p_user
    );
$$;

-- Codice a 8 caratteri, alfabeto senza caratteri ambigui (niente 0/O, 1/I/L): 31^8 ≈ 40 bit.
-- Il codice invito è l'unica cosa che protegge l'ingresso in uno spazio condiviso, quindi i byte
-- vengono da gen_random_bytes (CSPRNG di pgcrypto) e non da random(), che è un generatore
-- statistico e non è pensato per resistere alla predizione.
-- 'extensions' entra nel search_path perché è lì che Supabase installa pgcrypto.
create or replace function public.generate_invite_code()
returns text language plpgsql security definer
set search_path = public, extensions as $$
declare
    alfabeto constant text := 'ABCDEFGHJKMNPQRSTUVWXYZ23456789';  -- 31 caratteri
    base     constant int  := 31;
    limite   constant int  := 248;   -- 31 * 8, il massimo multiplo di 31 sotto 256
    tentativo text;
    b int;
begin
    loop
        tentativo := '';
        while length(tentativo) < 8 loop
            b := get_byte(gen_random_bytes(1), 0);
            -- Campionamento con rifiuto: 256 non è multiplo di 31, quindi prendere il resto su
            -- tutto l'intervallo 0-255 farebbe uscire i primi 8 caratteri dell'alfabeto 9 volte
            -- su 256 e gli altri 8. Scartare i byte da 248 in su elimina il bias.
            continue when b >= limite;
            tentativo := tentativo || substr(alfabeto, 1 + (b % base), 1);
        end loop;
        exit when not exists (select 1 from spaces where invite_code = tentativo);
    end loop;
    return tentativo;
end;
$$;

-- Le due scritture (spazio + membership) devono riuscire insieme: se fallisse la seconda, chi ha
-- creato lo spazio non ne sarebbe membro e la policy di SELECT glielo renderebbe invisibile per
-- sempre. Un client non può garantire l'atomicità di due chiamate PostgREST; questa funzione sì.
create or replace function public.create_space(p_name text)
returns uuid language plpgsql security definer
set search_path = public as $$
declare v_space uuid;
begin
    if auth.uid() is null then
        raise exception 'non autenticato';
    end if;

    insert into spaces (name, owner_id, invite_code, is_personal)
    values (btrim(p_name), auth.uid(), generate_invite_code(), false)
    returning id into v_space;

    insert into space_members (space_id, user_id)
    values (v_space, auth.uid());

    return v_space;
end;
$$;

create or replace function public.join_space(p_code text)
returns uuid language plpgsql security definer
set search_path = public as $$
declare v_space uuid;
begin
    if auth.uid() is null then
        raise exception 'non autenticato';
    end if;

    select id into v_space
      from spaces
     where invite_code = upper(btrim(p_code)) and not is_personal;

    if v_space is null then return null; end if;

    insert into space_members (space_id, user_id)
    values (v_space, auth.uid())
    on conflict (space_id, user_id) do nothing;

    return v_space;
end;
$$;

-- Al primo accesso: profilo, spazio personale e membership, in una sola transazione.
create or replace function public.handle_new_user()
returns trigger language plpgsql security definer
set search_path = public as $$
declare v_space uuid;
begin
    insert into profiles (id, display_name, avatar_url)
    values (new.id,
            coalesce(new.raw_user_meta_data ->> 'full_name',
                     new.raw_user_meta_data ->> 'name',
                     new.email),
            new.raw_user_meta_data ->> 'avatar_url')
    on conflict (id) do nothing;

    insert into spaces (name, owner_id, is_personal)
    values ('Personale', new.id, true)
    on conflict do nothing
    returning id into v_space;

    if v_space is not null then
        insert into space_members (space_id, user_id)
        values (v_space, new.id)
        on conflict (space_id, user_id) do nothing;
    end if;

    return new;
end;
$$;

drop trigger if exists on_auth_user_created on auth.users;
create trigger on_auth_user_created
    after insert on auth.users
    for each row execute function public.handle_new_user();

-- =====================================================================================
-- RLS. Abilitata su tutte le tabelle; ogni policy è ricreabile.
-- =====================================================================================

alter table public.profiles      enable row level security;
alter table public.spaces        enable row level security;
alter table public.space_members enable row level security;

-- ---------- profiles ----------
drop policy if exists profiles_select on public.profiles;
create policy profiles_select on public.profiles
    for select using (id = auth.uid() or public.shares_space_with(id));

drop policy if exists profiles_insert on public.profiles;
create policy profiles_insert on public.profiles
    for insert with check (id = auth.uid());

drop policy if exists profiles_update on public.profiles;
create policy profiles_update on public.profiles
    for update using (id = auth.uid()) with check (id = auth.uid());

-- ---------- spaces ----------
-- Nessuna policy di INSERT: si crea uno spazio solo tramite create_space().
drop policy if exists spaces_select on public.spaces;
create policy spaces_select on public.spaces
    for select using (public.is_space_member(id));

drop policy if exists spaces_update on public.spaces;
-- owner_id compare in WITH CHECK per NOME, non dentro is_space_owner(): una funzione che rilegge
-- la tabella vedrebbe il valore vecchio, quindi non vincolerebbe nulla. Nominare la colonna la
-- fa valutare sulla riga proposta, e impedisce di regalare uno spazio a un altro utente.
create policy spaces_update on public.spaces
    for update using (public.is_space_owner(id))
    with check (public.is_space_owner(id) and owner_id = auth.uid());

drop policy if exists spaces_delete on public.spaces;
create policy spaces_delete on public.spaces
    for delete using (public.is_space_owner(id) and not is_personal);

-- ---------- space_members ----------
-- Nessuna policy di INSERT né di UPDATE: si entra solo tramite create_space()/join_space().
drop policy if exists space_members_select on public.space_members;
create policy space_members_select on public.space_members
    for select using (public.is_space_member(space_id));

drop policy if exists space_members_delete on public.space_members;
-- Tre condizioni: (1) dallo spazio personale non si esce; (2) la riga del PROPRIETARIO non si
-- rimuove mai, nemmeno da lui stesso — altrimenti resterebbe proprietario (e potrebbe ancora
-- cancellare lo spazio) senza però più vederlo, perché la SELECT su spaces richiede di esserne
-- membro; per andarsene, il proprietario cancella lo spazio; (3) ciascuno rimuove sé stesso, il
-- proprietario rimuove chiunque altro.
create policy space_members_delete on public.space_members
    for delete using (
        not public.space_is_personal(space_id)
        and not public.is_space_owner_of(space_id, user_id)
        and (user_id = auth.uid() or public.is_space_owner(space_id))
    );

-- =====================================================================================
-- Privilegi.
--
-- Le policy RLS FILTRANO, non concedono: sono un setaccio applicato a un permesso che deve già
-- esistere. Una tabella con RLS perfetta e nessun GRANT non restituisce zero righe, restituisce
-- "permission denied for table". Le versioni recenti di Supabase non concedono più
-- automaticamente SELECT/INSERT/UPDATE/DELETE sulle tabelle nuove (restano solo REFERENCES,
-- TRIGGER, TRUNCATE): i permessi vanno concessi qui, esplicitamente.
--
-- Si concede il MINIMO che serve alle operazioni previste dalla matrice, non "all": così le
-- policy diventano il secondo strato di difesa invece dell'unico. In particolare l'assenza di
-- INSERT su spaces e space_members rende impossibile la creazione diretta di uno spazio o di
-- una membership già a livello di privilegi, un gradino sotto la RLS.
--
-- Il secondo strumento sono i privilegi di COLONNA: una policy non può riferirsi al valore
-- precedente di una colonna — nelle policy non esiste OLD — e quindi non può impedire che
-- is_personal passi da true a false. Il privilegio di colonna lo impedisce.
-- =====================================================================================

grant usage on schema public to anon, authenticated;

-- anon non legge e non scrive NULLA: l'applicazione richiede l'accesso, e la chiave anon serve
-- solo a raggiungere il gateway e ad autenticarsi. Le revoche sono difensive: oggi non c'è nulla
-- da revocare, ma proteggono nel caso in cui i privilegi di default della piattaforma cambino.
revoke all on public.profiles, public.spaces, public.space_members from anon;

-- profiles: si legge, si crea il proprio, e se ne cambiano nome mostrato e avatar. Nessun DELETE
-- (v. §5.2 della spec: la riga è 1:1 con auth.users e sparisce solo in cascata con l'account).
revoke update on public.profiles from anon, authenticated;
grant select, insert on public.profiles to authenticated;
grant update (display_name, avatar_url) on public.profiles to authenticated;

-- spaces: si legge, si rinomina, si cancella (mai il personale). Nessun INSERT: si crea solo con
-- create_space(). Di uno spazio il client modifica soltanto il nome — id, owner_id, invite_code,
-- is_personal e created_at sono immutabili: senza questo vincolo il proprietario potrebbe
-- azzerare is_personal sul proprio spazio personale e poi cancellarlo, aggirando spaces_delete.
revoke update on public.spaces from anon, authenticated;
grant select, delete on public.spaces to authenticated;
grant update (name) on public.spaces to authenticated;

-- space_members: si legge e si esce. Nessun INSERT (si entra solo con create_space/join_space)
-- e nessun UPDATE (una membership non si modifica: o c'è o non c'è).
grant select, delete on public.space_members to authenticated;

-- service_role è la chiave amministrativa: non finisce mai in un client, bypassa la RLS per
-- costruzione ed è ciò che usa il pannello di Supabase per sfogliare le tabelle.
grant all on public.profiles, public.spaces, public.space_members to service_role;

-- generate_invite_code non è pensata per essere chiamata da fuori: la usa solo create_space, che
-- essendo SECURITY DEFINER ne eredita i privilegi. Va tolta dalla superficie /rest/v1/rpc.
revoke all on function public.generate_invite_code() from public, anon, authenticated;

-- create_space e join_space sono per i soli utenti autenticati.
revoke all on function public.create_space(text) from public, anon;
grant execute on function public.create_space(text) to authenticated;
revoke all on function public.join_space(text) from public, anon;
grant execute on function public.join_space(text) to authenticated;

-- NON si revocano is_space_member, is_space_owner, is_space_owner_of, shares_space_with e
-- space_is_personal: le policy RLS le invocano a nome del ruolo che sta interrogando, quindi
-- senza EXECUTE ogni query fallirebbe con "permission denied for function" invece di restituire
-- zero righe. Espongono solo booleani su UUID che il chiamante deve già conoscere.
