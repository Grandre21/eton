handoff/CHIUSURA.md — lo scrive la sessione di chiusura
CHIUSURA: 4 unità — FATTO 4 · PARZIALE 0 · BLOCKED 0
           (02-budget: il suo resoconto dice ESITO: PARZIALE, per la prova nel browser che lì non era partita; la prova
           l'ha completata il capo il 12 nov, in storico/handoff/02-budget/prova-capo.md, e PARTIZIONE la porta FATTO.
           Divergenza spiegata, ma il resoconto da solo dice ancora PARZIALE)
COPERTURA: 7 clausole — coperte 6 · scoperte 0 · rinviate 1
           rinviata, in parte: «poi crei tu un piano per chiudere tutto cio che ce di aperto». Il piano c'è e i residui
           della spec §5 sono fatti (03, e il tetto alle rate nella 01). Restano aperti, dichiarati in APERTO di
           PIANO.md: la guardia d'uscita di Pages/RicorrenteEdit.razor:388-389, la granularità sotto la categoria,
           l'aggiornamento della PWA, i rilievi TIPO progetto BE1/BE2/BE3 e la resa per la 2.1-bis.
           Dubbio di coverage: la settima clausola è il divieto implicito «la grafica è il goal successivo»; senza di
           lei il conteggio è 6 clausole, 5 coperte, 1 rinviata.
           Divergenze PARTIZIONE ↔ resoconti (coverage): 02 FATTO contro PARZIALE (v. sopra); 04 «FATTO (codice)» con
           «la prova nel browser resta del capo»: testo superato, la prova è del 12 nov; 01 «non applicata»: superato
           dalla riga GATE migrazione SUPERATO del 25 ott.
CONTRATTI: 10 su 10 convergenti (checker, VERDETTI: convergenti 10 · divergenti 0 · non verificabili 0) —
           C1 colonne di category_budgets ↔ Models/CategoryBudget.cs · C2 grant insert/update ↔ ignoreOnInsert/
           ignoreOnUpdate e .Set di BudgetRepository · C3 check (primo del mese, ends_on >= starts_on, amount > 0,
           categoria 1-40) ↔ CalcoliBudget e BudgetEdit · C4 indice degli aperti + InizioDiUnNuovo contro la
           sovrapposizione · C5 policy autore-o-proprietario ↔ Permessi.PuoIntervenire · C6 trigger version ↔
           filtro e quattro esiti di BudgetRepository · C7 firme della 03 (ElencaConPrevisteAsync, CreaAsync, PerMese)
           ↔ chiamate della 02 · C8 Cambiata ↔ PaginaEditor.cs:48 e classi CSS di Budget.razor ↔ app.css ·
           C9 \set claims_* di verifica-rls-budget.sql (0 letterali) · C10 installments <= 60 ↔ select 2..60 di
           Spese.razor:127 e RicorrenteEdit.razor:100
           Note del checker, non divergenze: C4 la non-sovrapposizione con un budget chiuso vive solo nel client (una
           corsa fra due client la lascia passare, nessun vincolo nel database); C6 BudgetRepository.Esito è privato,
           senza il delegate che rende testabile quello delle ricorrenti.
COMBINATO: 2 rilievi del bug-hunter finale — 2 corretti · 0 rimandati al capo
           BH1 (bassa) Pages/Budget.razor:45 diceva che un affitto consuma il budget «dal primo giorno», ma conta solo
           dal giorno in cui scade → «dal giorno in cui scade»; verificato: risolto — Pages/Budget.razor:45
           BH2 (bassa) Pages/Spese.razor:459 un'entrata segnata arrivando da «Registra una spesa» riportava ai budget →
           `if (daBudget && nuovoTipo == TipoMovimento.Spesa)`; verificato: risolto — Pages/Spese.razor:460
           checker sui rilievi: VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
           checker sulla correzione: VERDETTI: risolti 2 · non risolti 0 · non verificabili 0
           threat-hunter finale sul diff combinato: RILIEVI: 0
           review della correzione (2 files changed, 3 insertions(+), 2 deletions(-) · 0 dichiarazioni/endpoint):
             bug-hunter RILIEVI: 0 · threat-hunter RILIEVI: 0 · conformity e backend-expert non lanciati (< ~30 righe)
           commit e59b04f sul ramo worktree-chiusura-2.4
