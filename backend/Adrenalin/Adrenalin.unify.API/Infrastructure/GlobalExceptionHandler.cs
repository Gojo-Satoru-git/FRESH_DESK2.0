using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Adrenalin.unify.API.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred: {Message}",
            exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                notFoundException.Message
            ),
            ForbiddenException forbiddenException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                forbiddenException.Message
            ),
            UnauthorizedAccessException unauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                unauthorizedAccessException.Message
            ),
            BadRequestException badRequestException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                badRequestException.Message
            ),
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                validationException.Message
            ),
            TicketDomainException ticketDomainException => MapTicketDomainException(ticketDomainException),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred on the server."
            )
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException valException && valException.Errors != null && valException.Errors.Any())
        {
            problemDetails.Extensions["errors"] = valException.Errors;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapTicketDomainException(TicketDomainException ex)
    {
        var msg = ex.Message ?? string.Empty;

        // Route TicketDomainException to the correct HTTP status code based on common phrasing:
        if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase) || 
            msg.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status404NotFound, "Not Found", msg);
        }

        if (msg.Contains("belong to", StringComparison.OrdinalIgnoreCase) || 
            msg.Contains("company", StringComparison.OrdinalIgnoreCase) || 
            msg.Contains("authorized", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("tenancy", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status403Forbidden, "Forbidden", msg);
        }

        return (StatusCodes.Status400BadRequest, "Bad Request", msg);
    }
}
