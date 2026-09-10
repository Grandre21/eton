UNITÀ: 08 — ESITO: FATTO

TOCCATI:
- `Pages/Home.razor` → +27/−5 (rilievo 7 e rilievo 11)
- `Pages/SpaceDetail.razor` → +33/−3 (rilievo 12, più la correzione del rilievo di `threat-hunter`)
- `Pages/Profile.razor` → +13/−1 (rilievo 12)

Nessun altro file toccato: `git status --porcelain` mostra solo questi tre. `wwwroot/css/app.css`
non è stato aperto in scrittura e non ho voci da accodare all'unità 11 — le classi esistenti
(`.cime`, `.testo-tenue`, `.testata`) bastavano tutte.

## CONTRATTI

**Il predicato del rilievo 7** — `Pages/Home.razor:233`:

    @if (!Spazi.Attivo.IsPersonal)

È il negato **letterale** di quello già presente nella stessa pagina a `Pages/Home.razor:75`
(`@if (Spazi.Attivo.IsPersonal)`, il ramo «Il tuo spazio personale…» del sottotitolo), riaperto da
me sul disco dopo l'edit. Nessun campo nuovo sul modello, nessun secondo predicato.

`conformity` colloca quel gemello a `:78`. È sbagliato: è a **`:75`**, verificato con
`sed -n '74,76p'` a modifiche applicate — l'edit del rilievo 11 ha inserito righe a partire da
`:115`, quindi non ha spostato nulla sopra. Riporto il numero giusto perché il prossimo che lo cita
non erediti l'errore.

**La forma della testata** — identica su entrambe le pagine, copiata da `Pages/SpesaEdit.razor:11-17`
come imponeva il mandato:

    Pages/SpaceDetail.razor:28    <TestataPagina Titolo="@TitoloSchermata">
    Pages/Profile.razor:12        <TestataPagina Titolo="Profilo">

Sta **fuori** dalla catena `@if`, come in `SpesaEdit`: titolo e «?» restano a schermo anche durante
il caricamento e nei rami d'errore. Su `SpaceDetail` questo ha imposto un titolo di ripiego, che è
la sola aggiunta a `@code` di tutta l'unità — `Pages/SpaceDetail.razor:163`:

    private string TitoloSchermata => spazio?.Name ?? "Spazio";

Il ripiego non è inventato: è **testualmente** quello che il `<PageTitle>` della stessa pagina già
faceva in linea prima di questa unità (`@(spazio?.Name ?? "Spazio")`), estratto in proprietà perché
ora serve in due punti. `<PageTitle>` ora legge `@TitoloSchermata`, cioè esattamente
`SpesaEdit.razor:9`. L'`<h1>@spazio.Name</h1>` preesistente è stato tolto: `TestataPagina` rende
già l'`<h1>` (`Shared/TestataPagina.razor:35`), e tenerlo avrebbe prodotto due `<h1>` sulla pagina.

Nessun `@using` aggiunto: `Eton.Shared` è importato globalmente in `_Imports.razor:10`, ed è il
motivo per cui nemmeno `SpesaEdit` ne ha uno. L'`implementer` aveva sollevato il dubbio in nota; è
infondato, verificato prima della build.

## ADJUDICA

Revisori lanciati: `bug-hunter`, `conformity`, `threat-hunter`, tutti e tre nello stesso messaggio
sul diff dell'unità. `backend-expert` **non** lanciato: nessuna superficie nuova (l'unica aggiunta a
`@code` è una proprietà calcolata privata a due call-site nello stesso file) e diff sotto le ~120
righe — il gate del §3 non lo richiedeva. `threat-hunter` sì, e non per il markup: i due pannelli
`<Aiuto>` **affermano semantica di autorizzazione e di sessione all'utente**, e un pannello che
dichiara il falso su chi legge o cancella i dati induce a condividere uno spazio credendo a una
regola che il sistema non applica. La regola dice «se esiti, lancialo»: esitavo.

    istruttoria: 0 rilievi su 0 file → checker no

Somma dei soli `bug-hunter` e `conformity`, come prescrive il §4: 0 + 0. Sotto entrambe le soglie
(≥ 4 rilievi, oppure ≥ 3 file distinti), quindi nessun `checker`.

**Rilievo 7 — FONDATO, corretto.** Il link prometteva una gestione inesistente. Nascosto, non
disabilitato, come deciso dall'utente il 3 settembre.
Riga: `Pages/Home.razor:233` — `@if (!Spazi.Attivo.IsPersonal)`.

