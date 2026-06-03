using System;
using System.Collections.Generic;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Lookup.Domain.Entities;

/// <summary>
/// Valid solution_type values: data_correction, patch_deployment, configuration, clarification, server_outage, ad_hoc, known_issue. Replaces free-text varchar on tickets.
/// </summary>
public partial class SolutionType : ActiveSoftDeleteEntity
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;
}
