# Spese in tabella (fase 2.2) — design

Scritto il **22 settembre 2026**, costruito in chat con l'utente sezione per sezione. È il secondo dei
tre lavori in cui è stata decomposta la «gestione spese più completa» — ricorrenti, **vista
tabellare**, analisi — e il primo pezzo di interfaccia del piano di prodotto
(`2026-09-10-modello-prodotto-design.md`, fase 2.2).

Presuppone il design delle spese del 24 agosto (`2026-08-24-spese-design.md`) e quello delle
ricorrenti del 3 settembre (`2026-09-03-spese-ricorrenti-design.md`), di cui eredita il percorso unico
di lettura e la regola dei totali.

**La direzione, verbatim.** 10 settembre: *«uno stile più tabellare e che ti permetta di gestire in
modo molto puntuale le tue spese, non solo un tool da app scema»*. 22 settembre: *«vorrei anche avere
una gestione piu tabellare in stile excel con delle funzioncine, magari solo lato desktop, mentre lato
mobile segno solo le mie spese quando le faccio tieni sempre a mente l'obbiettivo del progetto»*.

---

## 1. Cosa si costruisce, e cosa no

Una pagina `/expenses/table`, solo per schermi larghi, in cui le spese di uno spazio si **trovano**,
si **modificano** e si **esportano** come in un foglio di calcolo. Il telefono resta lo strumento per
**segnare**, e il registro di oggi non cambia.

| Entra nella 2.2 | Va altrove |
|---|---|
| periodo libero, filtri, ordinamento per colonna | l'incrocio categorie × mesi → **2.3** |
| modifica in cella, con navigazione a frecce come in Excel | le formule scritte dall'utente → **fase 4** (la 2.2 lascia il gancio, §2.2) |
| selezione multipla: «Cambia categoria», «Elimina» | la «riga nuova» in fondo alla tabella → rinviata (§5.5) |
| riepilogo sotto le colonne (somma, media, min, max, conteggio) | il companion AI in sola lettura → **fase nuova**, dopo la 3 (§9) |
| raggruppamento a una dimensione con subtotali | la resa visiva definitiva → **2.1-bis**, che ora viene dopo la 2.2 |
| esportazione CSV | |
| il «?» della pagina e un **tutorial guidato** generico (§6) | |

**La resa visiva è provvisoria, e dichiarata.** Decisione 4-bis del piano di prodotto: la 2.2 si
costruisce sui token di oggi, e la 2.1-bis la restilizza. Questo documento fissa **modello e
comportamento**. Il criterio del confine: *ciò che un test xUnit o un lettore di schermo può osservare
sta qui; ciò che cambia solo una variabile o una regola in `app.css` è della 2.1-bis.*

## 2. L'architettura: tre strati

### 2.1 I calcoli puri, generici

Ordinare, filtrare, raggruppare, aggregare (somma, media, minimo, massimo, conteggio), produrre il CSV:
funzioni statiche su `IReadOnlyList<T>`, senza rete e senza stato, accanto a `CalcoliSpese`. Sono la
parte che la 2.3 e la fase 3 riusano senza toccarla.

### 2.2 Il componente `Griglia<T>`, generico

Un componente con `@typeparam T`, guidato da un elenco di **descrittori di colonna**:

```
ColonnaGriglia<T>
  Chiave          string              identificatore stabile, anche nell'URL
  Etichetta       string
  Tipo            text | number | date | select | money
  Valore          Func<T, object?>    come si legge il valore dalla riga
  Aggregazioni    insieme di { Somma, Media, Min, Max, Conteggio }
  Ordinabile      bool
  Modificabile    Func<T, bool>       per riga, non per colonna
  Opzioni         per il tipo select
```

- **I tipi sono quelli di `SchemaCampi.TipiAmmessi` più `money`.** È il ponte verso i template: alla
  fase 3 un `CampoDefinizione` diventa una `ColonnaGriglia<CollectionItem>` con un adattatore breve.
- **`Valore` è una funzione, non un nome di proprietà.** È il gancio della fase 4: una colonna
  calcolata è una colonna il cui `Valore` lo produce l'interprete delle formule invece di una
  proprietà. Nessuna formula entra nella 2.2.
- **Lo stato di modifica delle celle sta fuori dai descrittori**, nel componente: se ci stesse
  dentro, la 2.3 non potrebbe riusarli.
- **Non si generalizza oltre.** Una griglia «piena», che tratti anche `Expense` come un dizionario di
  valori, farebbe perdere il compilatore sulle spese per servire un secondo cliente che esiste solo
  dalla fase 3. Il segnale che questa scelta era troppo stretta: alla fase 3, `CollectionItem` richiede
  più di un adattatore.

### 2.3 La parte specifica delle spese

