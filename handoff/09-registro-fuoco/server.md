# Server di sviluppo — unità 2.2-F

> ⛔ **FERMO** — fermato il 12 ottobre 2026 a fine unità: prima il figlio 6108, poi il padre 26692 (già
> terminato con il figlio); porta 5000 verificata libera. I PID qui sotto sono storia.

Avviato dall'esecutore il 12 ottobre 2026, dal worktree `G:\Sviluppo\Eton\.claude\worktrees\unita-2.2-F`
(branch `worktree-unita-2.2-F`), dopo build 0/0 e test 390/390, sul diff del punto 1.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/Eton/.claude/worktrees/unita-2.2-F/Eton.csproj --launch-profile Eton --no-build`

| PID | Ruolo |
|---|---|
| **26692** | padre (`dotnet run`) |
| **6108** | figlio (`blazor-devserver.dll`), **ascolta sulla 5000** |

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta con
`netstat -ano | grep -E ':5000\s+.*LISTENING' || echo "PORTA 5000 LIBERA"`.
Dopo ogni build va riavviato, e questi PID si aggiornano.
