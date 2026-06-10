using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories
{
    public class UserOtpCodeRepository:IUserOtpCodeRepository
    {
         private readonly AdrenalinDbContext _db;
          public UserOtpCodeRepository(
        AdrenalinDbContext db)
    {
        _db = db;
    }
        public async Task AddAsync(
    UserOtpCode otp,
    CancellationToken cancellationToken)
{
    await _db.UserOtpCodes
        .AddAsync(otp, cancellationToken);
}

public async Task<UserOtpCode?> GetByHashAsync(
    string codeHash,
    CancellationToken cancellationToken)
{
    return await _db.UserOtpCodes
        .FirstOrDefaultAsync(
            x => x.CodeHash == codeHash,
            cancellationToken);
}
    }
}