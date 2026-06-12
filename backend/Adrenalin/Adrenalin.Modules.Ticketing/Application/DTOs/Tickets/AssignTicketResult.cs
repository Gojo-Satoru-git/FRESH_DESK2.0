namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public record AssignTicketResult(
    Guid? AssignedAgentId,
    Guid? AssignedGroupId,
    string RuleMatched,
    string ActionApplied
);