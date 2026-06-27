using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands.Relations;

public sealed record AddRelationCommand(
    Guid ParentTicketId,
    Guid ChildTicketId,
    TicketRelationType RelationType
) : IRequest<Guid>;
