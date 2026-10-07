// Una griglia senza questo modulo funziona ancora col mouse: il guasto si contiene qui.
// Il JavaScript non decide niente: la logica dei tasti è in C# (Services/TastieraGriglia.cs). Il
// preventDefault sta qui perché @onkeydown:preventDefault di Blazor è un booleano statico per
// elemento, deciso prima di sapere quale tasto è: non lascerebbe passare Ctrl+C fermando le frecce.

/**
 * Ferma il comportamento del browser sui tasti della griglia (scorrimento, carattere perso, Tab).
 *
 * @param {HTMLTableElement} tabella
 * @returns un oggetto con ferma(), da chiamare quando il componente si smonta.
 */
export function avvia(tabella) {
    const suTasto = (e) => {
        const cella = e.target.closest("td[data-colonna]");
        if (!cella || !tabella.contains(cella)) return;
        const nelCampo = e.target.matches("input, select");
        // Come in Griglia.razor: Meta, oppure Ctrl o Alt da soli. AltGr arriva come Ctrl+Alt insieme
        // e produce caratteri (@, #, €), quindi non è un modificatore.
        const modificatore = e.metaKey || e.ctrlKey !== e.altKey;
        const carattere = e.key.length === 1 && !modificatore;
        const movimento = ["ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight", "Enter", "F2"].includes(e.key);
        if (!nelCampo && (carattere || (movimento && !modificatore)))
            e.preventDefault();          // niente scorrimento della pagina, niente carattere perso nel vuoto
        if (nelCampo && (e.key === "Enter" || e.key === "Tab"))
            e.preventDefault();          // Tab non deve far uscire il fuoco dalla tabella: lo sposta Blazor
    };
    tabella.addEventListener("keydown", suTasto);
    return { ferma: () => tabella.removeEventListener("keydown", suTasto) };
}

/**
 * Porta il fuoco sulla cella (o sul campo che contiene, col cursore in fondo al testo). Non lo
 * sposta se l'utente sta scrivendo fuori dalla tabella, per esempio nel filtro della pagina.
 *
 * @param {HTMLTableElement} tabella
 * @param {string} riga la chiave della riga
 * @param {string} colonna la chiave della colonna
 */
export function focalizza(tabella, riga, colonna) {
    const attivo = document.activeElement;
    if (attivo && attivo !== document.body && !tabella.contains(attivo)) return;
    const sel = `td[data-riga="${CSS.escape(riga)}"][data-colonna="${CSS.escape(colonna)}"]`;
    const cella = tabella.querySelector(sel);
    if (!cella) return;
    const campo = cella.querySelector("input, select");
    (campo ?? cella).focus();
    if (campo && campo.type === "text") campo.setSelectionRange(campo.value.length, campo.value.length);
}

/**
 * Scarica un testo come file CSV.
 *
 * @param {string} nomeFile
 * @param {string} testo il contenuto del file
 */
export function scarica(nomeFile, testo) {
    const url = URL.createObjectURL(new Blob([testo], { type: "text/csv;charset=utf-8" }));
    const a = Object.assign(document.createElement("a"), { href: url, download: nomeFile });
    a.click();
    URL.revokeObjectURL(url);
}
