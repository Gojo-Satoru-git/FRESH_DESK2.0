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
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
    LogoutRequestDTO request,
    CancellationToken cancellationToken)
    {
        await _dispatcher.Send(
            new LogoutCommand(
                request.RefreshToken),
            cancellationToken);

        return Ok(new
        {
            Message = "Logged out successfully"
        });
    }
    [HttpGet("verify-email")]
public async Task<IActionResult> VerifyEmail(
    string token,
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new VerifyEmailCommand(token),
        cancellationToken);

    return Ok(new
    {
        Message = "Email verified successfully"
    });
}
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(
    ForgotPasswordRequestDTO request,
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new ForgotPasswordCommand(
            request.Email),
        cancellationToken);

    return Ok(new
    {
        Message =
            "If the email exists, a password reset link has been sent."
    });
}
}
