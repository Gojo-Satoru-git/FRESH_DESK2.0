using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

/// <summary>
/// Strategy interface for auto-assigning an agent within a group.
/// Implement this interface to add new assignment strategies.
/// Register the implementation and map it to an <see cref="AssignmentStrategy"/> enum value.
/// 
/// Built-in strategies:
/// - <c>LeastLoaded</c>: Assigns to the agent with the fewest open tickets.
/// 
/// Extensibility guide for teammates:
/// 1. Add a new value to <see cref="AssignmentStrategy"/> enum.
/// 2. Create a class implementing this interface.
/// 3. Set <see cref="Strategy"/> to your new enum value.
/// 4. Register it in DI.
/// 5. The <see cref="AgentAssignmentStrategyFactory"/> will resolve it automatically.
/// </summary>
public interface IAgentAssignmentStrategy
{
    /// <summary>
    /// Which enum value this strategy handles.
    /// </summary>
    AssignmentStrategy Strategy { get; }

    /// <summary>
    /// Select the best agent to assign within the given group.
    /// Returns null if no agent is available.
    /// </summary>
    Task<Guid?> SelectAgentAsync(Guid groupId, CancellationToken ct = default);
}
