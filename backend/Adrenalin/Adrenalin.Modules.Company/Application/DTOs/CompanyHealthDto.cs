namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanyHealthDto(
    Guid CompanyId,
    string Name,
    int HealthScore,
    bool DeliverySupportActive,
    bool PaymentOnHold,
    bool IsActive
);
