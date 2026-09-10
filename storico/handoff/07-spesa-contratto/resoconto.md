UNITÀ: 07 — ESITO: FATTO

TOCCATI:
- `Pages/SpesaEdit.razor` → +30/−18 (400 righe → 412)

Nessun altro file, né sorgente né documentazione. Un solo `implementer`, un solo brief, nessun
`BLOCKED`.

## CONTRATTI

`Shared/PaginaEditor.cs` **non è stato toccato**: nessuna deroga, nessuna aggiunta, nessun
`BLOCKED`. Quarta adozione, quarto incastro senza attriti — il contratto ha ora retto su quattro
pagine, dalle 400 righe di questa alle 755 di `CollectionEdit`, senza che nessuna delle quattro
abbia dovuto chiedergli niente.

Tutti i `file:line` qui sotto li ho riaperti io sul file su disco dopo la build finale. Nessuno
viene da un revisore.

```razor
Pages/SpesaEdit.razor:4     @inherits PaginaEditor
Pages/SpesaEdit.razor:39    <NavigationLock ConfirmExternalNavigation="@Cambiata"
                                            OnBeforeInternalNavigation="GuardaUscita" />
```

```csharp
// Pages/SpesaEdit.razor:210 — era 'private bool Cambiata', corpo invariato riga per riga
protected override bool Cambiata => spesa is not null && (importoTesto != Denaro.Testo(spesa.Amount) || descrizione != spesa.Description
                                                 || categoria != spesa.Category || data?.Date != spesa.SpentOn.Date);

// Pages/SpesaEdit.razor:398 — dentro Elimina(), dopo EliminaAsync riuscita
Esci("expenses");
```

- **`@inject NavigationManager Navigation` tolto** (era `:7`). Verificato da me con un grep: la
  stringa «Navigation» compare oggi nel file **una volta sola**, a `:39`, ed è
  `<NavigationLock>`. Non ho dovuto rimetterla: l'unica navigazione della pagina era un'uscita da
  editor.
- **Un solo `Esci`, e senza `replace`.** La ricognizione del capo regge: `@page` uno solo, nessun
  ramo di creazione, nessun `Crea()`, un solo `NavigateTo`. Non ho trovato niente da dichiarare
  in `SCOSTAMENTI` su questo punto.
- **`IDisposable`: non implementato dalla pagina**, come il mandato vieta. Nessuna occorrenza di
  `IDisposable` né di `Dispose` nel file: nessun rischio di CS0108 — e il gate a 0 avvisi con
  ricompilazione forzata lo dimostra, non lo presume.
- **`<NavigationLock>` è dentro il ramo del modulo**, prima riga del ramo `else`, unica istanza
  nel file. Nessun revisore ne ha proposto lo spostamento fuori dai rami: il paragrafo del
  mandato non ha dovuto essere citato in adjudica, per la quarta volta su quattro.
- **`TitoloSchermata` (`:197`, `private string`) ha due call-site**: `<PageTitle>` a `:9` e
  `<TestataPagina Titolo>` a `:11`. L'espressione è **quella che `<PageTitle>` già calcolava**,
  spostata in una proprietà invece che duplicata: il titolo è riusato, non ricalcolato. Stessa
  forma, stesso nome e stessa posizione relativa nel blocco `@code` di `NoteEdit.razor:132`,
  `CollectionEdit.razor:327` e `ItemEdit.razor:165`.

**Il cambio di comportamento che il contratto porta qui, e che vale la pena scrivere.** Prima,
`Elimina()` chiamava `Navigation.NavigateTo("expenses")` grezza. `NavigationManager` è un
singleton condiviso da tutta l'applicazione, e `EliminaAsync` non è legata a nessuna
cancellazione: se l'utente premeva «Elimina» e poi se ne andava dalla barra di navigazione
mentre la DELETE era ancora in volo, quella chiamata tardiva **dirottava la pagina su cui era
approdato nel frattempo**. Con `Esci` il `if (smontata) return;` della base chiude quel caso: la
spesa resta eliminata, è solo la navigazione ad essere abbandonata. Non è un effetto collaterale
dell'adozione, è metà del suo scopo.

## IL GATE DI «CHIUDI»

