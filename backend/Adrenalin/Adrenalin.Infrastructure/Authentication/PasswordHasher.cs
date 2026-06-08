using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Interfaces;

namespace Adrenalin.Infrastructure.Authentication
{
    public sealed  class PasswordHasher: IPasswordHasher
    {
       public string Hash(string Password)
        {
            return BCrypt.Net.BCrypt.HashPassword(Password);

        }
        public bool Verify(string Password,string hash)
        {
            return BCrypt.Net.BCrypt.Verify(Password,hash);
        }

    }
}