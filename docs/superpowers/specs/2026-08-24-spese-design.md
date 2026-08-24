# Eton — le spese

Design della funzionalità «spese», scritto il 24 agosto 2026.

Si legge accanto a [`2026-08-11-eton-design.md`](2026-08-11-eton-design.md), da cui eredita
architettura, modello di sicurezza e convenzioni: qui si descrive solo ciò che si aggiunge, e le
poche cose che si aggiungono **diversamente** da come le fa il resto del progetto.

---

## 1. Cosa si costruisce

Un posto dove segnarsi le spese, e vedere quanto si è speso questo mese e in cosa.

Tre decisioni prese prima di scrivere una riga, ognuna scartando un'alternativa più grande:

**Si traccia, non si divide.** Chi ha pagato resta registrato, ma il primo taglio non calcola
nessun debito fra le persone: niente quote, niente saldi, niente «Marco deve 12,30 a Giulia».
La divisione è un prodotto dentro il prodotto — regole di ripartizione, storni, contestazioni — e
va costruita sapendo se la funzione si usa. La colonna `paid_by` esiste dal giorno uno proprio
perché quel giorno sia additivo (v. §11).

**Quattro campi in inserimento**: importo, descrizione, categoria, data. La descrizione non è
facoltativa e non è nascosta dietro un «aggiungi nota»: senza, dopo tre mesi il registro è una
colonna di numeri che non si sa più a cosa corrispondano, e la ricerca per testo non esiste.

**Il mese corrente è la spina dorsale delle analisi**: quanto, e in cosa. Non l'andamento nei mesi,
non un tetto di spesa da rispettare. Entrambi restano possibili dopo, sugli stessi dati e senza
migration (v. §11).

### Cosa NON si costruisce

Nessuna coda di scrittura offline, nessun budget, nessun grafico temporale, nessuna spesa
ricorrente, nessun allegato, nessuna divisione fra membri, nessuna valuta diversa dall'euro.

---

## 2. Perché una tabella nuova e non una collezione

Eton ha già le collezioni a campi liberi: una spesa si potrebbe fare come collezione con i campi
«importo», «data», «categoria», senza scrivere una riga di schema. È una falsa economia, e il
punto in cui si rompe è preciso — verificato nello schema, non supposto.

| Dove si rompe | Perché |
|---|---|
| `collection_items.data` è `jsonb` a forma libera, con il solo vincolo `jsonb_typeof = 'object'` | un importo lì dentro è un valore JSON senza vincolo numerico: niente `check (amount > 0)`, e ogni aggregazione richiede un cast `(data->>'importo')::numeric` che solleva un'eccezione al primo dato sporco |
| l'unico indice è `(collection_id, name)` | è il pattern «catalogo consultato per nome», dichiarato tale dal commento della migration. Le spese si leggono per data discendente: il pattern opposto |
| un elemento di collezione è referenziato da `reviews` | una spesa recensibile, con media dei voti e voto al buio, è rumore semantico che poi va spiegato a ogni lettore dello schema |

Il riuso vero è il **pattern**, non la tabella: `expenses` è una fotocopia strutturale di `notes` —
stesse funzioni `is_space_member()` / `is_space_owner()`, stesso trigger di versione, stessi
privilegi di colonna. Costo basso, e nessuna riga di sicurezza inventata da zero.

---

## 3. Modello dati

Una spesa vive dentro uno **spazio**, come tutto il resto di Eton. Nello spazio personale è tua;
in uno spazio condiviso è del gruppo. Non serve nessun concetto di visibilità nuovo: chi vede cosa
lo decide già l'appartenenza allo spazio.

### 3.1 DDL

