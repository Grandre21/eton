-- =====================================================================================
-- Eton — note: Markdown per spazio, con concorrenza ottimistica.
-- Idempotente e rieseguibile.
--
-- Dipende dalla migrazione 20260811000000_initial_schema.sql, da cui riusa senza modificarle
-- le funzioni is_space_member() e is_space_owner(): la regola d'accesso di una nota è già
-- interamente contenuta nell'appartenenza allo spazio, e non serve inventarne un'altra.
-- =====================================================================================

-- ---------- notes ----------
create table if not exists public.notes (
    id         uuid primary key default gen_random_uuid(),
    space_id   uuid not null references public.spaces (id)  on delete cascade,

    -- default auth.uid() come rete di sicurezza: la policy di INSERT pretende comunque
    -- owner_id = auth.uid(), quindi una nota con l'autore sbagliato viene respinta invece che
    -- salvata storta. Il default fa sì che il caso "il client si è dimenticato di mandarlo"
    -- diventi impossibile anziché diventare un errore.
    owner_id   uuid not null default auth.uid()
                    references auth.users (id) on delete cascade,

    title      text not null default ''     check (length(title) <= 200),
    body       text not null default ''     check (length(body)  <= 100000),

    -- Concorrenza ottimistica: il client rimanda la versione che aveva letto come FILTRO
    -- (?version=eq.N), non come valore da scrivere. Zero righe modificate significa che
    -- qualcun altro ha salvato nel frattempo.
    version    integer not null default 1,

    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

-- Composito e non due indici separati: l'unica lettura che esiste è "le note di questo spazio,
-- dalla più recente", e questo indice serve sia il filtro sia l'ordinamento. Un indice sul solo
-- space_id sarebbe un prefisso di questo, quindi ridondante.
create index if not exists notes_space_updated_idx
    on public.notes (space_id, updated_at desc);

-- ---------- versione e data di modifica ----------
-- Le calcola il database, non il client: se le scrivesse il client, chi vuole sovrascrivere il
-- lavoro altrui gli basterebbe rimandare la versione che preferisce, e la concorrenza ottimistica
-- diventerebbe una formalità. Per lo stesso motivo i privilegi di colonna più sotto NON
-- concedono l'UPDATE su version e updated_at.
--
-- Non è SECURITY DEFINER e non deve esserlo: il trigger modifica NEW in memoria, e i privilegi
-- di colonna vengono verificati sulle colonne NOMINATE nell'istruzione UPDATE, non su quelle che
-- un trigger tocca dopo. Un BEFORE UPDATE può quindi scrivere version anche se il chiamante non
-- avrebbe il diritto di scriverla direttamente. È il perno di tutto il meccanismo.
create or replace function public.handle_note_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Rimesse a forza al valore precedente. Oggi è ridondante — i privilegi di colonna non
    -- concedono l'UPDATE su nessuna delle tre — ma questa è l'ultima riga di difesa che resta
    -- in piedi se un domani una funzione SECURITY DEFINER scrivesse su notes scavalcando i
    -- privilegi: una nota non cambia autore, non cambia spazio e non ringiovanisce.
    new.owner_id   := old.owner_id;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;

    return new;
end;
$$;

drop trigger if exists notes_before_update on public.notes;
create trigger notes_before_update
    before update on public.notes
    for each row execute function public.handle_note_update();

-- =====================================================================================
-- RLS
-- =====================================================================================

alter table public.notes enable row level security;

-- Leggere: basta essere membri dello spazio. È questa riga a impedire che un estraneo legga le
-- note di uno spazio a cui non appartiene, anche conoscendone l'id.
drop policy if exists notes_select on public.notes;
create policy notes_select on public.notes
    for select using (public.is_space_member(space_id));

-- Scrivere: nello spazio di cui si è membri, e solo a proprio nome. Senza la seconda condizione
-- un membro potrebbe creare note firmate da qualcun altro.
drop policy if exists notes_insert on public.notes;
create policy notes_insert on public.notes
    for insert with check (public.is_space_member(space_id) and owner_id = auth.uid());

-- Modificare e cancellare: l'autore, oppure il proprietario dello spazio — che deve poter fare
-- pulizia a casa propria. `with check` esplicito e identico a `using`: omettendolo Postgres
-- userebbe comunque `using` per entrambi, ma scriverlo rende la regola leggibile senza dover
-- conoscere quel dettaglio.
drop policy if exists notes_update on public.notes;
create policy notes_update on public.notes
    for update using      (owner_id = auth.uid() or public.is_space_owner(space_id))
              with check  (owner_id = auth.uid() or public.is_space_owner(space_id));

drop policy if exists notes_delete on public.notes;
create policy notes_delete on public.notes
    for delete using (owner_id = auth.uid() or public.is_space_owner(space_id));

-- =====================================================================================
-- Privilegi.
--
-- La RLS FILTRA, non concede: senza i GRANT qui sotto ogni query fallirebbe con
-- "permission denied for table notes" prima ancora che una policy venga consultata.
--
-- E i privilegi di COLONNA fanno ciò che una policy non può: in una policy non esiste OLD,
-- quindi nessuna `with check` può impedire che version torni indietro o che updated_at venga
-- falsificato. Il privilegio di colonna lo impedisce un gradino più sotto.
-- =====================================================================================

revoke all on public.notes from anon, authenticated;

-- Si legge tutto (serve anche a filtrare per version) e si cancella per riga: a decidere chi
-- vede e chi cancella sono le policy.
grant select, delete on public.notes to authenticated;

-- In inserimento il client sceglie spazio, autore e contenuto. NON version, created_at e
-- updated_at: quelle nascono dai default.
grant insert (space_id, owner_id, title, body) on public.notes to authenticated;

-- In aggiornamento il client tocca SOLTANTO il contenuto. Non l'autore, non lo spazio, non la
-- versione: version la incrementa il trigger, ed è precisamente ciò che rende la concorrenza
-- ottimistica una difesa e non un suggerimento.
grant update (title, body) on public.notes to authenticated;

grant all on public.notes to service_role;
