handoff/CHIUSURA.md — lo scrive la sessione di chiusura
CHIUSURA: 4 unità — FATTO 4 · PARZIALE 0 · BLOCKED 0
           (PARTIZIONE di PIANO.md e ESITO dei resoconti coincidono per 01, 02, 03, 04. La riga «prova del capo», che non
           è un'unità, dice ancora PARZIALE: i suoi due KO li ha corretti e riprovati nel browser la 04, quindi il testo
           è superato, non divergente)
COPERTURA: 6 clausole — coperte 5 · scoperte 0 · rinviate 1
           rinviata: «se hai dubbi rivolgiti al techadvisor» — preferenza di metodo, nessun diff la tocca; i resoconti
           01-04 non registrano ricorsi a tech-advisor (le posizioni di tech-advisor del 20 nov stanno in DECISIONI).
           Senza di lei: 5 clausole, 5 coperte.
           Nota di coverage: la clausola «selezionare dalle voci di spesa già registrate quali fanno parte di quel budget»
           è coperta nella lettura ristretta dalla decisione dell'utente «Resta la categoria» (categoria intera più la
           scorciatoia «Spese del mese» alla tabella), non come selezione voce per voce.
           Residui della 2.4 rimasti fuori, dichiarati in APERTO di PIANO.md (non scoperti): BE1/BE2/BE3 e C1 della 2.4,
           «In arrivo» che sparisce al cambio mese, la resa per la 2.1-bis, i calcoli del cruscotto.
CONTRATTI: 6 su 6 convergenti (aperti sul codice dalla chiusura e dal bug-hunter finale)
           - 01 → 03 colonna: supabase/migrations/20261120000000_budget_periodo.sql:16 `description text not null default ''`
             ↔ Models/CategoryBudget.cs:20 `[Column("description")] public string Description { get; set; } = "";`
           - 01 → 03 privilegi: grant insert/update (description) della migrazione ↔ Services/BudgetRepository.cs:34
             (CreaAsync) e :47 (SalvaAsync `.Set(b => b.Description, …)`); ends_on già scrivibile dalla 2.4 (ChiudiAsync :55)
           - 01 → 03 limite: check `char_length(description) <= 200` ↔ Services/CalcoliBudget.cs:64 `DescrizioneMassima = 200`
             e Pages/BudgetEdit.razor:106 `maxlength` (conta unità UTF-16, più stretto del check: nessun rischio)
           - 01 → 03/04 rifiuto: 23P01 come HTTP 400 (provato dalla 01 via PostgREST) ↔ Services/CalcoliBudget.cs:87
             RifiutoPerSovrapposizione e Pages/BudgetEdit.razor:275 `catch (PostgrestException ex) when (…)`; Postgrest
             4.4.0 espone `int StatusCode` e `string? Content`. Completato dalla chiusura con 409/23505 (COMBINATO)
           - 01 → 03 periodo: vincolo `daterange(starts_on, ends_on + 1 mese, '[)')` ↔ CalcoliBudget InizioDiUnNuovo,
             FineDaMesi, SiPuoChiudere: senza concorrenza l'editor non propone un periodo che il vincolo rifiuta; la
             chiusura accorcia soltanto
           - 02 ↔ 03 Pages/BudgetEdit.razor (frase sulle ricorrenti :83-86 della 02, editor della 03, log della 04):
             stato del modulo coerente dopo salva, ricarica, conflitto di versione e rifiuto; Pages/Spese.razor ↔
             Services/DopoSegna.cs:12-13 TornaAiBudget, calcolato prima del ramo «a rate»
