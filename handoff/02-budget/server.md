# Server di sviluppo — unità 02 (budget)

> ⛔ **FERMO** — fermato il 25 ottobre 2026 alle 02:04 su risposta del capo (unità chiusa PARZIALE, prova
> nel browser rinviata): prima il figlio 22864, poi il padre 24868; porta 5000 verificata libera. I PID
> qui sotto sono storia.

Avviato dalla sessione-unità il 25 ottobre 2026, dal worktree `G:\Sviluppo\wt-Eton\02-budget`
(ramo `unita-02-budget`, commit 9d356d0), dopo build 0 errori e test 432/0.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/wt-Eton/02-budget/Eton.csproj --launch-profile Eton --no-build`

| PID | Ruolo |
|---|---|
| **24868** | padre (`dotnet run`) |
| **22864** | figlio (`blazor-devserver.dll`), **ascolta sulla 5000** |

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta con
`netstat -ano | grep -E ':5000\s+.*LISTENING' || echo "PORTA 5000 LIBERA"`.
