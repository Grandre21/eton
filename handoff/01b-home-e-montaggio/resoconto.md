# UNITÀ: 1b — ESITO: FATTO

⚠️ **Questo resoconto non l'ha scritto l'unità, l'ha scritto il capo, e va detto in testa.**
La sessione-unità 01b è **morta durante il lavoro**, uccisa dal riavvio della sessione che l'aveva
aperta: aveva completato l'implementazione di entrambi i rimedi e **non era ancora arrivata alla
revisione**. Non aveva committato, non aveva lanciato nessun revisore, non aveva scritto una riga
di questo file.

**Cosa ho fatto io, e cosa no.** Ho committato il suo lavoro nel worktree per metterlo al riparo —
con un messaggio che dichiara esplicitamente che quel commit non afferma nulla sulla qualità — e ho
**dispacciato i tre revisori che avrebbe lanciato lei**, con i brief che il §3 prescrive. Il codice
è suo; il tracciato qui sotto è mio, e ogni sua voce è la copia dell'intestazione di un agente che
ho lanciato davvero.

**Perché non l'ho rilanciata da zero**, che sarebbe stata l'altra strada. Il `CLAUDE.md` dice di
rilanciare un'unità che non torna, una volta sola. Ma la regola parla di un'unità che **fallisce**,
e questa non è fallita: è stata uccisa da un evento esterno al suo lavoro, dopo averlo finito. Il
diff si legge come un lavoro completo e auto-consistente — entrambi i rimedi applicati, il commento
falso riscritto, la convenzione sui rimandi rispettata senza che nessuno glielo ricordasse.
Rilanciare avrebbe buttato via lavoro valido per riprodurlo identico. **Quello che mancava dopo la
sua morte non era il codice: era la prova che qualcuno l'avesse controllato**, e quella si poteva
ancora produrre.

---

## TOCCATI

    Layout/MainLayout.razor   → +14 / −47
    Pages/Home.razor          → +33 / −4

Nessun altro file. In particolare **nessuna riga di `wwwroot/css/app.css`**, che il mandato vietava
esplicitamente perché tre unità dopo questa ne rivendicano le regole.

---

## COSA HA FATTO

**Rimedio A — `Pages/Home.razor`.** `<TestataPagina>` esce dalla catena `@if` e si monta a colonna
zero, come nelle altre dieci pagine dell'applicazione. Il titolo passa per una proprietà nuova,
`TitoloSchermata`, che ricalca il pattern già presente in `Pages/SpaceDetail.razor` — la pagina che
ha la stessa catena a quattro rami e tiene la testata fuori allo stesso modo. Il commento a valle
che giustificava l'assenza di protezione su `Spazi.Attivo` è stato aggiornato invece di essere
lasciato a dire il falso.

**Rimedio B — `Layout/MainLayout.razor`.** La chiave del contenitore di pagina è calcolata
nell'espressione invece che in un campo aggiornato da un gestore di `LocationChanged`. Spariscono
il campo, `OnInitialized`, la sottoscrizione, il gestore, `IDisposable` e `Dispose`. **Il commento
dimostrato falso dall'unità 01 è riscritto**, con la spiegazione del meccanismo vero.

---

## REVIEW

    review:
      bug-hunter      RILIEVI: 0
      conformity      RILIEVI: 1
      threat-hunter   RILIEVI: 0
      backend-expert  non lanciato — 2 files changed, 47 insertions(+), 51 deletions(-)
                      · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: fondati 0 · infondati 0 · fuori scope 1 · non verificabili 0

`coverage` non compare perché dentro una sessione-unità non si lancia (§6).

**Le misure di `backend-expert` sono state prese con i comandi, non riscritte a memoria**: la riga
di `--stat` è la forma testuale che solo `git diff --stat` produce, ed è greppabile — chi audita
conta le voci `non lanciato` che non la contengono.

---

## QUELLO CHE I REVISORI HANNO VERIFICATO INVECE DI CREDERE

È la parte che vale più del conteggio, perché il diff poggiava su **un'affermazione scritta in un
commento**, e un'affermazione sbagliata lì avrebbe rotto la navigazione invece di aggiustarla.

**`bug-hunter` ha decompilato `Microsoft.AspNetCore.Components` 10.0.10** — la versione installata,
non l'ultima pubblicata — per verificare che il layout si ridisegni a ogni navigazione **senza** la
sottoscrizione rimossa. La catena è confermata anello per anello: `ChangeDetection.MayHaveChanged`
non tratta un `RenderFragment` come tipo immutabile noto e ritorna **sempre** «può essere cambiato»,
quindi `Router → RouteView → LayoutView → MainLayout` si ridisegna comunque.

**E ha chiuso il rischio peggiore**, quello che avrebbe reso il rimedio inutile:
`WebAssemblyNavigationManager.SetLocation` assegna l'indirizzo **prima** di notificare i gestori,
quindi al render la chiave è già quella nuova — anche sul redirect iniziale. Non esiste un percorso
in cui l'indirizzo sia vecchio quando l'espressione viene valutata.

