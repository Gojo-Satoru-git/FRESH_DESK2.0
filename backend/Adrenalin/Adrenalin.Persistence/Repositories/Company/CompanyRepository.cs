using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private static readonly Func<AdrenalinDbContext, string, Task<Company?>> CompiledGetByDomain =
        EF.CompileAsyncQuery((AdrenalinDbContext db, string domain) =>
            db.Companies
                .Include(c => c.CompanyDomains)
                .FirstOrDefault(c => c.CompanyDomains.Any(d => d.Domain.ToLower() == domain)));

    private readonly AdrenalinDbContext _db;

    public CompanyRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Company?> GetByIdWithDomainsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .Include(c => c.CompanyDomains)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Company?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .Include(c => c.Contacts)
            .Include(c => c.CompanyContactsLimit)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Company?> GetByIdWithAllAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .Include(c => c.CompanyDomains)
            .Include(c => c.Contacts)
            .Include(c => c.CompanyContactsLimit)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Company?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        domain = domain.ToLowerInvariant();
        return CompiledGetByDomain(_db, domain);
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        await _db.Companies.AddAsync(company, cancellationToken);
    }

    public void Remove(Company company)
    {
        _db.Companies.Remove(company);
    }
}
