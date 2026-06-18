using System;

namespace Adrenalin.Modules.Lookup.Application.DTOs;

public sealed record ModuleDto(
    Guid Id,
    string Code,
    string Label,
    string? Description,
    bool IsActive);
