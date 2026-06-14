using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Results;
using Adrenalin.Modules.SLA.Application.Commands;
using Adrenalin.Modules.SLA.Domain.Interfaces;
using Adrenalin.SharedKernel.Contracts;

namespace Adrenalin.Modules.SLA.Application.Handlers;

public sealed class CheckEscalationsCommandHandler
    : IRequestHandler<CheckEscalationsCommand, Result<int>>
{
    private readonly ISlaRepository _slaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher; // ⚡ Inject IPublisher instead of IDispatcher

    public CheckEscalationsCommandHandler(
        ISlaRepository slaRepo,
        IUnitOfWork unitOfWork,
        IPublisher publisher) // Updated constructor
    {
        _slaRepo = slaRepo;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result<int>> Handle(CheckEscalationsCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var escalatedCount = 0;

        var rules = await _slaRepo.GetActiveEscalationRulesAsync(ct);
        var overdueTickets = await _slaRepo.GetOverdueSlaTicketsAsync(ct);

        foreach (var slaTicket in overdueTickets)
        {
            // ── First response breach check ────────────────────
            if (slaTicket.FirstResponseAt is null && now > slaTicket.FirstResponseDueAt && !slaTicket.FirstResponseBreached)
            {
                var minutesOverdue = (int)(now - slaTicket.FirstResponseDueAt).TotalMinutes;
                var rule = rules.Where(r => minutesOverdue >= r.NoResponseMinutes).OrderByDescending(r => r.NoResponseMinutes).FirstOrDefault();

                if (rule is not null)
                {
                    var groupId = await _slaRepo.GetTicketGroupIdAsync(slaTicket.TicketId, ct);
                    var ticketNumber = await _slaRepo.GetTicketNumberAsync(slaTicket.TicketId, ct);
                    if (groupId.HasValue)
                    {
                        var usersToNotify = await _slaRepo.GetUserIdsByRoleInGroupAsync(groupId.Value, rule.NotifyRole, ct);
                        await _slaRepo.MarkFirstResponseBreachedAsync(slaTicket.Id, ct);
                        escalatedCount++;

                        // ⚡ Fire event using your custom IPublisher
                        // (Verify if the method name inside IPublisher is Publish or PublishAsync)
                        await _publisher.Publish(new SlaBreachNotificationContract(
                            slaTicket.TicketId,
                           ticketNumber,
                            "First Response Breach",
                            rule.Name,
                            rule.NotifyRole,
                            usersToNotify
                        ), ct);
                    }
                }
            }

            // ── Resolution breach check ────────────────────────
            if (slaTicket.ResolvedAt is null && now > slaTicket.ResolutionDueAt && !slaTicket.ResolutionBreached)
            {
                var minutesOverdue = (int)(now - slaTicket.ResolutionDueAt).TotalMinutes;
                var rule = rules.Where(r => minutesOverdue >= r.NoResponseMinutes).OrderByDescending(r => r.NoResponseMinutes).FirstOrDefault();

                if (rule is not null)
                {
                    var groupId = await _slaRepo.GetTicketGroupIdAsync(slaTicket.TicketId, ct);
                    var ticketNumber = await _slaRepo.GetTicketNumberAsync(slaTicket.TicketId, ct);
                    if (groupId.HasValue)
                    {
                        var usersToNotify = await _slaRepo.GetUserIdsByRoleInGroupAsync(groupId.Value, rule.NotifyRole, ct);
                        await _slaRepo.MarkResolutionBreachedAsync(slaTicket.Id, ct);
                        escalatedCount++;

                        // ⚡ Fire event using your custom IPublisher
                        await _publisher.Publish(new SlaBreachNotificationContract(
                            slaTicket.TicketId,
                           ticketNumber,
                            "Resolution Breach",
                            rule.Name,
                            rule.NotifyRole,
                            usersToNotify
                        ), ct);
                    }
                }
            }

            // ── Follow-up breach check ─────────────────────────
            if (slaTicket.FollowUpAt is null && slaTicket.FollowUpDueAt.HasValue && now > slaTicket.FollowUpDueAt.Value && !slaTicket.FollowUpBreached)
            {
                await _slaRepo.MarkFollowUpBreachedAsync(slaTicket.Id, ct);
                escalatedCount++;

                

                var groupId = await _slaRepo.GetTicketGroupIdAsync(slaTicket.TicketId, ct);
                if (groupId.HasValue)
                {
                    var teamLeads = await _slaRepo.GetUserIdsByRoleInGroupAsync(groupId.Value, "team_lead", ct);
                    var ticketNumber = await _slaRepo.GetTicketNumberAsync(slaTicket.TicketId, ct);
                    // ⚡ Fire event using your custom IPublisher
                    await _publisher.Publish(new SlaBreachNotificationContract(
                        slaTicket.TicketId,
                        ticketNumber,
                        "Follow Up Breach",
                        "Standard Follow-up Target Breached",
                        "team_lead",
                        teamLeads
                    ), ct);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Success(escalatedCount);
    }
}