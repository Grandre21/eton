UNITÀ: 2 — ESITO: FATTO

TOCCATI:
- Pages/Spese.razor → +11/−6
- Pages/RicorrenteEdit.razor → +6/−7
- Pages/BudgetEdit.razor → +4/−0
- Services/DopoSegna.cs → +14/−0 (nuovo)
- Eton.Tests/DopoSegnaTests.cs → +23/−0 (nuovo)
- handoff/02-residui/brief-review.md, app.md → nuovi (brief comune dei revisori; porta e PID dell'app di prova)

REVIEW:
review: giro 1 — quattro residui (implementer)
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 2
                  NON VERIFICABILE — per live-testing/ui-critic: in G:\Sviluppo\Eton\Pages\BudgetEdit.razor:83-86 la nuova `<p class="spiega">` segue direttamente altre una o due `<p class="spiega">` (righe 67-70 in creazione, 77-80 su un budget esistente): tre righe di spiegazione consecutive sopra il campo Importo, resa e spaziatura da guardare a schermo.
  test rosso      DopoSegnaTests: Eton.Tests/DopoSegnaTests.cs:10:24  error CS0103: The name 'DopoSegna' does not exist in the current context [Eton.Tests] (4 errori, uno per test, righe 10/14/18/22)
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
                  ROSSO: confermati 1 · non confermati 0 · non verificabili 0
review: giro 2 — correzioni BH1, BE1, CF1/BE2 (implementer)
  checker         VERDETTI: risolti 3 · non risolti 0 · non verificabili 0
  live-testing    ESITO: verde (seconda corsa; la prima BLOCKED per login mancante nel pannello Browser)
  ui-critic       non lanciato — tabella §7 riga 2 — 5 files changed, 58 insertions(+), 13 deletions(-)

Misure del gate §3 sul giro 1, dopo `git add -N -- Services/DopoSegna.cs Eton.Tests/DopoSegnaTests.cs`: `git diff HEAD --stat --summary` → `7 files changed, 56 insertions(+), 11 deletions(-)` con 2 `create mode` (Eton.Tests/DopoSegnaTests.cs, Services/DopoSegna.cs); il conteggio comprendeva due file del capo (handoff/CODA.md +1/−1, handoff/PIANO.md +4/−2) non committati in quel momento e non di questa unità. Conteggio dichiarazioni/endpoint → `2`. backend-expert lanciato per file nuovo e dichiarazione di tipo. Cumulativo dei soli file dell'unità a fine giro 2: `5 files changed, 58 insertions(+), 13 deletions(-)`.

CONTRATTI: nessuna firma condivisa con altre unità. Firma nuova, a uso interno: `Services/DopoSegna.cs:12-13`
    public static bool TornaAiBudget(bool daBudget, string tipo, bool aRate)
        => daBudget && tipo == TipoMovimento.Spesa && !aRate;
La frase di (c) sta in un elemento a sé, `Pages/BudgetEdit.razor:83-86`, dopo il blocco della categoria e prima del campo Importo; il resto del modulo non è spostato.

ADJUDICA:
- bug-hunter BH1: fondato → corretto — toccare «Spesa» già acceso riportava la categoria a quella del budget, cancellando una scelta a mano — `if (tipo == TipoMovimento.Spesa && daBudget) nuovaCategoria = Categoria!;`
  verificato: risolto — Pages/Spese.razor:334 (`if (tipo == nuovoTipo) return;`)
- conformity CF1: fondato → corretto — riga di commento di ~160 caratteri in un blocco a ~100 — `non valido o senza descrizione, e la riga deve restare finché manca qualcosa di valido. Su una regola esistente serve invece !Cambiata && Valido, altrimenti il`
  verificato: risolto — Pages/RicorrenteEdit.razor:236-241
- backend-expert BE1: fondato → corretto — la decisione era calcolata prima di `if (aRate)` ma usata solo nell'`else`, dove aRate è falso per costruzione: il caso «a rate» lo decideva la struttura e non la funzione (difetto del mio brief, non dell'implementer) — `if (tornaAiBudget)` dentro l'`else`
  verificato: risolto — Pages/Spese.razor:466-470
