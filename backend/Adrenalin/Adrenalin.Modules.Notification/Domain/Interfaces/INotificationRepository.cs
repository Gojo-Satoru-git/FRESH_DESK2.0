using System;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Notification.Domain.Entities;

namespace Adrenalin.Modules.Notification.Domain.Interfaces;

public interface INotificationRepository
{
    Task<NotificationTemplate?> GetTemplateByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddLogAsync(NotificationLog log, CancellationToken cancellationToken = default);
    Task AddLogsAsync(IEnumerable<NotificationLog> logs, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<string?> ResolveEmailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, string>> ResolveEmailsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<System.Collections.Generic.List<string>> GetTeamLeadsEmailsAsync(CancellationToken cancellationToken = default);
    Task<string?> GetTicketReporterEmailAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<string?> GetTicketAssigneeEmailAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<Guid> GetCompanyIdByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<List<NotificationLog>> GetUnreadLogsAsync(string recipientEmail, CancellationToken cancellationToken);
}
