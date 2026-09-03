UNITÀ: 13/13 — Le altre cinque schermate smettono di mostrare il JSON di PostgreSQL

## PERCHÉ ESISTI

Non eri nel piano. Sei nata da un **censimento** fatto il 3 settembre sera, quando è emerso che
i rilievi 3 e 9 risultavano «fatti» ma erano stati chiusi **su una pagina sola su sei** — quella
che stava nel perimetro dell'unità che li ha scritti. La mappa del piano tracciava *file →
unità*, non *rilievo → unità*, e un rilievo chiuso da qualche parte risultava chiuso ovunque.

Sei l'ultima unità che tocca il codice C#. Dopo di te c'è solo il foglio di stile, poi il
browser.

## OBIETTIVO

**Rilievo 3 — l'errore parla italiano.** Diciotto punti in cinque file mostrano all'utente il
testo grezzo di un'eccezione. Il modello **esiste già ed è codice**: le sei frasi che l'unità 05
ha scritto in `Pages/CollectionEdit.razor`, e le frasi che l'unità 10 ha appena scritto in
`Shared/RecensioniElemento.razor`.

**Rilievo 9 — il pulsante spento dice cosa manca.** L'unità 05 ha aggiunto a `CollectionEdit`
un `<p class="testo-tenue">` che spiega perché «Salva» è spento. Gli altri editor non ce l'hanno.

**Lo stato vuoto di `CollectionDetail`.** L'unità 09 ha portato l'azione sotto il messaggio su
`/notes` e `/collections`; `Pages/CollectionDetail.razor:54-58` ha lo **stesso identico blocco**
— icona, `<p>`, `<p class="spiega">` — e resta **senza pulsante**. È l'ultima schermata con la
forma vecchia.

## PERIMETRO — file di tua proprietà esclusiva

- `Pages/NoteEdit.razor`
- `Pages/ItemEdit.razor`
- `Pages/SpesaEdit.razor`
- `Pages/SpaceDetail.razor`
- `Pages/CollectionDetail.razor`

Cinque file, tutti chiusi da unità precedenti e **riaperti apposta per te**. Nessun altro.

## IL CENSIMENTO, GIÀ FATTO DAL CAPO — verificalo, non fidartene

**I tre editor hanno la stessa identica quaterna**, ed è il fatto più utile del mandato:

| | aprire | salvare | sovrascrivere | eliminare |
|---|---|---|---|---|
| `NoteEdit.razor` | `:214` | `:235` | `:328` | `:351` |
| `ItemEdit.razor` | `:254` | `:340` | `:429` | `:452` |
| `SpesaEdit.razor` | `:284` | `:302` | `:386` | `:408` |

**`SpaceDetail.razor`** ne ha cinque, e sono azioni diverse: `:206` (caricare), `:236`
(rinominare), `:262` (rimuovere un membro), `:290` (uscire dallo spazio), `:316` (eliminare lo
spazio).

**`CollectionDetail.razor`** ne ha uno: `:242` (aprire la collezione).

**Non si toccano** le interpolazioni che finiscono in `Console.Error.WriteLine`: sono la
diagnostica. In `CollectionDetail` sono `:308` e `:322`; negli altri file cercale tu.

I numeri vengono da un `grep` del capo: **riaprili tu** prima di modificare, e se non tornano usa
quelli veri dichiarandolo in `SCOSTAMENTI`. L'unità 12 ha spostato righe in `SpesaEdit`.

## IL CONTRATTO — due fonti, e le apri entrambe

1. **`handoff/05-collezione-rilievi/resoconto.md`**, sezione `CONTRATTI`: le sei frasi verbatim,
   e `Pages/CollectionEdit.razor` per vederle in opera.
2. **`handoff/10-recensioni-errori/resoconto.md`**, sezione `CONTRATTI`: le frasi che l'unità 10
   ha scritto poche ore fa sullo stesso problema, ciascuna con la frase dell'unità 05 da cui
   discende. **Se l'unità 10 ha dovuto inventarne una senza corrispondente, l'ha dichiarato**:
   quella è la parte da guardare con più attenzione, perché è dove il modello non bastava.

La forma è **fatto, causa, azione**. E la frase che l'unità 05 ha reso il cuore del modello:

> «Quello che hai scritto è ancora qui: riprova fra un momento, e non chiudere la pagina.»

