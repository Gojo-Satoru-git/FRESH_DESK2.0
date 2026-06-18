using Adrenalin.Modules.Company.Domain.Entities;

namespace Adrenalin.Modules.Company.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<Entities.Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.Company?> GetByIdWithDomainsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.Company?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.Company?> GetByIdWithAllAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.Company?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
    Task AddAsync(Entities.Company company, CancellationToken cancellationToken = default);
    void Remove(Entities.Company company);
}
