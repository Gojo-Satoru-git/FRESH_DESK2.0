namespace Adrenalin.SharedKernel.Entities;

<<<<<<< HEAD
public abstract class SoftDeleteEntity :AuditableEntity
{
    public bool IsDeleted { get; protected set; }

    public bool IsActive { get; protected set; } = true;
=======
public abstract class SoftDeleteEntity : AuditableEntity
{
    public bool IsDeleted { get; protected set; }

    public void Delete()
    {
        IsDeleted = true;
    }
>>>>>>> ragavendra
}