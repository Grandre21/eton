UNITÀ: 3 — ESITO: FATTO
TOCCATI:
  Eton.Tests/CalcoliBudgetTests.cs → +57/−0
  Models/CategoryBudget.cs → +1/−0
  Pages/Budget.razor → +5/−3
  Pages/BudgetEdit.razor → +52/−17
  Services/BudgetRepository.cs → +6/−4
  Services/CalcoliBudget.cs → +53/−1
  handoff/03-budget-periodo/ → brief-implementer-1.md, brief-implementer-2.md, app.md, resoconto.md (nuovi)

REVIEW:
review: 1 — descrizione, durata, chiusura, 23P01, scorciatoia
  bug-hunter      RILIEVI: 0
                  NON VERIFICABILE — per live-testing: il `<select @bind="mesi">` su int con 25 `<option>` generate in un ciclo `@for` (selezione iniziale "ogni mese…" e valore dopo la scelta) è corretto per Blazor in lettura del codice, ma la resa nel browser non l'ho provata.
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0
                  NON VERIFICABILE — per live-testing/ui-critic: la resa di `<span class="spiega">` dentro `.azioni` nella riga di Budget.razor (descrizione lunga fino a 200 caratteri senza spazi, rischio di overflow).
  backend-expert  RILIEVI: 3
                  NON VERIFICABILE — per live-testing/ui-critic: (1) G:\Sviluppo\Eton\Pages\Budget.razor:92, lo `<span class="spiega">` come primo figlio del `.azioni` della riga: con una descrizione lunga (fino a 200 caratteri) va visto come va a capo accanto ai tre pulsanti, anche nella griglia larga dove `li` è `display: contents`; (2) gli anni dentro `TestoPeriodo` non sono più in `<span class="dato">` (Budget.razor:112, BudgetEdit.razor:68 e :78), quindi perdono la famiglia mono che avevano prima; (3) G:\Sviluppo\Eton\Pages\BudgetEdit.razor:110, il contatore `<p class="spiega">` subito dopo `label.campo` e prima del select «Durata»: da vedere lo stacco; (4) il link «Spese del mese» è identico in ogni riga di budget (Budget.razor:94, non dipende dalla riga): da valutare se ripeterlo N volte è la resa voluta.
  test rosso      FineDaMesi_conta_il_mese_di_inizio: Eton.Tests/CalcoliBudgetTests.cs:90:63  error CS0117: 'CalcoliBudget' does not contain a definition for 'FineDaMesi'
  test rosso      TestoPeriodo_nei_quattro_casi: Eton.Tests/CalcoliBudgetTests.cs:110:67  error CS0117: 'CalcoliBudget' does not contain a definition for 'TestoPeriodo'
  test rosso      Il_rifiuto_23P01_si_riconosce_solo_col_400_e_il_suo_codice: Eton.Tests/CalcoliBudgetTests.cs:120:35  error CS0117: 'CalcoliBudget' does not contain a definition for 'RifiutoPerSovrapposizione'
  test rosso      La_scorciatoia_apre_la_tabella_sul_mese_tipo_spesa_senza_categoria: Eton.Tests/CalcoliBudgetTests.cs:134:39  error CS0117: 'CalcoliBudget' does not contain a definition for 'IndirizzoSpeseDelMese'
  test rosso      Un_budget_a_periodo_si_chiude_prima_della_fine_ma_non_si_allunga: Eton.Tests/CalcoliBudgetTests.cs:101  Assert.True() Failure Expected: True Actual: False (con SiPuoChiudere al corpo di prima)
  checker         VERDETTI: fondati 0 · infondati 0 · fuori scope 0 · non verificabili 0
                  ROSSO: confermati 5 · non confermati 0 · non verificabili 0
  live-testing    ESITO: verde sul provato; scenari 1, 2 e 4 NON PROVATI, perché nessun budget esiste nei dati (vedi NON PROVATO). Lo scenario 3 è provato per intero. Nessun difetto osservato.
  ui-critic       non lanciato — tabella §7 riga 2 — 6 files changed, 174 insertions(+), 25 deletions(-)
review: 2 — correzione BE1 e BE3 (Pages/BudgetEdit.razor, base 4f1992e26da3fe49d6be7fef338abe064e0aed5c)
  bug-hunter      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  conformity      non lanciato — 1 file changed, 7 insertions(+), 8 deletions(-)
  backend-expert  non lanciato — 1 file changed, 7 insertions(+), 8 deletions(-)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         VERDETTI: risolti 2 · non risolti 0 · non verificabili 0

Misure del brief 1 (sul diff prima della correzione): `git diff HEAD --stat --summary` → «6 files changed, 172 insertions(+), 22 deletions(-)», nessun create mode; conteggio dichiarazioni/endpoint → 0. backend-expert lanciato per > ~120 righe.

