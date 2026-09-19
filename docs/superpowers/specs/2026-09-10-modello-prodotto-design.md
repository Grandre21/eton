# Il modello di prodotto di Eton — decomposizione e sequenza

Scritto il **10 settembre 2026**, da una conversazione fra l'utente e l'esecutore, con due
consulti di `tech-advisor` (Fable) verificati contro il codice e la documentazione.

**Cos'è questo documento.** Non è la specifica di una funzionalità: è la **decomposizione** di
un cambio di modello di prodotto in fasi indipendenti, ognuna delle quali diventerà una
sessione a sé con la propria specifica e il proprio piano. Chi lo legge non ha bisogno della
conversazione da cui nasce.

**Cosa non è.** Non autorizza nessuna implementazione. Ogni fase passa dal proprio ciclo
`brainstorming → spec → piano → implementazione`.

---

## 1. Da dove nasce

Appunti dell'utente, **verbatim**, 9 settembre 2026:

> «Passare da servizi pre impostati a gestore di servizio quality of Life tramite un market
> Place su cui basiamo il nostro modello di business, ragioniamoci assieme, l'idea sarebbe
> quella di offrire tools free e tools sotto abbonamento e permettere agli utenti di creare il
> proprio tool dando api o roba simile, ancora non ho un idea chiara»
>
> «Passare da app mobile a app + sito.+ App desktop per Windows / altri sistemi operativi,
> ragioniamoci»
>
> «Pensavo di aprire un crowfunding e metterlo nel sito, così se qualcuno volesse donare»

### Le decisioni prese dall'utente in conversazione

**9 settembre 2026:**

1. **Si fanno tutti e tre gli stadi** — catalogo con paywall, piattaforma, marketplace vero —
   in sequenza. Non sono alternative.
2. **Un «tool» creato da un utente deve arrivare a pescare dati da API esterne.** Non deve
   arrivare a eseguire codice scritto dall'utente.

**10 settembre 2026:**

3. **Le spese si fanno, non si archiviano**, e con una direzione dichiarata: *«uno stile più
   tabellare e che ti permetta di gestire in modo molto puntuale le tue spese, non solo un tool
   da app scema»*. V. fase 2.
4. **L'applicazione non deve funzionare offline.** Cade il vincolo di `README.md:157`, e con
   esso l'ostacolo alle immagini remote. V. §3.6.
5. **La piattaforma di donazione resta da scegliere**: l'utente la approfondisce. V. §6.

---

## 2. Tre fatti che riducono il lavoro, verificati

### 2.1 Il punto «app + sito + desktop» descrive lo stato presente

`wwwroot/manifest.webmanifest` ha `display: standalone`, `id`, `scope` e icone maskable.
Installata da Edge o Chrome, Eton **è già** un'applicazione desktop con finestra propria e voce
nel menu Start, su Windows, macOS e Linux. Non è mai esistita un'app mobile separata: è sempre
stata una PWA.

**Nessun wrapper va costruito** — non MAUI (già escluso da `2026-08-11-eton-design.md` §2.1),
non Electron, non Tauri. Le ragioni della spec valgono identiche per tutti: Google blocca OAuth
nelle WebView incorporate, quindi l'autenticazione andrebbe rifatta con browser di sistema,
schema personalizzato e PKCE; più firma del codice, canale di aggiornamento da costruire, CI per
sistema operativo, e una seconda distribuzione da tenere allineata a una PWA che si aggiorna da
sola.

Resta possibile, quando esista un motivo di marketing e non prima: **PWABuilder → MSIX** per
il Microsoft Store, mezza giornata, stessa categoria della TWA già prevista dalla spec.

### 2.2 Il marketplace gratuito non parte da zero

`Models/CampoDefinizione.cs` definisce già campi con `key/label/type/options/order` e cinque
tipi; `supabase/migrations/20260812120000_collections.sql` li salva come `fields` jsonb con i
vincoli. **Un utente di Eton configura già la struttura dei propri dati.** «Installare un tool»,
al primo gradino, significa *copiare un jsonb*.

### 2.3 L'API pubblica esiste di fatto, ma non è usabile com'è

PostgREST è raggiungibile con la chiave `anon`, che è pubblica per costruzione, e la RLS non
distingue uno script dall'interfaccia. **Sul dato, l'API c'è.** Sull'accesso, no:

