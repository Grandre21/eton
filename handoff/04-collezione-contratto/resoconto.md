UNITÀ: 04 — ESITO: FATTO

TOCCATI:
- `Pages/CollectionEdit.razor` → +30/−16
- `Pages/NoteEdit.razor` → +1/−1

## CONTRATTI

`Shared/PaginaEditor.cs` **non è stato toccato**: nessuna deviazione, nessun `BLOCKED`. La
firma reale prodotta dall'unità 03 si è incastrata senza attriti, e i cinque punti del
mandato sono stati eseguiti alla lettera. `file:line` verificati da me sul file su disco
dopo la correzione finale, non ripresi dai revisori (v. la nota sulle citazioni in coda).

```razor
Pages/CollectionEdit.razor:5     @inherits PaginaEditor
Pages/CollectionEdit.razor:40    <NavigationLock ConfirmExternalNavigation="@Cambiata"
                                                 OnBeforeInternalNavigation="GuardaUscita" />
```

```csharp
// Pages/CollectionEdit.razor:260
protected override bool Cambiata => Nuova
    ? !string.IsNullOrWhiteSpace(nome)
    : collezione is not null && ( … );        // corpo invariato rispetto a prima

// Pages/CollectionEdit.razor:545 — dopo Crea()
Esci($"collections/{creata.Id}/edit", replace: true);

// Pages/CollectionEdit.razor:625 — dopo Elimina()
Esci("collections");
```

- `@inject NavigationManager Navigation` **tolto**. Nessun `Navigation.` residuo nel file:
  i due `NavigateTo` erano gli unici due usi, ed erano entrambi post-`Crea()` e
  post-`Elimina()` come il contratto prevedeva. Nessun bisogno di rimettere l'`@inject`.
- `IDisposable`: **non implementato dalla pagina**. La base basta, non serviva pulizia
  propria, quindi nessun `public override void Dispose()` e nessun rischio di CS0108.
- `<NavigationLock>` è **dentro il ramo del modulo** (prima riga del ramo `else`), unica
  istanza nel file. Nessun revisore ha proposto di spostarla fuori dai rami — il paragrafo
  del mandato non ha dovuto essere citato in adjudica.

**Per le unità 06 e 07: il contratto regge così com'è.** L'adozione su una pagina di 625
righe con macchina a stati più complessa di `NoteEdit` (`generazione`, `conflitto`,
`sparita`, `PuoIntervenire`, `erroriValidazione`) non ha richiesto nessuna aggiunta né
deroga. `ItemEdit` e `SpesaEdit` possono seguire lo stesso schema senza verifiche nuove.

## GATE DI «CHIUDI»

Applicato su entrambi i file nella forma esatta del mandato, con `null` letterale:

```razor
Pages/CollectionEdit.razor:201   <a class="btn" href="@(occupato ? null : "collections")">Chiudi</a>
Pages/NoteEdit.razor:98          <a class="btn" href="@(occupato ? null : "notes")">Chiudi</a>
```

Nessun `?? ""`, nessun `disabled`, nessun `aria-disabled`, nessun `tabindex`, nessuno stile
inline, nessuna trasformazione in `<button>`. I quattro `<a class="btn" href="…">Torna
a…</a>` dei rami `sparita` ed errore non sono stati toccati: lì `occupato` non è in gioco.

**Il selettore `a.btn:not([href])` resta all'unità 11**, come da mandato. Fino ad allora i
due link sono funzionalmente inerti ma non spenti visivamente. È lo stato intermedio atteso.

## ADJUDICA

Revisori lanciati: `bug-hunter`, `conformity`, `threat-hunter`.

**`backend-expert` non lanciato, ed è una scelta dichiarata.** Il gate lo vuole su superficie
nuova (tipo, servizio, endpoint, astrazione), diff > ~120 righe, o richiesta esplicita:
qui il diff è di 47 righe, non nasce nessun tipo, nessun servizio, nessuna astrazione —
l'unità *consuma* un contratto scritto altrove, e il budget vietava esplicitamente di
ridisegnare. Lanciarlo avrebbe prodotto rilievi di design su codice che non era lecito
ridisegnare.

**`threat-hunter` lanciato benché il pattern fosse già passato nell'unità 03**, perché il
diff introduce testo scritto dall'utente (il nome della collezione) reso in un `<h1>` via
`TestataPagina` e in `<PageTitle>`: è render di markup, e il mandato dice di lanciarlo in
caso di esitazione. **0 rilievi.**

    istruttoria: 1 rilievo su 1 file → checker no

