using System.Collections.Generic;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.SharedKernel.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public byte[]? RowVersion { get; protected set; } // EF Core concurrency token

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
