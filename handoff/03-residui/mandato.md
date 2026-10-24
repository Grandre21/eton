UNITÀ: 3/3 — I residui dati della 2.3

OBIETTIVO: i quattro punti della spec `docs/superpowers/specs/2026-10-24-budget-design.md` §5, osservabili così:
(1) cambiare in massa la categoria di movimenti con una categoria dell'altro tipo (una di spesa su un'entrata o
il contrario) non scrive nulla per quei movimenti e lo dice con l'esito che la funzione già restituisce; il
database resta senza vincolo (ratificato il 24 ott); (2) esiste un file di test dedicato a
`RecurringExpenseRepository`, che copre almeno ciò che si può provare senza database (costruzione delle
scritture, concorrenza, conversioni), sul modello di `Eton.Tests/ExpenseRepositoryTests.cs`; (3) la chiave
«senza accenti e maiuscole» sta in un posto solo e i punti che oggi la ricopiano la chiamano, con
comportamento invariato (ordinamenti e ricerca identici, provati da test); (4) i claims JWT ripetuti negli
script `supabase/verifica-rls*.sql` stanno in un posto solo **se** il modo in cui gli script si lanciano lo
consente; altrimenti nessuna modifica e il motivo nel resoconto.

PERIMETRO: `Services/ExpenseRepository.cs` (solo `CambiaCategoriaAsync`), `Services/CalcoliRicorrenti.cs`,
`Services/CalcoliGriglia.cs`, `Services/TabellaSpese.cs`, `Services/SchemaCampi.cs`, un file nuovo per la
chiave se serve, `Eton.Tests/` (test nuovi e adeguati), `supabase/verifica-rls*.sql`, `handoff/03-residui/` |
NON TOCCARE: `supabase/migrations/`, i file del budget della unità 2 (elenco in `handoff/02-budget/resoconto.md`,
`TOCCATI`), `Pages/`, `Shared/`, `docs/`, `handoff/PIANO.md`

CAPO: coordinatore-Eton

CONTRATTI (dallo scout, 24 ott; nessuna firma pubblica cambia):
  `Services/ExpenseRepository.cs:355` `public async Task<EsitoMassa> CambiaCategoriaAsync(IReadOnlyCollection<Guid> ids, string categoria)` — consumata da `Pages/SpeseTabella.razor:676`
  `Services/CategorieSpesa.cs:31` `public static bool Conosciuta(string? categoria, string tipo)`
  `Models/Expense.cs:66` `public static class TipoMovimento`
  copie della chiave: `Services/CalcoliGriglia.cs:33-34`, `Services/CalcoliRicorrenti.cs:269`, e la copia di
  ricerca `Services/TabellaSpese.cs:242` (entra solo se la semantica è identica: `Trim` compreso o escluso
  senza cambiare risultati — da ratificare); `Services/SchemaCampi.cs:142-143` non è una copia (minuscole, per
  le chiavi di campo) e resta. Definizione di base: `Services/SchemaCampi.cs:177` `internal static string RiduciAccenti(string testo)`.

STATO: unità 1 (migrazione) e 2 (budget) — resoconti in `handoff/01-migrazione/` e `handoff/02-budget/`. Lavori
da `main` con la 2 già fusa.

AUTORE DEL DIFF: Codex 0.161.0, nella forma di `C:\Users\Andre!\.claude\TODO-synapse-codex.md`, «Regola per il
mandato». Leggila prima del primo `codex exec`, insieme a `C:\Users\Andre!\.claude\protocollo.md` per intero.
⚠️ Lezioni della 2.3: subito dopo la clausola verbatim «il file `-o`» aggiungi: «(è il tuo messaggio
finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`)». Nel brief vieta a Codex
`Set-Content`, `Out-File` e `[System.IO.File]::Write*`: le modifiche ai file solo con apply_patch; cercale nel log.

RESA: nessuna modifica visiva.

PROVA NEL BROWSER: solo se la richiede la tabella del §7; se sì, niente dati in produzione. Server avviato e
fermato da te dal tuo worktree, porta e PID in `handoff/03-residui/server.md`. Browser con
`deviceId d3148d48-d283-4d4a-a07a-95a77fa72150`. Gli script RLS, se li cambi, si provano sul Supabase locale
(Docker acceso solo per la prova, poi `docker desktop stop`).

GATE: `mcp__synapse__build root=<worktree>` → 0 errori; `mcp__synapse__test root=<worktree>` → 0 failed.
Nel resoconto **una riga `GATE:` per comando**, con l'esito copiato dall'output. | BUDGET: 6 M tok_eq

RESOCONTO IN: `handoff/03-residui/resoconto.md` nel tuo worktree

PARTI COMUNI: prima di cominciare, e di nuovo dopo ogni compaction, leggi per intero con Read
C:\Users\Andre!\.claude\architettura-sessioni\cartella-unita.md. La sua sezione «Le parti comuni del
mandato» — la domanda al capo, il lavoro nuovo, la forma del resoconto — vale come scritta in
questo mandato, e non è facoltativa.
