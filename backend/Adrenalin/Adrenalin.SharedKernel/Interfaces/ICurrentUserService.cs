namespace Adrenalin.SharedKernel.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}
