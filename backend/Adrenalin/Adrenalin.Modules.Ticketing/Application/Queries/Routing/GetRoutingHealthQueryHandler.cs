using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Queries.Routing;

public sealed class GetRoutingHealthQueryHandler : IRequestHandler<GetRoutingHealthQuery, Result<RoutingHealthResult>>
{
    private readonly IEmailAliasRoutingRepository _routingRepository;
    // Potentially ITicketRepository to verify company mapping or Group repository for broken mappings

    public GetRoutingHealthQueryHandler(IEmailAliasRoutingRepository routingRepository)
    {
        _routingRepository = routingRepository;
    }

    public async Task<Result<RoutingHealthResult>> Handle(GetRoutingHealthQuery request, CancellationToken cancellationToken)
    {
        var allRoutes = await _routingRepository.GetAllAsync(cancellationToken);
        
        var warnings = new List<string>();
        var criticals = new List<string>();

        if (!allRoutes.Any())
        {
            warnings.Add("No routing rules configured. The system might reject incoming emails or fail to route them.");
        }

        // 1. Broken mappings
        foreach (var route in allRoutes)
        {
            if (route.GroupId == Guid.Empty)
            {
                warnings.Add($"Alias {route.EmailAddress} has an empty GroupId. Emails might fail to assign properly.");
            }
            if (string.IsNullOrWhiteSpace(route.EmailAddress))
            {
                criticals.Add($"A routing rule with ID {route.Id} has an empty EmailAddress.");
            }
            else if (!route.EmailAddress.Contains("@"))
            {
                criticals.Add($"Alias {route.EmailAddress} has an invalid email format.");
            }
        }

        // 2. Duplicate Priorities within same company
        var groupedByCompany = allRoutes.GroupBy(r => r.CompanyId);
        foreach (var companyGroup in groupedByCompany)
        {
            var duplicatePriorities = companyGroup.GroupBy(r => r.Priority).Where(g => g.Count() > 1).ToList();
            if (duplicatePriorities.Any())
            {
                string companyStr = companyGroup.Key?.ToString() ?? "Global/System";
                warnings.Add($"Duplicate priority found for Company {companyStr}.");
            }
        }

        string status = criticals.Any() ? "Critical" : (warnings.Any() ? "Warning" : "Healthy");

        return Result<RoutingHealthResult>.Success(new RoutingHealthResult(status, warnings, criticals));
    }
}
