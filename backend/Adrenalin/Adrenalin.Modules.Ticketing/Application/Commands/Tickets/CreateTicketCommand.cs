using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Commands;

public sealed record CreateTicketCommand(Guid CompanyId, Guid ModuleId, string Subject, string Description, Guid? ContactId, Guid? CreatedByUserId) 
    : IRequest<Guid>;