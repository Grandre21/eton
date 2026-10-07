UNITÀ: 2.2-A — ESITO: FATTO
TOCCATI:
- Services/ColonnaGriglia.cs → +23/−0 (nuovo)
- Services/CalcoliGriglia.cs → +149/−0 (nuovo)
- Services/TabellaSpese.cs → +332/−0 (nuovo)
- Services/SchemaCampi.cs → +1/−1 (`RiduciAccenti` da `private` a `internal`)
- Eton.Tests/CalcoliGrigliaTests.cs → +161/−0 (nuovo, 15 test)
- Eton.Tests/TabellaSpeseTests.cs → +378/−0 (nuovo, 30 test)

REVIEW:
review: A — calcoli generici della griglia (task 1)
  bug-hunter      RILIEVI: 0
  conformity      RILIEVI: 2
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 3
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 8 · non risolti 0 · non verificabili 0
review: B — parte pura delle spese (task 2)
  bug-hunter      RILIEVI: 2
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 2
  checker         VERDETTI: fondati 3 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 8 · non risolti 0 · non verificabili 0
(La seconda voce `checker` è una sola istruttoria sulle correzioni dei due brief — 4 fix di A e 4 di B — ricopiata in entrambi i tracciati.)
Gate di `backend-expert`, per entrambi: file nuovi (5 `create mode`), 20 dichiarazioni/endpoint, più di 120 righe.

