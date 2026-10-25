# Brief per Codex — unità 04, giro 1: tre difetti della pagina dei budget

Lavori nel worktree `G:\Sviluppo\wt-Eton\04-correzioni-budget` (Blazor WebAssembly, .NET). Tre correzioni
piccole, già decise: applicale esattamente come scritto, senza esplorare oltre i file citati.

## File che puoi toccare (solo questi)

- `Pages/Budget.razor`
- `Pages/BudgetEdit.razor`
- `wwwroot/css/app.css` — **una sola regola nuova**, nessuna regola esistente modificata

NON TOCCARE: `Pages/Spese.razor`, `Pages/SpeseTabella.razor`, `Pages/RicorrenteEdit.razor`, `Shared/`,
`Services/`, `Models/`, `supabase/`, `docs/`, `handoff/`, `Eton.Tests/`.

## PIANO-DESIGN

Resa minima, nessun valore nuovo: tutto viene da `app.css`.
- Colori (esistenti, non se ne aggiungono): sfondo `#000000` (`--sfondo`), bordo/binario della barra
  `#242424` (`--bordo`), testo `#f2f2f2` (`--testo`), nome categoria `#a1a1a1` (`--testo-tenue`), cifre e
  riempimento `#b6f36a` (`--secondario`), premibile `#4c8dff` (`--accento`). «Sforato» resta senza colore
  d'allarme (fuori da questo giro).
- Typeface: Inter per i testi (`--prosa`), Plex Mono per le cifre (`.dato`), ruoli invariati.
- Layout: la riga di budget usa le stesse quattro colonne del Registro — nome a sinistra, barra elastica,
  quota e totale allineati a destra — e sotto, a tutta larghezza e allineate a sinistra, le due azioni
  (bersagli da 48 px già dati da `.btn.compatto`). A 390 px: nome|totale, poi barra|quota, poi le azioni.
- Principio: le barre di tutte le categorie partono dalla stessa ascissa e si confrontano a colpo d'occhio,
  come nel Registro; le azioni non rubano una colonna alle cifre.

## 1 — La barra del budget ha larghezza 0 a schermo largo

Causa: a ≥ media query larga, `wwwroot/css/app.css:2379-2386` fa di `ul.barre-categorie` una griglia a 4
colonne (`minmax(4.5rem, auto) 1fr auto auto`) e mette `li { display: contents }`, così i figli del `<li>`
riempiono le colonne **nell'ordine del markup**. Il Registro (`Pages/Spese.razor:212-217`) ha quattro figli
in ordine nome, barra, quota, totale. La riga del budget (`Pages/Budget.razor:86-93`) ha sei figli in ordine
nome, totale, barra, quota, link, link: la barra finisce nella terza colonna `auto`, che è vuota → 0 px.

Correzione, in `Pages/Budget.razor:86-93`: riordina i figli del `<li>` come nel Registro e raccogli i due
link in un contenitore con la classe esistente `azioni` (`app.css:862`):

```razor
<li>
    <span class="nome-categoria">@categoria</span>
    <span class="barra"><span class="riempimento" style="width: @(s.Quota)%"></span></span>
    <span class="quota dato">@(s.Residuo >= 0 ? "restano" : "sforato di") @Denaro.Testo(Math.Abs(s.Residuo))&#160;€</span>
    <span class="totale-categoria dato">@Denaro.Testo(s.Speso) / @Denaro.Testo(s.Budget.Amount)&#160;€</span>
    <div class="azioni">
        <a class="btn compatto" href="...stesso href di oggi...">Registra una spesa</a>
        <a class="btn compatto" href="...stesso href di oggi...">Modifica</a>
    </div>
</li>
```

Gli `href`, i testi e le espressioni restano identici a oggi; cambia solo l'ordine e il contenitore.

In `wwwroot/css/app.css`, **subito dopo** la regola `.barre-categorie .totale-categoria { ... }` (riga 2098),
aggiungi una sola regola, fuori da ogni media query, con un commento breve nello stile del file (italiano,
che dice il perché):

