using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Adrenalin.Modules.Auth.Application.Commands
{
    public sealed record RegisterUserCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string? Username,
        string? Phone
    ):IRequest<Guid>;
    
}