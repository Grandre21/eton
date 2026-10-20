UNITÀ: 4 — ESITO: FATTO

Tutto il mandato è fatto e provato nel browser. Dopo la ripresa del §7 (risposta del capo «sì, riprendi adesso») due
difetti visti nel browser sono stati corretti: la data del Registro dopo un piano, e la disposizione del modulo con
«a rate» acceso. Non resta nessun dato di prova in produzione, lo spazio attivo è «Personale», il server è fermo.

TOCCATI:
- Eton.Tests/GruppiRicorrentiTests.cs → +7/−0
- Eton.Tests/PianiARateTests.cs (nuovo) → +135/−0
- Models/RecurringExpense.cs → +4/−0
- Pages/RicorrenteEdit.razor → +160/−70
- Pages/Ricorrenti.razor → +7/−1
- Pages/Spese.razor → +68/−8
- Services/CalcoliRicorrenti.cs → +66/−17
- Services/RecurringExpenseRepository.cs → +15/−6
- handoff/04-rate/ → brief-codex-1..6, codex-1..6, server.md, 22 screenshot del collaudo (collaudo-browser-1..17), 4 della critica (critica-1..4), questo resoconto

REVIEW:
review: brief-codex-2 (+3, +4) — spese a rate
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
  live-testing    FORMATO ROTTO — nessuna riga di conteggio (primo lancio, fermato con TaskStop su ordine del capo prima del report)
  live-testing    ESITO: difetti (solo due osservazioni minori; i criteri 1-6 passano tranne quanto in NON PROVATO)
review: brief-codex-5 — la data del Registro torna a oggi dopo un piano
  autore          codex exec 0.161.0 — 1 file changed, 3 insertions(+), 1 deletion(-)
  bug-hunter      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  conformity      non lanciato — 1 file changed, 3 insertions(+), 1 deletion(-)
  backend-expert  non lanciato — 1 file changed, 3 insertions(+), 1 deletion(-)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         nessuna istruttoria
  live-testing    ESITO: verde
  ui-critic       RILIEVI: 4
review: brief-codex-6 — con «a rate» il modulo va in una colonna
  autore          codex exec 0.161.0 — 1 file changed, 2 insertions(+), 1 deletion(-)
  bug-hunter      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  conformity      non lanciato — 1 file changed, 2 insertions(+), 1 deletion(-)
  backend-expert  non lanciato — 1 file changed, 2 insertions(+), 1 deletion(-)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         nessuna istruttoria
  live-testing    ESITO: verde

Note al tracciato:
- Il primo `ROSSO` non confermava la seconda riga: nel primo lancio il test della quota non compilava per colpa di un
  altro file, quindi non era mai fallito sulla propria asserzione. Nel brief-codex-4 Codex ha rimesso per un momento il
  calcolo vecchio, ha visto il test fallire (41,67 contro 41,66) e ha rimesso il nuovo; il secondo `checker` l'ha
  confermato su codex-4.log:153-158 e ha verificato il ripristino identico.
- `live-testing` del secondo lancio: «ESITO: difetti» ma «Nessun difetto bloccante»; le due osservazioni erano la data del
  Registro rimasta sulla prima rata (fondata → brief-codex-5) e l'assenza del titolo «Fermare» per un piano non partito
  (infondata: è voluta, la sezione di un piano non partito offre solo Elimina, v. ADJUDICA). Non provati lì l'ordine dei
  gruppi e la riga «metti da parte» con altre regole a schermo: lo spazio di prova non ne aveva; li coprono i test puri
  `Gruppo_a_rate_e_fuori_dalla_quota` e `Quota_somma_i_totali_di_gruppo_arrotondati`.
- `ui-critic` è stato lanciato sul diff del brief-codex-5, con a schermo un piano di prova lasciato apposta; secondo la
  tabella del §7 era «no» (riga 2: markup in .razor esistenti, nessun .css, nessun metro), ma il mandato lo chiede. Metro:
  nessuno, resa provvisoria fino alla 2.1-bis, confronto con gli omologhi della stessa app.
