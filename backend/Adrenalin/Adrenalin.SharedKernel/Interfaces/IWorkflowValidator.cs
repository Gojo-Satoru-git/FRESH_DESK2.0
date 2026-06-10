using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.SharedKernel.Interfaces;

public interface IWorkflowValidator
{
    Task<bool> IsTransitionAllowedAsync(
        Guid graphId,
        string? fromStatus,
        string toStatus,
        IEnumerable<string> userRoles,
        CancellationToken cancellationToken = default);
}
