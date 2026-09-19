# L'ambiente del collaudo — vale per tutti e quattro i giri

Scritto una volta sola perché i quattro brief non lo ripetano. **Se un brief e questo file
divergono, vince il brief**, che conosce il proprio giro.

## Il server

- **URL: `http://localhost:5000`**, già avviato dal **capo**. PID in `handoff/server.md`.
- **Non avviarlo e non fermarlo tu.** Su Windows la morte del padre non uccide i figli: un
  agente che avvia il server lascia il processo vivo e la porta occupata, e il giro dopo si
  collega a una build vecchia ancora in ascolto riportando un esito falso.
- Se la porta non risponde, **fermati e dillo**: il capo lo riavvia fra un giro e l'altro.

## Il browser

**`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.** Due Chrome sono collegati e **i nomi si
scambiano a ogni riconnessione**: identifica per `deviceId`, mai per nome, e solo quello vede
`localhost`. Selezionalo con `select_browser` **senza chiedere niente a nessuno** — l'utente non
è raggiungibile, e la scelta è già stata presa in una sessione precedente.

**Playwright non si usa, non si installa, non si propone e non si nomina.** La prova
sull'interfaccia si fa solo con questo plugin. Ciò che non si può provare così è un limite da
riportare, non da aggirare.

## La sessione, e cosa fare se non c'è

**È già autenticata**, verificato dal capo il 4 settembre: aprendo la radice si arriva sulla
Home dello spazio «Personale». Se ti trovassi su `/benvenuto` senza sessione, **fermati e
dillo**: non c'è modo per te di fare un accesso Google, e il giro va rimandato all'utente.

## Il vincolo che pesa di più: il database è quello VERO

Lo sviluppo punta a **`fdqedhgvpneuybtykamf.supabase.co`**, cioè alla produzione. Non esiste un
database locale, non esiste un account di collaudo. **Tutto ciò che crei è un dato vero
dell'utente.**

1. **Ogni cosa che crei porta nel nome `COLLAUDO 4 SET`.**
2. **Crei il minimo indispensabile** per la prova che stai facendo.
3. **Non cancelli e non modifichi NIENTE che non abbia creato tu.** Se una prova richiede di
   eliminare qualcosa, elimini solo la tua roba. Nel dubbio non farlo, e scrivilo.
4. **Elenchi nell'esito tutto ciò che resta**, con nome e posizione, perché l'utente lo trovi.

Lo spazio «Personale» era vuoto quando il capo l'ha guardato. Roba che non hai creato tu è
dell'utente: si guarda, non si tocca.

## Cosa NON è un difetto, in sviluppo

- Il banner «**è disponibile una versione nuova**» che riappare a ogni avvio. Il service worker
  di dev è un no-op verificato, e la ricomparsa è il comportamento documentato da chi l'ha
  scritto: il worker resta in attesa finché la pagina non viene chiusa davvero.
- Un primo caricamento lento: è Blazor WebAssembly che scarica il runtime.
- `prefers-reduced-motion` **non è rispettato**, per scelta dell'utente dal 24 agosto 2026.
  L'animazione c'è sempre. Non segnalarlo.

## Come si riferisce

**Fatti osservati, non giudizi.** «Il riquadro compare 40px sotto il pulsante» è un fatto; «la
posizione è poco chiara» non lo è. Se trascrivi un messaggio a schermo, **verbatim**: la
differenza fra una frase nostra e il JSON grezzo di PostgreSQL è esattamente ciò che tre unità
hanno passato il lavoro a correggere.

**La console del browser si legge sempre**, in ogni giro. Nessuno l'aveva mai guardata prima del
4 settembre. Qualunque errore, avviso o riga `[Auth]` va trascritta.

Gli screenshot si salvano in `handoff/17-collaudo/`.

**Non correggi codice e non ne proponi.** Osservi e riferisci: la correzione è del capo.
