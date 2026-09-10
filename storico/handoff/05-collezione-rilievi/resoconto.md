UNITÀ: 05 — ESITO: FATTO

TOCCATI:
- `Pages/CollectionEdit.razor` → +141/−25
- `Services/CollectionRepository.cs` → **non toccato**, e la scelta è motivata sotto in `SCOSTAMENTI`

Tutti i `file:line` di questo resoconto li ho riaperti io sul file su disco dopo l'ultima
correzione. Nessuno viene da un revisore.

---

## CONTRATTI

### La forma esatta dei messaggi tradotti — verbatim, per le unità 08 e 10

Sono sei stringhe. Le prime cinque sono nuove, la sesta era già giusta e serve da metro.

```csharp
// Pages/CollectionEdit.razor:426 — fallimento in lettura (OnParametersSetAsync)
"Non è stato possibile aprire la collezione: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento."

// Pages/CollectionEdit.razor:611 — fallimento in scrittura (Salva)
"Non è stato possibile salvare: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Quello che hai scritto è ancora qui: riprova fra un momento, e non chiudere la pagina."

// Pages/CollectionEdit.razor:712 — fallimento in scrittura (Sovrascrivi)
"Non è stato possibile sovrascrivere: il database ha rifiutato la scrittura, oppure non è stato raggiunto. Quello che hai scritto è ancora qui: riprova fra un momento, e non chiudere la pagina."

// Pages/CollectionEdit.razor:748 — fallimento in cancellazione (Elimina, ramo catch)
"Non è stato possibile eliminare: il database ha rifiutato la cancellazione, oppure non è stato raggiunto. La collezione è ancora al suo posto: riprova fra un momento."

// Pages/CollectionEdit.razor:743 — RIFIUTO senza eccezione (Elimina, EliminaAsync ha reso false)
"Non è stato possibile eliminare la collezione: può cancellarla solo chi l'ha creata, o chi possiede lo spazio. Può anche darsi che qualcun altro l'abbia già eliminata: ricarica la pagina per vedere com'è adesso."

// Pages/CollectionEdit.razor:667 — PREESISTENTE e già conforme: è il metro, non l'ho toccato
"Non hai più il permesso di modificare questa collezione: può essere cambiato qualcosa nello spazio. Ricarica la pagina."
```

E la riga di diagnosi che le accompagna, una per ciascun `catch`
(`:425`, `:610`, `:711`, `:747`):

```csharp
Console.Error.WriteLine($"[Collezione] Apertura non riuscita: {ex.Message}");
Console.Error.WriteLine($"[Collezione] Salvataggio non riuscito: {ex.Message}");
Console.Error.WriteLine($"[Collezione] Sovrascrittura non riuscita: {ex.Message}");
Console.Error.WriteLine($"[Collezione] Eliminazione non riuscita: {ex.Message}");
```

### Il criterio — cinque regole, in ordine di importanza

Non copiate le frasi: copiate queste. Le frasi sono l'applicazione, e sui vostri file
l'applicazione cambia.

