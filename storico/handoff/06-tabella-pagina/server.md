# Server di sviluppo — unità 2.2-C

> ⛔ **FERMO** — fermato il 7 ottobre 2026 a fine unità: prima il figlio 27216, poi il padre 27092;
> porta 5000 verificata libera. I PID qui sotto sono storia.

Avviato dall'esecutore il 7 ottobre 2026, dal worktree `G:\Sviluppo\Eton\.claude\worktrees\unita-2.2-C`
(branch `worktree-unita-2.2-C`), dopo build 0/0 e test 389/389.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/Eton/.claude/worktrees/unita-2.2-C/Eton.csproj --launch-profile Eton`

| PID | Ruolo |
|---|---|
| **27092** | padre (`dotnet run`) |
| **27216** | figlio, **ascolta sulla 5000** |

Riavviato quattro volte: dopo le correzioni di `live-testing`, dopo quelle di `ui-critic` più lo step 8,
per l'`ui-critic` dello step 8 e dopo la sua correzione. I precedenti (10400/25740, 24080/21296,
12560/11988, 18348/23004) sono fermati.

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta con
`netstat -ano | grep -E ':5000\s+.*LISTENING' || echo "PORTA 5000 LIBERA"`.
Dopo ogni build va riavviato (il devserver si congela sui manifest), e questi PID si aggiornano.
