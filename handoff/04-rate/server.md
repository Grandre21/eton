# Server di sviluppo — unità 04 (rate)

> ⛔ **FERMO** — fermato il 20 ottobre 2026 a unità chiusa BLOCKED: prima il figlio 26772, poi il padre 31536;
> porta 5000 verificata libera. I PID qui sotto sono storia.

Avviato dalla sessione-unità il 20 ottobre 2026, dal worktree `G:\Sviluppo\wt-Eton\04-rate`
(ramo `unita-04-rate`, commit 3274a7f), dopo build 0 errori e test 407/0.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/wt-Eton/04-rate/Eton.csproj --launch-profile Eton --no-build`

| PID | Ruolo |
|---|---|
| **31536** | padre (`dotnet run`) |
| **26772** | figlio (`blazor-devserver.dll`), **ascolta sulla 5000** |

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta.