La pagina `Pages/SpeseTabella.razor` fornisce le colonne, il salvataggio di una cella, le azioni su
più righe e la lettura.

> **Divieto, ereditato dal design delle ricorrenti §5 e da ricopiare in ogni brief.** La tabella è il
> **terzo** chiamante della lettura delle spese, e legge **solo** dal percorso unico che fonde righe
> vere e occorrenze previste. Una chiamata diretta a `ExpenseRepository.ElencaAsync` fa divergere i
> totali fra registro, Home e tabella, e nessun test lo intercetta.

### 2.4 La navigazione

In testa alle pagine delle spese una sotto-navigazione **«Registro · Tabella · Ricorrenti»**. È la
decisione che il design delle ricorrenti §6.4 rimandava «a quando arriva la tabellare»: **la costruisce
la 2.1** (task 5), con la voce «Tabella» già presente. Finché la 2.2 non esiste, quella voce non si
mostra.

## 3. Solo su schermo largo

Sotto il breakpoint già in uso, **`40rem`**, la tabella non si rende. Al suo posto: *«La tabella è
pensata per uno schermo largo. Per segnare una spesa usa il registro.»* con il collegamento. La rotta
esiste sempre, quindi un link salvato non si rompe. Il comportamento è solo CSS (`display`), senza
`matchMedia`.

Questo chiude le tre voci della zona grigia annotate nel piano:

| Voce | Decisione |
|---|---|
| `<table>` contro griglia di `<div>` | **`<table>` semantica** — `thead`, `tbody` per gruppo, `tfoot` per il riepilogo, `th scope`, `aria-sort`. La ragione per i `<div>` era il collasso a schede sul telefono, che cade |
| strategia sotto ~600px | **nessuna**: la tabella non si mostra |
| densità delle righe | il pavimento di 48px resta su `pointer: coarse`; su `pointer: fine` la riga scende a **36-40px**. Il valore esatto è della 2.1-bis |

## 4. Cosa si vede e cosa si fa

### 4.1 Le colonne

**Data · Descrizione · Categoria · Importo · Pagato da · Stato.**

- «Pagato da» solo negli spazi condivisi, come nel registro.
- «Stato» vale **registrata**, **prevista** (ricorrente scaduta e non ancora scritta) o **in arrivo**
  (ricorrente futura). Le spese nate da una ricorrente portano un segno che lo dice.

### 4.2 Periodo, filtri, ordinamento, raggruppamento

- **Periodo**: questo mese · mese scorso · ultimi 3 mesi · quest'anno · dal … al …. All'apertura,
  **questo mese**. Se il periodo arriva nel futuro, le ricorrenti in arrivo compaiono marcate.
