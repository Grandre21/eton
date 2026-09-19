# CHIUSURA — il rapporto

*Scritto dalla sessione di chiusura il **10 settembre 2026**, a collaudo completo. Il mandato del
4 settembre e il campo `ESITO DEL COLLAUDO` restano sotto, invariati: servono a chi legge questo
rapporto.*

*Le cartelle delle unità le ha archiviate questa stessa sessione: dove qui o nel mandato si legge
`handoff/NN-slug/`, oggi il percorso è `storico/handoff/NN-slug/`.*

```
CHIUSURA: 15 unità — FATTO 15 · PARZIALE 0 · BLOCKED 0
COPERTURA: 20 clausole — coperte 19 · scoperte 1 · rinviate 0
           «correggere tutto» → la pendenza «Il medaglione 📋» in fondo a rilievi.md: nessuna
           unità l'ha avuta in mandato, nessuna l'ha fatta, nessuna ha detto di non averla fatta
CONTRATTI: PaginaEditor — convergente sui quattro editor · PaginaRegistro — convergente sulle tre
           derivate · OAuthRifiuto — il tipo tiene: nessun percorso porta error_description a schermo
COMBINATO: 2 rilievi — 1 corretto (3cf84ff, sette righe di commento) · 1 fuori scope, all'utente
GATE: dotnet build -warnaserror --no-incremental → 0 avvisi, 0 errori · dotnet test → 287/287
FUORI SCOPE: 18 voci — 5 di ui-critic, 1 del bug-hunter finale, 9 aperte dalle unità (6 difetti,
             3 verifiche mancanti), 3 decisioni lasciate aperte
ARCHIVIATO: 15 cartelle, da handoff/02-… a handoff/16-…, spostate in storico/handoff/ con git mv
```

## CHIUSURA — le quindici unità

Le cartelle-unità sono quindici, `02`…`16`. Tutti e quindici i resoconti portano `ESITO: FATTO`, e
la colonna di stato della `PARTIZIONE` di `PIANO.md` — quindici righe — dice lo stesso: **nessuna
divergenza fra piano e disco**. Una sola incongruenza, di conteggio e non di stato: la tabella di
`PROSSIMA AZIONE` scrive «**16 unità**», ma sedici sono i *rilievi*; le unità sono quindici.

`01-ricognizione-ui/` è la fonte dei rilievi e `17-collaudo/` è il collaudo: nessuna delle due è
un'unità.

## COPERTURA — l'obiettivo decomposto

L'obiettivo, verbatim da `PIANO.md`: «io vorrei correggere tutto in sequenza nel prossimo lavoro.»
— utente, 3 settembre 2026. «Tutto» sono i sedici rilievi 0-15 di `01-ricognizione-ui/rilievi.md`
più le tre pendenze in fondo a quel file: è la glossa di `PIANO.md`, confermata dal mandato qui
sotto. Venti clausole: diciannove cose da correggere e il vincolo «in sequenza».

| # | Clausola | Chi | Prova sullo stato finale | Esito |
|---|---|---|---|---|
| r0 | creare una collezione è impossibile | 02 | migrazione eseguita dall'utente il 3 set; giro A PASSA | coperta |
| r1 | il lavoro non salvato si perde | 03, 04, 06, 07 | contratto convergente (sotto); giro B 19/27 | coperta ⚠ |
| r2 | l'esito compare dove non guardi | 03-07 | giro B, NoteEdit 9 e ItemEdit 1-2 | coperta |
| r3 | il messaggio d'errore è JSON grezzo | 05, 10, 13, 14, 15 | `grep` dei sink rifatto oggi → 0 righe; 69 `.Message` su 69 dentro `Console.Error.WriteLine`; 71 `catch` su 71 chiamano `ex`; giro C: nessun JSON | coperta |
| r4 | l'avviso di aggiornamento non si rimanda | 11 | «Più tardi» in `index.html`; giro D | coperta |
| r5 | bersagli delle categorie a 22px | 11 | giro D: 48px nei tre posti | coperta |
| r6 | l'anteprima salta di 358px | 11 | giro D: ~7px | coperta |
| r7 | «Gestisci questo spazio» non gestisce niente | 08 | `Home.razor`: `@if (!Spazi.Attivo.IsPersonal)` | coperta |
| r8 | «Elimina» a 55px da «Chiudi» | 11 | giro D: 494px, i quattro controesempi intatti | coperta |
| r9 | pulsante spento che non dice cosa manca | 05, 13, 11 | giri C e D | coperta |
| r10 | l'icona è un campo di testo libero | 05 | tavolozza; il bug-hunter finale l'ha confrontata byte per byte coi modelli | coperta |
| r11 | «Spesa 100%» non dice di essere una categoria | 08 | giro C: «Per categoria: Spesa 100%» | coperta |
| r12 | due schermate non si spiegano | 03-08 | testate e «?»; giri B e C | coperta |
| r13 | lo stato vuoto invita ad agire lontano | 09, 13 | pulsante dentro `.vuoto` su `Notes`, `Collections`, `CollectionDetail` | coperta — mai vista dal vivo: serve uno spazio vuoto |
| r14 | selettore spazio e «Profilo» accavallati | 11 | giro D: `bottom` identico, scarto 0 | coperta ⚠ |
| r15 | logout non riuscito | — | ritirato dall'utente; il testo di `rilievi.md` è aggiornato, come la decisione chiedeva | coperta per decisione |
| P1 | il segnaposto `&#10;` | 03 | `&#10;` non compare in nessun `.razor`, `.cs`, `.html`, `.js`; giro B, prova 10 | coperta |
| P2 | **il medaglione 📋** | **nessuno** | `.icona-collezione` è ancora `{ flex: none; font-size: 1.4rem; line-height: 1; }` (`app.css:1308`): nessun contenitore | **scoperta** |
| P3 | «Il database ha rifiutato…» | 05, 10, 13 | la stringa non compare più in nessun sorgente | coperta |
| seq | «in sequenza» | capo | un'unità per volta, e nel punto in cui i tempi si avvicinano (11, secondo giro, commit 01:19 — 16, commit 01:49) il `git status` riportato dall'11 non mostra nessun file della 16 | coperta |

