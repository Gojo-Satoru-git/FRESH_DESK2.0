using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

public sealed record SetPrimaryCompanyDomainCommand(
    Guid CompanyId,
    Guid DomainId,
    Guid ModifiedBy
) : IRequest<Result>;