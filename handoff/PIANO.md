# Piano — chiudere i punti rimasti aperti dal ciclo dei sedici rilievi

Scritto il **19 settembre 2026** all'apertura del goal. Sostituisce il piano del 3 settembre, il
cui ciclo è chiuso e archiviato in `storico/handoff/PIANO.md` con il suo rapporto
`storico/handoff/CHIUSURA.md`.
Autosufficiente: chi riprende non ha bisogno della conversazione da cui nasce.

## OBIETTIVO

> «vorrei chiudere tutti i punti rimanenti in questa sessione»
> — utente, 19 settembre 2026

**La glossa, e perché non è scontata.** «I punti rimanenti» sono ciò che il ciclo precedente ha
lasciato aperto **dichiarandolo**, non tutto ciò che resta da fare nel progetto. In concreto:

1. le **18 voci** del `FUORI SCOPE — aggregato` di `storico/handoff/CHIUSURA.md`;
2. la clausola **scoperta** di quel rapporto: la pendenza del **medaglione 📋**;
3. il difetto dei **profili congelati**, che stava nel campo `APERTO` del piano precedente e non
   è mai entrato in nessun `FUORI SCOPE`;
4. la **ricognizione mai fatta** dell'area voti e recensioni, anch'essa da `APERTO`.

**Ventuno clausole.** La colonna «voci» della `PARTIZIONE` dice dove cade ognuna: è la mappa che
la sessione di chiusura userà per contare coperte e scoperte, e l'unica cosa di questo file che
non è ricostruibile leggendo i resoconti.

**Non** ne fanno parte la fase 2 (spese ricorrenti) né la fase 2.1-bis (mandato UI/UX): sono fasi
del piano di prodotto, settimane ciascuna, e una sessione porta un goal solo. La domanda posta
all'utente il 19 settembre offriva anche quelle due come obiettivo: ha scelto questo.

## STATO DI PARTENZA

`main` pulito, **0 avanti / 0 indietro** rispetto a `origin/main`, fino a `6b7e8b4`, ultimo
commit del **10 settembre**. Nove giorni di fermo, nessuna modifica non committata.
Porta 5000 **libera**, nessun processo `dotnet` vivo: verificato con `Get-NetTCPConnection`, non
dedotto dal file. I PID annotati in `handoff/server.md` appartengono al collaudo del 10 settembre
e sono storia.

**I gate, rieseguiti il 19 settembre su `6b7e8b4` prima di aprire qualunque unità**, perché uno
stato di partenza riportato da un rapporto di nove giorni fa è un fatto citato, non osservato:

    dotnet build -warnaserror --no-incremental → Avvisi: 0   Errori: 0
    dotnet test                                → Superati: 287   Non superati: 0   Totale: 287

Coincidono con quanto dichiarava il rapporto di chiusura. Chi trova un gate rosso a fine goal
sa quindi che a romperlo è stato questo lavoro.

## DECISIONI

*Append-only, datate. L'utente può appendere qui una riga in qualunque momento: ha lo stesso peso
di una detta in chat. Rileggere questo campo prima di ogni PROSSIMA AZIONE.*

- **19 set 2026** — **L'utente ha delegato l'autonomia**, con la formula «se hai domande me le
  poni subito e poi lavori totalmente in autonomia quanto possibile». Le due domande sono state
  poste e risposte prima di aprire il goal. Da qui in avanti le scelte che sarebbero andate a lui
  si prendono e si dichiarano in questo campo, col motivo. Resta l'eccezione permanente:
  **l'SQL di produzione lo esegue solo lui**. Le migrazioni si scrivono, si committano, e si
  consegnano nel rapporto finale.

