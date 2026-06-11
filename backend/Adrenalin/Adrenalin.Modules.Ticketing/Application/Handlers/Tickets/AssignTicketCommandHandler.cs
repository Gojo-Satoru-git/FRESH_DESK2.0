using System.Text.Json;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Results;
using Adrenalin.SharedKernel.Contracts;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public sealed class AssignTicketCommandHandler
    : IRequestHandler<AssignTicketCommand, Result<AssignTicketResult>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly IAutomationRuleRepository _ruleRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutomationConditionEvaluator _evaluator;

    public AssignTicketCommandHandler(
        ITicketRepository ticketRepo,
        IAutomationRuleRepository ruleRepo,
        IUnitOfWork unitOfWork,
        AutomationConditionEvaluator evaluator)
    {
        _ticketRepo = ticketRepo;
        _ruleRepo = ruleRepo;
        _unitOfWork = unitOfWork;
        _evaluator = evaluator;
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

        Guid? newAgentId = null;
        Guid? newGroupId = null;
        string ruleMatched = "None";
        string actionApplied = "None";

        if (!command.IsAutoAssignment)
        {
            // Manual override
            newAgentId = command.OverrideAgentId;
            newGroupId = command.OverrideGroupId;
            ruleMatched = "ManualAssignment";
            actionApplied = "ManualOverride";
        }
        else
        {
            // Auto — evaluate rules
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
                    var actionType = action
                        .GetProperty("action").GetString();
                    var actionValue = action
                        .GetProperty("value").GetString();

                    switch (actionType)
                    {
                        case "assign_group":
                            newGroupId = Guid.Parse(actionValue!);
                            newAgentId = await _ticketRepo
                                .GetLeastLoadedAgentInGroupAsync(
                                    newGroupId.Value,
                                    cancellationToken);
                            break;

                        case "assign_agent":
                            newAgentId = Guid.Parse(actionValue!);
                            break;

                        case "assign_group_only":
                            newGroupId = Guid.Parse(actionValue!);
                            break;
                    }
                }

                ruleMatched = rule.Name;
                actionApplied = $"AutoAssigned via: {rule.Name}";

                await _ruleRepo.LogExecutionAsync(
                    rule.Id,
                    ticket.Id,
                    actionApplied,
                    cancellationToken);

                break; // first match wins
            }
        }

        // 2. Update ticket via domain methods
        await _ticketRepo.UpdateAssignmentAsync(
            ticketId: ticket.Id,
            agentId: newAgentId,
            groupId: newGroupId,
            triggeredBy: command.TriggeredBy,
            cancellationToken);

        // 3. Commit
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AssignTicketResult>.Success(
            new AssignTicketResult(
                newAgentId,
                newGroupId,
                ruleMatched,
                actionApplied));
    }
}