Sotto entrambe le soglie (somma ≥ 4, oppure ≥ 3 file distinti). `bug-hunter` 0 rilievi,
`threat-hunter` 0 rilievi — e i rilievi di `threat-hunter` non entrerebbero comunque nella
somma per regola. La somma è il solo rilievo di `conformity`.

**1. `conformity`, severità media, duplicazione — FONDATO, corretto.**
Il primo paragrafo del pannello `<Aiuto>` che avevo scritto apriva con «I campi sono le
colonne che ogni elemento di questa collezione avrà», che ripete quasi parola per parola
ciò che la pagina dice già da sé a `Pages/CollectionEdit.razor:107`:

    <p class="testo-tenue">Le colonne che ogni elemento di questa collezione avrà.</p>

La prova l'avevo già letta io stesso all'apertura del lavoro, leggendo il file per intero:
il claim regge senza bisogno di riaprire nulla. Il criterio violato — il pannello dice ciò
che la pagina **non** dice da sé — è quello che rispettano tutti e cinque gli altri
consumatori di `<Aiuto>` del progetto. **L'errore era nel mio brief, non nell'esecuzione.**
Corretto togliendo la prima frase: `Pages/CollectionEdit.razor:14` è ora

    <p>Rinominare l'etichetta di un campo non tocca i valori già inseriti; toglierlo li lascia salvati ma non più visibili.</p>

che è informazione presente in pagina solo *dopo* aver premuto «Togli» (`:176`), cioè
quando è tardi per pianificare, e mai per la rinomina.

**Campione sugli infondati: non ce n'è nessuno.** Nessun rilievo è stato respinto in questa
unità — l'unico arrivato era fondato ed è stato corretto. Il controllo a campione del §5
non ha materia su cui applicarsi, e lo dichiaro invece di ometterlo in silenzio.

**Verifica indipendente su un «0 rilievi».** `bug-hunter` ha risposto 0 a una domanda che
avevo posto io e che era il punto più rischioso del diff — se qualche percorso scriva
`errore` o `avviso` e poi finisca in un ramo diverso da `else`, dove il blocco spostato non
verrebbe più reso. Ho aperto il codice e trovato **un caso che la sua risposta compattava
troppo**: il `catch` di `OnParametersSetAsync` (`:331-343`) imposta `errore` e
`collezione = null`, ma con `Nuova` vero la condizione `collezione is null && !Nuova` è
falsa, quindi la pagina resta nel ramo `else` e l'errore di caricamento compare ora **in
fondo** al modulo invece che in cima. **Non è una regressione e non l'ho corretto**: è
esattamente il comportamento che `NoteEdit` ha già dopo l'unità 03 (stesso `catch`, stesso
commento a `NoteEdit.razor:207-212` che spiega perché in creazione il modulo resta
accessibile), e su `/collections/new` la scheda «Campi» nasce vuota, quindi il modulo è
basso e il messaggio resta in vista. Va comunque guardato nel browser: è la prova 6 in
fondo.

**Nota sull'affidabilità delle citazioni.** Le `file:line` di `threat-hunter` sono risultate
sfasate in modo consistente (dà `<PageTitle>` a `:16` e «Chiudi» a `:68`, mentre stanno a
`:10` e `:201`). I suoi verdetti erano verificabili senza le righe — le due branch
dell'`href` sono letterali costanti, e la riga di `PuoIntervenire` compare nel diff come
contesto **invariato** — quindi li ho accolti, ma nessun numero di riga di questo resoconto
viene da lui. Quelle di `conformity` tornano tutte. **Vale la pena saperlo per le unità
06 e 07.**

## FUORI SCOPE

**1. L'esito del salvataggio si cerca ancora in due punti diversi.** Sollevato da
`conformity` come osservazione, non come rilievo, e lo confermo. Dopo aver premuto lo stesso
pulsante «Salva», l'utente trova l'esito sotto la scheda «Campi» (`:183-190`, il blocco che
questa unità ha spostato) se è un errore di rete, di permesso o l'avviso di successo, ma
**dentro** la scheda «Campi» e in cima a essa (`:109-119`, `erroriValidazione`) se è un
errore di validazione. Con pochi campi la differenza è di poche righe; con molti campi —
il vincolo SQL ne ammette fino a 40 — la scheda diventa alta diverse schermate e
l'errore di validazione torna fuori vista, cioè il difetto che il rilievo 2 correggeva.
**Non l'ho toccato**: il mandato diceva esplicitamente di spostare solo `errore`/`avviso` e
di lasciare `erroriValidazione` dov'è. La decisione è del capo o dell'unità 05, che lavora
su questo stesso file.

