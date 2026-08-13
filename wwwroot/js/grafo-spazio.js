// =============================================================================
// Il grafo di uno spazio, disegnato su canvas 2D.
//
// Non è decorazione: uno spazio di Eton È un grafo — delle persone che ci stanno
// dentro, delle cose che ci mettono, e dei voti che legano le une alle altre. Qui
// quel modello viene disegnato invece che spiegato. Le particelle che scorrono
// lungo un arco sono un voto che va da chi l'ha dato alla cosa votata.
//
// Canvas 2D e non WebGL, e nessuna libreria: il sito sta su GitHub Pages e deve
// funzionare offline come PWA, quindi ogni dipendenza sarebbe un file in più da
// servire e da tenere in cache. Tutto ciò che serve sta in questo file.
//
// I COLORI NON SONO SCRITTI QUI. Si leggono dalle variabili di :root, come ogni
// altra cosa del progetto: il blu è dove si preme — qui le persone, che sono i
// soggetti che agiscono — e il verde è dove si constata, quindi i voti e le cose
// votate. Un '#4c8dff' scritto dentro questo file sarebbe il primo colore del
// progetto a divergere dalla tavolozza il giorno che cambia.
// =============================================================================

/**
 * Avvia il grafo dentro un canvas.
 *
 * @param {HTMLCanvasElement} tela
 * @param {{persone: string[], elementi: {nome: string, voto: string}[]}} dati
 * @returns un oggetto con ferma(), da chiamare quando il componente si smonta.
 */
