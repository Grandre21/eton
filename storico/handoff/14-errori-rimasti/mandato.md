UNITÀ: 14/14 — Il rilievo 3 si chiude davvero

## PERCHÉ ESISTI, E PERCHÉ SEI LA SECONDA VOLTA

L'unità 13 è nata da un censimento che ha scoperto il rilievo 3 chiuso **su una pagina sola su
sei**. L'unità 13 ha chiuso quelle sei, e nel farlo ha censito il resto: **ne restano dieci, in
quattro file**.

È lo stesso errore due volte, e la causa è la stessa: una mappa *file → unità* dice che un
rilievo è chiuso quando è chiuso nel perimetro di chi l'ha toccato, non quando è chiuso ovunque.

**Tu sei l'unità che lo chiude.** Il censimento qui sotto viene da un `grep` dell'unità 13 su
`Pages`, `Shared` e `Services`, riaperto sul disco. **Rifallo tu**, con lo stesso comando, e se
trovi un undicesimo punto **dillo e trattalo**: è esattamente così che si scopre la terza volta.

## PERIMETRO — file di tua proprietà esclusiva

- `Pages/Benvenuto.razor`
- `Pages/Home.razor`
- `Pages/Spaces.razor`
- `Pages/Spese.razor`
- `Pages/CollectionEdit.razor`
- `Shared/PaginaRegistro.cs`

## LAVORO 1 — i dieci punti del rilievo 3

| file:line | stringa |
|---|---|
| `Pages/Benvenuto.razor:227` | `errore = $"Errore di accesso: {ex.Message}";` |
| `Pages/Home.razor:286` | `errore = $"Non è stato possibile caricare i tuoi spazi: {ex.Message}";` |
| `Pages/Home.razor:368` | `errore = $"{premessa}, ma non ho potuto leggerne i dettagli: {ex.Message}";` |
| `Pages/Spaces.razor:89` | `errore = $"Non è stato possibile caricare i tuoi spazi: {ex.Message}";` |
| `Pages/Spaces.razor:121` | `errore = $"Non è stato possibile creare lo spazio: {ex.Message}";` |
| `Pages/Spaces.razor:161` | `errore = $"Non è stato possibile entrare: {ex.Message}";` |
| `Pages/Spese.razor:327` | `errore = $"Non è stato possibile salvare: {ex.Message}";` |
| `Shared/PaginaRegistro.cs:121` | `errore = $"Spazio caricato, ma non ho potuto leggerne {NomePlurale}: {ex.Message}";` |
| `Shared/PaginaRegistro.cs:140` | `errore = $"Non è stato possibile caricare i tuoi spazi: {ex.Message}";` |
| `Shared/PaginaRegistro.cs:172` | `errore = $"Non è stato possibile aggiornare {NomePlurale}: {ex.Message}";` |

**Due punti valgono più degli altri, e l'ordine in cui li affronti conta.**

1. **Le tre di `Shared/PaginaRegistro.cs` sono la classe base dei registri**: quelle tre righe si
   mostrano su **ogni** pagina di elenco — note, collezioni, spese. Dieci righe di codice in
   tutto, ma molte più schermate. Attenzione: qui `NomePlurale` è interpolato, quindi la frase
   deve reggere per «note», «collezioni» e «spese» insieme.
2. **`Pages/Spaces.razor:121` è la peggiore del gruppo.** È un fallimento in **creazione** su una
   tabella con RLS: è **esattamente** la condizione — 42501 su una INSERT — che nell'osservazione
   originale del rilievo 3 fece comparire a schermo `GRANT INSERT ON public.collections TO
   authenticated;`. Lo stesso identico scenario, su un'altra pagina. **Se ne provi una sola nel
   browser, prova questa.**

## LAVORO 2 — una promessa falsa in `CollectionEdit.razor:748`

La frase dell'unità 05 dice «**La collezione è ancora al suo posto**: riprova fra un momento.»

Ma `CollectionRepository.EliminaAsync` fa **tre** chiamate di rete — lettura preventiva (`:146`),
DELETE (`:149`), rilettura di conferma (`:151`) — quindi un'eccezione può scoppiare **dopo** una
cancellazione riuscita, e in quel caso la collezione non è affatto al suo posto.

**Verificalo tu aprendo il repository e contando**, non fidandoti di questo paragrafo.

**L'unità 05 non aveva sbagliato a ragionare**: aveva scritto la regola giusta senza sapere che
le chiamate andavano contate — è stata l'unità 10 a ricavarlo, tre unità dopo. Il rimedio è una
frase, sulla forma che le unità 10 e 13 hanno già usato quattro volte:

