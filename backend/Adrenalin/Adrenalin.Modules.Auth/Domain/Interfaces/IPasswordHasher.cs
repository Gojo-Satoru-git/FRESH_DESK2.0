using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Auth.Domain.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string Password);
        bool Verify(string Password,string hash);
    }
}