using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Pagination;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Persistence.Repositories;

public sealed class TicketQueryService : ITicketQueryService
{
    private readonly AdrenalinDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TicketQueryService(AdrenalinDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private async Task<IQueryable<Ticket>> ApplyVisibilityFilterAsync(IQueryable<Ticket> query, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return query.Where(t => false);

        var roles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId.Value)
            .Select(ur => ur.Role.Name.ToLower())
            .ToListAsync(cancellationToken);

        if (roles.Contains("admin"))
        {
            return query;
        }
        else if (roles.Contains("manager") || roles.Contains("pmo") || roles.Contains("team_lead"))
        {
            var userGroups = await _context.UserGroups
                .Where(ug => ug.UserId == userId.Value)
                .Select(ug => ug.GroupId)
                .ToListAsync(cancellationToken);

            return query.Where(t => t.GroupId.HasValue && userGroups.Contains(t.GroupId.Value));
        }
        else if (roles.Contains("senior_agent") || roles.Contains("junior_agent"))
        {
            return query.Where(t => t.AssignedAgentId == userId.Value);
        }
        else if (roles.Contains("customer_admin"))
        {
            var companyId = await _context.Contacts
                .Where(c => c.UserId == userId.Value && !c.IsDeleted)
                .Select(c => c.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (companyId != Guid.Empty)
            {
                return query.Where(t => t.CompanyId == companyId);
            }
            return query.Where(t => t.CreatedByUserId == userId.Value);
        }
        else // default to customer
        {
            return query.Where(t => t.CreatedByUserId == userId.Value);
        }
    }

    public async Task<PagedResult<TicketListItemDto>> GetMyTicketsAsync(Guid userId, string? status, string? term, int page, int pageSize, CancellationToken cancellationToken)
    {
        var isInternal = _currentUserService.Roles.Intersect(new[] { "admin", "manager", "pmo", "team_lead", "senior_agent", "junior_agent" }, StringComparer.OrdinalIgnoreCase).Any();

        var query = _context.Tickets
            .AsNoTracking()
            .Where(t => t.CreatedByUserId == userId || t.ContactId == userId);

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
        var isInternal = _currentUserService.Roles.Intersect(new[] { "admin", "manager", "pmo", "team_lead", "senior_agent", "junior_agent" }, StringComparer.OrdinalIgnoreCase).Any();

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

        var tickets = await query.Select(t => t.Status).ToListAsync(cancellationToken);

        var activeStatuses = new[] { TicketStatus.New, TicketStatus.Open, TicketStatus.InProgress, TicketStatus.Reopened };

        var totalActive = tickets.Count(s => activeStatuses.Contains(s));
        var inProgress = tickets.Count(s => s == TicketStatus.InProgress);
        var pendingReply = tickets.Count(s => s == TicketStatus.PendingCustomer || s == TicketStatus.PendingInternal);
        var resolvedClosed = tickets.Count(s => s == TicketStatus.Resolved || s == TicketStatus.Closed);

        return new TicketDashboardDto(totalActive, inProgress, pendingReply, resolvedClosed);
    }

    public async Task<PagedResult<TicketListItemDto>> SearchTicketsAsync(SearchTicketsQuery request, CancellationToken cancellationToken)
    {
        var isInternal = _currentUserService.Roles.Intersect(new[] { "admin", "manager", "pmo", "team_lead", "senior_agent", "junior_agent" }, StringComparer.OrdinalIgnoreCase).Any();

        var query = _context.Tickets
            .AsNoTracking()
            .AsQueryable();

        query = await ApplyVisibilityFilterAsync(query, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim().ToLower();

            var matchingUserIds = await _context.Users
                .Where(u => !u.IsDeleted && (
                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.Username != null && u.Username.ToLower().Contains(term))
                ))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            var matchingContactIds = await _context.Contacts
                .Where(c => !c.IsDeleted && (
                    (c.Name != null && c.Name.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term))
                ))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

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

        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<TicketType>(request.Type, true, out var type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (request.AssigneeId.HasValue)
        {
            query = query.Where(t => t.AssignedAgentId == request.AssigneeId.Value);
        }

        if (request.ReporterId.HasValue)
        {
            query = query.Where(t => t.CreatedByUserId == request.ReporterId.Value || t.ContactId == request.ReporterId.Value);
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