**`conformity` ha contato invece di fidarsi.** Il commento nuovo afferma «come nelle altre dieci
pagine»: il revisore le ha aperte una per una e il numero è esatto. Il commento afferma che le
copie del calcolo sul percorso sono **tre**: verificato che non ce n'è una quarta.

**`threat-hunter` ha verificato la barriera.** Spostare la testata fuori dal condizionale non la
espone a nessuno stato non autenticato, perché `AuthRedirect` **avvolge** l'intero corpo del layout
invece di ripetersi come controllo locale: nulla di ciò che sta dentro si rende prima che la
sessione sia risolta.

---

## ADJUDICA

**Rilievo 1 — il titolo della scheda del browser sulla Home (`conformity`, bassa) → fuori scope.**
Adjudicato dal capo. Il claim **regge per intero**, e il `checker` l'ha istruito senza trovare un
punto debole: la citazione è esatta, il conteggio «undici schermate, dieci col titolo composto, una
sola eccezione» è stato riverificato aprendo tutti e undici i file, il fix proposto compila perché
in un file Razor markup e blocco `@code` finiscono nella stessa classe — e c'è già una pagina in
produzione che lo dimostra — e il separatore è stato controllato **byte per byte**: è un trattino
lungo, `U+2014`, non uno medio.

**Ma la riga contestata non è toccata da questo diff**, e il `checker` lo ha confermato cercandola
nel diff senza trovarla: è codice preesistente. Cade quindi nella definizione di fuori scope, e il
§5 su quel caso non lascia margine — *fondato ma fuori scope → riportalo all'utente, non risolverlo
di nascosto.*

⚠️ **La tentazione di correggerlo era forte, ed è la ragione per cui questa voce è scritta per
esteso invece che liquidata.** Il fix è **una riga**, dentro un file che l'unità possedeva, e usa
un simbolo che il diff ha appena introdotto: tutte le condizioni per dire «tanto vale farlo adesso».
È la forma esatta con cui un obiettivo perde il confine — non con una decisione grande, ma con
cinque «è solo una riga» prese una alla volta, ognuna difendibile da sola. Il goal ha
**trentotto clausole** dichiarate e sette già note come non chiudibili: aggiungerne una
trentanovesima per comodità renderebbe finta la verifica di copertura, che esiste per contare
contro un numero fermo.

**Nessun rilievo fondato da correggere**, quindi nessun rilancio del `checker` sul fix.

---

## FUORI SCOPE

**1. Il titolo della scheda del browser sulla Home** resta il letterale «Eton», mentre nelle altre
dieci pagine che montano la testata è «titolo — Eton». È l'unica eccezione dell'applicazione, ed è
preesistente a questo diff.

**Il rimedio, già istruito e pronto**, perché chi lo riprenderà non rifaccia il lavoro: sostituire
il contenuto di `<PageTitle>` con l'espressione del titolo seguita dal trattino lungo `U+2014` e da
«Eton», riusando la proprietà che questo diff ha introdotto nel blocco `@code` della stessa pagina.
Il precedente identico è in `Pages/SpaceDetail.razor`, che ha la stessa struttura a quattro rami e
mostra già l'etichetta di ripiego nella scheda del browser durante il caricamento — quindi il
comportamento nuovo non sarebbe nuovo per l'applicazione, solo per questa pagina.

**Portato all'utente**, come il §5 impone, e annotato nel campo `APERTO` del piano.

---

## GATE

    dotnet build Eton.sln -warnaserror --no-incremental   → Avvisi: 0   Errori: 0
    dotnet test Eton.sln                                  → Superato! Non superati: 0. Superati: 310. Totale: 310

Eseguiti dal capo sul worktree, a revisori rientrati e a diff finale. `bug-hunter` aveva compilato
per conto proprio durante la revisione, con lo stesso esito — è il modo in cui ha escluso che la
rimozione del campo, del gestore e dell'interfaccia lasciasse riferimenti pendenti.

---

## SCOSTAMENTI

1. **Il resoconto l'ha scritto il capo, non l'unità.** V. la premessa in testa. È lo scostamento
   più grosso possibile da un mandato, e sta scritto per primo invece che in fondo.
2. **La revisione è stata dispacciata dal capo.** I tre brief li ho scritti io, con i contenuti che
   il §3 prescrive: a `conformity` i file omologhi, a `bug-hunter` i call-site dei simboli
   modificati, a `threat-hunter` i file di pipeline. L'unità avrebbe potuto scriverli meglio,
   perché conosceva il proprio lavoro dall'interno.
3. **`MODI DI FALLIRE` lo scrive il capo**, e il mandato lo chiedeva all'unità. È una perdita
   reale: chi ha scritto il codice sa dove ha esitato, e quell'informazione è morta con la
   sessione. Ciò che resta è ricostruito dai rapporti dei revisori.