**1. Il testo dell'eccezione non arriva mai all'utente, ma non si butta.**
`PostgrestException.Message` **è** il corpo JSON della risposta: sul 42501 conteneva
SQLSTATE, nome della tabella e l'istruzione `GRANT INSERT ON public.collections TO
authenticated;`, cioè un ordine rivolto a un amministratore di database mostrato a chi
voleva salvare una collezione. Va in console con l'idioma che il progetto già usa
(`CollectionDetail.razor:308,322`, `Shared/GrafoSpazio.razor`, `Shared/RecensioniElemento.razor`):
marcatore fra parentesi quadre, azione al passato, due punti, `{ex.Message}`. Sostituire
l'interpolazione con una frase fissa **senza** la riga in console sarebbe barattare
un'indiscrezione con una cecità.

**2. La frase ha tre parti, sempre nello stesso ordine: fatto, causa, azione.**
È la forma di `NoteEdit.razor:291`, il cui commento la enuncia esplicitamente — «si dice il
fatto, si suggerisce la causa senza giurarci, e si indica l'azione». La causa si **suggerisce**:
dentro un `catch (Exception)` non si sa se il database abbia rifiutato o se non sia stato
raggiunto, e le due possibilità si nominano entrambe («…ha rifiutato la scrittura, oppure non
è stato raggiunto») invece di sceglierne una a caso. Un messaggio che afferma una causa che
non conosce è peggio di uno che ne ammette due.

**3. Dopo un fallimento in scrittura, dire se il lavoro è perso è parte del messaggio.**
È la domanda che si fa chi ha appena visto fallire un salvataggio, e nessuna delle vecchie
stringhe le rispondeva. Da qui «Quello che hai scritto è ancora qui» e «La collezione è
ancora al suo posto».

**4. L'azione suggerita non deve distruggere ciò che il messaggio ha appena promesso di
aver salvato.** Su un salvataggio fallito l'azione è **riprovare**, mai ricaricare: una
ricarica butterebbe via esattamente ciò che non è stato scritto. «Ricarica la pagina» è
giusta solo dove non c'è lavoro in sospeso — sul rifiuto di un'eliminazione, o sul
permesso perso di `:667`. Controllate questa, è la più facile da sbagliare.

**5. Quando il messaggio enuncia una regola di permessi, si usano le parole di
`Services/Permessi.cs:86-96`, non una riformulazione.** Quel `switch` è la formulazione
canonica per tutti e quattro gli oggetti (Spesa, Nota, Collezione, Elemento), e sulla stessa
pagina è già a schermo quando `!PuoIntervenire`. Il messaggio di `:743` prende da lì «può
cancellarla solo chi l'ha creata, o chi possiede lo spazio». **Non** si riusa
`Permessi.Spiegazione(...)` per interpolazione, perché apre affermando che l'oggetto *è di
qualcun altro*, e in un rifiuto di cancellazione quella è una delle due ipotesi, non un
fatto — ma le parole sì. Due modi di dire la stessa regola a due centimetri di distanza
sembrano due regole diverse.

### Come applicarlo alle vostre sei stringhe «Il database ha rifiutato…»

Sono di due specie diverse e vanno trattate diversamente. **Non sono nel mio perimetro e
non le ho toccate**: qui c'è solo lo stampo.

| Dove | Stringa attuale | Specie |
|---|---|---|
| `Pages/SpaceDetail.razor:189` | «Il database ha rifiutato la modifica: solo il proprietario può rinominare.» | rifiuto senza eccezione |
| `Pages/SpaceDetail.razor:217` | «Il database ha rifiutato la rimozione.» | rifiuto senza eccezione |
| `Pages/SpaceDetail.razor:243` | «Il database ha rifiutato l'uscita.» | rifiuto senza eccezione |
| `Pages/SpaceDetail.razor:271` | «Il database ha rifiutato l'eliminazione.» | rifiuto senza eccezione |
| `Shared/RecensioniElemento.razor:434` | «Il database ha rifiutato la modifica.» | rifiuto senza eccezione |
| `Shared/RecensioniElemento.razor:547` | «Il database ha rifiutato l'eliminazione.» | rifiuto senza eccezione |

Tutte e sei sono della specie di `:743`, non di quella dei `catch`: nascono da un metodo di
repository che ha reso `false`, non da un'eccezione. Quindi:

- **«Il database» sparisce dal soggetto.** All'utente non interessa quale componente ha
  detto di no; gli interessa perché e cosa fare. È il difetto centrale di tutte e sei:
  raccontano il meccanismo.
- **Si nominano tutte le cause che rendono `false` quel particolare metodo**, non solo la
  prima che viene in mente. Apritelo e contatele: nel mio caso `EliminaAsync` ne aveva due
  (nessun diritto / non più visibile) e il commento lungo del repository le enumerava già.
- **La regola di permessi si cita da `Permessi.cs`** quando l'oggetto è uno dei quattro.
  Per lo spazio (`SpaceDetail`) non c'è una `Spiegazione`: lì la formulazione la fissa
  l'unità 08, e da quel momento è canonica a sua volta.
- `:189` è già a metà strada — dice la causa — ma la premette con «Il database ha
  rifiutato», e non dice cosa fare.

**Nei quattro `catch` con `{ex.Message}` che vivono negli stessi due file** (`SpaceDetail.razor:176,206,232,260,286`
e `RecensioniElemento.razor:236,255,396,448,553`) vale invece lo stampo dei `catch`, riga di
console compresa, con marcatore `[Spazi]` e `[Recensioni]` — che quei file già usano per le
diagnosi non fatali.

### Quello che questa unità NON ha creato, e perché conta saperlo

**Nessun helper condiviso, nessun tipo nuovo, nessun metodo nuovo.** Il mandato chiedeva di
tornare `BLOCKED` se fosse servito un helper in `Shared/`. **Non è servito**, e la ragione
merita una riga perché a occhio sembrerebbe di sì: `{ex.Message}` finisce sotto gli occhi
dell'utente in **circa trenta punti su dodici file**, quindi tre file lo vorrebbero
eccome — ma un helper condiviso servirebbe solo se la traduzione dovesse **ispezionare**
l'eccezione, cioè mappare SQLSTATE su frasi. Non lo fa e non deve farlo: la regola 2 dice
che dentro un `catch (Exception)` la causa si suggerisce senza giurarci, e per farlo basta
una costante scritta nel punto in cui si sa quale operazione è fallita. Un helper
`Messaggio(ex)` saprebbe meno di chi lo chiama, non di più.

Se qualcuno più avanti volesse comunque una funzione condivisa, l'unica che avrebbe senso è
quella della **riga di console**, non del messaggio — ed è già un idioma di tre parole che
non vale un'astrazione.

---

## ADJUDICA

Revisori lanciati: `bug-hunter`, `conformity`, `threat-hunter`, tutti nello stesso
messaggio, sull'unica unità.

**`backend-expert` non lanciato, ed è una scelta dichiarata.** Il gate lo vuole su superficie
nuova (tipo, servizio, endpoint, astrazione), su diff > ~120 righe, o su richiesta esplicita.
Al momento della decisione il diff era di 111 righe (+94/−17) e non nasceva nessun tipo,
nessun servizio, nessuna astrazione: un `static readonly string[]` è un dato, e i due
blocchi di markup sono applicazioni di classi esistenti. Il diff è poi cresciuto a +141/−25
per le correzioni dell'adjudica, ma **la crescita è quasi tutta commento** — le righe di
codice aggiunte nel secondo giro sono sei assegnazioni `erroriValidazione = []`, un
`Cambiata &&` e un cambio di tag. Rilanciarlo su quello avrebbe prodotto rilievi di design
su codice che il budget vietava esplicitamente di ridisegnare.

    istruttoria: 2 rilievi su 1 file → checker no

Sotto entrambe le soglie (somma ≥ 4, oppure ≥ 3 file distinti). La somma è
`bug-hunter` 1 + `conformity` 1. `threat-hunter` ha reso **0 rilievi** e per regola non
entrerebbe comunque nella somma.

### 1. `bug-hunter`, severità media, logica — FONDATO, corretto

**Claim**: `erroriValidazione` non viene mai svuotato se non da un nuovo clic su «Salva» o
da un cambio di rotta, quindi può restare acceso insieme a messaggi che lo contraddicono.

**Verificato da me aprendo il codice**, perché è un difetto che il mio stesso diff rende
visibile. Prima delle correzioni il campo era assegnato in due soli punti — `:363`
(`OnParametersSetAsync`) e `:592` (`Salva`) — e in nessun altro. Lo scenario regge:
`Aggiungi campo` rende `Cambiata` vera, `Salva` popola `erroriValidazione` e ritorna prima
di scrivere, `Togli` → `Rimuovi()` chiama `Rinumera()` e riporta `campi` a coincidere con
`collezione.Fields`, quindi `Cambiata` torna falsa mentre il riquadro rosso resta acceso —
accanto alla riga nuova che dice «non hai ancora cambiato niente». Le due citazioni del
revisore corrispondono al file.

**La parte che è colpa mia va detta**: il difetto era **latente prima del mio diff**.
Spostando il riquadro accanto ai pulsanti — che è l'obiettivo 4 — l'ho messo a contatto con
un `<p>` che può dire il contrario e con un secondo riquadro rosso. È il costo tipico di un
consolidamento: due messaggi che non si vedevano mai insieme cominciano a vedersi.

**Corretto in due modi diversi, perché i casi sono due.**

*Il caso riportato* non passa da nessun metodo intercettabile a costo ragionevole: per
azzerare il campo servirebbe agganciare `Rimuovi`, `AggiungiCampo`, `Sposta`, `AlCambioTipo`,
`AggiungiOpzione`, `TogliOpzione` e tutti i `@bind` dei campi di testo. Risolto invece nel
markup, `Pages/CollectionEdit.razor:217`:

    @if (Cambiata && erroriValidazione.Count > 0)

Non è prudenza: è un'affermazione vera. Il riquadro dice **perché l'ultimo salvataggio è
stato rifiutato**, e quel perché vale finché c'è ancora qualcosa da salvare.

*Gli altri casi* — un'azione nuova che si affianca al verdetto della precedente — si
chiudono azzerando il campo dove già si azzera `errore`, cioè in cima all'azione. Quattro
punti (`:577` `Salva`, `:698` `Sovrascrivi`, `:723` `Elimina`, e `:687` `Ricarica`) più
`:489` `ApplicaModello`. Gli ultimi due sono i due punti in cui `campi` viene **sostituito in
blocco**: lì il commento preesistente di `Ricarica` («una conferma rimasta aperta si
riferirebbe a un campo di un elenco che non esiste più») è già l'argomento esatto, e non
valeva la pena farlo valere solo per metà. `ApplicaModello` non sarebbe coperto dal
`Cambiata &&`, perché un modello porta con sé un nome e lascia `Cambiata` vera.

L'azzeramento in cima a `Salva` (`:577`) chiude un terzo caso che il revisore non ha
nominato: se `SchemaCampi.Valida` lanciasse, il `catch` scriverebbe in `errore` lasciando
accesi gli errori del tentativo precedente.

### 2. `conformity`, severità media, duplicazione — FONDATO, corretto con un rimedio diverso dal suo

**Claim**: le ventiquattro pastiglie stavano in un `<div class="azioni">` generico, mentre
per lo stesso identico pattern — pastiglie con `aria-pressed` che valgono una scelta sola —
il progetto usa già due volte `<fieldset><legend>`; il gruppo restava senza nome accessibile.

**Verificata la prova da me, ed è esatta**: `Pages/SpesaEdit.razor:105-106` e
`Pages/Spese.razor:93-94` portano entrambi
`<fieldset class="scelta-categoria"><legend class="etichetta-campo">`. Due occorrenze, stesso
pattern. Il rilievo è fondato nella sostanza: ventiquattro bottoni senza legenda, chi ascolta
la pagina li incontra uno per uno senza sapere di che cosa siano l'elenco.

**Il suo rimedio l'ho scartato, e ho corretto altrimenti.** `conformity` proponeva un
`<fieldset>` con bordo e padding azzerati **con stile inline**, per non riusare una classe
dal nome scomodo. Ma `app.css:1885-1896` contiene già esattamente quella regola — bordo
azzerato, legenda a capo da sola via `flex-basis: 100%`, pastiglie che vanno a capo — e
riscriverla a mano sarebbe stata una duplicazione, cioè il tipo di difetto che il rilievo
stesso denuncia. Ho riusato la classe:

```razor
Pages/CollectionEdit.razor:96    <fieldset class="scelta-categoria">
Pages/CollectionEdit.razor:97        <legend class="etichetta-campo">Icona</legend>
Pages/CollectionEdit.razor:99        <input class="icona-input" … aria-label="Icona" … />
```

**Il campo di testo è entrato dentro il fieldset** e il suo `<label>` è sparito: la legenda
dice già «Icona», e ripeterlo a mezzo centimetro sarebbe stato rumore — con il `<label>`
rimasto sopra, la correzione avrebbe risolto l'accessibilità creando una duplicazione
visibile. Ci ha guadagnato anche il disegno: `.icona-input` dichiara `flex: none` e larghezza
fissa (`app.css:1370`), due proprietà che dentro un `<label>` non facevano niente e che ora
tengono il campo stretto **accanto** alle pastiglie — che è il posto in cui l'utente aveva
chiesto di metterlo.

**Campione sugli infondati: non ce n'è nessuno.** Nessun rilievo è stato respinto in questa
unità: erano due, entrambi fondati, entrambi corretti. Il controllo a campione del §5 non ha
materia, e lo dichiaro invece di ometterlo.

### Verifica indipendente su uno «0 rilievi»

`threat-hunter` ha reso **0 rilievi** e il suo verdetto portante è che la divulgazione del
42501 sia **chiusa e non spostata**: `Console.Error.WriteLine` scrive nella console del
browser dell'utente stesso, che quel testo lo vedeva già. Ho riaperto io
`wwwroot/index.html`: nessun `console.`, nessun override, nessuna telemetria, nessun invio
remoto. Il verdetto regge.

Il suo secondo verdetto — le stringhe di `erroriValidazione` interpolano etichette e chiavi
scritte dall'utente, ma `<li>@e</li>` è un'espressione di contenuto Razor su una `string` e
viene sempre codificata — l'ho verificato sul markup che ho scritto io: nessun
`MarkupString`, nessun attributo. Lo spostamento non cambia niente sul piano della sicurezza.

**Nota sulle citazioni, per le unità che seguono.** Contrariamente a quanto è successo
all'unità 04, **questa volta i `file:line` di `threat-hunter` tornavano tutti** (dava `:406`,
`:198-208`, `:291-296` e corrispondevano). Anche quelli di `bug-hunter` e `conformity`
tornavano. L'avvertenza del piano resta prudente, ma il campione di questa unità non la
conferma: conviene continuare a riaprire, non a diffidare per principio.

---

## FUORI SCOPE

Rilievi fondati che **non ho risolto**, con l'indicazione di chi possiede il rimedio.

**1. All'unità 11 — il bersaglio da toccare delle pastiglie è troppo piccolo, e non solo qui.**
`.pastiglia` dichiara `font-size: var(--t-xs)` e `padding: var(--s1) var(--s3)`
(`app.css:942-954`), cioè 11px di testo e 4px di padding verticale: alta circa 21px. Con lo
`style="font-size: var(--t-lg)"` che ho messo inline sale a circa 32px, ma il progetto
dichiara `--tocco: 48px` (`app.css:190`) e lo applica già a `.barra-elenco .pastiglia`
(`app.css:1484`) proprio perché quelle si premono. **Non l'ho corretto e non era un errore
farlo**: lo stesso difetto vale per `SpesaEdit.razor:109` e `Spese.razor:97`, e una riga in
`app.css` lo chiude in tutti e tre i posti mentre uno `style` inline lo chiuderebbe solo nel
mio, lasciando due usi della stessa pastiglia di due misure diverse. La riga è
`min-height: var(--tocco)` su `.scelta-categoria .pastiglia`.

**2. All'unità 11 — la metà visiva dell'obiettivo 2 non era mia.** Il mandato osservava che
«Salva» spento è reso con `opacity: .5` su fondo blu pieno e su nero resta saturo, quindi
legge come premibile. Quello è `app.css:704-709`, che è la **stessa regola** già assegnata
all'unità 11 per il selettore `a.btn:not([href])` di «Chiudi». Io ho fatto la metà che si
poteva fare senza CSS: **dire cosa manca** (`Pages/CollectionEdit.razor:244-252`). Finché
l'altra metà non arriva, il pulsante dice il perché ma non sembra spento.

**3. All'unità 11 — `.scelta-categoria` ha ora due domini e un nome solo.** La classe è usata
da `Spese.razor`, da `SpesaEdit.razor` e adesso da `CollectionEdit.razor` per le icone, ma il
nome parla di categorie di spesa. La regola che porta è del tutto generica. Un nome come
`.scelta-pastiglie` costerebbe tre sostituzioni e toglierebbe di mezzo un'incoerenza che
altrimenti si consolida. **Non è urgente**: è cosmesi di nomenclatura, e chi possiede
`app.css` decida.

**4. Al capo — `handoff/PIANO.md` si contraddice sui proprietari di due file.** La tabella
della `PARTIZIONE` assegna `SpaceDetail.razor` all'unità **08** e `RecensioniElemento.razor`
alla **10**, e il mio mandato dice lo stesso; ma la sezione `RAZIONALE`, al paragrafo «File
contesi, e a chi vanno», scrive «`SpaceDetail.razor` all'unità **07**, `RecensioniElemento.razor`
alla **09**». Ho seguito la tabella e il mandato. Va sanato prima che due unità si contendano
un file credendo entrambe di possederlo — è esattamente il caso che la proprietà esclusiva
esiste per impedire.

**5. Preesistente, segnalato e non toccato — `@using Eton.Services` a `Pages/CollectionEdit.razor:4`
è ridondante**, perché `_Imports.razor:9` lo importa globalmente. Già sollevato dall'unità 04
e ancora lì. Non è del mio perimetro tematico e cancellarlo di nascosto sarebbe stato
peggio che lasciarlo.

---

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**. Ho usato `-warnaserror` come chiedeva
  il mandato: più severo del minimo, e su un file con `@inherits` e `override` intercetta il
  CS0108 che una build permissiva declasserebbe.
- `dotnet test --no-build` → **267/267 superati**, 0 non superati, 0 ignorati (111 ms).

Compilato **una volta sola, a fine giro, con nessun agente vivo**. Gli implementer non hanno
compilato e i revisori avevano il divieto scritto nel brief.

**Nessun test nuovo, e la scelta va dichiarata.** `Eton.Tests` non ha bUnit: sono quindici
file di test su logica pura (`SchemaCampiTests`, `PermessiTests`, `CalcoliVotiTests`…), e
non esiste nel progetto un solo test che renda un componente. Tutto ciò che questa unità ha
prodotto vive dentro un `.razor`: markup, cinque costanti di testo e sei assegnazioni.
**Una cosa sarebbe stata testabile** — l'invariante «le prime tre emoji della tavolozza sono
quelle dei tre modelli di `SchemaCampi.Modelli`» — ma `TavolozzaIcone` è `private` dentro una
pagina, e allargarne la visibilità per poterlo asserire avrebbe cambiato il codice per fare
spazio al test. L'invariante è fissata da un commento a `Pages/CollectionEdit.razor:303-309`
che nomina il file e le righe di origine. Lo dichiaro perché è un buco reale, non perché sia
stato risolto.

---

## SCOSTAMENTI

**1. `Services/CollectionRepository.cs` non è stato toccato, benché fosse nel perimetro.**
Il posto naturale per tradurre un errore di database sembrerebbe il repository — è lui che
parla con PostgREST. Non lo è: dentro `CreaAsync` o `SalvaAsync` si sa *quale query* è
fallita ma non *quale schermata* la stava aspettando, e le tre parti della regola 2 (fatto,
causa, azione) hanno bisogno della seconda informazione, non della prima. «Quello che hai
scritto è ancora qui» è una frase che solo la pagina può dire. Il repository resta com'è, e
il perimetro concesso è stato usato per metà di proposito.

**2. Ho cambiato una stringa che era già in italiano** — `Pages/CollectionEdit.razor:743`,
il rifiuto di `Elimina`. Non rientrava nell'obiettivo 1 alla lettera, che parlava di JSON
mostrato all'utente. L'ho fatto perché è **l'unico esemplare della famiglia «Il database ha
rifiutato…» che vive in un file di mia proprietà**: correggendolo, il contratto per le unità
08 e 10 diventa un pezzo di codice che possono aprire invece di una descrizione in prosa che
devono interpretare. Costo: una riga di stringa e sette di commento, nessun cambio di
comportamento.

**3. Quattro righe `Console.Error.WriteLine` non erano chieste dal mandato.** Sono la
contropartita della regola 1: senza, la traduzione avrebbe barattato un'indiscrezione con una
cecità, e il primo a pagarla sarebbe stato chi deve diagnosticare il prossimo guasto. L'idioma
è preso da `CollectionDetail.razor:308,322`, marcatore compreso.

**Nient'altro diverge.** In particolare **non** divergono, e non sono state toccate: la
posizione di `<NavigationLock>` dentro il ramo del modulo, la firma di `Cambiata`, i due
`Esci(...)`, il gate di «Chiudi», `<TestataPagina>` e `<PageTitle>`, il pannello `<Aiuto>`,
la posizione del blocco `errore`/`avviso` rispetto ai pulsanti. Nessun componente nuovo,
nessun servizio, nessun tipo, nessun metodo, nessun pacchetto, nessun file nuovo, **nessuna
riga di `app.css`**, nessun `.js`. L'unico stile inline è `font-size: var(--t-lg)` sulle
pastiglie, dichiarato e motivato nel commento a `:88-94`.

**Il server di sviluppo non è stato avviato e nessuna prova è stata fatta nel browser**, come
imponeva il mandato. Nessun processo lasciato vivo, nessuna porta occupata.

---

## COME HO INNESCATO L'ERRORE POSTGREST: NON L'HO FATTO

Il mandato chiedeva di dichiararlo, e la risposta onesta è **non ci sono riuscito, e non ci
ho provato**, perché la stessa pagina del mandato mi vietava di avviare il server e di aprire
il browser. Le due istruzioni non sono conciliabili e ho obbedito alla più specifica.

**Che cosa è dimostrato senza browser**, e non è poco: che nessuno dei quattro `catch`
interpola più `ex.Message` in un messaggio a schermo. È una proprietà statica del file, l'ho
verificata io con un `grep` su `errore = $"` che rende **zero** occorrenze, e le uniche
`ex.Message` rimaste stanno dentro i quattro `Console.Error.WriteLine`. Il 42501 non può più
comparire a schermo perché non c'è più un percorso che ce lo porti.

