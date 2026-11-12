# Analisi di sicurezza di base — 12 novembre 2026

**Natura:** analisi in sola lettura. Nessun codice modificato, nessuna query su dati di produzione, nessuna
prova eseguita contro un database. Ogni rilievo cita la riga; dove una conclusione dipende dal comportamento di
PostgreSQL, PostgREST o Supabase è detto se viene dalla documentazione (con URL) o è dedotta.

**Perimetro:** le dieci migrazioni di `supabase/migrations/` (lette in ordine, controllando quale definizione di
policy è in vigore), gli script `supabase/verifica-rls*.sql`, `supabase/config.toml`, autenticazione e sessione
(`Services/SupabaseService.cs`, `BrowserSessionHandler.cs`, `AuthStateService.cs`, `OAuthCallback.cs`,
`PkceChallenge.cs`, `PkceStore.cs`, `RottaRichiesta.cs`), il rendering di contenuto utente
(`Services/MarkdownRenderer.cs`, ogni `MarkupString`), ciò che arriva al browser (`Eton.csproj`, `wwwroot/`), la
pubblicazione (`.github/workflows/deploy.yml`), i segreti nel repository e nella sua storia git (281 commit su
tutti i rami), gli spazi condivisi.

**Metodo:** due revisioni di sicurezza indipendenti (database; client e pubblicazione), istruttoria di ogni
rilievo riga per riga, verifica sulla documentazione di PostgreSQL 17 e PostgREST v12 per il rilievo S3, lettura
diretta del codice per i rilievi su dati e accessi (`initial_schema.sql`, `spese.sql`, `note.sql`,
`MarkdownRenderer.cs`).

**Sintesi:** il modello regge. Ogni tabella ha la RLS, `anon` non ha privilegi su nessuna tabella, le colonne
d'identità (`space_id`, `owner_id`, `paid_by`, …) non sono aggiornabili, non c'è modo di iscriversi da soli a uno
spazio né di promuoversi, nessuna XSS trovata, nessun segreto nella storia git. Un rilievo **alto**: l'espulsione
da uno spazio condiviso non è efficace, perché il codice d'invito non si può cambiare. Nessun rilievo critico.

---

## Rilievi, dal più grave

### S1 — ALTA · Un membro espulso rientra con lo stesso codice d'invito, che il proprietario non può cambiare

**Cosa.** Il codice d'invito è l'unica chiave d'ingresso a uno spazio condiviso (lo dice il commento a
`supabase/migrations/20260811000000_initial_schema.sql:105`). Ogni membro lo legge, nessuno lo può cambiare, e
`join_space` fa entrare chiunque lo conosca. Quindi «Rimuovi» in `Pages/SpaceDetail.razor:111` toglie l'accesso
solo finché l'espulso non richiama `join_space`.

**Dove.**
- `initial_schema.sql:240` — `for select using (public.is_space_member(id));` (ogni membro legge la riga di
  `spaces`, `invite_code` compreso; `grant select` a riga 310).
- `initial_schema.sql:311` — `grant update (name) on public.spaces to authenticated;` (il codice è immutabile; in
  nessuna migrazione c'è una funzione che lo rigeneri: l'unica scrittura è in `create_space`, riga 147-148).
- `initial_schema.sql:169` e `173-175` — `where invite_code = upper(btrim(p_code)) and not is_personal;` seguito
  dall'inserimento della membership: nessuna lista di esclusi.
- `Pages/SpaceDetail.razor:78` e `84` — `@if (!string.IsNullOrEmpty(spazio.InviteCode))` e
  `<code class="codice-grande">@spazio.InviteCode</code>`: l'interfaccia mostra il codice, con il pulsante Copia,
  a **ogni** membro, non solo al proprietario.

