using System.Text.Json;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Modules.Ticketing.Application.Handlers.Tickets;

public class AutomationConditionEvaluator
{
    public bool Evaluate(JsonElement conditions, Ticket ticket)
    {
        if (conditions.ValueKind != JsonValueKind.Array)
            return true;

        foreach (var condition in conditions.EnumerateArray())
        {
            var field = condition
                .GetProperty("field").GetString() ?? "";
            var op = condition
                .GetProperty("operator").GetString() ?? "";
            var value = condition
                .GetProperty("value").GetString() ?? "";

            if (!EvaluateSingle(field, op, value, ticket))
                return false;
        }
        return true;
    }

    private static bool EvaluateSingle(
        string field, string op,
        string value, Ticket ticket)
    {
        // Match exact column names from ticket.tickets
        var ticketValue = field switch
        {
            "module_id" => ticket.ModuleId.ToString(),
            "sub_module_id" => ticket.SubModuleId?.ToString(),
            "company_id" => ticket.CompanyId.ToString(),
            "group_id" => ticket.GroupId?.ToString(),
            "assigned_agent_id" => ticket.AssignedAgentId?.ToString(),
            "is_on_hold_payment" => ticket.IsOnHoldPayment
                                        .ToString().ToLower(),
            "sla_excluded" => ticket.SlaExcluded
                                        .ToString().ToLower(),
            "force_p1" => ticket.ForceP1
                                        .ToString().ToLower(),
            // ENUMs — compare lowercase string value
            "priority" => ticket.PriorityScore?
                                        .ToString().ToLower(),
            _ => null
        };

        return op switch
        {
            "eq" => ticketValue == value,
            "neq" => ticketValue != value,
            "contains" => ticketValue?
                            .Contains(value,
                                StringComparison.OrdinalIgnoreCase)
                            == true,
            "is_null" => ticketValue is null,
            "not_null" => ticketValue is not null,
            _ => false
        };
    }
}