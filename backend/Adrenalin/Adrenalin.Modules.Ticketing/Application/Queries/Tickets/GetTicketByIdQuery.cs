using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Tickets;

public sealed record GetTicketByIdQuery(
    Guid TicketId,
    bool IncludeInternalComments = true
) : IRequest<GetTicketByIdResponse>;