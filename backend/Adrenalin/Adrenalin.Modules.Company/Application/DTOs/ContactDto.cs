namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record ContactDto(
    Guid Id,
    Guid CompanyId,
    Guid? UserId,
    string Name,
    string Email,
    string? Phone,
    bool IsAuthorized,
    bool AutoCreated,
    bool CanViewAllTickets,
    DateTimeOffset CreatedAt
);