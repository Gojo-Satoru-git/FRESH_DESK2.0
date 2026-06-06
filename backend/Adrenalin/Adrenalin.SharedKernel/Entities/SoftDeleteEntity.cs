namespace Adrenalin.SharedKernel.Entities;

public abstract class SoftDeleteEntity : AuditableEntity
{
    public bool IsDeleted { get; protected set; }

    public void Delete()
    {
        IsDeleted = true;
    }
}