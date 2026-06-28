// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Queries/AgentEditorPreviewQueries.cs
// NEW FILE

using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Queries;

/// <summary>
/// FR-RP-043 — "Effective Permissions" preview for the Agent Editor's Access Level
/// selector. Takes the access level ID directly (not a saved agent) so the UI can call
/// this live as the admin changes the selection BEFORE saving.
/// </summary>
public sealed record GetEffectivePermissionsPreviewQuery(
    Guid AccessLevelId
) : IRequest<Result<EffectivePermissionsSummaryDto>>;

public sealed record EffectivePermissionsSummaryDto(
    Guid AccessLevelId,
    string AccessLevelName,
    IReadOnlyList<ModulePermissionSummaryDto> Modules);

public sealed record ModulePermissionSummaryDto(
    string Module,
    IReadOnlyList<string> AllowedActions);

/// <summary>
/// FR-RP-044 — "Stage Eligibility" preview for the Agent Editor's Primary/Additional
/// Role pickers. Same live-preview shape: takes role IDs directly, not a saved agent.
///
/// NOTE: this can only return real data once FS-02's stage "Allowed Roles" config exists
/// (see file 15's IStageRoleReferenceChecker doc comment — that table isn't built yet).
/// Until then, the handler for this query returns an empty list with a flag explaining
/// why, rather than fabricating stage names.
/// </summary>
public sealed record GetStageEligibilityPreviewQuery(
    Guid PrimaryWorkflowRoleId,
    IReadOnlyList<Guid> AdditionalWorkflowRoleIds
) : IRequest<Result<StageEligibilitySummaryDto>>;

public sealed record StageEligibilitySummaryDto(
    IReadOnlyList<string> EligibleStageNames,
    bool StageDataAvailable); // false until FS-02 exists — UI should show "Not yet configured" not an empty list
