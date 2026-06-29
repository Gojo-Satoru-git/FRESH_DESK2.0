namespace Adrenalin.EventBus.Events;


public record AgentProvisioningCompletedIntegrationEvent(
    Guid CorrelationId,
    string AdminEmail,
    string AgentEmail,
    string DisplayName);