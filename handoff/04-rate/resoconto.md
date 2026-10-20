UNITÀ: 4 — ESITO: BLOCKED: posso riprendere le prove nel browser (live-testing da capo, poi ui-critic) ed eliminare il piano di prova «PROVA COLLAUDO rate»?

Codice, test, revisori e istruttorie sono fatti e committati. Manca solo il §7: il capo ha ordinato di fermare le prove
nel browser («FERMATI con le prove nel browser, su richiesta dell'utente») mentre `live-testing` girava; gli ho risposto
con lo stato e ho aspettato 30 minuti senza risposta. Il piano di prova è rimasto in produzione, come chiesto.

TOCCATI:
- Eton.Tests/GruppiRicorrentiTests.cs → +7/−0
- Eton.Tests/PianiARateTests.cs (nuovo) → +135/−0
- Models/RecurringExpense.cs → +4/−0
- Pages/RicorrenteEdit.razor → +160/−70
- Pages/Ricorrenti.razor → +7/−1
- Pages/Spese.razor → +63/−6
- Services/CalcoliRicorrenti.cs → +66/−17
- Services/RecurringExpenseRepository.cs → +15/−6
- handoff/04-rate/ → brief-codex-1..4, codex-1..4, server.md, questo resoconto

REVIEW:
review: brief-codex-2 (+3) — spese a rate
  autore          codex exec 0.161.0 — 8 files changed, 393 insertions(+), 60 deletions(-) (primo giro); dopo la correzione: 8 files changed, 457 insertions(+), 100 deletions(-)
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 4
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 4
  test rosso      PianiARateTests: G:\Sviluppo\wt-Eton\04-rate\Eton.Tests\PianiARateTests.cs(11,48): error CS0117: 'CalcoliRicorrenti' non contiene una definizione per 'ImportoRata' [G:\Sviluppo\wt-Eton\04-rate\Eton.Tests\Eton.Tests.csproj]
  test rosso      GruppiRicorrentiTests.Quota_somma_i_totali_di_gruppo_arrotondati: Assert.Equal() Failure: Values differ / Expected: 41.66 / Actual: 41.67
  checker         VERDETTI: fondati 4 · infondati 1 · fuori scope 0 · non verificabili 0
                  ROSSO: confermati 1 · non confermati 1 · non verificabili 0
  checker         VERDETTI: risolti 5 · non risolti 0 · non verificabili 0
                  ROSSO: confermati 1 · non confermati 0 · non verificabili 0
  live-testing    FORMATO ROTTO — nessuna riga di conteggio (fermato con TaskStop su ordine del capo prima del report)
  ui-critic       non lanciato — prove nel browser fermate dal capo; tabella §7 riga 2 (markup modificato in .razor esistenti, nessun file .razor nuovo, nessun .css, nessun metro) — 8 files changed, 457 insertions(+), 100 deletions(-); il mandato lo chiede comunque, quindi resta da fare.

Note al tracciato:
- Il primo `ROSSO` non confermava la seconda riga: nel primo lancio il test della quota non compilava per colpa di un
  altro file, quindi non era mai fallito sulla propria asserzione. Nel brief di correzione Codex ha rimesso per un
  momento il calcolo vecchio, ha visto il test fallire (41,67 contro 41,66) e ha rimesso il nuovo; il secondo `checker`
  l'ha confermato su codex-4.log:153-158 e ha verificato il ripristino identico.
- `live-testing`, prima dello stop, aveva verificato (dal suo transcript, non da un report): avviso «Piano a rate
  creato: 3 rate da 33,33 €. Vai alle ricorrenti», gruppo «A rate» con «rata 1/3» e «residuo 100,00 €», editor del
  piano salvato a 4 rate («4 rate da 25,00 €», PATCH 200, poi «rata 1/4»). Non è una prova di accettazione: va rifatta.

CONTRATTI:
- invariati nella firma: `Services/CalcoliRicorrenti.cs:27` `public sealed record GruppoDiRicorrenti(GruppoRicorrenti Gruppo, IReadOnlyList<RecurringExpense> Regole,`;
  `Services/CalcoliRicorrenti.cs:242` `public static GruppoRicorrenti Gruppo(RecurringExpense r, DateTime oggi)`;
  `Services/CalcoliRicorrenti.cs:258` `public static IReadOnlyList<GruppoDiRicorrenti> Raggruppa(IReadOnlyList<RecurringExpense> regole, DateTime oggi)`;
  `Services/CalcoliRicorrenti.cs:272` `public static decimal QuotaDaAccantonare(IReadOnlyList<RecurringExpense> regole, DateTime oggi)`
  (ora somma i `TotaleMensile` già arrotondati di Annuali e OgniNMesi); `Services/CalcoliRicorrenti.cs:147`
  `public static Expense Occorrenza(RecurringExpense r, PeriodoDovuto d)` (punto unico di suffisso e resto);
  `Services/RecurringExpenseRepository.cs:49` `public async Task<RecurringExpense> CreaAsync(Guid spazioId, Guid pagante, DatiRegola dati)`;
  `Services/RecurringExpenseRepository.cs:80` `public async Task<RisultatoSalvataggio<RecurringExpense>> SalvaAsync(Guid regolaId, int versioneLetta, DatiRegola dati)`.
- cambiati, per l'unità 5:
  - `Services/CalcoliRicorrenti.cs:20` `public enum GruppoRicorrenti { Mensili, ARate, Annuali, OgniNMesi, Terminate }` —
    `OgniNMesi` resta col suo nome (in pagina il titolo è «Periodiche»); «Entrate» va prima di `Mensili`.
  - `Gruppo`: Terminate prima, poi `if (r.Installments is not null) return GruppoRicorrenti.ARate;`, poi lo switch:
    il ramo per kind va accanto a quello delle rate, prima dello switch.
  - `Services/RecurringExpenseRepository.cs:11` `public sealed record DatiRegola(decimal Importo, string Descrizione, string Categoria, int OgniMesi, int Giorno, DateTime Inizio, int? Rate = null, decimal? TotalePiano = null);`
    — `Kind` si aggiunge in coda allo stesso modo.
  - `Models/RecurringExpense.cs:37-38` `[Column("installments")] public int? Installments` e `[Column("plan_total")] public decimal? PlanTotal`; `Kind` non aggiunto.
- nuovi: `Services/CalcoliRicorrenti.cs:23` `public sealed record StatoPiano(int Prossima, int Rate, decimal Residuo);`;
  `:172` `ImportoRata(decimal totale, int rate)`; `:175` `public const int DescrizioneMassimaPiano = 187;`;
  `:178` `RataValida(decimal totale, int rate)`; `:181` `FinePiano(DateTime inizio, int rate)`;
  `:184` `public static DatiRegola DatiPiano(decimal totale, int rate, string descrizione, string categoria, DateTime inizio)`;
  `:188` `NumeroRata(RecurringExpense r, DateTime data)`; `:191` `public static StatoPiano? Stato(RecurringExpense r, DateTime oggi)`;
  `:202` `public static bool PianoPartito(RecurringExpense r, DateTime oggi)`.
- Per l'unità 5: il modulo del Registro (`Pages/Spese.razor`) ha ora `aRate`/`numeroRate`; la scelta «Spesa / Entrata»
  deve nascondere l'interruttore e spegnerlo (`aRate = false`) quando si passa a «Entrata».

ADJUDICA:
- bug-hunter BH1: fondato → corretto — il suffisso « · rata k/N» (fino a 13 caratteri) faceva superare i 200 del check di expenses.description e la rata non si materializzava mai — `descrizione = $"{r.Description} · rata {k}/{n}";`
  verificato: risolto — Services/CalcoliRicorrenti.cs:175 («`public const int DescrizioneMassimaPiano = 187;` … 187+13=200 … guardia in `DatiDaSalvare`, prima del ramo `Bloccato`, quindi vale anche per i piani bloccati»)
- conformity C1: fondato → corretto — blocco Termina copiato due volte nella sezione «Fermare» — `<ConfermaAzione Chiave="@Id" Etichetta="Termina" …/>`
  verificato: risolto — Pages/RicorrenteEdit.razor:240-263 («La stringa «Terminandola…» (riga 250) e la `ConfermaAzione` Termina (riga 252) compaiono una sola volta nel file»)
- conformity C2: fondato → corretto — contenuto dei nuovi `@if` non rientrato — `@if (!Piano)`
  verificato: risolto — Pages/RicorrenteEdit.razor:132-164 e 241-263 («rientrato di un livello rispetto alla graffa (12 spazi contro 8)»)
- conformity C3: infondato — la convenzione dei nomi degli inject non è uniforme: Pages/CollectionDetail.razor:4-5 usa `Elementi` e `Recensioni` — `@inject RecurringExpenseRepository Ricorrenti`
- conformity C4: fondato → corretto — soglia del centesimo ripetuta in 5 punti — `CalcoliRicorrenti.ImportoRata(importo, numeroRate) < 0.01m`
  verificato: risolto — Services/CalcoliRicorrenti.cs:178 («Grep di `0\.01m` … in Pages/ nessuna occorrenza»)
- backend-expert BE1: fondato → corretto — validità calcolata due volte, in `Valido` e in `DatiDaSalvare` — ora `private bool Valido => DatiDaSalvare() is not null;`
  verificato: risolto — Pages/RicorrenteEdit.razor:339 («Per una regola non-piano la validità coincide con HEAD … Salva … con `Nuova` chiama `Crea(dati.Importo)` … Sovrascrivi usa `var dati = DatiDaSalvare()`»)
- backend-expert BE2: fondato → corretto — quattro livelli di `@if` e blocco Termina duplicato (stesso difetto di C1, stessa correzione) — `var attiva = Piano ? … : regola!.EndsOn is null;`
  verificato: risolto — Pages/RicorrenteEdit.razor:240-263 (stessa istruttoria di C1)
- backend-expert BE3: fuori scope — un componente `Shared/NumeroRate.razor` condiviso da Registro ed editor: è un'astrazione nuova che il brief vietava, va decisa dall'utente; la soglia è già unificata da C4 — `<select @bind="numeroRate" …>`
- backend-expert BE4: fuori scope — TIPO progetto: un overload `Prossima(RecurringExpense r, DateTime oggi)` al posto dei cinque argomenti presi dalla stessa regola in 4 punti — `Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, oggi)`
- Campione sugli infondati: C3 riverificato da me su Pages/CollectionDetail.razor:4-5 (`@inject CollectionItemRepository Elementi`, `@inject ReviewRepository Recensioni`) — confermato infondato.
- threat-hunter: 0 rilievi; ha segnalato fuori mandato lo stesso difetto di BH1, già corretto.

FUORI SCOPE:
- backend-expert BE3 (per l'utente): componente unico per «Numero di rate» con anteprima della rata, usato da Registro ed editor.
- backend-expert BE4 (TIPO progetto, per l'utente): overload di `Prossima` che prende la regola; toglierebbe anche il wrapper privato di Pages/Ricorrenti.razor.

GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\04-rate → succeeded — 0 errors, 4 warnings | mcp__synapse__test root=G:\Sviluppo\wt-Eton\04-rate → 0 failed, 407 passed, 0 skipped | live-testing → non completato, fermato dal capo

SCOSTAMENTI:
- **Prove nel browser fermate.** Alle ~15:37 è arrivato dal capo (coordinatore-Eton, messaggio fra sessioni): «FERMATI
  con le prove nel browser, su richiesta dell'utente (chat, 20 ott: «ferma un attimo le prove nel browser»). […] Se hai
  creato dati «PROVA COLLAUDO», lasciali dove sono e scrivimi quali sono.» Eseguito subito (`TaskStop` su live-testing).
  Risposta al capo con lo stato alle ~15:40; timer di 30 minuti scaduto senza risposta → BLOCKED.
- **Dato di prova rimasto in produzione**, lasciato su ordine del capo: regola «PROVA COLLAUDO rate», id
  `137fde18-9b3e-454b-8e1e-965833d930b5`, spazio «COLLAUDO GIRO B TEST», 100 € in 4 rate dal 15 novembre 2026, nessuna
  spesa materializzata: si elimina dall'editor della ricorrente. Va eliminato prima di chiudere il ciclo.
- **Residui nel Chrome d3148d48…**: lo spazio attivo può essere rimasto «COLLAUDO GIRO B TEST» (il ripristino era a fine
  prova); la scheda id 780391175 è rimasta aperta.
- **Server di sviluppo**: avviato (31536/26772, porta 5000) e fermato a unità chiusa, porta verificata libera.
- **Due domande di Codex** (BLOCKED, dalla stessa sessione dell'unità, non dal capo): (1) «quale percorso devo usare per
  il file `-o`» → risposto nel brief-codex-2 (il file lo scrive la riga di comando); (2) «ogni piano ha già `EndsOn`
  valorizzato […] il blocco Termina non offre il pulsante» → difetto vero del mio brief, risposto in brief-codex-3
  (Termina per un piano partito e ancora attivo, «Concluso il» quando è finito).
- **Il file `-o`** non è comparso nella radice del worktree in nessuno dei quattro lanci: Codex ha scritto direttamente
  `codex-<n>.md`.
- **Nessuna scrittura con PowerShell inline** né interpreti nei log di Codex (verificato dai due `checker`): solo
  `apply_patch`. Al brief ho aggiunto una riga che vieta `Set-Content`, `Out-File`, `[System.IO.File]::Write*` (lezione
  dell'unità 02), oltre alle tre righe verbatim.
- Log di Codex: `C:\Users\Andre!\.claude\jobs\4ae8f623\tmp\codex-1..4.log`, fuori da `handoff/`.
- Domande al capo: una (quella del BLOCKED), inviata ~15:40, nessuna risposta entro ~16:10.

DECISIONI DELL'UTENTE QUI: nessuna

DA RATIFICARE:
- Un piano conta come «partito» anche se la data della prima rata è arrivata ma la rata non è ancora stata scritta (chi
  paga non ha aperto Eton): `MaterializedThrough is not null || StartsOn <= oggi`. Raccomando di tenerlo: la spec dice
  «almeno una rata materializzata», ma così una rata già dovuta non cambia importo sotto i piedi.
- Prima della prima rata il piano non offre «Termina» (si elimina). Raccomando di tenerlo: terminare un piano che non è
  partito equivale a eliminarlo, e terminarlo poi modificarlo lo riaprirebbe.
- Nella riga del gruppo «A rate», «rata k/N» indica la **prossima** rata e il residuo la comprende; una rata che cade oggi
  conta già pagata. Raccomando di tenerlo.
- Dopo «Segna» con «a rate» l'interruttore si spegne e il numero di rate resta quello scelto. Raccomando di tenerlo: la
  spesa successiva è quasi sempre singola.
- Descrizione di un piano limitata a 187 caratteri (200 meno « · rata 60/60»), con il messaggio accanto al campo.
  Raccomando di tenerlo: è la soluzione che non tronca mai il testo salvato.

SEGNALAZIONI:
STATO: aperta
DA: Eton, sessione-unità 04-rate, 2026-10-20
INNESCO: Codex autore, brief con la clausola verbatim «Il tuo resoconto va solo nel file `-o`»
Cosa è successo: al primo lancio Codex si è fermato con «BLOCKED: quale percorso devo usare per il file `-o` del resoconto?»,
senza toccare nulla: un giro sprecato.
Perché: la clausola verbatim nomina «il file `-o`» senza dire che lo scrive la riga di comando dal messaggio finale; nell'unità 01
la stessa frase faceva creare un file chiamato `-o`, qui ha fatto fermare Codex. Due esiti sbagliati diversi dalla stessa frase.
Proposta: nella «Regola per il mandato» cambiare la riga verbatim in «Il tuo resoconto è il tuo messaggio finale: lo salva da sé
l'opzione `-o` della riga di comando. Non scrivere in `handoff/<NN-slug>/resoconto.md`.»

COMMIT: 3274a7f (lavoro), fdbeef3 (server.md)
