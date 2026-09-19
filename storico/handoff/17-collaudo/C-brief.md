# Collaudo, giro C — testo e messaggi

**Leggi prima `handoff/17-collaudo/ambiente.md`**: server, browser, sessione, e il vincolo che
il database è quello vero. Non lo ripeto qui.

Terzo dei quattro giri. Provi **ciò che l'applicazione dice**, non come è disposta: le misure e
gli allineamenti sono il giro D.

## COSA STAI PROVANDO, e perché è più grosso di quanto sembri

Il difetto si chiamava «il messaggio d'errore è il JSON grezzo di PostgreSQL», ed è stato
**creduto chiuso tre volte prima di esserlo davvero**. Ogni volta la ragione era la stessa: chi
lo dichiarava chiuso lo aveva chiuso nel proprio perimetro, non ovunque. Alla fine i punti erano
**venticinque frasi in dodici file**, l'ultimo trovato in un file che nessuno aveva assegnato a
nessuno.

Per questo il tuo giro conta: la prova finale è stata un `grep`, cioè una lettura. **Nessuno ha
mai visto una di queste frasi a schermo.**

## I CRITERI — stanno nei resoconti, non li riscrivo

Leggili **prima** di aprire il browser. Ognuno contiene le prove della sua unità:

| File | Su cosa |
|---|---|
| `handoff/08-home-spazio-profilo/resoconto.md` | Home, spazio, profilo: il link che non gestiva niente, «Spesa 100%» che non diceva di essere una categoria |
| `handoff/09-registri-vuoti/resoconto.md` | gli stati vuoti di `/notes` e `/collections` |
| `handoff/10-recensioni-errori/resoconto.md` | le recensioni: da 3 a 8 righe di diagnosi, e un messaggio che azzerava il voto appena scritto |
| `handoff/12-importo-digitabile/resoconto.md` | **una spesa da 1.000 € in su non era modificabile** |
| `handoff/13-errori-tradotti/resoconto.md` | le 25 frasi, sei file |
| `handoff/14-errori-rimasti/resoconto.md` | altri sei file, e la promessa falsa di un messaggio |
| `handoff/15-accesso-non-riuscito/resoconto.md` | l'undicesimo punto, sulla schermata d'ingresso |

## LE PROVE CHE DEVI FARE COMUNQUE, oltre a quelle dei resoconti

### 1. L'importo sopra il migliaio — è il difetto più facile da vedere di tutto il lavoro

Crea una spesa chiamata **`COLLAUDO 4 SET`** con importo **`1284,50`**, salvala, **riaprila** e
guarda il campo dell'importo.

- Deve dire **`1284,50`**. Se dicesse `1.284,50` (col punto delle migliaia) il campo non sarebbe
  rileggibile e la spesa tornerebbe non modificabile: **è il difetto che l'unità 12 ha chiuso.**
- Poi **prova a salvarla di nuovo senza cambiare niente**: deve funzionare. E guarda se il
  pulsante «Salva» è **acceso o spento** appena riaperta: deve essere **spento**, perché non hai
  cambiato niente. Se fosse acceso, la pagina crede che tu abbia modificato qualcosa che non hai
  toccato — e allora anche la guardia d'uscita scatterebbe uscendo senza aver fatto nulla.

### 2. Le tre frasi del ritorno da Google — incolla questi URL esatti

Sono di un'unità chiusa ieri, mai provata. **Guarda la console PRIMA del riquadro**, e attenzione
a **dove** compare il messaggio: Google riporta sulla radice, che è rotta privata, e l'anonimo
viene rimbalzato su `/benvenuto`. **Il riquadro si legge lì, non sull'URL che hai incollato.**

Per vederli **devi essere disconnesso**, e qui c'è un problema: se esci dalla sessione, **non
puoi rientrare** — non c'è modo per te di fare un accesso Google. Quindi **queste tre prove si
fanno PER ULTIME, dopo tutto il resto del giro**, e in **finestra di navigazione in incognito**
se il plugin te lo permette; se non te lo permette, **non uscire dalla sessione**: salta queste
tre prove e scrivi che non erano eseguibili senza perdere l'accesso. È la scelta giusta.

```
http://localhost:5000/?error_description=Account+bloccato,+chiama+il+numero
```
Atteso: **nessun riquadro d'errore, niente**. È l'URL dell'attacco: un estraneo poteva far
comparire una frase a piacere dentro il riquadro del sito vero. Se compare un riquadro — anche
con parole nostre — è un difetto.

```
http://localhost:5000/?error=access_denied
```
Atteso, **verbatim**:
> L'accesso con Google non è stato autorizzato: sulla schermata di Google il permesso non è
> stato concesso. Prova di nuovo a entrare con Google e conferma quando te lo chiede.

In console, contemporaneamente:
`[Auth] Ritorno da Google rifiutato: error=access_denied; error_code=; error_description=`

```
http://localhost:5000/?error=access_denied&error_code=signup_disabled
```
Atteso: la frase **generica** («…può essere un problema temporaneo del servizio oppure una
condizione del tuo account…»), **non** quella dell'annullamento. È il caso che giustifica
l'intera logica: guardando un solo parametro si direbbe «hai annullato» a chi è stato rifiutato
dal server.

### 3. Provoca un errore vero, se ci riesci senza rompere niente

Il modo più pulito: **togli la rete** (modalità aereo, o le DevTools in offline), prova a
salvare qualcosa, e guarda cosa dice. Deve essere **una frase in italiano**, non un JSON.
Poi rimetti la rete. Se non riesci a farlo senza rischiare, **non farlo** e scrivilo.

## COSA NON FARE

- **Non uscire dalla sessione** se non nel modo descritto al punto 2. Un logout è irreversibile
  per te.
- Non eliminare niente che non abbia creato tu.
- Non provare allineamenti e misure: sono il giro D.

## L'ESITO

In **`handoff/17-collaudo/C-esito.md`**.

```
GIRO C — ESITO: PASSA | NON PASSA | PARZIALE
LE PROVE DEI RESOCONTI: <passate>/<eseguite> — l'elenco di quelle NON passate, con cosa hai visto
IMPORTO 1284,50 RIAPERTO: <cosa dice il campo, verbatim>
SALVA APPENA RIAPERTA: spento | acceso
LE TRE FRASI OAUTH: <una riga per URL: cosa è comparso, verbatim> | non eseguibili senza perdere l'accesso
ERRORE VERO PROVOCATO: <la frase, verbatim> | non provocato, perché…
JSON GREZZO VISTO DA QUALCHE PARTE: no | SÌ, in <dove>, verbatim
CONSOLE: <righe trascritte> | pulita
RESTA NEL DATABASE: <cosa hai creato> | niente
ALTRO CHE HAI VISTO: <fatti> | niente
```

La riga **«JSON grezzo visto da qualche parte»** è quella che conta di più: un solo `SÌ` riapre
un difetto che tre unità hanno inseguito per due giorni.