### La scoperta, e perché è sfuggita

La pendenza è definita in `caa5aef`, il piano del 26 agosto: «Il medaglione 📋 delle collezioni —
`Pages/Collections.razor` · `Pages/Home.razor` — **`.icona-collezione` da portare a un contenitore
40×40**. Ereditato da un ciclo precedente.» Oggi la classe compare anche in
`Pages/CollectionDetail.razor:40`.

In tutto `PIANO.md` l'unico punto che la assegnava è la riga «Copertura dei rilievi» della
`PARTIZIONE`: «10→r4, r5, r6, r14, **P3**». In quella riga «10» è il foglio di stile **nella
numerazione precedente alla scissione dell'unità 04**. Dopo la scissione il foglio di stile è
diventato l'11, la riga non è stata rinumerata — è la stessa famiglia di residui che il piano ha già
dovuto correggere due volte altrove —, e il mandato dell'unità 11 elenca otto voci senza il
medaglione. Il `grep` di `medaglione`, `icona-collezione` e `📋` su tutti i mandati e i resoconti non
trova niente. Il mandato di questa chiusura l'aveva previsto: «sono le pendenze il punto cieco più
probabile».

**Non l'ho corretto**, e non per prudenza generica. È una modifica di `app.css` che il protocollo
vuole preceduta da un `PIANO-DESIGN` e seguita da `live-testing` e `ui-critic` (§0 e §7), cioè dal
server di sviluppo e dal browser, che per questa sessione erano esclusi. Consegnare una modifica
d'interfaccia senza la sua prova è ciò che il §7 esiste per impedire. E «contenitore 40×40» è una
misura, non un disegno. È il primo candidato per il prossimo obiettivo: v. `DA PORTARE`, punto 7.

### Le due riserve (⚠)

- **r1** è chiuso nel codice e nel contratto, ma la metà che *chiede* — il dialogo nativo su
  «Chiudi», su Indietro e su F5 con modifiche non salvate — un agente non la può provare: cinque
  prove su ventisette del giro B. All'utente costa un minuto: `DA PORTARE`, punto 4.
- **r14**: la ricognizione misurava i riquadri (select fino a x=179, link da x=187) e concludeva
  «non si sovrappongono»; il difetto dichiarato erano le basi, 820 contro 838, ed è chiuso. Ma il
  rilievo 2 di `ui-critic` del 10 settembre ha misurato che l'**icona** di «Profilo» esce dal proprio
  riquadro e si sovrappone al selettore per **9px**, in orizzontale. La premessa del rilievo 14 era
  falsa: l'impressione di accavallamento che descriveva resta finché il rilievo 2 di `ui-critic` non
  è deciso.

## CONTRATTI — aperti sul codice, non letti nei resoconti

**`Shared/PaginaEditor.cs` — convergente.** La firma reale è quella della sezione `CONTRATTO` di
`PIANO.md`: `public abstract class PaginaEditor : ComponentBase, IDisposable` (`:29`), `Navigation`
e `JS` `private` (`:34-35`), `protected abstract bool Cambiata { get; }` (`:48`), `protected void
Esci(string uri, bool replace = false)` (`:54`), `protected async Task
GuardaUscita(LocationChangingContext ctx)` (`:76`), `public virtual void Dispose()` (`:87`). I quattro
consumatori ci sono atterrati allo stesso modo:

| | `@inherits` | `<NavigationLock …>`, identico | `override bool Cambiata` | `Esci` dopo Crea / Elimina | «Chiudi» con `href` null durante `occupato` |
|---|---|---|---|---|---|
| `NoteEdit` | `:4` | `:39` | `:153` | `:293` / `:373` | `:114` |
| `CollectionEdit` | `:5` | `:40` | `:338` | `:645` / `:732` | `:263` |
| `ItemEdit` | `:5` | `:47` | `:193` | `:393` / `:475` | `:135` |
| `SpesaEdit` | `:4` | `:39` | `:226` | — / `:428` (non crea) | `:154` |

