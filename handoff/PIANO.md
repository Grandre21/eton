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

**Ventuno clausole all'apertura. Ventisette alla sera del 19 settembre.** La colonna «voci» della
`PARTIZIONE` dice dove cade ognuna: è la mappa che la sessione di chiusura userà per contare
coperte e scoperte, e l'unica cosa di questo file che non è ricostruibile leggendo i resoconti.

⚠️ **L'obiettivo è cresciuto quattro volte, e ogni crescita è datata invece di essere assorbita in
silenzio.** Il tracciato, per chi conta a fine goal:

| Quando | Da | A | Cosa è entrato | Chi ha deciso |
|---|---|---|---|---|
| apertura | — | **21** | le 18 voci del `FUORI SCOPE` precedente, il medaglione, i profili congelati, la ricognizione mai fatta | l'obiettivo dell'utente |
| dopo la 01 | 21 | **24** | fusione delle pastiglie, `.btn.compatto` sulle frecce, rimandi ancorati al selettore → unità **07** | **l'utente**, secondo giro di domande |
| dopo la 02 | 24 | **25** | la corsa critica in `RecensioniElemento` → unità **03** | io |
| dopo la 03 | 25 | **26** | `Sovrascrivi()` che non controlla il nome → unità **04** | io |
| dopo la 04 | 26 | **27** | il `<label>` senza controllo → unità **07** | io |

**Chi conta a fine goal conta ventisette**, e le quattro crescite hanno tutte la stessa forma:
difetto reale trovato da un'unità, rimedio di poche righe, file già dentro un perimetro assegnato.
**Dalla quinta in poi non si cresce più**: la regola che mi sono dato sta in `DECISIONI`, alla voce
del `<label>`.

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
  incontrano mai (`app.css:2436` le spegne entrambe dove `.voce-piede` si accende).
  ⚠️ *Qui c'era scritto `:2269`, ed era già scaduto quando l'ho scritto: le 177 inserzioni
  dell'unità 01 avevano spostato quella regola di 167 righe. L'ha misurato l'unità 02. È la prima
  ricaduta della classe di difetto che l'unità 01 aveva segnalato — un rimando per numero che
  nessuno risincronizza — e capita dentro il documento che decide come correggerla.*
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

- **19 set 2026, secondo giro di domande** — **L'utente ha risposto alle quattro di `APERTO`, e
  l'obiettivo si allarga di una unità.** Tutte e quattro le raccomandazioni di `tech-advisor` sono
  state accettate:

  1. **Le tre regole di pastiglia si fondono in `button.pastiglia`.** `tech-advisor` dissente dal
     motivo per cui l'unità 01 l'aveva rinviata, e il dissenso è la parte utile: **la resa non
     cambia su nessuna schermata**, è un refactor a effetto visivo zero. La cascata è verificata —
     `.pastiglia` (0-1-0) non dichiara `min-height`, `.pastiglia.accesa` (0-2-0) dichiara solo
     colori, quindi `button.pastiglia` (0-1-1) vince su tutte e tre le proprietà e nessuna
     variante di stato lo scavalca. Bilancio: −8 righe di CSS, ~25 di commento da riscrivere.
     **Ciò che cambia è la policy futura**, non il presente: una pastiglia-bottone nuova in una
     riga di elenco prenderebbe 48px da sola — che è l'esito giusto per un bersaglio, ed è ciò che
     l'unità 01 ha appena fatto a mano nel voto.
  2. **Le frecce di mese portano `.btn.compatto` nel markup**, con
     `.navigazione-mese .btn { min-width: var(--tocco); }` — una proprietà invece di due.
     `.btn.compatto` non è teoria: ha **due call-site vivi** nello stesso ruolo di testata.
     Il raggio passa da 8 a 12px e **non è un costo**: 12 è il raggio di ogni `.btn`, e l'8 esiste
     solo in `.btn.piccolo`, dove serviva a una scatola da 35px.
  3. **I rimandi si ancorano al selettore, cross-file compresi.** `tech-advisor` dissente dalla
     cornice che avevo proposto — «numerici solo verso altri file» — e il rovesciamento regge:
     proprio perché nessuno apre `app.css` quando cambia un `.razor`, **quei numeri scadono più in
     fretta**, non meno. Il file ha già la forma giusta in un punto: file più token cercabile.
     ⚠️ **Gli scaduti sono quattro, non tre.** Il quarto l'unità 01 l'aveva mancato perché cercava
     solo `v. riga`: uno dei rimandi usa la forma «il commento a riga N». **Uno su tre è falso.**
  4. **Entrano in questo goal**, come unità in coda prima del collaudo, così la prova visiva le
     copre tutte in un giro solo di browser. **Conseguenza dichiarata:** l'obiettivo si allarga
     da 21 a 24 clausole, e le tre nuove sono nate *durante* il lavoro. Lo scrivo qui perché la
     verifica di copertura della chiusura conta le clausole dell'`OBIETTIVO`, e un obiettivo che
     cresce senza lasciare traccia è esattamente ciò che rende quella verifica impossibile.

