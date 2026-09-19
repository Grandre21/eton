# CHIUSURA — il rapporto

*Scritto dalla sessione di chiusura il **19 settembre 2026**, sullo stato finale `a6b23b9`, in un
worktree proprio. Il mandato che lo commissiona sta più sotto, invariato.*

```
CHIUSURA: 7 unità — FATTO 7 · PARZIALE 0 · BLOCKED 0
COPERTURA: 27 clausole — coperte 24 · scoperte 1 · rinviate 2
CONTRATTI: 6 aperti sul codice reale — convergenti 6 · divergenti 0
COMBINATO: <IN ATTESA>
GATE: dotnet build Eton.sln -warnaserror --no-incremental → Avvisi: 0  Errori: 0
      dotnet test Eton.sln → Non superati: 0. Superati: 310. Ignorati: 0. Totale: 310.
FUORI SCOPE: 25 voci — 10 lasciate per decisione dal collaudo · 12 aperte dalle unità ·
             3 misurate e non adjudicate
ARCHIVIATO: 7 cartelle, da handoff/01-foglio-di-stile a handoff/07-pastiglie-e-ancore,
            spostate in storico/handoff/ con git mv, con lo stesso nome
```

---

## 1 — COPERTURA: 27 clausole, e dove cade ognuna

L'obiettivo verbatim è **una frase** — «vorrei chiudere tutti i punti rimanenti in questa sessione»
— e le 27 clausole sono la sua decomposizione, che `PIANO.md` scrive in due pezzi: la glossa
(18 voci del `FUORI SCOPE` precedente + medaglione + profili congelati + ricognizione = **21**) e la
tabella delle **quattro crescite datate**, che porta a **27**. Ho contato contro quel numero, non
contro una mia ricostruzione.

**Nessuna divergenza fra piano e disco.** Tutti e sette i `resoconto.md` portano `ESITO: FATTO`, e
tutte e sette le righe della colonna di stato della `PARTIZIONE` dicono `FATTO`. Ho verificato anche
il gradino successivo, che la colonna dichiara e che nessuno aveva riaperto: i **sette commit di
integrazione** nominati (`c50981a`, `ab33d92`, `e20a059`, `6ce6ee3`, `8804763`, `b3afb1d`, `bb77cf7`)
esistono tutti e sono tutti antenati di `HEAD`. Il goal è venti commit, da `5035e0b` a `a6b23b9`.

### Le 24 coperte

