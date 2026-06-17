using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Auth.Domain.Constants
{
   public static class AuthConstants
{
    public const int MaxFailedAttempts = 5;

    public const int LockoutMinutes = 15;
     public const int FailedAttemptWindowMinutes = 30;
}
}