```razor
Pages/SpesaEdit.razor:141   <a class="btn" href="@(occupato ? null : "expenses")">Chiudi</a>
```

`null` letterale, nessun `?? ""`, nessun `disabled`, nessun `aria-disabled`, nessun `tabindex`,
nessuno stile inline, nessuna trasformazione in `<button>`. La destinazione è quella che il link
aveva già (`expenses`), costante come sulle due gemelle non parametriche.

I due `<a class="btn" href="expenses">Torna alle spese</a>` dei rami «spesa sparita» (`:27`) e
«errore di caricamento» (`:34`) **non sono stati toccati**: lì non c'è nessuna scrittura in volo
da proteggere, e le unità 04 e 06 hanno fatto la stessa distinzione.

**Il selettore `a.btn:not([href])` resta all'unità 11.** Fino ad allora il link è funzionalmente
inerte ma non spento visivamente: stato intermedio atteso, non difetto.

## L'ESITO DOVE SI GUARDA — il censimento, prima dello spostamento

Il mandato chiedeva di censire **prima** di spostare, e di dichiarare cosa avessi trovato,
perché su questa pagina «non si sa» se esista un secondo canale. Ho censito. Riferimenti alla
numerazione **di oggi**, riaperti da me.

**Punti di scrittura — tredici, tutti e soli su `errore` e `avviso`:**
`:234` (azzeramento in `OnParametersSetAsync`), `:281` (catch del caricamento), `:289`
(azzeramento in `Salva`), `:299` (catch di `Salva`), `:321` (`avviso = "Salvata."`), `:334`
(permesso revocato), `:353` (`avviso = "Caricata la versione più recente."`), `:360`
(azzeramento in `Sovrascrivi`), `:369` (validazione di `Sovrascrivi`), `:383` (catch di
`Sovrascrivi`), `:393` (azzeramento in `Elimina`), `:401` (eliminazione rifiutata), `:405`
(catch di `Elimina`).

**Punti di render — tre:** `:33` (`errore`, nel ramo `spesa is null`, **non toccato**), `:126`
(`errore`) e `:130` (`avviso`), entrambi nel ramo del modulo, ora **subito sopra**
`<div class="azioni">` a `:133`, fra la chiusura del `<fieldset>` della categoria a `:122` e i
pulsanti.

**Il secondo canale: cercato, trovato, e deliberatamente NON spostato.** Esistono due
`<p class="errore-campo">` — `:77` per l'importo e `:98` per la data — e a prima vista sono il
caso dell'unità 04. Non lo sono, e il criterio non me lo sono inventato: l'ha scritto l'unità 05
in `Pages/CollectionEdit.razor:205-206`, testualmente, *«Ora tutto ciò che «Salva» produce sta
qui, attaccato al pulsante che lo produce»*. I due `errore-campo` di questa pagina **non li
produce `Salva`**: sono calcolati dal markup a ogni render da `Denaro.Verifica(importoTesto)` e
da `DataRagionevole(data)`, mentre si digita, e stanno dentro la propria `<label class="campo">`.
Su `CollectionEdit` invece `erroriValidazione` è una lista **assegnata dentro `Salva()`**
(`CollectionEdit.razor:592`), cioè l'esito di una pressione del pulsante mostrato lontano dal
pulsante: tutt'altro oggetto. In più la forma identica — stessa classe, stessa posizione sotto
il campo — è già usata dalla pagina sorella `Pages/Spese.razor:66,85,89` nel modulo con cui le
spese si creano: spostarli qui e non là avrebbe fatto divergere due moduli che condividono gli
stessi quattro campi.

Ho chiesto esplicitamente a `conformity` di contraddirmi su questo punto. Ha riaperto entrambi i
file e ha confermato la distinzione (v. `ADJUDICA`, punto 5).

Il messaggio di validazione che `Salva`/`Sovrascrivi` **producono davvero** — «Controlla
importo, descrizione e data prima di sovrascrivere.», `:369` — scrive su `errore`, quindi si è
spostato insieme al blocco. Nessun terzo canale.

## LA TESTATA

`<TestataPagina Titolo="@TitoloSchermata">` a `:11-17`, fuori da tutti i rami e subito sotto
`<PageTitle>`, come nelle tre gemelle. API consumata, non modificata.

