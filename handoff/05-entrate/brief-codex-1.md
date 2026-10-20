# Brief 1 — entrate: modello, calcoli, filtri e test (unità 05 di Eton)

Lavori nel worktree `G:\Sviluppo\wt-Eton\05-entrate` (Blazor WebAssembly .NET, test xUnit in `Eton.Tests/`).
Questo brief fa la parte **non visiva**: il tipo di movimento (spesa / entrata), le categorie delle entrate, i
calcoli che separano spese ed entrate, il filtro e la colonna «Tipo» della tabella, e i test. Le pagine le fa
un brief successivo: qui le tocchi **solo** nei punti elencati in «Correzioni minime di compilazione».

## Contesto già deciso

Il database ha già (migrazione applicata) `expenses.kind` e `recurring_expenses.kind`: `text not null default
'expense'`, valori `'expense'` | `'income'`, concessi solo in INSERT (nessun grant UPDATE; i trigger di
aggiornamento rimettono `old.kind`). Un trigger BEFORE INSERT su `expenses` copia `kind` dalla regola quando
`recurring_id` è valorizzato, qualunque cosa mandi il client
(`supabase/migrations/*_rate_entrate.sql:94-124`). Un movimento non cambia tipo dopo la creazione. Le entrate
sono importi **positivi** con un tipo, mai importi negativi.

## Cosa fare

### 1. `Services/CategorieSpesa.cs`

Aggiungi in testa al file, nello stesso namespace, una classe:

```csharp
/// <summary>I due tipi di movimento, con i valori della colonna <c>kind</c> (check del database).</summary>
public static class TipoMovimento
{
    public const string Spesa = "expense", Entrata = "income";

    /// <summary>«Spesa» o «Entrata», per la colonna e il filtro della tabella e per gli editor.</summary>
    public static string Testo(string tipo) => tipo == Entrata ? "Entrata" : "Spesa";
}
```

In `CategorieSpesa`:
- `Elenco` resta com'è (sono le categorie delle spese) — aggiorna il suo commento in una riga.
- nuovo `public static readonly IReadOnlyList<string> Entrate = ["Stipendio", "Rimborso", "Regalo", "Vendita", "Altro"];`
- nuovo `public static readonly IReadOnlyList<string> Tutte = [.. Elenco, .. Entrate.Where(c => !Elenco.Contains(c))];`
  (per i filtri della tabella: «Altro» compare una volta sola)
