# UNITÀ: 2 — ESITO: FATTO

Cinque voci chiuse: quattro con codice, una dichiarando che non è un difetto. Nessuna `BLOCKED`,
niente da adjudicare per te tranne una decisione di testo che ti lascio in fondo.

Il lavoro sta nel branch `worktree-unita-02-voti-e-recensioni`, in due commit: `4f0840f` (il lavoro
dei due implementer) e `68e4aff` (la correzione del solo rilievo fondato).

---

## TOCCATI

    Pages/ItemEdit.razor              → +12/−1
    Shared/RecensioniElemento.razor   → +58/−8
    Shared/VotoInput.razor            → +10/−2
    wwwroot/css/app.css               → +11/−2

Nessun file nuovo. `Pages/CollectionDetail.razor` e `Services/CalcoliVoti.cs` erano nel perimetro e
**non sono stati toccati**: la voce che li riguardava si è chiusa senza codice.

---

## REVIEW

    review:
      bug-hunter      RILIEVI: 1
      conformity      RILIEVI: 0
      threat-hunter   RILIEVI: 0
      backend-expert  non lanciato — 4 files changed, 91 insertions(+), 13 deletions(-)
                      · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: fondati 1 · infondati 0 · fuori scope 0 · non verificabili 0
      checker (fix)   VERDETTI: risolto 1 · non risolto 0 · non verificabili 0

Il conteggio delle dichiarazioni viene dal comando del §3 eseguito dopo `git add -N .`, a
implementer rientrati; `git status --porcelain | grep '^??'` non ha restituito niente, quindi
`--summary` non era cieco su nessun file nuovo.

**Due note sul come, perché il tracciato non le contiene e servono a chi audita.**

I revisori li ho lanciati **una volta sola sul diff combinato** delle due unità di implementazione,
non due volte separatamente. Il gate sul combinato (91 insertions) chiede tutti e tre i revisori,
mentre l'unità B da sola (12 insertions) ne avrebbe chiesti due: è un superset, nessuna revisione
saltata, e `conformity` ha guardato anche `ItemEdit.razor` senza esservi obbligato.

Il `checker` compare **due volte** perché il §5 chiede di rilanciarlo sulla correzione: il secondo
lancio non istruisce un rilievo nuovo, istruisce il `FIX`.

---

## CONTRATTI

Il mandato non chiedeva nessuna firma nuova esposta ad altre unità, e infatti non ce n'è. L'unica
firma nuova del diff è privata e non esce dal file:

    Shared/RecensioniElemento.razor:565   private void Registra(Review aggiornata)

Nasce con due call-site — il `case EsitoSalvataggio.Conflitto` e `Aggiorna`, che ora la chiama
invece di duplicarne il corpo. `conformity` ha verificato che non debba invece convergere col
`catch` di `Crea`: là si assegna la **lista intera** ri-letta dal server (`recensioni = elencate!`),
qui si fonde **una riga sola** già in mano, e farci passare anche quel ramo avrebbe buttato via la
freschezza delle altre righe.

I due vincoli di forma sono rispettati:

1. **Rimandi ancorati al selettore o a un frammento cercabile.** Nessun numero di riga compare nel
   diff: il commento CSS nuovo rimanda a `Shared/RecensioniElemento.razor, class="voto-grande"`,
   quello di `VotoInput` alla `<label for>` «nel markup qui sopra», quello del conflitto al
   `<button>` di «Salva recensione». Verificato da `conformity`, punto 5 del suo rapporto.
2. **Testo cambiato, cercato altrove prima.** È il vincolo che ha cambiato il rimedio della voce 8:
   v. la sezione «il gemello» più sotto.

---

## ADJUDICA

**Un solo rilievo in tutta l'unità.**

`bug-hunter` · `Shared/RecensioniElemento.razor:476-477` · TIPO logica · **fondato → corretto**

> Dopo un `Conflitto` in cui la riga del server porta lo **stesso** contenuto che è già nel modulo,
> `Registra` allinea `mia` ma non tocca `mioVoto`/`mioCommento`; `Cambiata` diventa falso, il
> pulsante — `disabled="@(occupato || !Cambiata)"` — si spegne, e a schermo resta un avviso che
> dice «premi Salva di nuovo».

**verificato: risolto — `Shared/RecensioniElemento.razor:491-493`**

Il `checker` ha aggiunto due cose che il revisore non aveva:

