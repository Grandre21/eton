# Prova nel browser — unità 06 di Eton (testi dell'editor delle ricorrenti)

App: **http://localhost:5000** (server di sviluppo avviato dalla sessione, collegato al database di **produzione**).
Browser: usa solo il Chrome con `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150` (l'unico che vede `localhost`; i nomi
dei browser si scambiano a ogni riconnessione: identifica per `deviceId`). Apri una scheda nuova tua; non riusare
schede esistenti. Niente dialoghi nativi del browser (alert/confirm). Ricarica la pagina all'inizio (il server è
appena partito).

## Regola ferma: niente dati

**Non salvare niente, non premere «Salva», «Termina» né «Elimina», non aprire regole esistenti per modificarle.**
Questa prova non crea, non cambia e non cancella nessun dato, in nessuno spazio. Se per un criterio servirebbe
salvare, non lo provi: lo scrivi in `NON PROVATO:`.

## Cosa è cambiato

Nell'editor di una regola ricorrente, la frase sotto la data d'inizio ora dice «spese» o «entrate» secondo il tipo
scelto. Nient'altro di visibile deve essere cambiato.

## Criteri (per ciascuno: passa / non passa, con il testo ricopiato dalla pagina)

1. Apri `/expenses/recurring/new`. Con tipo **Spesa** (quello di partenza) metti come data d'inizio una data già
   passata (es. 1 settembre 2026) se la frase non compare già. Ricopia testualmente il paragrafo sotto la data. Atteso,
   carattere per carattere: «Se la data è già passata, le spese dei mesi trascorsi si segnano da sole: controlla che
   sia quella giusta.»
2. Sempre senza salvare, scegli il tipo **Entrata**. Ricopia lo stesso paragrafo. Atteso: «Se la data è già passata,
   le entrate dei mesi trascorsi si segnano da sole: controlla che sia quella giusta.» Ricopia anche il titolo della
   schermata (atteso «Nuova entrata ricorrente» se la descrizione è vuota).
3. Torna a **Spesa**: il paragrafo torna come al punto 1.
4. Nessun errore in console, nessun riquadro d'errore di Blazor. Poi lascia la pagina con «Chiudi» senza salvare e
   chiudi la scheda che hai aperto.

## Ritorno

Prima riga: `ESITO: verde` oppure `ESITO: difetti`. Poi un punto per criterio (1-4) con il testo ricopiato. Poi
`NON PROVATO:` con ciò che non hai potuto provare e perché. Errori di console: ricopiali.
