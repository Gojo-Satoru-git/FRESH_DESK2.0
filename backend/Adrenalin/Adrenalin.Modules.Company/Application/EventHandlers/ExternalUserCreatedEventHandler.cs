using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Company.Applications.Commands;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;


namespace Adrenalin.Modules.Company.Applications.EventHandlers
{
    public sealed  class ExternalUserCreatedEventHandler : IIntegrationEventHandler<ExternalUserCreatedEvent>
    {
          private readonly IDispatcher _dispatcher;

    public ExternalUserCreatedEventHandler(
         IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
     public async Task HandleAsync(
        ExternalUserCreatedEvent notification,
        CancellationToken cancellationToken)
    {
         Console.WriteLine("EVENT HANDLER HIT");
        await _dispatcher.Send(
            new CreateContactForExternalUserCommand(
                notification.UserId,
                notification.CompanyId,
                notification.Email,
                notification.FullName,
                 notification.CanViewAllTickets,
                notification.CreatedBy),
            cancellationToken);
        
    }
    }
}