> «Ricarica la pagina per vedere se la collezione c'è ancora.»

## LAVORO 3 — cinque riferimenti incrociati sfasati

Il diff dell'unità 13 ha allungato `NoteEdit.razor` e `CollectionDetail.razor`, rendendo stantii
i riferimenti che **altri file** facevano a quelle righe per numero. Tre dei cinque sono nel tuo
perimetro; **tutti e cinque** lo sono, verificando:

| dove | cita | valore giusto oggi |
|---|---|---|
| `Pages/CollectionEdit.razor:254` | `NoteEdit.razor:91-94` | `:107-110` |
| `Pages/CollectionEdit.razor:424` | `CollectionDetail.razor:308,322` | `:319,333` |
| `Pages/CollectionEdit.razor:636` | `NoteEdit.razor:260-263` | `:288-291` |
| `Pages/Home.razor:317` | `NoteEdit.razor:119` | `:135` |
| `Pages/Spese.razor:276` | `NoteEdit.razor:201-205` | **era già sbagliato prima**: il bersaglio vero è il messaggio a `NoteEdit.razor:282` |

**Non rinumerare: rendi nominali.** È il rimedio che l'unità 10 ha ricavato e la 13 ha applicato
cinque volte — un riferimento per **contenuto** («il commento su `nota = null`», «il blocco che
azzera i campi in creazione») non scade al prossimo diff, un numero sì. Rinumerare durerebbe fino
alla prossima unità che allunga quei file.

**Riapri ciascuno dei cinque bersagli** e verifica che il contenuto sia quello che il commento
vuole citare, prima di scrivere il riferimento nominale. L'ultimo della tabella era sbagliato
**due volte**: puntava a un commento invece che al messaggio.

## IL MODELLO — tre fonti, e le apri tutte

1. **`handoff/05-collezione-rilievi/resoconto.md`** — le sei frasi originali e le cinque regole.
2. **`handoff/10-recensioni-errori/resoconto.md`** — sette frasi, ciascuna con la frase di
   origine, e la lezione sul contare le chiamate di rete.
3. **`handoff/13-errori-tradotti/resoconto.md`** — venticinque frasi su sei file, con la tabella
   dei conteggi di rete per sei metodi di repository. **È la fonte più completa e la più vicina
   al tuo lavoro.**

La forma è **fatto, causa, azione**, più la **conseguenza** quando c'è qualcosa in mano
all'utente che può andare perso.

**Il criterio sulla diagnostica**: tradurre senza registrare baratterebbe un'indiscrezione con
una cecità. Ogni `catch` che traduce deve avere il suo `Console.Error.WriteLine` col dettaglio
per esteso. **Conta i `catch` e conta le righe di diagnosi, file per file, e riporta i due
numeri**: l'unità 10 è arrivata a otto e otto, la 13 ha fatto lo stesso. Nessun `catch` muto.

**Il marcatore di console lo detta il file, non il resoconto precedente.** `[Note]`, `[Spese]`,
`[Spazi]` esistono già nel progetto; `[Elemento]` l'ha creato la 13 perché il dominio non ne
aveva. **Guarda cosa usa ciascuno dei tuoi sei file** prima di sceglierne uno.

## LE TRE COSE CHE SU QUESTI FILE SONO DIVERSE

1. **`Benvenuto.razor` è la pagina di accesso**: chi legge quel messaggio **non è autenticato**,
   e non ha una sessione da «ricaricare». Le frasi degli altri file non si trasferiscono. Guarda
   cosa la pagina offre davvero — un pulsante per riprovare l'accesso, presumibilmente — e scrivi
   l'azione che esiste.
2. **`PaginaRegistro.cs` è C#, non markup**, ed è una **classe base**. Una frase sbagliata qui
   compare su tre pagine. `NomePlurale` è interpolato: rileggi la frase sostituendo mentalmente
   «note», «collezioni» e «spese» e verifica che regga per tutte e tre.
3. **`Spese.razor:327` è un salvataggio in creazione**, non in modifica: «quello che hai scritto
   è ancora qui» va verificato — il modulo di creazione potrebbe azzerarsi. **Apri il metodo e
   guarda** prima di promettere.

## NON TOCCARE

- **`Pages/NoteEdit.razor`, `Pages/ItemEdit.razor`, `Pages/SpesaEdit.razor`,
  `Pages/SpaceDetail.razor`, `Pages/CollectionDetail.razor`, `Shared/RecensioniElemento.razor`**:
  chiusi dalle unità 10 e 13. Li **leggi** come modello.
