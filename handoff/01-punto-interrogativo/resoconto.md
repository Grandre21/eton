# UNITÀ: 1 — ESITO: PARZIALE

**Voce 9 (allineamento): chiusa.** **Voce 6 (primo clic): diagnosticata, non corretta — la causa
sta in `Pages/Home.razor`, fuori dal perimetro.** È il caso previsto dal punto 3 del tetto, e la
catena causale è nella sezione `CAUSA`, che è l'artefatto principale di questa unità.

Il lavoro sta sul branch `worktree-unita-01-punto-interrogativo`.

---

## TOCCATI

    wwwroot/css/app.css   → +43 / −1

Nessun altro file. Tutti e cinque gli hunk cadono fra il commento
`/* --- testata di schermata --- */` e `/* --- pastiglie --- */`: verificato con
`git diff -U0` confrontando le intestazioni degli hunk con i confini del blocco. Le tredici voci
del goal che rivendicano `app.css` e le quattro unità che lavorano dopo di me trovano il resto del
foglio intatto — `:root` e la regola `h1` comprese, che il brief vietava esplicitamente.

`Shared/TestataPagina.razor` e `Layout/MainLayout.razor`, pure miei, **non sono stati toccati**: per
il primo non serviva, per il secondo la scelta è dichiarata in `SCOSTAMENTI`.

---

## REVIEW

    review:
      bug-hunter      RILIEVI: 0
      conformity      RILIEVI: 2
      threat-hunter   RILIEVI: 0
      backend-expert  non lanciato — 1 file changed, 43 insertions(+), 1 deletion(-)
                      · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: fondati 1 · infondati 1 · fuori scope 0 · non verificabili 0
      checker (fix)   VERDETTI: risolti 2 · non risolti 0

`coverage` non compare perché dentro una sessione-unità non si lancia (§6).

Le tre misure di `backend-expert` sono state prese **dopo `git add -N .`**, che è la sola ragione
per cui `0 create mode` è un dato e non un'omissione: senza, un file nuovo sarebbe stato invisibile
a `--summary` e il gate avrebbe prodotto un «non lanciato» formalmente completo e falso.

---

## CONTRATTI

Nessuno. Il mandato non ne prevedeva, ed è stato rispettato il vincolo di forma che le unità
successive ereditano: **nessun rimando per numero di riga** nei commenti aggiunti. È stato
verificato dal `checker`, che ha cercato tutti i commenti nuovi uno per uno e non ne ha trovato
nessuno ancorato a un numero; e su questa famiglia di difetto ha corretto anche me, v. `ADJUDICA`.

---

## ADJUDICA

**Rilievo 1 — duplicazione delle tre righe condivise (`conformity`, media) → fondato → corretto.**
`align-self: start` e la `margin-block` compensata erano ricopiate carattere per carattere in
`.aiuto-apri` e `.testata-azione`, mentre il foglio usa sistematicamente un selettore combinato per
le dichiarazioni condivise — e il commento sopra `.etichetta-piccola, .etichetta-campo` ne dà la
ragione esplicita: «*tenerle separate le avrebbe fatte divergere col tempo*». Fattorizzate in
`.aiuto-apri, .testata-azione`.
**verificato: risolto — `wwwroot/css/app.css`, regola `.aiuto-apri, .testata-azione`**: il checker
ha contato una sola occorrenza di ciascuna dichiarazione e ha confermato che il CSS **calcolato**
non cambia, perché un selettore-list ha la stessa specificità e la stessa posizione di sorgente per
entrambi i rami e nessuna delle due regole successive ridichiara quelle proprietà.

**Rilievo 2 — il rimando «il "?"» in `.testata-azione` (`conformity`, bassa) → infondato.**
Il commento **cita** `.aiuto-apri` nell'ultima frase, quindi un grep lo trova e la regola
dell'intestazione è rispettata. Il revisore aveva letto solo l'apertura del commento.
**È l'infondato che ho riverificato io**, come il §5 impone quando ce n'è almeno uno: ho riaperto
le righe della regola e confermo il verdetto del checker — il selettore c'è.
⚠️ **E lo stesso errore l'avevo fatto io prima di lui**: avevo dato il rilievo per buono guardando
la sola prima riga del commento. L'istruttoria ha corretto entrambi, che è esattamente ciò per cui
esiste.

