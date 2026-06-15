using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Auth.Domain.Enums;
namespace Adrenalin.Modules.Auth.Application.Commands
{
    public sealed record CreateInternalUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    string RoleName
) : IRequest<Guid>;
}