- La sessione vive in `localStorage["eton.session"]` (`Services/BrowserSessionHandler.cs:19`).
- `supabase/config.toml:171-174` ha `enable_refresh_token_rotation = true` con
  `refresh_token_reuse_interval = 10`. Usare quel token da uno script fuori dalla finestra di
  10 secondi fa considerare **l'intera sessione revocata**, ed Eton reagisce con un logout
  (`Services/SupabaseService.cs:279-282`).
- Uno script può fare **il proprio** login PKCE con redirect su `localhost` e ottenere una
  sessione sua. Il risultato è un bearer con **tutti i poteri dell'account, senza permessi
  ristretti e senza pannello di revoca**.

La forma corretta è già presente e disattivata in `supabase/config.toml:366-372`: il **server
OAuth 2.1** di Supabase Auth emette token con claim `client_id`, e una policy può negare le
scritture ai client di terzi con `auth.jwt()->>'client_id'`. È in beta pubblica e la
documentazione ne sconsiglia l'uso in produzione.

**Conseguenza per il piano:** l'API pubblica è *vera di fatto, non da promettere*. Si dichiara
quando il server OAuth esce di beta **e** qualcuno la chiede. Nota di prodotto: mette i tool
*fuori* da Eton, mentre il marketplace li vuole *dentro*.

---

## 3. Le regole che vincolano tutte le fasi

### 3.1 Il confine di sicurezza non cambia

`README.md` punto 2: la RLS è l'unico confine, perché qualunque controllo scritto in C# è
aggirabile interrogando PostgREST a mano. **Vale identico per il paywall.** Un gate «questo
tool è per abbonati» scritto nell'interfaccia non è un gate.

Il disegno che regge:

- Tabella `subscriptions`, `GRANT SELECT` ad `authenticated` **solo sulla propria riga**.
  Nessun `GRANT INSERT`, nessun `GRANT UPDATE`.
- La scrive esclusivamente una Edge Function col `service_role`, su webhook Stripe.
- Le policy dei contenuti a pagamento chiamano una funzione `stable security definer`
  `has_plan()`, come oggi chiamano `is_space_member`.

È il modello che il README già dichiara — *la RLS filtra, il GRANT concede* — applicato a un
dominio nuovo.

### 3.2 Si pesca alla scrittura, mai alla vista

**La regola di progetto più importante di tutto il documento.** Un tool che legge da un'API
esterna chiama quando **l'autore aggiunge o modifica l'elemento**; il risultato si salva dentro
l'elemento; chi guarda legge dal database.

Tre conseguenze, tutte gratuite:

- **L'elemento è la cache.** Non serve progettare un livello di caching.
- **La quota la paga chi scrive.** Il rischio «un membro dello spazio esaurisce la quota
  guardando la stessa pagina» non va difeso: non esiste per costruzione.
- **Il costo è prevedibile:** cresce col numero di elementi creati, non col traffico.

L'alternativa — pescare quando qualcuno apre la pagina — produce un sistema da difendere da
ogni suo utente.

*Limite noto:* la regola vale per identificatori stabili (un ISBN dà sempre lo stesso libro).
Per dati che cambiano nel tempo — meteo, cambi valuta — non basta, e il conteggio per utente
torna al centro. Se il catalogo si orienta lì, questa sezione va riaperta.

### 3.3 Il proxy generico non si fa

«L'utente incolla l'URL e la chiave dell'API che vuole» è escluso, e non per difficoltà di
scrittura. Le difese contro SSRF si scrivono tutte — solo `https`, risoluzione DNS preventiva
con rifiuto di loopback/RFC1918/link-local/CGNAT/ULA/IPv4-mappati, `redirect: "manual"` con
rivalidazione a ogni salto, mai inoltrare la chiave al dominio di destinazione, tetto su
dimensione e timeout, allowlist di header — ma resta un buco: **il `fetch` di Deno non permette
di fissare l'IP già verificato**, quindi la finestra fra controllo e connessione (DNS
rebinding) resta aperta. Ogni servizio che fa proxy generico presidia quella finestra con una
squadra.

E c'è la parte non tecnica: un utente affiderebbe una chiave che **addebita sul suo conto** a un
progetto di una persona sola, senza capacità di risposta a un incidente.

**La forma ammessa è una sola:** «porta la tua chiave» **limitata a un fornitore già presente
nel catalogo** (fase 8). Toglie il costo di quota senza aprire lo spazio degli URL.

### 3.4 Il crowdfunding resta donazione pura

Piattaforma esterna con link. **Non Kickstarter**, che è crowdfunding *reward-based*, cioè una
prevendita, cioè attività commerciale.

