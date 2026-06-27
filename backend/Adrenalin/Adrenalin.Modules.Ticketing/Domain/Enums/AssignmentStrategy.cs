namespace Adrenalin.Modules.Ticketing.Domain.Enums;

/// <summary>
/// Defines the strategy used to auto-assign agents within a group.
/// Extensible — add new values and implement <c>IAgentAssignmentStrategy</c>
/// to plug in custom assignment logic.
/// </summary>
public enum AssignmentStrategy
{
    /// <summary>
    /// Assigns to the agent with the fewest open tickets in the group.
    /// This is the default and currently implemented strategy.
    /// </summary>
    LeastLoaded = 0,

    /// <summary>
    /// Cycles through agents in order. Not yet implemented.
    /// </summary>
    RoundRobin = 1,

    /// <summary>
    /// Matches agent skills/competencies to ticket attributes. Not yet implemented.
    /// Requires a skill/competency model to be built.
    /// </summary>
    SkillBased = 2,

    /// <summary>
    /// No auto-assignment. Tickets land in the group's unassigned queue
    /// for a lead or manager to manually assign.
    /// </summary>
    Manual = 3,

    /// <summary>
    /// Role eligibility + hard-filter + weighted-scoring engine (the "full"
    /// auto-assignment design — region/skill/language/shift/capacity matching).
    /// 
    /// INTENTIONALLY STUBBED: the Workflow module (stages, factor policies,
    /// role eligibility) is being built by another developer separately and
    /// does not exist yet. <see cref="Adrenalin.Modules.Ticketing.Application.Services.FactorBasedAssignmentStrategy"/>
    /// always returns null (fails) on purpose so tickets fall through to the
    /// group queue for manual dispatch, instead of silently mis-assigning.
    /// 
    /// Swap the stub's implementation for the real engine once the Workflow
    /// module exposes role/factor data — no other code needs to change,
    /// since AgentAssignmentStrategyFactory resolves by this enum value.
    /// </summary>
    FactorBased = 4
}
