# Ricognizione UI/UX — elenco ordinato per gravità

Giro in sola lettura del 27 agosto 2026, esecutore. **Nessuna correzione applicata**, e
**nessun dato creato**: l'unico tentativo di scrittura, autorizzato dall'utente, è fallito
prima di toccare il database — vedi il rilievo 0, che è il motivo per cui è fallito.

## Metodo e limiti

Server su `http://localhost:5000`, ambiente Development, build 0 errori / 0 avvisi
ricompilata **prima** dell'avvio. Service worker di sviluppo verificato no-op
(`caches.keys()` vuoto): la cache della PWA non ha potuto falsare queste prove.

Larghezze provate: 1414px e 526px. **Non è stato provato 360px**: `resize_window` non
scende sotto ~526px su questa macchina, e simularlo via JS senza replicare le media query
attive a quella larghezza darebbe una misura falsa.

Pagine viste: `/`, `/spaces`, `/spaces/{id}`, `/notes`, `/notes/new`, `/collections`,
`/collections/new`, `/expenses`, `/expenses/{id}`, `/profile`.
**Non viste**: `/collections/{id}`, `/collections/{id}/edit`,
`/collections/{id}/items/{id}` — cioè tutta la parte di voti e recensioni. Non per il
vincolo sui dati, come si credeva a inizio giro, ma perché **crearle è impossibile**.

---

# BLOCCANTE

## 0 — Creare una collezione è impossibile, in produzione, da due settimane

**Osservato**: creata una collezione dal modello «Birre», premuto «Salva». Il salvataggio
fallisce con:

```
permission denied for table collections   (SQLSTATE 42501)
hint: Grant the required privileges to the current role with:
      GRANT INSERT ON public.collections TO authenticated;
```

**Non è RLS**: è un privilegio di colonna mancante, e la catena è ricostruibile per intero
nei file.

1. `20260812120000_collections.sql:221` concede l'INSERT **colonna per colonna**:
   `grant insert (space_id, owner_id, name, icon, fields, rating_max)`.
2. `20260812230000_voto_al_buio.sql` aggiunge la colonna `blind` e — righe 105-109 — la
   riconcede **solo in UPDATE**: `grant update (name, icon, fields, rating_max, blind)`.
   Il commento sopra quella riga dimostra che il meccanismo era ben compreso: «blind non
   comparirebbe nell'elenco delle colonne scrivibili finché non viene riconcesso qui».
   L'INSERT non è stato toccato.
3. `Models/Collection.cs:40` dichiara `[Column("blind")] public bool Blind` **senza**
   `ignoreOnInsert: true` — mentre `version`, `created_at` e `updated_at`, le altre colonne
   non concesse, ce l'hanno tutte.

Ogni `Insert<Collection>` invia quindi una colonna che il ruolo `authenticated` non può
scrivere, e PostgreSQL rifiuta **l'intera istruzione**. Modificare una collezione esistente
funzionerebbe; crearne una no.

**Portata**: l'app punta a `https://fdqedhgvpneuybtykamf.supabase.co`, cioè al database
vero. La funzione Collezioni — con voti, recensioni ed elementi, la parte più grande del
progetto — è **inutilizzabile dal 12 agosto 2026**, data della migrazione `voto_al_buio`.
È anche la spiegazione del «vincolo sui dati» di questa sessione: lo spazio ha 0 collezioni
perché non è possibile crearne.

**Verificato che il difetto è isolato**: confrontate una per una le colonne inviate dagli
altri modelli con i rispettivi grant di INSERT — `CollectionItem`, `Review` e `Note` sono
tutte coperte. Solo `Collection` è disallineata.

**Le due correzioni non sono equivalenti**, e la scelta è dell'utente:

- **SQL** (una riga, nuova migrazione): aggiungere `blind` al grant di INSERT. Corregge la
  causa. Tocca il database di produzione.
