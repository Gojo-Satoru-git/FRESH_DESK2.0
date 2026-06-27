using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Tickets;

public sealed record GetTicketHistoryQuery(Guid TicketId) : IRequest<TicketHistoryDto>;
