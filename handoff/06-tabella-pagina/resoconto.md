UNITÀ: 2.2-C — ESITO: FATTO — verifica: `mcp__synapse__test root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-C` → 0 failed, 389 passed, 0 skipped

Nessuna spesa di prova rimasta: le `PROVA COLLAUDO` create nei due giri di `live-testing` (5 + 4, tutte del 7 ottobre 2026) sono state eliminate con l'eliminazione multipla della tabella («5 spese eliminate.», «4 spese eliminate.»), verificato col filtro testo su «Questo mese», «Quest'anno» e su un intervallo libero 2000-2027. Nessuna spesa vera toccata.

TOCCATI:
- Pages/SpeseTabella.razor → +620/−0 (nuovo: la pagina `/expenses/table`)
- Pages/SpeseTabella.razor.css → +64/−0 (nuovo: resa provvisoria, avviso sotto i 40rem)
- Shared/Griglia.razor → +11/−6 (tre parametri d'ancora, «1 riga», date allineate a destra)
- Shared/Griglia.razor.css → +4/−0 (line-height 1.4 sul tbody su pointer: fine)
- Shared/NavigazioneSpese.razor → +15/−9 (step 8: voce «Tabella» accesa, flag `SoloLargo`)
- Shared/NavigazioneSpese.razor.css → +7/−0 (nuovo: voce nascosta sotto i 40rem, salvo quando è quella attiva)
- handoff/06-tabella-pagina/server.md → +23/−0 (nuovo)
(totale, resoconto compreso: `8 files changed, 912 insertions(+), 15 deletions(-)`, 5 `create mode`, 0 dichiarazioni/endpoint in `*.cs`)

REVIEW:
review: A — la pagina /expenses/table (task 5, step 1-6)
  bug-hunter      RILIEVI: 7
  conformity      RILIEVI: 4
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 3
  checker         VERDETTI: fondati 10 · infondati 0 · fuori scope 1 · non verificabili 0
  checker         VERDETTI: risolti 13 · non risolti 0 · non verificabili 0
  live-testing    ESITO: parziale
  checker         VERDETTI: risolti 6 · non risolti 0 · non verificabili 0
  live-testing    ESITO: verde
  ui-critic       RILIEVI: 5
  checker         VERDETTI: risolti 3 · non risolti 0 · non verificabili 0
  live-testing    ESITO: verde
review: B — la voce «Tabella» si accende (task 5, step 8)
  bug-hunter      RILIEVI: 0
  conformity      non lanciato — 2 files changed, 14 insertions(+), 9 deletions(-)
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 2
  checker         VERDETTI: risolti 1 · non risolti 0 · non verificabili 0
  live-testing    ESITO: verde
  ui-critic       RILIEVI: 1
  checker         VERDETTI: risolti 1 · non risolti 0 · non verificabili 0
  ui-critic       RILIEVI: 0
(Misure del gate. A, prima della review: `3 files changed, 658 insertions(+), 3 deletions(-)`, 2 `create mode`, 0 dichiarazioni/endpoint → tutti e quattro i revisori, `backend-expert` per file nuovo e > 120 righe. B: diff < 30 righe → `bug-hunter` + `threat-hunter`; un file nuovo (`NavigazioneSpese.razor.css`) → `backend-expert`. La prima voce `checker` di B manca perché `bug-hunter` e `conformity` di B sommano zero: nessuna istruttoria; quella presente è la verifica della correzione di `backend-expert`. In A il secondo `live-testing` verde e la terza voce `checker` seguono le correzioni nate dal primo `live-testing` e da `ui-critic`; il terzo `live-testing`, comune ad A e B, ha misurato la barra delle azioni dopo la correzione di `ui-critic` #1 e lo step 8. In B, `ui-critic` è lanciato perché c'è un `.razor.css` nuovo (tabella §7, riga 3). Dopo il suo unico rilievo vengono la verifica `checker` della correzione e la rimisura di `ui-critic`, che torna a zero.)

CONTRATTI:
- Rotta: `Pages/SpeseTabella.razor:1` `@page "/expenses/table"`
- Le nove ancore `data-tutorial` (per l'unità D):
  - `periodo` → `Pages/SpeseTabella.razor:64` `<div data-tutorial="periodo" class="strumenti-riga">`
  - `esporta` → `Pages/SpeseTabella.razor:106` `<button type="button" class="btn compatto" data-tutorial="esporta" …>`
  - `filtri` → `Pages/SpeseTabella.razor:111` `<fieldset class="strumenti-riga" data-tutorial="filtri">`
  - `azioni` → `Pages/SpeseTabella.razor:163` `<div class="azioni-massa …" data-tutorial="azioni" role="region" …>` — **sempre presente** quando c'è la griglia (forma neutra «vuota» senza selezione, accesa con selezione; v. SCOSTAMENTI)
  - `cella-modificabile` / `riga-sola-lettura` → `Pages/SpeseTabella.razor:397-400` `private string? AncoraCella(RigaSpesa riga, string colonna)` (sulla cella Data della prima riga modificabile / di sola lettura nell'ordine di resa, chiavi calcolate da `Ricalcola()`), reso come `Shared/Griglia.razor:98` `data-tutorial="@(AncoraCella?.Invoke(riga, colonna.Chiave))"`. `riga-sola-lettura` è assente se non c'è nessuna riga di sola lettura.
  - `intestazioni` → `Shared/Griglia.razor:17` `<thead data-tutorial="@AncoraIntestazioni">`
  - `selezione` → `Shared/Griglia.razor:21` `<th scope="col" class="griglia-casella" data-tutorial="@AncoraSelezione">` (la casella «tutte»)
  - `riepilogo` → `Shared/Griglia.razor:135` `<tfoot data-tutorial="@AncoraRiepilogo">`
- Parametri nuovi della griglia: `Shared/Griglia.razor:203-205` `[Parameter] public string? AncoraIntestazioni { get; set; }` · `[Parameter] public string? AncoraSelezione { get; set; }` · `[Parameter] public string? AncoraRiepilogo { get; set; }` (null = attributo assente; nessun altro chiamante cambia)
- `Shared/NavigazioneSpese.razor:24` `private sealed record Voce(string Etichetta, string Rotta, bool SoloLargo);` · `:30` `new("Tabella", "expenses/table", true),` — parametro `Attiva` (`:22`) invariato, ora accetta anche `"expenses/table"`
- Consumati senza cambiarli: tutte le firme di 2.2-A e 2.2-B ricopiate nel mandato; `ElencaConPrevisteAsync`, `SalvaAsync`, `Permessi.PuoIntervenire`, `SchedaConflitto`, `ConfermaAzione`, `TestataPagina`. `Services/*` non toccato.

ADJUDICA:
Brief A — revisori
- bug-hunter B1 (alta: `SalvaCella` è un `Func`, dopo un salvataggio la pagina non si ridisegna, la griglia resta coi `Gruppi` vecchi e la seconda modifica sulla stessa riga dà un conflitto contro se stessi): fondato → corretto. Rilievo su dati e concorrenza, riaperto da me: `Sostituisci` aggiorna i campi della pagina ma nessun `StateHasChanged` della pagina scatta da un handler della griglia. `Pages/SpeseTabella.razor:482` `StateHasChanged();`
  verificato: risolto — Pages/SpeseTabella.razor:480-482 `var risposta = EsitoCellaDa(risultato, modifica);` … `StateHasChanged();`; dal vivo: 10,00 → 12,50 aggiorna valore e totale subito, seconda modifica sulla stessa riga senza conflitto
- bug-hunter B2 (con un conflitto aperto, una modifica a un'altra cella della riga sostituisce la voce di `conflitti` e perde la prima modifica): fondato → corretto, poi rifinito dal vivo (V2). `Pages/SpeseTabella.razor:477-478` `if (conflitti.ContainsKey(modifica.Riga.Chiave)) return new RispostaCella(EsitoCella.NonValida, "Questa riga ha un conflitto aperto: …");`
  verificato: risolto — Pages/SpeseTabella.razor:477-478; Shared/Griglia.razor:515-525 lascia il campo aperto col messaggio, nessuna scrittura
- bug-hunter B3 (il cambio di spazio non azzera esito ed avvisi; filtro pagante / `grp=pagante` invisibili e non rimovibili in uno spazio personale): fondato → corretto. `Pages/SpeseTabella.razor:266` `protected override void PrimaDiRicaricare()`, `:280` `if (attivo.IsPersonal && (vista.Filtri.Pagante is not null || vista.RaggruppaPer == TabellaSpese.Gruppi.Pagante))`
  verificato: risolto — Pages/SpeseTabella.razor:266-270, 280-287
- bug-hunter B4 (ogni `change` di una data libera rilegge, disabilita il campo e ricade in silenzio su questo mese): fondato → corretto con date in bozza (`daScelta`, `aScelta`) e rilettura solo su intervallo valido. `Pages/SpeseTabella.razor:251-252` `private bool IntervalloValido => TabellaSpese.DataRagionevole(daScelta, DateTime.Today) && …`
  verificato: risolto — Pages/SpeseTabella.razor:247-248 (poi 251-252), 436-439, 85-88; dal vivo: «Dal» dopo «Al» → messaggio e nessuna rilettura
- bug-hunter B5 (`ElencaAsync` senza paginazione, PostgREST tronca a 1000 righe): fuori scope — `Services/*` non si tocca; v. FUORI SCOPE.
- bug-hunter B6 (Riprova di `ErroreRiprova` non disabilitato durante la rilettura): fondato → corretto. `Pages/SpeseTabella.razor:44` `<ErroreRiprova Messaggio="@errore" OnRiprova="Riprova" Disabilitato="@rileggo" />`
  verificato: risolto — Pages/SpeseTabella.razor:44
- bug-hunter B7 (membri non letti → tutti i gruppi «Membro non più nello spazio», falso): fondato → corretto. `Pages/SpeseTabella.razor:341` `… ?? (membriNonLetti ? "Pagante non disponibile" : "Membro non più nello spazio")`
  verificato: risolto — Pages/SpeseTabella.razor:309-313, 341
- conformity C1 (prefisso console `[Tabella]` invece di `[Spese]`): fondato → corretto. `Pages/SpeseTabella.razor:311` `Console.Error.WriteLine($"[Spese] Nomi dei paganti non disponibili: {ex.Message}");`
  verificato: risolto — Pages/SpeseTabella.razor:311, 487, 567, 596
- conformity C2 (`.pastiglie` duplica `.scelta-categoria`): fondato → corretto. `Pages/SpeseTabella.razor:113` `<div class="scelta-categoria" role="group" aria-label="Categorie">`
  verificato: risolto — Pages/SpeseTabella.razor:113, 140; .razor.css:39 `.scelta-categoria { margin: 0; }`
- conformity C3 (`.campo-strumento` duplica `label.campo` e inventa un terzo aspetto d'etichetta): fondato → corretto. `Pages/SpeseTabella.razor:65-66` `<label class="campo">` `<span class="etichetta-campo">Periodo</span>`
  verificato: risolto — Pages/SpeseTabella.razor:65-66, 165-166
- conformity C4 (frase d'errore senza la causa; Sovrascrivi diceva «salvare»): fondato → corretto, senza la promessa «quello che hai scritto è ancora qui», che nella griglia sarebbe falsa. `Pages/SpeseTabella.razor:488` `"Non è stato possibile salvare: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Riscrivi il valore e riprova fra un momento."`
  verificato: risolto — Pages/SpeseTabella.razor:488, 566-568 (nel catch di Sovrascrivi il conflitto resta)
- threat-hunter: 0 rilievi. Nota non conteggiata (Tab e CR iniziali non neutralizzati nel CSV, `Services/CalcoliGriglia.cs:89`): v. FUORI SCOPE.
- backend-expert #1 (blocco `rileggo/try/Carica/finally` ripetuto tre volte, try annidato in Massa): fondato → corretto con la RISCRITTURA (`Rileggi` e `Massa` finiscono con `await Riprova()`). `Pages/SpeseTabella.razor:412-419` `private async Task Rileggi(VistaTabella nuova)` … `await Riprova();`
  verificato: risolto — Pages/SpeseTabella.razor:319-330, 412-419, 586-601; `rileggo` resta l'ultima istruzione sincrona prima del primo await in ogni percorso
- backend-expert #2 (`EsitoCellaDa` riceveva `chiave` derivabile da `modifica`): fondato → corretto. `Pages/SpeseTabella.razor:494` `private RispostaCella EsitoCellaDa(RisultatoSalvataggio<Expense> risultato, ModificaCella<RigaSpesa> modifica)`
  verificato: risolto — Pages/SpeseTabella.razor:494-496
- backend-expert #3 (`Scheda` con un solo call-site): fondato → corretto inline. `Pages/SpeseTabella.razor:525` `private RenderFragment? SottoRiga(RigaSpesa riga) => !conflitti.ContainsKey(riga.Chiave) ? null : @<SchedaConflitto …/>;`
  verificato: risolto — Pages/SpeseTabella.razor:525-530; compila (build 0/0)
Brief A — primo live-testing (ESITO: parziale, 8 osservazioni)
- V1 filtro di testo con lo stile di default del browser (`type="search"` non è coperto da app.css:896): fondato → corretto. `Pages/SpeseTabella.razor:137` `<input type="text" placeholder="Cerca nella descrizione" …>`
  verificato: risolto — app.css:896 copre `input[type=text]`; dal vivo: background, bordo, font, altezza identici al select
- V2 avviso «conflitto aperto» appeso sotto la riga dopo «Ricarica la sua»: fondato → corretto (vedi B2, `NonValida`).
  verificato: risolto — Pages/SpeseTabella.razor:477-478; dal vivo: nessun avviso dopo «Ricarica la sua»
- V3 righe con la stessa data in ordine diverso a ogni lettura: fondato → corretto con un pre-ordinamento stabile. `Pages/SpeseTabella.razor:341` `.OrderByDescending(r => r.Spesa.CreatedAt).ThenBy(r => r.Chiave, StringComparer.Ordinal)`
  verificato: risolto — Pages/SpeseTabella.razor:341-342; dal vivo: stesso ordine su tre ricarichi
- V4 «1 righe» nel riepilogo: fondato → corretto nella griglia. `Shared/Griglia.razor:137` `… Testi.Conteggio(righeTotali.Count, "riga", "righe") … "Sull'unica riga mostrata" …`
  verificato: risolto — Shared/Griglia.razor:137; dal vivo: «Sull'unica riga mostrata», «Sulla selezione: 1 riga», «… 3 righe»
- V5 le colonne si allargano quando si apre un editor: fuori scope (resa della griglia, 2.1-bis).
- V6 casella «tutte» senza stato intermedio: fuori scope (2.1-bis).
- V7 nessuna frase con zero righe: fondato → corretto. `Pages/SpeseTabella.razor:197` `<p class="testo-tenue" role="status">Nessuna spesa in questo periodo con questi filtri.</p>`
  verificato: risolto — Pages/SpeseTabella.razor:195-198
- V8 messaggio delle date («l'anno prossimo») diverso dalla regola (oggi + 1 anno): fondato → corretto. `Pages/SpeseTabella.razor:87` `Scegli due date dal 2000 a non oltre un anno da oggi, con «Dal» non dopo «Al».`
  verificato: risolto — Pages/SpeseTabella.razor:87 contro Services/TabellaSpese.cs:246-247
Brief A — ui-critic (RILIEVI: 5, metro PIANO-DESIGN)
- #1 alta: comparendo, la barra delle azioni spostava la tabella di 82px sotto il cursore: fondato → corretto, la barra c'è sempre (forma neutra senza selezione). `Pages/SpeseTabella.razor.css:44-46` `.azioni-massa { min-height: calc(var(--tocco) + 2 * var(--s2) + 2px);`
  verificato: risolto — Pages/SpeseTabella.razor:161-186; dal vivo: Δ top prima riga 0px, barra 66px in entrambe le forme
- #2 fedeltà: le date in Plex Mono allineate a sinistra, il PIANO-DESIGN della griglia le vuole a destra: fondato → corretto nella griglia. `Shared/Griglia.razor:28` `class="@(Numerica(colonna) || colonna.Tipo == TipoColonna.Data ? "griglia-numero" : null)"`
  verificato: risolto — Shared/Griglia.razor:28, 95; `griglia-numero` ha solo `text-align: right` (Griglia.razor.css:36), `Numerica` invariata
- #3 due densità (controlli 48px, righe 38px; la barra occupa 275px): fuori scope — la densità esatta è della 2.1-bis (spec §3); v. FUORI SCOPE.
- #4 `TIPO: progetto`, contrasto dei bordi dei controlli 1,21:1 (sotto 3:1): fuori scope per mandato (2.1-bis).
- #5 fedeltà: td 39,75px invece dei 38 del PIANO-DESIGN: fondato → corretto. `Shared/Griglia.razor.css:137-139` `.griglia tbody td { line-height: 1.4; }`
  verificato: risolto — Shared/Griglia.razor.css:131-140, per calcolo (21 + 16 = 37 ≤ 38); **non rimisurato nel browser** (la finestra non scendeva sotto i 594px e il «largo» è stato provato in un iframe)
Brief B
- backend-expert #1 (class come ternario annidato): fondato → corretto con lo `switch` su tupla. `Shared/NavigazioneSpese.razor:9` `var classe = (attiva, v.SoloLargo) switch`
  verificato: risolto — Shared/NavigazioneSpese.razor:8-16
- ui-critic #1 (media, gerarchia, metro: resa di app.css per `.sotto-nav`): sotto i 40rem, su `/expenses/table`, la voce «Tabella» è accesa ma nascosta, quindi la barra non ha nessuna voce attiva visibile (nVisibleActive=0 a 356 e a 596px). Fondato → corretto: la voce nascosta resta visibile quando è quella attiva, e resta nascosta a chi arriva da Registro o da Ricorrenti. `Shared/NavigazioneSpese.razor.css:3` `.sotto-nav a.solo-largo.attiva { display: flex; }`
  verificato: risolto — Shared/NavigazioneSpese.razor.css:2-3 e Shared/NavigazioneSpese.razor:11; dal vivo (rimisura di `ui-critic`): `/expenses/table` 3 voci visibili con 1 attiva, a 356, 596 e 1222px; `/expenses` e `/expenses/recurring` a 356px 2 voci visibili con 1 attiva; nessun troncamento e nessuno scroll orizzontale
- backend-expert #2 (sostituire mobile-first con `@media (max-width: 39.99rem)`): infondato — lo step 8 del piano prescrive testualmente «nascosta di base e mostrata da `40rem` in su, mobile-first» (aggiunta del 7 ottobre), e il progetto usa solo `min-width: 40rem`.
Campione sugli infondati: l'unico infondato (B, backend-expert #2) l'ho riverificato io contro il piano (`docs/superpowers/plans/2026-10-07-spese-tabella.md`, aggiunta del 7 ottobre allo step 8): regge. Riaperti da me anche B1 (dati e concorrenza) prima della correzione.

FUORI SCOPE:
- **Troncamento a 1000 righe** (bug-hunter B5): `Services/ExpenseRepository.cs:63-72` legge senza paginazione, e PostgREST ha `max_rows = 1000` (`supabase/config.toml:18`, il valore di produzione non è verificato). Con «Quest'anno» o un intervallo libero lungo, una tabella oltre le 1000 righe perde in silenzio le più vecchie da totali e CSV. La spec §4.5 accetta la lettura senza paginazione fino a 2000 righe. Va all'utente: è `Services/*`, e tocca anche registro e Home.
- CSV: un testo che comincia con Tab o CR non viene neutralizzato (`Services/CalcoliGriglia.cs:89`, nota di threat-hunter, non promossa a rilievo).
- ui-critic #3 — densità: controlli a 48px sopra righe a 38px su pointer: fine; la barra degli strumenti occupa 275px e la tabella parte oltre il 64% del viewport. Per la 2.1-bis (spec §3).
- ui-critic #4 (`TIPO: progetto`) — bordi dei controlli #242424 su #000, 1,21:1 contro il 3:1 di WCAG 1.4.11: vale per tutti i moduli del progetto. Per la 2.1-bis.
- Transizioni di 160ms ereditate da app.css sui controlli, contro il «nessuna animazione» del PIANO-DESIGN della pagina (misurate da ui-critic oltre il tetto dei 5 rilievi). Per la 2.1-bis.
- live-testing V5 — aprendo l'editor di una cella la colonna Importo si allarga e le descrizioni vanno a capo. V6 — la casella «tutte» non ha lo stato intermedio con una selezione parziale. Per la 2.1-bis.
- live-testing — dopo «Ricarica la sua» il fuoco finisce su `body` (il pulsante sparisce dal DOM): le frecce scorrono la pagina finché non si riclicca una cella.
- Registro (`Pages/Spese.razor`, fuori perimetro): digitando l'importo della spesa successiva mentre la precedente si sta ancora salvando, l'importo si azzera e il «Segna» seguente non salva (osservato da live-testing durante la preparazione dei dati).
- `Pages/Spese.razor:26` ha lo stesso `<ErroreRiprova>` senza `Disabilitato` corretto qui (B6).
- `Shared/Griglia.razor:95` ripete la stessa stringa `"griglia-numero dato"` in due rami del ternario: corretta, solo ridondante.
- Ctrl+Spazio / Alt+Spazio (dal resoconto di 2.2-B): non emerso nella prova dal vivo, non toccato.

GATE:
`mcp__synapse__build root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-C` → succeeded — 0 errors, 0 warnings
`mcp__synapse__test root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-C` → 0 failed, 389 passed, 0 skipped
`live-testing` → ESITO: verde (secondo giro sulla pagina, terzo giro su step 8 e barra delle azioni)
`ui-critic` sulla sotto-navigazione corretta → RILIEVI: 0
`Get-NetTCPConnection -LocalPort 5000 -State Listen` → nessuna connessione: PORTA 5000 LIBERA (server fermato: prima il figlio 27216, poi il padre 27092)

SCOSTAMENTI:
- **Shared/Griglia.razor e Griglia.razor.css toccati oltre «solo per un difetto della prova dal vivo»**, in quattro punti:
  - tre parametri `Ancora*` e i loro attributi: necessari per le ancore `intestazioni`, `selezione` e `riepilogo` che il mandato chiede di produrre, perché cadono nel DOM della griglia, che la pagina non raggiunge;
  - «1 riga» (live-testing V4);
  - date a destra (ui-critic #2);
  - line-height (ui-critic #5).
  **Da ratificare**; raccomandazione: tenerli. Sono parametri opzionali, con default null, e nessun altro chiamante cambia.
- **La barra delle azioni c'è sempre** (forma neutra con una frase d'istruzione senza selezione), non «solo con una selezione» come dicono il piano (step 5) e il PIANO-DESIGN: comparendo spostava la tabella di 82px sotto il cursore (ui-critic #1). L'accento resta solo con una selezione. **Da ratificare**; raccomandazione: tenerla.
- **«Sovrascrivi con la mia» riapplica la sola cella modificata sulla versione del server**, non la riga intera come l'editor singolo (`Pages/SpesaEdit.razor:412-446`): gli altri campi cambiati dall'altra persona restano i suoi. Il piano dice «risalva la stessa modifica con la versione del server», e questa è la lettura meno distruttiva. **Da ratificare**; raccomandazione: tenerla.
- Con «Dal … al …» le date stanno in bozza (`daScelta`, `aScelta`) e la tabella si rilegge solo con un intervallo valido (B4). Un intervallo libero illeggibile nell'URL torna ancora a questo mese senza messaggio, come `VistaTabella.DaQuery` documenta.
- Con zero righe compare una frase sopra la griglia (V7), non prevista dal piano.
- Con `datiNonLetti` la barra degli strumenti resta visibile (si può cambiare periodo per riprovare); solo l'area della griglia mostra il riquadro con Riprova.
- Ordine a parità di valore: prima `CreatedAt` decrescente, poi la chiave della riga (V3). Il piano non lo prevedeva.
- **Sotto i 40rem la voce «Tabella» compare quando è quella attiva** (`ui-critic` B #1). Il piano (step 8) la vuole «nascosta sotto i 40rem». Resta nascosta a chi arriva da Registro e da Ricorrenti; chi invece è già su `/expenses/table` col telefono vede la propria voce accesa sopra l'avviso, invece di una barra senza voce attiva. **Da ratificare**; raccomandazione: tenerla.
- Collaudo: le descrizioni di prova cominciavano con `PROVA COLLAUDO` (mandato), non con `[collaudo]` (piano): vince il mandato.
- Limiti della prova nel browser: la finestra di Chrome non scendeva sotto i 594px di viewport, e nel terzo giro non tornava larga: lo «stretto» è provato sul viewport reale, il «largo» del terzo giro dentro un iframe same-origin largo 1200px. Il pavimento a 360px non è provato. La finestra del browser è rimasta piccola.
- `mcp__synapse__diagnostics` nei worktree non risolve i tipi Blazor: fa fede la build.
- Lavoro nuovo arrivato durante l'unità: nessuno.

DA PROVARE ALL'UTENTE:
1. **La guardia d'uscita** (non provabile da un agente: è un dialogo nativo). Apri `/expenses/table`, fai doppio clic su una cella Importo di una tua spesa, scrivi un numero diverso **senza** premere Invio, poi clicca «Registro» nella sotto-navigazione. Atteso: o la cella salva prima di uscire (il clic fuori salva) o compare l'avviso «vuoi uscire senza salvare?». Se la pagina cambia e il valore nuovo non c'è né in tabella né nel registro, la modifica si è persa: dimmelo.
2. **Le righe di un altro membro.** In uno spazio condiviso dove un altro membro ha segnato almeno una spesa, apri `/expenses/table`. Atteso:
   - la colonna «Pagato da» c'è;
   - la riga dell'altro è grigia e, al passaggio del mouse, spiega «Questa spesa l'ha segnata qualcun altro: …»;
   - Invio o un carattere su quella riga non aprono la modifica.
   Se lo spazio è tuo (ne sei proprietario) quelle righe invece si modificano: è la regola di `Permessi`.
3. **«Non erano tue»** (solo in uno spazio condiviso di cui **non** sei proprietario). Le righe altrui non hanno la casella, quindi dalla tabella non si possono selezionare: il caso si vede solo se un permesso cambia mentre la pagina è aperta. Il test puro lo copre, e qui basta controllare che le righe altrui non abbiano la casella.
4. **Una spesa ricorrente prevista o in arrivo.** In un mese in cui una tua ricorrente è dovuta ma non ancora scritta (o in un periodo futuro), atteso:
   - la riga dice «prevista» o «in arrivo»;
   - non ha la casella e non si modifica;
   - una prevista entra nel totale e la nota dice «di cui N previste»; una in arrivo no.
