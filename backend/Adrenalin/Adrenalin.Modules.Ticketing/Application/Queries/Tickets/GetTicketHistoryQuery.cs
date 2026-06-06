using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public sealed record GetTicketHistoryQuery(Guid TicketId) : IRequest<TicketHistoryDto>;
