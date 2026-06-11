using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Ticketing.Domain.Events;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class TicketCreatedDomainEventHandler : INotificationHandler<TicketCreatedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public TicketCreatedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(TicketCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new TicketCreatedIntegrationEvent(
            notification.TicketId,
            notification.TicketNumber ?? string.Empty,
            notification.Title,
            notification.ReporterId,
            notification.AssigneeId,
            notification.Type.ToString(),
            notification.Priority.ToString(),
            null, // Department (removed)
            null  // Region (removed)
        ), cancellationToken);
    }
}

public sealed class TicketAssignedDomainEventHandler : INotificationHandler<TicketAssignedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public TicketAssignedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(TicketAssignedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new TicketAssignedIntegrationEvent(
            notification.TicketId,
            notification.TicketNumber ?? string.Empty,
            notification.AssigneeId,
            notification.AssignedBy,
            "Ticket Assigned"
        ), cancellationToken);
    }
}

public sealed class TicketStatusChangedDomainEventHandler : INotificationHandler<TicketStatusChangedDomainEvent>
{
    private readonly IEventBus _eventBus;
    private readonly Adrenalin.Modules.Ticketing.Domain.Interfaces.ITicketRepository _ticketRepository;

    public TicketStatusChangedDomainEventHandler(IEventBus eventBus, Adrenalin.Modules.Ticketing.Domain.Interfaces.ITicketRepository ticketRepository)
    {
        _eventBus = eventBus;
        _ticketRepository = ticketRepository;
    }

    public async Task Handle(TicketStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(notification.TicketId, cancellationToken);
        var ticketNumber = ticket?.TicketNumber ?? "";

        await _eventBus.PublishAsync(new TicketStatusChangedIntegrationEvent(
            notification.TicketId,
            ticketNumber,
            notification.OldStatus.ToString(),
            notification.NewStatus.ToString(),
            notification.ChangedBy,
            "Status Changed"
        ), cancellationToken);
    }
}

public sealed class TicketCommentAddedDomainEventHandler : INotificationHandler<TicketCommentAddedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public TicketCommentAddedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(TicketCommentAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new TicketCommentAddedIntegrationEvent(
            notification.TicketId,
            notification.CommentId,
            notification.Body,
            notification.MentionedUsers.ToList(),
            null,
            null,
            "Public"
        ), cancellationToken);
    }
}

public sealed class TicketResolvedDomainEventHandler : INotificationHandler<TicketResolvedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public TicketResolvedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(TicketResolvedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new TicketResolvedIntegrationEvent(
            notification.TicketId,
            notification.TicketNumber ?? string.Empty,
            notification.ResolvedBy,
            "Ticket Resolved"
        ), cancellationToken);
    }
}

public sealed class TicketClosedDomainEventHandler : INotificationHandler<TicketClosedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public TicketClosedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(TicketClosedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new TicketClosedIntegrationEvent(
            notification.TicketId,
            notification.TicketNumber ?? string.Empty,
            notification.ClosedBy,
            "Ticket Closed"
        ), cancellationToken);
    }
}

public sealed class TicketReopenedDomainEventHandler : INotificationHandler<TicketReopenedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public TicketReopenedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(TicketReopenedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new TicketReopenedIntegrationEvent(
            notification.TicketId,
            notification.TicketNumber ?? string.Empty,
            notification.ReopenedBy,
            "Ticket Reopened"
        ), cancellationToken);
    }
}

public sealed class TicketMergedDomainEventHandler : INotificationHandler<TicketMergedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public TicketMergedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task Handle(TicketMergedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _eventBus.PublishAsync(new TicketMergedIntegrationEvent(
            notification.TicketId,
            notification.TicketNumber ?? string.Empty,
            notification.MasterTicketNumber,
            notification.MergedBy
        ), cancellationToken);
    }
}
