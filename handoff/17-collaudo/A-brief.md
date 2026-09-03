# Collaudo, giro A — il bloccante: creare una collezione funziona davvero?

**Sei il primo dei quattro giri, e vieni da solo.** Se questo fallisce, gli altri tre non hanno
soggetto: tutte le prove su `CollectionEdit` e sugli elementi presuppongono che una collezione
si possa creare. Riferisci l'esito e basta — non correggi codice, non ne proponi.

## L'AMBIENTE, e una cosa che devi sapere prima di toccare qualsiasi cosa

- **URL: `http://localhost:5000`** — server già avviato dal capo, PID annotati in
  `handoff/server.md`. **Non avviarlo e non fermarlo tu.**
- Browser: **`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`**. Due Chrome sono collegati e i
  nomi si scambiano a ogni riconnessione: **identifica per `deviceId`**, e solo quello vede
  `localhost`. Selezionalo con `select_browser` senza chiedere niente a nessuno — l'utente non
  è raggiungibile e la scelta è già stata presa in una sessione precedente.
- **La sessione è già autenticata**, verificato dal capo poco fa: aprendo la radice si arriva
  sulla Home dello spazio «Personale». Se ti trovassi su `/benvenuto` senza sessione, **fermati
  e dillo**: non c'è modo per te di fare un accesso Google, e il giro va rimandato all'utente.

**Lo sviluppo punta al Supabase di PRODUZIONE** (`fdqedhgvpneuybtykamf.supabase.co`): non esiste
un database locale e non esiste un account di collaudo. **Tutto ciò che crei è un dato vero
dell'utente.** Quindi:

1. **Ogni cosa che crei porta nel nome `COLLAUDO 4 SET`**, così l'utente la riconosce a colpo
   d'occhio e la rimuove quando vuole.
2. **Crei il minimo indispensabile**: una collezione, non tre.
3. **Non cancelli e non modifichi NIENTE che non abbia creato tu.** Se una prova richiede di
   eliminare qualcosa, elimini solo la tua roba. Nel dubbio, non farlo e scrivilo.
4. **Elenchi nell'esito tutto ciò che resta nel database**, con nome e dove si trova.

Lo spazio «Personale» era **vuoto** quando il capo l'ha guardato — nessuna nota, nessuna
collezione, nessuna spesa del mese corrente. Se trovi roba che non hai creato tu, è dell'utente:
non toccarla.

## IL DIFETTO CHE STAI VERIFICANDO

Dal 12 agosto 2026 creare una collezione falliva **in produzione** con:

```
permission denied for table collections   (SQLSTATE 42501)
```

Non era RLS: era un privilegio **di colonna**. La migrazione `voto_al_buio` aveva aggiunto la
colonna `blind` riconcedendola solo in UPDATE, mai in INSERT; il modello C# la inviava lo
stesso, e PostgreSQL rifiutava **l'intera istruzione**. L'unità 02 ha scritto la migrazione che
aggiunge `blind` al grant di INSERT, e **l'utente l'ha eseguita in produzione il 3 settembre**.

**L'effetto non è mai stato osservato da nessuno.** È un fatto riportato, non misurato. Sei tu
a misurarlo.

## LA PROVA, e perché l'interruttore è la parte che conta

C'era una seconda correzione possibile, scartata: mettere `ignoreOnInsert: true` sulla proprietà
`Blind` in C#. Avrebbe fatto passare l'INSERT, ma la collezione sarebbe nata **col valore di
default**, e l'interruttore «Voto al buio» della schermata di creazione sarebbe stato ignorato
**in silenzio** — un difetto al posto dell'altro.

Per questo la prova non è «si salva». È:

1. **Vai a creare una collezione** — dalla Home c'è «Nuova collezione», oppure da `/collections`.
2. **Dai il nome `COLLAUDO 4 SET`.** Se la schermata propone dei modelli (uno si chiama
   «Birre»), usane uno: è il percorso su cui il difetto era stato osservato.
3. **ACCENDI l'interruttore «Voto al buio»** prima di salvare. È il punto della prova.
4. **Premi «Salva».**
5. **Riporta cosa succede**: si salva? Compare un errore? Se compare, **trascrivilo verbatim** e
   di' se è JSON grezzo di PostgreSQL o una frase in italiano — è anche il rilievo 3, chiuso da
   altre unità, e qui lo vedresti dal vivo.
6. **Se si è salvata, riaprila** e guarda se «Voto al buio» è **ancora acceso**. Questo è il
   criterio che distingue la correzione vera dalla toppa scartata. Se è spento, il salvataggio
   è riuscito ma il valore si è perso: **è un difetto, e va riportato come tale.**

## DUE COSE DA GUARDARE MENTRE SEI LÌ, senza uscire dal giro

Non sono il tuo mandato, ma sei l'unico che passa di qui e costano zero:

- **La console del browser.** Prima di cominciare, aprila e leggila; se compare qualunque riga
  `[Auth]`, un errore o un avviso durante le tue azioni, **trascrivila**. Nessuno ha mai
  guardato la console di questa applicazione.
- **L'esito del salvataggio: dove compare?** Il rilievo 2 diceva che l'esito appariva lontano da
  dove si guarda, e cinque unità l'hanno corretto. Dimmi soltanto **dove** lo vedi comparire
  rispetto al pulsante «Salva».

## COSA NON È UN DIFETTO, in sviluppo

- Il banner «**è disponibile una versione nuova**» che riappare: il service worker di dev è un
  no-op verificato, ed è il comportamento documentato di chi l'ha scritto.
- Un primo caricamento lento: è Blazor WebAssembly che scarica il runtime.

## L'ESITO

Scrivilo in **`handoff/17-collaudo/A-esito.md`**, e tienilo corto. Gli screenshot che salvi
vanno nella stessa cartella.

```
GIRO A — ESITO: PASSA | NON PASSA | BLOCCATO: <perché>
LA COLLEZIONE SI CREA: sì | no — <cosa è successo, verbatim se c'è un errore>
VOTO AL BUIO DOPO IL SALVATAGGIO: acceso | spento | non verificabile — <perché>
DOVE COMPARE L'ESITO: <rispetto al pulsante Salva>
CONSOLE: <righe trascritte> | pulita
RESTA NEL DATABASE: <cosa hai creato, con che nome, dove si trova> | niente
ALTRO CHE HAI VISTO: <fatti osservati, non giudizi> | niente
```

**Se il giro A non passa, gli altri tre non partono.** Dillo chiaramente.
