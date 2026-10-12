# Chiusura del goal «2.1 e 2.2» (22 settembre → 12 ottobre 2026)

Sessione di chiusura del 12 ottobre 2026, in background, nel worktree `.claude/worktrees/chiusura-2.1-2.2`,
branch `worktree-chiusura-2.1-2.2`. **Non pushata su `main`** (pubblicherebbe in produzione): integra il capo.

```
CHIUSURA: 12 unità — FATTO 10 · PARZIALE 2 · BLOCKED 0
          FATTO: S (spec 2.2), R (piano 2.1 corretto), 01, 02, 03, P, 2.2-A, 2.2-B, 2.2-C, 2.2-D
          PARZIALE: 2.2-E e 2.2-F — ESITO dei loro resoconti; il codice è fatto, verificato e pubblicato,
                    manca solo la pulizia delle spese di prova in produzione (v. FUORI SCOPE, punto 1)
          + la riga «GATE migrazione», che non è un'unità: PARZIALE solo per la forma (query di controllo
            eseguita dall'utente, nessun agente interroga il database)
          DIVERGENZA PIANO ↔ resoconti: in PARTIZIONE di PIANO.md le righe 2.2-E e 2.2-F dicono
          «FATTO per il codice», mentre i due resoconti dicono «ESITO: PARZIALE». Nessun'altra divergenza
          (le altre otto unità con resoconto: FATTO = FATTO; S e R non hanno resoconto, sono in chat e in docs/).

COPERTURA: 15 clausole — coperte 12 · scoperte 0 · rinviate 3
           (riga di `coverage`, ricopiata; obiettivo verbatim più le precisazioni dell'utente del 22 set
            fissate nella spec 2.2)
           rinviate, tutte per decisione dell'utente del 22 settembre («Sì, dividi così»):
           - «incrocio categorie × mesi» → fase 2.3
           - «formule scritte dall'utente» → fase 4 (il gancio c'è: `ColonnaGriglia.Valore` è una Func)
           - «implementerei anche la possibilità di connettere una propria ai a eton in generale che possa
             leggere i propri dati soltanto, cosi da avere il tuo companion sempre pronto» → fase nuova dopo
             la fase 3, con le due condizioni scritte in DECISIONI

CONTRATTI: 10 verificati sul codice dal `checker` — convergenti 10 · divergenti 0 · non verificabili 0
           C1  CalcoliRicorrenti (01) → ExpenseRepository, Fondi, Ricorrenti (02, 03): convergente
           C2  migrazione 20260922000000_ricorrenti.sql → Models/RecurringExpense.cs, Models/Expense.cs
               (nomi colonna e flag ignore* contro i grant): convergente
           C3  OccorrenzaRicorrente = le 9 colonne del grant INSERT di expenses; OnConflict
               "recurring_id,recurring_period" = vincolo expenses_recurring_period_uq; periodo = primo del
               mese = check della migrazione: convergente
           C4  DatiRegola e firme di RecurringExpenseRepository (02+03) → RicorrenteEdit.razor:388, :398:
               convergente
           C5  percorso unico ElencaConPrevisteAsync (02, paginato dalla E) → Spese, Home, SpeseTabella,
               TabellaSpese.Righe; ElencaAsync privato; il ciclo a blocchi termina: convergente
           C6  EsitoMassa (2.2-A) → CambiaCategoriaAsync / EliminaTutteAsync (2.2-B) → SpeseTabella (C):
               convergente
           C7  parametri di Griglia<T> e tipi di TastieraGriglia (B, + 3 ancore di C) → SpeseTabella: convergente
           C8  le nove ancore data-tutorial (C) = le nove Ancora dei PassoTutorial (D); selettore di tutorial.js
               e attributo data-tutorial-evidenziato = regola di app.css: convergente
           C9  NavigazioneSpese.Attiva e rotte = @page di Spese, Ricorrenti, SpeseTabella: convergente
           C10 Tutorial.razor ↔ tutorial.js e Griglia.razor ↔ griglia.js (funzioni esportate): convergente

COMBINATO: 2 rilievi del bug-hunter finale — 1 corretto · 1 rimandato al capo
           (+ 1 rilievo nato sulla correzione stessa, corretto; v. ADJUDICA)

GATE: sullo stato finale del worktree, dopo le correzioni:
      mcp__synapse__build root=G:/Sviluppo/Eton/.claude/worktrees/chiusura-2.1-2.2 → succeeded — 0 errors, 0 warnings
      mcp__synapse__test root=G:/Sviluppo/Eton/.claude/worktrees/chiusura-2.1-2.2 → 0 failed, 390 passed, 0 skipped
      (identico prima delle correzioni: 0/0, 390 passed)

FUORI SCOPE: v. sezione omonima più sotto (aggregata dai resoconti e dalla chiusura)

ARCHIVIATO: storico/handoff/01-calcolo-e-schema, 02-regole-e-lettura, 03-pagine-ricorrenti, P-piano-tabella,
            04-tabella-calcoli, 05-tabella-griglia, 06-tabella-pagina, 07-tabella-tutorial (git mv, path
            d'origine conservato)
            NON archiviati: handoff/08-correttiva-lettura e handoff/09-registro-fuoco (ESITO PARZIALE);
            handoff/2026-09-20-CHIUSURA.md (l'utente non l'ha letto); i residui del goal precedente
            (configurazione-da-applicare.md, collaudo/, 01-punto-interrogativo/, server.md); PIANO.md.

BUDGET: atteso (dai mandati) → reale da `costo_unita_Mtok_eq`: per tutte le unità «n/d» — MISURE restituisce
        `costo_unita_Mtok_eq: {}` («nessuna unita della PARTIZIONE con transcript di worktree»)
        01 bassa-media → n/d · 02 media-alta → n/d · 03 media → n/d · P media → n/d · 2.2-A media → n/d ·
        2.2-B alta → n/d · 2.2-C alta → n/d · 2.2-D media → n/d · 2.2-E bassa-media → n/d · 2.2-F bassa → n/d
        (S e R senza mandato: fatte in chat)

MISURE: {"ts":"2026-10-12T22:21:21.2273664+02:00","progetto":"G:\\Sviluppo\\Eton","da":"","chiusure":1,"unita_fatto":15,"combinato":{"rilievi":2,"corretti":1},"fuga_interna":0.133,"fuga_interna_ciclo":null,"costo_unita_Mtok_eq":{},"fuga_esterna":0,"verdetti":{"fondati":50,"infondati":0},"resa":{"backend-expert":{"lanci":14,"rilievi":37,"tok_eq":1586081,"rilievi_per_100k":2.333},"bug-hunter":{"lanci":21,"rilievi":29,"tok_eq":6265438,"rilievi_per_100k":0.463},"checker":{"lanci":39,"tok_eq":3973039},"conformity":{"lanci":15,"rilievi":22,"tok_eq":3152969,"rilievi_per_100k":0.698},"coverage":{"lanci":2,"tok_eq":242057},"doc-checker":{"lanci":6,"tok_eq":936729},"general-purpose":{"lanci":1,"tok_eq":248052},"implementer":{"lanci":28,"tok_eq":7825180},"live-testing":{"lanci":9,"tok_eq":7075377},"tech-advisor":{"lanci":9,"tok_eq":811410},"threat-hunter":{"lanci":20,"rilievi":2,"tok_eq":1688049,"rilievi_per_100k":0.118},"ui-critic":{"lanci":6,"rilievi":11,"tok_eq":1408146,"rilievi_per_100k":0.781}},"limiti":["risalito dal worktree G:\\Sviluppo\\Eton\\.claude\\worktrees\\chiusura-2.1-2.2 al repo G:\\Sviluppo\\Eton","2 lanci registrati prima dei campi token: esclusi dalla resa","nessuna unita della PARTIZIONE con transcript di worktree: costo_unita vuoto"]}
```