**Rilievo 3 — il rimando nella media query (trovato dal `checker`, non da un revisore) → fondato →
corretto.** Il commento sopra `.testata { --riga-titolo: … }` dentro `@media (max-width: 26rem)`
diceva «l'ancora del "?" va ricalcolata» senza nominare **mai** un selettore: era l'istanza vera del
difetto che il rilievo 2 attribuiva al posto sbagliato.
**verificato: risolto — `wwwroot/css/app.css:990`**, e il checker ha eseguito il grep: la riga
compare ora fra i risultati di `grep '\.aiuto-apri'`.

**Riserva dichiarata dal checker, che riporto invece di appianarla.** Sul punto «il commento unito
ha perso informazioni rispetto ai due precedenti?» il verdetto è *non verificabile letteralmente*:
la forma duplicata non è mai stata committata, quindi non esiste in git un testo con cui
confrontarla parola per parola. Il giudizio per contenuto è che non si sia perso nulla, ma è un
giudizio, non una misura, e come tale sta scritto.

---

## FUORI SCOPE

### A — La causa della voce 6, e il suo rimedio. È il punto più importante di questo resoconto.

**Dove:** `Pages/Home.razor:68`, dentro il ramo `else` che apre a `:58`.

**Perché non l'ho corretta:** `Pages/Home.razor` è nominato esplicitamente fra i file da non
toccare, e il punto 3 del tetto prescrive di fermarsi qui. La catena è nella sezione `CAUSA`.

**Il rimedio che propongo**, ed è piccolo, ~8 righe, tutto dentro `Pages/Home.razor`:

1. Aggiungere una proprietà calcolata per il titolo, **con il pattern che il progetto usa già** in
   `Pages/SpaceDetail.razor:163` — `private string TitoloSchermata => spazio?.Name ?? "Spazio";` —
   che è esattamente il caso della Home: un nome che non c'è ancora, con un'etichetta di ripiego.
   Qui diventerebbe `Spazi.Attivo?.Name ?? "Spazio"`.
2. Portare `<TestataPagina Titolo="@TitoloSchermata">` **fuori** dal condizionale, a colonna zero,
   come nelle **altre dieci** pagine su undici.

⚠️ **Una decisione che spetta a chi ripartisce, non a me.** Spostando la testata fuori, il «?»
comparirebbe anche nei rami «nessuno spazio» ed «errore», dove oggi non c'è. Io credo sia giusto —
l'aiuto deve restare raggiungibile proprio quando qualcosa non va — e noto che lo stesso file ha già
ragionato in questo modo per l'`<header class="intestazione solo-stretto">`, il cui commento a
`Pages/Home.razor:32-34` avverte che spostarlo dentro l'`else` «*riaprirebbe il buco*», lasciando
«*chiuso fuori senza modo di raggiungere «Esci»*» chi non ha ancora uno spazio. Ma è un cambiamento
di comportamento visibile, e va deciso, non fatto di straforo.

### B — Il doppio montaggio di ogni pagina. Difetto suo, e amplificatore della voce 6.

**Dove:** `Layout/MainLayout.razor`, la sottoscrizione a `LocationChanged` che aggiorna
`chiaveRotta` **dopo** che il Router ha già renderizzato.

**Cosa succede**, verificato decompilando `Microsoft.AspNetCore.Components` 10.0.10 dal pacchetto
installato (nessun passaggio regge «a memoria»): `Router.Attach` si sottoscrive a `LocationChanged`
alla creazione del componente, quindi **prima** di qualunque discendente, e `Delegate.Combine`
accoda preservando l'ordine di invocazione. L'intero diff Router → LayoutView → MainLayout → pagina
avviene **dentro** l'handler del Router, in modo sincrono. La pagina viene dunque montata con la
chiave **vecchia** e `OnInitializedAsync` parte — prima richiesta HTTP. Poi `SuCambioRotta` aggiorna
la chiave, il diffing emette Remove + Prepend, la prima istanza viene dismessa e ne nasce una
seconda: **`OnInitializedAsync` gira due volte**. È il fatto 5 della diagnosi, «due volte quasi in
contemporanea», e spiega perché la guardia `caricamentoIniziale` di `Pages/Home.razor:265` —
scritta proprio per impedire «*due letture identiche*» — non lo fermi: la guardia è un campo
dell'istanza, e l'istanza è nuova.

