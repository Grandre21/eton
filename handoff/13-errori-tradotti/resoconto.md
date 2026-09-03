UNITÀ: 13 — ESITO: FATTO

TOCCATI:
- `Pages/NoteEdit.razor` → +45/−5
- `Pages/ItemEdit.razor` → +59/−17
- `Pages/SpesaEdit.razor` → +48/−10
- `Pages/SpaceDetail.razor` → +70/−9
- `Pages/CollectionDetail.razor` → +19/−8
- `Shared/RecensioniElemento.razor` → +1/−1 — la sola `:465`, come da mandato

`handoff/PIANO.md` risulta modificato nel working tree ma **non è mio**: è la chiusura dell'unità 10
e la riga di `DECISIONI` sulla `:465`, scritte dal capo prima di aprirmi. Non l'ho toccato.

Tutti i `file:line` di questo resoconto li ho riaperti io sul disco **dopo l'ultima correzione**.
Nessuno viene da un revisore.

---

## IL CENSIMENTO DEL CAPO ERA ESATTO

Tutti e diciotto i numeri del mandato combaciavano con il file su disco prima che toccassi
qualunque cosa — comprese le righe di `SpesaEdit`, che l'unità 12 aveva spostato. Nessuno
scostamento da dichiarare su questo punto.

---

## CONTRATTI

### La regola che ha deciso metà delle frasi, e che il mandato non poteva sapere

**Prima di copiare una promessa, ho aperto il metodo del repository e contato le chiamate di rete**
— la lezione 3 dell'unità 10. L'esito ha cambiato **quattro** messaggi su cui il modello sarebbe
stato copiabile a occhio:

| Metodo | Chiamate di rete | Conseguenza sul messaggio |
|---|---|---|
| `SpaceRepository.RinominaAsync` | **1** (una UPDATE) | la promessa «lo spazio si chiama ancora come prima» è **lecita** |
| `SpaceRepository.EsciAsync` | **2** (DELETE + rilettura) | nessuna promessa: l'eccezione può scoppiare **dopo** la rimozione |
| `SpaceRepository.EliminaAsync` | **3** (lettura, DELETE, rilettura) | nessuna promessa |
| `NoteRepository.EliminaAsync` | **3** | nessuna promessa |
| `CollectionItemRepository.EliminaAsync` | **3** | nessuna promessa |
| `ExpenseRepository.EliminaAsync` | **3** | nessuna promessa |

Quindi i tre `catch` di eliminazione degli editor **non discendono da `CollectionEdit:748`**, che
promette «La collezione è ancora al suo posto», ma da `RecensioniElemento:591`, che quella promessa
l'aveva già dovuta abbandonare per la stessa ragione. La divergenza è motivata in un commento
accanto a ciascuno dei tre.

### `Pages/NoteEdit.razor` — marcatore `[Note]` (da `Pages/Notes.razor:141`)

```csharp
// :236 — apertura (catch di OnParametersSetAsync). Da CollectionEdit:426, oggetto sostituito.
"Non è stato possibile aprire la nota: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento."

// :263 — salvataggio. CollectionEdit:611 VERBATIM.
"Non è stato possibile salvare: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Quello che hai scritto è ancora qui: riprova fra un momento, e non chiudere la pagina."

// :357 — sovrascrittura. CollectionEdit:712 VERBATIM.
"Non è stato possibile sovrascrivere: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Quello che hai scritto è ancora qui: riprova fra un momento, e non chiudere la pagina."

// :391 — eliminazione, catch. Da RecensioniElemento:591, NON da CollectionEdit:748: v. la tabella sopra.
"Non è stato possibile eliminare: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere se la nota c'è ancora."

// :382 — RIFIUTO senza eccezione. Da CollectionEdit:743; le parole della regola da Permessi.cs:93.
"Non è stato possibile eliminare la nota: può cancellarla solo chi l'ha scritta, o chi possiede lo spazio. Può anche darsi che qualcun altro l'abbia già eliminata: ricarica la pagina per vedere com'è adesso."
```
Diagnosi: `:235`, `:262`, `:356`, `:390` — `[Note] Apertura|Salvataggio|Sovrascrittura|Eliminazione
non riuscit*`.

### `Pages/ItemEdit.razor` — marcatore `[Elemento]`

```csharp
// :276 — apertura. Da CollectionEdit:426.
"Non è stato possibile aprire l'elemento: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento."

// :369 — salvataggio. CollectionEdit:611 VERBATIM.
// :459 — sovrascrittura. CollectionEdit:712 VERBATIM.
// (identiche a quelle di NoteEdit, sopra)

// :494 — eliminazione, catch. Da RecensioniElemento:591.
"Non è stato possibile eliminare: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere se l'elemento c'è ancora."

// :484 — RIFIUTO senza eccezione. Da CollectionEdit:743; parole da Permessi.cs:95.
"Non è stato possibile eliminare l'elemento: può cancellarlo solo chi l'ha aggiunto, o chi possiede lo spazio. Può anche darsi che qualcun altro l'abbia già eliminato: ricarica la pagina per vedere com'è adesso."
```
Diagnosi: `:275`, `:368`, `:458`, `:493`.

