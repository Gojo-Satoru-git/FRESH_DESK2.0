using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

public sealed record CreateContactCommand(
    Guid CompanyId,
    string Name,
    string Email,
    string? Phone,
    bool IsAuthorized,
    Guid CreatedBy
) : IRequest<Result<Guid>>;