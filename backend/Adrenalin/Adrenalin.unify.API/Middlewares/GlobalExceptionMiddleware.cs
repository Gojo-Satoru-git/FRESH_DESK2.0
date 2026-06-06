using FluentValidation;
using System.Net;
namespace Adrenalin.unify.API.Middlewares
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");

            context.Response.StatusCode =
                (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    Error = "Validation Failed",
                    Errors = ex.Errors.Select(x => new
                    {
                        x.PropertyName,
                        x.ErrorMessage
                    })
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    Error = ex.Message
                });
        }
    }
    }
}