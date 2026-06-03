namespace Adrenalin.SharedKernel.Entities;

public abstract class BaseEntity
{
<<<<<<< HEAD
    public Guid Id { get; protected set; }
=======
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public byte[]? RowVersion { get; protected set; } // EF Core concurrency token
    }
>>>>>>> ragavendra
}