**Scenario d'attacco.**
1. Alice (proprietaria) condivide lo spazio «Casa» con Bruno, che vede il codice in pagina.
2. I due litigano; Alice espelle Bruno (`DELETE /rest/v1/space_members?space_id=eq.S&user_id=eq.B`, consentito da
   `space_members_delete`, `initial_schema.sql:266-271`). L'espulsione ha effetto subito: `is_space_member` legge la
   tabella a ogni richiesta.
3. Bruno apre «Unisciti a uno spazio» nell'app e reincolla il codice (oppure `POST /rest/v1/rpc/join_space`).
   Rientra, e torna a leggere spese, entrate, rate, budget e note di Alice, e a scriverne di nuove.
4. Variante: un membro qualunque passa il codice a un terzo; Alice non ha modo di invalidarlo se non cancellando
   lo spazio, e con esso tutti i dati.

**Correzione proposta.**
- Una funzione `rotate_invite_code(p_space uuid)` SECURITY DEFINER, `search_path` fissato, che controlla
  `is_space_owner(p_space)` e riassegna `generate_invite_code()`; un pulsante «Cambia codice» per il proprietario.
- Rotazione automatica a ogni espulsione (nella stessa funzione che rimuove il membro, oppure in un trigger AFTER
  DELETE su `space_members` quando a cancellare non è l'utente stesso). È ciò che rende l'espulsione efficace.
- Mostrare il codice solo al proprietario (condizione `sonoProprietario` a `SpaceDetail.razor:78`) e, a livello di
  database, toglierlo dalla lettura dei membri: privilegio di colonna su `spaces` senza `invite_code`, più una
  funzione `invite_code_of(p_space)` riservata al proprietario.
- In alternativa o in aggiunta: scadenza del codice, oppure una tabella `space_bans(space_id, user_id)` controllata
  da `join_space`.
- Un caso in `verifica-rls.sql`: espulso che rientra (atteso: no), membro non proprietario che legge il codice
  (atteso: no).

### S2 — MEDIA · Nessuna politica di contenuto (CSP), con i token di sessione in `localStorage`

**Cosa.** La sessione Supabase (token di accesso e token di rinnovo) sta in `localStorage`, leggibile da
qualunque script giri sull'origine `github.io/eton`. Oggi l'unica barriera contro uno script iniettato è il filtro
del Markdown, che è corretto (vedi «Verificato e a posto»), ma è l'unico strato: se un aggiornamento di Markdig o
una modifica futura aprisse un varco, nulla impedirebbe allo script di leggere i token e mandarli fuori.

**Dove.**
- `Services/BrowserSessionHandler.cs:39` —
  `_js.InvokeVoid("localStorage.setItem", StorageKey, JsonConvert.SerializeObject(session));`
- `wwwroot/index.html`, `<head>`: nessun `<meta http-equiv="Content-Security-Policy">`, nessun
  `<meta name="referrer">`. GitHub Pages non permette header HTTP personalizzati, ma una CSP in `<meta>` sì.
- Unico punto di HTML non codificato: `Shared/MarkdownView.razor:29` —
  `reso = (MarkupString)Markdown.InHtml(Testo);`, usato solo da `Pages/NoteEdit.razor:68`.

**Scenario d'attacco (con un varco ipotetico nel filtro).** Un membro scrive una nota con un contenuto che supera
il filtro; un altro membro la apre; lo script legge `localStorage["eton.session"]` e invia il token di rinnovo a un
server esterno con `fetch`; l'attaccante ha accesso persistente ai dati economici della vittima finché il token
vale.

**Perché media e non alta:** nessun varco concreto trovato. È difesa in profondità sul punto più prezioso
(il token), e costa poco.

**Correzione proposta.** Una CSP in `<meta>` come primo elemento del `<head>`, da provare a mano su una copia
prima di pubblicarla, perché Blazor WebAssembly è esigente su `script-src`. Punto di partenza:
`default-src 'self'; script-src 'self' 'wasm-unsafe-eval' 'sha256-…'; style-src 'self' 'unsafe-inline';
img-src 'self' data: https://lh3.googleusercontent.com; connect-src 'self' https://fdqedhgvpneuybtykamf.supabase.co;
font-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'`. I tre script inline di `index.html`
(rimbalzo SPA righe 7-22, `importmap` riga 59, registrazione del service worker righe 86-139) vanno coperti da hash
o spostati in file. `connect-src` limitato a Supabase è ciò che impedisce l'esfiltrazione; `img-src` ristretto
chiude anche S5. Aggiungere `<meta name="referrer" content="strict-origin-when-cross-origin">` o `no-referrer`.
`frame-ancestors` non funziona in `<meta>`: la protezione dal clickjacking su GitHub Pages resta fuori portata.

### S3 — BASSA · Un ex membro conserva, sulla carta, il diritto di modificare e cancellare le righe che aveva scritto

**Cosa.** Le policy di UPDATE e DELETE di sette tabelle controllano l'autore della riga o il proprietario dello
spazio, ma non che chi scrive sia **ancora** membro. Nessuna è ridefinita da migrazioni successive.

| Tabella | UPDATE | DELETE | Condizione |
|---|---|---|---|
| `notes` | `20260812000000_note.sql:97-99` | `:102-103` | `owner_id = auth.uid() or is_space_owner(space_id)` |
| `collections` | `20260812120000_collections.sql:177-179` | `:182-183` | `owner_id = auth.uid() or is_space_owner(space_id)` |
| `collection_items` | `20260812120000_collections.sql:200-202` | `:205-206` | `added_by = auth.uid() or is_space_owner(space_id)` |
| `expenses` | `20260824000000_spese.sql:113-115` | `:118-119` | `paid_by = auth.uid() or is_space_owner(space_id)` |
| `recurring_expenses` | `20260922000000_ricorrenti.sql:171-173` | `:176-177` | `paid_by = auth.uid() or is_space_owner(space_id)` |
| `category_budgets` | `20261024000000_budget.sql:88-90` | `:92-93` | `created_by = auth.uid() or is_space_owner(space_id)` |
| `reviews` | `20260812200000_recensioni.sql:128-130` | `:133-134` | `user_id = auth.uid()` |

**Perché è bassa.** In PostgreSQL le policy di SELECT si sommano a quelle di UPDATE/DELETE quando il comando legge
colonne della riga, cioè quando c'è una WHERE o una RETURNING (doc PG 17, https://www.postgresql.org/docs/17/sql-createpolicy.html,
nota alla tabella delle policy per comando). Ogni SELECT pretende `is_space_member(space_id)`, quindi un
`PATCH /rest/v1/expenses?id=eq.X` dell'ex membro non trova la riga. Resta solo il comando senza alcun filtro
(`PATCH /rest/v1/expenses` col corpo `{"amount":99999}`), che toccherebbe tutte le sue righe ovunque: PostgREST di
suo lo accetta (https://docs.postgrest.org/en/v12/references/api/tables_views.html), ma su Supabase il ruolo che
serve le API ha l'estensione `pg-safeupdate`, che rifiuta UPDATE e DELETE senza WHERE. Quest'ultimo punto viene da
una fonte secondaria, una PR dei manutentori (https://github.com/supabase/postgres/pull/2105), non dalla
documentazione ufficiale. Il varco è quindi teorico finché safeupdate è attivo; la regola però non lo dice, e non
c'è uno script che lo provi.

**Scenario d'attacco (se safeupdate mancasse).** Bruno esce o viene espulso; con il suo JWT invia un PATCH senza
filtro e riscrive gli importi di tutte le spese che aveva registrato nello spazio, falsando totali e budget degli
ex coinquilini; oppure le cancella tutte con un DELETE senza filtro.

**Correzione proposta.** Aggiungere `public.is_space_member(space_id) and (…)` in `USING` e `WITH CHECK` di tutte
le policy della tabella (costo: una chiamata in più per riga scritta). Provarlo sul Supabase locale con un caso nuovo
negli script: ex membro che fa UPDATE e DELETE senza filtro e con filtro (atteso: zero righe).

### S4 — BASSA · Nessun limite di dimensione su alcuni campi, con registrazione aperta a chiunque

**Cosa.** Chiunque abbia un account Google entra in Eton (la registrazione è aperta per necessità) e può scrivere
nel proprio spazio personale. Alcune colonne non hanno un tetto in byte: un attaccante può riempire il database del
piano gratuito e fermare l'app per tutti.

**Dove.**
- `20260812120000_collections.sql:76` — `data jsonb not null default '{}'::jsonb,`; il CHECK a riga 83 controlla
  solo la forma (`jsonb_typeof(data) = 'object'`).
- `collections.sql:28` e `41-45` — `fields` ha un tetto di 40 elementi, ma non sulla dimensione di ciascuno.
- `initial_schema.sql:17-18` — `display_name text,` e `avatar_url text,` senza CHECK, scrivibili dall'utente
  (riga 303) e letti dai co-membri.
- `create_space` (`initial_schema.sql:138`) senza limite al numero di spazi per utente.
- Per confronto, altre colonne un tetto ce l'hanno: `notes.body` ≤ 100000 (`note.sql:23`), `image_url` ≤ 2000
  (`collections.sql:75`), `reviews.comment` ≤ 4000.

**Scenario d'attacco.** Un account nuovo scrive in ciclo `POST /rest/v1/collection_items` con `data` da qualche
megabyte nel proprio spazio personale, o crea migliaia di spazi; il database raggiunge la quota del piano gratuito.
L'unico limite di fatto è la dimensione massima della richiesta del gateway, che non è nel repository.

**Correzione proposta.** CHECK su `octet_length(data::text)`, su `octet_length(fields::text)`, su
`length(display_name)` e `length(avatar_url)`; un tetto al numero di spazi per proprietario (trigger o controllo in
`create_space`). Un limite per utente sul numero di righe è più costoso e può aspettare la soglia del rilascio
pubblico.

### S5 — BASSA · Un membro può far caricare un indirizzo a sua scelta al browser degli altri membri

**Cosa.** Non è esecuzione di codice (`javascript:` dentro `<img src>` non esegue nulla), ma un tracciamento: chi
controlla l'indirizzo vede IP, browser e ora in cui un altro membro apre la pagina.

**Dove.**
- `Pages/SpaceDetail.razor:98` — `<img class="avatar" src="@m.Avatar" alt="" referrerpolicy="no-referrer" />`,
  **senza** il filtro `UrlAmmessa`; `m.Avatar` è `profiles.avatar_url` (`Services/SpaceRepository.cs:125`), che
  l'utente può riscrivere a mano (`initial_schema.sql:303`).
- `Services/MarkdownRenderer.cs:67-69` e `137` — le immagini Markdown `![](url)` sono `LinkInline` e passano il
  filtro se `http`/`https`, quindi una nota può contenere un'immagine esterna. Dedotto dal comportamento standard di
  Markdig: nessun test copre un'immagine `https`.
- `Pages/CollectionDetail.razor:131` e `Pages/ItemEdit.razor:90` — `image_url` filtrato con `UrlAmmessa`
  (`CollectionDetail.razor:411`, `ItemEdit.razor:88`), che però ammette qualunque host.

**Scenario d'attacco.** Un membro appena entrato imposta via API il proprio `avatar_url` a
`https://attaccante.example/p.gif`, oppure scrive una nota con quell'immagine; ogni volta che un co-membro apre
l'elenco dei membri o la nota, il suo browser contatta il server dell'attaccante, che sa quando e da dove.

**Correzione proposta.** CHECK `avatar_url ~ '^https://[a-z0-9-]+\.googleusercontent\.com/'` (o nullo) e filtro
analogo in `SpaceDetail.razor:98`; `img-src` ristretto nella CSP di S2 (che chiude anche le immagini delle note e
delle collezioni). Per le immagini Markdown verso host esterni, valutare se renderle come link invece che come
`<img>`; per `image_url` delle collezioni l'immagine esterna è la funzione stessa, quindi lì basta
`referrerpolicy="no-referrer"`.

### S6 — BASSA · Il workflow di pubblicazione concede permessi di scrittura anche al job di build, con azioni non fissate

**Dove.** `.github/workflows/deploy.yml:8-11` — `permissions:` con `pages: write` e `id-token: write` a livello di
workflow, quindi anche nel job `build`, che esegue `dotnet publish` col ripristino dei pacchetti NuGet. Azioni per
tag: `actions/checkout@v7` (riga 21), `actions/setup-dotnet@v6` (23), `actions/upload-pages-artifact@v5` (46),
`actions/deploy-pages@v5` (58).

**Scenario d'attacco.** Un pacchetto NuGet compromesso o un tag spostato esegue codice nel job di build con un
token in grado di pubblicare su Pages: script arbitrari sull'origine che custodisce i token degli utenti (S2).
Probabilità bassa (azioni ufficiali `actions/*`, pochi pacchetti), impatto alto.

**Correzione proposta.** `permissions: contents: read` a livello di workflow, `pages: write` e `id-token: write`
solo nel job `deploy`; azioni fissate allo SHA del commit; file di lock dei pacchetti
(`RestorePackagesWithLockFile` più `--locked-mode`).

### S7 — BASSA · Il «voto al buio» si aggira

**Cosa.** Il voto al buio nasconde i voti altrui finché non si è votato, ma non resiste a un membro che vuole
barare. Lato lettura il meccanismo è corretto.

**Dove.**
- `20260812230000_voto_al_buio.sql:95-97` — `or public.has_reviewed(item_id)      -- hai votato: si apre`;
  `has_reviewed` (righe 54-55) conta qualunque riga, anche un solo commento.
- `20260812200000_recensioni.sql:155` — `grant update (rating, comment) on public.reviews to authenticated;`: il
  voto resta modificabile dopo aver letto gli altri.
- `voto_al_buio.sql:109` — `grant update (name, icon, fields, rating_max, blind) on public.collections to authenticated;`
  con `collections_update` (`collections.sql:178`): l'autore della collezione o il proprietario dello spazio può
  spegnere `blind`, leggere tutto e riaccenderlo.

**Scenario.** Bruno lascia un voto segnaposto, legge i voti altrui, corregge il proprio; oppure l'autore della
collezione spegne e riaccende il voto al buio.

**Correzione proposta.** Se il voto al buio deve essere una garanzia: voto non modificabile dopo la prima lettura,
oppure rivelazione solo a chiusura; `blind` modificabile solo finché la collezione non ha recensioni. Se è una
convenzione fra amici, dirlo nel commento della migrazione e chiudere il rilievo. Non riguarda i dati economici.

### S8 — BASSA · L'uscita può non revocare la sessione lato server

**Dove.** `Services/SupabaseService.cs:327` — `await _auth.SignOut(Constants.SignOutScope.Local);`, in un
`try/catch` (righe 329-332) che registra l'errore e procede con la sola pulizia locale.

**Scenario.** Senza rete, o con un token di accesso già scaduto, la chiamata `/auth/v1/logout` fallisce: il
browser cancella la sessione, ma il token di rinnovo resta valido sul server. È un rischio solo se il token era già
stato copiato (dispositivo condiviso, backup del profilo, S2). Che Supabase rifiuti la revoca con un token scaduto
è dedotto, non provato.

**Correzione proposta.** Tentare il rinnovo prima del `SignOut`; impostare nel pannello di Supabase una durata
massima della sessione e un limite d'inattività (sezione Auth → Sessions, disponibilità sul piano gratuito da
verificare).

### Note informative (non rilievi)

- **I `.pdb` forse pubblicati.** `Eton.csproj` non imposta `DebugType` in Release, e
  `wwwroot/service-worker.published.js:25` mette `/\.pdb$/` fra gli asset offline. La build Release locale li
  produce (`bin/Release/net10.0/wwwroot/_framework/Eton.*.pdb`); che il `dotnet publish` del workflow li includa
  non è verificato. Oggi l'impatto è nullo (il codice è pubblico); conta alla soglia del rilascio.
- **L'email come nome mostrato.** `initial_schema.sql:189-191` e `Services/IdentitaGoogle.cs:25` usano l'email se
  mancano `full_name` e `name`; il profilo è leggibile ai co-membri (`initial_schema.sql:226`). Con Google il nome
  c'è quasi sempre.
- **`pg_temp` nel `search_path`.** Le funzioni SECURITY DEFINER usano `set search_path = public` senza
  `pg_temp` in coda, come raccomanda PostgreSQL. Non sfruttabile via PostgREST (servirebbe una connessione SQL
  diretta); da aggiungere quando si ritoccano.
- **Funzioni booleane raggiungibili da `anon`.** `is_space_member`, `is_space_owner`, `is_space_owner_of`,
  `shares_space_with`, `space_is_personal` non sono revocate (`initial_schema.sql:331-334`, scelta motivata): a chi
  conosce un UUID rispondono sì/no. Impatto trascurabile.
- **Il codice d'invito resiste alla forza bruta:** 31^8 ≈ 8,5·10^11 combinazioni da `gen_random_bytes` con
  campionamento senza distorsione (`initial_schema.sql:109-133`); `join_space` non limita i tentativi, ma a quella
  dimensione non serve.
- **Budget «occupato».** L'indice unico sul budget aperto per categoria permette a un membro di occupare una
  categoria; solo lui o il proprietario la libera. Fastidio, non accesso.
- **Dati di lavoro nel repository pubblico.** I documenti di `handoff/` e `storico/handoff/` citano id di righe e
  di uno spazio di produzione e descrivono l'ambiente (lo sviluppo punta alla produzione, nessun account di
  collaudo). Gli id da soli non danno accesso (RLS); sono informazioni per un attaccante. La storia git espone
  l'indirizzo email personale dell'autore dei commit.

