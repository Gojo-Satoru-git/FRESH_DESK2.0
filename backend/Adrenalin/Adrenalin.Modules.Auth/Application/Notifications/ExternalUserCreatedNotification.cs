using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Notifications
{
    public sealed record ExternalUserCreatedNotification(
    Guid UserId,
    Guid CompanyId,
    string Email,
    string FullName,
      Guid CreatedBy
) : INotification;
}