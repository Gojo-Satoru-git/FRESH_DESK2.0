using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Infrastructure.Authentication
{
    public class OtpGenerator: IOtpGenerator
    {
        private readonly Random _random = new();
         public string Generate()
    {
        
        return _random
            .Next(100000, 999999)
            .ToString();
    }
    }
}