- backend-expert BE2: fondato → corretto — stesso difetto di CF1, stessa correzione — Pages/RicorrenteEdit.razor:239
  verificato: risolto — Pages/RicorrenteEdit.razor:236-241 (verdetto CF1 del checker)
Riverificato da me a campione: nessun infondato da campionare; threat-hunter 0 rilievi, letto il suo report (Categoria filtrata da CategorieSpesa.Conosciuta, Pages/Spese.razor OnParametersSet).

FUORI SCOPE:
- nessun rilievo fondato aperto.
- Osservazione mia, non un rilievo: arrivando da un budget, se si sceglie a mano un'altra categoria di spesa, dopo «Segna» si torna comunque ai budget (come oggi). Il mandato fissa i quattro casi di (d) e non la categoria; se si vuole che il ritorno dipenda anche dalla categoria è lavoro nuovo.

GATE: mcp__synapse__build root=G:\Sviluppo\Eton → build: succeeded — 0 errors, 0 warnings — 2.9s
GATE: mcp__synapse__test root=G:\Sviluppo\Eton → test: 0 failed, 446 passed, 0 skipped — 5.0s

SCOSTAMENTI:
- Prova nel browser: prima corsa di live-testing BLOCKED, il pannello Browser non aveva la sessione di Eton. L'utente ha fatto l'accesso con Google in chat; seconda corsa verde, 3/3 scenari, nessun dato creato o salvato, nessun dialogo nativo.
- Non provato nel browser: l'editor di un budget esistente e «Registra una spesa» partendo da un budget vero (nello spazio «Personale» non ci sono budget; cambiare spazio non è stato fatto per non toccare preferenze in produzione) — lo scenario (a) è stato provato aprendo /expenses?categoria=Casa, che è l'URL che quel pulsante apre; il caso «ogni categoria ha già un budget».
- Misure di resa (c), live-testing: a 1280 e 390 px le due `p.spiega` hanno margine 0/12 px, distanze 16 → 12 → 12 px fino al campo Importo; a 390 px la frase va su due righe, nessun overflow (scrollWidth 390). Il dubbio di backend-expert è chiuso.
- PROVA PER L'UTENTE (metà della guardia con la finestra nativa, non eseguibile da un agente): apri «Nuova spesa ricorrente», scrivi un importo (o una descrizione) senza salvare, poi premi «Chiudi»: deve comparire la domanda di conferma. Annulla, svuota il campo, scegli solo un'altra categoria e premi «Chiudi»: si esce senza domanda (questa seconda metà è già provata). Ripeti la prima metà con il tasto Indietro del browser e con F5.
- App di sviluppo avviata (PID 34844, http://localhost:5000, handoff/02-residui/app.md) e fermata prima del resoconto.
- Domande al capo: nessuna.

DECISIONI DELL'UTENTE QUI:
- 2026-11-20 utente (chat, sessione 02-residui): «Fatto, ho fatto l'accesso» (login con Google nel pannello Browser per la prova).

DA RATIFICARE:
- (b) In creazione di una ricorrente non conta più come modifica nessuna scelta con un default (categoria, cadenza, giorno, data, tipo), non solo la categoria: contano solo importo e descrizione scritti. Raccomando di tenerlo: è la regola dell'editor del budget («stessa regola, stessa forma» del mandato).
- (d) Un piano a rate creato da un budget resta nel Registro (lo chiedeva il mandato come scelta da ratificare in PIANO.md): raccomando di tenerlo, perché lì compare l'avviso «Piano a rate creato» col collegamento alle ricorrenti. PIANO.md non l'ho toccato (fuori perimetro): la riga in DECISIONI la scrive il capo.

SEGNALAZIONI: nessuna

COMMIT: 7d5ad61

COMPLETATO 7d5ad61
