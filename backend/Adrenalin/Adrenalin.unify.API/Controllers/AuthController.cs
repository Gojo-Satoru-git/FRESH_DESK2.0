using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Adrenalin.unify.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController :ControllerBase
    {
       private readonly IDispatcher _dispatcher;

        public AuthController( IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterUserRequestDTO request)
        {
            
            var userId=await _dispatcher.Send(
                new RegisterUserCommand(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.LastName,
                    request.Username,
                    request.Phone
                )
            );
            return Ok(new
            {
               UserId=userId 
            });
            
        }
        [HttpPost("login")]
public async Task<IActionResult> Login(
    LoginRequestDTO request,
    CancellationToken cancellationToken)
{
    var userId =
        await _dispatcher.Send(
            new LoginCommand(
                request.Email,
                request.Password),
            cancellationToken);

    return Ok(new
    {
        UserId = userId,
        Message = "Login successful"
    });
}
    }
    
    
    
}