using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Adrenalin.Modules.AI.Application.Queries;
using Adrenalin.Modules.AI.Application.Commands;
using Adrenalin.Modules.AI.Application.DTOs;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("tickets/{ticketId:guid}/summary")]
    [Authorize(Policy = "ai:view")]
    public async Task<ActionResult<TicketSummaryDto>> GenerateSummary(Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GenerateTicketSummaryQuery(ticketId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("tickets/{ticketId:guid}/suggest-resolution")]
    [Authorize(Policy = "ai:suggest")]
    public async Task<ActionResult<ResolutionSuggestionDto>> SuggestResolution(Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SuggestTicketResolutionQuery(ticketId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("tickets/{ticketId:guid}/sentiment")]
    [Authorize(Policy = "ai:analyze")]
    public async Task<ActionResult<SentimentAnalysisDto>> AnalyzeSentiment(Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AnalyzeTicketSentimentQuery(ticketId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("tickets/{ticketId:guid}/categorize")]
    [Authorize(Policy = "ai:categorize")]
    public async Task<ActionResult<TicketCategorizationDto>> Categorize(Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AutoCategorizeTicketCommand(ticketId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("tickets/{ticketId:guid}/similar")]
    [Authorize(Policy = "ai:suggest")]
    public async Task<ActionResult> SuggestSimilar(Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SuggestSimilarHistoricalTicketsQuery(ticketId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("tickets/{ticketId:guid}/kb-suggestions")]
    [Authorize(Policy = "ai:suggest")]
    public async Task<ActionResult> SuggestKb(Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SuggestKnowledgeBaseArticlesQuery(ticketId), cancellationToken);
        return Ok(result);
    }
}
