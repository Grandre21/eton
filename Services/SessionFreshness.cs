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

    /// <summary>Scadenza di una sessione Gotrue, che espone solo l'istante di creazione e una durata
    /// in secondi: <c>Session.ExpiresAt()</c> non esiste in Supabase.Gotrue 6.
    /// Il confronto che ne segue è immune allo scarto d'orologio col server, perché
    /// <c>CreatedAt</c> è timbrato dal client alla ricezione del token: si confronta sempre e solo
    /// l'orologio locale con se stesso.</summary>
    public static DateTime ScadenzaUtc(DateTime creataUtc, long duraSecondi)
    {
        // Una durata assurda arriverebbe da un localStorage manomesso o corrotto, non dal server:
        // AddSeconds lancerebbe, e un'eccezione qui fermerebbe l'avvio dell'app invece di
        // provocare — correttamente — un semplice rinnovo. Zero e i negativi cadono nel passato,
        // che è la direzione sicura: si rinfresca.
        var durata = Math.Clamp(duraSecondi, 0, (long)TimeSpan.FromDays(365).TotalSeconds);
        return DateTime.SpecifyKind(creataUtc, DateTimeKind.Utc).AddSeconds(durata);
    }
}