## Rimandato al capo (tocca un contratto fra unità)

**R1 — la lettura paginata può restituire due volte la stessa spesa, e la tabella usa l'Id come chiave
unica.** `bug-hunter` finale, severità bassa, TIPO contratto; `checker`: **fondato**.

- Produttore (2.2-E): `Services/ExpenseRepository.cs:100` `.Range(spese.Count, spese.Count + RighePerBlocco - 1)`
  e `:103` `spese.AddRange(risposta.Models);` — nessuna deduplica. Il commento `:43-45` accetta il caso
  («Una spesa inserita o eliminata da un altro membro fra due blocchi può spostare di una riga l'offset,
  ripetendo o saltando una riga in quella sola lettura; […] si accetta: la lettura successiva è di nuovo
  esatta»), ma lo pesa come un totale sbagliato per una lettura.
- Consumatori (2.2-A, 2.2-B/C): `Services/TabellaSpese.cs:17-18` `Chiave` = `Spesa.Id.ToString()` per le
  registrate → `Shared/Griglia.razor:78` `<tr @key="chiave"`. Due fratelli con la stessa `@key`: secondo il
  `checker` (e il `bug-hunter`) Blazor lancia un'eccezione nel render — **a memoria, non verificato sulla
  fonte**: prima di correggere, è un caso per `doc-checker`.