**2. `@using Eton.Services` a `Pages/CollectionEdit.razor:4` è ridondante**, perché
`_Imports.razor:9` lo importa già globalmente. **Preesistente al diff** e non toccato da
questa unità. `@using Eton.Models` a `:3` invece serve, non è negli import globali.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**. Ho usato `-warnaserror`, più severo
  del gate richiesto, perché con `@inherits` e `override` un CS0108 è l'errore che questo
  lavoro poteva realisticamente introdurre e che una build permissiva declasserebbe ad
  avviso ignorabile.
- `dotnet test --no-build` → **267/267 superati**, 0 non superati, 0 ignorati (101 ms).

Compilato **una volta sola**, a fine giro, con nessun agente vivo. Gli implementer non hanno
compilato.

## SCOSTAMENTI

**Uno solo, minore, e dichiarato in anticipo nel brief.** Ho fatto correggere due
riferimenti `file:line` dentro commenti preesistenti di `CollectionEdit.razor`, che l'unità
03 aveva reso stantii spostando le righe di `NoteEdit`:

- `:193` citava `NoteEdit.razor:79-82` → ora `NoteEdit.razor:91-94`
- `:539` citava `NoteEdit.razor:209-212` → ora `NoteEdit.razor:260-263`

Non era chiesto dal mandato. L'ho incluso perché erano due numeri dentro un file che stavo
comunque riscrivendo, il costo è una riga ciascuno, e lasciarli rotti significava lasciarli
rotti per sempre: nessuna unità futura ha motivo di aprire quei commenti. Non è
un'astrazione, non è una modifica di comportamento, e `conformity` li ha verificati
corrispondenti ai blocchi giusti.

**Nient'altro diverge.** In particolare **non** divergono: la posizione di `<NavigationLock>`
dentro il ramo del modulo, la firma di `Cambiata`, i due `Esci(...)`, la forma del gate di
«Chiudi» su entrambi i file, la posizione e l'API di `<TestataPagina>`, lo spostamento del
blocco `errore`/`avviso` a markup invariato. Nessuna astrazione nuova, nessun tipo nuovo,
nessun metodo nuovo oltre alla proprietà `TitoloSchermata` (`:250`, due call-site: `<PageTitle>`
a `:10` e `<TestataPagina>` a `:12`), nessun file `.js`, nessun servizio iniettato nuovo,
nessun pacchetto, nessuna riga di `app.css`, nessuno stile inline.

**Il server di sviluppo non è stato avviato** e nessuna prova è stata fatta nel browser,
come da mandato. Nessun processo lasciato vivo, nessuna porta occupata.

## DA PROVARE NEL BROWSER

Da aggiungere alle dieci prove già scritte in `handoff/03-contratto-editor/resoconto.md`,
che restano valide per `NoteEdit`. Ordinate per quanto costa scoprirle tardi.

**1. Il gate di «Chiudi» su entrambe le pagine — la prova nuova di questa unità.**
Su `/collections/new`: scrivere un nome, premere «Salva» e premere **subito** «Chiudi»,
mentre il pulsante mostra «Salvo…».
*Accettazione*: il clic su «Chiudi» **non fa niente** — nessuna navigazione, nessun cambio
di indirizzo, nessuna domanda. Appena la scrittura rientra, l'indirizzo diventa
`/collections/{id}/edit` e «Chiudi» torna premibile.
*Stessa prova su `/notes/new`*, con destinazione `/notes/{id}`.
**Limite dichiarato**: la finestra è un solo round-trip di rete, quindi a mano è stretta.
Se non si riesce a premere in tempo, la via affidabile è strozzare la rete dagli strumenti
per sviluppatori. Se non è praticabile, va detto come limite, non dato per verificato: la
verifica di ripiego è ispezionare il DOM del link durante `occupato` e controllare che
l'attributo `href` sia **assente**, non vuoto — `href=""` sarebbe un difetto peggiore del
problema di partenza, perché è un link valido verso la radice dell'applicazione.

**2. Il link inerte non è ancora spento, ed è atteso.** Durante la stessa finestra il link
«Chiudi» resta di aspetto normale: non è grigio, non sembra disabilitato.
*Accettazione*: **non è un difetto di questa unità.** Il selettore `a.btn:not([href])`
appartiene all'unità 11. Segnalarlo come difetto sarebbe un falso positivo; **non
segnalarlo affatto** all'unità 11 sarebbe peggio, perché resterebbe un link che sembra
premibile e non lo è.

