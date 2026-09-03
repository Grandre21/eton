UNITÀ: 02 — ESITO: FATTO

TOCCATI:
- `supabase/migrations/20260903000000_grant_insert_blind.sql` → +24/−0 (nuovo) — primo tentativo
- `supabase/verifica-rls-collezioni.sql` → +24/−17 — primo tentativo
- `supabase/verifica-rls-voto-al-buio.sql` → +21/−5 — primo tentativo
- `Eton.Tests/PrivilegiInsertTests.cs` → +196/−0 (nuovo) — questo giro
- `handoff/02-collezioni-insert/brief-test-privilegi.md` → +150/−0 (nuovo) — documento di lavoro, non codice

CONTRATTI:
La riga di GRANT reale, `supabase/migrations/20260903000000_grant_insert_blind.sql:24`:

```sql
grant insert (blind) on public.collections to authenticated;
```

Forma minima, come fissato dal mandato. Le due ragioni della deviazione dal precedente di
`20260812230000_voto_al_buio.sql:109` sono scritte nel commento immediatamente sopra, righe
15-23 dello stesso file, perché chi legge il diff le trovi senza cercare altrove.

Nessun altro contratto è stato prodotto o consumato: nessuna unità dipende da questa per
implementare.

ADJUDICA:

Revisori lanciati: `bug-hunter`, `conformity`, `backend-expert`. **`threat-hunter` non
lanciato**: il diff è un test che legge file del repository e attributi di reflection —
nessun input esterno, nessuna query composta, nessun render, nessun authn/authz, nessuna
(de)serializzazione. Non ho esitato, quindi non l'ho lanciato; lo dichiaro perché la scelta
sia auditabile.

**istruttoria: 2 rilievi su 1 file → checker no** (`bug-hunter` 0 + `conformity` 2; soglia
≥ 4 rilievi o ≥ 3 file distinti, nessuna delle due raggiunta).

`bug-hunter` — 0 rilievi. Ha ricostruito a mano l'output del parser per tutti e ~30 gli
statement `grant`/`revoke` dei sette file di migrazione e confermato che gli insiemi
coincidono con gli attesi colonna per colonna: i quattro test passano per le ragioni
giuste, non per coincidenza.

`conformity` — 2 rilievi, entrambi **fondati e corretti**:
1. gli helper privati stavano dopo i test, mentre in tutti i vicini precedono — prova a
   `PermessiTests.cs:8`, `CalcoliSpeseTests.cs:8`, `CalcoliVotiTests.cs:8`. Classe
   riordinata.
2. `Assert.Equal(new[] { … })` invece della collection expression — prova a
   `SchemaCampiTests.cs:189` e `CalcoliSpeseTests.cs:59`. Ho riverificato io il claim,
   perché suonava più largo di quanto fosse: `new[]` compare 26 volte nella suite, ma
   **sempre** per costruire i dati d'ingresso; come valore *atteso* di `Assert.Equal` la
   suite usa `[...]` in 2 casi su 2. La convenzione disattesa è quella. Corretto.