- **19 set 2026** — **La fase UI/UX cade fra la 2.1 e la 2.2**, diventando la fase 2.1-bis del
  piano di prodotto. Decisa da me su delega esplicita dell'utente («lascio la scelta a te»),
  dopo la posizione di `tech-advisor` (confidenza **media**, e il motivo della riserva è che quel
  progetto non ha ancora né spec né piano). Le tre ragioni stanno per esteso in
  `docs/superpowers/specs/2026-09-10-modello-prodotto-design.md`, §6 decisione 4, che da oggi è
  **chiusa**. In sintesi: la 2.1 ha spec e piano già approvati sui pattern di oggi e aprire prima
  la UI/UX li invaliderebbe; la 2.2 è l'unica schermata il cui design è ancora aperto e si può
  disegnare una volta sola; il tetto invalicabile resta «prima della fase 3», che moltiplica le
  schermate.

- **19 set 2026** — **Rilievo 1, il grigio: `#8a8a8a`.** Deciso da me. `#808080` è più
  conservativo ma dà **4,36:1** su `--superficie-alta`, sotto la soglia di 4,5:1, e reggerebbe
  solo alla condizione «nessuna micro-etichetta poggia mai su quel fondo» — che il documento dei
  rilievi dichiara *da verificare e non da assumere*. Una correzione che dipende da una condizione
  non verificata si rompe in silenzio il giorno in cui qualcuno sposta un'etichetta.
  `#8a8a8a` passa su tutti e quattro i fondi (6,08 / 5,73 / 5,43 / 4,99) senza condizioni.

- **19 set 2026** — **Rilievo 3, la colonna: `html { scrollbar-gutter: stable; }`**, non la
  strada `margin: 0` + `padding-left` fisso. Deciso da me **dopo `doc-checker`**, che ha
  verificato tre cose sulla fonte e non a memoria: la spec W3C CSS Overflow L3 §4.2 dice che con
  `stable` la gutter è presente *«regardless of whether the box is actually overflowing»*, cioè
  esattamente il salto da eliminare; il blog ufficiale WebKit dichiara che la proprietà **non ha
  alcun effetto con le overlay scrollbar**, che sono il default di sistema su iOS, iPadOS e
  macOS, quindi **non toglie un pixel sui telefoni**; il supporto è Chrome 94, Firefox 97,
  Safari 18.2 su macOS e iOS — Baseline da dicembre 2024.
  Il difetto quindi esiste solo su desktop con scrollbar classiche, e lì la riga lo chiude senza
  rinunciare alla centratura, che l'altra strada avrebbe dovuto sacrificare.
  ⚠️ `doc-checker` ha dichiarato **non verificato** il punto «overlay è il default anche su
  Android»: corroborato solo da conoscenza comune, nessuna fonte ufficiale letta. Non cambia la
  decisione — se su Android la scrollbar fosse classica, `stable` farebbe lì la stessa cosa utile
  che fa su Windows — ma non va ricopiato come se fosse confermato.

- **19 set 2026** — **Rilievo 4, applicato a metà, e la metà è dichiarata.** Deciso da me.
  *Si applica* all'editor di elemento: due pulsanti che portano entrambi la parola «Salva» e
  fanno cose diverse sono ambigui a prescindere da qualunque regola di sistema, quindi il secondo
  diventa «Salva recensione» e perde `primario`. *Non si applica* alla Home: lì i due primari sono
  scorciatoie gemelle e simmetriche, non due azioni in competizione per lo stesso oggetto, e
  «un solo primario per vista» è una regola che Eton non ha mai dichiarato. Dichiararla è lavoro
  della fase 2.1-bis, non di un fix di debito. L'esecutore di `ui-critic` l'aveva già scritto:
  *«va deciso, non applicato»*.
  **Conseguenza sulla partizione**, ed è il motivo per cui questa decisione sta qui e non nel
  mandato: senza di essa `Pages/Home.razor` sarebbe stato conteso da **cinque** gruppi invece che
  da due.

