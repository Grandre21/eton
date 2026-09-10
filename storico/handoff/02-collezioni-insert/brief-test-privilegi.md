# Brief — `Eton.Tests/PrivilegiInsertTests.cs`

Obiettivo 3 dell'unità 02. Scritto dall'esecutore, eseguito da un `implementer`.
Conservato su disco perché il resoconto lo cita e perché una compaction non lo porti via.

## Che cosa deve impedire

Che un modello invii in INSERT una colonna che nessuna migrazione ha concesso al ruolo
`authenticated`. È il difetto 42501 sulle collezioni, sopravvissuto due settimane: il
modello inviava `blind`, il grant non c'era. Senza database, a ogni `dotnet test`.

## File

Uno solo, nuovo: `Eton.Tests/PrivilegiInsertTests.cs`. Nient'altro si tocca.

## Le due sorgenti da confrontare

1. **Il modello** — per ogni classe di `Eton.Models` che deriva da `BaseModel` e ha
   `[Table("...")]`: le colonne inviate in INSERT sono le proprietà con `[Column(...)]`
   senza `ignoreOnInsert: true`, più le proprietà con `[PrimaryKey(nome, shouldInsert)]`
   dove `shouldInsert` è vero (`Expense` è l'unico caso: `Models/Expense.cs:23`).
2. **Le migrazioni** — i file `supabase/migrations/*.sql`, letti **in ordine di nome
   ordinale**, applicando in sequenza `grant` e `revoke`.

Asserzione: insieme del modello ⊆ insieme concesso.

## Esiti attesi oggi (se il tuo parser dà numeri diversi, il parser è sbagliato)

| tabella | concesso in INSERT | inviato dal modello |
|---|---|---|
| `profiles` | **tutte** (grant senza elenco di colonne) | `display_name`, `avatar_url` |
| `notes` | space_id, owner_id, title, body | gli stessi 4 |
| `collections` | space_id, owner_id, name, icon, fields, rating_max, **blind** | gli stessi 7 |
| `collection_items` | collection_id, space_id, added_by, name, image_url, data | gli stessi 6 |
| `reviews` | item_id, space_id, user_id, rating, comment | gli stessi 5 |
| `expenses` | **id**, space_id, paid_by, amount, description, category, spent_on | gli stessi 7 |
| `spaces` | nessuno | — non si controlla |
| `space_members` | nessuno | — non si controlla |

`blind` è concesso da `supabase/migrations/20260903000000_grant_insert_blind.sql:24`, che
è nuovo. Togliendo quel file il test deve diventare rosso: è la sua ragione di esistere.

## Struttura richiesta

Una sola classe `public class PrivilegiInsertTests`, tutti gli helper `private static`
dentro di essa. Quattro test:

1. `[Theory]` + `[MemberData]` — un caso per ogni modello la cui tabella **ha** almeno un
   `grant insert`. Asserisce l'inclusione e, se fallisce, nomina tabella e colonne
   mancanti nel messaggio.
2. `[Fact]` — le tabelle **senza** `grant insert` sono esattamente `spaces` e
   `space_members`. Questo test è la guardia contro il silenzio: senza di esso, una
   tabella che perdesse il proprio grant uscirebbe dal controllo del punto 1 senza che
   nessuno lo noti.
3. `[Fact]` — il grant scritto su due righe di `20260824000000_spese.sql:144-145` è stato
   riconosciuto: `expenses` concede `id` e `spent_on`.
4. `[Fact]` — `grant select, insert on public.profiles` **senza elenco di colonne**
   significa *tutte* le colonne, ed è trattato come tale.

## Come si legge la cartella delle migrazioni

Non esiste un test che legga file: sei il primo. Da `AppContext.BaseDirectory` risali di
padre in padre finché trovi una cartella che contiene `supabase/migrations`; se arrivi
alla radice del volume senza trovarla, **fallisci** con un messaggio che dica il percorso
di partenza. Mai un percorso relativo nudo: la working directory di `dotnet test` è
`bin/Debug/net10.0/`, non la radice del repository.

## Il parser, passo per passo

**Ordine obbligato, e il primo passo è il più importante.**

1. **Togli i commenti**: per ogni riga, scarta tutto ciò che segue `--`. Verificato: in
   queste sei migrazioni nessuna stringa letterale contiene `--`, quindi il taglio riga
   per riga è sicuro. Se non lo fai per primo, i commenti di
   `20260903000000_grant_insert_blind.sql:15-23` — che citano testualmente
   `grant insert (col)` e `grant update (a, b, c, col)` — entrano nel parsing e il test è
   rosso al primo giro.
2. Unisci le righe rimaste, **collassa ogni sequenza di spazi bianchi in un singolo
   spazio** (è così che il grant su due righe di `spese.sql` diventa uno statement solo),
   dividi per `;`, scarta i frammenti vuoti.
3. Per ogni statement, `Trim()`; se **non** contiene la parola `grant` né `revoke`
   (confronto senza distinzione di maiuscole, con `\b`), ignoralo.
4. Altrimenti classificalo con queste tre regex, **in quest'ordine**, sul testo in
   minuscolo:

```
tabella-grant:   ^grant\s+(?<privs>.+?)\s+on\s+(?<objs>public\.\w+(?:\s*,\s*public\.\w+)*)\s+to\s+(?<roles>[\w\s,]+)$
tabella-revoke:  ^revoke\s+(?<privs>.+?)\s+on\s+(?<objs>public\.\w+(?:\s*,\s*public\.\w+)*)\s+from\s+(?<roles>[\w\s,]+)$
non-pertinente:  ^(?:grant|revoke)\s+.+?\s+on\s+(?:schema|function|all\s|sequence)
```

   Se nessuna delle tre matcha → **`Assert.Fail`** con lo statement citato per esteso e il
   nome del file. Mai ignorare in silenzio: è il punto del mandato, e la fragilità va
   tenuta in questa direzione.

   Il vincolo `public\.\w+` sugli oggetti non è pignoleria: impedisce alla regex di
   agganciare un ` on ` che stia dentro l'elenco di colonne, e fa fallire un grant su una
   tabella non qualificata invece di interpretarlo a caso.

5. **Ruoli**: dividi `roles` per virgola e trimma. Se un ruolo non è fra `authenticated`,
   `anon`, `service_role`, `public` → `Assert.Fail`. Lo statement interessa solo se i
   ruoli includono `authenticated` (o `public`, che li comprende tutti).
6. **Privilegi**: cerca in `privs` la parola `insert` con `\b`, seguita facoltativamente da
   `(elenco)`.
   - `insert (a, b, c)` → aggiungi quelle colonne all'insieme della tabella;
   - `insert` senza parentesi → **tutte** le colonne;
   - nessun `insert` ma `\ball\b` → tutte le colonne;
   - né l'uno né l'altro (`grant select, delete ...`, `revoke update ...`) → statement
     riconosciuto ma non pertinente: **non fallire**, ignorare.
   Su un `revoke`, invece di aggiungere, **azzera** l'insieme della tabella.
7. **Oggetti**: dividi `objs` per virgola, togli il prefisso `public.`; lo statement vale
   per ognuna delle tabelle elencate (`initial_schema.sql:297` e `collections.sql:216` ne
   elencano più d'una).

**Rappresentazione dell'insieme concesso**: `Dictionary<string, HashSet<string>>` con
chiave il nome della tabella. Per «tutte le colonne» usa un `const string` sentinella
`"*"` dentro l'insieme, e un solo helper che risponde alla domanda «questa colonna è
concessa?». Niente `HashSet<string>?` nullable e niente tipo nuovo per rappresentare la
differenza: un tipo in più qui costa più di quanto renda.

## Superficie di reflection — verificata, usala così e non cercarne un'altra

`doc-checker` l'ha letta dal sorgente del commit `2bc8266` di
`supabase-community/postgrest-csharp`, che è quello dichiarato nel `.nuspec` del pacchetto
4.4.0 installato. Non è a memoria:

```csharp
Supabase.Postgrest.Attributes.ColumnAttribute      → string ColumnName { get; }
                                                     bool   IgnoreOnInsert { get; }
Supabase.Postgrest.Attributes.PrimaryKeyAttribute  → string ColumnName { get; }
                                                     bool   ShouldInsert { get; }
Supabase.Postgrest.Attributes.TableAttribute       → string Name { get; set; }
```

**`PrimaryKeyAttribute` non deriva da `ColumnAttribute`**: entrambe derivano da
`System.Attribute` e ridichiarano `ColumnName` per conto proprio. Conseguenza operativa:
`GetCustomAttribute<ColumnAttribute>()` su una proprietà marcata `[PrimaryKey(...)]`
restituisce `null`. Le due interrogazioni sono separate e i due insiemi vanno sommati —
non c'è modo di prendere le colonne con una query sola.

`ImplicitUsings` è attivo ma non copre `System.Reflection` né
`System.Text.RegularExpressions`: dichiarali.

I modelli si enumerano da `typeof(Eton.Models.Collection).Assembly`, filtrando i tipi non
astratti che derivano da `Supabase.Postgrest.Models.BaseModel` e portano
`[Table]` — `Eton.Models.CampoDefinizione` non ha né l'uno né l'altro e resta fuori da sé.

## Budget di complessità

- Nessun tipo nuovo oltre alla classe di test. Niente record, niente enum, niente
  interfacce.
- Nessun file oltre a `PrivilegiInsertTests.cs`.
- Nessun pacchetto NuGet nuovo.
- Un helper con un solo call-site va scritto inline.
- Sotto le ~200 righe compresi i commenti.

## Stile

Guarda `Eton.Tests/PermessiTests.cs` e `Eton.Tests/SchemaCampiTests.cs`. Nomi dei metodi
in italiano con gli underscore (`Ogni_modello_invia_solo_colonne_concesse`), un commento
sopra ogni test che dice **quale difetto reale** quel caso impedisce — non che cosa fa il
codice. Il commento in testa alla classe spiega perché il test esiste, citando il 42501
del 12 agosto 2026.

## Cosa NON fare

- **Non compilare e non eseguire i test.** Compila l'orchestratore, una volta, a fine
  giro: `obj/` non ha lock fra processi e due build concorrenti si corrompono a vicenda.
- Non toccare i modelli, le migrazioni, gli altri test, nessun `.csproj`.
- Non aggiungere un test che si connetta a un database.
