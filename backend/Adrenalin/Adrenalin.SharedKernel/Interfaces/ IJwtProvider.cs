using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.SharedKernel.Interfaces
{
    public interface  IJwtProvider
    {
        string GenerateToken(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> permissions);
    }
}