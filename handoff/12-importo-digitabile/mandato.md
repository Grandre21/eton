UNITÀ: 12/12 — Una spesa da mille euro in su torna modificabile

**Eseguita fuori numero, subito dopo la 07 e prima della 08.** Il numero è 12 perché i numeri
di cartella non si riusano; l'ordine di esecuzione non è quello della numerazione, ed è
dichiarato nel piano. Il motivo: è l'unico difetto **funzionale bloccante** rimasto, e tocca
`Pages/SpesaEdit.razor`, che l'unità 07 ha appena chiuso. Farla dopo vorrebbe dire riaprire
quel file quando `live-testing` l'ha già provato.

## IL DIFETTO — confermato, non ipotizzato

`Denaro.Testo(1284.50m)` produce `"1.284,50"`, col punto come separatore delle migliaia.
`Denaro.Verifica` rifiuta come `NonNumerico` qualunque stringa con più di un separatore
(`Services/Denaro.cs:80`).

`Pages/SpesaEdit.razor` riempie il campo **modificabile** dell'importo con `Denaro.Testo(...)`
e poi rilegge quella stessa stringa con `Denaro.Prova(...)`. Quindi, aprendo una spesa da
1.284,50 €:

- sotto il campo compare subito «Non è un importo valido: usa la virgola o il punto.», senza
  che l'utente abbia toccato niente;
- «Salva» e «Sovrascrivi» nascono spenti;
- **la spesa non è più modificabile da nessuno**, per sempre.

Sotto i mille euro non si vede, perché non c'è nessun separatore delle migliaia. Il difetto è
**preesistente**: l'ha trovato l'unità 07 cercando l'interazione fra la guardia di navigazione
e la validazione, e l'ha dichiarato fuori scope invece di correggerlo di nascosto.

**In creazione non si manifesta**, verificato: `Pages/Spese.razor` non ha l'accoppiamento —
`nuovoImporto` nasce `""`, è legato all'input e torna `""` dopo la creazione, senza mai passare
da `Testo`. Quindi una spesa da 1.284,50 € **si crea** digitando `1284,50`, e poi **non si
modifica più**.

## LA DECISIONE, GIÀ PRESA — non riaprirla

Il rimedio è **additivo**: nasce un secondo formattatore, e `Verifica` **non si tocca**.

Il motivo, che serve anche a te per adjudicare un revisore che proponesse il contrario:
`Verifica` accetta *sia* `.` *sia* `,` come separatore decimale (`Services/Denaro.cs:45-48`).
A quel punto nessuno dei due può essere **anche** separatore delle migliaia senza ambiguità:
«1.234» sarebbe 1,234 con tre decimali oppure milleduecentotrentaquattro. Allargare `Verifica`
non elimina la classe di difetto, la sposta — chi correggesse «1.284,50» in «1.2845,50»
otterrebbe un rifiuto **mentre digita un numero valido**. Un campo modificabile deve contenere
il valore nella **grammatica di input**, non in quella di visualizzazione.

L'asimmetria fra `Testo` e `Prova` è **voluta e documentata**: il commento a
`Services/Denaro.cs:74-79` la argomenta, e il test
`Eton.Tests/DenaroTests.cs:174-181`, `Testo_e_Prova_non_sono_l_uno_l_inverso_dell_altro_sopra_il_migliaio`,
la sanziona. **Quel test resta invariato nelle asserzioni**: non è lui a essere sbagliato, era
il call-site a violare la distinzione che documenta.

## PERIMETRO — file di tua proprietà esclusiva

- `Services/Denaro.cs`
- `Pages/SpesaEdit.razor`
- `Eton.Tests/DenaroTests.cs`

## NON TOCCARE

- **`Denaro.Verifica` e `Denaro.Prova`**: non una riga, non un `InlineData` dei loro test. Se
  un revisore propone di allargarli, adjudica **infondato** citando il paragrafo «LA DECISIONE».
- **`Pages/Spese.razor`**: verificato che non ha l'accoppiamento. I suoi tre `Denaro.Testo`
  (`:143`, `:153`, `:198`) e quello di `Home.razor:114` sono **markup di sola lettura** con `€`
  accanto: `Testo` lì è il metodo giusto e va lasciato.
- **`Shared/PaginaEditor.cs`** e tutto il resto del contratto degli editor: chiuso da quattro
  unità.
- **`wwwroot/css/app.css`**: unità 11.

## COSA FARE — 1. il nuovo formattatore

In `Services/Denaro.cs`, accanto a `Testo`:

```csharp
public static string TestoDigitabile(decimal importo)
    => importo.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
```

**Il nome è `TestoDigitabile` e non si cambia.** Il progetto nomina in italiano — `Testo`,
`Prova`, `Verifica` — e un `TestoPerInput` mescolerebbe le due lingue nello stesso helper.

- `"F2"` è fixed-point: due decimali, **nessun raggruppamento**. È la ragione per cui non va
  bene `"N2"`, che i gruppi li mette (ed è quello che `Testo` usa a `:117`).
- `CultureInfo.InvariantCulture` più `Replace` invece della cultura italiana: rende il risultato
  indipendente dalla cultura del browser, che su WebAssembly non è quella del server e non è
  garantita.
- Due decimali **sempre**, anche su `7m` → `"7,00"`. Non è cosmesi: `Cambiata` confronta
  **stringhe**, e i due lati del confronto devono passare dalla stessa funzione.

