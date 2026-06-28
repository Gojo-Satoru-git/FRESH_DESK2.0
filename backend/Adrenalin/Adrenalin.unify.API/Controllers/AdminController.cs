using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Adrenalin.Modules.Auth.Domain.Enums;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.SharedKernel.Interfaces;
namespace Adrenalin.unify.API.Controllers
{
    [Authorize(Policy = "user:create")]
    [ApiController]
    [Route("api/admin")]

    public sealed class AdminController: ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        

    public AdminController(
        IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
       
    }
    [HttpPost("internal-users")]
    public async Task<IActionResult> CreateInternalUser(
        CreateInternalUserRequestDTO request,
        CancellationToken cancellationToken)
    {
        var userId =
            await _dispatcher.Send(
                new CreateInternalUserCommand(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Phone,
                 request.RoleName),
                cancellationToken);

        return Ok(new
        {
            UserId = userId
        });
    }
    [HttpPost("external-users")]
public async Task<IActionResult> CreateExternalUser(
    CreateExternalUserRequestDTO request,
    CancellationToken cancellationToken)
{
    var userId =
        await _dispatcher.Send(
            new CreateExternalUserCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Phone,
                request.CompanyId,
                 request.RoleName),
            cancellationToken);

    return Ok(new
    {
        UserId = userId
    });
}
[Authorize(Policy = "user:unlock")]
[HttpPost("users/{userId:guid}/unlock")]
public async Task<IActionResult> UnlockUser(
    Guid userId,
    CancellationToken cancellationToken)
{
    await _dispatcher.Send(
        new UnlockUserCommand(userId),
        cancellationToken);

    return Ok(new
   
    {
        Message = "User account unlocked successfully."
    });
}
[Authorize(Policy = "user:unlock")]
[HttpGet("users/locked")]
public async Task<IActionResult> GetLockedUsers(
    CancellationToken cancellationToken)
{
    var users = await _dispatcher.Send(
        new GetLockedUsersQuery(),
        cancellationToken);

    return Ok(users);
}

    }
}