using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
   public sealed class GetMySessionsQueryHandler
    : IRequestHandler<
        GetMySessionsQuery,
        List<UserSessionDto>>
    {
        private readonly IUserSessionRepository _sessions;

    public GetMySessionsQueryHandler(
        IUserSessionRepository sessions)
    {
        _sessions = sessions;
    }
     public async Task<List<UserSessionDto>> Handle(
        GetMySessionsQuery request,
        CancellationToken cancellationToken)
    {
        var sessions =
            await _sessions.GetUserSessionsAsync(
                request.UserId,
                cancellationToken);

        return sessions
            .Select(x =>
                new UserSessionDto(
                    x.Id,
                    x.DeviceName,
                    x.IpAddress?.ToString(),
                    x.StartedAt,
                    x.LastActiveAt,
                    x.IsActive))
            .ToList();
    }
    }
}