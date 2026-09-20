# Esito del collaudo nel browser — 20 settembre 2026

```
ESITO: difetti
SCENARI: 10/10 eseguiti (più 5 prove dichiarate non eseguibili)
```

*Il rapporto l'ha prodotto `live-testing`; **il file l'ha scritto il capo**, perché quell'agente è
di sola lettura e non ha i tool di scrittura. Il suo testo è ricopiato; l'`ADJUDICA` in fondo è
mia.*

Server sul commit `1a534fe`, tutte e sei le unità integrate.

---

## LE PROVE VERDI

| Voce | Misura letta dal DOM |
|---|---|
| **9 + 5** — allineamento del «?» **a due larghezze** | sotto 640px: scarto **0,013 px**; sopra (1028px, titolo a 52px, ancora a 56,16): scarto **0,0019 px**. Non regressione: testata a titolo corto = **48 px** esatti |
| **6** — il pulsante d'aiuto esiste dal primo render | `.aiuto-apri` non nullo **subito**, su Home, editor di collezione ed editor di elemento. Cliccato appena dopo la navigazione: **il pannello si apre con «Caricamento…» ancora visibile sotto** |
| **montaggio unico** | navigazione fresca sulla Home: **4 chiamate REST, ciascuna una volta sola**. Non regressione: cambio di mese sulle spese → il nodo di pagina **non** viene ricreato |
| **2a** — il cursore del voto ha un nome | `id` del cursore = `for` dell'etichetta, `match: true` |
| **7** — il segnaposto dell'assenza di voto | «—», **36 px**, `rgb(138,138,138)`, `textShadow: none` |
| **8** — il conflitto non butta via il testo | comparso il messaggio nuovo, pulsante premibile, e **la textarea conserva il testo appena digitato** |
| **26 / 31** — la frase non è più doppia | «Il nome dell'elemento non può essere vuoto.» compare **1 volta** (prima 2); la scheda di conflitto **resta aperta** |
| **17** — al logout non resta niente | prima: `["eton.profilo","eton.session","eton.spazio"]` · dopo: **`[]`** |
| **2b** — i nomi accessibili **per riga** | sei `aria-label` tutti diversi e popolati; due opzioni con nomi diversi. **Il caso che contava**: premuto «Aggiungi campo» due volte senza etichetta → «Tipo del campo **campo_7**» e «**campo_8**», nomi diversi |
| **4** — le tre altezze | `.icona-input` = **48 px**; tutte e **24** le pastiglie del selettore = **48 px** |
| **24** — il primo campo | gap fra campi consecutivi: **[12, 12, 12, 12, 12, 12]** — nessuno 0, nessuno 24 |
| **32** — i collegamenti «Tutte» | **[48, 48]** |
| **23a** — la protezione iOS | `.data-spesa` a **16 px**: la protezione è attiva |
| **25** — il registro vuoto | i due pulsanti coesistono: testata 134 px (`compatto`), centro 182 px, **entrambi alti 48**. Screenshot acquisito |

---

## I DUE DIFETTI

### 1. `.data-spesa` è alto **71 px**, non 48 — e a sbagliare è **la misura attesa**, non il codice

Misurato su `/expenses` e su una spesa esistente. **Due resoconti** — quelli delle unità 04 e 05 —
dichiaravano quel campo invariato a 48 px.

**La causa, individuata dall'agente:** il campo sta dentro un **CSS Grid a due colonne**, affiancato
al campo dell'importo, che ha corpo 36 px e altezza naturale ~71. Lo *stretch* implicito della riga
porta il campo della data alla stessa altezza, **indipendentemente dal proprio corpo**.

**ADJUDICA — non è un difetto del codice, è un difetto della misura attesa.** Il calcolo dell'unità
05 era `16 × 1,25 + 26 = 46`, sotto il minimo, «quindi governa `min-height` e il campo resta a 48».
Il calcolo è **giusto in isolamento e irrilevante nel contesto**: dentro un grid l'altezza la decide
la riga, non il contenuto. La protezione che la voce 23a doveva introdurre **funziona** — il corpo è
16 px, misurato — e 71 px è ben sopra il pavimento di tocco.

⚠️ **Perché è successo, e vale più del caso.** Le due unità hanno scritto una misura attesa che
**non potevano verificare**: il loro mandato vietava di avviare il server, e il server è l'unico
posto dove il grid esiste. Una misura attesa calcolata invece che osservata è un'ipotesi travestita
da criterio — e questa è la prima volta in due cicli che il collaudo ne smentisce una.

**Niente da correggere.** Va corretta la misura nei due resoconti, non il codice.

### 2. Voce 27 — i due pulsanti di conferma **non stanno sulla stessa riga**, e l'ordine è invertito

Misurato su `/collections/{id}/edit`, componente di conferma **in pagina** (confermato: non è un
dialogo nativo).

    «Sì, elimina»  top = 291,6
    «Annulla»      top = 347,6      → differenza 56 px, righe diverse

