# Prova nel browser della 02, dalla sessione del capo

2026-10-25 utente (chat, popup): «Sì, prova tu (Consigliata)» — prova sui dati di produzione con un budget di
prova e una spesa «PROVA AGENTE», poi eliminati.

Server: `dotnet run --project G:/Sviluppo/Eton/Eton.csproj --launch-profile Eton --no-build`, dalla radice
del progetto (main con la 02 fusa), http://localhost:5000, PID in ascolto 26472, job del capo b03ah6ouw.
Da fermare a prova finita. Fermato il 25 ott (TaskStop b03ah6ouw).

## Esito di live-testing (25 ott): difetti, prova interrotta al passo 8

- Passi 2-6 ok: creazione, «Da spendere nei budget» 50 → 38, «Registra una spesa» con categoria precompilata e
  ritorno automatico, residuo 38, cambio mese senza lampeggio. Passo 7: «sforato di 2,00 €» corretto.
- Difetto 1 — la barra del budget è invisibile: in `/expenses/budgets` la griglia `ul.barre-categorie` ha 4
  colonne (143,6 / 436,4 / **0** / 130,4 px) e `span.barra` cade nella colonna da 0 px (misurata 0×8 px).
- Difetto 2 — su «Nuovo budget», scelta solo la categoria e nessun importo, «Torna ai budget» ha congelato la
  scheda (CDP in timeout): compatibile con la guardia d'uscita nativa attiva già per la sola categoria.
- Difetto 3 (da confermare se voluto) — con zero budget la pagina non mostra mese e frecce ‹ ›.
- Osservazione — «sforato» usa il verde d'accento, nessun colore d'allarme (già da ratificare nella 02).
- Non provati: passo 9 (categoria di entrata nell'indirizzo), passo 10 (390 px), pulizia.
- Console: nessun errore dell'app.
- Lasciati in produzione, spazio «Personale»: spesa «PROVA AGENTE» 12,00 € Istruzione 25/10/2026; budget
  Istruzione 10,00 € chiuso a ottobre 2026. Pulizia data all'utente in chat (SQL).

## Seconda prova (12 nov), dopo la fusione della 04

Sì dell'utente in `handoff/PIANO.md`, DECISIONI, 2026-11-12. Server: `dotnet run --project
G:/Sviluppo/Eton/Eton.csproj --launch-profile Eton --no-build` su `main` 6a2692c (build verde, 0 errori),
http://localhost:5000, PID in ascolto 24440, job del capo bzq2lc3xv. Da fermare a prova finita.

Primo lancio di live-testing rifiutato dal classificatore della modalità auto; l'utente ha cambiato modalità
(«Cambio modalità, riprova») e il secondo lancio è partito.

### Esito di live-testing (12 nov): `ESITO: difetti 2`, nessun difetto funzionale

- Passo 0: i resti del 25 ott c'erano (spesa «PROVA AGENTE» e budget Istruzione chiuso a ottobre); eliminati
  dall'interfaccia. La pulizia SQL del 25 ott quindi non era stata eseguita.
- Passi 1-4, 6-9, 11-14 ok: voce «Budget», stato vuoto con mese e frecce (difetto 3 della 04 corretto),
  guardia senza importo senza finestra nativa (difetto 2 corretto), creazione, «Da spendere nei budget»
  50 → 38, «Registra una spesa» con ritorno automatico, cambio mese senza lampeggio, chiusura,
  `?categoria=Stipendio` senza effetto, 390 px senza scorrimento (misura sintetica in iframe, il browser non
  scende sotto 574 px), azioni alte 48 px.
- Passo 5: barra larga 408-416 px a 1260 px (difetto 1 della 04 corretto), riempimento = quota × barra;
  **la barra sta 6,1 px sopra il centro delle cifre** a schermo largo (a 390 px è centrata) → passato a
  ui-critic come rilievo di resa.
- Passo 10: «sforato di 2,00 €» corretto, colore `rgb(182, 243, 106)` uguale a un budget rispettato → non è
  un difetto: è la scelta del PIANO-DESIGN della 02 e della 04, già fra le DA RATIFICARE della 02.
- Non provabile: la sezione «In arrivo» (nessun budget futuro); la seconda metà della guardia (con importo
  scritto, finestra nativa) resta all'utente a mano.
- Dati lasciati in produzione: nessuno. Console: nessun errore; rete: nessuna risorsa con stato ≥ 400.

### ui-critic (12 nov), correzioni e tracciato

ui-critic `RILIEVI: 5` (metro PIANO-DESIGN della 04): UC1 barra 6,08 px sopra le cifre (media), UC2 azioni
equidistanti fra due budget (media), UC3 «Salva» a 0 px dal campo importo in «Nuovo budget» (media), UC4
`aria-pressed=""` sulle pastiglie di tutta l'app (media, TIPO progetto → all'utente, `PIANO.md` APERTO), UC5
doppio stacco in fondo alla scheda del mese (bassa, corretto nel brief aperto dagli altri). Fuori dal tetto,
decisioni del PIANO-DESIGN non adjudicate: cifra e link di «Da spendere nei budget» in grigio, residuo non
distinto dal totale, binario della barra a contrasto 1,21:1 → per la 2.1-bis.

    review: correzioni di resa del budget
      bug-hunter      RILIEVI: 2
      threat-hunter   RILIEVI: 0
      conformity      non lanciato — 1 file changed, 17 insertions(+)
      backend-expert  non lanciato — 1 file changed, 17 insertions(+) · 0 create mode · 0 dichiarazioni/endpoint
      checker         VERDETTI: fondati 2 · infondati 0 · fuori scope 0 · non verificabili 0
      checker         VERDETTI: risolti 6 · non risolti 0 · non verificabili 0
      coverage        COPERTURA: 3 clausole — coperte 3 · scoperte 0 · rinviate 0

- BH1 (bassa): fondato → corretto — il commento sopra `label.campo + .azioni` taceva RecensioniElemento e
  ItemEdit; regola tenuta per tutti gli editor (da ratificare, `PIANO.md`) — verificato: risolto —
  wwwroot/css/app.css:953-958
- BH2 (bassa): fondato → corretto — durante la rilettura del mese lo scheletro conservava il margine —
  verificato: risolto — wwwroot/css/app.css:2039
- UC1 verificato: risolto — wwwroot/css/app.css:2408; UC2 — :2120; UC3 — :958; UC5 — :2039.
- NON VERIFICABILE (bug-hunter), non riprovato nel browser per la regola del §7 (la correzione di ui-critic
  si verifica a codice): resa della barra centrata con nome su due righe nel Registro, e dei 28 px fra
  budget successivi. Li vedrà l'utente o la 2.1-bis.
- GATE: `mcp__synapse__test root=G:\Sviluppo\Eton` → `test: 0 failed, 442 passed, 0 skipped`.
- Server fermato il 12 nov (TaskStop bzq2lc3xv).
