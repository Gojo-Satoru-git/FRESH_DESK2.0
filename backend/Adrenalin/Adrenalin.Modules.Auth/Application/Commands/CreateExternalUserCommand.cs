using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Commands
{
   public sealed record CreateExternalUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    Guid CompanyId,
    Guid CustomerRoleId
) : IRequest<Guid>;
}