- **un percorso più corto di quello descritto**, che non richiede nessun guasto — due schede aperte
  sulla stessa recensione, la stessa modifica fatta in entrambe, e la seconda riceve un conflitto
  il cui contenuto è identico al proprio;
- **la natura del difetto**: è una **regressione introdotta da questa unità**, non un debito
  pregresso. Prima il ramo chiamava `Aggiorna`, che riallineava sempre il modulo, quindi `Cambiata`
  era sempre falso dopo un conflitto — e l'avviso di allora non chiedeva niente. Il difetto nasce
  nel momento in cui il messaggio comincia a **chiedere un'azione**. Andava corretto qui, ed è
  stato corretto qui.

**Il fix non è quello che il revisore proponeva.** Lui suggeriva di confrontare a mano
`mioVoto`/`mioCommento` con la riga ricevuta; il codice invece legge **`Cambiata`**, cioè la stessa
espressione che governa il pulsante. Così il testo e il pulsante non possono divergere per
costruzione, mentre con un confronto a parte potrebbero — al primo che ritocca una delle due parti.
Letta **dopo** `Registra`, perché è lì che `mia` diventa la riga del server.

**Riverifica a campione sugli infondati: non applicabile, gli infondati sono zero.** Ho comunque
aperto io il codice, come il §5 impone per ogni rilievo che tocchi la concorrenza qualunque sia il
verdetto: letto `Cambiata` (`:201-203`), l'attributo `disabled` del pulsante (`:73`) e il ramo
`Conflitto` per intero dopo il fix.

---

## FUORI SCOPE

**Niente per la voce 26**, ed è la notizia: il rimedio è stato trovato **dentro** entrambe le
decisioni del 19 settembre, quindi non c'è nessuna revoca da adjudicare. Il pulsante «Sovrascrivi»
resta spento dal solo permesso (`SovrascriviAbilitato="@PuoIntervenire"`, non toccato) e il
messaggio della guardia sopravvive ancora alla correzione del campo: cambiano **solo le parole**,
da «Il nome dell'elemento non può essere vuoto.» a «Controlla il nome prima di sovrascrivere.» È il
precedente che il mandato indicava — `Pages/SpesaEdit.razor` dice «Controlla importo, descrizione e
data prima di sovrascrivere.» — e `conformity` ha verificato che le due frasi siano della stessa
famiglia.

**Una cosa sola, minore, che ti lascio perché tocca un testo che non era nel mio mandato.**

Il pulsante della recensione si chiama **«Salva recensione»** (`Shared/RecensioniElemento.razor:74`),
ma due avvisi dicono «premi **Salva** di nuovo»: quello nuovo del conflitto (`:492`) e quello
preesistente del `catch` di `Crea` (`:437`, «Avevi già recensito questo elemento altrove…»). Dentro
la scheda «La tua recensione» la forma breve è inequivocabile, e il messaggio nuovo è coerente col
suo vicino di venti righe. Ma un'azione che cambia nome fra il pulsante e il testo che lo nomina è
un difetto, e allinearli richiederebbe di toccare **anche** il messaggio di `Crea`, che nessuna
voce ha segnalato: due stringhe, nessun rischio, ma è scope che non era mio. La riga d'aiuto nuova
in `ItemEdit` scrive invece «Salva recensione» per esteso, e lì è obbligatorio: su quella schermata
c'è anche il «Salva» dell'elemento.

---

## GATE

    dotnet build Eton.sln -warnaserror --no-incremental   → Avvisi: 0   Errori: 0
    dotnet test Eton.sln                                  → Superati: 310, Non superati: 0

Eseguiti **due volte**: dopo il lavoro degli implementer e di nuovo dopo la correzione del rilievo.
Entrambe le volte con lo stesso esito. Nessun test aggiunto: il diff non introduce logica pura —
`Services/CalcoliVoti.cs` non è stato toccato, e nel progetto non esistono test di componenti Razor.

---

## SCOSTAMENTI

**Il §7 non l'ho eseguito, ed è il mandato a prescriverlo.** «Non avviare il server di sviluppo: è
vivo sulla porta 5000, avviato dal capo. La prova nel browser della tua correzione la fa il capo
dopo l'integrazione.» Quindi niente `live-testing` e niente `ui-critic` da parte mia, benché il
diff tocchi l'interfaccia e il §0 abbia prodotto un metro (`PIANO-DESIGN`, v. sotto). Le misure che
servono per quella prova sono nella sezione seguente, scritte perché tu le ricopi invece di
inventarle.

