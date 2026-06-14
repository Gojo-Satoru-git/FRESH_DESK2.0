using Adrenalin.Modules.Company.Application.Commands;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Adrenalin.unify.API.Controllers;

/// <summary>
/// Manages contact-level operations (update, delete, authorize, deactivate).
/// Company-scoped contact operations (list, create) are in CompaniesController.
/// </summary>
[ApiController]
[Route("api/contacts")]
[Authorize]
[Tags("Contacts")]
public sealed class ContactsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public ContactsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>Updates a contact's information.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "contact:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new UpdateContactCommand(id, request.Name, request.Email, request.Phone, actorId.Value);
        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { Message = "Contact updated successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Soft-deletes a contact.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "contact:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new DeleteContactCommand(id, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Contact deleted successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Authorizes a contact for portal access.</summary>
    [HttpPost("{id:guid}/authorize")]
    [Authorize(Policy = "contact:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AuthorizeContact(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new AuthorizeContactCommand(id, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Contact authorized successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Deactivates (revokes authorization from) a contact.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "contact:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new DeactivateContactCommand(id, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Contact deactivated successfully." })
            : BadRequest(new { error = result.Error });
    }

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public sealed record UpdateContactRequest(string Name, string Email, string? Phone = null);
