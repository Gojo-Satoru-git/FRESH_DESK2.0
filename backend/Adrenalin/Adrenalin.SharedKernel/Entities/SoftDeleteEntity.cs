namespace Adrenalin.SharedKernel.Entities;

public abstract class SoftDeleteEntity : AuditableEntity
{
    public bool IsDeleted { get; protected set; }
    public bool IsActive { get; protected set; } = true;
}