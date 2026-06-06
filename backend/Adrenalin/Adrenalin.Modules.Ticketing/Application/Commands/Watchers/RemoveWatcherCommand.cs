using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record RemoveWatcherCommand(
    Guid TicketId,
    Guid UserId
) : IRequest<Guid>;