**Il criterio dell'unità 05, che vale anche per te:** tradurre senza registrare avrebbe barattato
un'indiscrezione con una cecità, e a pagarla sarebbe stato chi deve diagnosticare il prossimo
guasto. **Se una traduzione ti fa perdere il dettaglio tecnico, aggiungi un
`Console.Error.WriteLine`** — non tenere il JSON a schermo.

## LE DUE COSE CHE CAMBIANO FRA I FILE, E CHE NON VANNO APPIATTITE

**1. `SpaceDetail` non è un editor.** Le sue cinque azioni sono distruttive o sociali —
rimuovere un membro, uscire, eliminare lo spazio — e «quello che hai scritto è ancora qui» non
ha senso per nessuna di esse. **Le sue frasi vanno pensate**, non copiate: cosa è successo, cosa
non è successo, e cosa può fare adesso chi legge.

**2. `SpaceDetail` ha un caso che nessun altro file ha.** L'unità 08 ha istruito che sulle
recensioni il proprietario **non può moderare**: `reviews_update` e `reviews_delete`
(`supabase/migrations/20260812200000_recensioni.sql:127-134`) hanno la sola condizione
`user_id = auth.uid()`. Se un rifiuto del database su quella pagina può nascere da lì, il
messaggio deve dirlo. **Verificalo aprendo la migrazione**, non fidandoti di questo paragrafo.

## RILIEVO 9 — la riga che dice cosa manca

L'unità 05 ha aggiunto a `CollectionEdit` un `<p class="testo-tenue">` che spiega perché «Salva»
è spento quando manca solo che non ci sia niente da salvare. **Aprila e guarda la forma esatta.**

**Non applicarla dove non serve, e questo è un giudizio che devi fare tu file per file.**
L'unità 07 ha scritto, di `SpesaEdit`: le cause di spegnimento diverse da «niente da salvare»
hanno **già ciascuna il proprio messaggio a schermo** — i due `errore-campo`, la riga `.spiega`,
`SchedaConflitto`. Se su una pagina la riga sarebbe muta o ridondante, **non metterla e dillo**.
Un testo che ripete lo schermo è il rilievo che si è preso l'unità 04.

`SpaceDetail` e `CollectionDetail` non sono editor: il rilievo 9 **non li riguarda**.

## LO STATO VUOTO DI `CollectionDetail`

`Pages/CollectionDetail.razor:54-58`. L'unità 09 ha fatto la stessa cosa su due file: **apri
`Pages/Collections.razor` e copiala**, adattando il testo al soggetto (un elemento in una
collezione) e non la struttura.

Fatti già verificati dall'unità 09, da riverificare in un istante:
- `wwwroot/css/app.css:592` — `.vuoto .btn { margin-top: var(--s4); }`: **il caso è già previsto
  dal foglio di stile**, non serve una riga di CSS.
- `app.css:639` — `.btn { display: inline-flex; }`: dentro `.vuoto`, che è centrato, il pulsante
  si centra da sé e non si stende per tutta la larghezza.

**Il pulsante in testata resta**, come ha deciso l'unità 09 con `tech-advisor`: a elenco pieno il
blocco vuoto non esiste, e togliendolo non resterebbe nessun modo di creare. Se `CollectionDetail`
non ha un pulsante in testata, allora quello nel blocco vuoto è l'unico e **a maggior ragione ci
va**.

## NON TOCCARE

- **`Pages/CollectionEdit.razor`** e **`Shared/RecensioniElemento.razor`**: sono i **modelli**.
  Li **leggi**, non li modifichi.
- **`Pages/Notes.razor`, `Pages/Collections.razor`**: chiusi dall'unità 09, sono il modello per
  lo stato vuoto.
- **`Shared/PaginaEditor.cs`**: il contratto degli editor, chiuso da quattro unità. Il tuo lavoro
  non lo tocca in nessun modo.
- **`Services/**`**: nessun repository. L'unità 05 ha **rifiutato metà del proprio perimetro**
  proprio su questo, e aveva ragione: dentro `CreaAsync` sai quale query è fallita, non quale
  schermata la stava aspettando.
- **`wwwroot/css/app.css`**: unità 11, che viene **dopo di te**. Se ti serve, torna `BLOCKED` e
  la voce si accoda alle sue.

