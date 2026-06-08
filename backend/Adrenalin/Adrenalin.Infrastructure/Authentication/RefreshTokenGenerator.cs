using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Infrastructure.Authentication
{
    public  sealed   class RefreshTokenGenerator :IRefreshTokenGenerator
    {
        public  string Generate()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }
    }
}