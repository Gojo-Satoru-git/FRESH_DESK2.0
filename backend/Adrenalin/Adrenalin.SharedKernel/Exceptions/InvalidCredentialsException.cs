using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.SharedKernel.Exceptions
{
    public class InvalidCredentialsException: Exception
    {
        public InvalidCredentialsException()
        : base("Invalid email or password")
    {
    }
    }
}