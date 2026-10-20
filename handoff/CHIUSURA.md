handoff/CHIUSURA.md — lo scrive la sessione di chiusura
CHIUSURA: 7 unità — FATTO 7 · PARZIALE 0 · BLOCKED 0
COPERTURA: 12 clausole — coperte 12 · scoperte 0 · rinviate 0
CONTRATTI: 10 su 10 convergenti (checker, VERDETTI: convergenti 10 · divergenti 0 · non verificabili 0) —
           C1 gruppi delle ricorrenti (02→04→05) · C2 quota da accantonare · C3 colonne, privilegi e trigger della
           migrazione contro modelli e scritture del client · C4 vincoli del piano a rate contro ImportoRata/DatiPiano/
           Occorrenza · C5 187 + « · rata 60/60» = 200 · C6 DatiRegola con Kind · C7 Occorrenza copia Kind e suffisso ·
           C8 «a rate» spento con Entrata · C9 overload Prossima · C10 doppioni della 01 ancora sul percorso di lettura
COMBINATO: 3 rilievi del bug-hunter finale — 3 corretti · 0 rimandati al capo
           (BH1 sul diff combinato, BH2 e BH3 trovati dai rilanci sulla correzione: stessa famiglia, chiusi da una
           correzione sola, commit 674c12e)
