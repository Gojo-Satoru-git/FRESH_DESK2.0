using System;

namespace Adrenalin.EventBus.Events;

public record UserIdentityProvisionedIntegrationEvent(
    Guid CorrelationId,
    string AdminEmail,
    Guid UserId,
    string Email,
    string DisplayName,
    Guid ShiftFactorValueId,
    string Timezone,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    string[] WorkingDays,
    int MaxConcurrentTickets
);