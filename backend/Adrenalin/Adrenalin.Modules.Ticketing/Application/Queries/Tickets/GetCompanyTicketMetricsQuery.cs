using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Tickets;

public sealed record GetCompanyTicketMetricsQuery(Guid CompanyId, Guid ActorId) : IRequest<Result<CompanyTicketMetricsDto>>;
