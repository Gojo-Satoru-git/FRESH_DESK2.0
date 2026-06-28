using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Agent.Domain.Entities;

namespace Adrenalin.Modules.Agent.Application.Interfaces;

public interface IAgentRepository
{
    Task<AgentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(AgentEntity agent, CancellationToken cancellationToken = default);
    Task AddAvailabilityAsync(AgentAvailability availability, CancellationToken cancellationToken = default);
    Task<IEnumerable<AgentEntity>> GetFilteredListAsync(string? search, string? status, Guid? roleId, Guid? teamId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}