using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Adrenalin.Persistence.Context;

namespace Adrenalin.unify.API.HealthChecks;

public class RoutingHealthCheck : IHealthCheck
{
    private readonly AdrenalinDbContext _db;

    public RoutingHealthCheck(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // A healthy routing configuration should not have circular fallback references
        // and ideally have at least one fallback strategy per company routing rule or group.
        
        bool hasCircularReferences = false;

        var groups = await _db.Groups.IgnoreQueryFilters()
            .Where(g => !g.IsDeleted && g.FallbackGroupId != null)
            .Select(g => new { g.Id, g.FallbackGroupId })
            .ToListAsync(cancellationToken);

        var groupDict = groups.ToDictionary(g => g.Id, g => g.FallbackGroupId);

        foreach (var group in groups)
        {
            var visited = new System.Collections.Generic.HashSet<System.Guid>();
            var currentFallback = group.FallbackGroupId;

            while (currentFallback.HasValue)
            {
                if (!visited.Add(currentFallback.Value))
                {
                    hasCircularReferences = true;
                    break;
                }

                if (groupDict.TryGetValue(currentFallback.Value, out var nextFallback))
                {
                    currentFallback = nextFallback;
                }
                else
                {
                    currentFallback = null;
                }
            }

            if (hasCircularReferences) break;
        }

        if (hasCircularReferences)
        {
            return HealthCheckResult.Degraded("Circular fallback references detected in Group configurations.");
        }

        return HealthCheckResult.Healthy("Routing configuration is healthy.");
    }
}
