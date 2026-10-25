UNITÀ: 2 — ESITO: PARZIALE
TOCCATI:
  Eton.Tests/CalcoliBudgetTests.cs → +103/−0 (nuovo)
  Models/CategoryBudget.cs → +25/−0 (nuovo)
  Pages/Budget.razor → +174/−0 (nuovo)
  Pages/BudgetEdit.razor → +355/−0 (nuovo)
  Pages/Spese.razor → +44/−0
  Program.cs → +1/−0
  Services/BudgetRepository.cs → +80/−0 (nuovo)
  Services/CalcoliBudget.cs → +57/−0 (nuovo)
  Shared/NavigazioneSpese.razor → +2/−1
  handoff/02-budget/ → brief-codex-1.md, brief-codex-2.md, codex-1.md, codex-2.md, server.md, resoconto.md (registro dell'unità)
REVIEW:
  review: brief-codex-1 — budget nel client
    autore          codex exec codex-cli 0.161.0 — 12 files changed, 1040 insertions(+), 1 deletion(-) (con i file di handoff/)
    bug-hunter      RILIEVI: 3
    conformity      RILIEVI: 3
    threat-hunter   RILIEVI: 0
    backend-expert  RILIEVI: 4
    test rosso      CalcoliBudgetTests: CalcoliBudgetTests.cs(8,20): error CS0246: Il nome di tipo o di spazio dei nomi 'CategoryBudget' non è stato trovato. Probabilmente manca una direttiva using o un riferimento all'assembly.
    checker         VERDETTI: fondati 5 · infondati 0 · fuori scope 1 · non verificabili 0
                    ROSSO: confermati 1 · non confermati 0 · non verificabili 0
  review: brief-codex-2 — correzioni BH1 BH2 BH3 BE1 C2 C3
    autore          codex exec codex-cli 0.161.0 (resume) — 9 files changed, 841 insertions(+), 1 deletion(-) (diff cumulativo dell'unità, senza handoff/)
    checker         fix: risolti 6 · non risolti 0 · non verificabili 0
    live-testing    non lanciato — rifiutato dal classificatore della modalità auto (v. FUORI SCOPE)
    ui-critic       non lanciato — tabella §7 riga 3 richiede ui-critic, ma solo dopo `ESITO: verde` di live-testing, che non c'è
CONTRATTI:
  tabella `category_budgets` (unità 1) — Models/CategoryBudget.cs:6-24: `[Table("category_budgets")] public class CategoryBudget : BaseModel`; `[Column("created_by", ignoreOnUpdate: true)] public Guid CreatedBy`; `version`/`created_at`/`updated_at` con `ignoreOnInsert: true, ignoreOnUpdate: true`.
  percorso di lettura unico — Pages/Budget.razor (Leggi): `var periodo = await Spese.ElencaConPrevisteAsync(attivo.Id, primo, ultimo);` poi `CalcoliSpese.PerMese(periodo.Righe, anno, mese)`; Pages/Spese.razor usa il `riepilogo` già calcolato da `PerMese`. Nessuna lettura diretta di `expenses`.
  residuo sulle stesse righe del totale — Services/CalcoliBudget.cs:22 `public static IReadOnlyList<StatoBudget> DelMese(IEnumerable<CategoryBudget> budget, RiepilogoMese riepilogo, int anno, int mese)`: lo speso è il `Totale` di `riepilogo.Categorie` (le entrate le esclude già `PerMese`).
  categorie — Services/CalcoliBudget.cs:37 `public static IReadOnlyList<string> CategorieDisponibili(IEnumerable<CategoryBudget> budget)` su `CategorieSpesa.Elenco`.
  concorrenza — Services/BudgetRepository.cs:40 `public async Task<RisultatoSalvataggio<CategoryBudget>> SalvaImportoAsync(Guid budgetId, int versioneLetta, decimal importo)` e :49 `public async Task<RisultatoSalvataggio<CategoryBudget>> ChiudiAsync(Guid budgetId, int versioneLetta, DateTime ultimoMese)`: `version` solo nel filtro, `.Set` solo su `Amount`/`EndsOn`; `Esito` privato come RecurringExpenseRepository.
  indirizzo di «Registra una spesa» — Pages/Budget.razor: `href="@($"expenses?categoria={Uri.EscapeDataString(categoria)}")"`; Pages/Spese.razor:326 `[SupplyParameterFromQuery(Name = "categoria")] public string? Categoria { get; set; }`, accettata solo se `CategorieSpesa.Conosciuta(Categoria, TipoMovimento.Spesa)`; dopo «Segna» `Navigation.NavigateTo("expenses/budgets")`.
  chiusura — Services/CalcoliBudget.cs:55 `public static bool SiPuoChiudere(CategoryBudget b, DateTime oggi)` (`ends_on` = mese corrente; un budget che parte il mese dopo si elimina soltanto). Eliminazione: `BudgetRepository.EliminaAsync` (le ricorrenti hanno «Elimina»).
  nessuna sovrapposizione — Services/CalcoliBudget.cs:43 `public static DateTime InizioDiUnNuovo(IEnumerable<CategoryBudget> budget, string categoria, DateTime oggi)`: massimo fra il mese corrente e `ends_on` + 1 mese dei budget chiusi della categoria; l'editor lo dice («Il budget precedente di X vale ancora questo mese.»).
  permessi — Pages/BudgetEdit.razor: `Permessi.PuoIntervenire(mioId, budget.CreatedBy, budget.SpaceId, Spazi.Elenco)`; chi non può non vede Salva/Chiudi/Elimina.
ADJUDICA:
  - bug-hunter BH1: fondato → corretto — lo stato «nessun budget» dipendeva da `budget.Count` azzerato prima dell'await, e lampeggiava a ogni cambio di mese — `else if (budget.Count == 0)`
    verificato: risolto — Pages/Budget.razor:53 (`else if (senzaBudget)`, assegnato a :138 dopo `ElencaAsync`; scheletro dentro la scheda a :72-75)
  - bug-hunter BH2: fondato → corretto — un budget che parte il mese dopo non era raggiungibile da nessuna schermata — `private bool PuoAndareAvanti => anno != DateTime.Now.Year || mese != DateTime.Now.Month;`
    verificato: risolto — Pages/Budget.razor:98-115 (sezione «In arrivo» con `href="expenses/budgets/@b.Id"`)
  - bug-hunter BH3: fondato → corretto — dopo un conflitto nato da «Chiudi», «Sovrascrivi» riscriveva l'importo e non chiudeva (difetto copiato da RicorrenteEdit.Termina) — `await SalvaCon(versione, importo);`
    verificato: risolto — Pages/BudgetEdit.razor:43 (`SovrascriviAbilitato="@(Valido && PuoIntervenire && !conflittoDaChiusura)"`)
  - conformity C1: fuori scope — vero, ma Shared/PaginaRegistro.cs è fuori perimetro, e il commento era già impreciso prima (anche SpeseTabella fa l'override) — `/// inCaricamento. Solo Spese la usa.`
  - conformity C2: fondato → corretto — mancava il commento di rimando a Permessi.Spiegazione sopra la frase sui permessi — `<p class="spiega">Questo budget l'ha creato qualcun altro: …</p>`
    verificato: risolto — Pages/BudgetEdit.razor:45
  - conformity C3: fondato → corretto — `OnParametersSet` e i campi nuovi in mezzo ai campi del modulo — `protected override void OnParametersSet()`
    verificato: risolto — Pages/Spese.razor:325-329 e :379
  - backend-expert BE1: fondato → corretto — «nessuno spazio attivo» lanciato come eccezione e ripreso dal catch, con un messaggio sbagliato — `if (attivo is null) throw new InvalidOperationException("Nessuno spazio attivo.");`
    verificato: risolto — Pages/BudgetEdit.razor:184-185
  - backend-expert BE2: fuori scope — TIPO: progetto: lo `switch` a quattro esiti ripetuto in SalvaCon e Chiudi, stessa forma di RicorrenteEdit (SalvaCon/Termina); va all'utente — `switch (esito.Esito)`
  - backend-expert BE3: fuori scope — TIPO: progetto: il blocco `rileggoMese = true; try { await Carica(); } finally { … }` in tre copie e l'aritmetica del mese a mano, stessa forma di Spese.razor; va all'utente — `if (mese == 1) { anno--; mese = 12; } else { mese--; }`
  - backend-expert BE4: fondato → corretto — stesso rilievo di conformity C3, corretto con lo stesso fix — `protected override void OnParametersSet()`
    verificato: risolto — Pages/Spese.razor:325-329 e :379
  Rilievi di sicurezza e concorrenza: threat-hunter 0; BH3 (concorrenza) l'ho aperto io in Pages/BudgetEdit.razor e confermato prima del fix. Campione sugli infondati: nessun infondato in questa unità.
FUORI SCOPE:
  - **Prova nel browser (§7) non eseguita**, e quindi nemmeno ui-critic: il lancio di live-testing, che per mandato crea in produzione un budget di prova e una spesa «PROVA AGENTE» e poi li elimina, è stato rifiutato dal classificatore della modalità auto («judged this action dangerous», senza motivo). Il capo ha scelto di chiudere PARZIALE e di portare la prova all'utente. Nessun dato di prova è stato creato. Passi per rigiocarla da fuori (server: `dotnet run --project <worktree o repo>/Eton.csproj --launch-profile Eton --no-build`, http://localhost:5000, Chrome deviceId d3148d48-d283-4d4a-a07a-95a77fa72150):
    1. /expenses: c'è la voce «Budget» nella sotto-navigazione; porta a /expenses/budgets (mese corrente con ‹ ›, la frase sulle spese ricorrenti).
    2. «Nuovo budget»: le pastiglie escludono le categorie con un budget aperto. Scegliere una categoria senza budget (es. «Istruzione»), importo 50,00; la pagina dice «Vale da <mese corrente>, ogni mese.». Salva → torna a /expenses/budgets; la riga mostra «X / 50,00 €», «restano …», la barra, «Registra una spesa» e «Modifica».
    3. /expenses: sotto «Entrate · Saldo» compare «Da spendere nei budget N €» con «Vedi i budget».
    4. Dai budget, «Registra una spesa» sulla riga di prova: indirizzo /expenses?categoria=<categoria>, Tipo «Spesa», categoria accesa, data di oggi. Importo 12,00, descrizione «PROVA AGENTE», «Segna» → si torna da soli ai budget, speso +12 e residuo −12.
    5. Nel Registro N è calato di 12 e «PROVA AGENTE» è nell'elenco.
    6. Nei budget, ‹ (mese prima): il budget di prova non c'è; ›. Durante il cambio la scheda con le frecce resta e non compare «Ancora nessun budget qui».
    7. «Modifica»: categoria in sola lettura, importo 50,00 → 10,00, Salva → «Salvato.»; nei budget la riga dice «sforato di …» con la barra piena.
    8. «Modifica» → «Chiudi» → «Sì, chiudi»: «Chiuso: vale fino alla fine di questo mese.», «Ultimo mese: <mese corrente>». «Nuovo budget» con la stessa categoria dice «Vale da <mese dopo>» e «Il budget precedente di <categoria> vale ancora questo mese.»: non salvare, uscire senza importo scritto.
    9. /expenses?categoria=Stipendio (di entrata): il modulo non cambia.
    10. A 390 px: nessuno scorrimento orizzontale; «Registra una spesa» e «Modifica» alti almeno 48 px.
    11. Pulizia: eliminare la spesa «PROVA AGENTE» dal Registro e il budget di prova da «Modifica» → «Elimina» → «Sì, elimina»; ricaricare /expenses e /expenses/budgets e verificare che non ci siano più. Attenzione: l'editor del budget ha la guardia d'uscita (NavigationLock); ricaricarlo o lasciarlo con modifiche non salvate apre una finestra di conferma nativa, che blocca il plugin.
    Poi ui-critic (tabella §7 riga 3: file .razor nuovi con markup e un PIANO-DESIGN nel brief).
  - conformity C1: aggiornare i commenti di Shared/PaginaRegistro.cs:16 e :64 («Solo Spese la usa»): oggi la usano Spese, SpeseTabella e Budget. File fuori perimetro.
  - backend-expert BE2 e BE3 (TIPO: progetto): ripetizioni copiate dai modelli delle ricorrenti e del Registro; decisione dell'utente se unificarle anche lì.
  - Nota del checker sulle correzioni: durante la rilettura di un mese la sezione «In arrivo» scompare e ricompare (Leggi azzera `budget` prima dell'await). Da far giudicare alla prova nel browser.
GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\02-budget → build: succeeded — 0 errors, 4 warnings — 2.6s (i 4 avvisi sono NU1900: indice delle vulnerabilità NuGet non raggiungibile)
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\02-budget → test: 0 failed, 432 passed, 0 skipped — 4.3s
SCOSTAMENTI:
  - Domanda al capo, inviata il 25 ottobre 2026 alle 01:3x circa (orario esatto non annotato; dopo il commit 9d356d0): «la prova nel browser sui dati di produzione è negata dal controllo dei permessi in background: come procedo? (A) chiudo PARZIALE con §7 in FUORI SCOPE; (B) lascio il server acceso e aspetto». Risposta di coordinatore-Eton, ricevuta prima delle 02:04: «opzione A. Chiudi l'unità PARZIALE e metti la prova nel browser (§7) in FUORI SCOPE, con il motivo […] Elenca nel resoconto i passi esatti della prova […] Ferma il server di sviluppo e scrivilo in server.md. Fai l'ultimo commit del resoconto sul ramo. Non provare altre strade per la prova: la porto io all'utente.»
  - Il ritorno ai budget dopo «Segna» è automatico (la spec dice «si torna al budget»); il mandato lasciava aperto il come.
  - Il selettore del mese della pagina dei budget non va oltre il mese corrente, come il Registro; i budget che partono dopo stanno nella sezione «In arrivo» (correzione di BH2).
  - `[SupplyParameterFromQuery]` non era mai usato nel progetto: verificato da doc-checker sulla documentazione di ASP.NET Core 10 (punto 1 VERIFICATO; il riuso dell'istanza fra /expenses?categoria=X e /expenses non è documentato, quindi il codice confronta il valore a ogni OnParametersSet e regge in entrambi i casi).
  - Log di Codex fuori dal repo: C:\Users\Andre!\.claude\jobs\f77272f3\tmp\codex-1.log e codex-2.log; il checker non vi ha trovato forme vietate (scritture solo via apply_patch).
  - Server avviato e fermato da questa sessione (handoff/02-budget/server.md).
DECISIONI DELL'UTENTE QUI: nessuna
DA RATIFICARE:
  - Rotte `/expenses/budgets`, `/expenses/budgets/new`, `/expenses/budgets/{id}` e parametro `?categoria=` per «Registra una spesa». Raccomandazione: confermare, seguono le rotte delle ricorrenti.
  - Ritorno automatico ai budget dopo «Segna» quando il Registro è stato aperto da un budget. Raccomandazione: confermare, è il «si torna al budget» della spec.
  - «Chiudi» offerto solo se il budget vale già nel mese corrente; uno che parte il mese dopo si può solo eliminare (il database rifiuterebbe ends_on < starts_on). Raccomandazione: confermare.
  - La «barra per categoria» del popup del 24 ottobre sta nella pagina dei budget; sotto il Registro c'è solo la riga «Da spendere nei budget» con il collegamento. Raccomandazione: confermare, è ciò che dicono spec §3 e mandato.
  - «sforato di X» usa lo stesso colore di «restano» (nessun CSS nuovo, la resa curata è della 2.1-bis). Raccomandazione: confermare per ora, da rivedere nella 2.1-bis.
SEGNALAZIONI: nessuna
COMMIT: 9d356d0 (codice), 26d136e (server.md fermato) sul ramo unita-02-budget
