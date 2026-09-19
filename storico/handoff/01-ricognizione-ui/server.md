# Server di sviluppo — sessione del 27 agosto 2026

Avviato dall'esecutore per la ricognizione UI/UX (Lavoro 2 del piano).

- URL: http://localhost:5000
- Ambiente: Development
- Comando: `dotnet run --launch-profile Eton`
- Build a monte: 0 errori / 0 avvisi, ricompilata prima dell'avvio

## PID da fermare a ciclo chiuso, entrambi

| PID | Processo | Ruolo |
|---|---|---|
| 3208 | `dotnet run --launch-profile Eton` | padre |
| 23912 | `blazor-devserver.dll` | figlio, è lui che ascolta sulla 5000 |

Su Windows la morte del padre non uccide il figlio: fermare solo 3208 lascia la porta
occupata da 23912, e il ciclo successivo si collega a una build vecchia ancora in ascolto.
