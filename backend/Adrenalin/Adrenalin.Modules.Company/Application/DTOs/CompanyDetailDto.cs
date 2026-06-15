namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanyDetailDto(
    Guid Id,
    string Name,
    string? CspId,
    string GeoRegion,
    string SupportTier,
    string? Industry,
    int HealthScore,
    bool IsActive,
    bool IsPayrollCustomer,
    bool PaymentOnHold,
    bool DeliverySupportActive,
    string? LeaveCreditCycle,
    DateOnly? MigrationDate,
    string? Notes,
    Guid? CamUserId,
    Guid? DeliveryManagerId,
    int ContactCount,
    int ContactLimitMax,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy,
    IReadOnlyList<CompanyDomainDto> Domains
);