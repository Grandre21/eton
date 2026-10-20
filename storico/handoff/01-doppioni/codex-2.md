FATTO
Aggiunto il commento di tre righe sopra LeggiABlocchiAsync; ordinati gli using Models/Services; spostati in fondo alla classe i due test della lettura a blocchi con i loro commenti e il separatore richiesto. Corpi dei test e comportamento invariati.

DEVIAZIONI
Nessuna modifica funzionale. Comandi esatti eseguiti dalla radice del worktree:

```powershell
Get-Content Eton.Tests/ExpenseRepositoryTests.cs; Get-Content Services/ExpenseRepository.cs -TotalCount 52
```

Script PowerShell eseguito per spostare i test e ordinare gli using:

```powershell
$percorso = Join-Path (Get-Location) 'Eton.Tests/ExpenseRepositoryTests.cs'
$testo = [System.IO.File]::ReadAllText($percorso)
$testo = $testo.Replace("using Eton.Services;`nusing Eton.Models;", "using Eton.Models;`nusing Eton.Services;").Replace("using Eton.Services;`r`nusing Eton.Models;", "using Eton.Models;`r`nusing Eton.Services;")
$inizio = $testo.IndexOf('    // Un inserimento fra due blocchi')
$fine = $testo.IndexOf('    [Fact]', $testo.IndexOf('public void Una_data_qualunque'))
$fine = $testo.LastIndexOf('    [Fact]', $testo.IndexOf('public void Una_data_qualunque'))
$blocco = $testo.Substring($inizio, $fine - $inizio)
$testo = $testo.Remove($inizio, $fine - $inizio)
$chiusura = $testo.LastIndexOf('}')
$accapo = if ($testo.Contains("`r`n")) { "`r`n" } else { "`n" }
$testo = $testo.Insert($chiusura, $accapo + '    // Da qui i test della lettura a blocchi (LeggiABlocchiAsync).' + $accapo + $blocco.TrimEnd() + $accapo)
[System.IO.File]::WriteAllText($percorso, $testo, [System.Text.UTF8Encoding]::new($false))
```

```powershell
dotnet build
dotnet test
git diff --check; Get-Content Eton.Tests/ExpenseRepositoryTests.cs -Tail 55
```

Commento del metodo e resoconto scritti con apply_patch. Il primo tentativo di sostituire -o non ha trovato il file; il resoconto è stato quindi creato.

VERIFICA
dotnet build: compilazione completata, 0 avvisi, 0 errori.
Ultimo dotnet test: Superato! - Non superati: 0. Superati: 392. Ignorati: 0. Totale: 392.
git diff --check: nessun errore.

NOTE
Synapse.Server non avviato. Nessuna modifica ad altri file sorgente o a handoff/.
