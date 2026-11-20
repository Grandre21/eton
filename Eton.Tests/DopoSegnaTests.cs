using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class DopoSegnaTests
{
    [Fact]
    public void Una_spesa_singola_arrivata_da_un_budget_torna_ai_budget()
        => Assert.True(DopoSegna.TornaAiBudget(true, TipoMovimento.Spesa, false));

    [Fact]
    public void Un_entrata_arrivata_da_un_budget_resta_nel_registro()
        => Assert.False(DopoSegna.TornaAiBudget(true, TipoMovimento.Entrata, false));

    [Fact]
    public void Un_piano_a_rate_arrivato_da_un_budget_resta_nel_registro()
        => Assert.False(DopoSegna.TornaAiBudget(true, TipoMovimento.Spesa, true));

    [Fact]
    public void Una_spesa_senza_budget_resta_nel_registro()
        => Assert.False(DopoSegna.TornaAiBudget(false, TipoMovimento.Spesa, false));
}