---

## Verificato e trovato a posto

**Segreti e storia git**
- In tutta la storia (281 commit, `git log --all -G`) c'è un solo JWT: la chiave pubblica di
  `wwwroot/appsettings.json`, introdotta col commit `62723f1`, con payload `"role":"anon"`. Nessuna chiave
  `service_role`, `sb_secret_`, token GitHub, chiave Google o segreto OAuth. L'unica stringa di connessione
  Postgres è quella del database locale (`postgres:postgres@127.0.0.1:54322`, commit `e061099`).
- Nessun file con un nome sensibile (`.env`, `.pem`, credenziali) è mai stato tracciato; `supabase/.temp` e
  `supabase/.branches` sono ignorati. `config.toml` ha solo `env(...)` o valori vuoti.

**Database**
- RLS abilitata su tutte le 11 tabelle: `initial_schema.sql:219-221`, `note.sql:78`, `collections.sql:159-160`,
  `recensioni.sql:108`, `spese.sql:92`, `ricorrenti.sql:156`, `budget.sql:74`. Nessuna policy `using (true)`,
  nessuna vista (quindi nessun problema di `security_invoker`).
- `anon` non ha privilegi su nessuna tabella: `initial_schema.sql:297` e un `revoke all … from anon, authenticated`
  in ogni migrazione prima dei grant minimi.
