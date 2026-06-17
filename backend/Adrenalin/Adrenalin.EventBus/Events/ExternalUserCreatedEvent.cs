using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.EventBus.Events
{
    public sealed record ExternalUserCreatedEvent(
    Guid UserId,
    Guid CompanyId,
    string Email,
    string FullName,
     Guid CreatedBy
);
}