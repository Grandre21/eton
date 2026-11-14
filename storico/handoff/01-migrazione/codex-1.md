FATTO: `supabase/migrations/20261024000000_budget.sql` creato, 134 righe.
FATTO: `supabase/verifica-rls-budget.sql` creato, 293 righe.

DEVIAZIONI: Solo i due file richiesti creati, tramite patch. Nessuno script o suite eseguito. Comandi di lettura e verifica lanciati:

```powershell
Get-Location; rg --files -g AGENTS.md -g '!**/.git/**' -g '!**/node_modules/**'; git status --short; Get-Content supabase/migrations/20260922000000_ricorrenti.sql; Get-Content supabase/migrations/20261020000000_rate_entrate.sql; Get-Content supabase/verifica-rls-rate-entrate.sql
Get-Content -Encoding UTF8 supabase/migrations/20261024000000_budget.sql; Get-Content -Encoding UTF8 supabase/verifica-rls-budget.sql; Get-Item supabase/migrations/20261024000000_budget.sql, supabase/verifica-rls-budget.sql | ForEach-Object { '{0}: {1} righe' -f $_.Name, (Get-Content -LiteralPath $_.FullName).Count }; git -c core.excludesFile=NUL diff --check; git -c core.excludesFile=NUL status --short
git -c core.excludesFile=.git/info/exclude diff --check; git -c core.excludesFile=.git/info/exclude status --short; Select-String -Path supabase/verifica-rls-budget.sql -Pattern '[^\x00-\x7F]'
```

Il primo `git status` ha segnalato accesso negato all’ignore globale; `core.excludesFile=NUL` è stato rifiutato. Controlli Git ripetuti con percorso interno al worktree.

VERIFICA: Riletti integralmente entrambi i file; controllati manualmente nomi del contratto, privilegi, RLS, trigger, vincoli, transazioni e risultati attesi. Script senza caratteri non ASCII. Collaudo database non eseguito. Esattamente 14 errori attesi: sezioni 4a, 4b, 4c, 4d, 4e, 4f, 4g, 5b, 6a, 6b, 6c, 6d, 6e, 11a.

NOTE: nessuna.