**Il marcatore `[Elemento]` è l'unico nuovo del diff, e la scelta va dichiarata.** Il dominio non ne
aveva uno: `Console.Error.WriteLine` non compariva in `ItemEdit.razor`. Non ho riusato
`[Collezione]` perché in console non si distinguerebbe da `CollectionEdit` e `CollectionDetail`, e
ho seguito la forma singolare che quei due file già usano, col nome che il progetto dà all'oggetto
in `Permessi.Oggetto.Elemento`. `[Note]` e `[Spese]` invece **non** sono scelte mie: esistevano già
in `Notes.razor:141` e `Spese.razor:412`, ed è la regola del mandato — l'idioma che il progetto già
usa batte un nome inventato.

### `Pages/SpesaEdit.razor` — marcatore `[Spese]` (da `Pages/Spese.razor:412`)

```csharp
// :304 — apertura. Da CollectionEdit:426.
"Non è stato possibile aprire la spesa: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento."

// :328 — salvataggio. CollectionEdit:611 VERBATIM.
// :413 — sovrascrittura. CollectionEdit:712 VERBATIM.

// :446 — eliminazione, catch. Da RecensioniElemento:591.
"Non è stato possibile eliminare: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere se la spesa c'è ancora."

// :437 — RIFIUTO senza eccezione. Da CollectionEdit:743; parole da Permessi.cs:92.
"Non è stato possibile eliminare la spesa: può cancellarla solo chi l'ha pagata, o chi possiede lo spazio. Può anche darsi che qualcun altro l'abbia già eliminata: ricarica la pagina per vedere com'è adesso."
```
Diagnosi: `:303`, `:327`, `:412`, `:445`.

«Chi l'ha **pagata**» e non «segnata»: è il verbo che `Permessi.Spiegazione(Oggetto.Spesa)` usa per
la regola, e il commento a `Services/Permessi.cs:87-91` dichiara che i due verbi divergono apposta.

### `Pages/SpaceDetail.razor` — marcatore `[Spazi]`, nove punti

Qui **non ho copiato niente**, come il mandato prescriveva: le cinque azioni sono distruttive o
sociali e «quello che hai scritto è ancora qui» non ha senso per nessuna.

```csharp
// :215 — caricamento, catch. Le due cause sono quelle che QUESTO catch non sa distinguere:
// la prima chiamata del try è AuthState.GetUserIdAsync(). "Non ne fai più parte" NON è fra loro —
// quel caso non lancia, rende zero righe, e ha già il suo ramo a schermo (:47-53).
"Non è stato possibile caricare lo spazio: può essere la connessione, oppure la sessione che è scaduta. Riprova fra un momento, e se non basta esci e rientra."

// :259 — rinomina, catch. UNICA promessa dell'intero file, e lecita: RinominaAsync fa una chiamata.
"Non è stato possibile rinominare: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Il nome che hai scritto è ancora qui e lo spazio si chiama ancora come prima: riprova fra un momento."

// :299 — rimozione di un membro, catch. EsciAsync fa DUE chiamate: nessuna promessa.
$"Non è stato possibile togliere {m.Nome} dallo spazio: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere chi ne fa parte adesso."

// :338 — uscita, catch. EsciAsync, DUE chiamate.
"Non è stato possibile uscire dallo spazio: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere se ne fai ancora parte."

// :376 — eliminazione, catch. EliminaAsync, TRE chiamate.
"Non è stato possibile eliminare lo spazio: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere se lo spazio c'è ancora."
```

E i quattro rifiuti senza eccezione, che erano le «Il database ha rifiutato…»:

```csharp
// :235 — rinomina rifiutata (era «Il database ha rifiutato la modifica: solo il proprietario può rinominare.»)
"Non è stato possibile rinominare lo spazio: può darsi che non ci sia più, eliminato da un altro dispositivo, oppure che il tuo accesso sia cambiato. Ricarica la pagina per vedere com'è adesso: il nome che hai scritto qui non è stato salvato."

// :277 — rimozione rifiutata (era «Il database ha rifiutato la rimozione.»)
$"Non è stato possibile togliere {m.Nome} dallo spazio: può rimuovere un membro solo chi possiede lo spazio, e da quando hai aperto la pagina il tuo accesso può essere cambiato. Ricarica la pagina per vedere com'è adesso."

// :316 — uscita rifiutata (era «Il database ha rifiutato l'uscita.»)
"Non è stato possibile uscire dallo spazio: chi lo possiede non può uscirne — per andarsene deve eliminarlo — e da quando hai aperto la pagina le cose possono essere cambiate. Ricarica la pagina per vedere com'è adesso."

// :355 — eliminazione rifiutata (era «Il database ha rifiutato l'eliminazione.»)
"Non è stato possibile eliminare lo spazio: può eliminarlo solo chi lo possiede, e può anche darsi che non ci sia già più, eliminato da un altro dispositivo. Ricarica la pagina per vedere com'è adesso."
```
Diagnosi nuove: `:214`, `:258`, `:298`, `:337`, `:375`. Le quattro preesistenti (`:247`, `:287`,
`:328`, `:365`) sono intatte.