| # | Clausola | Dove è caduta | Prova |
|---|---|---|---|
| 1 | contrasto delle micro-etichette | 01 | `--testo-fioco: #8a8a8a`; giro A misura 2 legge `rgb(138,138,138)` esatto |
| 2 | «Profilo» sovrapposto al selettore per 9px | 01 (scatola) + 02 (`.solo-lettori`) | giro A: overflow **0**, distanza **21px**, alto **48**, nome accessibile ancora «Profilo» |
| 3 | la colonna che si sposta di 7,6px | 01 | `scrollbar-gutter: stable`; giro A misura 3: scarto **0** *(con la riserva del §3 qui sotto)* |
| 4 | due azioni primarie nella stessa vista | 03 | `RecensioniElemento.razor:66-67` è ora `class="btn"` e dice «Salva recensione»; in `ItemEdit.razor:146` resta **un solo** `.btn.primario`. Sulla Home **non si applica**, per decisione scritta del 19 set |
| 5 | quattro controlli sotto i 48px | 01 | giro A misura 4: frecce **48×48 r12**, «Da provare» **48**, «Nessun voto» **48** (erano 22). Il quarto, «Tutte», è chiuso da `app.css:1448` — `.testa-registro a { min-height: var(--tocco) }` con margini negativi — **ma non è stato misurato**: v. §6 |
| 6 | il riquadro di `CollectionEdit` che sopravvive a `Rimuovi()` | 03 | `erroriValidazione = [];` dentro `Rimuovi`; giro B prova 4 PASSA. Residuo dichiarato in `FUORI SCOPE` |
| 7 | le due frasi di `SupabaseService` | 05 | riscritte; «sessione senza utente» non compare più in nessun ramo |
| 8 | le chiamate a `localStorage` fuori da ogni `try` | 05 | tre protette dentro `PkceStore`; la quarta **dimostrata già coperta** dal `catch` del suo unico call-site |
| 10 | l'errore di validazione che resta a schermo | 03 | condizione di render calcolata; giro B prova 3: la riga rossa sparisce **al primo carattere**, senza premere «Salva» |
| 11 | `@using Eton.Services` ridondante in tre file | 04 | un `Grep` su tutti i `.razor` restituisce oggi **una** riga, `_Imports.razor:9`; prima quattro |
| 14 | l'invariante delle prime tre emoji | 04 | `SchemaCampiTests.cs:253` confronta le **due liste fra loro**; 287 → 288 test |
| 15 | la Home che si ridisegna solo dopo la rete | 02 | giro B prova 7: nessun residuo dello spazio vecchio, e il selettore non sparisce mai. La forma **è stata decisa guardandola**, come il piano prometteva |
| 16 | rinominare `Denaro.Testo` | 04 | chiusa **documentando**: due `///` che dicono quale usare quando. Nessuna rinomina, i 7 consumatori e le 14 asserzioni intatti |
| 17 | l'importo di chi non può intervenire | 04 | ramo `@else` con `Denaro.Testo` in `SpesaEdit.razor:99`. **La prova nel browser non è stata possibile**: v. §6 |
| 18 | `FraseRifiuto` col suo test su `Enum.GetValues` | 05 | provato **per mutazione**: aggiunto `Sospeso` all'enum, il test fallisce **e nomina il valore** |
| P2 | il medaglione 📋 | 01 | `.icona-collezione` 40×40 **anello**; giro A misura 5: `border: 0.8px solid #333`, fondo trasparente, stesso centro verticale dell'`h1` |
| — | i profili congelati | 06 | `AllineatoreProfilo` + `IdentitaGoogle`, 310 test; giro B prova 6: **nessun `PATCH`** su `/rest/v1/profiles` al secondo avvio |
| — | la ricognizione mai fatta di voti e recensioni | collaudo, passo 4 | `handoff/collaudo/ricognizione-voti-recensioni.md`: 5 attriti, e un elenco di ciò che funziona |
| +1 | fusione delle pastiglie in `button.pastiglia` | 07 | giro A misura 6 **e il controllo negativo**: su `/spaces` gli `<span class="pastiglia">` sono fermi a **26,65px** |
| +2 | `.btn.compatto` sulle frecce di mese | 07 | giro A misura 4: 48×48, raggio 12px |
| +3 | i rimandi ancorati al selettore | 07 | 22 rimandi censiti, **7 scaduti** su 22 — non 3 come si credeva |
| +4 | la corsa critica di `RecensioniElemento` | 03 | `:300` `if (miaGenerazione == generazione) caricato = true;` — v. §3, l'ho riaperta io |
| +5 | `Sovrascrivi()` che non controlla il nome | 04 | guardia `if (!NomeValido)` nel metodo. **La prova nel browser non è stata fatta**: v. §6 |
| +6 | il `<label>` senza controllo | 07 | `.campo-lettura` aggiunta alla regola esistente + una `RenderFragment`; costo reale +35/−18, non le due righe preventivate |

### La scoperta — **1**

È la **voce 12**, e non è caduta in un'unità: la `PARTIZIONE` la assegnava al collaudo, insieme alle
altre due voci che «non producono codice». Ricopiata dalla fonte che la numerazione del piano usa
(`storico/handoff/CHIUSURA.md:219-221`):

> 12. La prova 8b dell'unità 11 — a 360px «Sì, elimina» e «Annulla» restano sulla stessa riga? — non
>     compare nel riepilogo del giro D, e il `D-esito.md` che il brief prescriveva non esiste: il
>     numero che decide quella voce non c'è ancora.

e dalla riga che gliela riassegnava in questo goal (`handoff/PIANO.md:386`):

> | **12** | la prova a 360px della coppia «Sì, elimina» / «Annulla» | collaudo, con `live-testing` |

**Nessuno l'ha fatta e nessuno ha detto di non averla fatta.** `grep` di `360`, di «Sì, elimina» e
di `ConfermaAzione` su tutto `handoff/` e sul documento di `ui-critic` non restituisce nessuna
misura di quella coppia: il giro A ha sette misure e nessuna è questa, il giro B ha sette prove e
nessuna è questa, la ricognizione non ci passa. `ui-critic` ha misurato a 360px il **pavimento**
(`scrollWidth == clientWidth` su tutte e dieci le rotte), che è un'altra cosa: quella coppia di
pulsanti si rende solo **dopo** aver premuto «Elimina», e a 360px nessuno l'ha resa.

