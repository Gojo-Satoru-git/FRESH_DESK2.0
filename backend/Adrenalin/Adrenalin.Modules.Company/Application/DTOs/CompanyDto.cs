namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanyDto(
    Guid Id,
    string Name,
    string GeoRegion,
    string SupportTier,
    int HealthScore,
    bool IsActive,
    DateTimeOffset CreatedAt
);