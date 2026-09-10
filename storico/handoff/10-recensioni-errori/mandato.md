UNITÀ: 10/13 — Le recensioni smettono di mostrare il JSON di PostgreSQL

## OBIETTIVO

**Il rilievo 3, sulla parte che ti compete.** `Shared/RecensioniElemento.razor` mostra
all'utente il testo grezzo di un'eccezione in **cinque** punti, e due messaggi che dicono «Il
database ha rifiutato…» senza dire altro.

Il modello **esiste già ed è codice, non una descrizione**: le sei frasi che l'unità 05 ha
scritto in `Pages/CollectionEdit.razor`. **Aprile e seguile.** Sono elencate verbatim in
`handoff/05-collezione-rilievi/resoconto.md`, sezione `CONTRATTI`, e stanno nel file vero.

Non inventare un settimo modo di dire la stessa cosa: è un **contratto**, e altre due unità
(la 13 e quella già chiusa) lo rispettano.

## PERIMETRO — file di tua proprietà esclusiva

- `Shared/RecensioniElemento.razor` (606 righe)

Un file solo. Se ti servisse toccarne un altro, è un `BLOCKED`.

## IL CENSIMENTO, GIÀ FATTO DAL CAPO — verificalo, non fidartene

**I cinque messaggi che l'utente legge**, tutti nella forma `errore = $"…: {ex.Message}"`:

| Riga | Cosa stava facendo |
|---|---|
| `:236` | identificare l'utente corrente |
| `:255` | leggere le recensioni |
| `:396` | salvare |
| `:448` | salvare (secondo `catch`) |
| `:553` | eliminare |

**Le tre che finiscono in `Console.Error.WriteLine` non c'entrano e NON si toccano**: `:276`,
`:309`, `:464`. Sono la diagnostica, e il criterio è quello dell'unità 05 — tradurre senza
registrare avrebbe barattato un'indiscrezione con una cecità, e a pagarla sarebbe stato chi
deve diagnosticare il prossimo guasto. **Se una traduzione ti fa perdere il dettaglio tecnico,
aggiungi un `Console.Error.WriteLine`**, non tenere il JSON a schermo.

