BRIEF implementer 1 — migrazione descrizione e periodo dei budget, più script di collaudo

PROGETTO: nessuno (il repository non ha CLAUDE.md di progetto; le convenzioni sono quelle dei file di riferimento qui sotto)

FILE DA CREARE (solo questi due; nient'altro si tocca):
  A. G:\Sviluppo\Eton\supabase\migrations\20261120000000_budget_periodo.sql
  B. G:\Sviluppo\Eton\supabase\verifica-budget-periodo.sql

NON TOCCARE: ogni altra migrazione, ogni altro script `supabase/verifica-*.sql`, `Models/`, `Services/`, `Pages/`,
`Shared/`, `wwwroot/`, `Eton.Tests/`, `docs/`, `handoff/`. Non eseguire nulla sul database: la prova la fa la sessione.

PATTERN DA RISPECCHIARE (leggili prima di scrivere):
  - stile della testata, commenti in italiano, idempotenza: supabase/migrations/20261024000000_budget.sql:1-8
  - tabella e vincoli esistenti di category_budgets (ends_on = primo giorno dell'ULTIMO mese valido, null = aperto):
    20261024000000_budget.sql:11-38
  - controllo preliminare in un blocco DO che fa `raise exception` con messaggio chiaro, poi drop/add del vincolo:
    20261024000000_budget.sql:114-129
  - forma minima dei grant di colonna, insert e update affiancati: supabase/migrations/20260903000000_grant_insert_blind.sql:15-24
  - grant oggi in vigore su category_budgets (il ruolo che scrive amount è `authenticated`): 20261024000000_budget.sql:96-106
  - estensioni nello schema `extensions`: 20260811000000_initial_schema.sql:11-12
  - script di collaudo: supabase/verifica-rls-budget.sql per intero (testata col numero esatto di errori attesi,
    \set ON_ERROR_STOP off, claims, utenti finti in auth.users, una transazione con rollback per ogni errore,
    \echo con l'atteso per sezione, pulizia con cancellazione dello spazio, riepilogo finale; SENZA accenti nel file B)

FILE A — contenuto richiesto, in quest'ordine:
  Testata come quella della 2.4: cosa fa, idempotente e rieseguibile, additiva (il client pubblicato non legge né
  scrive description: il default '' copre i suoi INSERT), dipende da 20261024000000_budget.sql. Dire in testata che il
  BLOCCO II si può togliere per intero senza toccare il BLOCCO I.

  BLOCCO I (intestazione commento ben visibile, «BLOCCO I — descrizione»):
    alter table public.category_budgets add column if not exists description text not null default '';
    drop constraint if exists + add constraint category_budgets_description_max check (char_length(description) <= 200);
    grant insert (description) on public.category_budgets to authenticated;
    grant update (description) on public.category_budgets to authenticated;
    Commento: forma minima come 20260903000000 (non ripetere l'elenco completo); nessun altro grant, nessun revoke.
    Il trigger handle_category_budget_update NON si tocca (description resta modificabile, come amount).

  BLOCCO II (intestazione «BLOCCO II — nessun periodo sovrapposto; si può togliere per intero»):
    create extension if not exists btree_gist with schema extensions;
    Commento che spiega: serve per usare `=` su uuid e text dentro un indice GiST.
    Blocco DO di controllo preliminare: conta le coppie distinte (a.id < b.id) con stesso space_id, stessa category e
    periodi che si sovrappongono, usando ESATTAMENTE la stessa espressione di periodo del vincolo; se > 0
    `raise exception` con messaggio chiaro in italiano che dice quante coppie e che il vincolo non si può aggiungere
    finché non sono corrette (modello: 20261024000000_budget.sql:118-126).
    alter table public.category_budgets drop constraint if exists category_budgets_no_overlap;
    alter table public.category_budgets add constraint category_budgets_no_overlap
        exclude using gist (
            space_id with =,
            category with =,
            daterange(starts_on, (ends_on + interval '1 month')::date, '[)') with &&
        );
    Commento sull'espressione: il periodo va da starts_on al giorno prima del mese successivo a ends_on (quindi
    l'ultimo giorno del mese di ends_on incluso); con ends_on null il limite superiore è null = senza fine; con '[)'
    due budget contigui (uno finisce a gennaio, l'altro inizia a febbraio) non si toccano. Dire che l'indice unico
    category_budgets_aperto_uq resta e che le policy non cambiano. Dire che una violazione dà codice 23P01.

FILE B — script di collaudo, testata «QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE N ERRORI: le sezioni …» con N e
  l'elenco giusti. Come si esegue: dopo `supabase db reset`,
  `docker exec -i supabase_db_Eton psql -U postgres -d postgres -f - < supabase/verifica-budget-periodo.sql`.
  Usa utenti con UUID e email DIVERSI da verifica-rls-budget.sql (per esempio prefissi a5500000…, b6600000…,
  c7700000…; email *-periodo@esempio.it), così i due script possono girare uno dopo l'altro. Sezioni:
  1. Privilegi (has_column_privilege): description INSERT t, description UPDATE t; e, per provare che non si è
     aperto altro, UPDATE f su space_id, created_by, category, starts_on, version; più l'esistenza dei vincoli
     (select conname from pg_constraint where conrelid='public.category_budgets'::regclass and conname in
     ('category_budgets_description_max','category_budgets_no_overlap')) e dell'estensione (pg_extension btree_gist
     con il suo schema). Scrivi l'atteso nell'\echo.
  2. Preparazione: tre utenti, Alice crea uno spazio, Bruno entra, Carla crea il suo (come lo script 2.4).
  3. Alice crea «Spesa» da 2026-01-01 a 2026-03-01 (ends_on chiuso) con description 'Trimestre' (atteso 1 riga,
     description restituita).
  4a. Sovrapposto: Alice crea «Spesa» 2026-03-01 → 2026-05-01 (atteso ERRORE exclusion category_budgets_no_overlap).
  4b. Contiguo: Alice crea «Spesa» 2026-04-01 → 2026-06-01 (atteso 1 riga) — commit.
  4c. Budget aperto: Alice crea «Spesa» aperta da 2026-07-01 (atteso 1 riga) — commit; poi, in transazione propria,
      «Spesa» chiusa 2027-01-01 → 2027-02-01 (atteso ERRORE exclusion: cade dopo un budget aperto).
  4d. Stessa categoria in un altro spazio: Carla crea «Spesa» 2026-01-01 aperta nel suo spazio (atteso 1 riga).
  4e. Altra categoria stesso periodo nello spazio condiviso: «Casa» 2026-01-01 → 2026-03-01 (atteso 1 riga).
  4f. Allungare un budget dentro il successivo via UPDATE: Alice porta ends_on del budget del punto 3 a 2026-04-01
      (atteso ERRORE exclusion).
  4g. Descrizione di 201 caratteri: insert con repeat('x', 201) (atteso ERRORE check category_budgets_description_max).
  4h. Descrizione di 200 caratteri via UPDATE sul budget del punto 3 (atteso 1 riga, char_length 200) — commit.
  4i. Insert senza description (come il client pubblicato): «Svago» aperta da 2026-01-01 senza la colonna nella
      lista (atteso 1 riga, description '').
  5. Pulizia: Alice cancella lo spazio condiviso, Carla il suo; conteggi prima/dopo come nello script 2.4.
  6. Riepilogo finale con gli attesi per sezione e la riga «QUESTO SCRIPT DEVE PRODURRE ESATTAMENTE N ERRORI …».
  Ogni sezione che deve fallire ha begin … rollback propria; le altre begin … commit. Usa :'condiviso' e
  :'spazio_carla' via \gset come lo script 2.4.

BUDGET DI COMPLESSITÀ: due file SQL, nessuna funzione nuova, nessun trigger nuovo, nessuna vista; nel file A un solo
  blocco DO. Nessuna modifica a tipi C#.

verifica: `mcp__synapse__diagnostics` sui file toccati (qui non si applica: sono SQL), poi `mcp__synapse__build`/`test`
  con root=G:\Sviluppo\Eton NON li lanci tu: le build parallele si corrompono, compila la sessione; `dotnet build|test`
  in shell lo nega `hooks/build-via-synapse.ps1`, e solo se il tool synapse fallisce si rilancia con `SYNAPSE_DOWN=1`
  davanti, detto nel resoconto.

RITORNO: FATTO / DEVIAZIONI / VERIFICA / NOTE, con N errori attesi del file B ed elenco delle sezioni.
