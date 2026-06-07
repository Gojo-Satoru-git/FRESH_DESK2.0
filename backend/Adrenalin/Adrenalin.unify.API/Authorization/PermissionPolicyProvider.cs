using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Adrenalin.unify.API.Authorization;

/// <summary>
/// Dynamically creates an <see cref="AuthorizationPolicy"/> for any string that
/// follows the "resource:action" pattern (contains a colon).
///
/// This means you never have to pre-register every permission in Program.cs —
/// simply decorate any endpoint with <c>[Authorize(Policy = "ticket:create")]</c>
/// and this provider builds the policy on the fly.
///
/// All other policy names (e.g. named policies without a colon) fall through to
/// the default <see cref="DefaultAuthorizationPolicyProvider"/>.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        => _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.Contains(':'))
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }
}
