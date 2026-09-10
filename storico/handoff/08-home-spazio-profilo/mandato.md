UNITÀ: 08/12 — Home, spazio e profilo: tre rilievi e una promessa ritirata

## OBIETTIVO

Tre rilievi, uno per file. Non c'è nessun contratto da adottare qui: queste **non sono pagine
editor**, non hanno modulo, non hanno `Cambiata` e non devono ereditare da `PaginaEditor`.
Se ti trovi a pensare a `NavigationLock`, sei sulla pagina sbagliata.

1. **Rilievo 7** — «Gestisci questo spazio» promette una gestione che sullo spazio personale
   non esiste. **Deciso dall'utente: si nasconde il link sul personale.**
2. **Rilievo 11** — nella Home si legge «Spesa 100%» e non si capisce che *Spesa* è una
   **categoria** e non un'etichetta generica.
3. **Rilievo 12, per la metà che ti compete** — `/spaces/{id}` e `/profile` non hanno la
   testata con l'infobutton «?». L'altra metà (gli editor) è già chiusa dalle unità 03-07.

## PERIMETRO — file di tua proprietà esclusiva

- `Pages/Home.razor` (410 righe)
- `Pages/SpaceDetail.razor` (312 righe)
- `Pages/Profile.razor` (58 righe)

## NON TOCCARE

- **`Shared/TestataPagina.razor`**: lo consumi con l'API esistente, non lo modifichi. Quattro
  unità l'hanno già consumato così.
- **`Shared/Navigazione.razor`, `Shared/SelettoreSpazio.razor`**: sono la barra laterale, e
  appartengono all'**unità 11** insieme al rilievo 14. Non toccarli nemmeno se ti sembra che il
  rilievo 7 si risolverebbe meglio lì.
- **`Pages/Notes.razor`, `Pages/Collections.razor`**: unità 09. In particolare **lo stato vuoto
  della Home non si tocca**: il rilievo 13 dice che il trattamento della Home è quello
  **giusto**, ed è l'unità 09 a doverlo imitare sulle altre due. Se lo cambiassi, romperesti il
  modello prima che venga copiato.
- **`wwwroot/css/app.css`**: unità 11. Usa classi esistenti; se non basta, torna `BLOCKED` e la
  voce si accoda a quelle in attesa.
- Tutti i `Pages/*Edit.razor`: chiusi.

## RILIEVO 7 — la promessa che si ritira

**Dove**: `Pages/Home.razor:215`, `<a href="spaces/@Spazi.Attivo.Id">Gestisci questo spazio</a>`.

**Cosa succede oggi**: per lo spazio personale `/spaces/{id}` mostra solo «MEMBRI (1)» col
proprio nome e il badge «proprietario». Nessuna azione possibile. Il link promette una gestione
che per quello spazio non esiste, e da lì non c'è ritorno se non la barra laterale.

**Cosa fare**: **nascondere il link quando lo spazio attivo è quello personale.** Non
disabilitarlo, non lasciarlo con un messaggio: nasconderlo. Lo ha deciso l'utente il 3 settembre.

Il predicato per distinguere il personale **esiste già in questa pagina** e lo usa due volte:
`:71` («Lo spazio personale lo vedi solo tu…») e `:77` («Il tuo spazio personale…»). **Usa
quello**, non inventarne un altro e non aggiungere un campo al modello.

**Il ritorno da `/spaces/{id}`, che il rilievo nomina, si risolve da sé** con la testata del
rilievo 12: `<TestataPagina>` dà titolo e contesto. Se dopo averla messa la pagina resta senza
un modo evidente di tornare indietro, aggiungi un link di ritorno **coerente con quelli che le
altre pagine usano già** — guarda come lo fanno, non inventarne una terza forma.

## RILIEVO 11 — «Spesa 100%» è una categoria, e non si vede

**Dove**: Home, la riga sotto il totale del mese. Il commento a `Pages/Home.razor:372` spiega
cosa quella riga è: «"Spesa 38% · Trasporti 22%": le prime due categorie del mese, già in
ordine di quota».

**Il problema**: una delle dieci categorie si chiama **Spesa** — nel senso di *spesa
alimentare* — e la sezione che la contiene si chiama *spese*. Con una sola categoria nel mese si
legge «Spesa 100%», che sembra un totale e non lo è.

**Cosa fare**: rendere leggibile che quelle sono **categorie**. Il rimedio minimo è
un'etichetta che introduce la riga — qualcosa che dica «per categoria» — oppure la scelta
esplicita di non abbreviare. **Non rinominare la categoria**: il nome sta in
`Services/CategorieSpesa.cs`, è fuori dal tuo perimetro, ed è già scritto nelle spese esistenti
sul database.

Non ti do la formulazione: **guarda la schermata nel codice**, vedi cosa c'è già intorno
(`QUESTO MESE`, il totale, il confronto col mese precedente) e scrivi qualcosa che stia in
quella famiglia. Poi **dichiara nel resoconto la formulazione esatta che hai scelto**, perché è
la cosa che `live-testing` andrà a leggere.

## RILIEVO 12 — le due schermate che tacciono

`Pages/SpaceDetail.razor` e `Pages/Profile.razor` hanno **zero** occorrenze di
`TestataPagina`, verificato. `Home.razor` ne ha due e non va toccata su questo punto.

**Cosa fare**: `<TestataPagina Titolo="…">` con `<Aiuto>`, come l'hanno fatta le quattro pagine
editor. **Guarda `Pages/SpesaEdit.razor:11-17`**, che è la più recente e la meglio istruita, e
segui quella forma.

