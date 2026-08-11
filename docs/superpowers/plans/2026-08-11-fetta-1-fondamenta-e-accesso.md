# Eton — Fetta 1: fondamenta e accesso

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this
> plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> *Nota per l'orchestratore di questo repo:* le sotto-skill `subagent-driven-development` e
> `requesting-code-review` sono disattivate dalle istruzioni globali dell'utente; vale il
> «Protocollo di implementazione» del `CLAUDE.md` globale (brief → implementer → `bug-hunter` +
> `conformity` → adjudica).

**Goal:** un'applicazione Blazor WebAssembly che si apre, riconosce se sei autenticato, ti fa
accedere con Google tramite Supabase e ti crea automaticamente profilo e spazio personale.

**Architecture:** progetto singolo Blazor WASM standalone. Il browser parla direttamente con
Supabase: `Supabase.Gotrue` per l'autenticazione, `Supabase.Postgrest` per i dati, RLS di
PostgreSQL come unico confine di sicurezza. Lo strato Supabase è un adattamento del codice
collaudato di `DndCompanion`, portato da gotrue-csharp 4.2.7 a Supabase.Gotrue 6.3.0 e convertito
dal flusso OAuth *implicit* a **PKCE**.

**Tech Stack:** .NET 10 · Blazor WebAssembly · Supabase.Gotrue 6.3.0 · Supabase.Postgrest 4.4.0 ·
xUnit 2.9.3 · PostgreSQL (Supabase) · GitHub Pages

**Fonte:** `docs/superpowers/specs/2026-08-11-eton-design.md`. Questo piano copre la **fetta 1**
delle sei elencate in §12 della spec. Le fette 2–6 avranno un piano ciascuna, scritto dopo che
questa sarà verde: la spec descrive il modello completo, ma scrivere ora il codice delle fette
successive significherebbe inventare dettagli prima di aver visto girare l'autenticazione.

---

## Global Constraints

- **.NET 10** (`net10.0`). SDK verificato in locale: 10.0.302.
- **Namespace radice `Eton`**. Identificatori C# e nomi di colonne in **inglese**; commenti, XML
  doc e testi dell'interfaccia in **italiano**.
- **Pacchetti a versione fissa:** `Supabase.Gotrue` **6.3.0**, `Supabase.Postgrest` **4.4.0**.
  Attenzione: gli ID dei pacchetti sono **diversi** da quelli di `DndCompanion`
  (`gotrue-csharp` / `postgrest-csharp`) e i namespace sono `Supabase.Gotrue` e
  **`Supabase.Postgrest`** (nel D&D era `Postgrest`).
- **`PublishTrimmed` + `TrimMode=full` in Release**, con `TrimmerRootAssembly` per `Eton`,
  `Supabase.Gotrue`, `Supabase.Postgrest`. Senza, il trimmer rimuove costruttori usati via
  reflection da Newtonsoft e l'app si rompe **solo sul sito pubblicato**.
- **`InvariantGlobalization=true`** (dimensione del bundle). Conseguenza vincolante: date e numeri
  vanno formattati con pattern **espliciti** (`"dd/MM/yyyy"`, `"0.00"`), mai con `"d"`, `"C"` o
  `CultureInfo.CurrentCulture`, che qui non esistono. Lo stesso flag va nel progetto di test, così
  i test verificano il mondo che esiste in produzione.
- **La sicurezza sta nelle policy RLS**, non nel C#. Nessun controllo lato client è considerato
  una difesa.
- **Un solo ramo, `main`.** Commit piccoli e a tema singolo. Il push si fa solo su richiesta
  esplicita dell'utente.
- **Firme verificate** l'11-08-2026 estraendo la documentazione XML dai pacchetti NuGet. Sono
  quelle riportate nel piano; non vanno indovinate diversamente.

---

## File Structure

| File | Responsabilità |
|---|---|
| `Eton.sln` | Soluzione: app + test |
| `Eton.csproj` | Progetto Blazor WASM, pacchetti, trimming |
| `Program.cs` | Composition root: registrazioni DI |
| `App.razor`, `_Imports.razor` | Router e using globali |
| `Layout/MainLayout.razor` | Guscio dell'app autenticata |
| `Layout/LoginLayout.razor` | Guscio a pagina piena per l'accesso |
| `Pages/Login.razor` | Pulsante "Accedi con Google", avvio del flusso PKCE |
| `Pages/Home.razor` | Segnaposto autenticato: mostra nome utente e logout |
| `Shared/AuthRedirect.razor` | Rimanda a `/login` chi non è autenticato |
| `Services/SupabaseService.cs` | Costruzione dei client, bootstrap sessione, refresh, logout |
| `Services/SupabaseClient.cs` | Facade su Gotrue + Postgrest (`Auth`, `From<T>`, `Rpc<T>`) |
| `Services/BrowserSessionHandler.cs` | Persistenza sessione su `localStorage` |
| `Services/PkceStore.cs` | Persistenza del verificatore PKCE fra redirect e ritorno |
| `Services/SessionFreshness.cs` | **Puro**: quando rinfrescare, quando ritentare |
| `Services/OAuthCallback.cs` | **Puro**: estrae `code` / `error_description` dall'URL di ritorno |
| `Services/AuthStateService.cs` | Identità dell'utente autenticato |
| `wwwroot/index.html` | Host HTML, `<base href>`, registrazione service worker |
| `wwwroot/appsettings.json` | `Supabase:Url` e `Supabase:AnonKey` |
| `wwwroot/manifest.webmanifest` | Manifest PWA |
| `wwwroot/css/app.css` | Stili globali, mobile first |
| `supabase/migrations/20260811000000_initial_schema.sql` | Profili, spazi, membri, funzioni, RLS |
| `Eton.Tests/Eton.Tests.csproj` | Progetto di test |
| `Eton.Tests/SessionFreshnessTests.cs` | Test della logica di rinnovo |
| `Eton.Tests/OAuthCallbackTests.cs` | Test del parsing dell'URL di ritorno |
| `.github/workflows/deploy.yml` | Publish e deploy su GitHub Pages |

Le due classi pure (`SessionFreshness`, `OAuthCallback`) esistono perché sono l'unica parte
dell'autenticazione verificabile senza un browser e senza un server: tutto ciò che è decisione
viene spinto lì, e ciò che resta in `SupabaseService` è I/O.

---

### Task 1: Scaffolding della soluzione

**Files:**
- Create: `Eton.csproj`, `Eton.sln`, `Program.cs`, `App.razor`, `_Imports.razor`
- Create: `wwwroot/index.html`, `wwwroot/appsettings.json`, `wwwroot/css/app.css`
- Create: `Layout/MainLayout.razor`, `Layout/LoginLayout.razor`, `Pages/Home.razor`
- Create: `Eton.Tests/Eton.Tests.csproj`
- Create: `.gitattributes`

**Interfaces:**
- Consumes: niente.
- Produces: namespace `Eton` e `Eton.Services`; l'app compila ed esegue mostrando la Home.

- [ ] **Step 1: Creare `.gitattributes`**

Evita il rumore CRLF/LF già visto al primo commit.

```
* text=auto eol=lf
*.sln text eol=crlf
*.png binary
*.ico binary
```