Nessun `NavigateTo` grezzo, nessun `@inject NavigationManager`, nessun override di `Dispose` nei
quattro file.

**`Shared/PaginaRegistro.cs` — convergente.** L'unità 14 ha cambiato tre stringhe e aggiunto tre
righe di console, **nessun membro**: le righe non di commento del suo diff sono sei, tutte `errore =
…` o `Console.Error.WriteLine`. Le tre derivate sovrascrivono `NomePlurale` e `Leggi` e chiamano
`SegnalaNonLetti(ex)` una volta ciascuna (`Notes.razor:121`, `Collections.razor:115`,
`Spese.razor:403`); `Spese` è l'unica a usare `ScartaCambioSpazio`, `PrimaDiRicaricare` e l'override
di `Riprova`, come la classe dichiara di sé.

**`OAuthRifiuto` (`Services/OAuthCallback.cs:4`) — il tipo tiene.** Il record è
`OAuthCallbackEsito(string? Codice, OAuthRifiuto Errore, string? Diagnostica)` (`:20`).
`Diagnostica`, l'unico campo che porta `error_description`, ha un solo lettore fuori dai test:
`Console.Error.WriteLine` a `SupabaseService.cs:113`. `ErroreAccesso`, l'unico campo che arriva a
schermo (`Benvenuto.razor:211`), riceve solo costanti letterali — `SupabaseService.cs:117`, `:118`,
`:121`, `:195`, `:203`, `:222` — più `null` a `:171`. Un `?error_description=` senza `error` non apre
nessun ramo (`OAuthCallback.cs:44`).

## COMBINATO — il bug-hunter sul diff `dc4ca55..c08bbb3`

Diff del codice: 28 file, +1469/−232. Dato **per file**, in cinque gruppi, ciascuno coi call-site di
cucitura che il mandato elenca.

    review:
      bug-hunter  contratto-editor     RILIEVI: 1
      bug-hunter  collezione-conferma  RILIEVI: 1
      bug-hunter  registri-pagine      RILIEVI: 0
      bug-hunter  autenticazione       RILIEVI: 0
      bug-hunter  sql-css-banner       RILIEVI: 0
      checker     VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
      checker     VERDETTI: fondati 0 · infondati 0 · fuori scope 1 · non verificabili 0
      correzione  review: nessuna — commento: Shared/PaginaEditor.cs
      checker     VERDETTI: risolti 1 · non risolti 0 · non verificabili 0
      checker     VERDETTI: risolti 1 · non risolti 0 · non verificabili 0

Il report del gruppo `sql-css-banner` ha messo la riga `RILIEVI: 0` al secondo posto, dopo una frase:
il conteggio c'è, la forma non è quella prescritta.

1. **`Shared/PaginaEditor.cs:56-62` — fondato → corretto, commit `3cf84ff`.** Il commento di `Esci` taceva il
   limite che `PIANO.md` — sezione `CONTRATTO`, «Una crepa nota di `smontata`, da annotare nel
   commento e non da correggere ora» — aveva deciso di dichiarare: sull'istanza riusata `smontata`
   non si alza, e un `Esci` tardivo porta via dalla pagina appena aperta. Dopo la 03 il file era nel
   `NON TOCCARE` di tutte le unità, e nessuna l'ha scritto. Sette righe di commento, zero di codice:
   eseguire una decisione che l'autore del contratto aveva già preso, non prenderne una nuova. La
   prima stesura, dettata da me, diceva «porta sull'elenco» anche per `Crea()`, che invece porta
   alla cosa appena creata; l'ho trovata io rileggendo il diff, non il checker, e l'ho fatta
   correggere. `verificato: risolto — Shared/PaginaEditor.cs:64-69`, e dopo il ritocco di nuovo
   `risolto — Shared/PaginaEditor.cs:64-69`, sui quattro editor.
2. **`Pages/CollectionEdit.razor:559-564` — fuori scope, all'utente.** `Rimuovi()` non azzera
   `erroriValidazione`: se resta un'altra modifica in sospeso, il riquadro rosso continua a citare un
   campo appena tolto, finché non si ripreme «Salva». È un'istanza della scelta dichiarata dall'unità
   05 — commento a `:209-216`, «il verdetto è dell'ultimo tentativo» —, e la correzione proposta,
   azzerare tutto in `Rimuovi`, nasconderebbe errori ancora veri sugli altri campi. Ho riaperto io la
   citazione del checker: regge. È una decisione: `DA PORTARE`, punto 8.

Nessun rilievo toccava sicurezza, dati o concorrenza, e non c'è nessun infondato da campionare.

## GATE — sullo stato finale, dopo la correzione

- `dotnet build Eton.sln -warnaserror --no-incremental` → **Avvisi: 0, Errori: 0**
- `dotnet test Eton.sln --no-build` → **Superati: 287, Non superati: 0, Totale: 287**: il numero che
  dichiarava il resoconto 16 (273 + 14).

