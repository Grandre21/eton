# Correzioni dopo la revisione — unità 04, stesso worktree, stessi file

Applica esattamente queste correzioni al diff che hai scritto. Nessun file nuovo, nessun file fuori da
`Services/CalcoliRicorrenti.cs`, `Pages/Spese.razor`, `Pages/RicorrenteEdit.razor`, `Eton.Tests/PianiARateTests.cs`.

## 1. Descrizione troppo lunga per il suffisso della rata (bug)
`expenses.description` ha un check di 200 caratteri; il suffisso `" · rata k/N"` aggiunge fino a 13 caratteri
(N ≤ 60), quindi un piano con descrizione oltre 187 caratteri non si materializzerebbe mai.
- In `Services/CalcoliRicorrenti.cs`, accanto a `ImportoRata`:
  `/// <summary>La descrizione più lunga di un piano: 200 (check di expenses.description) meno il suffisso più lungo, « · rata 60/60».</summary>`
  `public const int DescrizioneMassimaPiano = 187;`
  e `/// <summary>Un piano si accetta se ogni rata vale almeno un centesimo.</summary>`
  `public static bool RataValida(decimal totale, int rate) => ImportoRata(totale, rate) >= 0.01m;`
- Tutti e cinque i punti che oggi scrivono `ImportoRata(...) < 0.01m` o `>= 0.01m` (Spese.razor: markup, `ModuloValido`, `Segna`;
  RicorrenteEdit.razor: markup, `Valido`/`DatiDaSalvare`) usano `RataValida` (negato dove serve).
- `Pages/Spese.razor`: l'input della descrizione ha `maxlength="@(aRate ? CalcoliRicorrenti.DescrizioneMassimaPiano : 200)"`;
  `ModuloValido` e la guardia in `Segna` aggiungono `(!aRate || nuovaDescrizione.Trim().Length <= CalcoliRicorrenti.DescrizioneMassimaPiano)`;
  sotto il campo descrizione, se `aRate` e la descrizione la supera:
  `<p class="errore-campo">Per un piano la descrizione sta in @CalcoliRicorrenti.DescrizioneMassimaPiano caratteri: il resto serve a «rata k/N».</p>`.
- `Pages/RicorrenteEdit.razor`: stesso `maxlength` condizionale su `Piano`, stessa riga d'errore se `Piano` e la descrizione la supera,
  e la stessa condizione dentro `DatiDaSalvare` (punto 3) per tutti i piani, bloccati compresi.
- `Eton.Tests/PianiARateTests.cs`: un test `Suffisso_entra_nei_200_caratteri`: una descrizione di `DescrizioneMassimaPiano` caratteri,
  piano a 60 rate, l'occorrenza della rata 60 ha `Description.Length <= 200`.
  TEST DI CARATTERIZZAZIONE: Suffisso_entra_nei_200_caratteri (fissa la costante, non ha una riga test rosso).

## 2. Sezione «Fermare» di RicorrenteEdit.razor: un solo blocco Termina, rientri corretti
Sostituisci il contenuto della `<section class="scheda">` fino al commento `@* MaterializedThrough nullo …` con questa forma
(contenuto dei blocchi rientrato di un livello come il resto del file):
```razor
            @if (!Piano || Bloccato)
            {
                var r = conflitto ?? regola!;
                var attiva = Piano
                    ? CalcoliRicorrenti.Prossima(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth, DateTime.Today) is not null
                    : regola!.EndsOn is null;
                <h2>Fermare questa spesa ricorrente</h2>
                @if (attiva)
                {
                    <p class="testo-tenue">Terminandola, da domani non segna più spese. Le spese già segnate restano.</p>
                    <div class="azioni">
                        <ConfermaAzione Chiave="@Id" Etichetta="Termina" EtichettaConferma="Sì, termina" Occupato="@(occupato || conflitto is not null)" OnConferma="Termina" />
                    </div>
                }
                else if (Piano)
                {
                    <p class="testo-tenue">Concluso il @Testi.DataSola(regola!.EndsOn!.Value): non segna più rate. Le spese già segnate restano.</p>
                }
                else
                {
                    <p class="testo-tenue">Terminata il @Testi.DataSola(regola!.EndsOn!.Value): dopo quella data non segna più spese. Le spese già segnate restano.</p>
                }
            }
```
Rientra di un livello anche il contenuto del blocco `@if (!Piano)` attorno a «Ogni quanto» e «Giorno del mese».
Il resto della sezione (Elimina, commenti) invariato.