- [ ] **Step 2: Creare `Eton.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>Eton</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <OverrideHtmlAssetPlaceholders>true</OverrideHtmlAssetPlaceholders>
    <ServiceWorkerAssetsManifest>service-worker-assets.js</ServiceWorkerAssetsManifest>
    <InvariantGlobalization>true</InvariantGlobalization>
    <BlazorWebAssemblyLoadAllGlobalizationData>false</BlazorWebAssemblyLoadAllGlobalizationData>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>full</TrimMode>
    <DebuggerSupport>false</DebuggerSupport>
    <EventSourceSupport>false</EventSourceSupport>
    <HttpActivityPropagationSupport>false</HttpActivityPropagationSupport>
    <MetadataUpdaterSupport>false</MetadataUpdaterSupport>
    <UseSystemResourceKeys>true</UseSystemResourceKeys>
  </PropertyGroup>

  <ItemGroup Condition="'$(Configuration)' == 'Release'">
    <!-- I tipi Gotrue e Postgrest sono (de)serializzati via reflection da Newtonsoft: senza root,
         TrimMode=full rimuove i costruttori e l'app fallisce con "Unable to find a constructor to
         use for type ...". Il difetto NON si vede in Debug: solo nel publish Release. Nomi degli
         ASSEMBLY (DLL), non dei namespace. -->
    <TrimmerRootAssembly Include="Eton" />
    <TrimmerRootAssembly Include="Supabase.Gotrue" />
    <TrimmerRootAssembly Include="Supabase.Postgrest" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.5" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="10.0.5" PrivateAssets="all" />
    <PackageReference Include="Supabase.Gotrue" Version="6.3.0" />
    <PackageReference Include="Supabase.Postgrest" Version="4.4.0" />
  </ItemGroup>

  <ItemGroup>
    <ServiceWorker Include="wwwroot\service-worker.js" PublishedContent="wwwroot\service-worker.published.js" />
  </ItemGroup>

  <ItemGroup>
    <!-- Espone i membri internal (helper puri) al progetto di test. -->
    <InternalsVisibleTo Include="Eton.Tests" />
  </ItemGroup>

</Project>
```

> Se la versione 10.0.5 di `Microsoft.AspNetCore.Components.WebAssembly` non è disponibile,
> allineare alla patch più recente della banda 10.0.\* installata (`dotnet list package --outdated`);
> non cambiare la major.

- [ ] **Step 3: Creare `Eton.Tests/Eton.Tests.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <!-- Allinea i test alla produzione: il bundle WASM compila con InvariantGlobalization=true,
         quindi senza questo flag i test girerebbero con ICU pieno, verificando un mondo che in
         produzione non esiste. -->
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Eton.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 4: Creare `_Imports.razor`**

```razor
@using System.Net.Http
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using Eton
@using Eton.Layout
@using Eton.Services
@using Eton.Shared
```

- [ ] **Step 5: Creare `App.razor`**

```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Pagina non trovata.</p>
        </LayoutView>
    </NotFound>
</Router>
```

- [ ] **Step 6: Creare i due layout**

`Layout/LoginLayout.razor` — guscio a pagina piena, senza navigazione:

```razor
@inherits LayoutComponentBase

<main class="login-layout">
    @Body
</main>
```

`Layout/MainLayout.razor` — guscio autenticato; la barra di navigazione inferiore arriva nella
fetta 2, quando ci saranno più sezioni fra cui navigare:

```razor
@inherits LayoutComponentBase

<AuthRedirect />

<main class="app-layout">
    @Body
</main>
```

- [ ] **Step 7: Creare `Pages/Home.razor`** (segnaposto verificabile: mostra chi sei)

```razor
@page "/"
@inject AuthStateService AuthState
@inject NavigationManager Navigation

<PageTitle>Eton</PageTitle>

<h1>Eton</h1>

@if (nome is not null)
{
    <p>Ciao, @nome.</p>
    <button class="btn" @onclick="Esci">Esci</button>
}
else
{
    <p>Caricamento…</p>
}

@code {
    private string? nome;

    protected override async Task OnInitializedAsync()
        => nome = await AuthState.GetDisplayNameAsync();

    private async Task Esci()
    {
        await AuthState.LogoutAsync();
    }
}
```

- [ ] **Step 8: Creare `wwwroot/index.html`**

`<base href="/" />` resta così nel repository: il workflow di deploy lo riscrive in `/eton/` al
momento del publish (Task 6). Tenerlo a `/` è ciò che fa funzionare `dotnet run` in locale.

```html
<!DOCTYPE html>
<html lang="it">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover" />
    <title>Eton</title>
    <base href="/" />
    <link rel="manifest" href="manifest.webmanifest" />
    <link rel="apple-touch-icon" sizes="180x180" href="icon-180.png" />
    <link rel="icon" type="image/png" href="favicon.png" />
    <link rel="stylesheet" href="css/app.css" />
    <meta name="theme-color" content="#1f2933" />
</head>
<body>
    <div id="app">
        <div class="avvio">Caricamento…</div>
    </div>

    <div id="blazor-error-ui">
        Si è verificato un errore imprevisto.
        <a href="" class="reload">Ricarica</a>
        <span class="dismiss">🗙</span>
    </div>

    <script src="_framework/blazor.webassembly.js"></script>
    <script>
        navigator.serviceWorker?.register('service-worker.js');
    </script>
</body>
</html>
```

- [ ] **Step 9: Creare `wwwroot/appsettings.json`**

I due valori sono **vuoti di proposito**: li fornisce l'utente al Task 5, dopo aver creato il
progetto Supabase. `SupabaseService` fallisce subito e con un messaggio chiaro se mancano —
comportamento voluto: meglio un errore all'avvio che una richiesta non autenticata.

```json
{
  "Supabase": {
    "Url": "",
    "AnonKey": ""
  }
}
```

- [ ] **Step 10: Creare `wwwroot/css/app.css`**

Mobile first: si parte dal telefono e si allarga. Nessun framework.

```css
:root {
    --sfondo: #12171c;
    --superficie: #1b2229;
    --testo: #e8edf2;
    --testo-tenue: #9aa8b5;
    --accento: #4a9d7f;
    --bordo: #2b343d;
    --raggio: 12px;
}

* { box-sizing: border-box; }

html, body {
    margin: 0;
    padding: 0;
    background: var(--sfondo);
    color: var(--testo);
    font-family: system-ui, -apple-system, "Segoe UI", Roboto, sans-serif;
    font-size: 16px;
    line-height: 1.5;
}

.app-layout, .login-layout {
    max-width: 720px;
    margin: 0 auto;
    padding: 1rem;
    padding-bottom: calc(1rem + env(safe-area-inset-bottom));
}

.login-layout {
    min-height: 100dvh;
    display: flex;
    align-items: center;
    justify-content: center;
}

h1 { font-size: 1.5rem; margin: 0 0 1rem; }

.btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: .5rem;
    min-height: 48px;            /* bersaglio tattile: sotto i 44px si sbaglia a toccare */
    padding: 0 1.25rem;
    border: 1px solid var(--bordo);
    border-radius: var(--raggio);
    background: var(--superficie);
    color: var(--testo);
    font: inherit;
    cursor: pointer;
}

.btn.primary { background: var(--accento); border-color: var(--accento); color: #06120d; font-weight: 600; }
.btn:disabled { opacity: .6; cursor: default; }

.errore {
    border: 1px solid #6d2b2b;
    background: #2a1616;
    border-radius: var(--raggio);
    padding: .75rem 1rem;
    margin-bottom: 1rem;
}

.avvio { padding: 2rem; color: var(--testo-tenue); }

#blazor-error-ui {
    display: none;
    position: fixed;
    inset: auto 0 0 0;
    padding: .75rem 1rem;
    background: #ffe08a;
    color: #202020;
}
```

- [ ] **Step 11: Creare `Program.cs`** (registrazioni minime; crescerà nei task successivi)

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Eton;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
```

> `Pages/Home.razor` e `Layout/MainLayout.razor` referenziano `AuthStateService` e `AuthRedirect`,
> che nascono nei Task 4 e 5. Fino ad allora la soluzione **non compila**: è previsto. Per avere
> una build verde già qui, creare Home e MainLayout nella forma ridotta indicata allo Step 12 e
> sostituirli al Task 5.

- [ ] **Step 12: Versione provvisoria compilabile**

Sostituire temporaneamente il corpo di `Pages/Home.razor` con `@page "/"` + `<h1>Eton</h1>`, e
`Layout/MainLayout.razor` con `@inherits LayoutComponentBase` + `<main class="app-layout">@Body</main>`
(senza `<AuthRedirect />`). Il Task 5 ripristina le versioni definitive degli Step 6 e 7.

- [ ] **Step 13: Creare la soluzione e verificare la build**

```bash
cd /g/Sviluppo/Eton
dotnet new sln -n Eton
dotnet sln add Eton.csproj Eton.Tests/Eton.Tests.csproj
dotnet build Eton.sln
```

