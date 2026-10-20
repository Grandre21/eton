FATTO
1. Pages/Ricorrenti.razor:115: campo oggi condiviso da gruppi, quota e Prossima; assegnato in testa a Leggi (riga 130). Commento aggiornato a riga 148.
2. Pages/Ricorrenti.razor:82: foreach e intero corpo rientrati di quattro spazi, markup invariato.
3. Pages/Ricorrenti.razor:47: commento Razor richiesto e condizione quota > 0.
4. Services/CalcoliRicorrenti.cs:208: Raggruppa con corpo a blocco. Arrotonda aggiunto a riga 227 e usato anche da QuotaDaAccantonare (riga 223).

DEVIAZIONI
Comandi di build e suite eseguiti, in ordine:
dotnet build Eton.sln
dotnet test Eton.sln
dotnet build Eton.sln -p:UseSharedCompilation=false
dotnet test Eton.sln -p:UseSharedCompilation=false

Nessuno script eseguito. Altri comandi di lettura/verifica:
Get-Content Pages/Ricorrenti.razor -Encoding utf8; Get-Content Services/CalcoliRicorrenti.cs -Encoding utf8
git diff --check -- Pages/Ricorrenti.razor Services/CalcoliRicorrenti.cs; rg -n 'quota resta|foreach \(var r|private DateTime oggi|oggi = DateTime.Today|stesso giorno|Raggruppa\(|Arrotonda' Pages/Ricorrenti.razor Services/CalcoliRicorrenti.cs

UseSharedCompilation=false usato per superare CS2012: accesso negato alla scrittura di obj/Debug/net10.0/Eton.dll con il compilatore condiviso.

VERIFICA
- dotnet test Eton.sln -p:UseSharedCompilation=false: 398 superati, 0 falliti, 0 ignorati.
- dotnet build Eton.sln e dotnet test Eton.sln: falliti con CS2012 per accesso negato a Eton.dll.
- dotnet build Eton.sln -p:UseSharedCompilation=false: applicazione compilata; soluzione fallita con MSB3491 nel target Microsoft.CodeCoverage.targets, accesso negato al temporaneo per .msCoverageSourceRootsMapping_Eton.Tests. Build interamente verde non ottenuta nell'ambiente corrente.
- git diff --check sui due file: superato.

NOTE
Modificati soltanto i due file richiesti e questo resoconto. Firme pubbliche, nomi dei tipi e test invariati. Synapse.Server non avviato. Nessun file handoff modificato. Gate finale da rilanciare nell'ambiente del committente.
