UNITÀ: 2.2-D — ESITO: FATTO — verifica: `mcp__synapse__test root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-D-tutorial` → 0 failed, 389 passed, 0 skipped

> Spese di prova: questa unità **non ne ha create** (il tutorial non scrive; nessuna `PROVA COLLAUDO`
> in tabella, contate da `live-testing`). In «Quest'anno» la tabella dell'utente mostra però tre righe
> rimaste da collaudi **precedenti** a questa unità, non toccate: «COLLAUDO GIRO B» (19/09/2026,
> 1.284,50), «COLLAUDO 4 SET» (04/09/2026, 1.284,50), «PROVA AGENTE» (20/08/2026, 12,50). Da decidere
> con l'utente se eliminarle.

Branch: `worktree-unita-2.2-D-tutorial` (worktree `G:\Sviluppo\Eton\.claude\worktrees\unita-2.2-D-tutorial`).

TOCCATI:
- `Shared/Tutorial.razor` (nuovo) → +211/−0
- `Shared/Tutorial.razor.css` (nuovo) → +53/−0
- `wwwroot/js/tutorial.js` (nuovo) → +21/−0
- `wwwroot/css/app.css` → +6/−0 (la sola regola del contorno, in fondo)
- `Pages/SpeseTabella.razor` → +28/−0 (slot `Azione` e `PassiTutorial`)
- `handoff/07-tabella-tutorial/server.md` (nuovo) → +21/−0
(totale prima del resoconto: `6 files changed, 337 insertions(+)`, 4 `create mode`, 0 dichiarazioni/endpoint in `*.cs`)

REVIEW:
review: 2.2-D — tutorial guidato
  autore          codex exec 0.161.0 — 5 files changed, 291 insertions(+)
  bug-hunter      RILIEVI: 3
  conformity      RILIEVI: 3
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 2
  checker         VERDETTI: fondati 3 · infondati 0 · fuori scope 3 · non verificabili 0
  checker         VERDETTI: risolti 6 · non risolti 0 · non verificabili 0
  live-testing    ESITO: difetti
  checker         VERDETTI: fondati 5 (= risolti) · infondati 0 (= non risolti) · fuori scope 0 · non verificabili 0
  live-testing    ESITO: verde (con 3 osservazioni e 1 prova non eseguita, punto 8)
  ui-critic       RILIEVI: 1

(Correzioni: due giri di Codex `resume --last` — il primo sui rilievi della review, il secondo sul difetto
del primo `live-testing`; ciascuno istruito dal suo `checker`.)

CONTRATTI:
- `Shared/Tutorial.razor:40` `public sealed record PassoTutorial(string Ancora, string Titolo, string Testo, string SeAssente);`
- `Shared/Tutorial.razor:44` `[Parameter, EditorRequired] public IReadOnlyList<PassoTutorial> Passi { get; set; } = [];`
- `Shared/Tutorial.razor:47` `[Parameter, EditorRequired] public string Chiave { get; set; } = "";`
- Consumo da fuori, come `GrafoSpazio.Cosa`: `Pages/SpeseTabella.razor:236` `private static readonly IReadOnlyList<Tutorial.PassoTutorial> PassiTutorial =`
- Slot `Azione` di `TestataPagina`: `Pages/SpeseTabella.razor:15` `<Tutorial Chiave="spese-tabella" Passi="PassiTutorial" />`, dentro `<div class="tabella-larga">` (nascosto sotto i 40rem con la tabella).
- Ricordo nel browser: chiave `eton.tutorial.<Chiave>`, valore `"visto"` (`Shared/Tutorial.razor:56` `private string ChiaveRicordo => $"eton.tutorial.{Chiave}";`).
- Le nove ancore consumate, nell'ordine: `periodo`, `filtri`, `intestazioni`, `cella-modificabile`, `riga-sola-lettura`, `selezione`, `azioni`, `riepilogo`, `esporta`.
- Per la 2.1-bis: il contorno è l'**attributo** `data-tutorial-evidenziato` (non la classe del piano, v. SCOSTAMENTI), regola in fondo a `wwwroot/css/app.css`; il riquadro è un `popover="manual"` nel top layer.