Atteso: `Build succeeded`, zero errori.

- [ ] **Step 14: Verificare che l'app si apra**

```bash
dotnet run --project Eton.csproj
```

Atteso: il browser su `http://localhost:5xxx` mostra il titolo "Eton". Fermare con Ctrl-C.

- [ ] **Step 15: Commit**

```bash
git add -A
git commit -m "Scaffolding: progetto Blazor WASM, test, stili di base"
```

---

### Task 2: Migrazione SQL — profili, spazi, membri, funzioni, RLS

**Files:**
- Create: `supabase/migrations/20260811000000_initial_schema.sql`
- Create: `supabase/config.toml` (generato da `supabase init`)

**Interfaces:**
- Consumes: niente.
- Produces: tabelle `profiles`, `spaces`, `space_members`; funzioni
  `is_space_member(uuid) → boolean`, `is_space_owner(uuid) → boolean`,
  `shares_space_with(uuid) → boolean`, `generate_invite_code() → text`,
  `create_space(text) → uuid`, `join_space(text) → uuid`; trigger `on_auth_user_created`.
  Le tabelle di note, collezioni ed elementi arrivano nelle fette successive, con le proprie
  migrazioni.

- [ ] **Step 1: Inizializzare la cartella Supabase**

```bash
cd /g/Sviluppo/Eton
supabase init
```

Atteso: creati `supabase/config.toml` e `supabase/.gitignore`.

- [ ] **Step 2: Scrivere la migrazione**

File `supabase/migrations/20260811000000_initial_schema.sql`:

```sql
-- =====================================================================================
-- Eton — schema iniziale: identità, spazi, membri.
-- Idempotente e rieseguibile.
-- =====================================================================================

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

create or replace function public.is_space_owner(p_space uuid)
returns boolean language sql security definer stable
set search_path = public as $$
    select exists (
        select 1 from spaces
        where id = p_space and owner_id = auth.uid()
    );
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

-- Codice a 8 caratteri, alfabeto senza caratteri ambigui (niente 0/O, 1/I/L): 31^8 ≈ 40 bit,
-- enumerazione impraticabile. Riprova finché non trova un codice libero.
create or replace function public.generate_invite_code()
returns text language plpgsql security definer
set search_path = public as $$
declare
    alfabeto constant text := 'ABCDEFGHJKMNPQRSTUVWXYZ23456789';
    tentativo text;
    i integer;
begin
    loop
        tentativo := '';
        for i in 1..8 loop
            tentativo := tentativo || substr(alfabeto, 1 + floor(random() * length(alfabeto))::int, 1);
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
create policy spaces_update on public.spaces
    for update using (public.is_space_owner(id)) with check (public.is_space_owner(id));

drop policy if exists spaces_delete on public.spaces;
create policy spaces_delete on public.spaces
    for delete using (public.is_space_owner(id) and not is_personal);

-- ---------- space_members ----------
-- Nessuna policy di INSERT né di UPDATE: si entra solo tramite create_space()/join_space().
drop policy if exists space_members_select on public.space_members;
create policy space_members_select on public.space_members
    for select using (public.is_space_member(space_id));

drop policy if exists space_members_delete on public.space_members;
create policy space_members_delete on public.space_members
    for delete using (
        (user_id = auth.uid() or public.is_space_owner(space_id))
        and not exists (select 1 from public.spaces s where s.id = space_id and s.is_personal)
    );
```

- [ ] **Step 3: Verificare che la migrazione sia sintatticamente valida**

Richiede Docker Desktop in esecuzione.

```bash
cd /g/Sviluppo/Eton
supabase start
supabase db reset
```

Atteso: `Applying migration 20260811000000_initial_schema.sql...` senza errori, poi
`Finished supabase db reset.`

Se Docker non è disponibile, saltare e verificare al Task 5 incollando il file nell'editor SQL
del progetto Supabase cloud: l'esito atteso è lo stesso, `Success. No rows returned`.

- [ ] **Step 4: Verificare a mano che il trigger funzioni**

Con lo stack locale attivo:

```bash
supabase db reset
psql "postgresql://postgres:postgres@127.0.0.1:54322/postgres" -c \
  "insert into auth.users (id, email, raw_user_meta_data, aud, role)
   values (gen_random_uuid(), 'prova@esempio.it', '{\"full_name\":\"Prova\"}'::jsonb, 'authenticated', 'authenticated');"
psql "postgresql://postgres:postgres@127.0.0.1:54322/postgres" -c \
  "select p.display_name, s.name, s.is_personal, s.invite_code
     from profiles p join spaces s on s.owner_id = p.id;"
```

Atteso: una riga — `Prova | Personale | t | (null)`.

- [ ] **Step 5: Commit**

```bash
git add supabase/
git commit -m "Schema iniziale: profili, spazi, membri, funzioni SECURITY DEFINER e RLS"
```

---

### Task 3: Logica pura dell'autenticazione (TDD)

**Files:**
- Create: `Services/SessionFreshness.cs`, `Services/OAuthCallback.cs`
- Test: `Eton.Tests/SessionFreshnessTests.cs`, `Eton.Tests/OAuthCallbackTests.cs`

**Interfaces:**
- Consumes: niente.
- Produces:
  - `SessionFreshness.VaRinfrescata(DateTime scadenzaUtc, DateTime adessoUtc) → bool`
  - `SessionFreshness.SiPuoRitentare(DateTime? ultimoFallimentoUtc, DateTime adessoUtc) → bool`
  - `SessionFreshness.Margine` / `.AttesaDopoFallimento` (`TimeSpan`)
  - `OAuthCallback.Analizza(string uri) → OAuthCallbackEsito`
  - `record OAuthCallbackEsito(string? Codice, string? Errore)` — `Codice` valorizzato solo su
    ritorno riuscito, `Errore` solo su rifiuto; entrambi null se l'URL non è un ritorno OAuth.

- [ ] **Step 1: Scrivere i test di `SessionFreshness`**

`Eton.Tests/SessionFreshnessTests.cs`:

```csharp
using Eton.Services;

namespace Eton.Tests;

public class SessionFreshnessTests
{
    private static readonly DateTime Adesso = new(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Sessione_lontana_dalla_scadenza_non_va_rinfrescata()
        => Assert.False(SessionFreshness.VaRinfrescata(Adesso.AddMinutes(30), Adesso));

    [Fact]
    public void Sessione_dentro_il_margine_va_rinfrescata()
        => Assert.True(SessionFreshness.VaRinfrescata(Adesso.AddMinutes(4), Adesso));

    [Fact]
    public void Sessione_gia_scaduta_va_rinfrescata()
        => Assert.True(SessionFreshness.VaRinfrescata(Adesso.AddMinutes(-1), Adesso));

    [Fact]
    public void Senza_fallimenti_precedenti_si_puo_ritentare()
        => Assert.True(SessionFreshness.SiPuoRitentare(null, Adesso));

    [Fact]
    public void Subito_dopo_un_fallimento_non_si_ritenta()
        => Assert.False(SessionFreshness.SiPuoRitentare(Adesso.AddSeconds(-5), Adesso));

    [Fact]
    public void Passata_l_attesa_si_ritenta()
        => Assert.True(SessionFreshness.SiPuoRitentare(Adesso.AddSeconds(-31), Adesso));
}
```

- [ ] **Step 2: Eseguire i test e verificare che falliscano**

```bash
dotnet test Eton.Tests/Eton.Tests.csproj
```

Atteso: errore di compilazione, `SessionFreshness` non esiste.

- [ ] **Step 3: Implementare `Services/SessionFreshness.cs`**

