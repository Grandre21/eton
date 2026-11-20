BRIEF implementer 1 — budget: descrizione, durata di N mesi, chiusura anticipata, rifiuto 23P01, scorciatoia alla tabella

PROGETTO: nessun CLAUDE.md di progetto; convenzioni = i file omologhi citati qui sotto. Blazor WASM + Supabase
(Postgrest 4.4.0). Codice, nomi e commenti in italiano come nei file esistenti. Radice: G:\Sviluppo\Eton.

FILE DA TOCCARE (solo questi):
- Models/CategoryBudget.cs
- Services/CalcoliBudget.cs
- Services/BudgetRepository.cs
- Pages/Budget.razor
- Pages/BudgetEdit.razor
- Eton.Tests/CalcoliBudgetTests.cs

NON TOCCARE: supabase/, Pages/Spese.razor, Pages/RicorrenteEdit.razor, Pages/SpeseTabella.razor,
Services/TabellaSpese.cs, wwwroot/ (nessun CSS), docs/, handoff/. Nessuna dipendenza nuova (System.Text.Json è del
framework). In Pages/BudgetEdit.razor la frase «Anche le spese ricorrenti di @categoria consumano questo budget…»
(righe 83-86) resta dov'è, dopo la categoria e prima dell'importo.

CONTRATTO DEL DATABASE (già in produzione, migrazione supabase/migrations/20261120000000_budget_periodo.sql):
- colonna `category_budgets.description` text not null default '', check char_length(description) <= 200, scrivibile
  in INSERT e UPDATE;
- `ends_on` = primo giorno dell'ULTIMO mese coperto, null = senza fine; scrivibile in INSERT e UPDATE;
  `starts_on` e `category` solo in INSERT;
- vincolo di esclusione `category_budgets_no_overlap`: un budget che si sovrappone a un altro della stessa categoria
  nello stesso spazio torna da PostgREST come HTTP 400 col corpo
  {"code":"23P01","details":"Key conflicts with existing key.","hint":null,"message":"conflicting key value violates exclusion constraint \"category_budgets_no_overlap\""}
  Nel client arriva come `Supabase.Postgrest.Exceptions.PostgrestException`, proprietà `int StatusCode` e
  `string? Content` (il corpo).

1) Models/CategoryBudget.cs
   Aggiungi, dopo EndsOn:  `[Column("description")] public string Description { get; set; } = "";`
   (nessun ignoreOnInsert/ignoreOnUpdate: è scrivibile in entrambi).

2) Services/CalcoliBudget.cs — funzioni pure nuove o cambiate (firme esatte):
   a. `public const int MesiMassimi = 24;` e `public const int DescrizioneMassima = 200;` (con <summary> che cita il
      check del database, come CalcoliRicorrenti.DescrizioneMassimaPiano a Services/CalcoliRicorrenti.cs:179-180).
   b. `public static DateTime FineDaMesi(DateTime inizio, int mesi)` → `PrimoDelMese(inizio).AddMonths(mesi - 1)`;
      `mesi` fuori da 1..MesiMassimi lancia ArgumentOutOfRangeException (stesso principio del commento a
      Services/CalcoliRicorrenti.cs:43-46: degradare darebbe un risultato plausibile e sbagliato).
      Modello: CalcoliRicorrenti.FinePiano a Services/CalcoliRicorrenti.cs:186.
   c. `SiPuoChiudere(CategoryBudget b, DateTime oggi)` (riga 55) diventa: il budget è già iniziato
      (`b.StartsOn.Date <= PrimoDelMese(oggi)`) E (`b.EndsOn is null` OPPURE `b.EndsOn.Value.Date > PrimoDelMese(oggi)`).
      Così un budget a periodo si chiude prima della sua fine, e la chiusura (che scrive come ultimo mese il mese
      corrente) non può mai allungarlo: se finisce già questo mese o prima, non si chiude.
   d. `public static string TestoPeriodo(DateTime inizio, DateTime? fine)` — testo in minuscolo coi nomi di
      CalcoliSpese.NomeMese (Services/CalcoliSpese.cs:63):
        fine null                 → "da gennaio 2027, senza fine"
        stesso mese e anno        → "solo gennaio 2027"
        stesso anno               → "da gennaio a marzo 2027"
        anni diversi              → "da novembre 2026 a febbraio 2027"
   e. `public static bool RifiutoPerSovrapposizione(int stato, string? corpo)` → true solo se `stato == 400` e il corpo
      è un oggetto JSON con proprietà "code" stringa uguale a "23P01". Corpo null, vuoto, non JSON, non oggetto, senza
      "code" o con "code" non stringa → false, senza lanciare (JsonDocument.Parse dentro try/catch di JsonException).
      Commento di una riga: il vincolo è category_budgets_no_overlap, 23P01 = exclusion_violation.
   f. `public static string IndirizzoSpeseDelMese(int anno, int mese)` → "expenses/table" + la query di una
      `VistaTabella` costruita così, e SOLO usando l'API pubblica già esistente di Services/TabellaSpese.cs:
        new VistaTabella(TipoPeriodo.Libero, new DateTime(anno, mese, 1), new DateTime(anno, mese, DaysInMonth),
                         FiltriSpese.Nessuno with { Tipo = TipoMovimento.Spesa }, TabellaSpese.Colonne.Data, true, null)
        .InQuery()
      (VistaTabella a Services/TabellaSpese.cs:37, InQuery a :118, DaQuery a :70). Nessun filtro di categoria: le
      spese da portare nel budget stanno in altre categorie e si spostano con «Cambia categoria» della tabella.
   Nessun altro cambiamento in CalcoliBudget (CategorieDisponibili e InizioDiUnNuovo restano come sono: con un budget
   a periodo, ends_on non null, si comportano già bene).