Il server di sviluppo non è stato avviato: il collaudo è completo e non è stato rifatto.

## FUORI SCOPE — aggregato

**Da `ui-critic`, 10 settembre** — `docs/superpowers/specs/2026-09-10-rilievi-ui-critic.md`. Cinque
rilievi preesistenti, **non corretti per istruzione**: sono decisioni di progetto.

1. Le micro-etichette non raggiungono il contrasto minimo: `--testo-fioco: #6e6e6e` (`app.css:97`),
   da 3,38 a 4,12:1 contro 4,5:1. La scelta è fra `#8a8a8a` e `#808080`.
2. L'icona di «Profilo» si sovrappone al selettore di spazio per 9px (v. la riserva su r14). **La
   correzione (a) proposta dall'agente è sbagliata**: `display: none` sull'etichetta toglie al link
   il suo unico nome accessibile. Quella giusta usa `.solo-lettori`, oppure fa crescere la scatola.
3. La colonna del contenuto si sposta di 7,6px fra le pagine che scorrono e quelle che non scorrono.
4. Due azioni primarie nella stessa vista: nell'editor di elemento (due «Salva») e in Home.
5. Quattro controlli sotto i 48px di tocco: «Nessun voto» a 22px, le frecce del mese, «Tutte»,
   «Profilo».

**Dal bug-hunter finale.**

6. Il riquadro di validazione di `CollectionEdit` che sopravvive a `Rimuovi()`: v. COMBINATO, 2.

**Dalle unità, ancora aperti sullo stato finale** — riverificati oggi, non ricopiati. Da 7 a 11 e
il 15 sono difetti; 12, 13 e 14 sono verifiche che nessuno ha potuto fare:

7. `SupabaseService.cs:195`, «Accesso non completato: riprova dall'inizio.», e `:203`, «… sessione
   senza utente.», dicono il fatto e nient'altro; la seconda è gergo interno (resoconto 15, FUORI
   SCOPE 2).
8. Tre chiamate a `localStorage` via JS fuori da ogni `try` nel flusso d'accesso: `_pkce.Leggi()` a
   `:192` e `_pkce.Cancella()` nel `finally` a `:227`, le due che le unità 15 e 16 hanno nominato, più
   una terza che **nessuno ha nominato**, `_pkce.Cancella()` nel ramo d'errore del bootstrap a `:124`
   (preesistente). Con `localStorage` bloccato si finisce sulla barra d'errore di Blazor.
9. La barra gialla generica di Blazor, «Si è verificato un errore imprevisto», è comparsa nel giro C
   accanto al messaggio tradotto in tutte e tre le prove a rete bloccata (`C-esito.md`, ALTRO CHE HAI
   VISTO). Non è stata indagata: può essere un effetto della simulazione — `window.fetch` sovrascritto
   per l'intera pagina — oppure una chiamata non protetta. Non è JSON grezzo, ma è testo tecnico a
   schermo.
10. Un errore di validazione resta a schermo dopo aver corretto il campo senza ripremere «Salva»:
    visto dal giro B su `ItemEdit`, probabilmente comune ai quattro editor, preesistente. È la stessa
    famiglia del punto 6.
11. `@using Eton.Services` è ridondante, perché sta già in `_Imports.razor:9`, e in **tre** file, non
    due: `CollectionEdit.razor:4`, `ItemEdit.razor:4`, `CollectionDetail.razor:3`. Preesistente,
    cosmetico.
12. La prova 8b dell'unità 11 — a 360px «Sì, elimina» e «Annulla» restano sulla stessa riga? — non
    compare nel riepilogo del giro D, e il `D-esito.md` che il brief prescriveva non esiste: il
    numero che decide quella voce non c'è ancora.
13. Il banner di aggiornamento in condizioni vere — worker in attesa, «Più tardi», riapparizione
    all'avvio — si vede solo sul sito pubblicato: da guardare al primo rilascio (resoconto 11).
14. L'invariante «le prime tre emoji della tavolozza sono quelle dei modelli» è fissata da un
    commento e non da un test, perché `TavolozzaIcone` è `private` in una pagina (resoconto 05). Oggi
    è vera.
15. Cambiando spazio, la Home si ridisegna solo dopo il giro di rete: nome, sottotitolo e link dello
    spazio precedente restano a schermo per quel tempo (resoconto 08, preesistente).

**Decisioni lasciate aperte dalle unità** — non sono difetti, ma nessuno le ha prese:

16. Rinominare `Denaro.Testo`, perché nessuna delle due funzioni sia «il default» (resoconto 12).
17. Nel campo importo di chi non può intervenire compare `1284,50`, nell'elenco `1.284,50 €`
    (resoconto 12): se la differenza è accettabile lo decide l'utente.
18. L'helper `FraseRifiuto` col suo test su `Enum.GetValues`: rimandato dal capo il 4 settembre, con
    la specifica giusta già scritta in `DECISIONI` — «ogni valore dichiarato mappa a una frase
    distinta dalla generica».

