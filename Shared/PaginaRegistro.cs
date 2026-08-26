using Microsoft.AspNetCore.Components;
using Eton.Models;
using Eton.Services;

namespace Eton.Shared;

/// <summary>
/// La macchina di caricamento comune ai registri dell'applicazione (note, collezioni, spese): un
/// primo caricamento all'apertura della pagina, una rilettura quando l'utente preme Riprova, e un
/// riallineamento quando lo spazio attivo cambia mentre la pagina è già a schermo — con le stesse
/// garanzie di non sovrapposizione fra letture in corsa in tutti e tre i casi.
/// <para>
/// Due punti di estensione, non di più: <see cref="ScartaCambioSpazio"/> serve a una pagina che
/// alza una guardia propria prima di chiamare Carica(), oltre a quella su un caricamento già in
/// volo; <see cref="PrimaDiRicaricare"/> serve a una pagina con uno stato proprio da azzerare solo
/// sulle ricariche innescate da un cambio di spazio, non su quelle che avvia da sé. Oggi è Spese
/// l'unico caso che li usa.
/// </para>
/// </summary>
public abstract class PaginaRegistro : ComponentBase, IDisposable
{
    [Inject] protected SpaceStateService Spazi { get; set; } = default!;

    protected bool caricato;

    // Distingue "questo spazio non ha dati" da "i dati non sono riuscito a leggerli": l'elenco è
    // vuoto in entrambi i casi, ma dire "ancora nessun elemento qui" quando la lettura è fallita è
    // un'affermazione precisa su uno spazio di cui non sappiamo niente.
    protected bool datiNonLetti;

    protected string? errore;
    protected Guid spazioMostrato;

    // Vero mentre Carica() è in volo. La guardia sull'id non basta: AssicuraCaricatoAsync emette
    // Cambiato PRIMA di restituire il controllo, quindi alla prima visita della pagina l'evento
    // arriva mentre siamo ancora dentro Carica(), con spazioMostrato ancora vuoto — e il confronto
    // fra un id vero e uno vuoto lascia passare tutto. Il risultato sarebbe una seconda lettura
    // identica alla prima, in parallelo. Scartare l'evento non perde niente: il caricamento in
    // corso lo spazio aggiornato lo legge comunque.
    private bool inCaricamento;

    /// <summary>
    /// Il nome plurale della cosa che la pagina elenca, come compare nei messaggi
    /// d'errore: "le note", "le collezioni", "le spese". Con l'articolo: il valore finisce
    /// interpolato dentro una frase ("non ho potuto leggerne {NomePlurale}"), non scritto
    /// da solo come un'etichetta.
    /// </summary>
    protected abstract string NomePlurale { get; }

    /// <summary>
    /// L'unica parte che cambia fra una pagina e l'altra: azzera i propri campi dati
    /// e li rilegge per lo spazio passato. Se la lettura dei dati principali fallisce,
    /// l'implementazione chiama SegnalaNonLetti(ex) e ritorna.
    /// <para>
    /// Azzerati PRIMA di leggere, come in Home.CaricaDettagli. L'intestazione stampa già
    /// @Spazi.Attivo.Name, cioè il nome NUOVO: lasciare a schermo i dati del vecchio spazio li
    /// farebbe passare per dati di questo, e non c'è niente che smentisca.
    /// </para>
    /// </summary>
    protected abstract Task Leggi(Space attivo);

    /// <summary>
    /// Punto di estensione per una guardia AGGIUNTIVA in SuCambioSpazio, oltre a
    /// inCaricamento. Solo Spese la usa.
    /// </summary>
    protected virtual bool ScartaCambioSpazio() => false;

    /// <summary>
    /// Punto di estensione eseguito subito prima di una ricarica innescata dal cambio
    /// di spazio. Solo Spese la usa.
    /// </summary>
    protected virtual void PrimaDiRicaricare() { }

    protected override async Task OnInitializedAsync()
    {
        Spazi.Cambiato += SuCambioSpazio;
        await Carica();
    }

    protected async Task Carica()
    {
        inCaricamento = true;
        try
        {
            // Ciclo e non chiamata singola: SuCambioSpazio SCARTA gli eventi che arrivano mentre
            // questo metodo è in volo, e scartarli senza rileggere dopo li perderebbe per sempre.
            // Resterebbero l'intestazione di uno spazio e i dati di un altro, senza che nulla lo
            // corregga finché l'utente non esce dalla pagina e rientra.
            while (true)
            {
                // Catturato PRIMA del giro, ed è il punto delicato di tutto il metodo. Il confronto
                // finale si fa contro questo, non soltanto contro spazioMostrato: se il caricamento
                // fallisce, spazioMostrato non viene aggiornato: guardare solo quello lascerebbe la
                // condizione vera per sempre, e la pagina rifarebbe all'infinito una lettura che
                // fallisce sempre allo stesso modo. Confrontare lo spazio attivo di adesso con
                // quello di partenza distingue "è cambiato davvero" da "non è cambiato niente e la
                // lettura è andata male".
                var partenza = Spazi.Attivo?.Id;

                await CaricaUnGiro();

                var adesso = Spazi.Attivo?.Id;
                if (adesso is null || adesso == partenza || adesso == spazioMostrato) break;
            }
        }
        finally
        {
            caricato = true;
            inCaricamento = false;
        }
    }

    protected virtual async Task Riprova()
    {
        caricato = false;
        await Carica();
    }

    protected void SegnalaNonLetti(Exception ex)
    {
        errore = $"Spazio caricato, ma non ho potuto leggerne {NomePlurale}: {ex.Message}";
        datiNonLetti = true;
    }

    private async Task CaricaUnGiro()
    {
        errore = null;
        datiNonLetti = false;

        Space? attivo;
        try
        {
            attivo = await Spazi.AssicuraCaricatoAsync();
        }
        catch (Exception ex)
        {
            // Qui a mancare sono gli SPAZI, non i dati — e il markup reagisce diversamente:
            // senza spazio attivo l'errore si prende tutta la pagina, col pulsante Riprova,
            // invece di essere una riga sopra un elenco che comunque esiste.
            errore = $"Non è stato possibile caricare i tuoi spazi: {ex.Message}";
            return;
        }

        if (attivo is null) return;

        spazioMostrato = attivo.Id;

        await Leggi(attivo);
    }

    // Lo spazio attivo si cambia dalla Home, che resta montata sotto questa pagina solo in senso
    // logico: se cambia mentre siamo qui, i dati a schermo sono di un altro spazio.
    private void SuCambioSpazio() => _ = InvokeAsync(async () =>
    {
        if (inCaricamento) return;
        if (ScartaCambioSpazio()) return;
        if (Spazi.Attivo?.Id == spazioMostrato) return;

        PrimaDiRicaricare();
        caricato = false;
        StateHasChanged();
        try
        {
            await Carica();
        }
        catch (Exception ex)
        {
            // Carica() cattura già le proprie eccezioni, quindi qui si arriva solo per qualcosa che
            // sfugge a quel try. Senza questo blocco l'eccezione morirebbe dentro un Task che
            // nessuno osserva — il '_ =' rende esplicito che non lo osserva nessuno — e la pagina
            // resterebbe su "Caricamento…" per sempre, senza niente a schermo che lo spieghi.
            errore = $"Non è stato possibile aggiornare {NomePlurale}: {ex.Message}";
            caricato = true;
        }
        StateHasChanged();
    });

    public virtual void Dispose() => Spazi.Cambiato -= SuCambioSpazio;
}
