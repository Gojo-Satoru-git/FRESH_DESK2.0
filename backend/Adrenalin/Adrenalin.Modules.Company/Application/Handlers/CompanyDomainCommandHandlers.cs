using Adrenalin.Modules.Company.Application.Commands;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.SharedKernel.Exceptions.Company;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Handlers;

public sealed class AddCompanyDomainCommandHandler : IRequestHandler<AddCompanyDomainCommand, Result<Guid>>
{
    private readonly ICompanyRepository _companyRepository;

    public AddCompanyDomainCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<Guid>> Handle(AddCompanyDomainCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdWithDomainsAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.AddDomain(request.Domain, request.IsPrimary);

        var addedDomain = company.CompanyDomains.Last();
        return Result<Guid>.Success(addedDomain.Id);
    }
}

public sealed class SetPrimaryCompanyDomainCommandHandler : IRequestHandler<SetPrimaryCompanyDomainCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public SetPrimaryCompanyDomainCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(SetPrimaryCompanyDomainCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdWithDomainsAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.SetPrimaryDomain(request.DomainId);
        return Result.Success();
    }
}

public sealed class DeleteCompanyDomainCommandHandler : IRequestHandler<DeleteCompanyDomainCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public DeleteCompanyDomainCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(DeleteCompanyDomainCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdWithDomainsAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        var domain = company.CompanyDomains.FirstOrDefault(d => d.Id == request.DomainId)
            ?? throw new CompanyDomainNotFoundException(request.CompanyId, request.DomainId);

        domain.Delete();
        return Result.Success();
    }
}

public sealed class VerifyCompanyDomainCommandHandler : IRequestHandler<VerifyCompanyDomainCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public VerifyCompanyDomainCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(VerifyCompanyDomainCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdWithDomainsAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        var domain = company.CompanyDomains.FirstOrDefault(d => d.Id == request.DomainId)
            ?? throw new CompanyDomainNotFoundException(request.CompanyId, request.DomainId);

        domain.Verify();
        return Result.Success();
    }
}