- **19 set 2026** — **Voce 16, `Denaro.Testo` non si rinomina.** Deciso da me. Il resoconto 12
  chiedeva che «nessuna delle due sia il default»; la ricognizione ha misurato il costo: **sette**
  consumatori fuori dai test e **quattordici** asserzioni che nominano i due metodi per nome in
  `Eton.Tests/DenaroTests.cs`. Una rinomina è quindi un diff largo a guadagno nominale — e il
  default *giusto* c'è già: `Testo` è la resa di visualizzazione, `TestoDigitabile` è il caso
  speciale e il suo nome lo dice. Il dubbio si chiude **documentando**: un commento `///` su
  entrambi i metodi che dica quale usare quando, e perché scegliere l'altro produce il difetto da
  cui `TestoDigitabile` è nato. Costo: due commenti. Rischio: zero.

- **19 set 2026** — **Profili congelati: si corregge lato client, non con una migrazione.**
  Deciso da me. La ricognizione ha stabilito il fatto che decide: i privilegi **ci sono già** —
  `20260811000000_initial_schema.sql:303` concede `update (display_name, avatar_url)` e la
  policy `profiles_update` a `:232-234` la consente a chi è il proprietario della riga. Non manca
  un permesso: manca un chiamante. Una correzione client si rilascia **subito**, senza passare
  dall'unica cosa che in questo progetto richiede l'utente in persona, cioè l'SQL in produzione.
  **Il limite, dichiarato invece che scoperto dopo:** chi non riapre mai l'applicazione resta col
  nome vecchio agli occhi degli altri. Un trigger `on_auth_user_updated` coprirebbe anche loro, e
  resta la strada giusta il giorno in cui si toccherà lo schema per altro — la fase 2.1 lo farà.

- **19 set 2026** — **Voce 6: non c'è niente da revocare, c'è un precedente da rispecchiare.**
  Deciso da me dopo aver fatto citare il **verbatim** del commento, invece di fidarmi della
  parafrasi del rapporto di chiusura. Il rapporto (`storico/handoff/CHIUSURA.md:165`) gli
  attribuiva la frase «il verdetto è dell'ultimo tentativo», che legge come una **decisione di
  principio**; il testo reale (`Pages/CollectionEdit.razor:209-216`) dice un'altra cosa: azzerare
  a ogni mutazione *«avrebbe richiesto di intercettare sette punti più tutti i `@bind`»*. È un
  argomento di **costo**, non di merito, e non sostiene affatto che il riquadro debba restare
  acceso dopo una rimozione.
  Nello stesso file, settanta righe più su, `ApplicaModello` (`:481-492`) **azzera già**
  `erroriValidazione` dentro la mutazione, con un commento che descrive la meccanica identica alla
  voce 6 — «un errore rimasto acceso nominerebbe un campo di un elenco che non esiste più». Fra
  quel metodo e `Rimuovi` (`:559-564`), che non azzera, **non esiste motivazione scritta da
  nessuna parte**.
  Quindi: `Rimuovi` si allinea ad `ApplicaModello`. **`Shared/PaginaEditor.cs` non si tocca**, e
  non nasce nessun `AzzeraEsito()`: l'interfaccia nuova che la ricognizione proponeva come
  possibile non serve. Sul verbatim il fix è una riga; sulla parafrasi sarebbe stata la revoca di
  una decisione di progetto più una modifica alla classe base che tutto il ciclo precedente ha
  tenuto nel `NON TOCCARE`.

- **19 set 2026** — **Voce 10: vincolo di costo, e un esito «non si fa» è legittimo.** Deciso da
  me. La ricognizione ha censito ogni assegnazione nei quattro editor: `errore` si azzera **solo**
  dentro `Carica`, `Salva`, `Sovrascrivi` ed `Elimina` — mai sul cambio di un campo, in nessuno dei
  quattro, in nessuna forma. Correggerla nel modo ovvio significa intercettare ogni `@bind`, cioè
  **esattamente il costo che il progetto ha già valutato e rifiutato per iscritto**.
  Il vincolo per l'unità: **nessuna intercettazione dei `@bind`**. Se il rimedio non sta in una
  condizione di render o in un azzeramento dentro una mutazione **già esistente**, la voce si
  chiude come decisione dichiarata — «non si fa, e questo è il motivo» — e finisce nel rapporto.
  Una voce chiusa con un motivo è chiusa; una voce chiusa spendendo più di quanto valga, no.