CONTRATTI:
  Models/CategoryBudget.cs:20 `[Column("description")] public string Description { get; set; } = "";`
  Services/BudgetRepository.cs:28 `public async Task<CategoryBudget> CreaAsync(Guid spazioId, Guid autore, string categoria, decimal importo, string descrizione, DateTime inizio, DateTime? ultimoMese)`
  Services/BudgetRepository.cs:42 `public async Task<RisultatoSalvataggio<CategoryBudget>> SalvaAsync(Guid budgetId, int versioneLetta, decimal importo, string descrizione)` (sostituisce SalvaImportoAsync)
  Services/CalcoliBudget.cs:57 `public static bool SiPuoChiudere(CategoryBudget b, DateTime oggi)` → iniziato e (senza fine oppure fine dopo il mese corrente)
  Services/CalcoliBudget.cs:68 `public static DateTime FineDaMesi(DateTime inizio, int mesi)` (1..24, fuori lancia)
  Services/CalcoliBudget.cs:77 `public static string TestoPeriodo(DateTime inizio, DateTime? fine)`
  Services/CalcoliBudget.cs:87 `public static bool RifiutoPerSovrapposizione(int stato, string? corpo)` (400 + "code":"23P01")
  Services/CalcoliBudget.cs:105 `public static string IndirizzoSpeseDelMese(int anno, int mese)` → `expenses/table?periodo=libero&da=…&a=…&tipo=spesa` via VistaTabella.InQuery
  Contratto della 01 usato com'è: description scritta in INSERT e UPDATE, ends_on in INSERT (budget a periodo) e in UPDATE (chiusura).

ADJUDICA:
- backend-expert BE1: fondato → corretto — periodo mostrato e salvato calcolati in due punti — `var ultimo = mesi > 0 ? CalcoliBudget.FineDaMesi(inizio, mesi) : (DateTime?)null;`
  verificato: risolto — G:\Sviluppo\Eton\Pages\BudgetEdit.razor:186-187 `private DateTime Inizio => CalcoliBudget.InizioDiUnNuovo(budgetDelloSpazio, categoria, DateTime.Today);` / `private DateTime? UltimoMese => mesi > 0 ? CalcoliBudget.FineDaMesi(Inizio, mesi) : null;`
- backend-expert BE2: fuori scope — l'allineamento del modulo ripetuto in OnParametersSetAsync, SalvaCon e Ricarica è struttura preesistente dell'editor (vale anche per importo e categoria); il diff ha aggiunto una riga per punto — `descrizione = budget.Description;`
- backend-expert BE3: fondato → corretto — copia inutile della variabile di ciclo — `var valore = n;`
  verificato: risolto — G:\Sviluppo\Eton\Pages\BudgetEdit.razor:123 `<option value="@n">per @n @(n == 1 ? "mese" : "mesi")</option>`
