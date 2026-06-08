using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;                     

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public TicketsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketCommand command)
    {
        var ticketId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket created successfully.",
            TicketId = ticketId
        });
    }

    // POST api/tickets/{id}/assign  — manual assignment
    [HttpPost("{id:guid}/assign")]
    [Authorize]
    public async Task<IActionResult> AssignTicket(
        Guid id,
        [FromBody] AssignTicketRequest request,
        CancellationToken ct)
    {
        var agentId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new AssignTicketCommand(
            TicketId: id,
            TriggeredBy: agentId,
            IsAutoAssignment: false,
            OverrideAgentId: request.AgentId,
            OverrideGroupId: request.GroupId
        );

        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    public record AssignTicketRequest(
        Guid TriggeredBy,
        Guid? AgentId,
        Guid? GroupId);

    [HttpPost("{ticketId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid ticketId, [FromBody] ChangeTicketStatusRequest request)
    {
        var command = new ChangeTicketStatusCommand(ticketId, request.NewStatus, request.ChangedBy, request.Reason);

        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket status updated successfully.",
            TicketId = resultId,
            NewStatus = request.NewStatus.ToString()
        });
    }

    [HttpPost("{ticketId:guid}/comments")]
    public async Task<ActionResult<Guid>> AddComment(Guid ticketId, [FromBody] AddCommentRequest request)
    {
        var command = new AddCommentCommand(
            ticketId,
            request.AuthorId,
            request.ContactId,
            request.Body,
            request.Visibility
        );

        var commentId = await _dispatcher.Send(command);

        return Ok(commentId);
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] GetTicketsQuery query, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetById(Guid ticketId)
    {
        var query = new GetTicketByIdQuery(ticketId);
        var response = await _dispatcher.Send(query);

        return Ok(response);
    }

    //[HttpPost("{ticketId:guid}/watchers")]
    //public async Task<IActionResult> AddWatcher(Guid ticketId, [FromBody] AddWatcherRequest request)
    //{
    //    await _dispatcher.Send(new AddWatcherCommand(ticketId, request.UserId, request.AddedBy));

    //    return NoContent();
    //}

    [HttpPost("{ticketId:guid}/watchers/{userId:guid}")]
    public async Task<IActionResult> AddWatcher(Guid ticketId, Guid userId, [FromQuery] Guid? addedBy = null)
    {
        await _dispatcher.Send(new AddWatcherCommand(ticketId, userId, addedBy ?? userId));

        return NoContent();
    }

    [HttpDelete("{ticketId:guid}/watchers/{userId:guid}")]
    public async Task<IActionResult> RemoveWatcher(Guid ticketId, Guid userId)
    {
        await _dispatcher.Send(new RemoveWatcherCommand(ticketId, userId));

        return NoContent();
    }

    [HttpPost("{ticketId:guid}/relations")]
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
    public async Task<ActionResult<Guid>> UploadAttachment(Guid ticketId, [FromForm] UploadAttachmentRequest request)
    {
        await using var stream = request.File.OpenReadStream();

        var attachmentId =
            await _dispatcher.Send(
                new UploadTicketAttachmentCommand(
                    ticketId,
                    request.CommentId,
                    stream,
                    request.File.FileName,
                    request.File.Length,
                    request.File.ContentType,
                    request.UploadedBy));

        return Ok(attachmentId);
    }

    [HttpGet("{ticketId:guid}/attachments/{attachmentId:guid}")]
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
    public async Task<IActionResult> DeleteAttachment(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var command = new DeleteTicketAttachmentCommand(ticketId, attachmentId);
        await _dispatcher.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("{ticketId:guid}/merge")]
    public async Task<ActionResult<Guid>> MergeTicket(
    Guid ticketId,
    [FromBody] MergeTicketRequest request)
    {
        var relationId =
            await _dispatcher.Send(
                new MergeTicketCommand(
                    ticketId,
                    request.DuplicateTicketId,
                    request.MergedBy));

        return Ok(relationId);
    }

    [HttpPost("{ticketId:guid}/close")]
    public async Task<IActionResult> Close(Guid ticketId, [FromBody] CloseTicketRequest request)
    {
        var command = new CloseTicketCommand(ticketId, request.ClosedBy, request.Notes);
        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket closed successfully.",
            TicketId = resultId
        });
    }

    [HttpPost("{ticketId:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid ticketId, [FromBody] ReopenTicketRequest request)
    {
        var command = new ReopenTicketCommand(ticketId, request.ReopenedBy, request.Reason);
        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket reopened successfully.",
            TicketId = resultId
        });
    }

    [HttpPost("{ticketId:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid ticketId, [FromBody] ResolveTicketRequest request)
    {
        var command = new ResolveTicketCommand(ticketId, request.ResolvedBy, request.ResolutionSummary);
        var resultId = await _dispatcher.Send(command);

        return Ok(new
        {
            Message = "Ticket resolved successfully.",
            TicketId = resultId
        });
    }

    [HttpGet("{ticketId:guid}/history")]
    public async Task<ActionResult<TicketHistoryDto>> GetHistory(Guid ticketId, CancellationToken cancellationToken)
    {
        var query = new GetTicketHistoryQuery(ticketId);
        var response = await _dispatcher.Send(query, cancellationToken);

        return Ok(response);
    }

    [HttpPost("{ticketId:guid}/split")]
    public async Task<ActionResult<Guid>> SplitTicket(Guid ticketId, [FromBody] SplitTicketRequest request)
    {
        var command = new SplitTicketCommand(
            ticketId,
            request.NewSubject,
            request.NewDescription,
            request.CreatedByUserId,
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
}
