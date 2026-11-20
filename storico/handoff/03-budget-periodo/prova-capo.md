Prova del capo con salvataggio — 2026-11-20, sui dati di produzione, sì dell'utente in chat («Sì, procedi»).
Eseguita da live-testing nel browser integrato dell'app, spazio «Personale», app di sviluppo su http://localhost:5000.

passo 1: OK — «Ancora nessun budget qui.», nessun budget preesistente.
passo 2: OK — Casa, 10, «PROVA 2.5», per 3 mesi: «Vale da novembre 2026 a gennaio 2027.»; Salva (POST 201).
passo 3: OK — riga «Casa» con «PROVA 2.5 · da novembre 2026 a gennaio 2027» e i tre pulsanti; a 375 px nessun overflow.
passo 4: OK — «Spese del mese» → /expenses/table?periodo=libero&da=2026-11-01&a=2026-11-30&tipo=spesa, nessuna categoria.
passo 5: OK — «Mese successivo» disabilitato a novembre 2026.
passo 6: OK — Modifica: periodo giusto, nessun menu Durata, «Chiudi» presente.
passo 7: KO — limite di 200 caratteri e salvataggio OK; con 200 caratteri senza spazi overflow orizzontale
         (1280 px: scrollWidth 1808 contro 1265; 375 px: 1466 contro 375), lo span.spiega in .azioni non va a capo.
         Schermate passo7-1280-overflow.jpg, passo7-375-overflow.jpg.
passo 8: OK — chiusura anticipata: «Chiuso: vale fino alla fine di questo mese.», poi «solo novembre 2026», «Chiudi» sparito.
passo 9: KO in parte — il messaggio della sovrapposizione compare col testo esatto e il modulo resta, ma subito dopo
         compare la barra «Si è verificato un errore imprevisto. Ricarica» (#blazor-error-ui), riprodotto due volte.
         Console: 400 e «[Budget] Rifiutato per sovrapposizione: {"code":"23P01", …}». Schermata passo9-sovrapposizione.jpg.
passo 10: OK — pulizia completa: eliminati 9f651189-d20b-4ff0-b1fe-7ad1d827b5a8 (novembre, «PROVA 2.5 bis») e
          8170c474-5c3b-4f8d-8ff2-b97483e3e801 (dicembre, «PROVA 2.5 sovrapposto»); l'elenco torna «Ancora nessun budget qui.».
Nota: con soli budget futuri la card del mese dice «Nessun budget in questo mese.» e il budget si raggiunge da «In arrivo».
Console: a parte il passo 9, solo l'errore noto del service worker in sviluppo.
App fermata dal capo dopo la prova.
