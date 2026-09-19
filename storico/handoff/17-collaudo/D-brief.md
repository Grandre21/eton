# Collaudo, giro D — le misure

**Leggi prima `handoff/17-collaudo/ambiente.md`**: server, browser, sessione, e il vincolo che
il database è quello vero. Non lo ripeto qui.

Ultimo dei quattro giri, **e l'unico che prova un lavoro che non si verifica leggendo il
codice**. Gli altri tre hanno controllato comportamenti e frasi, cose che un test può
catturare. Qui si misura: altezze, allineamenti, che cosa sembra spento e che cosa sembra
premibile. Nessun test del progetto può dirlo.

## I CRITERI

Stanno in **`handoff/11-foglio-di-stile/resoconto.md`**, che è il resoconto di due giri di
lavoro sullo stesso file. Leggilo per intero prima di aprire il browser: contiene sia le voci
chiuse sia, in fondo, ciò che l'unità ha lasciato aperto.

Sette voci sono state chiuse. **Un'ottava non è stata fatta ed è una decisione, non una
dimenticanza**: era una rinomina di classe che non cambia un pixel. Non cercarla e non
segnalarla.

## LE SETTE MISURE

### 1. Le pastiglie delle categorie erano alte 21px

Il progetto dichiara **48px** come misura minima del tocco, e già la applica altrove. Le
pastiglie ne avevano ventuno. **Misurale in tre posti** — la pagina delle spese, la modifica di
una spesa, la modifica di una collezione — e riporta **l'altezza in pixel**, non l'impressione.

### 2. «Salva» spento non sembrava spento

Su fondo nero, un blu pieno al cinquanta per cento di opacità **resta la cosa più accesa della
schermata**. Apri un editor senza modificare niente e guarda «Salva»: **deve leggersi come
spento**. Confrontalo con un pulsante acceso nella stessa schermata e di' se si distinguono.

### 3. I «Chiudi» inerti sembravano premibili

Durante un salvataggio, i link «Chiudi» diventano inerti — non navigano. Prima **restavano
accesi all'occhio**. Salva qualcosa e, **mentre gira**, guarda «Chiudi»: deve sembrare spento.
È una finestra breve: se non riesci a coglierla, dillo invece di darla per buona.

### 4. L'anteprima faceva saltare il layout di 358 pixel

Nella modifica di una nota c'è un'anteprima. Attivala e **misura di quanto si sposta il
contenuto sotto**. Prima erano trecentocinquantotto pixel: ora l'altezza dovrebbe essere
riservata in anticipo, e lo spostamento **zero o quasi**.

### 5. Il selettore dello spazio e «Profilo» leggevano come accavallati

Nella barra laterale. Guarda e riporta **le posizioni**, non l'impressione.

### 6. Il banner della versione nuova, che copriva l'azione principale

Deve avere un «**Più tardi**» che lo chiude. Premilo: **il banner deve sparire**. Che
**riappaia al riavvio è corretto** e documentato — non è un difetto, non segnalarlo.
Verifica anche che, mentre è a schermo, **non copra** il pulsante principale della pagina.

### 7. «Elimina» era a 55px da «Chiudi» — e QUI STA LA PROVA PIÙ DELICATA DEL GIRO

Il pulsante distruttivo ora va **all'estremo opposto della fila**. Verificalo in un editor:
apri la modifica di una nota e guarda dove sta «Elimina» rispetto a «Salva» e «Chiudi».

Poi arma la conferma (premi «Elimina» **senza confermare**): compaiono «Sì, elimina» e
«Annulla», e **devono spostarsi insieme come coppia**, non separarsi.

**E adesso la parte che conta.** Il rimedio ovvio — spingere a destra ogni pulsante rosso dentro
un blocco di azioni — è stato **scartato** perché avrebbe rotto quattro punti che non passano da
quel componente. L'unità li ha elencati e li ha verificati leggendo. **Tu li verifichi
guardando**, e devono essere rimasti **come prima**:

| Dove | Cosa deve restare vero |
|---|---|
| **`/profile`** | «Esci» è l'unico pulsante della sua fila: **non deve** essere isolato a destra |
| Una conferma di eliminazione fuori dagli editor | «Sì, elimina» e «Annulla» restano **a sinistra e adiacenti** |
| La scheda di un conflitto, se riesci a vederne una | «Ricarica la mia» resta **accanto** a «Ricarica la sua»: sono scelte gemelle |
| Togliere la propria recensione da un elemento | «Sì, togli» resta **in mezzo** alla fila di frecce, non salta a destra |

Se anche uno solo di questi è saltato a destra, **è una regressione introdotta dalla correzione**
ed è il difetto più importante che puoi trovare in questo giro. Riportalo per primo.

Gli ultimi due potrebbero non essere raggiungibili senza dati adatti: **se non ci arrivi, scrivi
che non erano raggiungibili**, non che sono a posto.

## COME SI MISURA

Screenshot e, dove serve, l'ispezione della pagina. **Numeri, non aggettivi.** Se una misura non
la sai prendere, dillo: «non misurato» è un esito onesto, «sembra a posto» non lo è.

Salva gli screenshot in `handoff/17-collaudo/` con nomi che dicano cosa mostrano.

## L'ESITO

In **`handoff/17-collaudo/D-esito.md`**.

```
GIRO D — ESITO: PASSA | NON PASSA | PARZIALE
1. PASTIGLIE: <altezza in px, nei tre posti>
2. SALVA SPENTO: si distingue | non si distingue | non provato
3. CHIUDI INERTE: <cosa hai visto> | finestra troppo breve
4. ANTEPRIMA: <spostamento in px>
5. BARRA LATERALE: <posizioni>
6. BANNER: «Più tardi» c'è e chiude | <cosa fa> ; copre l'azione principale: sì | no
7. ELIMINA ALL'ESTREMO: sì | no ; la coppia armata resta insieme: sì | no
   I QUATTRO CHE NON DEVONO ESSERE CAMBIATI: <uno per riga: intatto | SALTATO A DESTRA | non raggiungibile>
CONSOLE: <righe trascritte> | pulita
RESTA NEL DATABASE: <cosa hai creato> | niente
ALTRO CHE HAI VISTO: <fatti> | niente
```