- **C#** (`ignoreOnInsert: true` su `Blind`): fa passare l'INSERT, ma la collezione
  nascerebbe sempre col valore di default. L'interruttore «Voto al buio» presente nella
  pagina di **creazione** verrebbe ignorato in silenzio — un secondo difetto al posto del
  primo. Da solo non basta.

---

# GRAVI

## 1 — Il lavoro non salvato si perde senza una domanda

**Dove**: tutti e quattro gli editor. `NoteEdit.razor:88` (`<a class="btn" href="notes">`),
e gli omologhi in `CollectionEdit`, `ItemEdit`, `SpesaEdit`.

**Osservato**: scritto del testo in `/notes/new`, premuto «Chiudi». Il testo sparisce senza
avviso; il tasto Indietro restituisce un editor **vuoto**. Irrecuperabile.

**Prova che vale ovunque**: in tutto il progetto ci sono **zero** occorrenze di
`beforeunload`, `NavigationLock`, `LocationChanging`, `confirm(`. Nessun editor protegge il
lavoro in corso. Vale anche per il clic sulla barra laterale e per il tasto Indietro.

**Perché è così in alto**: è perdita di dati silenziosa su un'app dove si scrivono note
lunghe in Markdown. È la stessa classe del difetto già corretto in questo progetto (la
conferma di eliminazione che restava armata).

**Nota utile per chi correggerà**: lo stato «c'è qualcosa da salvare» **esiste già** —
`NoteEdit.razor:125-127`, proprietà `Cambiata`, usata per accendere il pulsante Salva.
Manca solo di collegarlo all'uscita.

## 2 — Premi «Salva», e l'esito compare dove non stai guardando

**Dove**: `/collections/new`, e per costruzione ogni modulo lungo.

**Osservato dal vivo** durante il rilievo 0: il pulsante «Salva» sta **in fondo** a un
modulo che con cinque campi è alto tre schermate. Il messaggio di esito compare **in cima**.
Premuto Salva, la schermata non cambia di un pixel: nessuno spostamento, nessun messaggio
in vista, nessuno stato del pulsante che dica «ho finito, è andata male». Per accorgersi del
fallimento bisogna scorrere su di tre schermate — e non si ha alcun motivo di farlo, perché
nulla suggerisce che sia successo qualcosa.

**Perché conta**: è la definizione di «stato che non dice cosa sta succedendo». Un utente
qualunque a questo punto ripreme Salva, poi conclude che l'app è rotta. Che sia esattamente
ciò che è capitato a me, che il difetto lo stavo cercando, è la prova migliore che ho.

## 3 — Il messaggio d'errore è il JSON grezzo di PostgreSQL

**Dove**: `/collections/new`, riquadro rosso in cima.

**Osservato**, testualmente:

```
Non è stato possibile salvare: {"code":"42501","details":null,"hint":"Grant the required
privileges to the current role with: GRANT INSERT ON public.collections TO
authenticated;","message":"permission denied for table collections"}
```

Espone all'utente finale il codice SQLSTATE, il nome della tabella e **un'istruzione GRANT
rivolta a un amministratore di database**. Non dice cosa è andato storto in termini
comprensibili, non dice se riprovare, non dice se il lavoro è perso.

È la stessa famiglia delle sei stringhe «Il database ha rifiutato…» già in pendenza — ma un
gradino sotto: là il meccanismo è almeno tradotto in italiano, qui il JSON arriva crudo.
Vale la pena trattarli insieme quando si deciderà di trattarli.

---

# MEDI

## 4 — L'avviso di aggiornamento non si può rimandare, e copre l'azione principale

**Dove**: `wwwroot/index.html:75-78`, `#aggiornamento-pwa`. Su ogni pagina.

**Osservato**: un solo bottone, «Aggiorna». Verificato per ispezione del DOM, non a occhio:
nessun elemento di chiusura. Occupa 416×74 px ancorati in basso a destra.
**A 526px copre il pulsante «Segna»** della pagina Spese — cioè l'azione principale.

**Perché**: chi sta scrivendo e non vuole ricaricare adesso non ha un «più tardi». Il
vicino di casa nello stesso file, `#blazor-error-ui`, ha invece la sua
`<span class="dismiss">`: due avvisi persistenti, uno congedabile e uno no.

