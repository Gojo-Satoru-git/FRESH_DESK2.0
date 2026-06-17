using System;

namespace Adrenalin.SharedKernel.Exceptions.Company;

public class ContactNotFoundException : NotFoundException
{
    public ContactNotFoundException(Guid contactId) 
        : base($"Contact with ID '{contactId}' was not found.")
    {
    }
}
