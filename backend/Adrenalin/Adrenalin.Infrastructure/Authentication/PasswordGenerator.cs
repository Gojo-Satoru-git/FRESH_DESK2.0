using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Infrastructure.Authentication
{
    public class PasswordGenerator:
    IPasswordGenerator
    {
        public  string Generate()
    {
         return $"Temp@{Random.Shared.Next(100000,999999)}";
    }
    }
}