COMBINATO: 1 rilievo del bug-hunter finale — 1 corretto · 0 rimandati al capo
           BH1 (media, contratto) Pages/BudgetEdit.razor:275 il messaggio «C'è già un budget per questa categoria in quei
           mesi…» copriva solo 400/23P01; due budget «ogni mese» della stessa categoria creati da due schede o due membri
           con l'elenco non riletto violano l'indice univoco category_budgets_aperto_uq (23505, HTTP 409) e finivano nel
           catch generico (stderr, barra «errore imprevisto», «riprova fra un momento»). Correzione:
           Services/CalcoliBudget.cs:90 e :97 riconoscono anche 409/23505, test nuovo
           Il_rifiuto_23505_del_budget_aperto_si_riconosce_col_409; verificato: risolto — Services/CalcoliBudget.cs:90, :97
           Non verificato dal vivo: quale dei due errori Postgres dia per primo quando una riga viola entrambi, e il 409
           di PostgREST per il 23505 (comportamento standard noto, a memoria). Se fosse diverso si ricade nel messaggio
           generico di prima, senza danni.
           review: chiusura — correzione BH1
             bug-hunter      RILIEVI: 1   (diff combinato 30f49ce..6eab0c2, 13 files changed, 493 insertions(+), 38 deletions(-))
             checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
             bug-hunter      RILIEVI: 0   (correzione: 2 files changed, 13 insertions(+), 3 deletions(-))
             threat-hunter   RILIEVI: 0
             conformity      non lanciato — 2 files changed, 13 insertions(+), 3 deletions(-)
             backend-expert  non lanciato — 2 files changed, 13 insertions(+), 3 deletions(-)
                             · 0 create mode · 0 dichiarazioni/endpoint
             test rosso      Il_rifiuto_23505_del_budget_aperto_si_riconosce_col_409: Eton.Tests/CalcoliBudgetTests.cs:134 Assert.True() Failure Expected: True Actual: False
             checker         VERDETTI: risolti 1 · non risolti 0 · non verificabili 0
                             ROSSO: confermati 1 · non confermati 0 · non verificabili 0
             coverage        COPERTURA: 6 clausole — coperte 5 · scoperte 0 · rinviate 1
             live-testing    non lanciato — la correzione non tocca l'interfaccia (funzione pura e test)
           commit f2ee616 su main
GATE: mcp__synapse__build root=G:\Sviluppo\Eton → build: succeeded — 0 errors, 0 warnings — 10.2s (prima della correzione, 6eab0c2)
GATE: mcp__synapse__test root=G:\Sviluppo\Eton → test: 0 failed, 452 passed, 0 skipped — 4.2s (sullo stato finale, compilato dallo stesso comando)
FUORI SCOPE: (rilievi fondati che nessuna unità ha risolto)
           - Dalla 04, tutto il progetto: 106 `Console.Error.WriteLine` in 29 file; in Blazor WASM ogni scrittura su stderr
             accende la barra «Si è verificato un errore imprevisto», anche sugli errori gestiti che hanno già il loro
             messaggio. Unità a sé.
           - Dalla 01: BE2 (bassa) prove dei due blocchi mescolate nello script di collaudo; BE3 (progetto) script
             supabase/verifica-*.sql che diano l'esito da soli; nota di threat-hunter, preesistente e non provata: la
             policy category_budgets_update non chiede is_space_member (un ex membro autore potrebbe ancora modificare).
           - Dalla 03: BE2 (bassa) l'allineamento del modulo ripetuto in tre punti di BudgetEdit; con «per 1 mese» la
             frase «quello che resta non si riporta» stona; `Inizio >CalcoliBudget` senza spazio a Pages/BudgetEdit.razor:68.
           - Dalla 04, ui-critic: con una descrizione corta, a 1280 px il testo sta in alto nella riga e le etichette dei
             pulsanti al centro. Per la 2.1-bis.
           - Dalla 02, osservazione: arrivando da un budget e scegliendo a mano un'altra categoria, dopo «Segna» si torna
             comunque ai budget.
           - Dalla chiusura, EXTRA del checker: l'editor legge i budget dello spazio una volta sola all'apertura
             (Pages/BudgetEdit.razor:221-224); è la causa di fondo di entrambi i rifiuti, e il messaggio rimanda già alla
             rilettura. Nessuna azione proposta.
           - NON VERIFICABILE (bug-hunter finale): la descrizione di 200 caratteri senza spazi a 320 px (la 04 l'ha
             misurata a 360, 375 e 1280 px).
SFUGGITO: bug-hunter 03-budget-periodo: dopo il rifiuto per sovrapposizione si accendeva la barra «errore imprevisto» (log su stderr), vista dalla prova del capo; corretta dalla 04
          bug-hunter 03-budget-periodo: il rifiuto 409/23505 di due budget aperti in contemporanea non riconosciuto, visto dal bug-hunter finale; corretto dalla chiusura
