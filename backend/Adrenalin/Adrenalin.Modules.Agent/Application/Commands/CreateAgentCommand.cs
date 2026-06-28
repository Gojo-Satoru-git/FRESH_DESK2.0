using System;

namespace Adrenalin.Modules.Agent.Application.Commands;

public record CreateAgentCommand(
    string DisplayName,
    string Email,
    string? Phone,
    string AccessLevel, // Maps to "Admin", "Manager", "Agent", "Supervisor"
    Guid ShiftFactorValueId,
    string Timezone,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    string[] WorkingDays,
    int MaxConcurrentTickets = 10
);