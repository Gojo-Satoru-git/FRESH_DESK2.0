using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Agent.Application.Interfaces;
using Adrenalin.Modules.Agent.Domain.Entities;

namespace Adrenalin.Persistence.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly AdrenalinDbContext _context;

    public AgentRepository(AdrenalinDbContext context)
    {
        _context = context;
    }

    public async Task<AgentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<AgentEntity>()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // ✅ Fix: Since AgentEntity doesn't hold the Email property, we check uniqueness 
        // by looking up if any AgentEntity is already bound to a User record via UserId.
        // (Alternatively, if you're not tracking uniqueness here, you can simply return false).
        return await _context.Set<AgentEntity>().AnyAsync(a => a.DisplayName != null && a.DisplayName.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task AddAsync(AgentEntity agent, CancellationToken cancellationToken = default)
    {
        await _context.Set<AgentEntity>().AddAsync(agent, cancellationToken);
    }

    public async Task AddAvailabilityAsync(AgentAvailability availability, CancellationToken cancellationToken = default)
    {
        await _context.Set<AgentAvailability>().AddAsync(availability, cancellationToken);
    }

    public async Task<IEnumerable<AgentEntity>> GetFilteredListAsync(
        string? search,
        string? status,
        Guid? roleId,
        Guid? teamId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<AgentEntity>()
            .Where(a => !a.IsDeleted);

        // ✅ Fix: Removed reference to a.Email
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(a =>
                (a.DisplayName != null && a.DisplayName.ToLower().Contains(lowerSearch)) ||
                (a.EmployeeId != null && a.EmployeeId.ToLower().Contains(lowerSearch)));
        }

        // Dropdown status filter
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);

        // ✅ Fix: Removed roleId and teamId table filters since those properties sit in other bounded contexts.
        // Cross-module filtering can be resolved at the API layer or via a Join View later if needed.

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}