UNITÀ: 12 — ESITO: FATTO

## TOCCATI

- `Services/Denaro.cs` → +12/−1 — il metodo nuovo con la sua docstring, e la riga di rimando in coda alla docstring di `Testo`.
- `Pages/SpesaEdit.razor` → +7/−4 — le quattro sostituzioni, più tre righe di commento su `Cambiata` (v. `SCOSTAMENTI`).
- `Eton.Tests/DenaroTests.cs` → +51/−1 — sei test nuovi, una sezione nuova, la riga di rimando al commento dell'asimmetria.

Nessun altro file toccato. `Denaro.Verifica`, `Denaro.Prova`, il corpo di `Denaro.Testo`, `Pages/Spese.razor`, `Shared/PaginaEditor.cs` e `wwwroot/css/app.css` sono intatti.

## CONTRATTI

Tutti i `file:line` qui sotto sono **riaperti da me sull'albero finale**, dopo l'ultima modifica.

La firma, `Services/Denaro.cs:126`:

    public static string TestoDigitabile(decimal importo)
        => importo.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');

I quattro call-site in `Pages/SpesaEdit.razor`, tutti convertiti:

| Riga oggi | Riga nel mandato | Cos'è |
|---|---|---|
| `:213` | `:210` | il confronto dentro `Cambiata` |
| `:274` | `:271` | riempimento al caricamento |
| `:320` | `:317` | riempimento dopo un salvataggio riuscito |
| `:351` | `:348` | riempimento dopo «Ricarica» |

**I numeri del mandato erano esatti.** Li ho riaperti prima di toccarli e coincidevano tutti e quattro. Lo scarto di tre righe che si vede oggi è prodotto dal mio stesso diff: il commento aggiunto sopra `Cambiata` sposta in basso tutto ciò che segue. Cito i numeri post-diff perché sono quelli che il prossimo lettore troverà davvero.

`Denaro.Testo` resta usato in quattro punti, **tutti markup di sola lettura con `€` accanto**, e nessuno è stato toccato: `Pages/Home.razor:114`, `Pages/Spese.razor:143`, `:153`, `:198`.

## ISTRUTTORIA

