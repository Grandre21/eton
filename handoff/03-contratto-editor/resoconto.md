UNITÀ: 03 — ESITO: FATTO

TOCCATI:
- `Shared/PaginaEditor.cs` → +81/−0 (nuovo)
- `Pages/NoteEdit.razor` → +28/−14

## CONTRATTI

La firma reale, membro per membro, citata dal file su disco. **Diverge dal mandato in tre
punti**, tutti marcati `⚠` qui sotto: sono le cose da riportare nei mandati 04, 05 e 06.

```csharp
// Shared/PaginaEditor.cs:29
public abstract class PaginaEditor : ComponentBase, IDisposable        // ⚠ IDisposable in più

// Shared/PaginaEditor.cs:34-35
[Inject] private NavigationManager Navigation { get; set; } = default!;   // ⚠ private, non protected
[Inject] private IJSRuntime JS { get; set; } = default!;                  // ⚠ private, non protected

// Shared/PaginaEditor.cs:41
private bool disarmata;

// Shared/PaginaEditor.cs:43
private bool smontata;

// Shared/PaginaEditor.cs:48
protected abstract bool Cambiata { get; }

// Shared/PaginaEditor.cs:54
protected void Esci(string uri, bool replace = false)

// Shared/PaginaEditor.cs:69
protected async Task GuardaUscita(LocationChangingContext ctx)

// Shared/PaginaEditor.cs:80
public virtual void Dispose() => smontata = true;                      // ⚠ nuovo
```

La riga di markup è **invariata** rispetto al mandato, e va **dentro il ramo del modulo**
(v. `Pages/NoteEdit.razor:39`, prima riga del ramo `else`):

```razor
<NavigationLock ConfirmExternalNavigation="@Cambiata" OnBeforeInternalNavigation="GuardaUscita" />
```

### Cosa cambia per i mandati 04, 05 e 06

1. **`Navigation` è `private`.** Le pagine derivate **non** la vedono più. Tutti e cinque
   i loro `NavigateTo` (`CollectionEdit.razor:532` e `:611`, `ItemEdit.razor:351` e `:431`,
   `SpesaEdit.razor:386`) sono post-`Crea()` o post-`Elimina()`, quindi diventano tutti
   `Esci(...)` e nessuno resta scoperto. La riga `@inject NavigationManager Navigation`
   va **tolta**, come dice già il mandato. Se un'unità scoprisse di aver bisogno di
   navigare per altro, **rimette** `@inject NavigationManager Navigation` nella pagina: con
   la base `private` questo **non** produce l'avviso CS0108, mentre con `protected`
   l'avrebbe prodotto e avrebbe rotto il gate «0 avvisi».
2. **`JS` è `private`.** Nessuno dei tre editor inietta `IJSRuntime` oggi (verificato), e
   se servisse vale la stessa via del punto 1.
3. **La base implementa `IDisposable`.** Nessuno dei tre editor lo implementa oggi
   (verificato), quindi lo ereditano e basta. Se una unità dovesse aggiungere una propria
   pulizia, la forma è `public override void Dispose() { base.Dispose(); … }` — e
   **`base.Dispose()` non è facoltativo**: senza, la guardia del punto 2 dell'ADJUDICA
   smette di funzionare in silenzio.

Verificato con `doc-checker` contro il sorgente di ASP.NET Core al tag `v10.0.10`, cioè la
versione pinnata in `Eton.csproj:36`: `ComponentFactory` cerca le proprietà `[Inject]` con
`BindingFlags.Instance | Public | NonPublic` e `MemberAssignment.GetPropertiesIncludingInherited`
risale la gerarchia dei tipi chiamando `GetProperties(… | DeclaredOnly)` **livello per
livello**. Le proprietà `private` di una classe base vengono quindi trovate e popolate.
`NonPublic` è un flag unico che non distingue `private` da `protected`. Non riverificarlo.

## ADJUDICA

