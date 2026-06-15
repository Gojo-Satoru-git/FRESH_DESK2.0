using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Company.Applications.Commands;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Company.Applications.Handlers
{
    public class CreateContactForExternalUserCommandHandler: IRequestHandler<CreateContactForExternalUserCommand, Guid>
    {
         private readonly IContactRepository _contacts;
         public CreateContactForExternalUserCommandHandler(
        IContactRepository contacts)
    {
        _contacts = contacts;
    }
    public async Task<Guid> Handle(
        CreateContactForExternalUserCommand request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("COMMAND HANDLER HIT");
        var contact =
            Contact.Create(
                request.CompanyId,
                request.Email,
                request.FullName,
                true,
                true,
                request.UserId);

        await _contacts.AddAsync(
            contact,
            cancellationToken);
        Console.WriteLine($"CONTACT CREATED : {contact.Id}");

        return contact.Id;
    }
    }
}