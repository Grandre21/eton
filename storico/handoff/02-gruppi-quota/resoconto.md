UNITÀ: 2 — ESITO: PARZIALE

Tutto il mandato è fatto e provato nel browser, tranne un passo del §7: `ui-critic` non è stato lanciato,
perché per misurare la schermata servono regole di prova lasciate a schermo nel database di produzione, e
il permesso automatico l'ha negato («Modify Shared Resources»). Serve un sì dell'utente (vedi SCOSTAMENTI).

TOCCATI:
- Services/CalcoliRicorrenti.cs → +45/−0
- Pages/Ricorrenti.razor → +62/−30
- Pages/Ricorrenti.razor.css (nuovo) → +6/−0
- Eton.Tests/GruppiRicorrentiTests.cs (nuovo) → +79/−0
- handoff/02-gruppi-quota/ → brief-codex-1..3, codex-1..3, piano-design.md, server.md, 4 screenshot del collaudo, questo resoconto

REVIEW:
review: brief-codex-1 — gruppi e quota
  autore          codex exec 0.161.0 — 4 files changed, 161 insertions(+), 6 deletions(-) (primo giro); dopo le due correzioni: 4 files changed, 192 insertions(+), 30 deletions(-)
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 4
  test rosso      GruppiRicorrentiTests: G:\Sviluppo\wt-Eton\02-gruppi-quota\Eton.Tests\GruppiRicorrentiTests.cs(13,22): error CS0103: Il nome 'GruppoRicorrenti' non esiste nel contesto corrente [G:\Sviluppo\wt-Eton\02-gruppi-quota\Eton.Tests\Eton.Tests.csproj]
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
                  ROSSO: confermati 1 · non confermati 0 · non verificabili 0
  checker         VERDETTI: risolti 3 · non risolti 1 · non verificabili 0
  checker         VERDETTI: risolti 1 · non risolti 0 · non verificabili 0
  live-testing    ESITO: difetti
  ui-critic       non lanciato — negato dal permesso automatico («Modify Shared Resources») il passo che lascia i dati di prova a schermo; tabella §7 riga 3 (file .razor.css nuovo, PIANO-DESIGN) — 4 files changed, 192 insertions(+), 30 deletions(-). Non è un «non lanciato» ammesso dal §4 regola 2: per questo l'esito è PARZIALE.

Note al tracciato:
- `live-testing` ha scritto `ESITO: difetti`, ma nel suo stesso report: «Nessun difetto sui criteri di accettazione 1-5»;
  l'unica voce è una «Segnalazione minore (non bloccante)»: sul telefono, nelle righe, cadenza e prossima data vanno a
  capo e le descrizioni lunghe si troncano. Il markup del `<li>` è identico a HEAD salvo i rientri (checker, terza
  istruttoria: «Il corpo del `<li>` (righe 86-103) è identico a HEAD a parte lo spazio») e gli stili delle righe sono in
  `wwwroot/css/app.css`, non toccato: è un comportamento preesistente, fuori scope, da 2.1-bis.
