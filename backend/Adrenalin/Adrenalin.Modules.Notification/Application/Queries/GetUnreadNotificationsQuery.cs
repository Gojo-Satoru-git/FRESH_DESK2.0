using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Notification.Domain.Entities;
using Adrenalin.Modules.Notification.Domain.Interfaces;

namespace Adrenalin.Modules.Notification.Application.Queries;

// 1. Define the Query Structure contract
// ⚡ FIXED: Added a parameter slot to accept the requesting agent's email string context
public record GetUnreadNotificationsQuery(string RecipientEmail) : IRequest<List<NotificationLog>>;

// 2. Define the Query Handler Execution Layer
public sealed class GetUnreadNotificationsQueryHandler
    : IRequestHandler<GetUnreadNotificationsQuery, List<NotificationLog>>
{
    private readonly INotificationRepository _repository;

    public GetUnreadNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public Task<List<NotificationLog>> Handle(GetUnreadNotificationsQuery query, CancellationToken ct)
    {
        // ⚡ FIXED: Notification Center is not implemented yet. 
        // Return an empty list to avoid querying the NotificationLog table (which is for email logs).
        return Task.FromResult(new List<NotificationLog>());
    }
}