UNITÀ: 10 — ESITO: FATTO

TOCCATI:
- `Shared/RecensioniElemento.razor` → +45/−7

I file sono **nel working tree, non committati**, come chiede il mandato.

Tutti i `file:line` di questo resoconto li ho riaperti io sul file su disco **dopo** l'ultima
correzione e dopo i gate. Nessuno viene da un revisore.

**Il censimento del capo era esatto su tutte e dodici le righe** — le cinque `{ex.Message}` a
schermo (`:236`, `:255`, `:396`, `:448`, `:553`), le tre in console (`:276`, `:309`, `:464`), i
due «Il database ha rifiutato…» (`:434`, `:547`), i due già buoni (`:337`, `:441`). Riaperti uno
per uno prima di toccare qualsiasi cosa: nessuno sfasato. I numeri che seguono sono invece quelli
**finali**, spostati dalle mie stesse aggiunte.

---

## CONTRATTI

### Le sette frasi, verbatim, e da quale delle sei dell'unità 05 discende ciascuna

Due sono **identiche carattere per carattere** al modello; cinque sono adattamenti la cui
divergenza è motivata sotto e nel commento accanto al codice.

```csharp
// Shared/RecensioniElemento.razor:245 — catch dell'identificazione utente, in Carica()
"Non è stato possibile riconoscere il tuo accesso: può essere la connessione, oppure la sessione che è scaduta. Finché non si risolve, la tua recensione non viene riconosciuta come tua: ricarica la pagina, e se non basta esci e rientra."
```
Discende da **`CollectionEdit:611`** (fallimento in scrittura), non da `:426`, e la scelta conta
per la 13: ha quattro elementi, non tre — fatto, causa doppia, **conseguenza**, azione — perché la
regola 3 impone di dire cosa succede a ciò che l'utente ha in mano. Qui la conseguenza non è «il
lavoro è perso» ma «la tua recensione non viene distinta dalle altre», che è l'analogo esatto: con
`io` nullo, `mia` resta nulla (`:254`), la scheda «La tua recensione» compare vuota e su una
collezione cieca si resta coperti.

```csharp
// Shared/RecensioniElemento.razor:268 — catch della lettura recensioni, in Carica()
"Non è stato possibile leggere le recensioni: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento."
```
Discende da **`CollectionEdit:426`** (fallimento in lettura), che ricalca sostituendo solo
l'oggetto. L'azione «Riprova» è letterale: sotto c'è già un pulsante `Riprova` (`:18`), perché
questo ramo accende `recensioniNonLette`.

```csharp
// Shared/RecensioniElemento.razor:414 — catch di Crea(), ramo in cui la rilettura non trova nulla
// Shared/RecensioniElemento.razor:474 — catch di Modifica()
"Non è stato possibile salvare: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Quello che hai scritto è ancora qui: riprova fra un momento, e non chiudere la pagina."
```
**`CollectionEdit:611` verbatim**, in entrambi i punti. La promessa «Quello che hai scritto è
ancora qui» è vera in tutti e due: né `Crea` né `Modifica` toccano `mioVoto` e `mioCommento` prima
di quei rami, e il commento a `:418` lo dichiarava già per il ramo accanto.

```csharp
// Shared/RecensioniElemento.razor:458 — case EsitoSalvataggio.Rifiutata, in Modifica()
"Non hai più il permesso di modificare questa recensione: può farlo solo chi l'ha scritta, e da quando hai aperto la pagina il tuo accesso può essere cambiato. Ricarica la pagina."
```
Discende da **`CollectionEdit:667`**, la sesta frase — quella preesistente che l'unità 05 ha
tenuto come metro. Stessa architettura, due divergenze:
- **cade «o chi possiede lo spazio»**, perché su `reviews` la policy non ha quel ramo (sotto);
- il verbo canonico di `Permessi.cs` è «modificarla», ma ripeterlo dopo «modificare» violerebbe
  l'avvertenza esplicita di `Services/Permessi.cs:88-91` («evitare di dire lo stesso verbo due
  volte nella stessa frase»): diventa «può **farlo** solo chi l'ha scritta».

