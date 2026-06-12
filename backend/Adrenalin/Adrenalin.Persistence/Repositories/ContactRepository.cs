using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.Persistence.Context;

namespace Adrenalin.Persistence.Repositories
{
    public class ContactRepository: IContactRepository
    {
         private readonly AdrenalinDbContext _db;
        public ContactRepository(
        AdrenalinDbContext db)
    {
        _db = db;
    }
     public async Task AddAsync(
        Contact contact,
        CancellationToken cancellationToken)
    {
        await _db.Contacts.AddAsync(
            contact,
            cancellationToken);
    }
    }
}