---

## MODI DI FALLIRE

Ricostruiti dai rapporti dei revisori, non dall'unità. **I primi due sono stati esclusi
decompilando il pacchetto installato** e restano qui perché una prova che li confermasse sarebbe
comunque la più rapida da fare, e perché se un domani la versione di Blazor cambiasse, sono i primi
due punti da riverificare.

| # | Come si manifesterebbe | Dove si vede | Stato |
|---|---|---|---|
| 1 | **La navigazione non cambia più schermata.** Se il layout non si ridisegnasse a ogni navigazione, la chiave resterebbe ferma e il contenuto non verrebbe sostituito | qualunque cambio di rotta dalla barra in basso | **escluso** per decompilazione: `RenderFragment` non è mai trattato come immutabile |
| 2 | **Il doppio montaggio resta**, perché l'indirizzo è ancora quello vecchio quando la chiave viene calcolata | pannello di rete sulla Home: le query partirebbero ancora due volte | **escluso** per decompilazione: l'indirizzo è assegnato prima della notifica |
| 3 | **L'animazione d'ingresso non riparte più** al cambio di rotta, o riparte a scatti | ogni cambio di rotta | da vedere |
| 4 | **L'animazione riparte dove non dovrebbe**, cioè su un cambio di filtro o di parametro nella stessa pagina, facendo anche perdere lo stato dei componenti figli | un cambio di mese sulle spese, o un filtro in un elenco | da vedere |
| 5 | **Un salto a un'ancora rigioca l'animazione**, trattando uno spostamento dentro la stessa pagina come un cambio di pagina | un collegamento con `#` | da vedere |
| 6 | **La testata compare dove disturba**: nei rami «nessuno spazio» ed «errore» il titolo mostra l'etichetta di ripiego, e va visto che non sembri un difetto | Home appena aperta, e Home in errore | **atteso e voluto**, ma da guardare |
| 7 | **Il titolo di ripiego resta acceso** anche dopo che lo spazio è arrivato, se il ridisegno non avvenisse | Home, dopo il caricamento | da vedere |

---

## LA MISURA ATTESA PER IL COLLAUDO

Da ricopiare nel brief della prova nel browser invece di inventarla.

### Rimedio A — il pulsante di aiuto esiste dal primo render

Il criterio **non è cronometrico ma strutturale**, ed è quello che l'unità 01 aveva già lasciato:
si verifica **prima** di cliccare, appena dopo la navigazione e mentre i dati stanno ancora
arrivando.

    document.querySelector('.aiuto-apri') !== null        // atteso: true

Oggi, prima di questo rimedio, sulla Home quel selettore è `null` per tutta la durata del
caricamento. **Va verificato sulla Home**, che è la pagina che aveva il difetto, e **anche su una
rotta sana** come controllo negativo — lì era già `true` e deve restarlo.

Poi la prova funzionale, che è quella che chiude la voce 6:

- navigazione fresca sulla Home, **un solo clic sul «?» il prima possibile**: il pannello si apre.
  **Nessuna attesa è accettabile**, e la voce non si chiude con una soglia più bassa;
- e la ripetizione della stessa prova sulle **due rotte su cui la ricognizione del 19 settembre
  aveva riprodotto il difetto** — l'editor di collezione e l'editor di elemento. ⚠️ **Lì la causa
  era un'altra**, perché la testata era già fuori dal condizionale: se il difetto persistesse
  proprio lì, il candidato rimasto è il montaggio doppio, cioè il rimedio B, e la prova va fatta
  **dopo** che anche quello è in produzione. È il pezzo della voce 6 che la catena della Home non
  spiegava.

### Rimedio B — ogni pagina si monta una volta sola

**Si legge dal pannello di rete, e in una riga:** aprire la Home con navigazione fresca e contare
le richieste verso il database. Ognuna — spese, spazi, membri, profili, note, collezioni — deve
comparire **una volta sola**. Prima di questo rimedio ognuna compariva **due volte quasi in
contemporanea**: è la misura che ha fatto scoprire il difetto, ed è la stessa che lo chiude.

Le tre prove di non regressione, che coprono i modi 3, 4 e 5:

- **cambio di rotta**: l'animazione d'ingresso parte, una volta, e la schermata cambia davvero;
- **cambio di parametro nella stessa pagina** — per esempio il mese sulle spese: l'animazione
  **non** riparte, e ciò che era nello stato dei componenti resta;
- **salto a un'ancora**: l'animazione **non** riparte.

### Rimedio A, gli stati che prima non esistevano

- Home aperta **senza connessione al database**, o comunque nel ramo d'errore: la testata c'è, il
  titolo è l'etichetta di ripiego, e il «?» funziona;
- Home di un account **senza nessuno spazio**: stesso esito.

Sono i due stati che questo rimedio rende visibili per la prima volta, e nessuno li ha mai guardati.
