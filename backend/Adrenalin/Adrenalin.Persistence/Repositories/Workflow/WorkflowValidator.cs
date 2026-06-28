using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Workflow.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Workflow;

public sealed class WorkflowValidator : IWorkflowValidator
{
    private readonly AdrenalinDbContext _dbContext;
    private readonly IRoleNameLookup _roleNameLookup;

    public WorkflowValidator(AdrenalinDbContext dbContext, IRoleNameLookup roleNameLookup)
    {
        _dbContext = dbContext;
        _roleNameLookup = roleNameLookup;
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
        // If RequiresRoleId is null, anyone can perform it
        var roleList = userRoles.ToList();

        // Always allow admin
        if (roleList.Contains("admin", StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        // Resolve Access Level names only for the role IDs actually referenced by this
        // result set, via IRoleNameLookup — keeps Workflow.Domain free of any reference
        // to Auth.Domain.Entities.Role (StatusTransition only stores RequiresRoleId,
        // a bare Guid, never a navigation property to Role).
        var roleIdsNeeded = allowedTransitions
            .Where(t => t.RequiresRoleId.HasValue)
            .Select(t => t.RequiresRoleId!.Value)
            .Distinct()
            .ToList();

        var roleNames = roleIdsNeeded.Count > 0
            ? await _roleNameLookup.GetNamesByIdsAsync(roleIdsNeeded, cancellationToken)
            : new Dictionary<Guid, string>();

        foreach (var transition in allowedTransitions)
        {
            if (transition.RequiresRoleId is null)
            {
                return true;
            }

            if (roleNames.TryGetValue(transition.RequiresRoleId.Value, out var requiredRoleName) &&
                roleList.Contains(requiredRoleName, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}