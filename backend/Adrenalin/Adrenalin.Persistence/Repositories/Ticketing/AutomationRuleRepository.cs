using Adrenalin.Modules.SLA.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public class AutomationRuleRepository : IAutomationRuleRepository
{
    private readonly AdrenalinDbContext _db;

    public AutomationRuleRepository(AdrenalinDbContext db)
        => _db = db;

    public async Task<List<IAutomationRuleContract>>
        GetActiveRulesForTriggerAsync(
            CancellationToken ct = default)
    {
        var rules = await _db.AutomationRules
        .Where(r => r.IsActive && !r.IsDeleted)
        .OrderBy(r => r.ExecutionOrder)
        .ToListAsync(ct);

        return rules
            .Select(r => (IAutomationRuleContract)
                new AutomationRuleContractDto(
                    r.Id,
                    r.Name,
                    r.ExecutionOrder,
                    r.Conditions.ToString(),
                    r.Actions.ToString()))
            .ToList();
    }

    public async Task LogExecutionAsync(
        Guid ruleId,
        Guid ticketId,
        string actionTaken,
        CancellationToken ct = default)
    {
        // Use factory method we added to AutomationExecutionLog
        var log = AutomationExecutionLog.Create(
            ruleId: ruleId,
            ticketId: ticketId,
            succeeded: true);

        await _db.AutomationExecutionLogs.AddAsync(log, ct);
    }
}