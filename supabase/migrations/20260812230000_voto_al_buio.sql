-- =====================================================================================
-- Eton — voto al buio: su una collezione dichiarata "cieca" (blind), un elemento nasconde le
-- recensioni degli altri finché l'utente non ha messo la propria. Il numero di chi ha votato
-- resta sempre visibile, il contenuto no.
-- Idempotente e rieseguibile.
--
-- Dipende da 20260812120000_collections.sql, a cui aggiunge la colonna blind ed estende il grant
-- di UPDATE colonna per colonna, e da 20260812200000_recensioni.sql, di cui sostituisce SOLO la
-- policy di SELECT su reviews.
-- =====================================================================================

-- ---------- collections.blind ----------
-- Default false obbligatorio: le collezioni già esistenti in produzione devono continuare a
-- comportarsi esattamente come prima, senza bisogno di alcuna migrazione dei dati.
alter table public.collections
    add column if not exists blind boolean not null default false;

-- =====================================================================================
-- Funzioni. SECURITY DEFINER con search_path fissato per lo stesso motivo delle funzioni di
-- 20260811000000_initial_schema.sql: senza, chi può creare tabelle in uno schema che precede
-- public potrebbe sostituire collection_items, collections o reviews con una tabella finta e
-- farsi rispondere quello che vuole.
-- =====================================================================================

-- Vera se l'elemento appartiene a una collezione cieca. SECURITY DEFINER anche per restare nello
-- stesso contesto di esecuzione di has_reviewed qui sotto: le due funzioni vengono valutate
-- fianco a fianco nella stessa policy, e non deve essere il set di privilegi di chi legge a
-- decidere se una delle due vede meno righe dell'altra.
create or replace function public.item_is_blind(p_item uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    -- coalesce e non un risultato nullable: se l'elemento non esiste la sottoquery restituisce
    -- null, che dentro una policy si comporta come "falso" ma per una ragione diversa da "la
    -- collezione non è cieca" — un'ambiguità che poi nessuno ricorda.
    select coalesce(
        (select c.blind
           from collection_items ci
           join collections c on c.id = ci.collection_id
          where ci.id = p_item),
        false
    );
$$;

-- Vera se auth.uid() ha già recensito l'elemento. SECURITY DEFINER non facoltativo, a differenza
-- della funzione sopra: questa interroga reviews e viene usata DENTRO la policy di SELECT di
-- reviews stessa. Una funzione a diritti dell'invocatore rientrerebbe nella RLS di reviews mentre
-- la RLS di reviews sta ancora venendo valutata, e Postgres la respinge con "infinite recursion
-- detected in policy for relation "reviews"" — lo stesso problema che is_space_member risolve per
-- space_members (v. 20260811000000_initial_schema.sql).
create or replace function public.has_reviewed(p_item uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select exists (
        select 1 from reviews
        where item_id = p_item and user_id = auth.uid()
    );
$$;

-- Il conteggio di chi ha votato un elemento, indipendentemente da quante recensioni la policy
-- sotto lascia effettivamente leggere: senza, un elemento bloccato mostrerebbe "nessun voto"
-- mentre magari ne ha tre. SECURITY DEFINER e quindi esposta su /rest/v1/rpc: chiunque conosca un
-- UUID di spazio può chiamarla, saltando la RLS di reviews per costruzione. Per questo verifica
-- is_space_member(p_space) AL PROPRIO INTERNO — non è un dettaglio implementativo, è il perimetro
-- di sicurezza della funzione — e restituisce zero righe se l'utente non è membro dello spazio.
create or replace function public.review_counts(p_space uuid)
returns table (item_id uuid, voters integer)
language sql security definer stable
set search_path = public as $$
    -- Conta le RIGHE di reviews, non quelle con rating is not null: una recensione di solo
    -- commento è comunque una posizione presa, e sbloccare le altre per chi l'ha scritta ha lo
    -- stesso senso che se fosse un voto numerico.
    --
    -- Restituisce SOLO item_id e un conteggio: mai rating, mai comment, mai user_id, perché sono
    -- esattamente i dati che questa funzionalità esiste per nascondere.
    select r.item_id, count(*)::integer as voters
      from reviews r
     where r.space_id = p_space
       and public.is_space_member(p_space)
     group by r.item_id;
$$;

-- =====================================================================================
-- RLS
-- =====================================================================================

drop policy if exists reviews_select on public.reviews;
create policy reviews_select on public.reviews
    for select using (
        public.is_space_member(space_id)
        and (
            -- L'ordine dei tre disgiunti non è casuale. user_id = auth.uid() per primo perché è
            -- il caso più frequente — ogni utente controlla la propria recensione a ogni
            -- caricamento — e il più economico da valutare, e perché senza di esso chi ha votato
            -- non vedrebbe la propria recensione nel momento esatto in cui la salva.
            user_id = auth.uid()                 -- la tua la vedi sempre
            or not public.item_is_blind(item_id) -- collezione normale: tutto visibile
            or public.has_reviewed(item_id)      -- hai votato: si apre
        )
    );

-- =====================================================================================
-- Privilegi.
-- =====================================================================================

-- Senza questa riga l'interruttore "alla cieca" nell'interfaccia fallirebbe con "permission
-- denied for table collections": su questo progetto l'UPDATE si concede colonna per colonna
-- (v. il commento sui privilegi in fondo a 20260812120000_collections.sql), e blind non
-- comparirebbe nell'elenco delle colonne scrivibili finché non viene riconcesso qui.
grant update (name, icon, fields, rating_max, blind) on public.collections to authenticated;

-- item_is_blind e has_reviewed restano eseguibili da authenticated, e non è una svista: le policy
-- RLS le invocano a nome del ruolo che sta interrogando, quindi senza EXECUTE ogni lettura di
-- reviews fallirebbe con "permission denied for function" invece di restituire zero righe — lo
-- stesso motivo per cui restano eseguibili le funzioni di 20260811000000_initial_schema.sql.
--
-- Si revoca però l'esecuzione ad anon e a public, che è il privilegio di default su ogni funzione
-- nuova. Serve solo per item_is_blind: ogni funzione security definer è anche una RPC pubblica su
-- /rest/v1/rpc, e questa risponde a partire da un solo UUID di elemento, senza legare il risultato
-- a chi chiama. Senza la revoca, chiunque abbia la chiave anon — che sta nel bundle ed è pubblica —
-- potrebbe chiedere senza alcun accesso se una collezione è cieca. has_reviewed non ne avrebbe
-- bisogno, perché il suo risultato dipende da auth.uid() e per anon è sempre falso, ma si revoca
-- anche lei: anon non legge reviews in nessun caso, quindi non c'è motivo perché possa chiamare
-- funzioni che esistono solo per le policy di quella tabella.
--
-- Resta scoperto, e lo si accetta: un utente autenticato qualunque, in possesso dell'UUID di un
-- elemento di uno spazio a cui non appartiene, può sapere se quella collezione è cieca. È un flag
-- di configurazione, non un dato — nessun voto, nessun commento, nessun nome — e chiuderlo
-- costerebbe una verifica di appartenenza dentro una funzione che la policy valuta RIGA PER RIGA,
-- cioè un secondo accesso a space_members per ogni recensione letta. Il prezzo non vale il segreto.
revoke all on function public.item_is_blind(uuid) from public, anon;
grant execute on function public.item_is_blind(uuid) to authenticated;

revoke all on function public.has_reviewed(uuid) from public, anon;
grant execute on function public.has_reviewed(uuid) to authenticated;

revoke all on function public.review_counts(uuid) from public, anon;
grant execute on function public.review_counts(uuid) to authenticated;
