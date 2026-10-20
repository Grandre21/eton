UNITÀ: 3 — ESITO: FATTO

TOCCATI:
- supabase/migrations/20261020000000_rate_entrate.sql → +133/−0 (nuovo)
- supabase/verifica-rls-rate-entrate.sql → +278/−0 (nuovo)
- handoff/03-migrazione/brief-codex-1.md, brief-codex-2.md, codex-1.md, codex-2.md, resoconto.md → registro dell'unità

REVIEW:
review: migrazione rate ed entrate
  autore          codex exec 0.161.0 — 2 files changed, 411 insertions(+) · 2 create mode (dopo la correzione; primo giro: 2 files changed, 299 insertions(+))
  bug-hunter      RILIEVI: 3
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 1
  checker         VERDETTI: fondati 4 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 4 · non risolti 0 · non verificabili 0
  bug-hunter      RILIEVI: 0
  threat-hunter   RILIEVI: 0
(le ultime tre voci sono i rilanci sulla correzione; backend-expert non rilanciato sulla correzione, che aggiunge una funzione di 15 righe prescritta dal capo)

CONTRATTI:
- `kind`: supabase/migrations/20261020000000_rate_entrate.sql:12 `alter table public.expenses add column if not exists kind text not null default 'expense';` e :13 (stessa riga su recurring_expenses); valori :17 e :20 `check (kind in ('expense','income'))`.
- `installments`: :24 `add column if not exists installments smallint null,`
- `plan_total`: :25 `add column if not exists plan_total numeric(12,2) null;`
- Privilegi: :131 `grant insert (kind) on public.expenses to authenticated;` · :132 `grant insert (kind, installments, plan_total) on public.recurring_expenses to authenticated;` · :133 `grant update (installments, plan_total) on public.recurring_expenses to authenticated;`
- Per le unità 4-5: un'occorrenza con recurring_id prende SEMPRE il kind della sua regola (:110 `select r.kind into new.kind`), qualunque cosa mandi il client; il client deve calcolare amount per difetto (floor a 2 decimali), altrimenti :46 `check (plan_total >= amount * installments)` rifiuta il piano (es. 100/6 arrotondato a 16.67 dà 100.02 > 100).

ADJUDICA:
- bug-hunter BH1: fondato → corretto — il client pubblicato materializza senza kind, e un'occorrenza di una regola income nasceva 'expense' per sempre — rate_entrate.sql:110 `select r.kind into new.kind`
  verificato: risolto — supabase/migrations/20261020000000_rate_entrate.sql:110 «la funzione copia kind dalla regola quando recurring_id non è null e solleva errore se la regola non c'è o non è leggibile»
- bug-hunter BH2: fondato → corretto — `plan_total >= amount` permetteva un'ultima rata ≤ 0, mai materializzabile — rate_entrate.sql:46 `add constraint recurring_expenses_plan_total_min check (plan_total >= amount * installments);`
  verificato: risolto — supabase/migrations/20261020000000_rate_entrate.sql:46
- bug-hunter BH3: fondato → corretto — l'intestazione dello script non diceva `supabase db reset`, e alla seconda esecuzione gli errori diventano 10 — verifica-rls-rate-entrate.sql:12 `-- supabase db reset`
  verificato: risolto — supabase/verifica-rls-rate-entrate.sql:11-13
- conformity CF1: fondato → corretto — «e'»/«gia'» con apostrofo ASCII nei commenti della migrazione — rate_entrate.sql:54 `-- Il trigger esistente punta già a questa funzione e non va ricreato.`
  verificato: risolto — supabase/migrations/20261020000000_rate_entrate.sql (righe 6, 33, 54, 98, 100)
- backend-expert BE1: fuori scope — TIPO: progetto, il blocco di impersonazione ricopiato 14 volte nello script; stesso schema in tutti gli 8 verifica-rls-*.sql, è una decisione sul modello degli script, va all'utente — verifica-rls-rate-entrate.sql:47-49 `set local request.jwt.claims = '{"sub":"a1000000-…","role":"authenticated"}';`
Riverifica a campione degli infondati: nessun infondato. I rilievi di sicurezza (trigger e privilegi) li ho riletti io sul codice: rate_entrate.sql:105-125 e :131-133.