⚠️ **E un commento del progetto è falso.** `Layout/MainLayout.razor:51-53` afferma che senza quella
sottoscrizione il layout «*resterebbe fermo al render iniziale per l'intera sessione*». Non è vero:
`ChangeDetection.MayHaveChanged` tratta un `RenderFragment` come sempre «forse cambiato», perché non
è un tipo immutabile noto, quindi `LayoutView` fa ridisegnare MainLayout a **ogni** render del
Router. Un commento falso è peggio di un commento assente: chi lo legge non tocca la sottoscrizione
credendo di romperla.

**Il rimedio proposto:** `<div class="pagina-entra" @key="PercorsoSenzaQuery()">`, cioè calcolare la
chiave nell'espressione invece che in un campo aggiornato da un handler. `Navigation.Uri` è già
aggiornato quando gli handler girano, quindi al render del Router la chiave è **già** quella nuova:
un montaggio solo. È corretto anche nello scenario opposto — se quel render non avvenisse, la
sottoscrizione resta e fa il suo lavoro. Nella forma piena si tolgono anche il campo, la
sottoscrizione, `IDisposable` e `Dispose`, e **si riscrive il commento falso**.

**Effetto atteso sulla voce 6: dimezza la finestra, non la chiude.** La causa resta A.

### C — L'aggravante non censita: il difetto non è solo al montaggio.

`Pages/Home.razor:458-459` abbassa `caricato` e ridisegna **a ogni cambio di spazio**. Poiché la
testata vive nel ramo `else` di quel flag, il «?» **sparisce e riappare anche lì**. La voce 6 lo
descrive come un difetto del primo clic dopo il montaggio: è più largo di così. Il rimedio A lo
copre insieme al resto.

---

## CAUSA

### La catena stabilita, con le righe che la reggono

1. **Finché `caricato` è falso, la Home rende solo un paragrafo.** `Pages/Home.razor:43-46`:
   `@if (!caricato) { <p class="avvio">Caricamento…</p> }`.
2. **La testata sta nel ramo `else`.** `Pages/Home.razor:58` apre l'`else`, `:68` contiene
   `<TestataPagina Titolo="@Spazi.Attivo.Name">`.
3. **Quindi, fra il montaggio della pagina e la fine del caricamento, il pulsante «?» non esiste nel
   DOM.** Un clic in quella finestra non cade su un pulsante inerte: cade dove il pulsante **non
   c'è**.
4. **La durata della finestra è la latenza di due giri di rete in sequenza**, non di un'animazione:
   `Pages/Home.razor:279` (`await Spazi.AssicuraCaricatoAsync()`) e `:310`
   (`await CaricaDettagli("Spazio caricato")`).
