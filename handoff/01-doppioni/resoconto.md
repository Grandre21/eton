UNITÀ: 1 — ESITO: FATTO
TOCCATI:
  Services/ExpenseRepository.cs → +21/−15
  Eton.Tests/ExpenseRepositoryTests.cs → +48/−0
  handoff/01-doppioni/ → brief-codex-1.md, brief-codex-2.md, codex-1.md, codex-2.md, resoconto.md (registro dell'unità)
REVIEW:
  review: 01 — doppioni della lettura a blocchi
    autore          codex exec 0.161.0 — 2 files changed, 69 insertions(+), 15 deletions(-)
    bug-hunter      RILIEVI: 0
    conformity      RILIEVI: 3
    threat-hunter   RILIEVI: 0
    backend-expert  non lanciato — 2 files changed, 69 insertions(+), 15 deletions(-)
                    · 0 create mode · 0 dichiarazioni/endpoint
    test rosso      Una_spesa_ripetuta_fra_due_blocchi_torna_una_volta_sola: Assert.Equal() Failure: Values differ
    checker         VERDETTI: fondati 0 · infondati 0 · fuori scope 0 · non verificabili 0
                    ROSSO: confermati 1 · non confermati 0 · non verificabili 0
    checker         VERDETTI: fondati 3 · infondati 0 · fuori scope 0 · non verificabili 0
    checker         VERDETTI: fondati = non applicabile in modo fix · infondati = non applicabile · fuori scope 0 · non verificabili 0
                    risolti 3 · non risolti 0 · non verificabili 0
  (la riga --stat di backend-expert è quella dei soli file di codice, `git diff HEAD --stat -- Services Eton.Tests`;
   il diff intero con il registro di handoff/ era «7 files changed, 246 insertions(+), 15 deletions(-)», con 5
   create mode tutti in handoff/01-doppioni/, nessun .cs nuovo. Primo checker: solo la riga test rosso, lanciato
   prima che conformity rientrasse; secondo: i tre rilievi; terzo: verifica delle correzioni.)
CONTRATTI:
  Services/ExpenseRepository.cs:119 `public async Task<SpeseDelPeriodo> ElencaConPrevisteAsync(Guid spazioId, DateTime da, DateTime a)` — identica.
  Nuovo, non pubblico: Services/ExpenseRepository.cs:37 `internal static async Task<IReadOnlyList<Expense>> LeggiABlocchiAsync(Func<int, int, Task<IReadOnlyList<Expense>>> blocco)`; `RighePerBlocco` da private a internal const (per il test). ElencaAsync resta private.
ADJUDICA:
  - conformity C1: fondato → corretto — LeggiABlocchiAsync senza commento, mentre gli helper vicini ne hanno uno — `internal static async Task<IReadOnlyList<Expense>> LeggiABlocchiAsync(...)`
    verificato: risolto — Services/ExpenseRepository.cs:34-36 «// Separato da ElencaAsync per provarlo senza il client: blocco(da, a) chiede le righe …»
  - conformity C2: fondato → corretto — using in ordine inverso ai vicini — `using Eton.Services;` / `using Eton.Models;`
    verificato: risolto — Eton.Tests/ExpenseRepositoryTests.cs:1-2 «using Eton.Models; / using Eton.Services;»
  - conformity C3: fondato → corretto — test nuovi in cima, sotto una testata che parla solo di PerIlDatabase — `// Un inserimento fra due blocchi può ripetere una spesa…`
    verificato: risolto — Eton.Tests/ExpenseRepositoryTests.cs:90-136, dopo Giorno_e_mese_a_una_cifra_restano_quelli_scritti (riga 81)
FUORI SCOPE: nessuno
GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\01-doppioni → succeeded, 0 errors, 0 warnings
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\01-doppioni → 0 failed, 392 passed, 0 skipped (base prima del lavoro: 390 passed; +2 test nuovi)
PROVA NEL BROWSER: non serve — nessuna interfaccia toccata (nessun .razor/.css nel diff); sotto le 1000 spese il comportamento è identico, fissato dal test Sotto_un_blocco_la_lettura_resta_quella_di_prima.
SCOSTAMENTI:
  - La clausola verbatim del brief «Il tuo resoconto va solo nel file `-o`» è stata letta da Codex alla lettera, in entrambi i lanci: ha creato nel worktree un file chiamato `-o` col resoconto, e nel file `-o` vero (codex-1.md, codex-2.md) ha scritto solo «Resoconto scritto in -o». Ho spostato a mano il contenuto in handoff/01-doppioni/codex-1.md e codex-2.md e tolto il file `-o` (creato in questa sessione); non è entrato nel commit.
  - Al secondo lancio Codex ha riscritto il file di test con uno script PowerShell di 12 righe passato a `powershell.exe -Command` ([System.IO.File]::WriteAllText, UTF-8 senza BOM), codex-2.log righe 258-269. Non è fra le forme vietate alla lettera (python/node/here-string), ma è codice inline che scrive un file. Esito verificato: diff pulito, encoding UTF-8 invariato, accenti integri, nessuna riga fuori dallo spostamento richiesto.
  - Log dei lanci di Codex (fuori da handoff/): C:\Users\Andre!\.claude\jobs\432f3b03\tmp\codex-1.log e codex-2.log.
  - Nessun file fuori perimetro toccato.
DECISIONI DELL'UTENTE QUI: nessuna
DA RATIFICARE:
  - Il test usa un metodo estratto `internal static` (LeggiABlocchiAsync) invece di un'interfaccia o un finto client Supabase. Raccomandazione: tenerlo, è l'unica astrazione e ha un solo call-site in produzione.
SEGNALAZIONI:
  STATO: aperta
  DA: sessione-unità 01-doppioni (Eton), 2026-10-20
  INNESCO: clausola verbatim del brief Codex da autore, TODO-synapse-codex.md riga 38
  Cosa è successo: la frase «Il tuo resoconto va solo nel file `-o`» ha fatto creare a Codex, due volte su due, un file letterale chiamato `-o` nella radice del worktree, lasciando nel vero file di uscita solo un rimando.
  Perché: Codex non sa che `-o` è il flag del proprio lancio; legge «il file `-o`» come un nome di file. Senza correzione a mano il resoconto resta fuori da handoff/ e un `git add .` lo committerebbe nella radice del repo.
  Proposta: riscrivere la clausola senza nominare il flag, per esempio «Il tuo resoconto è il tuo ultimo messaggio: non scriverlo in nessun file, e non scrivere in `handoff/<NN-slug>/resoconto.md`.»
  ---
  STATO: aperta
  DA: sessione-unità 01-doppioni (Eton), 2026-10-20
  INNESCO: clausola «Niente interpreti con codice inline» dei brief Codex, TODO-synapse-codex.md righe 37-38
  Cosa è successo: Codex ha rispettato la lettera (niente python/node) ma ha riscritto un file con uno script PowerShell inline passato a `powershell.exe -Command`.
  Perché: su Windows Codex passa ogni comando da `powershell.exe -Command`, quindi PowerShell è sempre un interprete inline disponibile e l'elenco della clausola non lo nomina.
  Proposta: aggiungere alla clausola «e niente script PowerShell che scrivono file ([System.IO.File]::Write*, Set-Content, Out-File): le modifiche ai file si fanno solo con apply_patch».
COMMIT: 82f0e5a
