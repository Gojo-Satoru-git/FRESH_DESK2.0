using Adrenalin.SharedKernel.Mediator;
namespace Adrenalin.Modules.Auth.Application.Commands;
    public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Guid>;
