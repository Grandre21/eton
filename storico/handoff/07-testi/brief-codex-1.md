BRIEF — unità 07: le scritte «spesa» sull'editor di un'entrata ricorrente

Lavori nel worktree G:\Sviluppo\wt-Eton\07-testi (Blazor WebAssembly, .NET). Solo testi, zero CSS, zero markup nuovo.

OBIETTIVO
In `Pages/RicorrenteEdit.razor`, con una regola di tipo Entrata nessuna scritta visibile deve parlare di
«spesa/spese» né di «chi la paga». Con tipo Spesa ogni testo resta IDENTICO carattere per carattere a oggi,
salvo le due frasi neutre (punti 3 e 4).

FILE DA TOCCARE: solo `Pages/RicorrenteEdit.razor`.
NON TOCCARE: ogni altro file (CSS, Models/, Services/, supabase/, docs/, handoff/ salvo il tuo file -o).
I commenti `@* … *@` e `//` non si toccano: solo le stringhe che l'utente vede.

PATTERN ESISTENTI DA RISPECCHIARE
- `Pages/RicorrenteEdit.razor:340` `private string NomeTipo => TipoMovimento.Testo(tipo).ToLowerInvariant();` («spesa»/«entrata»)
- `Pages/RicorrenteEdit.razor:343` `private string NomeTipoPlurale => tipo == TipoMovimento.Entrata ? "entrate" : "spese";`
- uso nel markup: `:54` `Questa @NomeTipo ricorrente …`; uso in stringa interpolata: `:47` `$"Mentre la modificavi, questa {NomeTipo} ricorrente è cambiata."`

MODIFICHE (numeri di riga del file attuale)

1. Aggiungi subito sotto `NomeTipoPlurale` (:343), ciascuna con un commento di una riga nello stile di :339/:342:
   - `private string UnNomeTipo => tipo == TipoMovimento.Entrata ? "un'entrata" : "una spesa";`
   - `private string ChiLaPaga => tipo == TipoMovimento.Entrata ? "chi la riceve" : "chi la paga";`

2. :14 diventa
   `<p>La @NomeTipo ricorrente segna da sola @UnNomeTipo nel mese in cui è dovuta, quando @ChiLaPaga apre Eton.</p>`
   :15 — sostituisci «questa spesa ricorrente» con «questa @NomeTipo ricorrente» e «ci segna una spesa del mese» con
   «ci segna @UnNomeTipo del mese»; il resto della frase invariato.

3. :27 (regola sparita, il tipo non è noto) — frase neutra:
   «Questa ricorrente non c'è più: l'ha cancellata qualcuno, oppure non fai più parte dello spazio.»

4. :442 (errore in apertura, il tipo non è ancora caricato) — frase neutra:
   «Non è stato possibile aprire questa ricorrente: può essere la connessione, oppure il tuo accesso a questo spazio che è cambiato. Riprova fra un momento.»

5. :54 — sostituisci «solo chi la paga» con «solo @ChiLaPaga».

6. Stringhe in `@code`, diventano interpolate `$"…"` con:
   - :482 «creare una {NomeTipo} ricorrente.»
   - :544 «modificare questa {NomeTipo} ricorrente:»
   - :613 `$"Terminata: da domani non segna più {NomeTipoPlurale}."`
   - :621 «terminare questa {NomeTipo} ricorrente:»
   - :632 «terminare la {NomeTipo} ricorrente:»
   - :650 «eliminare la {NomeTipo} ricorrente: può cancellarla solo {ChiLaPaga}, o chi possiede lo spazio.» (resto invariato)
   - :658 «eliminare la {NomeTipo} ricorrente: se nel frattempo ha segnato delle {NomeTipoPlurale} si può solo terminare,» (resto invariato)

7. NON cambiare: :136 (già distinto per tipo), :274 («Le spese già segnate restano.»: un piano a rate è sempre una spesa),
   nessun'altra stringa. Alla fine fai un `grep -n -i "spes"` sul file e controlla che ogni occorrenza rimasta sia un commento,
   un identificatore/classe CSS, la :136 nel ramo Spesa o la :274.

BUDGET DI COMPLESSITÀ: nessuna astrazione nuova oltre le due proprietà del punto 1; nessun tipo nuovo; nessun test nuovo.

FORMATO DI RITORNO, nel file `-o` (è il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`):
FATTO / DEVIAZIONI / VERIFICA / NOTE — oppure `BLOCKED: <domanda>`.

VERIFICA: `dotnet build` e `dotnet test` nel worktree; il gate ufficiale lo rilancia la sessione con Synapse. Non avviare Synapse.Server.

Le modifiche ai file solo con apply_patch: vietati `Set-Content`, `Out-File`, `[System.IO.File]::Write*`.
Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.
Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/07-testi/resoconto.md`.
Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.
Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
