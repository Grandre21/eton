# Eton — design

**Data:** 2026-08-11
**Stato:** approvato, da tradurre in piano di implementazione

---

## 1. Cos'è

Un'applicazione **mobile first** per tenere, da soli o con altri, tre tipi di contenuto:

- **note personali**;
- **note condivise** con un gruppo di persone;
- **collezioni recensibili**: elenchi di oggetti con campi definiti dall'utente, su cui
  ciascun membro del gruppo lascia il **proprio** voto e commento.

Il caso d'uso che ha originato il progetto — un gruppo di amici che cataloga e recensisce i
liquidi da svapo acquistati — è una collezione fra le altre, non una funzionalità dedicata.
La generalità sta nel fatto che i campi di una collezione li definisce l'utente: "Birre",
"Ristoranti", "Film" si creano senza scrivere codice.

Accessibile anche da desktop, ma progettata sul telefono.

---

## 2. Decisioni prese, con la ragione

| Decisione | Scelta | Perché |
|---|---|---|
| Piattaforma | **Solo Blazor WebAssembly PWA**. Niente MAUI. | MAUI non produce un sito: non esiste un percorso "MAUI → PWA". Poiché la PWA serve comunque per gli utenti iPhone, MAUI sarebbe una *seconda* app accanto, non un'alternativa. Vedi §2.1. |
| Play Store | **TWA generata da Bubblewrap**, fuori dal piano di sviluppo | Il pacchetto TWA carica il sito live: è un artefatto separato che non tocca il codice e si genera in qualsiasi momento. |
| Backend | **Supabase free**, client diretto, nessun server applicativo | Stesso modello del precedente `DndCompanion`, già collaudato. |
| Sicurezza | **RLS di PostgreSQL** + funzioni `SECURITY DEFINER` | La chiave anon è pubblica per costruzione: senza RLS il database è aperto. |
| Autenticazione | **Solo Google OAuth** | Riuso diretto del D&D, e il servizio email integrato di Supabase è limitato a poche email l'ora: email+password o magic link richiederebbero un SMTP esterno. |
| Condivisione | **Spazi** con codice invito. Lo spazio personale è uno spazio con un solo membro. | Una sola regola di accesso (`sei membro dello spazio?`) invece di due casi (`gruppo_id NULL` vs valorizzato). |
| Note | **Markdown** con checklist, reso con Markdig e `.DisableHtml()` | Potenza alta a costo basso. `DisableHtml` chiude lo stored XSS: in uno spazio condiviso il testo lo scrive qualcun altro. |
| Campi delle collezioni | **Definiti dall'utente**, salvati in `jsonb`, con template precompilati | Colonne vere richiederebbero una migration per categoria; EAV renderebbe dolorosa ogni lettura. |
| Voti | **Tabella `reviews` a parte**, non dentro il `jsonb` | Serve per `AVG` in una query, per il vincolo "un voto a testa" e soprattutto perché la RLS ci si aggancia: *il tuo voto lo modifichi solo tu*. |
| Sincronizzazione | **Online**, con **concorrenza ottimistica** | Realtime e offline-first sono innesti successivi; la protezione anti-sovrascrittura invece va decisa ora perché aggiunge una colonna. |
| Immagini | **Solo URL** in questa versione | Il campo `image_url` resta identico quando in futuro ci finirà dentro un file caricato su Supabase Storage. |
| Nome | **Eton** — progetto, namespace, repo, titolo PWA | |

### 2.1 Perché MAUI è escluso

Gli stessi file `.razor` possono essere ospitati in due modi che differiscono per **dove gira il
runtime .NET**:

- **Blazor WebAssembly** — il runtime è compilato in WebAssembly e gira *dentro il browser*.
  L'output sono file statici serviti via HTTPS: è un sito, quindi può essere una PWA e può
  essere impacchettato in una TWA.
- **MAUI Blazor Hybrid** — il runtime .NET gira *nativo, in-process*, e i componenti sono
  renderizzati in una `BlazorWebView`. L'output è un `.apk`/`.ipa`/`.exe`.

L'asimmetria è decisiva: un sito si può **avvolgere** in un'app da store; un'app nativa non si
può **scartare** per farne un sito. Non esiste un target di build che lo faccia, e una PWA
richiede tre cose che un APK non ha per costruzione — un origin HTTPS, un manifest servito da
quell'origin e un service worker registrato dal browser per quell'origin.

Inoltre, in Blazor Hybrid l'autenticazione del precedente `DndCompanion` non è riusabile:

- il JS interop è **solo asincrono** — `IJSInProcessRuntime` non esiste, e il cast in
  `SupabaseService.cs:43` lancerebbe `InvalidCastException`;
