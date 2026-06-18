using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class TicketRoutingContextRepository : ITicketRoutingContextRepository
{
    private readonly AdrenalinDbContext _db;

    public TicketRoutingContextRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Guid>> GetCompanyGroupsAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _db.CompanyGroups.IgnoreQueryFilters()
            .Where(cg => cg.CompanyId == companyId && !cg.IsDeleted)
            .OrderBy(cg => cg.Priority)
            .Select(cg => cg.GroupId)
            .ToListAsync(ct);
    }

    public async Task<Guid?> GetCompanyDefaultGroupAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _db.CompanyGroups.IgnoreQueryFilters()
            .Where(cg => cg.CompanyId == companyId && cg.IsDefault && !cg.IsDeleted)
            .Select(cg => (Guid?)cg.GroupId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(string? RegionCode, int AssignmentStrategy)?> GetGroupRoutingInfoAsync(Guid groupId, CancellationToken ct = default)
    {
        var group = await _db.Groups.IgnoreQueryFilters()
            .Where(g => g.Id == groupId && !g.IsDeleted && g.IsActive)
            .Select(g => new { g.RegionCode, g.AssignmentStrategy })
            .FirstOrDefaultAsync(ct);

        if (group is null) return null;

        return (group.RegionCode, group.AssignmentStrategy);
    }

    public async Task<bool> GroupExistsAsync(Guid groupId, CancellationToken ct = default)
    {
        return await _db.Groups.IgnoreQueryFilters().AnyAsync(g => g.Id == groupId && !g.IsDeleted && g.IsActive, ct);
    }

    public async Task<string?> GetGroupNameAsync(Guid groupId, CancellationToken ct = default)
    {
        return await _db.Groups.IgnoreQueryFilters()
            .Where(g => g.Id == groupId && !g.IsDeleted)
            .Select(g => g.Name)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Dictionary<Guid, string>> GetGroupNamesAsync(IEnumerable<Guid> groupIds, CancellationToken ct = default)
    {
        var ids = groupIds.Distinct().ToList();
        return await _db.Groups.IgnoreQueryFilters()
            .Where(g => ids.Contains(g.Id) && !g.IsDeleted)
            .ToDictionaryAsync(g => g.Id, g => g.Name, ct);
    }

    public async Task<bool> IsUserInGroupAsync(Guid userId, Guid groupId, CancellationToken ct = default)
    {
        return await _db.UserGroups.IgnoreQueryFilters()
            .AnyAsync(ug => ug.UserId == userId && ug.GroupId == groupId && !ug.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Guid>> GetUserGroupIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UserGroups.IgnoreQueryFilters()
            .Where(ug => ug.UserId == userId && !ug.IsDeleted)
            .Select(ug => ug.GroupId)
            .ToListAsync(ct);
    }
    public async Task<IReadOnlyList<RoutingRuleHistoryDto>> GetRoutingRuleHistoryAsync(Guid ruleId, CancellationToken ct = default)
    {
        var logs = await _db.AuditLogs
            .Where(a => a.TableName == "company_routing_rules" && a.RecordId == ruleId)
            .OrderByDescending(a => a.ChangedAt)
            .Join(
                _db.Users.IgnoreQueryFilters(),
                a => a.ChangedBy,
                u => u.Id,
                (a, u) => new { Log = a, User = u }
            )
            .ToListAsync(ct);

        return logs.Select(x => new RoutingRuleHistoryDto(
            x.Log.Id,
            x.Log.RecordId,
            x.Log.Action,
            x.Log.OldValues,
            x.Log.NewValues,
            x.Log.ChangedBy,
            $"{x.User.FirstName} {x.User.LastName}".Trim(),
            x.Log.ChangedAt,
            x.Log.IpAddress?.ToString()
        )).ToList();
    }

    public async Task<string?> GetCompanyNameAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _db.Companies.IgnoreQueryFilters()
            .Where(c => c.Id == companyId && !c.IsDeleted)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Dictionary<Guid, string>> GetCompanyNamesAsync(IEnumerable<Guid> companyIds, CancellationToken ct = default)
    {
        var ids = companyIds.Distinct().ToList();
        return await _db.Companies.IgnoreQueryFilters()
            .Where(c => ids.Contains(c.Id) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);
    }

    public async Task<IReadOnlyList<CompanyDefaultRouteDto>> GetCompaniesWithDefaultGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        return await _db.CompanyGroups.IgnoreQueryFilters()
            .Where(cg => cg.GroupId == groupId && cg.IsDefault && !cg.IsDeleted)
            .Join(
                _db.Companies.IgnoreQueryFilters(),
                cg => cg.CompanyId,
                c => c.Id,
                (cg, c) => new CompanyDefaultRouteDto(c.Id, c.Name)
            )
            .ToListAsync(ct);
    }
}