- **19 set 2026** — **Rilievo 2: la testata per schermo stretto non si tocca.** Deciso da me.
  Il gemello di «Profilo» in `Pages/Home.razor:37-40` tiene l'etichetta **visibile di proposito**,
  e il commento a `:22-25` dice perché: `Shared/Icona.razor` marca l'`<svg>` `aria-hidden="true"`
  (`:15`) *«proprio perché conta sempre di trovarsi accanto a un'etichetta di testo visibile»*, e
  quel componente ha **un solo parametro**, `Nome` (`:82`) — nessun `AriaLabel`, nessun
  `CaptureUnmatchedValues`, quindi un `aria-label` passato dal call-site verrebbe **scartato in
  compilazione**. Le due forme divergono per ragioni documentate, e le due classi non si
  incontrano mai (`app.css:2269` le spegne entrambe dove `.voce-piede` si accende).
  **Conseguenza sul perimetro:** all'unità 02 di `Home.razor` resta il solo blocco `@code`.

- **19 set 2026** — **Voce 17: chi non può intervenire vede la resa di visualizzazione.** Deciso
  da me. Oggi nel campo importo di chi non ha il permesso di modificare compare `1284,50`, mentre
  lo stesso importo nell'elenco è `1.284,50 €`: la differenza esiste perché il campo, anche quando
  è spento, continua a mostrare la **grammatica di input** — quella che serve a chi digita.
  Ma un campo che nessuno può toccare non è un campo: è un testo, e un testo segue la grammatica
  di visualizzazione. Quindi in sola lettura si rende con `Denaro.Testo`, separatore delle
  migliaia e simbolo compresi, esattamente come l'elenco da cui l'utente ci è arrivato.
  Questo **non** riapre la voce 16: `TestoDigitabile` resta il formato di chi digita, ed è nato
  apposta perché `Verifica` rifiutava le stringhe con più di un separatore. Le due rese hanno due
  pubblici diversi, ed è la ragione per cui esistono entrambe.

## PARTIZIONE

Sei unità, **in sequenza, mai in parallelo**. La colonna «voci» usa la numerazione del
`FUORI SCOPE — aggregato` di `storico/handoff/CHIUSURA.md`.

| Unità | Perimetro | Voci | Dipende da | Stato |
|---|---|---|---|---|
| **01 foglio-di-stile** | `wwwroot/css/app.css` | 1, 3, 5, la quota CSS della 2, **P2** | — | **IN CORSO** (aperta 19 set, `7232601f`) |
| **02 barra-e-home** | `Shared/Navigazione.razor`, `Pages/Home.razor` (**solo `@code`**) | 2 (markup), 15 | 01 | PIANIFICATA |
| **03 editor-esiti** | `Pages/CollectionEdit.razor`, `Pages/ItemEdit.razor`, `Pages/NoteEdit.razor`, `Pages/SpesaEdit.razor`, `Shared/RecensioniElemento.razor` — **`Shared/PaginaEditor.cs` NON si tocca** | 4 (editor elemento), 6, 10 | 01 | PIANIFICATA |
| **04 igiene-e-importi** | `Pages/CollectionDetail.razor`, `Services/SchemaCampi.cs`, `Services/Denaro.cs`, `Eton.Tests/SchemaCampiTests.cs`, + le righe residue di `CollectionEdit`/`ItemEdit`/`SpesaEdit` | 11, 14, 16, 17 | 03 | PIANIFICATA |
| **05 accesso** | `Services/SupabaseService.cs`, `Services/OAuthCallback.cs`, `Services/PkceStore.cs`, `Services/BrowserSessionHandler.cs`, `Eton.Tests/OAuthCallbackTests.cs` | 7, 8, 9 (indagine), 18 | — | PIANIFICATA |
| **06 profilo-allineato** | `Services/AuthStateService.cs`, nuovo `Services/ProfileRepository.cs`, un call-site in `Services/SupabaseService.cs` | il difetto da `APERTO` | 05 | PIANIFICATA |

