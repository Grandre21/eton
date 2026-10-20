# Prova nel browser — unità 05 di Eton (entrate e bilancio del mese)

App: **http://localhost:5000** (server di sviluppo avviato dalla sessione, collegato al database di **produzione**).
Browser: usa solo il Chrome con `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150` (l'unico che vede `localhost`; i nomi
dei browser si scambiano a ogni riconnessione: identifica per `deviceId`). Apri una scheda nuova tua; non riusare
schede esistenti. Niente dialoghi nativi del browser (alert/confirm): se un pulsante ne apre uno, fermati e riportalo.
Ricarica la pagina all'inizio (il server è appena partito).

## Cosa è cambiato

Le entrate: un movimento ha un tipo, «Spesa» o «Entrata», scelto alla creazione e poi non più modificabile. Categorie
delle entrate: Stipendio, Rimborso, Regalo, Vendita, Altro. Il Registro e la Home mostrano spese, entrate e saldo del
mese; grafici e ripartizione per categoria restano solo spese. La tabella ha la colonna e il filtro «Tipo» e in fondo
Spese/Entrate/Saldo. Le Ricorrenti hanno un gruppo «Entrate», fuori dai totali delle spese e dalla quota da mettere da
parte.

## Dove lavorare

Spazio **«COLLAUDO GIRO B TEST»** (scegli dal selettore degli spazi). Ogni dato che crei ha la descrizione che comincia
con «PROVA COLLAUDO». Non toccare nessun altro dato, in nessuno spazio.

## Criteri (per ciascuno: passa / non passa, con la prova letta dalla pagina)

0. **Prima di creare**: su `/expenses` (mese corrente, ottobre 2026) ricopia i numeri del mese: totale spese, entrate,
   saldo, e il numero di movimenti. Su `/expenses/recurring` ricopia la quota «metti da parte … al mese» (se c'è) e i
   totali dei gruppi. Su `/` ricopia la striscia del mese.
1. **Modulo del Registro** (`/expenses`): c'è la scelta Spesa / Entrata. Con «Entrata» l'interruttore «a rate» sparisce
   (e se era acceso si spegne) e le categorie diventano Stipendio, Rimborso, Regalo, Vendita, Altro; tornando a «Spesa»
   tornano le categorie delle spese.
2. **Crea l'entrata** «PROVA COLLAUDO entrata», 1,00 €, categoria Altro, data di oggi. Nel registro la riga ha la
   pastiglia «Entrata» e l'importo «+1,00». I numeri del mese: entrate +1,00, saldo +1,00, totale spese **invariato**
   rispetto al punto 0. La ripartizione per categoria e i grafici non contengono «Altro» per effetto dell'entrata (se
   «Altro» c'era già fra le spese, il suo importo non cambia).
3. **Home** (`/`): la striscia del mese mostra entrate e saldo aggiornati come al punto 2.
4. **Dettaglio** dell'entrata (clic sulla riga, `/expenses/{id}`): il tipo si vede ma non si può cambiare; le categorie
   offerte sono quelle delle entrate.
5. **Tabella** (`/expenses/table`): colonna «Tipo» con «Entrata» sulla riga di prova; il filtro «Tipo» su «Entrata»
   lascia solo entrate, su «Spesa» solo spese; in fondo alla tabella Spese / Entrate / Saldo coerenti con il punto 2
   (con il filtro su «Tutti»). Seleziona **solo** la riga di prova: l'azione di massa sulla categoria offre le categorie
   delle entrate. Seleziona la riga di prova e una spesa (se esiste): compare la frase che la categoria si cambia a un
   tipo alla volta, e nessun cambio di categoria. **Non premere** «Cambia categoria» né «Elimina» sulle spese; deseleziona.
   Cambia periodo (mese precedente e ritorno) con la riga di prova selezionata: la pagina non si rompe (nessun errore
   in console, nessun riquadro d'errore di Blazor).
6. **Ricorrente**: `/expenses/recurring/new`, scegli «Entrata», descrizione «PROVA COLLAUDO ricorrente entrata»,
   1,00 €, categoria Stipendio, ogni mese, **inizio il 1° novembre 2026**. Salva. In `/expenses/recurring` compare nel
   gruppo «Entrate» con «+1,00»; la quota da mettere da parte e i totali degli altri gruppi sono **invariati** rispetto
   al punto 0. Apri la regola: il tipo si vede ma non si cambia. Facoltativo: su `/expenses` a novembre 2026 la
   prevista compare come entrata.
7. **Pulizia, obbligatoria anche se qualcosa fallisce**: elimina la ricorrente di prova e l'entrata di prova (dal loro
   dettaglio). Poi ricarica e verifica che i numeri di `/expenses`, `/expenses/recurring` e `/` siano **uguali** a quelli
   del punto 0. Rimetti **«Personale»** come spazio attivo e verifica dopo una ricarica. Chiudi tutte le schede che hai
   aperto.

## Ritorno

Prima riga: `ESITO: verde` oppure `ESITO: difetti`. Poi un punto per criterio (0-7) con la prova ricopiata; per la
pulizia, gli id dei dati creati ed eliminati e i numeri prima/dopo. Poi `NON PROVATO:` con ciò che non hai potuto
provare e perché. Errori di console: ricopiali.
