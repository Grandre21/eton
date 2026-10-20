using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class PianiARateTests
{
    [Fact]
    public void ImportoRata_per_difetto()
    {
        Assert.Equal(33.33m, CalcoliRicorrenti.ImportoRata(100, 3));
        Assert.Equal(0.02m, CalcoliRicorrenti.ImportoRata(0.05m, 2));
        Assert.Equal(0m, CalcoliRicorrenti.ImportoRata(0.01m, 2));
    }

    [Fact]
    public void DatiPiano_compone_la_regola()
    {
        var inizio = new DateTime(2026, 11, 15);
        var dati = CalcoliRicorrenti.DatiPiano(100, 3, "Divano", "Casa", inizio);
        Assert.Equal(33.33m, dati.Importo);
        Assert.Equal(1, dati.OgniMesi);
        Assert.Equal(15, dati.Giorno);
        Assert.Equal(inizio, dati.Inizio);
        Assert.Equal(3, dati.Rate);
        Assert.Equal(100m, dati.TotalePiano);
        Assert.Equal(new DateTime(2027, 1, 15), CalcoliRicorrenti.FinePiano(inizio, 3));
        Assert.Equal(new DateTime(2027, 2, 28), CalcoliRicorrenti.FinePiano(new DateTime(2027, 1, 31), 2));
    }

    [Fact]
    public void Occorrenza_suffisso_e_resto_sull_ultima()
    {
        var r = Regola();
        var prima = CalcoliRicorrenti.Occorrenza(r, new PeriodoDovuto("2026-11", r.StartsOn));
        var ultima = CalcoliRicorrenti.Occorrenza(r, new PeriodoDovuto("2027-01", r.EndsOn!.Value));
        Assert.Equal("Divano · rata 1/3", prima.Description);
        Assert.Equal(33.33m, prima.Amount);
        Assert.Equal("Divano · rata 3/3", ultima.Description);
        Assert.Equal(33.34m, ultima.Amount);
        r.Installments = null;
        r.PlanTotal = null;
        var normale = CalcoliRicorrenti.Occorrenza(r, new PeriodoDovuto("2026-11", r.StartsOn));
        Assert.Equal(r.Description, normale.Description);
        Assert.Equal(r.Amount, normale.Amount);
    }

    [Fact]
    public void Materializzazione_e_previsione_coincidono()
    {
        var r = Regola();
        var occorrenze = CalcoliRicorrenti.Dovuti(r.StartsOn, r.EndsOn, r.EveryMonths, r.DayOfMonth,
            null, new DateTime(2026, 11, 1), new DateTime(2027, 3, 31))
            .Select(d => CalcoliRicorrenti.Occorrenza(r, d)).ToList();
        Assert.Equal(3, occorrenze.Count);
        Assert.Equal(100m, occorrenze.Sum(o => o.Amount));
        Assert.Equal(["Divano · rata 1/3", "Divano · rata 2/3", "Divano · rata 3/3"], occorrenze.Select(o => o.Description));
    }

    [Fact]
    public void Gruppo_a_rate_e_fuori_dalla_quota()
    {
        var oggi = new DateTime(2026, 10, 20);
        var piano = Regola();
        Assert.Equal(GruppoRicorrenti.ARate, CalcoliRicorrenti.Gruppo(piano, oggi));
        var terminato = Regola();
        terminato.EndsOn = oggi.AddDays(-1);
        Assert.Equal(GruppoRicorrenti.Terminate, CalcoliRicorrenti.Gruppo(terminato, oggi));
        var mensile = Regola();
        mensile.Installments = null;
        mensile.PlanTotal = null;
        var annuale = Regola();
        annuale.Installments = null;
        annuale.PlanTotal = null;
        annuale.EveryMonths = 12;
        annuale.Amount = 120;
        Assert.Equal([GruppoRicorrenti.Mensili, GruppoRicorrenti.ARate, GruppoRicorrenti.Annuali],
            CalcoliRicorrenti.Raggruppa([annuale, piano, mensile], oggi).Select(g => g.Gruppo));
        Assert.Equal(10m, CalcoliRicorrenti.QuotaDaAccantonare([piano, annuale], oggi));
    }

    [Fact]
    public void Stato_prossima_rata_e_residuo()
    {
        var r = Regola(new DateTime(2026, 10, 15));
        var stato = CalcoliRicorrenti.Stato(r, new DateTime(2026, 10, 20));
        Assert.NotNull(stato);
        Assert.Equal(2, stato.Prossima);
        Assert.Equal(3, stato.Rate);
        Assert.Equal(66.67m, stato.Residuo);
        stato = CalcoliRicorrenti.Stato(r, new DateTime(2026, 10, 10));
        Assert.NotNull(stato);
        Assert.Equal(1, stato.Prossima);
        Assert.Equal(100m, stato.Residuo);
        r.Installments = null;
        r.PlanTotal = null;
        Assert.Null(CalcoliRicorrenti.Stato(r, new DateTime(2026, 10, 10)));
    }

    [Fact]
    public void PianoPartito()
    {
        var oggi = new DateTime(2026, 10, 20);
        var r = Regola(oggi.AddDays(1));
        Assert.False(CalcoliRicorrenti.PianoPartito(r, oggi));
        r.StartsOn = oggi;
        Assert.True(CalcoliRicorrenti.PianoPartito(r, oggi));
        r.StartsOn = oggi.AddDays(1);
        r.MaterializedThrough = "2026-10";
        Assert.True(CalcoliRicorrenti.PianoPartito(r, oggi));
    }

    [Fact]
    public void Suffisso_entra_nei_200_caratteri()
    {
        var r = Regola();
        r.Description = new string('a', CalcoliRicorrenti.DescrizioneMassimaPiano);
        r.Installments = 60;
        r.Amount = 1m;
        r.PlanTotal = 60m;
        r.EndsOn = r.StartsOn.AddMonths(59);
        var ultima = CalcoliRicorrenti.Occorrenza(r,
            new PeriodoDovuto(CalcoliRicorrenti.Periodo(r.EndsOn.Value), r.EndsOn.Value));
        Assert.EndsWith(" · rata 60/60", ultima.Description);
        Assert.True(ultima.Description.Length <= 200);
    }

    private static RecurringExpense Regola(DateTime? inizio = null) => new()
    {
        Id = Guid.NewGuid(), Amount = 33.33m, Description = "Divano", PlanTotal = 100m,
        Installments = 3, DayOfMonth = 15, EveryMonths = 1,
        StartsOn = inizio ?? new DateTime(2026, 11, 15),
        EndsOn = (inizio ?? new DateTime(2026, 11, 15)).AddMonths(2)
    };
}