**La trappola da evitare è una e precisa: «dona per sbloccare X».** Trasforma la donazione in
una vendita e fa collassare la fase 1 dentro la fase 7. Nessuna contropartita, nessun
collegamento fra chi dona e cosa vede nell'app.

*Sul fisco: nessuno degli autori di questo documento è un fiscalista.* Le liberalità senza
contropartita ricevute da persona fisica non sono reddito; se diventano abituali e legate
all'uso del software assomigliano a corrispettivi. Prima che superino la cifra da tasca, un
commercialista.

### 3.5 Le formule non si valutano con `eval`

Una formula scritta da un membro gira nel browser degli **altri** membri dello spazio. Un
mini-linguaggio senza rete e senza accesso alla pagina è sicuro per costruzione; un `eval` è
XSS persistente su chi condivide lo spazio.

### 3.6 Gli asset remoti sono ammessi — l'app non deve funzionare offline

**Decisione dell'utente, 10 settembre 2026.** Cade il vincolo di `README.md:157`: *«Nessun
asset remoto. Niente CDN, niente `@import`, niente immagini o font caricati da altri domini:
l'applicazione deve funzionare offline una volta installata.»*

Cosa cambia e cosa no:

- **Le immagini da API esterne sono ammesse** (copertine di libri, locandine). La decisione
  aperta n. 2 è chiusa: si mostrano dal dominio del fornitore, senza copiarle in Storage.
- **Il `README.md` va riscritto**, non lasciato in contraddizione. Chi lo lascia com'è produce
  un documento che vieta ciò che il codice fa, ed è il modo in cui una regola muore restando
  scritta. Va fatto nella prima fase che tocca un asset remoto, non prima e non dopo.
- **Il service worker resta.** Serve all'installabilità e al banner «è disponibile una versione
  nuova», che è già in produzione. Quello che cade è la *promessa* offline, e con essa
  l'obbligo di elencare ogni asset in `offlineAssetsInclude`. Il trattamento esatto del file è
  un compito della fase che lo tocca, non una decisione di questo documento.
- **Font e CSS: nessun motivo di cambiare.** I font serviti da noi restano serviti da noi. Il
  permesso di caricare da fuori non è un invito a farlo: ogni dominio in più è una dipendenza
  in più e un fornitore che può sparire.

**La conseguenza da non perdere di vista.** Un `<img>` verso il dominio di un terzo manda a
quel terzo l'**indirizzo IP e il referer di chi guarda**, a ogni visualizzazione. La regola
del §3.2 — si pesca alla scrittura — protegge la *quota* dell'API, non la privacy di chi
guarda: il dato di testo arriva dal database, ma l'immagine no. Non è un motivo per rovesciare
la decisione, è un fatto che va detto agli utenti nella dichiarazione della fase 5.4, e che
rende l'immagine diversa dal testo nel trattamento.

---

## 4. La sequenza

Ogni fase è **una sessione**. Le stime sono ordini di grandezza, non impegni.

### Fase 0 — Chiudere il ciclo aperto · *giorni*

Preesistente a questo documento, e va finita prima di aprire qualunque altra cosa.

| | |
|---|---|
| 0.1 | **Giro D del collaudo** — brief pronto in `handoff/17-collaudo/D-brief.md`, non va riscritto. Richiede il server avviato dal capo e i PID annotati in `handoff/server.md` |
| 0.2 | **Sessione di chiusura** — prompt già scritto in `handoff/CHIUSURA.md`. Prima di aprirla si riempie il campo `ESITO DEL COLLAUDO` con la riga del giro D |
| 0.3 | **Fermare il server di sviluppo** a giro chiuso — entrambi i PID di `handoff/server.md`, e verificare che la 5000 sia tornata libera |

### Fase 1 — La vetrina e le donazioni · *1-2 giorni · nessun codice di Eton toccato*

| | |
|---|---|
| 1.1 | Una **pagina statica separata**: repo proprio o dominio proprio, `index.html` che riusa i valori di `app.css`, con link all'app |
| 1.2 | **Piattaforma di donazione** scelta dall'utente (Ko-fi, GitHub Sponsors, Buy Me a Coffee sono tecnicamente equivalenti — il criterio è dove sta il pubblico) |

