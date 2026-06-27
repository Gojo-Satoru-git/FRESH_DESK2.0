namespace Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;

public record AssignTicketResult(
    Guid? AssignedAgentId,
    Guid? AssignedGroupId,
    string RuleMatched,
    string ActionApplied
);