**Rilievo 11 — FONDATO, corretto.** «Spesa» è `CategorieSpesa.Elenco[0]`
(`Services/CategorieSpesa.cs:18`, riaperto da me), e con una sola categoria nel mese la riga si
leggeva come un totale.
Riga: `Pages/Home.razor:124` — `<span class="cime">Per categoria: @cime</span>`.

**Rilievo 12 — FONDATO, corretto** su entrambe le pagine. Righe: `SpaceDetail.razor:28` e
`Profile.razor:12`.

**`threat-hunter`, 1 rilievo (authz, severità bassa) — FONDATO come incompletezza, corretto.**
Diceva che il paragrafo sulla moderazione enuncia un potere del proprietario che sulle recensioni
non esiste. **Aperto da me**, come impone il §5 per ogni rilievo che tocca autorizzazioni:
`supabase/migrations/20260812200000_recensioni.sql:127-134` ha `reviews_update` e `reviews_delete`
con la sola condizione `user_id = auth.uid()`, senza il ramo `is_space_owner` che invece note,
collezioni, elementi e spese hanno tutti; e il commento a `:118-122` dichiara la divergenza
deliberata — «un voto è un'opinione personale e riscriverla sarebbe falsificarla, non moderare».
Il rilievo **non** provava che la frase fosse falsa (era già circoscritta a «note, collezioni e
spese», e `threat-hunter` lo concede), ma un aiuto che enuncia una regola tacendo la sua unica
eccezione lascia un proprietario a tentare una cancellazione che il database rifiuta. Aggiunta una
frase: `Pages/SpaceDetail.razor:31`.

**I `file:line` di `threat-hunter` sono esatti su questo diff.** Citava `SpaceDetail.razor:22` per il
primo `<p>` dell'aiuto e `recensioni.sql:127-134` per le policy: riaperti entrambi, corrispondono.
Come sulla 07, e a differenza della 04 e della 05. Confermo la regola del piano — si riaprono i
numeri, non si tratta l'agente come inaffidabile per principio.

**Campione sugli infondati: non ce n'è nessuno da riverificare.** Nessuno dei tre revisori ha
prodotto un rilievo che io abbia scartato: due sono tornati a zero e il terzo è stato accolto. Al
suo posto ho riverificato **l'unico punto che un revisore ha scartato di propria iniziativa**:
`bug-hunter` ha isolato che `TitoloSchermata` potrebbe restare stantio quando Blazor riusa
l'istanza di `SpaceDetail` fra due `/spaces/{id}` diversi — `spazio` non viene azzerato in testa a
`OnParametersSetAsync`, a differenza di `membri` — e ha deciso di non riportarlo perché non
costruibile. **Riaperto: ha ragione.** `Shared/SelettoreSpazio.razor:56` chiama solo
`Spazi.Imposta(id)` e mai `NavigateTo`; `Shared/Navigazione.razor:60` punta a `spaces`, non a un id;
gli unici link a `spaces/{id}` stanno in `Pages/Spaces.razor:34` e `Pages/Home.razor:236`, cioè su
altre due pagine. Nessun percorso porta da `/spaces/{A}` a `/spaces/{B}` riusando l'istanza. Resta
**fragile**: il giorno in cui qualcuno mette un selettore di spazio sulla pagina di dettaglio, il
titolo mostrerà lo spazio precedente per la durata del caricamento. Non è un difetto oggi e non l'ho
corretto.

## LA DOMANDA PIÙ RISCHIOSA, VERIFICATA DA ME

Il mandato la nomina: il predicato del rilievo 7 può valutarsi con `Spazi.Attivo` nullo, o far
lampeggiare il link prima del caricamento? **No, e la prova non sta nel predicato ma nella catena
che lo contiene.** L'ho verificata prima che `bug-hunter` rientrasse, e lui c'è arrivato per conto
suo con la stessa conclusione.

