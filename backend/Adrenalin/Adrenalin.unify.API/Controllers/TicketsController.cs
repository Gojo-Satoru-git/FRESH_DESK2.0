using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Commands.Tickets;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
         

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    private readonly IAuthorizationService _authService;

    public TicketsController(IDispatcher dispatcher, IAuthorizationService authService)
    {
        _dispatcher = dispatcher;
        _authService = authService;
    }

    [HttpPost]
    [Authorize(Policy = "ticket:create")]
    public async Task<IActionResult> Create([FromBody] CreateTicketCommand command)
    {
        var actorId = GetActorId();
        var isCustomer = !(await _authService.AuthorizeAsync(User, "ticket:manage")).Succeeded;

        var commandToExecute = command with
        {
            ActorId = actorId,
            IsCustomer = isCustomer
        };

        var ticketId = await _dispatcher.Send(commandToExecute);

        return Ok(new
        {
            Message = "Ticket created successfully.",
            TicketId = ticketId
        });
    }

    [HttpPut("{ticketId:guid}")]
    [Authorize(Policy = "ticket:update")]
    public async Task<IActionResult> Update(Guid ticketId, [FromBody] UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new UpdateTicketCommand(
            ticketId,
            request.Title,
            request.Description,
            request.Priority,
            request.Type,
            actorId.Value
        );

        var resultId = await _dispatcher.Send(command, cancellationToken);

        return Ok(new
        {
            Message = "Ticket updated successfully.",
            TicketId = resultId
        });
    }

    [HttpDelete("{ticketId:guid}")]
    [Authorize(Policy = "ticket:delete")]
    public async Task<IActionResult> Delete(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new DeleteTicketCommand(ticketId, actorId.Value);
        var resultId = await _dispatcher.Send(command, cancellationToken);

        return Ok(new
        {
            Message = "Ticket deleted successfully.",
            TicketId = resultId
        });
    }

    [HttpPost("{ticketId:guid}/assign")]
    [Authorize(Policy = "ticket:assign")]
   
    public async Task<IActionResult> AssignTicket(
        Guid ticketId,
        [FromBody] AssignTicketRequest request,
        CancellationToken ct)
    {
        var actorId = GetActorId();
        var agentId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new AssignTicketCommand(
             TicketId: ticketId,
             TriggeredBy: actorId ?? request.TriggeredBy, // Fixed: request.AssignedBy -> TriggeredBy
             IsAutoAssignment: false,
             OverrideAgentId: request.AgentId,
             OverrideGroupId: request.GroupId
         );

        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("{ticketId:guid}/claim")]
    [Authorize(Policy = "ticket:assign")]
    public async Task<IActionResult> ClaimTicket(Guid ticketId, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new ClaimTicketCommand(ticketId, actorId.Value);

        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { Message = "Ticket claimed successfully.", TicketId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    public record AssignTicketRequest(
        Guid TriggeredBy,
        Guid? AgentId,
        Guid? GroupId, string? Notes = null);

    [HttpPost("bulk-assign")]
    [Authorize(Policy = "ticket:assign")]
    public async Task<IActionResult> BulkAssignTickets([FromBody] BulkAssignTicketsRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new BulkAssignTicketsCommand(
            request.TicketIds,
            request.AgentId,
            request.GroupId,
            actorId.Value
        );

        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    public record BulkAssignTicketsRequest(
        List<Guid> TicketIds,
        Guid? AgentId,
        Guid? GroupId
    );

    [HttpPost("{ticketId:guid}/status")]
    [Authorize(Policy = "ticket:update")]
    public async Task<IActionResult> ChangeStatus(Guid ticketId, [FromBody] ChangeTicketStatusRequest request)
    {
        var actorId = GetActorId();
        var command = new ChangeTicketStatusCommand(
            ticketId,
            request.NewStatus,
            actorId ?? request.ChangedBy,
            request.Reason
        );

        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket status updated successfully.",
            TicketId = resultId,
            NewStatus = request.NewStatus.ToString()
        });
    }

    [HttpPost("{ticketId:guid}/comments")]
    [Authorize(Policy = "ticket:comment")]
    public async Task<ActionResult<Guid>> AddComment(Guid ticketId, [FromBody] AddCommentRequest request)
    {
        var actorId = GetActorId();
        var isCustomer = !(await _authService.AuthorizeAsync(User, "ticket:manage")).Succeeded;

        var command = new AddCommentCommand(
            ticketId,
            isCustomer ? null : (actorId ?? request.AuthorId),
            isCustomer ? (actorId ?? request.ContactId) : request.ContactId,
            request.Body,
            request.IsPrivate
        );

        try
        {
            var commentId = await _dispatcher.Send(command);
            return Ok(commentId);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            var failedEntities = string.Join(", ", ex.Entries.Select(e => $"{e.Metadata.Name} (State: {e.State})"));
            Console.WriteLine($"[CRITICAL DEBUG] DbUpdateConcurrencyException on entities: {failedEntities}");
            throw;
        }
    }

    [HttpGet("{ticketId:guid}/comments")]
    [Authorize(Policy = "ticket:read")]
    public async Task<IActionResult> GetComments(Guid ticketId, [FromQuery] bool includeInternal = false, CancellationToken cancellationToken = default)
    {
        var isCustomer = !(await _authService.AuthorizeAsync(User, "ticket:manage")).Succeeded;
        if (isCustomer)
        {
            includeInternal = false;
        }

        var query = new GetTicketCommentsQuery(ticketId, includeInternal);
        var response = await _dispatcher.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Policy = "ticket:read")]
    public async Task<IActionResult> GetTickets([FromQuery] SearchTicketsQuery query, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("my")]
    [Authorize(Policy = "ticket:read")]
    public async Task<IActionResult> GetMyTickets([FromQuery] string? status = null, [FromQuery] string? term = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var query = new GetMyTicketsQuery(actorId.Value, status, term, page, pageSize);
        var response = await _dispatcher.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("assigned")]
    [Authorize(Policy = "ticket:read")]
    public async Task<IActionResult> GetAssignedTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var query = new GetAssignedTicketsQuery(actorId.Value, page, pageSize);
        var response = await _dispatcher.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = "ticket:read")]
    public async Task<IActionResult> GetDashboard([FromQuery] Guid? companyId = null, CancellationToken cancellationToken = default)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        // Customers only see their own tickets; agents/admins/team_leads see all
        var isCustomer = !(await _authService.AuthorizeAsync(User, "ticket:manage")).Succeeded;
        var userIdFilter = isCustomer ? actorId : null;

        var query = new GetTicketDashboardQuery(companyId, userIdFilter);
        var response = await _dispatcher.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{ticketId:guid}")]
    [Authorize(Policy = "ticket:read")]
    public async Task<IActionResult> GetById(Guid ticketId)
    {
        var isCustomer = !(await _authService.AuthorizeAsync(User, "ticket:manage")).Succeeded;
        var query = new GetTicketByIdQuery(ticketId, IncludeInternalComments: !isCustomer);
        var response = await _dispatcher.Send(query);

        return Ok(response);
    }

    [HttpGet("{ticketId:guid}/activities")]
    [Authorize(Policy = "ticket:read")]
    public async Task<IActionResult> GetActivities(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var query = new GetTicketActivitiesQuery(ticketId);
        var response = await _dispatcher.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{ticketId:guid}/watchers/{userId:guid}")]
    [Authorize(Policy = "ticket:update")]
    public async Task<IActionResult> AddWatcher(Guid ticketId, Guid userId, [FromQuery] Guid? addedBy = null)
    {
        var actorId = GetActorId();
        await _dispatcher.Send(new AddWatcherCommand(ticketId, userId, actorId ?? addedBy ?? userId));

        return NoContent();
    }

    [HttpDelete("{ticketId:guid}/watchers/{userId:guid}")]
    [Authorize(Policy = "ticket:update")]
    public async Task<IActionResult> RemoveWatcher(Guid ticketId, Guid userId)
    {
        await _dispatcher.Send(new RemoveWatcherCommand(ticketId, userId));

        return NoContent();
    }

    [HttpPost("{ticketId:guid}/relations")]
    [Authorize(Policy = "ticket:update")]
    public async Task<ActionResult<Guid>> LinkTicket(Guid ticketId, [FromBody] LinkTicketRequest request)
    {
        var relationId = await _dispatcher.Send(
            new LinkTicketsCommand(
                ticketId,
                request.ChildTicketId,
                request.RelationType
            )
        );

        return Ok(relationId);
    }

    [HttpPost("{ticketId:guid}/attachments")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "ticket:comment")]
    public async Task<ActionResult<Guid>> UploadAttachment(Guid ticketId, [FromForm] UploadAttachmentRequest request)
    {
        await using var stream = request.File.OpenReadStream();
        var actorId = GetActorId();

        var attachmentId =
            await _dispatcher.Send(
                new UploadTicketAttachmentCommand(
                    ticketId,
                    request.CommentId,
                    stream,
                    request.File.FileName,
                    request.File.Length,
                    request.File.ContentType,
                    actorId ?? request.UploadedBy));

        return Ok(attachmentId);
    }

    [HttpGet("{ticketId:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Policy = "ticket:read")]
    public async Task<IActionResult> GetAttachment(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var query = new GetAttachmentQuery(ticketId, attachmentId);
        var response = await _dispatcher.Send(query, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        return File(response.Stream, response.ContentType, response.FileName);
    }

    [HttpDelete("{ticketId:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Policy = "ticket:update")]
    public async Task<IActionResult> DeleteAttachment(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var command = new DeleteTicketAttachmentCommand(ticketId, attachmentId);
        await _dispatcher.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("{ticketId:guid}/merge")]
    [Authorize(Policy = "ticket:update")]
    public async Task<ActionResult<Guid>> MergeTicket(
        Guid ticketId,
        [FromBody] MergeTicketRequest request)
    {
        var actorId = GetActorId();
        var relationId =
            await _dispatcher.Send(
                new MergeTicketCommand(
                    ticketId,
                    request.DuplicateTicketId,
                    actorId ?? request.MergedBy));

        return Ok(relationId);
    }

    [HttpPost("{ticketId:guid}/close")]
    [Authorize(Policy = "ticket:close")]
    public async Task<IActionResult> Close(Guid ticketId, [FromBody] CloseTicketRequest request)
    {
        var actorId = GetActorId();
        var command = new CloseTicketCommand(ticketId, actorId ?? request.ClosedBy, request.Notes);
        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket closed successfully.",
            TicketId = resultId
        });
    }

    [HttpPost("{ticketId:guid}/reopen")]
    [Authorize(Policy = "ticket:reopen")]
    public async Task<IActionResult> Reopen(Guid ticketId, [FromBody] ReopenTicketRequest request)
    {
        var actorId = GetActorId();
        var command = new ReopenTicketCommand(ticketId, actorId ?? request.ReopenedBy, request.Reason);
        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket reopened successfully.",
            TicketId = resultId
        });
    }

    [HttpPost("{ticketId:guid}/resolve")]
    [Authorize(Policy = "ticket:update")]
    public async Task<IActionResult> Resolve(Guid ticketId, [FromBody] ResolveTicketRequest request)
    {
        var actorId = GetActorId();
        var command = new ResolveTicketCommand(ticketId, actorId ?? request.ResolvedBy, request.ResolutionSummary);
        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket resolved successfully.",
            TicketId = resultId
        });
    }

    [HttpGet("{ticketId:guid}/history")]
    [Authorize(Policy = "ticket:read")]
    public async Task<ActionResult<TicketHistoryDto>> GetHistory(Guid ticketId, CancellationToken cancellationToken)
    {
        var query = new GetTicketHistoryQuery(ticketId);
        var response = await _dispatcher.Send(query, cancellationToken);

        return Ok(response);
    }

    [HttpPost("{ticketId:guid}/split")]
    [Authorize(Policy = "ticket:update")]
    public async Task<ActionResult<Guid>> SplitTicket(Guid ticketId, [FromBody] SplitTicketRequest request)
    {
        var actorId = GetActorId();
        var command = new SplitTicketCommand(
            ticketId,
            request.NewSubject,
            request.NewDescription,
            actorId ?? request.CreatedByUserId,
            request.CommentIdsToMove,
            request.AttachmentIdsToMove
        );

        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket split successfully.",
            NewTicketId = resultId
        });
    }

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public sealed record UpdateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    TicketType Type
);
