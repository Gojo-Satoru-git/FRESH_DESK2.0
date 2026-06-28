// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Handlers/GetEffectivePermissionsPreviewQueryHandler.cs
// NEW FILE

using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Handlers;

/// <summary>
/// FR-RP-043 / NFR-RP-005 — plain-English-ish grouping by Module, suitable for both the
/// Access Level's own Permission Matrix summary panel (§8.2) and the Agent Editor's live
/// preview (§8.3). Same underlying data, two different presentation contexts in the UI.
/// </summary>
public sealed class GetEffectivePermissionsPreviewQueryHandler
    : IRequestHandler<GetEffectivePermissionsPreviewQuery, Result<EffectivePermissionsSummaryDto>>
{
    private readonly IRoleRepository _roles;
    private readonly IRolePermissionRepository _rp;

    public GetEffectivePermissionsPreviewQueryHandler(IRoleRepository roles, IRolePermissionRepository rp)
        => (_roles, _rp) = (roles, rp);

    public async Task<Result<EffectivePermissionsSummaryDto>> Handle(
        GetEffectivePermissionsPreviewQuery query, CancellationToken ct)
    {
        var role = await _roles.GetByIdAsync(query.AccessLevelId, ct);
        if (role is null)
            return Result<EffectivePermissionsSummaryDto>.Failure($"Access Level {query.AccessLevelId} not found.");

        var grants = await _rp.GetByRoleWithPermissionsAsync(query.AccessLevelId, ct);

        var modules = grants
            .GroupBy(g => g.Permission.Resource)
            .Select(grp => new ModulePermissionSummaryDto(
                Module: grp.Key,
                AllowedActions: grp.Select(g => g.Permission.Action).OrderBy(a => a).ToList()))
            .OrderBy(m => m.Module)
            .ToList();

        return Result<EffectivePermissionsSummaryDto>.Success(
            new EffectivePermissionsSummaryDto(role.Id, role.Name, modules));
    }
}
