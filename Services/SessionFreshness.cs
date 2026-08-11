namespace Eton.Services;

/// <summary>
/// Le due decisioni pure del rinnovo di sessione, isolate per essere verificabili senza un client
/// Gotrue: quando rinfrescare (<see cref="VaRinfrescata"/>) e se ha senso ritentare dopo un
/// fallimento (<see cref="SiPuoRitentare"/>).
/// Si rinfresca PRIMA della scadenza, non dopo: un token che scade a metà di una richiesta già
/// partita produce un 403 che l'utente vede come "errore" senza capirne il motivo.
/// </summary>
public static class SessionFreshness
{
    /// <summary>Margine di sicurezza: si rinfresca prima della scadenza vera, così una richiesta
    /// partita subito dopo il controllo non si trova col token morto a metà strada.</summary>
    public static readonly TimeSpan Margine = TimeSpan.FromMinutes(5);

    /// <summary>Intervallo minimo fra due tentativi falliti, per non martellare il server quando
    /// la rete è giù: senza, ogni chiamata dati riproverebbe subito.</summary>
    public static readonly TimeSpan AttesaDopoFallimento = TimeSpan.FromSeconds(30);

    /// <summary>True se la sessione che scade a <paramref name="scadenzaUtc"/> va rinfrescata ora.</summary>
    public static bool VaRinfrescata(DateTime scadenzaUtc, DateTime adessoUtc)
        => adessoUtc + Margine >= scadenzaUtc;

    /// <summary>True se è passato abbastanza tempo dall'ultimo tentativo fallito.
    /// <paramref name="ultimoFallimentoUtc"/> null = nessun tentativo fallito finora.</summary>
    public static bool SiPuoRitentare(DateTime? ultimoFallimentoUtc, DateTime adessoUtc)
        => ultimoFallimentoUtc is null || adessoUtc - ultimoFallimentoUtc.Value >= AttesaDopoFallimento;

    /// <summary>Una sessione mai valida: qualunque confronto la dà per scaduta. È il valore di
    /// ripiego di <see cref="ScadenzaUtc"/> quando i dati non hanno senso.</summary>
    private static readonly DateTime Scaduta = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

    /// <summary>Massimo che i server Gotrue accettano per <c>expires_in</c>: una settimana.
    /// Oltre, il dato non viene dal server.</summary>
    private const long DurataMassimaSecondi = 604_800;

    /// <summary>Scadenza di una sessione Gotrue, che espone solo l'istante di creazione e una durata
    /// in secondi: <c>Session.ExpiresAt()</c> non esiste in Supabase.Gotrue 6.
    /// Il confronto che ne segue è immune allo scarto d'orologio col server, perché
    /// <c>CreatedAt</c> è timbrato dal client alla ricezione del token: si confronta sempre e solo
    /// l'orologio locale con se stesso.
    /// <para>
    /// <b>Non lancia per nessun ingresso.</b> I valori arrivano da <c>localStorage</c>, che l'utente
    /// può manomettere e che una versione futura della libreria può scrivere in un formato diverso;
    /// questo metodo viene invocato durante il bootstrap, dove un'eccezione murerebbe l'app.
    /// Davanti a un dato che non ha senso risponde <see cref="Scaduta"/>, mai un'ipotesi
    /// ottimistica: sbagliare per eccesso di prudenza costa un rinnovo inutile, sbagliare per
    /// difetto costa richieste rifiutate dal gateway che l'utente legge come "i dati non si
    /// caricano".
    /// </para></summary>
    public static DateTime ScadenzaUtc(DateTime creataUtc, long duraSecondi)
    {
        if (duraSecondi <= 0 || duraSecondi > DurataMassimaSecondi)
            return Scaduta;

        var creata = DateTime.SpecifyKind(creataUtc, DateTimeKind.Utc);

        // Il confronto va fatto PRIMA della somma: verificare l'esito dopo significherebbe averla
        // già eseguita, cioè aver già lanciato.
        if (creata > DateTime.MaxValue.AddSeconds(-DurataMassimaSecondi))
            return Scaduta;

        return creata.AddSeconds(duraSecondi);
    }
}
