using System.Globalization;

namespace Eton.Services;

/// <summary>
/// I testi che compaiono identici in più pagine: conteggi accordati al singolare e al plurale, date.
/// <para>
/// Sta qui e non nelle pagine per lo stesso motivo di <see cref="CalcoliVoti.TestoVoti"/>: la stessa
/// frase serve alla testata di ogni registro dell'applicazione — collezioni, note, spazi, elementi di
/// una collezione — e quattro copie divergono al primo che ne ritocca una. Il difetto che ne segue è
/// dei peggiori da trovare: il singolare sbagliato compare in una sola schermata, e chi lo vede non
/// ha modo di sapere che altrove la stessa riga è scritta bene.
/// </para>
/// <para>
/// Nessun accesso a rete o a stato, come <see cref="CalcoliVoti"/>, <see cref="ValoriElemento"/> e
/// <see cref="SchemaCampi"/>: solo funzioni pure su ciò che riceve.
/// </para>
/// </summary>
public static class Testi
{
    /// <summary>Un conteggio accordato: «0 note», «1 nota», «5 note».
    /// <para>
    /// Singolare e plurale arrivano da fuori invece di essere dedotti dalla parola, e non per pigrizia:
    /// in italiano «spazio»/«spazi» e «collezione»/«collezioni» seguono schemi diversi, e una regola
    /// che indovina la desinenza funziona finché non arriva la parola che non ci sta — a quel punto
    /// sbaglia in silenzio, in un punto qualunque dell'interfaccia.
    /// </para>
    /// <para>
    /// Lo zero va al plurale, che in italiano è la forma giusta («0 note», non «0 nota»). Dove lo zero
    /// merita una frase invece di una cifra — «nessun voto» — serve un metodo suo, e infatti c'è:
    /// v. <see cref="CalcoliVoti.TestoVoti"/>.
    /// </para>
    /// </summary>
    public static string Conteggio(int quanti, string singolare, string plurale)
        => quanti == 1 ? $"1 {singolare}" : $"{quanti} {plurale}";

    /// <summary>Una data leggibile: «03/08/2026».
    /// <para>
    /// <c>InvariantCulture</c> come in <see cref="CalcoliVoti.Testo"/> e in
    /// <see cref="ValoriElemento"/>: con <c>InvariantGlobalization</c> attivo nel csproj non esiste
    /// nessun'altra cultura da poter impostare, e lasciare la cultura corrente darebbe formati
    /// diversi a seconda di come è avviato il runtime.
    /// </para>
    /// <para>
    /// La conversione al fuso locale è dentro, non a carico di chi chiama: le date arrivano da
    /// Postgres in UTC, e la sola volta che qualcuno se ne dimentica l'orario risulta sbagliato di
    /// un paio d'ore in una schermata sola.
    /// </para>
    /// <para>
    /// Solo per colonne <c>timestamptz</c> — un istante nel tempo (<c>created_at</c>,
    /// <c>updated_at</c>). Su una colonna <c>date</c> pura come <c>expenses.spent_on</c> non va usato:
    /// serve <see cref="DataSola"/>, e il motivo è nella sua documentazione.
    /// </para>
    /// </summary>
    public static string Data(DateTime quando)
        => quando.ToLocalTime().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    /// <summary>Una data con l'ora: «03/08/2026 14:05».
    /// <para>
    /// Serve dove due modifiche dello stesso giorno sono la norma — le note — e la sola data non
    /// basterebbe a dire quale sia l'ultima. Altrove si usa <see cref="Data"/>: l'ora di creazione di
    /// una collezione non la guarda nessuno, e allunga la riga per niente.
    /// </para>
    /// </summary>
    public static string DataOra(DateTime quando)
        => quando.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Una data pura, senza fuso: «05/01/2026». Per colonne <c>date</c> come
    /// <c>expenses.spent_on</c> — non per <c>timestamptz</c>, dove serve <see cref="Data"/>.
    /// <para>
    /// Una colonna <c>date</c> di Postgres non è un istante: è un giorno, senza ora e senza fuso.
    /// Applicarle <c>.ToLocalTime()</c> — come fa <see cref="Data"/>, giustamente, sui
    /// <c>timestamptz</c> — significa spostare qualcosa che non ha una posizione nel tempo da cui
    /// spostarsi.
    /// </para>
    /// <para>
    /// Il motivo per cui questo metodo esiste, e non basta "semplicemente non chiamare
    /// <c>.ToLocalTime()</c>" all'occorrenza: verificato contro <c>Supabase.Postgrest 4.4.0</c>, una
    /// colonna <c>date</c> arriva deserializzata come <see cref="DateTime"/> con
    /// <see cref="DateTimeKind.Unspecified"/>. E <c>.ToLocalTime()</c> su <c>Unspecified</c> non è
    /// un'operazione neutra: per specifica .NET tratta il valore <b>come se fosse UTC</b> e applica
    /// l'offset locale — l'assunto contrario a quello che verrebbe naturale, ed è da lì che nasce il
    /// difetto. In Blazor WebAssembly quell'offset è quello del dispositivo di chi guarda, non del
    /// server: per un fuso negativo su UTC (<c>America/New_York</c>, UTC-5) il 5 gennaio a
    /// mezzanotte si stampava «04/01/2026», un giorno indietro, senza nessun errore visibile. In
    /// Italia non si vedeva mai, perché l'offset è positivo e lo spostamento restava nello stesso
    /// giorno.
    /// </para>
    /// </summary>
    public static string DataSola(DateTime quando)
        => quando.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