ARCHIVIATO: storico/handoff/01-migrazione-periodo, 02-residui, 03-budget-periodo, 04-correzioni-periodo (git mv, commit di questa chiusura)
BUDGET: 01-migrazione-periodo 3 → non misurato · 02-residui 3 → non misurato · 03-budget-periodo 5 → non misurato · 04-correzioni-periodo 2 → non misurato
        (M tok_eq; atteso dal mandato. `costo_unita_Mtok_eq` non ha voci per queste unità: erano side session, e
        `limiti` dice «side Esecutore non attribuite (manca mandato.md o il capo nei file dell'app): 12»)
MISURE: {"ts":"2026-11-20T22:23:23.1244217+01:00","progetto":"G:\\Sviluppo\\Eton","da":"","sessioni":31,"chiusure":2,"unita_fatto":19,"combinato":{"rilievi":3,"corretti":2},"fuga_interna":0.158,"fuga_interna_ciclo":0.25,"costo_unita_Mtok_eq":{"01-migrazione":2.16,"02-budget":3.9,"02-gruppi-quota":3.41,"03-migrazione":2.93,"03-residui":2.61,"04-correzioni-budget":1.43,"04-rate":7.44,"05-entrate":9.66,"06-ritocchi":1.25,"07-testi":1.76},"fuga_esterna":0,"verdetti":{"fondati":53,"infondati":1},"fondati_per_agente":{"backend-expert":{"fondati":24,"infondati":3,"fuori_scope":12},"bug-hunter":{"fondati":18,"infondati":0,"fuori_scope":2},"conformity":{"fondati":27,"infondati":1,"fuori_scope":1},"ui-critic":{"fondati":4,"infondati":0,"fuori_scope":5}},"resa":{"backend-expert":{"lanci":15,"rilievi":41,"tok_eq":1723031,"rilievi_per_100k":2.38},"bug-hunter":{"lanci":38,"rilievi":32,"tok_eq":11539817,"rilievi_per_100k":0.277},"checker":{"lanci":51,"tok_eq":5587829},"conformity":{"lanci":16,"rilievi":26,"tok_eq":1923828,"rilievi_per_100k":1.351},"coverage":{"lanci":7,"tok_eq":631465},"doc-checker":{"lanci":4,"tok_eq":269657},"general-purpose":{"lanci":1,"tok_eq":248052},"implementer":{"lanci":15,"tok_eq":1034626},"live-testing":{"lanci":23,"tok_eq":14606449},"scout":{"lanci":3,"tok_eq":571750},"tech-advisor":{"lanci":17,"tok_eq":2733607},"threat-hunter":{"lanci":37,"rilievi":11,"tok_eq":3879220,"rilievi_per_100k":0.284},"ui-critic":{"lanci":8,"rilievi":24,"tok_eq":4526389,"rilievi_per_100k":0.53}},"per_modello":{"claude-fable-5-2":{"lanci":9,"tok_eq":2554742,"usd_pesati":15.6967},"claude-sonnet-5-5":{"lanci":60,"tok_eq":9773896,"usd_pesati":19.5478},"sconosciuto":{"lanci":166,"tok_eq":36947081,"usd_pesati":null}},"sessione_principale":{"tok_eq":55515775,"usd_pesati":143.6688,"modelli":["claude-opus-5-5"]},"listino":"guida claude-api di Claude Code, tabella del 2026-10-06; USD per Mtok; cache write = 1,25 x input; cache read stimato (0,1 x input) per claude-opus-5, claude-sonnet-5, claude-haiku-5-5, claude-haiku-4-5, claude-fable-5; claude-fable-5-2 dalla tabella del 2026-11-03, letta il 2026-11-18","lanci_codex":42,"limiti":["sessione_principale: dai transcript di sessione, non sommata ai subagent; gli output_tokens dei transcript principali sono sottostimati","albero: side Esecutore non attribuite (manca mandato.md o il capo nei file dell\u0027app): 12","albero: unità dal registro: 53, dal transcript: 98","lanci_codex: segmenti Bash/PowerShell con codex exec o e secondo la regola di questo file (tolta da gate-protocollo.ps1 il 2026-11-12), read-only compresi, help escluso, deduplicati per tool_use; non vede Codex lanciato da uno script o da un\u0027altra sessione, ne dentro virgolette","fondati_per_agente: 27 righe contate in forma tollerata (id senza lettere o con lettera in coda, nota fra parentesi, verdetto in maiuscolo)","fondati_per_agente: 35 righe \u0027\u003cagente\u003e \u003cid\u003e:\u0027 di ADJUDICA non riconosciute (id fuori forma, nota fra parentesi dopo l\u0027id o verdetto fuori grammatica)"]}
COMPLETATO f2ee616