⚠️ **Perché è caduta proprio lì, e vale più della voce.** Il brief di `live-testing` si compone
«**ricopiando** le misure attese dai sette resoconti», come `PIANO.md` prescrive due volte. Una
clausola che **nessun resoconto contiene** — perché nessuna unità l'aveva in mandato — non ha
nessuno che la ricopi: la regola che rende il brief onesto è la stessa che rende invisibili le tre
voci non assegnate. Non è una dimenticanza di chi ha scritto il brief, è una proprietà del modo in
cui il brief si costruisce.

C'è un solo documento che ci passa accanto senza nominarla: la ricognizione dichiara fra i «non
provati» che «**Elimina**» non è stato tentato «per divieto esplicito: produce dialoghi nativi che
bloccano il plugin». Spiega perché la misura sarebbe stata difficile; non dice che la voce 12 è
rimasta aperta.

**Non l'ho chiusa io**, e non per pigrizia: chiuderla richiede il server acceso e il browser, che il
mandato mi vieta entrambi.

### Le rinviate — **2**

**Voce 9 — la barra gialla di Blazor. Rinviata al prossimo giro di collaudo, con la prova scritta.**
Non è scoperta: l'unità 05 ha condotto un'istruttoria completa e ha escluso **sei** cause, ognuna con
la riga che la esclude — i tre percorsi di salvataggio del giro C, `GetClientAsync()` comune ai 52
call-site, `SpaceStateService`, il refresh automatico di Gotrue (smentito da `doc-checker` **sul
sorgente della versione installata**), il service worker, e i componenti condivisi. Ha anche smentito
la voce stessa: **la barra non è mai stata osservata sul percorso d'accesso**, quindi il punto 1 non
poteva farla sparire.
**Cosa resta e cosa costa:** non è escludibile la forma esatta della sovrascrittura di `window.fetch`
usata nella simulazione. Se sostituiva `fetch` con una funzione che **lancia sincronamente** invece
di restituire una Promise rifiutata, il punto di fallimento si sposta dentro il marshalling di Blazor
e può non essere catturabile dal `catch` C#. **La prova da eseguire è una riga**: al prossimo giro a
rete bloccata, riportare **verbatim** lo snippet usato. Decide fra due ipotesi e non costa altro.

**Voce 13 — il banner di aggiornamento in condizioni vere. Rinviata al primo rilascio.**
Si sapeva dall'apertura e sta scritto in `APERTO`: in sviluppo non è osservabile, perché il service
worker di dev è un no-op e il banner che riappare lì **non è un difetto**. **La prova da eseguire, sul
sito pubblicato:** pubblicare una versione nuova con la vecchia già aperta in una scheda; il worker
deve mettersi **in attesa**, il banner comparire, «Più tardi» deve chiuderlo senza aggiornare, e il
banner deve **riapparire al primo avvio successivo**. Tre osservazioni, un rilascio.

---

## 2 — CONTRATTI: sei, aperti sul codice reale

Non sui resoconti. Dove il numero di riga è cambiato dopo la dichiarazione dell'unità lo scrivo:
**il testo è il contratto, il numero no**, ed è la classe di difetto che l'unità 07 ha appena chiuso.

**1. `Shared/PaginaEditor.cs` — invariante. CONVERGENTE, e con la misura che nessuna unità poteva
fare.** Le unità 03, 04 e 06 lo dichiarano intatto ciascuna sul **proprio** diff; solo il diff
combinato prova che nessuna delle sette l'ha toccato. Verificato in due forme:
`git diff --name-only 6b7e8b4..HEAD -- Shared/PaginaEditor.cs` → **vuoto**, e
`git log 6b7e8b4..HEAD -- Shared/PaginaEditor.cs` → **nessun commit**. Non è stato aperto in
scrittura in nessun punto dei venti commit del goal.

**2. `.solo-lettori` e `.voce-piede`, fra 01 e 02. CONVERGENTI.**
Produttore `wwwroot/css/app.css:2512` — nove proprietà, e **né `display` né `visibility`**, che sono
le due che avrebbero tolto il testo dall'albero di accessibilità insieme alla vista. Consumatore
`Shared/Navigazione.razor:51`, `<span class="solo-lettori">Profilo</span>`, dentro
`<a href="profile" class="voce-piede">` a `:49`.
Produttore `app.css:2417`, dentro la media query, con `width: var(--tocco)` e `height: var(--tocco)`
a `:2422-2423`. Il giro A misura entrambe le facce sullo stesso elemento: **48/48, overflow 0, e il
nome accessibile ancora «Profilo»**. Le due unità sono atterrate sulla stessa scatola.

