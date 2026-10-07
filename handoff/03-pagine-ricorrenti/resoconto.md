UNITÀ: 3 — ESITO: FATTO
Verifica: `dotnet build Eton.sln -warnaserror` → Avvisi: 0, Errori: 0 · `dotnet test Eton.sln --no-build` → Superati: 335, Non superati: 0 (2026-09-23, branch `worktree-unita-03-pagine-ricorrenti`)

TOCCATI:
- `Services/RecurringExpenseRepository.cs` → +20/−14 (record `DatiRegola`, firme di `CreaAsync`/`SalvaAsync`)
- `Services/CalcoliRicorrenti.cs` → +43/−0 (`Cadenza`, `GiornoInParole`, costante `AnnoNonBisestile`)
- `Eton.Tests/CalcoliRicorrentiTests.cs` → +43/−0 (7 test su `Cadenza`)
- `Shared/NavigazioneSpese.razor` → +27/−0 (nuovo)
- `Pages/Ricorrenti.razor` → +119/−0 (nuovo)
- `Pages/RicorrenteEdit.razor` → +535/−0 (nuovo)
- `Pages/Spese.razor` → +2/−0 (solo `<NavigazioneSpese Attiva="expenses" />` dopo la testata)
- `wwwroot/css/app.css` → +40/−0 (sezione «spese ricorrenti» in fondo: `.sotto-nav`, `.sotto-nav a`, `.sotto-nav a.attiva`, `.riga-terminata`)

REVIEW:
review: A — DatiRegola e cadenza in parole
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 1
  checker         nessuna istruttoria
  checker         VERDETTI: risolti 2 · non risolti 0 · non verificabili 0   (istruzione dei FIX, condivisa con B1)

review: B1 — elenco delle ricorrenti e sotto-navigazione
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 2
  checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 2 · non risolti 0 · non verificabili 0   (istruzione dei FIX, condivisa con A)

review: B2 — editor di una spesa ricorrente
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 4
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 2 · non risolti 0 · non verificabili 0   (istruzione dei FIX)

CONTRATTI:
- Cambio di firma (eseguito per primo): `grep` prima della modifica → nessun chiamante di `CreaAsync`/`SalvaAsync` di `RecurringExpenseRepository`. Firme risultanti:
  - `Services/RecurringExpenseRepository.cs:11` — `public sealed record DatiRegola(decimal Importo, string Descrizione, string Categoria, int OgniMesi, int Giorno, DateTime Inizio);`
  - `Services/RecurringExpenseRepository.cs:49` — `public async Task<RecurringExpense> CreaAsync(Guid spazioId, Guid pagante, DatiRegola dati)`
  - `Services/RecurringExpenseRepository.cs:78` — `public async Task<RisultatoSalvataggio<RecurringExpense>> SalvaAsync(Guid regolaId, int versioneLetta, DatiRegola dati)`