**Perché fuori dal WASM.** `Pages/Benvenuto.razor` esiste ed è la vetrina attuale, ma si disegna
solo *dopo* il download del runtime .NET (`wwwroot/index.html:63` mostra «Caricamento…») ed è
invisibile ai motori di ricerca: Blazor WebAssembly standalone non prerenderizza. Per una pagina
che deve essere condivisa, indicizzata e aprirsi in un secondo su un telefono è il veicolo
sbagliato. Far prerenderizzare la vetrina WASM sarebbe invece un progetto.

**Perché prima.** Eton ha già qualcosa da mostrare — note, collezioni, recensioni, spazi
condivisi, collaudati. La vetrina non promette il marketplace: presenta il prodotto che esiste.
È anche lo strumento di misura: se non entra nessuno, l'abbonamento non ha mercato e si scopre
a costo zero. Si aggiorna in un'ora quando la fase 2 consegna.

**Nessun obbligo nuovo:** donazione pura, persona fisica, §3.4.

### Fase 2 — Le spese diventano uno strumento serio · *settimane*

Direzione dell'utente, 10 settembre, **verbatim**: *«uno stile più tabellare e che ti permetta
di gestire in modo molto puntuale le tue spese, non solo un tool da app scema»*.

**Questa direzione era già stata decisa e scritta il 3 settembre**, e il documento che la
registra è `docs/superpowers/specs/2026-09-03-spese-ricorrenti-design.md`, che si apre così:
*«È il primo dei tre lavori in cui è stata decomposta la richiesta "gestione spese più
completa": ricorrenti, poi vista tabellare, poi analisi.»* Non serve riaprire la
decomposizione: serve eseguirla.

| | | Stato |
|---|---|---|
| 2.1 | **Spese ricorrenti** | Design e piano da **sei task** già scritti e approvati (`2026-09-03-spese-ricorrenti-design.md`, `plans/2026-09-03-spese-ricorrenti.md`). Pronto a partire |
| 2.2 | **Vista tabellare** | Da progettare — è il cuore della direzione «puntuale, non da app scema» |
| 2.3 | **Analisi** | Da progettare |

**Perché le ricorrenti vengono per prime, e non la tabella che l'utente ha nominato.** Il
motivo è scritto al §2 di quel design ed è tecnico, non di preferenza: le ricorrenti sono
**l'unico dei tre lavori che tocca lo schema**, e in questo progetto le migrazioni si applicano
a mano in produzione. La forma della riga — `recurring_id`, e la distinzione fra «registrata» e
«prevista» — la decide quel lavoro. Costruire la vista tabellare prima significherebbe rifarne
le righe dopo.

Il criterio portante, già deciso: **il passato si materializza, il futuro si calcola.** Un mese
trascorso è un fatto e sta su disco; un mese a venire è una previsione e resta una regola.

**Perché questa fase sta qui, prima del marketplace.** Sotto il nuovo modello le spese non sono
«un servizio pre-impostato» da smantellare: sono il **tool di punta**, l'esempio di cosa un tool
di Eton può essere quando è fatto sul serio. È anche l'unico lavoro del piano che è già
progettato e pianificato: tutte le fasi successive devono ancora passare dal proprio
`brainstorming → spec → piano`.

### Fase 2.1-bis — Il mandato UI/UX · *settimane · collocata il 19 settembre 2026*

Si apre **dopo la 2.1 e prima della 2.2**, per la decisione 4 del §6, dove stanno le tre ragioni
per esteso. Non ha ancora né spec né piano: passa dal proprio `brainstorming → spec → piano` come
ogni altra fase che non sia la 2.1.

| | |
|---|---|
| 2.1-bis.1 | La direzione SLY tradotta in decisioni: token, tipografia, luce additiva su nero pieno — non neon |
| 2.1-bis.2 | Il sistema applicato alle schermate che esistono, `wwwroot/css/app.css` incluso |
| 2.1-bis.3 | La **2.2** si disegna dentro il sistema nuovo, non prima di esso e non restilizzata dopo |

**Il vincolo che regge anche se la sequenza cambia:** prima della fase 3. La fase 3 costruisce il
catalogo dei template e moltiplica le schermate; un mandato estetico che arriva dopo lavora su un
numero di schermate molto maggiore.

### Fase 3 — I tool gratuiti: template condivisibili · *giorni-settimane · nessun server*

**Questo è il marketplace v1.** I tool gratuiti *sono* i template pubblici.

