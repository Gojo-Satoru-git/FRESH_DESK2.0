using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public sealed record GetCompanyTicketMetricsQuery(Guid CompanyId, Guid ActorId) : IRequest<Result<CompanyTicketMetricsDto>>;
