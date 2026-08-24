using Eton.Services;

namespace Eton.Tests;

// ExpenseRepository.PerIlDatabase nasce per correggere un difetto di corruzione dati osservato in
// produzione. Una spesa creata con data 25/08/2026 si salvava giusta; riaperta in modifica
// cambiando SOLO l'importo, la data tornava indietro di un giorno nel database (25/08 -> 24/08),
// persistente dopo il ricaricamento.
//
// La causa: SalvaAsync passa il valore a .Set(), che finisce in un Dictionary<object, object?> e
// non nel modello. Su quel percorso Newtonsoft NON aggancia il DateTimeConverter della libreria
// (quello attraversato solo da CreaAsync via Insert, che infatti non ha mai mostrato il difetto):
// il DateTime viene serializzato dal convertitore GLOBALE, un IsoDateTimeConverter configurato con
// AdjustToUniversal, la cui WriteJson chiama .ToUniversalTime(). Su un DateTime con
// Kind = Unspecified quella chiamata lo tratta come se fosse Local e sottrae il fuso: in Italia in
// agosto (UTC+2) mezzanotte del 25 diventa le 22 del 24 in UTC, e la colonna 'date' prende il
// giorno sbagliato.
//
// Nessun test esistente lo vedeva perché nessuno di essi tocca la serializzazione verso PostgREST:
// DenaroTests e TestiTests coprono parsing e formattazione lato interfaccia, non cosa esce dal
// repository verso il database.
//
// PerIlDatabase ora si usa su ENTRAMBI i percorsi, non solo quello che rompeva. SalvaAsync la
// chiamava già per il motivo sopra; CreaAsync la chiama anche lei, pur producendo già oggi la data
// giusta su .Insert() per una ragione che non sta nel nostro codice (il DateTimeConverter della
// libreria, agganciato solo lì da PostgrestContractResolver.CreateProperty, non converte). I test
// sotto restano su PerIlDatabase in isolamento perché il comportamento è identico ovunque venga
// chiamata: non serve un test separato per CreaAsync, ma vale sapere che la regola qui verificata
// copre entrambi i chiamanti, non un caso particolare.
public class ExpenseRepositoryTests
{
    [Fact]
    public void Una_data_qualunque_arriva_a_mezzanotte_UTC()
    {
        var data = new DateTime(2026, 8, 25, 14, 30, 0, DateTimeKind.Local);
        var risultato = ExpenseRepository.PerIlDatabase(data);

        Assert.Equal(new DateTime(2026, 8, 25), risultato.Date);
        Assert.Equal(DateTimeKind.Utc, risultato.Kind);
        Assert.Equal(TimeSpan.Zero, risultato.TimeOfDay);
    }

    // Il caso che rompeva davvero: una mezzanotte con Kind = Unspecified, esattamente come arriva
    // da un DateTime costruito senza specificare il fuso (il caso comune nel form di modifica).
    // Prima della correzione, .Set(e => e.SpentOn, data) lasciava questo valore intatto fino al
    // convertitore globale, che con .ToUniversalTime() lo spostava al giorno prima.
    [Fact]
    public void La_mezzanotte_con_kind_non_specificato_non_arretra_di_un_giorno()
    {
        var data = new DateTime(2026, 8, 25, 0, 0, 0, DateTimeKind.Unspecified);
        var risultato = ExpenseRepository.PerIlDatabase(data);

        Assert.Equal(new DateTime(2026, 8, 25), risultato.Date);

        // La controprova del fix: .ToUniversalTime() è esattamente ciò che IsoDateTimeConverter
        // chiama in WriteJson. Su un valore già Kind = Utc è un'operazione nulla per specifica
        // .NET, quindi applicarla qui non deve spostare la data — è la stessa identica chiamata
        // che, sul valore NON corretto (Kind = Unspecified), causava il difetto.
        Assert.Equal(risultato, risultato.ToUniversalTime());
    }

    // Il cuore della regressione: il risultato non deve dipendere dal Kind del valore ricevuto.
    // Il difetto nasceva proprio perché il codice a valle (il convertitore globale) trattava
    // Kind = Unspecified come se fosse Local; PerIlDatabase invece rietichetta SENZA convertire
    // (DateTime.SpecifyKind, non ToUniversalTime/ToLocalTime), quindi i tre Kind devono coincidere.
    [Fact]
    public void Il_kind_del_valore_ricevuto_non_cambia_il_risultato()
    {
        var atteso = ExpenseRepository.PerIlDatabase(new DateTime(2026, 8, 25, 0, 0, 0, DateTimeKind.Unspecified));

        Assert.Equal(atteso, ExpenseRepository.PerIlDatabase(new DateTime(2026, 8, 25, 0, 0, 0, DateTimeKind.Local)));
        Assert.Equal(atteso, ExpenseRepository.PerIlDatabase(new DateTime(2026, 8, 25, 0, 0, 0, DateTimeKind.Utc)));
    }

    // Giorno e mese a una cifra: qui non c'è una stringa da zero-riempire (il valore resta un
    // DateTime, non "yyyy-MM-dd"), ma il caso resta utile come prova che Year/Month/Day non
    // vengono alterati per valori di calendario "piccoli" — un eventuale bug di arrotondamento
    // sul .Date o sullo SpecifyKind si vedrebbe proprio qui.
    [Fact]
    public void Giorno_e_mese_a_una_cifra_restano_quelli_scritti()
    {
        var risultato = ExpenseRepository.PerIlDatabase(new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Local));

        Assert.Equal(2026, risultato.Year);
        Assert.Equal(1, risultato.Month);
        Assert.Equal(5, risultato.Day);
    }
}
