using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Workflow.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Workflow;

public sealed class RoleNameLookup : IRoleNameLookup
{
    private readonly AdrenalinDbContext _db;
    public RoleNameLookup(AdrenalinDbContext db) => _db = db;

    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var ids = roleIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string>();

        return await _db.Roles
            .Where(r => ids.Contains(r.Id) && !r.IsDeleted)
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);
    }
}