GATE: mcp__synapse__build root=G:\Sviluppo\Eton\.claude\worktrees\chiusura-2.4 → build: succeeded — 0 errors, 0 warnings (sullo stato finale, e59b04f)
GATE: mcp__synapse__test root=G:\Sviluppo\Eton\.claude\worktrees\chiusura-2.4 → test: 0 failed, 442 passed, 0 skipped (sullo stato finale, e59b04f)
FUORI SCOPE: (rilievi fondati che nessuna unità ha risolto; quelli di progetto erano già nelle ratifiche del 12 nov o in APERTO)
           - Nuovo, dal checker della chiusura, comportamento da decidere: arrivando da «Registra una spesa» e scegliendo
             «a rate», dopo «Segna» si resta nel Registro (il ritorno ai budget c'è solo per la spesa singola). La spec
             §3 non dice nulla sul piano a rate. Raccomandazione: tenerlo, l'avviso del piano si legge solo lì.
           - Nuovo, dal bug-hunter della correzione (invariato rispetto a prima): dopo Entrata e poi di nuovo Spesa,
             ScegliTipo riporta la categoria alla prima dell'elenco e non a quella del budget, e la spesa successiva
             torna lo stesso ai budget.
           - Nuovo, dalla convergenza (C4): nessun vincolo di esclusione nel database contro due budget validi nello
             stesso mese; lo evita il client con InizioDiUnNuovo. Una corsa fra due client lo lascia passare. Si chiude
             con btree_gist: estensione e schema, quindi decisione dell'utente.
           - Nessun test automatico per il ritorno ai budget dopo «Segna» (Pages/Spese.razor:460): provato nel browser il
             12 nov sulla versione di prima della correzione.
           - Dalle unità, già all'utente: BE1 della 03 (un Esito<T> unico per sette repository), BE2/BE3 della 02
             (switch a quattro esiti e aritmetica del mese ripetuti), BE3 della 03 (TipiAmmessi in CategorieSpesa), BE3
             della 01 (collaudo dei privilegi una riga per controllo), C1 della 02 (commento di Shared/PaginaRegistro.cs),
             «In arrivo» che sparisce al cambio mese, guardia di Pages/RicorrenteEdit.razor:388-389, e per la 2.1-bis
             «sforato» senza colore d'allarme, la y fissa di «Salva», cifra e link di «Da spendere nei budget» in
             grigio, residuo non distinto dal totale, binario della barra a 1,21:1.
           - NON VERIFICABILE (bug-hunter finale), per una prova a schermo: `.riepilogo-mese > :last-child
             { margin-bottom: 0 }` vale anche per il riepilogo del Registro; lo stacco di `label.campo + .testo-tenue`
             in ItemEdit e RecensioniElemento. La chiusura non tocca l'interfaccia in modo visibile (una frase e una
             condizione): niente prova nel browser qui, la vedrà l'utente o la 2.1-bis.
SFUGGITO: bug-hunter, conformity 02-budget: la barra del budget invisibile (colonna della griglia da 0 px), vista dalla prova nel browser del 25 ott dopo la fusione; corretta dalla 04
          bug-hunter 02-budget: la guardia d'uscita di «Nuovo budget» scattava con la sola categoria scelta (finestra nativa che bloccava la scheda), vista dalla stessa prova; corretta dalla 04
