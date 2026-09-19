# Server di sviluppo — goal «tutto ciò che rimane», notte fra il 19 e il 20 settembre 2026

> **VIVO.** **Riavviato** il 20 settembre 2026 alle 01:40, dopo l'integrazione dell'unità 01 che ha
> modificato `wwwroot/css/app.css`. Il primo avvio era delle 23:57 del 19 settembre; i suoi PID
> (24148 e 21868) sono **storia** e sono stati fermati entrambi, con la porta verificata libera
> prima di riavviare.
> I PID qui sotto sono **quelli veri di adesso**: chi riavvia il server **riscrive la tabella**,
> perché cambiano a ogni avvio.

- URL: **http://localhost:5000**
- Ambiente: Development
- Comando: `dotnet run --launch-profile Eton`
- **Commit su cui gira: `fe438ef`** — l'unità 01 integrata (voce 9 chiusa), la 01b aperta
- Build a monte, su albero pulito (`rm -rf obj bin` prima):
  `dotnet build Eton.sln -warnaserror --no-incremental` → **0 avvisi, 0 errori**;
  `dotnet test Eton.sln` → **310/310** (misurato prima della pulizia, sullo stesso albero)

## PID da fermare a ciclo chiuso, **entrambi**

| PID | Processo | Ruolo |
|---|---|---|
| **26652** | `dotnet run --launch-profile Eton` | padre |
| **25108** | `microsoft.aspnetcore.components.webassembly.devserver` | figlio, **è lui che ascolta sulla 5000** |

Fermare solo il padre lascia la porta occupata dal figlio. Si fermano tutti e due, e si verifica
che la 5000 sia tornata libera.

**La forma che funziona**, misurata stanotte su entrambi i riavvii:

    # fermare
    foreach ($id in 25108, 26652) { Stop-Process -Id $id -Force }
    # verificare, dal tool Bash
    netstat -ano | grep -E ':5000\s+.*LISTENING' || echo "PORTA 5000 LIBERA"
    # riavviare, dalla radice del progetto, in background
    dotnet run --launch-profile Eton

⚠️ **`Get-NetTCPConnection` è stato negato dal classificatore della modalità automatica**, con la
motivazione «Git Destructive» — un falso positivo su un comando di sola lettura. La via che
funziona è `netstat -ano | grep -E ':5000\s+.*LISTENING'` dal tool `Bash`.

## Perché lo avvia il capo e non un agente

Su Windows la morte del padre **non** uccide i figli: un agente effimero che avviasse il server
lascerebbe il processo vivo e la porta occupata, e il giro successivo si collegherebbe a una
**build vecchia ancora in ascolto**, riportando un esito falso.

## Riavvio fra un giro e l'altro

**Il server va riavviato prima di ogni giro se qualcuno ha compilato nel frattempo.** Il DevServer
legge i manifest degli asset solo al proprio avvio: dopo una build successiva annuncia nomi con
impronta che non esistono più, e l'app non parte. Chi riavvia **aggiorna i due PID qui sopra**.

⚠️ **In questo goal qualcuno compila, ed è la differenza col ciclo precedente.** Le unità lavorano
in worktree propri e non compilano; ma il capo compila a ogni integrazione su `main`. **Ogni
integrazione obbliga a riavviare il server prima della prova successiva.**

## A cosa serve questo avvio

**Diagnosi della voce 6 prima di aprire l'unità 01.** Il pulsante «?» che non risponde al primo
clic è un difetto osservabile solo nel browser: un'unità che correggesse sulla base della sola
analisi statica rischierebbe di sbagliare causa e di scoprirlo solo al collaudo finale. Il fatto
osservato entra nel mandato al posto dell'ipotesi.

Le due rotte su cui la ricognizione del 19 settembre l'ha riprodotto:
`/collections/{id}/edit` e `/collections/{id}/items/{id}`.

## Cosa non è un difetto, in sviluppo

- La **cache della PWA non falsa le prove**: il service worker di dev è un no-op, verificato.
- Il banner «**è disponibile una versione nuova**» che riappare a ogni avvio **è corretto**: il
  worker resta in attesa finché la pagina non viene chiusa davvero. È anche la ragione per cui la
  voce 29 — il banner in condizioni vere — **non è provabile qui**: si vede solo sul pubblicato.

## Il browser

`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`. Due Chrome sono collegati e **i nomi si scambiano
a ogni riconnessione**: si identifica per `deviceId`, e solo quello vede `localhost`.

⚠️ **I dialoghi nativi bloccano il plugin.** Non premere «Elimina» su una spesa: il ciclo
precedente ha osservato lì un dialogo nativo che il codice non spiega, e quella è la voce 35.