**3. `button.pastiglia`, fra 01 e 07 — il contratto **revocato**. CONVERGENTE.**
La 01 aveva «sei l'unico a toccare `app.css`»; la 07 l'ha revocato, e il piano lo dichiara. La
condizione che rendeva sicura la frase regge: la 01 era rientrata e integrata (`c50981a`), le unità
02-06 non toccano il foglio — verificato sul diff combinato — quindi la 07 l'ha trovato fermo.
La cascata, aperta sul file finale e non creduta: `button.pastiglia` (`app.css:2132`, 0-1-1) dichiara
`min-height: var(--tocco)`; `.pastiglia` (`:1058`, 0-1-0) ha **undici** proprietà e **nessuna è
`min-height`**; `.pastiglia.accesa` (`:1072`, 0-2-0) dichiara **solo** `border-color`, `background`,
`color`. Nessuna variante di stato scavalca il pavimento.
I consumatori sono esattamente quelli che la 01 aveva contato: **sei** `<button class="pastiglia">`
e **quattro** `<span class="pastiglia">`. Le righe si sono spostate (le unità 03, 04 e 07 hanno
scritto sopra), il conto no. Il controllo negativo del giro A chiude il cerchio dall'altro lato:
gli `<span>` di `/spaces` sono fermi a 26,65px, cioè la fusione **non è colata fuori**.

**4. Le tre firme di `Denaro`, fra 04 e 03. CONVERGENTI.**
`Verifica(string?, out decimal)` a `Services/Denaro.cs:66` — invariata, riga compresa. `Testo(decimal)`
a `:123` e `TestoDigitabile(decimal)` a `:151`, **identiche nel testo**, spinte in basso dalle
docstring nuove. Nessuna rinomina.
Il punto che conta è che i due consumatori **non si incrocino**, e non si incrociano:
`SpesaEdit.razor:99` rende `Denaro.Testo` in un `<p>` — ramo di sola lettura, unità 04 — mentre
`Cambiata` a `:253` confronta `importoTesto` con `Denaro.TestoDigitabile`, e `importoTesto` è scritto
solo da `TestoDigitabile` (`:315`, `:373`, `:404`). **Il `<p>` non tocca `importoTesto`**: il ramo
nuovo non può rendere `Cambiata` vera all'apertura, che era il difetto da non riaprire. Il giro B
prova 1 lo conferma dal lato modificabile: «Chiudi» esce **subito, nessun dialogo**.

**5. Il tipo di esito OAuth e la sua enumerazione, unità 05 — contratto di sicurezza. CONVERGENTE, e
la diagnostica non raggiunge lo schermo in nessun ramo.**
`OAuthRifiuto` (`Services/OAuthCallback.cs:4`) ha i quattro membri di sempre; `OAuthCallbackEsito` a
`:20` e `Analizza(string)` a `:33` sono invariati. `FraseRifiuto` a `:88` prende **solo** un valore
dell'enumerazione: non ha accesso né alla query grezza né al record.
La verifica che il contratto chiede è un conteggio, e l'ho fatto io: `Diagnostica` compare in **tre**
punti in tutto il progetto — la docstring, la dichiarazione del record, e
`Services/SupabaseService.cs:118`, dentro `Console.Error.WriteLine`. **Zero occorrenze in `Pages/` e
in `Shared/`.** Il consumatore assegna `ErroreAccesso = OAuthCallback.FraseRifiuto(esito.Errore)`
(`:120`); le altre tre assegnazioni (`:206`, `:216`, `:235`) sono frasi scritte dall'unità, non testo
del provider.

**6. `AuthStateService` e i due servizi nuovi, unità 06. CONVERGENTI.**
`GetUserIdAsync()` a `Services/AuthStateService.cs:30` e `GetDisplayNameAsync()` a `:43` — invariate,
**e perfino alle stesse righe**. `GetDisplayNameAsync` ha cambiato corpo, non firma: delega a
`IdentitaGoogle.NomeDa` (`:46`), che è il produttore nuovo — produttore e consumatore stanno in due
file diversi e si incontrano lì.
`AllineatoreProfilo.AllineaAsync(SupabaseClient, bool)` a `:47`, consumatore unico
`Services/SupabaseService.cs:156`, registrazione unica `Program.cs:23`, iniezione nel costruttore a
`SupabaseService.cs:35`. Un produttore, un consumatore, un call-site: quanti il mandato ne concedeva.