**E il layout è irregolare in un modo che nessuno aveva previsto**: «Sì, elimina» si affianca al
**testo di avviso**, mentre «Annulla» va da solo sulla riga sotto — cioè il pulsante **distruttivo
sta sopra** quello di sicurezza, invertendo l'ordine visivo consueto.

⚠️ **Non è stato misurato a 360px**, ma a **594**: `resize_window` in questo ambiente non scende
sotto ~594 px di larghezza interna, provato fino a 220. **È un limite dell'ambiente, non del sito**
— e il dato resta reale: se già a 594 px i due pulsanti sono su righe diverse, a 360 lo saranno a
maggior ragione.

**ADJUDICA — rilievo fondato, e la clausola 27 è comunque chiusa.** La voce chiedeva di **provare**,
e la prova è stata fatta: l'esito è che i due pulsanti non stanno sulla stessa riga. Il difetto
trovato è **nuovo** e va portato all'utente, non corretto di nascosto — vale la regola che questo
goal si è dato sulle crescite dell'obiettivo. Riportato in `FUORI SCOPE`.

---

## LE CINQUE PROVE NON ESEGUIBILI, DICHIARATE

Il collaudo del ciclo precedente si dichiarò «verde, 6/7 eseguiti» su un elenco che non era più
quello pianificato, e **due prove sparirono senza comparire fra le non eseguite**. Qui ci sono tutte.

1. **Il paragrafo dell'importo in sola lettura.** Richiede un utente **senza permesso di
   intervento**: con l'unico account raggiungibile `.campo-lettura` è sempre `null`. **Non provata,
   non verde** — era previsto dal brief.
2. **La voce 27 a 360 px esatti.** Limite del ridimensionamento in questo ambiente.
3. **Il pavimento di tocco della navigazione a ≥1024 px.** Non eseguito per esaurimento del tempo
   disponibile. ⚠️ **È l'unica prova saltata per mancanza di tempo e non per impossibilità**: va
   rifatta.
4. **La Home nei rami «nessuno spazio» ed «errore».** Sono gli stati che il lavoro dell'unità 01b ha
   reso visibili **per la prima volta**, e nessuno li ha ancora guardati: serve un account senza
   spazi, o un modo sicuro di simulare un errore di rete.
5. **Il salto a un'ancora**, non regressione dell'animazione d'ingresso: nessun collegamento con `#`
   nei dati di prova.

---

## LO STATO IN CUI IL COLLAUDO HA LASCIATO LE COSE

- ⚠️ **Il browser è disconnesso da Eton**, su `/benvenuto`: la prova del logout richiede di premere
  «Esci», e l'agente non aveva credenziali per rientrare. **Chi riprende deve rifare l'accesso a
  mano.**
- L'agente ha **creato e poi eliminato** una collezione di prova nello spazio di collaudo, per non
  toccare dati esistenti.
- Ha **modificato e ripristinato** quattro campi su un elemento della collezione di ricognizione,
  per le prove multi-scheda, **verificando il ripristino con un refresh**.
- Tutte le schede aperte sono state chiuse.

## CONSOLE E RETE

**Nessun errore JavaScript riconducibile all'applicazione**, in tutta la sessione. Tre eccezioni
sono il pattern tipico di un'estensione del browser — probabilmente quella usata per il collaudo
stesso — con riga e colonna a zero e nessuno stack applicativo.

**Nessuna chiamata di rete fallita**: tutte le REST controllate, inclusi i salvataggi in conflitto,
sono tornate `200`.

---

## UNA NOTA METODOLOGICA CHE VALE PER I PROSSIMI COLLAUDI

Il primo tentativo di generare un conflitto è **fallito**, e non per un difetto dell'applicazione:
il data-binding di Blazor su `<textarea>` e `<input>` richiede **sia** l'evento `input` **sia**
`change` per registrare il valore prima del clic. Con il solo `input`, il salvataggio scriveva il
valore precedente.

È un vincolo di **come si simula la digitazione via script**, e chi collauda questo progetto lo
incontrerà di nuovo. Con entrambi gli eventi il comportamento è corretto.

---

## PERCHÉ `ui-critic` NON È STATO LANCIATO

Il §7 del `CLAUDE.md` è esplicito: `ui-critic` si lancia **solo se `live-testing` torna
`ESITO: verde`**. Qui è `difetti`, quindi non si lancia — «su una schermata con difetti funzionali
non si lancia, per la stessa ragione per cui non si collauda una versione che si sta per
correggere».

⚠️ **Dei due difetti però nessuno è funzionale**: il primo è una misura attesa sbagliata e il
secondo è di presentazione. **La decisione se lanciarlo comunque è dell'utente**, e gliela porto
invece di prenderla da me: la regola è scritta sull'esito, non sulla natura dei difetti, e
interpretarla larga sarebbe esattamente il tipo di auto-esenzione che questo impianto vieta
altrove.
