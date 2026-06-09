namespace Adrenalin.Modules.Ticketing.Domain.Exceptions;

#pragma warning disable RCS1194 // Implement exception constructors
public class TicketDomainException : Exception
#pragma warning restore RCS1194
{
    public TicketDomainException(string message) : base(message) { }
}
