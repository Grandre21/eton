# Server di sviluppo — unità 06 (ritocchi)

> ⛔ **FERMO** — fermato dopo live-testing (verde): prima il figlio 25848, poi il padre 23920; porta 5000
> verificata libera. I PID qui sotto sono storia.

Avviato dalla sessione-unità il 20 ottobre 2026, dal worktree `G:\Sviluppo\wt-Eton\06-ritocchi`
(ramo `unita-06-ritocchi`, diff non ancora committato), dopo build 0 errori e test 417/0.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/wt-Eton/06-ritocchi/Eton.csproj --launch-profile Eton --no-build`

| PID | Ruolo |
|---|---|
| **23920** | padre (`dotnet run`) |
| **25848** | figlio, **ascolta sulla 5000** |

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta.
