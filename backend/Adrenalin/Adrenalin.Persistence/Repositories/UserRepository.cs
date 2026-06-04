using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Adrenalin.Persistence.Repositories
{
    public sealed class UserRepository: IUserRepository
    {
     
            private readonly AdrenalinDbContext _db;
            public  UserRepository(AdrenalinDbContext db)
        {
            _db=db;
        }
    
        public async Task<User?> GetByEmailAsync(string Email,CancellationToken cancellationToken)
        {
           return await _db.Users.FirstOrDefaultAsync(x=>x.Email==Email,cancellationToken);
        }
        public async Task  AddAsync(User user,CancellationToken cancellationToken)
        {
            await _db.AddAsync(user,cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);;
        }
    }
}