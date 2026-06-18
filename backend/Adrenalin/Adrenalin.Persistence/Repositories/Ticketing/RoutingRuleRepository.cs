using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class RoutingRuleRepository : IRoutingRuleRepository
{
    private readonly AdrenalinDbContext _db;
    public RoutingRuleRepository(AdrenalinDbContext db) => _db = db;

    public Task<CompanyRoutingRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult<CompanyRoutingRule?>(null);

    public Task<IReadOnlyList<CompanyRoutingRule>> GetByCompanyOrderedAsync(Guid companyId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CompanyRoutingRule>>([]);

    public Task<IReadOnlyList<CompanyRoutingRule>> GetByGroupAsync(Guid groupId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CompanyRoutingRule>>([]);

    public Task<IReadOnlyList<CompanyRoutingRule>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CompanyRoutingRule>>([]);

    public Task<CompanyRoutingRule?> GetDefaultForCompanyAsync(Guid companyId, CancellationToken ct = default)
        => Task.FromResult<CompanyRoutingRule?>(null);

    public void Add(CompanyRoutingRule rule) { /* Feature disabled pending schema deployment */ }
    public void Update(CompanyRoutingRule rule) { /* Feature disabled pending schema deployment */ }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
}
