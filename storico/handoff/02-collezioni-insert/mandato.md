UNITÀ: 02/11 — Privilegio INSERT sulle collezioni

## OBIETTIVO

Creare una collezione torna possibile. Oggi ogni `Insert<Collection>` fallisce con
`permission denied for table collections` (SQLSTATE 42501) perché il modello invia la
colonna `blind`, che il ruolo `authenticated` non può scrivere: la migrazione che ha
aggiunto quella colonna l'ha riconcessa **solo in UPDATE**.

Tre risultati osservabili:

1. Una nuova migrazione che concede l'INSERT su `blind`, pronta perché **l'utente la
   esegua in produzione**. Non eseguirla tu, non provare a connetterti al database.
2. I due script di verifica RLS delle collezioni inseriscono **come inserisce
   l'applicazione**, cioè con `blind` fra le colonne. Oggi non lo fanno, ed è il motivo per
   cui il difetto è sopravvissuto due settimane: hanno collaudato un percorso che l'app non
   usa.
3. Un test in `Eton.Tests` che fallisce quando un modello invia in INSERT una colonna che
   nessuna migrazione ha concesso. Senza database, a ogni `dotnet test`.

## PERIMETRO — file di tua proprietà esclusiva

- `supabase/migrations/20260903000000_grant_insert_blind.sql` — **da creare**, con questo
  nome esatto.
- `supabase/verifica-rls-collezioni.sql`
- `supabase/verifica-rls-voto-al-buio.sql`
- Un file nuovo in `Eton.Tests/`, nome a tua scelta coerente con i vicini.

## NON TOCCARE

- **`Models/Collection.cs`.** Deciso: `ignoreOnInsert` e il grant sono complementari, non
  ridondanti. `blind` è una colonna dell'utente, non del trigger, e deve continuare a
  essere inviata. Aggiungere `ignoreOnInsert: true` farebbe nascere ogni collezione col
  valore di default, ignorando in silenzio l'interruttore «Voto al buio» che il modulo di
  creazione mostra attivo. Sarebbe un secondo difetto al posto del primo.
- Le migrazioni esistenti. Una migrazione già applicata non si riscrive.
- Qualunque file sotto `Pages/`, `Services/`, `Shared/`, `wwwroot/`. Appartengono ad altre
  unità.
- Gli altri `verifica-rls-*.sql` (note, recensioni, spese, e quello generale).

## CONTRATTI

**La riga da correggere, citata verbatim.** Da `supabase/migrations/20260812120000_collections.sql`,
il grant di INSERT originale:

```sql
grant insert (space_id, owner_id, name, icon, fields, rating_max) on public.collections to authenticated;
```

Da `supabase/migrations/20260812230000_voto_al_buio.sql`, la riconcessione che ha
dimenticato l'INSERT:

```sql
grant update (name, icon, fields, rating_max, blind) on public.collections to authenticated;
```

**La forma della nuova migrazione è fissata, e devia di proposito dal precedente:**

```sql
grant insert (blind) on public.collections to authenticated;
```

Forma **minima**, non l'elenco completo ripetuto. Due ragioni, ed entrambe vanno in un
commento sopra la riga perché chi legge il diff le trovi:

1. Un GRANT non revoca mai nulla: ripetere l'elenco completo **sembra** dichiarare uno
   stato ma non lo fa, e induce chi legge a credere il contrario.
2. La forma a elenco completo è **quella che ha prodotto questo difetto**. Con la forma
   minima, nella migrazione che aggiunge una colonna, `grant insert (col)` e
   `grant update (col)` stanno affiancati: un `grant update (col)` orfano salta all'occhio,
   mentre `grant update (a, b, c, col)` sembra completo.

`conformity` segnalerà la deviazione dal precedente. È attesa: adjudicala citando questo
paragrafo, non cambiando forma.

**Il test.** Deve confrontare, per ogni `BaseModel` dell'assembly, le colonne inviate in
INSERT (attributo `[Column]` senza `IgnoreOnInsert`, più `[PrimaryKey]` con `ShouldInsert`)
con l'insieme concesso, estratto leggendo i file di `supabase/migrations/` **in ordine di
nome**. Asserzione: insieme del modello ⊆ insieme concesso. Limita il controllo alle tabelle
per cui esiste un `grant insert`: `spaces` e `space_members` nascono da RPC, non da
`Insert`, e non ne hanno uno.

