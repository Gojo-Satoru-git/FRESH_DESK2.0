using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Interfaces;

namespace Adrenalin.unify.API.Middlewares
{
    public sealed  class SessionActivityMiddleware
    {
        private readonly RequestDelegate _next;
        public SessionActivityMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(
        HttpContext context,
        IUserSessionRepository sessions)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sessionIdClaim =
                context.User.FindFirst("session_id");

            if (sessionIdClaim != null &&
                Guid.TryParse(
                    sessionIdClaim.Value,
                    out var sessionId))
            {
                var session =
                    await sessions.GetByIdAsync(
                        sessionId,
                        context.RequestAborted);
             if (session != null &&
                    session.IsActive)
                {
                    session.UpdateActivity();

                    sessions.Update(session);
                }
            }
            await _next(context);  
        }
    } 
    }
}