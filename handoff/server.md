# Server di sviluppo — collaudo del goal «punti rimanenti», 19 settembre 2026

> **VIVO.** Avviato dal **capo** alle 19 settembre, dopo l'integrazione di tutte e sette le unità.
> I PID qui sotto sono **attuali**: vanno fermati entrambi a ciclo chiuso.

- URL: **http://localhost:5000**
- Ambiente: Development
- Comando: `dotnet run --launch-profile Eton`
- **Commit su cui gira: `bb77cf7`** — tutte e sette le unità integrate
- Build a monte, su albero pulito (`rm -rf obj bin` prima):
  `dotnet build Eton.sln -warnaserror --no-incremental` → **0 avvisi, 0 errori**;
  `dotnet test Eton.sln` → **310/310**

## PID da fermare a ciclo chiuso, **entrambi**

| PID | Processo | Ruolo |
|---|---|---|
| **29096** | `dotnet run --launch-profile Eton` | padre |
| **22680** | `microsoft.aspnetcore.components.webassembly.devserver` | figlio, **è lui che ascolta sulla 5000** (verificato con `Get-NetTCPConnection -LocalPort 5000 -State Listen`) |

Fermare solo il padre lascia la porta occupata dal figlio. Si fermano tutti e due, e si verifica
che la 5000 sia tornata libera.

Il PID **28760** è un nodo MSBuild residuo della build, non appartiene al server: si lascia stare.

## Perché lo avvia il capo e non un agente

Su Windows la morte del padre **non** uccide i figli: un agente effimero che avviasse il server
lascerebbe il processo vivo e la porta occupata, e il giro successivo si collegherebbe a una
**build vecchia ancora in ascolto**, riportando un esito falso.

## Riavvio fra un giro e l'altro

**Il server va riavviato prima di ogni giro se qualcuno ha compilato nel frattempo.** Il DevServer
legge i manifest degli asset solo al proprio avvio: dopo una build successiva annuncia nomi con
impronta che non esistono più, e l'app non parte. Chi riavvia **aggiorna i due PID qui sopra**.

In questo collaudo **nessuno compila**: gli agenti di prova sono in sola lettura sul codice, e il
capo non tocca i sorgenti finché il ciclo non è chiuso. Quindi un riavvio fra i due giri non serve.

## Cosa non è un difetto, in sviluppo

- La **cache della PWA non falsa le prove**: il service worker di dev è un no-op, verificato.
- Il banner «**è disponibile una versione nuova**» che riappare a ogni avvio **è corretto**: il
  worker resta in attesa finché la pagina non viene chiusa davvero. È anche la ragione per cui la
  voce 13 del `FUORI SCOPE` — il banner in condizioni vere — **non è provabile qui**: si vede solo
  sul sito pubblicato.

## Il browser

`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`. Due Chrome sono collegati e **i nomi si scambiano
a ogni riconnessione**: si identifica per `deviceId`, e solo quello vede `localhost`.

## I due giri di questo collaudo

Divisi per tema, **in sequenza e mai in parallelo**: il browser è una risorsa condivisa, e due
agenti sullo stesso Chrome si pestano senza che nessuno dei due se ne accorga.

| Giro | Cosa prova | Fonte dei criteri |
|---|---|---|
| **A — le misure** | contrasto, sovrapposizione di «Profilo», medaglione, bersagli di tocco, frecce di mese, colonna che non salta, e il **controllo negativo** su `/spaces` | resoconti 01, 02, 07 |
| **B — i comportamenti** | importo nei due stati, sovrascrittura col nome vuoto, validazione che si spegne, tavolozza, il **costo zero** del profilo, la Home al cambio spazio | resoconti 03, 04, 06 |
