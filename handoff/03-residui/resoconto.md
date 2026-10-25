UNITÀ: 3 — ESITO: FATTO
TOCCATI:
  Eton.Tests/ExpenseRepositoryTests.cs → +10/−0
  Eton.Tests/RecurringExpenseRepositoryTests.cs → +118/−0 (nuovo)
  Eton.Tests/SchemaCampiTests.cs → +8/−0
  Services/CalcoliGriglia.cs → +2/−2
  Services/CalcoliRicorrenti.cs → +1/−1
  Services/ExpenseRepository.cs → +16/−2 (solo CambiaCategoriaAsync e l'helper TipiAmmessi subito sopra)
  Services/RecurringExpenseRepository.cs → +17/−14 (fuori dal PERIMETRO iniziale, allargato dal capo: v. SCOSTAMENTI)
  Services/SchemaCampi.cs → +3/−0
  Services/TabellaSpese.cs → +1/−1
  supabase/verifica-rls-budget.sql → +35/−30
  supabase/verifica-rls-collezioni.sql → +34/−30
  supabase/verifica-rls-note.sql → +31/−27
  supabase/verifica-rls-rate-entrate.sql → +28/−23
  supabase/verifica-rls-recensioni.sql → +41/−37
  supabase/verifica-rls-ricorrenti.sql → +26/−21
  supabase/verifica-rls-spese.sql → +24/−20
  supabase/verifica-rls-voto-al-buio.sql → +24/−19
  supabase/verifica-rls.sql → +12/−8
  handoff/03-residui/ → brief-codex-1.md, brief-codex-2.md, codex-1.md, codex-2.md, resoconto.md (registro dell'unità)
REVIEW:
  review: brief-codex-1 — i quattro residui
    autore          codex exec codex-cli 0.161.0 — 18 files changed, 413 insertions(+), 235 deletions(-) (senza handoff/)
    bug-hunter      RILIEVI: 0
    conformity      RILIEVI: 3
    threat-hunter   RILIEVI: 0
    backend-expert  RILIEVI: 4
    test rosso      RecurringExpenseRepositoryTests: RecurringExpenseRepositoryTests.cs(20,49): error CS0117: 'RecurringExpenseRepository' non contiene una definizione per 'NuovaRegola'
    test rosso      ExpenseRepositoryTests.TipiAmmessi: ExpenseRepositoryTests.cs(140,63): error CS0117: 'ExpenseRepository' non contiene una definizione per 'TipiAmmessi'
    checker         VERDETTI: fondati 3 · infondati 0 · fuori scope 0 · non verificabili 0
                    ROSSO: confermati 2 · non confermati 0 · non verificabili 0
  review: brief-codex-2 — correzioni C1 C2 C3 BE2 BE4
    autore          codex exec codex-cli 0.161.0 (resume) — 18 files changed, 431 insertions(+), 235 deletions(-) (diff cumulativo dell'unità, senza handoff/)
    checker         fix: risolti 4 · non risolti 0 · non verificabili 0
    live-testing    non lanciato — nessun .razor/.css toccato, nessuna modifica all'interfaccia (RESA del mandato: nessuna modifica visiva)
    ui-critic       non lanciato — tabella §7 riga 1 — 18 files changed, 431 insertions(+), 235 deletions(-)
CONTRATTI:
  Services/ExpenseRepository.cs:362 `public async Task<EsitoMassa> CambiaCategoriaAsync(IReadOnlyCollection<Guid> ids, string categoria)` — firma invariata; consumata da Pages/SpeseTabella.razor:676.
  Services/ExpenseRepository.cs:346 `internal static IReadOnlyList<string> TipiAmmessi(string categoria) =>` (nuovo, su `CategorieSpesa.Conosciuta`).
  Services/RecurringExpenseRepository.cs:58 `internal static RecurringExpense NuovaRegola(Guid spazioId, Guid pagante, DatiRegola dati) =>` (nuovo); CreaAsync/SalvaAsync/TerminaAsync con firme invariate.
  Services/RecurringExpenseRepository.cs:127 `internal static async Task<RisultatoSalvataggio<RecurringExpense>> Esito(IReadOnlyList<RecurringExpense> toccate, int versioneLetta, Func<Task<RecurringExpense?>> rileggi)` (era privato d'istanza).
  Services/SchemaCampi.cs:187 `internal static string ChiaveConfronto(string testo) => RiduciAccenti(testo).ToUpperInvariant();` — chiamata da Services/CalcoliGriglia.cs:33-34, Services/CalcoliRicorrenti.cs:269, Services/TabellaSpese.cs:242 (`SchemaCampi.ChiaveConfronto(testo.Trim())`, Trim lasciato alla ricerca). SchemaCampi.ChiaveDa non toccata.
  supabase/verifica-rls*.sql — in testa a ogni script `\set claims_<nome> '{"sub":"<uuid>","role":"authenticated"}'`, usate come `set local request.jwt.claims = :'claims_<nome>';` (215 sostituzioni, 0 letterali rimasti).
ADJUDICA:
- conformity C1: fondato → corretto — guardia `Enumerable.Empty` in un ternario nella sorgente del foreach, unica nel repo — `foreach (var blocco in (tipi.Count == 0 ? Enumerable.Empty<Guid[]>() : richieste.Chunk(IdPerRichiesta)))`
  verificato: risolto — Services/ExpenseRepository.cs:372
- conformity C2: fondato → corretto — `kind` già coperto da PrivilegiInsertTests.cs:136-145; space_id/paid_by no — `foreach (var nome in new[] { "space_id", "paid_by", "kind" })`
  verificato: risolto — Eton.Tests/RecurringExpenseRepositoryTests.cs:103
- conformity C3: fondato → corretto — unica estrazione per i test senza commento sul perché — `internal static RecurringExpense NuovaRegola(Guid spazioId, Guid pagante, DatiRegola dati) =>`
  verificato: risolto — Services/RecurringExpenseRepository.cs:57
- backend-expert BE1: fuori scope — TIPO: progetto, va all'utente: la classificazione Salvata/Sparita/Conflitto/Rifiutata è copiata in sette repository e la proposta è un `Esito<T>` generico unico in RisultatoSalvataggio.cs con un'interfaccia `IVersionato`; tocca sei repository fuori perimetro — `internal static async Task<RisultatoSalvataggio<RecurringExpense>> Esito(...)`
- backend-expert BE2: fondato → corretto — stessa riga di C1, guardia nominata con commento e filtro kind costruito una volta — `foreach (var blocco in (tipi.Count == 0 ? Enumerable.Empty<Guid[]>() : richieste.Chunk(IdPerRichiesta)))`
  verificato: risolto — Services/ExpenseRepository.cs:372
- backend-expert BE3: fuori scope — la regola «quali tipi accettano questa categoria» starebbe meglio in CategorieSpesa accanto a Di/Conosciuta, ma Services/CategorieSpesa.cs non è nel PERIMETRO e la posizione attuale funziona — `internal static IReadOnlyList<string> TipiAmmessi(string categoria) =>`
- backend-expert BE4: fondato → corretto — un test di 35 righe con due scenari e un foreach; spezzato in tre Fact — `foreach (var r in new[] { regola, piano })`
  verificato: risolto — Eton.Tests/RecurringExpenseRepositoryTests.cs:18
FUORI SCOPE:
  - backend-expert BE1 (progetto, per l'utente): un `Esito<T>` generico unico per i sette repository al posto delle sette copie della distinzione Salvata/Sparita/Conflitto/Rifiutata.
  - backend-expert BE3: spostare TipiAmmessi in CategorieSpesa (es. `CategorieSpesa.TipiDi`) e il suo test in CategorieSpesaTests.
  - Il testo per una riga di tipo sbagliato nel cambio di massa resta «non era tua» (è la voce Rifiutate dell'EsitoMassa esistente, come chiedeva il mandato); dalla pagina il caso non si raggiunge (v. DA RATIFICARE).
GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\03-residui → build: succeeded — 0 errors, 4 warnings — 3.0s
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\03-residui → test: 0 failed, 442 passed, 0 skipped — 5.1s
PROVA SCRIPT RLS (Supabase locale, Docker acceso solo per la prova e poi spento con `docker desktop stop`): per ciascuno dei nove script `supabase db reset`, poi `docker exec -i supabase_db_Eton psql -U postgres -d postgres -f -` con la versione di HEAD e con quella nuova. Output confrontato dopo aver normalizzato uuid, numeri di riga di psql, codici d'invito e timestamp: identico per 6 script; budget e verifica-rls differiscono solo per un timestamp e un codice d'invito casuale; spese per l'intreccio fra stdout e stderr, e rilanciato coi due flussi separati è identico su entrambi. Conteggio degli ERROR identico in ogni script.
SCOSTAMENTI:
  - Domanda al capo, 2026-10-25 02:10: «per i test di RecurringExpenseRepository posso toccare Services/RecurringExpenseRepository.cs (fuori PERIMETRO) per estrarre punti statici testabili?» (opzione A: estrarre la costruzione della regola di CreaAsync ed Esito con la rilettura per delegate; opzione B: non toccarlo e lasciare scoperta la concorrenza). Risposta di coordinatore-Eton, 2026-10-25 02:10: «opzione A. Il PERIMETRO si allarga a Services/RecurringExpenseRepository.cs, solo per estrarre i due punti statici internal che descrivi […] Nessun cambio di comportamento, nessuna firma pubblica cambiata, revisione come il resto.» Timer fermato.
  - Services/RecurringExpenseRepository.cs toccato fuori dal PERIMETRO del mandato, per la risposta qui sopra.
  - Codex nel primo giro: due `dotnet build` falliti per accesso negato a un file temporaneo di code coverage, poi `dotnet build -m:1` riuscito (dal suo resoconto e dal log). Nessuna forma vietata nei log dei due lanci (checker).
DECISIONI DELL'UTENTE QUI: nessuna
DA RATIFICARE:
  - Allargamento del PERIMETRO a Services/RecurringExpenseRepository.cs (chiesto dal capo di scriverlo qui) — raccomando di ratificarlo: solo due estrazioni senza cambi di comportamento, revisionate e coperte da test.
  - Le righe dell'altro tipo nel cambio di categoria di massa si contano fra le «Rifiutate» e la frase dice «non era tua», che è imprecisa — raccomando di tenerla così: la pagina non manda mai selezioni miste, il caso è solo una difesa, e cambiare la frase vorrebbe dire aggiungere un campo a EsitoMassa.
  - Claims JWT «in un posto solo» letto come «uno per script» (blocco `\set` in testa a ogni file), non un file comune: gli script si lanciano via stdin nel container e un `\i` verso un file comune non lo troverebbe — raccomando di ratificarlo così.
SEGNALAZIONI: nessuna
COMMIT: 39785e652e8720947936251f08173e70e5914509
