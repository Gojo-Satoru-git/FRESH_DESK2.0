using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface IRoutingRuleRepository
{
    Task<CompanyRoutingRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CompanyRoutingRule>> GetByCompanyOrderedAsync(Guid companyId, CancellationToken ct = default);
    Task<IReadOnlyList<CompanyRoutingRule>> GetByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<IReadOnlyList<CompanyRoutingRule>> GetAllAsync(CancellationToken ct = default);
    Task<CompanyRoutingRule?> GetDefaultForCompanyAsync(Guid companyId, CancellationToken ct = default);
    void Add(CompanyRoutingRule rule);
    void Update(CompanyRoutingRule rule);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