- **Filtri**: categoria (più d'una) · pagante · testo nella descrizione · stato.
- **Ordinamento**: clic sull'intestazione, una colonna per volta, crescente e poi decrescente. Di
  default, data decrescente.
- **Raggruppamento**: nessuno · categoria · pagante · mese. Ogni gruppo ha un'intestazione con il
  subtotale, e si chiude e si apre.
- **Periodo, filtri, ordinamento e raggruppamento stanno nell'URL.** Ricaricare, tornare indietro o
  salvare il link ritrova la stessa vista. I gruppi chiusi no: non sono una vista, sono un gesto.

### 4.3 Il riepilogo in fondo

Nel `tfoot`, sotto l'importo: **somma · media · minimo · massimo · conteggio**. Si calcola sulle righe
filtrate; se c'è una selezione, **sulla selezione**, e l'etichetta dice quale dei due sta mostrando.

**Quali righe entrano**, per la regola del design delle ricorrenti §5: le **previste** sì, le **in
arrivo** no. Accanto alla somma, «di cui N previste».

### 4.4 L'esportazione

CSV delle righe filtrate, o della sola selezione se ce n'è una, con le colonne visibili più lo stato.
**Nel formato che l'Excel italiano apre direttamente**: `;` come separatore, virgola decimale, UTF-8
con BOM, e i campi contenenti `;`, `"` o un a capo racchiusi fra virgolette con le virgolette
raddoppiate.

### 4.5 Quanto si legge

Nessuna paginazione: si legge l'intero periodo scelto. Un anno sono alcune centinaia di righe. **Soglia
di rivalutazione: 2.000 righe per periodo.** È una soglia scritta, non un limite imposto.

## 5. La modifica

### 5.1 Cosa si modifica, e chi

Si modificano **Data, Descrizione, Categoria e Importo**, cioè le quattro colonne che i privilegi
concedono in UPDATE. Una riga è modificabile solo se chi guarda l'ha pagata o possiede lo spazio: è la
regola di `Permessi.PuoIntervenire`, la stessa di `SpesaEdit`. Le altre righe sono visibilmente in sola
lettura, con il motivo (`Permessi.Spiegazione`).

**Le previste e le in arrivo sono sempre in sola lettura**, escluse dalla selezione. Diventano spese vere
solo quando le scrive il generatore della 2.1; nessuna azione «segna adesso» nella 2.2.

### 5.2 La tastiera, come in Excel

Decisione dell'utente, **contro** la raccomandazione di `tech-advisor`, che rinviava le frecce per il loro
costo. Il costo è accettato e va messo in conto nel piano.

| Stato | Tasto | Effetto |
|---|---|---|
| fuori modifica | frecce | spostano la cella attiva |
| fuori modifica | Invio, F2 | entrano in modifica |
| fuori modifica | un carattere | entra in modifica **sostituendo** il contenuto |
| fuori modifica | Spazio | seleziona o deseleziona la riga |
| in modifica | Invio | salva e scende di una riga |
| in modifica | Tab / Shift+Tab | salva e va a destra / a sinistra |
| in modifica | Esc | annulla e ripristina il valore |
| in modifica | clic fuori | salva |

Il meccanismo è un **roving tabindex** (una sola cella della tabella è nel giro del Tab), più un piccolo
modulo JavaScript che sposta il fuoco. È l'unico JavaScript della pagina, e sta nel componente
generico: vale per ogni tabella futura. **Rischio dichiarato**: il fuoco si perde quando Blazor
ridisegna la tabella dopo un salvataggio o un filtro. Il componente deve ricordare la cella attiva per
chiave di riga e chiave di colonna, non per posizione.

La categoria in modifica è un menù sull'elenco fisso di oggi. La data usa lo stesso campo e la stessa
guardia del registro: anno fra il 2000 e l'anno prossimo.

### 5.3 Il salvataggio di una cella

Una cella salva **la riga intera** con il suo `version`, attraverso lo stesso `SalvaAsync` di
`SpesaEdit`: i tre campi non toccati vengono dal modello in memoria. Il filtro sulla versione protegge
così anche loro. La riga mostra «salvo…» finché non torna l'esito:

| Esito | Cosa succede |
|---|---|
| salvata | la riga si riallinea al valore restituito |
| conflitto | sotto la riga si apre **`SchedaConflitto`**, lo stesso componente dell'editor, in una riga `colspan` |
| rifiutata | la cella torna al valore precedente, con la spiegazione dei permessi |
| sparita | la riga esce, con un avviso |

Un valore non valido non parte: la cella resta segnata con il messaggio, e i messaggi sono quelli del
modulo di oggi (`Testi.MessaggioImporto` e gli altri).

### 5.4 Le azioni su più righe

- **Selezione**: casella di spunta per riga, casella nell'intestazione per «tutte le righe filtrate»,
  Shift+clic per un intervallo.
- **Azioni**: «Cambia categoria» ed «Elimina». Elimina chiede conferma con `ConfermaAzione`, il
  componente in pagina: **mai** un dialogo nativo, che blocca anche il collaudo automatico.
- **Una sola istruzione per azione**, filtrata sugli id selezionati. La RLS esclude da sola le righe su
  cui non si hanno i permessi, e l'esito si legge dalle righe restituite: *«12 su 15 modificate: 3 non
  erano tue»*.
- **Non atomiche rispetto alle modifiche altrui, e dichiarato**: nessun filtro `version`, quindi vince
  l'ultima scrittura. È accettabile perché la categoria sceglie da un elenco chiuso. **Se un giorno si
  vorranno cambiare gli importi in massa, questa scelta va rivista.**
- ⚠️ **Da verificare prima del piano** con `doc-checker`: la forma del filtro su un elenco di id
  (`Operator.In`) in `Supabase.Postgrest` 4.4.0, e che la risposta riporti le righe effettivamente
  toccate. È detta a memoria.

### 5.5 La riga nuova, rinviata

Nessuna «riga nuova» in fondo alla tabella: per segnare si usa il registro, che è il flusso del
telefono. Il componente lo permette, quindi aggiungerla dopo costa poco.

## 6. L'aiuto e il tutorial guidato

Chiesto dall'utente il 22 settembre e collocato **nella tabella**.

### 6.1 Il «?»

La pagina usa `TestataPagina` come tutte le altre, con il suo pannello d'aiuto. Deve dire almeno: cosa
vogliono dire i tre stati, perché le previste contano nel totale e le in arrivo no, perché alcune righe
non si modificano, e i tasti.

### 6.2 Il tutorial

Un componente **generico** `Tutorial`, riusabile dalla 2.1-bis su ogni pagina. Il pulsante «Tutorial»
sta nella testata, accanto al «?», nello slot `Azione` che `TestataPagina` espone già.

- **Un tutorial è un elenco di passi**; ogni passo ha un'ancora (l'elemento da mostrare), un titolo e un
  testo breve.
