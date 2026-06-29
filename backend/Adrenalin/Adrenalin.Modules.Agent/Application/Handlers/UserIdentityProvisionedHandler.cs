using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Agent.Application.Interfaces;
using Adrenalin.Modules.Agent.Domain.Entities;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Modules.Agent.Application.Handlers;

public class UserIdentityProvisionedHandler : IIntegrationEventHandler<UserIdentityProvisionedIntegrationEvent>
{
    private readonly IAgentRepository _repo;
    private readonly IEmailService _emailService;
    private readonly IEventBus _eventBus;
    public UserIdentityProvisionedHandler(IAgentRepository repo,IEmailService emailService, IEventBus eventBus)
    {
        _emailService = emailService;
        _repo = repo;
        _eventBus = eventBus;
    }

    // ✅ Matches the interface contract: HandleAsync + CancellationToken
    public async Task HandleAsync(UserIdentityProvisionedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        try
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


            await _emailService.SendAsync(
                @event.Email,
                "Welcome to the Support Team!",
                $"<h2>Hello {@event.DisplayName},</h2><p>Your agent profile has been set up. You can now access your dashboard.</p>"
            );
            await _eventBus.PublishAsync(new AgentProvisioningCompletedIntegrationEvent(
            @event.CorrelationId,
            @event.AdminEmail,
            @event.Email,
            @event.DisplayName
        ));
        }
        catch (Exception ex)
        {
            // 📢 Publish failure event if agent profile setup crashes
            await _eventBus.PublishAsync(new AgentProvisioningFailedIntegrationEvent(
                @event.CorrelationId,
                @event.AdminEmail,
                $"Profile assignment failed: {ex.Message}"
            ));
            throw;
        }
    }
}