## ARCHIVIATO

Con `git mv`, senza chiedere, come prescrive l'eccezione del `CLAUDE.md` per le unità `FATTO`:
`02-collezioni-insert` · `03-contratto-editor` · `04-collezione-contratto` · `05-collezione-rilievi`
· `06-elemento-contratto` · `07-spesa-contratto` · `08-home-spazio-profilo` · `09-registri-vuoti` ·
`10-recensioni-errori` · `11-foglio-di-stile` · `12-importo-digitabile` · `13-errori-tradotti` ·
`14-errori-rimasti` · `15-accesso-non-riuscito` · `16-oauth-insieme-chiuso`, da `handoff/` a
`storico/handoff/`, con lo stesso nome.

Restano in `handoff/`: `PIANO.md`; questo `CHIUSURA.md`, che non si archivia finché l'utente non
l'ha letto; `01-ricognizione-ui/`, la fonte dei rilievi; `17-collaudo/` e `server.md`, che non sono
unità e contengono gli esiti a cui questo rapporto rimanda.

## DA PORTARE ALL'UTENTE

I sei punti del mandato, aggiornati, più quattro che nascono da questa chiusura. Il messaggio di
passaggio e la riga `PROSSIMA AZIONE: GOAL CHIUSO` in `PIANO.md` spettano al capo: questa sessione
non li scrive.

1. **La migrazione SQL delle spese ricorrenti** la esegue solo l'utente, in produzione. Non è ancora
   scritta: dal 10 settembre le spese sono la fase 2 del piano di prodotto.
2. **`profiles.display_name` e `avatar_url` restano quelli del primo accesso**: chi cambia nome o
   foto su Google resta com'era per gli altri membri (resoconto 08, FUORI SCOPE 2).
3. **La ricognizione di `/collections/{id}` e dell'area voti e recensioni** non è mai stata fatta.
   `ui-critic` il 10 settembre ci è passato per l'aspetto, non per gli attriti.
4. **La guardia d'uscita, un minuto.** Aprire una nota, scrivere senza salvare, poi (a) premere
   Indietro, (b) premere F5. Tutti e due devono chiedere conferma, e su «Annulla» si resta col testo
   intatto. È l'unica correzione grave verificata solo leggendo.
