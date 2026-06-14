namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanyContactLimitDto(
    Guid CompanyId,
    string CompanyName,
    int MaxContacts,
    int CurrentContactCount
);