- Funzione pura aggiunta, consentita dal mandato: `Services/CalcoliRicorrenti.cs:96` — `public static string Cadenza(int ogniMesi, int giorno, DateTime inizio)` («ogni mese, il 5» · «ogni 2 mesi, l'8» · «ogni 3 mesi, l'ultimo giorno» · «ogni anno, il 1° marzo» · «ogni anno, l'ultimo giorno di febbraio»).
- Consumati dall'unità 2 senza modifiche: `ElencaAsync`, `LeggiAsync`, `TerminaAsync`, `EliminaAsync`, `CalcoliRicorrenti.Prossima`.
- Rotte nuove: `Pages/Ricorrenti.razor:1` `@page "/expenses/recurring"` · `Pages/RicorrenteEdit.razor:1-2` `@page "/expenses/recurring/new"` e `@page "/expenses/recurring/{Id:guid}"`.
- Componente: `Shared/NavigazioneSpese.razor:15` — `[Parameter, EditorRequired] public string Attiva { get; set; } = "";` (valori: `"expenses"`, `"expenses/recurring"`). Voce «Tabella» dichiarata con `Visibile: false`.
- Fatto 1 dell'unità 2 (`EliminaAsync` lancia 23503) — **come l'editor sa se ci sono occorrenze**: dal watermark, non da una query. `regola.MaterializedThrough is null` ⇒ nessuna spesa scritta, perché `ExpenseRepository.MaterializzaAsync` (righe 132-143) alza il watermark solo dopo un upsert riuscito. Con watermark valorizzato «Elimina» non è offerto, e una frase dice perché e indica «Termina» (`Pages/RicorrenteEdit.razor:212-222`). Rete di sicurezza: il `catch` di `Elimina` dice fatto, causa (può aver segnato delle spese nel frattempo) e azione. Due casi limite accettati e scritti nel commento: watermark non avanzato dopo un upsert riuscito → Elimina offerto e poi rifiutato dal catch; occorrenze cancellate tutte a mano → Elimina non offerto anche se riuscirebbe. **Da ratificare.**
- Fatto 2 dell'unità 2 (il watermark alza `version`): `Conflitto` da `SalvaAsync` **e** da `TerminaAsync` apre `SchedaConflitto`; non si sopprime niente. L'Aiuto della pagina lo dice: «…o se nel frattempo Eton ci segna una spesa del mese, il salvataggio si ferma…».
- Contratto degli editor (`Shared/PaginaEditor.cs`): `@inherits PaginaEditor`, `Cambiata` override, `<NavigationLock>` dentro il ramo del modulo, `Esci(...)` dopo Crea (`replace: true`) ed Elimina, nessun `@inject NavigationManager`, `href="@(occupato ? null : "expenses/recurring")"` con `null` letterale.

ADJUDICA:
- A · backend-expert · `Services/CalcoliRicorrenti.cs` anno `2001` senza nome → fondato → corretto: `private const int AnnoNonBisestile = 2001;` + `var ultimoDelMese = DateTime.DaysInMonth(AnnoNonBisestile, mese);`
  verificato: risolto — Services/CalcoliRicorrenti.cs:24-26,108-109 «il letterale 2001 è sostituito dalla costante nominata e commentata AnnoNonBisestile; il valore numerico è invariato»
- B1 · conformity (checker: fondato) + backend-expert #1, stesso difetto · `Pages/Ricorrenti.razor:71-85` `Terminata(r)` ricalcolava `Prossima(r)` due volte per riga, e il commento «calcolato una sola volta» era falso → fondato → corretto: `var prossima = Prossima(r);` riusata, `Terminata` rimossa.
  verificato: risolto — Pages/Ricorrenti.razor:71-85 «Prossima(r) compare una sola volta nel blocco (riga 72) … Terminata … nessuna occorrenza, il metodo non esiste più»
- B1 · backend-expert #2 · `wwwroot/css/app.css:2670-2703` `.sotto-nav` ricopia `.schede-testo` invece di condividerne il blocco con una lista di selettori → fondato nel merito, **fuori scope**: la correzione modifica regole esistenti (`.schede-testo`, riga 1627), e il mandato limita `app.css` alle «sole regole nuove». La resa la rifà la 2.1-bis.
- B2 · bug-hunter (checker: fondato) · `Pages/RicorrenteEdit.razor:265-269` in creazione `Cambiata` ignorava cadenza, giorno, categoria e data, quindi la guardia di uscita non scattava → fondato → corretto: il ramo `Nuova` confronta anche i quattro campi con i loro default; la riga «Scrivi importo e descrizione…» segue `!Valido`. Riletto da me (dati): `Pages/RicorrenteEdit.razor:268-270`.
  verificato: risolto — Pages/RicorrenteEdit.razor:268-273 «Cambiata nel ramo Nuova ora confronta anche categoria, ogniMesi, giorno e inizio … Il difetto originale non è più raggiungibile»; default coincidenti con Pages/RicorrenteEdit.razor:234-240 e 299-305
- B2 · conformity (checker: fondato) · la pagina chiamava l'entità in tre modi, «regola», «spesa ricorrente» e «spesa» → fondato → corretto: «spesa ricorrente» in ogni testo visibile, e il conteggio dell'elenco dice «N spese ricorrenti».
  verificato: risolto — Pages/RicorrenteEdit.razor:194 «<h2>Fermare questa spesa ricorrente</h2>» · «ogni occorrenza di "questa spesa" (righe 15, 27, 47, 194, 335, 417, 491) è seguita da "ricorrente"»
- B2 · backend-expert #1 · la frase del permesso è scritta nella pagina e non in `Permessi.Spiegazione` → fondato nel merito, **fuori scope** (`Services/Permessi.cs` è fuori perimetro). Corretto solo il commento (`Pages/RicorrenteEdit.razor:51`), che rimandava «al brief» e ora dice dove la frase andrà spostata.
- B2 · backend-expert #2 · `TIPO: progetto` · la copia regola→sei campi è ripetuta tre volte (righe ~316, ~397, ~427); proposti gli helper `Mostra`/`Dati` → **all'utente, non applicato**: applicarlo solo qui creerebbe una forma diversa da SpesaEdit, NoteEdit e ItemEdit; applicarlo a tutti è un'unità a sé.
- B2 · backend-expert #3 · leggibilità · lo switch di `Termina` ricopia tre casi su quattro di quello di `SalvaCon` → **scartato**: l'helper `Applica(esito, verbo)` proposto aggiunge un'indirezione per risparmiare tre righe per caso, i due rami `Salvata` fanno cose diverse (Termina non riallinea i campi del modulo, di proposito), e nessun editor fratello ha un helper del genere.
- B2 · backend-expert #4 · `TIPO: progetto` · `occupato || !PuoIntervenire` è ripetuto sette volte; proposto `Bloccato` → **all'utente, non applicato**: è la forma di tutti gli editor (CollectionEdit 17 occorrenze, SpesaEdit 5), da uniformare altrove o da nessuna parte.
- Campione sugli infondati (§5): il checker non ne ha dichiarati. Ho riaperto di persona l'unico rilievo che tocca dati, il #1 di B2 su `Cambiata`, alle righe 268-270, e la correzione regge.

FUORI SCOPE:
- `wwwroot/css/app.css`: `.sotto-nav` duplica le dichiarazioni di `.schede-testo`, invece di condividerle con una lista di selettori (backend-expert B1 #2). Da unificare nella 2.1-bis, che rifà la resa.
- `Services/Permessi.cs`: manca un `Oggetto.Ricorrente`, e la frase del permesso vive in `Pages/RicorrenteEdit.razor:54` (backend-expert B2 #1).
- Decisioni di progetto per l'utente (backend-expert B2, `TIPO: progetto`): helper `Mostra`/`Dati` per la copia modello↔modulo, e `Bloccato` al posto di `occupato || !PuoIntervenire`. Hanno senso solo se applicati a tutti gli editor.

GATE:
`dotnet build Eton.sln -warnaserror` → Avvisi: 0 · Errori: 0 · Compilazione completata.
`dotnet test Eton.sln --no-build` → Superato! Non superati: 0 · Superati: 335 · Totale: 335 (328 prima dell'unità + 7 nuovi su `Cadenza`).

SCOSTAMENTI:
- Decisioni prese da me, **da ratificare**, ciascuna annullabile con un revert locale:
  1. occorrenze rilevate dal watermark `MaterializedThrough` e non con una query su `expenses` (motivo e casi limite in CONTRATTI);
  2. «Termina» scrive `ends_on = oggi` («da domani non segna più spese»), con doppia conferma tramite `ConfermaAzione` (stile `.btn.rosso` del componente); non c'è un campo data per la fine, e una regola terminata non si riattiva dall'interfaccia (`SalvaAsync` non scrive `ends_on`);
  3. dopo la creazione l'editor resta sulla regola appena creata (`Esci($"expenses/recurring/{id}", replace: true)`), come NoteEdit;
  4. nell'interfaccia l'entità si chiama «spesa ricorrente», il pulsante «Nuova ricorrente»;
  5. «Giorno del mese» è un `select` 1..31, e non un numero libero, così non esistono stati invalidi;
  6. la data d'inizio è limitata come in SpesaEdit (dal 2000 a un anno da oggi), e in creazione un avviso dice che una data passata segnerà da sola le spese dei mesi trascorsi.
- Un `checker` (istruzione dei FIX di A e B1) ha lanciato da sé `dotnet test --filter CalcoliRicorrentiTests` senza che glielo chiedessi. In quel momento nessun `implementer` stava scrivendo, quindi non c'è stata corruzione di `obj/`; da allora i brief del `checker` dicono in testa «non compilare».
- Nessun lavoro nuovo arrivato durante l'unità.

LA MISURA ATTESA PER IL COLLAUDO
(dedotta dal codice, **attesa e non osservata**: in questa unità non è stato aperto nessun browser)

- `/expenses` — sotto la testata «Spese» c'è una barra a due voci, «Registro» e «Ricorrenti», a tutta larghezza, con lo stesso disegno delle schede Scrivi/Anteprima delle note. «Registro» è evidenziata (sfondo `--superficie-alta`, testo `--testo`) e porta `aria-current="page"`. **Nessuna voce «Tabella».** Il resto della pagina è invariato: modulo, riepilogo del mese, elenco.
- `/expenses/recurring` — titolo «Spese ricorrenti» con il «?» dell'aiuto (due paragrafi) e il pulsante «Nuova ricorrente» a destra. Sotto, la stessa barra con «Ricorrenti» evidenziata, poi «In <nome spazio>».
  - Spazio senza regole: il blocco vuoto con «Ancora nessuna spesa ricorrente qui.», la spiegazione (affitto, bolletta, abbonamento) e il pulsante «Nuova ricorrente».
  - Con regole: «N spese ricorrenti» (al singolare «1 spesa ricorrente»), poi una riga per regola con descrizione a sinistra e importo in mono a destra. Sotto: la pastiglia della categoria, la cadenza in parole («ogni mese, il 5»; con il 31 «ogni mese, l'ultimo giorno»; annuale «ogni anno, il 5 marzo») e «prossima: gg/mm/aaaa» in mono. Una regola con `ends_on` passato ha la riga in grigio (`--testo-tenue`) e la pastiglia «terminata» al posto della prossima. Le righe si aprono tutte.
- `/expenses/recurring/new` — titolo «Nuova spesa ricorrente», che diventa la descrizione man mano che la si digita. Campi, nell'ordine: Importo con la frase «L'importo è quello atteso…», Descrizione, «Ogni quanto» (select a cinque voci, partendo da «ogni mese»), «Giorno del mese» (select 1..31 sul giorno di oggi, più «31 = ultimo giorno del mese.»), «Dal» (oggi) con l'avviso sulle date passate, e le pastiglie delle categorie. Finché importo e descrizione non sono validi, «Salva» resta spento e c'è la riga «Scrivi importo e descrizione per poterla salvare.». Cambiando anche solo la cadenza e poi premendo un link interno, deve comparire la domanda di uscita. Dopo «Salva», l'indirizzo diventa `/expenses/recurring/<id>` senza nuova voce nella cronologia (Indietro non torna al modulo vuoto).
- `/expenses/recurring/<id>` di una regola propria:
  - la frase «Le spese già segnate non cambiano.» sopra «Salva / Chiudi»; «Salva» è spento finché non si cambia qualcosa, e c'è la riga «Non c'è niente da salvare…»;
  - sotto, la scheda «Fermare questa spesa ricorrente». Se la regola non è terminata: «Terminandola, da domani non segna più spese…» con il pulsante «Termina» → «Sì, termina» / «Annulla». Dopo la conferma: l'avviso «Terminata: da domani non segna più spese.», e la scheda passa a «Terminata il gg/mm/aaaa…»;
  - nella stessa scheda, **se la regola non ha mai segnato spese** (watermark nullo): «Non ha ancora segnato nessuna spesa, quindi si può anche eliminare.» con «Elimina» → «Sì, elimina», che riporta a `/expenses/recurring`. **Se ne ha segnate**: nessun pulsante Elimina, e al suo posto «Non si può eliminare: ha già segnato delle spese…».
- Conflitto riproducibile: aprire l'editor di una regola propria con una scheda, poi aprire `/expenses` in un'altra scheda con lo stesso utente pagante. La materializzazione alza `version` solo se c'è un periodo nuovo da segnare, quindi serve una regola con un'occorrenza dovuta non ancora scritta. A quel punto «Salva» o «Termina» nella prima scheda mostrano la scheda «Qualcun altro ha salvato prima di te» con «Mentre la modificavi, questa spesa ricorrente è cambiata.».
- Regola di un altro membro, per chi non possiede lo spazio: la frase «Questa spesa ricorrente l'ha dichiarata qualcun altro…», importo in sola lettura, campi spenti, nessuna scheda «Fermare…».