**I sei mandati sono già scritti**, tutti il 19 settembre, in `handoff/NN-slug/mandato.md`. Un
capo fresco non deve riscriverli: li apre in ordine, uno per volta, e aggiorna la colonna di
stato. Sono stati scritti insieme di proposito — dopo una sola ricognizione, con tutti i file
contesi sul tavolo — perché è in quel momento che si vede chi eredita cosa, e ogni mandato porta
già l'avvertenza su ciò che l'unità precedente gli avrà spostato sotto i piedi.

**Non assegnate a un'unità, perché non producono codice** — le fa il capo a ciclo chiuso, con il
server avviato da lui e i PID su disco:

| Voce | Cosa | Quando |
|---|---|---|
| **12** | la prova a 360px della coppia «Sì, elimina» / «Annulla» | collaudo, con `live-testing` |
| **13** | il banner di aggiornamento in condizioni vere | **non chiudibile in sviluppo**: si vede solo sul pubblicato |
| **4ª clausola** | la ricognizione mai fatta dell'area voti e recensioni | collaudo, dopo che le unità sono rientrate |

## RAZIONALE

**Il file che ha deciso la partizione non è `app.css`.** L'ipotesi di partenza era che il foglio
di stile, che in questo progetto ha storicamente un proprietario unico, fosse il collo di
bottiglia. La ricognizione l'ha smentita con i numeri: `app.css` è conteso da **due** gruppi e per
**una regola sola** — `.voce-piede` a `:2247-2258`, che i rilievi 2 e 5 colpiscono sui due assi
diversi (la sovrapposizione orizzontale di 9px e l'altezza di 36px sono la stessa scatola da
`2.25rem`). `Pages/Home.razor` invece lo volevano **cinque** gruppi.

Due decisioni l'hanno sciolto senza spostare un file:

- il **rilievo 4 non si applica alla Home** (v. `DECISIONI`), e un contendente sparisce;
- la **voce 16 non rinomina `Denaro.Testo`**, e sparisce il secondo (`Home.razor:114`).

Restano due: la testata per schermo stretto (unità 02) e il blocco `@code` del cambio spazio
(unità 02). Stessa unità: nessuna contesa.

**`app.css` va tutto all'unità 01, `.voce-piede` compreso.** All'unità 02 resta la sola riga di
markup di `Navigazione.razor`. Il rilievo 2 lo prevedeva già: *«il quadrato a `var(--tocco)`
(48px) invece di 2.25rem — che chiude anche il rilievo 5 per questo elemento»*. Un elemento, due
rilievi, un proprietario.

