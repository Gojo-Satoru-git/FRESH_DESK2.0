using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.Modules.Ticketing.Application.DTOs.Comments;
using Adrenalin.Modules.Ticketing.Application.DTOs.Attachments;
using Adrenalin.Modules.Ticketing.Application.Queries.Tickets;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Pagination;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

using Microsoft.Extensions.Logging;

namespace Adrenalin.Persistence.Repositories;

public sealed class TicketQueryService : ITicketQueryService
{
    private readonly AdrenalinDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITicketVisibilityService _visibilityService;
    private readonly ILogger<TicketQueryService> _logger;

    public TicketQueryService(AdrenalinDbContext context, ICurrentUserService currentUserService, ITicketVisibilityService visibilityService, ILogger<TicketQueryService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _visibilityService = visibilityService;
        _logger = logger;
    }

    private async Task<IQueryable<Ticket>> ApplyVisibilityFilterAsync(IQueryable<Ticket> query, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return query.Where(t => false);

        return await _visibilityService.ApplyVisibilityFilterAsync(query, userId.Value, cancellationToken);
    }

    public async Task<PagedResult<TicketListItemDto>> GetMyTicketsAsync(Guid userId, string? status, string? term, int page, int pageSize, CancellationToken cancellationToken)
    {
        var userIdValue = _currentUserService.UserId ?? Guid.Empty;
        var isInternal = await _visibilityService.IsInternalAgentAsync(userIdValue, cancellationToken);

        var contactId = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var query = _context.Tickets
            .AsNoTracking()
            .Where(t => t.ContactId == contactId);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var t = term.Trim().ToLower();
            query = query.Where(x => (x.TicketNumber != null && x.TicketNumber.ToLower().Contains(t)) || x.Title.ToLower().Contains(t) || x.Description.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
        {
            if (status.ToLower() == "open")
            {
                var openStatuses = new[] { TicketStatus.New, TicketStatus.Open, TicketStatus.InProgress, TicketStatus.Reopened };
                query = query.Where(x => openStatuses.Contains(x.Status));
            }
            else if (Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(x => x.Status == parsedStatus);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.TicketNumber ?? string.Empty,
                t.Title,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.Description.Length > 200 ? t.Description.Substring(0, 200) + "..." : t.Description,
                isInternal ? t.AssignedAgentId : null,
                t.CompanyId,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<TicketListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = tickets
        };
    }

    public async Task<PagedResult<TicketListItemDto>> GetAssignedTicketsAsync(Guid agentId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var userIdValue = _currentUserService.UserId ?? Guid.Empty;
        var isInternal = await _visibilityService.IsInternalAgentAsync(userIdValue, cancellationToken);

        var query = _context.Tickets
            .AsNoTracking()
            .Where(t => t.AssignedAgentId == agentId);

        var totalCount = await query.CountAsync(cancellationToken);

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.TicketNumber ?? string.Empty,
                t.Title,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.Description.Length > 200 ? t.Description.Substring(0, 200) + "..." : t.Description,
                isInternal ? t.AssignedAgentId : null,
                t.CompanyId,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<TicketListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = tickets
        };
    }

    public async Task<TicketDashboardDto> GetTicketDashboardAsync(Guid? companyId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = _context.Tickets.AsNoTracking();

        if (companyId.HasValue)
        {
            query = query.Where(t => t.CompanyId == companyId.Value);
        }

        query = await ApplyVisibilityFilterAsync(query, cancellationToken);

        // Fetch all fields needed for full dashboard
        var rawTickets = await query.Select(t => new { 
            t.Id,
            t.TicketNumber,
            t.Title,
            t.Status, 
            t.Priority, 
            t.GroupId, 
            t.AssignedAgentId, 
            t.CreatedAt 
        }).ToListAsync(cancellationToken);

        // Temporary Dashboard Logging
        var currentUserId = _currentUserService.UserId;
        var userGroups = currentUserId.HasValue ? await _context.UserGroups.Where(ug => ug.UserId == currentUserId.Value).Select(ug => ug.GroupId).ToListAsync(cancellationToken) : new List<Guid>();
        _logger.LogInformation("TeamLead Dashboard Logic - UserId: {UserId}, ManagedGroups: {Groups}, Visible Tickets: {Count}", 
            currentUserId, string.Join(",", userGroups), rawTickets.Count);

        var activeStatuses = new[] { TicketStatus.New, TicketStatus.Open, TicketStatus.InProgress, TicketStatus.Reopened };

        // Top Cards
        var totalTickets = rawTickets.Count;
        var totalActive = rawTickets.Count(t => activeStatuses.Contains(t.Status));
        var inProgress = rawTickets.Count(t => t.Status == TicketStatus.InProgress);
        var pendingReply = rawTickets.Count(t => t.Status == TicketStatus.PendingCustomer || t.Status == TicketStatus.PendingInternal);
        var resolvedClosed = rawTickets.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed);
        
        var countsDto = new TicketCountsDto(totalTickets, totalActive - inProgress, totalActive, pendingReply);

        // Performance Metrics
        var today = DateTime.UtcNow.Date;
        var receivedToday = rawTickets.Count(t => t.CreatedAt.Date == today);
        var resolvedToday = rawTickets.Count(t => (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed) && t.CreatedAt.Date == today);
        int? resolutionRate = totalTickets > 0 ? (int)Math.Round((double)resolvedClosed / totalTickets * 100) : null;
        var performanceDto = new PerformanceMetricsDto(receivedToday, resolvedToday, resolutionRate);

        // Trends (Hourly 8am to 8pm)
        var hourlyToday = new int[7];
        var hourlyYesterday = new int[7];
        var yesterday = today.AddDays(-1);

        foreach(var t in rawTickets)
        {
            var date = t.CreatedAt.Date;
            if (date == today || date == yesterday)
            {
                var hour = t.CreatedAt.Hour;
                int bucket = -1;
                if (hour <= 9) bucket = 0;
                else if (hour <= 11) bucket = 1;
                else if (hour <= 13) bucket = 2;
                else if (hour <= 15) bucket = 3;
                else if (hour <= 17) bucket = 4;
                else if (hour <= 19) bucket = 5;
                else bucket = 6;

                if (date == today) hourlyToday[bucket]++;
                else hourlyYesterday[bucket]++;
            }
        }

        var trendLabels = new[] { "08:00 AM", "10:00 AM", "12:00 PM", "02:00 PM", "04:00 PM", "06:00 PM", "08:00 PM" };
        var trendsDto = new List<TrendDataDto>();
        for(int i=0; i<7; i++)
        {
            trendsDto.Add(new TrendDataDto(trendLabels[i], hourlyToday[i], hourlyYesterday[i]));
        }

        // Todos
        var todosDto = rawTickets
            .Where(t => (t.Priority == TicketPriority.High || t.Priority == TicketPriority.Urgent) && activeStatuses.Contains(t.Status))
            .Take(5)
            .Select((t, i) => new TodoTaskDto(t.Id.ToString(), $"Followup on ticket: {t.Title}", t.Priority.ToString()))
            .ToList();

        // Group Metrics
        var groupGroups = rawTickets.Where(t => t.GroupId.HasValue).GroupBy(t => t.GroupId!.Value).ToList();
        var groupMetrics = new List<GroupMetricDto>();
        if (groupGroups.Any())
        {
            var gIds = groupGroups.Select(g => g.Key).ToList();
            var groupNames = await _context.Set<Adrenalin.Modules.Auth.Domain.Entities.Group>()
                .Where(g => gIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id, g => g.Name, cancellationToken);

            foreach(var grp in groupGroups)
            {
                int c = grp.Count(t => activeStatuses.Contains(t.Status));
                if (c > 0)
                {
                    var name = groupNames.TryGetValue(grp.Key, out var n) ? n : "Unknown Group";
                    groupMetrics.Add(new GroupMetricDto(grp.Key, name, c));
                }
            }
        }

        // Agent Workloads
        var agentGroups = rawTickets.Where(t => t.AssignedAgentId.HasValue).GroupBy(t => t.AssignedAgentId!.Value).ToList();
        var agentWorkloads = new List<DashboardAgentWorkloadDto>();
        if (agentGroups.Any())
        {
            var agentIds = agentGroups.Select(g => g.Key).ToList();
            var agentNames = await _context.Users
                .Where(u => agentIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
                .ToDictionaryAsync(u => u.Id, u => u.FirstName + " " + u.LastName, cancellationToken);

            foreach(var agrp in agentGroups)
            {
                int open = agrp.Count(t => activeStatuses.Contains(t.Status));
                int overdue = 0; // SLA calculation skipped for performance
                var name = agentNames.TryGetValue(agrp.Key, out var n) && !string.IsNullOrWhiteSpace(n) ? n : "Unknown Agent";
                agentWorkloads.Add(new DashboardAgentWorkloadDto(agrp.Key, name.Trim(), open, overdue));
            }
        }

        return new TicketDashboardDto(
            totalTickets, 
            totalActive, 
            inProgress, 
            pendingReply, 
            resolvedClosed,
            countsDto,
            performanceDto,
            trendsDto,
            agentWorkloads,
            groupMetrics,
            todosDto,
            new SlaMetricsDto(0, 0)
        );
    }

    public async Task<PagedResult<TicketListItemDto>> SearchTicketsAsync(SearchTicketsQuery request, CancellationToken cancellationToken)
    {
        var userIdValue = _currentUserService.UserId ?? Guid.Empty;
        var isInternal = await _visibilityService.IsInternalAgentAsync(userIdValue, cancellationToken);

        var query = _context.Tickets
            .AsNoTracking()
            .AsQueryable();

        query = await ApplyVisibilityFilterAsync(query, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();

            var matchingUserIds = _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted && (
                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.Username != null && u.Username.ToLower().Contains(term))
                ))
                .Select(u => u.Id);

            var matchingContactIds = _context.Contacts
            .AsNoTracking()
            .Where(c => !c.IsDeleted && (
                    (c.Name != null && c.Name.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term))
                ))
                .Select(c => c.Id);

