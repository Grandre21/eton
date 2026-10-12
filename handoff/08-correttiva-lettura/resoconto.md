```
UNITÀ: 2.2-E — ESITO: PARZIALE
       Codice fatto e verificato (build 0/0, test 390/390, review adjudicata, collaudo A-D verde).
       Manca la pulizia in PRODUZIONE: restano 3 spese «PROVA COLLAUDO R1/R2/R3» (12/10/2026, 1,00 €
       ciascuna, spazio «Personale»). L'eliminazione multipla di live-testing è stata negata dal
       sistema dei permessi («Cloud Storage Mass Delete»); nessuno l'ha aggirata. Domanda per
       l'utente: le elimini tu dalla tabella (/expenses/table, «Questo mese», sono le sole tre righe),
       oppure autorizzi in chat un agente a farlo?

TOCCATI:
  Services/ExpenseRepository.cs      → +41/−10
  Services/CalcoliGriglia.cs         → +2/−1
  Pages/Spese.razor                  → +1/−1
  Eton.Tests/CalcoliGrigliaTests.cs  → +8/−0
  handoff/08-correttiva-lettura/server.md, resoconto.md → nuovi

REGISTRO: non riprodotto — artefatto della prova, non corretto (mandato punto 2).
  Misura (live-testing, codice intatto, 4 tentativi con tasti reali + 2 di confronto): il campo
  importo passa `disabled=true` a +18…+30 ms dal clic su «Segna» e torna `false` a +626…+939 ms.
  Ogni tasto reale (isTrusted=true, primo a +29 ms) arriva su BODY con il campo `disabled`, zero
  eventi beforeinput/input, valore invariato; lo stesso con l'azione `type` del plugin. L'azzeramento
  si riproduce SOLO scrivendo via script (`form_input`: evento `input` isTrusted=false a +26,5 ms con
  il campo `disabled`, valore accettato e poi azzerato dal salvataggio): è il modo dell'agente del
  7 ottobre. Il «Segna» successivo «non salva» perché l'importo è vuoto e il pulsante è disabilitato.
  Restano fuori misura i ~20-30 ms fra il clic e il `disabled`, irraggiungibili dal plugin e da un umano.

REVIEW:
  review: A — punti 1, 3, 4
    autore          codex exec 0.161.0 — 4 files changed, 52 insertions(+), 12 deletions(-)
    bug-hunter      RILIEVI: 2
    conformity      RILIEVI: 1
    threat-hunter   RILIEVI: 1
    backend-expert  non lanciato — 4 files changed, 40 insertions(+), 11 deletions(-)
                    · 0 create mode · 0 dichiarazioni/endpoint
    checker         VERDETTI: fondati 3 · infondati 0 · fuori scope 0 · non verificabili 0
    checker         VERDETTI: risolti 2 · non risolti 0 · non verificabili 0
    live-testing    ESITO: difetti (nessun difetto di prodotto: A-D verdi; la PULIZIA E non è stata
                    eseguita, quindi 3 spese di prova restano in PRODUZIONE)
    ui-critic       non lanciato — tabella §7 riga 2 — 4 files changed, 40 insertions(+), 11 deletions(-)
  (La riga di `backend-expert` e `ui-critic` è la --stat misurata a revisori lanciati, prima dei fix;
   la riga `autore` è quella finale. Riproduzione del punto 2: live-testing a parte, sul codice
   intatto, ESITO: difetti — vedi REGISTRO.)

CONTRATTI:
  Firme pubbliche invariate. La paginazione è interna al metodo privato:
  Services/ExpenseRepository.cs:113
    public async Task<SpeseDelPeriodo> ElencaConPrevisteAsync(Guid spazioId, DateTime da, DateTime a)
  Services/ExpenseRepository.cs:87
    private async Task<IReadOnlyList<Expense>> ElencaAsync(Guid spazioId, DateTime da, DateTime a)
  Services/ExpenseRepository.cs:32
    private const int RighePerBlocco = 1000;

ADJUDICA:
  R1 bug-hunter (alta) — fondato → corretto. Get() manda sempre `Prefer: count=estimated`; sopra
     max_rows PostgREST usa la stima del planner e risponde 416 se l'offset la supera: un blocco
     reale poteva far fallire la lettura. Fix: conteggio esatto.
       .Get(default, Constants.CountType.Exact);
     verificato: risolto — Services/ExpenseRepository.cs:100-101
       `.Range(spese.Count, spese.Count + RighePerBlocco - 1)` / `.Get(default, Constants.CountType.Exact);`
  R3 conformity (bassa) — fondato → corretto. Riga `///` di 261 caratteri, mandata a capo.
     verificato: risolto — Services/ExpenseRepository.cs:43-45 (nessuna riga `///` oltre 100 caratteri)
  R2 bug-hunter (bassa) — fondato, fuori scope. `Disabilitato="@rileggoMese"` è inerte: CaricaUnGiro
     azzera `errore` prima del primo await (Shared/PaginaRegistro.cs:142 `errore = null;`), quindi
     mentre `rileggoMese` è vero il ramo con <ErroreRiprova> non è montato. Lasciato perché è la
     correzione chiesta dal mandato (punto 3), identica a Pages/SpeseTabella.razor:44, e innocua.
     Riaperto da me: confermato leggendo PaginaRegistro.cs:140-143 e Spese.razor:454-457.
  T1 threat-hunter (bassa, disponibilità) — fondato, fuori scope. `while (true)` senza tetto: un
     membro ostile di uno spazio condiviso che inserisce 100.000 spese costringe gli altri a 100
     letture sequenziali e a caricarle tutte. Prima la lettura si troncava a 1000 in silenzio. Un tetto
     che fallisce contraddice la clausola del mandato «qualunque periodo restituisce tutte le sue
     spese»: è una decisione dell'utente (tetto nel client, quota nel database, o niente).