**Il problema vero non è la X mancante**: chiuderlo equivarrebbe a rinunciare
all'aggiornamento per sempre, perché quel banner è l'unico canale verso di esso. Serve un
«più tardi», non una X.

**NON è un difetto**: in sviluppo il banner riappare dopo il clic. Il worker in attesa è
`service-worker.js` (no-op, senza listener `message`), quindi lo SKIP_WAITING cade nel
vuoto; il worker pubblicato il listener ce l'ha (`service-worker.published.js:15-16`).
Artefatto d'ambiente. Chi collauda non ci perda un'ora.

## 5 — Su telefono i bersagli delle categorie sono alti 22px

**Dove**: `/expenses`, i dieci pulsanti di categoria. Misurati a 526px.

**Osservato**: altezza **22px** per tutti e dieci (`Spesa` 60×22, `Ristoranti` 78×22,
`Abbigliamento` 106×22…). Il minimo raccomandato è 44px. Anche `‹` e `›` del mese sono
33×35.

**Perché**: segnare una spesa è l'operazione più frequente dell'app, e si fa col pollice.
Dieci bersagli alti 22px affiancati sono un errore di battitura in attesa di accadere.

## 6 — «Anteprima» fa saltare il layout di 358 pixel

**Dove**: `/notes/new` e `/notes/{id}`, selettore Scrivi/Anteprima.

**Osservato**: passando ad «Anteprima» il riquadro collassa dall'altezza fissa (~400px)
all'altezza del contenuto (~60px con una riga di testo), e i pulsanti Salva/Chiudi salgono
di 358px. Se stavi per premere «Salva», sotto il cursore ora c'è il vuoto. Tornando a
«Scrivi» il salto si ripete al contrario.

## 7 — «Gestisci questo spazio» non porta a niente da gestire

**Dove**: Home → «Gestisci questo spazio» → `/spaces/{id}`.

**Osservato**: per lo spazio personale la pagina mostra solo «MEMBRI (1)» col proprio nome
e il badge «proprietario». Nessuna azione. Il link promette una gestione che per quello
spazio non esiste, e non c'è un ritorno se non la barra laterale.

## 8 — «Elimina» sta a 55px da «Chiudi»

**Dove**: `/expenses/{id}`, e per costruzione gli altri editor di entità esistenti.

**Osservato**: la fila è `Salva` · `Chiudi` · `Elimina`, con `Elimina` immediatamente
adiacente. Il rosso aiuta, la distanza no — soprattutto col pollice. La conferma a valle
esiste (`ConfermaAzione`), quindi il danno è contenuto: resta un tocco sbagliato di troppo.

---

# MINORI

## 9 — Un pulsante spento che non dice cosa manca

**Dove**: `/collections/new`, «Salva».

**Osservato**: `disabled: true` ma reso con `opacity: 0.5` su fondo blu pieno — su nero
resta saturo e legge come premibile. `cursor: default`, nessun messaggio. L'utente non ha
modo di sapere che manca il nome: si scopre per tentativi. (Che abbia ingannato anche me,
a occhio, prima di misurarlo, è indicativo.)

## 10 — L'icona della collezione è un campo di testo libero

**Dove**: `/collections/new`, `input.icona-input`, 56×57px, `maxlength: 16`,
placeholder `📋`.

**Osservato**: non è un selettore di emoji: è un `<input type="text">` in cui si è attesi
digitare un'emoji a mano. Da desktop bisogna conoscere `Win + .`; e ci si può scrivere
qualunque cosa fino a 16 caratteri. I tre modelli («Liquidi svapo», «Birre», «Film») la
riempiono correttamente, quindi il problema si presenta solo a chi parte da zero.

## 11 — Nella Home «Spesa 100%» non dice di essere una categoria

**Dove**: Home, riga sotto il totale del mese.