| | |
|---|---|
| 3.1 | Tabella `templates` (nome, icona, descrizione, `fields` jsonb, pubblico/privato, autore), con RLS e privilegi di colonna secondo il modello esistente |
| 3.2 | **Pubblicare** una collezione come template |
| 3.3 | **Installare** un template nel proprio spazio — copiare il jsonb |
| 3.4 | Il **catalogo** dentro l'app |
| 3.5 | Aggiornare la vetrina della fase 1: adesso mostra invece di promettere |

**Il primo template del catalogo è già scritto:** le spese della fase 2, che diventano l'esempio
di riferimento di cosa un tool può essere.

### Fase 4 — I tool che calcolano: campi formula · *settimane · nessun server*

| | |
|---|---|
| 4.1 | Un **mini-linguaggio senza I/O**, scritto da noi, senza rete e senza accesso alla pagina (§3.5) |
| 4.2 | Un tipo `formula` in `CampoDefinizione`, con la sua migrazione e i suoi vincoli |
| 4.3 | Valutazione nel browser, con gli errori resi come dice il progetto (mai un messaggio grezzo a schermo) |

### Fase 5 — Il primo tool con dati esterni, **gratis con un tetto** · *settimane*

Un solo fornitore, dall'inizio alla fine. **Nessun pagamento in questa fase:** il tool è
gratuito fino a un tetto basso di chiamate.

| | |
|---|---|
| 5.1 | La regola del §3.2 scritta nel codice: si pesca alla scrittura |
| 5.2 | Edge Function adattatore — la prima del progetto, oggi `supabase/functions/` non esiste. Verifica il JWT, prende `sub`, e **prima** della chiamata esterna esegue un `update contatori set n = n+1 where user_id = $1 and n < limite returning n` — atomico, nella stessa invocazione. Inaggirabile perché la chiave del fornitore esiste solo nei secret della funzione |
| 5.3 | Cache in uno **schema non esposto**, letta e scritta solo dal `service_role`. La domanda sulla RLS non nasce: nessun ruolo client la raggiunge |
| 5.4 | **La dichiarazione di trasferimento**: il tool dichiara «questo campo viene inviato a X», e lo spazio deve poterlo vedere. Le immagini remote vanno dichiarate qui, per il motivo del §3.6 |
| 5.5 | **Riscrivere `README.md:157`**, che oggi vieta gli asset remoti (§3.6). Questa è la fase che li introduce: lasciare il vincolo scritto significherebbe pubblicare un documento che vieta ciò che il codice fa |

**Tetto anche per spazio, non solo per utente:** account Google multipli sono gratuiti.

**Sul costo:** le API candidate (Open Library, Open-Meteo, TMDB non commerciale, cambi valuta)
sono gratuite o a quota. **Il costo vero è mantenere gli adattatori e stare dentro i termini
d'uso di ognuno.** I tool con dati esterni si vendono perché valgono, non perché costano.

**Perché questa fase viene prima del pagamento, e non dopo.** Il contatore per utente è il
paywall, spento. Quando la fase 7 arriverà, non dovrà costruire un gate: dovrà solo far
dipendere un `limite` già esistente da `has_plan()`. E soprattutto: **il tetto misura**. Se
nessuno lo raggiunge, non c'è niente da vendere, e lo si sa prima di aver aperto una partita
IVA. Costruire Stripe prima di questa fase è la trappola nominata da `tech-advisor` — il
problema tecnico meglio specificato e più divertente, affrontato senza sapere cosa si vende.

### Fase 6 — Le decisioni che precedono il denaro · *nessun codice*

Nessuna riga di codice, e **niente della fase 7 può partire prima**. Si apre solo se il tetto
della fase 5 viene raggiunto da qualcuno.

| | |
|---|---|
| 6.1 | **Partita IVA** — vendere un servizio in modo continuativo è attività d'impresa. Forma e regime: commercialista |
| 6.2 | **Supabase Pro** — il piano free **mette in pausa il progetto dopo una settimana di inattività**. Un prodotto a pagamento non ci sta sopra. È la prima voce di spesa, prima delle commissioni |
| 6.3 | **Play Store: pubblicare o no** — dal 30 giugno 2026 Google applica il **10% di service fee anche sugli abbonamenti pagati sul web** quando l'app è pubblicata. Va deciso **prima** di scrivere il webhook, non dopo |
| 6.4 | **La linea gratis / a pagamento**, confermata sui numeri veri della fase 5 — v. §5 |

### Fase 7 — L'abbonamento: alzare il tetto · *settimane*