## BUDGET DI COMPLESSITÀ

Nessun tipo nuovo, nessun servizio, nessun file, **nessun helper di traduzione condiviso**.

**So che questo è il divieto che ti verrà più voglia di violare**, perché i tre editor hanno la
stessa identica quaterna e le tre traduzioni saranno quasi uguali. Il divieto resta, e il motivo
è quello dell'unità 05: il messaggio giusto dipende da *quale schermata stava aspettando*, e un
helper che prende un'eccezione e restituisce una stringa non lo sa. Ma **se dopo aver scritto le
tre quaterne scopri che sono identiche parola per parola tranne il soggetto**, quello è un fatto
nuovo e non un'impressione: **scrivilo in `FUORI SCOPE` come proposta al capo**, con le tre
quaterne a fronte. Non farlo, ma non tacerlo.

## STATO

Unità chiuse e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`), 05 (`e139ce8`),
06 (`f4f2dbd`), 07 (`4327598`), 12 (`8a4a89f`), 08 (`bdd858a`), 09 (`d05416b`), 10.

**Non committare.** Committa il capo, a resoconto letto.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: vince la riga più recente. C'è una riga
del 3 settembre sera che dice che **l'utente non è raggiungibile**: qualunque domanda tu abbia,
portala nel resoconto.

**Due fatti operativi.**

- Le `file:line` di `threat-hunter` sono state **sfasate** sulle unità 04 e 05 ed **esatte** sulle
  07 e 08. Riapri i numeri prima di riportarli.
- Se un tuo obiettivo e un tuo divieto si contraddicono, **obbedisci al più specifico e
  dichiaralo**.

**Se i revisori tornano tutti a zero rilievi, non è finita.** Riga di istruttoria comunque, e
verifica tu almeno la domanda più rischiosa del tuo diff.

**La domanda più rischiosa di questo diff, se non ne trovi una migliore:** dopo la traduzione,
**il dettaglio tecnico è ancora recuperabile?** Conta i `Console.Error.WriteLine` prima e dopo,
file per file. Se ne hai tolti, o se hai tradotto un `catch` senza lasciarne uno, hai fatto
esattamente ciò che l'unità 05 si è rifiutata di fare.

## IL GATE DELLA REVIEW

Il tuo diff tocca **testo mostrato all'utente a partire da un'eccezione**, su cinque file: è una
superficie di fiducia — il rilievo 3 nasce proprio dal fatto che un messaggio d'errore diceva
troppo. **`bug-hunter`, `conformity` e `threat-hunter`**, tutti e tre nello stesso messaggio.
`backend-expert` solo se nasce una superficie nuova, cosa che il budget vieta.

**Il tuo diff sarà grande** (cinque file, diciotto punti): l'istruttoria potrebbe superare la
soglia del §4 — somma ≥ 4 rilievi fra `bug-hunter` e `conformity`, oppure ≥ 3 file distinti
citati. **Il metro sono i file distinti, ed è probabile che tu la superi**: in quel caso lancia
`checker`, e non valutare la soglia cumulando file che appartengono a rilievi diversi.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**.
- `dotnet test` → **273 superati**, com'erano quando parti.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai: `obj/` non ha lock
fra processi, e con cinque file potresti essere tentato di lanciarne più d'uno — **puoi**, purché
**nessuno compili** e nessuno tocchi il file di un altro.

**Non avviare il server di sviluppo e non provare nel browser.**

BUDGET: 25 dollari

RESOCONTO IN: `handoff/13-errori-tradotti/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 13 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <le frasi scritte, verbatim, raggruppate per file, e da quale frase del modello discendono>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti, e a chi appartiene il rimedio>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

Aggiungi una riga d'esito **per ciascuno dei diciotto punti**: tradotto, lasciato com'era col
motivo, oppure non raggiungibile. Diciotto righe. È l'unico modo perché il capo sappia che il
rilievo 3 è **davvero** chiuso, invece di crederlo chiuso come è successo la prima volta.

Aggiungi `DA PROVARE NEL BROWSER` col **testo esatto** dei messaggi nuovi e come provocarli. Per
i casi che a mano non si provocano, **dichiarali come limite** invece di inventare una procedura
che non funziona: un limite dichiarato vale più di una prova data per fatta.
