# Misura finale della pagina Ricorrenti — esito (20 ott)

ui-critic   RILIEVI: 5

1. (alta, allineamento) a 390 px i metadati della riga vanno a capo dentro ciascun elemento: `.riga-spesa .meta`
   (`wwwroot/css/app.css:2145`) senza `flex-wrap`; fix `flex-wrap: wrap; row-gap: var(--s1)` e
   `.riga-spesa .meta > span { white-space: nowrap; }`. Stesso rilievo di ui-critic UC2 della 04.
2. (media, gerarchia) totale del gruppo 10,45 px, il testo più piccolo della pagina, stesso colore dell'etichetta
   (`Pages/Ricorrenti.razor.css:4`); fix `font-size: var(--t-sm); color: var(--testo-tenue)`.
3. (media, allineamento) bordo destro di «prossima:» varia di 35 px fra le righe; fix `min-width: 7rem` su
   `.riga-spesa .importo` (`app.css:2160`).
4. (bassa, gerarchia) entrata distinta dalla spesa solo dal «+»; proposta colore `--secondario`, da ratificare.
5. (bassa, progetto) 8 dimensioni di testo per `.dato { font-size: .95em }` (`app.css:417`).

Misurato senza rilievo: contrasto ≥ 6,08:1; spaziature {4, 8, 12, 16, 24, 32}; nessuno scorrimento orizzontale;
ordine dei gruppi corretto; quota 40,00 € = Annuali 10,00 + Periodiche 30,00; «rata 1/3», residuo 300,00 €.

Non misurabile: 390 px provato in un iframe (resize_window non cambia il viewport); angoli del contorno di fuoco.

Dati di prova (spazio «COLLAUDO GIRO B TEST», inizio 15/11/2026): cinque regole create ed eliminate dalla UI
(3dba070b…, 03992aae…, cc2ce881…, 63d5de9a…, 8cbcb820…). Dopo ricarica: Ricorrenti vuota, Registro 0,00 € ·
Entrate 0,00 € · Saldo 0,00 € come prima; spazio riportato a «Personale». Server di sviluppo (PID 29528) fermato
dal coordinatore, porta 5000 libera.