- **19 set 2026** — **L'allarme sui due plugin era un falso positivo, e il motivo vale più della
  smentita.** `tech-advisor` ha confrontato lo snapshot approvato dell'11 settembre — ancora in
  cache, marcato orfano — con quello attuale: **`diff -r` esce vuoto su entrambi i plugin**,
  `SKILL.md` 71 righe in tutte e due le copie. Il `PIANO-DESIGN` del medaglione viene da un file
  byte-identico a quello approvato. **Nessuno dei due ha hook**: sono prosa — una skill e un
  comando — quindi la classe di rischio è injection, non esecuzione.
  **Perché l'allarme è scattato:** nessuno dei due `plugin.json` dichiara `version`, quindi Claude
  Code usa lo SHA del commit; e il marketplace è un **monorepo**, dove ogni commit su un plugin
  qualunque re-installa anche questi due sotto uno SHA nuovo e muove `lastUpdated`.
  **La conseguenza per l'impianto, che è la parte da non perdere:** per un plugin senza `version`,
  `installedAt ≠ lastUpdated` **è rumore, non prova**. Il controllo che il `CLAUDE.md` affida a
  `config-critic` va integrato con un `diff -r` contro lo snapshot orfano — che però resta in
  cache solo fino allo sweep, quindi il confronto si fa **alla prima sessione dopo
  l'aggiornamento** o non si fa più.
  Entrambi restano abilitati: non c'è niente da rileggere.

- **19 set 2026, dopo l'unità 02** — **Una corsa critica vera, trovata in un file che l'unità non
  possedeva: entra nel mandato della 03.** Deciso da me. `Shared/RecensioniElemento.razor:294`
  scrive `caricato = true;` dopo un `await` e **senza la guardia di generazione**, mentre nello
  stesso file `occupato` ce l'ha in entrambi i rilasci e lo stesso `caricato` ce l'ha nell'altro
  punto. Lo scenario è costruibile e **il file lo dichiara da sé**: il suo commento avverte che
  due caricamenti possono sovrapporsi, perché navigando fra due elementi il router riusa
  l'istanza (nessun `@key` sul call-site). Una generazione sorpassata può quindi dichiarare il
  componente «caricato» sopra campi appena azzerati.
  È stato riaperto **per due vie indipendenti** — dall'unità e dal `checker` — con la stessa
  conclusione.
  **Perché entra qui e non nel prossimo goal:** il file è **già** nel perimetro dell'unità 03, il
  rimedio è una riga, ed è la stessa guardia che il file usa già tre volte. Non allarga nessun
  perimetro e non apre nessuna decisione di progetto. **L'obiettivo passa a 25 clausole**, ed è la
  seconda volta che cresce: come la prima, il numero è scritto con la data invece di essere
  aggiornato in silenzio.

