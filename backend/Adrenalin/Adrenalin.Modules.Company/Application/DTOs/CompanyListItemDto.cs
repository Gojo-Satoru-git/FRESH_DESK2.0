namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanyListItemDto(
    Guid Id,
    string Name,
    string GeoRegion,
    string SupportTier,
    string? Industry,
    int HealthScore,
    bool IsActive,
    bool PaymentOnHold,
    Guid? CamUserId,
    int ContactCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
