using Adrenalin.SharedKernel.Mediator; 

namespace Adrenalin.SharedKernel.Contracts;

public record SlaBreachNotificationContract(
    Guid TicketId,
    string TicketNumber,
    string BreachType,         
    string EscalationRuleName, 
    string TargetRole,         
    List<Guid> TargetUserIds   
) : INotification;             