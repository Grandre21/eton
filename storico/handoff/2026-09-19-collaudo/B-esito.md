# Giro B del collaudo — i comportamenti · **ESITO: verde**, 6/7 eseguiti

Eseguito il **19 settembre 2026** su `http://localhost:5000`, commit `bb77cf7`. Una prova non
eseguibile e dichiarata, non simulata.

## Le prove

| # | Prova | Esito |
|---|---|---|
| 1 | **L'importo sopra il migliaio, ramo modificabile** | **PASSA** — v. sotto |
| 2 | L'importo, ramo in sola lettura | **non provato**: lo spazio ha **un solo membro** e non c'è un secondo account |
| 3 | L'errore di validazione che si spegne digitando | **PASSA** |
| 4 | Il riquadro che si spegne togliendo il campo | **PASSA** |
| 5 | Tavolozza e modelli: la stessa pastiglia | **PASSA** |
| 6 | Il costo zero dell'allineamento del profilo | **PASSA** |
| 7 | La Home al cambio di spazio | **PASSA** |

## 1 — La prova che contava di più

Spesa creata da **1284,50**, riaperta, e premuto «Chiudi» senza toccare nulla.

- campo importo: **`1284,50`**, senza il punto delle migliaia — la grammatica di chi digita;
- elenco: **`1.284,50 €`**, col punto e col simbolo — la grammatica di chi guarda;
- l'editor dichiarava **«Non c'è niente da salvare: non hai ancora cambiato niente.»**
- «Chiudi» → si esce **subito, nessun dialogo**.

Era la prova che il difetto chiuso a settembre non fosse stato riaperto: se `Cambiata` fosse
risultata vera all'apertura, una spesa dal migliaio in su sarebbe tornata non modificabile.
**Non si è riaperto.**

## 3 e 4 — Gli esiti che non sopravvivono più alla correzione

**3:** svuotato il nome di un elemento → riga rossa **subito**; digitato **un** carattere → riga
sparita **immediatamente**, senza premere «Salva», e il pulsante si è riattivato. È il rimedio che
l'unità 03 ha copiato da `SpesaEdit`: l'errore non è più *stato* da azzerare, è un'**espressione
calcolata** che Blazor rivaluta a ogni ridisegno.

**4:** campo senza etichetta → «Salva» → riquadro «Il campo "campo" non ha un'etichetta.»; tolto il
campo → riquadro **sparito**, e lo stato è tornato a «Non c'è niente da salvare».

## 6 — Il costo zero, e il modo in cui è stato misurato

Due ricaricamenti consecutivi: **solo `GET` su `/rest/v1/profiles`, mai un `PATCH`.** La `GET` è la
lettura dei membri e c'era da prima.

⚠️ **La riserva, dichiarata dall'agente:** la sessione era già autenticata, quindi l'unica scrittura
«prima volta» può essere avvenuta prima di questo giro. Sul comportamento richiesto — nessun `PATCH`
ai riavvii successivi — il risultato è netto; sulla prima scrittura no. È coerente col disegno:
l'impronta si ignora **al ritorno da un accesso**, non a ogni avvio.

## 7 — La Home al cambio di spazio, e la decisione che avevo rimandato

Durante la transizione l'area principale mostrava **solo** «Caricamento…»: nessun residuo del vecchio
spazio — né titolo, né collegamenti, né dati. **Il selettore non è mai sparito** e mostrava già il
nome nuovo.

**Decisione presa guardandola, come avevo scritto di voler fare:** la forma va bene e **non si
cambia**. L'alternativa più fine — mostrare subito il nome nuovo tenendo in caricamento solo i
dettagli — richiederebbe rami nuovi nel markup, e il guadagno è un nome che comunque si legge già
nel selettore. Quello che il difetto originale produceva — i dati dello spazio *precedente* sotto il
nome di quello *nuovo* — è sparito, ed era il punto.

## ⚠️ Il claim che non regge, verificato da me aprendo il codice

L'agente ha riportato che «coesistono due implementazioni diverse per lo stesso tipo di guardia,
in-page e nativa», e l'ha chiamato «degno di segnalazione».

**Non regge, e l'ho verificato aprendo `Shared/PaginaEditor.cs:76-86`.** Esiste **una sola** guardia,
e usa **sempre** un `confirm` nativo:

```csharp
if (disarmata) { disarmata = false; return; }
if (!Cambiata) return;
var esci = await JS.InvokeAsync<bool>("confirm", "Hai modifiche non salvate: …");
```

I due casi che l'agente ha confrontato **non sono alla pari**: ha chiuso *senza* modifiche (prove 1,
3, 4 — `Cambiata` falsa, la guardia esce subito, nessun dialogo) e ha chiuso *con* modifiche
(`/collections/new` — dialogo). È lo stesso meccanismo, che in un caso non ha niente da chiedere.

**Resta vero il limite**, che era già noto e registrato: quel `confirm` **blocca il plugin**, quindi
la guardia d'uscita **non è collaudabile da un agente** e la sua prova va girata all'utente.

⚠️ **Un'osservazione che invece il codice letto NON spiega, e che non dichiaro risolta:** l'agente
riporta un dialogo nativo anche premendo «Elimina» su una spesa. Quel percorso passa da
`Shared/ConfermaAzione.razor`, che è **in pagina**, e da `Esci()`, che **disarma** la guardia prima
di navigare. Non ho una spiegazione dal codice, e non l'ho riprovato perché riprovare significa
bloccare di nuovo il browser. **Va nel rapporto come osservazione non confermata**, da verificare a
mano.

## Dati di prova rimasti, da eliminare a mano

I due dialoghi nativi hanno impedito all'agente di pulire. **Servono due gesti dell'utente:**

1. **Spesa «COLLAUDO GIRO B», 1.284,50 €, 19/09/2026**, spazio «Personale» → Spese, apri la voce,
   Elimina, conferma il dialogo del browser.
2. **Spazio «COLLAUDO GIRO B TEST»**, vuoto, un solo membro → Spazi, aprilo, Elimina lo spazio,
   conferma il dialogo.

La collezione abbozzata su `/collections/new` **non è stata salvata** e non esiste lato server.

## Console e rete

Nessun errore JavaScript in tutta la sessione. Nessuna chiamata fallita: tutte 200.
