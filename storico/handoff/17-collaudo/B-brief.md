# Collaudo, giro B — il contratto degli editor

**Leggi prima `handoff/17-collaudo/ambiente.md`**: server, browser, sessione, e il vincolo che
il database è quello vero. Non lo ripeto qui.

Sei il secondo dei quattro giri. Il giro A ha già verificato che creare una collezione funziona:
se non fosse passato non saresti stato lanciato.

## COSA STAI PROVANDO

Quattro schermate di modifica — **nota, collezione, elemento, spesa** — condividevano tre
difetti e ognuna li risolveva a modo suo, o non li risolveva. Sono stati unificati in una classe
base, `Shared/PaginaEditor.cs`, adottata da tutte e quattro in quattro unità di lavoro separate.

I tre difetti erano:

- **il lavoro non salvato si perdeva senza una domanda** (rilievo 1);
- **l'esito del salvataggio compariva dove non stai guardando** (rilievo 2);
- **due schermate su dodici non si spiegavano** (rilievo 12).

**Nessuna delle quattro adozioni è mai stata provata nel browser.** Sono verdi di compilazione e
di test, e questo è tutto ciò che si sa.

## I CRITERI — non li scrivo io, li hanno scritti le unità che hanno fatto il lavoro

**Non riscriverli e non inventarne di nuovi.** Sono già scritti, con la precisione di chi aveva
il codice davanti, in quattro file che devi leggere **prima di aprire il browser**:

| File | Quante prove | Su cosa |
|---|---|---|
| `handoff/03-contratto-editor/resoconto.md` | **dieci** | il contratto e il suo primo consumatore, `NoteEdit` |
| `handoff/04-collezione-contratto/resoconto.md` | **cinque** (prove 1-5) | `CollectionEdit` |
| `handoff/06-elemento-contratto/resoconto.md` | **sei** | `ItemEdit`, specifiche di quella pagina |
| `handoff/07-spesa-contratto/resoconto.md` | **sei** | `SpesaEdit` |

Cercale sotto un titolo che parla di prove o di criteri di accettazione. Sono ventisette in
tutto: **eseguile tutte**, e per ognuna scrivi passa o non passa. Se una non è eseguibile,
dillo e spiega perché invece di darla per buona.

**Una avvertenza esplicita**: la **prova 6 del resoconto 04** è dichiarata **non ripetibile** su
`ItemEdit`. Non provare a ripeterla lì.

## LE TRE COSE CHE VALE LA PENA PROVARE COMUNQUE, se i criteri non le coprono

1. **La guardia d'uscita dal tasto Indietro del browser.** È stato verificato *contro la
   documentazione* che `OnBeforeInternalNavigation` copre anche il tasto Indietro, ma **mai
   osservato**. Scrivi qualcosa in un editor, non salvare, e premi Indietro: deve chiedere
   conferma.
2. **La stessa cosa sulla chiusura della scheda o sulla ricarica**, che passa da un meccanismo
   diverso (`ConfirmExternalNavigation`). Prova con **F5**: deve comparire la finestra del
   browser che chiede se lasciare la pagina. **Se compare, ANNULLA** — non ricaricare, o perdi
   il resto del giro.
3. **La stessa istanza riusata fra due entità diverse.** Le pagine editor riusano la stessa
   istanza passando da un'entità all'altra: apri la nota A, arma «Elimina» fino a vedere «Sì,
   elimina», poi naviga alla nota B **senza confermare**. Su B il pulsante deve essere tornato
   «Elimina», non restare armato. Se restasse armato, un clic cancellerebbe un'entità che
   nessuno ha chiesto di cancellare: **è il difetto più grave che potresti trovare in questo
   giro**, e va riportato per primo.

## COSA NON TOCCARE

- **Non eliminare niente che non abbia creato tu.** La prova 3 qui sopra arma il pulsante ma
  **non lo conferma mai**: se ti trovi ad aver premuto «Sì, elimina» su una nota dell'utente,
  fermati e dillo subito nell'esito, in cima.
- Non provare le misure di CSS: sono il giro D.
- Non provare i messaggi d'errore tradotti: sono il giro C.

## L'ESITO

In **`handoff/17-collaudo/B-esito.md`**.

```
GIRO B — ESITO: PASSA | NON PASSA | PARZIALE
LE VENTISETTE PROVE: <passate>/<eseguite> — l'elenco di quelle NON passate, con cosa hai visto
NON ESEGUIBILI: <quali e perché> | nessuna
INDIETRO DEL BROWSER: chiede | non chiede | non provato
F5 / CHIUSURA SCHEDA: chiede | non chiede | non provato
ISTANZA RIUSATA (il pulsante armato che sopravvive): sicuro | DIFETTO | non provato
CONSOLE: <righe trascritte> | pulita
RESTA NEL DATABASE: <cosa hai creato> | niente
ALTRO CHE HAI VISTO: <fatti> | niente
```

Se una prova non passa, **trascrivi cosa hai visto**, non cosa ne pensi. Il capo decide.