`Permessi.Spiegazione` **non è riusata per interpolazione** — e qui, a differenza di
`CollectionEdit`, non potrebbe esserlo comunque: `Services/Permessi.cs:85-96` non ha un
`Oggetto.Recensione`, e i quattro casi che ha finiscono tutti con «o chi possiede lo spazio», che
su `reviews` sarebbe falso.

```csharp
// Shared/RecensioniElemento.razor:579 — ramo else di Elimina(), quando EliminaAsync ha reso false
"Non è stato possibile togliere la recensione: può darsi che non ci sia già più, tolta da un altro dispositivo o sparita insieme all'elemento, oppure che il tuo accesso a questo spazio sia cambiato. Ricarica la pagina per vedere com'è adesso."
```
Discende da **`CollectionEdit:743`** (rifiuto senza eccezione), di cui conserva la struttura e la
chiusa letterale «ricarica la pagina per vedere com'è adesso». Le cause nominate sono le **due
vere**, lette in `Services/ReviewRepository.cs:209-215`: `prima.Models.Count == 0` (riga non più
leggibile) e `dopo.Models.Count != 0` (DELETE rifiutata). «Togliere» e non «eliminare» perché è il
verbo del pulsante che lo provoca (`:72`, «Togli la mia recensione»).

```csharp
// Shared/RecensioniElemento.razor:591 — catch di Elimina()
"Non è stato possibile eliminare: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere se la recensione c'è ancora."
```
Discende da **`CollectionEdit:748`**, ma **la terza parte diverge, e la 13 deve saperlo**: il
modello dice «La collezione è ancora al suo posto», qui la frase corrispondente sarebbe **falsa in
un caso raggiungibile**. `EliminaAsync` fa **tre** chiamate di rete
(`Services/ReviewRepository.cs:209`, `:212`, `:214`: lettura preventiva, DELETE, rilettura di
conferma), quindi un'eccezione può scoppiare **dopo** una cancellazione riuscita. Dove la 13
trovasse un `Elimina` che chiama un metodo con una sola chiamata, la promessa del modello torna
lecita: **è una divergenza del repository, non della schermata**. Commento a `:586`.

### Le cinque righe di diagnosi che le accompagnano

Marcatore `[RecensioniElemento]`, azione al passato, due punti, `{ex.Message}`:

```csharp
Console.Error.WriteLine($"[RecensioniElemento] Identificazione dell'utente non riuscita: {ex.Message}");   // :244
Console.Error.WriteLine($"[RecensioniElemento] Lettura delle recensioni non riuscita: {ex.Message}");      // :267
Console.Error.WriteLine($"[RecensioniElemento] Creazione della recensione non riuscita: {ex.Message}");    // :399
Console.Error.WriteLine($"[RecensioniElemento] Salvataggio della recensione non riuscito: {ex.Message}");  // :473
Console.Error.WriteLine($"[RecensioniElemento] Eliminazione della recensione non riuscita: {ex.Message}"); // :590
```

Quella di `Crea()` sta **in cima al catch**, prima del bivio: così anche il ramo che finisce con
l'avviso «Avevi già recensito questo elemento altrove…» lascia traccia dell'eccezione invece di
scartarla in silenzio. È l'unico dei cinque punti in cui la posizione non è ovvia.

### Nessun helper, e la ragione vale anche per la 13

Nessun tipo, metodo, costante, campo o `using` nuovo. Le frasi di `:414` e `:474` sono identiche
fra loro e **restano due letterali**: sono uguali oggi per caso — due `catch` che possono divergere
domani, uno su una INSERT e uno su un UPDATE — non per contratto.

### Quello che NON ho scritto, e perché la 13 non deve riscriverlo

**Il messaggio sul proprietario dello spazio che non può moderare non esiste, di proposito.**

Verificato in `supabase/migrations/20260812200000_recensioni.sql`: `reviews_update` (`:127-130`) e
`reviews_delete` (`:132-134`) hanno la sola condizione `user_id = auth.uid()`, senza il ramo
`is_space_owner` che note, collezioni, elementi e spese hanno tutti; il commento a `:116-122`
dichiara la divergenza deliberata («un voto è un'opinione personale e riscriverla sarebbe
falsificarla, non moderare») e indica la via d'uscita, cancellare l'elemento per via
dell'`ON DELETE CASCADE`. Tutto confermato.

