using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Infrastructure.Authentication
{
    public sealed  class TokenHasher : ITokenHasher
    {
        public  string Hash(string token)
    {
        using var sha = SHA256.Create();

        var bytes = sha.ComputeHash(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
    }
}