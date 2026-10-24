# Il piano del collaudo — goal «tutto ciò che rimane»

Scritto dal capo il **20 settembre 2026**, mentre l'unità 05 è ancora in corso. Esiste su disco e
non in chat per la ragione che questa notte ha già insegnato due volte: **una sessione può morire
fra un passo e l'altro**, e ciò che vive solo nel contesto muore con lei.

## LA REGOLA CHE GOVERNA TUTTO IL COLLAUDO

⚠️ **Le misure attese non si inventano: si ricopiano.** Ogni resoconto di questo goal contiene una
sezione `LA MISURA ATTESA PER IL COLLAUDO`, scritta da chi ha fatto il lavoro, con gli snippet
pronti da eseguire nel browser e i valori attesi.

**Il ciclo precedente ha perso una clausola esattamente qui**, ed è la sua lezione più importante:
il brief si compone ricopiando dai resoconti, quindi **una clausola che nessun resoconto contiene
non ha nessuno che la ricopi**. Non fu una dimenticanza di chi scrisse il brief: era una proprietà
del modo in cui il brief si costruisce.

**Quindi le voci non assegnate a un'unità vanno aggiunte a mano**, e sono elencate qui sotto in una
sezione propria proprio per non sparire.

## L'ORDINE, CHE NON È NEGOZIABILE

1. **Server avviato dal capo**, con `handoff/server.md` riscritto: porta, PID di **entrambi** i
   processi, e il commit su cui gira. ⚠️ **Va riavviato dopo l'integrazione della 05**, perché il
   DevServer legge i manifest degli asset solo al proprio avvio e servirebbe la build precedente.
2. **`live-testing`**, con il brief composto **ricopiando** le misure dai sei resoconti.
3. **`ui-critic`, solo se `live-testing` torna `ESITO: verde`.** Su una schermata con difetti
   funzionali non si lancia.
4. **Le voci che nessuna unità ha prodotto**, sotto.

## DOVE STANNO LE MISURE, RESOCONTO PER RESOCONTO

| Resoconto | Cosa contiene |
|---|---|
| `01-punto-interrogativo` | l'allineamento del «?» con la **prima riga** di un titolo lungo — con un criterio **relativo** che legge `lineHeight` dal DOM invece di ricavarlo, e la spiegazione di perché la coordinata assoluta della diagnosi era sbagliata |
| `01b-home-e-montaggio` | il pulsante di aiuto **esiste dal primo render**; ogni pagina si monta **una volta sola** (si legge dal pannello di rete); più una sezione `MODI DI FALLIRE` con sette righe, di cui due già escluse per decompilazione |
| `02-voti-e-recensioni` | il cursore del voto ha un nome; il segnaposto dell'assenza di voto (36px, niente alone, grigio e non verde); il conflitto che non mente e **non butta via il testo digitato**; la frase d'errore che non è più doppia |
| `03-accesso-e-profilo` | al logout **non resta nessuna chiave** — e l'avvertenza che la prova va fatta **in una scheda sola**, perché con due il difetto della corsa fra schede la fa fallire legittimamente |
| `04-controlli-e-campi` | i nomi accessibili **per riga** (con il caso che conta: due campi nuovi senza etichetta devono avere due nomi diversi); il paragrafo dell'importo a margine zero; le **tre altezze** che tornano a 48; il primo campo a 12px dal secondo |
| `05-scala-e-metro` | *(da riempire al suo rientro)* |

## GLI EFFETTI COLLATERALI DICHIARATI IN ANTICIPO

L'unità 04 li ha **dichiarati invece di nasconderli**, con i numeri prima e dopo. Non sono difetti:
sono il prezzo dichiarato della regola sull'interlinea, e vanno **guardati, non temuti**.

- il campo del titolo scende da ~57 a ~51 px — resta sopra i 48, quindi non perde niente come
  bersaglio di tocco;
- il campo dell'importo scende da ~82 a ~71 px;
- la **textarea della nota non cambia**: è esclusa dalla regola, e **se cambiasse sarebbe un
  difetto**.

**Se alla prova uno dei due stona, la decisione è di chi guarda la schermata**, non dell'unità che
l'ha scritto. È il motivo per cui l'unità 04 non ha accolto il rimedio proposto dal suo revisore ma
ha accolto il fatto, e l'ha scritto qui.

## LE VOCI CHE NESSUNA UNITÀ HA PRODOTTO — vanno aggiunte a mano

Sono le clausole che la `PARTIZIONE` assegna al collaudo, e **nessun resoconto le contiene**.

| Voce | Cosa provare | Note |
|---|---|---|
| **27** | a 360px, «Sì, elimina» e «Annulla» restano sulla stessa riga? | È la clausola **scoperta** del ciclo precedente. Il componente è **in pagina**, non un dialogo nativo: si rende premendo «Elimina» su una **collezione**, non su una spesa |
| **31** | `Sovrascrivi()` col nome vuoto: stesso elemento in **due schede** | Non passa da dialoghi nativi. Atteso: «Il nome dell'elemento non può essere vuoto.» **una volta sola** — v. anche la voce 26 nel resoconto della 02 — e la scheda di conflitto che **resta aperta** |
| **32** | i collegamenti «Tutte» della schermata iniziale sono alti **48** | Una riga di misura. La regola esiste ed è stata letta; manca l'osservabile |
| **33** | la riserva sulla colonna che non salta | **Si chiude dichiarando**: la proprietà riserva la colonna sempre, quindi i due casi non possono più divergere per costruzione. Non richiede un altro giro |

⚠️ **E la prova che nessuno può fare da qui**: il ramo di sola lettura dell'importo (voce 30)
richiede un **secondo account**, perché ogni spazio raggiungibile ha un solo membro. Va dichiarata
**non provata**, non verde.

## I DIVIETI DEL BROWSER

- ⚠️ **Non premere «Elimina» su una spesa.** Il ciclo precedente ha osservato lì un dialogo nativo
  che il codice non spiega — è la voce **35**, e riprovarla significa bloccare il plugin.
- **I dialoghi nativi bloccano il plugin**: qualunque prova che passi da un `confirm()` non è
  eseguibile da un agente e va girata all'utente.
- **Il browser giusto ha `deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`.** Due Chrome sono
  collegati e i nomi si scambiano a ogni riconnessione: si identifica per `deviceId`, e **solo
  quello vede `localhost`**.
- **Il banner «versione nuova» non è un difetto in sviluppo**, e il service worker di dev è un no-op
  verificato: la cache non falsa le prove.

## COSA RESTA ALL'UTENTE, E NON SI CHIUDE QUI

1. **I tre dati di prova** sul database di sviluppo — la spesa, lo spazio e la collezione di
   collaudo del ciclo precedente. Passano da dialoghi nativi: **tre gesti suoi**.
2. **La voce 29**, il banner di aggiornamento in condizioni vere: si vede **solo sul sito
   pubblicato**.
3. **La voce 28**, la barra gialla di Blazor: richiede un giro a rete bloccata riportando
   **verbatim** lo snippet di sovrascrittura, che nessuno ha.
4. **Le due domande da una riga** in `APERTO`, entrambe con il rimedio già istruito.
5. **I due testi** di `handoff/configurazione-da-applicare.md`, che toccano superfici di
   configurazione.