**Due decisioni di merito su queste quattro, entrambe della famiglia «non rispondere a una domanda
che chi legge non ha fatto» — è la regola che l'unità 10 ha ricavato sulla moderazione delle
recensioni, e qui si applica due volte:**

1. **«Solo il proprietario può rinominare» è caduto.** Il modulo di rinomina vive dentro
   `@if (sonoProprietario && !spazio.IsPersonal)` (`:66`): quel messaggio lo leggerebbe **soltanto
   il proprietario**. Le cause raggiungibili sono altre — lo spazio non c'è più, o il ruolo è
   cambiato da quando la pagina è aperta — e sono quelle che si dicono.

2. **`:235` dice che il nome digitato è perso, invece di prometterlo salvo.** L'azione onesta qui è
   *ricaricare* (riprovare un rifiuto lo ripete, come ha stabilito `RecensioniElemento:458`), e
   ricaricando `nome` torna a `spazio.Name`. La regola 4 dell'unità 05 vieta di promettere e poi
   buttare: fra le due, si rinuncia alla promessa. È l'unico punto del diff dove la regola 3 e la
   regola 4 tiravano in direzioni opposte.

### `Pages/CollectionDetail.razor` — marcatore `[Collezione]`

```csharp
// :252 — apertura. CollectionEdit:426 VERBATIM: stesso guasto, stesso oggetto, stesse parole.
"Non è stato possibile aprire la collezione: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento."
```
Diagnosi `:251`: `[Collezione] Apertura **del dettaglio** non riuscita`. «Del dettaglio» non è
ornamento: `CollectionEdit:425` usa già `[Collezione] Apertura non riuscita`, e due righe identiche
in console per due schermate diverse non si distinguerebbero.

«Riprova» è **letterale** qui: il ramo d'errore rende `<ErroreRiprova … OnRiprova="Riprova" />`
(`:30`), che il pulsante ce l'ha davvero.

### `Shared/RecensioniElemento.razor:465` — una riga, e la verifica che il mandato chiedeva

**Verificato prima di scrivere**: `:462-464` azzerano davvero `mia`, `mioVoto` e `mioCommento`.
L'unità 10 aveva ragione, e la frase non mente.

```csharp
// :465 — case EsitoSalvataggio.Sparita, in Modifica()
"La tua recensione non c'è più: puoi averla tolta tu da un altro dispositivo, o può essere sparita insieme all'elemento, oppure il tuo accesso a questo spazio è cambiato. Quello che avevi appena scritto è stato tolto dal modulo: se vuoi rimetterla, riscrivila e salva."
```

Due ritocchi alla frase che l'unità 10 aveva lasciato pronta, entrambi verificati sul codice:

- **grammatica**: «puoi averla tolta tu» invece di «può averla tolta tu»;
- **una terza causa**, che mancava: `Sparita` significa `LeggiAsync` → `null`
  (`Services/ReviewRepository.cs:164-167`), cioè la riga non è **leggibile**. Le cause vere sono
  tre, non due: tolta da un altro dispositivo, sparita insieme all'elemento
  (`ON DELETE CASCADE` della chiave esterna composita verso `collection_items`,
  `supabase/migrations/20260812200000_recensioni.sql:63`), o accesso allo spazio cambiato. Sono
  esattamente le tre che la stessa unità 10 aveva già nominato a `:579` per il ramo gemello: la
  frase ora è coerente con la sua vicina.

**Non ho aggiunto il commento** che pure sarebbe stato utile: il mandato dice «una riga sola», ed è
la prescrizione più specifica. Le tre righe che azzerano il modulo stanno immediatamente sopra e si
leggono da sé.

### Il caso che nessun altro file ha — verificato, e la risposta è NO

Il mandato chiedeva di verificare **aprendo la migrazione** se un rifiuto su `SpaceDetail` possa
nascere dal fatto che il proprietario non può moderare le recensioni.

Confermato il fatto: `reviews_update` (`20260812200000_recensioni.sql:127-130`) e `reviews_delete`
(`:132-134`) hanno la sola condizione `user_id = auth.uid()`.

**Ma su questa pagina non può produrre nessun rifiuto, e quindi il messaggio non lo dice.** Le
cinque azioni di `SpaceDetail` sono rinominare lo spazio, rimuovere un membro, uscire, eliminare lo
spazio, caricare: **nessuna scrive su `reviews`**. La tabella non ha nemmeno una chiave esterna
verso `spaces` — referenzia `collection_items (id, space_id)` (`:63`) — quindi l'eliminazione di
uno spazio la raggiunge per catena di `CASCADE`, che è un'azione di integrità referenziale e non
passa dalle policy RLS. Scrivere «nemmeno chi possiede lo spazio può togliere una recensione» in un
messaggio d'errore di questa pagina sarebbe stato codice morto.

Il fatto **non si perde**: sta già a schermo, nell'`<Aiuto>` della testata (`:31`), scritto
dall'unità 08. È lì che chi si fa la domanda la trova.

### Nessun helper, e le tre quaterne NON sono identiche

