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

### Le due decisioni prese dall'utente in conversazione

1. **Si fanno tutti e tre gli stadi** — catalogo con paywall, piattaforma, marketplace vero —
   in sequenza. Non sono alternative.
2. **Un «tool» creato da un utente deve arrivare a pescare dati da API esterne.** Non deve
   arrivare a eseguire codice scritto dall'utente.

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
nel catalogo** (fase 7). Toglie il costo di quota senza aprire lo spazio degli URL.

### 3.4 Il crowdfunding resta donazione pura

Piattaforma esterna con link. **Non Kickstarter**, che è crowdfunding *reward-based*, cioè una
prevendita, cioè attività commerciale.

**La trappola da evitare è una e precisa: «dona per sbloccare X».** Trasforma la donazione in
una vendita e fa collassare la fase 1 dentro la fase 6. Nessuna contropartita, nessun
collegamento fra chi dona e cosa vede nell'app.

*Sul fisco: nessuno degli autori di questo documento è un fiscalista.* Le liberalità senza
contropartita ricevute da persona fisica non sono reddito; se diventano abituali e legate
all'uso del software assomigliano a corrispettivi. Prima che superino la cifra da tasca, un
commercialista.

### 3.5 Le formule non si valutano con `eval`

Una formula scritta da un membro gira nel browser degli **altri** membri dello spazio. Un
mini-linguaggio senza rete e senza accesso alla pagina è sicuro per costruzione; un `eval` è
XSS persistente su chi condivide lo spazio.

---

## 4. La sequenza

Ogni fase è **una sessione**. Le stime sono ordini di grandezza, non impegni.

### Fase 0 — Chiudere il ciclo aperto · *giorni*

Preesistente a questo documento, e va finita prima di aprire qualunque altra cosa.

| | |
|---|---|
| 0.1 | **Giro D del collaudo** — brief pronto in `handoff/17-collaudo/D-brief.md`, non va riscritto. Richiede il server avviato dal capo e i PID annotati in `handoff/server.md` |
| 0.2 | **Sessione di chiusura** — prompt già scritto in `handoff/CHIUSURA.md`. Prima di aprirla si riempie il campo `ESITO DEL COLLAUDO` con la riga del giro D |
| 0.3 | **Decisione sulle spese ricorrenti** — v. §6, decisione aperta n. 1 |

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

### Fase 2 — I tool gratuiti: template condivisibili · *giorni-settimane · nessun server*

**Questo è il marketplace v1.** I tool gratuiti *sono* i template pubblici.

| | |
|---|---|
| 2.1 | Tabella `templates` (nome, icona, descrizione, `fields` jsonb, pubblico/privato, autore), con RLS e privilegi di colonna secondo il modello esistente |
| 2.2 | **Pubblicare** una collezione come template |
| 2.3 | **Installare** un template nel proprio spazio — copiare il jsonb |
| 2.4 | Il **catalogo** dentro l'app |
| 2.5 | Aggiornare la vetrina della fase 1: adesso mostra invece di promettere |

### Fase 3 — I tool che calcolano: campi formula · *settimane · nessun server*

| | |
|---|---|
| 3.1 | Un **mini-linguaggio senza I/O**, scritto da noi, senza rete e senza accesso alla pagina (§3.5) |
| 3.2 | Un tipo `formula` in `CampoDefinizione`, con la sua migrazione e i suoi vincoli |
| 3.3 | Valutazione nel browser, con gli errori resi come dice il progetto (mai un messaggio grezzo a schermo) |

### Fase 4 — Il primo tool con dati esterni, **gratis con un tetto** · *settimane*

Un solo fornitore, dall'inizio alla fine. **Nessun pagamento in questa fase:** il tool è
gratuito fino a un tetto basso di chiamate.

| | |
|---|---|
| 4.0 | **La decisione sulle immagini** — v. §6, decisione aperta n. 2. Va presa *prima* |
| 4.1 | La regola del §3.2 scritta nel codice: si pesca alla scrittura |
| 4.2 | Edge Function adattatore — la prima del progetto, oggi `supabase/functions/` non esiste. Verifica il JWT, prende `sub`, e **prima** della chiamata esterna esegue un `update contatori set n = n+1 where user_id = $1 and n < limite returning n` — atomico, nella stessa invocazione. Inaggirabile perché la chiave del fornitore esiste solo nei secret della funzione |
| 4.3 | Cache in uno **schema non esposto**, letta e scritta solo dal `service_role`. La domanda sulla RLS non nasce: nessun ruolo client la raggiunge |
| 4.4 | **La dichiarazione di trasferimento**: il tool dichiara «questo campo viene inviato a X», e lo spazio deve poterlo vedere |

