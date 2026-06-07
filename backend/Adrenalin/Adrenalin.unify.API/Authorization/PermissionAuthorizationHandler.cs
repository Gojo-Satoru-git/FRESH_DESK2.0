using Microsoft.AspNetCore.Authorization;

namespace Adrenalin.unify.API.Authorization;

/// <summary>
/// Checks whether the authenticated user's JWT contains a "permissions" claim
/// that matches the required "resource:action" string.
///
/// The "permissions" claim array is stamped into the JWT at login time by
/// <see cref="Adrenalin.Modules.Auth.Application.Handlers.LoginCommandHandler"/>
/// using <c>GetUserEffectivePermissionsQuery</c>.
/// </summary>
public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User
            .FindAll("permission")
            .Any(c => c.Value == requirement.Permission);

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
