using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Routing;

public record RoutingHealthResult(string Status, List<string> Warnings, List<string> Criticals);

public record GetRoutingHealthQuery() : IRequest<Result<RoutingHealthResult>>;
