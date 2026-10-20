UNITÀ: 5 — ESITO: FATTO

TOCCATI: (da 333a5db, base dell'unità, a 8525788)
- Eton.Tests/CalcoliSpeseTests.cs → +45/−1
- Eton.Tests/CategorieSpesaTests.cs → +17/−3
- Eton.Tests/FusioneRicorrentiTests.cs → +13/−0
- Eton.Tests/GruppiRicorrentiTests.cs → +15/−1
- Eton.Tests/PrivilegiInsertTests.cs → +17/−0
- Eton.Tests/TabellaSpeseTests.cs → +32/−1
- Models/Expense.cs → +13/−0
- Models/RecurringExpense.cs → +3/−0 (solo `Kind`)
- Pages/Home.razor → +2/−1
- Pages/RicorrenteEdit.razor → +46/−12
- Pages/Ricorrenti.razor → +5/−3
- Pages/SpesaEdit.razor → +9/−3
- Pages/Spese.razor → +53/−28
- Pages/SpeseTabella.razor → +73/−21
- Services/CalcoliRicorrenti.cs → +5/−3
- Services/CalcoliSpese.cs → +37/−12
- Services/CategorieSpesa.cs → +10/−4
- Services/ExpenseRepository.cs → +4/−2
- Services/RecurringExpenseRepository.cs → +3/−2
- Services/TabellaSpese.cs → +14/−8
- Shared/Griglia.razor → +34/−4 (solo il riepilogo: parametro `VociRiepilogo`, riga in fondo e testate dei gruppi)
- handoff/05-entrate/: brief-codex-1..4.md, codex-1.md, codex-1b.md, codex-2.md, codex-3.md, codex-4.md, brief-live-testing.md, server.md, resoconto.md

REVIEW:
review: brief 1 — tipo di movimento, categorie delle entrate, bilancio del mese (commit c191eea)
  autore          codex exec 0.161.0 — 18 files changed, 226 insertions(+), 41 deletions(-)
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 3
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 4
  test rosso      CalcoliSpeseTests.Le_entrate_non_entrano_nel_totale_ne_nelle_categorie: error CS1061: 'RiepilogoMese' non contiene una definizione di 'Entrate'
  test rosso      CalcoliSpeseTests.Il_confronto_col_mese_prima_guarda_solo_le_spese: error CS0117: 'Expense' non contiene una definizione per 'Kind'
  test rosso      CalcoliSpeseTests.BilancioDi_separa_spese_ed_entrate: error CS0117: 'CalcoliSpese' non contiene una definizione per 'BilancioDi'
  test rosso      CalcoliSpeseTests.ConSegno_mette_piu_meno_e_niente_sullo_zero: error CS0117: 'CalcoliSpese' non contiene una definizione per 'ConSegno'
  test rosso      CategorieSpesaTests.Una_categoria_vale_solo_per_il_suo_tipo: error CS1501: Nessun overload del metodo 'Conosciuta' accetta 2 argomenti
  test rosso      GruppiRicorrentiTests.Le_entrate_hanno_il_loro_gruppo_e_stanno_fuori_dalla_quota: error CS0117: 'GruppoRicorrenti' non contiene una definizione per 'Entrate'
  test rosso      FusioneRicorrentiTests.Un_entrata_ricorrente_prevista_e_in_arrivo_resta_un_entrata: error CS1061: 'RecurringExpense' non contiene una definizione di 'Kind'
  test rosso      TabellaSpeseTests.Il_filtro_per_tipo_e_l_url: error CS0117: 'FiltriSpese' non contiene una definizione per 'Tipo'
  test rosso      TabellaSpeseTests.La_categoria_di_un_entrata_viene_dal_suo_elenco: error CS1061: 'Expense' non contiene una definizione di 'Kind'
  test rosso      PrivilegiInsertTests.Il_tipo_si_scrive_solo_in_creazione: error CS0117: 'Expense' non contiene una definizione per 'Kind'
  checker         VERDETTI: fondati 3 · infondati 0 · fuori scope 0 · non verificabili 0
                  ROSSO: confermati 10 · non confermati 0 · non verificabili 0
review: brief 2 — le pagine delle entrate (commit cf40a76)
  autore          codex exec 0.161.0 — 7 files changed, 153 insertions(+), 30 deletions(-)
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 3
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 5
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 2 · non verificabili 0
review: brief 3 — correzioni dopo la revisione (commit 2f411e7)
  autore          codex exec 0.161.0 — 15 files changed, 193 insertions(+), 168 deletions(-)
  checker         VERDETTI: risolti 12 · non risolti 1 · non verificabili 0
  bug-hunter      RILIEVI: 0
review: prova nel browser (sul commit 2f411e7)
  live-testing    ESITO: verde
  ui-critic       RILIEVI: 5
review: brief 4 — correzioni dopo la critica della schermata (commit 8525788)
  autore          codex exec 0.161.0 — 3 files changed, 17 insertions(+), 9 deletions(-)
  bug-hunter      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  conformity      non lanciato — 3 files changed, 17 insertions(+), 9 deletions(-)
  backend-expert  non lanciato — 3 files changed, 17 insertions(+), 9 deletions(-)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         VERDETTI: risolti 3 · non risolti 0 · non verificabili 0
  live-testing    ESITO: verde (solo il criterio 1, l'unico provabile; i criteri 2 e 3 non si sono potuti provare)

CONTRATTI:
- Models/Expense.cs:42 `[Column("kind", ignoreOnUpdate: true)] public string Kind { get; set; } = TipoMovimento.Spesa;`
- Models/RecurringExpense.cs:22 `[Column("kind", ignoreOnUpdate: true)] public string Kind { get; set; } = TipoMovimento.Spesa;`
- Models/Expense.cs:66-68 `public static class TipoMovimento` — `public const string Spesa = "expense", Entrata = "income";` (valori della colonna `kind` della migrazione dell'unità 3)
- Services/CategorieSpesa.cs:25 `public static readonly IReadOnlyList<string> Entrate = ["Stipendio", "Rimborso", "Regalo", "Vendita", "Altro"];`
- Services/CategorieSpesa.cs:26 `public static readonly IReadOnlyList<string> Tutte = [.. Elenco.Union(Entrate)];`
- Services/CategorieSpesa.cs:28 `public static IReadOnlyList<string> Di(string tipo)`; :31 `public static bool Conosciuta(string? categoria, string tipo)` (la forma a un argomento non c'è più)
- Services/CalcoliSpese.cs:10 `public sealed record Bilancio(decimal Spese, decimal Entrate)` (con `Saldo`); :32 `public static Bilancio BilancioDi(IEnumerable<Expense> movimenti)`; :16-21 `RiepilogoMese(…, decimal Entrate = 0)`; :43 `public static string ConSegno(decimal importo)`
- Services/ExpenseRepository.cs:220 `public async Task<Expense> CreaAsync(Guid spazioId, Guid pagante, decimal importo, string descrizione, string categoria, DateTime data, string tipo)`
- Services/RecurringExpenseRepository.cs:11 `public sealed record DatiRegola(decimal Importo, string Descrizione, string Categoria, int OgniMesi, int Giorno, DateTime Inizio, int? Rate = null, decimal? TotalePiano = null, string Kind = TipoMovimento.Spesa);`
- Services/CalcoliRicorrenti.cs:20 `public enum GruppoRicorrenti { Entrate, Mensili, ARate, Annuali, OgniNMesi, Terminate }`
- Services/TabellaSpese.cs:30 `public sealed record FiltriSpese(IReadOnlySet<string> Categorie, Guid? Pagante, string Testo, IReadOnlySet<StatoSpesa> Stati, string? Tipo = null)` — nell'URL `tipo=spesa|entrata`
- Shared/Griglia.razor:227 `[Parameter] public Func<IReadOnlyList<T>, IReadOnlyList<(string Etichetta, string Valore)>>? VociRiepilogo { get; set; }`
- Con l'unità 4 (rate): il modulo del Registro nasconde l'interruttore «a rate» con «Entrata» e lo spegne passando a «Entrata» (Pages/Spese.razor, `ScegliTipo`); un piano a rate è sempre una spesa (vincolo del database).

PUNTI DI SOMMA (ricerca: Grep di `.Sum(`, `Aggregate(`, `+= …Amount` su Services/, Pages/, Shared/, Models/, Layout/; `sum(` in supabase/ e somme in wwwroot/*.js: nessuna):
- Services/CalcoliSpese.cs:37-38 `BilancioDi` — entrate e spese in due somme separate; Kind sconosciuto = spesa.
- Services/CalcoliSpese.cs:72 `totale` di `PerMese` — solo `speseDelMese` (Kind ≠ entrata).
- Services/CalcoliSpese.cs:73 `entrate` di `PerMese` — solo Kind = entrata; il saldo è `Entrate − Totale`.
- Services/CalcoliSpese.cs:79 `totalePrecedente` (confronto col mese prima) — solo spese.
- Services/CalcoliSpese.cs:87 totali per categoria (ripartizione e grafici di Registro e Home) — da `speseDelMese`, quindi solo spese.
- Services/CalcoliSpese.cs:116 somma delle quote percentuali — non sono importi; opera sulle categorie già filtrate.
- Services/CalcoliRicorrenti.cs:267 totale mensile di ogni gruppo delle Ricorrenti — le entrate stanno nel gruppo «Entrate», separate; le terminate non hanno totale.
- Services/CalcoliRicorrenti.cs:275 `QuotaDaAccantonare` — solo gruppi Annuali e Periodiche; un'entrata va sempre nel gruppo Entrate (anche se annuale), quindi è esclusa.
- Services/CalcoliGriglia.cs:60 `Aggrega`, somma generica della griglia — usata solo dalle colonne con aggregazioni; la colonna Importo della tabella non ne ha più, quindi non somma importi misti. Il fondo della tabella e le testate dei gruppi usano `VociRiepilogo` → `BilancioDi` (Pages/SpeseTabella.razor, `Bilancio`).
- Pages/Spese.razor e Pages/Home.razor non sommano: mostrano `Totale`, `Entrate` e `Saldo` di `PerMese` (Spese.razor:516, Home.razor:424).
- Previste delle ricorrenti: `CalcoliRicorrenti.Occorrenza` copia `Kind` dalla regola, quindi `PerMese` e `BilancioDi` le classificano come la regola.
- Services/CalcoliVoti.cs:37 — voti delle collezioni, non importi: fuori tema.

ADJUDICA:
- conformity C1 (brief 1): fondato → corretto — `Models/RecurringExpense.cs` usava il nome qualificato `Eton.Services.TipoMovimento`: i modelli dipendevano dai servizi — `[Column("kind", ignoreOnUpdate: true)] public string Kind { get; set; } = TipoMovimento.Spesa;`
  verificato: risolto — Models/RecurringExpense.cs:22 — qualifica Eton.Services tolta, TipoMovimento ora in Eton.Models
- conformity C2 (brief 1): fondato → corretto — i test nuovi stavano prima dei campi e degli helper che aprono le classi, diversamente dagli omologhi — `public void Il_tipo_si_scrive_solo_in_creazione()`
  verificato: risolto — Eton.Tests/PrivilegiInsertTests.cs:133-148 — test nuovi dopo campi e helper; corpi identici al diff tranne le costanti
- conformity C3 (brief 1): fondato → corretto — test e helper scrivevano "expense"/"income" a mano invece delle costanti — `string tipo = TipoMovimento.Spesa) => new()`
  verificato: risolto — Eton.Tests/TabellaSpeseTests.cs:13 — grep di "income"/"expense" in Eton.Tests a vuoto; "kind" ammesso resta
- backend-expert BE1 (brief 1): fondato → corretto — `TipoMovimento` in Services costringeva Models a dipendere da Services — `public static class TipoMovimento`
  verificato: risolto — Models/Expense.cs:66 — nessun `Eton.Services` in Models/ (grep a vuoto); classe nel namespace Models
- backend-expert BE2 (brief 1): fondato → corretto in parte — `PerMese` decideva spesa/entrata in tre punti (due `BilancioDi` e un `Where`); ora non chiama più `BilancioDi` e riusa `speseDelMese` per totale e categorie — `var speseDelMese = delMese.Where(s => s.Kind != TipoMovimento.Entrata).ToList();`
  verificato: non risolto — Services/CalcoliSpese.cs:71-78 — «il predicato spesa/entrata resta scritto 3 volte in PerMese (righe 71, 73, 78) … Il comportamento è invariato … l'orchestratore decide se il residuo basta». Decisione mia: il residuo resta. È la forma che il brief 3 prescriveva (spese del mese, entrate del mese, spese del mese prima: tre insiemi diversi, ciascuno col suo filtro); i numeri non cambiano (bug-hunter sulle correzioni: «Le due partizioni sono complementari come nel vecchio if/else»). Un helper `EUnaEntrata` per tre usi sarebbe un'astrazione in più per una condizione di una riga.
- backend-expert BE3 (brief 1): fondato → corretto — `Tutte` costruita con `Where` + `Contains` invece di un'unione — `public static readonly IReadOnlyList<string> Tutte = [.. Elenco.Union(Entrate)];`
  verificato: risolto — Services/CategorieSpesa.cs:26 — Union: Elenco poi le Entrate non presenti, stesso ordine di prima
- backend-expert BE4 (brief 1): fondato → corretto — `ConSegno` con un ternario annidato — `< 0 => "−" + Denaro.Testo(-importo),`
  verificato: risolto — Services/CalcoliSpese.cs:43-48 — switch con gli stessi tre esiti (+, −, 0,00)
- threat-hunter (brief 1 e 2): nessun rilievo. Il tipo è protetto dal database (colonna concessa solo in INSERT, trigger che rimette `old.kind`), e lato client da `ignoreOnUpdate`.
- bug-hunter BH1 (brief 2): fondato → corretto — `righe.First(r => selezione.Contains(r.Chiave))` lanciava cambiando periodo con righe selezionate (righe vuote, selezione piena): pagina rotta — `return tipi.Count == 1 ? tipi[0] : null;`
  verificato: risolto — Pages/SpeseTabella.razor:653-654 — niente First/Single (grep a vuoto); righe vuote o chiavi perse danno null senza lanciare
  verificato: risolto — Pages/SpeseTabella.razor:187 — frase mista solo se almeno una riga selezionata esiste ancora tra le righe
- conformity C1 (brief 2): fondato → corretto — il tipo della selezione era calcolato in tre punti con tre espressioni — `private string? TipoSelezione`
  verificato: risolto — Pages/SpeseTabella.razor:649 — un solo membro, usato in markup (185), CategoriaMassa (662), CambiaCategoria (675)
- conformity C2 (brief 2): fondato → corretto — il «+» delle entrate era un ternario `"+" : ""` davanti a `Denaro.Testo` invece di `CalcoliSpese.ConSegno`, che esiste per questo. Il checker l'ha dato fuori scope perché il mio brief 2 lo prescriveva così: è un difetto del mio brief, quindi lo correggo qui — `@(r.Kind == TipoMovimento.Entrata ? CalcoliSpese.ConSegno(r.Amount) : Denaro.Testo(r.Amount))&#160;€`
  verificato: risolto — Pages/Ricorrenti.razor:107 — ConSegno per le entrate, Denaro.Testo per le spese; nessun `? "+"` residuo nel repo
- conformity C3 (brief 2): fondato → corretto — rientri dei blocchi `@if`/`else` nuovi non allineati ai vicini e `ScegliTipo` in mezzo ai campi — `<label class="interruttore">`
  verificato: risolto — Pages/Spese.razor:111 — blocchi riletti rientrati (Spese 109-141, Griglia 66-73); ScegliTipo dopo i campi
- backend-expert BE1 (brief 2): fondato → corretto — stesso punto di conformity C1: un solo membro per il tipo della selezione — `private string? TipoSelezione`
  verificato: risolto — Pages/SpeseTabella.razor:649 — un solo membro, usato in markup (185), CategoriaMassa (662), CambiaCategoria (675)
- backend-expert BE2 (brief 2): fondato → corretto — rientri, come conformity C3 — `<label class="interruttore">`
  verificato: risolto — Pages/Spese.razor:111 — blocchi riletti rientrati (Spese 109-141, Griglia 66-73); ScegliTipo dopo i campi
- backend-expert BE3 (brief 2): fondato → corretto — il segno delle entrate ripetuto in due pagine; la riscrittura proponeva un helper `TestoMovimento`, ho preferito `ConSegno` già esistente (nessun helper nuovo) — `CalcoliSpese.ConSegno(r.Amount)`
  verificato: risolto — Pages/Ricorrenti.razor:107 — ConSegno per le entrate, Denaro.Testo per le spese; nessun `? "+"` residuo nel repo
- backend-expert BE4 (brief 2): fondato → corretto — titolo della schermata con ternario annidato — `return Nuova ? $"Nuova {TipoMovimento.Testo(tipo).ToLowerInvariant()} ricorrente" : $"{TipoMovimento.Testo(tipo)} ricorrente";`
  verificato: risolto — Pages/RicorrenteEdit.razor:342 — un solo ternario; testi uguali: Nuova spesa/entrata ricorrente, Spesa/Entrata ricorrente
- backend-expert BE5 (brief 2): fondato → corretto — `ScegliTipo` nell'elenco dei campi — `ScegliTipo` dopo i campi
  verificato: risolto — Pages/Spese.razor:111 — blocchi riletti rientrati (Spese 109-141, Griglia 66-73); ScegliTipo dopo i campi
- nota del checker (brief 2): fondato → corretto — l'etichetta di conferma dell'eliminazione diceva «spese» anche con entrate selezionate — `EtichettaConferma="@($"Sì, elimina {Testi.Conteggio(selezione.Count, "movimento", "movimenti")}")"`
  verificato: risolto — Pages/SpeseTabella.razor:207 — etichetta neutra per selezioni miste
- Infondati: nessuno, quindi nessun campione da riverificare.
- Il rilievo BH1 tocca i dati solo in lettura (render), non la scrittura: l'ho comunque riletto io in Pages/SpeseTabella.razor (blocco delle azioni di massa, `TipoSelezione`, `CambiaCategoria` che esce con tipo nullo).
- ui-critic UC1: fondato → corretto — nel dettaglio la nota «Il tipo non si cambia…» era un figlio diretto della griglia e finiva accanto a «Quando» (x 747 contro 341 dell'etichetta) — `<p class="spiega">Il tipo non si cambia: per farne una spesa o un'entrata, eliminala e segnala di nuovo.</p>` ora dentro `div.campo-lettura`
  verificato: risolto — Pages/SpesaEdit.razor:68 — «Ora sta alle righe 65-69 dentro `<div class="campo campo-lettura">` … Non ho misurato il rendering nel browser.» (la misura nel browser non è stata possibile: vedi SCOSTAMENTI)
- ui-critic UC2: fondato → corretto in parte — la riga entrate/saldo della Home ha grafia e stile diversi dal Registro. Corretta la grafia; lo stile (16 px chiaro contro 13 px tenue) richiede CSS dentro `.striscia-spese` ed è per la 2.1-bis — `<span>Entrate <span class="dato">…</span> · Saldo <span class="dato">…</span></span>`
  verificato: risolto — Pages/Home.razor:131 — «Il Registro a Pages/Spese.razor:199 scrive `Entrate <span …> · Saldo <span …>`: maiuscole identiche.»
- ui-critic UC3: fuori scope — la cella Importo della tabella non mostra il «+» delle entrate. La colonna è modificabile in cella, e il segno nel valore mostrato va deciso insieme alla modifica inline; il tipo si legge nella colonna «Tipo» accanto. Rinviato alla 2.1-bis — `new(TabellaSpese.Colonne.Importo, "Importo", TipoColonna.Denaro, r => r.Spesa.Amount)`
- ui-critic UC4: fuori scope — le pastiglie «Entrata» e della categoria hanno lo stesso stile; distinguerle è CSS, escluso dal mandato (resa provvisoria fino alla 2.1-bis) — `<span class="pastiglia">@s.Category</span>`
- ui-critic UC5: fondato → corretto — sull'editor di un'entrata ricorrente i testi dicevano «spesa» e «bolletta» — `private string NomeTipo => TipoMovimento.Testo(tipo).ToLowerInvariant();`
  verificato: risolto — Pages/RicorrenteEdit.razor:340 — «Usato in :47 …, :54 …, :264 …, :290 …, :347 (titolo). Frase dell'importo distinta a :135-136 … Con tipo Spesa … tutti i testi diventano identici a prima»
- live-testing (prova completa, osservazione): il piede della tabella dice «Sull'unica riga mostrata» con due righe a schermo, una «in arrivo» esclusa dai totali: fuori scope — testo di `Shared/Griglia.razor` precedente all'unità, numeri corretti.
- live-testing (prova completa, osservazione): nell'editor di una ricorrente il giorno del mese non segue da solo la data d'inizio: fuori scope — comportamento precedente all'unità.

FUORI SCOPE:
- ui-critic UC2 (stile), UC3, UC4 e i tre punti non misurati oltre il tetto (margine del valore nei campi in sola lettura,
  totale del mese a 52 px che sovrasta le entrate in un mese di sole entrate, filtro categorie della tabella che mischia
  le due liste): per la 2.1-bis.
- Testi secondari dell'editor di una ricorrente che, per un'entrata, parlano ancora di «spese» già segnate (checker sul
  brief 4): Pages/RicorrenteEdit.razor:200 «le spese dei mesi trascorsi si segnano da sole», :245, :267, :278, :297. Non
  corretti in questo giro; si sistemano con lo stesso `NomeTipo`.
- Nota di bug-hunter sulle correzioni: `ExpenseRepository.CambiaCategoriaAsync` non controlla il tipo e il database non ha un controllo sulla categoria; la protezione contro «categoria di spesa su un'entrata» sta solo nell'interfaccia. Era così anche prima per le categorie delle spese; un vincolo nel database è lavoro della migrazione (supabase/, fuori perimetro).

GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\05-entrate → succeeded — 0 errors, 4 warnings
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\05-entrate → 0 failed, 417 passed, 0 skipped
GATE: live-testing → ESITO: verde (prova completa, 8/8 scenari, sul commit 2f411e7)

SCOSTAMENTI:
- **Codex, primo lancio `BLOCKED`**: «Qual è il percorso del file `-o`…», la stessa domanda dell'unità 04. Ho aggiunto
  dopo la clausola verbatim una riga che spiega che il file `-o` è il messaggio finale salvato dalla riga di comando, e
  rilanciato (codex-1.md è il BLOCKED, codex-1b.md l'esito). Nessun file chiamato `-o` è comparso nella radice del
  worktree in nessuno dei quattro lanci.
- **Nessuna scrittura proibita** nei log di Codex (Set-Content, Out-File, `[System.IO.File]::Write*`, interpreti inline):
  verificato dai checker su codex-1b.log e codex-3.log; il brief 2 l'ho letto io nel diff. Log fuori da `handoff/`:
  `C:\Users\Andre!\.claude\jobs\05d2abcb\tmp\`.
- **Il «+» delle entrate**: il brief 2 lo prescriveva con un ternario; corretto nel brief 3 (vedi ADJUDICA, conformity C2).
- **Dati di prova in produzione: nessuno resta.** Tutti nello spazio «COLLAUDO GIRO B TEST», che prima delle prove era
  vuoto (0,00 € di spese, 0,00 € di entrate, nessuna ricorrente):
  - live-testing: entrata «PROVA COLLAUDO entrata» 1,00 € `b5b5c364-ada5-44f0-a7d4-6a0058a9c3fb` e ricorrente
    «PROVA COLLAUDO ricorrente entrata» dal 1° novembre 2026 `b1e661fd-c137-48dd-90d4-1806b7abfa6f`, eliminate;
  - ui-critic: le stesse due, ricreate per avere qualcosa a schermo, `f12bca71-7d15-4b15-a958-a7174aaf2aea` e
    `8e9086de-ca26-4152-9cf2-81a7b926ab96`, eliminate.
  Dopo ogni pulizia, con ricarica: Registro «0,00 €», «Entrate 0,00 € · Saldo 0,00 €», «Ancora nessuna spesa qui.»;
  Ricorrenti «Ancora nessuna spesa ricorrente qui.»; Home «Nessuna spesa questo mese». **I totali del mese sono tornati
  quelli di prima.** Spazio attivo: «Personale», verificato dopo ricarica. Schede di Chrome aperte dagli agenti: chiuse.
- **Prova nel browser delle correzioni del brief 4: solo in parte.** Provato il cambio dei testi dell'editor fra Spesa ed
  Entrata. Non provati nel browser la posizione della nota nel dettaglio e la grafia della Home: lo spazio «Personale» non
  ha movimenti, e il cambio verso lo spazio di collaudo è stato negato dal sistema dei permessi («Modify Shared
  Resources»); l'agente non l'ha aggirato. Per non creare altri dati non ho chiesto un terzo giro: le due correzioni sono
  di solo markup, verificate sul codice dal checker («risolti 3»).
- **Un frammento di token di sessione** è passato nell'output dell'ultimo live-testing mentre leggeva le chiavi di
  localStorage per controllare lo spazio attivo; l'agente non l'ha riportato nel resoconto. Resta nel suo transcript locale.
- **Due criteri della prova completa non provati**: la selezione mista spesa+entrata nella tabella e la ripartizione per
  categoria con «Altro» già fra le spese, perché lo spazio di collaudo non ha spese e il mandato autorizza solo
  un'entrata e una ricorrente di prova. La logica della selezione mista è coperta dalla lettura del bug-hunter sulle
  correzioni; quella delle categorie dal test `Le_entrate_non_entrano_nel_totale_ne_nelle_categorie`.
- **Server di sviluppo**: avviato due volte dal worktree (23672/29944, poi 27060/3680), fermato entrambe le volte; porta
  5000 libera a fine unità (handoff/05-entrate/server.md). L'ho fermato prima del brief 4 perché la build di Codex non
  trovasse i file in uso.
- **ui-critic lanciato anche se la tabella del §7 diceva «no»** (markup modificato in pagine esistenti, nessun file
  nuovo, nessun metro): lo chiede il mandato.

DECISIONI DELL'UTENTE QUI: nessuna

DA RATIFICARE (raccomandazione dell'unità: tenerle tutte):
1. **Nella tabella la categoria di un'entrata non si cambia in riga**, solo dal suo dettaglio; il cambio di categoria di
   massa compare solo se la selezione è tutta di un tipo. Raccomandazione: tenere, evita di scrivere una categoria di
   spesa su un'entrata.
2. **Dopo «Segna» il modulo resta sul tipo scelto** (chi segna due entrate di fila non deve rimetterlo). Raccomandazione:
   tenere.
3. **I conteggi dicono «movimento/movimenti»** dove possono esserci spese ed entrate (registro, conferma
   dell'eliminazione). Raccomandazione: tenere.
4. **Entrate e saldo si vedono sempre** sotto il totale del mese, anche a zero entrate (il mandato chiede spese, entrate,
   saldo). Raccomandazione: tenere; se disturba, nasconderli a zero è una riga.
5. **Le occorrenze materializzate delle ricorrenti non mandano `kind`**: lo copia dalla regola il trigger BEFORE INSERT
   della migrazione dell'unità 3 (commento in `Services/ExpenseRepository.cs`). Raccomandazione: tenere, una sola fonte.
6. **`PerMese` scrive tre volte la condizione spesa/entrata** (BE2 del brief 1, «non risolto» per il checker): tre
   insiemi diversi, numeri invariati. Raccomandazione: tenere.

SEGNALAZIONI:
STATO: aperta
DA: sessione-unità 05-entrate (Eton), 20 ottobre 2026
INNESCO: primo `codex exec` dell'unità tornato `BLOCKED` sulla clausola «Il tuo resoconto va solo nel file `-o`»
Cosa è successo: Codex non sa quale sia «il file `-o`» e si ferma a chiederlo; è la seconda unità di fila (04 e 05).
Perché: la riga verbatim di `TODO-synapse-codex.md`, «Regola per il mandato», nomina l'opzione della riga di comando come
se fosse un file che Codex conosce, ma Codex vede solo il brief da stdin.
Proposta: nella riga verbatim, dopo «file `-o`», aggiungere «(è il tuo messaggio finale: la riga di comando lo salva da
sé; non creare un file chiamato `-o`)».

COMMIT: 8525788 (ramo unita-05-entrate; lavoro in c191eea, cf40a76, 2f411e7, 8525788)