**Che cosa resta non dimostrato**, e va provato al primo giro di `live-testing`: che le frasi
nuove **leggano bene dove compaiono davvero** — lunghezza sul telefono, posizione rispetto ai
pulsanti, e soprattutto che la riga in console porti davvero il JSON invece di una stringa
vuota. Un `catch (Exception)` che riceve un'eccezione senza `Message` utile produrrebbe una
diagnosi muta, e questo si vede solo eseguendo.

**Come innescarlo, per chi ci proverà — e perché quasi certamente non ci riuscirà.**
Ho aperto la migrazione per rispondere invece di supporre. `supabase/migrations/20260812120000_collections.sql`
dichiara quattro vincoli sulla tabella, e **ognuno ha davanti una guardia lato client che lo
rende irraggiungibile dall'interfaccia**:

| Vincolo SQL | Guardia che lo precede |
|---|---|
| `:26` `name … check (length(btrim(name)) between 1 and 100)` | `maxlength="100"` sul campo, più il controllo su nome vuoto in `Salva` |
| `:27` `icon text check (icon is null or length(icon) <= 16)` | `maxlength="16"` sul campo |
| `:29` `rating_max … check (rating_max in (5, 10))` | il `<select>` offre solo 5 e 10 |
| `:41` `collections_fields_shape` | `SchemaCampi.Valida` più il tetto `campi.Count >= MassimoCampi` sul pulsante |

