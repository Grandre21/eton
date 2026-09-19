# Ricognizione dell'area voti e recensioni — il buco dell'elenco, colmato

Eseguita il **19 settembre 2026** sul commit `bb77cf7`. È la **quarta clausola** dell'obiettivo di
questo goal, e l'unica che non nasceva da un difetto ma da un'**assenza**: la ricognizione del
27 agosto — quella da cui sono nati sedici rilievi e due mesi di lavoro — non aveva mai visto
`/collections/{id}`, `/collections/{id}/edit` e `/collections/{id}/items/{id}`, perché all'epoca
erano irraggiungibili.

**Non era un buco della partizione: era un buco dell'elenco da cui la partizione nasce.**

L'agente ha usato le schermate per davvero: collezione nuova con **sei campi di tutti i tipi
disponibili**, voto al buio attivo, due elementi, voto dato e cambiato, minimo e massimo della
scala, voto rimosso tenendo il commento.

## I cinque attriti

### 1 — Il pulsante «?» non risponde al primo clic · **grave** · sistemico

**Riprodotto tre volte, su due rotte diverse, con navigazione fresca ogni volta.** Il primo clic
dopo il caricamento mette **solo il fuoco** sul pulsante; serve un **secondo** clic per aprire il
pannello. Dopo, il comportamento è corretto.

**Perché è il più grave dei cinque:** quel pannello è **l'unico canale in-app** che spiega le
funzioni meno intuitive — il voto al buio, i conflitti di salvataggio, cosa succede a un campo con
un valore non interpretabile. Un utente che lo preme una volta, non vede niente e conclude che il
pulsante è rotto, **perde esattamente la spiegazione che gli serviva**.

⚠️ Va detto che gli infobutton erano stati misurati come *funzionanti* dalla ricognizione di
agosto: o il difetto è comparso dopo, o l'ha mascherato il fatto che chi provava cliccasse due
volte senza farci caso. **La prima ipotesi va verificata prima di correggere.**

### 2 — L'aggregato del voto mostra un frammento rotto · media

Con una recensione che ha **commento ma nessun voto** — scritta così, o ottenuta togliendo il voto
e tenendo il commento — l'intestazione mostra *una barra verde troncata* seguita da
`/ 10 — nessun voto, 1 commento`, **senza nessun numero né simbolo prima dello slash**.

È incoerente col caso «nessuna recensione», che mostra correttamente `? / 10 — coperto finché non
voti`. Sembra un elemento visivo rotto, non un segnaposto voluto.

### 3 — Un conflitto annunciato dove non c'è nessun altro · media

Dopo due salvataggi ravvicinati della **propria** recensione, stesso browser, stesso account,
spazio con **un solo membro**, è comparso: *«La tua recensione era stata modificata altrove: ho
caricato la versione più recente.»*

**Due problemi distinti, e il secondo è più serio del primo:**

1. in un contesto mono-utente il messaggio è ingiustificato e allarmante — parla di «altrove»
   quando nessun altro esiste;
2. **il comportamento non corrisponde a ciò che l'aiuto della stessa schermata promette.** L'aiuto
   descrive una **scelta** fra ricaricare e sovrascrivere; nella pratica il sistema ha **ricaricato
   da solo**, senza chiedere.

Nessun dato è andato perso nelle prove. Ma un meccanismo di conflitto che annuncia un conflitto
inesistente e si comporta diversamente da come si descrive erode la fiducia proprio dove serve.

### 4 — Il pulsante «?» fluttua accanto a un titolo lunghissimo · lieve

Con un nome elemento oltre i ~150 caratteri, il titolo va a capo su cinque o sei righe e il
pulsante resta a un'altezza fissa, **a metà del blocco di testo**. Non è un giudizio estetico: è un
comportamento a un bordo che era stato chiesto esplicitamente.

### 5 — «Ordina per» offre solo «Nome» · minore

Verificato su due collezioni, con uno e con due elementi, e con campi Numero, Data e Scelta
definiti: il menu non offre altro. **Segnalato con cautela** dall'agente stesso: può essere un'area
non ancora sviluppata più che un difetto. Con una collezione di birre o di vini, ordinare per voto
medio è però l'aspettativa naturale.

## Cosa funziona, e vale la pena non rompere

Una ricognizione che elenca solo difetti dà un'immagine storta.

- **Il voto al buio è spiegato bene, e in due punti diversi con lo stesso testo**: alla creazione
  della collezione e sull'aggregato di ogni elemento non ancora votato. Prima di attivarlo si
  capisce cosa fa; dopo, si capisce perché non si vedono i voti altrui. È la funzione più
  particolare del progetto ed è quella spiegata meglio.
- **Cambiare voto funziona e si aggiorna ovunque subito** — aggregato, cursore, elenco — provato da
  8 a 3, a 10, a 0,5 e ritorno a 8,5. *(Nota: il minimo vero della scala è 0,5, non 0.)*
- **«Togli la mia recensione» e «Nessun voto» sono distinti correttamente**: il primo cancella
  tutto, il secondo toglie il numero e tiene il commento.
- **Gli stati vuoti spiegano il concetto** invece di dire «niente qui»: *«Un elemento è una cosa da
  giudicare: una birra, un film, un posto.»*
- **Tutti e sei i tipi di campo** si creano, si salvano e si rendono correttamente.
- **Un nome lunghissimo si presenta bene nella card dell'elenco**, e i campi opzionali vuoti
  **non** producono sottotitoli con `undefined`: semplicemente non compaiono.
- **Nessuna chiamata di rete fallita** in tutta la sessione.

## Cosa non è stato provato, e perché

- **Il cuore del voto al buio con un secondo attore.** Ogni spazio raggiungibile da questo account
  ha **un solo membro**. È lo stesso limite che ha bloccato la prova 2 del giro B: senza un secondo
  account, la funzione più particolare del progetto resta collaudata solo a metà.
- **«Elimina» e «Chiudi con modifiche»**, per divieto esplicito: producono dialoghi nativi che
  bloccano il plugin.
- **Il cambio della scala dei voti** su una collezione che ha già voti — l'agente non l'ha tentato
  per non alterare i dati in modo imprevedibile. **È un'ottima domanda da porsi**, e nessuno sa
  cosa succede.
- **URL malformati** nei campi «Immagine» e «Collegamento».

## Dati di prova rimasti sul database di sviluppo

Non rimovibili senza «Elimina», che blocca il plugin. **Da togliere a mano**, insieme ai due del
giro B:

3. **Collezione «Collaudo Ricognizione»** (spazio Personale), con due elementi — «Chianti Classico
   2021» con voto 8,5 e un commento, e l'elemento dal nome lunghissimo con un commento di prova.

Sono riconoscibili dal nome: nessun dubbio su cosa sia di collaudo e cosa no.