- **`Services/**`**: nessun repository. L'unità 05 ha rifiutato metà del proprio perimetro
  proprio su questo — dentro `CreaAsync` sai quale query è fallita, non quale schermata la stava
  aspettando. `Shared/PaginaRegistro.cs` è nel tuo perimetro **come classe base di pagine**, non
  come servizio: ci scrivi frasi, non logica.
- **`wwwroot/css/app.css`**: unità 11, che viene **dopo di te**. Se ti serve, torna `BLOCKED`.
- **`Services/Permessi.cs`**: lo **leggi** per prendere le parole delle regole di autorizzazione,
  come hanno fatto le unità 10 e 13. Non lo modifichi.

## BUDGET DI COMPLESSITÀ

Nessun tipo nuovo, nessun servizio, nessun file, **nessun helper di traduzione condiviso** — il
divieto è alla sua quarta unità e il motivo non è cambiato: il messaggio giusto dipende da quale
schermata stava aspettando.

**Ma tu hai un caso che le altre non avevano**, e va detto: `PaginaRegistro.cs` **è già** il posto
condiviso, perché è la classe base. Scrivere lì una frase buona per tre pagine **non è un
helper**: è il codice che ci sta. Non confondere le due cose e non spostarci le frasi degli altri
file.

## STATO

Unità chiuse e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`), 05 (`e139ce8`),
06 (`f4f2dbd`), 07 (`4327598`), 12 (`8a4a89f`), 08 (`bdd858a`), 09 (`d05416b`), 10 (`2650dc7`),
13 (`459a2fc`). Resta solo l'unità 11, il foglio di stile, che viene dopo di te.

**Non committare.** Committa il capo, a resoconto letto.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: vince la riga più recente. C'è una riga del
3 settembre sera che dice che **l'utente non è raggiungibile**: qualunque domanda tu abbia, portala
nel resoconto.

**Se i revisori tornano tutti a zero rilievi, non è finita.** Riga di istruttoria comunque, e
verifica tu almeno la domanda più rischiosa del tuo diff. Le unità 06, 07, 08, 10, 12 e 13
l'hanno fatto e ogni volta ne è uscito qualcosa: il difetto dell'importo, la contraddizione del
piano, il messaggio «già buono» che non lo era, e questa stessa unità.

**La domanda più rischiosa di questo diff, se non ne trovi una migliore:** la frase che scrivi in
`PaginaRegistro.cs` **regge su tutte e tre le pagine che ereditano**? Non è una domanda retorica:
`NomePlurale` cambia genere e numero, e una frase che scorre bene con «note» può stonare con
«spese». Provale tutte e tre a mente, e scrivi nel resoconto le tre versioni risultanti.

## IL GATE DELLA REVIEW

Testo mostrato all'utente a partire da un'eccezione, su sei file, **fra cui una classe base**:
`bug-hunter`, `conformity` e `threat-hunter`, tutti e tre nello stesso messaggio.
`backend-expert` solo se nasce una superficie nuova, che il budget vieta.

**Il tuo diff tocca sei file**: l'istruttoria supererà probabilmente la soglia del §4 (≥ 3 file
distinti citati). In quel caso **lancia `checker`**.

## GATE

- `dotnet build -warnaserror --no-incremental` → **0 errori, 0 avvisi**. Non incrementale: tocchi
  markup e una classe base, e con i generatori di Razor una build incrementale può non
  rigenerare.
- `dotnet test` → **273 superati**, com'erano quando parti.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai.

**Non avviare il server di sviluppo e non provare nel browser.**

BUDGET: 22 dollari

RESOCONTO IN: `handoff/14-errori-rimasti/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 14 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <le frasi scritte, verbatim, per file, e da quale frase del modello discendono>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <cosa resta e a chi appartiene>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

**Chiudi il resoconto con una sezione `IL RILIEVO 3 È CHIUSO?`**, e rispondi con il `grep` che
hai rifatto: quante occorrenze di `errore = $"` restano nel progetto, in quali file, e perché
ciascuna è legittima o non lo è. **Se ne resta anche una, il rilievo non è chiuso e va detto**:
è la terza volta che questa domanda si pone, e le prime due volte la risposta creduta era
sbagliata.

Aggiungi `DA PROVARE NEL BROWSER` col testo esatto e come provocarlo, dichiarando come limite i
casi non raggiungibili a mano.
