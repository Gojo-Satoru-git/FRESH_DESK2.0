using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Auth.Domain.Constants
{
   public static class AuthConstants
{
    public const int MaxFailedAttempts = 5;

    public const int LockoutHours = 24;
     public const int FailedAttemptWindowMinutes = 30;
     public const int SessionIdleTimeoutMinutes = 60;
}
}