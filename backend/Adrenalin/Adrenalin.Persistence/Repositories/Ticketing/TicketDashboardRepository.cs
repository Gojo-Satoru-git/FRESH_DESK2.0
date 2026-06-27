using Adrenalin.Modules.Ticketing.Application.Queries.Groups;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Modules.Ticketing.Application.DTOs.Groups;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class TicketDashboardRepository : ITicketDashboardRepository
{
    private readonly AdrenalinDbContext _db;

    public TicketDashboardRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsGroupMemberOrAdminAsync(Guid groupId, Guid userId, CancellationToken ct = default)
    {
        var isMember = await _db.UserGroups.IgnoreQueryFilters().AnyAsync(ug => ug.GroupId == groupId && ug.UserId == userId && !ug.IsDeleted, ct);
        if (isMember) return true;

        return await _db.UserRoles
            .Include(ur => ur.Role)
            .IgnoreQueryFilters()
            .AnyAsync(ur => ur.UserId == userId && !ur.IsDeleted && ur.Role.Name.ToLower() == "admin", ct);
    }

    public async Task<bool> IsCompanyMemberOrAdminAsync(Guid companyId, Guid userId, CancellationToken ct = default)
    {
        // Check if user is associated with this company
        var isCompanyContact = await _db.Contacts.IgnoreQueryFilters()
            .AnyAsync(c => c.CompanyId == companyId && c.UserId == userId && !c.IsDeleted, ct);
            
        if (isCompanyContact) return true;

        // Check if user is an agent in a group assigned to this company
        var isCompanyAgent = await _db.CompanyGroups.IgnoreQueryFilters()
            .Join(_db.UserGroups.IgnoreQueryFilters(), cg => cg.GroupId, ug => ug.GroupId, (cg, ug) => new { cg, ug })
            .AnyAsync(x => x.cg.CompanyId == companyId && x.ug.UserId == userId && !x.cg.IsDeleted && !x.ug.IsDeleted, ct);

        if (isCompanyAgent) return true;

        return await _db.UserRoles
            .Include(ur => ur.Role)
            .IgnoreQueryFilters()
            .AnyAsync(ur => ur.UserId == userId && !ur.IsDeleted && ur.Role.Name.ToLower() == "admin", ct);
    }

    public async Task<GroupDashboardDto?> GetGroupDashboardDataAsync(Guid groupId, Guid actorId, CancellationToken ct = default)
    {
        var groupName = await _db.Groups.IgnoreQueryFilters()
            .Where(g => g.Id == groupId && !g.IsDeleted)
            .Select(g => g.Name)
            .FirstOrDefaultAsync(ct);

        if (groupName is null) return null;

        var tickets = _db.Tickets.Where(t => t.GroupId == groupId && !t.IsDeleted);

        var totalCount = await tickets.CountAsync(ct);
        var unassignedCount = await tickets.CountAsync(t => t.AssignedAgentId == null, ct);
        var assignedCount = await tickets.CountAsync(t => t.AssignedAgentId != null, ct);
        var criticalCount = await tickets.CountAsync(t => t.Priority == TicketPriority.Urgent || t.ForceP1, ct);

        var overdueThreshold = DateTimeOffset.UtcNow.AddHours(-24);
        var overdueCount = await tickets.CountAsync(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && t.CreatedAt < overdueThreshold, ct);

        var groupMembers = await _db.UserGroups.IgnoreQueryFilters().AsNoTracking()
            .Where(ug => ug.GroupId == groupId && !ug.IsDeleted)
            .Select(ug => new { ug.UserId, Email = ug.User != null ? ug.User.Email : "Unknown" })
            .ToListAsync(ct);

        var memberIds = groupMembers.Select(m => m.UserId).ToList();

        var agentTickets = await _db.Tickets.AsNoTracking()
            .Where(t => t.GroupId == groupId && t.AssignedAgentId != null && memberIds.Contains(t.AssignedAgentId.Value) && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && !t.IsDeleted)
            .Select(t => new { t.AssignedAgentId, t.CreatedAt })
            .ToListAsync(ct);

        var agentWorkloads = new List<AgentWorkloadDto>();
        foreach (var member in groupMembers)
        {
            var memberTickets = agentTickets.Where(t => t.AssignedAgentId == member.UserId).ToList();
            var overdueAgentCount = memberTickets.Count(t => t.CreatedAt < overdueThreshold);
            agentWorkloads.Add(new AgentWorkloadDto(member.UserId, member.Email, memberTickets.Count, overdueAgentCount));
        }

        return new GroupDashboardDto(groupId, groupName, totalCount, unassignedCount, assignedCount, overdueCount, criticalCount, 0, agentWorkloads);
    }

    public async Task<GroupQueueResultDto?> GetGroupQueueDataAsync(Guid groupId, string queueType, Guid actorId, int page, int pageSize, CancellationToken ct = default)
    {
        var groupExists = await _db.Groups.IgnoreQueryFilters().AnyAsync(g => g.Id == groupId && !g.IsDeleted, ct);
        if (!groupExists) return null;

        var tickets = _db.Tickets.Where(t => t.GroupId == groupId && !t.IsDeleted);

        var overdueThreshold = DateTimeOffset.UtcNow.AddHours(-24);

        tickets = queueType?.ToLowerInvariant() switch
        {
            "unassigned" => tickets.Where(t => t.AssignedAgentId == null),
            "assigned" => tickets.Where(t => t.AssignedAgentId != null),
            "overdue" => tickets.Where(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && t.CreatedAt < overdueThreshold),
            "critical" => tickets.Where(t => t.Priority == TicketPriority.Urgent || t.ForceP1),
            _ => tickets
        };

        var totalCount = await tickets.CountAsync(ct);

        var pagedTickets = await tickets.AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new { t.Id, t.TicketNumber, t.Title, t.Status, t.Priority, t.CreatedAt, t.AssignedAgentId })
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var items = pagedTickets.Select(t => new TicketQueueItemDto(
            t.Id, t.TicketNumber, t.Title, t.Status.ToString(), t.Priority.ToString(),
            null, t.CreatedAt, t.AssignedAgentId, null, now - t.CreatedAt)).ToList();

        return new GroupQueueResultDto(items, totalCount, page, pageSize);
    }

    public async Task<LeadDashboardDto?> GetLeadDashboardDataAsync(Guid leadUserId, CancellationToken ct = default)
    {
        var leadGroupIds = await GetLeadGroupIdsAsync(leadUserId, ct);
        if (!leadGroupIds.Any()) return null;

        var dashboards = new List<GroupDashboardDto>();
        foreach (var groupId in leadGroupIds)
        {
            var dash = await GetGroupDashboardDataAsync(groupId, leadUserId, ct);
            if (dash is not null) dashboards.Add(dash);
        }

        var totalUnassigned = dashboards.Sum(d => d.UnassignedCount);
        var totalOverdue = dashboards.Sum(d => d.OverdueCount);
        var totalCritical = dashboards.Sum(d => d.CriticalCount);

        return new LeadDashboardDto(dashboards, totalUnassigned, totalOverdue, totalCritical);
    }

    private async Task<IReadOnlyList<Guid>> GetLeadGroupIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UserGroups.IgnoreQueryFilters()
            .Where(ug => ug.UserId == userId && ug.IsLead && !ug.IsDeleted)
            .Select(ug => ug.GroupId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GroupAgentWorkloadDto>?> GetGroupWorkloadAsync(Guid groupId, CancellationToken ct = default)
    {
        var groupExists = await _db.Groups.IgnoreQueryFilters().AnyAsync(g => g.Id == groupId && !g.IsDeleted, ct);
        if (!groupExists) return null;

        var groupMembers = await _db.UserGroups.IgnoreQueryFilters().AsNoTracking()
            .Where(ug => ug.GroupId == groupId && !ug.IsDeleted)
            .Select(ug => new { ug.UserId, FirstName = ug.User != null ? ug.User.FirstName : null, Email = ug.User != null ? ug.User.Email : null })
            .ToListAsync(ct);

        var overdueThreshold = DateTimeOffset.UtcNow.AddHours(-24);
        var workloads = new List<GroupAgentWorkloadDto>();
        int defaultCapacity = 20;

        var memberIds = groupMembers.Select(m => m.UserId).ToList();
        
        var agentTickets = await _db.Tickets.AsNoTracking()
            .Where(t => t.GroupId == groupId && t.AssignedAgentId != null && memberIds.Contains(t.AssignedAgentId.Value) && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && !t.IsDeleted)
            .Select(t => new { t.AssignedAgentId, t.Priority, t.ForceP1, t.CreatedAt })
            .ToListAsync(ct);

        foreach (var member in groupMembers)
        {
            var memberTickets = agentTickets.Where(t => t.AssignedAgentId == member.UserId).ToList();

            int activeTickets = memberTickets.Count;
            int criticalTickets = memberTickets.Count(t => t.Priority == TicketPriority.Urgent || t.ForceP1);
            int overdueTickets = memberTickets.Count(t => t.CreatedAt < overdueThreshold);
            
            int utilPercent = defaultCapacity > 0 ? (int)Math.Round((double)activeTickets / defaultCapacity * 100) : 0;

            workloads.Add(new GroupAgentWorkloadDto(
                member.UserId,
                member.FirstName ?? member.Email ?? "Unknown",
                activeTickets,
                criticalTickets,
                overdueTickets,
                defaultCapacity,
                utilPercent
            ));
        }

        return workloads;
    }

    public async Task<CompanyTicketMetricsDto> GetCompanyTicketMetricsAsync(Guid companyId, CancellationToken ct = default)
    {
        var tickets = _db.Tickets.IgnoreQueryFilters().Where(t => t.CompanyId == companyId && !t.IsDeleted);

        var total = await tickets.CountAsync(ct);
        var unassigned = await tickets.CountAsync(t => t.AssignedAgentId == null && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed, ct);
        var open = await tickets.CountAsync(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed, ct);
        var critical = await tickets.CountAsync(t => (t.Priority == TicketPriority.Urgent || t.ForceP1) && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed, ct);
        var slaBreached = await tickets.CountAsync(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && t.SlaSeverityScore > 0, ct);
        var resolved = await tickets.CountAsync(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed, ct);

        var overdueThreshold = DateTimeOffset.UtcNow.AddHours(-24);
        var overdue = await tickets.CountAsync(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && t.CreatedAt < overdueThreshold, ct);

        var breached = Math.Max(slaBreached, overdue);

        return new CompanyTicketMetricsDto(
            companyId,
            total,
            unassigned,
            open,
            critical,
            breached,
            resolved
        );
    }
}
