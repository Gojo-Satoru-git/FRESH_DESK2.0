// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/DTOs/WorkflowRoleDto.cs
// NEW FILE

namespace Adrenalin.Modules.Auth.Application.DTOs;

/// <summary>FR-RP-007 — list/detail shape for the Workflow Role list screen.</summary>
public sealed record WorkflowRoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool IsSystemDefault,
    int AgentsAssignedCount,
    int StagesReferencingCount,
    DateTimeOffset? LastModified);