5. **Le rotte sane hanno la testata fuori dal condizionale.** Misura riproducibile con un comando:

       grep -n '^[[:space:]]\+<TestataPagina' Pages/*.razor   → una sola riga, Home.razor:68
       grep -c '^<TestataPagina' Pages/*.razor                → 1 in ciascuno degli altri dieci file

   **Una pagina su undici.** Ed è quella su cui il difetto si riproduce in modo affidabile.

### Perché questa catena spiega i fatti misurati nel browser, tutti

| Fatto della diagnosi | Spiegazione |
|---|---|
| Soglia fra 0,3 e 0,5 s, incompatibile con i 200 ms dell'animazione | è la latenza del punto 4, non un'animazione |
| `activeElement` resta `BODY`, il fuoco non arriva mai | non c'è nessun elemento da mettere a fuoco |
| «fuoco sì, pannello no» mai osservato | coerente: o il pulsante c'è e fa tutto, o non c'è e non fa nulla |
| Fallisce su Home, non su Note/Collezioni/Spese | punto 5 |
| Sequenza `1 → 1 → 0 → 1` dell'osservatore di mutazioni | **lo `0` è la finestra del difetto** |

⚠️ **L'ultimo merita una riga a sé.** Quell'esperimento era stato condotto per **escludere**
l'ipotesi dell'identificatore duplicato, e l'ha esclusa correttamente. Ma lo stesso tracciato
conteneva la prova dell'altra ipotesi: uno stato con **zero** pannelli. Nessuno l'aveva letto in
quella direzione, perché si stava cercando un due, non uno zero.

### Ciò che ho escluso, e con quale riga

- **Il containing block dell'animazione**, che la diagnosi aveva lasciato «non esclusa ma
  insufficiente». **Escluso**, e dal fatto 2 stesso: se un `transform` su un antenato rompesse il
  posizionamento del popover, il pannello risulterebbe **aperto e mal messo**, quindi `:popover-open`
  sarebbe **vero**. La diagnosi l'ha misurato **falso**, con `activeElement` su `BODY`. Un popover
  che non si apre e un pulsante che non prende il fuoco non sono un problema di posizionamento.
- **L'identificatore duplicato.** Già escluso dalla diagnosi; il codice concorda
  (`Shared/TestataPagina.razor:30-33` spiega perché un id statico basta, e la sequenza misurata non
  mostra mai due pannelli insieme). **Non va reinseguito.**
- **Un difetto di `Shared/TestataPagina.razor`.** Il componente è corretto: se la pagina non lo
  monta, nessuna riga scritta lì dentro può rimediare.

### Ciò che resta indeterminato, e va detto

**Il difetto riprodotto il 19 settembre su editor di collezione ed editor di elemento non è
spiegato da questa catena.** In `Pages/CollectionEdit.razor:11` e `Pages/ItemEdit.razor:12` la
testata è a colonna zero, fuori dal condizionale: lì il pulsante **esiste dal primo render**. O
quella riproduzione era un artefatto del metodo di misura, oppure su quelle rotte agisce una
seconda causa — e il candidato è il doppio montaggio del punto B, che dismette e ricrea l'intero
sottoalbero. **Non l'ho dimostrato**: senza browser e senza bUnit (il progetto ha solo `xunit` su
logica pura, limite già censito dall'unità 02 del ciclo scorso) non ho modo di osservare un
montaggio. Chi riparte da qui parta da lì.

---

## LA MISURA ATTESA PER IL COLLAUDO

Da ricopiare nel brief della prova nel browser invece di inventarla.

### Voce 9 — dove deve stare il centro verticale del «?»

**Criterio relativo, e deve essere relativo.** Il centro verticale del «?» deve coincidere, entro
**±1 px**, con il centro della **prima riga** del titolo:

    const h1  = document.querySelector('.testata h1');
    const btn = document.querySelector('.aiuto-apri');
    const riga = parseFloat(getComputedStyle(h1).lineHeight);   // NON altezza/numero di righe
    const centroPrimaRiga = h1.getBoundingClientRect().top + riga / 2;
    const r = btn.getBoundingClientRect();
    const centroBottone   = r.top + r.height / 2;
    Math.abs(centroBottone - centroPrimaRiga) <= 1              // atteso: true

Due controlli di non regressione, sulle schermate a titolo corto:

- `.testata` con titolo su una riga resta alta **48 px**, come prima della modifica;
- `.testata-azione`, dove c'è, ha lo stesso centro verticale del «?» — entro ±1 px.

⚠️ **Perché il criterio non è la coordinata assoluta della diagnosi.** La diagnosi dà
`y=76,08` per il centro della prima riga, e quel numero **non regge contro il foglio**. Con
`h1 { line-height: 1.08 }` e `.testata h1 { font-size: var(--t-xl) }` sotto 26rem, una riga è alta
26 × 1,08 = **28,08 px**; il blocco misurato era alto **336,9 px**, cioè **12 righe esatte**
(12 × 28,08 = 336,96), non sei. Con dodici righe il centro della prima sta a 48 + 14,04 = **62,04**.
Il valore 76,08 è precisamente ciò che si ottiene dividendo 336,9 per **sei** invece che per dodici:
è un numero derivato da un conteggio di righe stimato a occhio. **Il mandato chiedeva di dichiarare
una smentita: questa è la smentita.** Il criterio sopra è immune all'errore perché legge
`lineHeight` dal DOM invece di ricavarlo.

### Voce 6 — dopo quanto tempo dal montaggio il primo clic deve funzionare

**Subito. Zero.** Nessuna attesa è accettabile, e la voce non si chiude con una soglia più bassa.

Il criterio robusto non è cronometrico ma strutturale, e si verifica **prima** di cliccare:

    // appena dopo la navigazione, prima che i dati arrivino
    document.querySelector('.aiuto-apri') !== null     // atteso: true — oggi sulla Home è false

Oggi, sulla Home, quel selettore è `null` per tutta la durata del caricamento. Finché lo è, la voce
6 è aperta qualunque cosa dica un cronometro — e nessuna misura di questa unità la chiude, perché
la correzione sta in `Pages/Home.razor`.

---

## GATE

    dotnet build Eton.sln -warnaserror --no-incremental   → Compilazione completata. Avvisi: 0  Errori: 0
    dotnet test Eton.sln                                  → Superato! Non superati: 0. Superati: 310. Totale: 310

Eseguiti da me a subagent rientrati, sul worktree, a diff finale.

---

## SCOSTAMENTI

1. **`.testata-azione` è stata trattata insieme al «?», e il mandato nominava solo il «?».**
   Non è un allargamento ma una **non regressione**: portando il titolo e il «?» all'inizio del
   blocco, l'azione sarebbe rimasta l'unica centrata sul blocco intero, disallineata di 4,56 px
   dagli altri due — cioè il difetto della voce 9 reintrodotto in piccolo dalla correzione della
   voce 9. Con un titolo lungo sarebbe finita a metà del testo esattamente come il «?».
2. **`Layout/MainLayout.razor` è mio e non l'ho toccato — scelta deliberata, non una dimenticanza.**
   Il rimedio del punto B sta nel mio perimetro e sarebbe di poche righe. Due ragioni per fermarmi:
   il ⚠️ del mandato sul fatto 5 dice che se il doppio caricamento **non** è la causa della voce 6
   va riportato in `FUORI SCOPE` e **non toccato** — e non lo è, è un amplificatore; e il rimedio
   cambia l'animazione d'ingresso su **ogni** rotta, con due modi di fallire che solo una prova nel
   browser può escludere, che quest'unità non può fare. Consegnarlo non provato sarebbe stato
   peggio che consegnarlo documentato.
3. **Il budget di complessità che avevo scritto — «meno di 25 righe» — è stato superato: 43.**
   L'errore è mio: avevo dettato io all'implementer, verbatim, commenti che da soli superavano la
   soglia. L'implementer l'ha segnalato invece di tagliare i commenti per rientrarci, che è la
   scelta giusta.
4. **Il primo brief conteneva un errore che ho corretto a valle.** Avevo prescritto
   `min-height: var(--riga-titolo)` su `.testata-azione`, dichiarazione **inerte**: il suo contenuto
   è un `.btn.compatto`, che da `.btn` eredita `min-height: var(--tocco)` = 48 px e non lo riduce.
   Scoperto verificando la regola invece di assumerla, e corretto con un secondo giro sullo stesso
   implementer.
5. **Nessun `PIANO-DESIGN` incollato da una schermata nuova**: la skill `frontend-design` è stata
   invocata come il §0 impone (il brief tocca `.css`), e il suo piano è servito a decidere
   l'allineamento e a scartare il default «`align-items: flex-start` e via», che costava 4,56 px di
   spostamento su ogni schermata a una riga. Palette e tipografia sono rimaste quelle del sistema,
   deliberatamente: l'intestazione di `app.css` vieta i colori scritti a mano, e introdurne uno
   sarebbe stato il difetto.
6. **Il PID 24148 segnalato all'avvio come «server residuo» non è un orfano**: è il server del capo,
   documentato in `handoff/server.md`, vivo di proposito sulla 5000 sul commit `cdb0999`. Non l'ho
   toccato e non ne ho avviato un secondo, come il mandato prescrive. **Chi chiude il ciclo fermi
   entrambi i PID**, padre e figlio: `handoff/server.md` spiega che fermare solo il padre lascia la
   porta occupata.