e la policy di INSERT (`:169`, `is_space_member(space_id) and owner_id = auth.uid()`) è
soddisfatta per costruzione da `Crea()`, che prende lo spazio attivo e l'utente corrente.

Sull'icona in particolare non serve nemmeno provarci: `maxlength` conta **unità UTF-16**
mentre `length()` di Postgres conta **caratteri**, e un'emoji è due unità ma un carattere —
quindi il limite del browser è più severo di quello del database, non più permissivo.

Resta perciò una via sola praticabile senza toccare il database vero: **disattivare la rete**
dagli strumenti per sviluppatori. Prova il `catch` e la riga in console, ma **non** prova la
forma di `PostgrestException`, perché la chiamata muore prima di raggiungere PostgREST.

**Questo non è un buco del collaudo: è il motivo per cui il 42501 era un difetto.** Le
guardie lato client coprono tutto, quindi un'eccezione PostgREST su questa pagina significa
per definizione che *il database ha cambiato idea rispetto a ciò che il client credeva* — un
privilegio revocato, una policy modificata, una migrazione a metà. Esattamente il caso del
42501. Che sia difficile da innescare a comando è la prova che le guardie funzionano, e la
ragione per cui il messaggio deve essere comprensibile quando finalmente compare: nessuno lo
vedrà mai in prova, tutti lo vedranno il giorno in cui qualcosa va storto in produzione.