            query = query.Where(t => 
                (t.TicketNumber != null && t.TicketNumber.ToLower().Contains(term)) ||
                t.Title.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term) ||
                (t.CreatedByUserId.HasValue && matchingUserIds.Contains(t.CreatedByUserId.Value)) ||
                (t.ContactId.HasValue && matchingContactIds.Contains(t.ContactId.Value))
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TicketStatus>(request.Status, true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<TicketPriority>(request.Priority, true, out var priority))
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (request.GroupId.HasValue)
        {
            query = query.Where(t => t.GroupId == request.GroupId.Value);
        }

        if (request.AssignedAgentId.HasValue)
        {
            query = query.Where(t => t.AssignedAgentId == request.AssignedAgentId.Value);
        }

        if (request.Unassigned == true)
        {
            query = query.Where(t => !t.AssignedAgentId.HasValue);
        }

        if (request.Breached == true)
        {
            // SLA Breached property missing in MVP
            // query = query.Where(t => t.SlaBreached);
        }

        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.CreatedTo.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.TicketNumber ?? string.Empty,
                t.Title,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.Description.Length > 200 ? t.Description.Substring(0, 200) + "..." : t.Description,
                isInternal ? t.AssignedAgentId : null,
                t.CompanyId,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<TicketListItemDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            Items = tickets
        };
    }

    public async Task<IReadOnlyList<TicketActivityDto>> GetTicketActivitiesAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var statusHistories = await _context.TicketStatusHistories
            .AsNoTracking()
            .Where(h => h.TicketId == ticketId)
            .Select(h => new TicketActivityDto(
                h.Id,
                "StatusChange",
                h.FromStatus != null ? h.FromStatus.ToString() : null,
                h.ToStatus.ToString(),
                h.ChangedBy,
                h.ChangedAt,
                null
            ))
            .ToListAsync(cancellationToken);

        var assignmentLogs = await _context.TicketAssignmentLogs
            .AsNoTracking()
            .Where(a => a.TicketId == ticketId)
            .Select(a => new TicketActivityDto(
                a.Id,
                "Assignment",
                a.FromAgentId.HasValue ? a.FromAgentId.Value.ToString() : null,
                a.ToAgentId.ToString(),
                a.ChangedBy,
                a.AssignedAt,
                null
            ))
            .ToListAsync(cancellationToken);

        var activities = statusHistories.Concat(assignmentLogs)
            .OrderByDescending(a => a.PerformedAt)
            .ToList();

        var userIds = activities.Where(a => a.PerformedBy.HasValue).Select(a => a.PerformedBy!.Value).Distinct().ToList();
        if (userIds.Any())
        {
            var users = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
                .ToListAsync(cancellationToken);

            var userDict = users.ToDictionary(
                u => u.Id,
                u => {
                    var fullName = $"{u.FirstName} {u.LastName}".Trim();
                    return !string.IsNullOrWhiteSpace(fullName) ? fullName : u.Email.Split('@').First();
                }
            );

            for (int i = 0; i < activities.Count; i++)
            {
                if (activities[i].PerformedBy.HasValue && userDict.TryGetValue(activities[i].PerformedBy!.Value, out var name))
                {
                    activities[i] = activities[i] with { PerformedByName = name };
                }
            }
        }

        return activities;
    }

    public async Task<IReadOnlyList<CommentDto>> GetTicketCommentsAsync(Guid ticketId, bool includeInternal, CancellationToken cancellationToken)
    {
        var query = _context.TicketComments
            .AsNoTracking()
            .Include(c => c.Attachments)
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId);

        if (!includeInternal)
        {
            query = query.Where(c => !c.IsPrivate);
        }

        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var authorIds = comments.Where(c => c.AuthorId.HasValue).Select(c => c.AuthorId!.Value).Distinct().ToList();
        var contactIds = comments.Where(c => c.ContactId.HasValue).Select(c => c.ContactId!.Value).Distinct().ToList();

        var contactDisplayNames = new Dictionary<Guid, string>();
        if (contactIds.Any())
        {
            var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => contactIds.Contains(c.Id) && !c.IsDeleted)
                .Select(c => new { c.Id, c.Name, c.Email })
                .ToListAsync(cancellationToken);
            contactDisplayNames = contacts.ToDictionary(
                c => c.Id,
                c => !string.IsNullOrWhiteSpace(c.Name) ? c.Name : c.Email.Split('@').First()
            );
        }

        var unresolvedContactIds = contactIds.Where(id => !contactDisplayNames.ContainsKey(id)).ToList();
        if (unresolvedContactIds.Any())
        {
            authorIds.AddRange(unresolvedContactIds);
            authorIds = authorIds.Distinct().ToList();
        }

        var userDisplayNames = new Dictionary<Guid, string>();
        if (authorIds.Any())
        {
            var users = await _context.Users
            .AsNoTracking()
            .Where(u => authorIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                .ToListAsync(cancellationToken);
            userDisplayNames = users.ToDictionary(
                u => u.Id,
                u => {
                    var fullName = $"{u.FirstName} {u.LastName}".Trim();
                    return !string.IsNullOrWhiteSpace(fullName) ? fullName : u.Email.Split('@').First();
                }
            );

            foreach (var contactId in unresolvedContactIds)
            {
                if (userDisplayNames.TryGetValue(contactId, out var userName))
                {
                    contactDisplayNames[contactId] = userName;
                }
            }
        }

        return comments.Select(c => new CommentDto(
            c.Id,
            c.AuthorId,
            c.ContactId,
            c.Body,
            c.IsPrivate ? "Internal" : "Public",
            c.CreatedAt,
            c.Attachments.Select(a => new AttachmentDto(
                a.Id,
                a.FileName,
                a.FileUrl,
                a.FileSizeBytes,
                a.MimeType,
                a.CreatedAt
            )).ToList(),
            AuthorName: c.AuthorId.HasValue && userDisplayNames.TryGetValue(c.AuthorId.Value, out var aun) ? aun : null,
            ContactName: c.ContactId.HasValue && contactDisplayNames.TryGetValue(c.ContactId.Value, out var cn) ? cn : null
        )).ToList();
    }
}