**Osservato**: si legge «Spesa 100%» accanto a «QUESTO MESE 12,50 €». Solo aprendo
`/expenses` si scopre che *Spesa* è una **categoria** (accanto a Casa, Trasporti,
Ristoranti…) e non un'etichetta generica. Un nome che collide con quello della sezione che
lo contiene.

## 12 — Due schermate su dodici non si spiegano

**Dove**: `/spaces/{id}` e `/profile` non hanno l'infobutton «?»; gli editor non hanno
nemmeno una testata né un titolo di pagina.

**Osservato**: `infobuttonInPagina: false` su entrambe. Coerente col lavoro fatto (cinque
schermate), ma dall'esterno legge come un'incoerenza: alcune pagine si spiegano, altre
tacciono. Gli editor per giunta si aprono «nudi», senza dire dove sei.

## 13 — Lo stato vuoto invita all'azione lontano da dove si legge

**Dove**: `/notes` e `/collections` vuote.

**Osservato**: il messaggio («Ancora nessuna nota qui», più una spiegazione) sta al centro;
l'unico pulsante per agire è in **alto a destra**. Nella Home, a parità di stato vuoto, il
pulsante è invece inline sotto il messaggio. Due trattamenti diversi per la stessa
situazione, e quello meno comodo è nella pagina dedicata.

## 14 — Selettore spazio e «Profilo» leggono come accavallati

**Dove**: barra laterale in basso, a 1414px.

**Osservato**: misurato — **non** si sovrappongono (select fino a x=179, link da x=187: 8px
di gap). Ma le basi sono disallineate (820 contro 838) e la prossimità è tale che l'occhio
li legge come accavallati. Rifinitura.

---

# DA VERIFICARE, non osservato

## 15 — Un logout non riuscito non lo dice a nessuno

**Il logout è affidabile**, e il 503 di ieri era già assorbito: `SignOutAsync()` avvolge
ogni passo nel proprio `try` (`SupabaseService.cs:282-300`), il fallimento di rete viene
catturato e la pulizia locale procede comunque. È la difesa che funziona, non un difetto.

**Il punto aperto**: `AuthStateService.cs:84-86` usa il `bool` di ritorno solo per decidere
`forceLoad: !uscito`. Quando l'uscita **non** è riuscita l'utente arriva comunque alla
vetrina, senza alcun messaggio — vede una schermata che dice «sei fuori» nel caso in cui il
codice stesso sa di non esserne certo. È lo scenario del dispositivo condiviso citato nel
commento a `SupabaseService.cs:268-269`. Dedotto dal codice, **non riprodotto**: servirebbe
far fallire `DestroySession()`.

---

# Le tre pendenze note, ritrovate dal giro

Nessuna toccata, come da consegna. Che siano riemerse da sé è il segnale che il giro
guardava dove doveva.

| Pendenza | Dove si è vista |
|---|---|
| Il segnaposto `&#10;` | `/notes/new`, visibile testualmente nel campo del corpo |
| Il medaglione 📋 | `/collections/new`, campo icona |
| «Il database ha rifiutato…» | non incontrata di persona, ma il rilievo 3 è la sua versione peggiore: stesso problema, senza nemmeno la traduzione |

---

# Cosa funziona, e vale la pena non rompere

Un elenco di attriti dà un'immagine storta se non si dice anche cosa regge.

- **I modelli di collezione** («Liquidi svapo», «Birre», «Film») portano da zero a una
  collezione configurata con nome, icona e cinque campi tipizzati **in un solo clic**. È la
  cosa meglio riuscita che ho incontrato.
- **Gli stati vuoti spiegano il concetto**, non si limitano a dire «niente qui»: «Una
  collezione è un elenco che si vota insieme: birre, film, ristoranti».
- **Il layout stretto regge**: a 526px nessuno scorrimento orizzontale, la barra laterale
  diventa barra inferiore, i campi si impilano nell'ordine giusto.
- **Il logout è costruito bene**, come dettagliato al rilievo 15.
- **Gli infobutton** funzionano dove ci sono, e il testo che mostrano è scritto per chi non
  conosce l'app.
