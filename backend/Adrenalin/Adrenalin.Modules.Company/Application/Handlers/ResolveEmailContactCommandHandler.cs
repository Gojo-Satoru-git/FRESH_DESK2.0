using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.Modules.Company.Domain.Entities;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.Modules.Company.Application.Commands;
using Microsoft.Extensions.Logging;

using Adrenalin.SharedKernel.Results;
using Adrenalin.SharedKernel.Contracts;

namespace Adrenalin.Modules.Company.Application.Handlers;

public sealed class ResolveEmailContactCommandHandler : IRequestHandler<ResolveEmailContactContractCommand, Result<ResolveEmailContactContractResult>>
{
    private readonly IContactRepository _contactRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResolveEmailContactCommandHandler> _logger;

    public ResolveEmailContactCommandHandler(
        IContactRepository contactRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        ILogger<ResolveEmailContactCommandHandler> logger)
    {
        _contactRepository = contactRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ResolveEmailContactContractResult>> Handle(ResolveEmailContactContractCommand request, CancellationToken cancellationToken)
    {
        var email = request.EmailAddress.Trim().ToLowerInvariant();

        // 1. Try to find an existing contact
        var existingContact = await _contactRepository.GetByEmailAsync(email, cancellationToken);
        if (existingContact != null)
        {
            _logger.LogInformation("Contact resolved by direct email match: {Email}", email);
            return Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(
                existingContact.CompanyId,
                existingContact.Id,
                existingContact.UserId,
                false
            ));
        }

        // 2. Fallback to Domain Matching
        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(null, null, null, false));
        }

        var domain = parts[1];
        var matchingCompany = await _companyRepository.GetByDomainAsync(domain, cancellationToken);
        
        if (matchingCompany != null)
        {
            if (matchingCompany.AllowAutoContactCreation)
            {
                Guid? userId = request.RequestingUserId;
                if (userId.HasValue && userId.Value == Guid.Empty)
                {
                    userId = null;
                }

                _logger.LogInformation(
                    "Email Contact Resolution. Email={Email}, Company={CompanyId}, RequestingUserId={RequestingUserId}, FinalUserId={FinalUserId}",
                    email,
                    matchingCompany.Id,
                    request.RequestingUserId,
                    userId
                );

                var newContact = Contact.Create(
                    matchingCompany.Id,
                    email,
                    string.IsNullOrWhiteSpace(request.DisplayName) ? email : request.DisplayName,
                    true,
                    true,
                    userId
                );
                
                await _contactRepository.AddAsync(newContact, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Contact auto-created via domain match: {Email} for Company {CompanyId}", email, matchingCompany.Id);

                return Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(
                    matchingCompany.Id,
                    newContact.Id,
                    userId,
                    true
                ));
            }

            _logger.LogInformation("Company resolved via domain, but auto-creation disabled: {Domain}", domain);
            return Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(
                matchingCompany.Id,
                null,
                null,
                false
            ));
        }

        // 3. Unclassified
        _logger.LogInformation("No Company or Contact resolved for: {Email}", email);
        return Result<ResolveEmailContactContractResult>.Success(new ResolveEmailContactContractResult(
            null,
            null,
            null,
            false
        ));
    }
}
