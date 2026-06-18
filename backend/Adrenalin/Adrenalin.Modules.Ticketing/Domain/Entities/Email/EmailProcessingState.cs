namespace Adrenalin.Modules.Ticketing.Domain.Entities.Email;

public enum EmailProcessingState
{
    Pending,
    Processing,
    Processed,
    Ignored,
    Failed,
    DeadLetter
}
