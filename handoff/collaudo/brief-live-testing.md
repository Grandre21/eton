# Brief per `live-testing` — collaudo del goal «tutto ciò che rimane»

Scritto dal capo il **20 settembre 2026**, a sei unità rientrate, **prima** che il server fosse
disponibile. Chi lo esegue lo trova già pronto.

## COME SI USA QUESTO BRIEF

⚠️ **Le misure attese non stanno qui dentro per intero, e non è pigrizia: è la scelta più
robusta.** Ogni unità ha scritto la **propria** sezione `LA MISURA ATTESA PER IL COLLAUDO` nel
proprio resoconto, con gli snippet pronti e i valori esatti. **Aprili e leggili dal disco**, invece
di fidarti di una trascrizione.

Il motivo è misurato: stanotte l'unità 05 ha scoperto di aver riportato due numeri **scritti a
memoria dai propri messaggi di commit** invece che misurati, e li ha corretti dichiarandolo — in un
resoconto la cui tesi centrale era «verifica i conteggi». Una trascrizione di seconda mano è
esattamente il difetto che questo goal ha passato la notte a correggere negli altri.

**Quello che sta qui per esteso** è solo ciò che **nessun resoconto contiene**: le quattro voci
assegnate al collaudo, e i due avvisi che cambiano il modo di provare.

## IL SERVER E IL BROWSER

- **http://localhost:5000**, ambiente Development. Lo avvia il capo, **non tu**: i PID sono in
  `handoff/server.md`. Se non risponde, **fermati e dillo** invece di avviarne uno.