---

## DA PROVARE NEL BROWSER

Da aggiungere alle prove già scritte nei resoconti delle unità 03 e 04, che restano valide.
Ordinate per quanto costa scoprirle tardi.

**1. La tavolozza, e il caso che l'implementazione ingenua sbaglia.**
Su `/collections/new`: premere l'emoji 🍺 nella tavolozza.
*Accettazione*: la pastiglia si accende (bordo e sfondo cambiano, non solo il cursore), il
campo di testo accanto mostra 🍺, e **nessun'altra** pastiglia è accesa. Poi premere il
modello «Birre» nella scheda «Parti da un modello»: il campo diventa 🍺 e la pastiglia 🍺
**si accende da sola**, perché è lo stesso carattere. Se si accendessero due pastiglie, o se
il modello lasciasse spenta la sua, il legame fra tavolozza e modelli è rotto.
Poi scrivere a mano un'emoji che non è in elenco (per esempio 🐙 con `Win + .`):
*accettazione*, **nessuna** pastiglia accesa e il campo la conserva — è la via d'uscita, e
deve restare aperta.

**2. Il bersaglio da toccare, che è il limite noto di questa unità.**
Sul telefono, o restringendo la finestra: provare a premere le pastiglie col dito.
*Accettazione*: **è atteso che siano piccole** (circa 32px invece dei 48 che il progetto si
dà come minimo). Non è un difetto da segnalare come nuovo: è la voce 1 di `FUORI SCOPE`, e
appartiene all'unità 11. Quello che serve misurare è **quanti tentativi a vuoto** costa
premerne una: è quel numero, non l'opinione, a dire se la riga di CSS è urgente o cosmetica.