- Nessuna iscrizione fai-da-te: nessun INSERT su `spaces` e `space_members`
  (`initial_schema.sql:310`, `315`); si entra solo da `create_space` e `join_space`, che rifiutano chi non è
  autenticato (righe 143, 163) e sono revocate ad `anon` (326-329). `generate_invite_code` è revocata a tutti (323).
- Nessuna auto-promozione: il ruolo non esiste in `space_members` (riga 37); `owner_id` non è aggiornabile (311) e
  la WITH CHECK di `spaces_update` lo vincola comunque (248). La riga del proprietario non si cancella (269), lo
  spazio personale non si lascia né si cancella (252, 268), `is_personal` non è aggiornabile.
- Nessuno spostamento di righe fra spazi: nessuna colonna di legame nei grant di UPDATE (es. `spese.sql:152-153`,
  `note.sql:129`, `budget.sql:107`, `recensioni.sql:155`), trigger BEFORE UPDATE che rimettono i valori vecchi
  (`spese.sql:75-77`), chiavi esterne composite che legano figlio e padre allo stesso spazio
  (`collections.sql:85`, `recensioni.sql:62`).
- Nessun autore falsificato: ogni policy di INSERT pretende autore `= auth.uid()` e appartenenza allo spazio
  (es. `note.sql:90`, `spese.sql:106`; la versione in vigore per le spese, più stretta, è in `ricorrenti.sql:187`).
