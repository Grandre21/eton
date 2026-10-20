# Server di sviluppo — unità 02 (gruppi e quota)

> ⛔ **FERMO** — fermato il 20 ottobre 2026 a fine unità: prima il figlio 22536, poi il padre 27816;
> porta 5000 verificata libera. I PID qui sotto sono storia.

Avviato dalla sessione-unità il 20 ottobre 2026, dal worktree `G:\Sviluppo\wt-Eton\02-gruppi-quota`
(ramo `unita-02-gruppi-quota`), dopo build 0/0 e test 398/398 sul diff corretto.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/wt-Eton/02-gruppi-quota/Eton.csproj --launch-profile Eton --no-build`

| PID | Ruolo |
|---|---|
| **27816** | padre (`dotnet run`) |
| **22536** | figlio (`blazor-devserver.dll`), **ascolta sulla 5000** |

Riavviato dopo la correzione dell'indentazione (build 0/0, test 398/398); i primi PID, 18936/23820,
sono fermati e la porta era stata verificata libera.

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta con
`netstat -ano | grep -E ':5000\s+.*LISTENING' || echo "PORTA 5000 LIBERA"`.
