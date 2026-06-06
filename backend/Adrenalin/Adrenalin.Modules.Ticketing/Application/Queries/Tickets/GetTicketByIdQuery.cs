using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public sealed record GetTicketByIdQuery(
    Guid TicketId,
    bool InclueInternalComments = true
) : IRequest<GetTicketByIdResponse>;