using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.unify.API.Middlewares
{
    public sealed  class SessionActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionActivityMiddleware> _logger;
       
        public SessionActivityMiddleware(
            RequestDelegate next,
            ILogger<SessionActivityMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
           
        }
    public async Task InvokeAsync(
    HttpContext context,
    IUserSessionRepository sessions,
    IUnitOfWork unitOfWork)
{
    var user = context.User;

    // 1. If no identity → PUBLIC API → just continue
    if (user?.Identity?.IsAuthenticated != true)
    {
        await _next(context);
        return;
    }

    // 2. Extract session id safely
    var sessionIdClaim = user.FindFirst("session_id");

    if (sessionIdClaim == null || !Guid.TryParse(sessionIdClaim.Value, out var sessionId))
    {
        // Authenticated user but no session → optional:
        // either ignore or block depending on security level
        await _next(context);
        return;
    }

    // 3. Load session
    var session = await sessions.GetByIdAsync(sessionId, context.RequestAborted);

    if (session == null || !session.IsActive)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Session expired");
        return;
    }

    // 4. Update activity (ONLY valid session)
    session.UpdateActivity();
    sessions.Update(session);
    await unitOfWork.SaveChangesAsync(context.RequestAborted);

    await _next(context);
}
    }
}