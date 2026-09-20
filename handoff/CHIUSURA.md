# CHIUSURA — il mandato

*Scritto dal capo il **20 settembre 2026**, a collaudo fatto. Il rapporto lo scrive la sessione di
chiusura, in testa a questo file, sopra questo mandato.*

⚠️ **Il rapporto del ciclo precedente stava qui e ora sta in
`storico/handoff/2026-09-19-CHIUSURA-sedici-rilievi.md`**, accanto al proprio piano. Il capo stava
per sovrascriverlo e se n'è accorto perché la scrittura è stata rifiutata: il file era cambiato
dopo l'ultima lettura. **Il path `handoff/CHIUSURA.md` è fisso per architettura, quindi due cicli
consecutivi se lo contendono** — chi apre il ciclo successivo archivi il rapporto precedente prima
di scrivere il proprio mandato, con un prefisso di data, come è stato fatto per il piano.

## CHI SEI

Sei la **sessione di chiusura** del goal «tutto ciò che rimane da chiudere». Non sei un quarto
revisore e non sei un riassuntore.

**Leggi `~/.claude/architettura-sessioni.md` come prima azione**, sezione «La sessione di chiusura»:
contiene il formato del tuo rapporto, che non conosci altrimenti, e il confine di cosa puoi
correggere.

## L'OBIETTIVO, VERBATIM

> «tutto cio che rimane da chiudere voglio che sia ciuso in questa sessione»
> — utente, 19 settembre 2026, notte

**Lo leggi da `handoff/PIANO.md`, dove sta verbatim, non da questa copia.** La glossa che lo
decompone sta lì sotto, con la tabella che dice da dove viene ognuna delle clausole.

⚠️ **Conti contro trentotto.** E ⚠️ **sette di quelle trentotto erano dichiarate non chiudibili fin
dall'apertura**, con il motivo scritto: è la differenza fra una copertura onesta e una che si
sgonfia al conteggio finale. **Verificale**: se una di quelle sette risultasse chiusa, o se una
delle altre trentuno non lo fosse, è esattamente ciò che devi trovare.

## COSA DEVI FARE, NELL'ORDINE

1. **Copertura dell'obiettivo.** Confronta le 38 clausole con l'unione di ciò che i **sei
   resoconti** dichiarano — più l'esito del collaudo in `handoff/collaudo/esito-live-testing.md` —
   ed elenca **ciò che nessuno ha fatto e nessuno ha detto di non aver fatto**. Ogni altro anello
   verifica i claim *fatti*; tu sei il solo che cerca quelli **non fatti**, e il capo — che ha
   disegnato la partizione — è il candidato peggiore a trovarne i buchi.
   Nello stesso passaggio confronta la colonna di stato della `PARTIZIONE` con gli `ESITO` reali.
   ⚠️ **Avvertenza specifica di questo goal**: la partizione è cambiata **due volte in corsa** — è
   nata un'unità `01b` che il piano iniziale non aveva, e la voce 2 è stata **spezzata in `2a` e
   `2b`** fra due unità. Il piano lo dichiara in `DECISIONI` con il motivo, e il conteggio non è
   cambiato: la voce 2 vale **una** clausola coperta, e solo se **entrambe** le metà sono chiuse.

