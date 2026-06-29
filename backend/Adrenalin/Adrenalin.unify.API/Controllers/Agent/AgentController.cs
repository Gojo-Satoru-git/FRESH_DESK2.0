using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Adrenalin.EventBus;               
using Adrenalin.EventBus.Events;       
using Adrenalin.Modules.Agent.Application.Commands; 
using Microsoft.AspNetCore.Authorization;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.unify.API.Controllers.Agent;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IEventBus _eventBus;
    private readonly ICurrentUserService _currentUserService;
    public AgentController(IEventBus eventBus, ICurrentUserService currentUserService)
    {
        _eventBus = eventBus;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IActionResult> InitiateAgentCreation([FromBody] CreateAgentCommand command)
    {
        var correlationId = Guid.NewGuid();
        var adminId = _currentUserService.UserId;
        var nameParts = command.DisplayName.Split(' ', 2);
        string firstName = nameParts[0];
        string lastName = nameParts.Length > 1 ? nameParts[1] : "User";
        var adminEmail = _currentUserService.Email;
        var integrationEvent = new CreateInternalUserIntegrationEvent(
            correlationId,
            adminId.Value,
            adminEmail,
            command.Email.ToLower(),
            firstName,
            lastName,
            command.Phone,
            command.AccessLevel,
            command.ShiftFactorValueId,
            command.Timezone,
            command.ShiftStart,
            command.ShiftEnd,
            command.WorkingDays,
            command.MaxConcurrentTickets
        );

        await _eventBus.PublishAsync(integrationEvent);

        return Accepted(new { Message = "Agent creation initiated. Provisioning identity context...", CorrelationId = correlationId });
    }
}