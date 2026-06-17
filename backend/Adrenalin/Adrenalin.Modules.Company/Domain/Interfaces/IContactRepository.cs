using Adrenalin.Modules.Company.Domain.Entities;

namespace Adrenalin.Modules.Company.Domain.Interfaces;

public interface IContactRepository
{
    Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Remove(Contact contact);
}