3) Services/BudgetRepository.cs
   - `CreaAsync` diventa
     `CreaAsync(Guid spazioId, Guid autore, string categoria, decimal importo, string descrizione, DateTime inizio, DateTime? ultimoMese)`:
     `Description = descrizione.Trim()`, `EndsOn = ultimoMese is { } u ? ExpenseRepository.PerIlDatabase(CalcoliBudget.PrimoDelMese(u)) : null`.
     Non cattura eccezioni: la PostgrestException del 23P01 sale alla pagina.
   - `SalvaImportoAsync` diventa `SalvaAsync(Guid budgetId, int versioneLetta, decimal importo, string descrizione)`
     con `.Set(b => b.Amount, importo).Set(b => b.Description, descrizione.Trim())` — pattern di
     Services/ExpenseRepository.cs:274-275. Il resto (filtro di versione, Esito) invariato.
   - `ChiudiAsync` invariato.

4) Pages/BudgetEdit.razor
   Stato nuovo nel @code: `private string descrizione = "";` e `private int mesi;` (0 = «ogni mese, finché non lo
   chiudo»; 1..24 = per N mesi; vale solo in creazione).
   a. Descrizione, in creazione e in modifica, SUBITO DOPO il blocco dell'importo (dopo l'errore-campo dell'importo,
      riga 102-105) e prima della frase «Cambiare l'importo vale anche per i mesi passati»:
        se PuoIntervenire:
          <label class="campo">
              <span class="etichetta-campo">Descrizione</span>
              <input type="text" maxlength="@CalcoliBudget.DescrizioneMassima" placeholder="Facoltativa"
                     @bind="descrizione" @bind:event="oninput" disabled="@occupato" />
          </label>
          <p class="spiega">@descrizione.Length di @CalcoliBudget.DescrizioneMassima caratteri</p>
        altrimenti, solo se la descrizione del budget non è vuota: `<div class="campo campo-lettura"><span
        class="etichetta-campo">Descrizione</span><p>@budget!.Description</p></div>` (come l'importo in sola lettura,
        righe 97-100). Testo sempre come espressione Razor (@…), mai MarkupString: è testo dell'utente.
   b. Durata, solo in creazione (Nuova), subito dopo la descrizione: un `<select>` e non un input numerico, così non
      esiste uno stato invalido (stesso principio del commento a Pages/RicorrenteEdit.razor:179-180; markup di
      riferimento Pages/RicorrenteEdit.razor:159-175):
          <label class="campo">
              <span class="etichetta-campo">Durata</span>
              <select @bind="mesi" disabled="@occupato">
                  <option value="0">ogni mese, finché non lo chiudo</option>
                  @for (var n = 1; n <= CalcoliBudget.MesiMassimi; n++) { <option value="@n">per @n @(n == 1 ? "mese" : "mesi")</option> }
              </select>
          </label>
      Attenzione alla cattura della variabile del for dentro il markup: usa una locale (`var valore = n;`) se serve.
   c. La frase della creazione oggi a riga 67 («Vale da … ogni mese.») diventa
      `<p class="spiega">Vale @CalcoliBudget.TestoPeriodo(inizio, mesi > 0 ? CalcoliBudget.FineDaMesi(inizio, mesi) : null). Lo stesso importo vale ogni mese, e quello che resta non si riporta.</p>`
      (resta dov'è, nel ramo `else` di disponibili). La frase a riga 70 «Il budget precedente di @categoria vale ancora
      questo mese.» resta invariata.
   d. In modifica, le righe 77-81 («Vale da…» e «Ultimo mese:…») diventano una riga sola:
      `<p class="spiega">Vale @CalcoliBudget.TestoPeriodo(budget!.StartsOn, budget.EndsOn).</p>`
   e. `Cambiata` (righe 164-166): in creazione `!string.IsNullOrWhiteSpace(importoTesto) || !string.IsNullOrWhiteSpace(descrizione)`;
      in modifica `importoTesto != Denaro.TestoDigitabile(budget.Amount) || descrizione.Trim() != budget.Description`.
      Aggiorna il commento sopra in una riga.
   f. Ovunque oggi si allinea `importoTesto` al budget letto/salvato/ricaricato (OnParametersSetAsync righe 199 e 211,
      SalvaCon riga 262, Ricarica riga 282) allinea anche `descrizione` (`""` in creazione, `budget.Description`
      altrimenti); in creazione anche `mesi = 0`. Dopo `Chiudi` (riga 321) NON si tocca descrizione, come non si tocca
      l'importo.
   g. Salva in creazione (riga 239-240):
        var ultimo = mesi > 0 ? CalcoliBudget.FineDaMesi(inizio, mesi) : (DateTime?)null;
        await Repository.CreaAsync(attivo.Id, autore, categoria, importo, descrizione, inizio, ultimo);
      In modifica `SalvaCon(versione, importo)` chiama `Repository.SalvaAsync(Id!.Value, versione, importo, descrizione)`.
      Sovrascrivi idem (passa per SalvaCon).
   h. Rifiuto per sovrapposizione: in `Salva`, PRIMA del `catch (Exception ex)` esistente, aggiungi
        catch (PostgrestException ex) when (CalcoliBudget.RifiutoPerSovrapposizione(ex.StatusCode, ex.Content))
        {
            Console.Error.WriteLine($"[Budget] Rifiutato per sovrapposizione: {ex.Message}");
            errore = "C'è già un budget per questa categoria in quei mesi: forse l'ha appena creato qualcun altro. Torna ai budget per vedere com'è adesso.";
        }
      con `@using Supabase.Postgrest.Exceptions` in testa.
   i. Chiusura: il blocco a righe 132-138 resta guardato da `CalcoliBudget.SiPuoChiudere`, che ora ammette anche i
      budget a periodo non ancora finiti. Il testo «Chiudendolo vale fino alla fine di questo mese, poi non più.» va
      bene per entrambi. Il resto di `Chiudi` invariato.
   j. Il testo dell'<Aiuto> riga 12 «Il budget riparte pieno ogni mese: il residuo non si riporta.» resta.

5) Pages/Budget.razor
   a. Nella riga di ogni budget (`<li>` righe 86-95), dentro il `<div class="azioni">` esistente, come PRIMO figlio,
      prima dei pulsanti:
        <span class="spiega">@(string.IsNullOrEmpty(s.Budget.Description) ? "" : s.Budget.Description + " · ")@CalcoliBudget.TestoPeriodo(s.Budget.StartsOn, s.Budget.EndsOn)</span>
      Nessun elemento nuovo come figlio diretto del <li>: la griglia di .barre-categorie (wwwroot/css/app.css:2086-2120)
      ha posto solo per nome/barra/quota/totale/azioni, e il CSS non si tocca.
   b. Fra «Registra una spesa» e «Modifica» un terzo link:
        <a class="btn compatto" href="@CalcoliBudget.IndirizzoSpeseDelMese(anno, mese)">Spese del mese</a>
      (anno e mese sono quelli mostrati dalla pagina, campi del @code righe 124).
   c. Sezione «In arrivo» (righe 105-114): la `.meta` diventa
      `<span class="meta">@CalcoliBudget.TestoPeriodo(b.StartsOn, b.EndsOn)</span>` e la `.descrizione` mostra
      `@b.Category` seguita, se la descrizione non è vuota, da ` · @b.Description`.
   d. L'<Aiuto> riga 16: «Il budget vale ogni mese e riparte pieno: il residuo non si riporta.» diventa
      «Il budget vale ogni mese, senza fine o per il numero di mesi che scegli, e riparte pieno: il residuo non si riporta.»
   Tutto il testo dell'utente come espressione Razor, mai MarkupString.

