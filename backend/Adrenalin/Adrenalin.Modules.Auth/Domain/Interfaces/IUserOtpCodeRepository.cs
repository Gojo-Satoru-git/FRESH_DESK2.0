using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces
{
    public interface  IUserOtpCodeRepository
    {
         Task AddAsync(
        UserOtpCode otp,
        CancellationToken cancellationToken);

    Task<UserOtpCode?> GetByHashAsync(
        string codeHash,
        CancellationToken cancellationToken);
    }
}