CONTRATTI:
- `Services/ColonnaGriglia.cs:3` `public enum TipoColonna { Testo, Numero, Data, Selezione, Denaro }`
- `Services/ColonnaGriglia.cs:6` `public enum Aggregazione { Nessuna = 0, Somma = 1, Media = 2, Min = 4, Max = 8, Conteggio = 16 }` (`[Flags]`)
- `Services/ColonnaGriglia.cs:11` `public sealed record ColonnaGriglia<T>(string Chiave, string Etichetta, TipoColonna Tipo, Func<T, object?> Valore)` con `Aggregazioni { get; init; }` (:13), `Ordinabile { get; init; } = true` (:14), `Func<T, bool>? Modificabile { get; init; }` (:15), `IReadOnlyList<string> Opzioni { get; init; } = []` (:16)
- `Services/ColonnaGriglia.cs:20` `public sealed record Gruppo<T>(string? Etichetta, IReadOnlyList<T> Righe);`
- `Services/ColonnaGriglia.cs:23` `public sealed record Riepilogo(int Conteggio, decimal Somma, decimal? Media, decimal? Min, decimal? Max);`
- `Services/CalcoliGriglia.cs:16` `public static IReadOnlyList<T> Ordina<T>(IReadOnlyList<T> righe, ColonnaGriglia<T> colonna, bool decrescente)`
- `Services/CalcoliGriglia.cs:44` `public static IReadOnlyList<Gruppo<T>> Raggruppa<T>(IReadOnlyList<T> righe, Func<T, string>? chiave)`
- `Services/CalcoliGriglia.cs:54` `public static Riepilogo Aggrega<T>(IReadOnlyList<T> righe, Func<T, decimal> valore)`
- `Services/CalcoliGriglia.cs:68` `public static string Csv<T>(IReadOnlyList<ColonnaGriglia<T>> colonne, IReadOnlyList<T> righe)`
- `Services/CalcoliGriglia.cs:101` `public static IReadOnlyList<string> Intervallo(IReadOnlyList<string> chiaviInOrdine, string ancora, string fine)` — `fine` assente → lista vuota; `ancora` assente → `[fine]`
- `Services/CalcoliGriglia.cs:118` `public static string TestoCella(TipoColonna tipo, object? valore)`
- `Services/CalcoliGriglia.cs:136` `public static string TestoModificabile(TipoColonna tipo, object? valore)`
- `Services/SchemaCampi.cs:177` `internal static string RiduciAccenti(string testo)`
- `Services/TabellaSpese.cs:8` `public enum StatoSpesa { Registrata, Prevista, InArrivo }`
- `Services/TabellaSpese.cs:11` `public sealed record RigaSpesa(Expense Spesa, StatoSpesa Stato)` con `public string Chiave => …` (:17: `Id` per le registrate, `"<recurring_id>:<yyyy-MM>"` per le altre) e `public bool ContaNeiTotali => Stato != StatoSpesa.InArrivo;` (:22)
- `Services/TabellaSpese.cs:26` `public enum TipoPeriodo { QuestoMese, MeseScorso, UltimiTreMesi, QuestAnno, Libero }`
- `Services/TabellaSpese.cs:30` `public sealed record FiltriSpese(IReadOnlySet<string> Categorie, Guid? Pagante, string Testo, IReadOnlySet<StatoSpesa> Stati)` con `public static FiltriSpese Nessuno { get; }` (:32)
- `Services/TabellaSpese.cs:37` `public sealed record VistaTabella(TipoPeriodo Periodo, DateTime? Da, DateTime? A, FiltriSpese Filtri, string OrdinaPer, bool Decrescente, string? RaggruppaPer)` con `Predefinita { get; }` (:61), `public static VistaTabella DaQuery(string query)` (:67), `public string InQuery()` (:114)
- `Services/TabellaSpese.cs:144` `public sealed record EsitoModifica(Expense? Nuova, string? Errore);`
- `Services/TabellaSpese.cs:146` `public enum AzioneMassa { CambiaCategoria, Elimina }`
- `Services/TabellaSpese.cs:151` `public sealed record EsitoMassa(int Richieste, int Toccate, int Rifiutate, int Sparite)` con `public static EsitoMassa DaEliminazione(IReadOnlyCollection<Guid> richieste, IReadOnlySet<Guid> presentiPrima, IReadOnlySet<Guid> presentiDopo)` (:157) e `public static EsitoMassa DaModifica(IReadOnlyCollection<Guid> richieste, IReadOnlySet<Guid> toccate, IReadOnlySet<Guid> nonToccateAncoraPresenti)` (:166)
- `Services/TabellaSpese.cs:176` `public static class TabellaSpese`: `Colonne` (:184: `Data, Descrizione, Categoria, Importo, Pagante, Stato`), `Gruppi` (:190: `Categoria, Pagante, Mese`), `Righe(SpeseDelPeriodo)` (:196), `QuantePreviste(IEnumerable<RigaSpesa>)` (:202), `Intervallo(TipoPeriodo, DateTime oggi, DateTime? da, DateTime? a)` (:208), `Filtra(IReadOnlyList<RigaSpesa>, FiltriSpese)` (:224), `Modificabile(RigaSpesa, Guid? mioId, IReadOnlyList<Space>)` (:240), `DataRagionevole(DateTime?, DateTime oggi)` (:246), `Applica(Expense, string colonna, string testo, DateTime oggi)` (:253), `TestoStato(RigaSpesa)` (:307), `TestoEsitoMassa(AzioneMassa, EsitoMassa)` (:316)
- In più, non nel piano: `internal const string FormatoData = "yyyy-MM-dd";` (`Services/TabellaSpese.cs:179`), che è anche il formato del valore di un `<input type="date">`.

ADJUDICA:
Brief A
- conformity #1 (il BOM è scritto come carattere U+FEFF letterale e invisibile): fondato → corretto. `Services/CalcoliGriglia.cs:70` `var csv = new StringBuilder("\uFEFF");`
  verificato: risolto — Services/CalcoliGriglia.cs:70; il Grep del carattere U+FEFF su tutti i `*.cs` non trova nulla
- conformity #2 (il Numero in cella non è reso come in `ValoriElemento`): fondato → corretto, **da ratificare**. In cella si usa `ValoriElemento.Testo(valore, "number")` (al più due decimali, come nelle collezioni). Nel campo modificabile e nel CSV resta il valore pieno, perché lì la stringa si rilegge e arrotondarla riscriverebbe il dato (è la ragione di `Denaro.TestoDigitabile`). Nuovo test `Il_numero_in_cella_si_legge_come_nelle_collezioni_ma_si_modifica_ed_esporta_esatto`.
  verificato: risolto — Services/CalcoliGriglia.cs:127 `TipoColonna.Numero => ValoriElemento.Testo(valore, "number"),`; :145; :85-87
