using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Domain.Services;

public sealed class GroupFallbackValidationService
{
    private readonly IGroupRepository _groups;

    public GroupFallbackValidationService(IGroupRepository groups)
    {
        _groups = groups;
    }

    public async Task<Result> ValidateFallbackChainAsync(Guid sourceGroupId, Guid? targetFallbackId, CancellationToken ct = default)
    {
        if (!targetFallbackId.HasValue)
            return Result.Success();

        if (sourceGroupId == targetFallbackId.Value)
            return Result.Failure("A group cannot be its own fallback.");

        var visited = new HashSet<Guid> { sourceGroupId, targetFallbackId.Value };
        var currentId = targetFallbackId.Value;

        int depth = 0;
        const int MaxDepth = 10;

        while (depth < MaxDepth)
        {
            var group = await _groups.GetByIdAsync(currentId, ct);
            if (group is null || group.IsDeleted)
                return Result.Failure($"Fallback group {currentId} is inactive or deleted.");

            if (!group.FallbackGroupId.HasValue)
                return Result.Success();

            var nextId = group.FallbackGroupId.Value;
            
            if (visited.Contains(nextId))
                return Result.Failure("Circular fallback chain detected.");

            visited.Add(nextId);
            currentId = nextId;
            depth++;
        }

        return Result.Failure("Fallback chain exceeds maximum allowed depth.");
    }
}