**Il pannello `<Aiuto>` deve dire ciò che la pagina non dice già da sé.** L'unità 04 si è presa
un rilievo fondato per aver ripetuto nell'aiuto una frase visibile due righe sotto. Leggi ogni
pagina per intero prima di scrivere il testo, e cerca il comportamento **invisibile**:

- su `/spaces/{id}`: cosa vede davvero un altro membro, cosa comporta il codice invito, cosa
  succede a ciò che hai scritto se lo spazio viene eliminato o se ne esci. Non «qui vedi i
  membri»: quello è già a schermo.
- su `/profile`: la pagina è di 58 righe, quindi dice pochissimo. Proprio per questo l'aiuto ha
  qualcosa da dire — ma **solo se è vero**: verifica nel codice cosa la pagina fa davvero prima
  di scrivere che lo fa.

**Se una delle due pagine non ha niente di invisibile da spiegare, dillo nel resoconto e metti
la testata col solo titolo.** Un `<Aiuto>` che ripete lo schermo è peggio di nessun `<Aiuto>`:
è la lezione dell'unità 04, e vale anche al contrario.

## BUDGET DI COMPLESSITÀ

Nessuna astrazione nuova, nessun tipo nuovo, nessun servizio, nessun file `.js`, nessun
componente nuovo. Tre file, tre rilievi, e il più grosso dei tre è mettere una testata che
esiste già. Se ti trovi a progettare qualcosa, sei fuori strada.

Un solo `implementer` per tutti e tre i file, oppure uno per file: scegli tu, ma **mai due
`implementer` sullo stesso file**.

## STATO

Unità precedenti, tutte `FATTO` e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`),
05 (`e139ce8`), 06 (`f4f2dbd`), 07 (`4327598`), 12. Sei l'unica unità viva: nessun'altra sessione
tocca il repository mentre lavori, e il working tree è pulito quando parti.

L'unità 12 ha aggiunto `Denaro.TestoDigitabile` e alcuni test in `Eton.Tests/DenaroTests.cs`.
Non ti riguarda — nessuno dei tuoi tre file usa `Denaro` in scrittura — ma spiega perché il
numero dei test è cambiato rispetto ai 267 che i mandati precedenti citavano.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: se ci trovi una riga che contraddice
questo mandato, vince la più recente. In particolare c'è una riga del 3 settembre sera che
dice che **l'utente non è raggiungibile**: qualunque domanda tu abbia, non aspettarla, portala
nel resoconto.

**Due fatti operativi che ti risparmiano un errore.**

- Le `file:line` di `threat-hunter` sono risultate **sfasate** sui diff delle unità 04 e 05, ma
  **esatte** su quello della 07. Accogli i suoi verdetti se reggono per contenuto, e riapri i
  numeri prima di riportarli — senza trattarlo come inaffidabile per principio.
- Se un tuo obiettivo e un tuo divieto si contraddicono, **obbedisci al più specifico e
  dichiaralo** nel resoconto. È successo alle unità 05 e 07, e entrambe l'hanno gestito bene.

**Se i revisori tornano tutti a zero rilievi, non è finita.** Scrivi comunque la riga di
istruttoria, dichiara che non c'è nessun campione da riverificare, e **verifica tu almeno la
domanda più rischiosa del tuo diff**. Le unità 06 e 07 l'hanno fatto e in entrambi i casi ne è
uscito qualcosa che i revisori non avevano isolato — nella 07, un difetto bloccante che è
diventato un'unità propria.

**La domanda più rischiosa di questo diff, se non ne trovi una migliore:** il predicato con cui
nascondi il link del rilievo 7. Verifica che sia vero **anche prima che gli spazi siano
caricati** — se `Spazi.Attivo` fosse nullo durante il primo render, il link potrebbe comparire
per un istante e poi sparire, oppure far saltare la pagina. Guarda come `:71` e `:77` si
proteggono e fai lo stesso.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**.
- `dotnet test` → tutti verdi. Erano 267 fino all'unità 07; la 12 ne ha aggiunti alcuni a
  `DenaroTests`. **Conta quanti sono quando parti** e verifica che alla fine siano gli stessi,
  tutti verdi: il tuo diff non deve cambiarne nessuno, né in più né in meno.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai: `obj/` non ha lock
fra processi.

**Non avviare il server di sviluppo e non provare nel browser.** Lo fa il capo con
`live-testing` quando tutte le unità sono rientrate.

BUDGET: 20 dollari

RESOCONTO IN: `handoff/08-home-spazio-profilo/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 08 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <la forma della testata e il predicato del rilievo 7, con file:line riaperti da te>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti, e a chi appartiene il rimedio>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

Aggiungi `DA PROVARE NEL BROWSER`. **Tre prove sono obbligatorie**, una per rilievo, e devono
contenere il **testo esatto** che hai scritto, perché è quello che verrà cercato a schermo:

1. Sullo spazio **personale** il link «Gestisci questo spazio» **non c'è**; passando a uno
   spazio condiviso **ricompare**. Il cambio di spazio si fa dal selettore in barra laterale.
2. Con una sola spesa nel mese, la riga delle categorie si legge senza confondere la categoria
   *Spesa* col totale. Scrivi la formulazione esatta che deve comparire.
3. Su `/spaces/{id}` e `/profile` c'è il «?», e ciò che dice **non è già scritto sullo schermo
   sotto**.
