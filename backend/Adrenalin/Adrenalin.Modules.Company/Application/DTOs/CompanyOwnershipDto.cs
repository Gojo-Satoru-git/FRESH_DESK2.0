namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanyOwnershipDto(
    Guid CompanyId,
    string Name,
    Guid? CamUserId,
    Guid? DeliveryManagerId,
    bool DeliverySupportActive
);