- **19 set 2026, dopo l'unità 03** — **Tre decisioni sui suoi `FUORI SCOPE`, prese da me.**

  1. **Il riquadro di `CollectionEdit` che si spegne con un errore vero rimasto: resta così, e la
     decisione è mia, non dell'unità.** Il `checker` ha dichiarato quel sub-claim `non risolto`, e
     l'unità l'ha **ricopiato invece di ammorbidirlo** — un commento non risolve un comportamento.
     Confermo la scelta di non correggere, per tre fatti che l'unità ha già istruito: gli errori di
     validazione sono stringhe interpolate **senza legame strutturale col campo**, e uno di essi non
     ne nomina nessuno, quindi filtrare sarebbe un confronto di sottostringhe — una forma nuova e
     fragile; il pulsante «Salva» **resta acceso** nello scenario, e un clic ridà il verdetto
     completo; e il progetto ha già un principio scritto altrove, *fra un'affermazione falsa e una
     mancante scegli la seconda*. Il prezzo ora è **scritto nel file**, che era il punto: un
     trade-off muto è indistinguibile da una svista.
  2. **`Sovrascrivi()` di `ItemEdit` non controlla il nome: entra nel mandato dell'unità 04.**
     Difetto **preesistente** e non peggiorato dal diff — anzi lievemente migliorato. Il percorso è
     costruibile: l'input del nome non è disabilitato dalla scheda di conflitto, il pulsante
     «Sovrascrivi» è spento solo dal permesso, e `Sovrascrivi()` non passa da `Salva()`. Il nome
     vuoto arriva al database, che lo respinge col vincolo `check`, e l'utente legge un messaggio
     generico. **L'omologo che chiude il buco esiste già in `SpesaEdit`**, col commento che spiega
     perché lì un `return` muto non basterebbe: il rimedio è tre righe ricalcate da quelle.
     Entra nella 04 perché quel file è già nel suo perimetro. **L'obiettivo passa a 26 clausole**,
     terza crescita, di nuovo scritta con la data.
  3. **Il residuo di `SpesaEdit` non si tocca.** È l'ultimo membro della famiglia del punto 3, ma a
     coprirlo c'è una **decisione scritta e motivata** nel file — «un return silenzioso lascerebbe
     il pulsante sembrare inerte». La distinzione che l'unità ha fatto è quella giusta e la adotto:
     dove l'argomento è di **costo** è revocabile (ed è stato revocato, nella voce 6); dove è di
     **principio**, revocarlo è una decisione di progetto e non conformità.

- **19 set 2026, dopo l'unità 04** — **Un `<label>` che non etichetta più niente: entra nella 07,
  ed è l'ultima aggiunta che accetto in questo goal.** Deciso da me. Il ramo di sola lettura che
  la voce 4 ha introdotto lascia un `<label class="campo">` che contiene un `<p>` invece di un
  `<input>`: non è invalido e il testo resta leggibile, ma un `<label>` senza controllo associato
  non fa il proprio mestiere per chi ascolta la pagina. L'unità l'ha **dichiarato invece di
  tacerlo**, e ha spiegato perché non l'ha fatto: rendere condizionale il contenitore le avrebbe
  fatto duplicare l'etichetta in entrambi i rami, e il suo mandato vietava di allargarsi sul campo.
  Il rimedio è **due righe** — un `<div class="campo">` nel ramo che non ha controlli — e cade
  bene nella 07, che è l'unica unità rimasta a toccare la presentazione.
  **L'obiettivo passa a 27 clausole, quarta crescita.**

  ⚠️ **E qui mi fermo, con una regola che mi do per iscritto perché non si veda solo a posteriori:
  da questo punto in poi le voci nuove che le unità aprono vanno nel rapporto finale come candidate
  per il goal successivo, non nel goal corrente.** Quattro crescite sono il limite oltre il quale
  «chiudere i punti rimanenti» smette di avere un confine e diventa un lavoro che non finisce: la
  verifica di copertura esiste per contare contro un numero, e un numero che si muove a ogni
  resoconto non è un numero. Le quattro accettate hanno tutte la stessa forma — difetto reale,
  rimedio di poche righe, file già dentro un perimetro assegnato — e la prossima che avrà quella
  forma andrà comunque nel rapporto.

- **19 set 2026** — **Le altre due osservazioni dell'unità 04 restano come sono, e le confermo.**
  Il pulsante «Sovrascrivi» di `ItemEdit` resta spento dal solo permesso e non anche dalla validità
  del nome: l'unità ha applicato **una** delle due protezioni dell'omologo, che è quella che chiude
  il buco — il nome vuoto non raggiunge più il database — e l'altra avrebbe richiesto di toccare un
  markup che il suo `NON TOCCARE` proteggeva. Con la sola guardia nel metodo, per giunta, il
  messaggio d'errore **serve davvero**, mentre nell'omologo è difensivo. E il messaggio che
  sopravvive alla correzione del campo è la stessa famiglia chiusa dall'unità 03 su `SpesaEdit`,
  dove a tenerla c'è una decisione di **principio**: riaprirla qui sarebbe stato deciderla da capo.

