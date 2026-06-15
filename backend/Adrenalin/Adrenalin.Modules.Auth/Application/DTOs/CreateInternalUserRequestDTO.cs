using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Auth.Application.DTOs
{
    public sealed record CreateInternalUserRequestDTO(
    string Email,
    string FirstName,
    string LastName,
    string Phone,
   string  RoleName
);
}