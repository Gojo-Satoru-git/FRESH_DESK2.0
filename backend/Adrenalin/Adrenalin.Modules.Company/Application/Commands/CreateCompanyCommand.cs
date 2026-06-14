using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

public sealed record CreateCompanyCommand(
    string Name,
    string GeoRegion,
    string SupportTier,
    string? CspId,
    string? Industry,
    bool IsPayrollCustomer,
    string? LeaveCreditCycle,
    DateTimeOffset? MigrationDate,
    Guid CreatedBy
) : IRequest<Result<Guid>>;