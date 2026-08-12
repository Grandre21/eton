# Eton

Note personali, note condivise e collezioni da recensire insieme.
Applicazione web installabile: <https://grandre21.github.io/eton/>

Ogni cosa vive dentro uno **spazio**. Ognuno ha il proprio spazio personale, che nessun altro
vede; gli spazi condivisi si creano e si aprono agli altri con un codice di sei caratteri, e da
lì in poi note, collezioni e voti appartengono a chi ci sta dentro.

- **Note** in Markdown, con anteprima.
- **Collezioni** con i campi decisi da chi le crea — testo, numero, data, sì/no, elenco a scelta —
  e un elenco di elementi che li compilano.
- **Recensioni**: un voto da 0,5 a 10 e un commento per persona, con media, ordinamento per voto
  e filtro «da provare».
- **Voto al buio**: una collezione può nascondere le recensioni altrui finché non hai messo la tua
  — resta visibile solo quante persone hanno recensito. Non è l'interfaccia a coprirle: è la policy
  di `reviews` a non lasciarle uscire dal database.

---

## Com'è fatto

Blazor WebAssembly **standalone**: non esiste un server applicativo. Il browser scarica
l'applicazione da GitHub Pages e da lì parla **direttamente** con Supabase — Gotrue per
l'autenticazione, PostgREST per i dati.

```
browser  ──►  GitHub Pages        (file statici: .wasm, .js, .css)
   │
   ├────────►  Gotrue    /auth/v1  (accesso con Google, flusso PKCE)
   └────────►  PostgREST /rest/v1  (dati, filtrati dalle policy RLS)
```

Tre conseguenze che spiegano quasi tutte le scelte del progetto:

1. **La chiave `anon` è pubblica.** Sta in `wwwroot/appsettings.json`, viene scaricata da chiunque,
   ed è normale che sia così. Non è un segreto e non protegge niente.
2. **La RLS è l'unico confine di sicurezza.** Non c'è un livello applicativo che possa filtrare:
   qualunque controllo scritto in C# è aggirabile interrogando PostgREST a mano. Ogni regola su chi
   vede e chi scrive cosa sta nelle migration, in `supabase/migrations/`.
3. **La RLS filtra, non concede.** Una policy restringe un privilegio che deve già esistere: senza
   il `GRANT` corrispondente la query fallisce con `permission denied` prima che qualsiasi policy
   venga consultata. E poiché dentro una policy non esiste `OLD`, le colonne che il client non deve
   poter cambiare — chiavi, `space_id`, `owner_id`, contatori di versione, date di creazione — sono
   difese dai **privilegi di colonna**, non dalle policy.

Il documento di design, con la matrice completa delle policy e il ragionamento dietro ognuna, è in
[`docs/superpowers/specs/2026-08-11-eton-design.md`](docs/superpowers/specs/2026-08-11-eton-design.md).

## Sviluppo

