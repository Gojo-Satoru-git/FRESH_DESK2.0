using Adrenalin.SharedKernel.Interfaces;
using System.Security.Claims;

namespace Adrenalin.unify.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }
    }
        public string? Email =>
        _httpContextAccessor.HttpContext?
            .User?
            .FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => UserId.HasValue;
}
