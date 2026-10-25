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
