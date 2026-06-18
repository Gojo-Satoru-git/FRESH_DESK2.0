using System;

namespace Adrenalin.Modules.Lookup.Application.DTOs;

public sealed record ProductVersionDto(
    Guid Id,
    string Code,
    string Label,
    DateOnly? ReleaseDate,
    bool IsLegacy);
