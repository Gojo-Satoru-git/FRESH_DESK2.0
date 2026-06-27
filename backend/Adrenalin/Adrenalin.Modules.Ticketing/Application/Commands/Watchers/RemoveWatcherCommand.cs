using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Watchers;

public sealed record RemoveWatcherCommand(
    Guid TicketId,
    Guid UserId
) : IRequest<Guid>;