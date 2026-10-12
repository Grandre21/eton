// Attributo e non classe, perché Blazor riscrive class.
export function evidenzia(ancora) {
    togli();
    const el = document.querySelector(`[data-tutorial="${CSS.escape(ancora)}"]`);
    // getClientRects vuoto: l'elemento c'è ma non si vede (gruppo chiuso, schermo stretto). Per il
    // tutorial è come se non ci fosse.
    if (!el || el.getClientRects().length === 0) return null;
    el.setAttribute("data-tutorial-evidenziato", "");
    el.scrollIntoView({ block: "center" });
    const r = el.getBoundingClientRect();
    return { sopra: r.top, sotto: r.bottom, sinistra: r.left, altezzaFinestra: window.innerHeight };
}

export function togli() {
    document.querySelectorAll("[data-tutorial-evidenziato]").forEach(e => e.removeAttribute("data-tutorial-evidenziato"));
}

// Il top layer sfugge al transform degli antenati.
export function mostra(riquadro) {
    if (riquadro.showPopover && !riquadro.matches(":popover-open")) riquadro.showPopover();
}