- `version`, `updated_at`, `created_at` li calcola il database, non il client (`spese.sql:64-81`).
- Funzioni SECURITY DEFINER tutte con `search_path` fissato; `review_counts` controlla l'appartenenza al proprio
  interno (`voto_al_buio.sql:78`) e restituisce solo conteggi.

**Autenticazione e sessione**
- PKCE corretto: verificatore da 32 byte di `RandomNumberGenerator` (`PkceChallenge.cs:24`), sfida S256
  (`PkceChallenge.cs:30`, `SupabaseService.cs:188`), verificatore cancellato dopo lo scambio (`SupabaseService.cs:240`),
  flusso `pkce` senza token nell'URL (`SupabaseService.cs:186`), URL ripulito dopo lo scambio (`SupabaseService.cs:144`).
  Un `?code=` fatto aprire alla vittima non si scambia senza il suo verificatore.
- `redirect_to` fisso sulla base dell'app (`SupabaseService.cs:189`); nessun redirect aperto al ritorno dal login:
  `RottaRichiesta.cs:45-48` accetta solo percorsi che iniziano con `/` e senza `:`, la rotta si consuma alla lettura
  (riga 27).
- I messaggi d'errore del provider non arrivano in pagina (`OAuthCallback.cs:55-67`); i token non finiscono mai nei
  log.
