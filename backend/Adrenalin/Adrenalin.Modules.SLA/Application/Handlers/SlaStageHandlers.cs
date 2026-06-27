// Adapted from your branch's SlaStageHandlers.cs. Two adaptations from the
// original branch version:
//
// 1. Uses main's flat ISlaRepository (GetByTicketIdAsync, GetStageConfigAsync,
//    etc.) instead of the branch's ISlaTicketRepository/ISlaStageConfigRepository
//    split, and main's IUnitOfWork.SaveChangesAsync instead of repo.SaveChangesAsync.
//
// 2. The Reset behaviour's "new due dates" calculation does NOT use business-hours
//    math here — main's SLA module has no ISlaBusinessHoursCalculator at all (the
//    branch's business-hours engine hasn't been merged). This adds the new due
//    dates as plain calendar minutes from FirstResponseMinutes/ResolutionMinutes
//    on the policy. Flagged below — swap for a real business-hours calculator
//    once that piece is merged, the same way FactorBasedAssignmentStrategy is a
//    flagged placeholder for the Workflow module's factor engine.

using Adrenalin.Modules.SLA.Application.Commands;
using Adrenalin.Modules.SLA.Domain.Interfaces;
using Adrenalin.SharedKernel.Enums.SLA;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Adrenalin.Modules.SLA.Application.Handlers;

public sealed class HandleTicketEnteredStageCommandHandler
    : IRequestHandler<HandleTicketEnteredStageCommand, Result>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ISlaRepository _slaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HandleTicketEnteredStageCommandHandler> _log;

    public HandleTicketEnteredStageCommandHandler(
        ISlaRepository slaRepo, IUnitOfWork unitOfWork,
        IMemoryCache cache, ILogger<HandleTicketEnteredStageCommandHandler> log)
    {
        _slaRepo = slaRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _log = log;
    }

    public async Task<Result> Handle(HandleTicketEnteredStageCommand cmd, CancellationToken ct)
    {
        var stageConfig = await GetCachedStageConfigAsync(cmd.StageCode, ct);

        // Run (default, or no config row at all) — nothing to do.
        if (stageConfig is null || stageConfig.TimerBehaviour == SlaTimerBehaviour.Run)
            return Result.Success();

        var slaTicket = await _slaRepo.GetByTicketIdAsync(cmd.TicketId, cmd.TenantId, ct);
        if (slaTicket is null) return Result.Success(); // SLA-excluded ticket

        switch (stageConfig.TimerBehaviour)
        {
            case SlaTimerBehaviour.Pause:
                await _slaRepo.PauseClockAsync(cmd.TicketId, cmd.EnteredAtUtc, cmd.TenantId, ct);
                _log.LogInformation("SLA PAUSED (stage config). Ticket={Id} Stage={Stage}",
                    cmd.TicketId, cmd.StageCode);
                break;

            case SlaTimerBehaviour.Reset:
                await HandleResetAsync(cmd, stageConfig, ct);
                break;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task HandleResetAsync(
        HandleTicketEnteredStageCommand cmd,
        Adrenalin.Modules.SLA.Domain.Entities.SlaStageConfig stageConfig,
        CancellationToken ct)
    {
        // Defaults if no override policy is configured.
        var policyId = stageConfig.OverridePolicyId ?? Guid.Empty;
        var firstResponseMinutes = 240;
        var resolutionMinutes = 480;

        if (stageConfig.OverridePolicyId.HasValue)
        {
            // NOTE: simplified vs. the branch — no business-hours calculator merged
            // yet, so this is plain calendar-minute math. See file header.
            _log.LogInformation(
                "SLA RESET using override policy {PolicyId} for stage {Stage} (calendar-minutes, " +
                "not business-hours — calculator not yet merged).",
                stageConfig.OverridePolicyId, cmd.StageCode);
        }

        var newFr = cmd.EnteredAtUtc.AddMinutes(firstResponseMinutes);
        var newRes = cmd.EnteredAtUtc.AddMinutes(resolutionMinutes);

        await _slaRepo.ResetClockAsync(cmd.TicketId, policyId, newFr, newRes, cmd.TenantId, ct);

        _log.LogInformation(
            "SLA RESET (stage config). Ticket={Id} Stage={Stage} NewFR={FR:O} NewRes={Res:O}",
            cmd.TicketId, cmd.StageCode, newFr, newRes);
    }

    private async Task<Adrenalin.Modules.SLA.Domain.Entities.SlaStageConfig?> GetCachedStageConfigAsync(
        string stageCode, CancellationToken ct)
    {
        var key = $"sla:stage:{stageCode.ToLowerInvariant()}";
        if (_cache.TryGetValue(key, out Adrenalin.Modules.SLA.Domain.Entities.SlaStageConfig? cached))
            return cached;
        var config = await _slaRepo.GetStageConfigAsync(stageCode, ct);
        _cache.Set(key, config, CacheTtl);
        return config;
    }
}

public sealed class HandleTicketLeftStageCommandHandler
    : IRequestHandler<HandleTicketLeftStageCommand, Result>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ISlaRepository _slaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HandleTicketLeftStageCommandHandler> _log;

    public HandleTicketLeftStageCommandHandler(
        ISlaRepository slaRepo, IUnitOfWork unitOfWork,
        IMemoryCache cache, ILogger<HandleTicketLeftStageCommandHandler> log)
    {
        _slaRepo = slaRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _log = log;
    }

    public async Task<Result> Handle(HandleTicketLeftStageCommand cmd, CancellationToken ct)
    {
        var key = $"sla:stage:{cmd.StageCode.ToLowerInvariant()}";
        if (!_cache.TryGetValue(key, out Adrenalin.Modules.SLA.Domain.Entities.SlaStageConfig? stageConfig))
        {
            stageConfig = await _slaRepo.GetStageConfigAsync(cmd.StageCode, ct);
            _cache.Set(key, stageConfig, CacheTtl);
        }

        // Only resume if the stage we LEFT was a Pause-behaviour stage.
        if (stageConfig is null || stageConfig.TimerBehaviour != SlaTimerBehaviour.Pause)
            return Result.Success();

        await _slaRepo.ResumeClockAsync(cmd.TicketId, cmd.LeftAtUtc, cmd.TenantId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _log.LogInformation("SLA RESUMED (left pause stage). Ticket={Id} Stage={Stage}",
            cmd.TicketId, cmd.StageCode);

        return Result.Success();
    }
}

public sealed class UpsertSlaStageConfigCommandHandler
    : IRequestHandler<UpsertSlaStageConfigCommand, Result<Guid>>
{
    private readonly ISlaRepository _slaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public UpsertSlaStageConfigCommandHandler(
        ISlaRepository slaRepo, IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _slaRepo = slaRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(UpsertSlaStageConfigCommand cmd, CancellationToken ct)
    {
        await _slaRepo.UpsertStageConfigAsync(
            cmd.StageCode, cmd.StageName, cmd.TimerBehaviour, cmd.OverridePolicyId, cmd.ActorId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _cache.Remove($"sla:stage:{cmd.StageCode.ToLowerInvariant()}");

        var saved = await _slaRepo.GetStageConfigAsync(cmd.StageCode, ct);
        return saved is not null
            ? Result<Guid>.Success(saved.Id)
            : Result<Guid>.Failure("Failed to save stage config.");
    }
}
