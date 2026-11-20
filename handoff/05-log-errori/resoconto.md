# 05 — I log degli errori gestiti non accendono più la barra

ESITO: FATTO
COPERTURA: 8 clausole · coperte 4 · scoperte 0 · rinviate 4
(le 4 rinviate erano, per costruzione, i passi successivi alla revisione: prova nel browser, resoconto, commit e push, riga di chiusura; tutti fatti qui sotto)

COSA È CAMBIATO
- Services/Diagnostica.cs, nuovo: `public static class Diagnostica` con due metodi.
  - `ErroreGestito(string)` scrive su stdout (console.log): per l'errore che la pagina racconta già con un suo messaggio, o che degrada da sé.
  - `ErroreNonSegnalato(string)` scrive su stderr di proposito: per il guasto che lo schermo non racconta in altro modo, dove la barra «errore imprevisto» resta l'unico segnale.
- 105 `Console.Error.WriteLine(` in 29 file (Pages, Shared, Services) → `Diagnostica.ErroreGestito(`, argomento identico (confronto a macchina di bug-hunter: 105 su 105). Le due eccezioni, sotto, usano `ErroreNonSegnalato`.
- Pages/BudgetEdit.razor:277: il `Console.WriteLine` della correzione 04 passa alla stessa forma; il commento sul perché ora sta una volta sola sull'helper.
- Shared/Griglia.razor:334 e :345 (esportazione CSV fallita): `Diagnostica.ErroreNonSegnalato(`. Lì nessuna pagina mostra un messaggio (Pages/SpeseTabella.razor:704-706 non lo ha), quindi la barra resta, come prima.
- Nessun messaggio letto dall'utente è cambiato. Il percorso delle eccezioni non gestite del framework non è toccato (nessuna modifica a Program.cs, wwwroot/index.html o al JS).

PERCHÉ LA FORMA
tech-advisor ha raccomandato l'helper unico su stdout, scartando:
- `ILogger<T>`: @inject in circa 25 file, costruttori nuovi, e due casi fuori dalla DI (`ValoriElemento` è statica; `AllineatoreProfilo` nasce con `new` in Program.cs:25);
- `Console.WriteLine` diretto: nessun punto unico da cambiare domani.
Verificato da tech-advisor nel JS del framework (bin/Debug/net10.0/wwwroot/_framework/blazor.webassembly.w3qd1tpl0e.js): `out` → console.log, `err` → console.error più barra, e `dotNetCriticalError` è la stessa funzione di `err`. Che il logger di WASM mandi Warning a console.warn senza barra è letto dal sorgente del tag v10.0.0, non dal pacchetto 10.0.10 installato.

review:
  bug-hunter      RILIEVI: 1
  conformity      RILIEVI: 1
  threat-hunter   RILIEVI: 0
  backend-expert  RILIEVI: 0
  checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
  checker         VERDETTI: risolti 2 · non risolti 0
  coverage        COPERTURA: 8 clausole · coperte 4 · scoperte 0 · rinviate 4
  ui-critic       non lanciato — tabella §7 riga 1 — 30 files changed, 125 insertions(+), 108 deletions(-)
(la revisione è stata fatta una volta sul diff cumulativo dei tre brief meccanici: 30 files changed, 118 insertions(+), 108 deletions(-) · create mode 100644 Services/Diagnostica.cs · 1 dichiarazione; i .razor sono toccati solo dentro @code)

ADJUDICA
- conformity, bassa, Services/Diagnostica.cs:9 `internal` mentre le altre classi statiche di Services sono `public` → fondato → corretto. verificato: risolto — Services/Diagnostica.cs:9 `public static class Diagnostica`.
- bug-hunter, media, Shared/Griglia.razor:332-346: l'esportazione fallita non ha nessun messaggio a schermo, quindi la barra era l'unico segnale → fondato → corretto con `ErroreNonSegnalato` (comportamento uguale a prima). verificato: risolto — Shared/Griglia.razor:334 e :345. Il checker ha riletto tutte le 106 chiamate: è l'unico punto senza messaggio e senza una degradazione dichiarata nei commenti.

PROVA NEL BROWSER (live-testing, browser integrato, http://localhost:5000, sola lettura)
Il database dietro l'app di sviluppo è quello di produzione: la prova ha bloccato nella pagina le richieste a supabase.co/rest, quindi nessuna lettura o scrittura è arrivata al database (0 richieste passate).
- Note e Spese, a rete bloccata: la pagina mostra «Lo spazio si è caricato, ma non è stato possibile leggerne le note: …» (e l'equivalente per le spese) con «Riprova». `#blazor-error-ui` display «none». In console: «[log] [Registro] Lettura del registro non riuscita: TypeError: rete bloccata per la prova», a livello log e mai error.
- Controprova: `Blazor._internal.dotNetCriticalError('prova barra')` (la funzione del framework collegata a stderr) → barra visibile (display «block»), chiusa col ✕.
- Console: solo il rumore noto del service worker di sviluppo.

GATE: mcp__synapse__build root=G:\Sviluppo\Eton → build: succeeded — 0 errors, 0 warnings — 2.6s — root: G:/Sviluppo/Eton
GATE: mcp__synapse__test root=G:\Sviluppo\Eton → test: 0 failed, 452 passed, 0 skipped — 4.2s — root: G:/Sviluppo/Eton

DECISIONI DA RATIFICARE
- 2026-11-21 forma unica `Diagnostica.ErroreGestito` su stdout (raccomandazione di tech-advisor), da ratificare. Si annulla con un revert. Se un giorno servono gli errori nel filtro «Errors» o «Warnings» dei DevTools, si cambia la sola riga dell'helper.
- 2026-11-21 l'esportazione CSV fallita tiene la barra (`ErroreNonSegnalato`) invece di avere un messaggio nuovo, perché il mandato chiedeva di non cambiare i messaggi; da ratificare.

FUORI SCOPE
- L'esportazione della tabella spese fallita non ha un messaggio suo: oggi l'utente vede solo la barra generica. Un messaggio vero vuol dire testo nuovo per l'utente, più un esito restituito da `Griglia.EsportaAsync` e mostrato da `SpeseTabella` (per esempio in `avvisoPagina`).
- checker, sotto soglia: in Pages/SpaceDetail.razor:287 il catch della rilettura dei membri dopo una rimozione non ha un commento che dichiari la degradazione (la pagina mostra comunque l'avviso). Era così già prima.
- checker, sotto soglia: il summary della classe in Services/Diagnostica.cs:3-8 parla solo di stdout. Il metodo `ErroreNonSegnalato` dice di suo che va su stderr.
- Il filtro «Errors» dei DevTools non mostra più gli errori gestiti (ora sono log). È la conseguenza voluta.

SCOSTAMENTI
- I brief erano tre, meccanici e su file disgiunti; la revisione è stata una sola, sul diff cumulativo, con tutti e quattro i revisori.
- `handoff/PIANO.md` non è stato creato: nessuno l'ha chiesto.
- L'app è stata avviata e fermata da questa sessione (PID 16696, handoff/05-log-errori/app.md).
