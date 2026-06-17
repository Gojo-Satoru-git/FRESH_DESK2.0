namespace Adrenalin.SharedKernel.Contracts;

public record AutomationRuleContractDto(
    Guid Id,
    string Name,
    int ExecutionOrder,
    string Conditions,
    string Actions
) : IAutomationRuleContract;