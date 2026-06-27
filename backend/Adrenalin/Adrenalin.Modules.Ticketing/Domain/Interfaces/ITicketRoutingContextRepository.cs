using Adrenalin.Modules.Ticketing.Application.DTOs.Routing;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketRoutingContextRepository
{
    Task<IReadOnlyList<Guid>> GetCompanyGroupsAsync(Guid companyId, CancellationToken ct = default);
    Task<Guid?> GetCompanyDefaultGroupAsync(Guid companyId, CancellationToken ct = default);
    Task<(string? RegionCode, int AssignmentStrategy)?> GetGroupRoutingInfoAsync(Guid groupId, CancellationToken ct = default);
    Task<bool> GroupExistsAsync(Guid groupId, CancellationToken ct = default);
    Task<string?> GetGroupNameAsync(Guid groupId, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetGroupNamesAsync(IEnumerable<Guid> groupIds, CancellationToken ct = default);
    Task<bool> IsUserInGroupAsync(Guid userId, Guid groupId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetUserGroupIdsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<RoutingRuleHistoryDto>> GetRoutingRuleHistoryAsync(Guid ruleId, CancellationToken ct = default);
    Task<string?> GetCompanyNameAsync(Guid companyId, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetCompanyNamesAsync(IEnumerable<Guid> companyIds, CancellationToken ct = default);
    Task<IReadOnlyList<CompanyDefaultRouteDto>> GetCompaniesWithDefaultGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<bool> IsUserLeadOfGroupAsync(Guid userId, Guid groupId, CancellationToken ct = default);
    Task<IReadOnlyList<(Guid GroupId, string GroupName)>> GetGroupsLedByUserAsync(
    Guid userId, CancellationToken ct = default);
}
