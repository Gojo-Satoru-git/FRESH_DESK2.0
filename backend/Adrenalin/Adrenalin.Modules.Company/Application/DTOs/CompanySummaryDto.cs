namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanySummaryDto(
    Guid Id,
    string Name,
    string SupportTier,
    string GeoRegion,
    int HealthScore,
    bool IsActive,
    int ContactCount,
    int DomainCount,
    Guid? CamUserId,
    Guid? DeliveryManagerId
);
