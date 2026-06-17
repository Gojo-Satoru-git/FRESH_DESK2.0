using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

public sealed record UpdateCompanyCommand(
    Guid CompanyId,
    string Name,
    string GeoRegion,
    string SupportTier,
    string? Industry,
    string? Notes,
    Guid ModifiedBy
) : IRequest<Result>;