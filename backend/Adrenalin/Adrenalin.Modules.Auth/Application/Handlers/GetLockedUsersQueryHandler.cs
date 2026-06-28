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
    public sealed  class GetLockedUsersQueryHandler : IRequestHandler<GetLockedUsersQuery, IReadOnlyList<LockedUserDto>>
    {
        private readonly IUserRepository _users;

    public GetLockedUsersQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<IReadOnlyList<LockedUserDto>> Handle(
        GetLockedUsersQuery request,
        CancellationToken cancellationToken)
    {
        return await _users.GetLockedUsersAsync(cancellationToken);
    }
    }
}