FUORI SCOPE:
- backend-expert BE1 (TIPO: progetto): negli script di collaudo usare `\set come_alice '…'` e `:'come_alice'` invece di ricopiare i claims JWT a ogni sezione. Riguarda tutti gli 8 script `supabase/verifica-rls-*.sql` (189 occorrenze); decisione dell'utente, non di questa unità.
- Il piano a rate materializzato da una PWA vecchia esce senza «rata k/N» e con l'ultima rata uguale alle altre: il database non può correggerlo (vedi DA RATIFICARE, vincolo operativo).

GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\03-migrazione → succeeded, 0 errors, 4 warnings
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\03-migrazione → 0 failed, 392 passed, 0 skipped
GATE: supabase db reset locale + verifica-rls-rate-entrate.sql → 11 errori (attesi 11); verifica-rls-spese.sql → 7 (attesi 7); verifica-rls-ricorrenti.sql → 10 (attesi 10), ciascuno su db pulito

SCOSTAMENTI:
- Due aggiunte allo schema della spec §3, decise dal capo: il check `plan_total >= amount * installments` al posto di `plan_total >= amount`, e la funzione `handle_expense_insert()` con il trigger `expenses_before_insert` (copia kind dalla regola nelle occorrenze, fallisce se la regola non è leggibile). In più, non chiesto dalla spec ma coerente: `new.kind := old.kind` anche nel trigger di update delle regole (rate_entrate.sql:89).
- Domanda al capo (2026-10-20 ~14:50): «posso aggiungere due vincoli che la spec §3 non scrive? 1) check plan_total >= amount * installments; 2) trigger BEFORE INSERT su expenses che copia kind dalla regola». Risposta di coordinatore-Eton (2026-10-20 ~14:55): «1 sì, 2 sì. […] Se la regola non è leggibile, l'INSERT deve fallire, non passare con kind nullo o di default: verificalo nel collaudo. […] falle vedere tutte e due a threat-hunter. Scrivi in DA RATIFICARE entrambe le aggiunte, con la riga sul vincolo operativo». Le sezioni di collaudo 8f e 12a-12d e il rilancio di threat-hunter vengono da quella risposta: verificano le correzioni chieste, non sono lavoro nuovo.
- Script di collaudo nuovo sotto `supabase/` (nel perimetro: il progetto ne ha uno per ciascuna migrazione).
- PrivilegiInsertTests non si è rotto: nessun test adeguato qui.
- Codex ha scritto i file con `apply patch`, nessun interprete inline (codex-1.log e codex-2.log, controllati dal checker). Nessun file `-o` comparso nella radice del worktree, in nessuno dei due lanci.
- Docker e Supabase locale: avviati per il collaudo, spenti a fine lavoro (`supabase stop`, `docker desktop stop`, nessun processo Docker rimasto).

DECISIONI DELL'UTENTE QUI: nessuna

DA RATIFICARE:
- Check `plan_total >= amount * installments` invece di `plan_total >= amount` (spec §3). Raccomandazione: sì — garantisce un'ultima rata positiva; col calcolo per difetto della spec §4.1 non rifiuta nessun piano legittimo.
- Trigger `expenses_before_insert`: l'occorrenza di una regola prende il tipo della regola, e l'INSERT fallisce se la regola non è leggibile. Raccomandazione: sì — impedisce che una PWA vecchia scriva per sempre un'entrata come spesa; threat-hunter: nessun oracolo né lettura nuova.
- `new.kind := old.kind` anche nel trigger di update delle regole. Raccomandazione: sì — stessa difesa delle spese, i privilegi già lo vietano.
- Vincolo operativo: aggiornare la PWA su tutti i dispositivi prima di creare il primo piano a rate (e la prima entrata ricorrente). Una PWA vecchia materializza un piano senza «rata k/N» e con l'ultima rata uguale alle altre, e il database non può impedirlo.

SEGNALAZIONI: nessuna

COMMIT: d6d136f

## SQL PER L'UTENTE

Da incollare nell'SQL editor di Supabase, in produzione, **prima** del push della prima unità che usa le colonne nuove (4 o 5). È additiva: il client già pubblicato continua a funzionare.

