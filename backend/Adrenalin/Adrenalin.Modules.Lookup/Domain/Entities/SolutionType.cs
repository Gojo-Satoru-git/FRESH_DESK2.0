using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

/// <summary>
/// Valid solution_type values: data_correction, patch_deployment, configuration, clarification, server_outage, ad_hoc, known_issue. Replaces free-text varchar on tickets.
/// </summary>
public partial class SolutionType
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