Nessun tipo, metodo, campo, costante, `using` o helper nuovo. Il divieto che il mandato prevedeva
avrei avuto voglia di violare **non ha nemmeno tirato**, e il motivo merita una riga perché è un
fatto, non un'impressione: delle dodici frasi dei tre editor, **sei sono identiche** (i due
`salvare`/`sovrascrivere` per file, verbatim dal modello) e **sei divergono nell'oggetto** — «la
nota», «l'elemento», «la spesa» — e nel verbo della regola di permesso: «chi l'ha **scritta**», «chi
l'ha **aggiunto**», «chi l'ha **pagata**», che vengono da tre rami diversi di
`Permessi.Spiegazione`. Un helper che prendesse un'eccezione non saprebbe scegliere fra quei tre
verbi, che è esattamente l'argomento dell'unità 05.

Le sei identiche restano **sei letterali distinti** per la ragione dell'unità 10: sono uguali oggi
per caso — sono `catch` che possono divergere domani — non per contratto.

---

## RILIEVO 9 — dove la riga serve e dove no

Applicata su **tutti e tre** gli editor, e la condizione è calibrata file per file.

- `Pages/NoteEdit.razor:99` — `@if (!occupato && PuoIntervenire && conflitto is null && !Cambiata)`.
  Ternario su `Nuova`: **«Scrivi il titolo o il testo della nota per poterla salvare.»**, perché
  `Cambiata` in creazione è `!IsNullOrWhiteSpace(titolo) || !IsNullOrWhiteSpace(corpo)` — basta uno
  dei due, e dire «il titolo» avrebbe mentito.
- `Pages/ItemEdit.razor:119` — stessa condizione, flag `Nuovo` (maschile, è così che il file lo
  chiama). **«Scrivi il nome dell'elemento per poterlo salvare.»**
- `Pages/SpesaEdit.razor:143` — `&& ImportoValido` **in più**, e **nessun ternario**: la pagina non
  crea, `Id` è un `Guid` obbligatorio. Una frase sola: «Non c'è niente da salvare: non hai ancora
  cambiato niente.»

  `ImportoValido` nella condizione non è cautela: senza, con un importo o una data non validi la
  riga direbbe «non hai ancora cambiato niente» **accanto a un `.errore-campo` rosso che dice il
  contrario**. È il rilievo che si è preso l'unità 04, evitato.

**Su `SpaceDetail` e `CollectionDetail` non l'ho messa**: non sono editor, non hanno un «Salva»
spento, il rilievo non li riguarda. Il mandato lo diceva e l'ho verificato.

**La decisione lasciata aperta dall'unità 07 è chiusa in favore del sì.** Aveva scritto che su
`SpesaEdit` le altre cause di spegnimento hanno già ciascuna il proprio messaggio — vero, e proprio
per questo `!Cambiata` è l'**unica** scoperta, che è precisamente il caso che la riga copre.

---

## LO STATO VUOTO DI `CollectionDetail`

`Pages/CollectionDetail.razor:61` — aggiunto
`<a class="btn primario" href="collections/@Id/items/new">Nuovo elemento</a>` dentro `.vuoto`,
identico carattere per carattere a quello già in testata (`:46`). Icona, `<p>` e `<p class="spiega">`
invariati.

Il commento che diceva l'opposto — «Niente pulsante qui dentro» — è stato sostituito con il
ragionamento dell'unità 09, adattato: a registro vuoto l'occhio sta sul blocco centrato, ed è lì che
va l'azione; a registro pieno il blocco non esiste e resta solo la testata.

**Le due verifiche di CSS chieste dal mandato, riaperte in un istante e confermate:**
- `wwwroot/css/app.css:592` — `.vuoto .btn { margin-top: var(--s4); }` ✔
- `wwwroot/css/app.css:638-639` — `.btn { display: inline-flex; }` ✔

Nessuna riga di CSS aggiunta, `app.css` non è nel diff. Nessun `BLOCKED` da accodare all'unità 11.

---

## ESITO DEI DICIOTTO PUNTI

Uno per riga, com'è stato chiesto. La colonna «ora» è il numero riaperto sul disco a lavoro finito.

| # | file | punto | era | ora | esito |
|---|---|---|---|---|---|
| 1 | `NoteEdit` | aprire | `:214` | `:236` (console `:235`) | **tradotto** |
| 2 | `NoteEdit` | salvare | `:235` | `:263` (console `:262`) | **tradotto** |
| 3 | `NoteEdit` | sovrascrivere | `:328` | `:357` (console `:356`) | **tradotto** |
| 4 | `NoteEdit` | eliminare | `:351` | `:391` (console `:390`) | **tradotto** |
| 5 | `ItemEdit` | aprire | `:254` | `:276` (console `:275`) | **tradotto** |
| 6 | `ItemEdit` | salvare | `:340` | `:369` (console `:368`) | **tradotto** |
| 7 | `ItemEdit` | sovrascrivere | `:429` | `:459` (console `:458`) | **tradotto** |
| 8 | `ItemEdit` | eliminare | `:452` | `:494` (console `:493`) | **tradotto** |
| 9 | `SpesaEdit` | aprire | `:284` | `:304` (console `:303`) | **tradotto** |
| 10 | `SpesaEdit` | salvare | `:302` | `:328` (console `:327`) | **tradotto** |
| 11 | `SpesaEdit` | sovrascrivere | `:386` | `:413` (console `:412`) | **tradotto** |
| 12 | `SpesaEdit` | eliminare | `:408` | `:446` (console `:445`) | **tradotto** |
| 13 | `SpaceDetail` | caricare | `:206` | `:215` (console `:214`) | **tradotto** |
| 14 | `SpaceDetail` | rinominare | `:236` | `:259` (console `:258`) | **tradotto** |
| 15 | `SpaceDetail` | rimuovere un membro | `:262` | `:299` (console `:298`) | **tradotto** |
| 16 | `SpaceDetail` | uscire | `:290` | `:338` (console `:337`) | **tradotto** |
| 17 | `SpaceDetail` | eliminare lo spazio | `:316` | `:376` (console `:375`) | **tradotto** |
| 18 | `CollectionDetail` | aprire | `:242` | `:252` (console `:251`) | **tradotto** |