Due giri, perché il secondo diff (un test aggiunto dopo l'adjudica) ha attivato di nuovo il gate.

    istruttoria: 3 rilievi su 2 file → checker no
    istruttoria: 0 rilievi su 0 file → checker no

Primo giro: `bug-hunter` 0, `conformity` 3, `threat-hunter` 0, `backend-expert` 2. La somma che conta è 0 + 3 = 3 su `Services/Denaro.cs` e `Eton.Tests/DenaroTests.cs`, sotto entrambe le soglie. Secondo giro: solo `bug-hunter` sul test aggiunto (diff < 30 righe, nessun impatto su sicurezza, dati o concorrenza), 0 rilievi.

`threat-hunter` è stato lanciato benché la superficie sia modesta, perché la catena `TestoDigitabile → Prova → SalvaAsync → numeric(12,2)` tocca un dato che finisce nel database: la regola dice di lanciarlo quando si esita, e io esitavo.

## ADJUDICA

**1. `conformity`, `Services/Denaro.cs:123-130`, media — FONDATO, corretto.**
La motivazione implementativa stava nel `<summary>` invece che in un commento `//` fra firma e corpo, che è il pattern del progetto per un formattatore a una riga. Ho riaperto la prova, `Services/CalcoliVoti.cs:84-89`, e regge testualmente: `/// <summary>Un voto reso leggibile: "7,5", "8", oppure "—" se non c'è.</summary>` seguito dal commento `// Stesso approccio di ValoriElemento.FormattaNumero: …` fra la firma e il `=>`. Applicato.

**2. `conformity`, `Services/Denaro.cs:108`, bassa — FONDATO, corretto.**
La riga di rimando cominciava con `<c>Testo</c> serve solo per la visualizzazione`: un membro che nomina sé stesso nel proprio `<summary>`, forma che il progetto non usa. Oggi la riga è `/// Serve solo per la visualizzazione; in un campo modificabile si usa <see cref="TestoDigitabile"/>, …`.

**3. `conformity`, `Eton.Tests/DenaroTests.cs:167-173`, bassa — FONDATO, corretto meglio del `FIX` proposto.**
Verificato con `cat -A`: nella sezione `Testo` il commento è attaccato al `[Fact]` (`:150` → `:151`), nella sezione nuova c'era una riga vuota. Il `FIX` proposto era togliere la riga vuota, ma il commento parla dei **due decimali sempre presenti**, che è il tema del *secondo* test, non del primo: attaccarlo al primo l'avrebbe reso fuorviante. L'ho spostato sopra `TestoDigitabile_di_un_intero_mostra_comunque_due_decimali` (`Eton.Tests/DenaroTests.cs:173-178`), dove spiega qualcosa. L'anomalia sparisce e il commento guadagna un bersaglio.

**4. `backend-expert`, `Services/Denaro.cs:126-130`, bassa, TIPO leggibilità — FONDATO IN PARTE.**
Accolta la parte che coincide col rilievo 1: il `<summary>` è ora di tre righe, il contratto e nient'altro, praticamente la `RISCRITTURA` proposta. **Scartata** la parte che voleva eliminare del tutto la motivazione su `"F2"` e sulla cultura invariante: la prova del rilievo 1 la smentisce. `CalcoliVoti.cs:86-88` **ripete** il fatto sulla cultura invariante che è già scritto nella docstring di classe di `ValoriElemento`, dichiarandolo pure (`// Stesso approccio di ValoriElemento.FormattaNumero`). In questo codebase quella ripetizione è una convenzione deliberata, non rumore. La motivazione è stata **spostata**, non cancellata: `Services/Denaro.cs:127-129`.

**5. `backend-expert`, `Pages/SpesaEdit.razor:210-212`, bassa, TIPO leggibilità — INFONDATO.**
Il claim: il commento su `Cambiata` è «la terza copia della stessa motivazione», già presente su `Denaro.Testo:108-109` e su `TestoDigitabile:124-126`.

**È il campione che ho riverificato aprendo io il codice**, come impone il §5, e la premessa è falsa. Le due docstring dicono, verbatim: «in un campo modificabile si usa `TestoDigitabile`, perché `Prova` rifiuta il punto delle migliaia» e «ciò che ne esce è rileggibile da `Prova`, l'uscita di `Testo` sopra il migliaio no». **Nessuna delle due afferma il fatto che il commento afferma**: che con `Testo` in un *confronto* la pagina nascerebbe già «modificata» e la guardia di uscita chiederebbe conferma a ogni singola uscita. `Denaro` non conosce `SpesaEdit` e non può dirlo — è precisamente l'informazione che solo il call-site possiede. Anche l'argomento di simmetria («è commentato solo `:213` e non i tre riempimenti») non regge: i tre riempimenti sono ovvi, il confronto no, ed è lì che la regressione produce il danno peggiore. Commento mantenuto.

Le altre tre domande poste a `backend-expert` sono tornate senza rilievo, e una sua osservazione va registrata perché è una **decisione di progetto**, non un difetto: v. `FUORI SCOPE`.

**6. `bug-hunter`, secondo giro, 0 rilievi — ma il suo punto 3 è stato raccolto.**
Ha dichiarato di **non poter verificare** l'affermazione, contenuta nel commento del test nuovo, che la scala di un `decimal` «sopravvive alla deserializzazione del record letto dal database»: dipende da come PostgREST serializza `numeric(12,2)`, e non è osservabile da questo repository. Ho controllato: `Models/Expense.cs:30` è un `[Column("amount")] public decimal Amount`, quindi il fatto sta fuori dal codice e nessuno l'ha misurato. Ho fatto riscrivere il commento (`Eton.Tests/DenaroTests.cs:184-191`): oggi dice che quale scala arrivi *non è sotto il controllo di questo codice né verificato qui*, e che **proprio per questo** l'invariante deve reggere per qualunque scala. L'affermazione è più debole, l'invariante che difende è più forte.

## LA DOMANDA PIÙ RISCHIOSA — verificata, e pinnata da un test

La domanda del mandato: dopo la correzione i due lati di `Cambiata` coincidono carattere per carattere all'apertura, sopra il migliaio, sotto il migliaio e con decimali `,00`?

Per costruzione sì — è la stessa funzione pura sullo stesso `spesa.Amount`. Ma quell'argomento nasconde un'assunzione: che `ToString("F2")` sia **indifferente alla scala interna** del `decimal`. In .NET `1284.5m`, `1284.50m` e `1284.500m` sono uguali come valore e diversi come rappresentazione, e la scala di `spesa.Amount` la decide la serializzazione di PostgREST, che non controlliamo. `bug-hunter` aveva *affermato* la normalizzazione; nessuno l'aveva *misurata*.

L'ho misurata, e ho lasciato la misura sul posto: `Eton.Tests/DenaroTests.cs:192-199`, `TestoDigitabile_non_dipende_dalla_scala_del_decimal`, quattro asserzioni — `1284.5m` (scala 1), `1284.500m` (scala 3), `1284m` (scala 0) e `7.0m` — che coprono i tre casi chiesti dal mandato. **Passa.** L'invariante è vero per costruzione, non per fortuna, e da adesso è sanzionato.

## FUORI SCOPE

**1. Rinominare `Testo` in `TestoMostrato`, o equivalente. Decisione di progetto, non mia.**
`backend-expert` la solleva rispondendo alla domanda sul presidio: oggi restano due funzioni quasi omonime di cui una è sbagliata per il caso d'uso più insidioso, e `Testo` è «il default» per posizione e per nome. Rinominarle entrambe in modo che nessuna delle due sia il default costa quanto il presidio attuale (otto call-site: quattro di visualizzazione, quattro di input) e toglie la trappola invece di documentarla. **Non l'ho fatta**: è fuori dal mio perimetro — tocca `Home.razor` e `Spese.razor` — e la classe di difetto è già chiusa dalla correzione. È materia del capo, o dell'utente.

**2. Il campo importo di chi non può intervenire mostra `1284,50` invece di `1.284,50`.**
Effetto collaterale voluto e inevitabile del rimedio: il campo è un `<input>`, quindi contiene la grammatica di input anche quando è disabilitato. L'elenco `/expenses` e la Home continuano a mostrare `1.284,50` con `Testo`. Non è un difetto — il mandato prescrive la conversione per tutti e quattro i call-site — ma è l'unica differenza visibile che questa unità introduce per un utente che non stava incontrando il difetto, e va vista nel browser (v. sotto).

Nessun altro rilievo fondato è rimasto aperto.

## GATE

Eseguiti da me, una volta sola, sull'albero **finale**, dopo l'ultima modifica. Agli implementer e ai revisori la build era vietata esplicitamente nel brief: `obj/` non ha lock fra processi.

- `dotnet build -warnaserror --no-incremental` → **0 errori, 0 avvisi**.
- `dotnet test --no-build` → **273/273 superati**, 0 non superati, 0 ignorati (256 ms).

Erano 267. Sei test nuovi → 273, e il mandato chiedeva «271 o più». **Nessuno dei 267 preesistenti ha cambiato esito**: i falliti sono 0 e il totale è cresciuto esattamente del numero dei nuovi. In particolare `Testo_e_Prova_non_sono_l_uno_l_inverso_dell_altro_sopra_il_migliaio` è verde con le asserzioni invariate, che è la prova che `Verifica` e `Prova` non sono state toccate.

Server di sviluppo **non avviato**, nessuna prova nel browser, nessun processo lasciato vivo, nessuna porta occupata.

**Il lavoro non è committato.** I tre file sono modificati nel working tree. Come l'unità 07, lo lascio pronto e lo dichiaro: i sette commit precedenti hanno messaggi scritti dal capo, che copre l'unità nel piano, e non mi intesto quella voce.

## SCOSTAMENTI

**Tre, tutti additivi, nessuno tocca ciò che il mandato vieta.**

**1. Tre righe di commento sopra `Cambiata`** (`Pages/SpesaEdit.razor:210-212`), non richieste: il mandato chiedeva quattro sostituzioni. Le ho volute perché il presidio scelto dal mandato — la docstring di rimando su `Testo` e la coppia di test — copre chi guarda `Denaro`, non chi rilegge `Cambiata` e vede un `TestoDigitabile` in un confronto accanto a pagine che usano `Testo` ovunque. `backend-expert` ha chiesto di toglierle; ho adjudicato infondato con la prova in mano (v. `ADJUDICA` 5).

**2. Un sesto test**, `TestoDigitabile_non_dipende_dalla_scala_del_decimal`. Il mandato ne elencava cinque «tutti obbligatori» — un minimo, non un tetto — e chiedeva di verificare la domanda più rischiosa. Un test è la forma durevole di quella verifica: fra sei mesi il ragionamento non ci sarà più, il test sì. Il budget resta rispettato: nessun tipo nuovo, nessuna astrazione, nessun helper, nessun file nuovo.

**3. Tre correzioni di forma da review**, tutte su docstring e commenti, nessuna sul comportamento: v. `ADJUDICA` 1, 2, 3.

**Non divergono**, e lo dichiaro perché il mandato lo chiedeva punto per punto: il nome `TestoDigitabile`, la firma e il corpo del metodo carattere per carattere come prescritti; i quattro call-site, non tre; `Verifica` e `Prova` intatte, corpi, docstring e `InlineData` dei loro test; le asserzioni di `Testo_e_Prova_non_sono_l_uno_l_inverso_dell_altro_sopra_il_migliaio` invariate; i quattro `Denaro.Testo` di `Home.razor` e `Spese.razor` lasciati dove sono; `Pages/Spese.razor`, `Shared/PaginaEditor.cs` e `wwwroot/css/app.css` mai aperti in scrittura. Nessun parser, nessuna maschera di input, nessun servizio, nessun pacchetto.

## DA PROVARE NEL BROWSER

**1. La prova obbligatoria, quella che chiude il rilievo.** Segnare da `/expenses` una spesa da `1284,50`, poi riaprirla dal registro.
*Accettazione*: il campo importo mostra **`1284,50`**, senza il punto delle migliaia; **nessun messaggio d'errore** sotto il campo; «Salva» è **spento**. Il punto delicato è *perché* è spento: deve esserlo perché non c'è niente da salvare, non perché l'importo è invalido — e la differenza si vede dall'assenza del messaggio rosso. Poi cambiare la **descrizione** e premere «Salva»: deve riuscire, e comparire «Salvata.» sopra i pulsanti. È il caso che prima del diff era impossibile: la spesa non era modificabile da nessuno.

**2. La pagina non deve nascere «modificata» — il rovescio della prova 1, e la trappola del mandato.** Sulla stessa spesa da `1284,50`, appena aperta e **senza toccare niente**, premere «Chiudi».
*Accettazione*: si esce **subito**, senza la domanda «Hai modifiche non salvate…». Se la domanda comparisse, `Cambiata` è vera all'apertura e la riga `:213` non sta facendo il suo lavoro — sarebbe il difetto peggiore di quello corretto. Ripetere con una spesa **sotto** i mille euro e con una a decimali tondi (`1284,00`): sono i tre casi che il test `TestoDigitabile_non_dipende_dalla_scala_del_decimal` pinna in memoria, ma solo il browser prova che il valore che arriva dal database ha davvero la scala che il test assume.

**3. Il round-trip dopo un salvataggio riuscito** (`:320`). Su una spesa qualunque, scrivere l'importo **col punto del tastierino** — `1500.75` — e salvare.
*Accettazione*: dopo «Salvata.» il campo mostra `1500,75` **con la virgola**, «Salva» torna spento, e premendo «Chiudi» non arriva nessuna domanda. Provare anche digitando un intero secco, `90`: dopo il salvataggio il campo deve mostrare `90,00` e «Salva» deve essere spento — i due decimali comparsi non sono una modifica.

**4. Il ramo «Ricarica» del conflitto** (`:351`), l'unico call-site che nessuna delle altre prove attraversa. Due schede sulla stessa spesa **sopra i mille euro**, entrambe con permesso di intervenire. Nella prima cambiare l'importo e salvare; nella seconda cambiare la descrizione e premere «Salva», poi «Ricarica» sulla scheda di conflitto.
*Accettazione*: compare «Caricata la versione più recente.» sopra i pulsanti, il campo importo mostra il valore scritto dalla prima scheda **senza punto delle migliaia e senza messaggio d'errore**, e «Salva» è spento. Se comparisse l'errore, `:351` è rimasto indietro.

**5. La differenza visibile a chi non può intervenire — v. `FUORI SCOPE` 2.** Con due account dello stesso spazio, dove chi guarda non è il pagante e non possiede lo spazio: aprire una spesa altrui **sopra i mille euro**.
*Accettazione*: i campi sono grigi, c'è la riga «Questa spesa l'ha segnata qualcun altro…», e il campo importo mostra `1284,50` — **senza** il punto delle migliaia, a differenza dell'elenco `/expenses` che continua a mostrare `1.284,50 €`. È il comportamento atteso, non un difetto: si guarda per decidere se quella differenza è accettabile per l'occhio, e la decisione è dell'utente.