`Spazi.Attivo` è una `Space?` con setter privato (`Services/SpaceStateService.cs:34`), scritta solo
in `CaricaAsync` (`:112`), `Imposta` (`:128`) e `Dimentica`, e in tutti e tre i casi
**sincronamente**, prima che parta l'evento `Cambiato`. Il ramo `else` di `Home.razor:58` è
raggiungibile solo dopo che `:43`, `:47` e `:51` hanno escluso «caricamento in corso», «errore senza
spazio» e «nessuno spazio»; fra `:51` e `:233` non c'è nessun `await`, `BuildRenderTree` è sincrono e
il contesto di sincronizzazione di Blazor WASM è a thread singolo, quindi nessun `Dimentica()` può
infilarsi a metà passata di render. Il link non ha una classe di rischio propria: `@Spazi.Attivo.Id`
sulla stessa riga dereferenziava già lo stesso oggetto **prima** di questa unità.

**Quello che invece ho trovato, e che nessun revisore ha isolato come conseguenza operativa:** c'è
un ritardo vero nel cambio di spazio. `Home.razor:408-429` (`SpazioCambiato`) chiama
`StateHasChanged()` **dopo** `await CaricaDettagli(...)`, quindi cambiando spazio dal selettore la
Home non si ridisegna finché il giro di rete non è finito. Il link riappare con lo stesso ritardo
con cui cambiano il titolo (`:68`) e il sottotitolo (`:75`) — è **preesistente**, il diff lo eredita
e non lo peggiora, e non l'ho corretto perché sarebbe stato un intervento su un comportamento fuori
dai tre rilievi. Ma va detto a chi collauda: chi guarda lo schermo dentro quel round-trip vede il
link ancora assente e potrebbe scambiare il ritardo per un difetto. È nella prova 1 qui sotto.

## FUORI SCOPE

Rilievi fondati che non ho risolto, con il proprietario del rimedio.

1. **`Pages/SpaceDetail.razor` interpola `ex.Message` grezzo in cinque messaggi che l'utente legge**
   — `:206` (`OnParametersSetAsync`), `:236` (`Rinomina`), `:262` (`Rimuovi`), `:290` (`Esci`),
   `:316` (`Elimina`), tutti nella forma `errore = $"…: {ex.Message}"`. Numeri riaperti sul disco a
   modifiche applicate, non dedotti dal diff. (Ci sono altre quattro interpolazioni di `ex.Message`,
   a `:231`, `:257`, `:285` e `:311`, ma finiscono in `Console.Error.WriteLine` e non a schermo:
   quelle non c'entrano col rilievo.) È il **rilievo 3**, la stessa classe che l'unità 05 ha chiuso
   su `CollectionEdit` con sei frasi tradotte.
   **Qui c'è una contraddizione nel piano, e la segnalo invece di scioglierla da solo.** La prosa di
   `PIANO.md`, sezione RAZIONALE, assegna esplicitamente all'unità **08** le stringhe «Il database ha
   rifiutato…» di `SpaceDetail.razor`, dicendo che vanno tradotte come quelle dell'unità 05. Ma la
   tabella MAPPA RILIEVO → UNITÀ assegna il rilievo 3 a «05 + 13», e il perimetro dell'unità 13 è
   `NoteEdit`, `ItemEdit`, `SpesaEdit` — non `SpaceDetail`. **Il mio mandato non lo nomina affatto**:
   dice «tre rilievi, uno per file» ed elenca 7, 11 e 12.
   Ho obbedito al più specifico — il mandato — come prescrive la regola sui conflitti, e non ho
   allargato il diff. Ma così `SpaceDetail` resta l'unico file con il rilievo 3 aperto e **senza
   un'unità che lo possieda**: o si accoda all'unità 13, o serve un giro in più. Va deciso dal capo.
   Il piano dice che in caso di dubbio vince la tabella, il che darebbe la 13; lo dico perché la
   prosa era stata corretta di proposito il 3 settembre, quindi non è ovvio che sia lei quella
   stantia.

2. **`profiles.display_name` e `profiles.avatar_url` sono congelati al primo accesso.**
   `handle_new_user` (`supabase/migrations/20260811000000_initial_schema.sql:187-193`) li scrive con
   `on conflict (id) do nothing`, e **nessun punto dell'applicazione li aggiorna mai** — l'unico
   consumo è in lettura, `Services/SpaceRepository.cs:122-123`. Conseguenza: chi cambia nome o foto
   su Google li vede aggiornati sulla **propria** pagina `/profile` (che legge la sessione Gotrue
   viva, `AuthStateService.cs:43-62`), ma **gli altri membri continuano a vedere quelli del primo
   accesso, per sempre**.
   Non è nel mio perimetro — il rimedio è nel database o in un servizio, non nei miei tre file — e
   non è fra i sedici rilievi. L'ho scoperto proprio verificando cosa potevo scrivere nell'aiuto di
   `/profile`, ed è il motivo per cui quell'aiuto **non** promette che una correzione fatta su Google
   arrivi agli altri: sarebbe stata l'affermazione falsa che il mandato mi chiedeva di evitare.

3. **Per l'unità 10 (`Shared/RecensioniElemento.razor`), un fatto già istruito.** Il commento a
   `recensioni.sql:121-122` indica la via d'uscita quando si vuole togliere una recensione altrui:
   «deve cancellare l'elemento (v. l'ON DELETE CASCADE…)». È materiale buono per l'aiuto di quella
   schermata, che è il posto dove serve; non l'ho messo nel mio perché su `/spaces/{id}` sarebbe
   stato un dettaglio su un'altra pagina.

