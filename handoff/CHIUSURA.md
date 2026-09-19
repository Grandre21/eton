# CHIUSURA — il mandato

*Scritto dal capo il **19 settembre 2026**, a collaudo completo e server fermo. Il rapporto lo
scrive la sessione di chiusura, in testa a questo file, sopra questo mandato.*

## CHI SEI

Sei la **sessione di chiusura** del goal «chiudere i punti rimasti aperti dal ciclo dei sedici
rilievi». Non sei un quarto revisore e non sei un riassuntore.

**Leggi `~/.claude/architettura-sessioni.md` come prima azione**, sezione «La sessione di
chiusura»: contiene il formato del tuo rapporto, che non conosci altrimenti, e il confine di cosa
puoi correggere.

## L'OBIETTIVO, VERBATIM

> «vorrei chiudere tutti i punti rimanenti in questa sessione»
> — utente, 19 settembre 2026

**Lo leggi da `handoff/PIANO.md`, dove sta verbatim, non da questa copia.** La glossa che lo
decompone sta lì sotto, e con essa la tabella delle **quattro crescite datate**: l'obiettivo è
passato da 21 a **27 clausole**, e ogni crescita ha una riga con chi l'ha decisa.

⚠️ **Conti contro ventisette.** Il numero è scritto due volte nel piano proprio perché tu possa
contarlo senza ricostruirlo.

## COSA DEVI FARE, nell'ordine

1. **Copertura dell'obiettivo.** Confronta le 27 clausole con l'unione di ciò che i **sette
   resoconti** dichiarano, ed elenca **ciò che nessuno ha fatto e nessuno ha detto di non aver
   fatto**. Ogni altro anello verifica i claim *fatti*; tu sei il solo che cerca quelli **non
   fatti**, e il capo — che ha disegnato la partizione — è il candidato peggiore a trovarne i buchi.
   Nello stesso passaggio confronta la colonna di stato della `PARTIZIONE` con gli `ESITO` reali
   dei resoconti: una divergenza dice che il capo ha smesso di tenere il piano al passo.
2. **Convergenza dei contratti.** Apri il codice e verifica che produttore e consumatore siano
   atterrati sulla **stessa firma reale**, non solo che entrambi l'abbiano dichiarata. I contratti
   di questo goal, e dove guardare:
   - `Shared/PaginaEditor.cs` — **dichiarato invariante da tre unità** (03, 04, 06) e mai aperto in
     scrittura da nessuna. Verificalo sul diff complessivo, non sui resoconti.
   - `.solo-lettori` e `.voce-piede` fra le unità 01 e 02.
   - `button.pastiglia` fra le unità 01 e 07 — la 07 ha **revocato** un contratto della 01
     («sei l'unico a toccare `app.css`»), e la revoca è dichiarata nel piano.
   - Le tre firme di `Denaro` fra le unità 04 e 03.
   - Il tipo di esito OAuth e la sua enumerazione, unità 05 — **contratto di sicurezza**: la
     diagnostica non deve raggiungere lo schermo in nessun ramo.
   - `AuthStateService` e i due servizi nuovi dell'unità 06.
3. **`bug-hunter` sul diff combinato**, con i call-site di cucitura, più i gate rieseguiti sullo
   stato finale.

## LO STATO DI FATTO

- **Sette unità su sette `FATTO`.** Nessuna `PARZIALE`, nessuna `BLOCKED`. I resoconti stanno in
  `handoff/NN-slug/resoconto.md`.
- **`main` a `a159aef` e oltre**, tutto pushato su `origin`. Ogni unità è entrata con un merge del
  proprio branch, e worktree e branch sono stati rimossi.
- **Gate sullo stato finale**, rieseguiti dal capo su albero pulito:
  `dotnet build Eton.sln -warnaserror --no-incremental` → **0 avvisi, 0 errori**;
  `dotnet test Eton.sln` → **310/310** (erano 287 all'apertura).
- **Collaudo completo**, quattro passi, esiti in `handoff/collaudo/`. Server **fermo**, porta
  libera.

## COSA NON DEVI FARE

1. ⚠️ **Non correggere i dieci rilievi aperti dal collaudo** — i cinque di `ui-critic` e i cinque
   della ricognizione. **Non sono stati dimenticati: sono stati lasciati per decisione**, scritta
   in `DECISIONI` del piano. L'obiettivo è cresciuto quattro volte e il capo si è dato la regola
   che dalla quinta in poi le voci nuove vanno nel rapporto. Il tuo compito su di essi è
   **aggregarli in `FUORI SCOPE`**, non chiuderli.
2. **Non avviare il server e non aprire il browser.** Il collaudo è fatto e il server è fermo;
   riaprirlo lascerebbe un processo vivo sulla 5000 che il prossimo ciclo scambierebbe per suo.
3. **Non toccare `supabase/migrations/`.** Nessuna unità l'ha toccata, e un file nuovo lì dentro
   finisce dentro il parser di `PrivilegiInsertTests`, che può diventare rosso senza che nessuno
   abbia toccato il test.
4. **Tutto ciò che tocca un contratto fra unità torna al capo**, e non lo risolvi tu. È il confine
   rigido che l'architettura ti dà.

## COSA PUOI CORREGGERE

Ciò che il `bug-hunter` finale trova **dentro** il perimetro già toccato, dispacciando
`implementer` col protocollo normale. Lasciare fino al mattino un difetto già noto butta via le ore
di lavoro che l'architettura serviva a guadagnare.

## POI

Archivia in `storico/handoff/` **solo** le unità `FATTO` — che sono tutte e sette — con `git mv`,
senza chiedere: è l'eccezione dichiarata nel `CLAUDE.md`. **Non archiviare** `handoff/collaudo/`
né questo file: il rapporto non si archivia finché l'utente non l'ha letto.

Lascia il rapporto **in testa a questo file**, nel formato che `architettura-sessioni.md`
prescrive. `COPERTURA:` porta **tre numeri** che devono sommare a 27, e **non ammette il valore
`completa`**.

## TRE COSE CHE VANNO NEL RAPPORTO E CHE NESSUN RESOCONTO CONTIENE

1. **Tre dati di prova restano sul database di sviluppo**, perché i dialoghi nativi del browser
   hanno impedito agli agenti di eliminarli: la spesa «COLLAUDO GIRO B» da 1.284,50 €, lo spazio
   «COLLAUDO GIRO B TEST», e la collezione «Collaudo Ricognizione» con due elementi. **Servono tre
   gesti dell'utente.**
2. **La voce 13 non si chiude in questo goal e si sapeva dall'apertura**: il banner di
   aggiornamento in condizioni vere si vede **solo sul sito pubblicato**. Va riportata come
   **rinviata al primo rilascio**, con scritta la prova da eseguire — non come coperta.
3. **Un'osservazione che il codice letto non spiega**, e che non va dichiarata risolta: un dialogo
   nativo comparso premendo «Elimina» su una spesa, dove il percorso passa da un componente in
   pagina e da un `Esci()` che disarma la guardia. Non riprovata, perché riprovare significa
   bloccare di nuovo il browser.