**I due messaggi già tradotti ma muti**: `:434` («Il database ha rifiutato la modifica.») e
`:547` («Il database ha rifiutato l'eliminazione.»). Non mostrano JSON, ma dicono solo *che*
è andata male: niente sul perché, niente su cosa fare, niente su cosa è successo al testo
appena scritto. Il modello dell'unità 05 li copre: **fatto, causa, azione**.

**Due messaggi sono già buoni e non si toccano**: `:337` («La sessione non è più valida: esci e
rientra.») e `:441` («La tua recensione non c'è più.»). Verifica che sia vero prima di lasciarli
stare.

I numeri di riga vengono da un `grep` del capo: **riaprili tu** prima di modificare, e se non
tornano usa quelli veri dichiarandolo in `SCOSTAMENTI`.

## LA FRASE CHE MANCA, E CHE SOLO QUESTA SCHERMATA PUÒ DIRE

L'unità 08 ha istruito un fatto che appartiene a questa schermata e a nessun'altra, e l'ha
lasciato qui apposta.

Sulle recensioni **il proprietario dello spazio non può moderare**: `reviews_update` e
`reviews_delete` (`supabase/migrations/20260812200000_recensioni.sql:127-134`) hanno la sola
condizione `user_id = auth.uid()`, senza il ramo `is_space_owner` che note, collezioni, elementi
e spese hanno tutti. Il commento a `:118-122` dichiara la divergenza deliberata: «un voto è
un'opinione personale e riscriverla sarebbe falsificarla, non moderare», e la via d'uscita per
togliere una recensione altrui è **cancellare l'elemento**, per via dell'`ON DELETE CASCADE`.

**Verificalo tu aprendo la migrazione** — non fidarti di questo paragrafo — e poi: se un
proprietario che prova a cancellare la recensione di un altro incontra un errore, quell'errore
deve **dire questo**, non «Il database ha rifiutato l'eliminazione.» È il caso in cui la
traduzione vale più di tutte le altre, perché il rifiuto è **voluto** e l'utente non ha modo di
saperlo.

Se scopri che l'interfaccia non offre affatto quel pulsante a chi non è l'autore — cioè il caso
non è raggiungibile — **dillo nel resoconto e non aggiungere il messaggio**: un errore per un
percorso che non esiste è codice morto.

## NON TOCCARE

- **`Pages/CollectionEdit.razor`**: è il **modello**, chiuso dall'unità 05. Lo **leggi**, non lo
  modifichi.
- **`supabase/migrations/**`**: le apri in lettura per verificare le policy. Nessuna migrazione
  nuova: questa unità non tocca il database.
- **`wwwroot/css/app.css`**: unità 11. Usa classi esistenti; se non basta, torna `BLOCKED`.
- **`Pages/NoteEdit.razor`, `Pages/ItemEdit.razor`, `Pages/SpesaEdit.razor`,
  `Pages/SpaceDetail.razor`**: hanno lo stesso difetto e sono dell'**unità 13**. Non anticiparla:
  due unità sullo stesso file si sovrascrivono.

## BUDGET DI COMPLESSITÀ

Nessuna astrazione nuova, nessun tipo, nessun servizio, nessun file, **nessun helper di
traduzione**. Se ti viene voglia di estrarre una funzione `TraduciErrore(Exception)` condivisa:
**no**. L'unità 05 ha stabilito che il messaggio giusto dipende da *quale schermata stava
aspettando*, non da quale query è fallita — è la ragione per cui ha **rifiutato metà del proprio
perimetro** invece di mettere la traduzione nel repository. Se pensi che valga la pena, scrivilo
in `FUORI SCOPE` come proposta al capo.

## STATO

Unità chiuse e committate: 02 (`8a1d438`), 03 (`d101fdf`), 04 (`3206150`), 05 (`e139ce8`),
06 (`f4f2dbd`), 07 (`4327598`), 12 (`8a4a89f`), 08 (`bdd858a`), 09.

**Non committare.** Committa il capo, a resoconto letto. Lascia i file modificati nel working
tree e dichiaralo.

Il piano è in `handoff/PIANO.md`. Rileggi `DECISIONI`: vince la riga più recente. C'è una riga
del 3 settembre sera che dice che **l'utente non è raggiungibile**: qualunque domanda tu abbia,
portala nel resoconto.

**Due fatti operativi.**

- Le `file:line` di `threat-hunter` sono state **sfasate** sulle unità 04 e 05 ed **esatte**
  sulle 07 e 08. Riapri i numeri prima di riportarli.
- Se un tuo obiettivo e un tuo divieto si contraddicono, **obbedisci al più specifico e
  dichiaralo**.

**Se i revisori tornano tutti a zero rilievi, non è finita.** Riga di istruttoria comunque, e
verifica tu almeno la domanda più rischiosa del tuo diff.

**La domanda più rischiosa di questo diff, se non ne trovi una migliore:** dopo aver tradotto,
**il dettaglio tecnico è ancora recuperabile da chi deve diagnosticare?** Conta i
`Console.Error.WriteLine` prima e dopo: se ne hai tolti, o se hai tradotto un `catch` senza
lasciarne uno, hai barattato un'indiscrezione con una cecità — che è esattamente ciò che
l'unità 05 si è rifiutata di fare.

## IL GATE DELLA REVIEW

Il tuo diff tocca **testo mostrato all'utente a partire da un'eccezione**: è una superficie di
fiducia, e `threat-hunter` va lanciato — è il rilievo 3 stesso, che nasce dal fatto che un
messaggio d'errore diceva troppo. `bug-hunter` e `conformity` sempre. `backend-expert` solo se
il diff supera le ~120 righe o nasce una superficie nuova, cosa che il budget vieta.

## GATE

- `dotnet build -warnaserror` → **0 errori, 0 avvisi**.
- `dotnet test` → **273 superati**, com'erano quando parti.

Compili **tu**, una volta, a fine giro. Gli `implementer` non compilano mai.

**Non avviare il server di sviluppo e non provare nel browser.**

BUDGET: 18 dollari

RESOCONTO IN: `handoff/10-recensioni-errori/resoconto.md`

## SCHELETRO DEL RESOCONTO — scrivilo in questa forma esatta

```
UNITÀ: 10 — ESITO: FATTO | PARZIALE | BLOCKED: <domanda>
TOCCATI: <file → +x/−y, una riga per file — mai diff grezzo>
CONTRATTI: <le frasi che hai scritto, verbatim, e da quale frase dell'unità 05 discende ciascuna>
ADJUDICA: <per ogni rilievo: verdetto, motivo in una riga, riga di codice citata>
FUORI SCOPE: <rilievi fondati non risolti, e a chi appartiene il rimedio>
GATE: <comando → esito>
SCOSTAMENTI: <cosa diverge dal mandato e perché> | nessuno
```

**La sezione `CONTRATTI` è la più importante del tuo resoconto**, perché l'unità 13 dovrà
scrivere le stesse frasi su altri quattro file. Elencale **verbatim**, e per ciascuna di' da
quale delle sei frasi dell'unità 05 discende. Se ne hai dovuta inventare una che non ha
corrispondente, **dillo**: è materiale che la 13 deve poter riusare o rifiutare consapevolmente.

Aggiungi `DA PROVARE NEL BROWSER` col **testo esatto** di ogni messaggio nuovo e come provocarlo.
Per gli errori difficili da provocare, di' quale strumento serve (rete disattivata, due schede,
due account) e, se un caso non è raggiungibile a mano, **dichiaralo come limite** invece di
inventare una procedura che non funziona.