**Una verifica in più, che il §5 mi impone perché tocca la concorrenza.** La corsa critica trovata
dall'unità 02 e corretta dalla 03 l'ho riaperta io: `Shared/RecensioniElemento.razor:300` porta
`if (miaGenerazione == generazione) caricato = true;` come **ultima** riga di `Carica()`, dopo
l'`await` di `:292`. È la stessa forma delle altre guardie del file, che sono oltre venti, e
l'assegnazione non guardata a `:208` sta **prima** di qualunque `await`. Il call-site è unico
(`Pages/ItemEdit.razor:164`) e senza `@key`, come l'unità dichiarava: è il motivo per cui la guardia
serve, e c'è.

---

## 3 — COMBINATO

<SEGNAPOSTO-COMBINATO>

---

## 4 — GATE, rieseguiti sullo stato finale

Eseguiti da me nel worktree di chiusura, su `a6b23b9`, a **nessun agente in scrittura attivo**:

    dotnet build Eton.sln -warnaserror --no-incremental  →  Avvisi: 0   Errori: 0
    dotnet test Eton.sln                                 →  Superato! Non superati: 0.
                                                            Superati: 310. Ignorati: 0. Totale: 310.

**310, contro i 287 di `6b7e8b4`.** I ventitré nuovi si ricompongono: +1 dall'unità 04 (l'invariante
della tavolozza), +2 dalla 05 (completezza dell'enumerazione e invariante di `Nessuno`), +20 dalla 06
(le due suite di decisioni pure più i due difensivi). Nessuno dei 287 preesistenti ha cambiato esito.

Il numero coincide con quello che il capo dichiara nel mandato: chi trova un gate rosso da qui in
avanti sa che a romperlo non è stato questo goal.

**Il server non è stato avviato e il browser non è stato aperto**, come il mandato vieta.
Verificato, non dedotto: la **porta 5000 è libera** (`Get-NetTCPConnection` non restituisce nulla).
L'unico processo `dotnet` vivo è un nodo MSBuild con `nodeReuse:true` lasciato dalla mia stessa
esecuzione dei gate — non un server di sviluppo, e non è in ascolto su nessuna porta dell'app.

---

## 5 — FUORI SCOPE — aggregato

**25 voci.** Nessuna è stata corretta da questa sessione, e per i primi dieci è un'istruzione
esplicita del mandato.

### A — dal collaudo, lasciati per **decisione scritta** (10)

Non sono stati dimenticati. Vale la regola che il capo si è dato dopo la quarta crescita
dell'obiettivo — *dalla quinta in poi le voci nuove vanno nel rapporto* — e violarla alla prima
occasione l'avrebbe resa finta. Hanno una destinazione dichiarata: la **fase 2.1-bis**.

**Da `ui-critic`** — `docs/superpowers/specs/2026-09-19-rilievi-ui-critic.md`. Pavimento passato su
tutte e dieci le rotte; questi cinque stanno sopra il pavimento:

1. **Le tre superfici esistono come token ma non come scala** · alta · `TIPO: progetto`. È l'unico
   che nomina una **regola assente** invece di un valore sbagliato, ed è l'ingresso naturale della
   fase 2.1-bis.
2. **Nove controlli senza nome accessibile** · alta · `TIPO: coerenza`. ⚠️ **È l'unico dei cinque che
   non è una decisione di progetto**: è un difetto oggettivo col fix noto e piccolo (`aria-labelledby`
   sul cursore del voto, `aria-label` sui cinque `<select>` e sui tre campi delle opzioni). Se
   l'utente vuole chiuderne uno subito, è questo. È anche la **stessa famiglia** del rilievo che
   questo goal ha appena chiuso su «Profilo».
3. Due voci di navigazione a **44px invece di 48**, scritte a mano · media · `TIPO: fedeltà`.
4. Controlli sulla stessa riga con altezze diverse (8,6px nel selettore di icone) · media.
5. Il titolo di pagina non segue una regola sola · media · `TIPO: gerarchia`.

**Dalla ricognizione di voti e recensioni** — `handoff/collaudo/ricognizione-voti-recensioni.md`:

6. **Il pulsante «?» non risponde al primo clic** · **grave** · sistemico. Riprodotto **tre volte su
   due rotte**. È il più grave dei dieci: quel pannello è l'unico canale in-app che spiega il voto al
   buio e i conflitti, e chi lo preme una volta conclude che è rotto. ⚠️ Gli infobutton erano stati
   misurati **funzionanti** ad agosto: prima di correggere va stabilito se il difetto è comparso dopo.
7. L'aggregato del voto mostra un frammento rotto con commento senza voto · media.
8. **Un conflitto annunciato dove non c'è nessun altro** · media. Due problemi, e il secondo è più
   serio: il comportamento (ricarica da solo) **non corrisponde a ciò che l'aiuto della stessa
   schermata promette** (una scelta fra ricaricare e sovrascrivere).
9. Il pulsante «?» fluttua accanto a un titolo lunghissimo · lieve.
10. «Ordina per» offre solo «Nome» · minore, segnalato con cautela.

### B — aperti dalle unità, ancora vivi sullo stato finale (12)

Ognuno è una **decisione dichiarata**, non una svista, e ognuno porta il proprio motivo nel resoconto
ora archiviato:

11. **Il riquadro di `CollectionEdit` può ancora spegnersi con un errore vero rimasto su un altro
    campo** (03). Il `checker` sul fix ha detto `non risolto` e l'unità l'ha **ricopiato invece di
    ammorbidirlo**. Il capo ha confermato di non correggere: filtrare sarebbe un confronto di
    sottostringhe, e un clic su «Salva» ridà il verdetto completo. Il prezzo è ora **scritto nel
    file**.
12. **`SpesaEdit.Sovrascrivi():398`, l'errore che sopravvive alla correzione dei campi** (03). Non si
    tocca: lì la decisione è di **principio** e scritta — «un return silenzioso lascerebbe il pulsante
    sembrare inerte».
13. **Il pulsante «Sovrascrivi» di `ItemEdit` resta spento dal solo permesso** (04), non anche dalla
    validità del nome. L'unità ha applicato **una** delle due protezioni dell'omologo, quella che
    chiude il buco.
14. **Il messaggio della guardia nuova di `ItemEdit` sopravvive alla correzione del campo** (04).
    Stessa famiglia del 12, e `bug-hunter` l'aveva già giudicato non-rilievo.
15. **`BrowserSessionHandler.SaveSession` e `DestroySession` restano senza `try`** (05). Scoperti *di
    proposito* nello stesso file che contiene il precedente protetto; la verifica «esiste un ripiego
    onesto?» su di loro non l'ha fatta nessuno.
16. **`AllineatoreProfilo` è registrato in DI invece che costruito in linea** (06). Due agenti
    indipendenti dicono la stessa cosa; i suoi due fratelli identici sono costruiti a mano dentro
    `SupabaseService`. Costa tre righe, ed è una decisione del brief, non dell'unità.
17. **`eton.profilo` sopravvive al logout** (06), unico artefatto locale che lo fa: `eton.session`,
    `eton.pkce` e `eton.spazio` vengono cancellati. Nessun danno funzionale, ma il prossimo che
    aggiunga un dato più sensibile all'impronta erediterebbe la dimenticanza.
18. **Il testo d'aiuto di `Pages/Profile.razor:14-15` può ora promettere di più** (06). Non è
    diventato falso: è diventato incompleto in senso favorevole.
19. **Il `<p class="importo-spesa dato">` in sola lettura è alto ~72px di troppo** (07), perché
    `app.css` non ha un reset dei margini di `p` e i margini dei figli di un flex container non
    collassano. **Preesistente al diff della 07 e non peggiorato da lei.** Rimedio: una riga,
    `margin: 0`.
20. **Un rimando scaduto fuori dal perimetro della 07**: `Pages/CollectionEdit.razor:75`. Il fatto
    largo è che **la convenzione nuova vive nell'intestazione di `app.css`, ma il difetto che cura
    esiste anche nei `.razor`**, dove nessuna intestazione la dichiara.
21. **Due proprietà ridondanti in `.barra-elenco .pastiglia`** (07), tenute perché il mandato le
    metteva in `NON TOCCARE`.
22. **Il commento di `.btn.compatto` (`app.css:764`) è diventato più stretto della realtà** (07): dice
    che la variante serve «nella testata di schermata», e da oggi serve anche nella testata di una
    scheda. Non falso, ma incompleto; l'informazione esiste altrove nel file.

### C — misurati e **non adjudicati** (3)

`ui-critic` li ha dichiarati fuori dal proprio tetto dei cinque. Non sono istruiti, e vanno istruiti
prima di essere creduti:

23. `.dato { font-size: .95em }` produce **12,35px e 15,2px** su quattro rotte, mentre la stessa
    classe con corpo esplicito legge 13px: **tre corpi per una classe sola**.
24. Nella scheda elemento il primo campo è a **0px** dal secondo e tutti gli altri a 12px, perché il
    margine sta su una classe che il primo non ha.
25. «Nuova collezione» è `.btn.primario` 16px/181px in Home e `.btn.primario.compatto` 13px/133px su
    `/collections`: stesso testo, due forme.

---

## 6 — TRE COSE CHE NESSUN RESOCONTO CONTIENE

Il mandato le chiede per nome, e nessuna si legge nei sette resoconti perché nessuna nasce da
un'unità.

### 1. Tre dati di prova restano sul database di sviluppo — **servono tre gesti dell'utente**

I dialoghi nativi del browser bloccano il plugin, quindi gli agenti non hanno potuto eliminarli. Sono
riconoscibili dal nome e nessuno di essi è ambiguo:

| # | Cosa | Dove | Come si toglie |
|---|---|---|---|
| 1 | la spesa **«COLLAUDO GIRO B»**, 1.284,50 €, 19/09/2026 | spazio «Personale» → Spese | apri la voce, Elimina, **conferma il dialogo del browser** |
| 2 | lo spazio **«COLLAUDO GIRO B TEST»**, vuoto, un solo membro | Spazi | aprilo, Elimina lo spazio, conferma il dialogo |
| 3 | la collezione **«Collaudo Ricognizione»**, con due elementi — «Chianti Classico 2021» e l'elemento dal nome lunghissimo | spazio «Personale» → Collezioni | aprila, Elimina, conferma il dialogo |

La collezione abbozzata su `/collections/new` durante il giro B **non è stata salvata** e non esiste
lato server: non c'è niente da togliere.

### 2. La voce 13 non si chiude in questo goal, e si sapeva dall'apertura

È la seconda delle due rinviate, e il §1 ne porta la prova da eseguire. **Va letta come rinviata al
primo rilascio, non come coperta**: la differenza non è formale, perché una voce dichiarata coperta
non torna sul tavolo di nessuno.

### 3. Un'osservazione che il codice letto non spiega, e che **non è risolta**

Durante il giro B è comparso un **dialogo nativo premendo «Elimina» su una spesa**. Il percorso però
passa da `Shared/ConfermaAzione.razor`, che è un componente **in pagina**, e da `Esci()`, che
**disarma** la guardia prima di navigare: `Shared/PaginaEditor.cs:76-86` ha una sola guardia, e quel
ramo non dovrebbe produrre un `confirm`. Il capo ha letto il codice e non ha una spiegazione.
**Non è stata riprovata**, perché riprovare significa bloccare di nuovo il browser — e non la
dichiaro risolta. Va verificata a mano, ed è la stessa mossa che chiude la voce 12: **entrambe
stanno dietro lo stesso dialogo**.

### E due prove che il collaudo non ha potuto fare, dichiarate invece che taciute

Non sono clausole scoperte — il codice c'è ed è verificabile leggendolo — ma il loro **osservabile**
non è mai stato visto, e chi legge deve saperlo:

- **Voce 17, il ramo di sola lettura dell'importo.** Il giro B prova 2 è `non provato`: lo spazio ha
  **un solo membro** e non esiste un secondo account. Il valore atteso è `1.284,50 €` nel campo, col
  punto delle migliaia e il simbolo, identico all'elenco.
- **La clausola +5, `Sovrascrivi()` che ora controlla il nome — ed è la voce 12 una seconda volta.**
  `handoff/server.md:68` elenca «**sovrascrittura col nome vuoto**» fra ciò che il giro B prova.
  `B-esito.md` **non la nomina affatto**: non è fra le sette righe della sua tabella, non è fra le
  «non eseguibili», e `grep` di `Sovrascriv` su tutto `handoff/collaudo/` restituisce **zero**. Il
  giro si dichiara «verde, 6/7 eseguiti» su un elenco di sette che non è più quello pianificato.
  **Non cambia la copertura** — la clausola è chiusa nel codice e l'ho riaperta io:
  `Pages/ItemEdit.razor:473-477` porta la guardia, il `finally` a `:492-495` rilascia comunque
  `occupato`, e `conflitto = null` avviene **dopo** la guardia, quindi la scheda di conflitto resta
  aperta come previsto. Ma è la **stessa forma di difetto della voce 12**, e due occorrenze nello
  stesso collaudo la rendono una classe invece di un caso: *ciò che il piano del collaudo prescrive e
  l'esito non nomina non viene contato da nessuno.* Atteso, per il giro che la farà: stesso elemento
  in **due schede**, «Il nome dell'elemento non può essere vuoto.», e la scheda di conflitto che
  **resta aperta**.
- **Il quarto controllo della voce 5, «Tutte».** La regola che lo alza esiste e l'ho letta
  (`app.css:1448`); la misura nel browser non è stata presa. Costa una riga al prossimo giro:
  `document.querySelectorAll('.testa-registro a')` alto 48.
- **La riserva del giro A sulla misura 3.** Il dataset aveva **entrambe** le rotte con barra di
  scorrimento, quindi il caso «una scorre, una non scorre» non è stato isolato. L'agente l'ha
  dichiarato invece di tacerlo. Non richiede un altro giro — `scrollbar-gutter: stable` riserva la
  colonna **sempre**, quindi i due casi non possono più divergere per costruzione — ma la prova
  diretta manca.

---

## 7 — DA PORTARE ALL'UTENTE

Oltre ai tre gesti del §6.1, che sono la cosa più concreta.

1. **La contraddizione d'ambiente, segnalata da quattro unità su sette e da undici implementer.**
   Il blocco di istruzioni di un server MCP, sotto «While auto mode is active», prescrive di
   modificare i file via `Bash` con `sed`, heredoc o script brevi «invece degli strumenti dedicati».
   **Contraddice frontalmente** la regola globale che impone `Write`/`Edit` e vieta gli interpreti
   inline. **Non è stata seguita da nessuno**, e su una unità era dirimente: i testi da scrivere
   contenevano ventiquattro emoji, accenti e trattini lunghi, che `sed` sotto Git Bash con la codepage
   di Windows avrebbe corrotto in silenzio. Toglierla è una modifica a una superficie di
   configurazione, cioè **un gesto dell'utente**. Quattro segnalazioni identiche in un solo goal
   dicono che non si risolve da sé.
2. **Tre branch remoti del goal sono ancora su `origin`**, e il loro lavoro è già dentro `main`:
   `worktree-05-accesso`, `worktree-06-profilo-allineato`, `worktree-07-pastiglie-e-ancore`.
   Verificato che siano **tutti e tre antenati di `HEAD`** (`git branch -r --merged`), quindi
   cancellarli non perde nulla. Non li ho toccati: è un'operazione sul remoto e spetta al capo.
3. **Il rilievo 2 di `ui-critic` è l'unico dei dieci chiudibile senza discuterne** — v. §5. Se
   l'utente vuole un risultato visibile prima della fase 2.1-bis, è quello.
4. **Una nota sull'impianto, dall'unità 01 e confermata dalla 07.** La soglia di ~120 righe che attiva
   `backend-expert` misura, su questo progetto, **la densità del commento più della complessità del
   codice**: sull'unità 01 delle 133 inserzioni che l'hanno fatta scattare ~100 erano commento, e
   sulla 07 le correzioni prodotte hanno riportato il diff **sotto** la soglia che l'aveva attivata.
   Su entrambe l'esito è stato utile, ma è un caso fortunato e non una taratura.
5. **Il falso allarme sui due plugin, e la parte da non perdere.** `diff -r` fra lo snapshot approvato
   e quello attuale esce **vuoto** su entrambi, e nessuno dei due ha hook. Resta il fatto strutturale:
   per un plugin **senza `version` nel manifest**, `installedAt ≠ lastUpdated` è **rumore**, perché il
   marketplace è un monorepo e ogni commit muove la data di tutti.

---

## 8 — ARCHIVIATO

Con `git mv`, senza chiedere, secondo l'eccezione dichiarata nel `CLAUDE.md` per le unità `FATTO` —
che sono **tutte e sette**:

`01-foglio-di-stile` · `02-barra-e-home` · `03-editor-esiti` · `04-igiene-e-importi` · `05-accesso` ·
`06-profilo-allineato` · `07-pastiglie-e-ancore`, da `handoff/` a `storico/handoff/`, con lo stesso
nome. Nessuna collisione con le diciassette cartelle del ciclo precedente: gli slug sono tutti
diversi.

**Restano in `handoff/`**, e non si archiviano: `PIANO.md`, dove il capo scrive
`PROSSIMA AZIONE: GOAL CHIUSO`; questo `CHIUSURA.md`, che non si archivia finché l'utente non l'ha
letto; `handoff/collaudo/`, che il mandato esclude esplicitamente; e `server.md`.

---

## 9 — DOVE STA QUESTO LAVORO

Sessione in background, quindi worktree isolato, come per le sette unità:

    branch:    worktree-chiusura-sedici-rilievi
    percorso:  G:\Sviluppo\Eton\.claude\worktrees\chiusura-sedici-rilievi
    contiene:  questo rapporto in testa a handoff/CHIUSURA.md, e le sette cartelle
               spostate in storico/handoff/ con git mv

**L'integrazione su `main` spetta al capo**, come per ogni unità di questo goal. Nessun file sorgente
è stato modificato da questa sessione: il diff è **solo** `handoff/` e `storico/handoff/`.

---

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
