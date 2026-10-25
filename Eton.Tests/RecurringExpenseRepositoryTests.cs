using System.Reflection;
using Eton.Models;
using Eton.Services;
using Supabase.Postgrest.Attributes;

namespace Eton.Tests;

// Creazione, esiti di concorrenza e colonne scrivibili si provano senza database:
// i delegate isolano la rilettura, mentre la reflection controlla cosa il modello può inviare.
public class RecurringExpenseRepositoryTests
{
    private static readonly Guid Spazio = Guid.NewGuid();
    private static readonly Guid Pagante = Guid.NewGuid();
    private static readonly DateTime Inizio = new(2026, 1, 31, 15, 20, 0, DateTimeKind.Unspecified);
    private static readonly DatiRegola Dati = new(120m, "  Stipendio  ", " Stipendio ", 2, 31, Inizio, Kind: TipoMovimento.Entrata);

    [Fact]
    public void NuovaRegola_copia_i_dati_rifila_i_testi_e_porta_la_data_a_mezzanotte_UTC()
    {
        var regola = RecurringExpenseRepository.NuovaRegola(Spazio, Pagante, Dati);

        Assert.Equal("Stipendio", regola.Description);
        Assert.Equal("Stipendio", regola.Category);
        Assert.Equal(new DateTime(2026, 1, 31), regola.StartsOn);
        Assert.Equal(TimeSpan.Zero, regola.StartsOn.TimeOfDay);
        Assert.Equal(DateTimeKind.Utc, regola.StartsOn.Kind);
        Assert.Null(regola.EndsOn);
        Assert.Null(regola.Installments);
        Assert.Null(regola.PlanTotal);
        Assert.Equal(TipoMovimento.Entrata, regola.Kind);
        Assert.Null(regola.MaterializedThrough);
        Assert.Equal(Spazio, regola.SpaceId);
        Assert.Equal(Pagante, regola.PaidBy);
        Assert.Equal(120m, regola.Amount);
        Assert.Equal(2, regola.EveryMonths);
        Assert.Equal(31, regola.DayOfMonth);
    }

    [Fact]
    public void NuovaRegola_con_rate_calcola_la_fine_del_piano_in_UTC()
    {
        var piano = RecurringExpenseRepository.NuovaRegola(Spazio, Pagante, Dati with { Rate = 3, TotalePiano = 360m });

        Assert.Equal(CalcoliRicorrenti.FinePiano(Inizio, 3), piano.EndsOn);
        Assert.Equal(DateTimeKind.Utc, piano.EndsOn!.Value.Kind);
        Assert.Equal(TimeSpan.Zero, piano.EndsOn.Value.TimeOfDay);
        Assert.Equal(3, piano.Installments);
        Assert.Equal(360m, piano.PlanTotal);
    }

    [Fact]
    public void NuovaRegola_genera_un_id_nuovo_a_ogni_chiamata()
    {
        var prima = RecurringExpenseRepository.NuovaRegola(Spazio, Pagante, Dati);
        var seconda = RecurringExpenseRepository.NuovaRegola(Spazio, Pagante, Dati);

        Assert.NotEqual(Guid.Empty, prima.Id);
        Assert.NotEqual(Guid.Empty, seconda.Id);
        Assert.NotEqual(prima.Id, seconda.Id);
    }

    [Fact]
    public async Task Esito_con_righe_toccate_non_rilegge()
    {
        var riga = new RecurringExpense();
        var chiamate = 0;
        var esito = await RecurringExpenseRepository.Esito([riga], 4, () =>
        {
            chiamate++;
            return Task.FromResult<RecurringExpense?>(null);
        });
        Assert.Equal(EsitoSalvataggio.Salvata, esito.Esito);
        Assert.Same(riga, esito.Aggiornata);
        Assert.Equal(0, chiamate);
    }

    [Fact]
    public async Task Esito_senza_riga_riletta_segnala_sparita()
    {
        var esito = await RecurringExpenseRepository.Esito([], 4, () => Task.FromResult<RecurringExpense?>(null));
        Assert.Equal(EsitoSalvataggio.Sparita, esito.Esito);
        Assert.Null(esito.Aggiornata);
    }

    [Fact]
    public async Task Esito_con_versione_diversa_segnala_conflitto()
    {
        var riga = new RecurringExpense { Version = 5 };
        var esito = await RecurringExpenseRepository.Esito([], 4, () => Task.FromResult<RecurringExpense?>(riga));
        Assert.Equal(EsitoSalvataggio.Conflitto, esito.Esito);
        Assert.Same(riga, esito.Aggiornata);
    }

    [Fact]
    public async Task Esito_con_versione_uguale_segnala_rifiutata()
    {
        var riga = new RecurringExpense { Version = 4 };
        var esito = await RecurringExpenseRepository.Esito([], 4, () => Task.FromResult<RecurringExpense?>(riga));
        Assert.Equal(EsitoSalvataggio.Rifiutata, esito.Esito);
        Assert.Same(riga, esito.Aggiornata);
    }

    // kind è già coperto da PrivilegiInsertTests.Il_tipo_si_scrive_solo_in_creazione.
    [Fact]
    public void Le_colonne_protette_non_si_riscrivono()
    {
        var colonne = typeof(RecurringExpense).GetProperties()
            .Select(p => p.GetCustomAttribute<ColumnAttribute>())
            .Where(c => c is not null).ToDictionary(c => c!.ColumnName, c => c!);
        foreach (var nome in new[] { "space_id", "paid_by" })
            Assert.True(colonne[nome].IgnoreOnUpdate);
        foreach (var nome in new[] { "version", "created_at", "updated_at" })
        {
            Assert.True(colonne[nome].IgnoreOnInsert);
            Assert.True(colonne[nome].IgnoreOnUpdate);
        }
    }
}
