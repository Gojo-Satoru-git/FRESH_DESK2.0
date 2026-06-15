using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

public sealed record AssignCamCommand(
    Guid CompanyId,
    Guid CamUserId,
    Guid ModifiedBy
) : IRequest<Result>;