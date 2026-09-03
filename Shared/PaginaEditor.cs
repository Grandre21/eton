using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Eton.Shared;

/// <summary>
/// La guardia comune agli editor dell'applicazione (note, collezioni, spese, elementi): chiede
/// conferma prima di abbandonare un modulo con modifiche non salvate.
/// <para>
/// Copre due canali distinti. <see cref="GuardaUscita"/>, agganciata a
/// <c>OnBeforeInternalNavigation</c> di <c>NavigationLock</c>, intercetta i link interni, le
/// chiamate a <c>NavigateTo</c> e il tasto Indietro/Avanti del browser, e usa <c>confirm()</c>
/// per poter decidere se annullare la navigazione. <c>ConfirmExternalNavigation</c>, che ogni
/// pagina imposta direttamente nel markup, copre invece la chiusura della scheda, il ricaricamento,
/// la barra indirizzi e i link esterni: è l'evento <c>beforeunload</c> del browser, il cui testo
/// non è personalizzabile, e su iOS in modalità PWA resta best-effort. Il caso interno usa
/// <c>confirm()</c> con lo stesso messaggio proprio per assomigliare a quell'altro caso, non per
/// caso.
/// </para>
/// <para>
/// Una classe base e non un componente con <c>[Parameter]</c>: dopo Crea() l'editor chiama
/// NavigateTo con Cambiata ancora vera, e fra quella decisione e la navigazione non c'è nessun
/// render. Un [Parameter] porterebbe il valore catturato all'ultimo render, e la guardia
/// chiederebbe "hai modifiche non salvate" subito dopo un salvataggio riuscito. Una classe base
/// legge invece il campo vivo nell'istante dell'handler.
/// </para>
/// </summary>
public abstract class PaginaEditor : ComponentBase, IDisposable
{
    // private, non protected: nei sette call-site di NavigateTo dei quattro editor del progetto la
    // chiamata grezza è sempre quella da sostituire con Esci, ed esporla accanto al wrapper
    // corretto offrirebbe la via sbagliata con la stessa comodità di quella giusta.
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // Disarma la guardia sulla PROSSIMA navigazione interna. Serve perché Cambiata da sola non
    // basta: dopo Crea() e dopo Elimina() lo stato del modulo È legittimamente sporco (la nota è
    // appena stata creata, o la nota che si sta per lasciare non esiste più), e senza un secondo
    // canale la guardia scatterebbe su un'uscita voluta, subito dopo un'operazione riuscita.
    private bool disarmata;

    private bool smontata;

    /// <summary>
    /// Vero quando il modulo ha dati non salvati.
    /// </summary>
    protected abstract bool Cambiata { get; }

    /// <summary>
    /// L'unico modo di uscire da un editor: alza <see cref="disarmata"/> prima di navigare, per i
    /// casi descritti lì.
    /// </summary>
    protected void Esci(string uri, bool replace = false)
    {
        // Crea() ed Elimina() possono essere ancora sospesi su una chiamata di rete quando l'utente
        // ha già lasciato questa pagina: il Task non è legato a nessuna cancellazione e prosegue
        // dopo lo smontaggio del componente. Navigation è un singleton condiviso da tutta
        // l'applicazione, quindi navigare a questo punto dirotterebbe qualunque pagina l'utente
        // stia guardando in quel momento, senza che l'abbia chiesto. L'oggetto creato o eliminato
        // resta creato o eliminato: è solo la navigazione ad essere abbandonata, e l'utente lo
        // ritroverà nell'elenco.
        if (smontata) return;

        disarmata = true;
        Navigation.NavigateTo(uri, replace: replace);
    }

    protected async Task GuardaUscita(LocationChangingContext ctx)
    {
        // Consumato qui, prima di guardare Cambiata: un flag alzato e non consumato disarmerebbe in
        // silenzio la guardia sulla navigazione successiva.
        if (disarmata) { disarmata = false; return; }
        if (!Cambiata) return;

        var esci = await JS.InvokeAsync<bool>("confirm", "Hai modifiche non salvate: se esci adesso le perdi. Vuoi uscire lo stesso?");
        if (!esci) ctx.PreventNavigation();
    }

    public virtual void Dispose() => smontata = true;
}
