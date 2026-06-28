// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Handlers/GetWorkflowRolesQueryHandler.cs
// NEW FILE

using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Handlers;

/// <summary>FR-RP-007 (table columns) / FR-RP-008 (filter by status, free-text search).</summary>
public sealed class GetWorkflowRolesQueryHandler
    : IRequestHandler<GetWorkflowRolesQuery, Result<IReadOnlyList<WorkflowRoleDto>>>
{
    private readonly IWorkflowRoleRepository _roles;
    private readonly IStageRoleReferenceChecker _stageChecker;

    public GetWorkflowRolesQueryHandler(IWorkflowRoleRepository roles, IStageRoleReferenceChecker stageChecker)
    {
        _roles = roles;
        _stageChecker = stageChecker;
    }

    public async Task<Result<IReadOnlyList<WorkflowRoleDto>>> Handle(
        GetWorkflowRolesQuery query, CancellationToken ct)
    {
        var all = await _roles.GetAllAsync(ct);

        var filtered = all.Where(r =>
            (query.IsActive is null || r.IsActive == query.IsActive) &&
            (string.IsNullOrWhiteSpace(query.SearchText) ||
             r.Name.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase) ||
             (r.Description?.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase) ?? false)));

        var result = new List<WorkflowRoleDto>();
        foreach (var role in filtered)
        {
            var agentCount = await _roles.CountAssignedAgentsAsync(role.Id, ct);
            var stageCount = await _stageChecker.CountReferencingStagesAsync(role.Id, ct);

            result.Add(new WorkflowRoleDto(
                role.Id,
                role.Name,
                role.Description,
                role.IsActive,
                role.IsSystemDefault,
                agentCount,
                stageCount,
                role.UpdatedAt));
        }

        return Result<IReadOnlyList<WorkflowRoleDto>>.Success(result);
    }
}
