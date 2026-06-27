using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    private readonly ICurrentUserService _currentUser;
    public AuthController(IDispatcher dispatcher,
        ICurrentUserService currentUser)
    {
        _dispatcher = dispatcher;
        _currentUser = currentUser;
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
    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDTO request,
        CancellationToken cancellationToken)
    {

        var userId = await _dispatcher.Send(
            new LoginCommand(request.Email, request.Password, HttpContext.Connection.RemoteIpAddress,
    HttpContext.Request.Headers["User-Agent"]),
            cancellationToken);

        return Ok(new { UserId = userId, Message = "Login successful" });
    }
    [EnableRateLimiting("RefreshPolicy")]
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
[EnableRateLimiting("ForgotPasswordPolicy")]
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
    }
    ); 
}
[EnableRateLimiting("LoginPolicy")]
[HttpPost("resend-verification")]
public async Task<IActionResult> ResendVerification(
    ResendVerificationRequestDTO request,
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new ResendVerificationCommand(
            request.Email),
        cancellationToken);

    return Ok(new
    {
        Message = "Verification email sent."
    });
}

[EnableRateLimiting("LoginPolicy")]
[HttpPost("verify-email-otp")]
public async Task<IActionResult> VerifyEmailOtp(
    VerifyEmailOtpRequestDTO request,
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new VerifyEmailOtpCommand(
            request.Otp),
        cancellationToken);

    return Ok(new
    {
        Message = "Email verified successfully"
    });
}
[EnableRateLimiting("LoginPolicy")]
[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword(
    ResetPasswordRequestDTO request,
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new ResetPasswordCommand(
            request.Token,
            request.NewPassword),
        cancellationToken);

    return Ok(new
    {
        Message =
            "Password reset successful"
    });
}
[Authorize]
[HttpGet("sessions")]
public async Task<IActionResult> GetSessions(
    CancellationToken cancellationToken)
{
    
    var result =
        await _dispatcher.Send(
            new GetMySessionsQuery(
                _currentUser.UserId!.Value
            ),
            cancellationToken);

    return Ok(result);
}
[Authorize]
[HttpDelete("sessions/{sessionId}")]
public async Task<IActionResult> LogoutSession(
    Guid sessionId,
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new LogoutSessionCommand(sessionId),
        cancellationToken);

    return Ok();
}
[Authorize]
[HttpPost("logout-all")]
public async Task<IActionResult> LogoutAll(
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new LogoutAllSessionsCommand(
            _currentUser.UserId!.Value),
        cancellationToken);

    return Ok(new
    {
        Message = "Logged out from all devices"
    });
}

[Authorize]
[HttpPost("change-password")]
public async Task<IActionResult> ChangePassword(
    ChangePasswordRequestDTO request,
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new ChangePasswordCommand(
            _currentUser.UserId!.Value,
            request.CurrentPassword,
            request.NewPassword),
        cancellationToken);

    return Ok(new
    {
        Message = "Password changed successfully. Please login again."
    });
}
}