- Il terzo `live-testing` ha provato la correzione della colonna e ha fatto la pulizia.

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
- Per l'unità 5: il modulo del Registro (`Pages/Spese.razor`) ha ora `aRate`/`numeroRate`, e con `aRate` il contenitore
  prende anche `campi-ricorrente`; la scelta «Spesa / Entrata» deve nascondere l'interruttore e spegnerlo (`aRate = false`)
  quando si passa a «Entrata». Dopo «Segna» di un piano `nuovaData` torna a oggi.

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
- live-testing (osservazione 1): fondato → corretto — dopo un piano il campo «Quando» restava sulla data della prima rata e la spesa singola successiva sarebbe finita in un mese futuro — `nuovaData = DateTime.Today;`
  verificato: risolto — bug-hunter sul diff («`nuovaData = DateTime.Today;` (riga 397) viene dopo `await Ricorrenti.CreaAsync(...)` (riga 393) … Se CreaAsync lancia, il flusso salta al catch») e live-testing («input[type=date].value = "2026-10-20" … non 2026-11-15»)
- live-testing (osservazione 2): infondato — il titolo «Fermare» manca di proposito per un piano non partito: prima della prima rata si elimina, non si termina (brief-codex-1 §5) — `@if (!Piano || Bloccato)`
- ui-critic UC1: fondato → corretto — a schermo largo, con «a rate» acceso, la riga «3 rate da 33,33 €» e l'errore stavano 381 px a sinistra del select che spiegano — `<div class="campi-spesa @(aRate ? "campi-ricorrente" : null)">`
  verificato: risolto — live-testing finale («Interruttore x=366 w=746,4. Select x=366 w=746,4. `p.spiega` «3 rate da 33,33 €» x=366 w=746,4. Gap select-riga d'aiuto = 8,0 px … classList = solo `campi-spesa`» a interruttore spento)
- ui-critic UC2: fuori scope — a 390 px le quattro voci della `.meta` di una riga a rate si schiacciano in colonnine invece di andare a capo fra loro: la regola `.riga-spesa .meta` è in wwwroot/css/app.css, fuori perimetro, e il difetto c'era già con tre voci (resoconto dell'unità 02) — `.riga-spesa .meta { … display: flex; … }`
- ui-critic UC3: fuori scope, in parte corretto da UC1 — l'aiuto ora sta a 8 px dal select; resta il doppio spazio (32 px) sotto l'interruttore, che viene dal `margin-bottom` di `.interruttore` in app.css, fuori perimetro — `.interruttore { … margin-bottom: var(--s4); }`
- ui-critic UC4: fuori scope — il totale della testata «33,33 € al mese» è a 10,45 px perché `.dato` (0.95em) sta dentro una testata a 11 px: preesistente dall'unità 02, regola in app.css — `<span class="totale dato">`
- Campione sugli infondati: C3 riverificato da me su Pages/CollectionDetail.razor:4-5 (`@inject CollectionItemRepository Elementi`, `@inject ReviewRepository Recensioni`) — confermato infondato. L'osservazione 2 di live-testing riverificata da me su Pages/RicorrenteEdit.razor (`@if (!Piano || Bloccato)`) — confermata voluta.
- threat-hunter: 0 rilievi in tutti e tre i giri; nel primo ha segnalato fuori mandato lo stesso difetto di BH1, già corretto.
- ui-critic, punto non misurabile: l'avviso «Piano a rate creato» sembrava sparire nel secondo live-testing. Nel codice e nel
  CSS nulla lo azzera tranne il «Segna» successivo (ui-critic: «nessun keyframe o transizione lo tocca … nessun NavigateTo,
  Timer o Task.Delay»); nel primo live-testing era rimasto a schermo col suo link. Probabile effetto del pulsante «Aggiorna»
  del banner «versione nuova», che ricarica la pagina. Non è un difetto accertato: da guardare all'uso.

