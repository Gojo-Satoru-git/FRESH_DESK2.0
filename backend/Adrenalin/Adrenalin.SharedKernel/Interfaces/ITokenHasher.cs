using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.SharedKernel.Interfaces
{
    public interface ITokenHasher
    {
         string Hash(string token);
    }
}