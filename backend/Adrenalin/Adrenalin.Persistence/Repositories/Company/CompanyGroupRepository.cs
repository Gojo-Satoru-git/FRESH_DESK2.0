using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class CompanyGroupRepository : ICompanyGroupRepository
{
    private readonly AdrenalinDbContext _db;
    public CompanyGroupRepository(AdrenalinDbContext db) => _db = db;

    public async Task<CompanyGroup?> GetAsync(Guid companyId, Guid groupId, CancellationToken ct = default)
        => await _db.CompanyGroups.IgnoreQueryFilters()
            .FirstOrDefaultAsync(cg => cg.CompanyId == companyId && cg.GroupId == groupId && !cg.IsDeleted, ct);

    public async Task<CompanyGroup?> GetIncludingDeletedAsync(Guid companyId, Guid groupId, CancellationToken ct = default)
        => await _db.CompanyGroups.IgnoreQueryFilters()
            .FirstOrDefaultAsync(cg => cg.CompanyId == companyId && cg.GroupId == groupId, ct);

    public async Task<IReadOnlyList<CompanyGroup>> GetByCompanyAsync(Guid companyId, CancellationToken ct = default)
        => await _db.CompanyGroups
            .Where(cg => cg.CompanyId == companyId)
            .OrderBy(cg => cg.Priority)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CompanyGroup>> GetByGroupAsync(Guid groupId, CancellationToken ct = default)
        => await _db.CompanyGroups
            .Where(cg => cg.GroupId == groupId)
            .OrderBy(cg => cg.Priority)
            .ToListAsync(ct);

    public async Task<CompanyGroup?> GetDefaultForCompanyAsync(Guid companyId, CancellationToken ct = default)
        => await _db.CompanyGroups
            .FirstOrDefaultAsync(cg => cg.CompanyId == companyId && cg.IsDefault, ct);

    public async Task ClearDefaultForCompanyAsync(Guid companyId, Guid actorId, CancellationToken ct = default)
    {
        var defaults = await _db.CompanyGroups
            .Where(cg => cg.CompanyId == companyId && cg.IsDefault)
            .ToListAsync(ct);
        foreach (var cg in defaults) cg.SetDefault(false, actorId);
    }

    public void Add(CompanyGroup cg) => _db.CompanyGroups.Add(cg);
    public void Update(CompanyGroup cg) => _db.CompanyGroups.Update(cg);

    public async Task SoftDeleteByCompanyAsync(Guid companyId, Guid actorId, CancellationToken ct = default)
    {
        var rows = await _db.CompanyGroups
            .Where(cg => cg.CompanyId == companyId)
            .ToListAsync(ct);
        foreach (var cg in rows) cg.SoftDelete(actorId);
    }

    public async Task SoftDeleteByGroupAsync(Guid groupId, Guid actorId, CancellationToken ct = default)
    {
        var rows = await _db.CompanyGroups
            .Where(cg => cg.GroupId == groupId)
            .ToListAsync(ct);
        foreach (var cg in rows) cg.SoftDelete(actorId);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);

    public async Task<bool> GroupExistsAsync(Guid groupId, CancellationToken ct = default)
        => await _db.Groups.AnyAsync(g => g.Id == groupId && !g.IsDeleted, ct);

    public async Task<string?> GetGroupNameAsync(Guid groupId, CancellationToken ct = default)
    {
        var group = await _db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted, ct);
        return group?.Name;
    }

    public async Task<Dictionary<Guid, string>> GetGroupNamesAsync(IEnumerable<Guid> groupIds, CancellationToken ct = default)
    {
        var ids = groupIds.Distinct().ToList();
        return await _db.Groups
            .AsNoTracking()
            .Where(g => ids.Contains(g.Id) && !g.IsDeleted)
            .ToDictionaryAsync(g => g.Id, g => g.Name, ct);
    }

    public async Task<bool> HasRoutingRulesAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _db.CompanyRoutingRules.AnyAsync(r => r.CompanyId == companyId && !r.IsDeleted, ct);
    }
}
