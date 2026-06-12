using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Company.Applications.Commands
{
    public sealed record CreateContactForExternalUserCommand(
    Guid UserId,
    Guid CompanyId,
    string Email,
    string FullName,
    Guid CreatedBy
) : IRequest<Guid>;
}