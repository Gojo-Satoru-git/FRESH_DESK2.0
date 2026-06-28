// FILE: Adrenalin/Adrenalin.Modules.Auth/Domain/Interfaces/IStageRoleReferenceChecker.cs
// NEW FILE

namespace Adrenalin.Modules.Auth.Domain.Interfaces;

/// <summary>
/// FS-05 FR-RP-006 / BR-RP-003 — checks whether a Workflow Role is referenced in any
/// workflow stage's "Allowed Roles" configuration (owned by FS-02, Workflow Engine —
/// Stages &amp; Assignment).
///
/// IMPORTANT: I did not find a WorkflowStage / "Allowed Roles" table anywhere in your
/// uploaded source — only SlaStageConfig exists, which is SLA timer config, not stage
/// role-eligibility config. FS-02 itself doesn't appear to be built yet.
///
/// Implement this for real once FS-02's stage config table exists — wire it to query
/// that table for rows referencing the given WorkflowRoleId. Until then,
/// NotBuiltYetStageRoleReferenceChecker below returns 0 unconditionally, which means
/// the delete guard in DeleteWorkflowRoleCommandHandler will currently only block on
/// the "assigned to an agent" half of FR-RP-006 — not the "referenced by a stage" half.
/// That's a known, deliberate gap until FS-02 lands, not a bug in this handler.
/// </summary>
public interface IStageRoleReferenceChecker
{
    Task<int> CountReferencingStagesAsync(Guid workflowRoleId, CancellationToken ct = default);
}
