using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using MediatR;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class RegisterUserCommandHandler:IRequestHandler<RegisterUserCommand,Guid>
    {
         private readonly IUserRepository _users;

        private readonly IPasswordHasher _hasher;
        public RegisterUserCommandHandler(IUserRepository users,IPasswordHasher hasher)
        {
            _users=users;
            _hasher=hasher;
        }
        public async Task<Guid> Handle(RegisterUserCommand request,CancellationToken cancellationToken)
        {
            var existing=await _users.GetByEmailAsync(request.Email,cancellationToken);
            if(existing is not null){
                throw new Exception("Email already exists");
            }
            
            var passwordHash=_hasher.Hash(request.Password);
            var user=User.Create(request.Email,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.Username,
                request.Phone);
            await _users.AddAsync(user,cancellationToken);
            return user.Id;
            
        }
        
    }
}