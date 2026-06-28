using System;

namespace Adrenalin.EventBus.Events;

public record CreateInternalUserIntegrationEvent(
    Guid CorrelationId,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string RoleName,
    Guid ShiftFactorValueId,
    string Timezone,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    string[] WorkingDays,
    int MaxConcurrentTickets
);