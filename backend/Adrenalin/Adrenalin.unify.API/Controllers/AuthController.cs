using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Adrenalin.unify.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController :ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator =mediator ;
        }
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterUserRequestDTO request)
        {
            
            var userId=await _mediator.Send(
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
    }
}