Il pannello `<Aiuto>` ha tre paragrafi, come le tre gemelle. Il secondo (il conflitto) e il terzo
(l'uscita con modifiche non salvate) sono **volutamente** le stesse frasi delle gemelle, adattate
al soggetto: sono la parte del contratto che si spiega allo stesso modo ovunque. Il primo è
nuovo, e l'ho scelto applicando la lezione che l'unità 04 si è presa come rilievo fondato — dire
ciò che la pagina **non** dice già da sé:

> Importo, categoria e data rifanno il riepilogo del mese nel registro delle spese: cambiando la
> data la spesa passa nel mese di quella data, e sparisce dal totale e dalle percentuali del mese
> in cui stava prima.

È il comportamento più invisibile che questa pagina abbia: il riepilogo mensile — totale,
percentuali per categoria, confronto col mese precedente — **non compare da nessuna parte su
questa schermata**, vive solo in `Pages/Spese.razor` e in `Home.razor`, e la data è l'unico campo
che può far sparire una spesa da un mese intero. Non ho scritto niente sulla regola dei permessi:
quella è già a schermo in `<p class="spiega">` esattamente per chi la subisce, e ripeterla
nell'aiuto sarebbe stato il rilievo dell'unità 04 rifatto tale e quale.

L'ho fatto verificare per **verità**, non solo per forma: `conformity` ha riaperto
`Services/CalcoliSpese.cs` e `Pages/Spese.razor` e ha confermato che il filtro per anno/mese e il
calcolo delle quote fanno ciò che il paragrafo dichiara.

## ADJUDICA

Revisori lanciati, tutti e tre **nello stesso messaggio**: `bug-hunter`, `conformity`,
`threat-hunter`. Ciascuno con diff, brief e i propri materiali: a `conformity` i tre file
omologhi più `PaginaEditor`, `TestataPagina` e — in più rispetto alle unità precedenti — la
pagina sorella `Pages/Spese.razor`, che con questa condivide i quattro campi e la classe
`errore-campo`; a `bug-hunter` l'unico call-site che entra nella pagina (`Pages/Spese.razor:188`,
verificato da me con un grep: non esiste nessun `NavigateTo` verso questa rotta in tutto il
progetto) più i componenti figli; a `threat-hunter` `Program.cs`, la migration con le policy RLS
su `expenses` e le tre pagine omologhe.

**`backend-expert` non lanciato, ed è una scelta dichiarata.** Il gate lo vuole su superficie
nuova, diff > ~120 righe o richiesta esplicita: qui il diff è di 48 righe, non nasce nessun tipo,
nessun servizio, nessuna astrazione — `TitoloSchermata` è una proprietà privata a due call-site
già stabilita da tre unità precedenti, non una superficie. L'unità *consuma* un contratto scritto
altrove e il budget vietava di ridisegnare. Stessa scelta e stesso ragionamento delle unità 04 e 06.

**`threat-hunter` lanciato** benché il pattern fosse già passato tre volte, perché il diff porta
testo scritto dall'utente — la descrizione della spesa, che qualunque membro dello spazio può
scrivere — in un punto di render **nuovo**, l'`<h1>` di `TestataPagina`, e compone un `href` a
runtime. Il mandato dice di lanciarlo in caso di esitazione.

    istruttoria: 0 rilievi su 0 file → checker no

`bug-hunter` 0, `conformity` 0, `threat-hunter` 0 (e i suoi non entrerebbero comunque nella somma
per regola). Sotto entrambe le soglie, e non per stretta misura: la somma è zero.

**Nessun rilievo da adjudicare, e nessun campione da riverificare.** Il §5 chiede almeno un
infondato riaperto per unità *quando ce ne sono*: qui non ne è arrivato nessuno, né fondato né
infondato. Lo dichiaro invece di ometterlo.

**Sui numeri di riga di `threat-hunter`.** Il mandato avvertiva che erano risultati sfasati sulle
unità 04 e 05 e di non riportarne nessuno senza riaprirlo. Li ho riaperti tutti e cinque
(`SpesaEdit.razor:141` per «Chiudi», e `:281`, `:299`, `:383`, `:405` per le interpolazioni di
`ex.Message`): **questa volta sono tutti esatti**. Lo scrivo perché un'avvertenza che non si
aggiorna diventa un pregiudizio.