```sql
create table if not exists public.expenses (
    id          uuid primary key default gen_random_uuid(),
    space_id    uuid not null references public.spaces (id) on delete cascade,

    -- Chi ha anticipato i soldi. Oggi coincide sempre con chi crea la riga, perché la policy di
    -- INSERT pretende paid_by = auth.uid(); esiste separato per il giorno in cui si dividerà una
    -- spesa fra i membri, dove i due ruoli divergono (v. §11).
    paid_by     uuid not null default auth.uid()
                    references auth.users (id) on delete cascade,

    amount      numeric(12,2) not null check (amount > 0),

    -- btrim e non la lunghezza grezza, come per spaces.name e collections.name: una descrizione
    -- di soli spazi supererebbe il controllo e comparirebbe nel registro come una riga vuota.
    description text not null check (length(btrim(description)) between 1 and 200),

    -- Testo libero nel database, elenco chiuso nell'interfaccia. Non è una svista: un
    -- check contro una lista renderebbe «aggiungi la categoria Animali» una migration da
    -- incollare a mano nel SQL Editor di produzione, mentre così è una riga di C#. I dati restano
    -- puliti lo stesso, perché a scrivere la categoria non è mai la tastiera (v. §5.3).
    category    text not null check (length(btrim(category)) between 1 and 40),

    -- 'date' e non 'timestamptz': una spesa appartiene a un giorno, non a un istante, e un fuso
    -- orario qui produrrebbe spese che cambiano mese a seconda di dove ti trovi.
    spent_on    date not null default current_date,

    version     integer not null default 1,
    created_at  timestamptz not null default now(),
    updated_at  timestamptz not null default now()
);

-- Composito, come notes_space_updated_idx: l'unica lettura che esiste è «le spese di questo
-- spazio, dalla più recente», e questo indice serve sia il filtro sia l'ordinamento.
create index if not exists expenses_space_date_idx
    on public.expenses (space_id, spent_on desc);
```

Il trigger `handle_expense_update()` ricalca `handle_note_update()` alla lettera: incrementa
`version`, aggiorna `updated_at`, e rimette a forza `paid_by`, `space_id` e `created_at` ai valori
precedenti. Non è `SECURITY DEFINER` e non deve esserlo — i privilegi di colonna si verificano
sulle colonne **nominate** nell'istruzione `UPDATE`, non su quelle che un trigger tocca dopo.

### 3.2 L'unica differenza deliberata da `notes`: l'`id` lo genera il client

Su `notes` il `grant insert` **non** include `id`, che nasce dal default del database. Su
`expenses` lo include.

Il motivo è il caso d'uso: una spesa si segna al bar, col telefono, con la rete che va e viene. Se
l'inserimento fallisce a metà, l'utente non sa se la riga è passata, e riprova. Con l'`id`
generato dal client il secondo tentativo porta lo stesso uuid: se il primo era passato, la chiave
primaria rifiuta il duplicato, e non nasce una spesa doppia. Con l'`id` generato dal database ne
nascerebbero due, identiche, indistinguibili.

È anche la fondazione su cui una coda offline si costruirà senza migration di rottura (v. §10).

---

## 4. Sicurezza

`expenses` non inventa nessuna regola: riusa `is_space_member()` e `is_space_owner()`.

### 4.1 Matrice RLS

| Operazione | Condizione | Perché |
|---|---|---|
| `select` | `is_space_member(space_id)` | la spesa appartiene allo spazio, come una nota |
| `insert` | `is_space_member(space_id) and paid_by = auth.uid()` | si registra ciò che si è pagato di persona: senza la seconda condizione un membro potrebbe attribuire pagamenti a qualcun altro |
| `update` | `paid_by = auth.uid() or is_space_owner(space_id)` | corregge chi l'ha scritta, o il proprietario dello spazio che deve poter fare pulizia a casa propria |
| `delete` | `paid_by = auth.uid() or is_space_owner(space_id)` | idem |

`with check` esplicito e identico a `using` sull'`update`, come su `notes`: omettendolo Postgres
userebbe comunque `using` per entrambi, ma scriverlo rende la regola leggibile senza conoscere
quel dettaglio.

**Nota per il giorno della divisione**: la condizione `paid_by = auth.uid()` sull'`insert` è
esattamente ciò che andrà rivisto per poter registrare che ha pagato qualcun altro. Meglio che sia
una decisione deliberata allora, che un permesso lasciato largo adesso.

### 4.2 Privilegi

```sql
revoke all on public.expenses from anon, authenticated;

grant select, delete on public.expenses to authenticated;

-- id compreso, a differenza di notes: v. §3.2.
grant insert (id, space_id, paid_by, amount, description, category, spent_on)
    on public.expenses to authenticated;

-- Non paid_by e non space_id: cambiarli sposterebbe la spesa sotto un'altra regola di
-- visibilità, cioè sarebbe una fuga di dati e non un dispetto — lo stesso motivo per cui
-- collection_items non concede l'UPDATE su collection_id.
grant update (amount, description, category, spent_on)
    on public.expenses to authenticated;

grant all on public.expenses to service_role;
```

`version`, `created_at` e `updated_at` non compaiono in nessun grant: li scrive il trigger, ed è
precisamente ciò che rende la concorrenza ottimistica una difesa e non un suggerimento.

### 4.3 Uno script di verifica