| | |
|---|---|
| 7.1 | Tabella `subscriptions` con i privilegi del §3.1 e la funzione `has_plan()` |
| 7.2 | Edge Function `stripe-webhook`: `verify_jwt = false` in `config.toml`, firma verificata con `constructEventAsync`, segreti `STRIPE_API_KEY` e `STRIPE_WEBHOOK_SIGNING_SECRET`. Gestisce `checkout.session.completed` e `customer.subscription.updated/deleted` |
| 7.3 | **Stripe Payment Link statico** con `?client_reference_id=<auth.uid()>` — il valore torna nel webhook. Zero codice per creare sessioni di pagamento |
| 7.4 | Il `limite` della fase 5.2 diventa funzione di `has_plan()`. Le altre policy a pagamento chiamano la stessa funzione |
| 7.5 | Lo stato dell'abbonamento nell'interfaccia, e il portale clienti Stripe come link |

**Cosa non regge, ed è già stato scartato:** qualunque controllo in C#; il piano dentro il JWT
via Custom Access Token Hook (utile solo come cache, e il webhook serve lo stesso);
`stripe_fdw` dentro una policy (una chiamata API per riga).

### Fase 8 — Allargare il catalogo, e «porta la tua chiave» · *settimane*

| | |
|---|---|
| 8.1 | Secondo e terzo adattatore. Ogni fornitore è un adattatore da mantenere |
| 8.2 | **«Porta la tua chiave»**, limitata ai fornitori del catalogo (§3.3). Custodia in **Supabase Vault**, letto solo con `service_role`, revocato ad `anon` e `authenticated`. Vault è un deposito **di progetto, non per utente**: garantire che la funzione decifri solo il segreto del chiamante è responsabilità del codice, e un errore lì espone la chiave di un utente a un altro |

### Fase 9 — Il marketplace vero · *mesi · solo se le fasi 3-8 hanno prodotto creatori*

Gli utenti vendono i propri tool ad altri utenti. **Non è un incremento delle fasi precedenti:
è un cambio di natura.** Nel momento in cui il denaro passa *attraverso* di te per arrivare a
qualcun altro, smetti di vendere un servizio e diventi un intermediario di pagamento: Stripe
Connect, payout, rimborsi per conto di terzi, moderazione dei contenuti che vendi, fiscalità di
persone che non sei tu.

**La condizione per aprirla non è tecnica:** che esistano utenti che pubblicano template e
utenti che li installano, in numero tale da rendere sensato che qualcuno li paghi.

---

### Il debito visivo, misurato — e il vincolo su dove cade

Il 10 settembre `ui-critic` ha prodotto **cinque rilievi**, ognuno con il valore letto dal DOM e
la misura attesa dopo il fix. Stanno in
**`docs/superpowers/specs/2026-09-10-rilievi-ui-critic.md`**, e sono tutti **preesistenti e fuori
dal perimetro** del lavoro dei sedici rilievi: nessuno dei sedici li nominava.

Non sono una fase, perché non sono un progetto: sono debito con un numero. Ma hanno un **vincolo
di collocazione**, e non è una preferenza:

> **Due dei cinque si correggono in un punto solo e valgono per tutta l'applicazione** — il token
> `--testo-fioco` (contrasto di ogni micro-etichetta) e il pavimento di tocco a 48px. Correggerli
> **prima della fase 3** significa che ogni schermata nuova del catalogo li eredita già giusti.
> Correggerli dopo significa toccare anche le schermate nuove.

Il rilievo più urgente è il secondo — l'icona di «Profilo» disegnata a cavallo del selettore di
spazio su ogni schermata larga, 9px di sovrapposizione — e ha la particolarità che **la
correzione proposta dall'agente è sbagliata**: nasconderebbe l'etichetta con `display: none`,
togliendo al link il suo unico nome accessibile. Il file spiega qual è il fix giusto, che il
progetto già possiede.

**Collocati il 19 settembre 2026.** Tutti e cinque entrano nel goal aperto quel giorno —
*«vorrei chiudere tutti i punti rimanenti in questa sessione»* — insieme alle altre tredici voci
del `FUORI SCOPE` della chiusura precedente. Il vincolo «i due globali prima della fase 3» è
quindi soddisfatto con un margine ampio, e la fase 2.1 non li trova più davanti a sé: cadono
prima, e `Pages/Spese.razor` e `Pages/Home.razor` — che il task 4 della 2.1 modifica — non si
contendono più con loro.

## 5. La linea gratis / a pagamento

