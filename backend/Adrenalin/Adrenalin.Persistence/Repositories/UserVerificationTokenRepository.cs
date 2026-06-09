using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories
{
    public class UserVerificationTokenRepository: IUserVerificationTokenRepository
    {
         private readonly AdrenalinDbContext _db;

    public UserVerificationTokenRepository(
        AdrenalinDbContext db)
    {
        _db = db;
    }

     public async Task AddAsync(
        UserVerificationToken token,
        CancellationToken cancellationToken)
    {
        await _db.UserVerificationTokens
            .AddAsync(token, cancellationToken);
    }
       public async Task<UserVerificationToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        return await _db.UserVerificationTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }
     
    }
}