using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Company.Domain.Entities;

namespace Adrenalin.Modules.Company.Domain.Interfaces
{
    public interface IContactRepository
    {
         Task AddAsync(
        Contact contact,
        CancellationToken cancellationToken);
    }
}