namespace Adrenalin.SharedKernel.Entities;

public abstract class ActiveSoftDeleteEntity : SoftDeleteEntity
{
    public bool IsActive { get; protected set; } = true;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