Come per le altre tabelle, `supabase/verifica-rls-spese.sql`: prova da dentro il database che le
policy rifiutino ciò che devono rifiutare — inserire una spesa a nome di un altro, leggere le
spese di uno spazio a cui non si appartiene, modificare `space_id`, falsificare `version`.
Dichiara in testa quanti errori deve produrre.

---

## 5. Il denaro, e perché costa codice

### 5.1 Il tipo

`numeric(12,2)` nel database, `decimal` in C#. **Non** interi in centesimi.

Il progetto usa già `numeric(3,1)` per i voti delle recensioni e sa portarlo lungo tutta la catena
PostgREST → JSON → Newtonsoft → `decimal`. A scala (12,2) non esiste un problema di precisione
JSON — quello morde oltre 2^53, cioè mai qui — mentre i centesimi interi comprerebbero
un'esattezza che `decimal` ha già, al prezzo di moltiplicazioni e divisioni per 100 sparse in ogni
punto in cui il numero entra o esce.

### 5.2 Leggere «12,50» e scrivere «1.284,50»

`InvariantGlobalization` è attivo: `new CultureInfo("it-IT")` lancia a runtime. Non esiste una
cultura da impostare, quindi la virgola decimale si gestisce a mano — come già fa
`CalcoliVoti.Testo` per i voti.

Un file nuovo, `Services/Denaro.cs`, puro e senza dipendenze:

- `bool Prova(string testo, out decimal importo)` — accetta sia «12,50» sia «12.50» (chi digita
  su un tastierino numerico ottiene il punto, chi digita su una tastiera italiana la virgola:
  rifiutare uno dei due significa rifiutare metà degli inserimenti). Rifiuta zero, negativi, e
  più di due decimali.
- `string Testo(decimal importo)` — «1.284,50», con il separatore delle migliaia. Si formatta con
  `InvariantCulture` e poi si scambiano i due separatori, **in tre passaggi e non in due**: una
  sostituzione diretta `,` → `.` seguita da `.` → `,` produrrebbe due virgole, perché la seconda
  ritrova ciò che ha scritto la prima.

Entrambi con i test: sono logica pura, che è esattamente ciò che la suite `Eton.Tests` copre.

**Un tipo da decidere guardando, non ragionando**: `spent_on` è un `date`, e in C# la scelta è fra
`DateTime` e `DateOnly`. `DateOnly` sarebbe il tipo giusto — una spesa appartiene a un giorno — ma
attraversa Newtonsoft dentro Postgrest, e in `Release` il trimming è `full`. Si parte da
`DateTime`, che è il tipo già usato da `Note` e `Collection` per le loro colonne temporali e che
quindi è dimostrato funzionare lungo questa catena. Se qualcuno vuole `DateOnly`, va provato
**sull'applicazione pubblicata** e non solo in sviluppo: è la categoria di difetto che compila,
passa i test, e fallisce solo da pubblicata (v. `TrimmerRootAssembly` nel design principale).

### 5.3 Le categorie

`Services/CategorieSpesa.cs`: un elenco chiuso, in italiano, offerto dall'interfaccia a pastiglie.

> Spesa · Casa · Trasporti · Ristoranti · Salute · Svago · Abbigliamento · Istruzione · Regali · Altro

Il database accetta qualunque testo (v. §3.1). Se un domani servissero categorie definite
dall'utente, sono una tabella in più e nessuna migrazione dei dati esistenti.

### 5.4 I nomi dei mesi

Trappola dello stesso genere, e va scritta prima di inciamparci:
`DateTime.ToString("MMMM")` sotto `InvariantGlobalization` restituisce **«August»**, non
«agosto». I nomi dei mesi vanno in un array in italiano dentro `CalcoliSpese`, come le altre
stringhe del progetto.

---

## 6. Le analisi

### 6.1 Si calcolano nel client

In C#, su `Services/CalcoliSpese.cs`, puro come `CalcoliVoti`:

- il totale del mese;
- la ripartizione per categoria, ordinata per importo decrescente, con la quota percentuale di
  ciascuna sul totale (serve alla larghezza delle barre);
- il confronto con il mese precedente, come differenza e come percentuale.

**Non** con viste o funzioni Postgres, e la ragione è specifica di questo progetto più che
generale: le migration qui **si applicano a mano, incollandole nel SQL Editor di produzione**.
Ogni analisi nuova sarebbe quindi un intervento manuale sul database di produzione, mentre
un'aggregazione lato client si spedisce con un `git push`.

Il costo è il traffico: un anno di spese familiari sono forse 1500-3000 righe, cioè qualche
centinaio di kilobyte di JSON, e LINQ le aggrega in microsecondi. La soglia oltre cui questa
scelta va ripensata sono le decine di migliaia di righe, o le analisi su più anni.