**Ma il caso non è raggiungibile dall'interfaccia**, e il mandato dice che allora il messaggio è
codice morto. La prova: nel markup, le recensioni altrui sono rese dentro `riga-recensione`
(`:118` e seguenti) con tre soli elementi — nome e data, voto, commento — e **nessuna azione**; il
pulsante «Togli la mia recensione» sta a `:72`, dentro un `@if (mia is not null)`, e `mia` è per
costruzione la riga il cui `UserId` coincide con quello di chi guarda (`:254`). Chi legge un
messaggio di rifiuto su un'eliminazione **è per forza l'autore**: dirgli «nemmeno chi possiede lo
spazio può togliere la tua recensione» sarebbe rispondere a una domanda che non ha fatto.

Il fatto dell'unità 08 non si perde, però: è scritto nel commento a `:576`, dove il prossimo che si
chiederà perché manca «o chi possiede lo spazio» lo trova.

---

## ADJUDICA

**istruttoria: 1 rilievo su 1 file → checker no** (soglia: somma ≥ 4 fra `bug-hunter` e
`conformity`, oppure ≥ 3 file distinti; qui 0 + 1 su un solo file).

`bug-hunter` **0** · `conformity` **1** · `threat-hunter` **0**. Niente `backend-expert`: 45 righe,
nessuna superficie nuova — il budget la vietava.

**`conformity`, `:245` (ora), media, «errori» → INFONDATO.**
Claim: la frase ha quattro componenti invece delle tre del contratto, e la divergenza non è
motivata da un commento. **Il metro citato è quello sbagliato fra due che il contratto mette sullo
stesso piano.** La prova addotta è `Pages/CollectionEdit.razor:426` — la forma *minima*, un
fallimento in lettura che non ha conseguenze da dichiarare. Ma fra le sei frasi verbatim c'è anche
`Pages/CollectionEdit.razor:611`, che ho riaperto: *«Non è stato possibile salvare: il database ha
rifiutato la scrittura, oppure non è stato raggiunto.* **Quello che hai scritto è ancora qui:**
*riprova fra un momento, e non chiudere la pagina.»* — quattro elementi, con la conseguenza
incastrata fra causa e azione, cioè la stessa architettura della frase contestata. È la regola 3
del contratto a imporre quel quarto elemento («dire se il lavoro è perso è parte del messaggio»).
Non c'è divergenza, quindi non serve un commento che ne giustifichi una.

**Il campione sugli infondati, come impone il §5**: è questo, l'unico rilievo dell'unità. L'ho
riaperto io, senza passare da nessuno, su tutti e tre i file in gioco —
`Shared/RecensioniElemento.razor:245`, `Pages/CollectionEdit.razor:426` e `:611` — ed è
**infondato**, per la ragione sopra.

**Un difetto istruito da me, non da un revisore, e corretto.** Il commento che avevo fatto
scrivere in cima al primo `catch` citava le tre diagnosi preesistenti come `(:276, :309, :464)`:
numeri giusti nel file di partenza, sfasati **dalle mie stesse aggiunte** poche righe sopra di
loro — oggi quelle tre righe stanno a `:289`, `:322`, `:490`. L'ho trovato
con un `grep` mentre i revisori lavoravano; `bug-hunter` l'ha poi riportato per conto suo come
nota, giustamente non come rilievo (è un commento, non è eseguito). Corretto rendendo i riferimenti
**nominali** invece che numerici — «i nomi degli autori, il conteggio dei recensori, la rilettura
silenziosa» — così non si sfaseranno mai più. **La 13 lo copi: in un file che si sta allungando,
un `file:line` interno allo stesso file è un riferimento a scadenza.**

**La domanda più rischiosa del diff**, verificata da me come chiede il mandato — *dopo aver
tradotto, il dettaglio tecnico è ancora recuperabile?*
`Console.Error.WriteLine` **prima 3, dopo 8**. Nessuna tolta, cinque aggiunte. Nel file ci sono
**otto** `catch (Exception ex)` (`:233`, `:261`, `:286`, `:319`, `:392`, `:469`, `:488`, `:582`) e
**otto** righe di diagnosi (`:244`, `:267`, `:289`, `:322`, `:399`, `:473`, `:490`, `:590`), una
per ciascuno: la corrispondenza è uno a uno e **non esiste più un `catch` muto in questo file**.
Nessuna indiscrezione barattata con una cecità. `threat-hunter` ha rifatto il conteggio per conto
suo e coincide.

