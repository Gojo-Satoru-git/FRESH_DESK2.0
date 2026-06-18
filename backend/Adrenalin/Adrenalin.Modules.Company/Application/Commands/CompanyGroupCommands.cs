using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

// ── Company ↔ Group ───────────────────────────────────────────────────────────

public sealed record AssignCompanyToGroupCommand(
    Guid CompanyId, Guid GroupId, bool IsDefault, int Priority, Guid ActorId)
    : IRequest<Result>;

public sealed record RemoveCompanyFromGroupCommand(
    Guid CompanyId, Guid GroupId, Guid ActorId)
    : IRequest<Result>;

public sealed record SetDefaultCompanyGroupCommand(
    Guid CompanyId, Guid GroupId, Guid ActorId)
    : IRequest<Result>;
