namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanyDomainDto(
    Guid Id,
    Guid CompanyId,
    string Domain,
    bool IsPrimary,
    bool IsVerified,
    DateTimeOffset? VerifiedAt
);