## GATE

- `dotnet build -warnaserror` → **Avvisi: 0, Errori: 0**. Rifatto anche con `--no-incremental`,
  stesso esito: lo stesso rigore che l'unità 07 aveva dichiarato, perché su una build incrementale
  i `.razor` già generati potrebbero non essere rianalizzati.
- `dotnet test` → **273 superati, 0 non superati, 0 ignorati**.
  Contati **273 anche in partenza**, prima di toccare qualunque file: sono i 267 che i mandati fino
  alla 07 citavano più i 6 aggiunti dall'unità 12 a `DenaroTests`. Il mio diff non ne ha cambiato
  nessuno, né in più né in meno, come richiesto.
- Compilato **una volta sola, da me, a fine giro**. Nessun `implementer` ha compilato, e nessun
  revisore: glielo ho vietato nel brief per non far collidere due build su `obj/`.
- Server di sviluppo **non avviato**, browser non aperto: lo fa il capo con `live-testing` quando
  tutte le unità sono rientrate.

## SCOSTAMENTI

1. **Non ho aggiunto il link di ritorno su `/spaces/{id}`, e la condizione del mandato non si è
   avverata.** Il mandato lo autorizzava «se dopo aver messo la testata la pagina resta senza un modo
   evidente di tornare indietro», e imponeva una forma «coerente con quelle che le altre pagine usano
   già». Ho guardato: **`Pages/CollectionDetail.razor` è l'analogo esatto** — pagina di dettaglio
   raggiunta da un elenco — e nel suo ramo caricato (`else` a `:32`) **non ha nessun link di
   ritorno**; ce l'ha solo nei rami d'errore, esattamente come `SpaceDetail` già fa a `:45` e
   `:52` (numeri attuali, riaperti sul disco). Il ritorno lo fa la barra: `Shared/Navigazione.razor:60` ha la voce «Spazi» in **entrambe**
   le forme, telefono e schermo largo, e `EAttiva` (`:103`, `corrente.StartsWith("spaces")`) la
   accende con `aria-current="page"` mentre si è su `/spaces/{id}`. Aggiungerlo alla sola
   `SpaceDetail` l'avrebbe resa l'unica pagina di dettaglio del progetto con un ritorno in pagina —
   cioè quella «terza forma» che il mandato vietava di inventare. Se il capo giudica insufficiente il
   ritorno dalla barra, è una decisione di progetto che vale per **tutte** le pagine di dettaglio, non
   per questa sola.

2. **Il pannello `<Aiuto>` di `/spaces/{id}` ha quattro paragrafi, non tre.** Il quarto è la
   correzione del rilievo di `threat-hunter`, adjudicata sopra.

3. **Non ho scritto niente sull'eliminazione dello spazio nell'aiuto**, benché il mandato la
   elencasse fra le cose da coprire. È già a schermo più in basso nella stessa pagina:
   `SpaceDetail.razor:124`
   («Sparisce per tutti i membri, insieme a tutto quello che contiene. Non si torna indietro.»).
   Ripeterla sarebbe stato il rilievo che si è presa l'unità 04. Per la stessa ragione l'aiuto non
   ripete «Chi ha questo codice può entrare» (`:82`) né «Smetterai di vederne il contenuto» (`:140`):
   di quelle tre frasi l'aiuto dice solo ciò che **aggiungono** — che il codice non scade e non si
   revoca, e che uscendo il proprio lavoro resta dentro.

