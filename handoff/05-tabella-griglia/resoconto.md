UNITÀ: 2.2-B — ESITO: FATTO — verifica: `mcp__synapse__test root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-B` → 0 failed, 389 passed, 0 skipped
TOCCATI:
- Services/ExpenseRepository.cs → +77/−0 (`CambiaCategoriaAsync`, `EliminaTutteAsync`, `LeggiIdAsync`, costante `IdPerRichiesta`)
- Services/TastieraGriglia.cs → +94/−0 (nuovo)
- Eton.Tests/TastieraGrigliaTests.cs → +96/−0 (nuovo, 9 test)
- Shared/Griglia.razor → +570/−0 (nuovo)
- Shared/Griglia.razor.css → +144/−0 (nuovo)
- wwwroot/js/griglia.js → +61/−0 (nuovo)
(totale: `6 files changed, 1042 insertions(+)`, 5 `create mode`)

REVIEW:
review: A — scritture di massa (task 3)
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  non lanciato — 1 file changed, 62 insertions(+) · 0 create mode · 0 dichiarazioni/endpoint
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 2 · non risolti 0 · non verificabili 0
review: B1 — tastiera pura e tipi del contratto di cella (task 4, step 1-4)
  bug-hunter      RILIEVI: 2
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 0
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 14 · non risolti 0 · non verificabili 0
review: B2 — componente Griglia, CSS e modulo JS (task 4, step 5-7)
  bug-hunter      RILIEVI: 5
  conformity      RILIEVI: 2
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 5
  checker         VERDETTI: fondati 7 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 14 · non risolti 0 · non verificabili 0
  live-testing    non lanciato — vincolo del mandato: «Nessuna prova nel browser in questa unità», nessuna pagina monta la griglia; la prova la fa l'unità C
  ui-critic       non lanciato — stesso vincolo del mandato (la tabella §7 direbbe «sì»: `.razor` nuovo con markup e PIANO-DESIGN); rinviato all'unità C
