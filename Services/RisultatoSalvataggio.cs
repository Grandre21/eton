namespace Eton.Services;

/// <summary>Come è finito un salvataggio. Non è un booleano perché i tre modi di fallire
/// vogliono tre rimedi diversi, e confonderli produrrebbe messaggi bugiardi.</summary>
public enum EsitoSalvataggio
{
    /// <summary>Scritta.</summary>
    Salvata,
    /// <summary>Qualcun altro ha salvato dopo che tu avevi aperto la riga. La versione sua è in
    /// <c>Aggiornata</c>: si chiede a chi scrive se ricaricare o sovrascrivere.</summary>
    Conflitto,
    /// <summary>La RLS ha detto di no: non hai il diritto di scrivere questa riga.
    /// Riprovare non serve a niente.</summary>
    Rifiutata,
    /// <summary>La riga non c'è più: cancellata da qualcun altro, o non sei più nello spazio.</summary>
    Sparita
}

/// <summary>L'esito di un salvataggio con concorrenza ottimistica, e la riga come sta ADESSO sul
/// server — che serve sia per riallineare dopo un salvataggio riuscito, sia per mostrare la
/// versione altrui quando c'è un conflitto.
/// <para>
/// Generico perché la logica è identica per ogni tabella con un contatore di versione: cambia solo
/// il tipo della riga. Note, collezioni ed elementi la usano già; le recensioni la useranno.
/// </para>
/// </summary>
public sealed record RisultatoSalvataggio<T>(EsitoSalvataggio Esito, T? Aggiornata) where T : class;
