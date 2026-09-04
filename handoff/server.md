# Server di sviluppo — collaudo del 4 settembre 2026

> **FERMATO il 4 settembre alle 03:30**, alla chiusura della sessione. Entrambi i PID uccisi,
> **porta 5000 verificata libera**, nessun processo `dotnet` residuo. I numeri qui sotto sono
> storia: chi riprende il collaudo riavvia il server e **riscrive questa tabella**, perché i
> PID cambiano.

Avviato dal **capo**, non da un agente: su Windows la morte del padre non uccide i figli, e un
agente effimero che avvia il server lascia il processo vivo e la porta occupata. Il ciclo
successivo si collegherebbe a una **build vecchia ancora in ascolto**, riportando un esito
falso.

- URL: **http://localhost:5000**
- Ambiente: Development
- Comando: `dotnet run --launch-profile Eton`
- Build a monte: `dotnet build -warnaserror` → 0 errori, 0 avvisi; `dotnet test` → 287/287,
  sullo stato `c08bbb3` (unità 16 committata)

## PID da fermare a ciclo chiuso, **entrambi**

| PID | Processo | Ruolo |
|---|---|---|
| **5752** | `dotnet run --launch-profile Eton` | padre |
| **2376** | `blazor-devserver.dll` | figlio, **è lui che ascolta sulla 5000** |

Fermare solo il padre lascia la porta occupata dal figlio. Si fermano tutti e due, e si
verifica che la 5000 sia tornata libera.

Il PID **27244** è un nodo MSBuild residuo della build, non appartiene al server: si lascia
stare.

## Riavvio fra un giro e l'altro

**Il server va riavviato prima di ogni giro di collaudo.** Dopo un po' di build il server di
sviluppo annuncia asset che non esistono più, e la pagina si rompe per un motivo che non c'entra
con ciò che si sta provando. Chi riavvia **aggiorna i due PID qui sopra**, perché cambiano.

**Fra il giro A e il giro B il capo NON ha riavviato, deliberatamente.** La regola esiste perché
il dev-server annuncia asset di build **successive al proprio avvio**: questo è stato lanciato
dopo l'ultima compilazione e da allora nessuno ha compilato, quindi non c'è nulla che possa
essersi sfasato. Riavviare avrebbe cambiato i PID di questa pagina in cambio di niente. **Se fra
un giro e l'altro qualcuno compila, il riavvio torna obbligatorio.**

## Cosa non è un difetto, in sviluppo

- La **cache della PWA non falsa le prove**: il service worker di dev è un no-op, verificato.
- Il banner «**è disponibile una versione nuova**» che riappare a ogni avvio **è corretto**: il
  worker resta in attesa finché la pagina non viene chiusa davvero, ed è il comportamento
  documentato da chi l'ha scritto.

## Il browser

`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`. Due Chrome sono collegati e **i nomi si
scambiano a ogni riconnessione**: si identifica per `deviceId`, e solo quello vede `localhost`.