---

## FUORI SCOPE

**Un rilievo fondato che non ho risolto, perché il mandato me lo vieta per nome.**

`Shared/RecensioniElemento.razor:465`, `case EsitoSalvataggio.Sparita` in `Modifica()`:
«La tua recensione non c'è più.» Il mandato lo elenca fra i «due già buoni che non si toccano», e
chiede di verificare che sia vero prima di lasciarlo stare. **Ho verificato, e non è vero:**

- dice il **fatto** e nient'altro — nessuna causa, nessuna azione: è la forma che la regola 2
  dell'unità 05 esclude;
- soprattutto **tace l'unica cosa che l'utente deve sapere**. Le tre righe immediatamente sopra
  (`:462-464`) azzerano `mia`, `mioVoto` e `mioCommento`: il voto e il commento che l'utente aveva
  appena digitato **spariscono dallo schermo nello stesso istante in cui compare il messaggio**. È
  esattamente la domanda a cui la regola 3 dell'unità 05 impone di rispondere, e questo messaggio
  la lascia senza risposta mentre la risposta è «sì, è perso».

Un obiettivo («segui il modello a sei frasi») e un divieto («questi due non si toccano») si
contraddicono, e il mandato prescrive di obbedire **al più specifico**: il divieto nomina la riga,
l'obiettivo no. **Quindi non l'ho toccata.** Il rimedio appartiene al capo, che può assegnarla a un
giro successivo o all'unità 13 se decide che il difetto è di specie.

Una frase possibile, se il capo la vuole, nella forma di `:743` + regola 3:
«La tua recensione non c'è più: può averla tolta tu da un altro dispositivo, oppure il tuo accesso
a questo spazio è cambiato. Quello che avevi appena scritto è stato tolto dal modulo: per rimetterla,
riscrivila e salva.»

**Nessuna proposta di helper condiviso.** Il budget lo vietava e non è servito: confermo la
diagnosi dell'unità 05 dal mio lato: un `TraduciErrore(ex)` saprebbe meno di chi lo chiama, perché
qui la frase giusta dipende da *quale schermata stava aspettando* — «la tua recensione non viene
riconosciuta come tua» non è deducibile da nessuna eccezione.

---

## GATE

- `dotnet build -warnaserror` → **Avvisi: 0, Errori: 0**. Compilazione completata in 00:00:06.85.
- `dotnet test` → **Superato! Non superati: 0. Superati: 273. Ignorati: 0. Totale: 273.**
  Esattamente i 273 di partenza: questa unità non aggiunge né tocca test, e non poteva —
  nessun test copre le stringhe di un componente.
- Server di sviluppo **non avviato**, browser **non usato**: il mandato lo vieta. Nessun processo
  lasciato vivo, nessuna porta occupata.
- Compilato **io, una volta, a fine giro**. Gli `implementer` non hanno compilato.

---

## SCOSTAMENTI

1. **Marcatore di console `[RecensioniElemento]`, non `[Recensioni]`.** Il resoconto dell'unità 05
   suggeriva `[Recensioni]` per questo file. Ho usato `[RecensioniElemento]` perché è quello che il
   file **già usa** nelle sue tre diagnosi preesistenti (`:289`, `:322`, `:490`), e la regola 1 del
   contratto dice «l'idioma che il progetto già usa» — che batte un nome proposto da chi quel file
   non l'aveva aperto. **La 13 verifichi lo stesso sui propri quattro file** invece di fidarsi del
   marcatore suggerito: `SpaceDetail.razor` potrebbe usarne un altro ancora.

2. **`:591` non promette che la recensione è al suo posto**, dove `CollectionEdit:748` lo promette.
   Motivo verificato nel repository, spiegato sopra in `CONTRATTI` e nel commento a `:586`.

3. **`:579` e `:458` non citano «o chi possiede lo spazio»**, dove tutte le formulazioni di
   `Permessi.cs` lo citano. Su `reviews` sarebbe falso. Commento a `:576`.

4. **Il messaggio sul proprietario che non può moderare non è stato aggiunto**: caso non
   raggiungibile dall'interfaccia, prova in `CONTRATTI`. È il ramo che il mandato prevedeva.