ARCHIVIATO: storico/handoff/01-migrazione, 02-budget, 03-residui, 04-correzioni-budget (git mv, commit di questa chiusura)
BUDGET: 01 4 → 2.16 · 02 10 → 3.9 · 03 6 → 2.61 · 04 4 → 1.43 (M tok_eq; atteso dal mandato, reale da `costo_unita_Mtok_eq`)
MISURE: {"ts":"2026-11-14T15:09:28.2112744+01:00","progetto":"G:\\Sviluppo\\Eton","da":"","sessioni":25,"chiusure":1,"unita_fatto":15,"combinato":{"rilievi":2,"corretti":1},"fuga_interna":0.133,"fuga_interna_ciclo":null,"costo_unita_Mtok_eq":{"01-migrazione":2.16,"02-budget":3.9,"02-gruppi-quota":3.41,"03-migrazione":2.93,"03-residui":2.61,"04-correzioni-budget":1.43,"04-rate":7.44,"05-entrate":9.66,"06-ritocchi":1.25,"07-testi":1.76},"fuga_esterna":0,"verdetti":{"fondati":50,"infondati":1},"fondati_per_agente":{"backend-expert":{"fondati":19,"infondati":2,"fuori_scope":10},"bug-hunter":{"fondati":17,"infondati":0,"fuori_scope":2},"conformity":{"fondati":26,"infondati":1,"fuori_scope":1},"ui-critic":{"fondati":4,"infondati":0,"fuori_scope":5}},"resa":{"backend-expert":{"lanci":12,"rilievi":33,"tok_eq":1300269,"rilievi_per_100k":2.538},"bug-hunter":{"lanci":31,"rilievi":30,"tok_eq":10550996,"rilievi_per_100k":0.284},"checker":{"lanci":45,"tok_eq":5242122},"conformity":{"lanci":13,"rilievi":25,"tok_eq":1692870,"rilievi_per_100k":1.477},"coverage":{"lanci":6,"tok_eq":532996},"doc-checker":{"lanci":4,"tok_eq":269657},"general-purpose":{"lanci":1,"tok_eq":248052},"implementer":{"lanci":8,"tok_eq":336613},"live-testing":{"lanci":18,"tok_eq":12083558},"scout":{"lanci":3,"tok_eq":571750},"tech-advisor":{"lanci":16,"tok_eq":2436248},"threat-hunter":{"lanci":31,"rilievi":11,"tok_eq":3636311,"rilievi_per_100k":0.303},"ui-critic":{"lanci":7,"rilievi":24,"tok_eq":4349872,"rilievi_per_100k":0.552}},"per_modello":{"claude-fable-5-2":{"lanci":4,"tok_eq":1658106,"usd_pesati":null},"claude-sonnet-5-5":{"lanci":25,"tok_eq":4646126,"usd_pesati":9.2923},"sconosciuto":{"lanci":166,"tok_eq":36947081,"usd_pesati":null}},"sessione_principale":{"tok_eq":45257428,"usd_pesati":117.9859,"modelli":["claude-opus-5-5"]},"listino":"guida claude-api di Claude Code, tabella del 2026-10-06; USD per Mtok; cache write = 1,25 x input; cache read stimato (0,1 x input) per claude-opus-5, claude-sonnet-5, claude-haiku-5-5, claude-haiku-4-5, claude-fable-5","lanci_codex":42,"limiti":["risalito dal worktree G:\\Sviluppo\\Eton\\.claude\\worktrees\\chiusura-2.4 al repo G:\\Sviluppo\\Eton","per_modello: id fuori listino, usd_pesati null: claude-fable-5-2","sessione_principale: dai transcript di sessione, non sommata ai subagent; gli output_tokens dei transcript principali sono sottostimati","albero: unità dal registro: 42, dal transcript: 98","lanci_codex: segmenti Bash/PowerShell con codex exec o e secondo la regola di questo file (tolta da gate-protocollo.ps1 il 2026-11-12), read-only compresi, help escluso, deduplicati per tool_use; non vede Codex lanciato da uno script o da un\u0027altra sessione, ne dentro virgolette","fondati_per_agente: 27 righe contate in forma tollerata (id senza lettere o con lettera in coda, nota fra parentesi, verdetto in maiuscolo)","fondati_per_agente: 35 righe \u0027\u003cagente\u003e \u003cid\u003e:\u0027 di ADJUDICA non riconosciute (id fuori forma, nota fra parentesi dopo l\u0027id o verdetto fuori grammatica)"]}