- Errori di rinnovo definitivi portano all'uscita, quelli di rete no (`SupabaseService.cs:291-297`); con sessione
  scaduta si manda la chiave pubblica, mai un JWT scaduto (riga 70).

**Contenuto utente**
- Un solo `MarkupString` reale (`Shared/MarkdownView.razor:29`); nessun `innerHTML`, `document.write` o `eval` in
  `wwwroot/js/`. Tutto il resto passa dalla codifica di Razor, in attributi quotati.
- Markdown: HTML grezzo disattivato (`MarkdownRenderer.cs:24`), estensioni scelte a mano senza `UseMediaLinks`
  (righe 20-30), bonifica sull'albero sia dei link sia degli autolink (67-91), schema giudicato dopo aver tolto spazi
  e caratteri di controllo (125), solo `http`, `https`, `mailto` o relativi (137). I test coprono `javascript:` in
  varie forme, `data:` e `vbscript:`.
- Esportazione CSV protetta dall'iniezione di formule (`Services/CalcoliGriglia.cs:90`).

**Browser e pubblicazione**
- Nessuno script da domini esterni; font locali.
- `wwwroot/404.html` ricostruisce l'indirizzo con host fisso e non lo scrive nel DOM.
- Il service worker mette in cache solo gli asset dell'app, mai le risposte di Supabase: nessun dato economico
  resta nella cache del browser.
