## FATTO

- `Services/CategorieSpesa.cs`: aggiunti tipi di movimento, categorie entrate, elenco unificato e validazione per tipo.
- `Models/Expense.cs`, `Models/RecurringExpense.cs`: aggiunto `Kind`, scrivibile solo in creazione.
- `Services/ExpenseRepository.cs`, `Services/RecurringExpenseRepository.cs`: tipo trasmesso negli INSERT; modello delle occorrenze invariato, salvo commento.
- `Services/CalcoliRicorrenti.cs`: propagazione del tipo e gruppo Entrate, escluso dall’accantonamento.
- `Services/CalcoliSpese.cs`: bilancio, saldo, riepilogo con entrate separate e formattazione con segno.
- `Services/TabellaSpese.cs`: filtro tipo, URL, colonna ordinabile, categorie per tipo e copia di `Kind`.
- `Pages/Spese.razor`, `SpesaEdit.razor`, `RicorrenteEdit.razor`, `Ricorrenti.razor`: esclusivamente le quattro correzioni richieste.
- Nei sei file di test indicati: aggiunti i dieci test richiesti e aggiornati helper e chiamate esistenti.

## DEVIAZIONI

Nessuno scostamento funzionale. In `RecurringExpense.cs` il default usa il nome completo `Eton.Services.TipoMovimento.Spesa`, evitando ulteriori modifiche al file.

I nuovi test sono stati verificati insieme sul codice originale: la compilazione fallita costituisce il rosso previsto dal brief. Per confronto mensile e privilegi, gli errori riguardano rispettivamente l’helper condiviso e `nameof(Expense.Kind)`.

Comandi esatti delle suite eseguite:

```powershell
dotnet test Eton.Tests/Eton.Tests.csproj --filter "FullyQualifiedName~Le_entrate_non_entrano_nel_totale_ne_nelle_categorie|FullyQualifiedName~Il_confronto_col_mese_prima_guarda_solo_le_spese|FullyQualifiedName~BilancioDi_separa_spese_ed_entrate|FullyQualifiedName~ConSegno_mette_piu_meno_e_niente_sullo_zero|FullyQualifiedName~Una_categoria_vale_solo_per_il_suo_tipo|FullyQualifiedName~Le_entrate_hanno_il_loro_gruppo_e_stanno_fuori_dalla_quota|FullyQualifiedName~Un_entrata_ricorrente_prevista_e_in_arrivo_resta_un_entrata|FullyQualifiedName~Il_filtro_per_tipo_e_l_url|FullyQualifiedName~La_categoria_di_un_entrata_viene_dal_suo_elenco|FullyQualifiedName~Il_tipo_si_scrive_solo_in_creazione"
dotnet test Eton.Tests/Eton.Tests.csproj --no-restore --filter "FullyQualifiedName~Il_tipo_si_scrive_solo_in_creazione"
dotnet test Eton.Tests/Eton.Tests.csproj --no-restore --filter "FullyQualifiedName~Il_tipo_si_scrive_solo_in_creazione" 2>&1 | Select-String 'PrivilegiInsertTests'
dotnet build
dotnet test
```

`dotnet build` e `dotnet test` eseguiti due volte: prima e dopo l’ultimo aggiustamento di posizione/commento. Nessuno script creato o eseguito. Eseguito anche `git diff --check`, senza segnalazioni.

## VERIFICA

Estratti letterali degli errori del compilatore sul codice originale:

```text
test rosso  CalcoliSpeseTests.Le_entrate_non_entrano_nel_totale_ne_nelle_categorie: error CS1061: 'RiepilogoMese' non contiene una definizione di 'Entrate'
test rosso  CalcoliSpeseTests.Il_confronto_col_mese_prima_guarda_solo_le_spese: error CS0117: 'Expense' non contiene una definizione per 'Kind'
test rosso  CalcoliSpeseTests.BilancioDi_separa_spese_ed_entrate: error CS0117: 'CalcoliSpese' non contiene una definizione per 'BilancioDi'
test rosso  CalcoliSpeseTests.ConSegno_mette_piu_meno_e_niente_sullo_zero: error CS0117: 'CalcoliSpese' non contiene una definizione per 'ConSegno'
test rosso  CategorieSpesaTests.Una_categoria_vale_solo_per_il_suo_tipo: error CS1501: Nessun overload del metodo 'Conosciuta' accetta 2 argomenti
test rosso  GruppiRicorrentiTests.Le_entrate_hanno_il_loro_gruppo_e_stanno_fuori_dalla_quota: error CS0117: 'GruppoRicorrenti' non contiene una definizione per 'Entrate'
test rosso  FusioneRicorrentiTests.Un_entrata_ricorrente_prevista_e_in_arrivo_resta_un_entrata: error CS1061: 'RecurringExpense' non contiene una definizione di 'Kind'
test rosso  TabellaSpeseTests.Il_filtro_per_tipo_e_l_url: error CS0117: 'FiltriSpese' non contiene una definizione per 'Tipo'
test rosso  TabellaSpeseTests.La_categoria_di_un_entrata_viene_dal_suo_elenco: error CS1061: 'Expense' non contiene una definizione di 'Kind'
test rosso  PrivilegiInsertTests.Il_tipo_si_scrive_solo_in_creazione: error CS0117: 'Expense' non contiene una definizione per 'Kind'
```

Esito finale di `dotnet build`:

```text
Compilazione completata.
    Avvisi: 4
    Errori: 0
```

Esito finale di `dotnet test`:

```text
Superato!     - Non superati:     0. Superati:   417. Ignorati:     0. Totale:   417. Durata: 152 ms - Eton.Tests.dll (net10.0)
```

## NOTE

I quattro avvisi NU1900 dipendono dall’impossibilità di raggiungere NuGet per i dati sulle vulnerabilità. Il test sul modello di scrittura delle occorrenze continua a passare. GATE Synapse demandato alla sessione, come previsto. App e Synapse.Server non avviati; modifiche preesistenti in `handoff/` lasciate intatte.