export function avvia(tela, dati) {
    const ctx = tela.getContext("2d");
    if (!ctx) return { ferma() { } };

    // Le tinte arrivano dal foglio di stile: stessa tavolozza dell'applicazione,
    // stesso significato. Lette una volta sola, perché non cambiano a runtime.
    const stile = getComputedStyle(document.documentElement);
    const blu = leggiRgb(stile.getPropertyValue("--accento"), [76, 141, 255]);
    const verde = leggiRgb(stile.getPropertyValue("--secondario"), [182, 243, 106]);
    const grigio = [160, 160, 160];

    // Chi ha chiesto meno movimento al sistema operativo non vede animazioni: si
    // disegna un fotogramma solo, completo e fermo. Non si mostra un riquadro vuoto
    // — il grafo è contenuto, non effetto — e non si ascolta il cambiamento della
    // preferenza a metà sessione: si legge all'avvio, come fa il CSS.
    const fermo = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    const persone = (dati?.persone ?? []).map((nome, i) => ({ nome, i }));
    const elementi = (dati?.elementi ?? []).map((e, i) => ({ ...e, i, caldo: 0 }));

    const stato = {
        larghezza: 0,
        altezza: 0,
        dpr: 1,
        t0: performance.now(),
        flusso: 0,
        energia: 0,
        ultimo: performance.now(),
        mouse: { x: -9999, y: -9999, dentro: false },
        onde: [],
        polvere: [],
        visibile: true,
        animazione: 0,
    };

    // --- dimensionamento ------------------------------------------------------

    function ridimensiona() {
        // Il devicePixelRatio va applicato ai pixel del canvas, non al CSS:
        // senza, su uno schermo a densità doppia le linee sottili risultano
        // sfocate. Tetto a 2 perché oltre si paga il quadruplo dei pixel per una
        // differenza che non si vede.
        stato.dpr = Math.min(2, window.devicePixelRatio || 1);
        stato.larghezza = tela.clientWidth;
        stato.altezza = tela.clientHeight;
        tela.width = Math.floor(stato.larghezza * stato.dpr);
        tela.height = Math.floor(stato.altezza * stato.dpr);

        // La polvere è proporzionale all'area, non un numero fisso: su un telefono
        // sarebbero altrimenti le stesse particelle di un monitor, cioè una nebbia.
        const quante = Math.min(90, Math.round(stato.larghezza * stato.altezza / 9000));
        stato.polvere = Array.from({ length: quante }, () => ({
            x: Math.random() * stato.larghezza,
            y: Math.random() * stato.altezza,
            vx: (Math.random() - .5) * .08,
            vy: (Math.random() - .5) * .08,
            r: .4 + Math.random() * .9,
            o: .05 + Math.random() * .14,
        }));
    }

    const osservatoreMisura = new ResizeObserver(() => { ridimensiona(); if (fermo) disegna(performance.now()); });
    osservatoreMisura.observe(tela);

    // Fuori dallo schermo non si disegna niente: su una vetrina lunga il grafo
    // resterebbe altrimenti a consumare batteria mentre si legge tutt'altro.
    const osservatoreVista = new IntersectionObserver(
        ([voce]) => { stato.visibile = voce.isIntersecting; },
        { threshold: 0 }
    );
    osservatoreVista.observe(tela);

    // --- interazione ----------------------------------------------------------

    function muovi(e) {
        const r = tela.getBoundingClientRect();
        stato.mouse.x = e.clientX - r.left;
        stato.mouse.y = e.clientY - r.top;
        stato.mouse.dentro = true;
    }

    function esci() {
        stato.mouse.dentro = false;
        stato.mouse.x = stato.mouse.y = -9999;
    }

    function tocca() {
        stato.energia = Math.min(1.6, stato.energia + .9);
        stato.onde.push({ r: 10, a: .34 });
    }

    tela.addEventListener("pointermove", muovi);
    tela.addEventListener("pointerleave", esci);
    tela.addEventListener("pointerdown", tocca);

    // --- disegno --------------------------------------------------------------

    /**
     * Un alone radiale attorno a un punto. Va disegnato mentre il blending è additivo:
     * è la somma di due aloni sovrapposti — non la loro copertura — a fare la luce che
     * si vede sui siti presi a riferimento. Un pallino pieno da solo resta un pallino.
     */
    function alone(x, y, r, colore, intensita) {
        const g = ctx.createRadialGradient(x, y, 0, x, y, r);
        g.addColorStop(0, rgba(colore, intensita));
        g.addColorStop(.45, rgba(colore, intensita * .35));
        g.addColorStop(1, rgba(colore, 0));
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(x, y, r, 0, Math.PI * 2);
        ctx.fill();
    }

    function disegna(ora) {
        const t = (ora - stato.t0) / 1000;
        // Il passo è limitato a 50ms: tornando su una scheda lasciata in secondo
        // piano, dt varrebbe qualche secondo e il flusso salterebbe in avanti di
        // colpo invece di riprendere da dov'era.
        const dt = Math.min(.05, (ora - stato.ultimo) / 1000);
        stato.ultimo = ora;

        // L'energia si accumula al tocco e decade da sé: è ciò che fa accelerare i
        // voti in transito per un istante dopo un'interazione, e poi tornare calmi.
        stato.energia *= .96;
        stato.flusso += dt * (1 + stato.energia * 2.2);

        const w = stato.larghezza, h = stato.altezza;
        const cx = w / 2, cy = h / 2;

        // Un'ELLISSE e non un cerchio, ed è una correzione fatta guardando il risultato:
        // con un raggio unico preso da Math.min(larghezza, altezza) comanda sempre
        // l'altezza — il canvas è largo il doppio di quanto è alto — e i nodi finivano
        // ammassati in una macchia centrale con le etichette una sopra l'altra, sprecando
        // metà larghezza. Separando i due raggi la figura riempie lo spazio che ha.
        // Il tetto a 340px serve al monitor: oltre, i nodi si allontanano tanto dal centro
        // da non leggersi più come un insieme.
        const raggioX = Math.min(w * .38, 340);
        const raggioY = h * .36;
        // L'orbita interna aveva lo stesso rapporto sui due assi, e su un'ellisse già
        // schiacciata questo la riduceva a una sessantina di pixel in verticale: la persona
        // che capita in basso finiva addosso al nodo dello spazio. Il minimo assoluto in Y
        // tiene la distanza anche quando il canvas si abbassa.
        const internoX = raggioX * .5;
        const internoY = Math.max(raggioY * .55, 74);

        ctx.setTransform(stato.dpr, 0, 0, stato.dpr, 0, 0);
        ctx.clearRect(0, 0, w, h);

        // --- polvere, che si scansa dal cursore ---
        for (const p of stato.polvere) {
            p.x += p.vx; p.y += p.vy;
            if (stato.mouse.dentro) {
                const dx = p.x - stato.mouse.x, dy = p.y - stato.mouse.y;
                const d = Math.hypot(dx, dy);
                if (d < 150 && d > .01) {
                    const spinta = (1 - d / 150) * .3;
                    p.x += dx / d * spinta;
                    p.y += dy / d * spinta;
                }
            }
            // Riavvolgimento ai bordi: una particella che esce rientra dall'altra
            // parte, così il campo non si svuota col passare dei minuti.
            if (p.x < -8) p.x = w + 8; if (p.x > w + 8) p.x = -8;
            if (p.y < -8) p.y = h + 8; if (p.y > h + 8) p.y = -8;

            ctx.beginPath();
            ctx.fillStyle = rgba(grigio, p.o);
            ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
            ctx.fill();
        }

        // --- la camera si inclina verso il cursore ---
        // Un decimo di grado di parallasse: appena percettibile, ed è ciò che fa
        // sembrare la scena un oggetto guardato invece che un disegno stampato.
        const ox = stato.mouse.dentro ? (stato.mouse.x - cx) * .012 : 0;
        const oy = stato.mouse.dentro ? (stato.mouse.y - cy) * .012 : 0;
        ctx.translate(-ox, -oy);

        // --- posizioni ---
        for (const el of elementi) {
            const a = -Math.PI / 2 + el.i / elementi.length * Math.PI * 2;
            // Il respiro è sfasato per nodo (a + i): tutti insieme sembrerebbero un
            // battito cardiaco, sfalsati sembrano cose vive ciascuna per conto suo.
            const respiro = 1 + Math.sin(t * .5 + el.i * 1.3) * .022;
            el.x = cx + Math.cos(a) * raggioX * respiro;
            el.y = cy + Math.sin(a) * raggioY * respiro;

            // Il nodo si accende avvicinandosi, e si spegne piano: l'inseguimento
            // smorzato (il moltiplicatore .1) è la differenza fra un interruttore e
            // qualcosa che reagisce.
            const vicino = stato.mouse.dentro
                && Math.hypot(el.x - stato.mouse.x - ox, el.y - stato.mouse.y - oy) < 46;
            el.caldo += ((vicino ? 1 : 0) - el.caldo) * .1;
        }

        for (const p of persone) {
            const a = -Math.PI / 2 + (p.i + .5) / persone.length * Math.PI * 2;
            p.x = cx + Math.cos(a) * internoX;
            p.y = cy + Math.sin(a) * internoY;
        }

        // --- archi: chi ha votato cosa ---
        // In additivo, ed è il punto: dove due archi si sovrappongono la luce si
        // somma invece di coprirsi, che è come si comporta la luce vera. È questo —
        // non la saturazione del colore — a far sembrare acceso un tratto tenue.
        ctx.globalCompositeOperation = "lighter";

        // Ogni cosa è votata da due persone, non da una: con un arco solo il disegno
        // diceva «ognuno ha le sue», che è l'opposto di quello che Eton fa. Il secondo
        // arco è più tenue e senza voti in transito — serve la trama, non il traffico.
        elementi.forEach((el, i) => {
            disegnaArco(el, persone[i % persone.length], i, 1);
            disegnaArco(el, persone[(i + 1) % persone.length], i + 100, .45);
        });

        function disegnaArco(el, p, seme, peso) {
            const acceso = (.20 + el.caldo * .5) * peso;

            // Curva e non retta: il punto di controllo sta di lato e oscilla piano,
            // così l'arco respira invece di stare teso.
            const mx = (p.x + el.x) / 2, my = (p.y + el.y) / 2;
            const nx = -(el.y - p.y), ny = el.x - p.x;
            const n = Math.hypot(nx, ny) || 1;
            const curva = (seme % 2 ? 1 : -1) * (.16 + Math.sin(t * .5 + seme) * .04);
            const kx = mx + nx / n * n * curva * .5;
            const ky = my + ny / n * n * curva * .5;

            // Il tratto non è grigio ma sfuma dal blu al verde, e non è un vezzo: è la
            // grammatica del progetto letta lungo una linea. Parte da chi preme e arriva
            // a ciò che si constata — l'arco È quel passaggio.
            const sfuma = ctx.createLinearGradient(p.x, p.y, el.x, el.y);
            sfuma.addColorStop(0, rgba(blu, acceso));
            sfuma.addColorStop(1, rgba(verde, acceso * .85));

            ctx.beginPath();
            ctx.strokeStyle = sfuma;
            ctx.lineWidth = peso === 1 ? 1.1 : .8;
            ctx.moveTo(p.x, p.y);
            ctx.quadraticCurveTo(kx, ky, el.x, el.y);
            ctx.stroke();

            if (peso < 1) return;

            // I voti in transito. Verdi perché un voto è un valore constatato, e
            // vanno DALLA persona ALLA cosa votata: la direzione è il significato.
            const quanti = 2 + (seme % 2);
            for (let k = 0; k < quanti; k++) {
                const u = (stato.flusso * (.10 + (seme % 3) * .015) + k / quanti + seme * .17) % 1;
                const q = puntoSuCurva(u, p, { x: kx, y: ky }, el);
                const intensita = (1 - Math.abs(u - .5) * 1.05) * (.6 + el.caldo * .4 + stato.energia * .3);
                alone(q.x, q.y, 7, verde, Math.max(0, intensita) * .3);
                ctx.beginPath();
                ctx.fillStyle = rgba(verde, Math.max(0, intensita) * .9);
                ctx.arc(q.x, q.y, 1.7 + el.caldo * .8, 0, Math.PI * 2);
                ctx.fill();
            }
        }

        // I raggi dal centro alle persone: lo spazio non è un nodo accanto agli altri,
        // è ciò che contiene le persone. Senza questi tratti il disegno non lo diceva.
        for (const p of persone) {
            ctx.beginPath();
            ctx.strokeStyle = rgba(blu, .22);
            ctx.lineWidth = .9;
            ctx.moveTo(cx, cy);
            ctx.lineTo(p.x, p.y);
            ctx.stroke();
        }

        // --- onde al tocco ---
        stato.onde = stato.onde.filter(o => o.a > .012);
        for (const o of stato.onde) {
            o.r += 3 + stato.energia * 3;
            o.a *= .955;
            ctx.beginPath();
            ctx.strokeStyle = rgba(blu, o.a);
            ctx.lineWidth = 1;
            ctx.arc(cx, cy, o.r, 0, Math.PI * 2);
            ctx.stroke();
        }

        // --- alone centrale ---
        // Il gradiente radiale è per forza circolare: prende il maggiore dei due raggi
        // interni, così l'alone copre l'orbita delle persone anche sull'asse lungo.
        alone(cx, cy, Math.max(internoX, internoY) * 1.7, blu, .20);

        // --- gli aloni dei nodi, finché il blending è ancora additivo ---
        // Prima gli aloni di TUTTI i nodi, poi (fuori dall'additivo) tutti i dischi: se
        // si alternassero, l'alone di un nodo si sommerebbe sopra il disco del vicino
        // schiarendolo, e i nodi al centro risulterebbero più chiari di quelli ai bordi.
        alone(cx, cy, 34, blu, .34);
        for (const p of persone) alone(p.x, p.y, 22, blu, .30);
        for (const el of elementi) alone(el.x, el.y, 18 + el.caldo * 14, verde, .22 + el.caldo * .3);

        ctx.globalCompositeOperation = "source-over";

        // --- le persone ---
        for (const p of persone) {
            ctx.beginPath();
            ctx.fillStyle = rgba(blu, .95);
            ctx.arc(p.x, p.y, 6, 0, Math.PI * 2);
            ctx.fill();

            ctx.font = `600 11px ${stile.getPropertyValue("--mono") || "monospace"}`;
            ctx.fillStyle = rgba(grigio, .82);
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText(p.nome, p.x, p.y - 17);
        }

        // --- le cose votate ---
        for (const el of elementi) {
            ctx.beginPath();
            ctx.fillStyle = rgba(verde, .7 + el.caldo * .3);
            ctx.arc(el.x, el.y, 5 + el.caldo * 2.5, 0, Math.PI * 2);
            ctx.fill();

            ctx.textAlign = "center";
            ctx.textBaseline = "middle";

            // Il nome in Inter, il voto in mono: è lo stesso discrimine di tutta
            // l'applicazione — la prosa si legge, i dati si confrontano.
            ctx.font = `500 12px ${stile.getPropertyValue("--prosa") || "sans-serif"}`;
            ctx.fillStyle = rgba(grigio, .62 + el.caldo * .38);
            ctx.fillText(el.nome, el.x, el.y + 19);

            ctx.font = `600 12px ${stile.getPropertyValue("--mono") || "monospace"}`;
            ctx.fillStyle = rgba(verde, .62 + el.caldo * .38);
            ctx.fillText(el.voto, el.x, el.y - 16);
        }

        // --- il nodo dello spazio ---
        const pulsa = 1 + Math.sin(t * 1.1) * .12;
        ctx.beginPath();
        ctx.fillStyle = rgba(blu, 1);
        ctx.arc(cx, cy, 9 * pulsa, 0, Math.PI * 2);
        ctx.fill();
        // L'anello attorno: distingue lo spazio dalle persone senza doverlo scrivere.
        // Un cerchio più grande e basta sarebbe stato «una persona importante».
        ctx.beginPath();
        ctx.strokeStyle = rgba(blu, .35);
        ctx.lineWidth = 1;
        ctx.arc(cx, cy, 16 * pulsa, 0, Math.PI * 2);
        ctx.stroke();

        ctx.setTransform(1, 0, 0, 1, 0, 0);
    }

    function ciclo(ora) {
        if (stato.visibile) disegna(ora);
        else stato.ultimo = ora;   // senza, al rientro in vista il flusso salterebbe
        stato.animazione = requestAnimationFrame(ciclo);
    }

    ridimensiona();
    if (fermo) disegna(performance.now());
    else stato.animazione = requestAnimationFrame(ciclo);

    return {
        ferma() {
            cancelAnimationFrame(stato.animazione);
            osservatoreMisura.disconnect();
            osservatoreVista.disconnect();
            tela.removeEventListener("pointermove", muovi);
            tela.removeEventListener("pointerleave", esci);
            tela.removeEventListener("pointerdown", tocca);
        }
    };
}

// --- utilità ----------------------------------------------------------------

function puntoSuCurva(u, a, c, b) {
    const v = 1 - u;
    return {
        x: v * v * a.x + 2 * v * u * c.x + u * u * b.x,
        y: v * v * a.y + 2 * v * u * c.y + u * u * b.y,
    };
}

function rgba([r, g, b], a) {
    return `rgba(${r},${g},${b},${a})`;
}

/**
 * Legge un colore da una variabile CSS. Accetta le sole forme che il progetto
 * usa davvero — '#rrggbb' — e ripiega sul valore atteso se la variabile manca o
 * è scritta in un modo che qui non si sa leggere: un grafo con un colore di
 * ripiego è un difetto estetico, un grafo che lancia è una pagina rotta.
 */
function leggiRgb(valore, ripiego) {
    const v = (valore || "").trim();
    const m = /^#([0-9a-f]{6})$/i.exec(v);
    if (!m) return ripiego;
    const n = parseInt(m[1], 16);
    return [(n >> 16) & 255, (n >> 8) & 255, n & 255];
}
