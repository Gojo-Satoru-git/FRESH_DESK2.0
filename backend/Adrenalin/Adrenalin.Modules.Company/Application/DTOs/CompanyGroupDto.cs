namespace Adrenalin.Modules.Company.Application.DTOs;

public sealed record CompanyGroupDto(
    Guid CompanyId,
    string CompanyName,
    Guid GroupId,
    string GroupName,
    bool IsDefault,
    int Priority,
    DateTimeOffset CreatedAt);
