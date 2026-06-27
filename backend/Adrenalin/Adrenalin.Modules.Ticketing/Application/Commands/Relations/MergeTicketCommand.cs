using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Relations;

public sealed record MergeTicketCommand(
    Guid MasterTicketId,
    Guid DuplicateTicketId,
    Guid MergedBy
) : IRequest<Guid>;