### Le verifiche indipendenti che ho fatto io

Tre «0 rilievi» su un diff che nessuno aveva mai istruito non li ho accettati così com'erano.

**1. La domanda più rischiosa del mio diff, e la sola specifica di questa pagina: `Cambiata` è
più larga del gate di «Salva». Verificata, regge, ma cambia il comportamento.** Sulle tre gemelle
`Cambiata` e la condizione che accende «Salva» quasi coincidono. Qui no: `ImportoValido`
(`:204`) è una soglia **separata** e più stretta, quindi esiste uno stato — importo non
interpretabile, o data svuotata — in cui `Cambiata` è vera, la guardia è **armata**, e «Salva» è
**spento**. Ho ripercorso il percorso: l'utente preme «Chiudi», `GuardaUscita` chiede conferma,
«Annulla» resta in pagina, «OK» esce perdendo modifiche che non erano comunque salvabili. Non è
una trappola — non esiste uno stato da cui non si esca — ma è un'interazione che nessuna delle
tre gemelle ha nella stessa forma, e per questo è la prova 1 del browser.

**2. Il membro che non può intervenire non riceve mai la domanda. Verificato sul markup.**
Con `PuoIntervenire` falso i quattro campi legati sono tutti `disabled` (`:72`, `:82`, `:93`,
`:110`, `:119` nella numerazione di partenza): nessuno dei quattro valori confrontati da
`Cambiata` può cambiare, quindi `Cambiata` resta falsa per costruzione e sia `GuardaUscita` sia
`ConfirmExternalNavigation` restano inerti. Chi apre la spesa di un altro esce sempre senza
domande. È una proprietà che discende dai permessi, non dalla guardia, e non l'aveva isolata
nessuno dei tre revisori.

**3. Un difetto vero, preesistente, che ho trovato cercando l'interazione fra la guardia e la
validazione — dichiarato in `FUORI SCOPE` e non corretto.** V. sotto: è il punto su cui ho dovuto
ragionare di più, ed è l'unica cosa che questa unità lascia aperta.

## FUORI SCOPE

**1. Una spesa da mille euro in su non si può salvare, e il difetto è preesistente.**

`Pages/SpesaEdit.razor` riempie il campo importo con `importoTesto = Denaro.Testo(spesa.Amount)`
— tre punti: `:271` (caricamento), `:317` (dopo un salvataggio riuscito), `:348` (dopo
«Ricarica»). Poi `ImportoValido` (`:204`) rilegge **quella stessa stringa** con
`Denaro.Prova(importoTesto, out _)`.

`Denaro.Testo(1284.50m)` produce `"1.284,50"` — punto alle migliaia. `Denaro.Verifica` rifiuta
come `NonNumerico` qualunque stringa con più di un separatore (`Services/Denaro.cs:80`,
`if (pulito.Count(c => c is ',' or '.') > 1) return EsitoImporto.NonNumerico;`). Quindi, aprendo
una spesa di 1.284,50 €:

- sotto il campo importo compare subito «Non è un importo valido: usa la virgola o il punto.»,
  **senza che l'utente abbia toccato niente**;
- «Salva» è spento (`ImportoValido` falso), e «Sovrascrivi» pure;
- la spesa **non è più modificabile da nessuno**.

L'asimmetria del helper è **voluta e già coperta da un test**:
`Eton.Tests/DenaroTests.cs:174-180`, `Testo_e_Prova_non_sono_l_uno_l_inverso_dell_altro_sopra_il_migliaio`,
il cui commento dice che i due metodi «servono due direzioni diverse — uno legge ciò che una
persona digita, l'altro mostra un valore già validato». Il test documenta l'asimmetria del
helper; **non sanziona questo call-site**, che è esattamente il punto in cui le due direzioni si
toccano: `Testo` scrive in un campo *modificabile* e `Prova` lo rilegge. Sotto i mille euro non
si vede, perché non c'è nessun separatore delle migliaia — la stessa soglia che il commento del
test indica come l'unica in cui il difetto si manifesta.

