using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class UnlockUserCommandHandler  : IRequestHandler<UnlockUserCommand, bool>
    {
        private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
     public UnlockUserCommandHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }
    public async Task<bool> Handle(
        UnlockUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
            throw new Exception("User not found");

        user.Unlock();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
    }
}