**Serve**: [.NET SDK 10](https://dotnet.microsoft.com/download), e — solo per lavorare sul
database — [Docker Desktop](https://www.docker.com/products/docker-desktop/) e la
[CLI di Supabase](https://supabase.com/docs/guides/cli).

```bash
dotnet run                                   # http://localhost:5000
dotnet test Eton.Tests/Eton.Tests.csproj     # la suite completa
```

L'applicazione avviata così punta al **Supabase di produzione**: la configurazione sta in
`wwwroot/appsettings.json`. Per lavorare su un database locale, cambia lì `Supabase:Url` e
`Supabase:AnonKey` con i valori che `supabase start` stampa alla fine.

### Database locale

```bash
supabase start        # richiede Docker acceso
supabase db reset     # ricrea da zero e riapplica tutte le migration
supabase stop
```

Le porte in `supabase/config.toml` **non** sono quelle predefinite della CLI:

| Servizio | Porta |
|---|---|
| API (PostgREST + Gotrue) | 55321 |
| PostgreSQL | 55322 |
| Studio | 55323 |
| Inbucket (posta finta) | 55324 |

Sono state spostate perché su Windows l'intervallo predefinito (54320-54419) ricade spesso fra le
porte che il sistema si è già riservato per conto proprio, e `supabase start` fallisce con un
errore di binding che non nomina la causa.

### Migration

I file in `supabase/migrations/` si applicano da soli in locale con `supabase db reset`. **In
produzione si applicano a mano**, incollandoli nel SQL Editor del pannello Supabase: non esiste un
passo di CI che tocchi il database, ed è deliberato — una migration sbagliata su un database di
produzione non si annulla con un `git revert`.

L'ordine conta: la migration va applicata **prima** del `git push` che pubblica il codice che la
usa, altrimenti fra il deploy e l'esecuzione della migration l'applicazione online interroga tabelle
che non esistono ancora.

Accanto alle migration ci sono gli script `verifica-rls-*.sql`: si eseguono su un database locale e
provano, da dentro il database, che le policy rifiutino ciò che devono rifiutare. Ogni script
dichiara in testa quanti errori deve produrre; ottenerne un numero diverso significa che qualcosa è
cambiato.

## Pubblicazione

Ogni push su `main` fa partire `.github/workflows/deploy.yml`, che pubblica su GitHub Pages. Due
passaggi del workflow non sono ovvi e non vanno tolti:

- **Il `base href` viene riscritto nel sorgente**, prima di `dotnet publish`. Il sito è servito da
  `/eton/` e non dalla radice del dominio. La riscrittura va fatta prima perché la pubblicazione
  registra in `service-worker-assets.js` l'hash SHA-256 di ogni file: modificare `index.html` dopo
  che il suo hash è stato calcolato lo fa rifiutare dal controllo di integrità, e l'applicazione non
  si avvia affatto.
- **`.nojekyll`**, senza il quale GitHub Pages scarta le cartelle che cominciano con un underscore,
  cioè `_framework`, cioè tutto il runtime .NET.

## Struttura

```
Pages/          una pagina per rotta (@page)
Shared/         componenti riusabili
Layout/         MainLayout (privato) e VetrinaLayout (pubblico)
Models/         le righe del database, come le vede Postgrest
Services/       accesso ai dati, sessione, helper puri
Eton.Tests/     xUnit — solo logica pura, nessun database
supabase/       migration e script di verifica delle policy
docs/           design e piani
wwwroot/css/    app.css: foglio unico, tutto costruito sulle variabili di :root
wwwroot/fonts/  Inter, un solo woff2 variabile per tutti i pesi
```

La navigazione è **un solo componente** (`Shared/Navigazione.razor`) in due forme: barra in fondo
allo schermo sul telefono, colonna a sinistra da 64rem in su. A cambiare è solo il CSS — due
componenti separati sarebbero due elenchi di voci da tenere allineati a mano.

I due colori d'accento non sono intercambiabili, ed è scritto in testa ad `app.css`: il **blu** è
dove si preme (pulsanti, collegamenti, voce attiva, campo a fuoco), il **verde acido** è dove si
constata (voti, medie, conteggi). Usare il verde per un pulsante toglie all'interfaccia
un'informazione che oggi trasmette senza parole.

Alcune regole che il codice dà per acquisite:

- **`InvariantGlobalization` è attivo** (`Eton.csproj`). `new CultureInfo("it-IT")` lancia a runtime:
  le date e i numeri si formattano con `CultureInfo.InvariantCulture` e, dove serve la virgola
  decimale, la si scrive a mano (v. `Services/CalcoliVoti.Testo`).
- **Nessun asset remoto.** Niente CDN, niente `@import`, niente immagini o font caricati da altri
  domini: l'applicazione deve funzionare offline una volta installata. Il font (Inter) è servito da
  noi, in `wwwroot/fonts/`, in un unico file variabile che copre tutti i pesi; le icone della
  navigazione sono SVG disegnati in `Shared/Icona.razor`. Se aggiungi un formato di file nuovo,
  ricordati di elencarlo in `offlineAssetsInclude` dentro `wwwroot/service-worker.published.js`,
  altrimenti offline non c'è.
- **In `Release` il trimming è `full`.** I tipi di Gotrue e Postgrest sono costruiti per reflection
  da Newtonsoft, quindi i loro assembly sono dichiarati `TrimmerRootAssembly`: senza, l'applicazione
  compila, passa i test, e fallisce **solo da pubblicata** con «Unable to find a constructor to use».
- **Tutto è in italiano**: testi, commenti, nomi di variabili, classi CSS. E i commenti spiegano
  *perché* una scelta è stata fatta, non cosa fa la riga sotto — spesso citando l'alternativa
  scartata e il motivo per cui è stata scartata.

## Stato

Funzionano: accesso con Google, spazi personali e condivisi con codice d'invito, note in Markdown,
collezioni a campi liberi con i loro elementi, recensioni con media e filtri, voto al buio, vetrina
pubblica prima dell'accesso, interfaccia a colonna su schermo largo.

Non ci sono ancora: allegati e immagini caricate, ricerca, notifiche, passaggio di proprietà di uno
spazio, applicazione sul Play Store (prevista come TWA attorno a questa stessa PWA).
