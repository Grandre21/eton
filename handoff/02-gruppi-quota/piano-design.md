PIANO-DESIGN — Ricorrenti divise in gruppi (resa provvisoria fino alla 2.1-bis)

Il mandato fissa la direzione: resa minima, solo token e componenti esistenti. Il piano quindi
non inventa un'identità: sceglie, fra i pezzi che Eton ha già, quelli che dicono «gruppo» e
«totale» senza aggiungere una superficie.

Colori (tutti token esistenti di wwwroot/css/app.css, nessun valore nuovo):
- fondo/superficie  #121212  (--superficie) — il contenitore .registro di ogni gruppo
- filo              #242424  (--bordo)
- testo             #f2f2f2  (--testo) — l'importo della quota
- testo tenue       #a1a1a1  (--testo-tenue) — la frase della quota
- testo fioco       #8a8a8a  (--testo-fioco) — la testata di gruppo (.testa-registro)
- accento           #4c8dff  (--accento) — solo l'hover della riga, già esistente

Caratteri: Inter (--prosa) per la frase della quota; Plex Mono (--mono) per testate e cifre,
come già fa .testa-registro e .dato.

Layout, allineato a sinistra, una colonna:

    Spese ricorrenti                         [Nuova ricorrente]
    In <spazio>
    Per annuali e periodiche, metti da parte 42,50 € al mese
                                                    (gap --s6)
    MENSILI                                  812,00 € al mese
    ┌──────────────────────────────────────────────────────┐
    │ Affitto        Casa  ogni mese, il 5  prossima …  800 │
    │ Netflix        …                                   12 │
    └──────────────────────────────────────────────────────┘
                                                    (gap --s6)
    ANNUALI                                   10,00 € al mese
    ┌ … ┐
    TERMINATE                                  (nessun totale)
    ┌ … ┐

Principi:
1. Il gruppo è una testata .testa-registro più un .registro: lo stesso pezzo delle pagine di
   elenco e della Home, ripetuto. Nessuna scheda, nessun titolo nuovo.
2. Il totale sta a destra nella testata, sopra la colonna degli importi che riassume; in
   cifre non spaziate (letter-spacing 0, minuscolo), perché il maiuscolo largo della testata
   su un numero lo rende illeggibile.
3. Fra un gruppo e il successivo --s6, la stessa distanza fra sottotitolo e contenuto: i gruppi
   si separano con l'aria, non con fili in più.
4. La quota è una frase, non un riquadro: testo tenue, importo in --testo e .dato.
5. Le Terminate restano in fondo, con la loro riga già tenue (.riga-terminata) e senza totale.
