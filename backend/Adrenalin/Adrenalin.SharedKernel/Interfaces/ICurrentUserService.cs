namespace Adrenalin.SharedKernel.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
     string? Email { get; }
    bool IsAuthenticated { get; }

    IEnumerable<string> Roles { get; }
}