```csharp
namespace Eton.Services;

/// <summary>
/// Le due decisioni pure del rinnovo di sessione, isolate per essere verificabili senza un client
/// Gotrue: quando rinfrescare (<see cref="VaRinfrescata"/>) e se ha senso ritentare dopo un
/// fallimento (<see cref="SiPuoRitentare"/>).
/// Si rinfresca PRIMA della scadenza, non dopo: un token che scade a metà di una richiesta già
/// partita produce un 403 che l'utente vede come "errore" senza capirne il motivo.
/// </summary>
public static class SessionFreshness
{
    /// <summary>Margine di sicurezza: si rinfresca prima della scadenza vera, così una richiesta
    /// partita subito dopo il controllo non si trova col token morto a metà strada.</summary>
    public static readonly TimeSpan Margine = TimeSpan.FromMinutes(5);

    /// <summary>Intervallo minimo fra due tentativi falliti, per non martellare il server quando
    /// la rete è giù: senza, ogni chiamata dati riproverebbe subito.</summary>
    public static readonly TimeSpan AttesaDopoFallimento = TimeSpan.FromSeconds(30);

    /// <summary>True se la sessione che scade a <paramref name="scadenzaUtc"/> va rinfrescata ora.</summary>
    public static bool VaRinfrescata(DateTime scadenzaUtc, DateTime adessoUtc)
        => adessoUtc + Margine >= scadenzaUtc;

    /// <summary>True se è passato abbastanza tempo dall'ultimo tentativo fallito.
    /// <paramref name="ultimoFallimentoUtc"/> null = nessun tentativo fallito finora.</summary>
    public static bool SiPuoRitentare(DateTime? ultimoFallimentoUtc, DateTime adessoUtc)
        => ultimoFallimentoUtc is null || adessoUtc - ultimoFallimentoUtc.Value >= AttesaDopoFallimento;
}
```

- [ ] **Step 4: Eseguire i test e verificare che passino**

```bash
dotnet test Eton.Tests/Eton.Tests.csproj
```

Atteso: 6 test passati.

- [ ] **Step 5: Scrivere i test di `OAuthCallback`**

`Eton.Tests/OAuthCallbackTests.cs`:

```csharp
using Eton.Services;

namespace Eton.Tests;

public class OAuthCallbackTests
{
    [Fact]
    public void Url_normale_non_e_un_ritorno_oauth()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/eton/");
        Assert.Null(esito.Codice);
        Assert.Null(esito.Errore);
    }

    [Fact]
    public void Estrae_il_codice_dalla_query()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/eton/?code=abc123");
        Assert.Equal("abc123", esito.Codice);
        Assert.Null(esito.Errore);
    }

    [Fact]
    public void Estrae_il_codice_anche_con_altri_parametri()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?stato=x&code=abc123&altro=y");
        Assert.Equal("abc123", esito.Codice);
    }

    [Fact]
    public void Decodifica_i_valori_percent_encoded()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?error_description=Accesso%20negato");
        Assert.Equal("Accesso negato", esito.Errore);
        Assert.Null(esito.Codice);
    }

    [Fact]
    public void Il_parametro_error_vale_come_errore_anche_senza_descrizione()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?error=access_denied");
        Assert.Equal("access_denied", esito.Errore);
    }

    [Fact]
    public void Un_errore_ha_la_precedenza_sul_codice()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?code=abc&error=access_denied");
        Assert.Null(esito.Codice);
        Assert.Equal("access_denied", esito.Errore);
    }

    [Fact]
    public void Non_confonde_un_parametro_che_finisce_per_code()
    {
        var esito = OAuthCallback.Analizza("https://esempio.it/?postcode=20100");
        Assert.Null(esito.Codice);
    }
}
```

- [ ] **Step 6: Eseguire i test e verificare che falliscano**

```bash
dotnet test Eton.Tests/Eton.Tests.csproj
```

Atteso: errore di compilazione, `OAuthCallback` non esiste.

- [ ] **Step 7: Implementare `Services/OAuthCallback.cs`**

```csharp
namespace Eton.Services;

/// <summary>Esito dell'analisi dell'URL di ritorno da Google.</summary>
/// <param name="Codice">Codice di autorizzazione monouso (flusso PKCE); null se assente o se c'è un errore.</param>
/// <param name="Errore">Messaggio di rifiuto del provider; null se l'accesso non è stato rifiutato.</param>
public sealed record OAuthCallbackEsito(string? Codice, string? Errore);

/// <summary>
/// Analisi pura dell'URL su cui il provider ci riporta dopo l'accesso. Isolata da
/// <see cref="SupabaseService"/> perché è l'unico pezzo del flusso OAuth verificabile senza un
/// browser: qui si sbaglia in silenzio (un parametro letto male = login che non si completa mai),
/// e un test costa niente.
/// Si legge SOLO la query, non il fragment: col flusso PKCE il provider restituisce un codice
/// monouso in <c>?code=</c>. Il fragment conterrebbe un access token — è il motivo per cui il
/// flusso implicit è stato abbandonato.
/// </summary>
public static class OAuthCallback
{
    public static OAuthCallbackEsito Analizza(string uri)
    {
        var parametri = LeggiQuery(uri);

        // L'errore ha la precedenza: se il provider ha rifiutato, un eventuale code è inutilizzabile.
        if (parametri.TryGetValue("error_description", out var descrizione) && !string.IsNullOrWhiteSpace(descrizione))
            return new OAuthCallbackEsito(null, descrizione);

        if (parametri.TryGetValue("error", out var errore) && !string.IsNullOrWhiteSpace(errore))
            return new OAuthCallbackEsito(null, errore);

        if (parametri.TryGetValue("code", out var codice) && !string.IsNullOrWhiteSpace(codice))
            return new OAuthCallbackEsito(codice, null);

        return new OAuthCallbackEsito(null, null);
    }

    private static Dictionary<string, string> LeggiQuery(string uri)
    {
        var risultato = new Dictionary<string, string>(StringComparer.Ordinal);

        var inizio = uri.IndexOf('?');
        if (inizio < 0) return risultato;

        var query = uri[(inizio + 1)..];
        var fine = query.IndexOf('#');
        if (fine >= 0) query = query[..fine];

        foreach (var coppia in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatore = coppia.IndexOf('=');
            if (separatore <= 0) continue;

            var chiave = Uri.UnescapeDataString(coppia[..separatore]);
            var valore = Uri.UnescapeDataString(coppia[(separatore + 1)..].Replace('+', ' '));
            risultato[chiave] = valore;
        }

        return risultato;
    }
}
```

- [ ] **Step 8: Eseguire i test e verificare che passino**

```bash
dotnet test Eton.Tests/Eton.Tests.csproj
```

Atteso: 13 test passati.

- [ ] **Step 9: Commit**

```bash
git add Services/SessionFreshness.cs Services/OAuthCallback.cs Eton.Tests/
git commit -m "Logica pura di sessione e ritorno OAuth, con i suoi test"
```

---

### Task 4: Strato Supabase su Gotrue 6 con flusso PKCE

**Files:**
- Create: `Services/BrowserSessionHandler.cs`, `Services/PkceStore.cs`,
  `Services/SupabaseClient.cs`, `Services/SupabaseService.cs`
- Modify: `Program.cs`

**Interfaces:**
- Consumes: `SessionFreshness`, `OAuthCallback` (Task 3).
- Produces:
  - `SupabaseClient` con `Auth` (`Supabase.Gotrue.Client`),
    `From<T>() → Supabase.Postgrest.Table<T>`,
    `Rpc<T>(string, Dictionary<string, object>) → Task<T?>`
  - `SupabaseService.GetClientAsync() → Task<SupabaseClient>` — bootstrap idempotente
  - `SupabaseService.AvviaAccessoGoogleAsync() → Task` — redirige a Google
  - `SupabaseService.SignOutAsync() → Task` — non lancia mai
  - `SupabaseService.ErroreAccesso → string?` — messaggio dell'ultimo rifiuto, letto da `Login.razor`

