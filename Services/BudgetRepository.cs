using Eton.Models;
using Supabase.Postgrest;
using Supabase.Postgrest.Responses;

namespace Eton.Services;

/// <summary>Come RecurringExpenseRepository: client ripreso a ogni metodo; l'autorizzazione spetta alla RLS.</summary>
public class BudgetRepository
{
    private readonly SupabaseService _supabase;
    public BudgetRepository(SupabaseService supabase) => _supabase = supabase;

    public async Task<IReadOnlyList<CategoryBudget>> ElencaAsync(Guid spazioId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<CategoryBudget>().Where(b => b.SpaceId == spazioId)
            .Order("created_at", Constants.Ordering.Ascending).Get();
        return risposta.Models;
    }

    public async Task<CategoryBudget?> LeggiAsync(Guid budgetId)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<CategoryBudget>().Where(b => b.Id == budgetId).Get();
        return risposta.Models.FirstOrDefault();
    }

    public async Task<CategoryBudget> CreaAsync(Guid spazioId, Guid autore, string categoria, decimal importo, DateTime inizio)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<CategoryBudget>().Insert(new CategoryBudget
        {
            Id = Guid.NewGuid(), SpaceId = spazioId, CreatedBy = autore, Category = categoria,
            Amount = importo, StartsOn = ExpenseRepository.PerIlDatabase(CalcoliBudget.PrimoDelMese(inizio)), EndsOn = null
        });
        return risposta.Models.FirstOrDefault()
            ?? throw new InvalidOperationException("Il database non ha restituito il budget appena creato.");
    }

    public async Task<RisultatoSalvataggio<CategoryBudget>> SalvaImportoAsync(Guid budgetId, int versioneLetta, decimal importo)
    {
        var client = await _supabase.GetClientAsync();
        // Version è solo filtro: il trigger la incrementa e il grant non consente di scriverla.
        var risposta = await client.From<CategoryBudget>().Where(b => b.Id == budgetId && b.Version == versioneLetta)
            .Set(b => b.Amount, importo).Update();
        return await Esito(risposta, budgetId, versioneLetta);
    }

    public async Task<RisultatoSalvataggio<CategoryBudget>> ChiudiAsync(Guid budgetId, int versioneLetta, DateTime ultimoMese)
    {
        var client = await _supabase.GetClientAsync();
        var risposta = await client.From<CategoryBudget>().Where(b => b.Id == budgetId && b.Version == versioneLetta)
            .Set(b => b.EndsOn!, ExpenseRepository.PerIlDatabase(CalcoliBudget.PrimoDelMese(ultimoMese))).Update();
        return await Esito(risposta, budgetId, versioneLetta);
    }

    // Zero righe distingue cancellazione, concorrenza e RLS, come in RecurringExpenseRepository.Esito.
    // Resta la stessa finestra fra rilettura e risposta descritta in ExpenseRepository.SalvaAsync.
    private async Task<RisultatoSalvataggio<CategoryBudget>> Esito(ModeledResponse<CategoryBudget> risposta, Guid budgetId, int versioneLetta)
    {
        if (risposta.Models.Count > 0)
            return new(EsitoSalvataggio.Salvata, risposta.Models[0]);
        var attuale = await LeggiAsync(budgetId);
        if (attuale is null) return new(EsitoSalvataggio.Sparita, null);
        return attuale.Version != versioneLetta
            ? new(EsitoSalvataggio.Conflitto, attuale)
            : new(EsitoSalvataggio.Rifiutata, attuale);
    }

    public async Task<bool> EliminaAsync(Guid budgetId)
    {
        var client = await _supabase.GetClientAsync();
        // Le due letture distinguono una riga assente da una DELETE filtrata dalla RLS.
        var prima = await client.From<CategoryBudget>().Where(b => b.Id == budgetId).Get();
        if (prima.Models.Count == 0) return false;
        await client.From<CategoryBudget>().Where(b => b.Id == budgetId).Delete();
        var dopo = await client.From<CategoryBudget>().Where(b => b.Id == budgetId).Get();
        return dopo.Models.Count == 0;
    }
}
