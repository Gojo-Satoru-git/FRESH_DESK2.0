using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class WorkflowValidator : IWorkflowValidator
{
    private readonly AdrenalinDbContext _dbContext;

    public WorkflowValidator(AdrenalinDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsTransitionAllowedAsync(
        Guid graphId,
        string? fromStatus,
        string toStatus,
        IEnumerable<string> userRoles,
        CancellationToken cancellationToken = default)
    {
        // Find matching transitions
        var query = _dbContext.Set<Adrenalin.Modules.Workflow.Domain.Entities.StatusTransition>()
            .Where(t => t.GraphId == graphId && t.ToStatus == toStatus);

        // FromStatus can be null on ticket creation/initial state transitions
        if (fromStatus == null)
        {
            query = query.Where(t => t.FromStatus == null);
        }
        else
        {
            query = query.Where(t => t.FromStatus == fromStatus);
        }

        var allowedTransitions = await query.ToListAsync(cancellationToken);

        if (!allowedTransitions.Any())
        {
            return false;
        }

        // Check if any of the transitions are permitted by the user's roles
        // If RequiresRole is null, anyone can perform it
        var roleList = userRoles.ToList();
        
        // Always allow admin
        if (roleList.Contains("admin", StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var transition in allowedTransitions)
        {
            if (string.IsNullOrWhiteSpace(transition.RequiresRole))
            {
                return true;
            }

            if (roleList.Contains(transition.RequiresRole, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