**Firme verificate su Supabase.Gotrue 6.3.0** (dalla documentazione XML del pacchetto):
`Client(ClientOptions)` · `ClientOptions { Url, Headers, AutoRefreshToken, AllowUnconfirmedUserSessions, DebugRefreshToken, MaximumRefreshWaitTime }` ·
`SetPersistence(IGotrueSessionPersistence<Session>)` · `IGotrueSessionPersistence<T> { LoadSession(), SaveSession(T), DestroySession() }` (sincrona) ·
`LoadSession()` · `SignIn(Constants.Provider, SignInOptions) → ProviderAuthState { Uri, PKCEVerifier, State }` ·
`SignInOptions { FlowType, RedirectTo, Scopes, QueryParams, State }` · `Constants.OAuthFlowType { Implicit, PKCE }` ·
`ExchangeCodeForSession(string codeVerifier, string authCode)` · `RefreshToken(string, string)` ·
`SignOut(Constants.SignOutScope)` con `SignOutScope { Global, Local, Others }` ·
`DestroySession()` e `UpdateSession(Session)` **pubblici** · `NotifyAuthStateChange(Constants.AuthState)`.
**Su Supabase.Postgrest 4.4.0:** namespace `Supabase.Postgrest`; `Client(string, ClientOptions)`;
`GetHeaders` property; `Table<T>()`; `Rpc<T>(string, object)`; `Supabase.Postgrest.Models.BaseModel`.

- [ ] **Step 1: Creare `Services/BrowserSessionHandler.cs`**

```csharp
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace Eton.Services;

/// <summary>
/// Persistenza della sessione Gotrue su localStorage. L'interfaccia
/// <see cref="IGotrueSessionPersistence{T}"/> è SINCRONA, quindi serve
/// <see cref="IJSInProcessRuntime"/> (Invoke sincrono) e non <see cref="IJSRuntime"/>.
/// È anche il motivo per cui questo file non funzionerebbe in un'app MAUI Blazor Hybrid, dove
/// il JS interop è solo asincrono: v. §2.1 della spec.
/// La <see cref="Session"/> si serializza con Newtonsoft (i suoi attributi sono
/// <c>[JsonProperty]</c>), coerentemente con la libreria.
/// </summary>
public class BrowserSessionHandler : IGotrueSessionPersistence<Session>
{
    private const string StorageKey = "eton.session";
    private readonly IJSInProcessRuntime _js;

    public BrowserSessionHandler(IJSInProcessRuntime js) => _js = js;

    public void SaveSession(Session session)
        => _js.InvokeVoid("localStorage.setItem", StorageKey, JsonConvert.SerializeObject(session));

    public void DestroySession()
        => _js.InvokeVoid("localStorage.removeItem", StorageKey);

    public Session? LoadSession()
    {
        var json = _js.Invoke<string?>("localStorage.getItem", StorageKey);
        return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<Session>(json);
    }
}
```

- [ ] **Step 2: Creare `Services/PkceStore.cs`**

```csharp
using Microsoft.JSInterop;

namespace Eton.Services;

/// <summary>
/// Custodisce il verificatore PKCE fra la partenza verso Google e il ritorno.
/// Serve perché nel mezzo il browser lascia l'applicazione: quando torna, la pagina è stata
/// ricaricata da zero e nulla che stesse in memoria è sopravvissuto. Il verificatore è l'unica
/// prova che chi presenta il codice è lo stesso che l'ha richiesto: senza, il codice intercettato
/// da qualcun altro sarebbe spendibile.
/// Si cancella subito dopo l'uso — è monouso per definizione.
/// </summary>
public class PkceStore
{
    private const string StorageKey = "eton.pkce";
    private readonly IJSInProcessRuntime _js;

    public PkceStore(IJSInProcessRuntime js) => _js = js;

    public void Salva(string verificatore)
        => _js.InvokeVoid("localStorage.setItem", StorageKey, verificatore);

    public string? Leggi()
    {
        var valore = _js.Invoke<string?>("localStorage.getItem", StorageKey);
        return string.IsNullOrEmpty(valore) ? null : valore;
    }

    public void Cancella() => _js.InvokeVoid("localStorage.removeItem", StorageKey);
}
```

- [ ] **Step 3: Creare `Services/SupabaseClient.cs`**

```csharp
using Supabase.Gotrue;
using Postgrest = Supabase.Postgrest;

namespace Eton.Services;

/// <summary>
/// Facade unica su autenticazione e dati, così i repository dipendono da un solo tipo e non
/// dai due client separati.
/// </summary>
public sealed class SupabaseClient
{
    private readonly Postgrest.Client _postgrest;

    public SupabaseClient(Client auth, Postgrest.Client postgrest)
    {
        Auth = auth;
        _postgrest = postgrest;
    }

    /// <summary>Client Gotrue: CurrentSession, SignIn, SignOut, ExchangeCodeForSession, …</summary>
    public Client Auth { get; }

    /// <summary>Tabella tipizzata. <c>Client.Table&lt;T&gt;()</c> dichiara il ritorno come
    /// <c>IPostgrestTable&lt;T&gt;</c>, ma l'istanza concreta è sempre <c>Table&lt;T&gt;</c>:
    /// il cast tiene la superficie pubblica comoda per i repository.</summary>
    public Postgrest.Table<T> From<T>() where T : Postgrest.Models.BaseModel, new()
        => (Postgrest.Table<T>)_postgrest.Table<T>();

    /// <summary>Chiamata a una funzione del database (create_space, join_space, …).</summary>
    public Task<T?> Rpc<T>(string nomeFunzione, Dictionary<string, object> parametri)
        => _postgrest.Rpc<T>(nomeFunzione, parametri);
}
```

