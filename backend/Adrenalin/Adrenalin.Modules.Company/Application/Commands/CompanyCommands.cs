using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Commands;

public sealed record DeleteCompanyCommand(Guid CompanyId, Guid DeletedBy) : IRequest<Result>;

public sealed record RestoreCompanyCommand(Guid CompanyId, Guid RestoredBy) : IRequest<Result>;

public sealed record ActivateCompanyCommand(Guid CompanyId, Guid ModifiedBy) : IRequest<Result>;

public sealed record DeactivateCompanyCommand(Guid CompanyId, Guid ModifiedBy) : IRequest<Result>;

public sealed record UpdateHealthScoreCommand(Guid CompanyId, int Score, Guid ModifiedBy) : IRequest<Result>;

public sealed record UpdateTierCommand(Guid CompanyId, string SupportTier, Guid ModifiedBy) : IRequest<Result>;

public sealed record UpdateContactLimitCommand(Guid CompanyId, int MaxContacts, Guid ModifiedBy) : IRequest<Result>;

public sealed record DeleteCompanyDomainCommand(Guid CompanyId, Guid DomainId, Guid DeletedBy) : IRequest<Result>;

public sealed record VerifyCompanyDomainCommand(Guid CompanyId, Guid DomainId, Guid ModifiedBy) : IRequest<Result>;

public sealed record UpdateContactCommand(Guid ContactId, string Name, string Email, string? Phone, Guid ModifiedBy) : IRequest<Result>;

public sealed record DeleteContactCommand(Guid ContactId, Guid DeletedBy) : IRequest<Result>;

public sealed record AuthorizeContactCommand(Guid ContactId, Guid ModifiedBy) : IRequest<Result>;

public sealed record DeactivateContactCommand(Guid ContactId, Guid ModifiedBy) : IRequest<Result>;
