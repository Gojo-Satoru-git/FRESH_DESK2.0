using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Workflow.Domain.Interfaces;

public interface IRoleNameLookup
{
    Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
}