2. **Convergenza dei contratti.** Apri il codice e verifica che produttore e consumatore siano
   atterrati sulla **stessa firma reale**, non solo che entrambi l'abbiano dichiarata:
   - **`wwwroot/css/app.css`, riscritto da quattro unità** — 01 (blocco della testata), 02 (regole
     del voto), 04 (sette punti), 05 (token e scala). **È il contratto più delicato del goal**,
     perché il perimetro non era per file ma **per regole dentro un file**: è la prima volta che
     questo progetto partiziona così. Verifica che nessuna unità abbia scritto fuori dal proprio
     blocco.
   - **L'ancora del pulsante d'aiuto**, fra 01 e 05: la 01 l'ha costruita, la 05 ha scoperto che era
     **rotta sopra i 640px** e l'ha rifatta derivandola da due token. Il collaudo l'ha misurata
     verde a **entrambe** le larghezze — verifica che le due unità siano atterrate sulla stessa
     regola e non su due che si sovrappongono.
   - **Il ciclo di vita del montaggio**, unità 01b: ogni pagina si monta ora **una volta sola**.
     Verifica che nessuna guardia contro le doppie letture sia stata tolta altrove credendola
     inutile.
   - **`AuthStateService`** e i servizi del profilo, unità 03: un parametro in più nel costruttore,
     un metodo nuovo con un solo chiamante.
   - **Gli `aria-label` in ciclo**, unità 04: verifica che siano **per riga** e non tutti uguali —
     il collaudo l'ha misurato verde, tu verificalo sul codice.
   - **`Shared/PaginaEditor.cs`** — dichiarato invariante da **sei** unità in due cicli e mai aperto
     in scrittura. Verificalo sul diff complessivo, non sui resoconti.

3. **`bug-hunter` sul diff combinato**, con i call-site di cucitura, più i gate rieseguiti sullo
   stato finale.
   ⚠️ **Dagli il mandato esplicito di ignorare i commenti.** Il ciclo precedente ne ha lanciati due
   sullo stesso diff: quello della chiusura trovò **quattro** rilievi, tre dei quali erano commenti
   falsi; quello lanciato in parallelo, con l'ordine di ignorarli e cercare solo ciò che emerge
   dalla **somma** delle unità, chiuse con **zero** verificando le cuciture una per una. I due
   esiti non si contraddicevano. **Qui interessa il secondo tipo.**

## COSA NON DEVI FARE

1. ⚠️ **Non correggere i rilievi che `ui-critic` ha prodotto al collaudo.** Destinazione dichiarata:
   la **fase 2.1-bis** del piano di prodotto. Il tuo compito su di essi è **aggregarli in
   `FUORI SCOPE`**, non chiuderli.
2. **Non correggere il difetto dei pulsanti di conferma** trovato dal collaudo — a schermo stretto
   non stanno sulla stessa riga e il distruttivo sta sopra quello di sicurezza. **L'utente ha deciso
   che va nel rapporto**, ed è in `DECISIONI`.
3. **Non avviare il server e non aprire il browser.** Il collaudo è fatto. ⚠️ **Il server è vivo**:
   i PID sono in `handoff/server.md`, e **lo ferma il capo**, non tu.
4. **Non toccare `supabase/migrations/`.** Nessuna unità l'ha toccata, e un file nuovo lì dentro
   finisce nel parser di `PrivilegiInsertTests`, che può diventare rosso senza che nessuno abbia
   toccato il test.
5. **Tutto ciò che tocca un contratto fra unità torna al capo**, e non lo risolvi tu.

## COSA PUOI CORREGGERE

Ciò che il `bug-hunter` finale trova **dentro** il perimetro già toccato, dispacciando `implementer`
col protocollo normale.

⚠️ **E una cosa in più, specifica di questo goal**: il collaudo ha dimostrato che **due resoconti
portano una misura attesa sbagliata** — il campo della data dichiarato a 48px, misurato a **71**
perché sta in un grid affiancato a un campo alto 71, dove l'altezza la decide la riga e non il
contenuto. **Correggi quei due resoconti**: sono documenti che qualcuno rileggerà, e una misura
attesa falsa in un resoconto archiviato è peggio di una misura assente. **Il codice non va toccato**:
la protezione che quella voce introduceva funziona, il corpo è 16px misurato.

## SETTE COSE CHE VANNO NEL RAPPORTO E CHE NESSUN RESOCONTO CONTIENE

1. **Il browser è rimasto disconnesso da Eton** dopo il collaudo — la prova del logout richiede di
   premere «Esci» e l'agente non aveva credenziali. L'utente ha poi rifatto l'accesso per
   `ui-critic`. Non è un difetto, è uno stato.