**Le unità 03 e 04 si passano tre file, e la proprietà è nel tempo, non nello spazio.**
`CollectionEdit`, `ItemEdit` e `SpesaEdit` li tocca prima la 03 (gli esiti di validazione) e poi
la 04 (l'igiene e il formato degli importi). Non è una violazione del criterio: le unità sono
sequenziali per regola, e la 04 non parte finché la 03 non è rientrata. È dichiarato qui perché
un capo fresco che legge solo la tabella vedrebbe due perimetri che si sovrappongono e
penserebbe a un errore.

**Perché la 05 e la 06 sono separate benché tocchino lo stesso file.** `SupabaseService.cs` è il
file di cucitura: la 05 lo riscrive in profondità (le quattro chiamate a `localStorage`, le due
frasi, l'estrazione di `FraseRifiuto`), la 06 gli aggiunge **un solo call-site** nel punto in cui
il bootstrap è concluso. Fonderle significherebbe un'unità che fa due lavori scorrelati in un
file delicato; separarle costa una sequenza in più e nient'altro.

**La quarta chiamata che le 18 voci non contavano.** La voce 8 ne elencava tre; la ricognizione
ne ha trovata una **quarta**, `SupabaseService.cs:174` (`_pkce.Salva`) dentro
`AvviaAccessoGoogleAsync`, che non ha alcun `try` — ed è il ramo che l'utente percorre **per
primo**, premendo «Entra con Google». Entra nel mandato della 05: una voce che si scopre
incompleta si corregge, non si esegue alla lettera.

## PROSSIMA AZIONE

PROSSIMA AZIONE: attendere il resoconto dell'unità **01 foglio-di-stile**, aperta il 19 settembre
(`7232601f`). Al suo rientro: auditare `CONTRATTI` e `SCOSTAMENTI` — in particolare la misura del
medaglione sul **terzo** call-site, che è il punto in cui un numero può essere giusto in due posti
e sbagliato nel terzo — poi aprire l'unità **02 barra-e-home**, il cui mandato è già scritto in
`handoff/02-barra-e-home/mandato.md`.

⚠️ **Sequenziale, mai in parallelo.** Nessuna unità si apre finché la precedente non è rientrata,
anche quando i perimetri sembrano disgiunti: le unità 03 e 04 si passano tre file, e la 05 e la 06
se ne passano uno.

## APERTO

**Domande per l'utente — nessuna, al momento.** Le due che c'erano sono state poste e risposte il
19 settembre prima di aprire il goal.

**Dubbi miei, non ancora domande.**

- ~~Il contratto `PaginaEditor` e la voce 6: una decisione scritta va revocata.~~ **Istruito e
  chiuso il 19 settembre**, e l'esito è che non c'era niente da revocare: v. `DECISIONI`. Non
  sparisce da qui perché il **modo** in cui si è chiuso vale più dell'esito — il rapporto di
  chiusura ne portava una **parafrasi** che leggeva come decisione di principio, il verbatim dice
  un'altra cosa, e sulla parafrasi avrei aperto un'unità per modificare la classe base che tutto
  il ciclo precedente ha tenuto nel `NON TOCCARE`. Chi in futuro trova in un rapporto una frase
  fra virgolette che giustifica un lavoro grosso, se la faccia citare dal file.
- **L'unità 04 è la più esposta a tornare `PARZIALE`**: quattro voci su sei file, tre dei quali
  ereditati dalla 03. È la prima candidata a essere ripartizionata.
- **La voce 9 (la barra gialla di Blazor) potrebbe non esistere come difetto.** Il giro C l'ha
  vista a rete bloccata, con `window.fetch` sovrascritto per l'intera pagina: può essere un
  effetto della simulazione. L'unità 05 la **indaga**, non la corregge per forza — e se conclude
  che era la simulazione, quello è un esito legittimo, purché porti la riga che lo dimostra.
- **La voce 13 non si chiude in questo goal e lo so già.** Il banner di aggiornamento in
  condizioni vere si vede solo sul sito pubblicato. Andrà nel rapporto finale come **rinviata al
  primo rilascio**, con scritta la prova da eseguire — non come «coperta».

## FATTI OPERATIVI CHE COSTANO CARI SE DIMENTICATI

Ereditati dal piano precedente, tutti ancora validi. Il dettaglio sta in `handoff/server.md`.

- **Riavvia il server prima di ogni prova nel browser**, e non far compilare nessuno mentre è
  vivo. Il DevServer legge i manifest degli asset solo al proprio avvio: dopo qualche build
  annuncia nomi con impronta che non esistono più, e l'app non parte.
- **Il server lo avvia e lo ferma il capo**, annotando porta e PID **su disco**. Su Windows la
  morte del padre non uccide i figli: `dotnet run` lascia un DevServer vivo, e il giro dopo si
  collega a una build vecchia ancora in ascolto riportando un esito falso.
- **Gli implementer non compilano.** `obj/` non ha lock fra processi: due build in parallelo si
  corrompono a vicenda. Compila il capo, a fine giro.
- **Il browser giusto ha `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.** Due Chrome sono
  collegati e i nomi si scambiano a ogni riconnessione: si identifica per `deviceId`, e solo
  quello vede `localhost`.
- **La cache della PWA non falsa le prove in sviluppo** — il service worker di dev è un no-op
  verificato — e il banner «versione nuova» che riappare lì **non è un difetto**.