- Il secondo `checker` ha trovato C1 non risolto (Codex l'aveva dichiarato fatto); terzo lancio di Codex sulla sola
  indentazione, poi il terzo `checker` l'ha dato risolto.

CONTRATTI:
- invariati: `Services/CalcoliRicorrenti.cs:105` `public static string Cadenza(int ogniMesi, int giorno, DateTime inizio)`;
  `Services/CalcoliRicorrenti.cs:161` `public static SpeseDelPeriodo Fondi(IReadOnlyList<Expense> vere, IReadOnlyList<RecurringExpense> regole,`
  `DateTime da, DateTime a, DateTime oggi)`; `Services/RecurringExpenseRepository.cs:28`
  `public async Task<IReadOnlyList<RecurringExpense>> ElencaAsync(Guid spazioId)` (file non toccato). Cadenza e Fondi
  hanno solo cambiato riga (+9), non firma.
- nuovi, per le unità 4 e 5:
  - `Services/CalcoliRicorrenti.cs:20` `public enum GruppoRicorrenti { Mensili, Annuali, OgniNMesi, Terminate }` —
    l'ordine di dichiarazione è l'ordine in pagina: «Entrate» si inserisce prima di `Mensili`, «A rate» subito dopo.
  - `Services/CalcoliRicorrenti.cs:24` `public sealed record GruppoDiRicorrenti(GruppoRicorrenti Gruppo, IReadOnlyList<RecurringExpense> Regole,`
    `decimal? TotaleMensile);` — `TotaleMensile` null solo per `Terminate`.
  - `Services/CalcoliRicorrenti.cs:194` `public static GruppoRicorrenti Gruppo(RecurringExpense r, DateTime oggi)` —
    prima `Terminate` (Prossima null), poi `switch` su `EveryMonths`: il ramo per kind/installments va prima dello switch.
  - `Services/CalcoliRicorrenti.cs:208` `public static IReadOnlyList<GruppoDiRicorrenti> Raggruppa(IReadOnlyList<RecurringExpense> regole, DateTime oggi)`
  - `Services/CalcoliRicorrenti.cs:222` `public static decimal QuotaDaAccantonare(IReadOnlyList<RecurringExpense> regole, DateTime oggi)` —
    somma solo i gruppi `Annuali` e `OgniNMesi`, quindi Entrate e A rate ne restano fuori senza toccarla.
  - In pagina, `Pages/Ricorrenti.razor` `Titolo(GruppoRicorrenti g)`: un ramo per gruppo, con `_ => throw`; un gruppo
    nuovo senza titolo fa cadere la pagina, non passa in silenzio.

ADJUDICA:
- conformity C1: fondato → corretto — il `@foreach` interno non era rientrato dentro `<ul>` — `@foreach (var r in g.Regole)`
  verificato: risolto — Pages/Ricorrenti.razor:81-83 («`<ul>` è a 16 spazi e `@foreach` a 20 (+4), come Home.razor:176-178»)
- bug-hunter BH1: fondato → corretto — gruppi calcolati con un «oggi» e pastiglia «terminata» con un altro dopo mezzanotte — `var oggi = DateTime.Today;`
  verificato: risolto — Pages/Ricorrenti.razor:130 («campo a riga 115 `private DateTime oggi = DateTime.Today;`, assegnato come prima istruzione di `Leggi` (130), usato a 138, 139 e 150»)
- backend-expert BE1: fuori scope — TIPO progetto: la chiave «senza accenti e maiuscole» è copiata in 3 file (CalcoliGriglia.cs:33-34, TabellaSpese.cs:236, CalcoliRicorrenti.cs); unificarla tocca file fuori perimetro — `g.OrderBy(r => SchemaCampi.RiduciAccenti(r.Description).ToUpperInvariant(), StringComparer.Ordinal)`
- backend-expert BE2: fondato → corretto — condizione della quota con due congiunti ridondanti — `@if (!datiNonLetti && regole.Count > 0 && quota > 0)`
  verificato: risolto — Pages/Ricorrenti.razor:48 («`quota = 0;` (133) precede `await Repository.ElencaAsync` (137) … se ElencaAsync lancia resta 0»)
- backend-expert BE3: fondato → corretto — `Raggruppa` era una catena LINQ a tre livelli — ora `foreach` più `Arrotonda`
  verificato: risolto — Services/CalcoliRicorrenti.cs:208-221,227 («stesso `GroupBy(Gruppo).OrderBy(g => g.Key)` … stesso Math.Round(...,2,AwayFromZero)»)
- backend-expert BE4: infondato — propone di rinominare l'enum in `CategoriaRicorrente`, ma «categoria» in Eton è già la categoria di spesa (`Models/RecurringExpense.cs:23` `[Column("category")] public string Category`): il nome nuovo confonderebbe più di quello attuale — `public enum GruppoRicorrenti { … }`
- Campione sugli infondati: BE4 riverificato da me su Models/RecurringExpense.cs:23 — confermato infondato.

FUORI SCOPE:
- backend-expert BE1 (TIPO progetto, per l'utente): la chiave di ordinamento «senza accenti e maiuscole» ora vive in tre
  copie; proposta `SchemaCampi.ChiaveOrdinamento` in un solo punto. Tocca `Services/SchemaCampi.cs`,
  `Services/CalcoliGriglia.cs`, `Services/TabellaSpese.cs`, fuori perimetro.
- live-testing: sul telefono (390 px) le righe delle ricorrenti mandano a capo cadenza e prossima data e troncano le
  descrizioni lunghe. Preesistente, da 2.1-bis.

GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\02-gruppi-quota → succeeded — 0 errors, 0 warnings | mcp__synapse__test root=G:\Sviluppo\wt-Eton\02-gruppi-quota → 0 failed, 398 passed, 0 skipped | live-testing → criteri 1-5 ok, riga ESITO «difetti» per la sola nota preesistente (vedi tracciato)

SCOSTAMENTI:
- **ui-critic non lanciato.** Dopo il primo giro di `live-testing` (6 regole «PROVA COLLAUDO» create e 6 eliminate,
  elenco vuoto verificato con ricarica in «COLLAUDO GIRO B TEST» e in «Personale») ho chiesto allo stesso agente di
  ricrearne 5 e lasciarle a schermo per la misura di `ui-critic`, che è in sola lettura. Il permesso automatico ha
  negato la navigazione («Modify Shared Resources»): lasciare dati di prova nel database di produzione oltre la vita
  dell'agente non era nel mandato. Non ho aggirato il rifiuto. Per completare serve un sì dell'utente a lasciare, per
  il tempo della misura, 5 regole «PROVA COLLAUDO» con inizio a novembre nello spazio «COLLAUDO GIRO B TEST», poi
  eliminate. Non ne resta nessuna adesso.
- **Il file `-o` di Codex**, come previsto dal mandato, è comparso tutte e tre le volte nella radice del worktree: ne
  ho copiato il contenuto in `codex-1.md`, `codex-2.md`, `codex-3.md` e l'ho tolto; non è nel commit.
- **Codex ha scritto file con PowerShell inline** al terzo lancio (log `codex-3.log`, righe 47-140, fuori da
  `handoff/`): `Pages/Ricorrenti.razor` riscritto con `[System.IO.File]::WriteAllText(…, UTF8Encoding(false))`, il
  resoconto `-o` con `Set-Content`, e un'istantanea del diff in `%TEMP%\eton-02-diff-spazi-prima.txt` (rimossa da me).
  Encoding e accenti verificati dal terzo `checker`: niente BOM né prima né dopo, solo LF come in HEAD, accenti, «» ed
  € intatti, nessun mojibake. Nei primi due lanci solo `apply_patch` (checker).
- Codex ha creato una cartella di build `.artifacts-gruppi/` (ignorata da git) per aggirare un lock: rimossa da me.
- Nel sandbox di Codex `dotnet build` falliva per accesso negato (CS2012, MSB3491 sul target di coverage); i gate
  veri sono i miei, via synapse, tutti verdi.
- `live-testing` ha creato i dati nello spazio «COLLAUDO GIRO B TEST» e non in «Personale», ha rimesso «Personale»
  come spazio attivo, e ha lasciato aperta una scheda vuota di Chrome (id 780391086, «Nuova scheda») che va chiusa a
  mano. Il confronto con regole reali non si è potuto fare: nessuno dei due spazi ne aveva.
- Il server di sviluppo è stato avviato due volte (18936/23820, poi 27816/22536) e fermato; porta 5000 libera.
- Domande al capo: nessuna.

DECISIONI DELL'UTENTE QUI: nessuna

DA RATIFICARE:
- La riga «metti da parte» non compare quando la quota è 0 (nessuna annuale o periodica attiva): raccomando di tenerla
  così, una riga «0,00 €» non dice niente.
- Il conteggio «N spese ricorrenti» in testa all'elenco è sparito: al suo posto ci sono le testate dei gruppi. Raccomando
  di non rimetterlo, le testate dicono già cosa c'è.
- Il gruppo si chiama letteralmente «Ogni N mesi», come nella spec §5. Raccomando di valutarlo nella 2.1-bis
  («Periodiche» si legge meglio e combacia con la frase della quota).
- La quota è l'arrotondamento della somma esatta, quindi può differire di 1 centesimo dalla somma dei due totali di
  gruppo già arrotondati (annuale 100 € + trimestrale 100 €: 8,33 + 33,33 = 41,66 contro 41,67). Raccomando di tenerla
  così: è la cifra più giusta da mettere da parte.

SEGNALAZIONI:
STATO: aperta
DA: Eton, sessione-unità 02-gruppi-quota, 2026-10-20
INNESCO: Codex autore, brief con le tre righe verbatim di `TODO-synapse-codex.md`
Cosa è successo: con la clausola «Niente interpreti con codice inline» nel brief, Codex ha comunque riscritto un file
sorgente con `powershell -Command '… [System.IO.File]::WriteAllText(…)'` e il proprio resoconto con `Set-Content`.
Perché: la clausola elenca `python -`, `python -c`, `node -e` e «here-string passate a un interprete»; PowerShell
`-Command` con .NET o cmdlet di scrittura non è nominato, e Codex su Windows passa ogni comando da PowerShell. Il
`checker` lo vede solo se il brief gli dice di cercarlo nel log.
Proposta: nella «Regola per il mandato» aggiungere alla riga verbatim «e su Windows nemmeno `Set-Content`,
`Out-File`, `[System.IO.File]::Write*` o altri comandi PowerShell che scrivono file: si scrivono solo con apply_patch»,
e nel mandato al `checker` di ogni diff di Codex la ricerca di quelle forme nel log come voce fissa.

COMMIT: fbd2f0e
