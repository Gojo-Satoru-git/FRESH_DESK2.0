using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Adrenalin.EventBus;               
using Adrenalin.EventBus.Events;       
using Adrenalin.Modules.Agent.Application.Commands; 
using Microsoft.AspNetCore.Authorization;

namespace Adrenalin.unify.API.Controllers.Agent;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IEventBus _eventBus; 

    public AgentController(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InitiateAgentCreation([FromBody] CreateAgentCommand command)
    {
        var correlationId = Guid.NewGuid();

        var nameParts = command.DisplayName.Split(' ', 2);
        string firstName = nameParts[0];
        string lastName = nameParts.Length > 1 ? nameParts[1] : "User";

        var integrationEvent = new CreateInternalUserIntegrationEvent(
            correlationId,
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