- `BrowserSessionHandler` è irrecuperabile: servirebbero `Preferences`/`SecureStorage`;
- Google **blocca** l'OAuth nelle WebView embedded (`disallowed_useragent`), e la
  `BlazorWebView` su Android gira su un origin virtuale su cui non è possibile redirigere.
  Servirebbero `WebAuthenticator` + custom scheme + PKCE.

Aggiungere MAUI significherebbe quindi mantenere **due** implementazioni proprio dello strato
più fragile, in cambio di zero funzionalità richieste. Per completezza: **Uno Platform** compila
davvero un solo codebase sia in WASM sia in nativo, ma è un framework diverso e farebbe buttare
via il precedente da cui si parte. Scartato.

---

## 3. Architettura

```
   Browser / TWA su Android              Supabase (piano free)
  ┌──────────────────────────┐         ┌───────────────────────────┐
  │  Eton (Blazor WASM)      │  HTTPS  │  Gotrue    /auth/v1       │
  │                          │────────▶│  ↳ Google OAuth, sessioni │
  │  Services/               │         ├───────────────────────────┤
  │   ↳ SupabaseService      │  HTTPS  │  PostgREST /rest/v1       │
  │   ↳ Repositories         │────────▶│                           │
  │                          │         ├───────────────────────────┤
  │  AccessControl → solo UX │         │  PostgreSQL + RLS         │
  └──────────────────────────┘         │  ↳ qui sta la sicurezza   │
         GitHub Pages                  └───────────────────────────┘
```

**Stack:** Blazor WebAssembly standalone su .NET 10, PWA con service worker, hosting GitHub
Pages con deploy da GitHub Actions al push su `main`.