Aggiungi alla docstring di `Testo` (`:106-107`) una riga di rimando: solo per visualizzazione;
in un campo modificabile si usa `TestoDigitabile`, perché `Prova` rifiuta il punto delle
migliaia. È il presidio contro il prossimo call-site che sbaglierebbe allo stesso modo.

## COSA FARE — 2. i QUATTRO punti di `SpesaEdit.razor`

`Denaro.Testo` → `Denaro.TestoDigitabile` in **quattro** punti. Non tre.

| Riga | Cos'è |
|---|---|
| `:210` | dentro `Cambiata`, nel confronto `importoTesto != Denaro.Testo(spesa.Amount)` |
| `:271` | riempimento al caricamento |
| `:317` | riempimento dopo un salvataggio riuscito |
| `:348` | riempimento dopo «Ricarica» |

**La trappola, dichiarata perché è quella che si sbaglia.** Se salti `:210` il difetto non
sparisce, **peggiora in modo silenzioso**: sopra i mille euro il campo conterrebbe `1284,50` e
`Testo(spesa.Amount)` darebbe `1.284,50`, quindi `Cambiata` sarebbe **sempre vera**. «Salva»
resterebbe acceso senza che nessuno abbia modificato niente, e la guardia di navigazione
chiederebbe «hai modifiche non salvate» **a ogni singola uscita**. Sarebbe un difetto peggiore
di quello che stai correggendo, e nessun test lo prenderebbe.

I numeri di riga vengono dal resoconto dell'unità 07 e da una lettura di `tech-advisor`:
**riaprili tu** prima di modificare, e se non tornano usa quelli veri dichiarandolo in
`SCOSTAMENTI`.

## COSA FARE — 3. i test

`Eton.Tests/DenaroTests.cs`. Il test dell'asimmetria (`:174-181`) **resta invariato nelle
asserzioni**; al suo commento (`:169-173`) si aggiunge una riga che rimanda al gemello nuovo.

Test nuovi, tutti obbligatori:

- `TestoDigitabile(1284.50m) == "1284,50"` — il caso del difetto.
- `TestoDigitabile(7m) == "7,00"` — i due decimali ci sono sempre.
- `TestoDigitabile(1000000m) == "1000000,00"` — nessun raggruppamento a nessuna scala.
- **L'andata e ritorno**, che è il test che conta: `Denaro.Prova(Denaro.TestoDigitabile(x), out var y)`
  è vero e `y == x`, per `1284.50m` **e** per `9999999999.99m` — quest'ultimo è il bordo di
  `numeric(12,2)`, il tipo della colonna sul database.

La coppia «il test dell'asimmetria» + «il test dell'andata e ritorno» è ciò che dice a chi verrà
dopo **quale** delle due funzioni usare per un campo modificabile. È il presidio vero: il parser
non può impedire la scelta sbagliata, i due test messi in fila sì.

## BUDGET DI COMPLESSITÀ

Un metodo nuovo, di una riga. Nessun tipo nuovo, nessuna astrazione, nessun file nuovo, nessun
servizio. Se ti trovi a scrivere un parser, a introdurre una maschera di input o a toccare
`Verifica`, sei fuori strada: torna `BLOCKED`.

## STATO

Unità precedenti, tutte `FATTO` e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`),
05 (`e139ce8`), 06 (`f4f2dbd`), 07 (`4327598`).

Il difetto e la sua analisi sono in `handoff/07-spesa-contratto/resoconto.md`, sezione
`FUORI SCOPE`, punto 1. **Leggila**: contiene il percorso esatto e il perché l'unità 07 non
l'ha corretto.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: se ci trovi una riga che contraddice
questo mandato, vince la più recente.

**Se i revisori tornano tutti a zero rilievi, non è finita.** Scrivi comunque la riga di
istruttoria, dichiara che non c'è nessun campione da riverificare, e verifica tu almeno la
domanda più rischiosa del tuo diff. Le unità 06 e 07 l'hanno fatto e in entrambi i casi ne è
uscito qualcosa che i revisori non avevano isolato — nel caso della 07, proprio il difetto che
stai correggendo.

**La domanda più rischiosa di questo diff, se non ne trovi una migliore:** dopo la correzione,
`Cambiata` confronta `importoTesto` con `TestoDigitabile(spesa.Amount)`. Verifica che
**all'apertura** i due lati coincidano carattere per carattere per un importo sopra il migliaio,
sotto il migliaio, e con decimali `,00` — perché se non coincidessero la pagina nascerebbe già
«modificata».

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**.
- `dotnet test` → tutti verdi. Erano **267**; con i tuoi nuovi devono essere **271 o più**, e
  **nessuno dei 267 esistenti può cambiare esito**. Se un test di `Denaro` diventa rosso, hai
  toccato `Verifica` o `Prova`: torna indietro.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai: `obj/` non ha lock
fra processi.

**Non avviare il server di sviluppo e non provare nel browser.**

BUDGET: 15 dollari

RESOCONTO IN: `handoff/12-importo-digitabile/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 12 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <la firma di TestoDigitabile e i quattro call-site, con file:line riaperti da te>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti, e a chi appartiene il rimedio>
GATE: <comando → esito, col numero di test>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

Aggiungi `DA PROVARE NEL BROWSER`. Una prova sola è obbligatoria, ed è quella che chiude il
rilievo: **segnare una spesa da `1284,50`, riaprirla, verificare che il campo mostri `1284,50`
senza messaggio d'errore e che «Salva» sia spento perché non c'è niente da salvare — non perché
l'importo è invalido.** Poi cambiare la descrizione e salvare: deve riuscire.
