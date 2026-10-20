# Brief 2 — correzioni alla migrazione rate ed entrate (unità 03)

Stesso worktree e stessi due file del brief 1: `supabase/migrations/20261020000000_rate_entrate.sql` e
`supabase/verifica-rls-rate-entrate.sql`. Nessun altro file. Valgono tutti i divieti del brief 1 (niente
`revoke`, le parole `grant`/`revoke` solo negli statement di privilegio e nei commenti `--`, nessuna policy
nuova, non avviare Docker/Supabase/Synapse.Server).

## Correzioni nella migrazione

A. Riga 33: «e'» → «è». Riga 49: «gia'» → «già». Le migrazioni usano gli accenti veri; il «senza accenti»
   vale solo per lo script di collaudo.

B. Il vincolo `recurring_expenses_plan_total_min` diventa `check (plan_total >= amount * installments)`
   (stesso nome, stesso schema drop/add). Commento del perché: l'ultima rata vale
   `plan_total − amount × (installments − 1)` e deve restare positiva; con `plan_total >= amount` un UPDATE
   di amount (client vecchio o API diretta) può portarla a zero o sotto, e la sua materializzazione
   fallirebbe per sempre sul check `amount > 0` di expenses. Con il calcolo del client
   (amount = totale / N per difetto) vale sempre amount × N ≤ totale. Con installments null il check vale
   vero.

C. Una funzione trigger nuova e il suo trigger, in una sezione `-- ---------- tipo dell'occorrenza ----------`
   dopo quella dei trigger estesi:
   ```sql
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
   ```
   Commenti del perché (in italiano, con accenti): il client già pubblicato materializza le regole senza
   mandare kind, quindi l'occorrenza di una regola income nascerebbe 'expense' col default e non si
   correggerebbe più (niente UPDATE su kind, il trigger di update lo rimette); copiandolo qui l'occorrenza ha
   sempre il tipo della sua regola, qualunque cosa mandi il client. NON è SECURITY DEFINER, di proposito: la
   SELECT passa dalla RLS di chi inserisce, e la policy di insert su expenses già pretende che sia il pagante
   della regola, quindi una regola legittima è sempre leggibile; una regola non leggibile (o inesistente) fa
   fallire l'INSERT invece di farlo passare con il tipo di default. Il trigger BEFORE scatta prima della
   with check della policy e prima del controllo di `on conflict`, ed è innocuo con `do nothing`.
   Il messaggio dell'eccezione non deve contenere le parole grant o revoke.

## Correzioni nello script di collaudo

D. Intestazione «Come si esegue»: come in `verifica-rls-ricorrenti.sql:24-27` — serve Docker Desktop
   avviato, `supabase start`, `supabase db reset`, poi il `docker exec`. Dire che alla seconda esecuzione
   senza reset gli utenti finti già esistono e gli errori diventano uno in più.

E. Sezioni nuove, ciascuna con `\echo` dell'atteso, e aggiornare riepilogo e riga «ESATTAMENTE N ERRORI»
   contando davvero:
   - 8f. Alice porta `amount` a 50.00 sul proprio piano da 3 rate e 100.00 (UPDATE) → ERRORE check
     `recurring_expenses_plan_total_min`. Usa il piano della sezione 3 prima che la sezione 9 lo cambi,
     oppure creane uno apposta; `rollback` dopo.
   - 12a. Alice materializza un'occorrenza della propria regola income mandando `kind = 'expense'` (tutte
     le 10 colonne) → 1 riga con `kind = 'income'`.
   - 12b. Il client vecchio: Alice materializza un'occorrenza della propria regola income con le 9 colonne
     di prima, senza kind → 1 riga con `kind = 'income'`.
   - 12c. Una regola non leggibile: Carla (estranea) inserisce nel proprio spazio una spesa con
     `recurring_id` = la regola income di Alice → ERRORE (quello del trigger, «non trovata o non
     leggibile»), non una riga con kind 'expense'. Se Carla non ha uno spazio, crealo con
     `public.create_space` come fa lo script per Alice.
   - 12d. Spesa a mano senza recurring_id con `kind = 'income'` → 1 riga con `kind = 'income'` (il trigger
     non tocca i movimenti senza regola).
   Mettile prima della sezione di pulizia, e aggiorna i conteggi «prima» della pulizia se cambiano.
   Le sezioni 12a/12b usano periodi diversi (`recurring_period` primo del mese) per non collidere sul
   vincolo unique.

## Budget di complessità
Una funzione nuova e un trigger nuovo, nient'altro. Nessuna policy, nessun indice, nessun grant nuovo.

## Verifica
`dotnet build` e `dotnet test` nel worktree, verdi. Rileggi e conta gli errori attesi.

## Formato di ritorno
`FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/<NN-slug>/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
