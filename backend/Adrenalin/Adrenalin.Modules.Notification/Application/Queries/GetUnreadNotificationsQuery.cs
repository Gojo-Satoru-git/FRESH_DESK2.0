using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Interfaces;

namespace Adrenalin.Modules.Notification.Application.Queries;

public record GetUnreadNotificationsQuery(string RecipientEmail) : IRequest<List<NotificationLog>>;

public sealed class GetUnreadNotificationsQueryHandler
    : IRequestHandler<GetUnreadNotificationsQuery, List<NotificationLog>>
{
    private readonly INotificationRepository _repository;

    public GetUnreadNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationLog>> Handle(GetUnreadNotificationsQuery query, CancellationToken ct)
    {
        // ✅ Read real live audit traces from the database logs repository table context
        // This filters down to logs sent to the active authenticated admin that haven't failed.
        var logs = await _repository.GetUnreadLogsAsync(query.RecipientEmail, ct);

        return logs.Where(n => !n.IsFailedDelivery)
            .ToList() ?? new List<NotificationLog>();
    }
}