6) Eton.Tests/CalcoliBudgetTests.cs — test xunit nuovi, nello stile dei presenti (helper `Budget(...)` riga 8; puoi
   estenderlo con parametri opzionali senza rompere le chiamate esistenti):
   TEST NUOVO: FineDaMesi_conta_il_mese_di_inizio — (nov 2026, 1) → 1 nov 2026; (nov 2026, 3) → 1 gen 2027;
       (25 nov 2026, 24) → 1 ott 2028; 0 e 25 lanciano ArgumentOutOfRangeException.
   TEST NUOVO: Un_budget_a_periodo_si_chiude_prima_della_fine_ma_non_si_allunga — oggi 25 ott 2026: budget da ott a
       dic 2026 → true; budget che finisce a ott 2026 → false; budget che finisce a set 2026 → false; budget futuro
       (da nov) a periodo → false.
   TEST NUOVO: TestoPeriodo_nei_quattro_casi — i quattro esempi del punto 2d.
   TEST NUOVO: Il_rifiuto_23P01_si_riconosce_solo_col_400_e_il_suo_codice — vero col corpo esatto del contratto e 400;
       falso con lo stesso corpo e 409; falso con code "23505"; falso con null, "", "non json", "[]", {"code":23}.
   TEST NUOVO: La_scorciatoia_apre_la_tabella_sul_mese_tipo_spesa_senza_categoria — `IndirizzoSpeseDelMese(2026, 2)`
       comincia con "expenses/table?"; `VistaTabella.DaQuery(<parte dopo "expenses/table">)` dà Periodo Libero, Da 1 feb
       2026, A 28 feb 2026, Filtri.Tipo == TipoMovimento.Spesa, Filtri.Categorie vuoto.
   Il test esistente `Non_si_chiude_un_budget_futuro_o_gia_chiuso` deve restare verde senza modifiche.
   Ordine: scrivi PRIMA i test, esegui `mcp__synapse__test root=G:\Sviluppo\Eton filter=FullyQualifiedName~CalcoliBudgetTests`
   sul codice di prima, e nel VERIFICA ricopia una riga per ciascun TEST NUOVO nel formato
       test rosso  <nome del test>: <riga di fallimento ricopiata dall'output>
   (un errore di compilazione ricopiato dall'output del comando di test vale come fallimento). Poi il codice, poi
   riesegui.

BUDGET DI COMPLESSITÀ: nessun tipo nuovo, nessun file nuovo, nessuna astrazione nuova; solo le funzioni elencate in
CalcoliBudget; niente helper con un solo call-site nelle pagine. Diff atteso ~200 righe.

verifica: `mcp__synapse__diagnostics` sui file toccati, poi `mcp__synapse__build`/`test` (root=G:\Sviluppo\Eton);
`dotnet build|test` in shell lo nega `hooks/build-via-synapse.ps1`, e solo se il tool synapse fallisce si rilancia con
`SYNAPSE_DOWN=1` davanti (PowerShell: `$env:SYNAPSE_DOWN=1;`), detto nel resoconto.

Formato di ritorno: FATTO / DEVIAZIONI / VERIFICA / NOTE, oppure BLOCKED: <domanda>.
