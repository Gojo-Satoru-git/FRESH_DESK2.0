using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public class ContactRepository : IContactRepository
{
    private static readonly Func<AdrenalinDbContext, string, Task<Contact?>> CompiledGetByEmail =
        EF.CompileAsyncQuery((AdrenalinDbContext db, string email) =>
            db.Contacts.FirstOrDefault(c => c.Email == email));

    private readonly AdrenalinDbContext _db;

    public ContactRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        await _db.Contacts.AddAsync(contact, cancellationToken);
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Contact?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return CompiledGetByEmail(_db, email);
    }

    public void Remove(Contact contact)
    {
        _db.Contacts.Remove(contact);
    }
}