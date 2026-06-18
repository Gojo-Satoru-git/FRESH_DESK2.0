using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Queries
{
   public sealed record GetMySessionsQuery(
    Guid UserId)
    : IRequest<List<UserSessionDto>>;
}