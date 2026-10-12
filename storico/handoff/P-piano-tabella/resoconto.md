UNITÀ: P — ESITO: FATTO
TOCCATI: docs/superpowers/plans/2026-10-07-spese-tabella.md → +1540/−0 (nuovo; `git show --stat`: 1 file changed, 1540 insertions(+))
         handoff/P-piano-tabella/resoconto.md → nuovo
REVIEW: review: nessuna — solo documentazione (nessun implementer lanciato)
CONTRATTI: proposti nel piano, sezione «Partizione proposta in sessioni-unità», tabella «Firme fra le unità».
  - `ColonnaGriglia<T>`, `TipoColonna`, `Aggregazione`, `Gruppo<T>`, `Riepilogo`, `CalcoliGriglia.*`
    (unità A, task 1) → consumati da `Shared/Griglia.razor` (B, task 4 step 6, file nuovo) e da
    `Pages/SpeseTabella.razor` `Ricalcola()` (C, task 5 step 2-3, file nuovo)
  - `RigaSpesa` (`Chiave`, `ContaNeiTotali`), `StatoSpesa`, `TabellaSpese.*`, `VistaTabella`, `FiltriSpese`
    (A, task 2) → `Pages/SpeseTabella.razor` (C, step 1-5)
  - `EsitoMassa.DaEliminazione/DaModifica` (A, task 2) → `Services/ExpenseRepository.cs`, accanto a
    `EliminaAsync` (`Services/ExpenseRepository.cs:289-300`) (B, task 3)
  - `ExpenseRepository.CambiaCategoriaAsync(IReadOnlyCollection<Guid>, string) → Task<EsitoMassa>` e
    `EliminaTutteAsync(IReadOnlyCollection<Guid>) → Task<EsitoMassa>` (B, task 3) → `Pages/SpeseTabella.razor` (C, step 5)
  - `Griglia<T>` coi parametri del task 4 (compreso `AncoraCella`), `ModificaCella<T>`, `RispostaCella`,
    `EsitoCella`, `EsportaAsync` (B, task 4) → `Pages/SpeseTabella.razor` (C, step 3-6)
  - ancore `data-tutorial` (nove nomi) (C, task 5) → passi del tutorial in `Pages/SpeseTabella.razor` (D, task 6 step 4)
  - `PassoTutorial`, `Tutorial(Passi, Chiave)` (D, task 6) → `Pages/SpeseTabella.razor` e la 2.1-bis
  - firme esistenti consumate senza cambiarle: `ElencaConPrevisteAsync` (`Services/ExpenseRepository.cs:82`),
    `SalvaAsync` (`:231`), `Permessi.PuoIntervenire` (`Services/Permessi.cs:63`), `SchedaConflitto`
    (`Shared/SchedaConflitto.razor:31-35`), `ConfermaAzione` (`Shared/ConfermaAzione.razor:43-48`),
    `TestataPagina.Azione` (`Shared/TestataPagina.razor:56`), `NavigazioneSpese` (`Shared/NavigazioneSpese.razor:24`)
ADJUDICA: nessuna
FUORI SCOPE:
  - `Shared/TestataPagina.razor:8-10` dice «il progetto non ha un file .js proprio»: falso da quando esiste
    `wwwroot/js/grafo-spazio.js`. Commento da correggere, non della 2.2.
  - La guardia sulla data è in tre copie con due forme diverse (`Pages/Spese.razor:265` per anno,
    `Pages/SpesaEdit.razor:216-218` e `Pages/RicorrenteEdit.razor:246-248` per data). La 2.2 non ne aggiunge
    una quarta (va in `TabellaSpese.DataRagionevole`), ma riunire le tre esistenti è un lavoro a sé.