Proposta, da confermare alla fase 6.4 sui numeri veri raccolti nella fase 5:

| Gratis | A pagamento |
|---|---|
| Struttura: template pubblici, installazione, catalogo | **Le chiamate esterne oltre il tetto gratuito** |
| Calcoli: campi formula | Eventualmente: allegati e spazio |
| **I tool con dati esterni, fino al tetto** | |
| Tutto ciò che esiste oggi | |

**La categoria «tool con dati esterni» non è a pagamento: lo è il suo consumo oltre una
soglia.** La differenza non è cosmetica — un utente che non ha mai raggiunto il tetto usa il
prodotto completo, e chi paga sa esattamente per cosa. Il tetto esiste già dalla fase 5.2:
l'abbonamento non aggiunge un cancello, sposta un numero.

**Il criterio è la percezione, non il costo.** Un utente capisce di pagare per qualcosa che
consuma; un paywall su funzioni che non costano nulla si sente arbitrario — e, dato il §3.1,
va comunque difeso nel database, non nell'interfaccia.

---

## 6. Decisioni

### Chiuse il 10 settembre 2026

**1 — Le spese ricorrenti: si fanno.** Non si archiviano e non si rinviano. Sono la **fase 2**,
e la direzione dell'utente — *«più tabellare, gestione molto puntuale, non un tool da app
scema»* — coincide con la decomposizione già approvata il 3 settembre: ricorrenti, poi vista
tabellare, poi analisi. Sotto il nuovo modello non sono un residuo da smantellare: sono il tool
di punta.

**2 — Le immagini: ammesse.** L'applicazione **non deve funzionare offline**, quindi il vincolo
di `README.md:157` cade e le copertine si mostrano dal dominio del fornitore, senza copiarle in
Storage. Il ragionamento completo e le sue conseguenze — fra cui l'IP di chi guarda che raggiunge
il fornitore a ogni visualizzazione — stanno al **§3.6**. Il `README.md` si riscrive alla fase
5.5.

### Aperta

