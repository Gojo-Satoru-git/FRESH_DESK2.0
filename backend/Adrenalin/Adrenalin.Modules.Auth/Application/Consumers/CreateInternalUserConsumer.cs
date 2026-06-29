using System;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Handlers;
using Adrenalin.SharedKernel.Interfaces; // ◄ Adjust namespace to match your IUnitOfWork location

namespace Adrenalin.Modules.Auth.consumers;

public class CreateInternalUserConsumer : IIntegrationEventHandler<CreateInternalUserIntegrationEvent>
{
    private readonly CreateInternalUserCommandHandler _authHandler;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork; // ◄ Using interface tracking instead of DB context directly

    public CreateInternalUserConsumer(
        CreateInternalUserCommandHandler authHandler,
        IEventBus eventBus,
        IUnitOfWork unitOfWork) // ◄ Inject interface abstraction
    {
        _authHandler = authHandler;
        _eventBus = eventBus;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CreateInternalUserIntegrationEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var internalCommand = new CreateInternalUserCommand(
                @event.Email,
                @event.FirstName,
                @event.LastName,
                @event.Phone ?? string.Empty,
                @event.RoleName,
                @event.AdminId,
                IsSystemCall: true
            );

            Guid newUserId = await _authHandler.Handle(internalCommand, cancellationToken);

            // ✅ Safely persist the new identity state through the interface contract layer
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var successEvent = new UserIdentityProvisionedIntegrationEvent(
                @event.CorrelationId,
                @event.AdminEmail,
                newUserId,
                @event.Email,
                $"{@event.FirstName} {@event.LastName}",
                @event.ShiftFactorValueId,
                @event.Timezone,
                @event.ShiftStart,
                @event.ShiftEnd,
                @event.WorkingDays,
                @event.MaxConcurrentTickets
            );

            await _eventBus.PublishAsync(successEvent);
        }
        catch (Exception ex)
        {
           
            await _eventBus.PublishAsync(new AgentProvisioningFailedIntegrationEvent(
                @event.CorrelationId,
                @event.AdminEmail,
                $"Identity setup failed: {ex.Message}"
            ));
        }
    }
}