- **19 set 2026, dopo l'unità 05** — **Il mio mandato diceva «tutte e quattro» e si sbagliava sulla
  quarta: l'unità ha fatto l'opposto, e aveva ragione.** Lo registro qui perché è una deviazione
  sostanziale da un mandato, non una sfumatura.
  Il mandato ordinava di proteggere **quattro** chiamate a `localStorage`, inclusa quella
  nell'avvio dell'accesso con Google che «non ha alcun `try` in tutta la sua lunghezza». Vero — ma
  quel metodo ha **un solo call-site in tutto il progetto**, e lì è invocato dentro un `try/catch`
  che già scrive in console, già rimette il pulsante premibile, e già mostra una frase **mirata
  proprio a quel caso**: «può essere il browser che non lascia salvare i dati di questo sito —
  succede con la navigazione anonima…».
  **Proteggerla sarebbe stato un peggioramento misurabile:** avrebbe reso morto quel `catch`,
  l'applicazione sarebbe partita verso Google con un verificatore mai salvato, e l'utente avrebbe
  ricevuto al ritorno un messaggio generico invece della frase mirata che oggi riceve **subito**.
  Il precedente che il mandato stesso indicava conferma la lettura: quel servizio protegge la
  lettura e lascia scoperte scrittura e cancellazione. **Non dice «proteggi tutto»: dice «proteggi
  dove esiste un ripiego onesto».** Per la lettura il ripiego è `null`; per la cancellazione è
  «niente», e il valore è monouso; per il salvataggio non esiste, e l'unica cosa giusta è non
  partire.
  L'unità ha portato il bivio a `tech-advisor` **prima** di scrivere i brief, con le tre opzioni in
  chiaro e l'invito a smentirla, e ha eseguito la condizione che lui ha aggiunto: un commento che
  dichiara **dove** vive la copertura, così che il prossimo lettore non «uniformi» i tre metodi
  spegnendo la frase. Senza quel commento la regressione sarebbe stata a un passo, e sarebbe
  sembrata una pulizia.

- **19 set 2026** — **La voce 9, la barra gialla di Blazor: istruita, non determinata, e resta
  aperta.** Il mandato ammetteva tre forme di chiusura e l'unità ha usato la terza — «non
  determinato, e ho escluso …» — che è la sola onesta quando le prove non bastano.
  **Cosa ha escluso, con la riga per ognuna:** i percorsi di salvataggio delle tre schermate in cui
  la barra è comparsa, tutti dentro `try/catch` e tutti produttori del messaggio italiano osservato;
  il metodo comune ai 52 call-site, che non propaga eccezioni di rete; l'altro servizio che tocca
  `localStorage`, protetto su tutte e tre le chiamate; il **refresh automatico di Gotrue**, che era
  l'ipotesi più forte e che `doc-checker` ha smentito **sul sorgente della versione installata** —
  il timer esiste e il suo handler avvolge tutto in un `catch` che non rilancia mai; il service
  worker; e i componenti condivisi montati su ogni pagina privata.
  ⚠️ **E ha smentito la voce stessa**: la barra **non è mai stata osservata sul percorso d'accesso**
  — le prove OAuth del giro C stanno fra i «non provato», perché richiedevano di essere disconnessi.
  Quindi il punto 1 non poteva farla sparire, contrariamente a quanto la voce ipotizzava.
  **Ciò che non è escludibile** è la forma esatta della sovrascrittura di `window.fetch` usata nella
  simulazione: se sostituiva `fetch` con una funzione che **lancia sincronamente** invece di
  restituire una Promise rifiutata, il punto di fallimento si sposta dentro il marshalling di Blazor
  e può non essere catturabile dal `catch` C#. Senza il codice della simulazione l'ipotesi non è né
  confermabile né escludibile. **Va nel rapporto finale come clausola non chiusa, con il motivo.**

## PARTIZIONE

Sei unità, **in sequenza, mai in parallelo**. La colonna «voci» usa la numerazione del
`FUORI SCOPE — aggregato` di `storico/handoff/CHIUSURA.md`.

