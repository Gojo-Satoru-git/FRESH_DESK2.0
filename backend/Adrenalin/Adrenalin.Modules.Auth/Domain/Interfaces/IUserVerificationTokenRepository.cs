using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces
{
    public interface IUserVerificationTokenRepository
    {
        Task AddAsync(
        UserVerificationToken token,
        CancellationToken cancellationToken);

    Task<UserVerificationToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

   

    }
}