**3. Il gruppo ha un nome, e il campo non lo ripete.**
`/collections/new`, ispezionare la zona dell'icona.
*Accettazione*: c'è una sola scritta «ICONA», sopra la fila; il campo di testo sta **sulla
stessa riga** delle pastiglie, stretto, non a tutta larghezza. Se «Icona» compare due volte,
o se il campo si allarga a tutta riga, il `<fieldset>` non sta applicando `.scelta-categoria`.

**4. L'esito sta in un posto solo — la prova che chiude il rilievo dell'unità 04.**
Aggiungere una decina di campi, così il modulo scorre per più schermate. Poi, in sequenza:
(a) svuotare il Nome e premere «Salva»; (b) rimettere il nome, lasciare un campo senza
etichetta, premere «Salva»; (c) sistemare tutto e premere «Salva».
*Accettazione*: in tutti e tre i casi il messaggio compare **appena sopra i pulsanti**, in
vista senza scorrere, e **mai** dentro la scheda «Campi». È la voce 7 delle prove dell'unità
04, che chiedeva di misurare quanto si deve scorrere: la risposta attesa ora è **zero**.

**5. L'errore di validazione non sopravvive alla propria causa — il rilievo di `bug-hunter`.**
Su una collezione **esistente**: premere «Aggiungi campo» senza scrivere l'etichetta, premere
«Salva» (compare «Il campo … non ha un'etichetta»), poi premere «Togli» → «Sì, togli» su quel
campo appena aggiunto.
*Accettazione*: il riquadro rosso **sparisce** nello stesso istante, e al suo posto compare
«Non c'è niente da salvare: non hai ancora cambiato niente.» I due messaggi **non devono mai
vedersi insieme**: era la contraddizione corretta in adjudica, ed è la prova che la vale.
*Variante*: ripetere ma invece di togliere il campo, scrivergli un'etichetta. Il riquadro
rosso **resta** finché non si ripreme «Salva» — è voluto, non è un difetto: il verdetto è
dell'ultimo tentativo, non del testo che si sta digitando.

**6. Il pulsante spento dice cosa manca.**
Aprire `/collections/new` senza scrivere niente.
*Accettazione*: sopra i pulsanti si legge «Scrivi il nome della collezione per poterla
salvare.», in grigio tenue e **non in rosso**. Scrivendo il nome la riga sparisce e «Salva»
si accende. Poi su una collezione **esistente** appena aperta: la riga dice «Non c'è niente
da salvare: non hai ancora cambiato niente.» Cambiare un carattere qualsiasi: sparisce.
*Nota attesa e non un difetto*: il pulsante resta **blu e saturo** anche da spento. È la
voce 2 di `FUORI SCOPE`, dell'unità 11.
*Da verificare che non accada mai*: la riga **non** deve comparire mentre il pulsante dice
«Salvo…», né quando c'è la scheda del conflitto aperta, né quando in cima si legge già
«Questa collezione è di qualcun altro…». In quei tre casi il perché è già scritto altrove, e
ripeterlo sarebbe rumore.

**7. Il messaggio d'errore, e la console che deve contenere il JSON.**
Aprire gli strumenti per sviluppatori sulla scheda Console, disattivare la rete, premere
«Salva».
*Accettazione*: a schermo compare **soltanto** «Non è stato possibile salvare: il database ha
rifiutato la scrittura, oppure non è stato raggiunto. Quello che hai scritto è ancora qui:
riprova fra un momento, e non chiudere la pagina.» — nessuna graffa, nessun `code`, nessun
`GRANT`. In console compare `[Collezione] Salvataggio non riuscito: …` **con dentro qualcosa
di leggibile**: se la parte dopo i due punti è vuota, la diagnosi è muta e va riportata.
Riattivare la rete e ripremere «Salva»: **deve salvare**, perché il testo era rimasto nel
modulo. È l'affermazione che il messaggio fa, e va verificata invece che creduta.

**8. Il primo collaudo della migrazione dell'unità 02** — ancora non fatto, e questa unità
non ha potuto farlo. Creare una collezione da `/collections/new` e verificare che il **42501
non compaia più**. Finché non è visto, resta un fatto riferito. Vale la pena farlo **per
primo**, perché se fallisse renderebbe inutile tutto il resto di questa lista.