## 3. `Valido` e `DatiDaSalvare`: una sola sede
In `Pages/RicorrenteEdit.razor` sostituisci `Valido` con `private bool Valido => DatiDaSalvare() is not null;` e scrivi
`DatiDaSalvare` così (aggiungendo la condizione del punto 1):
```csharp
    // L'unica sede delle regole di validità: Valido ne è la proiezione, Salva e Sovrascrivi la usano per i dati.
    private DatiRegola? DatiDaSalvare()
    {
        if (string.IsNullOrWhiteSpace(descrizione)) return null;
        if (Piano && descrizione.Trim().Length > CalcoliRicorrenti.DescrizioneMassimaPiano) return null;
        if (Bloccato)
        {
            // Senza Rate: il repository non tocca ends_on, e un piano terminato in anticipo resta terminato.
            var r = conflitto ?? regola!;
            return new DatiRegola(r.Amount, descrizione, categoria, r.EveryMonths, r.DayOfMonth, r.StartsOn);
        }
        if (!DataRagionevole(inizio)) return null;
        if (!Piano)
            return Denaro.Prova(importoTesto, out var i) ? new DatiRegola(i, descrizione, categoria, ogniMesi, giorno, inizio!.Value) : null;
        return Denaro.Prova(totaleTesto, out var t) && CalcoliRicorrenti.RataValida(t, rate)
            ? CalcoliRicorrenti.DatiPiano(t, rate, descrizione, categoria, inizio!.Value) : null;
    }
```
Verifica che `Salva` (ramo `Nuova` che chiama `Crea(importo)`) e `Sovrascrivi` restino corretti con questa forma; se
`Salva` oggi rifà `Denaro.Prova(importoTesto, …)` per `Crea`, lascialo.

## 4. Prova rossa del test della quota (da fare PRIMA dei punti 1-3)
Il test `GruppiRicorrentiTests.Quota_somma_i_totali_di_gruppo_arrotondati` non è mai stato visto fallire sulla propria
asserzione (nel primo lancio non compilava per colpa di un altro file). Fallo così:
1. con apply_patch rimetti temporaneamente in `QuotaDaAccantonare` il corpo di prima:
   `=> Arrotonda(regole.Where(r => Gruppo(r, oggi) is GruppoRicorrenti.Annuali or GruppoRicorrenti.OgniNMesi).Sum(QuotaMensile));`
2. `dotnet test --filter Quota_somma_i_totali_di_gruppo_arrotondati` da `Eton.Tests`: deve fallire sull'asserzione (atteso 41.66, ottenuto 41.67);
3. con apply_patch rimetti il corpo nuovo (`Raggruppa(...)...Sum(g => g.TotaleMensile!.Value)`), identico a prima.
Nel `VERIFICA` ricopia la riga: `test rosso  GruppiRicorrentiTests.Quota_somma_i_totali_di_gruppo_arrotondati: <riga di fallimento dall'output>`.

## Verifica
`dotnet build` e `dotnet test` nel worktree. Non avviare Synapse.Server né l'applicazione.

## Formato del ritorno
Il tuo resoconto è il tuo messaggio finale (il file `-o` lo scrive la riga di comando): `FATTO / DEVIAZIONI / VERIFICA / NOTE`, oppure `BLOCKED: <domanda>`.

Niente interpreti con codice inline: vietati `python -`, `python -c`, `node -e` e here-string passate a un interprete; i file si scrivono come file.

Il tuo resoconto va solo nel file `-o`: non scrivere in `handoff/04-rate/resoconto.md`.

Per ogni script o suite che lanci, scrivi in `DEVIAZIONI` il comando esatto.

In più, su Windows: non scrivere file con `Set-Content`, `Out-File`, `[System.IO.File]::Write*` o altri comandi PowerShell; i file si modificano solo con apply_patch.

Non leggere nulla fuori dal worktree, e in particolare: `~/.claude/**`, `~/.codex/auth.json`, `~/.ssh/**`, `%LOCALAPPDATA%\Synapse\**`, file `.env*` e `appsettings*.json` diversi da `appsettings.Development.json`.
