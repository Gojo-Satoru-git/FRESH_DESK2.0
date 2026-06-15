using Adrenalin.Modules.Company.Application.Commands;
using Adrenalin.Modules.Company.Domain.Interfaces;
using Adrenalin.SharedKernel.Exceptions.Company;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Handlers;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<Guid>>
{
    private readonly ICompanyRepository _companyRepository;

    public CreateCompanyCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = Domain.Entities.Company.Create(
            request.Name,
            request.GeoRegion,
            request.SupportTier,
            request.CreatedBy);

        await _companyRepository.AddAsync(company, cancellationToken);
        return Result<Guid>.Success(company.Id);
    }
}

public sealed class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public UpdateCompanyCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.UpdateDetails(request.Name, request.Industry, request.SupportTier, request.ModifiedBy);
        return Result.Success();
    }
}

public sealed class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public DeleteCompanyCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.Delete();
        return Result.Success();
    }
}

public sealed class RestoreCompanyCommandHandler : IRequestHandler<RestoreCompanyCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public RestoreCompanyCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(RestoreCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.Restore();
        return Result.Success();
    }
}

public sealed class ActivateCompanyCommandHandler : IRequestHandler<ActivateCompanyCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public ActivateCompanyCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(ActivateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.Activate(request.ModifiedBy);
        return Result.Success();
    }
}

public sealed class DeactivateCompanyCommandHandler : IRequestHandler<DeactivateCompanyCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public DeactivateCompanyCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.Deactivate(request.ModifiedBy);
        return Result.Success();
    }
}

public sealed class AssignCamCommandHandler : IRequestHandler<AssignCamCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public AssignCamCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(AssignCamCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.AssignCam(request.CamUserId, request.ModifiedBy);
        return Result.Success();
    }
}

public sealed class AssignDeliveryManagerCommandHandler : IRequestHandler<AssignDeliveryManagerCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public AssignDeliveryManagerCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(AssignDeliveryManagerCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.AssignDeliveryManager(request.DeliveryManagerId, request.ModifiedBy);
        return Result.Success();
    }
}

public sealed class UpdateHealthScoreCommandHandler : IRequestHandler<UpdateHealthScoreCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public UpdateHealthScoreCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(UpdateHealthScoreCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.UpdateHealthScore(request.Score, request.ModifiedBy);
        return Result.Success();
    }
}

public sealed class UpdateTierCommandHandler : IRequestHandler<UpdateTierCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public UpdateTierCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(UpdateTierCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        company.UpdateDetails(company.Name, company.Industry, request.SupportTier, request.ModifiedBy);
        return Result.Success();
    }
}

public sealed class UpdateContactLimitCommandHandler : IRequestHandler<UpdateContactLimitCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;

    public UpdateContactLimitCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(UpdateContactLimitCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdWithAllAsync(request.CompanyId, cancellationToken)
            ?? throw new CompanyNotFoundException(request.CompanyId);

        if (company.CompanyContactsLimit is null)
        {
            // Create new limit — EF tracks the entity via company navigation
            var limit = Domain.Entities.CompanyContactsLimit.Create(company.Id, request.MaxContacts);
            // We need to directly work with the DbContext for this; set via reflection would be wrong.
            // Instead, let the UoW handle this through the company aggregate.
            throw new CompanyContactLimitNotConfiguredException();
        }

        company.CompanyContactsLimit.ChangeLimit(request.MaxContacts, request.ModifiedBy);
        return Result.Success();
    }
}