ADJUDICA:
- B1 (bug-hunter, alta) fondato → corretto. `Tutorial@if (invito) {…}` su una riga: Razor lo leggeva come indirizzo email, pastiglia sempre visibile con testo letterale (prova del checker nella DLL). `<button …>Tutorial@if (invito) { <span class="pastiglia accesa">nuovo</span> }</button>`
  verificato: risolto — `Shared/Tutorial.razor:12-18`, pulsante su più righe; nella DLL ricompilata «Tutorial@if» = 0 occorrenze.
- B2 (bug-hunter, media) fondato → corretto. Blazor riscrive `class` delle ancore dinamiche (`azioni-massa @(…"vuota"…)`, `griglia-attiva`) e cancellava il contorno. `el.classList.add('tutorial-evidenziato');`
  verificato: risolto — `wwwroot/js/tutorial.js:8` `el.setAttribute("data-tutorial-evidenziato", "");`, `app.css` `[data-tutorial-evidenziato] { … }`; confermato dal vivo: il contorno resta selezionando una riga.
- B3 (bug-hunter, bassa) — il checker: fuori scope (prescritto dal brief). Adjudicato **fondato → corretto**, perché il brief era mio: sotto i 40rem e con un gruppo chiuso le frasi `SeAssente` affermavano una causa falsa. `if (!el || el.getClientRects().length === 0) return null;`
  verificato: risolto — `Pages/SpeseTabella.razor:13-15` pulsante dentro `.tabella-larga`; `:245,:247` le due frasi citano il gruppo chiuso.
- C1 (conformity, bassa) fuori scope, confermato: localStorage dal componente e non da un servizio; un servizio per un solo valore sfora il budget del brief, e il componente è generico per `Chiave`. `if (await JS.InvokeAsync<string?>("localStorage.getItem", ChiaveRicordo) is null)` — duplicazione della chiave tolta con `ChiaveRicordo`.
- C2 (conformity, bassa) — il checker: fuori scope (JS dettato dal brief). Adjudicato **fondato → corretto** (stile dei vicini). `el.classList.add('tutorial-evidenziato');`
  verificato: risolto — `wwwroot/js/tutorial.js` a 4 spazi e doppi apici.
- C3 (conformity, bassa) fondato → corretto. `.tutorial-riquadro {` senza commento di testa.
  verificato: risolto — `Shared/Tutorial.razor.css:1` `/* Resa provvisoria del tutorial: solo token di app.css. La rifà la 2.1-bis. */`
- BE1 (backend-expert, bassa) accettato sulla RISCRITTURA: campo `centrato` ridondante con `stile is null`, annidamento a 4 livelli.
  verificato: risolto — `Shared/Tutorial.razor:23` `class="tutorial-riquadro @(stile is null ? "centrato" : "")"`.
- BE2 (backend-expert, bassa) accettato sulla RISCRITTURA: un `string.Create(CultureInfo.InvariantCulture, …)` per stringa.
  verificato: risolto — `Shared/Tutorial.razor:148-150`.
- L1 (live-testing, difetto bloccante) fondato → corretto. Il riquadro `position: fixed` stava dentro `.pagina-entra`, la cui animazione con `fill-mode: both` lascia un `transform`: blocco contenitore e contesto di impilamento. Riquadro spostato di +280/+48, copriva le ancore ai passi 3 e 7, usciva dallo schermo al passo 9, il banner «versione nuova» gli passava sopra. Rimedio: `popover="manual"` + `showPopover()` (top layer), senza toccare `.pagina-entra`.
  verificato: risolto — `Shared/Tutorial.razor:24` `popover="@(modulo is null ? null : "manual")"`, `wwwroot/js/tutorial.js:20`; dal vivo: sovrapposizione 0 px² e scarto di bordo sinistro 0 in 8 passi su 8 con ancora, passo 9 dentro lo schermo con pulsanti cliccabili, riquadro sopra il banner.
