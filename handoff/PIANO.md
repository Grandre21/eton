# Piano — fase 2.3: doppioni, rate, ricorrenti divise, quota da accantonare, entrate e bilancio

Scritto il **20 ottobre 2026**. Il piano del goal precedente (2.1 e 2.2, chiuso il 12 ottobre) è archiviato
in `storico/handoff/2026-09-22-PIANO-ricorrenti-e-tabella.md`. Autosufficiente: chi riprende non ha bisogno
della conversazione da cui nasce.

## OBIETTIVO

> «nella sessione di ieri ho portato diversi problemi da correggere, fai un recap e vediamo di chiuderli in
> questa sessione» — utente, 20 ottobre 2026
>
> Al popup sul perimetro: **«Tutta la 2.3 (Consigliata)»** — utente, 20 ottobre 2026

**La glossa.** «Tutta la 2.3» è l'opzione del popup che elencava cinque punti, dalle richieste del 19 ottobre
(`docs/superpowers/specs/2026-09-10-modello-prodotto-design.md` §6, «Richieste del 19 ottobre»):
**0** i doppioni della lettura a blocchi oltre 1000 spese (rilievo R1 del 12 ottobre); **1** spese a rate
(Scalapay, Cofidis); **2** ricorrenti divise in mensili / annuali / ogni N mesi, con i totali; **3** la quota
mensile da accantonare per le non mensili; **4** entrate e bilancio del mese. Le parole del 19 ottobre,
verbatim, per la copertura:

- «Ci sono delle spese che faccio in un mese e vorrei partizionare in piu mesi perchè magari uso servizi come
  scalapay o cofidis, quindi non pesano interamente sul mese corrente, sono ripetibili ma non per sempre,
  avrebbe senso inserire la spesa come spesa nel mese e poi avere un flag dove indico su quanto si
  partiziona.»
- «nelle spese ricorrenti non ce una divisione su quelle mensili/annuali, sono mischiate penso in ordine
  alfabetico e mi si storgono gli occhi. Inoltre le spese annuali non vengono divise per l'anno, è vero che
  pesano interamente su un mese ma vorrei avere idea di quanti soldi al mese occorre tenere per non gravare
  unicamente su un mese.»
- «Sarebbe utile anche gestire le entrate, in modo da avere un bilanciamento»

**Fuori dal goal**: l'incrocio categorie × mesi e il fisso/variabile della 2.3 originaria («Analisi»), che il
popup non nominava; tutta la grafica (2.1-bis), compresa «graficamente la tabella può essere molto
migliorata»; segnala-un-bug, collezioni importabili, amministrazione.

## DECISIONI

- **2026-10-20 utente (chat, popup)** — perimetro della sessione: «Tutta la 2.3 (Consigliata)».
- **2026-10-20 utente (chat, popup)** — con le posizioni di `tech-advisor` nel popup: stipendio «Sì, anche
  ricorrenti (Consigliata)»; categorie delle entrate «Stipendio, Rimborso, Regalo, Vendita, Altro
  (Consigliata)»; piano a rate avviato «Solo chiuderlo prima (Consigliata)»; interruttore delle rate «Solo
  sulla spesa nuova (Consigliata)».
- **2026-10-20 — Decisioni del capo, da ratificare** (posizioni di `tech-advisor` del 20 ott, adottate nella
  spec `docs/superpowers/specs/2026-10-20-rate-entrate-design.md`): quota da accantonare solo nella pagina
  Ricorrenti, non in Home; il tipo di un movimento non si cambia dopo la creazione; numero di rate da 2 a 60,
  importo per rata arrotondato per difetto con il resto sull'ultima; «rata k/N» come suffisso salvato nella
  descrizione; ordine dei gruppi Entrate, Mensili, A rate, Annuali, Ogni N mesi, Terminate; cinque unità
  U1-U5 con una migrazione sola. **Ratificate 2026-10-20** (riga qui sotto).
- **2026-10-20 utente (chat, popup)** — sulla spec e sulle decisioni del capo: «Approvo, vai (Consigliata)».
- **2026-10-20 — Decisione del capo, da ratificare**: nessun piano a task (`writing-plans`) fra spec e unità;
  ogni unità scrive il proprio brief dalla spec. Raccomandazione: tenerla, la spec nomina già file, regole e
  test, e un'unità di solo piano costerebbe un giro intero. Si annulla scrivendo il piano prima della 03.
- **2026-10-20 — Decisione dell'unità 01, da ratificare**: il test della lettura a blocchi passa da un metodo
  estratto `internal static` (`LeggiABlocchiAsync`) invece che da un finto client Supabase. Raccomandazione
  dell'unità: tenerlo, è l'unica astrazione e ha un solo chiamante. Si annulla con un revert locale.
- **2026-10-20 — Quattro decisioni dell'unità 02, da ratificare** (raccomandazione dell'unità: tenerle tutte;
  la 02 non si pubblica prima della ratifica): (1) la riga «metti da parte» non compare con quota 0; (2) tolto
  il conteggio «N spese ricorrenti» in testa, sostituito dalle testate dei gruppi; (3) il gruppo si chiama
  «Ogni N mesi» come nella spec, «Periodiche» da valutare in 2.1-bis; (4) la quota è l'arrotondamento della
  somma esatta, e può differire di 1 centesimo dalla somma dei totali di gruppo.