**Diciotto su diciotto tradotti. Nessuno lasciato com'era, nessuno non raggiungibile.**

### I sette punti in più, e perché li ho presi

Non erano nei diciotto. Sono nei miei file, sono la stessa famiglia, e **nessuno li possedeva**:
l'unità 05 aveva preparato lo stampo per quattro di essi assegnandoli all'unità 08, l'unità 08 li ha
formalmente restituiti come orfani (`08-home-spazio-profilo/resoconto.md`, `FUORI SCOPE` 1), e il
testo stesso del rilievo 3 dice che le due famiglie «vale la pena trattarle insieme»
(`01-ricognizione-ui/rilievi.md:130`). Sono l'ultima unità che tocca il C#: dopo di me nessuno
riapre questi file.

| # | file | punto | era | ora | esito |
|---|---|---|---|---|---|
| E1 | `NoteEdit` | rifiuto di eliminazione | `:347` | `:382` | **tradotto** (era «Non è stato possibile eliminare la nota.») |
| E2 | `ItemEdit` | rifiuto di eliminazione | `:448` | `:484` | **tradotto** |
| E3 | `SpesaEdit` | rifiuto di eliminazione | `:404` | `:437` | **tradotto** |
| E4 | `SpaceDetail` | rifiuto di rinomina | `:219` | `:235` | **tradotto** (era «Il database ha rifiutato la modifica…») |
| E5 | `SpaceDetail` | rifiuto di rimozione | `:247` | `:277` | **tradotto** |
| E6 | `SpaceDetail` | rifiuto di uscita | `:273` | `:316` | **tradotto** |
| E7 | `SpaceDetail` | rifiuto di eliminazione | `:301` | `:355` | **tradotto** |

Il motivo che ha deciso, oltre alla proprietà del file: E1–E3 stanno **due righe sopra** un `catch`
che ho riscritto, ed E4–E7 idem. Lasciarli avrebbe messo due registri a due centimetri di distanza
sulla stessa schermata — che è testualmente ciò contro cui mette in guardia la regola 5 dell'unità
05.

---

## LA DOMANDA PIÙ RISCHIOSA — dopo la traduzione il dettaglio tecnico è ancora recuperabile?

Contati da me, file per file, sul disco a lavoro finito:

| file | `catch (Exception ex)` | `Console.Error.WriteLine` | `ex.Message` |
|---|---|---|---|
| `NoteEdit.razor` | 4 | 4 | 4 |
| `ItemEdit.razor` | 4 | 4 | 4 |
| `SpesaEdit.razor` | 4 | 4 | 4 |
| `SpaceDetail.razor` | 9 | 9 | 9 |
| `CollectionDetail.razor` | 3 | 3 | 3 |
| `RecensioniElemento.razor` | 8 | 8 | 8 |

**Corrispondenza uno a uno in tutti e sei. Nessun `catch` muto. Nessuna riga di diagnosi tolta: il
diff ne aggiunge 18 e non ne rimuove nessuna** — `NoteEdit`, `ItemEdit` e `SpesaEdit` passano da
**zero** a quattro, `SpaceDetail` da quattro a nove, `CollectionDetail` da due a tre,
`RecensioniElemento` resta a otto.

E `ex.Message` compare **esattamente tante volte quanti sono i `catch`**, cioè solo dentro le righe
di console: nessuna via residua verso lo schermo in questi sei file.

