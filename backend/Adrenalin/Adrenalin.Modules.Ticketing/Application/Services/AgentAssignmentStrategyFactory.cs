using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

namespace Adrenalin.Modules.Ticketing.Application.Services;

/// <summary>
/// Resolves the correct <see cref="IAgentAssignmentStrategy"/> for a given
/// <see cref="AssignmentStrategy"/> enum value.
/// 
/// All registered strategies are injected via DI. If no strategy matches,
/// falls back to <see cref="AssignmentStrategy.LeastLoaded"/>.
/// </summary>
public sealed class AgentAssignmentStrategyFactory
{
    private readonly IEnumerable<IAgentAssignmentStrategy> _strategies;

    public AgentAssignmentStrategyFactory(IEnumerable<IAgentAssignmentStrategy> strategies)
        => _strategies = strategies;

    public IAgentAssignmentStrategy Resolve(AssignmentStrategy strategy)
    {
        var resolved = _strategies.FirstOrDefault(s => s.Strategy == strategy);
        if (resolved is not null)
            return resolved;

        // Fallback to LeastLoaded if requested strategy is not implemented
        return _strategies.First(s => s.Strategy == AssignmentStrategy.LeastLoaded);
    }

    /// <summary>
    /// Overload accepting the raw int stored on Group.AssignmentStrategy.
    /// </summary>
    public IAgentAssignmentStrategy Resolve(int strategyValue)
    {
        if (Enum.IsDefined(typeof(AssignmentStrategy), strategyValue))
            return Resolve((AssignmentStrategy)strategyValue);
        return Resolve(AssignmentStrategy.LeastLoaded);
    }
}