**Tetto anche per spazio, non solo per utente:** account Google multipli sono gratuiti.

**Sul costo:** le API candidate (Open Library, Open-Meteo, TMDB non commerciale, cambi valuta)
sono gratuite o a quota. **Il costo vero è mantenere gli adattatori e stare dentro i termini
d'uso di ognuno.** I tool con dati esterni si vendono perché valgono, non perché costano.

**Perché questa fase viene prima del pagamento, e non dopo.** Il contatore per utente è il
paywall, spento. Quando la fase 6 arriverà, non dovrà costruire un gate: dovrà solo far
dipendere un `limite` già esistente da `has_plan()`. E soprattutto: **il tetto misura**. Se
nessuno lo raggiunge, non c'è niente da vendere, e lo si sa prima di aver aperto una partita
IVA. Costruire Stripe prima di questa fase è la trappola nominata da `tech-advisor` — il
problema tecnico meglio specificato e più divertente, affrontato senza sapere cosa si vende.

### Fase 5 — Le decisioni che precedono il denaro · *nessun codice*

Nessuna riga di codice, e **niente della fase 6 può partire prima**. Si apre solo se il tetto
della fase 4 viene raggiunto da qualcuno.

| | |
|---|---|
| 5.1 | **Partita IVA** — vendere un servizio in modo continuativo è attività d'impresa. Forma e regime: commercialista |
| 5.2 | **Supabase Pro** — il piano free **mette in pausa il progetto dopo una settimana di inattività**. Un prodotto a pagamento non ci sta sopra. È la prima voce di spesa, prima delle commissioni |
| 5.3 | **Play Store: pubblicare o no** — dal 30 giugno 2026 Google applica il **10% di service fee anche sugli abbonamenti pagati sul web** quando l'app è pubblicata. Va deciso **prima** di scrivere il webhook, non dopo |
| 5.4 | **La linea gratis / a pagamento**, confermata sui numeri veri della fase 4 — v. §5 |

### Fase 6 — L'abbonamento: alzare il tetto · *settimane*

| | |
|---|---|
| 6.1 | Tabella `subscriptions` con i privilegi del §3.1 e la funzione `has_plan()` |
| 6.2 | Edge Function `stripe-webhook`: `verify_jwt = false` in `config.toml`, firma verificata con `constructEventAsync`, segreti `STRIPE_API_KEY` e `STRIPE_WEBHOOK_SIGNING_SECRET`. Gestisce `checkout.session.completed` e `customer.subscription.updated/deleted` |
| 6.3 | **Stripe Payment Link statico** con `?client_reference_id=<auth.uid()>` — il valore torna nel webhook. Zero codice per creare sessioni di pagamento |
| 6.4 | Il `limite` della fase 4.2 diventa funzione di `has_plan()`. Le altre policy a pagamento chiamano la stessa funzione |
| 6.5 | Lo stato dell'abbonamento nell'interfaccia, e il portale clienti Stripe come link |

**Cosa non regge, ed è già stato scartato:** qualunque controllo in C#; il piano dentro il JWT
via Custom Access Token Hook (utile solo come cache, e il webhook serve lo stesso);
`stripe_fdw` dentro una policy (una chiamata API per riga).

### Fase 7 — Allargare il catalogo, e «porta la tua chiave» · *settimane*

| | |
|---|---|
| 7.1 | Secondo e terzo adattatore. Ogni fornitore è un adattatore da mantenere |
| 7.2 | **«Porta la tua chiave»**, limitata ai fornitori del catalogo (§3.3). Custodia in **Supabase Vault**, letto solo con `service_role`, revocato ad `anon` e `authenticated`. Vault è un deposito **di progetto, non per utente**: garantire che la funzione decifri solo il segreto del chiamante è responsabilità del codice, e un errore lì espone la chiave di un utente a un altro |