## PARTIZIONE

| Unità | Cosa | Dove | Dipende da | Stato |
|---|---|---|---|---|
| **S — progetto 2.3** | spec dei punti 0-4, approvata in chat | `docs/superpowers/specs/2026-10-20-rate-entrate-design.md` | — | **PARZIALE** — approvata dall'utente in chat il 20 ott («Approvo, vai»); è un documento, non c'è un comando che lo verifichi |
| **01-doppioni** | punto 0 | sessione-unità, worktree `G:\Sviluppo\wt-Eton\01-doppioni`, mandato in `handoff/01-doppioni/` | S | **FATTO** — fusa su `main` il 20 ott, 0 rilievi aperti, una decisione da ratificare (DECISIONI); verifica: `mcp__synapse__test` -> `0 failed, 392 passed` |
| **02-gruppi-quota** | punti 2 e 3, senza schema | sessione-unità, mandato in `handoff/02-gruppi-quota/` | 01 (solo ordine) | **PARZIALE** — fusa su `main` **solo in locale** il 20 ott (non pubblicata: quattro decisioni da ratificare, DECISIONI); gate dell'unità verdi (398 test), `live-testing` verde; manca `ui-critic` (servono regole di prova lasciate a schermo in produzione, negato dal classificatore all'unità) |
| **03-migrazione** | la migrazione del §3, scritta e **non applicata** | sessione-unità, worktree da `origin/main` **senza** la 02 (file disgiunti) | 02 (solo ordine) | **IN CORSO** — job `8e5fe2f0`, rete armata |
| **GATE migrazione** | l'utente applica la migrazione dall'SQL editor e incolla l'esito della query di controllo | utente | 03 | PIANIFICATA |
| **04-rate** | punto 1 | sessione-unità | GATE | PIANIFICATA |
| **05-entrate** | punto 4 | sessione-unità | 04 | PIANIFICATA |

## RAZIONALE

Dalla ricognizione dello scout (20 ott): i file contesi sono `Services/ExpenseRepository.cs` (punti 0 e 4,
forse 1), `Models/Expense.cs` (4, e la costruzione delle occorrenze in `CalcoliRicorrenti`) e
`Pages/SpesaEdit.razor` (1 e 4). Le unità vanno in sequenza, e il contesto di ciascuna lo decide la spec.

## PROSSIMA AZIONE

Aspettare il resoconto della **03** e il parere di `tech-advisor` sulle ratifiche (D1-D6), sulla misura
`ui-critic` mancante della 02 e sul FUORI SCOPE BE1 della 02 (chiave di ordinamento in tre copie); poi **un solo
popup** all'utente: ratifiche + SQL della migrazione. Ratificate: push di `main` (01 già pubblicata, 02 solo
locale). Dopo «applicata»: 04, poi 05. ⚠️ `main` locale è avanti a `origin/main` con la 02.

controllo contesto 2026-10-20 14:15: nulla

## APERTO

- Due segnalazioni sulla configurazione dall'unità 01, trascritte in `~/.claude/segnalazioni/` il 20 ott:
  `2026-10-20-codex-file-meno-o.md` (la clausola «il file `-o`» fa creare a Codex un file «-o») e
  `2026-10-20-codex-powershell-scrive-file.md`. Nei mandati 02-05 c'è un'avvertenza in attesa della correzione.
- `RecurringExpenseRepository` non ha un file di test dedicato (scout, 20 ott): da considerare nel piano.
- Restano vivi dai goal precedenti e **non sono di questo goal**: `handoff/CHIUSURA.md` (12 ott) e
  `handoff/2026-09-20-CHIUSURA.md`, non ancora letti dall'utente, quindi non archiviati;
  `handoff/configurazione-da-applicare.md`, `handoff/collaudo/`, `handoff/01-punto-interrogativo/`,
  `handoff/server.md`.
- Le tre spese di prova vecchie («COLLAUDO GIRO B», «COLLAUDO 4 SET», «PROVA AGENTE») risultano eliminate il
  12 ottobre (piano archiviato, `PROSSIMA AZIONE`): la domanda in `APERTO` del piano vecchio è chiusa.

## FATTI OPERATIVI CHE COSTANO CARI SE DIMENTICATI

⚠️⚠️ **`main` PUBBLICA IN PRODUZIONE A OGNI PUSH** (GitHub Pages, nessun gate). Una migrazione la applica
**l'utente** dall'SQL editor; lo schema nuovo deve reggere col client vecchio già pubblicato.

⚠️⚠️ **IL WORKTREE DI UN'UNITÀ NASCE DA `origin/main`**: prima di aprirla si committa **e si pusha**.

- Codex 0.161.0 è l'autore dei diff fino al 2026-11-08 (forma dei lanci in `~/.claude/TODO-synapse-codex.md`).
- Il server lo avvia e lo ferma il capo; riavviarlo prima di ogni prova nel browser.
- Gli implementer non compilano: compila il capo, a fine giro.
- Il browser giusto ha `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.
- L'app di sviluppo punta a Supabase di **produzione**: le prove creano dati veri, da eliminare a fine prova.
- Un'autorizzazione su dati di produzione non passa a un'unità `--bg`: si esegue dalla sessione interattiva.