- **Si evidenzia l'elemento, non si oscura la pagina**: lo si porta in vista, gli si dà un contorno
  marcato, e accanto compare un riquadro con il testo e i comandi «Indietro · Avanti · Chiudi».
- **È un dialogo accessibile**: `role="dialog"`, fuoco dentro il riquadro, Esc chiude, e alla chiusura
  il fuoco torna al pulsante.
- **Non parte da solo.** Si apre solo col pulsante. Chi lo ha completato lo trova ancora lì, e il
  browser ricorda che è stato visto **solo per non riproporre un invito**: se il ricordo manca, non si
  rompe niente.
- **Un'ancora assente non blocca**: se un passo punta a un elemento che non c'è — la tabella senza
  righe, un gruppo chiuso — il passo si mostra centrato e dice cosa si vedrebbe.
- **I passi della tabella**: il periodo, i filtri, l'ordinamento cliccando le intestazioni, una cella
  modificabile e i tasti, una riga in sola lettura e perché, la selezione e le azioni, il riepilogo in
  fondo e cosa conta, l'esportazione.

## 7. Test

Nella tradizione del progetto: si testa la **logica pura**.

- ordinamento per tipo (testo con accenti, numero, data, importo), stabile a parità;
- filtri combinati, compreso il testo senza distinzione di maiuscole;
- raggruppamento e subtotali, gruppi vuoti esclusi;
- le cinque aggregazioni, compresi zero righe (nessuna media, nessun minimo) e una riga;
- il riepilogo che passa dalle righe filtrate alla selezione;
- previste dentro e in arrivo fuori dai totali, e il conteggio «di cui N previste»;
- il CSV: separatore, virgola decimale, BOM, campi con `;`, `"` e a capo;
- `Modificabile` per riga: pagante, proprietario, altro membro, prevista;
- il testo dell'esito di massa per ogni combinazione di toccate e non toccate;
- la lettura e la scrittura dei parametri dell'URL, e un parametro illeggibile che torna al default
  invece di rompere la pagina.

Nel browser, con `live-testing`: le frecce e i tasti della §5.2, un conflitto provocato con due schede,
un'azione di massa su righe miste, l'avviso sotto i `40rem`, il tutorial fino in fondo e con un'ancora
assente.

## 8. Come questo design può sbagliare

- **Se la tabella legge senza passare dal percorso unico**, i totali divergono in silenzio (§2.3).
- **Se il fuoco non sopravvive ai ridisegni**, la navigazione a frecce diventa inservibile dopo il primo
  salvataggio, ed è la funzione che l'utente ha voluto contro il parere tecnico.
- **Se `Operator.In` non si comporta come atteso**, le azioni di massa vanno ripensate prima del piano.
- **Se la 2.1-bis cambia il modello d'interazione delle righe**, e non solo i token, la tabella si
  riscrive invece di restilizzarsi: è il segnale che la decisione 4-bis era sbagliata.
- **Se la fase 3 richiede più di un adattatore** per far leggere a `Griglia<T>` gli elementi di una
  collezione, il generico «ristretto» era troppo stretto.

## 9. Fuori scope, e dove va

| Cosa | Dove | Nota |
|---|---|---|
| incrocio categorie × mesi, fisso/variabile, grafici | **2.3** | riusa i calcoli puri e il filtro periodo della 2.2 |
| formule scritte dall'utente | **fase 4** | si attaccano a `ColonnaGriglia<T>.Valore` |
| **companion AI in sola lettura** | **fase nuova, dopo la 3** | v. sotto |
| riga nuova in tabella | aggiunta futura | §5.5 |
| cambio d'importo in massa | aggiunta futura | richiede di rivedere la §5.4 |
| tutorial sulle altre pagine | **2.1-bis** | il componente della §6.2 è già generico |

**Il companion AI**, chiesto dall'utente il 22 settembre: *«connettere una propria ai a eton in generale
che possa leggere i propri dati soltanto, cosi da avere il tuo companion sempre pronto»*. È l'«API
pubblica» che il piano di prodotto teneva in attesa di una richiesta: la richiesta è arrivata. Senza un
server applicativo, l'unica forma che regge è il **server OAuth 2.1 di Supabase**, con la sola lettura
imposta **nel database** (le policy di scrittura rifiutano i token emessi a un client esterno), più un
server MCP senza logica di autorizzazione propria. **Oggi non è costruibile**: il server OAuth è in beta,
e la issue `supabase/auth#2820` (aperta il 20 settembre 2026) mostra che i connettori MCP reali non
completano il collegamento. Resta da decidere, nella sua fase, cosa succede negli spazi condivisi,
dove l'AI di un membro leggerebbe i dati degli altri.