**Perché non l'ho corretto.** Il rimedio sta in `Services/Denaro.cs` (far accettare a `Verifica`
il separatore delle migliaia che `Testo` produce, oppure dare a `Testo` una forma senza gruppi
per l'uso in un campo di input): **fuori dal mio perimetro**, che è un file solo. Correggerlo
dentro la pagina avrebbe richiesto un helper nuovo — vietato dal budget — e avrebbe lasciato
rotta la stessa combinazione ovunque riappaia. È una decisione di progetto, e il §5 dice che
queste vanno all'utente, non risolte di nascosto.

**L'unico punto in cui questo difetto tocca il mio diff**, e va detto perché è il mio: chi cerca
di rimediare riscrivendo `1284,50` a mano rende `Cambiata` vera, e **da adesso** uscire produce
la domanda «hai modifiche non salvate» per una modifica che non cambia il valore. Prima del mio
diff quella stessa condizione accendeva solo un pulsante. Non ho introdotto il difetto; gli ho
dato un secondo sintomo.

**2. Le quattro interpolazioni di `ex.Message`** (`:281`, `:299`, `:383`, `:405`) portano a
schermo il testo grezzo di un'eccezione, che su PostgREST può contenere nomi di policy o
frammenti di schema. `threat-hunter` l'ha confermato **preesistente** riaprendo il diff, e non
l'ha contato come rilievo. Non è mio: la stessa forma è su tutte e quattro le pagine editor,
quindi se si decide di filtrare quei messaggi si fa in un colpo solo, non un'unità per volta.

**3. Nessun equivalente della riga «cosa manca per salvare».** L'unità 05 ha aggiunto a
`CollectionEdit` un `<p class="testo-tenue">` che spiega perché «Salva» è spento quando manca
solo `!Cambiata`. Qui non l'ho aggiunta: non è fra i quattro obiettivi del mandato, e su questa
pagina le cause di spegnimento diverse da `!Cambiata` hanno già ciascuna il proprio messaggio a
schermo (i due `errore-campo`, la riga `.spiega`, `SchedaConflitto`). Lo segnalo solo perché è
l'unica differenza rimasta fra le quattro pagine dopo questo giro, e la decisione se colmarla è
del capo.

## GATE

- `dotnet build -warnaserror --no-incremental` → **0 errori, 0 avvisi**. Ricompilazione completa
  e non incrementale di proposito: su un file con `@inherits` e `override` il gate serve proprio
  a intercettare il CS0108 che una build permissiva declasserebbe, e con i generatori di sorgente
  di Razor una build incrementale può non rigenerare la classe.
- `dotnet test --no-build` → **267/267 superati**, 0 non superati, 0 ignorati (243 ms). Esattamente
  il numero che il mandato dichiarava.

Compilato **io**, una volta sola, prima di dispacciare i revisori; i test dopo che tutti e tre
erano rientrati. Ai tre revisori e all'`implementer` ho vietato esplicitamente nel brief di
eseguire `dotnet build` e `dotnet test`, perché `obj/` non ha lock fra processi.

**Il lavoro non è committato**: il working tree porta `Pages/SpesaEdit.razor` modificato. Come
l'unità 06, lo lascio pronto e lo dichiaro invece di deciderlo da solo.

## SCOSTAMENTI

**Uno, e riguarda un numero di riga del mandato.**

La tabella dei tre commenti stantii dice che la riga cercata in `ItemEdit.razor` «è oggi la
**226**». Riaperto il file: `campi = SchemaCampi.Ordina(collezione.Fields);` è a
**`ItemEdit.razor:225`**, non 226. Il mandato riportava il numero dal resoconto dell'unità 06,
che era sfasato di uno. Ho scritto nel commento il numero che ho verificato io.

I tre riferimenti corretti, tutti riaperti da me sui file su disco e tutti puntati alla scrittura
di stato che il commento vuole citare — il blocco che resetta i campi fra la guardia di
generazione e la lettura:

