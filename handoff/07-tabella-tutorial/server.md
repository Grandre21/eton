# Server di sviluppo — unità 2.2-D

> ⛔ **FERMO** — fermato il 12 ottobre 2026 a fine unità: prima il figlio 1916, poi il padre 19360;
> porta 5000 verificata libera. I PID qui sotto sono storia.

Avviato dall'esecutore il 12 ottobre 2026, dal worktree `G:\Sviluppo\Eton\.claude\worktrees\unita-2.2-D-tutorial`
(branch `worktree-unita-2.2-D-tutorial`), dopo build 0/0 e test 389/389.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project G:/Sviluppo/Eton/.claude/worktrees/unita-2.2-D-tutorial/Eton.csproj --launch-profile Eton`

| PID | Ruolo |
|---|---|
| **19360** | padre (`dotnet run`) |
| **1916** | figlio, **ascolta sulla 5000** |

Riavviato una volta, dopo la correzione del top layer. Il precedente (figlio 23252, padre 19444) è fermato.

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta con
`netstat -ano | grep -E ':5000\s+.*LISTENING' || echo "PORTA 5000 LIBERA"`.
Dopo ogni build va riavviato (il devserver si congela sui manifest), e questi PID si aggiornano.