5. **I dati di collaudo in produzione — e il punto 5 del mandato va corretto.** Dice «nessuna
   spesa», ma nello stesso commit (`e2b7a69`) `17-collaudo/C-esito.md` dichiara di aver lasciato la
   spesa **«COLLAUDO 4 SET» da 1.284,50 €, del 4 settembre**, nello spazio Personale; il giro D dice
   di aver rimosso una «spesa di prova» senza dire quale. Salvo rimozioni non registrate, in
   produzione ci sono: la collezione «COLLAUDO 4 SET» (`21cb3ec5-1286-4026-8d0f-2736d00b863c`, con le
   opzioni del campo «Formato» ora nell'ordine 33 · 75 · 50 cl), l'elemento omonimo, la nota omonima,
   quella spesa, e la spesa «PROVA AGENTE» da 12,50 € del 20 agosto. Una guardata a `/expenses` di
   settembre lo chiarisce.
6. **Le tre prove OAuth e l'annullamento vero su Google**: ricetta e URL in
   `storico/handoff/16-oauth-insieme-chiuso/resoconto.md`, sezione `DA PROVARE NEL BROWSER`. La
   prova 4 è l'unica che chiude la confidenza media di `tech-advisor` sulla tabella dei codici.
7. **Il medaglione 📋**, la clausola scoperta: decidere se entra nel prossimo obiettivo e con quale
   aspetto.
8. **Il riquadro di validazione che sopravvive a `Rimuovi()`** (COMBINATO, 2): accettarlo come
   verdetto dell'ultimo tentativo, oppure chiedere una forma che non nasconda gli errori degli altri
   campi.
9. **I cinque rilievi di `ui-critic`**: quattro sono decisioni (contrasto, colonna, primari, tocco),
   e il quinto ha una correzione proposta sbagliata (il 2).
10. **Un secondo spazio**, se si vuole collaudare ciò che oggi non si può: gli stati vuoti dei
    registri, il pannello dello spazio condiviso, il link «Gestisci questo spazio», i permessi fra
    due membri. È un dato vero in produzione, quindi decide l'utente.

---

# Il prompt della sessione di chiusura

Scritto dal capo il **4 settembre 2026**, mentre l'unità 16 girava — cioè nel momento in cui
`~/.claude/architettura-sessioni.md` era ancora nel suo contesto. Scriverlo dopo il collaudo
avrebbe voluto dire riscriverlo a memoria.

**Non si apre questa sessione prima che il collaudo sia finito.** L'ordine è: unità 16 →
quattro giri di `live-testing` → chiusura. Una chiusura che gira prima del browser dichiara
«copertura completa» su un lavoro di cui nessuno ha ancora visto l'effetto.

**Prima di aprirla, il capo riempie il campo `ESITO DEL COLLAUDO` in fondo a questo file.**
È l'unica parte che non si può scrivere in anticipo, ed è quella che dice alla chiusura se
sta chiudendo un lavoro riuscito o uno con difetti noti.

---

## Il prompt da passare (`claude -p`, ASCII puro nell'argomento)

> Sei la **sessione di chiusura** del lavoro sui sedici rilievi della ricognizione UI di
> Eton. Non sei un quarto revisore e non sei un riassuntore.
>
> **Leggi per primo `~/.claude/architettura-sessioni.md`**, sezione «La sessione di
> chiusura»: contiene il tuo mandato completo e il formato del rapporto, che non conosci
> altrimenti. Poi leggi `handoff/CHIUSURA.md`, che è questo file, e `handoff/PIANO.md`.
>
> L'obiettivo dell'utente sta **verbatim** in `PIANO.md`, campo `OBIETTIVO`. Confrontalo con
> l'unione di ciò che i resoconti in `handoff/NN-*/resoconto.md` dichiarano, e **elenca ciò
> che nessuno ha fatto e nessuno ha detto di non aver fatto**.
>
> Il resoconto in `handoff/16-oauth-insieme-chiuso/resoconto.md` è l'ultimo del lavoro.
>
> **Non committare i tuoi commit di correzione senza dirlo nel rapporto.** L'utente non è
> raggiungibile: qualunque domanda va nel rapporto, non in chat.

## Le tre verifiche, e cosa sono in questo lavoro specifico

### 1. Copertura dell'obiettivo

L'obiettivo era «correggere tutto in sequenza». «Tutto» sono i sedici rilievi di
`handoff/01-ricognizione-ui/rilievi.md` (0-15) **più le tre pendenze minori in fondo a quel
file** — e sono le pendenze il punto cieco più probabile, perché nessuna tabella di questo
piano le ha mai avute come riga propria.

La `MAPPA RILIEVO → UNITÀ` di `PIANO.md` dice chi possiede cosa. **Confrontala con gli
`ESITO` reali dei resoconti**: una divergenza fra la colonna di stato e il disco è un difetto
rilevabile senza giudizio, e dice che il capo ha smesso di tenere il piano al passo.

Due cose sono chiuse **per decisione, non per esecuzione**, e non vanno segnalate come buchi:

- il **rilievo 15** (logout non riuscito) è stato **ritirato**: non era un difetto;
- l'**ottava voce dell'unità 11** (rinomina `.scelta-categoria` → `.scelta-pastiglie`) non si
  fa. Entrambe le motivazioni stanno in `DECISIONI`.

Il **rilievo 3** merita un occhio in più: è stato creduto chiuso **tre volte** prima di
esserlo davvero, e ogni volta la ragione era la stessa — una mappa *file → unità* dichiara un
rilievo chiuso quando è chiuso nel perimetro di chi l'ha toccato. L'unità 15 l'ha chiuso con
una prova (`grep` dei sink a zero righe, non dei sorgenti). **Rifai quel `grep`**: il modo di
verificarlo sta nel suo resoconto, e se qualche unità successiva ha reintrodotto un
`ex.Message` a schermo, questo è l'unico anello che può accorgersene.

### 2. Convergenza dei contratti

Due contratti hanno attraversato più unità, e vanno verificati **aprendo il codice**, non
leggendo i resoconti:

- **`Shared/PaginaEditor.cs`** — prodotto dall'unità 03, adottato da 04 (`CollectionEdit`),
  06 (`ItemEdit`), 07 (`SpesaEdit`) e dalla 03 stessa (`NoteEdit`). Quattro consumatori: la
  firma reale su cui sono atterrati deve essere **la stessa**. Il contratto testuale sta in
  `PIANO.md`, sezione `CONTRATTO — Shared/PaginaEditor.cs`.
- **`Shared/PaginaRegistro.cs`** — preesistente, ma l'unità 14 l'ha toccato mentre 09 e 13
  toccavano i suoi consumatori.

Il terzo è nuovo e ha un solo consumatore, quindi è meno esposto ma più delicato:
l'**`enum`** di `Services/OAuthCallback.cs` prodotto dall'unità 16 e consumato da
`Services/SupabaseService.cs`. Qui la verifica non è «convergono», è **«il tipo tiene»**:
controlla che nessun percorso riporti la stringa `error_description` verso una schermata.

### 3. `bug-hunter` sul diff combinato

Il diff da dargli è `git diff dc4ca55..HEAD` — `dc4ca55` è l'ultimo commit prima che questo
lavoro cominciasse. È grande: **passaglielo per file, non in blocco**, e dagli in più i
**call-site di cucitura**, cioè i punti in cui il lavoro di due unità diverse si tocca:

- le quattro pagine editor che consumano `PaginaEditor`;
- `Pages/Notes.razor` e `Pages/Collections.razor`, che consumano `PaginaRegistro` toccato
  dalla 14;
- `Shared/ConfermaAzione.razor` e i suoi quattro call-site, dove l'unità 11 ha aggiunto una
  classe che il foglio di stile usa;
- `Services/SupabaseService.cs`, toccato dalle unità 15 e 16 a poche righe di distanza.

Poi rieseguì i gate del progetto sullo stato finale: `dotnet build -warnaserror` (0 avvisi) e
`dotnet test`. **Erano 273 test prima dell'unità 16**, che ne aggiunge e ne riscrive quattro:
il numero atteso lo dichiara il suo resoconto.

## Il confine: cosa correggi e cosa no

**Correggi**, dispacciando `implementer` col protocollo normale, tutto ciò che è interno a una
sola unità. Lasciare fino al mattino un difetto già noto butta via le ore che questa
architettura serviva a guadagnare.

**Non correggi, e torna al capo**, tutto ciò che tocca un contratto fra unità: se
`PaginaEditor` è divergente su una delle quattro pagine, la decisione è di chi ha scritto il
contratto, non tua.

## L'archiviazione

Solo le unità con esito **`FATTO`**, secondo l'eccezione dichiarata nel `CLAUDE.md` globale:
si spostano in `storico/handoff/NN-slug/` con `git mv`, **senza chiedere conferma**, perché il
giudizio «superato» l'utente l'ha già dato in anticipo per i documenti di `handoff/`.

Restano vivi in `handoff/`:

- le unità `PARZIALE` o `BLOCKED`, se ce ne sono;
- **`PIANO.md`**, che non è un'unità;
- **`01-ricognizione-ui/`**, che è la fonte dei rilievi e non un'unità di lavoro;
- **il tuo rapporto**, che non si archivia finché l'utente non l'ha letto.

## Cosa va nel rapporto finale, oltre al formato

Il formato sta in `~/.claude/architettura-sessioni.md`. In più, **tre cose che il capo deve
consegnare all'utente e che nessun resoconto contiene per intero** — raccoglile dai resoconti
e mettile in fondo, sotto un titolo `DA PORTARE ALL'UTENTE`:

1. **La migrazione SQL delle spese ricorrenti** (task 2 del piano in
   `docs/superpowers/plans/2026-09-03-spese-ricorrenti.md`): **nessun agente la esegue**, la
   esegue l'utente in produzione. Non è ancora scritta — è lavoro del filone successivo — ma
   il vincolo va ricordato ora perché è l'unica cosa in tutto il progetto che si ferma su una
   persona.
2. **`profiles.display_name` e `avatar_url` sono congelati al primo accesso**: chi cambia nome
   o foto su Google non lo vede riflesso. Emerso durante il lavoro, fuori dal perimetro di
   ogni unità.
3. **La proposta di ricognire `/collections/{id}` e tutta l'area voto/recensioni**: la
   ricognizione del 27 agosto non le ha mai viste, quindi i sedici rilievi non le coprono.
   L'unità 09 ha trovato lì un orfano (`CollectionDetail.razor`) senza cercarlo.
4. **La guardia d'uscita non è provabile da un agente, e resta l'unica correzione grave del
   piano verificata solo per lettura.** Il rilievo 1 — «il lavoro non salvato si perde senza
   una domanda» — si manifesta come un **dialogo nativo del browser** (`confirm`,
   `beforeunload`), e un dialogo nativo blocca il plugin che comanda Chrome: l'estensione
   smette di ricevere ordini finché qualcuno non lo chiude a mano. Il giro B non l'ha
   provocato di proposito, ed è la scelta corretta — è un limite dello strumento da riportare,
   non da aggirare, e **Playwright non è un'alternativa: non si usa, non si installa, non si
   propone**. Cinque prove su ventisette cadono qui. **All'utente basta un minuto**: aprire una
   nota, scrivere qualcosa **senza salvare**, e (a) premere il tasto **Indietro** del browser,
   (b) premere **F5**. Entrambe devono chiedere conferma prima di buttare via il lavoro; su
   «Annulla» si resta sulla pagina col testo intatto. Il giro B ha verificato il controllo
   opposto — F5 su una nota **senza** modifiche pendenti ricarica pulito, nessun dialogo —
   quindi si sa già che la guardia non scatta a sproposito. Manca la metà che scatta.

5. **I dati di collaudo lasciati in produzione**, che l'utente rimuove quando vuole. Nello
   spazio **Personale**, tutti chiamati **`COLLAUDO 4 SET`**: una **collezione**
   (`21cb3ec5-1286-4026-8d0f-2736d00b863c`, 5 campi, Voto al buio acceso) con dentro un
   **elemento** omonimo, e una **nota**. Nessuna spesa: quella creata è stata eliminata come
   parte di una prova. Tutto ciò che i giri hanno cancellato l'avevano creato loro — **nessun
   dato dell'utente è stato toccato**, verificato sui tre esiti.
6. **Le tre prove OAuth dell'unità 16 e l'annullamento vero su Google**: entrambe richiedono di
   essere disconnessi, e per un agente il logout è irreversibile — non ha modo di rientrare. Gli
   URL esatti e la ricetta stanno in `handoff/16-oauth-insieme-chiuso/resoconto.md`, sezione
   `DA PROVARE NEL BROWSER`.

---

## ESITO DEL COLLAUDO

*Lo riempie il capo prima di aprire questa sessione, con una riga per giro.*

> **COLLAUDO COMPLETO — 10 settembre 2026.** Tutti e quattro i giri sono stati eseguiti.
> **Zero difetti trovati** in tutto il collaudo: i parziali sono di copertura, mai di esito.
> La sessione di chiusura può essere aperta.

- **Giro A — il bloccante** (creare una collezione): **PASSA**, 4 set. Salvataggio riuscito,
  nessun `permission denied`, e **«Voto al buio» ancora acceso in due riaperture indipendenti**
  — il criterio che distingue la migrazione dalla toppa scartata. Resta in produzione la
  collezione «COLLAUDO 4 SET» (`21cb3ec5-1286-4026-8d0f-2736d00b863c`), che l'utente rimuove
  quando vuole. Console pulita a parte un'eccezione di un'estensione di Chrome, estranea
  all'app.
- **Giro B — il contratto degli editor**: **PASSA sul provabile**, 4 set. **19 prove su 27
  eseguite, tutte passate**; il controllo più delicato — «Elimina» armato che sopravvive a un
  cambio di entità, cioè un clic che cancellerebbe una cosa che nessuno ha chiesto di
  cancellare — è **sicuro**. Otto non eseguibili, tutte con motivo: **cinque richiedono un
  dialogo nativo** (v. sotto, è il limite che pesa), due un secondo account reale, una la
  disattivazione della rete dagli strumenti per sviluppatori. In più ha riconfermato dal vivo
  che gli importi ≥ 1.000 € non danno più errore, verificandolo anche nel sorgente.
- **Giro C — testo e messaggi**: **PARZIALE**, 4 set — e il parziale è per copertura, non per
  difetti: **zero difetti su tutto ciò che ha provato**. La riga che conta: **nessun JSON grezzo
  da nessuna parte**, il che chiude il rilievo 3 *dal vivo* e non più solo col `grep`. Ha
  provocato **tre errori veri** su tre file diversi sovrascrivendo `window.fetch` (dichiarato,
  e ripristinato ogni volta) e tutte e tre le frasi sono uscite in italiano, **verbatim** come
  le unità 13 e 14 le avevano scritte, con la diagnosi in console e mai a schermo. Confermati
  anche l'importo `1284,50` riletto due volte e «Salva» spento con la frase che dice perché.
  Non eseguite: le tre prove OAuth (richiedono la disconnessione, irreversibile per un agente),
  gli stati vuoti dei registri e il pannello dello spazio condiviso (**serve un secondo spazio,
  e ne esiste uno solo**), i messaggi delle recensioni.
- **Giro D — le misure**: **PASSA**, 10 set. **7 misure su 7 eseguite, zero difetti**, ed è
  l'unico giro che ha coperto per intero il proprio mandato. Tutti i numeri letti dal DOM, non
  impressioni: le pastiglie delle categorie sono a **48px** in entrambi i posti richiesti (erano
  21) e a **56,6px** in `CollectionEdit`, dove restano allineate a `.icona-input`; «Segna» spento
  è `rgb(27,27,27)` a `opacity 0.5` contro un acceso a `rgb(76,141,255)`; «Chiudi» durante il
  salvataggio è stato **colto a metà transizione** — `opacity 0.9397` in discesa verso 0.5,
  `cursor: default`, `href` rimosso — e ripristinato dopo; l'anteprima della nota sposta le azioni
  di **~7px** su una nota corta (erano 358) e `.corpo-nota` resta a 40vh esatti; selettore spazio
  e «Profilo» hanno lo **stesso `bottom`, scarto 0** (erano 18px); il banner «versione nuova»
  lascia **24px** di margine sulle azioni e «Più tardi» non genera né richieste di rete né chiavi
  in storage; «Elimina» è a filo del bordo destro, a **494px** da «Chiudi» (erano 8), e **tutti e
  quattro i controesempi** — «Esci» nel profilo, la conferma di eliminazione di uno spazio, la
  scheda di conflitto, «Sì, togli» nei campi di una collezione — sono rimasti a `margin-left: 0`.
  L'agente ha verificato anche il **quinto call-site** che nessuno gli aveva chiesto (la rimozione
  di una recensione), raggiungendo un **vero conflitto di salvataggio** con due schede concorrenti.
  **Niente resta nel database**: spesa di prova, recensione di prova e spazio di prova rimossi, e
  il testo della nota `COLLAUDO 4 SET` ripristinato carattere per carattere. Console pulita, tutte
  le richieste Supabase a 200.
  *Limite di metodo dichiarato dall'agente:* la finestra non è stata portata esattamente ai
  1414px del resoconto — `resize_window` non ha avuto effetto osservabile — quindi la prova 5 è
  stata condotta fra 1198 e 1296px, comunque sopra il breakpoint di 1024px che governa quel
  layout. È un limite dello strumento, non dell'applicazione.
