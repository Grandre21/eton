*Trascritto dal capo il 10 settembre 2026 dal rapporto di `live-testing`, **scritto dopo la
sessione di chiusura**, che ne ha segnalato l'assenza (`CHIUSURA.md`, FUORI SCOPE 12). Il brief
chiedeva all'agente di scrivere questo file e di salvare gli screenshot qui, ma `live-testing` è in
sola lettura e non può fare né l'una né l'altra cosa: **nessuno screenshot è stato salvato**. Ogni
valore qui sotto è quello riportato dall'agente, non riletto dal DOM dal capo.*

```
GIRO D — ESITO: PASSA
1. PASTIGLIE: 48px su tutte e 10 in /expenses · 48px su tutte e 10 in /expenses/{id} ·
   56,6px su tutte e 24 in /collections/{id}/edit (pastiglie-icona a font-size 20px,
   alte quanto .icona-input, 56,6px). Misura: getBoundingClientRect().height.
2. SALVA SPENTO: si distingue. Spento — background rgb(27,27,27), opacity 0.5, cursor
   default. Acceso di confronto («Aggiorna» nel banner) — rgb(76,141,255), opacity 1.
   Durante il salvataggio (classi «btn primario occupato») resta blu, rgb(73,137,251),
   non grigio, con disabled: true.
3. CHIUDI INERTE: colto a metà transizione, nell'istante del click su «Salva» della
   spesa COLLAUDO 4 SET — opacity 0.9397 in calo verso 0.5, cursor default (non pointer),
   attributo href rimosso. A salvataggio concluso: opacity 1, cursor pointer, href presente.
4. ANTEPRIMA: nota lunga (~30 righe) — +453px, verso il basso, atteso. Nota corta (testo
   sostituito senza salvare, poi ripristinato verificato carattere per carattere) — ~7px
   (erano 358). In entrambi i casi .corpo-nota e .markdown alti 342px = 40vh esatto.
5. BARRA LATERALE: bottom del select in .nav-piede .selettore = 839,2px; bottom di
   .voce-piede = 839,2px; scarto 0 (erano 820 e 838). Selettore non stirato, «Profilo» a
   filo del bordo destro. Misurato fra 1198 e 1296px di larghezza, non ai 1414px del
   resoconto: v. ALTRO.
6. BANNER: «Più tardi» c'è e chiude (hidden: true), nessuna richiesta di rete generata dal
   click, storage invariato (solo eton.session ed eton.spazio preesistenti);
   copre l'azione principale: no — 23,7px di scarto fra .azioni e il banner (≈ --s5),
   banner largo al massimo 416px (26rem), in basso a destra, fuori dalla colonna di
   navigazione.
7. ELIMINA ALL'ESTREMO: sì — a filo del bordo destro della fila (scarto 0px), a 494px da
   «Chiudi» (erano 8); la coppia armata resta insieme: sì — «Sì, elimina» e «Annulla» a
   filo destro, adiacenti (gap 8px), stessa riga.
   I QUATTRO CHE NON DEVONO ESSERE CAMBIATI:
   /profile, «Esci» unico della fila — intatto (margin-left: 0px, a sinistra)
   conferma di eliminazione di uno spazio — intatto («Sì, elimina»/«Annulla»
     margin-left: 0px, a sinistra, adiacenti). Spazio di prova creato apposta, poi eliminato.
   scheda di conflitto — intatto («Ricarica la sua»/«Sovrascrivi con la mia»
     margin-left: 0px, adiacenti, stessa riga). Conflitto vero, provocato salvando la stessa
     nota da due schede in ordine concorrente; risolto con «Ricarica la sua».
   «Sì, togli» nella fila di frecce dei CAMPI di CollectionEdit — intatto (margin-left:
     0px; la posizione verso destra viene dal testo d'avviso che lo precede, e «Annulla» va
     a capo per un wrap preesistente, non una regressione). Armato e annullato.
CONSOLE: pulita. Unico messaggio: [INFO] blazor.webassembly…js: Debugging hotkey:
   Shift+Alt+D, standard e non applicativo. Tutte le richieste Supabase viste a 200.
RESTA NEL DATABASE: niente di nuovo. Creati e rimossi: una spesa di prova da 1,00 €, una
   recensione di prova (voto 5) sull'elemento COLLAUDO 4 SET, uno spazio di prova
   «COLLAUDO 4 SET». Ripristinati: il testo della nota COLLAUDO 4 SET (carattere per
   carattere) e l'importo della spesa COLLAUDO 4 SET, riportato a 1.284,50 €.
ALTRO CHE HAI VISTO:
   - Il banner «versione nuova» è comparso spontaneamente durante la sessione, non
     simulato: coerente con ambiente.md, non un difetto.
   - resize_window non ha avuto effetto osservabile sulla viewport in più tentativi: la
     misura 5 non è stata presa a 1414px. Limite dello strumento, non dell'applicazione;
     la larghezza usata sta comunque sopra il breakpoint di 1024px che governa quel layout.
   - Oltre ai quattro controesempi, l'agente ha verificato il QUINTO call-site di
     ConfermaAzione, «Togli la mia recensione» in RecensioniElemento: si sposta a destra,
     ed è l'esito atteso per un call-site del componente, non un controesempio.
```

## Due note del capo, dopo la chiusura

**La quarta riga della tabella dei controesempi, nel brief, era ambigua**, e l'agente l'ha sciolta
nel modo giusto. Il brief diceva «Togliere la propria recensione da un elemento — "Sì, togli" resta
in mezzo alla fila di frecce». Ma la fila di frecce `↑ ↓` sta nei CAMPI di `CollectionEdit`, e la
rimozione della recensione passa invece da `ConfermaAzione` — cioè è un call-site del componente, e
lì spostarsi a destra è il comportamento corretto. L'agente ha provato **entrambe** le cose e le ha
riportate separate: il controesempio con le frecce è intatto, la recensione si sposta come deve. Il
brief nominava una cosa e ne descriveva un'altra.

**La prova 8b dell'unità 11 — a 360px «Sì, elimina» e «Annulla» restano sulla stessa riga? — non è
stata eseguita, e non per una mancanza dell'agente: non era nel brief.** Il brief D elenca sette
misure e nessuna è a 360px. È un buco di copertura del brief, che resta aperto. `ui-critic`, lo
stesso giorno, ha controllato a 357px (in un iframe, perché Chrome desktop non scende sotto 526px di
finestra) che nessuna rotta produca scorrimento orizzontale, ma non ha misurato quella coppia.
