using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Agent.Application.Interfaces;
using Adrenalin.Modules.Agent.Domain.Entities;

namespace Adrenalin.Modules.Agent.Application.Handlers;

public class UserIdentityProvisionedHandler : IIntegrationEventHandler<UserIdentityProvisionedIntegrationEvent>
{
    private readonly IAgentRepository _repo;

    public UserIdentityProvisionedHandler(IAgentRepository repo)
    {
        _repo = repo;
    }

    // ✅ Matches the interface contract: HandleAsync + CancellationToken
    public async Task HandleAsync(UserIdentityProvisionedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        if (await _repo.ExistsByEmailAsync(@event.Email, cancellationToken)) return;

        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            UserId = @event.UserId,
            DisplayName = @event.DisplayName,
            Status = "Active",
            StatusSince = DateTimeOffset.UtcNow,
            MaxConcurrentTickets = @event.MaxConcurrentTickets
        };

        var availability = new AgentAvailability
        {
            Agent = agent,
            ShiftFactorValueId = @event.ShiftFactorValueId,
            Timezone = @event.Timezone,
            ShiftStart = @event.ShiftStart,
            ShiftEnd = @event.ShiftEnd,
            WorkingDays = JsonSerializer.Serialize(@event.WorkingDays)
        };

        await _repo.AddAsync(agent, cancellationToken);
        await _repo.AddAvailabilityAsync(availability, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }
}