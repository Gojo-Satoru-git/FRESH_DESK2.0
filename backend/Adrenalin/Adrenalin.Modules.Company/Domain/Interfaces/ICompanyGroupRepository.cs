using Adrenalin.Modules.Company.Domain.Entities;

namespace Adrenalin.Modules.Company.Domain.Interfaces;

public interface ICompanyGroupRepository
{
    Task<CompanyGroup?> GetAsync(Guid companyId, Guid groupId, CancellationToken ct = default);
    Task<CompanyGroup?> GetIncludingDeletedAsync(Guid companyId, Guid groupId, CancellationToken ct = default);
    Task<IReadOnlyList<CompanyGroup>> GetByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<IReadOnlyList<CompanyGroup>> GetByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<CompanyGroup?> GetDefaultForCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task ClearDefaultForCompanyAsync(Guid companyId, Guid actorId, CancellationToken ct = default);
    void Add(CompanyGroup cg);
    void Update(CompanyGroup cg);
    Task SoftDeleteByCompanyAsync(Guid companyId, Guid actorId, CancellationToken ct = default);
    Task SoftDeleteByGroupAsync(Guid groupId, Guid actorId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    // Cross-module lookups to avoid Auth dependency
    Task<bool> GroupExistsAsync(Guid groupId, CancellationToken ct = default);
    Task<string?> GetGroupNameAsync(Guid groupId, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetGroupNamesAsync(IEnumerable<Guid> groupIds, CancellationToken ct = default);
    Task<bool> HasRoutingRulesAsync(Guid companyId, CancellationToken ct = default);
}