5. **Un numero di riga sbagliato nel mio brief, non nel censimento del capo.** Avevo scritto
   all'`implementer` che il `catch` di `Crea()` cominciava a `:387`: era `:379`. L'`implementer` ha
   localizzato il punto dal contenuto testuale — univoco — invece di bloccarsi, e me l'ha
   dichiarato. Nessun effetto sul risultato; lo annoto perché è la stessa classe di errore del
   punto 3 dell'`ADJUDICA`, e l'ho commessa io due volte nello stesso giro.

6. **Nessuno scostamento sul censimento del capo**: tutte e dodici le righe erano esatte. Lo
   registro perché il mandato avvisava che le `file:line` erano state sfasate su due unità
   precedenti — qui non lo erano.

---

## DA PROVARE NEL BROWSER

Nessuna di queste prove è stata fatta: il mandato vieta di avviare il server. Il testo è quello
esatto che deve comparire.

| # | Testo esatto atteso | Come provocarlo |
|---|---|---|
| 1 | «Non è stato possibile leggere le recensioni: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento.» | DevTools → Network → **Offline**, poi aprire un elemento di una collezione. Compare **con il pulsante Riprova** e senza il resto della scheda. |
| 2 | «Non è stato possibile salvare: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Quello che hai scritto è ancora qui: riprova fra un momento, e non chiudere la pagina.» | Aprire un elemento **mai recensito**, scrivere voto e commento, mettere **Offline**, premere Salva. Verificare che voto e commento **restino nel modulo** — è ciò che la frase promette. Stesso testo su un elemento **già recensito** (percorso `Modifica`, `:474`). |
| 3 | «Non è stato possibile togliere la recensione: può darsi che non ci sia già più, tolta da un altro dispositivo o sparita insieme all'elemento, oppure che il tuo accesso a questo spazio sia cambiato. Ricarica la pagina per vedere com'è adesso.» | **Due schede, un solo account.** Aprire lo stesso elemento in A e B, entrambe con la propria recensione. In A premere «Togli la mia recensione» e confermare. In B, **senza ricaricare**, premere lo stesso pulsante: la lettura preventiva di `EliminaAsync` non trova più la riga e rende `false`. |
| 4 | «Non è stato possibile eliminare: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. Ricarica la pagina per vedere se la recensione c'è ancora.» | Su un elemento con la propria recensione: **Offline**, poi «Togli la mia recensione» e confermare. |
| 5 | In console, otto marcatori `[RecensioniElemento]` | Durante le prove 1-4 tenere aperta la **console DevTools**: ogni errore a schermo deve avere accanto la sua riga `[RecensioniElemento] … non riuscit*: {…}` col JSON per esteso. **Se un messaggio compare senza la riga in console, è un difetto.** |

**Due casi che non so far provocare a mano, dichiarati come limite invece di inventare una
procedura che non funziona:**

- **`:245`** («Non è stato possibile riconoscere il tuo accesso: …»). Servirebbe far fallire
  `AuthState.GetUserIdAsync()` **senza** far fallire la lettura delle recensioni che segue. Andando
  offline cadono entrambe, e `errore` viene **sovrascritto** da quello della lettura (`:268`): il
  messaggio esiste ma a schermo non si vede mai in quel modo. Cancellare il token da
  `localStorage` è la strada più promettente, ma non l'ho provata e non garantisco che produca
  un'eccezione anziché un `null` pulito. **Da verificare prima di dichiararlo collaudato.**
- **`:458`** («Non hai più il permesso di modificare questa recensione: …»). Richiede che la RLS
  rifiuti un UPDATE su una riga **ancora leggibile** e con la **versione invariata**
  (`Services/ReviewRepository.cs:179-181`). Con `reviews_update` = `user_id = auth.uid()` e `mia`
  che è per costruzione la propria riga, dall'interfaccia non ci si arriva: servirebbe cambiare
  `user_id` della riga direttamente nel database fra il caricamento e il Salva. **Non è codice
  morto** — il `case` esisteva già e il ramo è raggiungibile se la sessione cambia identità sotto
  la pagina — ma **non è collaudabile a mano**, e chi proverà non lo veda come un fallimento.
