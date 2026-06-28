// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Commands/WorkflowRoleCommands.cs
// NEW FILE

using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Commands;

/// <summary>FR-RP-001 — Status defaults to Active; not exposed as a parameter here.</summary>
public sealed record CreateWorkflowRoleCommand(
    string Name,
    string? Description,
    Guid ActorId
) : IRequest<Result<Guid>>;

/// <summary>FR-RP-003 — Name and Description editable at any time.</summary>
public sealed record RenameWorkflowRoleCommand(
    Guid WorkflowRoleId,
    string Name,
    string? Description,
    Guid ActorId
) : IRequest<Result>;

/// <summary>FR-RP-004 — hides from pickers; existing assignments preserved.</summary>
public sealed record DeactivateWorkflowRoleCommand(
    Guid WorkflowRoleId,
    Guid ActorId
) : IRequest<Result>;

/// <summary>FR-RP-005 — restores visibility immediately.</summary>
public sealed record ReactivateWorkflowRoleCommand(
    Guid WorkflowRoleId,
    Guid ActorId
) : IRequest<Result>;

/// <summary>
/// FR-RP-006 / BR-RP-003 — blocked if assigned to any agent OR referenced by any stage's
/// Allowed Roles config. On block, Result.Error carries a human-readable summary; the
/// handler also returns the raw counts via Result&lt;WorkflowRoleDeletionBlockedInfo&gt;
/// so the API layer can render "12 agents, 3 stages" instead of just a string.
/// </summary>
public sealed record DeleteWorkflowRoleCommand(
    Guid WorkflowRoleId,
    Guid ActorId
) : IRequest<Result<WorkflowRoleDeletionBlockedInfo?>>;

public sealed record WorkflowRoleDeletionBlockedInfo(
    int AssignedAgentCount,
    int ReferencingStageCount);