### (1) La migrazione, testo integrale

```sql
-- =====================================================================================
-- Eton — rate ed entrate: il tipo di un movimento e il piano a rate di una regola ricorrente.
-- Idempotente e rieseguibile.
--
-- Dipende da 20260824000000_spese.sql e 20260922000000_ricorrenti.sql, di cui estende
-- le due funzioni trigger. È additiva: un client già pubblicato, che non manda kind,
-- scrive una spesa grazie al default.
-- =====================================================================================

-- ---------- tipo del movimento ----------
-- Le entrate sono importi positivi con un tipo, non importi negativi: amount > 0 resta.
alter table public.expenses add column if not exists kind text not null default 'expense';
alter table public.recurring_expenses add column if not exists kind text not null default 'expense';

alter table public.expenses drop constraint if exists expenses_kind_valido;
alter table public.expenses
    add constraint expenses_kind_valido check (kind in ('expense','income'));
alter table public.recurring_expenses drop constraint if exists recurring_expenses_kind_valido;
alter table public.recurring_expenses
    add constraint recurring_expenses_kind_valido check (kind in ('expense','income'));

-- ---------- piano a rate ----------
alter table public.recurring_expenses
    add column if not exists installments smallint null,
    add column if not exists plan_total numeric(12,2) null;

-- La coppia deve essere completa: un solo valore non descriverebbe un piano utilizzabile.
alter table public.recurring_expenses drop constraint if exists recurring_expenses_piano_coppia;
alter table public.recurring_expenses
    add constraint recurring_expenses_piano_coppia
    check ((installments is null) = (plan_total is null));

-- Con null i check sono soddisfatti (UNKNOWN è ammesso dalla semantica SQL): una regola
-- senza piano li passa tutti. Un piano presente richiede almeno due rate e un totale
-- sufficiente per tutte le rate; si applica soltanto alle spese mensili.
alter table public.recurring_expenses drop constraint if exists recurring_expenses_installments_min;
alter table public.recurring_expenses
    add constraint recurring_expenses_installments_min check (installments >= 2);
alter table public.recurring_expenses drop constraint if exists recurring_expenses_plan_total_min;
-- L'ultima rata vale plan_total − amount × (installments − 1) e deve restare positiva.
-- Con il solo plan_total >= amount, un UPDATE di amount da client vecchio o API diretta
-- può portarla a zero o sotto: materializzarla fallirebbe per sempre su expenses.amount > 0.
-- Il client divide il totale per N per difetto, quindi amount × N ≤ totale vale sempre.
-- Con installments null il check è soddisfatto: UNKNOWN è ammesso dalla semantica SQL.
alter table public.recurring_expenses
    add constraint recurring_expenses_plan_total_min check (plan_total >= amount * installments);
alter table public.recurring_expenses drop constraint if exists recurring_expenses_piano_solo_spese_mensili;
alter table public.recurring_expenses
    add constraint recurring_expenses_piano_solo_spese_mensili
    check (installments is null or (kind = 'expense' and every_months = 1));

-- ---------- trigger di expenses, esteso ----------
-- Un movimento non cambia tipo: per cambiarlo lo si cancella e lo si riscrive.
-- Il trigger esistente punta già a questa funzione e non va ricreato.
-- Non SECURITY DEFINER: come prima, il BEFORE UPDATE modifica NEW in memoria.
create or replace function public.handle_expense_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Rimesse a forza al valore precedente, ultima riga di difesa (v. spese.sql per il dettaglio).
    new.paid_by    := old.paid_by;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;

    new.recurring_id     := old.recurring_id;
    new.recurring_period := old.recurring_period;
    new.kind := old.kind;

    return new;
end;
$$;

-- ---------- trigger di recurring_expenses, esteso ----------
-- Stessa ultima riga di difesa sul tipo; installments e plan_total restano aggiornabili.
create or replace function public.handle_recurring_expense_update()
returns trigger language plpgsql
set search_path = public as $$
begin
    new.version    := old.version + 1;
    new.updated_at := now();

    -- Rimesse a forza al valore precedente, ultima riga di difesa come in expenses.
    new.paid_by    := old.paid_by;
    new.space_id   := old.space_id;
    new.created_at := old.created_at;
    new.kind := old.kind;

    return new;
end;
$$;

-- ---------- tipo dell'occorrenza ----------
-- Il client già pubblicato materializza senza kind: una regola income produrrebbe una
-- expense per default, senza poterla più correggere (nessun UPDATE su kind e il trigger
-- di update lo rimette). Copiarlo qui dà sempre all'occorrenza il tipo della sua regola,
-- qualunque cosa mandi il client.
-- Non è SECURITY DEFINER, di proposito: la SELECT passa dalla RLS di chi inserisce.
-- La policy di insert su expenses già pretende il pagante della regola, quindi una regola
-- legittima è sempre leggibile. Se non è leggibile o non esiste, l'INSERT fallisce invece
-- di passare con il tipo di default. Il BEFORE scatta prima della with check della policy
-- e del controllo di on conflict, ed è innocuo con do nothing.
create or replace function public.handle_expense_insert()
returns trigger language plpgsql
set search_path = public as $$
begin
    if new.recurring_id is not null then
        select r.kind into new.kind
          from public.recurring_expenses r
         where r.id = new.recurring_id;
        if not found then
            raise exception 'regola ricorrente % non trovata o non leggibile', new.recurring_id
                using errcode = '23503';
        end if;
    end if;
    return new;
end;
$$;

drop trigger if exists expenses_before_insert on public.expenses;
create trigger expenses_before_insert
    before insert on public.expenses
    for each row execute function public.handle_expense_insert();

-- ---------- privilegi aggiuntivi ----------
-- Niente grant update su kind, di proposito: il tipo si decide solo alla creazione.
-- L'UPDATE del piano serve a modificarlo prima che parta; il blocco a piano avviato
-- lo fa il client. Si aggiungono solo i privilegi delle nuove colonne.
grant insert (kind) on public.expenses to authenticated;
grant insert (kind, installments, plan_total) on public.recurring_expenses to authenticated;
grant update (installments, plan_total) on public.recurring_expenses to authenticated;
```