| Citato prima | Cosa c'era davvero | Scritto adesso | Cosa c'è a quella riga |
|---|---|---|---|
| `NoteEdit.razor:200` | una riga vuota | `NoteEdit.razor:175-181` | il blocco `if (Nuova) { nota = null; titolo = corpo = ""; … }` |
| `CollectionEdit.razor:309` | un commento sulla tavolozza di emoji | `CollectionEdit.razor:385-394` | il blocco `if (Nuova) { collezione = null; nome = icona = ""; … }` |
| `ItemEdit.razor:214` | (riga spostata) | `ItemEdit.razor:225` | `campi = SchemaCampi.Ordina(collezione.Fields);` |

Nessuna delle tre citazioni è stata tolta: tutte e tre hanno ancora un bersaglio sensato. Ho
usato un intervallo per le prime due perché il bersaglio è un blocco di più assegnazioni, non una
riga sola — una riga sola avrebbe dovuto sceglierne una arbitrariamente fra quattro. Il mio diff
non tocca nessuno dei tre file, quindi i numeri restano validi dopo di me.

**Non divergono, e lo dichiaro perché il mandato lo chiedeva punto per punto:** la posizione di
`<NavigationLock>` dentro il ramo del modulo, la firma di `Cambiata` e il suo corpo invariato,
l'unico `Esci` con la destinazione preesistente e senza `replace`, la forma del gate di «Chiudi»
col `null` letterale, i due link dei rami di errore lasciati intatti, la posizione e l'API di
`<TestataPagina>`, la struttura a tre paragrafi dell'`<Aiuto>`, lo spostamento del blocco
`errore`/`avviso` a markup invariato carattere per carattere. Nessuna astrazione nuova, nessun
tipo nuovo, nessun metodo nuovo oltre alla proprietà `TitoloSchermata`, nessun file `.js`,
nessun servizio iniettato nuovo, nessun pacchetto, nessuna riga di `app.css`, nessuno stile
inline. Nessun file fuori dal perimetro è stato toccato.

Il mandato prevedeva il caso di un obiettivo in contraddizione con un divieto. **Si è
presentato una volta, e ho obbedito al più specifico.** L'obiettivo dice «se esiste un secondo
canale d'esito, portalo nello stesso posto»; il divieto di budget e di perimetro dice di non
ridisegnare niente. I due `errore-campo` sono l'oggetto della contraddizione: ho applicato il
criterio più specifico — quello scritto dall'unità 05, «tutto ciò che *Salva* produce» — che li
esclude, invece della formulazione generica «messaggi d'esito», che li avrebbe inclusi. La scelta
è argomentata sopra e verificata da `conformity` contro entrambi i file.

**Il server di sviluppo non è stato avviato** e nessuna prova è stata fatta nel browser, come da
mandato. Nessun processo lasciato vivo, nessuna porta occupata.

## DA PROVARE NEL BROWSER

Le prove generali dell'adozione del contratto **non le ripeto**: sono già scritte tre volte, in
`handoff/03-contratto-editor/resoconto.md` (le dieci di `NoteEdit`),
`handoff/04-collezione-contratto/resoconto.md` (le cinque) e
`handoff/06-elemento-contratto/resoconto.md` (le sei). Su questa pagina cambiano solo gli
indirizzi: si entra da `/expenses` cliccando una riga del registro, la rotta è `/expenses/{id}`,
e «Chiudi» torna a `/expenses`. Qui sotto **solo ciò che è specifico di `SpesaEdit`**, e in
particolare ciò che il mandato dice non essere ancora stato provato da nessuno: come la guardia
si comporta su un modulo di spesa **condivisa**.

**1. La guardia armata contro il pulsante spento — l'unica pagina in cui i due non coincidono.**
Aprire una spesa **sotto i mille euro** (v. prova 2 per il perché). Cancellare l'importo e
scrivere `abc`, oppure svuotare la data con la «×» del campo. Premere «Chiudi».
*Accettazione*: «Salva» è spento e sotto il campo c'è il messaggio rosso, **ma la domanda «Hai
modifiche non salvate…» arriva lo stesso**. «Annulla» deve riportare al modulo con il testo
sbagliato ancora dentro; «OK» deve uscire. Se la domanda **non** arrivasse, `Cambiata` non si
sta leggendo nell'istante giusto e il contratto va riaperto. È lo stato che nessuna delle tre
gemelle ha nella stessa forma.

