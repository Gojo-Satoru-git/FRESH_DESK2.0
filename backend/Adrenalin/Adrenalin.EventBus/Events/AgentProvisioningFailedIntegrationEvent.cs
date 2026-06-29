namespace Adrenalin.EventBus.Events;


public record AgentProvisioningFailedIntegrationEvent(
    Guid CorrelationId,
    string AdminEmail,
    string ErrorMessage);