using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class GruppiRicorrentiTests
{
    private static readonly DateTime Oggi = new(2026, 10, 20);

    [Fact]
    public void Le_entrate_hanno_il_loro_gruppo_e_stanno_fuori_dalla_quota()
    {
        var mensile = Regola(tipo: TipoMovimento.Entrata);
        var annuale = Regola(importo: 1200, mesi: 12, tipo: TipoMovimento.Entrata);
        var terminata = Regola(fine: Oggi.AddMonths(-1), tipo: TipoMovimento.Entrata);
        Assert.Equal(GruppoRicorrenti.Entrate, CalcoliRicorrenti.Gruppo(mensile, Oggi));
        Assert.Equal(GruppoRicorrenti.Entrate, CalcoliRicorrenti.Gruppo(annuale, Oggi));
        Assert.Equal(GruppoRicorrenti.Terminate, CalcoliRicorrenti.Gruppo(terminata, Oggi));
        Assert.Equal(GruppoRicorrenti.Entrate, CalcoliRicorrenti.Raggruppa([Regola(), annuale, mensile, terminata], Oggi)[0].Gruppo);
        Assert.Equal(10m, CalcoliRicorrenti.QuotaDaAccantonare([Regola(importo: 120, mesi: 12), annuale], Oggi));
    }

    [Fact]
    public void Classifica_per_cadenza_e_prossima_occorrenza()
    {
        Assert.Equal(GruppoRicorrenti.Mensili, CalcoliRicorrenti.Gruppo(Regola(), Oggi));
        Assert.Equal(GruppoRicorrenti.Annuali, CalcoliRicorrenti.Gruppo(Regola(mesi: 12), Oggi));
        Assert.Equal(GruppoRicorrenti.OgniNMesi, CalcoliRicorrenti.Gruppo(Regola(mesi: 3), Oggi));
        Assert.Equal(GruppoRicorrenti.Terminate, CalcoliRicorrenti.Gruppo(Regola(fine: Oggi.AddMonths(-1)), Oggi));
        Assert.Equal(GruppoRicorrenti.Mensili, CalcoliRicorrenti.Gruppo(Regola(fine: Oggi.AddMonths(2)), Oggi));
    }

    [Fact]
    public void Ordina_i_gruppi_e_omette_quelli_vuoti()
    {
        var gruppi = CalcoliRicorrenti.Raggruppa([
            Regola(fine: Oggi.AddMonths(-1)), Regola(mesi: 3), Regola(mesi: 12), Regola()], Oggi);
        Assert.Equal([GruppoRicorrenti.Mensili, GruppoRicorrenti.Annuali,
            GruppoRicorrenti.OgniNMesi, GruppoRicorrenti.Terminate], gruppi.Select(g => g.Gruppo));
        Assert.Equal(GruppoRicorrenti.Mensili, Assert.Single(CalcoliRicorrenti.Raggruppa([Regola()], Oggi)).Gruppo);
        Assert.Empty(CalcoliRicorrenti.Raggruppa([], Oggi));
    }

    [Fact]
    public void Ordina_descrizioni_senza_accenti_e_maiuscole_poi_per_id()
    {
        var prima = Regola(descrizione: "Ècole", id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var seconda = Regola(descrizione: "ecole", id: Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var gruppo = Assert.Single(CalcoliRicorrenti.Raggruppa([
            seconda, Regola(descrizione: "bolletta"), prima, Regola(descrizione: "Affitto")], Oggi));
        Assert.Equal(["Affitto", "bolletta", "Ècole", "ecole"], gruppo.Regole.Select(r => r.Description));
        Assert.Equal(prima.Id, gruppo.Regole[2].Id);
    }

    [Fact]
    public void Totali_mensili_arrotondati_e_terminate_escluse()
    {
        var gruppi = CalcoliRicorrenti.Raggruppa([
            Regola(importo: 120, mesi: 12), Regola(importo: 30, mesi: 3),
            Regola(importo: 50, mesi: 2), Regola(fine: Oggi.AddMonths(-1))], Oggi);
        Assert.Equal(10m, gruppi[0].TotaleMensile);
        Assert.Equal(35m, gruppi[1].TotaleMensile);
        Assert.Null(gruppi[2].TotaleMensile);
        Assert.Equal(8.33m, Assert.Single(CalcoliRicorrenti.Raggruppa([Regola(importo: 100, mesi: 12)], Oggi)).TotaleMensile);
        Assert.Equal(0.01m, Assert.Single(CalcoliRicorrenti.Raggruppa([Regola(importo: 0.01m, mesi: 2)], Oggi)).TotaleMensile);
        Assert.Equal(16.67m, Assert.Single(CalcoliRicorrenti.Raggruppa([
            Regola(importo: 100, mesi: 12), Regola(importo: 100, mesi: 12)], Oggi)).TotaleMensile);
    }

    [Fact]
    public void Quota_include_annuali_periodiche_e_future()
    {
        Assert.Equal(40m, CalcoliRicorrenti.QuotaDaAccantonare([
            Regola(importo: 120, mesi: 12), Regola(importo: 30, mesi: 3), Regola(importo: 500),
            Regola(importo: 1200, mesi: 12, fine: Oggi.AddMonths(-1)),
            Regola(importo: 240, mesi: 12, inizio: new DateTime(2026, 11, 1))], Oggi));
        Assert.Equal(0.01m, CalcoliRicorrenti.QuotaDaAccantonare([Regola(importo: 0.01m, mesi: 2)], Oggi));
    }

    [Fact]
    public void Quota_con_sole_mensili_e_zero()
    {
        Assert.Equal(0m, CalcoliRicorrenti.QuotaDaAccantonare([Regola(importo: 500)], Oggi));
    }

    [Fact]
    public void Quota_somma_i_totali_di_gruppo_arrotondati()
    {
        Assert.Equal(41.66m, CalcoliRicorrenti.QuotaDaAccantonare([
            Regola(importo: 100, mesi: 12), Regola(importo: 100, mesi: 3)], Oggi));
    }

    private static RecurringExpense Regola(decimal importo = 10, int mesi = 1,
        string descrizione = "Regola", DateTime? fine = null, DateTime? inizio = null, Guid? id = null, string tipo = TipoMovimento.Spesa) => new()
    {
        Id = id ?? Guid.NewGuid(), Amount = importo, Description = descrizione,
        Kind = tipo,
        EveryMonths = mesi, DayOfMonth = 1, StartsOn = inizio ?? new DateTime(2026, 1, 1), EndsOn = fine
    };
}
