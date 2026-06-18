using System;

namespace Adrenalin.Modules.Lookup.Application.DTOs;

public sealed record SolutionTypeDto(
    Guid Id,
    string Code,
    string Label,
    bool IsActive);
