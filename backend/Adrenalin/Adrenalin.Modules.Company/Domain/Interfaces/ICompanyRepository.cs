namespace Adrenalin.Modules.Company.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<Adrenalin.Modules.Company.Domain.Entities.Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Adrenalin.Modules.Company.Domain.Entities.Company?> GetByIdWithDomainsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Adrenalin.Modules.Company.Domain.Entities.Company?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Adrenalin.Modules.Company.Domain.Entities.Company?> GetByIdWithAllAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Adrenalin.Modules.Company.Domain.Entities.Company company, CancellationToken cancellationToken = default);
    void Remove(Adrenalin.Modules.Company.Domain.Entities.Company company);
}