- **Il browser giusto ha `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.** Due Chrome sono
  collegati e i nomi si scambiano a ogni riconnessione: identificalo per `deviceId`, perché **solo
  quello vede `localhost`**.
- ⚠️ **Non premere «Elimina» su una spesa.** Il ciclo precedente ha osservato lì un dialogo nativo
  che il codice non spiega: bloccherebbe il plugin per il resto del turno.
- Il banner «è disponibile una versione nuova» in sviluppo **non è un difetto**, e il service worker
  di dev è un no-op verificato: la cache non falsa le prove.

## I DUE AVVISI CHE CAMBIANO IL MODO DI PROVARE

### 1. L'allineamento del «?» va provato a **due larghezze**, non a una

È la scoperta più importante della notte per il collaudo. L'unità 01 ha allineato il pulsante
d'aiuto alla prima riga di un titolo lungo e ha lasciato la sua misura. **Ma quella misura era vera
solo sotto i 640px**: dentro una media query il titolo saliva a un corpo maggiore mentre l'ancora
restava tarata su quello minore, e sopra quella soglia il «?» stava **8,64 px più in alto** del
centro della prima riga. L'ha trovato l'unità 05, otto ore dopo, e l'ha corretto.

**Quindi: la misura del resoconto 01 va eseguita due volte**, una sotto e una sopra i 640px di
larghezza della finestra. La versione del resoconto 01 da sola sarebbe passata lasciando vivo il
difetto.

### 2. Due effetti collaterali sono **dichiarati in anticipo**: vanno guardati, non temuti

L'unità 04 ha cambiato l'interlinea dei campi e ha **scritto prima i numeri**, invece di lasciarli
scoprire:

- il campo del titolo scende da **~57 a ~51 px** — resta sopra i 48, quindi non perde niente come
  bersaglio di tocco;
- il campo dell'importo scende da **~82 a ~71 px**;
- ⚠️ **la textarea della nota non deve cambiare**: è esclusa dalla regola, e **se cambiasse sarebbe
  un difetto**.

**Se uno dei due stona, non è un errore da segnalare come difetto**: è una decisione di chi guarda
la schermata. Riportalo con lo screenshot e lascia decidere.

## LE PROVE CHE VENGONO DAI RESOCONTI

Apri ognuna dalla propria fonte. Le sezioni si chiamano tutte `LA MISURA ATTESA PER IL COLLAUDO`.

| # | Fonte | Cosa prova |
|---|---|---|
| 1 | `storico/…/01-punto-interrogativo/resoconto.md` oppure `handoff/01-punto-interrogativo/resoconto.md` | il «?» allineato alla **prima riga** di un titolo lungo — criterio **relativo**, legge `lineHeight` dal DOM. **A due larghezze**, v. avviso 1 |
| 2 | `handoff/01b-home-e-montaggio/resoconto.md` | il pulsante d'aiuto **esiste dal primo render**, su ogni rotta e in ogni stato; e **ogni pagina si monta una volta sola** — si legge dal pannello di rete |
| 3 | `handoff/02-voti-e-recensioni/resoconto.md` | il cursore del voto ha un nome; il segnaposto dell'assenza di voto; il conflitto che **non butta via il testo digitato**; la frase d'errore non più doppia |
| 4 | `handoff/03-accesso-e-profilo/resoconto.md` | al logout **non resta nessuna chiave**. ⚠️ **In una scheda sola**: con due, il difetto noto della corsa fra schede la fa fallire legittimamente |
| 5 | `handoff/04-controlli-e-campi/resoconto.md` | i nomi accessibili **per riga**; il paragrafo dell'importo a margine zero; **tre altezze** che tornano a 48; il primo campo a 12px dal secondo |
| 6 | `handoff/05-scala-e-metro/resoconto.md` | la protezione a 16px sui campi di testo; i due controlli di navigazione a 48; il titolo che ora deriva da due soli token |

⚠️ **La prova 2 della tabella ha un caso che vale più degli altri**: `document.querySelector('.aiuto-apri')` deve essere **non nullo mentre i dati stanno ancora arrivando**, non dopo. Prima di questo goal, sulla schermata iniziale era `null` per tutta la durata del caricamento — ed è per questo che il primo clic non funzionava: non cadeva su un pulsante inerte, cadeva **dove il pulsante non c'era**.

⚠️ **La prova 5 ha il caso che conta di più**: premere «Aggiungi campo» **due volte senza scrivere l'etichetta**. I due menù nuovi devono avere **due nomi diversi**. Se leggi due volte lo stesso nome, la voce non è chiusa — si è solo sostituito «controlli senza nome» con «controlli con lo stesso nome», che è lo stesso difetto in un'altra forma.

## LE QUATTRO VOCI CHE NESSUN RESOCONTO CONTIENE

Sono assegnate al collaudo dalla partizione. **Stanno qui per esteso perché nessuno le
ricopierebbe**: è esattamente così che il ciclo precedente ne ha persa una.

### Voce 27 — a 360px, «Sì, elimina» e «Annulla» restano sulla stessa riga?

È la clausola **scoperta** del ciclo precedente, mai provata. Il componente di conferma è **in
pagina**, non un dialogo nativo — quindi è provabile.

⚠️ **Si rende premendo «Elimina» su una collezione, non su una spesa.** La spesa è quella che ha
prodotto il dialogo nativo inspiegato.

Atteso: a 360px di larghezza i due pulsanti stanno sulla stessa riga, oppure vanno a capo in modo
leggibile. Riporta la misura, non un giudizio: le due `getBoundingClientRect()` e se il loro `top`
coincide.

### Voce 31 — `Sovrascrivi()` col nome vuoto

Stesso elemento aperto in **due schede**. In una si modifica e si salva; nell'altra si svuota il
nome e si preme «Sovrascrivi».

Atteso: il messaggio sul nome vuoto compare **una volta sola** — è la voce 26 chiusa dall'unità 02,
che prima lo rendeva due volte — e **la scheda di conflitto resta aperta**.

Non passa da dialoghi nativi.

### Voce 32 — i collegamenti «Tutte» sono alti 48

Una riga di misura sulla schermata iniziale:

    [...document.querySelectorAll('.testa-registro a')].map(a => a.getBoundingClientRect().height)
    // atteso: tutti 48

La regola esiste ed è stata letta nel ciclo precedente; mancava solo l'osservabile.

### Voce 25 — l'unico caso che nessuno ha guardato

**L'elenco delle collezioni a registro vuoto.** È l'unica schermata in cui lo stesso pulsante
«Nuova collezione» compare **due volte insieme**, con due larghezze diverse — in testata e al
centro.

**Non c'è un atteso**: serve uno **screenshot** e una descrizione. L'unità 04 raccomandava di
dichiarare la doppia forma intenzionale; l'unità 05 si è rifiutata di scriverlo **prima** che
qualcuno guardasse questa schermata. Sei tu che guardi.

## COSA VOGLIO INDIETRO

Il formato del tuo file, con `ESITO:` in testa. In più:

- **Una riga per ogni prova**, con il **valore letto**, non una descrizione.
- **Ciò che non sei riuscito a provare, dichiarato invece che taciuto.** ⚠️ Il ciclo precedente si è
  dichiarato «verde, 6/7 eseguiti» su un elenco di sette che non era più quello pianificato: due
  prove erano sparite senza comparire fra le non eseguite. **Se una prova di questo brief non la
  fai, deve comparire nel tuo esito con il motivo.**
- **Gli screenshot** per la voce 25 e per i due effetti collaterali dichiarati.

**Una prova che sai già di non poter fare**: il ramo di sola lettura dell'importo richiede un
**secondo account**, e ogni spazio raggiungibile ha un solo membro. Dichiarala **non provata**, non
verde.
