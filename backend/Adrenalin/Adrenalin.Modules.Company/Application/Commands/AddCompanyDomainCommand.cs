using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

public sealed record AddCompanyDomainCommand(
    Guid CompanyId,
    string Domain,
    bool IsPrimary,
    Guid CreatedBy
) : IRequest<Result<Guid>>;