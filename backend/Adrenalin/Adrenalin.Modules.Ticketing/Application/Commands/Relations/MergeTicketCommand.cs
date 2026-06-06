using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record MergeTicketCommand(
    Guid MasterTicketId,
    Guid DuplicateTicketId,
    Guid MergedBy
) : IRequest<Guid>;