Tre trappole già individuate nel parsing, verificale:

- esiste un grant scritto **su due righe** in `20260824000000_spese.sql`;
- esiste un `grant select, insert on public.profiles` **senza elenco di colonne**, che
  significa *tutte* le colonne e va trattato come tale;
- vanno riconosciuti i `revoke all ... from ... authenticated`, che azzerano l'insieme.

Se una regex non riconosce un grant, il test deve **fallire**, mai passare in silenzio: la
fragilità va tenuta in quella direzione.

## STATO AL RILANCIO — 3 settembre 2026, leggi questo per primo

**Un primo tentativo su questo stesso mandato ha esaurito il budget** dopo aver completato
due obiettivi su tre. Il tetto era sottostimato dal capo, non è colpa della partizione: ora
è più alto. **Non rifare ciò che è già fatto.**

Già su disco, **verificato dal capo, conforme al mandato, non toccarlo**:

- ✅ **Obiettivo 1** — `supabase/migrations/20260903000000_grant_insert_blind.sql` esiste,
  con la forma minima e il commento che spiega la deviazione. Verificato.
- ✅ **Obiettivo 2** — i due `verifica-rls-*.sql` inseriscono ora `blind` in tutti i loro
  `insert into public.collections`, e l'atteso dei privilegi di INSERT è passato da 9 a 10
  colonne con `ins_blind` al posto giusto. Verificato.

**Resta da fare, ed è tutto il tuo lavoro:**

- ❌ **Obiettivo 3** — il test statico in `Eton.Tests`. Non esiste ancora nessun file.
- ❌ **Il resoconto**, che il primo tentativo non ha fatto in tempo a scrivere. Deve
  coprire **tutti e tre** gli obiettivi, non solo il tuo: per i primi due leggi i file su
  disco e riportali come `FATTO`, dichiarando nella riga `SCOSTAMENTI` che sono opera del
  primo tentativo.

Una nota di conformità già istruita dal capo, così non la riapri: negli script
`verifica-rls-*` gli accenti sono resi con apostrofi (`perche'`, `e'`). **È la convenzione
del progetto**, non un difetto — tutti e cinque quegli script hanno zero accenti, mentre le
migrazioni ne hanno da 17 a 32 ciascuna. Non correggerla e non farla segnalare.

## STATO

Sei la **prima** unità del lavoro. Nessuna unità precedente, nessun resoconto da leggere.

Il difetto è istruito per intero in `handoff/01-ricognizione-ui/rilievi.md`, sezione
«0 — Creare una collezione è impossibile»: **leggila**, contiene la catena completa. Il
piano del lavoro è in `handoff/PIANO.md`, sezione «IL QUARTO ANELLO DEL RILIEVO 0» per il
punto 2 del tuo obiettivo.

Nessun'altra unità dipende da te per **implementare**. Tre dipendono da te per
**collaudare**: finché l'utente non esegue la migrazione, `/collections/{id}/items/{id}`
resta irraggiungibile.

## GATE

- `dotnet build` → **0 errori, 0 avvisi**.
- `dotnet test` → tutti verdi, compreso il tuo test nuovo. Erano 258 all'ultimo giro.

Compili **tu**, una volta, a fine giro: gli `implementer` non compilano mai — `obj/` non ha
lock fra processi e due build concorrenti si corrompono a vicenda.

Non avviare il server di sviluppo: questa unità non ha nulla da vedere nel browser.

BUDGET: 4 dollari

RESOCONTO IN: `handoff/02-collezioni-insert/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 02 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <la riga di GRANT reale che hai scritto, citata testualmente con file:line>
ADJUDICA: <per ogni rilievo dei revisori: verdetto, motivo in una riga, riga citata>
FUORI SCOPE: <rilievi fondati che non hai risolto>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge da questo mandato e perché> | nessuno
```

Aggiungi in coda al resoconto, sotto il titolo `DA CONSEGNARE ALL'UTENTE`, il **testo
integrale della migrazione** che dovrà eseguire in produzione: la legge da lì senza aprire
il file.
