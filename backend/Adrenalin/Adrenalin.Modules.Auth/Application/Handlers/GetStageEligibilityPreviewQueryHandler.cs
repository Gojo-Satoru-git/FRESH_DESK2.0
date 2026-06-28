// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Handlers/GetStageEligibilityPreviewQueryHandler.cs
// NEW FILE

using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Handlers;

/// <summary>
/// FR-RP-044 — see the query's doc comment (file 38). StageDataAvailable=false today,
/// always, since FS-02's stage "Allowed Roles" config doesn't exist in this codebase yet.
/// Wire this up for real once that table exists: query it for rows whose AllowedRoleIds
/// intersects {PrimaryWorkflowRoleId} ∪ AdditionalWorkflowRoleIds, return the stage names.
/// </summary>
public sealed class GetStageEligibilityPreviewQueryHandler
    : IRequestHandler<GetStageEligibilityPreviewQuery, Result<StageEligibilitySummaryDto>>
{
    private readonly IWorkflowRoleRepository _roles;

    public GetStageEligibilityPreviewQueryHandler(IWorkflowRoleRepository roles) => _roles = roles;

    public async Task<Result<StageEligibilitySummaryDto>> Handle(
        GetStageEligibilityPreviewQuery query, CancellationToken ct)
    {
        var primary = await _roles.GetByIdAsync(query.PrimaryWorkflowRoleId, ct);
        if (primary is null)
            return Result<StageEligibilitySummaryDto>.Failure(
                $"Primary Workflow Role {query.PrimaryWorkflowRoleId} not found.");

        // TODO once FS-02 exists: replace this stub with a real query against the stage
        // config table. Returning StageDataAvailable=false tells the frontend to render
        // "Stage eligibility preview not yet available" instead of a misleading empty list.
        return Result<StageEligibilitySummaryDto>.Success(
            new StageEligibilitySummaryDto(EligibleStageNames: Array.Empty<string>(), StageDataAvailable: false));
    }
}
