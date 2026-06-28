// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Queries/GetWorkflowRolesQuery.cs
// NEW FILE

using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Queries;

/// <summary>FR-RP-007 / FR-RP-008 — searchable, sortable, filterable Workflow Role list.</summary>
public sealed record GetWorkflowRolesQuery(
    bool? IsActive,
    string? SearchText
) : IRequest<Result<IReadOnlyList<WorkflowRoleDto>>>;