GATE: `git -C /g/Sviluppo/Eton/.claude/worktrees/unita-P-piano-tabella log --oneline -1 -- docs/superpowers/plans/2026-10-07-spese-tabella.md` → `e38b75c Il piano della 2.2: spese in tabella, sei task in quattro unità`
SCOSTAMENTI:
  - Nessuno sul mandato. Le correzioni alla spec stanno nel piano, sezione «Correzioni del 7 ottobre 2026»
    (dodici punti), e la spec non è stata toccata. Le tre che correggono il **testo** della spec, e non solo
    il codice che presupponeva:
    1. spec §5.4, ultima frase («una riga cancellata da un altro membro nel frattempo conta fra le
       eliminate»): con il conteggio «presenti prima ∖ presenti dopo» ratificato il 7 ottobre una riga già
       assente nella lettura «prima» non si conta fra le eliminate, si dice a parte («non c'era più»);
    2. spec §2.2, «i tipi sono quelli di `SchemaCampi.TipiAmmessi` più `money`»: `TipiAmmessi` ne ha sei
       (`bool` e `url` compresi), la spec ne elenca cinque; il piano dichiara i cinque e rimanda gli altri
       alla fase 3;
    3. spec §5.2, «è l'unico JavaScript della pagina»: servono due moduli generici (`griglia.js`,
       `tutorial.js`), e lo scaricamento del CSV, che Blazor non sa fare senza JavaScript.
  - Correzioni sul codice di oggi che i futuri mandati devono portarsi dietro: gli id delle previste
    cambiano a ogni lettura (`Services/CalcoliRicorrenti.cs:136`), quindi la chiave di riga non è l'`Id`;
    `InvariantGlobalization` (`Eton.csproj:10`) rende ordinale ogni confronto culturale, quindi
    l'ordinamento con accenti passa da `SchemaCampi.RiduciAccenti`; **nessuna migrazione serve** (i
    privilegi ci sono già), quindi nessun gate dell'utente fra le unità.
  - Lavoro nuovo arrivato durante l'unità: nessuno.

DECISIONI DA RATIFICARE (prese nel piano perché si annullano con un revert locale; la raccomandazione è la
scelta già scritta):
  - Nel CSV un testo che comincia con `=`, `+`, `-`, `@` esce preceduto da un apostrofo, per non diventare
    una formula in Excel (piano, correzione 9). Da tenere: negli spazi condivisi la descrizione la scrive un
    altro membro, e il file lo apre chi esporta.
  - Un carattere premuto su una cella **data** o **menù** entra in modifica senza sostituire (task 4 step 3).
    Da tenere: «sostituire con un carattere» lì non ha un significato.
  - L'URL si aggiorna con `replace: true` (task 5 step 2): ogni tasto nel filtro non diventa un passo della
    cronologia. Da tenere; il costo è che «indietro» porta alla pagina precedente, non alla vista precedente.
  - Il ricordo «tutorial visto» si scrive alla prima apertura, e fino ad allora il pulsante porta una
    pastiglia «nuovo» (task 6 step 2).

DOMANDE PER L'UTENTE (la spec non le decide; il piano ha un default che le rende non bloccanti. Non poste:
da porre in chat dal capo, con la posizione di `tech-advisor`):
  1. **«Ultimi 3 mesi» comprende il mese in corso?** Il piano dice sì: il mese corrente e i due precedenti
     (dal 1° agosto al 31 ottobre, visto da oggi). L'alternativa sono i tre mesi interi prima di questo.
     Raccomandazione: sì, il mese corrente compreso — è ciò che si intende guardando una tabella oggi.
  2. **Sul telefono la voce «Tabella» della sotto-navigazione si mostra?** La spec dice che sotto i `40rem`
     la pagina mostra solo un avviso con il link al registro, ma non dice se la voce compare nella riga
     «Registro · Tabella · Ricorrenti». Mostrata, porta a una pagina che dice di tornare indietro.
     Raccomandazione: nasconderla sotto i `40rem` con una regola CSS, nel task 5 step 8; la rotta resta e un
     link salvato continua a funzionare.
  3. **Che forma ha l'invito al tutorial?** La spec dice che il browser ricorda il tutorial visto «solo per
     non riproporre un invito», ma non dice quale invito. Il piano mette una pastiglia «nuovo» sul pulsante
     finché non lo si apre la prima volta. Raccomandazione: tenerla; nulla parte da solo, come vuole la spec.
  4. **Nelle prove nel browser, le righe di un altro membro.** Il collaudo lavora sul database di produzione
     e solo su righe create per la prova. Se nello spazio di prova non ci sono righe pagate da un altro
     membro, l'azione di massa «su righe miste» non si prova dal vivo, e il caso «3 non erano tue» resta
     coperto solo dal test puro. Raccomandazione: accettarlo come limite dichiarato; in alternativa l'utente
     crea lui una riga di prova da un secondo account in uno spazio condiviso (un agente non può: la policy
     di insert vuole `paid_by = auth.uid()`).
