using Microsoft.AspNetCore.Authorization;

namespace Adrenalin.unify.API.Authorization;

/// <summary>
/// Carries a single "resource:action" string as an authorization requirement.
/// Created automatically by <see cref="PermissionPolicyProvider"/> for every
/// policy name that contains a colon (e.g. "ticket:create").
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
        => Permission = permission;
}
