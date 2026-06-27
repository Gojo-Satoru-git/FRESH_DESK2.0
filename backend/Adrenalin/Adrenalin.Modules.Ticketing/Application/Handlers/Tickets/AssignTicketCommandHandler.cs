using System.Text.Json;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Results;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Application.DTOs.Tickets;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public sealed class AssignTicketCommandHandler
    : IRequestHandler<AssignTicketCommand, Result<AssignTicketResult>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly IAutomationRuleRepository _ruleRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutomationConditionEvaluator _evaluator;
    private readonly ITicketRoutingEngine _routingEngine;
    private readonly IGroupAssignmentHistoryRepository _historyRepo;
    private readonly ITicketRoutingContextRepository _routingContext;

    public AssignTicketCommandHandler(
        ITicketRepository ticketRepo,
        IAutomationRuleRepository ruleRepo,
        IUnitOfWork unitOfWork,
        AutomationConditionEvaluator evaluator,
        ITicketRoutingEngine routingEngine,
        IGroupAssignmentHistoryRepository historyRepo,
        ITicketRoutingContextRepository routingContext)
    {
        _ticketRepo = ticketRepo;
        _ruleRepo = ruleRepo;
        _unitOfWork = unitOfWork;
        _evaluator = evaluator;
        _routingEngine = routingEngine;
        _historyRepo = historyRepo;
        _routingContext = routingContext;
    }

    public async Task<Result<AssignTicketResult>> Handle(
        AssignTicketCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Load ticket
        var ticket = await _ticketRepo
            .GetByIdAsync(command.TicketId, cancellationToken);

        if (ticket is null)
            return Result<AssignTicketResult>
                .Failure("Ticket not found");

        Guid? previousGroupId = ticket.GroupId;
        Guid? newAgentId = null;
        Guid? newGroupId = null;
        string ruleMatched = "None";
        string actionApplied = "None";

        if (!command.IsAutoAssignment)
        {
            // ── Manual Assignment ─────────────────────────────────────────
            newAgentId = command.OverrideAgentId;
            newGroupId = command.OverrideGroupId;

            // Validate: if assigning an agent to a group, ensure the agent belongs to that group
            if (newAgentId.HasValue && newGroupId.HasValue)
            {
                var isMember = await _routingContext.IsUserInGroupAsync(newAgentId.Value, newGroupId.Value, cancellationToken);
                if (!isMember)
                {
                    return Result<AssignTicketResult>.Failure(
                        "Cannot assign agent to a ticket in a group they do not belong to.");
                }
            }

            ruleMatched = "ManualAssignment";
            actionApplied = "ManualOverride";
        }
        else
        {
            // ── Auto Assignment ───────────────────────────────────────────

            // Step 1: Try the enterprise routing engine (4-tier cascade)
            var routingResult = await _routingEngine.RouteAsync(ticket, cancellationToken);
            if (routingResult.GroupId.HasValue)
            {
                newGroupId = routingResult.GroupId;
                newAgentId = routingResult.AgentId;
                ruleMatched = routingResult.MatchedRule;
                actionApplied = $"RoutingEngine: {routingResult.RuleDescription}";
            }
            else
            {
                // Step 2: Fallback to existing AutomationRules
                var rules = await _ruleRepo
                    .GetActiveRulesForTriggerAsync(cancellationToken);

                foreach (var rule in rules)
                {
                    var conditions = JsonDocument
                        .Parse(rule.Conditions)
                        .RootElement;

                    if (!_evaluator.Evaluate(conditions, ticket))
                        continue;

                    var actions = JsonDocument
                        .Parse(rule.Actions)
                        .RootElement;

                    foreach (var action in actions.EnumerateArray())
                    {
                        if (!action.TryGetProperty("action", out var actionProp))
                            continue;

                        var actionType = actionProp.GetString();

                        switch (actionType)
                        {
                            case "assign_group":
                                if (action.TryGetProperty("value", out var groupVal))
                                {
                                    newGroupId = Guid.Parse(groupVal.GetString()!);
                                    newAgentId = await _ticketRepo
                                        .GetLeastLoadedAgentInGroupAsync(
                                            newGroupId.Value,
                                            cancellationToken);
                                }
                                break;

                            case "assign_agent":
                                if (action.TryGetProperty("value", out var agentVal))
                                {
                                    newAgentId = Guid.Parse(agentVal.GetString()!);
                                }
                                break;

                            case "assign_group_only":
                                if (action.TryGetProperty("value", out var groupOnlyVal))
                                {
                                    newGroupId = Guid.Parse(groupOnlyVal.GetString()!);
                                }
                                break;
                        }
                    }

                    ruleMatched = rule.Name;
                    actionApplied = $"AutomationRule: {rule.Name}";

                    await _ruleRepo.LogExecutionAsync(
                        rule.Id,
                        ticket.Id,
                        actionApplied,
                        cancellationToken);

                    break; // first match wins
                }
            }
        }

        // 2. Update ticket via domain methods
        await _ticketRepo.UpdateAssignmentAsync(
            ticketId: ticket.Id,
            agentId: newAgentId,
            groupId: newGroupId,
            triggeredBy: command.TriggeredBy,
            notes: actionApplied,
            clearAgentIfNull: !command.IsAutoAssignment && !command.OverrideAgentId.HasValue,
            ct: cancellationToken);

        // 3. Record group assignment history if group changed
        if (newGroupId.HasValue && newGroupId != previousGroupId)
        {
            _historyRepo.Add(GroupAssignmentHistory.Create(
                ticketId: ticket.Id,
                previousGroupId: previousGroupId,
                newGroupId: newGroupId,
                assignedBy: command.TriggeredBy,
                reason: actionApplied,
                routingRuleMatched: ruleMatched));
        }

        // 4. Commit
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AssignTicketResult>.Success(
            new AssignTicketResult(
                newAgentId,
                newGroupId,
                ruleMatched,
                actionApplied));
    }
}
