using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

public sealed record AssignDeliveryManagerCommand(
    Guid CompanyId,
    Guid DeliveryManagerId,
    Guid ModifiedBy
) : IRequest<Result>;