**Pacchetti NuGet** (versioni verificate su nuget.org l'11-08-2026):

| Pacchetto | Versione | Nota |
|---|---|---|
| `Supabase.Gotrue` | 6.3.0 | Il D&D usa `gotrue-csharp` 4.2.7 — **ID rinominato, due major di distanza** |
| `Supabase.Postgrest` | 4.4.0 | Il D&D usa `postgrest-csharp` 3.5.1 — ID rinominato, un major |
| `Markdig` | 1.3.2 | Configurato con `.DisableHtml()` |
| `Microsoft.AspNetCore.Components.WebAssembly` | 10.x | |

`Supabase.Gotrue` e `Supabase.Postgrest` vanno dichiarati `TrimmerRootAssembly` in Release fin
dal primo commit: è la lezione già pagata in `DndCompanion`, dove il difetto — un costruttore
usato via reflection e rimosso dal trimmer — si manifesta **solo sul sito pubblicato**. Per
Markdig la necessità va accertata al primo collaudo in Release, non presunta.

**Codice portato da `DndCompanion`** — da riadattare a Gotrue 6, non da copiare alla cieca:
`SupabaseService`, `SupabaseClient`, `BrowserSessionHandler`, `AuthStateService`,
`CurrentUserService`, `AccessControl`, `PwaUpdateService`, `ToastService`, `ConfirmService`,
`Login.razor`, e i componenti condivisi `BottomNav`, `ToastHost`, `ConfirmDialog`,
`LoadingSpinner`, `UpdateBanner`, `DbErrorBanner`, `AuthRedirect`.

`CampaignStateService` diventa `SpaceStateService` (spazio attivo persistito).

---

## 4. Modello dati

```
   auth.users ──1:1── profiles
        │
        │ owner_id
        ▼
     spaces ◀────── space_members ──▶ auth.users
        │        (chi è dentro; il proprietario
        │  space_id   è spaces.owner_id)
        ├──────────────┬────────────────┐
        ▼              ▼                │
      notes       collections           │
                       │                │
                       │ collection_id  │ space_id
                       ▼                │  (denormalizzato)
                collection_items ◀──────┤
                       │                │
                       │ item_id        │
                       ▼                │
                   reviews ◀────────────┘
              unique(item_id, user_id)
```

### 4.1 Perché `space_id` è ripetuto

Senza denormalizzazione, la policy RLS su `reviews` dovrebbe risalire
`reviews → collection_items → collections → space_id`: due join **per ogni riga controllata**, e
una policy illeggibile è una policy in cui si nasconde un buco. Con `space_id` a bordo, ogni
policy dell'app è la stessa riga: `is_space_member(space_id)`.

La coerenza la garantisce il database, non il codice: si aggiunge un `unique (id, space_id)`
(ridondante rispetto alla primary key, ma legale) e la foreign key dei figli diventa
**composita**. Inserire un elemento con lo `space_id` sbagliato diventa impossibile.

### 4.2 DDL

```sql
-- ---------- profiles ----------
create table public.profiles (
    id           uuid primary key references auth.users (id) on delete cascade,
    display_name text,
    avatar_url   text,
    updated_at   timestamptz not null default now()
);

-- ---------- spaces ----------
create table public.spaces (
    id          uuid primary key default gen_random_uuid(),
    name        text not null check (length(btrim(name)) between 1 and 60),
    owner_id    uuid not null references auth.users (id) on delete cascade,
    invite_code text unique,                      -- null sullo spazio personale
    is_personal boolean not null default false,
    created_at  timestamptz not null default now()
);

-- un solo spazio personale per utente, garantito dall'indice
create unique index spaces_one_personal_per_owner
    on public.spaces (owner_id) where is_personal;

-- ---------- space_members ----------
-- Chi sta dentro uno spazio. NON contiene il ruolo: l'unica fonte di verità sul
-- proprietario è spaces.owner_id. Due colonne che devono restare d'accordo fra loro
-- sono un bug in attesa di succedere.
create table public.space_members (
    id        uuid primary key default gen_random_uuid(),
    space_id  uuid not null references public.spaces (id) on delete cascade,
    user_id   uuid not null references auth.users (id) on delete cascade,
    joined_at timestamptz not null default now(),
    unique (space_id, user_id)
);

-- ---------- notes ----------
create table public.notes (
    id         uuid primary key default gen_random_uuid(),
    space_id   uuid not null references public.spaces (id) on delete cascade,
    owner_id   uuid not null references auth.users (id) on delete cascade,
    title      text not null default '',
    body       text not null default '',          -- Markdown
    version    integer not null default 1,        -- concorrenza ottimistica
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

-- ---------- collections ----------
create table public.collections (
    id          uuid primary key default gen_random_uuid(),
    space_id    uuid not null references public.spaces (id) on delete cascade,
    owner_id    uuid not null references auth.users (id) on delete cascade,
    name        text not null,
    icon        text,                             -- emoji
    fields      jsonb not null default '[]'::jsonb,
    rating_max  smallint not null default 10 check (rating_max in (5, 10)),
    version     integer not null default 1,
    created_at  timestamptz not null default now(),
    updated_at  timestamptz not null default now(),
    unique (id, space_id)                         -- bersaglio della FK composita
);

-- ---------- collection_items ----------
create table public.collection_items (
    id            uuid primary key default gen_random_uuid(),
    collection_id uuid not null,
    space_id      uuid not null,
    added_by      uuid not null references auth.users (id) on delete cascade,
    name          text not null,
    image_url     text,
    data          jsonb not null default '{}'::jsonb,
    version       integer not null default 1,
    created_at    timestamptz not null default now(),
    updated_at    timestamptz not null default now(),
    foreign key (collection_id, space_id)
        references public.collections (id, space_id) on delete cascade,
    unique (id, space_id)
);

-- ---------- reviews ----------
create table public.reviews (
    id         uuid primary key default gen_random_uuid(),
    item_id    uuid not null,
    space_id   uuid not null,
    user_id    uuid not null references auth.users (id) on delete cascade,
    rating     numeric(3,1) check (rating > 0 and rating <= 10),
    comment    text,
    version    integer not null default 1,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    foreign key (item_id, space_id)
        references public.collection_items (id, space_id) on delete cascade,
    unique (item_id, user_id)                     -- un voto a testa
);

create index on public.notes            (space_id);
create index on public.collections      (space_id);
create index on public.collection_items (collection_id);
create index on public.reviews          (item_id);
```

### 4.3 Forma di `collections.fields` e `collection_items.data`

```jsonc
// collections.fields
[
  { "key": "marca",     "label": "Marca",          "type": "text",   "order": 1 },
  { "key": "nicotina",  "label": "Nicotina (mg)",  "type": "number", "order": 2 },
  { "key": "pgvg",      "label": "PG/VG",          "type": "select",
    "options": ["50/50", "60/40", "70/30"],                          "order": 3 },
  { "key": "prezzo",    "label": "Prezzo (€)",     "type": "number", "order": 4 }
]

// collection_items.data
{ "marca": "Vaporart", "nicotina": 6, "pgvg": "60/40", "prezzo": 12.90 }
```

Tipi ammessi: `text`, `number`, `select`, `date`, `bool`, `url`.
`name`, `image_url` e il voto **non** sono campi: sono presenti su ogni elemento e vivono in
colonne vere, perché ci si deve cercare e ordinare sopra.

Un template è semplicemente un `fields` precompilato: nessun meccanismo separato.

### 4.4 Concorrenza ottimistica

Un trigger incrementa `version` e aggiorna `updated_at` a ogni `UPDATE` di `notes`,
`collections`, `collection_items`, `reviews`. Il client rimanda la `version` che aveva letto:

```
PATCH /rest/v1/notes?id=eq.<id>&version=eq.<letta>
```

Zero righe modificate ⇒ qualcuno ha scritto prima: si mostra il dialogo *Ricarica / Sovrascrivi*.

**Un intero, non un timestamp.** Un `timestamptz` fa un giro di andata e ritorno come stringa
JSON fra Postgres (microsecondi) e .NET (tick da 100 ns): un arrotondamento e il confronto
fallisce sempre, producendo un falso conflitto a ogni salvataggio. `updated_at` resta, ma solo
per mostrare "modificato 2 ore fa".

---

## 5. Sicurezza

### 5.1 Funzioni

Tutte con **`set search_path = public`**; le prime tre dichiarate **`stable`**.

| Funzione | Cosa fa |
|---|---|
| `is_space_member(uuid) → boolean` | Sei membro dello spazio? Spezza la ricorsione RLS su `space_members`. |
| `is_space_owner(uuid) → boolean` | `spaces.owner_id = auth.uid()`. Unica fonte di verità sul proprietario. |
| `shares_space_with(uuid) → boolean` | Condividi almeno uno spazio con quell'utente? Gate per la lettura dei `profiles`. |
| `create_space(text) → uuid` | Crea lo spazio **e** la membership del creatore, in una transazione. **Unico modo** di crearne uno. |
| `generate_invite_code() → text` | Codice a 8 caratteri, alfabeto senza ambigui, riprovando in caso di collisione. |
| `join_space(text) → uuid` | Valida il codice invito e crea la membership. **Unico modo** di entrare. |
| `handle_new_user() → trigger` | Su `insert` in `auth.users`: crea profilo, spazio personale e membership, in una transazione. |

**Perché `create_space` è una funzione e non una `INSERT` diretta.** Le due scritture — la riga
in `spaces` e quella in `space_members` — devono avvenire insieme: se la seconda fallisce, chi ha
creato lo spazio non ne è membro, e la policy di `SELECT` (`is_space_member(id)`) glielo rende
invisibile per sempre. Un client non può garantire l'atomicità di due chiamate PostgREST; una
funzione sì. Stessa ragione per cui `join_space` esiste.

```sql
create or replace function public.is_space_member(p_space uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select exists (
        select 1 from space_members
        where space_id = p_space and user_id = auth.uid()
    );
$$;

create or replace function public.is_space_owner(p_space uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select exists (
        select 1 from spaces
        where id = p_space and owner_id = auth.uid()
    );
$$;

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
```

**`set search_path` non è pignoleria.** Una `SECURITY DEFINER` gira coi privilegi del
proprietario del database: senza `search_path` fissato, chi può creare tabelle in uno schema che
precede `public` può piazzarci una `space_members` finta, e la funzione risponderebbe "sì, è
membro" a chiunque.

**`stable`** evita che il planner tratti la funzione come `volatile` e la chiami una volta per
riga esaminata.

Il **codice invito** riusa l'algoritmo del D&D (`CampaignRepository.cs:151`): 8 caratteri da un
alfabeto di 31 senza caratteri ambigui (niente `0/O`, `1/I/L`), ≈ 40 bit di entropia —
enumerazione impraticabile. Rispetto al D&D si sposta **da C# a SQL**, dentro `create_space`:
generare il codice nel client significa fare "genera → controlla se esiste → inserisci" con una
finestra di corsa fra il controllo e l'inserimento; dentro la funzione, il vincolo `unique` e il
ritentativo stanno nella stessa transazione. L'algoritmo resta comunque duplicato in C# come
funzione pura, per poterlo testare in `Eton.Tests` senza un database.

### 5.2 Matrice RLS

RLS abilitata su **tutte** le tabelle. Nessun accesso al ruolo `anon` oltre l'autenticazione.

| Tabella | SELECT | INSERT | UPDATE | DELETE |
|---|---|---|---|---|
| `spaces` | `is_space_member(id)` | **nessuna policy** — solo via `create_space` | `is_space_owner(id)` | `is_space_owner(id)` ∧ `not is_personal` |
| `space_members` | `is_space_member(space_id)` | **nessuna policy** — solo via `create_space` / `join_space` | nessuna | `not is_personal(space)` ∧ ( `user_id = auth.uid()` — uscire — ∨ `is_space_owner(space_id)` — espellere ) |
| `profiles` | `id = auth.uid()` ∨ `shares_space_with(id)` | `id = auth.uid()` | `id = auth.uid()` | **nessuna policy** — vedi sotto |
| `notes` | `is_space_member(space_id)` | `is_space_member(space_id)` ∧ `owner_id = auth.uid()` | `owner_id = auth.uid()` ∨ `is_space_owner(space_id)` | idem UPDATE |
| `collections` | `is_space_member(space_id)` | `is_space_member(space_id)` ∧ `owner_id = auth.uid()` | `owner_id = auth.uid()` ∨ `is_space_owner(space_id)` | idem UPDATE |
| `collection_items` | `is_space_member(space_id)` | `is_space_member(space_id)` ∧ `added_by = auth.uid()` | `added_by = auth.uid()` ∨ `is_space_owner(space_id)` | idem UPDATE |
| `reviews` | `is_space_member(space_id)` | `user_id = auth.uid()` ∧ `is_space_member(space_id)` | **solo** `user_id = auth.uid()` | **solo** `user_id = auth.uid()` |

Ogni `UPDATE` porta lo stesso predicato sia in `USING` sia in `WITH CHECK`, come nelle migration
del D&D: senza `WITH CHECK` si potrebbe modificare una riga *spostandola* fuori dal proprio
spazio.

### 5.3 La RLS filtra, non concede

Questo è il livello più basso, e il più facile da dare per scontato: **una policy RLS non
autorizza nulla.** Restringe un permesso che deve già esistere. L'ordine di valutazione è
`GRANT` prima, policy dopo — se il ruolo non ha il privilegio sulla tabella, la query fallisce con
`permission denied for table ...` senza che nessuna policy venga mai consultata.

Su Supabase questo è oggi una trappola: nei progetti creati di recente il ruolo `authenticated`
**non** riceve automaticamente `SELECT/INSERT/UPDATE/DELETE` sulle tabelle nuove del suo schema.
I `default privileges` concedono `REFERENCES, TRIGGER, TRUNCATE` — tutto ciò che *non* serve a
un'applicazione. Un migration file fatto di sole `create table` + `create policy` produce quindi
uno schema in cui **niente funziona**, e l'errore non arriva alla scrittura della migration ma al
primo `select` reale.

Un dettaglio che rende la cosa scivolosa: uno schema esportato con `pg_dump` da un progetto
funzionante *contiene* i `grant`, perché `pg_dump` riversa lo stato finale. Chi impara leggendo un
dump non vede il problema, perché la soluzione è già lì dentro, mescolata a decine di righe
generate. Chi scrive una migration da zero lo incontra in pieno.

La migration di Eton chiude quindi con una sezione di privilegi **espliciti e minimi**: `anon` non
tocca nulla, `authenticated` riceve solo le operazioni che l'app esegue davvero, `service_role`
resta pieno per il pannello amministrativo.

```sql
grant usage on schema public to anon, authenticated;
revoke all on public.profiles, public.spaces, public.space_members from anon;
grant select, insert on public.profiles      to authenticated;
grant select, delete on public.spaces        to authenticated;
grant select, delete on public.space_members to authenticated;
```

Nessun `insert` su `spaces` e `space_members`: si entra solo da `create_space()` e `join_space()`,
che sono `security definer`. Il divieto sta quindi **sotto** la RLS, non dentro.

### 5.4 Le policy non bastano: servono anche i privilegi di colonna

Le policy RLS decidono **quali righe** si toccano. Non decidono **quali colonne**, e su questo
hanno due limiti strutturali:

- **In una policy non esiste `OLD`.** Non si può scrivere "`is_personal` non può passare da `true`
  a `false`", perché il valore precedente non è accessibile.
- **`WITH CHECK` valuta la riga proposta, ma solo per le colonne che nomina.** Un predicato come
  `is_space_owner(id)` sembra proteggere, e non protegge: la funzione riceve `id` — che non
  cambia — e va a rileggere `owner_id` dalla tabella, dove c'è ancora il valore vecchio. Il
  contrasto con `profiles`, dove `with check (id = auth.uid())` funziona benissimo, è istruttivo:
  lì la colonna è nominata direttamente. **In `WITH CHECK` si controlla una colonna solo
  nominandola; incapsularla in una funzione che rilegge equivale a non controllarla.**

Senza difesa, la conseguenza era concreta: il proprietario poteva azzerare `is_personal` sul
proprio spazio personale con una `PATCH`, e poi cancellarlo — aggirando l'invariante "lo spazio
personale non si cancella", che con `notes` e `collections` in cascata significa perdita di dati.

La difesa sta un livello più sotto, nei **privilegi di colonna**:

```sql
revoke update on public.spaces from anon, authenticated;
grant  update (name) on public.spaces to authenticated;

revoke update on public.profiles from anon, authenticated;
grant  update (display_name, avatar_url) on public.profiles to authenticated;
```

Regola generale per le tabelle future: **si concede l'`UPDATE` colonna per colonna, solo su ciò
che l'utente modifica davvero.** Chiavi, `space_id`, `owner_id`, contatori di versione e
timestamp non sono mai scrivibili dal client.

Sulla stessa linea, le funzioni non pensate per il client vengono tolte dalla superficie
`/rest/v1/rpc` (`revoke all on function ... from public, anon, authenticated`). **Eccezione
obbligatoria:** le funzioni usate *dentro* le policy — `is_space_member`, `is_space_owner`,
`is_space_owner_of`, `shares_space_with`, `space_is_personal` — devono restare eseguibili da
`authenticated`, perché le policy le invocano a nome del ruolo che sta interrogando: revocarle
farebbe fallire ogni query con `permission denied for function` invece di restituire zero righe.
Per questo restituiscono **booleani** e non identificatori: chi le chiama direttamente deve già
conoscere gli UUID su cui interroga.

### 5.5 Il proprietario non esce dal proprio spazio

La policy di `DELETE` su `space_members` protegge esplicitamente la riga del proprietario, che
non è rimovibile da nessuno — nemmeno da lui stesso. Il motivo: `spaces_delete` usa
`is_space_owner`, mentre `spaces_select` usa `is_space_member`. Un proprietario che uscisse
resterebbe quindi in grado di **cancellare** lo spazio pur non riuscendo più a **vederlo**.
Per andarsene, il proprietario cancella lo spazio; il passaggio di proprietà a un altro membro è
una funzionalità futura, non presente in questa versione.

Lo spazio personale non si abbandona e non si cancella: le policy di `DELETE` su `spaces` e
`space_members` escludono esplicitamente `is_personal`. È l'unico spazio senza `invite_code`,
quindi non è nemmeno raggiungibile da `join_space`.

**Perché `profiles` non ha una policy di `DELETE`.** Con RLS attiva, un'operazione priva di policy
è vietata a chiunque: l'assenza *è* la regola. E qui il divieto è voluto. La riga in `profiles` è
1:1 con `auth.users` e viene creata **una sola volta**, dal trigger `on_auth_user_created`: un
utente che cancellasse il proprio profilo restando autenticato finirebbe in uno stato senza
uscita — il trigger non riparte, e per gli altri membri dei suoi spazi i suoi voti resterebbero
attribuiti a un profilo inesistente. La cancellazione dell'account avviene a monte, su
`auth.users`, e da lì il `on delete cascade` porta via il profilo. (Anche in `DndCompanion`
`profiles` non ha mai avuto una policy di `DELETE`.)

Il vincolo *"il voto non supera `rating_max` della sua collezione"* non è esprimibile in una
`CHECK` (attraversa tre tabelle): il database garantisce solo `0 < rating ≤ 10`, la coerenza con
la scala scelta la impone il client. È l'unico punto del modello in cui la validazione non è
inaggirabile — accettabile, perché un voto fuori scala è un fastidio estetico, non una falla.

`AccessControl.CanEdit`, lato client, resta un controllo **puramente estetico** che rispecchia la
matrice per non mostrare pulsanti che fallirebbero.

---

## 6. Struttura del progetto

```
G:\Sviluppo\Eton\
  Eton.sln
  Eton.csproj                    Blazor WASM, net10.0, PWA
  Program.cs                     DI: servizi + un repository per aggregato
  App.razor · _Imports.razor
  Layout\        MainLayout · LoginLayout
  Shared\        BottomNav · SpaceSwitcher · ToastHost · ConfirmDialog
                 LoadingSpinner · UpdateBanner · DbErrorBanner · AuthRedirect
                 MarkdownView · FieldEditor · FieldInput · RatingInput    ← nuovi
  Pages\         Login · Home · Spaces · SpaceDetail
                 Notes · NoteEdit
                 Collections · CollectionEdit · Items · ItemDetail · Profile
  Models\        Space · SpaceMember · Profile · Note
                 Collection · CollectionField · CollectionItem · Review
  Services\
    SupabaseService · SupabaseClient · BrowserSessionHandler   ← adattati a Gotrue 6
    AuthStateService · CurrentUserService · AccessControl      ← portati
    SpaceStateService                                          ← da CampaignStateService
    ToastService · ConfirmService · PwaUpdateService           ← portati
    MarkdownRenderer                                           ← Markdig, .DisableHtml()
    FieldSchema · ItemDataMapper · RatingCalculations          ← logica pura
    Repositories\  ISpace · IProfile · INote · ICollection · IItem · IReview
  wwwroot\       index.html · manifest.webmanifest · service-worker · app.css · icone
  supabase\migrations\           SQL versionato
  docs\superpowers\specs\ e \plans\
Eton.Tests\              xUnit — funzioni pure
Eton.Tests.Integration\  xUnit — policy RLS
```

**Convenzioni**, ereditate da `DndCompanion`: identificatori C# e nomi di colonne in **inglese**;
commenti, XML doc e interfaccia utente in **italiano**. CSS isolation per componente
(`.razor.css`) più un `app.css`, nessun framework CSS. Un repository per aggregato dietro
interfaccia, registrato in DI.

---

## 7. Navigazione e schermate

```
  ┌─────────────────────────────┐
  │ 👥 Svapo con gli amici   ▾  │ ← selettore spazio
  ├─────────────────────────────┤
  │                             │
  │   contenuto della pagina    │
  │                        ╭───╮│
  │                        │ + ││ ← azione principale, contestuale
  │                        ╰───╯│
  ├─────────────────────────────┤
  │  🏠      📝      📊      👤 │
  │ Home   Note  Collez.  Profilo│
  └─────────────────────────────┘
```

Lo spazio attivo è tenuto e persistito da `SpaceStateService`; ogni pagina legge il contesto da
`CurrentUserService.EnsureLoadedAsync()`.

| Rotta | Contenuto |
|---|---|
| `/login` | Accedi con Google |
| `/` | Home dello spazio attivo: note recenti, collezioni, membri |
| `/spaces` | I tuoi spazi · crea · entra con un codice |
| `/spaces/{id}` | Membri, codice invito, rinomina, esci / espelli |
| `/notes` | Elenco note dello spazio |
| `/notes/{id}` | Editor Markdown con anteprima e checklist |
| `/collections` | Elenco collezioni |
| `/collections/new`, `/collections/{id}/edit` | Editor dei campi, con template |
| `/collections/{id}` | Elementi, ordinamento per media voto, filtro "da provare" |
| `/collections/{id}/items/{itemId}` | Scheda elemento, recensioni di tutti, il tuo voto |
| `/profile` | Nome visualizzato, logout |

---

## 8. Gestione degli errori

| Situazione | Comportamento |
|---|---|
| Rete assente / 5xx | Toast; i dati già caricati restano; **nessun logout** |
| Sessione scaduta | Refresh automatico; se il refresh token è morto, logout pulito verso `/login` |
| Conflitto di versione | Dialogo *Ricarica / Sovrascrivi*. Mai una perdita silenziosa |
| Nuova versione della PWA | `UpdateBanner` |
| Campo obbligatorio mancante | Validazione client + vincolo `not null` a database |

---

## 9. Test

**`Eton.Tests.Integration` — le policy RLS.** È il test più importante del progetto, e non tocca
il C# applicativo: in un'app senza server la sicurezza vive interamente nelle policy, e una
policy sbagliata non fa fallire nessuna compilazione né nessun collaudo manuale — perché chi
prova l'app i propri dati li vede giustamente. Il buco si vede solo dal punto di vista di un
secondo utente.

Autenticandosi come **due utenti distinti**, si asseriscono le negazioni:

- B non legge lo spazio personale di A;
- B non entra in uno spazio senza il codice invito;
- B non modifica il voto di A, pur conoscendone l'`id`;
- B non legge il `profile` di A se non condividono spazi;
- B non legge le note di uno spazio a cui non appartiene;
- B non sposta un proprio elemento in uno spazio altrui (`WITH CHECK`);
- B non si aggiunge a uno spazio scrivendo direttamente in `space_members`;
- B non crea uno spazio scrivendo direttamente in `spaces` (nessuna policy di `INSERT`);
- nessuno cancella né abbandona il proprio spazio personale.

Nella fetta 1 questo test esiste già, ma come **script `psql`** — `supabase/verifica-rls.sql`,
tredici sezioni eseguite contro lo stack Supabase locale, ciascuna con il risultato atteso
dichiarato accanto. Diventerà un progetto xUnit nella fetta 2, quando ci sarà abbastanza da
asserire da giustificarlo. Nella forma attuale ha già ripagato il costo: ha scoperto due difetti
che nessuna revisione statica aveva colto — i `grant` mancanti di §5.3, e il fatto che un client
non può cercare uno spazio dal codice invito prima di entrarci (la `select` è filtrata dalla RLS,
quindi il codice va passato *direttamente* a `join_space`, mai usato per una ricerca preliminare).

**`Eton.Tests` — logica pura**, con `InternalsVisibleTo` come nel D&D: `FieldSchema` (validazione
e serializzazione dei campi `jsonb`), `ItemDataMapper`, `RatingCalculations` (medie, conteggi,
"non l'hai ancora provato"), `AccessControl`, generazione del codice invito.

---

## 10. Rischi e vie d'uscita

| Rischio | Probabilità | Via d'uscita |
|---|---|---|
| **Gotrue 6 rompe l'adattamento del codice del D&D** — due major di distanza, l'interfaccia di persistenza della sessione potrebbe essere cambiata | Alta | Il login è la **prima fetta**: si scopre subito. Ripiego documentato: tornare a `gotrue-csharp` 4.2.7, versione già collaudata, dove tutti i workaround nei commenti del D&D valgono verbatim |
| Trimming rimuove costruttori usati via reflection: rompe **solo il sito pubblicato** | Media | `TrimmerRootAssembly` fin dal primo commit; verifica con `dotnet publish -c Release` e prova su `publish/wwwroot` con accesso fatto e una pagina di dati aperta |
| Progetto Supabase free **in pausa dopo 7 giorni** di inattività | Certa se l'app non viene usata | Nessuna perdita di dati: si riattiva dal pannello. Da sapere, non da mitigare |
| Il trigger su `auth.users` non è creabile con i privilegi disponibili | Bassa | Ripiego: creazione di profilo e spazio personale lato client al primo accesso, idempotente |
| L'editor dei campi si allarga (riordino, tipi, cancellazione con dati esistenti) | Media | Nella prima versione: aggiunta e rinomina sì; cancellazione di un campo lascia il dato orfano nel `jsonb` e lo ignora, senza migrazione |
| **Cancellare un utente cancella i suoi spazi condivisi.** `spaces.owner_id` ha `on delete cascade` verso `auth.users`: eliminare un account dal pannello Supabase porta via *tutti* gli spazi di cui era proprietario, e con essi note, collezioni e recensioni **di tutti i membri**, senza preavviso | Bassa (accade solo da pannello amministrativo) | **Accettato consapevolmente** in questa versione. Le alternative peggiorano: `on delete restrict` renderebbe impossibile cancellare un account finché possiede spazi, `set null` non è applicabile perché `owner_id` è `not null`. La via giusta, quando servirà, è il **passaggio di proprietà** prima della cancellazione — la stessa funzionalità che manca a §5.5 |

---

## 11. Fuori scope, e cosa costerebbe dopo

| Fuori | Costo di aggiungerlo |
|---|---|
| Upload foto dal telefono | **Basso** — `image_url` è già lì, cambia solo cosa ci finisce dentro |
| Aggiornamento in tempo reale (Supabase Realtime) | **Basso** — si innesta pagina per pagina |
| Link pubblici in sola lettura | **Basso** — una policy e un token |
| Tag e cartelle per le note | **Medio** — una tabella e del filtro |
| TWA sul Play Store | **Nullo sul codice** — artefatto separato. Serve: repo `<utente>.github.io` con `.nojekyll` e `.well-known/assetlinks.json` contenente la SHA-256 della **App Signing Key di Play** (non della upload key), più 25 $ una tantum e — su account personali creati dopo nov. 2023 — 12 tester per 14 giorni di closed testing |
| Editor a blocchi stile Notion | **Alto** — è un progetto a sé |
| Offline-first con sincronizzazione | **Alto** — copia locale, coda di modifiche, conflitti |
| App MAUI | **Alto** — la seconda autenticazione di §2.1 |

---

## 12. Ordine di implementazione

Il rischio per primo; ogni fetta lascia l'app usabile.

1. **Login Google su Gotrue 6** — scheletro del progetto, `SupabaseService` adattato, PWA
   minima, deploy funzionante. Fino a qui non si costruisce altro.
2. **Spazi** — spazio personale automatico, creazione, codice invito, ingresso con codice,
   membri. Con i test RLS.
3. **Note** — Markdown, checklist, concorrenza ottimistica.
4. **Collezioni** — editor dei campi, template "Liquidi svapo", elementi.
5. **Recensioni** — voto personale, medie, ordinamenti, filtro "da provare".
6. **Rifinitura** — icone, manifest, rifiniture mobile, deploy definitivo su GitHub Pages.

Dopo la fetta 3 l'app ha già senso da usare; dopo la 5 fa quello che il progetto si proponeva.
