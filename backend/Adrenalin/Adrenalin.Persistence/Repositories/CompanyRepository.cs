using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly AdrenalinDbContext _db;

    public CompanyRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<Adrenalin.Modules.Company.Domain.Entities.Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Adrenalin.Modules.Company.Domain.Entities.Company?> GetByIdWithDomainsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .Include(c => c.CompanyDomains)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Adrenalin.Modules.Company.Domain.Entities.Company?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .Include(c => c.Contacts)
            .Include(c => c.CompanyContactsLimit)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Adrenalin.Modules.Company.Domain.Entities.Company?> GetByIdWithAllAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .Include(c => c.CompanyDomains)
            .Include(c => c.Contacts)
            .Include(c => c.CompanyContactsLimit)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Adrenalin.Modules.Company.Domain.Entities.Company company, CancellationToken cancellationToken = default)
    {
        await _db.Companies.AddAsync(company, cancellationToken);
    }

    public void Remove(Adrenalin.Modules.Company.Domain.Entities.Company company)
    {
        _db.Companies.Remove(company);
    }
}