**3 — La piattaforma di donazione (blocca la fase 1.2, e nient'altro).** L'utente la
approfondisce. Tecnicamente sono equivalenti: è un link. **I numeri qui sotto non sono
riportati apposta** — le commissioni e la disponibilità per Paese cambiano, e un documento che
li fissa invecchia peggio di uno che dice dove guardare. Gli assi su cui confrontarle:

1. **Dove sta il pubblico.** GitHub Sponsors vive dentro GitHub: lo vedono sviluppatori, ed è
   invisibile a chiunque altro. Ko-fi e Buy Me a Coffee sono generalisti e **non richiedono un
   account a chi dona** — il che, per una donazione da tasca, conta più della commissione.
2. **Quanto trattiene la piattaforma**, che è una cosa diversa da **quanto trattiene il
   processore di pagamento**. Vanno lette come due voci separate, perché spesso vengono
   presentate come una.
3. **Come arrivano i soldi** — accredito diretto su PayPal, oppure payout via Stripe. Cambia i
   tempi e cosa devi già possedere per iniziare.
4. **Se paga verso l'Italia.** Si verifica in cinque minuti creando l'account, ed è la prima
   cosa da controllare, non l'ultima.
5. **Una tantum o ricorrente.** Tutte e tre offrono anche un *abbonamento di sostegno mensile*.
   **Attenzione:** un versamento ricorrente e continuativo verso di te assomiglia molto di più a
   un corrispettivo che a una liberalità. Il §3.4 dice di restare sulla donazione pura: se si
   abilita il sostegno mensile, lo si fa sapendo che avvicina la fase 1 alla fase 7, non prima
   di averne parlato con un commercialista.

### Chiusa il 19 settembre 2026

**4 — La fase dedicata all'interfaccia cade fra la 2.1 e la 2.2.** L'utente ha approvato il
**13 agosto 2026** un mandato pieno sulla UI/UX, con la direzione SLY (luce additiva su nero
pieno, non neon) e il lavoro fatto con un browser vero. **Questo documento non lo conteneva**,
perché nasce dagli appunti del 9 settembre, che parlavano di modello di prodotto.

Non è la stessa cosa dei cinque rilievi di `2026-09-10-rilievi-ui-critic.md`: quelli sono debito
con un numero, si chiudono in ore e hanno solo un vincolo di collocazione. Il mandato UI/UX è un
**progetto di settimane** con una direzione estetica da tradurre in decisioni. Le due cose non si
sostituiscono a vicenda, e infilare la seconda dentro la prima farebbe sparire la più grande.

**La collocazione, decisa il 19 settembre 2026.** L'utente ha delegato la scelta — *«lascio la
scelta a te»* — dopo aver letto la posizione di `tech-advisor`. La fase UI/UX si apre **dopo la
2.1 (spese ricorrenti) e prima della 2.2 (vista tabellare)**, diventando la **fase 2.1-bis** della
sequenza. Tre ragioni, in ordine di forza:

1. **La 2.1 ha spec e piano già approvati** (`2026-09-03-spese-ricorrenti-design.md` e il piano da
   sei task), costruiti sui pattern di oggi e su `PaginaRegistro`/`PaginaEditor`. Aprire la fase
   UI/UX prima significherebbe invalidare un piano approvato o farlo ripianificare, e la
   migrazione della 2.1 — l'unico pezzo irreversibile del piano, applicato a mano in produzione —
   resterebbe ferma ad aspettare una decisione estetica con cui non c'entra.
2. **La 2.2 è la prima schermata il cui design è ancora aperto.** Il §4 la chiama «il cuore della
   direzione *puntuale, non da app scema*» e la dà per *da progettare*: è l'unica del piano che si
   può disegnare **una volta sola**, sotto il sistema nuovo, invece di disegnarla e poi
   restilizzarla.
3. **La fase 3 moltiplica le schermate** — lo dice questo stesso documento al §4 — quindi il
   tetto invalicabile resta «prima della fase 3». «Dopo tutta la fase 2» è una collocazione
   difendibile, e costa la restilizzazione della tabellare; «dopo la fase 3» no.

`tech-advisor` dà confidenza **media** su questa sequenza, e il motivo va scritto invece che
taciuto: è una posizione su un progetto che **non ha ancora né spec né piano**, quindi la sua
dimensione reale non è nota a nessuno. Quando la fase UI/UX passerà dal proprio
`brainstorming → spec → piano`, questa collocazione va riconfermata sui numeri che ne usciranno,
non data per acquisita.

---

## 7. Cosa non si fa

| | Perché |
|---|---|
| **Wrapper desktop** (MAUI, Electron, Tauri) | §2.1. La PWA installata è già l'app desktop |
| **Proxy generico verso API arbitrarie** | §3.3. DNS rebinding non chiudibile in Deno, e custodia di chiavi altrui senza capacità di risposta |
| **Esecuzione di codice scritto dall'utente** | Escluso dall'utente. Senza sandbox è XSS persistente su chi condivide lo spazio; con sandbox è un'API di plugin da versionare; se deve girare quando l'utente non c'è serve un runtime multi-tenant |
| **API pubblica dichiarata** | §2.3. Quando il server OAuth 2.1 esce di beta *e* qualcuno la chiede |
| **Kickstarter** | §3.4. È una prevendita, cioè attività commerciale |
| **«Dona per sbloccare X»** | §3.4. Fa collassare la fase 1 dentro la fase 7 |

---

## 8. Provenienza

`tech-advisor` consultato due volte il 9-10 settembre 2026. Confidenza dichiarata: **alta** sul
disegno del paywall, sui confini della scala dei tool, sul «non fare wrapper», sull'esclusione
del proxy generico; **media** sui dettagli del Play Store (regole in movimento), su PWABuilder,
sulla percentuale di riuso della cache; **bassa** sul fiscale, dove non ha titolo.

Verificato nel codice di questo repo: `wwwroot/manifest.webmanifest`, `wwwroot/index.html:63`,
`Layout/VetrinaLayout.razor`, `Pages/Benvenuto.razor`, `Services/BrowserSessionHandler.cs:19`,
`Services/SupabaseService.cs:279-282`, `Services/MarkdownRenderer.cs:23-24`,
`Models/CampoDefinizione.cs`, `supabase/config.toml:171-174` e `:366-372`,
`supabase/migrations/20260812120000_collections.sql`, `README.md:157`, assenza di
`supabase/functions/`, `docs/superpowers/specs/2026-08-11-eton-design.md` §2 e §2.1.

Verificato in documentazione esterna: Stripe Payment Links (`client_reference_id`), esempio
ufficiale Supabase `stripe-webhooks`, pricing Supabase, Supabase Auth (sessioni e rotazione dei
refresh token, redirect URL, server OAuth 2.1, Vault, limiti delle Edge Functions), blog Android
Developers di giugno 2026 sulle fee di Play.