```css
/* La riga di un budget porta in più le sue azioni (Pages/Budget.razor): vanno su una
   riga propria a tutta larghezza, sia nella griglia stretta del <li> sia in quella larga
   del genitore, così le quattro colonne restano quelle del Registro. */
.barre-categorie > li > .azioni { grid-column: 1 / -1; }
```

Il Registro non ha `.azioni` dentro i `<li>`, quindi resta identico. Non toccare altre regole.

## 2 — «Nuovo budget»: la guardia d'uscita scatta già per la sola categoria

`Pages/BudgetEdit.razor:159-161`, ramo `Nuova` di `Cambiata`, oggi:

```csharp
? !string.IsNullOrWhiteSpace(importoTesto) || categoria != (disponibili.FirstOrDefault() ?? "")
```

Diventa:

```csharp
? !string.IsNullOrWhiteSpace(importoTesto)
```

Aggiorna il commento sopra (riga 158) in una riga che dica anche questo: in creazione conta solo l'importo
scritto, perché una categoria scelta senza importo non è lavoro che si perde. Il ramo `!Nuova` non cambia.
Nient'altro in questo file.

## 3 — Con zero budget mancano mese e frecce ‹ ›

In `Pages/Budget.razor:53-116` oggi il ramo `else if (senzaBudget)` mostra il blocco «Ancora nessun budget
qui» al posto dell'intera `section.scheda.riepilogo-mese`, quindi la testata col mese e le frecce sparisce.

Correzione: togli il ramo `else if (senzaBudget) { ... }` di riga 53-61 e sposta il suo `<div class="vuoto">`
(i quattro elementi interni identici) **dentro** la `section.riepilogo-mese`, come nuovo ramo fra quello di
`rileggoMese` e quello di `stati.Count == 0`:

```razor
@if (rileggoMese)
{
    <Scheletro Righe="2" />
}
else if (senzaBudget)
{
    <div class="vuoto"> ...i quattro elementi di oggi, identici... </div>
}
else if (stati.Count == 0)
...
```

La `div.testa-registro` con mese e frecce resta invariata e ora si vede anche con zero budget. La sezione
«In arrivo» e il blocco `@code` non cambiano.

## Budget di complessità

Nessuna astrazione nuova, nessun tipo nuovo, nessun metodo nuovo, nessun file nuovo, nessun test nuovo.
Una sola regola CSS nuova. Diff atteso sotto le 40 righe.

## Verifica

`dotnet build` nel worktree, poi `dotnet test` nel worktree. Non avviare Synapse.Server e non avviare l'app.
Il GATE lo rifà la sessione con Synapse.

## Divieti di scrittura

I file si creano e si modificano solo con le tue patch. Vietati gli interpreti con codice inline (`python -`, `python -c`, `node -e`, here-string passate a un interprete) e vietato scrivere file da PowerShell: niente comandi `powershell.exe -Command` né script `.ps1` usati per creare o riscrivere file, con `Set-Content`, `Out-File`, `Add-Content`, `[System.IO.File]::Write*` o sostituzioni a regex; leggere, compilare e lanciare i test da PowerShell resta permesso. Se una modifica non si può fare con una patch, fermati e scrivi `BLOCKED:` col motivo; se l'hai fatta in un altro modo, scrivi in `DEVIAZIONI` il comando esatto e i file che ha toccato.

Il tuo resoconto è il tuo messaggio finale: Codex lo salva da solo in `G:\Sviluppo\wt-Eton\04-correzioni-budget\handoff\04-correzioni-budget\codex-1.md` quando finisci. Non scrivere tu quel file, non creare altri file di resoconto e non scrivere in `handoff/04-correzioni-budget/resoconto.md`. (È il tuo messaggio finale: la riga di comando lo salva da sé; non creare un file chiamato `-o`.)

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

## Formato del messaggio finale

```
FATTO: <una riga per correzione 1/2/3>
DEVIAZIONI: <ogni scostamento da questo brief, e ogni comando lanciato> | nessuna
VERIFICA: <esito di dotnet build e dotnet test, righe di riepilogo copiate>
NOTE: <altro> | nessuna
```
oppure `BLOCKED: <motivo>`.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
