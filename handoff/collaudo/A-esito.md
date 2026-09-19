# Giro A del collaudo — le misure · **ESITO: verde**, 7/7

Eseguito il **19 settembre 2026** su `http://localhost:5000`, commit `bb77cf7`, server avviato dal
capo (PID in `handoff/server.md`). Nessun difetto.

## Le sette misure, col valore letto

| # | Misura | Atteso | Letto | |
|---|---|---|---|---|
| 1 | «Profilo» — overflow del collegamento | `scrollWidth === clientWidth` | **48 / 48**, differenza **0** | PASSA |
| 1 | «Profilo» — distanza dal selettore di spazio | ≥ 8px | **21px** (188,2 − 167,2) | PASSA |
| 1 | «Profilo» — altezza | 48px | **48px** | PASSA |
| 1 | **«Profilo» — nome accessibile** | ancora «Profilo» | `link "Profilo"` dal motore di accessibilità | PASSA |
| 2 | `--testo-fioco` | `#8a8a8a` | **`#8a8a8a`**, e l'etichetta resa è `rgb(138,138,138)` esatto su fondo `#0a0a0a` | PASSA |
| 3 | Colonna del contenuto, `/notes` contro `/expenses` | ±0,5px | **313,6 e 313,6**, scarto **0** | PASSA |
| 4 | Frecce di mese | 48×48, raggio 12px | **48×48, raggio 12px** | PASSA |
| 4 | Pastiglie di categoria del modulo «Segna» | 48px | **48px**, tutte e dieci | PASSA |
| 4 | «Da provare» | 48px | **48px** | PASSA |
| 4 | «Nessun voto» | 48px (erano 22) | **48px** | PASSA |
| 5 | Medaglione | 40×40, **anello** | 40×40, `border: 0.8px solid #333`, fondo **trasparente** | PASSA |
| 5 | Medaglione accanto all'`h1` | allineato | **stesso centro verticale**, 76,075px entrambi | PASSA |
| 6 | Pastiglie dentro le righe di elenco | restano piccole | **26,65px**, font 11px = `--t-xs` esatto | PASSA |
| 6 | Tavolozza emoji | resta a `--t-lg` | **20px** = `--t-lg` esatto, su 24 campionate | PASSA |
| 7 | **CONTROLLO NEGATIVO** — `<span class="pastiglia">` su `/spaces` | **non** devono crescere | **26,65px** entrambe, e restano `<span>` | PASSA |

## Perché il controllo negativo vale più delle altre sei

Una regola è appena passata da **tre selettori di luogo** a uno di **condizione semantica**,
`button.pastiglia`. Le sei misure positive dicono che ha agganciato ciò che doveva; solo la settima
dice che **non ha agganciato altro**. Le pastiglie di `/spaces` sono `<span>`, stanno in righe di
elenco, e se fossero salite a 48px la fusione sarebbe colata fuori dal proprio perimetro — in una
schermata che nessun mandato nominava. Sono ferme a 26,65px.

L'unità 07 ha aggiunto questa prova **da sé**, e non era nel suo mandato.

## ⚠️ La riserva, dichiarata dall'agente invece che taciuta

Sulla misura 3, il dataset di prova aveva **entrambe** le rotte con barra di scorrimento: il caso
«una scorre, una non scorre» **non è stato isolato**. Lo scarto misurato è 0 e la misura passa, ma
non è la prova del difetto originale — che nasceva proprio dalla differenza fra i due stati.

**Non è un difetto e non richiede un altro giro**, per un motivo strutturale: `scrollbar-gutter:
stable` riserva la colonna **sempre**, che la pagina scorra o no, quindi i due casi non possono più
divergere per costruzione. Ma la prova diretta manca, e chi rilegge deve saperlo invece di
dedurlo dal verde.

## Console e rete

Nessun errore in console — solo il messaggio informativo del runtime Blazor a ogni caricamento.
Nessuna chiamata fallita: le sette richieste verso Supabase hanno tutte risposto 200.

Il banner «è disponibile una versione nuova» è riapparso durante la navigazione ed è stato
dismesso con «Più tardi» prima di ogni misura, per non sovrapporlo agli elementi sotto esame. In
sviluppo **non è un difetto**: è il comportamento documentato del service worker.