- [ ] **Step 4: Creare `Services/SupabaseService.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Supabase.Gotrue;
using Postgrest = Supabase.Postgrest;

namespace Eton.Services;

/// <summary>
/// Provider di sessione e client Supabase: costruisce un <see cref="Client"/> Gotrue (auth) e un
/// <see cref="Postgrest.Client"/> (dati), e li espone dietro <see cref="SupabaseClient"/>.
///
/// Flusso di accesso: <b>PKCE</b>, non implicit. Google riporta un codice monouso in
/// <c>?code=</c>; lo si scambia con la sessione presentando il verificatore custodito da
/// <see cref="PkceStore"/>. Col flusso implicit l'access token arriverebbe nel fragment dell'URL,
/// cioè nella cronologia del browser e in ogni log che registri gli URL.
/// </summary>
public class SupabaseService
{
    private readonly Client _auth;
    private readonly Postgrest.Client _postgrest;
    private readonly SupabaseClient _facade;
    private readonly NavigationManager _navigation;
    private readonly BrowserSessionHandler _sessionHandler;
    private readonly PkceStore _pkce;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;
    private DateTime? _ultimoRefreshFallito;

    /// <summary>Messaggio dell'ultimo rifiuto del provider, letto da <c>Login.razor</c>.</summary>
    public string? ErroreAccesso { get; private set; }

    public SupabaseService(IConfiguration configuration, IJSRuntime js, NavigationManager navigation)
    {
        _navigation = navigation;

        var url = configuration["Supabase:Url"];
        var anonKey = configuration["Supabase:AnonKey"];

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(anonKey))
            throw new InvalidOperationException(
                "Supabase:Url e Supabase:AnonKey vanno valorizzati in wwwroot/appsettings.json.");

        _auth = new Client(new ClientOptions
        {
            Url = $"{url}/auth/v1",
            Headers = new Dictionary<string, string> { { "apikey", anonKey } },
        });

        _sessionHandler = new BrowserSessionHandler((IJSInProcessRuntime)js);
        _auth.SetPersistence(_sessionHandler);
        _pkce = new PkceStore((IJSInProcessRuntime)js);

        // Il token entra per-richiesta: così la RLS vede sempre quello valido. Si manda il Bearer
        // dell'utente SOLO se la sessione esiste e non è scaduta — un token scaduto verrebbe
        // rifiutato dal gateway (403 bad_jwt), mentre con l'anon key la richiesta arriva e viene
        // valutata dalle policy.
        _postgrest = new Postgrest.Client($"{url}/rest/v1", new Postgrest.ClientOptions())
        {
            GetHeaders = () =>
            {
                var session = _auth.CurrentSession;
                var bearer = session is not null && !session.Expired()
                    ? session.AccessToken
                    : anonKey;
                return new Dictionary<string, string>
                {
                    { "apikey", anonKey },
                    { "Authorization", $"Bearer {bearer}" },
                };
            },
        };

        _facade = new SupabaseClient(_auth, _postgrest);
    }

    /// <summary>
    /// Bootstrap idempotente e serializzato: ripristino da localStorage, scambio del codice PKCE
    /// se siamo appena tornati da Google, refresh se la sessione sta per scadere.
    /// Ogni chiamata dati passa di qui, quindi è anche il punto in cui si garantisce un token vivo:
    /// <c>GetHeaders</c> è sincrono e non potrebbe rinfrescare nulla.
    /// </summary>
    public async Task<SupabaseClient> GetClientAsync()
    {
        if (_initialized)
        {
            var corrente = _auth.CurrentSession;
            if (corrente is not null && SessionFreshness.VaRinfrescata(corrente.ExpiresAt(), DateTime.UtcNow))
                await RinnovaSessioneSeServeAsync();
            return _facade;
        }

        await _initLock.WaitAsync();
        try
        {
            if (!_initialized)
            {
                // 1) Sessione persistita (sincrono, nessuna rete) → si resta loggati al reload.
                _auth.LoadSession();

                // 2) Ritorno da Google?
                var esito = OAuthCallback.Analizza(_navigation.Uri);

                if (esito.Errore is not null)
                {
                    ErroreAccesso = esito.Errore;
                    _pkce.Cancella();
                }
                else if (esito.Codice is not null)
                {
                    await ScambiaCodiceAsync(esito.Codice);
                }
                else
                {
                    // Nessun ritorno OAuth: sessione ripristinata ma forse da rinfrescare.
                    // NIENTE in questo ramo può propagare un'eccezione, altrimenti l'app resta
                    // bloccata sul caricamento. Versione SENZA lock: siamo già dentro _initLock,
                    // che non è rientrante.
                    await RinnovaSessioneAsync();
                }

                _initialized = true;

                // 3) Ripulisce l'URL dai parametri OAuth, dopo aver marcato _initialized.
                if (esito.Codice is not null || esito.Errore is not null)
                    _navigation.NavigateTo(_navigation.BaseUri, forceLoad: false, replace: true);
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _facade;
    }

    /// <summary>Avvia l'accesso con Google: chiede l'URL del provider e ci porta il browser.</summary>
    public async Task AvviaAccessoGoogleAsync()
    {
        ErroreAccesso = null;

        var stato = await _auth.SignIn(Constants.Provider.Google, new SignInOptions
        {
            FlowType = Constants.OAuthFlowType.PKCE,
            RedirectTo = _navigation.BaseUri,
        });

        // Il verificatore deve sopravvivere al redirect: fra poco questa pagina non esisterà più.
        if (!string.IsNullOrEmpty(stato.PKCEVerifier))
            _pkce.Salva(stato.PKCEVerifier);

        // La libreria non redirige da sola in WebAssembly: forceLoad obbligatorio, altrimenti
        // il router di Blazor tratterebbe l'URL di Google come una rotta interna.
        _navigation.NavigateTo(stato.Uri.ToString(), forceLoad: true);
    }

    private async Task ScambiaCodiceAsync(string codice)
    {
        var verificatore = _pkce.Leggi();
        if (string.IsNullOrEmpty(verificatore))
        {
            ErroreAccesso = "Accesso non completato: riprova dall'inizio.";
            return;
        }

        try
        {
            var session = await _auth.ExchangeCodeForSession(verificatore, codice);
            if (session?.User is null)
                ErroreAccesso = "Accesso non completato: sessione senza utente.";
        }
        catch (Exception ex)
        {
            ErroreAccesso = $"Accesso non riuscito: {ex.Message}";
        }
        finally
        {
            // Monouso: si cancella comunque, riuscito o no.
            _pkce.Cancella();
        }
    }

    /// <remarks>
    /// Prende <see cref="_initLock"/>: da chiamare SOLO fuori dal lock.
    /// <see cref="SemaphoreSlim"/> non è rientrante — il bootstrap, che gira già dentro il lock,
    /// chiama <see cref="RinnovaSessioneAsync"/> direttamente.
    /// </remarks>
    private async Task RinnovaSessioneSeServeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            await RinnovaSessioneAsync();
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Rinnovo vero e proprio, senza prendere il lock. Ricontrolla da sé se serve (due chiamate
    /// concorrenti possono arrivare qui entrambe) e non propaga MAI eccezioni.
    /// Si usa l'overload a DUE argomenti di <c>RefreshToken</c>: quello senza argomenti si rifiuta
    /// di lavorare quando l'access token è già scaduto, cioè proprio nel caso per cui esiste.
    /// </summary>
    private async Task RinnovaSessioneAsync()
    {
        var session = _auth.CurrentSession;
        if (session is null
            || string.IsNullOrEmpty(session.AccessToken)
            || string.IsNullOrEmpty(session.RefreshToken)
            || !SessionFreshness.VaRinfrescata(session.ExpiresAt(), DateTime.UtcNow))
            return;

        if (!SessionFreshness.SiPuoRitentare(_ultimoRefreshFallito, DateTime.UtcNow))
            return;

        try
        {
            await _auth.RefreshToken(session.AccessToken, session.RefreshToken);
            _ultimoRefreshFallito = null;
            _auth.NotifyAuthStateChange(Constants.AuthState.SignedIn);
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (
            ex.Reason == Supabase.Gotrue.Exceptions.FailureHint.Reason.ExpiredRefreshToken
            || ex.Reason == Supabase.Gotrue.Exceptions.FailureHint.Reason.InvalidRefreshToken)
        {
            Console.Error.WriteLine($"[Auth] Refresh token non valido, eseguo il logout: {ex.Message}");
            await SignOutAsync();
        }
        catch (Exception ex)
        {
            // Rete assente, 5xx, timeout: NON sloggare. Il refresh token può essere ancora buono.
            Console.Error.WriteLine($"[Auth] Refresh sessione fallito, riprovo più avanti: {ex.Message}");
            _ultimoRefreshFallito = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Porta l'app a uno stato di logout pulito senza MAI propagare eccezioni.
    /// In Gotrue 6 <c>DestroySession()</c> è pubblico, quindi non serve più il giro tortuoso che
    /// in 4.2.7 sfruttava l'eccezione di <c>SetSession("","")</c> per ripulire lo stato interno.
    /// <c>SignOutScope.Local</c>: si esce da questo dispositivo senza revocare le sessioni altrove.
    /// </summary>
    public async Task SignOutAsync()
    {
        try
        {
            await _auth.SignOut(Constants.SignOutScope.Local);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] SignOut lato server fallito, procedo con la pulizia locale: {ex.Message}");
        }

        try
        {
            _auth.DestroySession();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] Pulizia sessione in memoria: {ex.Message}");
        }

        try
        {
            _sessionHandler.DestroySession();
            _pkce.Cancella();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Auth] Pulizia localStorage fallita: {ex.Message}");
        }
    }
}
```

- [ ] **Step 5: Registrare il servizio in `Program.cs`**

Aggiungere dopo la registrazione di `HttpClient`:

```csharp
builder.Services.AddSingleton<SupabaseService>();
```

e in cima `using Eton.Services;`.

- [ ] **Step 6: Verificare la compilazione**

```bash
dotnet build Eton.sln
```

Atteso: `Build succeeded`. Se il compilatore segnala che `Postgrest.ClientOptions` non ha
`Headers`, va bene: le intestazioni sono già fornite per-richiesta da `GetHeaders`, e il codice
sopra infatti non le imposta nelle opzioni.

- [ ] **Step 7: Commit**

```bash
git add Services/ Program.cs
git commit -m "Strato Supabase su Gotrue 6 con flusso PKCE"
```

---

### Task 5: Accesso con Google, guardia di rotta e logout

**Files:**
- Create: `Services/AuthStateService.cs`, `Shared/AuthRedirect.razor`, `Pages/Login.razor`
- Modify: `Program.cs`, `Layout/MainLayout.razor`, `Pages/Home.razor`, `wwwroot/appsettings.json`

**Interfaces:**
- Consumes: `SupabaseService.GetClientAsync()`, `.AvviaAccessoGoogleAsync()`, `.SignOutAsync()`,
  `.ErroreAccesso` (Task 4).
