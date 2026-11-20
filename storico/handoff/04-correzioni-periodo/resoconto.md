UNITÀ: 4 — ESITO: FATTO
TOCCATI:
- Pages/BudgetEdit.razor → +3/−1
- wwwroot/css/app.css → +5/−0
- handoff/04-correzioni-periodo/app.md (nuovo, porta e pid dell'app), prova/ (tre schermate), resoconto.md
REVIEW:
review:
  bug-hunter      RILIEVI: 0
                  NON VERIFICABILE — per live-testing/ui-critic: i numeri di scrollWidth (1808/1265 a 1280 px, 1466/375 a 375 px) non riprodotti; da confermare in un browser che a 375 e a 1280 px la pagina non ecceda più, e che le quattro colonne restino allineate
  threat-hunter   RILIEVI: 0
  conformity      non lanciato — 2 files changed, 8 insertions(+), 1 deletion(-)
  backend-expert  non lanciato — 2 files changed, 8 insertions(+), 1 deletion(-)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         nessuna istruttoria
  live-testing    ESITO: verde (con una nota su un numero a 1280 px, sotto)
  ui-critic       RILIEVI: 0
La riga NON VERIFICABILE di bug-hunter è entrata nel brief di live-testing (scenario 1) ed è stata provata: 1280 px nessun trabocco (max right 1264,8), 375 px scrollWidth 375 = clientWidth 375; ui-critic ha rimisurato anche a 360 px.
CONTRATTI: invariati quelli della 03. Il messaggio della sovrapposizione è rimasto parola per parola:
- Pages/BudgetEdit.razor:280 `errore = "C'è già un budget per questa categoria in quei mesi: forse l'ha appena creato qualcun altro. Torna ai budget per vedere com'è adesso.";`
CAUSA DI (a): non era un'eccezione non gestita. In Blazor WebAssembly 10 il runtime collega l'uscita d'errore (stderr) a una funzione che scrive in console E mostra `#blazor-error-ui`: in bin/Debug/net10.0/wwwroot/_framework/blazor.webassembly.w3qd1tpl0e.js, `out:Ot,err:Tt` con `Tt=e=>{console.error(e||"(null)"),yt()}`, e `yt()` imposta `display:block` su `#blazor-error-ui`. Il `Console.Error.WriteLine` del ramo della sovrapposizione accendeva quindi la barra. Correzione: Pages/BudgetEdit.razor:277-279, il log va su `Console.WriteLine`. Nessun test rosso: la causa sta nel JavaScript del framework, non in codice puro (lo chiedeva il mandato di dichiararlo).
CORREZIONE DI (b): wwwroot/css/app.css:2125 `.barre-categorie > li > .azioni > .spiega { overflow-wrap: anywhere; }`, accanto alle regole di `.barre-categorie > li > .azioni`. `anywhere` e non `break-word` (che il body già eredita, app.css:285): solo `anywhere` abbassa la larghezza minima dell'elemento flessibile.
PROVA NEL BROWSER (live-testing, browser integrato, spazio «Personale», dati di produzione col sì dell'utente in questa sessione):
- (b) 1280 px: scrollWidth 1265, clientWidth 1280, scrollX resta 0, max right 1264,8 (prima 1808); descrizione su 3 righe dentro la scheda, pulsanti dentro la scheda. 375 px: scrollWidth 375 = clientWidth 375. Schermate prova/s1-elenco-1280.jpg, prova/s1-elenco-375.jpg.
- (a) scheda B, due «Salva»: messaggio esatto, modulo compilato, `#blazor-error-ui` display «none» entrambe le volte; il log «[Budget] Rifiutato per sovrapposizione: {"code":"23P01",…}» compare come log normale; 2 POST 400 attesi. Schermata prova/s2-rifiuto-scheda-B.jpg.
- Pulizia: creati ed eliminati eae55877-beec-485b-807f-40d2fd296c3c (PROVA2.5xxx…) e c04bd535-c1a5-4827-8175-a2a262f70c85 (PROVA 2.5 bis); nessun salvataggio della scheda B è riuscito. Elenco finale «Ancora nessun budget qui.», nessun «In arrivo».
- Console: solo l'errore noto del service worker in sviluppo e le due righe del browser per i 400.
ADJUDICA: nessun rilievo
FUORI SCOPE:
- Lo stesso meccanismo vale per tutto il progetto: 106 `Console.Error.WriteLine` in 29 file (conteggio di Grep su *.cs e *.razor). Ogni errore gestito che scrive su stderr accende la barra «errore imprevisto» anche quando la pagina mostra già il suo messaggio. Nel solo BudgetEdit restano quelli di apertura, salvataggio, sovrascrittura, chiusura ed eliminazione non riusciti. Va deciso se passare i log degli errori gestiti su stdout (o su ILogger a livello Warning) in tutto il progetto: è un'unità a sé.
- ui-critic, non misurato e precedente alla correzione: con una descrizione corta, a 1280 px il testo di `.spiega` sta in alto nella riga mentre le etichette dei pulsanti (alti 48 px) stanno al centro (`.azioni` ha `align-items: normal`).
GATE: mcp__synapse__build root=G:\Sviluppo\Eton → build: succeeded — 0 errors, 0 warnings — 8.8s — root: G:/Sviluppo/Eton
GATE: mcp__synapse__test root=G:\Sviluppo\Eton → test: 0 failed, 451 passed, 0 skipped — 5.1s — root: G:/Sviluppo/Eton
SCOSTAMENTI:
- Il mandato chiedeva a 1280 px `scrollWidth` uguale a `clientWidth`: misurato 1265 contro 1280. Non è un trabocco (scrollWidth minore, scroll orizzontale impossibile, max right 1264,8): 1265 è la larghezza utile del body, come nella misura del capo; la differenza di 15 px è la barra di scorrimento verticale nell'emulazione del viewport. ui-critic, con la sua sonda, ha letto 1265 = 1265.
- Il primo lancio di live-testing è stato negato dal classificatore della modalità automatica perché l'autorizzazione a scrivere sui dati di produzione arrivava dal mandato e non dall'utente in questa sessione. Ho chiesto all'utente in chat, che ha risposto «Sì, procedi»; poi la prova è partita.
- ui-critic ha lavorato sulle schermate e su una riga ricostruita solo nel DOM (aggiunta e tolta via script, nessun dato scritto), perché i budget di prova erano già cancellati.
DECISIONI DELL'UTENTE QUI: 2026-11-20 utente (chat, sessione 04-correzioni-periodo): «Sì, procedi» alla prova con salvataggio e cancellazione dei budget di prova «Casa»/«PROVA…» nello spazio «Personale».
DA RATIFICARE: nessuna
SEGNALAZIONI:
STATO: aperta
DA: unità 04 di Eton, 2026-11-20
INNESCO: un mandato che porta un'autorizzazione dell'utente sui dati di produzione a una side session
Cosa è successo: il mandato riportava il sì dell'utente alla prova con salvataggio, ma il classificatore della modalità automatica ha negato il lancio di live-testing, perché quel sì non era stato detto in questa sessione; è servita una domanda in più all'utente.
Perché: è lo stesso confine già registrato per le unità `--bg` (un sì su dati di produzione non passa via mandato); vale anche per le side session aperte con start_session.
Proposta: nel mandato di una side che scrive su dati di produzione, prevedere fin dall'inizio la domanda all'utente in chat prima della prova, invece di citarne l'autorizzazione.
COMMIT: d6b8030