- Il workflow parte solo da `push` su `main` e da avvio manuale (niente `pull_request_target`), non usa né stampa
  segreti, pubblica solo `publish/wwwroot`.

## Non verificato (fuori dalla portata di un'analisi in sola lettura)

- **La configurazione di produzione di Supabase.** `config.toml` vale solo per il database locale. Da controllare
  nel pannello: che sia attivo solo il provider Google (accesso con email e password spento, oppure con conferma
  obbligatoria), l'elenco degli indirizzi di ritorno ammessi, durata e inattività delle sessioni, che gli accessi
  anonimi siano spenti.
- **Che lo schema di produzione coincida con le migrazioni del repository.** Lo si accerterebbe solo con una query
  in produzione, esclusa dal mandato.
- **Che `pg-safeupdate` sia attivo sul ruolo delle API in produzione** (S3).
- **Che il `dotnet publish` del workflow includa i `.pdb`.**
- **La resa nel browser di un'immagine Markdown esterna** (S5) e il funzionamento della CSP proposta (S2).

## Lacune degli script `verifica-rls*.sql`

Coprono bene i privilegi di colonna, l'isolamento fra membro ed estraneo e i trigger di versione. Non provano:
l'espulso che rientra (S1); un membro non proprietario che legge `invite_code` (S1); l'ex membro che scrive o
cancella (S3); `avatar_url` arbitrario o lungo (S4, S5); `profiles_insert` e `profiles_update` su un id altrui;
`anon` su `create_space`, `join_space` e sulle funzioni booleane; `join_space` con codice errato, di uno spazio
personale o in minuscolo; il voto segnaposto e lo spegnimento di `blind` da parte dell'autore (S7); l'inserimento di
un figlio con `collection_id` o `item_id` di un altro spazio. Inoltre girano con `set local role` e non attraverso
PostgREST con JWT veri, e si leggono a occhio (lo dice `verifica-rls.sql:22-23`).

