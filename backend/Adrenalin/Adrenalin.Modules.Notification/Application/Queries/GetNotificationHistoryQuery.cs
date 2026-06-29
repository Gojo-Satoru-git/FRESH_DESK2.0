using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Interfaces;

namespace Adrenalin.Modules.Notification.Application.Queries;

// 1. The Contract
public record GetNotificationHistoryQuery(string RecipientEmail) : IRequest<List<NotificationLog>>;

// 2. The Handler Execution Layer
public sealed class GetNotificationHistoryQueryHandler
    : IRequestHandler<GetNotificationHistoryQuery, List<NotificationLog>>
{
    private readonly INotificationRepository _repository;

    public GetNotificationHistoryQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationLog>> Handle(GetNotificationHistoryQuery query, CancellationToken ct)
    {
        // Fetches all matching data records cleanly from the DB
        var logs = await _repository.GetUnreadLogsAsync(query.RecipientEmail, ct);
        return logs ?? new List<NotificationLog>();
    }
}