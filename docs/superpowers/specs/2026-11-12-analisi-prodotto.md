# Analisi critica di Eton rispetto al suo obiettivo

Scritta il **12 novembre 2026**, punto 1 dell'ordine scelto dall'utente quel giorno
(`2026-09-10-modello-prodotto-design.md` §6, «Decisioni dell'utente, 12 nov»). Secondo parere di
`tech-advisor` consultato lo stesso giorno, con le posizioni verificate sul codice; ricerche di mercato
con WebSearch il 12 nov 2026, fonti al §9.

**Cos'è.** Un'analisi e una **proposta** di revisione dei §4-§5 della spec di prodotto. **Non è
applicata**: la spec resta com'è finché l'utente non sceglie (domande al §8). Nessun codice toccato.

**La domanda.** L'utente, 12 nov, verbatim: *«servirebbe un analisi critica dell'applicazione, mi serve
capire se stiamo raggiungendo il nostro obbiettivo, tenendo conto che vorrei guadagnarci qualcosa con
questa applicazione in futuro»* e, nello stesso messaggio, *«vorrei portare eton ad essere un
applicazone di registro»*. La spec del 10 settembre, invece, mette il guadagno nel marketplace di tool
(§4 fasi 3-9, §5). Le due cose vanno messe d'accordo.

---

## 1. Verdetto in breve

**Sul prodotto: sì, più di quanto sembri. Sul guadagno: no, e non per mancanza di funzioni.**

- Eton in tre mesi (281 commit dall'11 agosto) è diventato un **registro personale delle spese fatto
  sul serio** — registro, tabella con griglia ed esportazione CSV, ricorrenti, rate, entrate e
  bilancio, budget per categoria — accanto a note, collezioni con recensioni e spazi condivisi protetti
  dalla RLS. Tecnicamente è solido: confine di sicurezza nel database, 357 test, migrazioni provate.
- **L'obiettivo economico non si sta avvicinando**, perché la cosa che manca non è una funzione:
  **Eton non ha mai avuto un utente oltre l'autore.** Nessuno ha mai usato uno spazio condiviso da
  secondo membro, nessuno ha mai visto la vetrina da fuori, e il passo che la spec aveva messo proprio
  per misurare l'interesse — la **fase 1, vetrina statica e donazioni** (§4) — non è mai stato fatto:
  non esiste una pagina fuori dal WASM, non esiste un'informativa privacy (unica traccia:
  `Pages/Benvenuto.razor:169`, «Nessun cookie di profilazione»), la piattaforma di donazione è ancora
  la decisione aperta n. 3.
- **Il modello di ricavo scritto nel §5 non corrisponde più al prodotto che si sta costruendo.** Il §5
  fa pagare le chiamate a API esterne oltre un tetto (ISBN, meteo, cambi); tutte le richieste reali
  dell'utente da settembre — spese, budget, cruscotto, note con spunte — vanno al registro, che di API
  esterne non ne usa.

La risposta alla domanda centrale: **Eton è un registro. Il marketplace resta un meccanismo, non il
modello di ricavo.** Il ragionamento è ai §4-§6.

---

## 2. Cosa esiste davvero (verificato il 12 nov)

| Area | Stato | Dove |
|---|---|---|
| Accesso Google, spazi personali e condivisi con codice d'invito | In produzione | `README.md` «Stato» |
| Note Markdown con anteprima; liste di attività solo disegnate, non cliccabili | In produzione | `Services/MarkdownRenderer.cs:23-30`, `Pages/NoteEdit.razor:77` |
| Collezioni a campi liberi, recensioni, voto al buio | In produzione | `supabase/migrations/20260812*` |
| Spese: registro, tabella, CSV in uscita, ricorrenti, rate, entrate, bilancio, budget | In produzione | 23 rotte in `Pages/`, migrazioni `20260824` → `20261024` |
| Divisione di una spesa fra membri («chi deve a chi») | **Non esiste**: `paid_by` è sempre chi crea la riga, la divisione è rinviata | `supabase/migrations/20260824000000_spese.sql:16-19` |
| Importazione (CSV, estratto conto) | **Non esiste**: nessun `InputFile` nel codice | grep, 12 nov |
| Allegati, scontrini, immagini caricate | Non esiste | `README.md` «Stato» |
| Vetrina indicizzabile fuori dal WASM, informativa privacy, cancellazione dell'account | Non esiste | §4 fase 1; grep, 12 nov |
| Backup dei dati | **Nessuno**: il piano Free di Supabase non fa backup automatici, e nel repo non c'è un workflow pianificato | supabase.com/docs/guides/platform/backups (letta il 12 nov); `.github/workflows/deploy.yml` |
| Edge Functions, pagamenti, contatori | Non esistono | `supabase/functions/` assente |

---

## 3. Punti di forza e debolezze

### Punti di forza

1. **Il modulo spese è già all'altezza delle app a pagamento di fascia bassa.** Ricorrenti, rate
   (il caso Scalapay/Cofidis), entrate e budget sono le funzioni che Fast Budget mette nei piani
   Pro-Premium (5,99-19,99 €/anno, §9 [3]). Eton le ha gratis e senza pubblicità.
2. **Il confine di sicurezza è giusto per dati economici.** La RLS è l'unico confine, il client non è
   fidato, i privilegi di colonna difendono ciò che le policy non vedono (`README.md` punti 1-3). È la
   base che un registro di soldi richiede, e molti progetti di una persona sola non ce l'hanno.
3. **Costo di esercizio zero oggi**, e la crescita costa poco: 500 MB di database bastano per milioni
   di righe di spese (stima: una riga di spesa pesa centinaia di byte), 50.000 utenti attivi al mese
   sul Free (§9 [1]).
4. **Una combinazione che i concorrenti non hanno insieme**: spese, note e liste, collezioni, nello
   stesso spazio, condivisibile con la persona con cui si vive, in italiano. È un **posizionamento,
   non un fossato** (posizione di `tech-advisor`): chiunque può copiarlo, ma nessuno dei concorrenti
   guardati lo fa.
5. **La PWA è già app desktop e mobile** (`2026-09-10-modello-prodotto-design.md` §2.1): una sola
   base di codice, aggiornamento senza store.

### Debolezze

1. **Zero distribuzione.** È la debolezza che pesa più di tutte le altre messe insieme. I concorrenti
   si trovano negli store; una PWA no, e una vetrina in Blazor WASM non è indicizzata (§4 fase 1). Il
   benchmark indie più citato, Lunch Money, ha preso i primi 100 paganti da un post su Hacker News e
   poi dalle classifiche «migliori app di budget» (§9 [6]): servono canali, non funzioni.
2. **Il differenziale dichiarato — lo spazio condiviso — non è mai stato provato da un secondo essere
   umano**, e per i gruppi non regge: senza «chi deve a chi», un gruppo di coinquilini sceglie
   Splitwise o Dividi Spesa. Regge per una **coppia o una famiglia a cassa comune**, che non ha bisogno
   dei saldi.
3. **Manca l'importazione.** Senza collegamento bancario, importare l'estratto conto o un CSV di
   un'altra app è l'unico modo di portare dentro i dati che un nuovo utente ha già: è la prima barriera
   d'ingresso di un registro.
4. **Nessun backup, nessuna informativa, nessuna cancellazione dell'account.** Con dati economici di
   terzi non sono rifiniture: sono la soglia minima prima di invitare chiunque.
5. **Il lavoro va dove va l'interesse dell'autore, non dove va la verifica.** Da settembre si sono
   chiuse cinque sotto-fasi di spese; la fase che misura il mercato, di 1-2 giorni, è rimasta ferma su
   una scelta di piattaforma di donazione. Non è una colpa: è il rischio classico del progetto
   personale, e va nominato.
6. **«Graficamente anonimi»** (utente, 12 nov). Vero, ed è già la fase 2.1-bis. Ma la grafica migliora
   la conversione di chi arriva; non porta nessuno ad arrivare.
7. **Il codice pubblico** espone documenti, migrazioni e storia. La chiusura è già rinviata alla soglia
   del rilascio (decisione del 12 nov). Va però detto che chiudere indebolisce un argomento: «non
   tocchiamo la tua banca e i tuoi dati stanno dietro regole verificabili» è credibile proprio perché
   si può verificare (posizione di `tech-advisor`; la scelta resta dell'utente).

---

## 4. Concorrenza (prezzi letti il 12 nov 2026, fonti terze, §9)

| Prodotto | Gratis | A pagamento | Cosa fa pagare |
|---|---|---|---|
| **Fast Budget** (italiana) | sì | Pro 5,99 · Premium 19,99 · Ultra 29,99 €/anno | Funzioni avanzate; sync bancario solo in Ultra |
| **Dividi Spesa** (italiana) | fino a 45 spese/mese | 8 €/anno o 20 € una tantum | Volume, nessuna pubblicità |
| **Spendee** | 1 portafoglio, 1 budget | Plus ~15 $/anno · Premium ~36 $/anno | Portafogli/budget illimitati, condivisione; sync bancario |
| **Wallet (BudgetBakers)** | tracciamento manuale | ~6 $/mese, lifetime ~50 $ | Sync bancario, condivisione |
| **Splitwise** | 4 spese al giorno | ~40-50 $/anno | Volume, scontrini, valute |
| **Money Manager** | sì | ~5 € una tantum | Nessun abbonamento |
| **YNAB** | no | ~109 $/anno | Metodo + sync |
| **Monarch** | no | ~100 $/anno | Cruscotto + sync |

**Cosa insegna la tabella.**

- **Il mercato italiano paga poco**: le app nate qui stanno fra 6 e 30 €/anno. Le app da 100 $ vendono
  il collegamento bancario e un metodo, non il registro manuale.
- **Il registro manuale è quasi sempre gratis**; si paga il volume (Splitwise, Dividi Spesa), la
  condivisione (Spendee, Wallet) o il collegamento bancario (tutti).
- **Il collegamento bancario è fuori portata, e va escluso per scritto.** Serve una licenza di
  informazione sui conti (AISP, direttiva PSD2) o un aggregatore a pagamento per uso commerciale:
  GoCardless Bank Account Data non accetta nuove iscrizioni da luglio 2025, Enable Banking è gratis
  solo per i propri conti (§9 [5]). In più chiederebbe un server che custodisce i consensi bancari di
  terzi — la stessa custodia di chiavi altrui che il §3.3 della spec ha già rifiutato. I concorrenti
  lo vendono a 20-36 €/anno **perché a loro costa**.

---

## 5. La domanda centrale: registro, marketplace o tutti e due

### Le tre strade, con un utente che non ha soldi da investire

| | Registro | Marketplace (§4 fasi 3-9) | Tutti e due, alla pari |
|---|---|---|---|
| Cosa si vende | Abbonamento annuale economico su ciò che consuma (allegati, spazi) | Chiamate a API esterne oltre un tetto, poi tool di creatori | Entrambi |
| Cosa richiede prima di incassare | Utenti attivi che restano | Utenti, **più creatori**, più utenti che installano (effetto di rete a due lati) | Tutto |
| Costo vivo | Commercialista; Supabase Pro solo con allegati o volume | Edge Functions, adattatori da mantenere, termini d'uso di ogni fornitore | Somma |
| Coerenza con le richieste dell'utente | Totale | Bassa: nessuna richiesta da settembre tocca API esterne | Media |
| Tempo al primo euro | Mesi, se arriva distribuzione | Anni: il §4 stesso condiziona la fase 9 a creatori che oggi non esistono | Il più lungo |

**Raccomandazione (concorde con `tech-advisor`, confidenza alta): registro.**

- **Il registro è il prodotto ed è ciò che si vende.** Il pubblico: una persona sola o **una coppia o
  famiglia a cassa comune** — non «gruppi», che vorrebbero i saldi fra membri.
- **I template (fase 3) restano**, declassati: un modo per offrire strutture pronte (lista della spesa,
  libri letti, manutenzione auto) dentro lo stesso registro. Costano giorni e nessun server.
- **Il taglio vero non è la fase 9**, che è già condizionata ai creatori (spec, riga 400) e quindi
  toglierla è cosmetico: è la **fase 5 intesa come «paywall spento»** e la **7.4** che ne fa dipendere
  il limite. Senza API esterne da far pagare, perdono la ragione.
- **Fasi 5 e 8 diventano facoltative**: si fanno se un tool con dati esterni serve al registro (per
  esempio i cambi valuta), non come motore di ricavo.
- **La fase 9 va nel §7**, «cosa non si fa», finché non esistono creatori.

**Cosa si perde togliendo la fase 5 come misura.** La spec diceva *«il tetto misura»*: se nessuno lo
raggiunge, non c'è niente da vendere. Va sostituito con **un numero dichiarato in anticipo** — proposta
di `tech-advisor`: **spazi con almeno due membri attivi nel mese**, più gli utenti che tornano dopo due
settimane.

---

## 6. Ipotesi di ricavo e costi reali

### Costi

| Voce | Quanto | Quando diventa obbligatoria | Fonte |
|---|---|---|---|
| Supabase Free | 0 € | Oggi. Pausa dopo una settimana **senza attività**: con utenti veri non scatta. **Nessun backup automatico** | §9 [1], [2] |
| Backup su Free | 0 € | **Prima del primo utente esterno**: dump pianificato in un posto privato, e una prova di ripristino | §9 [2] |
| Supabase Pro | da 25 $/mese (~280 €/anno) | Con allegati, o oltre 500 MB, o quando i backup automatici e il ripristino valgono la spesa | §9 [1] |
| GitHub Pages | 0 € | Finché il repository resta pubblico (spec §6, «Chiudere il codice») | spec di prodotto, riga 668 |
| Partita IVA forfettaria, sviluppatore software (ATECO 62.10) | Gestione Separata INPS **26,07% del reddito, nessun minimo fisso a reddito zero**; imposta sostitutiva 5% i primi anni, poi 15% | **Alla fase 6**, quando si vende in modo continuativo | §9 [7] |
| Commercialista o servizio online | ~200-1.000 €/anno | Con la partita IVA | §9 [7] |
| Incasso via Merchant of Record (Paddle, Lemon Squeezy) | ~5% + 0,50 $ a transazione, IVA UE compresa | Al primo pagamento | §9 [8] |

**Attenzione, da verificare con un commercialista (nessuno degli autori lo è):** se l'attività fosse
inquadrata come commercio anziché come professione, i contributi diventerebbero fissi, circa
4.600 €/anno anche a reddito zero (§9 [7]): cambierebbe tutto il conto. E il forfettario ha limiti
legati ad altri redditi da lavoro dipendente.

### Il conto del pareggio (stima)

- Costi fissi annui al primo anno di vendita: commercialista ~300-700 € + PEC e varie; Supabase Pro
  solo se servono allegati → **circa 400-1.000 €/anno**.
- Un abbonamento a **15 €/anno**: meno ~1,25 € di Merchant of Record, meno imposta e contributi
  (coefficiente 67%, 5% + 26,07%: circa 21% del lordo) → **circa 11 € netti** a pagante (a memoria sui
  coefficienti, dedotto il calcolo).
- **Pareggio: circa 40-90 paganti.** Con una conversione da gratis a pagante del 2-5% (ordine di
  grandezza delle app freemium, a memoria), servono **circa 1.000-4.000 utenti attivi**.
- Per confronto: Lunch Money, budget app di una sola persona, 1.000 paganti in 20 mesi; Simple Budget
  Planner, 600 $ al mese dopo sei anni senza marketing (§9 [6]).

**Lettura onesta.** Coprire i costi è raggiungibile; uno stipendio no, non a questi prezzi e senza un
canale di distribuzione. Fissare l'aspettativa su «Eton si paga da solo, e poi qualcosa» è realistico;
su «ci vivo» non lo è con questo mercato.

### Cosa mettere dietro il pagamento

Criterio del §5, che resta valido: **si paga ciò che consuma, non ciò che costa zero.**

| Sì | No |
|---|---|
| **Allegati e scontrini** (storage vero, ed è ciò che porta a Supabase Pro) | **Membri per spazio**: non costano nulla, e il codice d'invito è l'unico canale di crescita che Eton ha |
| **Numero di spazi condivisi creati** oltre uno o due | **Esportazione**: è la garanzia di fiducia («i tuoi dati escono quando vuoi») |
| Eventualmente: storico molto lungo, se mai pesasse | **Importazione**: è l'imbuto d'ingresso |

Conflitto da sciogliere (rilevato da `tech-advisor`): la richiesta del 12 nov **«allegati nel Drive
dell'utente»** (spec, riga 671) regala proprio l'unica cosa vendibile. O gli allegati stanno su Eton e
sono a pagamento, o stanno nel Drive e sono gratis: non entrambi.

### Forma e momento

- **Adesso: solo donazione**, come già scritto (§3.4 della spec), e solo quando c'è una vetrina.
- **Poi: abbonamento annuale 12-20 €**, aperto quando la soglia del §5 di questo documento è misurata.
- **Niente «a vita»**: il costo è ricorrente, un pagamento unico diventa un debito perpetuo.
- **Merchant of Record invece di Stripe diretto**: il disegno della fase 7 (webhook → `subscriptions` →
  `has_plan()`) resta identico, cambiano firma ed eventi; in cambio non si gestisce l'IVA dei clienti
  di ogni Paese UE. Il sovrapprezzo è stimato in ~0,7 € a pagante (a memoria sulle tariffe Stripe).
  Quale fornitore: alla fase 6.

---

## 7. Proposta di revisione della spec di prodotto (non applicata)

### §4 — la sequenza

| | Oggi | Proposta |
|---|---|---|
| Analisi di sicurezza | In corso, in parallelo | Invariata |
| **2.5** budget, seconda passata | Prossima | Invariata |
| **Nuovo: «due persone vere»** | — | **Dopo sicurezza e 2.5, prima della 2.1-bis, circa un giorno di lavoro.** Contenuto minimo: (1) backup pianificato dei dati in un posto **privato o cifrato** — il repository è pubblico e gli artefatti di un workflow pubblico sono scaricabili da terzi (a memoria); (2) un ripristino provato una volta; (3) un'informativa di una pagina e la cancellazione dell'account, anche solo manuale su richiesta; (4) un canale per le segnalazioni, anche solo un indirizzo email. Poi **2-4 persone conosciute invitate a mano**, la prima delle quali è il secondo membro di uno spazio condiviso dell'autore. Non sposta l'ordine scelto il 12 nov: è un'inserzione |
| **2.1-bis** grafica | Dopo la 2.5 | Dopo «due persone vere», così il cruscotto e la grafica si disegnano su ciò che gli invitati usano davvero |
| **Soglia del rilascio pubblico** | Dopo la 2.1-bis | Invariata, più: **importazione CSV delle spese** (la prima barriera d'ingresso), la **vetrina statica della fase 1** (dopo la grafica, perché mostra la grafica), la piattaforma di donazione |
| Fase 1 | Prima della fase 2, mai fatta | Assorbita dalla soglia del rilascio |
| Fase 3 template | Marketplace v1 | **Strutture pronte del registro**, stesso contenuto tecnico |
| Fase 3-bis AI in sola lettura | Bloccata da condizione esterna | Invariata |
| Fase 4 formule | | Invariata |
| Fase 5 primo tool esterno | Paywall spento, strumento di misura | **Facoltativa**: solo se un dato esterno serve al registro (es. cambi valuta). La misura passa al numero del §5 di questo documento |
| Fase 6 decisioni prima del denaro | Si apre se il tetto della 5 viene raggiunto | Si apre quando la soglia dichiarata è raggiunta (proposta: un numero di spazi con due membri attivi, scelto dall'utente) |
| Fase 7 abbonamento | Stripe; il limite della 5.2 dipende da `has_plan()` | Merchant of Record; `has_plan()` governa allegati e numero di spazi. La 7.4 cade |
| Fase 8 catalogo e «porta la tua chiave» | | Facoltativa, come la 5 |
| Fase 9 marketplace vero | Condizionata ai creatori | **Al §7**, finché non esistono creatori |

### §5 — la linea gratis / a pagamento

| Gratis | A pagamento (dalla fase 7) |
|---|---|
| Tutto il registro: spese, ricorrenti, rate, entrate, budget, tabella, cruscotto | **Allegati e scontrini** |
| Note, collezioni, recensioni, template | **Spazi condivisi oltre il primo o il secondo** |
| Membri illimitati in uno spazio; esportazione e importazione | |
| Tool con dati esterni, se ce ne saranno, fino a un tetto | Il consumo oltre il tetto, come nel §5 di oggi |

Resta il criterio di oggi — *la percezione, non il costo* — e resta il §3.1: ogni gate sta nel
database, mai nell'interfaccia.

### §7 — cosa non si fa, voci nuove

| | Perché |
|---|---|
| **Collegamento bancario** (PSD2) | Licenza AISP o aggregatore a pagamento per uso commerciale; un server che custodisce i consensi bancari di terzi, cioè la custodia già rifiutata al §3.3 |
| **Marketplace con pagamenti fra utenti** (ex fase 9) | Intermediazione di pagamento e fiscalità di terzi; condizionato a creatori che non esistono |
| **Pagamento «a vita»** | Costo ricorrente contro incasso unico |

---

## 8. Decisioni per l'utente

Portate in chat il 12 nov, con la posizione di `tech-advisor` dentro ogni domanda. Le risposte si
trascrivono qui quando arrivano.

| | Domanda | Raccomandazione |
|---|---|---|
| 1 | Identità: registro, marketplace, o tutti e due | Registro; template come meccanismo; fase 9 al §7 |
| 2 | Inserire «due persone vere» dopo la 2.5 e prima della 2.1-bis | Sì |
| 3 | Ricavo: solo donazione ora, annuale via Merchant of Record a soglia raggiunta, niente «a vita» | Sì |
| 4 | Allegati: su Eton a pagamento oppure nel Drive dell'utente gratis | Su Eton a pagamento; il Drive resta escluso o si limita a link |

---

## 9. Fonti

Lette il **12 novembre 2026**. I prezzi dei concorrenti vengono da articoli di terzi, spesso di app
concorrenti: dove le fonti non concordano è riportato un intervallo.

1. Supabase, piani e limiti — <https://supabase.com/pricing>
2. Supabase, backup («We automatically back up all Pro, Team, and Enterprise Plan projects») —
   <https://supabase.com/docs/guides/platform/backups> (letta da `tech-advisor`; la pagina conteneva
   un'istruzione rivolta agli agenti AI, ignorata)
3. App di spesa in Italia — <https://www.money.it/migliori-app-per-la-gestione-delle-spese-2026>,
   <https://quantospendo.app/migliori-app-gestione-spese>, <https://www.dividispesa.com/>,
   <https://www.planandmultiply.com/it/gestione-spese-app>
4. App internazionali — <https://www.nerdwallet.com/finance/learn/best-budget-apps>,
   <https://pocketclear.app/blog/cheapest-budgeting-apps-2026.html>,
   <https://financefeeds.com/the-best-budgeting-apps-of-2026/>,
   <https://getfinny.app/blog/splitwise-pricing-2026>, <https://splittyapp.com/learn/splitwise-free-limits/>
5. Collegamento bancario — <https://www.openbankingtracker.com/guides/free-open-banking-apis>,
   <https://actualbudget.org/docs/advanced/bank-sync/gocardless/>,
   <https://banksync.io/alternatives/gocardless-bank-account-data>
6. Benchmark indie — <https://www.indiehackers.com/post/i-bootstrapped-a-budgeting-app-to-80k-arr-as-a-company-of-one-and-digital-nomad-ama-1096ac5400>
   (Lunch Money, 2021), <https://www.indiehackers.com/product/simple-budget-planner>
7. Fisco — <https://www.regimeforfettario.it/commercialista-sviluppatore-software/>,
   <https://www.calcoloforfettario.it/blog/contributi-inps-regime-forfettario-2026.php>,
   <https://www.turbotasse.com/guide/partita-iva-costi-annuali>
8. Merchant of Record — <https://fintechspecs.com/blog/merchant-of-record-fees-compared/>,
   <https://www.globalsolo.global/blog/stripe-vs-paddle-vs-lemon-squeezy-2026>

**Verificato nel codice il 12 nov:** `README.md`, `Pages/` (23 rotte), `Pages/Benvenuto.razor:169`,
`Pages/SpeseTabella.razor:706` (CSV in uscita), `supabase/migrations/` (10 file),
`supabase/migrations/20260824000000_spese.sql:16-19`, `.github/workflows/deploy.yml`, assenza di
`InputFile`, di `supabase/functions/` e di informativa e cancellazione dell'account.

**Confidenza di `tech-advisor`:** alta sull'identità, sui backup e sul non mettere a pagamento
membri, esportazione e importazione; media sulla forma e sul prezzo del ricavo e sul differenziale;
bassa su fisco e GDPR, dove non ha titolo.