- backend-expert #1 (BOM invisibile): è lo stesso rilievo di conformity #1, corretto dallo stesso fix.
  verificato: risolto — Services/CalcoliGriglia.cs:70
- backend-expert #2 (ternario annidato sui null nel comparatore): fondato → corretto con la RISCRITTURA.
  verificato: risolto — Services/CalcoliGriglia.cs:27-28 `if (a is null) return b is null ? 0 : -1;` / `if (b is null) return 1;`
- backend-expert #3 (`Intervallo` copia la lista e poi rienumera l'originale): fondato → corretto con la RISCRITTURA.
  verificato: risolto — Services/CalcoliGriglia.cs:113 `return chiavi.GetRange(da, Math.Abs(indiceAncora - indiceFine) + 1);`
Brief B
- bug-hunter #1 (una descrizione oltre 200 caratteri passa `Applica`, poi il database la rifiuta con l'errore generico): fondato → corretto. `Services/TabellaSpese.cs:269-270` `if (descrizione.Length > LunghezzaMassimaDescrizione) return new EsitoModifica(null, "La descrizione può avere al massimo 200 caratteri.");`, più il test `La_descrizione_non_supera_il_limite_del_database`.
  verificato: risolto — Services/TabellaSpese.cs:266,269-270
- bug-hunter #2 (`?periodo=libero&da=2026-01-01&a=9999-12-31` arriva a `CalcoliRicorrenti.Dovuti`, che genera un'occorrenza al mese fino al 9999 per ogni regola senza fine): fondato → corretto. La guardia sta in `TabellaSpese.Intervallo`, l'unico punto da cui le date arrivano al repository, così copre sia l'URL sia i campi data della pagina: un periodo libero con un estremo fuori da `DataRagionevole` torna a questo mese. `DaQuery` non cambia. Ho riaperto io il codice a valle (`ExpenseRepository.ElencaConPrevisteAsync`, `CalcoliRicorrenti.Dovuti`): non c'è nessun tetto, quindi il rilievo regge. Nuovo test `Un_periodo_libero_fuori_dalla_guardia_delle_date_torna_a_questo_mese`.
  verificato: risolto — Services/TabellaSpese.cs:218-220 `TipoPeriodo.Libero when DataRagionevole(da, oggi) && DataRagionevole(a, oggi) && da!.Value.Date <= a!.Value.Date`
- conformity #1 (la normalizzazione «senza accenti, maiuscolo» è scritta due volte, in `TabellaSpese.Normalizza` e in `CalcoliGriglia.Confronta`): fondato → **fuori scope**. Per unificarla servirebbe un helper in `Services/SchemaCampi.cs`, dove il mandato concede una parola sola. Le due copie non sono nemmeno identiche: il filtro fa anche `Trim`. `Services/TabellaSpese.cs:233` `private static string Normalizza(string testo) => SchemaCampi.RiduciAccenti(testo.Trim()).ToUpperInvariant();`
- backend-expert #1 (`DaQuery` usa `Array.Find` con un pattern sul default della tupla, e LINQ annidato sugli stati): fondato → corretto con la RISCRITTURA. La tupla del default ha i nomi espliciti: senza, l'inferenza li perdeva (CS1061).
  verificato: risolto — Services/TabellaSpese.cs:88 `var periodo = Periodi.FirstOrDefault(p => p.Nome == Valore("periodo"), (Nome: "mese", Tipo: TipoPeriodo.QuestoMese)).Tipo;`; :103-104
- backend-expert #2 (`"yyyy-MM-dd"` è una costante in `VistaTabella` e un literal in `Applica`): fondato → corretto, ora c'è una sola costante `TabellaSpese.FormatoData`.
  verificato: risolto — Services/TabellaSpese.cs:179; il Grep di `"yyyy-MM-dd"` nel file trova solo la costante
