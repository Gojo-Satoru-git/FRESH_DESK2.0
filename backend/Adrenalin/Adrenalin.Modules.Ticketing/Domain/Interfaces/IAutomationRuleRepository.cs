// Ticketing/Domain/Interfaces/IAutomationRuleRepository.cs
using Adrenalin.SharedKernel.Contracts;

public interface IAutomationRuleRepository
{
    Task<List<IAutomationRuleContract>> GetActiveRulesForTriggerAsync(
        CancellationToken ct = default);

    Task LogExecutionAsync(
        Guid ruleId,
        Guid ticketId,
        string actionTaken,
        CancellationToken ct = default);
}