`backend-expert` — 5 rilievi, **tutti fondati, tutti accolti**, nessuno `TIPO: progetto`
(quindi nessuno da portare all'utente come decisione):
1. `PrivilegiInsertTests.cs:140` — due regex quasi identiche e un flag `revoca: bool` che
   attraversava il confine di metodo. Unificate in `PatternTabella` con `verbo` e `dir`
   catturati; `ApplicaStatement` ha un solo call-site e nessun flag.
2. `:188` — «niente concesso» aveva due rappresentazioni (chiave assente, set vuoto) e il
   predicato scritto due volte, dritto e negato. Il revoke ora fa `Remove`, i due filtri
   fanno `ContainsKey`.
3. `:169` — `List<string>?` usato come flag di controllo di flusso. Early return, 12 righe
   → 5, `UnionWith` al posto del `foreach`.
4. `:124` — due meccanismi di fallimento per la stessa classe di errore. Unificato.
5. `:96` — `Assert.Contains(Tutte, concesse)` asseriva la rappresentazione interna, già
   coperta dal comportamento della riga successiva. Rimossa.

**Nessun rilievo infondato in tutta l'unità**, quindi il campione di riverifica sugli
infondati (§5 del CLAUDE.md) non si applica: lo dichiaro invece di ometterlo. Nessun
rilievo toccava sicurezza, dati o concorrenza.

Ho aperto io il codice, senza passare da nessuno, su un punto: il rilievo 4 di
`backend-expert` proponeva di unificare i fallimenti su `Assert.Fail`, e **ho accolto il
rilievo deviando dalla riscrittura** — l'ho unificato su `throw new
InvalidOperationException`, cioè nella direzione opposta. Due motivi. Tutti e tre i punti
di fallimento stanno nell'inizializzatore statico del campo `Concesso`
(`PrivilegiInsertTests.cs:26`), mai dentro un metodo di test: lì `Assert.Fail` non porta il
vantaggio che avrebbe in un'asserzione, perché il CLR avvolge comunque tutto in
`TypeInitializationException`. E `Assert.Fail` restituisce `void` senza essere annotato
`[DoesNotReturn]` in xUnit 2.9.3: il compilatore avrebbe continuato a trattare `cartella`
come possibilmente null subito dopo, producendo un avviso `CS86xx` — e il gate di questa
unità è 0 avvisi.

Stessa cosa sul rilievo 1: ho accolto la ristrutturazione ma nella **variante stretta** che
il revisore stesso offriva, con verbo *e* direzione catturati e un controllo di coerenza
(`PrivilegiInsertTests.cs:93`). Un `(?:to|from)` non alternato al verbo avrebbe accettato
`grant … from`, che non è SQL valido: in un parser il cui mandato è fallire su ciò che non
riconosce, allargare la grammatica sarebbe il difetto opposto a quello che si stava
correggendo. Ho scartato invece la sotto-proposta di alimentare i test 3 e 4 con SQL
letterale invece che con i file veri: quei due test esistono per verificare il parsing dei
dati reali del repository, ed è ciò che il mandato chiede quando dice «tre trappole già
individuate nel parsing, **verificale**».

FUORI SCOPE: nessuno.

GATE:
- `dotnet build Eton.Tests/Eton.Tests.csproj` → **0 errori, 0 avvisi**
- `dotnet test Eton.Tests/Eton.Tests.csproj` → **267 superati, 0 non superati** (erano 258:
  +9, cioè 6 casi del `[Theory]` — una per tabella con `grant insert` — più 3 `[Fact]`)

Due prove di mutazione, entrambe eseguite sul codice **dopo** le correzioni, entrambe con
ripristino verificato:
- rimosso `20260903000000_grant_insert_blind.sql` → 1 test rosso,
  `collections: colonne inviate in INSERT ma non concesse: blind`. È il difetto reale, con
  il nome della colonna che PostgreSQL non dice quando risponde 42501.
- aggiunta una migrazione temporanea con `grant insert (foo) on collections to
  authenticated;` (tabella non qualificata, quindi non riconoscibile) → 4 test rossi su 4
  con `InvalidOperationException: Statement grant/revoke non riconosciuto in …`. Nessun
  verde silenzioso: è la regola non negoziabile del mandato, e ora è dimostrata, non
  asserita. Il file di prova è stato cancellato; `supabase/migrations/` contiene le sette
  migrazioni vere e nient'altro.

Non ho avviato il server di sviluppo: questa unità non ha nulla da vedere nel browser.

SCOSTAMENTI:

1. **Gli obiettivi 1 e 2 sono opera del primo tentativo**, non di questo giro. Li ho
   verificati sul disco e li riporto come `FATTO`: la migrazione ha la forma minima
   prescritta e il commento che spiega la deviazione; i due `verifica-rls-*.sql`
   inseriscono `blind` in tutti i loro `insert into public.collections` e l'atteso dei
   privilegi di INSERT è passato da 9 a 10 colonne con `ins_blind` al posto giusto
   (`verifica-rls-collezioni.sql:58,65`). Non li ho fatti rivedere: erano già stati
   verificati dal capo, e il mandato dice di non toccarli.

   Una cosa che il primo tentativo ha fatto meglio di quanto il mandato chiedesse, e che
   vale la pena non perdere: in `verifica-rls-voto-al-buio.sql` aggiungere `blind` agli
   insert avrebbe fatto sparire la copertura dell'**UPDATE** su `blind`, che prima era
   l'unico modo in cui la cecità si accendeva nello script. La correzione crea invece la
   collezione già cieca *e* aggiunge uno spegni-riaccendi con due UPDATE, che lasciano
   l'impianto nello stato esatto in cui le sezioni successive lo trovavano.

2. **Deviazione dalla riscrittura del rilievo 4 di `backend-expert`**, motivata sopra in
   ADJUDICA: rilievo accolto, forma opposta a quella proposta, per non introdurre un
   avviso del compilatore.

3. **Un file in più nel perimetro**: `handoff/02-collezioni-insert/brief-test-privilegi.md`,
   il brief dato all'`implementer`. Non era elencato nel mandato, ma sta nella cartella di
   questa unità ed è un documento, non codice. L'ho scritto su disco invece che solo in
   chat perché il riferimento alle regex e agli esiti attesi sopravvivesse a una
   compaction, e perché i revisori potessero misurare il codice contro il budget di
   complessità dichiarato invece che contro il proprio gusto.

4. **`doc-checker` consultato una volta**, non richiesto dal mandato ma dovuto per il caso 2
   del CLAUDE.md: il test legge via reflection proprietà di attributi di
   `Supabase.Postgrest` che un `Grep` non trova mai usate nel codebase. Ha verificato
   `ColumnAttribute.ColumnName` / `.IgnoreOnInsert`, `PrimaryKeyAttribute.ColumnName` /
   `.ShouldInsert`, `TableAttribute.Name` e la **non**-ereditarietà fra `PrimaryKeyAttribute`
   e `ColumnAttribute` contro il sorgente del commit `2bc8266`, che è quello dichiarato nel
   `.nuspec` del pacchetto 4.4.0 installato — non contro l'ultima versione pubblicata. Tutti
   e cinque i claim veri. La non-ereditarietà è la ragione per cui il test interroga i due
   attributi con due query separate (`PrivilegiInsertTests.cs:146,150`) invece di una sola.

---

# DA CONSEGNARE ALL'UTENTE

Questa è la migrazione da eseguire in produzione, sul database
`fdqedhgvpneuybtykamf.supabase.co`. **Nessun agente l'ha eseguita e nessuno si è connesso al
database.** Finché non è eseguita, creare una collezione continua a fallire con
`permission denied for table collections` (SQLSTATE 42501), e le unità 05, 09 e 11 non sono
collaudabili nel browser.

È idempotente e rieseguibile: un GRANT ripetuto non fa danno. Testo integrale del file
`supabase/migrations/20260903000000_grant_insert_blind.sql`:

```sql
-- =====================================================================================
-- Eton — collezioni: concede l'INSERT sulla colonna blind, mancante dal 12 agosto 2026.
-- Da allora ogni Insert<Collection> fallisce con 42501 permission denied for table
-- collections: il client invia blind in ogni INSERT — l'unica colonna scrivibile
-- dall'utente senza ignoreOnInsert (v. Models/Collection.cs) — ma
-- 20260812230000_voto_al_buio.sql, che ha aggiunto la colonna, l'aveva riconcessa solo
-- in UPDATE.
-- Idempotente e rieseguibile.
--
-- Dipende da 20260812120000_collections.sql, che ha concesso l'INSERT colonna per
-- colonna, e da 20260812230000_voto_al_buio.sql, che ha aggiunto blind riconcedendo solo
-- l'UPDATE.
-- =====================================================================================

-- Elenco minimo di una sola colonna, non l'intero elenco ripetuto come fa invece
-- l'UPDATE di 20260812230000_voto_al_buio.sql:109, per due motivi. Primo: un GRANT non
-- revoca mai nulla, quindi ripetere l'elenco completo sembra dichiarare lo stato attuale
-- dei privilegi ma non lo fa — induce chi legge a credere il contrario. Secondo: è
-- proprio la forma a elenco completo che ha prodotto questo difetto. Con la forma
-- minima, nella migrazione che aggiunge una colonna, grant insert (col) e
-- grant update (col) stanno affiancati: un grant update (col) orfano salta all'occhio,
-- mentre grant update (a, b, c, col) sembra completo anche quando manca il grant insert
-- gemello.
grant insert (blind) on public.collections to authenticated;
```

Se preferisci incollare solo l'essenziale nell'editor SQL di Supabase, la riga che conta è
l'ultima:

```sql
grant insert (blind) on public.collections to authenticated;
```

Come verificare che sia andata, senza aprire l'applicazione — deve restituire `t`:

```sql
select has_column_privilege('authenticated', 'public.collections', 'blind', 'INSERT');
```
