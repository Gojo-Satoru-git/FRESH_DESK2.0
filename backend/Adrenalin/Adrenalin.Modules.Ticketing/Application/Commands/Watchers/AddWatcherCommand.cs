using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record AddWatcherCommand(
    Guid TicketId,
    Guid UserId,
    Guid AddedBy
) : IRequest<Guid>;