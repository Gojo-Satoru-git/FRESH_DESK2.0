using System;

namespace Adrenalin.Modules.Lookup.Application.DTOs;

public sealed record CustomerTierDto(
    Guid Id,
    string Code,
    string Label,
    string? Description,
    int PriorityBump);
