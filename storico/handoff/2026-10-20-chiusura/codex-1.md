FATTO: applicata esattamente la modifica in `Pages/Spese.razor:490`, con rientro a 4 spazi. `PrimaDiRicaricare()` azzera anche `avvisoPiano`.

DEVIAZIONI: nessuna modifica aggiuntiva. Nessuno script o suite eseguito.

VERIFICA: diff conforme; `git diff --check` superato. Build e test non eseguiti.

NOTE: controllo finale dello stato Git fallito: `git -c core.excludesFile=NUL status --short` rifiuta `NUL` come file di esclusione.