GATE: mcp__synapse__build root=G:\Sviluppo\Eton\.claude\worktrees\chiusura-2-3 → build: succeeded — 0 errors, 0 warnings
GATE: mcp__synapse__test root=G:\Sviluppo\Eton\.claude\worktrees\chiusura-2-3 → test: 0 failed, 417 passed, 0 skipped (sullo stato finale, 674c12e)
FUORI SCOPE: (rilievi fondati che nessuna unità ha risolto, tutti già portati all'utente nelle ratifiche del 20 ott)
           - 2.1-bis, grafica: i 5 rilievi di ui-critic della misura finale (righe delle ricorrenti a 390 px, totale del
             gruppo a 10,45 px, allineamento di «prossima:», colore delle entrate, otto dimensioni di `.dato`); UC3 e UC4
             della 04; UC2 (stile), UC3 «+» nella cella Importo, UC4 pastiglie della 05; filtro categorie della tabella
             che mischia le due liste.
           - BE1 della 02 (TIPO progetto): la chiave di ordinamento «senza accenti e maiuscole» in tre copie.
           - BE1 della 03 (TIPO progetto): i claims JWT ricopiati in tutti gli script `supabase/verifica-rls-*.sql`.
           - BE3 della 04: componente unico «Numero di rate» (rinviato alla 2.1-bis).
           - 05: `CambiaCategoriaAsync` non controlla il tipo, e il database non vincola la categoria al tipo.
           - Nuovo, dal threat-hunter finale (non è un rilievo, nessun danno a terzi): il database non ha un tetto al
             numero di rate (il client si ferma a 60). Chi scrive dall'API un piano con più di 60 rate e una
             descrizione di 200 caratteri rende non materializzabili le proprie rate. Si chiude con un check
             `installments <= 60` in una migrazione futura: tocca lo schema, quindi è dell'utente.
           - Vincolo operativo della 03, ancora valido: aggiornare la PWA su tutti i dispositivi prima di creare il
             primo piano a rate.
ARCHIVIATO: storico/handoff/01-doppioni, 02-gruppi-quota, 03-migrazione, 04-rate, 05-entrate, 06-ritocchi,
           07-testi, misura-finale; il rapporto del goal precedente handoff/CHIUSURA.md (12 ott) →
           storico/handoff/2026-10-12-CHIUSURA.md (su istruzione del capo nel prompt di apertura)
BUDGET: 01 3 → n.d. · 02 6 → n.d. · 03 4 → n.d. · 04 8 → n.d. · 05 12 → n.d. · 06 3 → n.d. · 07 3 → n.d. (M tok_eq;
        `costo_unita_Mtok_eq` è vuoto: «nessuna unita della PARTIZIONE con transcript di worktree»)
MISURE: {"ts":"2026-10-20T20:01:42.5754932+02:00","progetto":"G:\\Sviluppo\\Eton","da":"","sessioni":26,"chiusure":2,"unita_fatto":25,"combinato":{"rilievi":4,"corretti":2},"fuga_interna":0.16,"fuga_interna_ciclo":0.2,"costo_unita_Mtok_eq":{},"fuga_esterna":0,"verdetti":{"fondati":73,"infondati":1},"fondati_per_agente":{"backend-expert":{"fondati":5,"infondati":1,"fuori_scope":4},"bug-hunter":{"fondati":7,"infondati":0,"fuori_scope":0},"conformity":{"fondati":10,"infondati":1,"fuori_scope":0},"ui-critic":{"fondati":4,"infondati":0,"fuori_scope":5}},"resa":{"backend-expert":{"lanci":21,"rilievi":56,"tok_eq":2226392,"rilievi_per_100k":2.515},"bug-hunter":{"lanci":38,"rilievi":40,"tok_eq":11567074,"rilievi_per_100k":0.346},"checker":{"lanci":61,"tok_eq":6324729},"conformity":{"lanci":23,"rilievi":39,"tok_eq":3998120,"rilievi_per_100k":0.975},"coverage":{"lanci":3,"tok_eq":378886},"doc-checker":{"lanci":6,"tok_eq":936729},"general-purpose":{"lanci":1,"tok_eq":248052},"implementer":{"lanci":28,"tok_eq":7825180},"live-testing":{"lanci":18,"tok_eq":12333418},"scout":{"lanci":2,"tok_eq":291393},"tech-advisor":{"lanci":15,"tok_eq":1461371},"threat-hunter":{"lanci":36,"rilievi":2,"tok_eq":3446885,"rilievi_per_100k":0.058},"ui-critic":{"lanci":9,"rilievi":25,"tok_eq":4283358,"rilievi_per_100k":0.584}},"lanci_codex":36,"limiti":["risalito dal worktree G:\\Sviluppo\\Eton\\.claude\\worktrees\\chiusura-2-3 al repo G:\\Sviluppo\\Eton","2 lanci registrati prima dei campi token: esclusi dalla resa","nessuna unita della PARTIZIONE con transcript di worktree: costo_unita vuoto","lanci_codex: comandi Bash/PowerShell con codex exec in posizione di comando, deduplicati per tool_use; non vede Codex lanciato da uno script o da un\u0027altra sessione"]}
        (nota della chiusura: risalito al checkout principale, lo script ha letto il rapporto del 12 ottobre ancora lì;
        `combinato` 4/2 è quello del ciclo precedente, questo ciclo ha 3 rilievi e 3 corretti. Da rilanciare dopo il
        merge, quando questo `CHIUSURA.md` sarà su `main`.)

## Riconciliazione piano ↔ resoconti

La colonna di stato di `PIANO.md` e gli `ESITO` dei resoconti divergono in due righe, e qui si riconciliano:

- **02-gruppi-quota**: resoconto `PARZIALE` (manca `ui-critic`, negato dai permessi), piano `PARZIALE` «finché la
  chiusura non lo riconcilia». Il passo mancante l'ha fatto il coordinatore nella misura finale del 20 ott
  (`storico/handoff/misura-finale/esito.md`, `ui-critic RILIEVI: 5`, dati di prova eliminati), con i 5 rilievi
  passati alla 2.1-bis su ratifica dell'utente. → **FATTO**.
- **07-testi**: resoconto `PARZIALE` (prova nel browser rinviata al capo), piano `FATTO`. La prova c'è: il
  `live-testing` lanciato dal coordinatore nella sessione `bf92fb39-470a-4f03-879d-8a9d47914a17`, transcript
  `subagents/agent-a6bb2962ee38b3903.jsonl`, riga 259: «SCENARI: 5/5 … Salva/Crea/Elimina/Termina mai premuti».
  Non c'è un file d'esito su disco: la prova sta solo nel transcript. → **FATTO**.

Le righe `S — progetto 2.3` e `GATE migrazione` del piano non sono unità e restano `PARZIALE` per la sola forma
(un documento approvato in chat, una query lanciata dall'utente): non entrano nel conteggio.

## Tracciato della chiusura

    review: combinato — 19bfee4..70f2bae, 26 files changed, 1584 insertions(+), 233 deletions(-)
      bug-hunter      RILIEVI: 1
      threat-hunter   RILIEVI: 0
      checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
    review: correzione 1 — avvisoPiano in PrimaDiRicaricare
      autore          codex exec 0.161.0 — 1 file changed, 5 insertions(+), 1 deletion(-)
      bug-hunter      RILIEVI: 1
      threat-hunter   RILIEVI: 0
      checker         VERDETTI: risolti 1 · non risolti 0 · non verificabili 0
      checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
      coverage        COPERTURA: 12 clausole — coperte 12 · scoperte 0 · rinviate 0
    review: correzione 2 — avvisoPiano dopo Carica in Segna
      autore          codex exec 0.161.0 — 1 file changed, 8 insertions(+), 1 deletion(-)
      bug-hunter      RILIEVI: 1
      threat-hunter   RILIEVI: 0
      checker         VERDETTI: risolti 1 · non risolti 0 · non verificabili 0
    review: correzione 3 — un solo controllo in Leggi, le due toppe tolte
      autore          codex exec 0.161.0 — 1 file changed, 4 insertions(+)
      bug-hunter      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      conformity      non lanciato — 1 file changed, 4 insertions(+)
      backend-expert  non lanciato — 1 file changed, 4 insertions(+)
                      · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: risolti 3 · non risolti 0 · non verificabili 0
      live-testing    non lanciato — la prova chiede di creare un piano a rate in produzione, vietato dal mandato
                      («Nessun dato in produzione: niente prove nel browser che scrivono»)
      ui-critic       non lanciato — tabella §7 riga 1 — 1 file changed, 4 insertions(+) (solo dentro `@code`)

Note al tracciato:
- Le righe `autore` delle correzioni 1 e 2 sono `git diff HEAD --stat` di quel momento, prese dal diff mostrato
  (5+/1−, poi 8+/1− cumulato); la correzione 3 ha annullato le prime due, e la sua riga è il diff finale.
- `coverage` è stato lanciato dopo la correzione 1, non dopo l'ultima: le correzioni 2 e 3 chiudono lo stesso
  difetto, non aggiungono né tolgono clausole.
- Il terzo rilievo (BH3: cambio di spazio durante Riprova o cambio di mese) non ha avuto un'istruttoria a sé:
  l'ho riconosciuto fondato sul codice (stessa guardia `ScartaCambioSpazio`, già istruita per BH2), e il checker
  della correzione 3 l'ha verificato risolto insieme agli altri due.
- Codex: nessun file `-o` comparso, nessuna scrittura con PowerShell o interpreti inline nei tre log (checker).
  Brief e ritorni in `handoff/chiusura/`.

## Adjudica

- bug-hunter BH1 (combinato): fondato → corretto — l'avviso «Piano a rate creato» restava dopo un cambio di spazio,
  e «Vai alle ricorrenti» apriva quelle di un altro spazio — `avvisoPiano = $"Piano a rate creato: …";`
  verificato: risolto — Pages/Spese.razor:497 «if (attivo.Id != spazioDelPiano) avvisoPiano = null;»
- bug-hunter BH2 (correzione 1): fondato → corretto — lo stesso, con il cambio di spazio arrivato durante «Segna»
  e scartato da `ScartaCambioSpazio` — `protected override bool ScartaCambioSpazio() => rileggoMese;`
  verificato: risolto — Pages/Spese.razor:497 e :423 «spazioDelPiano = attivo.Id;»
- bug-hunter BH3 (correzione 2): fondato → corretto — lo stesso, durante Riprova e le frecce del mese —
  `rileggoMese = true;` poi `await Carica();` (Pages/Spese.razor:561, :584, :599)
  verificato: risolto — Pages/Spese.razor:497
- Scelta mia, **da ratificare**: cambiando mese nello stesso spazio l'avviso resta (il piano non è legato a un
  mese). Raccomandazione: tenerlo. Si annulla togliendo la condizione.
- threat-hunter combinato: 0 rilievi; la nota sul tetto di 60 rate è in FUORI SCOPE. Ho aperto io due vecchi file
  di `storico/` dove il threat-hunter vedeva «pattern tipo segreto»: nessun token, chiave o password (Grep su JWT,
  `sk-`, `service_role`, `password=`, `secret=`, `apikey=`).
- Infondati: nessuno, quindi nessun campione da riverificare.

## Cosa resta all'utente

- **Pubblicare la correzione**: è nel branch `worktree-chiusura-2-3`, non su `main`. `main` pubblica in produzione
  a ogni push; l'integrazione è del capo.
- **La prova nel browser della correzione non c'è**: per vederla serve creare un piano a rate in uno spazio e poi
  cambiare spazio, cioè scrivere in produzione. Va fatta dall'utente o autorizzata.
- La ratifica qui sopra (l'avviso resta cambiando mese).
- Il tetto di 60 rate nel database (FUORI SCOPE), se lo si vuole.