Campione sugli infondati: nessun infondato. Rilievi di threat-hunter: nessuno; ho letto io il diff delle pagine (descrizione solo come espressione Razor, nessun MarkupString, nessun href con testo dell'utente).

FUORI SCOPE:
- backend-expert BE2 (bassa): un metodo unico «allinea il modulo al budget» al posto delle tre copie; da fare quando l'editor si ritocca.
- Nota del collaudo: con «per 1 mese» la frase continua «Lo stesso importo vale ogni mese, e quello che resta non si riporta», un po' stonata per un mese solo. Solo testo, non un difetto.
- Segnalato dal checker: `Inizio >CalcoliBudget` senza spazio a Pages/BudgetEdit.razor:68, solo forma.

GATE: mcp__synapse__build root=G:\Sviluppo\Eton → build: succeeded — 0 errors, 0 warnings — 2.9s
GATE: mcp__synapse__test root=G:\Sviluppo\Eton → test: 0 failed, 451 passed, 0 skipped — 5.3s

SCOSTAMENTI:
- Descrizione e periodo nell'elenco stanno in uno `<span class="spiega">` come primo figlio della fila `.azioni` del budget, e non in un elemento accanto al nome della categoria: la griglia di `.barre-categorie` (wwwroot/css/app.css:2086-2120) ha posto solo per nome, barra, quota, totale e azioni, e una riga in più chiedeva una regola CSS nuova. Così nessun file in wwwroot/ è toccato. Nella sezione «In arrivo» la descrizione sta accanto alla categoria («Casa · …»).
- Gli anni del periodo non sono più in `<span class="dato">` (carattere monospazio): il periodo è un testo unico prodotto da CalcoliBudget.TestoPeriodo, testabile.
- La durata è un menu a tendina (0 = senza fine, 1..24 mesi) e non un campo numerico: nessuno stato invalido, come «Ogni quanto» delle ricorrenti.
- Il messaggio della sovrapposizione è «C'è già un budget per questa categoria in quei mesi: forse l'ha appena creato qualcun altro. Torna ai budget per vedere com'è adesso.»: l'editor fa partire un budget nuovo dopo la fine di tutti quelli della categoria, quindi in pratica il rifiuto arriva solo da una creazione in contemporanea.
- Prova nel browser: scenari 1 (righe dell'elenco), 2 (scorciatoia) e 4 (modifica) NON PROVATI perché negli spazi «Personale» (gen–nov 2026) e «COLLAUDO GIRO B TEST» (set 2023–nov 2026) non esiste nessun budget, e creare un budget è una scrittura sui dati di produzione. Restano da guardare anche i dubbi dei revisori sull'elenco (descrizione lunga accanto ai pulsanti, anche a 375px; link «Spese del mese» ripetuto). Passano alla prova con salvataggio del capo, qui sotto.
- Prova con salvataggio per il capo (con un sì nuovo dell'utente), un passo per riga, con l'atteso:
  1. Avvia l'app di sviluppo (riavvio fresco), apri /expenses/budgets nello spazio «Personale» → «Ancora nessun budget qui.».
  2. «Nuovo budget», categoria «Casa», importo 10, descrizione «PROVA 2.5», durata «per 3 mesi» → la frase dice «Vale da novembre 2026 a gennaio 2027.»; «Salva» → torna all'elenco.
  3. Elenco di novembre 2026 → riga «Casa» con «PROVA 2.5 · da novembre 2026 a gennaio 2027» e i pulsanti «Registra una spesa», «Spese del mese», «Modifica»; guardare anche a 375px come va a capo.
  4. «Spese del mese» → /expenses/table con periodo libero dal 1 al 30 novembre 2026, tipo spesa, nessuna categoria filtrata.
  5. Torna, «›» non va oltre novembre; da gennaio 2027 il budget non è visibile se non si arriva al mese (la pagina non va nel futuro): basta che a novembre la riga ci sia.
  6. «Modifica» → «Vale da novembre 2026 a gennaio 2027.», descrizione «PROVA 2.5», nessun menu Durata, c'è «Chiudi».
  7. Cambia la descrizione in «PROVA 2.5 bis», «Salva» → «Salvato.»; l'elenco mostra la descrizione nuova.
  8. «Chiudi» → «Sì, chiudi» → «Chiuso: vale fino alla fine di questo mese.»; la riga dice ora «Vale solo novembre 2026.» e «Chiudi» non c'è più (non si può allungare).
  9. Rifiuto per sovrapposizione: con il budget chiuso a novembre, apri «Nuovo budget» in una seconda scheda e lascialo aperto su «Casa» (partirà da dicembre); nella prima scheda crea un altro «Casa» per 1 mese (partirà da dicembre) e salvalo; nella seconda scheda scrivi importo 5 e «Salva» → compare «C'è già un budget per questa categoria in quei mesi: forse l'ha appena creato qualcun altro. Torna ai budget per vedere com'è adesso.» e il modulo resta com'era.
  10. Pulizia: elimina i due budget «Casa» di prova da «Modifica» → «Elimina» → «Sì, elimina»; l'elenco torna «Ancora nessun budget qui.».
- L'app avviata per la prova (pid 30776, porta 5000, annotata in handoff/03-budget-periodo/app.md) è stata fermata prima del resoconto.
- Messaggio ricevuto durante l'unità dall'«AMMINISTRATORE P - 01» (local_d88be40f-62a9-4fa1-bf85-7f98e6640b93), 2026-11-20: l'utente si allontana, lavorare in autonomia senza bloccarsi. Nessun lavoro nuovo; nessuna domanda al capo è servita.
- Prova rossa del test sulla chiusura: al primo giro la riga era dedotta (il progetto di test non compilava per le altre funzioni); l'implementer è stato ripreso per rimettere temporaneamente il corpo vecchio di SiPuoChiudere, vedere il test fallire e ripristinarlo. Confermata dal checker.

DECISIONI DELL'UTENTE QUI: nessuna
DA RATIFICARE:
- Descrizione e periodo nella fila dei pulsanti invece che accanto al nome della categoria, per non toccare il CSS. Raccomandazione: tenerla; se in prova la riga con descrizione lunga non regge, una regola CSS di una riga la porta su una riga sua.
- Anni del periodo non più in carattere monospazio. Raccomandazione: tenerlo, il periodo si legge come una frase.
SEGNALAZIONI: nessuna
COMMIT: bced3ea1fa05cbaed9e9e84c0f776024ae13d07b