**2. La spesa da mille euro in su — conferma del rilievo fuori scope, non della mia unità.**
Segnare da `/expenses` una spesa di `1284,50`, poi aprirla.
*Accettazione attesa oggi*: sotto l'importo compare **subito** «Non è un importo valido: usa la
virgola o il punto.» e «Salva» è spento senza aver toccato niente. Se è così, il rilievo è
confermato e va deciso dall'utente. Poi, nella stessa schermata, riscrivere l'importo come
`1284,50` senza il punto e premere «Chiudi»: la domanda della guardia deve arrivare. È l'unico
punto in cui il difetto preesistente e il mio diff si toccano.

**3. Il membro che non può intervenire non deve ricevere nessuna domanda.** Con due account dello
stesso spazio, dove chi guarda **non** è il pagante e **non** possiede lo spazio: aprire la spesa
dell'altro.
*Accettazione*: tutti i campi grigi, la riga «Questa spesa l'ha segnata qualcun altro…», nessun
pulsante «Elimina», e «Chiudi» che esce **senza nessuna domanda**, subito. Se comparisse la
domanda, `Cambiata` sta diventando vera su una pagina in sola lettura, ed è un difetto.

**4. L'esito sopra i pulsanti su una spesa condivisa, e il conflitto fra due membri.** È la prova
che il mandato dice mancare a tutte e quattro le pagine. Due browser, due account dello stesso
spazio, entrambi con la **stessa** spesa aperta (pagata da chi fa la seconda mossa, o entrambi
proprietari dello spazio). A cambia la descrizione e salva. B cambia l'importo e salva.
*Accettazione*: a B compare `SchedaConflitto` con «Mentre scrivevi, questa spesa è cambiata.».
Premendo «Ricarica», il messaggio «Caricata la versione più recente.» deve comparire **appena
sopra i pulsanti**, non in cima al modulo — e i campi devono mostrare la descrizione scritta da A.
Premendo invece «Sovrascrivi», «Salvata.» deve comparire **nello stesso punto**, e riaprendo la
pagina da A si deve vedere il valore di B.

**5. La testata dice ciò che la pagina non dice — e il primo paragrafo va provato davvero.**
Su una spesa qualunque, aprire il «?».
*Accettazione*: tre paragrafi. Il primo è l'unico che descrive un comportamento **invisibile da
questa schermata**, quindi si prova: annotare il totale del mese e le percentuali mostrate su
`/expenses`, poi aprire una spesa di quel mese, **cambiarne la data portandola al mese
precedente**, salvare, tornare a `/expenses`.
*Accettazione*: la spesa non compare più nell'elenco del mese corrente, il totale è diminuito del
suo importo, le percentuali per categoria sono ricalcolate, e premendo «‹» la spesa compare nel
mese precedente. Se non fosse così, il paragrafo dell'aiuto sta mentendo e va riscritto.

**6. La spesa sparita sotto i piedi, con la testata nuova sopra — il caso su cui si regge il
divieto di spostare `<NavigationLock>` fuori dai rami.** Due schede sulla stessa spesa (propria).
Nella prima eliminarla; nella seconda modificare l'importo e premere «Salva».
*Accettazione*: nella seconda scheda compare «Questa spesa non c'è più: l'ha cancellata
qualcuno…», la testata **resta in cima** e l'`<h1>` continua a mostrare la descrizione dell'ultima
versione conosciuta — che è corretto, non è un titolo vuoto — e premendo «Torna alle spese» esce
**senza nessuna domanda**, benché il modulo modificato sia ancora in memoria. Se comparisse la
domanda, `<NavigationLock>` è finita fuori dal ramo del modulo.

**Cosa non è praticabile a mano, e perché.** Il dirottamento della navigazione tardiva — premere
«Elimina» e andarsene dalla barra di navigazione mentre la DELETE è ancora in volo, che è il caso
che `Esci` chiude e `NavigateTo` non chiudeva — richiede una finestra di rete che a mano non si
coglie in modo affidabile. Con la strozzatura della rete negli strumenti per sviluppatori si può
tentare; l'accettazione è: **si resta sulla pagina di destinazione**, senza essere riportati a
`/expenses` un istante dopo. È già dichiarato come limite nel resoconto dell'unità 03 e non lo
riapro.

PRONTO PER LIVE-TESTING: sì
