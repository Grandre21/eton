Progetto Eton (Blazor WASM .NET 10 + Supabase), radice G:\Sviluppo\Eton, branch main, diff non committato.
PROGETTO: nessuno (nessun CLAUDE.md di progetto; convenzioni = i file vicini)

DIFF DA REVISIONARE — solo questi file (gli altri modificati in `handoff/` non sono di questa unità):
    git -C G:/Sviluppo/Eton diff HEAD -- Pages/Spese.razor Pages/RicorrenteEdit.razor Pages/BudgetEdit.razor Services/DopoSegna.cs Eton.Tests/DopoSegnaTests.cs
(i due file nuovi sono in indice con `git add -N`, quindi compaiono nel diff)

COSA DOVEVA FARE (brief dell'implementer, riassunto fedele):
(a) Pages/Spese.razor, ScegliTipo: arrivando da «Registra una spesa» di un budget (/expenses?categoria=X, `daBudget` vero), passare il tipo a Entrata e poi di nuovo a Spesa deve riportare la categoria a quella del budget (`Categoria`), non alla prima dell'elenco.
(b) Pages/RicorrenteEdit.razor, `Cambiata` ramo Nuova: la guardia d'uscita non deve scattare per la sola scelta di categoria/cadenza/giorno/data/tipo; in creazione contano solo i campi scritti (importo, descrizione), la stessa regola di Pages/BudgetEdit.razor:158-162 (`Nuova ? !string.IsNullOrWhiteSpace(importoTesto) : ...`). Ramo !Nuova invariato. Commenti che descrivevano la regola vecchia aggiornati.
(c) Pages/BudgetEdit.razor: una frase in un elemento a sé, sotto il blocco della categoria, che dice che le spese ricorrenti della categoria consumano il budget dal giorno in cui scadono (l'elenco dei budget, Pages/Budget.razor:45, lo dice già). Nascosta quando `categoria` è "" (creazione con tutte le categorie occupate).
(d) La decisione «dopo Segna si torna ai budget?» estratta in Services/DopoSegna.cs, funzione pura `TornaAiBudget(bool daBudget, string tipo, bool aRate)`: spesa singola da budget → sì; entrata da budget → no; piano a rate da budget → no; spesa senza budget → no. Calcolata in Segna prima di `if (aRate)` perché quel ramo rimette aRate = false. Test xunit in Eton.Tests/DopoSegnaTests.cs.
Vincoli: nessuna resa grafica nuova (niente colori, spaziature, classi o componenti nuovi); nessun tocco a supabase/, Models/, Services/BudgetRepository.cs, Services/CalcoliBudget.cs, Pages/Budget.razor, wwwroot/. Budget di complessità: un solo tipo nuovo, nessun helper in più.

FILE DI CONTESTO UTILI:
- Shared/PaginaEditor.cs (Cambiata, GuardaUscita, NavigationLock degli editor)
- Services/CategorieSpesa.cs (Conosciuta, Di; «Altro» vale per entrambi i tipi)
- Models/Expense.cs:66-72 (TipoMovimento)

GATE già eseguiti dall'implementer: build 0 errori 0 warning; test 0 failed, 446 passed.
Sei in sola lettura: non compilare e non lanciare test.
