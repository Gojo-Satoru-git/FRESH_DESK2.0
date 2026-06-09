using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Commands
{
   public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword)
    : IRequest<bool>;
}