(`SpaceDetail` ha un decimo `catch`, quello di `Copia()` a `:388`, che non cattura una variabile
d'eccezione ed è preesistente e deliberato — il commento accanto lo dichiara. Fuori conteggio.)

---

## ADJUDICA

Gate della review eseguito come prescritto: `bug-hunter`, `conformity` e `threat-hunter`, tutti e
tre nello stesso messaggio, sul diff dei sei file.

    istruttoria: 0 rilievi su 0 file → checker no

Sotto entrambe le soglie del §4 (somma ≥ 4, oppure ≥ 3 file distinti), e non per poco: la somma è
zero. **Nessun `checker`.** Non essendoci rilievi infondati, non c'è nulla da ricampionare.

- **`bug-hunter` → 0 rilievi.** Ha riaperto i quattro repository e verificato **contro il codice** i
  conteggi di chiamate di rete che i miei commenti dichiarano (`RinominaAsync` 1, `EsciAsync` 2,
  `EliminaAsync` 3 su spazio, nota, elemento e spesa). Ha verificato il bilanciamento dei quattro
  `try/catch/finally` espansi in `SpaceDetail`, compresa la sopravvivenza di `confermaRichiesta =
  false`; lo scope di `m` nelle due interpolazioni superstiti; l'assenza di `$` superfluo sui
  letterali che non interpolano più; e le cause nominate contro le policy di
  `20260811000000_initial_schema.sql`.
- **`conformity` → 0 rilievi.** Ha confermato i tre marcatori riusati (`[Note]`, `[Spese]`,
  `[Spazi]`) contro le occorrenze preesistenti e ha giudicato `[Elemento]` motivato e non arbitrario;
  ha verificato **con un grep sull'intera frase** che le sei copie verbatim siano davvero identiche
  al modello, e che ogni divergenza abbia il suo commento accanto al codice.
- **`threat-hunter` → 0 rilievi.** Ha aggiunto l'argomento che chiude nel merito la questione della
  console in WebAssembly, e non solo per anzianità: **il corpo JSON della risposta PostgREST arriva
  comunque al client via rete prima che l'eccezione nasca**, quindi chi ha accesso sufficiente al
  browser per leggere la console ha già accesso diretto allo stesso payload. Ha confermato che
  `errore` è sempre reso come testo — l'unico `MarkupString` del progetto è in
  `Shared/MarkdownView.razor`, per il corpo delle note, che questo diff non tocca.

**Le `file:line` di `threat-hunter` su questo diff sono esatte**: le ho riaperte, come il mandato
avvertiva di fare dopo gli sfasamenti delle unità 04 e 05.

### Una osservazione non-rilievo di `bug-hunter`, che conservo perché il browser la vedrà

Subito dopo un salvataggio riuscito, la riga «Non c'è niente da salvare: non hai ancora cambiato
niente.» compare **insieme** all'avviso «Salvata.», perché il salvataggio riallinea i campi e
`Cambiata` torna falsa. Non è un difetto introdotto qui: è identico al comportamento già in
produzione su `CollectionEdit:244-251`. Lo segnalo perché è la cosa più visibile del rilievo 9 e chi
proverà nel browser la incontrerà al primo salvataggio.

---

## FUORI SCOPE

Tre cose fondate che **non ho risolto**, con il proprietario del rimedio.

### 1. Il rilievo 3 NON si chiude con questa unità: restano dieci punti in quattro file

È il fatto più importante di questo resoconto, ed è la ripetizione esatta dell'errore che ha fatto
nascere l'unità 13 — una mappa *file → unità* invece di *rilievo → unità*. Censimento fatto da me
con `grep` su `Pages`, `Shared` e `Services`, riaperto sul disco:

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

**Le tre di `Shared/PaginaRegistro.cs` contano più delle altre sette messe insieme**: è la classe
base dei registri, quindi quelle tre righe si mostrano su **ogni** pagina di elenco — note,
collezioni, spese. Dieci righe di codice, ma molte più schermate.

**`Pages/Spaces.razor:121` è la peggiore del gruppo**, e vale la pena dirlo: è un fallimento in
**creazione** su una tabella con RLS, cioè esattamente la condizione — 42501 su una INSERT — che
nell'osservazione originale del rilievo 3 produsse a schermo `GRANT INSERT ON public.collections TO
authenticated;`. È lo stesso identico scenario, su un'altra pagina.

Il rimedio è già scritto: lo stampo dell'unità 05, le cinque regole, e adesso anche undici frasi in
più fra questa unità e la 10. Serve un'unità che possieda quei quattro file. **Non l'ho fatto io
perché il perimetro dice «Sei file, tutti chiusi da unità precedenti e riaperti apposta per te.
Nessun altro»**, ed è la prescrizione più specifica.

### 2. `CollectionEdit.razor:748` fa una promessa falsa in un caso raggiungibile

La frase dell'unità 05 dice «**La collezione è ancora al suo posto**: riprova fra un momento.» Ma
`CollectionRepository.EliminaAsync` fa **tre** chiamate di rete — lettura preventiva (`:146`),
DELETE (`:149`), rilettura di conferma (`:151`) — quindi un'eccezione può scoppiare **dopo** una
cancellazione riuscita, e in quel caso la collezione non è affatto al suo posto.

È lo stesso difetto che l'unità 10 ha evitato su `RecensioniElemento:591` e che io ho evitato su
tutti e quattro i miei `catch` di eliminazione. **L'unità 05 non aveva sbagliato a ragionare: aveva
scritto la regola giusta senza aver contato le chiamate** — è stata l'unità 10 a scoprire che
andavano contate. Il rimedio è una frase: sostituire la promessa con «Ricarica la pagina per vedere
se la collezione c'è ancora.»

`Pages/CollectionEdit.razor` è nel mio **NON TOCCARE** come modello. Serve una riga di mandato.

### 3. Il mio diff ha sfasato cinque `file:line` incrociate in file che non posso toccare

Allungando `NoteEdit.razor` (+16 righe a `:87`) e `CollectionDetail.razor` (+7 a `:243`) ho reso
stantii i riferimenti che altri file facevano a quelle righe **per numero**:

| dove | cita | stato |
|---|---|---|
| `Pages/CollectionEdit.razor:254` | `NoteEdit.razor:91-94` | ora `:107-110` |
| `Pages/CollectionEdit.razor:424` | `CollectionDetail.razor:308,322` | ora `:319,333` |
| `Pages/CollectionEdit.razor:636` | `NoteEdit.razor:260-263` | ora `:288-291` |
| `Pages/Home.razor:317` | `NoteEdit.razor:119` | ora `:135` |
| `Pages/Spese.razor:276` | `NoteEdit.razor:201-205` | ora `:217-221` — **ma era già sbagliato** |

Le **cinque** che stavano dentro i miei sei file le ho corrette io, rendendole **nominali** invece
che numeriche — è il rimedio che l'unità 10 ha ricavato per i riferimenti interni, e vale identico
per quelli incrociati. Queste cinque no: i file sono fuori perimetro.

**Due riferimenti erano già sbagliati prima di me**, e vale la pena registrarlo perché mostra che il
problema non è nato con questo diff:
- `CollectionDetail.razor:256` citava `NoteEdit.razor:161` per l'azzeramento di `nota`, ma la 161
  conteneva un commento su `PuoIntervenire`. Questo l'ho corretto io, rendendolo nominale.
- `Pages/Spese.razor:276` cita `NoteEdit.razor:201-205` per «il messaggio se la sessione non è
  valida», ma quelle righe erano il commento su `nota = null`: il messaggio stava alla `:254`
  (oggi `:282`). Fuori perimetro, quindi resta com'è — e ora è sbagliato due volte.

**Il rimedio strutturale, se il capo lo vuole**, non è rinumerare — durerebbe fino al prossimo diff
— ma convertire a nominali le `file:line` **incrociate** rimaste. Sono poche e le ho contate: le
cinque qui sopra. Non è urgente e non è mio.

---

## GATE

- `dotnet build -warnaserror --no-incremental` → **0 errori, 0 avvisi**. Non incrementale di
  proposito: con i generatori di sorgente di Razor una build incrementale può non rigenerare la
  classe, e questo diff tocca il markup di cinque file.
- `dotnet test --no-build` → **273 superati**, 0 non superati, 0 ignorati (254 ms). Esattamente il
  numero che il mandato dichiarava alla partenza.

Eseguiti **due volte**: una dopo il rientro dei sei `implementer`, e una dopo la correzione dei
riferimenti incrociati. Stesso esito.

Compilato **io**, e agli `implementer` e ai revisori l'ho vietato esplicitamente nel brief, perché
`obj/` non ha lock fra processi e ne ho lanciati sei che scrivevano.

**Non ho avviato il server di sviluppo e non ho provato nel browser**, come il mandato prescrive.

---

## SCOSTAMENTI

1. **Ho tradotto sette punti in più dei diciotto**, tutti dentro il mio perimetro. Motivo, prova e
   ragionamento nella tabella `E1–E7` sopra. È l'unico allargamento del diff, ed è dichiarato invece
   che nascosto.

2. **Ho corretto cinque `file:line` incrociate nei miei file**, rendendole nominali. Non era nel
   mandato: è la riparazione di un danno che il mio stesso diff ha prodotto. Solo commenti, nessuna
   riga eseguibile, quindi nessuna review secondo il gate del §3 — ma build e test rifatti dopo.

3. **`RecensioniElemento.razor:465`: non ho aggiunto il commento** che avrei voluto. «Una riga sola»
   è la prescrizione più specifica e ha vinto, come il mandato prescrive per i conflitti.

4. **La frase di `:465` diverge in due punti** da quella che l'unità 10 aveva lasciato pronta: una
   correzione grammaticale e una **terza causa** che mancava. Entrambe verificate sul codice e
   motivate in `CONTRATTI`.

5. **Il messaggio di rifiuto della rinomina non promette che il nome digitato sia salvo**, e anzi
   dice che è perso. È l'unico punto in cui le regole 3 e 4 dell'unità 05 tiravano in direzioni
   opposte; la scelta e il motivo sono in `CONTRATTI`.

6. **Nessun `BLOCKED`, nessuna domanda in sospeso.** Il mandato dice che l'utente non è
   raggiungibile: non ho incontrato niente che richiedesse di fermarsi.

---

## DA PROVARE NEL BROWSER

I messaggi d'errore di questo diff hanno un problema di prova che va detto prima dell'elenco: **la
quasi totalità nasce da un `catch` su una chiamata di rete o da un rifiuto della RLS, e nessuna delle
due si provoca a mano dall'interfaccia.** Preferisco dichiararlo che inventare procedure che non
funzionano.

### Si provano davvero, senza toccare il database

**1 — La riga «cosa manca per salvare» (rilievo 9), tre schermate.**
- `/notes/new`: la pagina si apre e sotto i campi si legge **«Scrivi il titolo o il testo della nota
  per poterla salvare.»** Scrivi una lettera nel titolo: la riga sparisce e «Salva» si accende.
  Cancellala: torna.
- `/collections/{id}/items/new`: stessa cosa con **«Scrivi il nome dell'elemento per poterlo
  salvare.»**
- Su una nota o un elemento **esistenti**, e su una spesa da `/expenses/{id}`: appena aperti si legge
  **«Non c'è niente da salvare: non hai ancora cambiato niente.»** Cambia un carattere: sparisce.
  Rimettilo com'era: torna.
- **Su `SpesaEdit`, la prova che conta**: scrivi un importo non valido (per esempio `12,,5`). Deve
  comparire **solo** il messaggio rosso dell'importo, **non** la riga «non hai ancora cambiato
  niente» — sono le due frasi che si contraddirebbero.
- **Da aspettarsi, e non è un difetto**: subito dopo un salvataggio riuscito la riga «Non c'è niente
  da salvare» compare **insieme** all'avviso verde «Salvata.». È il comportamento già in produzione
  su `/collections/{id}/edit`.

**2 — Il pulsante nello stato vuoto della collezione.**
Apri una collezione **senza elementi**. Sotto «Ancora nessun elemento in questa collezione» e la
riga di spiegazione deve comparire un pulsante **«Nuovo elemento»**, centrato e largo quanto il suo
testo — non steso per tutta la larghezza — e staccato dal testo. Porta a
`collections/{id}/items/new`, come quello in testata. Aggiungi un elemento: il blocco sparisce e
resta solo il pulsante in testata.

**3 — Che nessun messaggio nuovo sia rimasto muto in console.**
Tieni aperti gli strumenti per sviluppatori mentre fai la prova 1: non deve comparire nessuna riga
`[Note]`, `[Elemento]`, `[Spese]`, `[Spazi]`, `[Collezione]` finché tutto va bene. Serve da
controprova che le diagnosi nuove non scattino su percorsi normali.

### Si provano solo staccando la rete

**4 — I sei messaggi di apertura e i sei di salvataggio.**
Con gli strumenti per sviluppatori, scheda Rete, metti la modalità **offline**, poi:
- ricarica `/notes/{id}` → **«Non è stato possibile aprire la nota: può essere la connessione,
  oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento.»** e in console
  `[Note] Apertura non riuscita: …`. Idem per `/collections/{id}/items/{itemId}`
  (`[Elemento]`), `/expenses/{id}` (`[Spese]`), `/spaces/{id}` (`[Spazi]`, con la frase sulla
  sessione scaduta), `/collections/{id}` (`[Collezione] Apertura del dettaglio non riuscita`).
- con la pagina già caricata, **poi** vai offline, cambia un campo e premi Salva → **«Non è stato
  possibile salvare: … Quello che hai scritto è ancora qui: riprova fra un momento, e non chiudere
  la pagina.»** **La cosa da guardare è che il testo digitato sia ancora nel campo**: è la promessa
  che il messaggio fa.
- offline, premi Elimina su una nota → **«Non è stato possibile eliminare: … Ricarica la pagina per
  vedere se la nota c'è ancora.»** — e verifica che **non** dica «la nota è ancora al suo posto»,
  che è la divergenza deliberata di questo diff.
- offline, su `/spaces/{id}` premi «Salva» della rinomina → **«… Il nome che hai scritto è ancora
  qui e lo spazio si chiama ancora come prima: riprova fra un momento.»** È l'unica promessa di
  quella pagina; controlla che il nome digitato sia davvero ancora nel campo.

**5 — La sovrascrittura.** Serve la scheda del conflitto, quindi due sessioni: apri la stessa nota in
due finestre, salva nella prima, poi salva nella seconda → compare la scheda del conflitto. **A quel
punto** vai offline e premi «Sovrascrivi» → **«Non è stato possibile sovrascrivere: … Quello che hai
scritto è ancora qui …»**

### Limiti dichiarati — non li ho provati e non so come provarli senza toccare il database

- **I sette messaggi di rifiuto (`E1–E7`)** nascono tutti da un metodo di repository che ha reso
  `false`, cioè da una policy RLS che ha filtrato. Dall'interfaccia **non sono raggiungibili**: i
  pulsanti che li innescano sono già dentro i `@if` che escludono le condizioni di rifiuto. Per
  vederli servirebbe un secondo utente e una manipolazione concorrente — per esempio: due
  dispositivi sullo stesso spazio, uno elimina lo spazio mentre l'altro ha la pagina aperta e preme
  «Salva» sulla rinomina. **È una prova reale ma non l'ho eseguita**, e non ho modo di dichiararla
  fatta.
- **`RecensioniElemento:465`** (`EsitoSalvataggio.Sparita` in `Modifica()`) richiede che la
  recensione sparisca **fra** l'apertura della pagina e il salvataggio: due dispositivi, oppure la
  cancellazione dell'elemento da un'altra scheda. Stessa situazione.
- **Il messaggio di uscita rifiutata (`:316`)** è il più difficile di tutti: dice che il proprietario
  non può uscire, ma il pulsante «Esci» al proprietario non viene nemmeno mostrato. È un messaggio
  per una finestra temporale, non per uno stato stabile. **Non credo sia provabile a mano**, e lo
  dichiaro invece di suggerire una procedura che non funziona.
- **Nessuna prova di aspetto**: il foglio di stile è dell'unità 11, che viene dopo. Se `.vuoto .btn`
  o `.testo-tenue` cambiano lì, le prove 1 e 2 vanno rifatte dopo.