- L2 (live-testing, secondario) fondato → corretto: lampo «centrato» a ogni apertura/cambio di passo (stato azzerato prima dell'`await`).
  verificato: risolto — `Shared/Tutorial.razor:158-160` `stile = nuovoStile; assente = nuovoAssente; indice = nuovo;`; dal vivo: titolo e posizione cambiano nello stesso task.
- UI1 (ui-critic, `TIPO: progetto`) → FUORI SCOPE, per la 2.1-bis.
- Riverifica a campione: non ci sono infondati; ho riaperto io B1 (`Shared/Tutorial.razor:12`, letto prima della correzione) e B3 (`Pages/SpeseTabella.razor.css:6-10`, `.tabella-larga`), esito conforme al checker.

FUORI SCOPE:
- UI1 (`ui-critic`, `TIPO: progetto`, per la 2.1-bis): «nuovo» in `.pastiglia.accesa` ha contrasto 4,45:1 a 11px/600 (soglia 4,5:1). È la classe di progetto, usata anche dai filtri. Rimedio proposto: `--accento-tenue` con alpha ≤ .08, oppure testo #6ea1ff (≈ 5,6:1).
- C1: accesso a `localStorage` dal componente e non da un servizio (v. ADJUDICA).
- Casi limite segnalati dal checker della seconda correzione, improbabili: se `showPopover()` lanciasse, il riquadro resterebbe con `popover="manual"` chiuso, quindi invisibile; e un clic su «Tutorial» nei primi millisecondi, prima che il modulo sia caricato, darebbe un riquadro che sparisce al render successivo.
- Il riquadro non segue lo scorrimento (resa provvisoria, decisione del brief): scorrendo indietro la tabella gli passa sotto e può coprire l'ancora. Il riquadro copre anche la prima colonna delle righe ai passi 3, 4, 6 e 7 (non l'ancora). Per la 2.1-bis.
- Il pulsante «Tutorial» sta nello slot `Azione`, a destra della testata, a ~227px dal «?» (la spec dice «accanto al «?», nello slot `Azione`»: le due cose non coincidono con la testata attuale). Per la 2.1-bis.
- Il piano storico `docs/superpowers/plans/2026-10-07-spese-tabella.md` nomina ancora la classe `.tutorial-evidenziato` (non si modifica).

GATE:
`mcp__synapse__build root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-D-tutorial` → succeeded — 0 errors, 0 warnings
`mcp__synapse__test root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-D-tutorial` → 0 failed, 389 passed, 0 skipped
`live-testing` → ESITO: verde (secondo giro)
`netstat -ano | Select-String ':5000\s+.*LISTENING'` → nessuna riga: PORTA 5000 LIBERA (server fermato: prima il figlio 1916, poi il padre 19360)

SCOSTAMENTI:
- **Contorno per attributo, non per classe.** Il piano (task 6 step 1 e 3) prescrive la classe `.tutorial-evidenziato`; è diventata l'attributo `data-tutorial-evidenziato` perché Blazor riscrive `class` sulle ancore dinamiche (rilievo B2). **Da ratificare**; raccomandazione: tenerla.
- **Riquadro nel top layer** (`popover="manual"` + una terza funzione `mostra` in `tutorial.js`), non previsto dal piano: senza, il riquadro è spostato dall'animazione di `.pagina-entra` (rilievo L1). **Da ratificare**; raccomandazione: tenerla — è la stessa API di `TestataPagina`, e l'alternativa era toccare `.pagina-entra` in `app.css`, fuori perimetro.
- **Pulsante nascosto sotto i 40rem** (dentro `.tabella-larga`) e **due frasi `SeAssente` diverse dal piano** per `cella-modificabile` e `riga-sola-lettura` (citano il gruppo chiuso; rilievo B3). **Da ratificare**; raccomandazione: tenerle.
- Restore nella sandbox di Codex: senza rete, il primo `dotnet build` di Codex è fallito e ha lasciato in `obj/` 4 avvisi NU1900 che la build di synapse ha poi riletto dalla cache. Tolte le due `project.nuget.cache` generate nella sessione e rifatto il restore fuori sandbox: 0 avvisi. Codex ha comunque scritto il diff: nessun ripiego su `implementer`.
- Prova 8 (finestra sotto i 40rem) **non eseguita**: `resize_window` non cambia `innerWidth` sulla finestra massimizzata. Provato solo dal CSS caricato: il pulsante è figlio di `.tabella-larga`. V. DA PROVARE ALL'UTENTE.

DA PROVARE ALL'UTENTE:
1. Apri `/expenses/table` su un PC, poi restringi la finestra del browser fino a quando compare l'avviso «La tabella è pensata per uno schermo largo». Va bene se in quel momento il pulsante «Tutorial» in alto **non** c'è più. Se c'è ancora, il pulsante non si nasconde con la tabella: dillo in chat.
2. Allarga di nuovo la finestra: il pulsante «Tutorial» ricompare accanto al titolo. Se non ricompare, dillo in chat.

LAVORO NUOVO: nessuno arrivato durante l'unità.
