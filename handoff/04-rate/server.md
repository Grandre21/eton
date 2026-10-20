# Server di sviluppo — unità 04 (rate)

> ⛔ **FERMO** — fermato il 20 ottobre 2026 a unità chiusa BLOCKED: prima il figlio 26772, poi il padre 31536;
> porta 5000 verificata libera. I PID qui sotto sono storia.

## Riavviato il 20 ottobre 2026 per riprendere il §7 (risposta del capo: «sì, riprendi adesso»)

Stesso comando, stesso commit di lavoro. PID padre 24836, PID figlio 21592 — fermati dopo la correzione della data,
porta verificata libera.

## Riavviato sulla build della correzione della data (commit 0200873)

PID padre 4844, PID figlio 26416 — fermati dopo la correzione della colonna, porta verificata libera.

## Riavviato sulla build della correzione della colonna del modulo

PID padre 21688, PID figlio 30732.

> ⛔ **FERMO** — fermato a fine unità (prova finale verde): prima il figlio 30732, poi il padre 21688; porta 5000
> verificata libera. Nessun server dell'unità resta acceso.

Avviato dalla sessione-unità il 20 ottobre 2026, dal worktree `G:\Sviluppo\wt-Eton\04-rate`
(ramo `unita-04-rate`, commit 3274a7f), dopo build 0 errori e test 407/0.

- URL: **http://localhost:5000**
- Comando: `dotnet run --project /g/Sviluppo/wt-Eton/04-rate/Eton.csproj --launch-profile Eton --no-build`

| PID | Ruolo |
|---|---|
| **31536** | padre (`dotnet run`) |
| **26772** | figlio (`blazor-devserver.dll`), **ascolta sulla 5000** |

Da fermare a fine unità: prima il figlio, poi il padre, e verificare la porta.
