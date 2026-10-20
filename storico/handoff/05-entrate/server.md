# Server di sviluppo — unità 05 (entrate)

> ⛔ **FERMO** — fermato dopo live-testing (verde) e ui-critic, prima del brief 4: prima il figlio 29944, poi il
> padre 23672; porta 5000 verificata libera. I PID qui sotto sono storia.

## Riavviato sulla build del brief 4 (correzioni di ui-critic, non ancora committate)

Stesso comando. PID padre 27060, PID figlio 3680.

> ⛔ **FERMO** — fermato a fine unità dopo la prova breve: prima il figlio 3680, poi il padre 27060; porta 5000
> verificata libera. Nessun server dell'unità resta acceso.

Avviato dalla sessione-unità il 20 ottobre 2026, dal worktree `G:\Sviluppo\wt-Eton\05-entrate`
(ramo `unita-05-entrate`, commit 2f411e7), dopo build 0 errori e test 417/0.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/wt-Eton/05-entrate/Eton.csproj --launch-profile Eton --no-build`

| PID | Ruolo |
|---|---|
| **23672** | padre (`dotnet run`) |
| **29944** | figlio (`blazor-devserver.dll`), **ascolta sulla 5000** |

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta.
