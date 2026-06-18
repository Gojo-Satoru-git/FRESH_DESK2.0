using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Company.Domain.Entities;

namespace Adrenalin.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AdrenalinDbContext _context;

    public NotificationRepository(AdrenalinDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate?> GetTemplateByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsDeleted && t.IsActive, cancellationToken);
    }

    public async Task AddLogAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        // Deduplication: prevent sending the exact same notification to the same recipient within 5 minutes
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
        var isDuplicate = await _context.NotificationLogs.AnyAsync(l =>
            l.TemplateId == log.TemplateId &&
            l.RecipientEmail == log.RecipientEmail &&
            l.TicketId == log.TicketId &&
            l.SentAt >= fiveMinutesAgo &&
            !l.IsFailedDelivery, cancellationToken);

        if (!isDuplicate)
        {
            await _context.NotificationLogs.AddAsync(log, cancellationToken);
        }
    }

    public async Task AddLogsAsync(IEnumerable<NotificationLog> logs, CancellationToken cancellationToken = default)
    {
        if (!logs.Any()) return;

        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

        // Fetch recent logs for potential deduplication
        var ticketIds = logs.Select(l => l.TicketId).Distinct().ToList();
        var templateIds = logs.Select(l => l.TemplateId).Distinct().ToList();
        var recipientEmails = logs.Select(l => l.RecipientEmail).Distinct().ToList();

        var recentLogs = await _context.NotificationLogs
            .AsNoTracking()
            .Where(l => 
                ticketIds.Contains(l.TicketId) && 
                templateIds.Contains(l.TemplateId) && 
                recipientEmails.Contains(l.RecipientEmail) &&
                l.SentAt >= fiveMinutesAgo &&
                !l.IsFailedDelivery)
            .ToListAsync(cancellationToken);

        var newLogs = new List<NotificationLog>();

        foreach (var log in logs)
        {
            var isDuplicate = recentLogs.Any(l => 
                l.TemplateId == log.TemplateId &&
                l.RecipientEmail == log.RecipientEmail &&
                l.TicketId == log.TicketId);

            if (!isDuplicate)
            {
                newLogs.Add(log);
            }
        }

        if (newLogs.Any())
        {
            await _context.NotificationLogs.AddRangeAsync(newLogs, cancellationToken);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> ResolveEmailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userEmail = await _context.Users
            .Where(u => u.Id == id && !u.IsDeleted)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrEmpty(userEmail))
            return userEmail;

        var contactEmail = await _context.Contacts
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => c.Email)
            .FirstOrDefaultAsync(cancellationToken);

        return contactEmail;
    }

    public async Task<Dictionary<Guid, string>> ResolveEmailsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        
        var userEmails = await _context.Users
            .AsNoTracking()
            .Where(u => idList.Contains(u.Id) && !u.IsDeleted)
            .Select(u => new { u.Id, u.Email })
            .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken);

        var missingIds = idList.Except(userEmails.Keys).ToList();

        if (missingIds.Any())
        {
            var contactEmails = await _context.Contacts
                .AsNoTracking()
                .Where(c => missingIds.Contains(c.Id) && !c.IsDeleted)
                .Select(c => new { c.Id, c.Email })
                .ToDictionaryAsync(c => c.Id, c => c.Email, cancellationToken);

            foreach (var kvp in contactEmails)
            {
                userEmails[kvp.Key] = kvp.Value;
            }
        }

        return userEmails;
    }

    public async Task<List<string>> GetTeamLeadsEmailsAsync(CancellationToken cancellationToken = default)
    {
        var emails = await (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             join u in _context.Users on ur.UserId equals u.Id
                             where r.Name == "team_lead" && !ur.IsDeleted && !r.IsDeleted && !u.IsDeleted
                             select u.Email)
                            .Distinct()
                            .ToListAsync(cancellationToken);

        return emails;
    }

    public async Task<string?> GetTicketReporterEmailAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .Where(t => t.Id == ticketId && !t.IsDeleted)
            .Select(t => new { t.CreatedByUserId, t.ContactId })
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket == null)
            return null;

        if (ticket.ContactId.HasValue)
        {
            var email = await _context.Contacts
                .Where(c => c.Id == ticket.ContactId.Value && !c.IsDeleted)
                .Select(c => c.Email)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrEmpty(email))
                return email;
        }

        if (ticket.CreatedByUserId.HasValue)
        {
            var email = await _context.Users
                .Where(u => u.Id == ticket.CreatedByUserId.Value && !u.IsDeleted)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrEmpty(email))
                return email;
        }

        return null;
    }

    public async Task<string?> GetTicketAssigneeEmailAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .Where(t => t.Id == ticketId && !t.IsDeleted)
            .Select(t => t.AssignedAgentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket == null || !ticket.HasValue)
            return null;

        return await _context.Users
            .Where(u => u.Id == ticket.Value && !u.IsDeleted)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);
    }
    // Add this method to the bottom of your NotificationRepository class:
    public async Task<Guid> GetCompanyIdByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => t.CompanyId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<NotificationLog>> GetUnreadLogsAsync(string recipientEmail, CancellationToken cancellationToken)
    {
        try
        {
            // 🎯 THE FIX: Filters the logs so agents only get records matched to their specific email account
            return await _context.Set<NotificationLog>()
                .AsNoTracking()
                .Where(log => log.RecipientEmail.ToLower() == recipientEmail.ToLower())
                .OrderByDescending(log => log.SentAt)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Warning: Notification query stream was canceled by client subscription.");
            return new List<NotificationLog>();
        }
    }
}
