using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Auth.Application.DTOs
{
    public sealed record RegisterUserRequestDTO(
    
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string? Username,
        string? Phone
    );

        
    
}