4. **Ho valutato e scartato un paragrafo sullo spazio personale** nell'aiuto di `/spaces/{id}`.
   `Pages/Spaces.razor:11` dice già «Lo spazio personale non si condivide», e la pagina genitore è a
   un clic. Al suo posto ho circoscritto il primo paragrafo con «Chi entra…», così il testo non parla
   di condivisione a chi sta guardando uno spazio che non si condivide.

5. **Nessuna voce accodata all'unità 11 per `app.css`.** Non ne è servita nessuna.

## DA PROVARE NEL BROWSER

Le tre prove obbligatorie, col testo esatto che va cercato a schermo.

**1 — Il link «Gestisci questo spazio» sparisce sul personale e ricompare sul condiviso.**
Sulla Home con lo spazio **personale** attivo, scorri in fondo: il link **«Gestisci questo spazio»**
(testo esatto) **non c'è**, e non c'è nemmeno il paragrafo che lo conteneva — non deve restare uno
spazio vuoto in fondo alla pagina. Poi cambia spazio dal **selettore in barra laterale** (in cima
alla Home sul telefono, in fondo alla colonna su schermo largo) scegliendo uno spazio **condiviso**:
il link **ricompare**, e porta a `/spaces/{id}` di quello spazio.
**Attenzione al ritardo, che non è un difetto:** la Home si ridisegna solo **dopo** che i dettagli
del nuovo spazio sono stati letti dalla rete (`Home.razor:408-429`). Per la durata di quel giro
restano a schermo il nome, il sottotitolo **e la visibilità del link** dello spazio precedente. È
comportamento preesistente, condiviso col titolo. Aspetta che il nome dello spazio in testata sia
cambiato **prima** di giudicare il link.

**2 — La riga delle categorie non si confonde con un totale.**
Sulla Home, nella striscia sotto `QUESTO MESE` e il totale in euro, con **una sola** categoria di
spesa nel mese in corso deve leggersi esattamente:

    Per categoria: Spesa 100%

e con due categorie, la stessa riga nella forma `Per categoria: Spesa 38% · Trasporti 22%` (le
percentuali dipendono dai dati veri). La cosa da verificare è che **«Per categoria:» e i valori
stiano sulla stessa riga logica e vicini**: `.cime` ha `margin-left: auto`
(`wwwroot/css/app.css:1937`) e la striscia ha `flex-wrap: wrap`, quindi su schermo stretto la riga
va a capo — è accettabile che vada a capo **tutta insieme**, non che l'etichetta resti separata dai
valori. Se lo stacco si vede, è una voce di CSS da accodare all'unità 11, non un errore di questa.
Da fare **su uno spazio che ha spese nel mese corrente**: senza spese la striscia mostra «Nessuna
spesa questo mese» e la riga non compare affatto.

**3 — Le due schermate hanno il «?», e dice cose che non sono già a schermo.**
Su **`/spaces/{id}`** di uno spazio **condiviso**: accanto al titolo c'è il pulsante «?»
(`aria-label="Come si usa questa schermata"`). Aprendolo compaiono **quattro** paragrafi. Verifica
che nessuno dei quattro ripeta le tre frasi già visibili scorrendo la pagina — «Chi ha questo codice
può entrare. Dallo solo a chi vuoi dentro.», «Sparisce per tutti i membri, insieme a tutto quello
che contiene.», «Smetterai di vederne il contenuto. Puoi rientrare col codice.» I quattro cominciano
con: «Chi entra non si limita a guardare…», «Le recensioni fanno eccezione…», «Il codice invito non
scade…», «Se esci da uno spazio…».
Su **`/profile`**: stesso «?», **tre** paragrafi, che cominciano con «Nome ed email arrivano
dall'account Google…», «Il nome e la foto compaiono agli altri membri…», ««Esci» chiude la sessione
solo su questo dispositivo…». A schermo sotto ci sono solo il nome, l'email e il pulsante «Esci»:
nessuno dei tre paragrafi deve ripeterli.
Controlla anche che su entrambe le pagine ci sia **un solo `<h1>`** — su `SpaceDetail` l'`<h1>`
preesistente è stato tolto perché `TestataPagina` ne rende già uno, e un secondo titolo sarebbe il
segno che la rimozione non ha attecchito. E che il titolo compaia **anche durante il caricamento**
(«Spazio» come ripiego, poi il nome vero) e nel ramo «Questo spazio non esiste, oppure non ne fai
più parte».