**Nessun lavoro nuovo è arrivato durante l'unità**, da nessun canale.

**Un fatto della ricognizione smentito**, ed è nella sezione «il gemello» qui sotto: vale la pena
leggerla prima di girare la voce 8 alla 04.

---

## IL METRO DEL §0, PER CHI LEGGERÀ IL DIFF

Il brief toccava `app.css`, quindi il ramo «nessun sito da imitare» del §0 si applicava e ho
invocato `frontend-design`. Il piano è in sessione; qui basti la parte che serve a giudicare la
resa, ed è un metro **scelto**, non misurato — `PIANO-DESIGN`, non `SPECIFICA-UI`, quindi uno
scostamento è una decisione da discutere, non un errore da correggere:

- il colore dell'assenza è `--testo-fioco` (#8a8a8a), **non** `--secondario`: il verde è «dove si
  constata un dato», e l'assenza di dato non è un dato;
- il corpo scende di un gradino, da `--t-3xl` (52px) a `--t-2xl` (36px);
- nessuna crenatura negativa e nessun alone: i −.09em stringono la virgola fra due cifre, e
  l'unico `text-shadow` su del testo in tutta l'applicazione resta riservato alla cifra per cui si
  apre la pagina;
- nessuna parola nuova a schermo: la frase accanto dice già «nessun voto, 1 commento».

---

## LA MISURA ATTESA PER IL COLLAUDO

Una voce per riga: cosa si deve vedere, con quale valore. La radice del foglio è a
`font-size: 16px`, quindi i `rem` si leggono per sedici.

**2a — il cursore ha un nome.** Sulla schermata di un elemento esistente, il cursore del voto porta
`id="voto-<32 esadecimali>"` e l'etichetta accanto è una `<label>` con `for` **uguale a quel valore**.
Il nome accessibile risultante è `Voto`. Nessun `aria-label` sul controllo: se ce ne fosse uno
sarebbe un difetto nuovo, non un rafforzamento.

    document.querySelector('.voto-input input[type=range]').id
      === document.querySelector('.voto-input label').getAttribute('for')   → true

**7 — l'assenza di voto ha il suo segnaposto.** Serve un elemento con **almeno una recensione che
abbia commento e nessun voto** (si ottiene anche togliendo il voto a una recensione e tenendo il
commento). In cima alla scheda, al posto del voto:

    elemento:  span.voto-assente   testo "—"
    font-size  36px        (non 52px)
    color      rgb(138, 138, 138)   (non rgb(182, 243, 106))
    text-shadow  none
    accanto:   "/ 10 — nessun voto, 1 commento"

Il controllo che conta davvero è `text-shadow: none` insieme a `36px`: erano l'alone e il corpo
display a far sembrare quel trattino una barra verde rotta.

E il caso da non rompere: **con almeno un voto** lo `<span>` deve tornare a essere
`span.voto-grande`, 52px, `rgb(182, 243, 106)`, con l'alone.

**8 — il conflitto non mente e non butta via niente.** Due salvataggi ravvicinati della propria
recensione:

- l'avviso **non** contiene più la parola «altrove»;
- se il testo che hai in mano è **diverso** da quello salvato, comincia con «Questa recensione
  risultava già salvata in una versione più recente…» e finisce con «premi Salva di nuovo per
  sostituirla», e **il pulsante è premibile**;
- se è **identico**, comincia con «Questa recensione era già salvata identica a quella che hai in
  mano…» e finisce con «Non c'è altro da fare.», e il pulsante è **spento**: è coerente, non è un
  difetto;
- in **entrambi** i casi la `<textarea>` del commento e la posizione del cursore **non cambiano**.
  È la misura più importante delle tre: prima il conflitto sovrascriveva quello che l'utente aveva
  appena digitato.

Il pannello di aiuto della schermata ha ora **quattro** paragrafi, e il terzo comincia con «La
recensione qui sotto si salva a parte».

**26 — la frase non è più doppia.** Con la scheda di conflitto aperta sull'elemento e il nome
svuotato, premendo «Sovrascrivi»:

    document.body.innerText.split("Il nome dell'elemento non può essere vuoto.").length - 1   → 1

e il riquadro rosso dice «Controlla il nome prima di sovrascrivere.» Prima quel conteggio dava 2.

**10** — nessuna misura: non è stato toccato niente.

---

## L'ESITO DELLA 10, IN UNA RIGA