| Unità | Perimetro | Voci | Dipende da | Stato |
|---|---|---|---|---|
| **01 foglio-di-stile** | `wwwroot/css/app.css` | 1, 3, 5, la quota CSS della 2, **P2** | — | **FATTO** — integrata su `main` con `c50981a`, worktree e branch rimossi |
| **02 barra-e-home** | `Shared/Navigazione.razor`, `Pages/Home.razor` (**solo `@code`**) | 2 (markup), 15 | 01 | **FATTO** — integrata con `ab33d92`, pushata, worktree rimosso |
| **03 editor-esiti** | `Pages/CollectionEdit.razor`, `Pages/ItemEdit.razor`, `Pages/NoteEdit.razor`, `Pages/SpesaEdit.razor`, `Shared/RecensioniElemento.razor` — **`Shared/PaginaEditor.cs` NON si tocca** | 4 (editor elemento), 6, 10, **+ la corsa critica trovata dalla 02** | 02 | **FATTO** — integrata con `e20a059`, pushata, worktree e branch remoto rimossi |
| **04 igiene-e-importi** | `Pages/CollectionDetail.razor`, `Services/SchemaCampi.cs`, `Services/Denaro.cs`, `Eton.Tests/SchemaCampiTests.cs`, + le righe residue di `CollectionEdit`/`ItemEdit`/`SpesaEdit` | 11, 14, 16, 17, **+ `Sovrascrivi()` che non controlla il nome** | 03 | **FATTO** — integrata con `6ce6ee3`, pushata, worktree rimosso. **288 test** |
| **05 accesso** | `Services/SupabaseService.cs`, `Services/OAuthCallback.cs`, `Services/PkceStore.cs`, `Eton.Tests/OAuthCallbackTests.cs` | 7, 8, 18 · **9 istruita e non determinata** | 04 | **FATTO** — integrata con `8804763`, pushata. **290 test** |
| **06 profilo-allineato** | `Services/AuthStateService.cs`, nuovo `Services/ProfileRepository.cs`, un call-site in `Services/SupabaseService.cs` | il difetto da `APERTO` | 05 | PIANIFICATA |
| **07 pastiglie-e-ancore** | `wwwroot/css/app.css`, `Pages/Spese.razor`, **+ due righe di `Pages/SpesaEdit.razor`** | le **tre clausole nuove** del 19 set: fusione delle pastiglie, `.btn.compatto` sulle frecce, i rimandi ancorati al selettore, **+ il `<label>` senza controllo** | 06 | PIANIFICATA |

⚠️ **L'unità 07 revoca un contratto dell'unità 01**, e va detto invece di lasciarlo scoprire: il
mandato della 01 diceva «`app.css`, tutto il file, e sei l'unico a toccarlo in tutto il goal».
Non è più vero — l'utente ha aggiunto tre clausole il 19 settembre. La condizione che rendeva
sicura quella frase **regge comunque**: la 01 è rientrata e integrata, le unità 02-06 non toccano
il foglio, quindi la 07 lo trova fermo e ne è l'unica proprietaria dal suo turno in poi. Ciò che
cambia non è la sicurezza, è che un lettore del mandato 01 non deve credere che quel file sia
chiuso per sempre.

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

PROSSIMA AZIONE: aprire l'unità **06 profilo-allineato**, l'ultima delle sei pianificate, mandato scritto, committato e pushato. Poi la **07**, poi il collaudo nel browser.

Le unità 01, 02 e 03 sono rientrate `FATTO`, auditate e integrate. Tutti i contratti convergono, e
`Shared/PaginaEditor.cs` è uscito dal goal **senza essere stato aperto in scrittura da nessuno**,
come tutte e tre le unità dichiarano.

**La misura che il collaudo deve ritrovare a zero:** la sovrapposizione di «Profilo» sul selettore
di spazio. La 01 l'ha portata da 8,5px a 2,5px per lato allargando la scatola; la 02 l'ha chiusa
spostando l'etichetta a `.solo-lettori`, così il contenuto del link è la sola icona da 22px dentro
48. Il collaudo verifica `scrollWidth === clientWidth`, `svg.left − select.right ≥ 8`, l'elemento
alto 48 — **e che il nome accessibile del collegamento sia ancora «Profilo»**, che è la sola
misura il cui fallimento avrebbe scambiato un difetto visibile con uno invisibile.

⚠️ **Sequenziale, mai in parallelo.** Nessuna unità si apre finché la precedente non è rientrata,
anche quando i perimetri sembrano disgiunti: le unità 03 e 04 si passano tre file, e la 05 e la 06
se ne passano uno.