Note dei revisori fuori conteggio (nessuna è un rilievo):
- threat-hunter A: le **etichette** delle colonne non passano dalla protezione anti-formula, come prescrive il brief. Oggi nessuna etichetta la scrive un utente. Quando la fase 3 costruirà le colonne dai campi di una collezione (`CampoDefinizione`), l'etichetta sarà testo scritto da un altro membro e l'intestazione andrà protetta.
- threat-hunter A: l'elenco OWASP aggiunge TAB e CR iniziali a `= + - @`. La decisione dell'utente copre i quattro caratteri; non è provato che Excel valuti come formula un campo che comincia con TAB.
- bug-hunter A: un valore non `DateTime` in una colonna `Data`, o non numerico in una `Numero`, lancia un'eccezione. Oggi le colonne sono fisse nel codice; il punto riguarda l'adattatore della fase 3.
- checker: mancano due spazi dopo la virgola (`Services/TabellaSpese.cs:123,276`), solo formattazione. `Services/CalcoliGriglia.cs:144` ha ancora il literal `"yyyy-MM-dd"`, fuori dal perimetro di B4.
Campione sugli infondati: nessun rilievo è stato giudicato infondato, quindi non c'era niente da riverificare. Il rilievo su dati e risorse (bug-hunter B #2) l'ho riaperto io, come scritto sopra.

FUORI SCOPE:
- La normalizzazione del testo duplicata fra `TabellaSpese.Normalizza` e `CalcoliGriglia.Confronta` (conformity B #1): servirebbe un helper accanto a `SchemaCampi.RiduciAccenti`.
- Fase 3: proteggere anche le etichette dell'intestazione del CSV quando arriveranno da campi definiti dagli utenti, e decidere cosa fa la griglia davanti a un valore del tipo sbagliato.

GATE:
`mcp__synapse__build root=G:/Sviluppo/Eton/.claude/worktrees/unita-04-tabella-calcoli` → succeeded — 0 errors, 0 warnings
`mcp__synapse__test root=G:/Sviluppo/Eton/.claude/worktrees/unita-04-tabella-calcoli` → 0 failed, 380 passed, 0 skipped

SCOSTAMENTI:
- Test: **380**, non i 377 del piano. I 3 in più vengono dall'adjudica: `Il_numero_in_cella_si_legge_come_nelle_collezioni_ma_si_modifica_ed_esporta_esatto` (in CalcoliGriglia, che ha 15 test invece di 14), `Un_periodo_libero_fuori_dalla_guardia_delle_date_torna_a_questo_mese` e `La_descrizione_non_supera_il_limite_del_database` (in TabellaSpese, che ne ha 30 invece di 28). Il gate del task 4 (unità B) va quindi letto come 380 + 8 = **388**, non 385.
- Contratto, comportamento diverso dal piano a parità di firme: `TabellaSpese.Intervallo(Libero, …)` torna a questo mese anche quando un estremo è fuori da `DataRagionevole` (dal 2000-01-01 a oggi + 1 anno). Chi costruisce la pagina (C) non deve aggiungere un'altra guardia sulle date del periodo libero.
- Contratto, comportamento in più: `TabellaSpese.Applica` rifiuta una descrizione oltre 200 caratteri. Nella pagina (C) l'`<input>` della cella Descrizione può avere `maxlength="200"` come il registro, ma la guardia c'è già nella parte pura.
- Contratto, resa: `CalcoliGriglia.TestoCella(Numero, …)` usa `ValoriElemento.Testo(…, "number")` (al più due decimali); `TestoModificabile(Numero, …)` e il CSV portano il valore pieno. **Da ratificare.** La tabella delle spese non ha colonne `Numero`, quindi oggi non si vede: conta per la fase 3.
- Un simbolo nuovo che il piano non prevede: `TabellaSpese.FormatoData` (`internal const`, `Services/TabellaSpese.cs:179`).
- Nel piano il BOM (riga 374 e nei test) è un carattere letterale invisibile; qui è l'escape `\uFEFF`. Chi ricopia dal piano altre stringhe col BOM deve fare lo stesso.
- Lavoro nuovo arrivato durante l'unità: nessuno.
