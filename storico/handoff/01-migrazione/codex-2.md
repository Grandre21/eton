FATTO: `supabase/migrations/20261024000000_budget.sql`, blocco originario righe 30–43: spostati i tre vincoli nominati dentro `create table`, conservando commenti ed espressioni; eliminati i relativi `drop/add`. Resto invariato.

DEVIAZIONI: nessuna. Nessuno script o suite eseguito. Comando di sola lettura eseguito due volte: `Get-Content -Encoding UTF8 supabase/migrations/20261024000000_budget.sql`.

VERIFICA: riletto integralmente il file; confermati i tre nomi esatti, virgole e chiusura `);` del `create table`. Blocco `do` del tetto rate invariato. Docker e Supabase non avviati.

NOTE: nessuna.