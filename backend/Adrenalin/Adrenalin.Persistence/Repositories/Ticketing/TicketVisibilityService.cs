using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;

namespace Adrenalin.Persistence.Repositories;

public sealed class TicketVisibilityService : ITicketVisibilityService
{
    private readonly AdrenalinDbContext _context;

    public TicketVisibilityService(AdrenalinDbContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<Ticket>> ApplyVisibilityFilterAsync(IQueryable<Ticket> query, Guid userId, CancellationToken cancellationToken)
    {
        // 1. Get all permission strings for the user
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions, 
                  ur => ur.RoleId, 
                  rp => rp.RoleId, 
                  (ur, rp) => rp.PermissionId)
            .Join(_context.Set<Adrenalin.Modules.Auth.Domain.Entities.Permission>(),
                  pId => pId,
                  p => p.Id,
                  (pId, p) => p.Resource + ":" + p.Action)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 2. Global view (Admin / Management)
        if (permissions.Contains("ticket:manage_all") || permissions.Contains("system:admin"))
        {
            return query;
        }

        // 3. Group/Queue view (Team Lead / PMO)
        if (permissions.Contains("ticket:read_team") || permissions.Contains("ticket:read_queue"))
        {
            var userGroups = await _context.UserGroups
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GroupId)
                .ToListAsync(cancellationToken);

            return query.Where(t => t.GroupId.HasValue && userGroups.Contains(t.GroupId.Value));
        }

        // 4. Agent view (Junior/Senior Agent)
        if (permissions.Contains("ticket:manage"))
        {
            return query.Where(t => t.AssignedAgentId == userId);
        }

        // 5. Customer Admin view
        if (permissions.Contains("ticket:view_company"))
        {
            var companyId = await _context.Contacts
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Select(c => c.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (companyId != Guid.Empty)
            {
                return query.Where(t => t.CompanyId == companyId);
            }
            // fallback for Customer Admin without a valid company
            var adminContactId = await _context.Contacts
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return query.Where(t => t.ContactId == adminContactId);
        }

        // 6. Default Customer view
        var contactId = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return query.Where(t => t.ContactId == contactId);
    }

    public async Task<bool> IsInternalAgentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var hasInternalPerm = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionId)
            .Join(_context.Set<Adrenalin.Modules.Auth.Domain.Entities.Permission>(), pid => pid, p => p.Id, (pid, p) => p.Resource + ":" + p.Action)
            .AnyAsync(perm => perm == "ticket:manage" || perm == "ticket:manage_all" || perm == "system:admin" || perm == "ticket:read_team" || perm == "ticket:read_queue", cancellationToken);
            
        return hasInternalPerm;
    }
}