## APERTO

**Domande per l'utente — nessuna aperta.** Le quattro elencate qui sotto sono state **poste e
risposte il 19 settembre sera**, su richiesta dell'utente («facciamo il round di domande»), e le
risposte stanno in `DECISIONI`: fondere sì, `.btn.compatto` sì, ancorare al selettore sì compresi
i cross-file, e tutte e tre **dentro questo goal** come unità 07.

L'elenco resta qui perché contiene il **merito** di ognuna, che le righe di `DECISIONI` non
ripetono per esteso.

1. **`TIPO: progetto` — le tre regole di pastiglia andrebbero fuse in `button.pastiglia`.**
   Fondato nel merito, e il fatto che lo regge è stato riverificato dall'unità: i
   `<button class="pastiglia">` sono **sei**, tutti dentro uno dei tre contenitori; i quattro
   `<span class="pastiglia">` stanno tutti in righe di elenco e nessuno verrebbe agganciato.
   Sostituirebbe un selettore **di luogo** con uno **di condizione semantica** ed eliminerebbe due
   regole. Non fatto perché tocca regole che il mandato proteggeva e cambia il rendering di
   schermate fuori dai cinque rilievi: è la decisione giusta, ma non dentro un'unità di debito.
2. **`TIPO: progetto` — le frecce di mese dovrebbero portare `.btn.compatto` nel markup.** Oggi
   il CSS dice «qui `.piccolo` non è piccolo», e chi fra sei mesi legge `class="btn piccolo"`
   trova un'esenzione che una regola contraddice tre schermate più in basso. **Il difetto misurato
   è chiuso** (l'unità l'ha risolto dal foglio); questa è la strada più pulita, passa dal `.razor`
   e ha un effetto visivo da decidere — `.compatto` non porta `border-radius: var(--raggio-s)`.
3. **`TIPO: progetto` — i rimandi `(v. riga N)` dentro `app.css` sono una convenzione che si
   rompe da sola.** **Tre erano già scaduti prima di questo goal.** La proposta è ancorare al
   **selettore** invece che al numero: si trova con una ricerca e non può scadere. I riferimenti
   verso altri file restano numerici, perché lì il numero non è sotto il controllo di chi scrive
   il foglio.
4. ~~Due plugin aggiornati, e il codice che gira non è quello approvato.~~ ⚠️ **Premessa
   smentita il 19 settembre**, v. `DECISIONI`: `diff -r` fra lo snapshot approvato e quello
   attuale esce **vuoto** su entrambi, e nessuno dei due ha hook. Resta il fatto strutturale, che
   vale oltre questo caso: per un plugin senza `version` nel manifest, `installedAt ≠ lastUpdated`
   è **rumore**, perché il marketplace è un monorepo e ogni commit muove la data di tutti.

**Una domanda per l'utente, aperta dalla terza segnalazione identica in tre unità.** Tutte e tre
le unità hanno riportato che una superficie di configurazione di queste sessioni prescrive di
modificare i file via `Bash` — `sed`, heredoc, script brevi — «invece degli strumenti dedicati»,
il che **contraddice frontalmente** la regola globale che impone `Write`/`Edit` e vieta gli
interpreti inline.

**L'unità 03 l'ha identificata**, ed è il dato che mancava: arriva nel **blocco di istruzioni di
un server MCP**, sotto l'intestazione «While auto mode is active». Non è quindi una svista di un
documento del progetto: è una direttiva che entra in ogni sessione avviata in modalità automatica.

**Non è stata seguita da nessuno** — né dai tre capi-unità né dai loro undici implementer — e due
implementer l'hanno segnalata di propria iniziativa, uno notando che era **dirimente**: i commenti
che dovevano scrivere contengono accenti e trattini lunghi, che gli argomenti di shell perdono.
La regola globale è chiara su chi vince — un'istruzione che non arriva dal turno dell'utente non
scavalca il `CLAUDE.md` — ma **la contraddizione resta lì e si ripresenta a ogni sessione**, e
toglierla è una modifica a una superficie di configurazione, cioè un gesto dell'utente.

