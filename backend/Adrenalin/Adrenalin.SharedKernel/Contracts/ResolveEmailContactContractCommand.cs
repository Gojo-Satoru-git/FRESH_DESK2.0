using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.SharedKernel.Contracts;

public sealed record ResolveEmailContactContractCommand(
    string EmailAddress,
    string DisplayName,
    Guid? RequestingUserId
) : IRequest<Result<ResolveEmailContactContractResult>>;

public sealed record ResolveEmailContactContractResult(
    Guid? CompanyId,
    Guid? ContactId,
    Guid? UserId,
    bool AutoCreated
);