**Non è un difetto.** Il menù ha già la seconda voce, e l'unica condizione che la spegne è il voto
al buio: `Pages/CollectionDetail.razor:205` dice
`private bool OrdinePerVotoPossibile => collezione is not null && !collezione.Blind;` — e dentro
quel ramo del markup `collezione` non è mai nullo, perché ci si arriva solo dopo il ramo
`else if (collezione is null)`. Quindi il flag **è** `!Blind`, e nient'altro.

**Il fatto che lo dimostra** sta nella ricognizione stessa, al suo secondo capoverso: l'agente
dichiara di aver lavorato su una collezione «con **voto al buio attivo**». La schermata di prova
non aveva le condizioni, esattamente come il mandato sospettava.

E il flag non è troppo restrittivo: su una collezione cieca le righe altrui non arrivano affatto
finché non hai votato tu, quindi `Media` vale `null` per quasi tutti gli elementi, e ordinare per
voto metterebbe in fondo — insieme a quelli che nessuno ha giudicato — proprio gli elementi con tre
voti alti che non puoi ancora vedere. È la stessa dottrina applicata a `votiMancanti` venti righe
sopra: *un comando che risponde senza fare quello che promette è peggio di un comando assente*.
L'aspettativa che la ricognizione nomina — «con una collezione di birre ordinare per voto è
naturale» — su una collezione **non** cieca è già soddisfatta oggi.

---

## IL GEMELLO DEL TESTO D'AIUTO: **no, non ha lo stesso difetto** — e non ce l'ha nemmeno il mio

Letta su `Pages/CollectionEdit.razor:14`. La riga è:

> «Se qualcun altro salva questa collezione mentre la stai modificando, il salvataggio si ferma e
> ti fa scegliere fra ricaricare la sua versione e sovrascrivere con la tua.»

**Quella frase esiste in quattro file** — `ItemEdit:15`, `CollectionEdit:14`, `NoteEdit:14`,
`SpesaEdit:14` — declinata sull'entità della pagina, **ed è vera in tutti e quattro**: per tutte e
quattro quelle entità esiste davvero una `SchedaConflitto` con la scelta fra Ricarica e Sovrascrivi.

**Quindi la ricognizione ha letto una frase vera come se fosse falsa.** Diceva che «il
comportamento non corrisponde a ciò che l'aiuto promette»: l'aiuto non parlava della recensione,
parlava dell'elemento, e sull'elemento la scelta c'è. Il difetto vero è **un'assenza**, non una
bugia — quella schermata rende anche `<RecensioniElemento>`, la recensione si salva con un'altra
regola, e l'aiuto non la nominava affatto. Chi legge applica la frase a tutto ciò che la schermata
salva.

**Conseguenza sul rimedio, ed è il motivo per cui questa sezione conta più di quanto sembri.** Non
ho toccato quella riga: cambiarla in un posto solo l'avrebbe fatta divergere dalle altre tre, che è
esattamente la classe di difetto del contratto 2. Ho **aggiunto** un paragrafo, che non ha gemelli
e non ne può far divergere nessuno.

**Per la 04**: il gemello in `CollectionEdit` non ha niente da correggere. Se qualcuno le passasse
la voce 8 come «l'aiuto mente anche qui», quella premessa è falsa — e la collezione, a differenza
dell'elemento, non contiene recensioni, quindi non le serve nemmeno il paragrafo che ho aggiunto
io.

---

## UNA COSA DA SAPERE SUL CONFLITTO, PER CHI TORNERÀ QUI

Il falso conflitto osservato in collaudo — «due salvataggi ravvicinati, un solo membro» — ha una
spiegazione che il codice regge e che nessuno ha ancora potuto confermare nel browser: **la risposta
di un salvataggio riuscito che non arriva**. `SalvaAsync` scrive e legge nella stessa chiamata; se
l'UPDATE passa ma la risposta si perde, `Modifica` cade nel `catch` generico e `mia` resta alla
versione vecchia col modulo intatto. Il salvataggio successivo trova zero righe, rilegge, vede una
versione diversa e la classifica `Conflitto` — un conflitto con se stessi.

Il messaggio nuovo nomina questa causa insieme all'altra, senza sceglierne una: è il motivo per cui
dice «oppure può essere un salvataggio di poco fa che qui non era stato registrato». Se la prova nel
browser dovesse mostrare che il falso conflitto **non si ripresenta più**, la causa potrebbe essere
stata un'altra ancora — il doppio montaggio che la 01b ha appena tolto — e in quel caso il messaggio
resta comunque vero, perché non afferma quale dei due casi sia.
