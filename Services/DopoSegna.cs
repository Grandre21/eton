using Eton.Models;

namespace Eton.Services;

/// <summary>
/// Dove porta il «Segna» del Registro. Si torna ai budget solo per una spesa singola arrivata da
/// «Registra una spesa» di un budget: un'entrata non consuma un budget, e un piano a rate resta
/// nel Registro per mostrare l'avviso «Piano a rate creato» con il collegamento alle ricorrenti.
/// </summary>
public static class DopoSegna
{
    public static bool TornaAiBudget(bool daBudget, string tipo, bool aRate)
        => daBudget && tipo == TipoMovimento.Spesa && !aRate;
}
