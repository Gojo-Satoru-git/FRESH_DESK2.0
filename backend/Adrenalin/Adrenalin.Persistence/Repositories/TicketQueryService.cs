using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Pagination;

namespace Adrenalin.Persistence.Repositories;

public sealed class TicketQueryService : ITicketQueryService
{
    private readonly AdrenalinDbContext _context;

    public TicketQueryService(AdrenalinDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TicketListItemDto>> GetMyTicketsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Where(t => t.CreatedByUserId == userId);

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
                t.AssignedAgentId,
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
                t.AssignedAgentId,
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

        if (userId.HasValue)
        {
            query = query.Where(t => t.CreatedByUserId == userId.Value || t.AssignedAgentId == userId.Value);
        }

        var tickets = await query.Select(t => t.Status).ToListAsync(cancellationToken);

        var activeStatuses = new[] { TicketStatus.New, TicketStatus.Open, TicketStatus.Assigned, TicketStatus.InProgress, TicketStatus.Reopened };

        var totalActive = tickets.Count(s => activeStatuses.Contains(s));
        var inProgress = tickets.Count(s => s == TicketStatus.InProgress);
        var pendingReply = tickets.Count(s => s == TicketStatus.Pending);
        var resolvedClosed = tickets.Count(s => s == TicketStatus.Resolved || s == TicketStatus.Closed);

        return new TicketDashboardDto(totalActive, inProgress, pendingReply, resolvedClosed);
    }

    public async Task<PagedResult<TicketListItemDto>> SearchTicketsAsync(SearchTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .Include(t => t.TicketTags)
            .AsNoTracking()
            .AsQueryable();

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
                t.TicketTags.Any(tag => tag.TagName.ToLower().Contains(term)) ||
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

        if (!string.IsNullOrWhiteSpace(request.Category) && Enum.TryParse<TicketCategory>(request.Category, true, out var category))
        {
            query = query.Where(t => t.Category == category);
        }

        if (request.AssigneeId.HasValue)
        {
            query = query.Where(t => t.AssignedAgentId == request.AssigneeId.Value);
        }

        if (request.ReporterId.HasValue)
        {
            query = query.Where(t => t.CreatedByUserId == request.ReporterId.Value || t.ContactId == request.ReporterId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(t => t.Department == request.Department);
        }

        if (!string.IsNullOrWhiteSpace(request.Region))
        {
            query = query.Where(t => t.Region == request.Region);
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
                t.AssignedAgentId,
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
        var activities = await _context.TicketActivities
            .AsNoTracking()
            .Where(a => a.TicketId == ticketId)
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync(cancellationToken);

        var performerIds = activities.Where(a => a.PerformedBy.HasValue).Select(a => a.PerformedBy!.Value).Distinct().ToList();
        var userDisplayNames = new Dictionary<Guid, string>();
        if (performerIds.Any())
        {
            var users = await _context.Users
                .Where(u => performerIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                .ToListAsync(cancellationToken);
            userDisplayNames = users.ToDictionary(
                u => u.Id,
                u => {
                    var fullName = $"{u.FirstName} {u.LastName}".Trim();
                    return !string.IsNullOrWhiteSpace(fullName) ? fullName : u.Email.Split('@').First();
                }
            );
        }

        return activities.Select(a => new TicketActivityDto(
            a.Id,
            a.ActivityType,
            a.OldValue,
            a.NewValue,
            a.PerformedBy,
            a.PerformedAt,
            PerformedByName: a.PerformedBy.HasValue && userDisplayNames.TryGetValue(a.PerformedBy.Value, out var pbn) ? pbn : null
        )).ToList();
    }

    public async Task<IReadOnlyList<CommentDto>> GetTicketCommentsAsync(Guid ticketId, bool includeInternal, CancellationToken cancellationToken)
    {
        var query = _context.TicketComments
            .Include(c => c.Attachments)
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId);

        if (!includeInternal)
        {
            query = query.Where(c => c.Visibility != CommentVisibility.Internal);
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
            c.Visibility.ToString(),
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
