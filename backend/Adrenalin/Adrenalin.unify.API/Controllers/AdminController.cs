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
        private readonly ICurrentUserService _currentUserService;

        public AdminController(
        IDispatcher dispatcher, ICurrentUserService currentUserService)
    {
        _dispatcher = dispatcher;
            _currentUserService= currentUserService;
       
    }
    [HttpPost("internal-users")]
    public async Task<IActionResult> CreateInternalUser(
        CreateInternalUserRequestDTO request,

        CancellationToken cancellationToken)
    {
            var adminId = _currentUserService.UserId; // Or whatever your injected current user service field name is here
            if (!adminId.HasValue)
            {
                return Unauthorized("Admin context session not found.");
            }
            var userId =
            await _dispatcher.Send(
                new CreateInternalUserCommand(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Phone,
                 request.RoleName, adminId.Value, IsSystemCall: false),
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

    }
}