2. **Tre dati di prova del ciclo precedente sono ancora sul database di sviluppo**: la spesa
   «COLLAUDO GIRO B», lo spazio «COLLAUDO GIRO B TEST», la collezione «Collaudo Ricognizione».
   **Servono tre gesti dell'utente**, perché passano da dialoghi nativi. ⚠️ Il collaudo di stanotte
   ha creato **e poi eliminato** una propria collezione di prova, e ha modificato **e ripristinato**
   quattro campi su un elemento verificando il ripristino con un refresh: **non ha aggiunto
   residui**.
3. **Cinque prove sono state dichiarate non eseguibili**, in `handoff/collaudo/esito-live-testing.md`.
   ⚠️ **Una sola delle cinque è stata saltata per mancanza di tempo e non per impossibilità** — il
   pavimento di tocco della navigazione sopra i 1024px — e va riportata come tale, distinta dalle
   altre quattro.
4. **Il rilievo sull'impianto più importante del goal.** Due unità hanno scritto una misura attesa
   **che non potevano verificare**, perché il loro mandato vietava di avviare il server ed è il
   server l'unico posto dove quel grid esiste. Il calcolo era **giusto in isolamento e irrilevante
   nel contesto**. Le misure attese scritte da ogni unità sono state il **guadagno più grande** di
   questo collaudo — hanno reso il brief un atto di trascrizione invece che di invenzione — **e
   questo ne è il limite**: una misura calcolata invece che osservata è un'ipotesi travestita da
   criterio.
5. **Un secondo rilievo sull'impianto**: un passo del §4 è saltato nell'unità 04 — il `checker`
   sull'istruttoria del secondo giro di revisione — e **l'unità l'ha dichiarato**. È il **secondo
   caso in due cicli** con la stessa forma: il ciclo precedente aveva misurato che `coverage` si
   perdeva perché cadeva **dopo** il momento in cui si scrive il tracciato. **Un passo che cade fra
   due momenti in cui si scrive il tracciato tende a saltare**, e nessuna delle due volte è stata
   pigrizia.
6. **La clausola 37 è preparata ma non applicata**: i due testi stanno in
   `handoff/configurazione-da-applicare.md` e toccano `~/.claude/`, cioè una superficie di
   configurazione. **È un gesto dell'utente.** ⚠️ E va detto che **il capo stesso ha violato quella
   regola una volta**, usando `sed -i` su un file di progetto e dichiarandolo nel messaggio di
   commit: zero violazioni su undici implementer nel ciclo precedente, **una su un capo**. La
   direttiva non vince contro chi ha il divieto fresco davanti in un mandato appena letto: vince
   nell'attrito di un'operazione meccanica fatta di fretta.
7. **Quattro premesse del rapporto precedente sono cadute su quattro verificate.** Nessuna è stata
   scoperta rileggendolo: sono emerse perché ogni mandato chiedeva di **verificare il fatto** prima
   di correggere, dichiarando che se il fatto fosse caduto la decisione sarebbe caduta con lui.
   **È il risultato metodologico del goal**, e vale più delle singole voci.

## POI

Archivia in `storico/handoff/` **solo** le unità `FATTO` — con `git mv`, senza chiedere: è
l'eccezione dichiarata nel `CLAUDE.md`.

⚠️ **L'unità 01 è `PARZIALE`**, e il suo lavoro è stato completato dalla `01b`: **decidi tu se
archiviarla, e dichiara la decisione.** Le altre cinque sono `FATTO`.

⚠️ **Attenzione ai nomi.** `storico/handoff/` contiene già i cicli precedenti, e il ciclo del 19
settembre è archiviato con un **prefisso di data** — `2026-09-19-PIANO-sedici-rilievi.md`,
`2026-09-19-CHIUSURA-sedici-rilievi.md`, `2026-09-19-collaudo/` — proprio perché i nomi si
ripetono. **Fai lo stesso.**

**Non archiviare** `handoff/collaudo/`, né questo file, né
`handoff/configurazione-da-applicare.md`, che è lavoro dell'utente ancora da fare.

Lascia il rapporto **in testa a questo file**, nel formato che `architettura-sessioni.md` prescrive.
`COPERTURA:` porta **tre numeri che devono sommare a 38**, e **non ammette il valore `completa`**.