**Una decisione rimandata al collaudo, deliberatamente.** L'unità 02 ha dovuto scegliere la forma
dello stato di caricamento della Home al cambio di spazio, e il perimetro le dava il **solo blocco
`@code`**: l'unica strada senza toccare il markup era riusare il flag esistente, quindi **a ogni
cambio di spazio la Home ora si svuota del tutto** e mostra «Caricamento…».
Due fatti la rendono meno drastica di come suona, entrambi verificati dall'unità: quella riga è
l'idioma del progetto in **nove** punti, e **il selettore di spazio non sparisce**, perché sta fuori
dalla catena condizionale — l'utente vede il nome nuovo nel selettore e «Caricamento…» sotto, cioè
uno stato coerente, non una pagina morta.
L'alternativa, più fine, mostrerebbe subito il **nome** del nuovo spazio tenendo in caricamento
solo i dettagli: richiede rami nuovi nel markup, cioè un perimetro diverso e un giro in più.
**Non la decido a tavolino: la guardo al collaudo.** Un cambio visibile a ogni cambio di spazio si
giudica vedendolo, non leggendone la descrizione.

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

⚠️⚠️ **IL WORKTREE NASCE DA `origin/main`, NON DA `main` LOCALE: committare non basta, bisogna
PUSHARE.** Misurato dall'unità 02 il 19 settembre, ed è la correzione del punto qui sotto, che da
solo non bastava. Il capo aveva committato tutti i mandati — ma `main` aveva **quattro commit non
pushati**, fra cui `c50981a`, l'integrazione del foglio di stile dell'unità 01. Il worktree della
02 è partito da `6b7e8b4` e **il lavoro della 01 lì dentro non c'era**.

**Come se n'è accorta, e perché è fragile:** `.solo-lettori` risultava alla riga citata dal
**mandato** e non a quella dichiarata dal **resoconto dell'unità 01** — una discrepanza fra due
documenti appena letti. Con il solo mandato in mano non avrebbe avuto modo di accorgersene, e
avrebbe lavorato contro un CSS vecchio **dichiarando contratti falsi**. Non è un rilevamento su cui
si possa contare.

**La regola, d'ora in poi:** il capo committa `PIANO.md` e i mandati, **poi pusha**, e solo dopo
apre l'unità. Il push è la metà che si dimentica, perché il commit *sembra* aver messo al sicuro il
lavoro. Il ripiego — l'unità fa da sé `git merge --ff-only main` dal worktree, previa verifica con
`git merge-base --is-ancestor HEAD main` — funziona ma dipende dal fatto che se ne accorga.

⚠️ **Le sessioni-unità si aprono un worktree per conto proprio, e il capo deve saperlo prima di
cercarne il resoconto.** Scoperto il 19 settembre sull'unità 01: la sua `cwd` in
`claude agents --json` non era più la radice ma `.claude/worktrees/01-foglio-di-stile`, e il suo
`app.css` modificato **non compariva** nel `git status` del capo. Tre conseguenze, tutte
verificate:

1. **Una sentinella sul resoconto va puntata su entrambi i path**, quello del worktree e quello
   dell'albero principale. La prima, puntata solo sul secondo, è scaduta dopo trenta minuti senza
   eventi mentre l'unità stava lavorando benissimo: il silenzio non distingueva «sta lavorando»
   da «è morta», che è esattamente ciò che una sentinella esiste per distinguere.
2. **Il worktree nasce dall'ultimo commit, non dall'albero di lavoro.** L'unità 01 è nata da
   `6b7e8b4`, dove `handoff/` conteneva ancora lo stato del **ciclo precedente**: il suo mandato
   non c'era, e i path `storico/handoff/…` che quel mandato cita lì non esistevano. Ha lavorato
   lo stesso perché il mandato l'aveva già letto prima di entrare. **Rimedio, per ogni unità
   successiva: il capo committa piano e mandati PRIMA di aprire l'unità** — fatto con `5035e0b`.
3. **L'integrazione spetta al capo**, come per la sessione di chiusura del ciclo precedente: il
   lavoro sta nel branch `worktree-NN-slug`, e il capo lo porta su `main` e poi rimuove worktree
   e branch. Qui non sarà un fast-forward, perché `main` è avanti di `5035e0b`: sarà un merge, che
   è pulito perché i perimetri non si incrociano — l'unità tocca `wwwroot/`, il capo `handoff/` e
   `docs/`.

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