(La seconda voce `checker` di B1 e di B2 è una sola istruttoria sulle correzioni dei due brief — 2 di B1, 7 di B2, 5 riscritture di backend-expert — ricopiata in entrambi i tracciati. La misura del gate di `backend-expert` per A è quella presa prima della correzione, sul solo `Services/ExpenseRepository.cs`; il diff intero dell'unità è `6 files changed, 1042 insertions(+)`, 5 `create mode`, 6 dichiarazioni.)

CONTRATTI (firme reali, da ricopiare nell'unità C):
- `Services/ExpenseRepository.cs:317` `public async Task<EsitoMassa> CambiaCategoriaAsync(IReadOnlyCollection<Guid> ids, string categoria)`
- `Services/ExpenseRepository.cs:348` `public async Task<EsitoMassa> EliminaTutteAsync(IReadOnlyCollection<Guid> ids)`
  Entrambi: id duplicati assorbiti; zero id → nessuna richiesta, esito (0,0,0,0); richieste a blocchi di 100 id (`:305`), in sequenza, **non atomiche fra i blocchi**: un'eccezione a metà risale alla pagina con i blocchi già passati applicati.
- `Services/TastieraGriglia.cs:4` `public enum AzioneTasto { Nessuna, Su, Giu, Sinistra, Destra, Modifica, ModificaSostituendo, SelezionaRiga, SalvaEGiu, SalvaEDestra, SalvaESinistra, Annulla }`
- `Services/TastieraGriglia.cs:16` `public sealed record ModificaCella<T>(T Riga, string Colonna, string Testo);`
- `Services/TastieraGriglia.cs:22` `public enum EsitoCella { Salvata, NonValida, Chiusa }`
- `Services/TastieraGriglia.cs:26` `public sealed record RispostaCella(EsitoCella Esito, string? Messaggio);`
- `Services/TastieraGriglia.cs:41` `public static AzioneTasto Interpreta(string tasto, bool shift, bool ctrlAltMeta, bool inModifica, TipoColonna tipo)`
- `Services/TastieraGriglia.cs:69` `public static (int Riga, int Colonna) Sposta((int Riga, int Colonna) da, AzioneTasto azione, int righe, int colonne)`
- `Services/TastieraGriglia.cs:87` `public static string? Riallinea(IReadOnlyList<string> chiavi, string? attiva, int indicePrecedente)`
- `Shared/Griglia.razor:1` `@typeparam T`, parametri:
  - `:168` `[Parameter, EditorRequired] public IReadOnlyList<ColonnaGriglia<T>> Colonne { get; set; } = [];`
  - `:172` `[Parameter, EditorRequired] public IReadOnlyList<Gruppo<T>> Gruppi { get; set; } = [];`
  - `:174` `[Parameter, EditorRequired] public Func<T, string> ChiaveRiga { get; set; } = default!;`
  - `:177` `[Parameter, EditorRequired] public string Didascalia { get; set; } = "";`
  - `:180` `[Parameter] public Func<T, bool> ContaNeiTotali { get; set; } = _ => true;`
  - `:182` `[Parameter] public Func<T, bool> Selezionabile { get; set; } = _ => false;`
  - `:183` `[Parameter] public IReadOnlySet<string> Selezione { get; set; } = new HashSet<string>();`
  - `:184` `[Parameter] public EventCallback<IReadOnlySet<string>> SelezioneChanged { get; set; }` (quindi `@bind-Selezione`)
  - `:186` `[Parameter] public string? OrdinataPer { get; set; }` · `:187` `[Parameter] public bool Decrescente { get; set; }` · `:190` `[Parameter] public EventCallback<string> OnOrdina { get; set; }` (riceve la `Chiave` della colonna; il verso lo decide la pagina)
  - `:193` `[Parameter] public Func<ModificaCella<T>, Task<RispostaCella>>? SalvaCella { get; set; }`
  - `:195` `[Parameter] public Func<T, string?>? MotivoSolaLettura { get; set; }`
  - `:196` `[Parameter] public Func<T, RenderFragment?>? SottoRiga { get; set; }`
  - `:197` `[Parameter] public string? NotaRiepilogo { get; set; }`
  - `:200` `[Parameter] public Func<T, string, string?>? AncoraCella { get; set; }` (→ attributo `data-tutorial` sulla `td`; null = assente)
- `Shared/Griglia.razor:295` `public async Task EsportaAsync(string nomeFile)` (selezione se non vuota, altrimenti tutte le righe dei gruppi, nell'ordine di resa)
- Comportamenti che la pagina deve sapere:
  - una cella entra in modifica solo se `colonna.Modificabile?.Invoke(riga) == true` **e** `SalvaCella` non è null;
  - testo invariato rispetto a quello visto all'ingresso → `SalvaCella` non viene chiamata;
  - `Chiusa` con messaggio: avviso sotto la riga, oppure in un `<p role="status">` dopo la tabella se la riga non è più mostrata (caso `Sparita`);
  - un'eccezione di `SalvaCella` risale alla pagina; la griglia resta in modifica e riabilita la conferma;
  - Shift+clic applica all'intervallo il nuovo stato della riga cliccata (vedi SCOSTAMENTI).

ADJUDICA:
Brief A
- bug-hunter #1 (tutti gli id in un solo `id=in.(…)`: ≈ 45 caratteri per id dopo la codifica, 300 id ≈ 13,5 KB, una selezione grande supera il tetto della riga di richiesta e l'azione fallisce per intero): fondato → corretto con blocchi da 100 id. `Services/ExpenseRepository.cs:305` `private const int IdPerRichiesta = 100;`. Rilievo su dati: ho riaperto io il codice corretto (`:300-377`), e la logica regge (DELETE solo sui presenti prima, rilettura degli stessi, insiemi uniti prima delle factory).
  verificato: risolto — ExpenseRepository.cs:323/354/369, le tre sole occorrenze di `Constants.Operator.In` stanno dentro `Chunk(IdPerRichiesta)`
- conformity #1 (`new EsitoMassa(0, 0, 0, 0)` scritto a mano due volte): fondato → corretto. Con i blocchi un insieme vuoto non produce richieste, quindi le guardie sono cadute e l'esito esce dalle factory. `Services/ExpenseRepository.cs:304` `// vuoto produce zero blocchi e quindi zero richieste: niente \`in.()\` vuoto, che filtrerebbe male.`
  verificato: risolto — ExpenseRepository.cs:335 `return EsitoMassa.DaModifica(richieste, toccate, ancoraPresenti);`, :362 `return EsitoMassa.DaEliminazione(richieste, prima, dopo);`; nessun `new EsitoMassa(` rimasto fuori dai test
Brief B1
- bug-hunter #1 (Alt/Cmd+Freccia, Invio, F2 intercettati: il Back del browser è soppresso): fondato → corretto. `Services/TastieraGriglia.cs:54` `"ArrowUp" when !ctrlAltMeta => AzioneTasto.Su,`; JS senza `preventDefault` con un modificatore; nuovo test `Con_un_modificatore_frecce_Invio_e_F2_restano_del_browser`.
  verificato: risolto — Services/TastieraGriglia.cs:54-58; wwwroot/js/griglia.js:22 `if (!nelCampo && (carattere || (movimento && !modificatore)))`
- bug-hunter #2 (AltGr, cioè Ctrl+Alt insieme sulla tastiera italiana, scarta @ # € [ ]): fondato → corretto nel chiamante e nel JS, la firma di `Interpreta` non cambia. `Shared/Griglia.razor:443` `… e.MetaKey || e.CtrlKey != e.AltKey …`
  verificato: risolto — Shared/Griglia.razor:443; wwwroot/js/griglia.js:19 `const modificatore = e.metaKey || e.ctrlKey !== e.altKey;`
Brief B2
- bug-hunter #1 (Shift+clic su una riga già selezionata: casella vuota, riga selezionata, perché Blazor non riscrive `checked` se il valore reso non cambia): fondato → corretto rendendo la casella controllata dallo stato. Ogni clic cambia lo stato della riga cliccata, e la casella «tutte» è disabilitata senza righe selezionabili. `Shared/Griglia.razor:566-567` `if (nuova.Contains(chiave)) nuova.ExceptWith(intervallo);` / `else nuova.UnionWith(intervallo);`
  verificato: risolto — Shared/Griglia.razor:564-568, :23 `disabled="@(chiaviSelezionabili.Count == 0)"`
- bug-hunter #2 (dopo l'`await SalvaCella` sposta la cella e ruba il fuoco anche se l'utente è andato altrove): fondato → corretto. `Shared/Griglia.razor:509` `var ripristinaFuoco = azione != AzioneTasto.Nessuna && attiva == a;`; `focalizza` non ruba il fuoco a un elemento fuori dalla tabella.
  verificato: risolto — Shared/Griglia.razor:509, :524-527; wwwroot/js/griglia.js:41
- bug-hunter #3 (avviso di `Chiusa` perso se la riga è sparita): fondato → corretto. `Shared/Griglia.razor:165` `@if (avviso is { } notaPersa && !chiaviMostrate.Contains(notaPersa.Riga)) { <p class="griglia-avviso" role="status">@notaPersa.Testo</p> }`
  verificato: risolto — Shared/Griglia.razor:165; Griglia.razor.css:93-94
- bug-hunter #4 (testo invariato confrontato col valore corrente: un Invio senza cambi dopo una rilettura sovrascrive): fondato → corretto. `Shared/Griglia.razor:487` `if (SalvaCella is not null && testoInModifica != testoIniziale)`
  verificato: risolto — Shared/Griglia.razor:452, :487
- bug-hunter #5 (un'eccezione di `SalvaCella` lascia la cella bloccata in modifica): fondato → corretto. `Shared/Griglia.razor:496-502` `catch { rigaInVolo = null; daConfermare = true; StateHasChanged(); throw; }`
  verificato: risolto — Shared/Griglia.razor:496-502
- conformity #1 (`width: 2.5rem` fuori dalla scala `--s*`): fondato → corretto. `Shared/Griglia.razor.css:41` `padding-inline: var(--s2);`
  verificato: risolto — Griglia.razor.css:40-43, nessun `2.5rem` rimasto
- conformity #2 (`griglia.js` non segue lo stile di `grafo-spazio.js`): fondato → corretto (4 spazi, apici doppi, JSDoc sulle tre esportate).
  verificato: risolto — wwwroot/js/griglia.js:6-11, 31-38, 50-55
- backend-expert #1 (il corpo della tabella ha 8 livelli; il campo di modifica va in un `RenderFragment`): fondato → corretto con la RISCRITTURA. `Shared/Griglia.razor:336` `private RenderFragment Campo(T riga, ColonnaGriglia<T> colonna, bool inVolo, string? descrizione) => @<text>`
  verificato: risolto — Shared/Griglia.razor:336-357, :103 (il corpo della riga resta a 58 righe, poco sopra le ~55 indicate)
- backend-expert #2 (due contatori confrontati solo per uguaglianza sono un bit): fondato → corretto. `Shared/Griglia.razor:479-480` `if (!inModifica || !daConfermare || attiva is not { } a) return;` / `daConfermare = false;`
  verificato: risolto — Invio + focusout e Esc + focusout non salvano due volte (Griglia.razor:479-480, :407-412)
- backend-expert #3 (switch del riepilogo dentro il markup, contenuto duplicato fra `th` e `td`): fondato → corretto. `Shared/Griglia.razor:318` `private static string? TestoRiepilogo(…)`, `:328` `private RenderFragment Aggregato(…)`
  verificato: risolto — Shared/Griglia.razor:149-157
- backend-expert #4 (tabindex deciso da quattro fatti per ogni cella): fondato → corretto. `Shared/Griglia.razor:96` `tabindex="@(cellaNelTab == (chiave, colonna.Chiave) ? 0 : -1)"`
  verificato: risolto — Shared/Griglia.razor:44-45
- backend-expert #5 (predicato «colonna numerica» ripetuto tre volte): fondato → corretto. `Shared/Griglia.razor:316` `private static bool Numerica(ColonnaGriglia<T> c) => c.Tipo is TipoColonna.Denaro or TipoColonna.Numero;`
  verificato: risolto — Shared/Griglia.razor:28, :95, :157, :350
Campione sugli infondati: nessun rilievo è stato giudicato infondato, quindi non c'era niente da riverificare. Il rilievo su dati (A, bug-hunter #1) l'ho riaperto io dopo la correzione, come scritto sopra.

FUORI SCOPE:
- Ctrl+Spazio e Alt+Spazio selezionano la riga (`Services/TastieraGriglia.cs:59`, `" "` senza `when !ctrlAltMeta`), mentre il JS con un modificatore non chiama `preventDefault`. È un'incoerenza minore, segnalata dal checker come nota e non come rilievo. Da sistemare nell'unità C se la prova dal vivo la mostra.
- Su Mac, Option da solo produce caratteri (per esempio @), e `CtrlKey != AltKey` lo tratta come modificatore: lì quei caratteri non aprono la modifica sostituendo (F2 o Invio sì). Il mandato copriva AltGr su Windows.
- La stima del tetto della riga di richiesta di Supabase (gateway) non è verificata: i blocchi da 100 id (≈ 4,5 KB) stanno sotto i tetti tipici, ma il valore reale non è documentato nel repo.
- `DataMinima`/`DataMassima` di `Griglia.razor` sono la terza copia della guardia sulla data (dopo `SpesaEdit` e `RicorrenteEdit`): unificarle resta fuori scope, come dice la correzione 10 del piano.

GATE:
`mcp__synapse__build root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-B` → succeeded — 0 errors, 0 warnings
`mcp__synapse__test root=G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-B` → 0 failed, 389 passed, 0 skipped (380 + 9)

SCOSTAMENTI:
- Test: **389** = 380 + 9. Gli 8 del piano più `Con_un_modificatore_frecce_Invio_e_F2_restano_del_browser`, aggiunto nell'adjudica.
- `ModificaCella<T>`, `EsitoCella`, `RispostaCella` stanno in `Services/TastieraGriglia.cs`, non in `Shared/Griglia.razor` come diceva il piano. Un `.razor` non può dichiarare tipi fuori dalla sua classe, e annidati in `Griglia<T>` la pagina li chiamerebbe `Griglia<RigaSpesa>.ModificaCella`. Firme invariate. **Da ratificare**; raccomandazione: tenerli lì.
- Shift+clic segue la regola di Gmail: applica all'intervallo il **nuovo** stato della riga cliccata, quindi deseleziona se la riga era già selezionata. È la correzione del bug-hunter B2 #1. La spec §5.4 dice solo «Shift+clic per un intervallo». **Da ratificare**; raccomandazione: tenerla, perché è l'unico modo che non lascia la casella e lo stato in disaccordo.
- `Interpreta` con `ctrlAltMeta` vero restituisce `Nessuna` anche per frecce, Invio e F2, non solo per i caratteri. Il chiamante calcola `ctrlAltMeta` come `Meta || Ctrl != Alt`, quindi AltGr non conta come modificatore.
- Scritture di massa a blocchi di 100 id, non in un'istruzione sola come diceva il piano (task 3, step 1). Sono quindi non atomiche anche fra i blocchi, e il `<summary>` lo dichiara.
- `live-testing` e `ui-critic` non lanciati per il vincolo del mandato: nessuna pagina monta ancora la griglia. Il §7 del protocollo, per un `.razor` nuovo con markup, li prescriverebbe. Vanno fatti nell'unità C sulla pagina vera, con questo PIANO-DESIGN come metro.
- PIANO-DESIGN usato nel brief del componente (prima passata di `frontend-design`, solo token esistenti di `app.css`): fondo `--sfondo`; testata, gruppi e tfoot su `--superficie`; solo linee orizzontali in `--bordo`; `--bordo-forte` sotto la testata e sopra il tfoot; cella attiva con `outline` 2px in `--accento`; selezione con sfondo `--accento-tenue`; sola lettura in `--testo-tenue`. Inter per i testi e le intestazioni in tondo (`--t-sm`, 600); Plex Mono (`.dato`) per denaro, numeri e date, che stanno a destra. Testata sticky. Densità di 38px su `pointer: fine`, `var(--tocco)` su `pointer: coarse`. Nessuna animazione oltre allo sfondo delle righe; riga in volo a opacità .6.
- `mcp__synapse__diagnostics` non serve nei worktree: non risolve i pacchetti NuGet né i tipi Blazor, e sui `.razor` non vede gli errori di sintassi. Lo hanno riportato tutti gli implementer. Fa fede la build.
- Decisione di ratifica trovata in `handoff/PIANO.md` (DECISIONI, 2026-10-07, resa del `Numero` in cella dell'unità 2.2-A): da mostrare all'utente. Questa unità non la tocca, perché la tabella delle spese non ha colonne `Numero`.
- Lavoro nuovo arrivato durante l'unità: nessuno.