- Produces:
  - `AuthStateService.IsLoggedInAsync() → Task<bool>`
  - `AuthStateService.GetUserIdAsync() → Task<string?>`
  - `AuthStateService.GetEmailAsync() → Task<string?>`
  - `AuthStateService.GetDisplayNameAsync() → Task<string?>`
  - `AuthStateService.LogoutAsync() → Task`

- [ ] **Step 1: Creare `Services/AuthStateService.cs`**

```csharp
using Microsoft.AspNetCore.Components;

namespace Eton.Services;

/// <summary>
/// Identità dell'utente autenticato, letta dalla sessione Gotrue. Ogni metodo passa da
/// <see cref="SupabaseService.GetClientAsync"/>, che garantisce il bootstrap già avvenuto: le
/// pagine non devono sapere nulla dell'ordine di inizializzazione.
/// </summary>
public class AuthStateService
{
    private readonly SupabaseService _supabase;
    private readonly NavigationManager _navigation;

    public AuthStateService(SupabaseService supabase, NavigationManager navigation)
    {
        _supabase = supabase;
        _navigation = navigation;
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var client = await _supabase.GetClientAsync();
        return client.Auth.CurrentSession?.User is not null;
    }

    /// <summary>Id Gotrue dell'utente (<c>auth.users.id</c>): è l'<c>owner_id</c> di ogni risorsa.</summary>
    public async Task<string?> GetUserIdAsync()
    {
        var client = await _supabase.GetClientAsync();
        return client.Auth.CurrentSession?.User?.Id;
    }

    public async Task<string?> GetEmailAsync()
    {
        var client = await _supabase.GetClientAsync();
        return client.Auth.CurrentSession?.User?.Email;
    }

    /// <summary>Nome visualizzato: nome completo Google, con ripiego sull'email.</summary>
    public async Task<string?> GetDisplayNameAsync()
    {
        var client = await _supabase.GetClientAsync();
        var user = client.Auth.CurrentSession?.User;
        if (user is null) return null;

        if (user.UserMetadata is not null)
        {
            foreach (var chiave in new[] { "full_name", "name" })
            {
                if (user.UserMetadata.TryGetValue(chiave, out var valore)
                    && valore is string s && !string.IsNullOrWhiteSpace(s))
                {
                    return s;
                }
            }
        }

        return user.Email;
    }

    public async Task LogoutAsync()
    {
        await _supabase.SignOutAsync();
        _navigation.NavigateTo("login");
    }
}
```

- [ ] **Step 2: Creare `Shared/AuthRedirect.razor`**

```razor
@inject AuthStateService AuthState
@inject SupabaseService SupabaseService
@inject NavigationManager Navigation

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        // GetClientAsync esegue tutto il bootstrap (LoadSession + eventuale scambio del codice
        // PKCE): dopo questo await la sessione è risolta, in un senso o nell'altro.
        await SupabaseService.GetClientAsync();

        if (!await AuthState.IsLoggedInAsync())
            Navigation.NavigateTo("login", forceLoad: false, replace: true);
    }
}
```

- [ ] **Step 3: Creare `Pages/Login.razor`**

```razor
@page "/login"
@layout LoginLayout
@inject SupabaseService SupabaseService
@inject AuthStateService AuthState
@inject NavigationManager Navigation

<PageTitle>Accedi — Eton</PageTitle>

<div class="login-card">
    <h1>Eton</h1>
    <p class="sottotitolo">Note e collezioni, da soli o con gli altri.</p>

    @if (!string.IsNullOrEmpty(errore))
    {
        <div class="errore" role="alert">@errore</div>
    }

    <button type="button" class="btn primary" @onclick="Accedi" disabled="@occupato">
        @(occupato ? "Attendere…" : "Accedi con Google")
    </button>
</div>

@code {
    private string? errore;
    private bool occupato;

    protected override async Task OnInitializedAsync()
    {
        // Il bootstrap può essere già stato eseguito (ritorno da Google): se è andato male,
        // il motivo sta qui.
        await SupabaseService.GetClientAsync();
        errore = SupabaseService.ErroreAccesso;

        if (await AuthState.IsLoggedInAsync())
            Navigation.NavigateTo("", replace: true);
    }

    private async Task Accedi()
    {
        occupato = true;
        errore = null;
        try
        {
            await SupabaseService.AvviaAccessoGoogleAsync();
        }
        catch (Exception ex)
        {
            errore = $"Errore di accesso: {ex.Message}";
            occupato = false;
        }
    }
}
```

- [ ] **Step 4: Ripristinare `MainLayout.razor` e `Home.razor`**

Sostituire le versioni provvisorie del Task 1 Step 12 con quelle definitive degli Step 6 e 7.

- [ ] **Step 5: Registrare `AuthStateService` in `Program.cs`**

```csharp
builder.Services.AddSingleton<AuthStateService>();
```

- [ ] **Step 6: Aggiungere gli stili della schermata di accesso a `wwwroot/css/app.css`**

```css
.login-card {
    width: 100%;
    max-width: 380px;
    background: var(--superficie);
    border: 1px solid var(--bordo);
    border-radius: var(--raggio);
    padding: 2rem 1.5rem;
    text-align: center;
}

.login-card .sottotitolo { color: var(--testo-tenue); margin: 0 0 1.5rem; }
.login-card .btn { width: 100%; }
```

- [ ] **Step 7: Verificare la compilazione**

```bash
dotnet build Eton.sln
```

Atteso: `Build succeeded`.

- [ ] **Step 8: 🔴 AZIONE UMANA — creare il progetto Supabase**

Questo passo richiede il browser dell'utente e **non va eseguito da un agente**. Va spiegato in
chat, un passo per volta, aspettando l'esito di ciascuno:

1. creare un progetto su `supabase.com` (piano free, regione europea);
2. copiare *Project URL* e *anon public key* da Project Settings → API;
3. incollarli in `wwwroot/appsettings.json`;
4. Authentication → Sign In / Providers → **Google**: servono Client ID e Client Secret da Google
   Cloud Console (progetto → OAuth consent screen → Credentials → OAuth client ID di tipo
   *Web application*);
5. in Google Cloud, fra gli *Authorized redirect URIs*, va messo
   `https://<ref>.supabase.co/auth/v1/callback`;
6. in Supabase, Authentication → URL Configuration: `Site URL` e *Redirect URLs* devono includere
   **sia** `http://localhost:5xxx/` (la porta che usa `dotnet run`) **sia** l'URL di GitHub Pages.
   Senza `localhost` fra i redirect, ogni prova locale rimbalza sul sito pubblicato;
7. eseguire la migrazione del Task 2: SQL Editor → incollare
   `supabase/migrations/20260811000000_initial_schema.sql` → Run.

- [ ] **Step 9: Verificare l'accesso end-to-end**

```bash
dotnet run --project Eton.csproj
```

Sequenza attesa:
1. `/` rimanda a `/login`;
2. "Accedi con Google" porta alla schermata di Google;
3. al ritorno l'URL contiene `?code=…` per un istante, poi viene ripulito;
4. la Home mostra "Ciao, \<nome\>";
5. **ricaricando la pagina si resta autenticati** (sessione da `localStorage`);
6. "Esci" riporta a `/login`, e un ulteriore reload non rientra;
7. nell'SQL Editor di Supabase:
   `select p.display_name, s.name, s.is_personal from profiles p join spaces s on s.owner_id = p.id;`
   restituisce una riga con lo spazio `Personale`.

Se il punto 4 fallisce con "Accesso non completato: riprova dall'inizio", il verificatore PKCE non
è sopravvissuto al redirect: controllare che `eton.pkce` sia presente in `localStorage` subito
prima di lasciare l'app.

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "Accesso con Google, guardia di rotta e logout"
```

---

### Task 6: PWA e deploy su GitHub Pages

**Files:**
- Create: `wwwroot/manifest.webmanifest`, `wwwroot/service-worker.js`,
  `wwwroot/service-worker.published.js`, `wwwroot/404.html`, icone
- Create: `.github/workflows/deploy.yml`

**Interfaces:**
- Consumes: l'app funzionante del Task 5.
- Produces: sito installabile e pubblicato su `https://<utente>.github.io/eton/`.

