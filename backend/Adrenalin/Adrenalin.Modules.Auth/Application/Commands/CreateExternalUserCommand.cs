using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.Modules.Auth.Domain.Enums;
namespace Adrenalin.Modules.Auth.Application.Commands
{
   public sealed record CreateExternalUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    Guid CompanyId,
    string RoleName
    
) : IRequest<Guid>;
}