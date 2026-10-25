UNITÀ: 4 — ESITO: FATTO

TOCCATI:
- Pages/Budget.razor → +14/−12
- Pages/BudgetEdit.razor → +3/−2
- wwwroot/css/app.css → +5/−0
- handoff/04-correzioni-budget/brief-codex-1.md, brief-codex-2.md, codex-1.md, codex-2.md → nuovi (registro dei lanci di Codex)

REVIEW:
review: giro 1 — tre difetti della pagina dei budget
  autore          codex exec codex-cli 0.161.0 — 3 files changed, 21 insertions(+), 14 deletions(-)
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 0
  checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
  live-testing    non lanciato — escluso dal mandato («PROVA NEL BROWSER: non la fai tu, e non lanci live-testing»)
  ui-critic       non lanciato — escluso dal mandato («ui-critic non si lancia qui»); la tabella §7 direbbe sì (`.css` toccato, PIANO-DESIGN nel brief)
review: giro 2 — commento di Cambiata (correzione di CF1)
  autore          codex exec codex-cli 0.161.0 (resume --last) — 1 file changed, 1 insertion(+)
  checker         VERDETTI: risolti 1 · non risolti 0 · non verificabili 0

Misure del gate §3 sul giro 1, dopo `git add -N .`: `git diff HEAD --stat --summary` → `6 files changed, 185 insertions(+), 14 deletions(-)` con 3 `create mode` (i tre documenti di handoff: brief-codex-1.md, codex-1.md, resoconto.md); conteggio dichiarazioni/endpoint → `0`. backend-expert lanciato per la regola «file nuovo», letta sul diff reale.

CONTRATTI: nessuna firma cambiata. `Pages/BudgetEdit.razor:160-162`:
    protected override bool Cambiata => Nuova
        ? !string.IsNullOrWhiteSpace(importoTesto)
        : budget is not null && importoTesto != Denaro.TestoDigitabile(budget.Amount);
(proprietà astratta di `Shared/PaginaEditor.cs:48`, firma invariata).

ADJUDICA:
- conformity CF1: fondato → corretto — il commento sopra Cambiata era stato sostituito e aveva perso la ragione del ramo !Nuova — `// Per un budget esistente, stessa conversione del campo, così sopra mille euro il modulo non nasce già modificato.`
  verificato: risolto — Pages/BudgetEdit.razor:158-162

Note dei revisori senza rilievo, girate al capo per la prova (sotto, PASSI PER IL CAPO): spazio fra azioni e budget successivo uguale a quello interno alla riga (backend-expert, bug-hunter); barra stretta a 640-720 px con nomi lunghi; `.vuoto` dentro `.scheda` con possibile spazio doppio sotto; allineamento verticale della barra a schermo largo (bug-hunter).

Controllo del log di Codex (checker, giri 1 e 2): forme di scrittura vietate fuori dall'eco del brief 0 e 0; modifiche solo con apply_patch; nessun file `-o` nel worktree.

FUORI SCOPE:
- Facoltativo non fatto: la sezione «In arrivo» che sparisce durante il cambio di mese. Nasce da `budget = [];` in testa a `Leggi` (`Pages/Budget.razor`), che è il contratto di `Shared/PaginaRegistro.cs:55` («Azzerati PRIMA di leggere»): evitarlo vuol dire codice nuovo (tenere l'elenco durante la sola rilettura del mese), quindi fuori dalla condizione del mandato.

GATE: mcp__synapse__build root=G:\Sviluppo\wt-Eton\04-correzioni-budget → build: succeeded — 0 errors, 4 warnings — 7.3s
GATE: mcp__synapse__test root=G:\Sviluppo\wt-Eton\04-correzioni-budget → test: 0 failed, 442 passed, 0 skipped — 5.1s

PASSI PER IL CAPO (prova nel browser dopo il merge, con almeno un budget nel mese):
1. Barra. In `/expenses/budgets` a 1258 px: ogni `ul.barre-categorie > li > span.barra` ha larghezza > 0 (la colonna `1fr`, attesi qualche centinaio di px) e `span.riempimento` ha larghezza = quota × larghezza della barra (piena se sforato). Le colonne della griglia sono nome | barra | quota | totale, e `div.azioni` sta su una riga sua a tutta larghezza (`grid-column: 1 / -1`). Nel Registro (`/expenses`) le colonne di `ul.barre-categorie` restano 4, con le stesse larghezze di prima. A 390 px: `document.documentElement.scrollWidth` = 390; ogni riga è nome|totale, poi barra|quota, poi le due azioni; `a.btn.compatto` alti 48 px. Da guardare a occhio: se le azioni si leggono come appartenenti al budget sopra (stesso stacco verso il budget sotto); a 640-720 px con un nome lungo la barra non si riduce a pochi px; la barra a schermo largo è centrata in verticale rispetto alle cifre.
2. Guardia. Su «Nuovo budget» tocca solo un'altra categoria (`fieldset.scelta-categoria button.pastiglia`), nessun importo, poi «Torna ai budget»: si torna a `/expenses/budgets` senza finestra nativa. Con un importo scritto, «Torna ai budget» deve ancora chiedere conferma (⚠️ è la finestra nativa che blocca il plugin: quella seconda metà la prova l'utente a mano).
3. Zero budget. In uno spazio senza budget: dentro `section.riepilogo-mese` si vedono `div.testa-registro` col mese corrente e le due frecce `‹ ›` (48 px), e sotto `div.vuoto` con «Ancora nessun budget qui». Da guardare: spazio sotto il `div.vuoto` non doppio (padding della `.scheda` più margine del `.vuoto`).

SCOSTAMENTI:
- Guardia d'uscita (punto 2): l'editor delle ricorrenti usa la stessa regola che ha causato il blocco — `Pages/RicorrenteEdit.razor:388-389`, ramo nuovo: `… || categoria != CategorieSpesa.Elenco[0] || …`, cioè una scelta diversa dal default conta come modifica. Come chiede il mandato, il budget è allineato al criterio «importo scritto o modificato», che è quello di note, collezioni ed elementi (contano solo i campi scritti). RicorrenteEdit è fuori perimetro e non è cambiato.
- Codex al giro 1 non è riuscito a compilare né a lanciare i test (CS2012, `obj\Debug\net10.0\Eton.dll` negato dalla sandbox): build e test li ha fatti la sessione, verdi.
- ui-critic e live-testing non lanciati per il mandato, nonostante la tabella §7.
- Domande al capo: nessuna.

DECISIONI DELL'UTENTE QUI: nessuna

DA RATIFICARE:
- Le azioni della riga di budget sono raccolte in un `div.azioni` sotto le cifre invece che in una colonna: raccomando di tenerlo, è l'unico modo di avere le stesse quattro colonne del Registro con una sola regola CSS.

SEGNALAZIONI: nessuna

COMMIT: 11797cf
