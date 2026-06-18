using System;

namespace Adrenalin.Modules.Lookup.Application.DTOs;

public sealed record GeoRegionDto(
    Guid Id,
    string Code,
    string Label,
    string Timezone,
    TimeOnly BusinessStart,
    TimeOnly BusinessEnd,
    string WorkingDays);