Se un giorno servissero delle viste: `security_invoker = true` è **obbligatorio**, altrimenti la
vista gira con i privilegi del proprietario e scavalca la RLS. È il default, ed è una trappola
nota.

### 6.2 Come si vedono

Nessuna libreria, nemmeno vendorizzata: un istogramma sarebbe duecento kilobyte per fare peggio
ciò che venti righe fanno nello stile della casa. La ripartizione per categoria è un **registro
con barre proporzionali** — contenitore col bordo, righe separate da un filo, la barra è un `div`
con larghezza percentuale, l'importo in Plex Mono a destra.

Niente torta: la regola del progetto dice registri, non pile di riquadri, e su una torta i valori
piccoli diventano spicchi illeggibili proprio quando servirebbe confrontarli.

I colori seguono il discrimine di sempre: **verde acido** per i totali e gli importi, che si
constatano; **blu** per il pulsante «Segna» e per i controlli del mese, che si premono.

---

## 7. Schermate

### 7.1 `Pages/Spese.razor` — `@page "/expenses"`

Una pagina sola, tre parti nell'ordine in cui servono:

1. **Il modulo di inserimento, in cima e sempre aperto.** Quattro campi: importo (con
   `inputmode="decimal"`, a fuoco all'apertura), descrizione, categoria a pastiglie, data
   (preimpostata a oggi). Nessuna navigazione per segnare una spesa: il requisito da cui è nata
   questa funzionalità è la parola «comodamente», e un tocco in più per arrivare al modulo è
   esattamente ciò che la fa abbandonare dopo una settimana.
2. **Il riepilogo del mese**: il totale in Plex Mono a corpo grande, la variazione sul mese
   precedente, e sotto il registro delle categorie con le barre. Frecce ◀ ▶ per scorrere i mesi.
3. **Il registro delle spese del mese**, dalla più recente: data, descrizione, categoria,
   importo. Chi ha pagato compare **solo sulle righe in cui `paid_by` è diverso da chi sta
   guardando**. Non «solo negli spazi condivisi»: quella regola richiederebbe di conoscere il
   numero di membri, cioè una query in più, e darebbe lo stesso risultato — in uno spazio
   personale `paid_by` non differisce mai. La condizione più economica è anche quella più
   precisa, perché in uno spazio condiviso nasconde comunque il rumore delle proprie righe.

### 7.2 `Pages/SpesaEdit.razor` — `@page "/expenses/{Id:guid}"`

Correggere una spesa è raro: merita una pagina propria e non un modulo che appesantisce il
registro. Stessi quattro campi, più l'eliminazione. Concorrenza ottimistica come su note e
collezioni: la versione letta si rimanda come **filtro**, e zero righe modificate significano che
qualcun altro ha salvato nel frattempo.

### 7.3 Un riquadro nella Home

La Home mostra già le note recenti: si aggiunge il totale del mese con un collegamento alle
spese. Costa poco ed è il posto dove l'informazione viene cercata per prima.

---

## 8. La navigazione: la sesta voce, e chi cede il posto

`Shared/Navigazione.razor` ha esattamente cinque voci, e il suo commento dichiara che sul telefono
la striscia in fondo ha spazio «per cinque voci e nient'altro». Le spese fanno sei.

**Esce il Profilo, entrano le Spese.** Non è una scelta arbitraria: la pagina profilo mostra nome,
email e un pulsante «Esci», e il suo stesso commento dice che quel pulsante è *«l'unica cosa per
cui si arriva davvero su questa pagina»*. Una pagina il cui unico scopo è uscire non merita un
quinto della barra del pollice, mentre una funzione che si usa al bar in dieci secondi sì.

Il profilo si raggiunge dal piede della colonna di navigazione, il `div.nav-piede` che oggi
contiene `<SelettoreSpazio />`: gli si affianca un collegamento a `/profile`. Sul telefono quel
piede è nascosto dal CSS, quindi va reso raggiungibile anche di là — il posto naturale è la Home,
accanto al selettore di spazio che già vive lì.

Serve un'icona nuova in `Shared/Icona.razor`, disegnata in SVG come le altre sette: nessuna emoji,
per lo stesso motivo per cui le emoji sono state tolte dalla vetrina — sono disegni del sistema
operativo, diversi su Windows, Android e iPhone.

Rotta `/expenses` in inglese come tutte le altre (`notes`, `collections`, `spaces`, `profile`):
l'italiano è nei testi, non negli URL.

---

## 9. Test

`Eton.Tests` copre solo logica pura, senza database. Quindi:

- **`Denaro`** — «12,50» e «12.50» danno 12,50; «0», «-3», «12,505», «», «abc» sono rifiutati;
  `Testo(1284.50m)` è «1.284,50», `Testo(0.05m)` è «0,05», `Testo(1000000m)` è «1.000.000,00».
  Il caso che merita un test da solo è lo scambio dei separatori a tre passaggi (§5.2): è
  precisamente il punto in cui una scorciatoia produce «1,284,50» e nessuno se ne accorge finché
  gli importi non superano il migliaio.
- **`CalcoliSpese`** — un mese senza spese dà totale zero e nessuna categoria, non un'eccezione;
  le categorie sono ordinate per importo decrescente; le quote percentuali sommano a 100 anche
  quando la divisione non è esatta; il confronto con un mese precedente vuoto non divide per zero.
- **`CategorieSpesa`** — l'elenco non ha duplicati né stringhe vuote.

Repository e pagine non si testano, come per il resto del progetto: richiederebbero un database, e
il confine di sicurezza vero lo prova `verifica-rls-spese.sql` da dentro Postgres.

---

## 10. L'offline

Il primo taglio **non** ha una coda di scrittura. Una coda fatta bene deve attraversare la
concorrenza ottimistica, la scadenza del token mentre si è in coda, la deduplica e uno stato «in
attesa» visibile nell'interfaccia: è un sottosistema, e costruirlo prima di sapere se la funzione
si usa è ingegneria prematura.

Quello che il primo taglio garantisce:

- un inserimento fallito **non perde ciò che hai digitato**: il modulo resta compilato, con
  l'errore visibile e un pulsante per riprovare;
- il ritentativo è innocuo, perché l'`id` lo ha già generato il client (§3.2).

Su queste due la coda si costruirà dopo, senza migration e senza cambiare una policy.

---

## 11. Fuori scope, e cosa costerebbe dopo

| Cosa | Costo quando servirà |
|---|---|
| **Dividere le spese fra i membri** (chi deve quanto a chi) | `paid_by` c'è già. Serve rivedere la policy di `insert` (§4.1), una tabella delle quote o una convenzione di divisione, e la pagina dei saldi. Nessuna migrazione dei dati esistenti |
| **L'andamento nei mesi** | zero schema: gli stessi dati, un'aggregazione in più in `CalcoliSpese` e una schermata |
| **Un tetto di spesa** | un concetto nuovo da memorizzare — colonna o tabella, con RLS propria — più una schermata per impostarlo. È l'unica delle tre che tocca il database |
| **Spese ricorrenti** | una tabella di modelli più un modo per materializzarle: senza un server applicativo, o le genera il client all'apertura o serve un `pg_cron`. Da progettare, non da improvvisare |
| **Categorie definite dall'utente** | una tabella `expense_categories` per spazio. I dati esistenti non si toccano, perché la categoria è già testo libero (§3.1) |
| **Allegati (lo scontrino fotografato)** | Supabase Storage, che il progetto oggi non usa affatto: un confine di sicurezza nuovo da progettare da zero |

---

## 12. Ordine di implementazione

1. **Migration** `supabase/migrations/20260824000000_spese.sql` + `verifica-rls-spese.sql`,
   applicati su un database locale con `supabase db reset`. **In produzione si applica a mano**, e
   **prima** del push che pubblica il codice che li usa: fra il deploy e la migration
   l'applicazione online interrogherebbe una tabella che non esiste.
2. **Logica pura**: `Denaro`, `CategorieSpesa`, `CalcoliSpese`, con i loro test. Non dipendono da
   niente e sbloccano tutto il resto.
3. **`Models/Expense.cs` + `Services/ExpenseRepository.cs`**, ricalcando `Note` e `NoteRepository`
   — concorrenza ottimistica e distinzione dei tre esiti di salvataggio comprese.
4. **`Pages/Spese.razor`**: prima il registro e l'inserimento, poi il riepilogo. Un elenco che
   funziona vale più di un'analisi su dati che non ci sono ancora.
5. **`Pages/SpesaEdit.razor`**.
6. **Navigazione**: icona nuova, sesta voce, profilo spostato (§8).
7. **Riquadro nella Home** (§7.3).
8. **Prova nel browser** con l'applicazione avviata, come per la vetrina.

I passi 2 e 3 sono indipendenti dal 4 e dal 5 e possono procedere in parallelo; il passo 1 li
precede tutti perché lo schema è ciò che il modello ricalca.