- [ ] **Step 1: Creare `wwwroot/manifest.webmanifest`**

```json
{
  "name": "Eton",
  "short_name": "Eton",
  "id": "./",
  "start_url": "./",
  "scope": "./",
  "display": "standalone",
  "background_color": "#12171c",
  "theme_color": "#1f2933",
  "lang": "it",
  "dir": "ltr",
  "icons": [
    { "src": "icon-192.png", "type": "image/png", "sizes": "192x192" },
    { "src": "icon-512.png", "type": "image/png", "sizes": "512x512" },
    { "src": "icon-512-maskable.png", "type": "image/png", "sizes": "512x512", "purpose": "maskable" }
  ]
}
```

- [ ] **Step 2: Creare le icone**

Servono `favicon.png`, `icon-180.png`, `icon-192.png`, `icon-512.png`, `icon-512-maskable.png` in
`wwwroot/`. Per ora vanno bene icone generate a tinta unita col carattere "E": vanno sostituite
prima della pubblicazione sul Play Store, non prima.

- [ ] **Step 3: Creare `wwwroot/service-worker.js`** (sviluppo: non fa nulla, di proposito)

```javascript
// In sviluppo il service worker non deve mettere niente in cache: altrimenti si continua a
// vedere la versione precedente dell'app dopo ogni modifica. Quello vero è
// service-worker.published.js, che il publish mette al suo posto.
self.addEventListener('fetch', () => { });
```

- [ ] **Step 4: Creare `wwwroot/service-worker.published.js`**

```javascript
// Cache-first sugli asset dell'applicazione, network-first per tutto il resto.
// L'elenco degli asset lo genera il publish in service-worker-assets.js.
self.importScripts('./service-worker-assets.js');

self.addEventListener('install', event => event.waitUntil(onInstall()));
self.addEventListener('activate', event => event.waitUntil(onActivate()));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

// Sul sito pubblicato l'app vive sotto /eton/: la richiesta di navigazione va risolta con
// l'index.html di QUESTO scope, non con quello della radice del dominio.
const base = self.registration.scope.replace(self.location.origin, '');
const baseUrl = new URL(base, self.location.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall() {
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate() {
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }
    return cachedResponse || fetch(event.request);
}
```

- [ ] **Step 5: Creare `wwwroot/404.html`**

GitHub Pages non conosce le rotte di Blazor: senza questo file, un accesso diretto a `/eton/login`
restituisce 404. Rimandare tutto all'app:

```html
<!DOCTYPE html>
<html lang="it">
<head>
    <meta charset="utf-8" />
    <title>Eton</title>
    <script>
        sessionStorage.setItem('rotta-richiesta', location.pathname + location.search);
        location.replace('/eton/');
    </script>
</head>
<body></body>
</html>
```

E in `wwwroot/index.html`, subito prima del tag di chiusura `</body>`, ripristinare la rotta:

```html
<script>
    (function () {
        var rotta = sessionStorage.getItem('rotta-richiesta');
        if (rotta) { sessionStorage.removeItem('rotta-richiesta'); history.replaceState(null, '', rotta); }
    })();
</script>
```

- [ ] **Step 6: Creare `.github/workflows/deploy.yml`**

```yaml
name: Deploy su GitHub Pages

on:
  push:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: pages
  cancel-in-progress: true

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Publish
        run: dotnet publish Eton.csproj -c Release -o publish

      # L'app è servita da https://<utente>.github.io/eton/, non dalla radice del dominio:
      # senza questa riscrittura il browser cercherebbe _framework/ sulla radice e non lo troverebbe.
      - name: Correggi il base href
        run: sed -i 's|<base href="/" />|<base href="/eton/" />|g' publish/wwwroot/index.html

      # Senza .nojekyll, GitHub Pages scarta le cartelle che iniziano con l'underscore:
      # _framework, cioè tutto il runtime .NET.
      - name: Aggiungi .nojekyll
        run: touch publish/wwwroot/.nojekyll

      - uses: actions/upload-pages-artifact@v3
        with:
          path: publish/wwwroot

  deploy:
    needs: build
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - id: deployment
        uses: actions/deploy-pages@v4
```

- [ ] **Step 7: Verificare il publish in Release — il collaudo che conta**

```bash
cd /g/Sviluppo/Eton
dotnet publish Eton.csproj -c Release -o publish
```

Il nome del progetto è obbligatorio: senza, il CLI prende la soluzione, tira dentro i test e
affianca copie **non trimmate** al `wwwroot`, che maschererebbero proprio il difetto che si sta
cercando.

Poi servire `publish/wwwroot` e **fare l'accesso**: `dotnet build` non attiva il trimming, e la
sola schermata di login non esercita alcuna deserializzazione Gotrue. Il difetto si manifesta al
primo ritorno da Google, come `Unable to find a constructor to use for type …`.

```bash
npx --yes serve publish/wwwroot -l 5050
```

Atteso: accesso completato e Home con il nome utente, esattamente come in Debug.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "PWA: manifest, service worker, workflow di deploy su GitHub Pages"
```

- [ ] **Step 9: 🔴 AZIONE UMANA — creare il repository GitHub e attivare Pages**

Da spiegare in chat un passo per volta: creazione del repo `eton`, `git remote add origin`,
Settings → Pages → Source: **GitHub Actions**, e infine il push (che, da istruzione permanente,
si fa **solo** su richiesta esplicita dell'utente).

Dopo il primo deploy, aggiungere l'URL di GitHub Pages fra i *Redirect URLs* di Supabase.

---

## Self-Review

**Copertura rispetto alla spec** — questo piano copre §12.1 (fetta 1) e le parti di §3, §4, §5, §6,
§7 che le servono: architettura e pacchetti (Task 1), tabelle `profiles`/`spaces`/`space_members`,
tutte e sei le funzioni e le loro policy (Task 2), rotte `/login` e `/` (Task 5), PWA e hosting
(Task 6). **Restano scoperti e vanno ai piani successivi:** tabelle `notes`, `collections`,
`collection_items`, `reviews` e relative policy; `SpaceStateService`, `CurrentUserService`,
`AccessControl`; `BottomNav` e `SpaceSwitcher`; Markdig e `MarkdownRenderer`; `FieldSchema`,
`ItemDataMapper`, `RatingCalculations`; i repository; la concorrenza ottimistica; il progetto
`Eton.Tests.Integration` con i test RLS a due utenti. **Scelta deliberata**: i test RLS richiedono
almeno due tabelle con dati correlati per essere significativi, e arrivano con la fetta 2, dove
gli spazi diventano manipolabili dall'interfaccia.

**Segnaposto:** nessuno. Gli unici valori non presenti nel piano sono `Supabase:Url` e
`Supabase:AnonKey`, che per definizione esistono solo dopo che l'utente ha creato il progetto:
sono marcati come azione umana (Task 5 Step 8) e l'applicazione fallisce all'avvio con un
messaggio esplicito se mancano.

**Coerenza dei tipi:** `SupabaseService.GetClientAsync()` restituisce `SupabaseClient`, ed è così
che lo usano `AuthStateService` (`client.Auth.CurrentSession`) e `AuthRedirect`.
`OAuthCallback.Analizza` restituisce `OAuthCallbackEsito`, letto in `SupabaseService` come
`esito.Codice` / `esito.Errore` — gli stessi nomi definiti nel record.
`SessionFreshness.VaRinfrescata(DateTime, DateTime)` è invocata con
`session.ExpiresAt()` e `DateTime.UtcNow` in entrambi i punti d'uso.
`PkceStore.Salva/Leggi/Cancella` combaciano con le chiamate in `SupabaseService`.

**Rischio residuo dichiarato:** le firme di Gotrue 6.3.0 sono state estratte dalla documentazione
XML del pacchetto, non compilate. Se una discrepanza emerge al Task 4 Step 6, si corregge lì —
è esattamente il motivo per cui l'autenticazione è la prima fetta e non l'ultima.
