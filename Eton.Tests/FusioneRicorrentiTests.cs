using System.Reflection;
using Eton.Models;
using Eton.Services;
using Supabase.Postgrest.Attributes;

namespace Eton.Tests;

public class FusioneRicorrentiTests
{
    private static RecurringExpense Regola(
        decimal amount = 50m, int day = 5, DateTime? startsOn = null,
        string? materializedThrough = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            SpaceId = Guid.NewGuid(),
            PaidBy = Guid.NewGuid(),
            Amount = amount,
            Description = "Affitto",
            Category = "Casa",
            EveryMonths = 1,
            DayOfMonth = day,
            StartsOn = startsOn ?? new DateTime(2026, 6, 5),
            EndsOn = null,
            MaterializedThrough = materializedThrough
        };

    [Fact]
    public void La_riga_vera_vince_sulla_previsione()
    {
        var regola = Regola(amount: 50m, day: 5, startsOn: new DateTime(2026, 6, 5));
        var vera = new Expense
        {
            Id = Guid.NewGuid(),
            SpaceId = regola.SpaceId,
            PaidBy = regola.PaidBy,
            Amount = 62m,
            Description = "Affitto (corretto)",
            Category = "Casa",
            SpentOn = new DateTime(2026, 9, 5),
            RecurringId = regola.Id,
            RecurringPeriod = new DateTime(2026, 9, 1)
        };

        var risultato = CalcoliRicorrenti.Fondi([vera], [regola],
            da: new DateTime(2026, 9, 1), a: new DateTime(2026, 9, 30), oggi: new DateTime(2026, 9, 20));

        Assert.Single(risultato.Righe);
        Assert.Equal(62m, risultato.Righe[0].Amount);
        Assert.Empty(risultato.Previste);
        Assert.Equal(62m, CalcoliSpese.PerMese(risultato.Righe, 2026, 9).Totale);
    }

    [Fact]
    public void La_prevista_scaduta_entra_nel_totale()
    {
        var regola = Regola(amount: 40m, day: 5, startsOn: new DateTime(2026, 6, 5));

        var risultato = CalcoliRicorrenti.Fondi([], [regola],
            da: new DateTime(2026, 9, 1), a: new DateTime(2026, 9, 30), oggi: new DateTime(2026, 9, 20));

        Assert.Single(risultato.Righe);
        var riga = risultato.Righe[0];
        Assert.Contains(riga.Id, risultato.Previste);
        Assert.Equal(new DateTime(2026, 9, 5), riga.SpentOn);
        Assert.Equal(new DateTime(2026, 9, 1), riga.RecurringPeriod);
        Assert.Equal(regola.PaidBy, riga.PaidBy);
        Assert.Equal(40m, CalcoliSpese.PerMese(risultato.Righe, 2026, 9).Totale);
    }

    [Fact]
    public void La_futura_va_in_arrivo_e_non_nel_totale()
    {
        var regola = Regola(amount: 40m, day: 25, startsOn: new DateTime(2026, 6, 25));

        var risultato = CalcoliRicorrenti.Fondi([], [regola],
            da: new DateTime(2026, 9, 1), a: new DateTime(2026, 9, 30), oggi: new DateTime(2026, 9, 20));

        Assert.Empty(risultato.Righe);
        Assert.Single(risultato.InArrivo);
        Assert.Equal(0m, CalcoliSpese.PerMese(risultato.Righe, 2026, 9).Totale);
    }

    [Fact]
    public void L_occorrenza_di_oggi_e_gia_scaduta()
    {
        var regola = Regola(amount: 40m, day: 20, startsOn: new DateTime(2026, 6, 20));

        var risultato = CalcoliRicorrenti.Fondi([], [regola],
            da: new DateTime(2026, 9, 1), a: new DateTime(2026, 9, 30), oggi: new DateTime(2026, 9, 20));

        Assert.Single(risultato.Righe);
        Assert.Contains(risultato.Righe[0].Id, risultato.Previste);
        Assert.Empty(risultato.InArrivo);
    }

    [Fact]
    public void Il_buco_dietro_il_watermark_resta_vuoto()
    {
        var regola = Regola(amount: 40m, day: 5, startsOn: new DateTime(2026, 6, 5), materializedThrough: "2026-09");

        var risultato = CalcoliRicorrenti.Fondi([], [regola],
            da: new DateTime(2026, 9, 1), a: new DateTime(2026, 9, 30), oggi: new DateTime(2026, 9, 20));

        Assert.Empty(risultato.Righe);
        Assert.Empty(risultato.InArrivo);
    }

    [Fact]
    public void Le_righe_vere_restano_e_si_ordinano_dalla_piu_recente()
    {
        var regola = Regola(amount: 40m, day: 10, startsOn: new DateTime(2026, 6, 10));
        var e3 = new Expense { Id = Guid.NewGuid(), SpaceId = regola.SpaceId, PaidBy = regola.PaidBy, Amount = 10m, Description = "Spesa", Category = "Varie", SpentOn = new DateTime(2026, 9, 3) };
        var e28 = new Expense { Id = Guid.NewGuid(), SpaceId = regola.SpaceId, PaidBy = regola.PaidBy, Amount = 20m, Description = "Spesa", Category = "Varie", SpentOn = new DateTime(2026, 9, 28) };

        var risultato = CalcoliRicorrenti.Fondi([e28, e3], [regola],
            da: new DateTime(2026, 9, 1), a: new DateTime(2026, 9, 30), oggi: new DateTime(2026, 9, 20));

        Assert.Equal([new DateTime(2026, 9, 28), new DateTime(2026, 9, 10), new DateTime(2026, 9, 3)],
            risultato.Righe.Select(r => r.SpentOn));
    }

    // In Postgrest 4.4.0 l'upsert scarta anche le colonne IgnoreOnUpdate; PrivilegiInsertTests
    // controlla le colonne in più rispetto ai grant, non quelle mancanti — questo test chiude
    // quel lato.
    [Fact]
    public void Il_modello_di_scrittura_non_omette_nessuna_colonna()
    {
        var tipo = typeof(OccorrenzaRicorrente);
        var nomiColonna = new List<string>();

        foreach (var proprieta in tipo.GetProperties())
        {
            var colonna = proprieta.GetCustomAttribute<ColumnAttribute>();
            if (colonna is not null)
            {
                Assert.False(colonna.IgnoreOnInsert, $"{proprieta.Name} ha IgnoreOnInsert");
                Assert.False(colonna.IgnoreOnUpdate, $"{proprieta.Name} ha IgnoreOnUpdate");
                nomiColonna.Add(colonna.ColumnName);
            }

            var chiave = proprieta.GetCustomAttribute<PrimaryKeyAttribute>();
            if (chiave is not null)
            {
                Assert.True(chiave.ShouldInsert);
                nomiColonna.Add(chiave.ColumnName);
            }
        }

        Assert.Equal(
            new[] { "amount", "category", "description", "id", "paid_by", "recurring_id", "recurring_period", "space_id", "spent_on" },
            nomiColonna.OrderBy(n => n, StringComparer.Ordinal));
    }
}
