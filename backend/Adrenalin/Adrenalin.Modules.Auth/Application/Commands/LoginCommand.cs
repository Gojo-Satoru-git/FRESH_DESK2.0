using System.Net;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
namespace Adrenalin.Modules.Auth.Application.Commands;
    public sealed record LoginCommand(
    string Email,
    string Password,
    IPAddress? IpAddress,
    string? DeviceInfo
) : IRequest<LoginResponseDTO>;