**3. La domanda all'uscita compare anche qui.** Su `/collections/new` scrivere un nome,
premere «Chiudi».
*Accettazione*: compare il dialogo nativo con «Hai modifiche non salvate: se esci adesso le
perdi. Vuoi uscire lo stesso?». Annullando si resta nell'editor **con i campi intatti**.
Ripetere col tasto Indietro del browser e con un link della barra di navigazione: stessa
domanda. Sul tasto Indietro, se annullando l'URL cambia comunque, è un difetto.

**4. Nessuna domanda dopo creazione, salvataggio ed eliminazione riusciti.** Tre prove:
(a) `/collections/new`, scrivere un nome, «Salva», attendere che l'indirizzo diventi
`/collections/{id}/edit`, premere «Chiudi»; (b) su una collezione esistente, modificare,
«Salva», attendere «Salvata.», «Chiudi»; (c) su una collezione esistente, modificare
**senza salvare**, poi eliminarla.
*Accettazione*: in tutti e tre i casi si esce **senza nessuna domanda**. Il caso (a) è
quello che l'implementazione ingenua sbaglia; il caso (c) esce benché il modulo sia sporco.

**5. La testata c'è e non ripete la pagina.** `/collections/new` e `/collections/{id}/edit`.
*Accettazione*: titolo in testa e infobutton «?» che apre il pannello con i **tre**
paragrafi; il pannello si chiude con Esc e col tocco fuori. Il primo paragrafo **non** deve
ripetere «Le colonne che ogni elemento di questa collezione avrà», che è già visibile nella
scheda «Campi» sotto — è il rilievo corretto in adjudica.
Nota attesa e **non** un difetto: l'`<h1>` **cambia mentre si digita** nel campo Nome, ed è
«Collezione» finché il campo è vuoto. È la stessa espressione che alimenta il titolo della
scheda del browser, riusata come chiede il mandato.

**6. L'esito dove si guarda, e il caso che ho verificato a mano.** Su una collezione con
molti campi (aggiungerne una decina, così il modulo scorre): premere «Salva».
*Accettazione*: «Salvata.» compare **appena sopra i pulsanti**, in vista senza scorrere.
Poi, il caso che ho isolato leggendo il codice e che vale una prova: aprire
`/collections/new` **con la rete disattivata** dagli strumenti per sviluppatori, così che
il caricamento degli spazi fallisca. *Accettazione*: il messaggio «Non è stato possibile
aprire la collezione: …» compare in **fondo** al modulo, sopra i pulsanti, e non in cima.
È il comportamento voluto e identico a quello di `NoteEdit`, ma è l'unico punto in cui
l'esito riguarda il **caricamento** e non il salvataggio, quindi merita un occhio: se il
modulo su `/collections/new` risultasse più alto di una schermata e mezza, il messaggio
sarebbe fuori vista all'apertura e la cosa andrebbe riportata al capo.

**7. L'errore di validazione, che sta ancora altrove.** Su una collezione con molti campi:
svuotare il Nome e premere «Salva».
*Accettazione*: l'errore compare **dentro** la scheda «Campi», in cima a essa, non sopra i
pulsanti. **Non è un difetto di questa unità** — è la voce 1 di `FUORI SCOPE`, ed è
esattamente ciò che serve misurare per decidere se vada corretto: annotare **quanto** si
deve scorrere per vederlo, perché è quel numero, non l'opinione, a dire se il rilievo 2 è
davvero chiuso su questa pagina.

**8. Il primo collaudo della migrazione dell'unità 02.** Il piano lo chiede al primo giro
utile e questa unità non ha potuto farlo: creare una collezione da `/collections/new` e
verificare che il **42501 non compaia più**. Finché non è visto, resta un fatto riferito.

**Cosa non è praticabile a mano, e perché.** La collezione sparita sotto i piedi — il caso
su cui l'unità 03 ha respinto lo spostamento di `<NavigationLock>` fuori dai rami — richiede
due schede sulla stessa collezione, eliminarla nella prima e salvare nella seconda. È
fattibile ma laborioso; se c'è occasione, l'accettazione è: compare «Questa collezione non
c'è più…», e premendo «Torna alle collezioni» **nessuna domanda**, benché il modulo
modificato sia ancora in memoria. Se non si fa, va detto come limite.
Il dirottamento della navigazione tardiva (`smontata`/`Dispose`) non è riproducibile a mano
in modo affidabile senza strozzare la rete, ed è già dichiarato come limite nel resoconto
dell'unità 03: non lo riapro qui.
