using Adrenalin.Modules.Ticketing.Application.Queries.Groups;
using Adrenalin.Modules.Ticketing.Application.DTOs.Groups;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketDashboardRepository
{
    Task<GroupDashboardDto?> GetGroupDashboardDataAsync(Guid groupId, Guid actorId, CancellationToken ct = default);
    Task<GroupQueueResultDto?> GetGroupQueueDataAsync(Guid groupId, string queueType, Guid actorId, int page, int pageSize, CancellationToken ct = default);
    Task<LeadDashboardDto?> GetLeadDashboardDataAsync(Guid leadUserId, CancellationToken ct = default);
    Task<IReadOnlyList<GroupAgentWorkloadDto>?> GetGroupWorkloadAsync(Guid groupId, CancellationToken ct = default);
    Task<bool> IsGroupMemberOrAdminAsync(Guid groupId, Guid userId, CancellationToken ct = default);
    Task<bool> IsCompanyMemberOrAdminAsync(Guid companyId, Guid userId, CancellationToken ct = default);
    Task<CompanyTicketMetricsDto> GetCompanyTicketMetricsAsync(Guid companyId, CancellationToken ct = default);
}