FUORI SCOPE:
  - R2: durante «Riprova» dallo stato «spazi non letti» la pagina mostra per un istante «Serve uno
    spazio…» invece di uno scheletro (Spese.Riprova non rimette `caricato = false`); stesso schema in
    Pages/SpeseTabella.razor:44 e :330. Il parametro `Disabilitato` lì è inerte in entrambe le pagine.
  - T1: nessun tetto alla lettura a blocchi (sopra).
  - Registro: dopo «Segna» il fuoco passa a BODY (il pulsante a fuoco viene disabilitato) e non torna
    sul campo importo; chi digita subito perde i tasti senza segnale. È un difetto d'esperienza
    diverso da quello del punto 2, misurato nella riproduzione: non corretto, è da decidere.
  - Ordine fra spese della stessa data: il registro segue il repository (spent_on desc, id desc, id
    uuid casuale), la tabella ordina per CreatedAt desc (Pages/SpeseTabella.razor:348): ciascuna vista
    è stabile, ma le due mostrano i pari data in ordine diverso.
  - `max_rows` di produzione non verificato (mandato): se fosse < 1000, la lettura si fermerebbe al
    primo blocco come prima.

GATE:
  mcp__synapse__build root=<worktree> → succeeded — 0 errors, 0 warnings
  mcp__synapse__test root=<worktree>  → 0 failed, 390 passed, 0 skipped (389 + 1 nuovo, punto 4)
  live-testing → ESITO: difetti — solo per la pulizia negata; A-D verdi, nessun 416, totali coerenti
  porta 5000 → PORTA 5000 LIBERA (fermati figlio 23228 e padre 2040; prima 23352 e 13996)

SCOSTAMENTI:
  - Punto 1 senza test unitario: ElencaAsync è privato e parla con il client Supabase; testarlo
    avrebbe richiesto un'astrazione nuova, vietata dal budget. Coperto da collaudo (A-C) e doc-checker.
  - La verifica interna di Codex è fallita per l'ambiente (sandbox: accesso negato a Eton.Tests/obj e
    bin, «1 avviso, 1 errore» di accesso); build e test li ho fatti io con synapse, verdi.
  - Pulizia di produzione non completata (vedi ESITO).
  - Nessun lavoro nuovo arrivato durante l'unità.
```
