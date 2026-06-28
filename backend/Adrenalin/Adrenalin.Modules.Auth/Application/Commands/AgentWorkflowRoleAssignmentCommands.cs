// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Commands/AgentWorkflowRoleAssignmentCommands.cs
// NEW FILE

using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Commands;

/// <summary>
/// FS-05 §7 / FS-03 §3.2 — sets an agent's complete Workflow Role assignment in one call:
/// exactly one Primary Role, plus zero or more Additional Roles. Replaces whatever was
/// there before (idempotent), mirroring the existing SetUserRolesCommand style for Access
/// Level roles, but enforcing the Primary/Additional distinction that FS-05 requires and
/// SetUserRolesCommand does not have.
/// </summary>
public sealed record SetAgentWorkflowRolesCommand(
    Guid UserId,
    Guid PrimaryWorkflowRoleId,
    IReadOnlyList<Guid> AdditionalWorkflowRoleIds,
    Guid ActorId
) : IRequest<Result>;