- nuovo `public static IReadOnlyList<string> Di(string tipo) => tipo == TipoMovimento.Entrata ? Entrate : Elenco;`
- `Conosciuta` prende il tipo e **perde la forma a un argomento**:
  `public static bool Conosciuta(string? categoria, string tipo) => categoria is not null && Di(tipo).Contains(categoria, StringComparer.Ordinal);`
  (spec: «una categoria si accetta se appartiene all'elenco del proprio tipo»).

### 2. Modelli

`Models/Expense.cs`, accanto a `recurring_id`/`recurring_period` (stessa forma, `ignoreOnUpdate: true`, riga 37-38):

```csharp
    // Il tipo, 'expense' o 'income': concesso solo in INSERT come recurring_id, e il trigger di
    // aggiornamento lo rimette com'era. Su un'occorrenza lo copia dalla regola il trigger di insert.
    [Column("kind", ignoreOnUpdate: true)] public string Kind { get; set; } = TipoMovimento.Spesa;
```
(serve `using Eton.Services;` in testa al file.)

`Models/RecurringExpense.cs`: la stessa proprietà `Kind`, con `ignoreOnUpdate: true`, sotto `paid_by`
(riga 77-78), con un commento di una riga che rimanda a `Expense.Kind`. **Nient'altro** in questo file.

**`OccorrenzaRicorrente`** (`Services/ExpenseRepository.cs:424-436`) **non** acquista `kind`: lo scrive il
trigger. Aggiungi solo una riga al suo commento: «kind manca di proposito: lo copia dalla regola il trigger
expenses_before_insert». Il test `Il_modello_di_scrittura_non_omette_nessuna_colonna`
(`Eton.Tests/FusioneRicorrentiTests.cs:125`) resta com'è e deve continuare a passare.

### 3. Repository

- `Services/ExpenseRepository.cs:220` — `CreaAsync` acquista un ultimo parametro `string tipo` e scrive
  `Kind = tipo` nel nuovo `Expense`. Firma risultante, testuale:
  `public async Task<Expense> CreaAsync(Guid spazioId, Guid pagante, decimal importo, string descrizione, string categoria, DateTime data, string tipo)`
- `Services/RecurringExpenseRepository.cs:11` — `DatiRegola` acquista in coda `string Kind = TipoMovimento.Spesa`
  (dopo `decimal? TotalePiano = null`); `CreaAsync` (riga 49) scrive `Kind = dati.Kind`. `SalvaAsync` **non**
  tocca il tipo (il grant UPDATE non c'è). Aggiorna di mezza riga il commento di `DatiRegola`.

### 4. `Services/CalcoliRicorrenti.cs`

- `Occorrenza` (riga 147): il nuovo `Expense` porta `Kind = r.Kind`. È il punto unico da cui escono previste e
  materializzate: un'entrata ricorrente in arrivo è un'entrata.
- `GruppoRicorrenti` (riga 20): `public enum GruppoRicorrenti { Entrate, Mensili, ARate, Annuali, OgniNMesi, Terminate }`;
  aggiorna il commento sopra (Entrate è ora al suo posto).
- `Gruppo` (riga 242): dopo il ramo `Terminate` e **prima** di quello delle rate,
  `if (r.Kind == TipoMovimento.Entrata) return GruppoRicorrenti.Entrate;`. Una regola di entrata terminata
  resta in `Terminate`, fuori dai totali.
- `QuotaDaAccantonare` non cambia: somma solo `Annuali` e `OgniNMesi`, quindi le entrate ne restano fuori
  senza toccarla. Aggiorna solo il suo commento se lo nomina.

### 5. `Services/CalcoliSpese.cs`

- Nuovo record, sopra `RiepilogoMese`:
  ```csharp
  /// <summary>Spese ed entrate di un insieme di movimenti, e il saldo.</summary>
  public sealed record Bilancio(decimal Spese, decimal Entrate)
  {
      public decimal Saldo => Entrate - Spese;
  }
  ```
- Nuovo metodo `public static Bilancio BilancioDi(IEnumerable<Expense> movimenti)`: somma `Amount` delle
  righe con `Kind == TipoMovimento.Entrata` in `Entrate` e di **tutte le altre** in `Spese` (un valore
  sconosciuto conta come spesa, come il default del database).
- `RiepilogoMese` acquista in coda `decimal Entrate = 0` e una proprietà `public decimal Saldo => Entrate - Totale;`.
  `Totale` resta il nome del totale delle **spese**: i costruttori esistenti `new RiepilogoMese(0, [], 0, null)`
  devono continuare a compilare.
- `PerMese` (riga 146): `Totale`, `TotalePrecedente`, `VariazionePercentuale` e `Categorie` si calcolano
  **solo sulle spese**; `Entrate` sulle entrate del mese. Usa `BilancioDi` per le due somme del mese e del mese
  prima; le categorie da `delMese` filtrate sulle spese. Aggiorna i due commenti `<summary>` (di
  `RiepilogoMese` e di `PerMese`) in una riga ciascuno: «solo spese; le entrate a parte».
- Nuovo `public static string ConSegno(decimal importo)`: `"+" + Denaro.Testo(importo)` se > 0,
  `"−" + Denaro.Testo(-importo)` se < 0 (U+2212, il segno meno tipografico), `Denaro.Testo(0m)` se zero. Serve a
  Registro, Home e tabella per entrate e saldo.

### 6. `Services/TabellaSpese.cs`

- `FiltriSpese` (riga 30) acquista in coda `string? Tipo = null` (null = entrambi; altrimenti
  `TipoMovimento.Spesa` o `TipoMovimento.Entrata`). `Nessuno` invariato nel significato.
- `Filtra` (riga 224): `.Where(r => filtri.Tipo is null || r.Spesa.Kind == filtri.Tipo)`.
- `VistaTabella.DaQuery`: parametro URL `tipo` con valori `spesa` → `TipoMovimento.Spesa`, `entrata` →
  `TipoMovimento.Entrata`, qualunque altro → null. `InQuery`: scrive `tipo=spesa|entrata` solo se il filtro
  c'è, **dopo** `stato` e prima di `ord`. Una tabella di coppie come `Stati` (riga 46) è il pattern.
- Le categorie nell'URL: `DaQuery` accetta quelle in `CategorieSpesa.Tutte` (oggi `CategorieSpesa.Conosciuta`,
  riga 101); `InQuery` le ordina su `CategorieSpesa.Tutte` (oggi `Elenco`, riga 126).
- `Colonne` (riga 186): nuova costante `Tipo = "tipo"`; aggiungila a `ColonneOrdinabili` (riga 51).
- `Applica`, ramo `Colonne.Categoria` (riga 284): `CategorieSpesa.Conosciuta(testo, spesa.Kind)`; messaggio
  invariato.
- `Copia` (riga 297): copia anche `Kind`.

### 7. Correzioni minime di compilazione nelle pagine (e solo queste)

Il brief successivo rifà queste pagine: qui solo ciò che serve a compilare, senza cambiare comportamento.
- `Pages/Spese.razor:402` — `Repository.CreaAsync(..., nuovaData!.Value, TipoMovimento.Spesa)`.
- `Pages/SpesaEdit.razor:145` — `CategorieSpesa.Conosciuta(categoria, TipoMovimento.Spesa)`.
- `Pages/RicorrenteEdit.razor:196` — `CategorieSpesa.Conosciuta(categoria, TipoMovimento.Spesa)`.
- `Pages/Ricorrenti.razor:124-132` — in `Titolo` aggiungi `GruppoRicorrenti.Entrate => "Entrate",` come
  primo ramo (senza, la pagina lancia su un gruppo nuovo).
Se il compilatore segnala un altro punto, correggilo nello stesso spirito e scrivilo in `DEVIAZIONI`.

## Test

Nei file di test esistenti, con il loro stile (helper privati come `CalcoliSpeseTests.Spesa`,
`Eton.Tests/CalcoliSpeseTests.cs:8`; `GruppiRicorrentiTests.Regola`; `TabellaSpeseTests.Spesa`, riga 13). Gli
helper acquistano un parametro opzionale `tipo` dove serve. Aggiorna i test esistenti di `CategorieSpesaTests`
alla firma a due argomenti senza cambiarne il significato.

TEST NUOVO: CalcoliSpeseTests.Le_entrate_non_entrano_nel_totale_ne_nelle_categorie — un mese con 100 € di spese
in due categorie e 1.500 € di entrate: `Totale` 100, `Entrate` 1.500, `Saldo` 1.400, le categorie sono solo
quelle delle spese e le quote fanno 100.
TEST NUOVO: CalcoliSpeseTests.Il_confronto_col_mese_prima_guarda_solo_le_spese — un'entrata nel mese prima non
cambia `TotalePrecedente` né la variazione.
TEST NUOVO: CalcoliSpeseTests.BilancioDi_separa_spese_ed_entrate — anche con un insieme vuoto (tutto a zero).
TEST NUOVO: CalcoliSpeseTests.ConSegno_mette_piu_meno_e_niente_sullo_zero — `+1.500,00`, `−20,50`, `0,00`.
TEST NUOVO: CategorieSpesaTests.Una_categoria_vale_solo_per_il_suo_tipo — «Stipendio» per l'entrata sì e per la
spesa no, «Casa» il contrario, «Altro» per entrambe; `Entrate` è esattamente
Stipendio, Rimborso, Regalo, Vendita, Altro; `Tutte` non ha doppioni.
TEST NUOVO: GruppiRicorrentiTests.Le_entrate_hanno_il_loro_gruppo_e_stanno_fuori_dalla_quota — un'entrata mensile e
un'entrata annuale vanno in `Entrate` (primo gruppo in `Raggruppa`), un'entrata terminata in `Terminate`, e
`QuotaDaAccantonare` con una spesa annuale da 120 € e un'entrata annuale da 1.200 € dà 10.
TEST NUOVO: FusioneRicorrentiTests.Un_entrata_ricorrente_prevista_e_in_arrivo_resta_un_entrata — con `Fondi`, sia la
prevista sia quella in arrivo di una regola `income` hanno `Kind == "income"`.
TEST NUOVO: TabellaSpeseTests.Il_filtro_per_tipo_e_l_url — `Filtra` con `Tipo` entrata/spesa/null; una vista con
`tipo=entrata` e una categoria delle entrate («Stipendio») si rilegge identica da `InQuery` a `DaQuery`; un
`tipo=boh` torna a null.
TEST NUOVO: TabellaSpeseTests.La_categoria_di_un_entrata_viene_dal_suo_elenco — `Applica` su un'entrata accetta
«Rimborso», rifiuta «Casa», e la copia tiene `Kind`.
TEST NUOVO: PrivilegiInsertTests.Il_tipo_si_scrive_solo_in_creazione — su `Expense.Kind` e
`RecurringExpense.Kind`: `ColumnAttribute` con nome `kind`, `IgnoreOnInsert` falso, `IgnoreOnUpdate` vero, e
`kind` è in `Concesso["expenses"]` e in `Concesso["recurring_expenses"]`.

Per ogni `TEST NUOVO`: scrivi il test **prima** del codice, eseguilo sul codice di prima e copia nel tuo
`VERIFICA` una riga a formato fisso per ciascuno:
`test rosso  <Classe.Metodo>: <riga di fallimento ricopiata dall'output>`
(non compilare sul codice di prima vale come fallire: la riga è l'errore del compilatore). Poi scrivi il codice
e riesegui. Un test nuovo che sul codice di prima passa non prova niente: fermati con `BLOCKED:`.

## Budget di complessità

Nessuna astrazione nuova oltre a quelle nominate qui (`TipoMovimento`, `Bilancio`); nessun tipo nuovo oltre a
questi due; un helper con un solo call-site va inline. Nessun pacchetto nuovo. Commenti: italiani, densità come
i vicini, uno per decisione non ovvia, niente commenti che ripetono il codice.

## Non toccare

`supabase/`, `docs/`, `handoff/` (salvo il tuo file `-o`), `wwwroot/`, `Shared/`, ogni file non nominato qui.
Le pagine solo nei quattro punti del §7. Non avviare l'app e non avviare Synapse.Server.

## Verifica

`dotnet build` e `dotnet test` nel worktree (`G:\Sviluppo\wt-Eton\05-entrate`): 0 errori, 0 test falliti. Il GATE
lo rilancia la sessione con `mcp__synapse__build root=<worktree>`.

## Formato del ritorno

Il tuo messaggio finale, in quattro sezioni: `FATTO` (cosa, per file), `DEVIAZIONI` (ogni scostamento da questo
brief col perché, e per ogni script o suite che lanci il comando esatto), `VERIFICA` (le righe `test rosso`,
poi l'esito di build e test con i conteggi ricopiati), `NOTE`. Oppure una riga sola `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.
Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/05-entrate/resoconto.md`.
(Il file `-o` non lo crei tu: è il tuo **messaggio finale**, che la riga di comando salva da sé in
`handoff/05-entrate/codex-1b.md`. Non creare file chiamati `-o`.)
Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.
Non scrivere file con `Set-Content`, `Out-File` o `[System.IO.File]::Write*`: modifica i file con le tue patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
