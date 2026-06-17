using Adrenalin.Modules.Company.Application.Commands;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.SharedKernel.Exceptions.Company;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Handlers;

public sealed class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, Result<Guid>>
{
    private readonly ICompanyRepository _companyRepository;

    public CreateContactCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<Guid>> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdWithContactsAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        var contact = company.AddContact(request.Name, request.Email, request.Phone);
        return Result<Guid>.Success(contact.Id);
    }
}

public sealed class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, Result>
{
    private readonly IContactRepository _contactRepository;

    public UpdateContactCommandHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<Result> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(request.ContactId, cancellationToken)
            ?? throw new ContactNotFoundException(request.ContactId);

        contact.Update(request.Name, request.Email, request.Phone);
        return Result.Success();
    }
}

public sealed class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, Result>
{
    private readonly IContactRepository _contactRepository;

    public DeleteContactCommandHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<Result> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(request.ContactId, cancellationToken)
            ?? throw new ContactNotFoundException(request.ContactId);

        contact.Delete();
        return Result.Success();
    }
}

public sealed class AuthorizeContactCommandHandler : IRequestHandler<AuthorizeContactCommand, Result>
{
    private readonly IContactRepository _contactRepository;

    public AuthorizeContactCommandHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<Result> Handle(AuthorizeContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(request.ContactId, cancellationToken)
            ?? throw new ContactNotFoundException(request.ContactId);

        contact.Authorize();
        return Result.Success();
    }
}

public sealed class DeactivateContactCommandHandler : IRequestHandler<DeactivateContactCommand, Result>
{
    private readonly IContactRepository _contactRepository;

    public DeactivateContactCommandHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<Result> Handle(DeactivateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(request.ContactId, cancellationToken)
            ?? throw new ContactNotFoundException(request.ContactId);

        contact.RevokeAuthorization();
        return Result.Success();
    }
}
