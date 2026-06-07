using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public AuthController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(
        RegisterUserRequestDTO request,
        CancellationToken cancellationToken)
    {
        var userId = await _dispatcher.Send(
            new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Username,
                request.Phone),
            cancellationToken);

        return Ok(new { UserId = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDTO request,
        CancellationToken cancellationToken)
    {

        var userId = await _dispatcher.Send(
            new LoginCommand(request.Email, request.Password, HttpContext.Connection.RemoteIpAddress?.ToString(),
    HttpContext.Request.Headers["User-Agent"]),
            cancellationToken);

        return Ok(new { UserId = userId, Message = "Login successful" });
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
    RefreshTokenRequestDTO request,
    CancellationToken cancellationToken)
    {
        var result =
            await _dispatcher.Send(
                new RefreshTokenCommand(
                    request.RefreshToken),
                cancellationToken);

        return Ok(result);
    }
}