FUORI SCOPE:
- backend-expert BE3 (per l'utente): componente unico per «Numero di rate» con l'anteprima della rata, usato da Registro ed editor.
- backend-expert BE4 (TIPO progetto, per l'utente): overload di `Prossima` che prende la regola; toglierebbe anche il wrapper privato di Pages/Ricorrenti.razor.
- ui-critic UC2 (per la 2.1-bis): `flex-wrap: wrap` su `.riga-spesa .meta` e `white-space: nowrap` sulle sue voci, perché sul telefono le righe con più dati vadano a capo fra una voce e l'altra.
- ui-critic UC3, resto (per la 2.1-bis): `margin-bottom: 0` per `.interruttore` dentro `.campi-spesa`.
- ui-critic UC4 (per la 2.1-bis): il totale nelle testate dei gruppi a 11 px come l'etichetta (`.testa-registro .dato { font-size: inherit; }`).

GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\04-rate → succeeded — 0 errors, 4 warnings | mcp__synapse__test root=G:\Sviluppo\wt-Eton\04-rate → 0 failed, 407 passed, 0 skipped | live-testing → ESITO: verde (terzo lancio, sulla build finale 01a476a)

SCOSTAMENTI:
- **Prove nel browser fermate e riprese.** Alle ~15:37 il capo (coordinatore-Eton, messaggio fra sessioni): «FERMATI con le
  prove nel browser, su richiesta dell'utente (chat, 20 ott: «ferma un attimo le prove nel browser»). […] Se hai creato
  dati «PROVA COLLAUDO», lasciali dove sono e scrivimi quali sono.» Eseguito subito (`TaskStop` su live-testing);
  risposta con lo stato alle ~15:40. Il timer di 30 minuti è scaduto senza risposta: l'unità si è chiusa una prima volta
  `BLOCKED` (commit 80dc823, ramo pubblicato). Poi il capo: «sì, riprendi adesso. L'utente ha scelto in chat (20 ott,
  popup): «Riprendi adesso». […] elimina il piano «PROVA COLLAUDO rate» (id 137fde18-…) e ogni altro dato di prova che
  crei, rimetti «Personale» come spazio attivo, chiudi la scheda di Chrome che hai aperto e spegni il server». Ripreso
  (prima riga di nuovo `in corso`) e completato.
- **Dati di prova: nessuno resta.** Eliminati, tutti nello spazio «COLLAUDO GIRO B TEST» e tutti senza spese
  materializzate (prima rata il 15 novembre 2026): `137fde18-9b3e-454b-8e1e-965833d930b5` (lasciato dalla prova
  interrotta), `6ce39680-f358-4ce9-a30b-3a2f59dc2af2` (seconda prova), `851de8ad-8d11-4b35-84dc-8705899d15f8` (lasciato
  a schermo per ui-critic, poi eliminato dall'ultima prova). Verificato con ricarica: nessuna ricorrente nello spazio,
  registro di ottobre vuoto. Nessuna spesa singola creata.
- **Spazio attivo**: «Personale», verificato dopo ricarica.
- **Una scheda di Chrome può essere ancora aperta**: id 780391223, su localhost:5000 (la porta ora è chiusa, quindi la
  scheda non mostra più niente). È uscita dal gruppo di schede del plugin quando il secondo live-testing ha chiuso la
  scheda di servizio, e nessun agente ha più potuto raggiungerla. Va chiusa a mano. Tutte le altre schede aperte sono
  state chiuse.
- **Server di sviluppo**: avviato quattro volte (31536/26772, 24836/21592, 4844/26416, 21688/30732), sempre dal worktree
  e sempre fermato; porta 5000 libera a fine unità (handoff/04-rate/server.md).
- **Due correzioni nate dalla prova nel browser** (brief-codex-5 e -6), ciascuna con build, test e i due revisori della
  tabella del §3. Sono difetti del lavoro di questa unità, non lavoro nuovo.
- **Due domande di Codex** (BLOCKED, dalla stessa sessione dell'unità, non dal capo): (1) «quale percorso devo usare per
  il file `-o`» → risposto nel brief-codex-2 (il file lo scrive la riga di comando); (2) «ogni piano ha già `EndsOn`
  valorizzato […] il blocco Termina non offre il pulsante» → difetto vero del mio brief, risposto nel brief-codex-3
  (Termina per un piano partito e ancora attivo, «Concluso il» quando è finito).
- **Il file `-o`** non è comparso nella radice del worktree in nessuno dei sei lanci: Codex ha scritto direttamente
  `codex-<n>.md`.
- **Nessuna scrittura con PowerShell inline** né interpreti nei log di Codex dei lanci 2-4 (verificato dai due
  `checker`): solo `apply_patch`. Al brief ho aggiunto una riga che vieta `Set-Content`, `Out-File`,
  `[System.IO.File]::Write*` (lezione dell'unità 02), oltre alle tre righe verbatim. I lanci 5 e 6 (due righe ciascuno)
  non sono stati istruiti da un `checker` (nessuna istruttoria prevista): il diff è stato letto per intero da me.
- Log di Codex: `C:\Users\Andre!\.claude\jobs\4ae8f623\tmp\codex-1..6.log`, fuori da `handoff/`.
- Domande al capo: una («posso riprendere le prove nel browser…?»), inviata ~15:40, timer scaduto senza risposta; risposta
  arrivata dopo la prima chiusura `BLOCKED`, dal nome di `CAPO:`, ed eseguita.

DECISIONI DELL'UTENTE QUI: nessuna (le parole dell'utente citate in SCOSTAMENTI arrivano attraverso il capo, non da questa sessione)

DA RATIFICARE:
- Un piano conta come «partito» anche se la data della prima rata è arrivata ma la rata non è ancora stata scritta (chi
  paga non ha aperto Eton): `MaterializedThrough is not null || StartsOn <= oggi`. Raccomando di tenerlo: la spec dice
  «almeno una rata materializzata», ma così una rata già dovuta non cambia importo sotto i piedi.
- Prima della prima rata il piano non offre «Termina» (si elimina). Raccomando di tenerlo: terminare un piano che non è
  partito equivale a eliminarlo, e terminarlo poi modificarlo lo riaprirebbe.
- Nella riga del gruppo «A rate», «rata k/N» indica la **prossima** rata e il residuo la comprende; una rata che cade oggi
  conta già pagata. Raccomando di tenerlo.
- Dopo «Segna» con «a rate» l'interruttore si spegne, la data torna a oggi e il numero di rate resta quello scelto.
  Raccomando di tenerlo: la spesa successiva è quasi sempre singola e di oggi.
- Descrizione di un piano limitata a 187 caratteri (200 meno « · rata 60/60»), con il messaggio accanto al campo.
  Raccomando di tenerlo: è la soluzione che non tronca mai il testo salvato.
- Con «a rate» acceso il modulo del Registro va in una colonna (come l'editor delle ricorrenti) invece della griglia a
  due colonne. Raccomando di tenerlo fino alla 2.1-bis: è l'unico modo senza CSS nuovo di tenere ogni aiuto sotto il suo campo.

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

STATO: aperta
DA: Eton, sessione-unità 04-rate, 2026-10-20
INNESCO: domanda al capo con timer di 30 minuti («La domanda al capo», cartella-unita.md)
Cosa è successo: il capo ha fermato le prove su richiesta dell'utente e ha risposto «riprendi» dopo che il timer era scaduto;
l'unità si era già chiusa BLOCKED, e ha dovuto riaprirsi riportando la prima riga a `in corso` dopo un commit d'esito.
Perché: quando la domanda nasce da una pausa decisa dall'utente, i 30 minuti del timer misurano il tempo dell'utente, non quello
del capo; la procedura non prevede la riapertura di un'unità già chiusa con un esito finale.
Proposta: in «La domanda al capo» dire cosa fa un'unità chiusa BLOCKED che riceve dopo la risposta valida del capo (riapre con
un nuovo commit d'esito, come qui, oppure resta chiusa e il capo apre una sessione nuova), e permettere al capo di indicare nel
«fermati» un timer più lungo.

COMMIT: 80a1914
