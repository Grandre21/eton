BRIEF DI CORREZIONE — unità 07 (stesso file, stesse regole del brief precedente)

Lavori nel worktree G:\Sviluppo\wt-Eton\07-testi, solo `Pages/RicorrenteEdit.razor`. Con tipo Spesa ogni testo
resta IDENTICO a HEAD (salvo i testi neutri già decisi e quelli del punto 4, che valgono solo a regola non caricata).
I numeri di riga sono quelli del file come l'hai lasciato.

1. ARTICOLI (con Entrata oggi esce «La entrata», «una entrata», «la entrata»).
   - Aggiungi accanto a `UnNomeTipo`, con commento di una riga nello stesso stile:
     `private string LaNomeTipo => tipo == TipoMovimento.Entrata ? "l'entrata" : "la spesa";`
   - :638 «terminare la {NomeTipo} ricorrente» → «terminare {LaNomeTipo} ricorrente»
   - :656 e :664 «eliminare la {NomeTipo} ricorrente» → «eliminare {LaNomeTipo} ricorrente»
   - :488 «creare una {NomeTipo} ricorrente» → «creare {UnNomeTipo} ricorrente»
   - :14 l'inizio «La @NomeTipo ricorrente» → `@(tipo == TipoMovimento.Entrata ? "L'entrata" : "La spesa") ricorrente`
     (stessa forma del ternario già usato a :134-136).
   Le forme «questa @NomeTipo» / «questa {NomeTipo}» / «nessuna @NomeTipo» restano: sono corrette per entrambi.

2. RINOMINA: la proprietà `ChiLaPaga` diventa `Titolare` (valori invariati), in tutti i suoi usi. Commento:
   `// «chi la paga» o «chi la riceve»: la persona a cui la ricorrente appartiene.`

3. TIPO NOTO: aggiungi, vicino alle proprietà del tipo, con commento di una riga che dica perché
   (finché la regola non è caricata — caricamento, errore in apertura, regola sparita — `tipo` è ancora il default Spesa):
   `private bool TipoNoto => Nuova || regola is not null;`

4. TESTI A REGOLA NON CARICATA:
   - Pannello Aiuto (:14-15): avvolgi i due paragrafi attuali in `@if (TipoNoto) { … } else { … }`; nel ramo else
     due paragrafi neutri:
     «Una ricorrente si segna da sola nel mese in cui è dovuta, quando chi la paga o la riceve apre Eton.»
     «Se qualcun altro salva questa ricorrente mentre la modifichi, o se nel frattempo Eton ci segna il movimento del mese, il salvataggio si ferma e ti fa scegliere fra ricaricare la versione più recente e sovrascrivere con la tua.»
     Il terzo paragrafo («Uscendo con modifiche…») resta fuori dall'if, invariato.
   - `TitoloSchermata` (:356 circa): quando non è `Nuova` e `!TipoNoto` restituisce "Ricorrente"; gli altri casi invariati.

BUDGET: nessuna astrazione oltre `LaNomeTipo` e `TipoNoto`; nessun tipo nuovo; nessun test nuovo; niente CSS; commenti esistenti intatti.
Alla fine `grep -n -i "spes\|la paga"` sul file: ogni occorrenza visibile rimasta deve essere un ramo Spesa, :274, un commento o un identificatore.

FORMATO DI RITORNO, nel file `-o` (è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`):
FATTO / DEVIAZIONI / VERIFICA / NOTE — oppure `BLOCKED: <domanda>`.

VERIFICA: `dotnet build` e `dotnet test` nel worktree; il gate ufficiale lo rilancia la sessione con Synapse. Non avviare Synapse.Server.

Le modifiche ai file solo con apply_patch: vietati `Set-Content`, `Out-File`, `[System.IO.File]::Write*`.
Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.
Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/07-testi/resoconto.md`.
Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.
Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