### (2) La query di controllo, da lanciare dopo

```sql
select
    (select count(*) from information_schema.columns
      where table_schema = 'public'
        and ((table_name = 'expenses' and column_name = 'kind')
          or (table_name = 'recurring_expenses' and column_name in ('kind', 'installments', 'plan_total')))) as colonne_nuove,
    (select count(*) from pg_constraint
      where conname in ('expenses_kind_valido', 'recurring_expenses_kind_valido',
                        'recurring_expenses_piano_coppia', 'recurring_expenses_installments_min',
                        'recurring_expenses_plan_total_min', 'recurring_expenses_piano_solo_spese_mensili')) as vincoli_nuovi,
    (select count(*) from pg_trigger
      where tgname = 'expenses_before_insert' and not tgisinternal) as trigger_nuovo,
    has_column_privilege('authenticated', 'public.expenses', 'kind', 'INSERT')                  as spese_kind_insert,
    has_column_privilege('authenticated', 'public.expenses', 'kind', 'UPDATE')                  as spese_kind_update,
    has_column_privilege('authenticated', 'public.recurring_expenses', 'kind', 'UPDATE')        as regole_kind_update,
    has_column_privilege('authenticated', 'public.recurring_expenses', 'plan_total', 'UPDATE')  as regole_piano_update,
    (select count(*) from public.expenses where kind <> 'expense')                              as movimenti_non_spesa;
```

Risultato atteso, una riga sola (provato sul database locale):

| colonne_nuove | vincoli_nuovi | trigger_nuovo | spese_kind_insert | spese_kind_update | regole_kind_update | regole_piano_update | movimenti_non_spesa |
|---|---|---|---|---|---|---|---|
| 4 | 6 | 1 | true | false | false | true | 0 |

`movimenti_non_spesa = 0` dice che tutte le spese già scritte hanno preso il tipo «spesa».

### (3) Se l'app dice «colonna sconosciuta»

Il servizio che espone il database all'app (*PostgREST*) può tenere in memoria lo schema vecchio per qualche istante. Se dopo la migrazione compare un errore di colonna sconosciuta (`kind`, `installments` o `plan_total`), lancia nell'SQL editor:

```sql
NOTIFY pgrst, 'reload schema';
```
