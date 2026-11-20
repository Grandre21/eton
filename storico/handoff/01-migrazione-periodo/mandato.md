UNITÀ: 1/3 — La migrazione della descrizione e del vincolo sui periodi dei budget, scritta e non applicata

OBIETTIVO: esiste una migrazione nuova in `supabase/migrations/` (data 20261120, dopo l'ultima esistente), additiva e
sicura con il client già pubblicato, in due blocchi separati e commentati come tali:
(I) su `category_budgets` la colonna `description` text not null default '' con un check di lunghezza ≤ 200
caratteri, scrivibile in INSERT e in UPDATE dai privilegi di colonna (oggi senza grant una colonna nuova non si
scrive);
(II) l'estensione `btree_gist` nello schema `extensions` e un vincolo di esclusione con un nome esplicito che
impedisce due budget della stessa categoria nello stesso spazio con periodi che si toccano (`ends_on` null = senza
fine), preceduto da un controllo che fa fallire la migrazione con un messaggio chiaro se in tabella ci sono già
sovrapposizioni. Il blocco II deve poter essere tolto per intero senza toccare il blocco I.
**Non la applichi**: nessun agente tocca il database di produzione; la applica l'utente da DBeaver. Il resoconto
porta una sezione `SQL PER L'UTENTE` con: (1) il testo integrale della migrazione; (2) una query di sola lettura da
lanciare **prima**, che dice se in produzione esistono già sovrapposizioni (attesa: zero); (3) una query di
controllo da lanciare **dopo**, che restituisce una riga sola di valori con l'atteso accanto (colonna presente,
check presente, grant di colonna presenti, estensione presente, vincolo presente); (4) il ripiego
`NOTIFY pgrst, 'reload schema';` se compare «colonna sconosciuta»; (5) la versione senza blocco II, nel caso
l'utente non voglia l'estensione. Scritto per DBeaver. I modelli C# **non** cambiano in questa unità.

PERIMETRO: il file nuovo in `supabase/migrations/`, uno script nuovo di collaudo RLS e vincoli in `supabase/` sul
modello degli script `supabase/verifica-rls-*.sql` esistenti, `handoff/01-migrazione-periodo/` | NON TOCCARE: le
migrazioni esistenti, gli script di collaudo esistenti, `Models/`, `Services/`, `Pages/`, `Shared/`, `wwwroot/`,
`Eton.Tests/`, `docs/`, `handoff/PIANO.md`, `handoff/CODA.md`

CAPO: eton 1 (la sessione coordinatore che ti ha aperto con start_session; il nome è quello di ListAgents)

CONTRATTI: nomi decisi dal capo, li usa la 03. Non rinominarli.
  colonna `category_budgets.description` text not null default '', check `char_length(description) <= 200`.
  Privilegi: `description` in INSERT e in UPDATE per lo stesso ruolo che oggi scrive `amount`.
  vincolo di esclusione `category_budgets_no_overlap`: (`space_id` =, `category` =, periodo dei mesi coperti &&),
  dove il periodo va da `starts_on` all'ultimo giorno del mese di `ends_on` incluso, senza fine se `ends_on` è null.
  Il codice d'errore di una violazione è 23P01 (lo intercetta la 03): verificalo sul Supabase locale e scrivilo nel
  resoconto con il messaggio esatto che torna da PostgREST.
  Nessun cambiamento alle policy esistenti né all'indice unico degli aperti (resta).

STATO: prima unità del goal. Partizione e decisioni in `handoff/PIANO.md`. Il goal precedente (2.4) ha creato
`category_budgets` in `supabase/migrations/20261024000000_budget.sql`, con il suo script di collaudo
`supabase/verifica-rls-budget.sql`: sono il riferimento per stile, privilegi e collaudo. Rapporto della 2.4:
`storico/handoff/2026-11-14-CHIUSURA-budget-2.4.md`.

AUTORE DEL DIFF: `implementer`, come da `C:\Users\Andre!\.claude\protocollo.md`, che leggi per intero prima del
primo agente del ciclo. `threat-hunter` guarda in particolare i privilegi di colonna: la nuova colonna non deve
aprire la scrittura di colonne che oggi non sono scrivibili.

PROVA NEL BROWSER: non serve. Prova la migrazione **sul Supabase locale** (memorie del progetto: porte riservate da
Windows; Docker lo avvii tu e lo spegni appena finito, con `docker desktop stop`), insieme allo script di collaudo:
almeno un inserimento sovrapposto rifiutato, uno contiguo accettato, uno dopo un budget aperto rifiutato, la
descrizione oltre 200 rifiutata, e il controllo preliminare che fa fallire la migrazione su dati sovrapposti.
Scrivi nel resoconto gli errori attesi e ottenuti. Se l'estensione in locale è rifiutata, è una domanda al capo.

GATE: `mcp__synapse__build root=G:\Sviluppo\Eton` → 0 errori; `mcp__synapse__test root=G:\Sviluppo\Eton` → 0
failed. Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 3 M tok_eq

COMMIT: a lavoro finito committi su `main` nella cartella del progetto **senza push** (il push lo fa il capo).

RESOCONTO IN: G:\Sviluppo\Eton\handoff\01-migrazione-periodo\resoconto.md

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