---

## Lista per la soglia del rilascio pubblico

Chiudere il repository non nasconde l'app: la DLL compilata (`_framework/Eton.dll`) arriva comunque al browser di
chiunque, e si decompila. Il bundle rivela nomi di tabelle e colonne (attributi `[Table]` e `[Column]` dei modelli),
le funzioni `create_space`, `join_space`, `review_counts`, le chiavi di `localStorage` e l'identificativo del
progetto Supabase. I controlli lato client (`Services/Permessi.cs`, `Shared/AuthRedirect.razor`) servono
all'interfaccia e si saltano chiamando le API a mano. **La RLS resta l'unica difesa, con o senza codice chiuso.**

Prima del rilascio pubblico:

1. Chiudere S1 (rotazione del codice, codice visibile al solo proprietario) e S3 (`is_space_member` nelle policy di
   scrittura), con i casi mancanti negli script di verifica.
2. CSP in `index.html` (S2), provata a mano su una copia.
3. Limiti di dimensione e di numero (S4): con il pubblico la registrazione aperta diventa un rischio concreto per il
   piano gratuito.
4. Filtro di `avatar_url` e `img-src` (S5).
5. Permessi del workflow per job, azioni fissate a SHA, file di lock dei pacchetti (S6).
6. Durata massima e inattività delle sessioni nel pannello di Supabase (S8); solo provider Google.
7. Togliere i `.pdb` dal pubblicato (`<DebugType>none</DebugType>` in Release, `/\.pdb$/` via dal service worker).
8. Decidere cosa esce dal repository prima di chiuderlo o di lasciarlo pubblico: `handoff/` e `storico/handoff/` con
   id di produzione e descrizione dell'ambiente; l'email personale nella storia git (si toglie solo riscrivendo la
   storia, gesto da decidere a parte).
9. Un progetto Supabase separato per lo sviluppo: oggi lo sviluppo punta alla produzione
   (`storico/handoff/17-collaudo/ambiente.md:33`), quindi ogni prova lascia o tocca dati veri.
10. Portare gli script `verifica-rls*.sql` in test automatici che passano da PostgREST con JWT veri
    (già previsto da `verifica-rls.sql:22`, che li destinava a `Eton.Tests.Integration`), così una regola rotta da una migrazione futura si vede prima della
    pubblicazione.
11. Ripetere questa analisi sullo schema in vigore a quella data, confrontandolo con la produzione.
