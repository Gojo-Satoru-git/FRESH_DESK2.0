// Ported from your branch (aspnet_source_old). Lives in SharedKernel so both
// the SLA module and the future Workflow module can reference it without a
// cross-module dependency in either direction.

namespace Adrenalin.SharedKernel.Enums.SLA;

public enum SlaTimerBehaviour
{
    /// <summary>Clock continues running normally (default).</summary>
    Run = 0,

    /// <summary>
    /// Clock pauses. ResolutionDueAt is extended when the ticket
    /// leaves this stage, by the duration of the pause.
    /// </summary>
    Pause = 1,

    /// <summary>
    /// Clock resets to zero. A fresh SLA clock is started using
    /// the stage's OverridePolicyId (or the original policy if null).
    /// Used for escalation stages that restart SLA commitments.
    /// </summary>
    Reset = 2,
}
