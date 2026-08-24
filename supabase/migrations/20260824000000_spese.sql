-- =====================================================================================
-- Eton — spese: registro per spazio, con concorrenza ottimistica.
-- Idempotente e rieseguibile.
--
-- Dipende dalla migrazione 20260811000000_initial_schema.sql, da cui riusa senza modificarle
-- le funzioni is_space_member() e is_space_owner(): la regola d'accesso di una spesa è già
-- interamente contenuta nell'appartenenza allo spazio, come per notes, e non serve inventarne
-- un'altra.
-- =====================================================================================

-- ---------- expenses ----------
create table if not exists public.expenses (
    id          uuid primary key default gen_random_uuid(),
    space_id    uuid not null references public.spaces (id) on delete cascade,

    -- Chi ha anticipato i soldi. Oggi coincide sempre con chi crea la riga, perché la policy di
    -- INSERT pretende paid_by = auth.uid(); esiste separato per il giorno in cui si dividerà una
    -- spesa fra i membri, dove i due ruoli divergono.
    paid_by     uuid not null default auth.uid()
                    references auth.users (id) on delete cascade,

    amount      numeric(12,2) not null check (amount > 0),

    -- btrim e non la lunghezza grezza, come per collections.name: una descrizione di soli spazi
    -- supererebbe il controllo e comparirebbe nel registro come una riga vuota.
    description text not null check (length(btrim(description)) between 1 and 200),

    -- Testo libero nel database, elenco chiuso nell'interfaccia. Un check contro una lista
    -- renderebbe l'aggiunta di una categoria una migration da incollare a mano nel SQL Editor di
    -- produzione, mentre così è una riga di C#. I dati restano puliti lo stesso, perché a
    -- scrivere la categoria non è mai la tastiera.
    category    text not null check (length(btrim(category)) between 1 and 40),

    -- 'date' e non 'timestamptz': una spesa appartiene a un giorno, non a un istante, e un fuso
    -- orario qui produrrebbe spese che cambiano mese a seconda di dove ti trovi.
    spent_on    date not null default current_date,

    -- Concorrenza ottimistica: il client rimanda la versione che aveva letto come FILTRO
    -- (?version=eq.N), non come valore da scrivere. Zero righe modificate significa che
    -- qualcun altro ha salvato nel frattempo.
    version     integer not null default 1,

    created_at  timestamptz not null default now(),
    updated_at  timestamptz not null default now()
);

-- Composito e non due indici separati: l'unica lettura che esiste è "le spese di questo spazio,
-- dalla più recente", e questo indice serve sia il filtro sia l'ordinamento — stesso ragionamento
-- di notes_space_updated_idx, ma su spent_on invece che su updated_at, perché una spesa si
-- consulta per QUANDO È STATA FATTA, non per quando è stata corretta.
create index if not exists expenses_space_date_idx
    on public.expenses (space_id, spent_on desc);

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
create or replace function public.handle_expense_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Rimesse a forza al valore precedente. Oggi è ridondante — i privilegi di colonna non
    -- concedono l'UPDATE su nessuna delle tre — ma questa è l'ultima riga di difesa che resta
    -- in piedi se un domani una funzione SECURITY DEFINER scrivesse su expenses scavalcando i
    -- privilegi: una spesa non cambia chi l'ha pagata, non cambia spazio e non ringiovanisce.
    new.paid_by    := old.paid_by;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;

    return new;
end;
$$;

drop trigger if exists expenses_before_update on public.expenses;
create trigger expenses_before_update
    before update on public.expenses
    for each row execute function public.handle_expense_update();

-- =====================================================================================
-- RLS
-- =====================================================================================

alter table public.expenses enable row level security;

-- Leggere: basta essere membri dello spazio. È questa riga a impedire che un estraneo legga le
-- spese di uno spazio a cui non appartiene, anche conoscendone l'id.
drop policy if exists expenses_select on public.expenses;
create policy expenses_select on public.expenses
    for select using (public.is_space_member(space_id));

-- Scrivere: nello spazio di cui si è membri, e solo per ciò che si è pagato di persona. Senza la
-- seconda condizione un membro potrebbe attribuire pagamenti a qualcun altro — ed è esattamente
-- la condizione da rivedere il giorno in cui si dividerà una spesa fra i membri: meglio una
-- decisione deliberata allora che un permesso lasciato largo adesso.
drop policy if exists expenses_insert on public.expenses;
create policy expenses_insert on public.expenses
    for insert with check (public.is_space_member(space_id) and paid_by = auth.uid());

-- Modificare e cancellare: chi ha pagato, oppure il proprietario dello spazio — che deve poter
-- fare pulizia a casa propria. with check esplicito e identico a using: omettendolo Postgres
-- userebbe comunque using per entrambi, ma scriverlo rende la regola leggibile senza dover
-- conoscere quel dettaglio.
drop policy if exists expenses_update on public.expenses;
create policy expenses_update on public.expenses
    for update using      (paid_by = auth.uid() or public.is_space_owner(space_id))
              with check  (paid_by = auth.uid() or public.is_space_owner(space_id));

drop policy if exists expenses_delete on public.expenses;
create policy expenses_delete on public.expenses
    for delete using (paid_by = auth.uid() or public.is_space_owner(space_id));

-- =====================================================================================
-- Privilegi.
--
-- La RLS FILTRA, non concede: senza i GRANT qui sotto ogni query fallirebbe con
-- "permission denied for table expenses" prima ancora che una policy venga consultata.
--
-- E i privilegi di COLONNA fanno ciò che una policy non può: in una policy non esiste OLD,
-- quindi nessuna with check può impedire che version torni indietro o che updated_at venga
-- falsificato. Il privilegio di colonna lo impedisce un gradino più sotto.
-- =====================================================================================

revoke all on public.expenses from anon, authenticated;

-- Si legge tutto (serve anche a filtrare per version) e si cancella per riga: a decidere chi
-- vede e chi cancella sono le policy.
grant select, delete on public.expenses to authenticated;

-- In inserimento il client sceglie anche l'id — a differenza di notes, dove nasce dal default del
-- database. Una spesa si segna al bar, col telefono, con la rete che va e viene: se
-- l'inserimento fallisce a metà non si sa se è passato, e si riprova. Con l'id generato dal
-- client il secondo tentativo porta lo stesso uuid, e se il primo era passato la chiave primaria
-- rifiuta il duplicato invece di farne nascere uno indistinguibile. È anche la fondazione su cui
-- una coda offline si costruirà senza migration di rottura.
grant insert (id, space_id, paid_by, amount, description, category, spent_on)
    on public.expenses to authenticated;

-- In aggiornamento il client tocca SOLTANTO il contenuto. Non paid_by, non space_id: cambiarli
-- sposterebbe la spesa sotto un'altra regola di visibilità, cioè sarebbe una fuga di dati e non
-- un dispetto — lo stesso motivo per cui collection_items non concede l'UPDATE su
-- collection_id. Non version: la incrementa il trigger, ed è precisamente ciò che rende la
-- concorrenza ottimistica una difesa e non un suggerimento.
grant update (amount, description, category, spent_on)
    on public.expenses to authenticated;

grant all on public.expenses to service_role;
