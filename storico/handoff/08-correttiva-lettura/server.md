# Server di sviluppo — unità 2.2-E

> ⛔ **FERMO** — fermato il 12 ottobre 2026 a fine unità: prima il figlio 23228, poi il padre 2040;
> porta 5000 verificata libera. I PID qui sotto sono storia.

Avviato dall'esecutore il 12 ottobre 2026, dal worktree `G:\Sviluppo\Eton\.claude\worktrees\unita-2.2-E`
(branch `worktree-unita-2.2-E`), dopo build 0/0, **sul codice ancora intatto** (riproduzione del punto 2).

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/Eton/.claude/worktrees/unita-2.2-E/Eton.csproj --launch-profile Eton --no-build`

| PID | Ruolo |
|---|---|
| **2040** | padre (`dotnet run`) |
| **23228** | figlio, **ascolta sulla 5000** |

Riavviato il 12 ottobre dopo le correzioni (build 0/0, test 390/390). Il primo server (padre 13996,
figlio 23352), usato per la riproduzione del punto 2, è fermato: prima il figlio, poi il padre,
porta 5000 verificata libera.

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta con
`netstat -ano | grep -E ':5000\s+.*LISTENING' || echo "PORTA 5000 LIBERA"`.
Dopo ogni build va riavviato, e questi PID si aggiornano.