### Fase 8 — Il marketplace vero · *mesi · solo se le fasi 2-7 hanno prodotto creatori*

Gli utenti vendono i propri tool ad altri utenti. **Non è un incremento delle fasi precedenti:
è un cambio di natura.** Nel momento in cui il denaro passa *attraverso* di te per arrivare a
qualcun altro, smetti di vendere un servizio e diventi un intermediario di pagamento: Stripe
Connect, payout, rimborsi per conto di terzi, moderazione dei contenuti che vendi, fiscalità di
persone che non sei tu.

**La condizione per aprirla non è tecnica:** che esistano utenti che pubblicano template e
utenti che li installano, in numero tale da rendere sensato che qualcuno li paghi.

---

## 5. La linea gratis / a pagamento

Proposta, da confermare alla fase 5.4 sui numeri veri raccolti nella fase 4:

| Gratis | A pagamento |
|---|---|
| Struttura: template pubblici, installazione, catalogo | **Le chiamate esterne oltre il tetto gratuito** |
| Calcoli: campi formula | Eventualmente: allegati e spazio |
| **I tool con dati esterni, fino al tetto** | |
| Tutto ciò che esiste oggi | |

**La categoria «tool con dati esterni» non è a pagamento: lo è il suo consumo oltre una
soglia.** La differenza non è cosmetica — un utente che non ha mai raggiunto il tetto usa il
prodotto completo, e chi paga sa esattamente per cosa. Il tetto esiste già dalla fase 4.2:
l'abbonamento non aggiunge un cancello, sposta un numero.

**Il criterio è la percezione, non il costo.** Un utente capisce di pagare per qualcosa che
consuma; un paywall su funzioni che non costano nulla si sente arbitrario — e, dato il §3.1,
va comunque difeso nel database, non nell'interfaccia.

---

## 6. Decisioni aperte

Nessuna blocca la fase 0 o la fase 1.

**1 — Le spese ricorrenti.** Esistono un design (`docs/superpowers/specs/2026-09-03-spese-ricorrenti-design.md`)
e un piano da sei task (`docs/superpowers/plans/2026-09-03-spese-ricorrenti.md`), mai partiti.
Sono l'ultimo «servizio pre-impostato», cioè esattamente ciò da cui questo documento si muove
via. Tre uscite: **(a)** si fanno come previsto, perché il lavoro è già pagato; **(b)** si
rinviano dopo la fase 2 e diventano il **primo template** del catalogo; **(c)** si archiviano in
`storico/`. **Decide l'utente.**

**2 — Le immagini (blocca la fase 4, non prima).** Un libro pescato da ISBN porta una copertina,
e una copertina è un `<img>` verso il dominio di un terzo. Ma `README.md:157` dichiara: *«Nessun
asset remoto. Niente CDN, niente immagini o font caricati da altri domini: l'applicazione deve
funzionare offline una volta installata.»* Tre uscite: copiare le immagini in Supabase Storage
(costo e diritti d'uso), rinunciare alle immagini, o modificare il vincolo del README con
cognizione. **Va deciso prima del primo adattatore.**

**3 — La piattaforma di donazione (fase 1.2).** Tecnicamente equivalenti. Criterio: dove sta il
pubblico — GitHub Sponsors se sono sviluppatori, Ko-fi altrimenti. **Preferenza dell'utente.**

---

## 7. Cosa non si fa

| | Perché |
|---|---|
| **Wrapper desktop** (MAUI, Electron, Tauri) | §2.1. La PWA installata è già l'app desktop |
| **Proxy generico verso API arbitrarie** | §3.3. DNS rebinding non chiudibile in Deno, e custodia di chiavi altrui senza capacità di risposta |
| **Esecuzione di codice scritto dall'utente** | Escluso dall'utente. Senza sandbox è XSS persistente su chi condivide lo spazio; con sandbox è un'API di plugin da versionare; se deve girare quando l'utente non c'è serve un runtime multi-tenant |
| **API pubblica dichiarata** | §2.3. Quando il server OAuth 2.1 esce di beta *e* qualcuno la chiede |
| **Kickstarter** | §3.4. È una prevendita, cioè attività commerciale |
| **«Dona per sbloccare X»** | §3.4. Fa collassare la fase 1 dentro la fase 6 |

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
