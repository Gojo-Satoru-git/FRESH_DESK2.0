namespace Adrenalin.SharedKernel.Contracts;

public interface IAutomationRuleContract
{
    Guid Id { get; }
    string Name { get; }
    int ExecutionOrder { get; }
    string Conditions { get; }
    string Actions { get; }
}