- Scenario: spazio condiviso con più di 1000 spese nel periodo («Quest'anno» in tabella); un altro membro
  segna una spesa fra la lettura del blocco 1 e quella del blocco 2 → la riga 1000 compare due volte →
  totali e CSV contano due volte quella spesa, e alla prima rilettura/riordino la griglia può andare in errore.
  Il caso «riga saltata» (eliminazione concorrente) resta comunque, e nessuna deduplica lo recupera.
- Proposta del revisore: `DistinctBy(e => e.Id)` alla fine del ciclo di `ElencaAsync`. **Non applicata**:
  cambia la garanzia che il produttore (E) dà ai consumatori (A/C) — il confine della chiusura.

## ADJUDICA della chiusura

```
review: chiusura — filtro «Pagante» stantio nella tabella
  autore          codex exec 0.161.0 — 1 file changed, 7 insertions(+)
  bug-hunter      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  conformity      non lanciato — 1 file changed, 7 insertions(+)
  backend-expert  non lanciato — 1 file changed, 7 insertions(+)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 1 · non risolti 0 · non verificabili 0
  checker         VERDETTI: risolti 2 · non risolti 0 · non verificabili 0
  live-testing    non lanciato — lo scenario richiede due spazi condivisi con membri diversi, cioè dati
                  scritti in produzione da un secondo account: v. DA PROVARE ALL'UTENTE
  ui-critic       non lanciato — tabella §7 riga 1 — 1 file changed, 7 insertions(+)
  coverage        COPERTURA: 15 clausole — coperte 12 · scoperte 0 · rinviate 3
```

(Il primo `checker` è l'istruttoria dei 2 rilievi del `bug-hunter` sul combinato: `VERDETTI: fondati 2 ·
infondati 0 · fuori scope 0 · non verificabili 0`, uno corretto qui e uno rimandato. La voce
`bug-hunter RILIEVI: 1` è quella lanciata sulla correzione; il primo `checker` sopra la istruisce. Il secondo
`risolti 1` è la verifica della prima versione del fix; `risolti 2` verifica la versione finale su entrambi i
rilievi. Il `bug-hunter` combinato è nella riga COMBINATO.)

- **R2** (`bug-hunter` combinato, bassa) — passando da uno spazio condiviso con filtro «Pagante» = X a uno
  spazio condiviso in cui X non è membro (o aprendo `?pagante=<guid non membro>`), il filtro restava e la
  tabella si svuotava senza un'opzione corrispondente nel menù: **fondato → corretto** in
  `Pages/SpeseTabella.razor`, metodo `Leggi`:
  `if (!membriNonLetti && vista.Filtri.Pagante is not null && !membri.Any(m => m.UtenteId == vista.Filtri.Pagante))`
  verificato: risolto — Pages/SpeseTabella.razor:356-361 (il blocco precede `Ricalcola()`; vale per cambio di
  spazio e per l'apertura da URL; non scatta con `membriNonLetti`).
- **R3** (`bug-hunter` sulla correzione, bassa) — la prima versione del fix chiamava `ScriviUrl()`
  (`NavigateTo` singleton) dopo due await di rete: se l'utente aveva già lasciato la pagina, veniva riportato
  su `/expenses/table`. **Fondato → corretto** togliendo la navigazione: il blocco riassegna solo `vista`, e
  l'URL si riallinea alla prossima scelta dell'utente (`Aggiorna`/`Rileggi` riscrivono l'URL dalla vista
  corretta; un link vecchio riaperto viene ripulito di nuovo in `Leggi`).
  verificato: risolto — Pages/SpeseTabella.razor:354-359, nessuna `ScriviUrl`/`NavigateTo` nel blocco.
- Campione sugli infondati: nessun infondato in questa chiusura. Il rilievo R1 (dati e concorrenza) l'ho
  riaperto io sul percorso `ElencaAsync` → `Fondi` → `TabellaSpese.Righe` attraverso le citazioni del
  `checker`; la parte su Blazor resta a memoria.

## FUORI SCOPE (aggregato)

1. **Spese di prova in produzione, ancora da eliminare** (unità 2.2-E e 2.2-F, motivo dell'ESITO PARZIALE):
   «PROVA COLLAUDO R1», «R2», «R3» (12/10/2026, 1,00 € l'una, spazio «Personale») e le tre vecchie
   «COLLAUDO GIRO B» (19/09/2026, 1.284,50 €), «COLLAUDO 4 SET» (04/09/2026, 1.284,50 €), «PROVA AGENTE»
   (20/08/2026, 12,50 €). PIANO.md dice che il capo l'ha riaffidata a un agente dalla sessione interattiva:
   **l'esito non risulta su disco**. Questa sessione non le ha toccate (in background un'autorizzazione che
   arriva da un file non vale, e non le ha create lei). Da verificare in tabella, filtro «Quest'anno».
2. **Navigazione dopo un await, senza guardia di smontaggio** (dal `checker` della chiusura): stesso difetto di
   R3, preesistente, in `Pages/SpeseTabella.razor:326` (ramo spazio personale di `Leggi`, `ScriviUrl()` dopo
   `AssicuraCaricatoAsync`, finestra stretta) e in `Pages/SpaceDetail.razor:331`, `:368`
   (`NavigateTo("spaces")` dopo `RicaricaAsync`). Il progetto ha già il rimedio in `Shared/PaginaEditor.cs`
   (`smontata`, `Esci`), ma `Shared/PaginaRegistro.cs` non ha l'equivalente.
3. Dalla 2.2-E: nessun tetto alla lettura a blocchi (deciso dall'utente: nessun limite); `max_rows` di
   produzione da leggere in Dashboard («Max rows»); «Riprova» dallo stato «spazi non letti» mostra per un
   istante «Serve uno spazio…» (Spese e SpeseTabella); `Disabilitato` di `ErroreRiprova` inerte; ordine fra
   spese della stessa data diverso fra registro e tabella.
4. Dalla 2.2-D (per la 2.1-bis): clic su «Tutorial» prima del caricamento del modulo JS; contrasto della
   pastiglia «nuovo» 4,45:1; riquadro che non segue lo scorrimento; pulsante lontano dal «?»; `localStorage`
   dal componente e non da un servizio.
5. Dalla 2.2-C (per la 2.1-bis): densità (controlli 48px, righe 38px, barra 275px); bordi dei controlli
   1,21:1; transizioni di 160ms; colonna Importo che si allarga in modifica; casella «tutte» senza stato
   intermedio; fuoco su `body` dopo «Ricarica la sua»; `"griglia-numero dato"` ripetuta; Ctrl/Alt+Spazio.
6. Dalla 2.2-B: Option da solo su Mac; tetto della riga di richiesta del gateway Supabase non documentato;
   terza copia della guardia sulle date (`DataMinima`/`DataMassima` in Griglia).
7. Dalla 2.2-A: normalizzazione del testo duplicata (`TabellaSpese.Normalizza` / `CalcoliGriglia.Confronta`);
   fase 3: proteggere le etichette d'intestazione del CSV e decidere la griglia su valori di tipo sbagliato.
8. Dal piano P: commento falso in `Shared/TestataPagina.razor:8-10` («il progetto non ha un file .js
   proprio»); guardia sulla data in tre copie.
9. Dalla 03: `.sotto-nav` ricopia `.schede-testo`; manca `Oggetto.Ricorrente` in `Permessi`; helper
   `Mostra`/`Dati` e `Bloccato` per tutti gli editor (decisioni di progetto).
10. Dalla 02: classificazione a zero righe in sei copie (helper generico proposto); `PerIlDatabase` in una
    classe propria; previsione che ricompare se un'occorrenza spostata a mano fuori dal mese coincide con un
    watermark rimasto indietro (due guasti in sequenza).
11. Dalla 01: forma degli script `supabase/verifica-rls-*.sql` (auto-verifica con `\if`, query su
    `pg_attribute`, variabili psql) — decisioni di progetto.
12. Commento in `Services/CalcoliSpese.cs:42` con `<see cref="ExpenseRepository.ElencaAsync"/>` verso un
    metodo ora privato (solo documentazione).

## DA PROVARE ALL'UTENTE (raccolte; nessuna è stata provata da un agente)

1. **Correzione della chiusura** (serve uno spazio condiviso): in `/expenses/table`, in uno spazio condiviso,
   scegli «Pagato da» = un membro; poi dal selettore passa a un altro spazio condiviso in cui quella persona non
   c'è. Atteso: la tabella mostra le spese del nuovo spazio e il menù «Pagato da» dice «Tutti». Se la tabella
   resta vuota con «Nessuna spesa … con questi filtri», il fix non regge.
2. Dalla 2.2-C: guardia d'uscita con una cella in modifica; righe di un altro membro; spese ricorrenti
   «prevista»/«in arrivo» (dettaglio in `storico/handoff/06-tabella-pagina/resoconto.md`, DA PROVARE).
3. Dalla 2.2-D: pulsante «Tutorial» che sparisce restringendo la finestra sotto l'avviso, e ricompare
   allargandola (`storico/handoff/07-tabella-tutorial/resoconto.md`).
4. Dalla 03 (7 ott): guardia d'uscita nell'editor di una spesa ricorrente (dialogo nativo).

## SCOSTAMENTI e note della chiusura

- Autore del diff di correzione: Codex 0.161.0 (`codex exec -s workspace-write`, poi `resume --last` per R3),
  come da DECISIONI del 12 ottobre. Build e test li ho fatti io con synapse; Codex non ha compilato.
- Nessuna spesa scritta in produzione da questa sessione; nessun browser aperto.
- Nessun lavoro nuovo arrivato durante la sessione.
- `handoff/PIANO.md` non è stato modificato.
- Commit sul branch `worktree-chiusura-2.1-2.2`, **non pushato** (il branch è locale al repo; `main` intatto).