Revisori lanciati: `bug-hunter`, `conformity`, `backend-expert`, `threat-hunter` (JS interop
più il titolo scritto dall'utente reso in `<h1>`).

    istruttoria: 1 rilievo su 1 file → checker no

Sotto entrambe le soglie (somma ≥ 4, oppure ≥ 3 file distinti). `conformity` 0 rilievi,
`threat-hunter` 0 rilievi, quindi la somma è il solo rilievo di `bug-hunter`.

**1. `bug-hunter`, severità alta, concorrenza — FONDATO, corretto.**
`Crea()` ed `Elimina()` restano sospesi su una chiamata di rete, e «Chiudi»
(`Pages/NoteEdit.razor:98`, `<a class="btn" href="notes">Chiudi</a>`) **non** è disabilitato
da `occupato`: l'utente esce mentre la chiamata è in volo, il `Task` prosegue oltre lo
smontaggio, e `NavigateTo` su un `NavigationManager` che è singleton dell'applicazione
dirotta la pagina che l'utente sta guardando in quel momento, facendo scattare la guardia di
*quella* pagina con una domanda fuori contesto.
Aperto io il codice (il rilievo tocca la concorrenza): confermato in ogni anello.
**Il dirottamento preesisteva a questa unità** — prima il codice chiamava `NavigateTo`
grezzo con lo stesso esito — ma la domanda fuori contesto è nuova, e il rimedio appartiene
alla base perché lo stesso schema è nei cinque call-site degli altri tre editor.
Corretto con `smontata` + `IDisposable`: `Shared/PaginaEditor.cs:63` (`if (smontata) return;`)
e `:80` (`public virtual void Dispose() => smontata = true;`).

**2. `backend-expert`, severità media, struttura — FONDATO in parte.**

*Accolta* la prima metà: `Navigation` e `JS` da `protected` a `private`
(`Shared/PaginaEditor.cs:34-35`). Verificato io il claim che la motiva, aprendo i tre file
non di mia proprietà: nei sette call-site di `NavigateTo` dei quattro editor la chiamata
grezza è **sempre** quella da sostituire, quindi esporla accanto al wrapper corretto offriva
a tre unità future la via sbagliata con la stessa comodità di quella giusta.

*Respinta* la seconda metà — spostare `<NavigationLock>` fuori dai rami condizionali.
**Introdurrebbe un difetto.** `SalvaCon` a `Pages/NoteEdit.razor:295-296` fa
`case EsitoSalvataggio.Sparita: sparita = true;` **senza** azzerare `nota`, `titolo` né
`corpo`: nel ramo `sparita` (`Pages/NoteEdit.razor:23`) `Cambiata` è quindi **vera** se
l'utente aveva modificato, e la guardia chiederebbe «hai modifiche non salvate» su una nota
che non esiste più e che non si può salvare — una domanda con una sola risposta possibile.
Il caso che la proposta chiudeva (un `Esci` con il `NavigationLock` smontato) non ha un
percorso concreto: gli unici due chiamanti di `Esci` sono raggiungibili solo da pulsanti che
vivono nel ramo `else`.

**3. `backend-expert`, severità bassa, leggibilità — FONDATO, corretto.**
Il consumo del flag in tre righe con variabile temporanea → `Shared/PaginaEditor.cs:73`,
`if (disarmata) { disarmata = false; return; }`. Equivalente: quando `disarmata` è falsa,
riassegnarla a `false` non fa nulla.

**4. `backend-expert`, severità bassa, leggibilità — FONDATO, accolto in parte.**
Lo stesso «perché» era scritto in cinque punti. Consolidato: il paragrafo «classe base e non
`[Parameter]`» spostato nel `<summary>` di classe (`Shared/PaginaEditor.cs:21-27`), il
`<summary>` di `Cambiata` ridotto a una riga (`:46`), il commento sul campo `disarmata`
lasciato per esteso perché è il punto dove la decisione si prende (`:37-40`). Non accolta la
parte che chiedeva di **togliere** i due commenti ai call-site in `NoteEdit`: sono stati
ridotti a una riga ciascuno (`Pages/NoteEdit.razor:264` e `:343`) invece che eliminati —
chi legge `Esci(...)` dentro `Crea()` deve trovare lì il motivo per cui si esce senza
guardia, e il rimando costa una riga.

**Campione sugli infondati.** Ho riverificato la sola proposta respinta — il
`<NavigationLock>` fuori dai rami — aprendo `Pages/NoteEdit.razor:23` e `:295-296`: la
verifica ha **confermato il rifiuto**, ed è quella che ha prodotto l'argomento del caso
`sparita` scritto sopra.

## FUORI SCOPE

**Il link «Chiudi» non è disabilitato mentre un salvataggio è in volo.**
`Pages/NoteEdit.razor:98` è un `<a href>`, e a differenza del pulsante Salva accanto non
guarda `occupato`. È l'innesco del rilievo 1: la guardia della base impedisce ora il danno
peggiore (la navigazione tardiva viene abbandonata), ma resta il fatto che si può uscire da
un modulo mentre la sua scrittura è in corso, e l'utente non ha modo di sapere se la nota è
stata creata o no finché non guarda l'elenco. Correggerlo bene significa decidere cosa deve
fare quel link durante `occupato` — spegnersi, o chiedere una domanda diversa da quella
sulle modifiche non salvate — e la stessa decisione vale per i quattro editor: è materia da
contratto, non da unità. **Non l'ho toccato**: è una scelta di progetto, e l'omologo esiste
in tutti e quattro i file, tre dei quali non sono miei.

## GATE

- `dotnet build` → **0 errori, 0 avvisi**
- `dotnet test` → **267/267 superati**, 0 non superati, 0 ignorati (126 ms)

Compilato una volta sola, a fine giro, con nessun agente vivo. Gli implementer non hanno
compilato.

## SCOSTAMENTI

Tre, tutti nella sezione `CONTRATTI` con il segno `⚠`, tutti conseguenza di rilievi
adjudicati fondati:

1. **`Navigation` e `JS` sono `private`, non `protected`.** Rilievo 2. Il mandato li dava
   `protected`; la firma reale prevale, e i mandati 04-06 vanno aggiornati.
2. **La classe implementa `IDisposable` e ha `public virtual void Dispose()`.** Rilievo 1.
   Non era nel contratto del mandato.
3. **Un secondo campo privato, `smontata`.** Deroga esplicita al budget «un solo campo
   privato», concessa da me per correggere il rilievo 1. Nessun'altra parte del budget è
   stata toccata: nessuna astrazione oltre a `PaginaEditor`, nessun tipo nuovo, nessun file
   `.js`, nessun servizio iniettato nuovo, nessun pacchetto.

Nient'altro diverge. In particolare **non** divergono: la riga di `<NavigationLock>` e la sua
posizione dentro il ramo del modulo, le firme di `Cambiata`, `Esci` e `GuardaUscita`, e i
tre interventi su `NoteEdit` chiesti dal mandato (testata, esito spostato, segnaposto).

## DA PROVARE NEL BROWSER

Da far verificare a `live-testing` quando le unità 03-06 sono tutte rientrate. Ordinati per
quanto costa scoprirli tardi. I primi tre sono i tre risultati osservabili del mandato.

1. **La domanda compare.** Su `/notes/new` scrivere del testo, premere «Chiudi».
   *Accettazione*: compare un dialogo nativo del browser con il testo «Hai modifiche non
   salvate: se esci adesso le perdi. Vuoi uscire lo stesso?». Annullando si resta
   nell'editor **con il testo intatto**, non svuotato.
2. **Vale anche per il tasto Indietro e per la navigazione laterale.** Stesso stato del
   punto 1, ma uscendo col tasto Indietro del browser, e in una seconda prova con un link
   della barra di navigazione. *Accettazione*: stessa domanda, e annullando si resta
   nell'editor col testo. Il tasto Indietro è il caso che più facilmente si rompe da solo:
   se annullando l'URL cambia comunque, o la pagina resta ma l'indirizzo no, è un difetto.
3. **Nessuna domanda dopo un salvataggio riuscito.** Su una nota esistente: modificare,
   premere «Salva», attendere «Salvata.», premere «Chiudi». *Accettazione*: si esce
   **senza** nessuna domanda.
4. **Nessuna domanda subito dopo aver creato una nota.** Su `/notes/new`: scrivere, premere
   «Salva», attendere che l'indirizzo diventi `/notes/{id}`, premere «Chiudi».
   *Accettazione*: nessuna domanda. È il caso che l'implementazione ingenua sbaglia, ed è il
   motivo per cui `Esci()` esiste.
5. **Nessuna domanda subito dopo aver eliminato.** Su una nota esistente: modificare il
   testo **senza salvare**, poi eliminare la nota. *Accettazione*: si torna a `/notes` senza
   nessuna domanda, benché il modulo fosse sporco.
6. **La nota sparita non chiede niente.** Difficile da innescare ma è il caso su cui ho
   respinto un rilievo, quindi vale una prova se c'è occasione: con la stessa nota aperta in
   due schede, eliminarla nella prima, poi modificare e premere «Salva» nella seconda.
   *Accettazione*: compare «Questa nota non c'è più…», e premendo «Torna alle note»
   **nessuna domanda** — benché il testo modificato sia ancora in memoria.
7. **Chiusura della scheda e ricarica.** Con del testo non salvato, premere F5 e in una
   seconda prova chiudere la scheda. *Accettazione*: compare il dialogo **nativo** del
   browser, con il testo **standard del browser** e non il nostro: è `beforeunload`, il suo
   testo non è personalizzabile. Con la nota salvata, F5 **non** deve chiedere niente.
8. **La testata c'è e si spiega.** `/notes/new` e `/notes/{id}`. *Accettazione*: titolo in
   testa e infobutton «?» che apre il pannello con i tre paragrafi; il pannello si chiude con
   Esc e col tocco fuori. Nota attesa e non un difetto: il titolo dell'`<h1>` **cambia
   mentre si digita** nel campo Titolo, ed è «Nota» finché il campo è vuoto — è la stessa
   espressione che alimenta il titolo della scheda del browser, riusata come chiede il
   mandato.
9. **L'esito dove si guarda.** Su una nota lunga (riempire il corpo fino a scorrere):
   premere «Salva». *Accettazione*: «Salvata.» compare **appena sopra i pulsanti**, in vista
   senza scorrere. Stessa prova per il messaggio d'errore, se si riesce a innescarne uno.
10. **Il segnaposto va a capo.** Aprire `/notes/new` e guardare il campo del corpo vuoto.
    *Accettazione*: il testo di esempio è su più righe e **non** contiene la sequenza
    letterale `&#10;` da nessuna parte.

**Cosa non si può provare nel browser, e perché.** Il rilievo di concorrenza corretto al
punto 1 dell'ADJUDICA richiede una risposta di rete lenta *e* un clic su «Chiudi» mentre è
in volo: non è riproducibile a mano in modo affidabile senza strozzare la rete dagli
strumenti per sviluppatori. Se `live-testing` ha modo di farlo, la prova è: su `/notes/new`
scrivere, premere «Salva», premere subito «Chiudi» e confermare l'uscita. *Accettazione*:
si resta su `/notes` — non si deve essere sbalzati sulla nota appena creata quando la
chiamata ritorna. Se non è praticabile, va detto come limite, non dato per verificato.

**Su iOS in modalità PWA il punto 7 resta best-effort**: `beforeunload` è notoriamente
inaffidabile lì. Non è una promessa del contratto, e un esito negativo su iPhone non è un
difetto di questa unità. Il gesto Indietro, che è il caso frequente, passa dal punto 2 ed è
coperto.
